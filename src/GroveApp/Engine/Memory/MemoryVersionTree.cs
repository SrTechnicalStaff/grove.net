using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using GroveApp.Models.Memory;

namespace GroveApp.Engine.Memory;

public enum DeltaOpKind : byte
{
    Retain = 1,
    Insert = 2,
    Delete = 3
}

/// <summary>
/// One deterministic text edit operation. Length is measured in UTF-16 chars,
/// matching the string representation used by the text editor.
/// </summary>
public readonly record struct DeltaChunk
{
    public required DeltaOpKind Kind { get; init; }
    public int Length { get; init; }
    public string InsertText { get; init; } = string.Empty;

    public DeltaChunk()
    {
    }

    public static DeltaChunk Retain(int length) =>
        new() { Kind = DeltaOpKind.Retain, Length = ValidateLength(length) };

    public static DeltaChunk Delete(int length) =>
        new() { Kind = DeltaOpKind.Delete, Length = ValidateLength(length) };

    public static DeltaChunk Insert(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        return new() { Kind = DeltaOpKind.Insert, Length = text.Length, InsertText = text };
    }

    private static int ValidateLength(int length) =>
        length >= 0 ? length : throw new ArgumentOutOfRangeException(nameof(length));
}

/// <summary>
/// Immutable, exactly reconstructible text delta. The builder uses Myers' shortest
/// edit path so unrelated edits remain separate retain/insert/delete chunks.
/// </summary>
public sealed record MemoryDelta
{
    public required Guid SourceMemoryId { get; init; }
    public required Guid TargetMemoryId { get; init; }
    public ImmutableList<DeltaChunk> Chunks { get; init; } = ImmutableList<DeltaChunk>.Empty;

    public static MemoryDelta Create(
        Guid sourceMemoryId,
        Guid targetMemoryId,
        string sourceText,
        string targetText)
    {
        ArgumentNullException.ThrowIfNull(sourceText);
        ArgumentNullException.ThrowIfNull(targetText);

        return new MemoryDelta
        {
            SourceMemoryId = sourceMemoryId,
            TargetMemoryId = targetMemoryId,
            Chunks = BuildMyersChunks(sourceText, targetText)
        };
    }

    private static ImmutableList<DeltaChunk> BuildMyersChunks(string sourceText, string targetText)
    {
        int sourceLength = sourceText.Length;
        int targetLength = targetText.Length;
        int maxDistance = sourceLength + targetLength;
        var trace = new List<Dictionary<int, int>>();
        var frontier = new Dictionary<int, int> { [1] = 0 };

        for (int distance = 0; distance <= maxDistance; distance++)
        {
            var next = new Dictionary<int, int>();
            for (int diagonal = -distance; diagonal <= distance; diagonal += 2)
            {
                int x;
                if (diagonal == -distance ||
                    (diagonal != distance && Read(frontier, diagonal - 1) < Read(frontier, diagonal + 1)))
                {
                    x = Read(frontier, diagonal + 1);
                }
                else
                {
                    x = Read(frontier, diagonal - 1) + 1;
                }

                int y = x - diagonal;
                while (x < sourceLength && y < targetLength && sourceText[x] == targetText[y])
                {
                    x++;
                    y++;
                }

                next[diagonal] = x;
                if (x >= sourceLength && y >= targetLength)
                {
                    trace.Add(next);
                    return BacktrackMyers(trace, sourceText, targetText);
                }
            }

            trace.Add(next);
            frontier = next;
        }

        throw new InvalidOperationException("Myers diff did not reach the target text.");
    }

