using System;
using System.Buffers;
using System.Collections.Generic;
using GroveApp.Models.Memory;

namespace GroveApp.Engine.Memory;

/// <summary>
/// Inclusive grid-cell bounds. An empty box is represented by inverted bounds and
/// is useful as the identity for MBR expansion.
/// </summary>
public readonly record struct SpatialBoundingBox(
    int XMin,
    int YMin,
    int XMax,
    int YMax,
    Guid LayerId)
{
    public static SpatialBoundingBox Empty =>
        new(int.MaxValue, int.MaxValue, int.MinValue, int.MinValue, Guid.Empty);

    public bool IsEmpty => XMin > XMax || YMin > YMax;

    public int Width => IsEmpty ? 0 : checked(XMax - XMin + 1);

    public int Height => IsEmpty ? 0 : checked(YMax - YMin + 1);

    public long Area => IsEmpty ? 0L : (long)Width * Height;

    public bool Intersects(in SpatialBoundingBox other)
    {
        if (IsEmpty || other.IsEmpty)
        {
            return false;
        }

        bool layerMatches = LayerId == Guid.Empty ||
                            other.LayerId == Guid.Empty ||
                            LayerId == other.LayerId;
        return layerMatches &&
               XMin <= other.XMax && XMax >= other.XMin &&
               YMin <= other.YMax && YMax >= other.YMin;
    }

    public SpatialBoundingBox Expand(in SpatialBoundingBox other)
    {
        if (IsEmpty)
        {
            return other;
        }

        if (other.IsEmpty)
        {
            return this;
        }

        return new SpatialBoundingBox(
            Math.Min(XMin, other.XMin),
            Math.Min(YMin, other.YMin),
            Math.Max(XMax, other.XMax),
            Math.Max(YMax, other.YMax),
            LayerId == other.LayerId ? LayerId : Guid.Empty);
    }

    public static SpatialBoundingBox ForAnchor(in MemoryAnchor anchor) =>
        new(
            anchor.CellX,
            anchor.CellY,
            checked(anchor.CellX + Math.Max(1, anchor.CellWidth) - 1),
            checked(anchor.CellY + Math.Max(1, anchor.CellHeight) - 1),
            anchor.LayerId);
}

/// <summary>
/// R-tree node. The lists are implementation-owned; callers normally use
/// <see cref="MemorySpatialRTree"/>, which is the deep query seam.
/// </summary>
public sealed class RTreeNode
{
    public const int MaxEntries = 16;
    public const int MinEntries = 4;

    internal RTreeNode? Parent { get; set; }

    public bool IsLeaf { get; }
    public SpatialBoundingBox Mbr { get; private set; } = SpatialBoundingBox.Empty;
    public List<RTreeNode> Children { get; } = new(MaxEntries);
    public List<MemoryAnchor> Anchors { get; } = new(MaxEntries);

    public RTreeNode(bool isLeaf)
    {
        IsLeaf = isLeaf;
    }

    public void RecalculateMbr()
    {
        SpatialBoundingBox bounds = SpatialBoundingBox.Empty;
        if (IsLeaf)
        {
            foreach (MemoryAnchor anchor in Anchors)
            {
                bounds = bounds.Expand(SpatialBoundingBox.ForAnchor(anchor));
            }
        }
        else
        {
            foreach (RTreeNode child in Children)
            {
                bounds = bounds.Expand(child.Mbr);
            }
        }

        Mbr = bounds;
    }
}

/// <summary>
/// Thread-safe multi-layer R-tree for memory anchors. Queries write into a caller
/// supplied span; the implementation owns all tree allocations and keeps layer
/// filtering in the spatial box seam.
/// </summary>
public sealed class MemorySpatialRTree
{
    private RTreeNode _root = new(isLeaf: true);
    private readonly object _treeGate = new();

    public int Count { get; private set; }

    public RTreeNode Root
    {
        get
        {
            lock (_treeGate)
            {
                return _root;
            }
        }
    }

    public void Insert(in MemoryAnchor anchor)
    {
        if (anchor.AnchorId == Guid.Empty)
        {
            throw new ArgumentException("An anchor ID is required for spatial indexing.", nameof(anchor));
        }

        lock (_treeGate)
        {
            if (TryFindLeaf(_root, anchor.AnchorId, out RTreeNode? existingLeaf, out int existingIndex))
            {
                existingLeaf!.Anchors[existingIndex] = anchor;
                RecalculateUpwards(existingLeaf);
                return;
            }

            RTreeNode leaf = ChooseLeaf(_root, SpatialBoundingBox.ForAnchor(anchor));
            leaf.Anchors.Add(anchor);
            Count++;
            RecalculateUpwards(leaf);

            if (leaf.Anchors.Count > RTreeNode.MaxEntries)
            {
                SplitNode(leaf);
            }
        }
    }