    private static ImmutableList<DeltaChunk> BacktrackMyers(
        IReadOnlyList<Dictionary<int, int>> trace,
        string sourceText,
        string targetText)
    {
        var reversed = new List<DeltaChunk>();
        int x = sourceText.Length;
        int y = targetText.Length;

        for (int distance = trace.Count - 1; distance > 0; distance--)
        {
            Dictionary<int, int> previous = trace[distance - 1];
            int diagonal = x - y;
            int previousDiagonal = diagonal == -distance ||
                (diagonal != distance && Read(previous, diagonal - 1) < Read(previous, diagonal + 1))
                    ? diagonal + 1
                    : diagonal - 1;
            int previousX = Read(previous, previousDiagonal);
            int previousY = previousX - previousDiagonal;

            while (x > previousX && y > previousY)
            {
                reversed.Add(DeltaChunk.Retain(1));
                x--;
                y--;
            }

            if (x == previousX)
            {
                reversed.Add(DeltaChunk.Insert(targetText[y - 1].ToString()));
                y--;
            }
            else
            {
                reversed.Add(DeltaChunk.Delete(1));
                x--;
            }
        }

        while (x > 0 && y > 0)
        {
            reversed.Add(DeltaChunk.Retain(1));
            x--;
            y--;
        }

        while (x-- > 0)
        {
            reversed.Add(DeltaChunk.Delete(1));
        }

        while (y-- > 0)
        {
            reversed.Add(DeltaChunk.Insert(targetText[y].ToString()));
        }

        reversed.Reverse();
        var chunks = ImmutableList.CreateBuilder<DeltaChunk>();
        foreach (DeltaChunk chunk in reversed)
        {
            if (chunks.Count > 0 && chunks[^1].Kind == chunk.Kind)
            {
                DeltaChunk previous = chunks[^1];
                chunks[^1] = chunk.Kind switch
                {
                    DeltaOpKind.Retain => DeltaChunk.Retain(previous.Length + chunk.Length),
                    DeltaOpKind.Delete => DeltaChunk.Delete(previous.Length + chunk.Length),
                    DeltaOpKind.Insert => DeltaChunk.Insert(previous.InsertText + chunk.InsertText),
                    _ => chunk
                };
            }
            else
            {
                chunks.Add(chunk);
            }
        }

        return chunks.ToImmutable();
    }

    private static int Read(Dictionary<int, int> frontier, int diagonal) =>
        frontier.TryGetValue(diagonal, out int x) ? x : 0;

    public string Apply(string sourceText)
    {
        ArgumentNullException.ThrowIfNull(sourceText);
        StringBuilder result = new();
        int sourceOffset = 0;

        foreach (DeltaChunk chunk in Chunks)
        {
            if (chunk.Length < 0)
            {
                throw new InvalidOperationException("A delta cannot contain a negative chunk length.");
            }

            switch (chunk.Kind)
            {
                case DeltaOpKind.Retain:
                    EnsureAvailable(sourceText, sourceOffset, chunk.Length);
                    result.Append(sourceText.AsSpan(sourceOffset, chunk.Length));
                    sourceOffset += chunk.Length;
                    break;

                case DeltaOpKind.Insert:
                    if (chunk.Length != chunk.InsertText.Length)
                    {
                        throw new InvalidOperationException("Insert length does not match insert text.");
                    }

                    result.Append(chunk.InsertText);
                    break;

                case DeltaOpKind.Delete:
                    EnsureAvailable(sourceText, sourceOffset, chunk.Length);
                    sourceOffset += chunk.Length;
                    break;

                default:
                    throw new InvalidOperationException($"Unknown delta operation {chunk.Kind}.");
            }
        }

        if (sourceOffset != sourceText.Length)
        {
            throw new InvalidOperationException("Delta did not consume the complete source payload.");
        }

        return result.ToString();
    }

    private static void EnsureAvailable(string sourceText, int offset, int length)
    {
        if (length > sourceText.Length - offset)
        {
            throw new InvalidOperationException("Delta references characters outside the source payload.");
        }
    }
}

public sealed class MemoryMergeConflictException : InvalidOperationException
{
    public MemoryMergeConflictException(int firstStart, int secondStart)
        : base($"Memory edits overlap at base offsets {firstStart} and {secondStart}.")
    {
        FirstStart = firstStart;
        SecondStart = secondStart;
    }

    public int FirstStart { get; }
    public int SecondStart { get; }
}

public sealed record MemoryVersionNode
{
    public required Guid MemoryId { get; init; }
    public Guid? ParentMemoryId { get; init; }
    public required Guid RootMemoryId { get; init; }
    public required uint Generation { get; init; }
    public string BranchName { get; init; } = "main";
    public ContentHash Hash { get; init; }
    public long CreatedAtTicks { get; init; }
    public MemoryDelta? DeltaFromParent { get; init; }
}

public interface IMemoryVersionTree
{
    void AddVersionNode(MemoryRecord record);

    void AddVersionNode(MemoryRecord record, IMemoryLedger ledger, string branchName = "main");

    MemoryVersionNode? GetNode(Guid memoryId);

    IReadOnlyList<MemoryVersionNode> GetLineagePath(Guid memoryId);

    Guid FindLowestCommonAncestor(Guid memoryIdA, Guid memoryIdB);

    IReadOnlyList<MemoryVersionNode> GetBranchHeads(Guid rootMemoryId);

    MemoryRecord MergeBranches(
        Guid memoryIdA,
        Guid memoryIdB,
        IMemoryLedger ledger,
        string targetBranchName = "main");
}

/// <summary>
/// Thread-safe lineage graph. The interface exposes graph operations while parent,
/// root, generation, and cycle invariants stay local to this implementation.
/// </summary>
public sealed class MemoryVersionTree : IMemoryVersionTree
{
    private readonly Dictionary<Guid, MemoryVersionNode> _nodes = new();
    private readonly Dictionary<Guid, SortedSet<Guid>> _childrenByParent = new();
    private readonly object _graphGate = new();

    public int Count
    {
        get
        {
            lock (_graphGate)
            {
                return _nodes.Count;
            }
        }
    }

    public void AddVersionNode(MemoryRecord record) => AddVersionNodeCore(record, ledger: null, "main");

    public void AddVersionNode(MemoryRecord record, string branchName)
    {
        AddVersionNodeCore(record, ledger: null, branchName);
    }

    public void AddVersionNode(MemoryRecord record, IMemoryLedger ledger, string branchName = "main")
    {
        AddVersionNodeCore(record, ledger, branchName);
    }

    private void AddVersionNodeCore(MemoryRecord record, IMemoryLedger? ledger, string branchName)
    {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentException.ThrowIfNullOrWhiteSpace(branchName);

        MemoryVersionNode node = new()
        {
            MemoryId = record.MemoryId,
            ParentMemoryId = record.ParentMemoryId,
            RootMemoryId = record.RootMemoryId,
            Generation = record.Generation,
            BranchName = branchName,
            Hash = record.Hash,
            CreatedAtTicks = record.CreatedAtTicks,
            DeltaFromParent = ledger is not null && record.ParentMemoryId is Guid parentMemoryId
                ? CreateTextDeltaIfSupported(parentMemoryId, record.MemoryId, ledger)
                : null
        };

        lock (_graphGate)
        {
            if (_nodes.TryGetValue(node.MemoryId, out MemoryVersionNode? existing))
            {
                if (existing != node)
                {
                    throw new InvalidOperationException($"Version node {node.MemoryId} has conflicting metadata.");
                }

                return;
            }

            if (node.ParentMemoryId is null)
            {
                if (node.Generation != 0 || node.RootMemoryId != node.MemoryId)
                {
                    throw new InvalidOperationException("A root version must point to itself and have generation zero.");
                }
            }
            else
            {
                if (!_nodes.TryGetValue(node.ParentMemoryId.Value, out MemoryVersionNode? parent))
                {
                    throw new KeyNotFoundException($"Parent version {node.ParentMemoryId.Value} is not in the tree.");
                }

                if (parent.RootMemoryId != node.RootMemoryId || node.Generation != parent.Generation + 1)
                {
                    throw new InvalidOperationException(
                        "A child version must retain its root and increment its parent's generation by one.");
                }

                // A new node has no children yet, so the only cycle it can create
                // at insertion time is a self-parent pointer.
                if (node.ParentMemoryId == node.MemoryId)
                {
                    throw new InvalidOperationException("Adding this parent pointer would introduce a cycle.");
                }
            }

            _nodes.Add(node.MemoryId, node);
            if (node.ParentMemoryId is Guid parentId)
            {
                if (!_childrenByParent.TryGetValue(parentId, out SortedSet<Guid>? children))
                {
                    children = new SortedSet<Guid>();
                    _childrenByParent[parentId] = children;
                }

                children.Add(node.MemoryId);
            }
        }
    }

    public MemoryVersionNode? GetNode(Guid memoryId)
    {
        lock (_graphGate)
        {
            return _nodes.TryGetValue(memoryId, out MemoryVersionNode? node) ? node : null;
        }
    }

    public IReadOnlyList<MemoryVersionNode> GetLineagePath(Guid memoryId)
    {
        lock (_graphGate)
        {
            List<MemoryVersionNode> path = new();
            HashSet<Guid> visited = new();
            Guid? current = memoryId;

            while (current is Guid currentId && _nodes.TryGetValue(currentId, out MemoryVersionNode? node))
            {
                if (!visited.Add(currentId))
                {
                    throw new InvalidOperationException("The version graph contains a cycle.");
                }

                path.Add(node);
                current = node.ParentMemoryId;
            }

            path.Reverse();
            return path.ToArray();
        }
    }