    public bool Remove(Guid anchorId, out MemoryAnchor removed)
    {
        if (anchorId == Guid.Empty)
        {
            removed = default;
            return false;
        }

        lock (_treeGate)
        {
            if (!TryFindLeaf(_root, anchorId, out RTreeNode? leaf, out int index))
            {
                removed = default;
                return false;
            }

            removed = leaf!.Anchors[index];
            leaf.Anchors.RemoveAt(index);
            Count--;
            RemoveEmptyNodes(leaf);
            RecalculateUpwards(_root);
            return true;
        }
    }

    public bool TryGet(Guid anchorId, out MemoryAnchor anchor)
    {
        lock (_treeGate)
        {
            if (TryFindLeaf(_root, anchorId, out RTreeNode? leaf, out int index))
            {
                anchor = leaf!.Anchors[index];
                return true;
            }

            anchor = default;
            return false;
        }
    }

    public MemoryAnchor[] Snapshot()
    {
        lock (_treeGate)
        {
            var anchors = new List<MemoryAnchor>(Count);
            CollectAnchors(_root, anchors);
            return anchors.ToArray();
        }
    }

    public int QueryBoundingBox(in SpatialBoundingBox queryBox, Span<MemoryAnchor> destination)
    {
        return QueryBoundingBox(queryBox, ReadOnlySpan<Guid>.Empty, destination);
    }

    /// <summary>
    /// Queries one box over either all layers (empty filter) or an explicit layer subset.
    /// </summary>
    public int QueryBoundingBox(
        in SpatialBoundingBox queryBox,
        ReadOnlySpan<Guid> layerFilter,
        Span<MemoryAnchor> destination)
    {
        lock (_treeGate)
        {
            int written = 0;
            QueryNode(_root, queryBox, layerFilter, destination, ref written);
            return written;
        }
    }

    public void Clear()
    {
        lock (_treeGate)
        {
            _root = new RTreeNode(isLeaf: true);
            Count = 0;
        }
    }

    private static void CollectAnchors(RTreeNode node, List<MemoryAnchor> destination)
    {
        if (node.IsLeaf)
        {
            destination.AddRange(node.Anchors);
            return;
        }

        foreach (RTreeNode child in node.Children)
        {
            CollectAnchors(child, destination);
        }
    }

    private static RTreeNode ChooseLeaf(RTreeNode node, in SpatialBoundingBox bounds)
    {
        if (node.IsLeaf)
        {
            return node;
        }

        RTreeNode? best = null;
        long bestEnlargement = long.MaxValue;
        long bestArea = long.MaxValue;
        foreach (RTreeNode child in node.Children)
        {
            long area = child.Mbr.Area;
            long enlargement = child.Mbr.Expand(bounds).Area - area;
            if (best is null || enlargement < bestEnlargement ||
                enlargement == bestEnlargement && area < bestArea)
            {
                best = child;
                bestEnlargement = enlargement;
                bestArea = area;
            }
        }

        return ChooseLeaf(best!, bounds);
    }

    private void SplitNode(RTreeNode node)
    {
        RTreeNode sibling = new(node.IsLeaf);
        if (node.IsLeaf)
        {
            MemoryAnchor[] entries = node.Anchors.ToArray();
            Array.Sort(entries, CompareAnchorsBySpread(entries));
            node.Anchors.Clear();
            int midpoint = entries.Length / 2;
            for (int i = 0; i < midpoint; i++)
            {
                node.Anchors.Add(entries[i]);
            }

            for (int i = midpoint; i < entries.Length; i++)
            {
                sibling.Anchors.Add(entries[i]);
            }
        }
        else
        {
            RTreeNode[] children = node.Children.ToArray();
            Array.Sort(children, CompareNodesBySpread(children));
            node.Children.Clear();
            int midpoint = children.Length / 2;
            for (int i = 0; i < midpoint; i++)
            {
                node.Children.Add(children[i]);
                children[i].Parent = node;
            }

            for (int i = midpoint; i < children.Length; i++)
            {
                sibling.Children.Add(children[i]);
                children[i].Parent = sibling;
            }
        }

        node.RecalculateMbr();
        sibling.RecalculateMbr();

        if (node.Parent is null)
        {
            RTreeNode newRoot = new(isLeaf: false);
            node.Parent = newRoot;
            sibling.Parent = newRoot;
            newRoot.Children.Add(node);
            newRoot.Children.Add(sibling);
            newRoot.RecalculateMbr();
            _root = newRoot;
            return;
        }

        RTreeNode parent = node.Parent;
        sibling.Parent = parent;
        parent.Children.Add(sibling);
        RecalculateUpwards(parent);
        if (parent.Children.Count > RTreeNode.MaxEntries)
        {
            SplitNode(parent);
        }
    }