    public Guid FindLowestCommonAncestor(Guid memoryIdA, Guid memoryIdB)
    {
        lock (_graphGate)
        {
            HashSet<Guid> ancestorsA = GetAncestorSetUnderLock(memoryIdA);
            Guid? current = memoryIdB;
            while (current is Guid currentId && _nodes.TryGetValue(currentId, out MemoryVersionNode? node))
            {
                if (ancestorsA.Contains(currentId))
                {
                    return currentId;
                }

                current = node.ParentMemoryId;
            }

            throw new InvalidOperationException($"No common ancestor exists for {memoryIdA} and {memoryIdB}.");
        }
    }

    public IReadOnlyList<MemoryVersionNode> GetBranchHeads(Guid rootMemoryId)
    {
        lock (_graphGate)
        {
            return _nodes.Values
                .Where(node => node.RootMemoryId == rootMemoryId &&
                               (!_childrenByParent.TryGetValue(node.MemoryId, out SortedSet<Guid>? children) ||
                                children.Count == 0))
                .OrderBy(node => node.Generation)
                .ThenBy(node => node.MemoryId.ToString("N"), StringComparer.Ordinal)
                .ToArray();
        }
    }

    public MemoryDelta CreateTextDelta(Guid sourceMemoryId, Guid targetMemoryId, IMemoryLedger ledger)
    {
        ArgumentNullException.ThrowIfNull(ledger);
        MemoryRecord source = ledger.GetMemory(sourceMemoryId)
            ?? throw new KeyNotFoundException($"Memory {sourceMemoryId} was not found.");
        MemoryRecord target = ledger.GetMemory(targetMemoryId)
            ?? throw new KeyNotFoundException($"Memory {targetMemoryId} was not found.");

        if (!source.HasTextPayload || !target.HasTextPayload)
        {
            throw new InvalidOperationException("Text deltas require text payload kinds.");
        }

        return MemoryDelta.Create(
            sourceMemoryId,
            targetMemoryId,
            source.GetUtf8Payload(),
            target.GetUtf8Payload());
    }

    public MemoryRecord MergeBranches(
        Guid memoryIdA,
        Guid memoryIdB,
        IMemoryLedger ledger,
        string targetBranchName = "main")
    {
        ArgumentNullException.ThrowIfNull(ledger);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetBranchName);

        Guid lcaId = FindLowestCommonAncestor(memoryIdA, memoryIdB);
        MemoryRecord lca = ledger.GetMemory(lcaId)
            ?? throw new KeyNotFoundException($"LCA memory {lcaId} was not found.");
        MemoryRecord branchA = ledger.GetMemory(memoryIdA)
            ?? throw new KeyNotFoundException($"Memory {memoryIdA} was not found.");
        MemoryRecord branchB = ledger.GetMemory(memoryIdB)
            ?? throw new KeyNotFoundException($"Memory {memoryIdB} was not found.");

        if (!lca.HasTextPayload || !branchA.HasTextPayload || !branchB.HasTextPayload)
        {
            throw new InvalidOperationException("Branch merging currently requires text payload kinds.");
        }

        string baseText = lca.GetUtf8Payload();
        string textA = branchA.GetUtf8Payload();
        string textB = branchB.GetUtf8Payload();
        string mergedText = MergeText(baseText, textA, textB);