    private static Comparison<MemoryAnchor> CompareAnchorsBySpread(MemoryAnchor[] entries)
    {
        (int xMin, int xMax, int yMin, int yMax) = BoundsOf(entries);
        bool useX = (long)xMax - xMin >= (long)yMax - yMin;
        return (left, right) =>
        {
            int primary = (useX ? left.CellX : left.CellY).CompareTo(useX ? right.CellX : right.CellY);
            if (primary != 0)
            {
                return primary;
            }

            int secondary = (useX ? left.CellY : left.CellX).CompareTo(useX ? right.CellY : right.CellX);
            return secondary != 0
                ? secondary
                : left.AnchorId.CompareTo(right.AnchorId);
        };
    }

    private static Comparison<RTreeNode> CompareNodesBySpread(RTreeNode[] nodes)
    {
        (int xMin, int xMax, int yMin, int yMax) = BoundsOf(nodes);
        bool useX = (long)xMax - xMin >= (long)yMax - yMin;
        return (left, right) =>
        {
            int leftPrimary = useX ? Center(left.Mbr.XMin, left.Mbr.XMax) : Center(left.Mbr.YMin, left.Mbr.YMax);
            int rightPrimary = useX ? Center(right.Mbr.XMin, right.Mbr.XMax) : Center(right.Mbr.YMin, right.Mbr.YMax);
            int primary = leftPrimary.CompareTo(rightPrimary);
            if (primary != 0)
            {
                return primary;
            }

            return left.Mbr.XMin.CompareTo(right.Mbr.XMin);
        };
    }

    private static (int XMin, int XMax, int YMin, int YMax) BoundsOf(MemoryAnchor[] entries)
    {
        int xMin = int.MaxValue;
        int xMax = int.MinValue;
        int yMin = int.MaxValue;
        int yMax = int.MinValue;
        foreach (MemoryAnchor entry in entries)
        {
            xMin = Math.Min(xMin, entry.CellX);
            xMax = Math.Max(xMax, entry.CellX);
            yMin = Math.Min(yMin, entry.CellY);
            yMax = Math.Max(yMax, entry.CellY);
        }

        return (xMin, xMax, yMin, yMax);
    }

    private static (int XMin, int XMax, int YMin, int YMax) BoundsOf(RTreeNode[] nodes)
    {
        int xMin = int.MaxValue;
        int xMax = int.MinValue;
        int yMin = int.MaxValue;
        int yMax = int.MinValue;
        foreach (RTreeNode node in nodes)
        {
            xMin = Math.Min(xMin, node.Mbr.XMin);
            xMax = Math.Max(xMax, node.Mbr.XMax);
            yMin = Math.Min(yMin, node.Mbr.YMin);
            yMax = Math.Max(yMax, node.Mbr.YMax);
        }

        return (xMin, xMax, yMin, yMax);
    }

    private static int Center(int min, int max) => (int)(((long)min + max) / 2);

    private static bool TryFindLeaf(
        RTreeNode node,
        Guid anchorId,
        out RTreeNode? leaf,
        out int index)
    {
        if (node.IsLeaf)
        {
            for (int i = 0; i < node.Anchors.Count; i++)
            {
                if (node.Anchors[i].AnchorId == anchorId)
                {
                    leaf = node;
                    index = i;
                    return true;
                }
            }

            leaf = null;
            index = -1;
            return false;
        }

        foreach (RTreeNode child in node.Children)
        {
            if (TryFindLeaf(child, anchorId, out leaf, out index))
            {
                return true;
            }
        }

        leaf = null;
        index = -1;
        return false;
    }

    private static void QueryNode(
        RTreeNode node,
        in SpatialBoundingBox queryBox,
        ReadOnlySpan<Guid> layerFilter,
        Span<MemoryAnchor> destination,
        ref int written)
    {
        if (!node.Mbr.Intersects(queryBox))
        {
            return;
        }

        if (node.IsLeaf)
        {
            foreach (MemoryAnchor anchor in node.Anchors)
            {
                if (!MatchesLayer(anchor.LayerId, layerFilter) ||
                    !SpatialBoundingBox.ForAnchor(anchor).Intersects(queryBox))
                {
                    continue;
                }

                if (written < destination.Length)
                {
                    destination[written] = anchor;
                    written++;
                }
            }

            return;
        }

        foreach (RTreeNode child in node.Children)
        {
            QueryNode(child, queryBox, layerFilter, destination, ref written);
        }
    }

    private static bool MatchesLayer(Guid layerId, ReadOnlySpan<Guid> layerFilter)
    {
        if (layerFilter.IsEmpty)
        {
            return true;
        }

        foreach (Guid candidate in layerFilter)
        {
            if (candidate == layerId)
            {
                return true;
            }
        }

        return false;
    }

    private static void RemoveEmptyNodes(RTreeNode node)
    {
        while (!node.IsLeaf && node.Children.Count == 0 && node.Parent is not null)
        {
            RTreeNode parent = node.Parent;
            parent.Children.Remove(node);
            node = parent;
        }
    }

    private static void RecalculateUpwards(RTreeNode node)
    {
        RTreeNode? current = node;
        while (current is not null)
        {
            current.RecalculateMbr();
            current = current.Parent;
        }
    }
}

/// <summary>
/// Thread-safe 64x64 cell tile cache. The internal key retains the full layer GUID;
/// the public long key helper remains available for diagnostics and compatibility.
/// </summary>
public sealed class SpatialQueryCache
{
    public const int TileSize = 64;

    private readonly Dictionary<TileKey, MemoryAnchor[]> _tiles = new();
    private readonly object _cacheGate = new();

    public int Count
    {
        get
        {
            lock (_cacheGate)
            {
                return _tiles.Count;
            }
        }
    }

    public static long ComputeTileKey(int cellX, int cellY, Guid layerId)
    {
        int tileX = cellX >> 6;
        int tileY = cellY >> 6;
        unchecked
        {
            long spatialHash = ((long)tileX << 32) | (uint)tileY;
            return spatialHash ^ layerId.GetHashCode();
        }
    }

    public bool TryGetTile(int cellX, int cellY, Guid layerId, out MemoryAnchor[]? cachedAnchors)
    {
        TileKey key = TileKey.FromCell(cellX, cellY, layerId);
        lock (_cacheGate)
        {
            return _tiles.TryGetValue(key, out cachedAnchors);
        }
    }

    public void PutTile(int cellX, int cellY, Guid layerId, MemoryAnchor[] anchors)
    {
        ArgumentNullException.ThrowIfNull(anchors);
        TileKey key = TileKey.FromCell(cellX, cellY, layerId);
        lock (_cacheGate)
        {
            _tiles[key] = (MemoryAnchor[])anchors.Clone();
        }
    }

    public void InvalidateRegion(in SpatialBoundingBox region)
    {
        if (region.IsEmpty)
        {
            return;
        }

        int minTileX = region.XMin >> 6;
        int maxTileX = region.XMax >> 6;
        int minTileY = region.YMin >> 6;
        int maxTileY = region.YMax >> 6;

        lock (_cacheGate)
        {
            for (int tileX = minTileX; tileX <= maxTileX; tileX++)
            {
                for (int tileY = minTileY; tileY <= maxTileY; tileY++)
                {
                    List<TileKey> keysToRemove = new();
                    foreach (TileKey key in _tiles.Keys)
                    {
                        bool sameTile = key.TileX == tileX && key.TileY == tileY;
                        bool sameLayer = region.LayerId == Guid.Empty ||
                                         key.LayerId == Guid.Empty ||
                                         key.LayerId == region.LayerId;
                        if (sameTile && sameLayer)
                        {
                            keysToRemove.Add(key);
                        }
                    }

                    foreach (TileKey key in keysToRemove)
                    {
                        _tiles.Remove(key);
                    }
                }
            }
        }
    }

    public void Clear()
    {
        lock (_cacheGate)
        {
            _tiles.Clear();
        }
    }

    private readonly record struct TileKey(int TileX, int TileY, Guid LayerId)
    {
        public static TileKey FromCell(int cellX, int cellY, Guid layerId) =>
            new(cellX >> 6, cellY >> 6, layerId);
    }
}