        MemoryRecord merged = ledger.AppendMemory(
            branchA.PayloadKind,
            Encoding.UTF8.GetBytes(mergedText),
            parentMemoryId: memoryIdA);
        AddVersionNode(merged, ledger, targetBranchName);
        return merged;
    }

    private MemoryDelta? CreateTextDeltaIfSupported(Guid sourceMemoryId, Guid targetMemoryId, IMemoryLedger ledger)
    {
        MemoryRecord? source = ledger.GetMemory(sourceMemoryId);
        MemoryRecord? target = ledger.GetMemory(targetMemoryId);
        if (source is null || target is null || !source.HasTextPayload || !target.HasTextPayload)
        {
            return null;
        }

        return MemoryDelta.Create(
            sourceMemoryId,
            targetMemoryId,
            source.GetUtf8Payload(),
            target.GetUtf8Payload());
    }

    private static string MergeText(string baseText, string textA, string textB)
    {
        if (textA == textB)
        {
            return textA;
        }

        if (textA == baseText)
        {
            return textB;
        }

        if (textB == baseText)
        {
            return textA;
        }

        List<TextEdit> editsA = ToEdits(MemoryDelta.Create(Guid.Empty, Guid.Empty, baseText, textA));
        List<TextEdit> editsB = ToEdits(MemoryDelta.Create(Guid.Empty, Guid.Empty, baseText, textB));
        List<TextEdit> edits = MergeEdits(editsA, editsB);
        StringBuilder result = new(baseText);

        for (int index = edits.Count - 1; index >= 0; index--)
        {
            TextEdit edit = edits[index];
            result.Remove(edit.Start, edit.DeleteLength);
            result.Insert(edit.Start, edit.InsertText);
        }

        return result.ToString();
    }

    private static List<TextEdit> ToEdits(MemoryDelta delta)
    {
        var edits = new List<TextEdit>();
        int sourceOffset = 0;
        int pendingDelete = 0;
        string pendingInsert = string.Empty;

        void Flush()
        {
            if (pendingDelete != 0 || pendingInsert.Length != 0)
            {
                edits.Add(new TextEdit(sourceOffset - pendingDelete, pendingDelete, pendingInsert));
                pendingDelete = 0;
                pendingInsert = string.Empty;
            }
        }

        foreach (DeltaChunk chunk in delta.Chunks)
        {
            switch (chunk.Kind)
            {
                case DeltaOpKind.Retain:
                    Flush();
                    sourceOffset += chunk.Length;
                    break;
                case DeltaOpKind.Delete:
                    pendingDelete += chunk.Length;
                    sourceOffset += chunk.Length;
                    break;
                case DeltaOpKind.Insert:
                    pendingInsert += chunk.InsertText;
                    break;
            }
        }

        Flush();
        return edits;
    }

    private static List<TextEdit> MergeEdits(IReadOnlyList<TextEdit> editsA, IReadOnlyList<TextEdit> editsB)
    {
        var merged = new List<TextEdit>(editsA.Count + editsB.Count);
        int a = 0;
        int b = 0;

        while (a < editsA.Count || b < editsB.Count)
        {
            if (a == editsA.Count)
            {
                merged.Add(editsB[b++]);
                continue;
            }

            if (b == editsB.Count)
            {
                merged.Add(editsA[a++]);
                continue;
            }

            TextEdit left = editsA[a];
            TextEdit right = editsB[b];
            if (!Overlaps(left, right))
            {
                if (left.Start < right.Start || (left.Start == right.Start && left.DeleteLength > 0))
                {
                    merged.Add(left);
                    a++;
                }
                else
                {
                    merged.Add(right);
                    b++;
                }

                continue;
            }

            if (left == right)
            {
                merged.Add(left);
                a++;
                b++;
                continue;
            }

            throw new MemoryMergeConflictException(left.Start, right.Start);
        }

        merged.Sort(static (left, right) =>
        {
            int start = left.Start.CompareTo(right.Start);
            return start != 0 ? start : right.DeleteLength.CompareTo(left.DeleteLength);
        });
        return merged;
    }

    private static bool Overlaps(TextEdit left, TextEdit right)
    {
        if (left.DeleteLength == 0 && right.DeleteLength == 0)
        {
            return left.Start == right.Start;
        }

        if (left.DeleteLength == 0)
        {
            return left.Start >= right.Start && left.Start < right.Start + right.DeleteLength;
        }

        if (right.DeleteLength == 0)
        {
            return right.Start >= left.Start && right.Start < left.Start + left.DeleteLength;
        }

        return left.Start < right.Start + right.DeleteLength &&
               right.Start < left.Start + left.DeleteLength;
    }

    private readonly record struct TextEdit(int Start, int DeleteLength, string InsertText);

    private HashSet<Guid> GetAncestorSetUnderLock(Guid memoryId)
    {
        HashSet<Guid> ancestors = new();
        Guid? current = memoryId;
        while (current is Guid currentId && _nodes.TryGetValue(currentId, out MemoryVersionNode? node))
        {
            if (!ancestors.Add(currentId))
            {
                throw new InvalidOperationException("The version graph contains a cycle.");
            }

            current = node.ParentMemoryId;
        }

        return ancestors;
    }
}