/// <summary>
/// Deep spatial seam composing the R-tree and its 64x64 tile cache. Mutations
/// invalidate only overlapping tiles, while queries can remain allocation-free on
/// cache hits and always write into caller-owned spans.
/// </summary>
public sealed class MemorySpatialIndex
{
    private readonly MemorySpatialRTree _tree = new();
    private readonly SpatialQueryCache _cache = new();

    public int Count => _tree.Count;
    public MemorySpatialRTree Tree => _tree;
    public SpatialQueryCache Cache => _cache;

    public MemoryAnchor[] Snapshot() => _tree.Snapshot();

    public void Clear()
    {
        _tree.Clear();
        _cache.Clear();
    }

    public void Insert(in MemoryAnchor anchor)
    {
        if (_tree.TryGet(anchor.AnchorId, out MemoryAnchor previous))
        {
            _cache.InvalidateRegion(SpatialBoundingBox.ForAnchor(previous));
        }

        _tree.Insert(anchor);
        _cache.InvalidateRegion(SpatialBoundingBox.ForAnchor(anchor));
    }

    public bool Remove(Guid anchorId, out MemoryAnchor removed)
    {
        if (!_tree.Remove(anchorId, out removed))
        {
            return false;
        }

        _cache.InvalidateRegion(SpatialBoundingBox.ForAnchor(removed));
        return true;
    }

    public int QueryBoundingBox(in SpatialBoundingBox queryBox, Span<MemoryAnchor> destination)
    {
        if (queryBox.IsEmpty || destination.IsEmpty)
        {
            return 0;
        }

        if (queryBox.LayerId != Guid.Empty)
        {
            return QueryCachedSingleLayer(queryBox, destination);
        }

        // A composite all-layer query can use the same tile cache. The cache key
        // uses Guid.Empty to mean the all-layer view.
        return QueryCachedSingleLayer(queryBox, destination);
    }

    public int QueryBoundingBox(
        in SpatialBoundingBox queryBox,
        ReadOnlySpan<Guid> layerFilter,
        Span<MemoryAnchor> destination)
    {
        if (layerFilter.IsEmpty)
        {
            return QueryBoundingBox(queryBox, destination);
        }

        return _tree.QueryBoundingBox(queryBox, layerFilter, destination);
    }

    private int QueryCachedSingleLayer(in SpatialBoundingBox queryBox, Span<MemoryAnchor> destination)
    {
        int minTileX = queryBox.XMin >> 6;
        int maxTileX = queryBox.XMax >> 6;
        int minTileY = queryBox.YMin >> 6;
        int maxTileY = queryBox.YMax >> 6;
        int written = 0;

        for (int tileX = minTileX; tileX <= maxTileX; tileX++)
        {
            for (int tileY = minTileY; tileY <= maxTileY; tileY++)
            {
                int tileMinX = unchecked(tileX * SpatialQueryCache.TileSize);
                int tileMinY = unchecked(tileY * SpatialQueryCache.TileSize);
                SpatialBoundingBox tileBox = new(
                    tileMinX,
                    tileMinY,
                    unchecked(tileMinX + SpatialQueryCache.TileSize - 1),
                    unchecked(tileMinY + SpatialQueryCache.TileSize - 1),
                    queryBox.LayerId);

                if (!_cache.TryGetTile(tileMinX, tileMinY, queryBox.LayerId, out MemoryAnchor[]? tileAnchors))
                {
                    int capacity = Math.Max(1, _tree.Count);
                    MemoryAnchor[] rented = ArrayPool<MemoryAnchor>.Shared.Rent(capacity);
                    try
                    {
                        int count = _tree.QueryBoundingBox(tileBox, rented.AsSpan());
                        tileAnchors = new MemoryAnchor[count];
                        rented.AsSpan(0, count).CopyTo(tileAnchors);
                        _cache.PutTile(tileMinX, tileMinY, queryBox.LayerId, tileAnchors);
                    }
                    finally
                    {
                        ArrayPool<MemoryAnchor>.Shared.Return(rented, clearArray: true);
                    }
                }

                MemoryAnchor[] resolvedTileAnchors = tileAnchors ?? Array.Empty<MemoryAnchor>();
                foreach (MemoryAnchor anchor in resolvedTileAnchors)
                {
                    if (!SpatialBoundingBox.ForAnchor(anchor).Intersects(queryBox))
                    {
                        continue;
                    }

                    if (written < destination.Length)
                    {
                        destination[written] = anchor;
                        written++;
                    }
                }
            }
        }

        return written;
    }
}
