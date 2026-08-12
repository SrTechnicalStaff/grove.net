using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media;
using GroveApp.DesignSystem;
using GroveApp.Models;
using Colors = GroveApp.DesignSystem.Colors;

namespace GroveApp.Engine
{
    /// <summary>
    /// Represents a content metadata source contributing to a cell's field energy.
    /// Weight W_i = M_i / (1 + 0.4 * d_i^2).
    /// </summary>
    public class CellMetadataSource
    {
        public string ContentId { get; set; } = string.Empty;
        public string TextSnippet { get; set; } = string.Empty;
        public string LayerId { get; set; } = "Layer0";
        public ushort ContentKind { get; set; }
        public double Mass { get; set; } = 1.0;
        public double Weight { get; set; }
        public Color SourceColor { get; set; } = Colors.NoteViolet;
    }

    /// <summary>
    /// Packed coordinate used by the field-ledger value object.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public readonly record struct GridCellPosition(int X, int Y)
    {
        public long ToSpatialKey() => ((long)X << 32) | (uint)Y;
    }

    /// <summary>
    /// Fixed-width source provenance carried inline by a field-ledger entry.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public readonly record struct FieldSourceMetadata
    {
        public Guid ContentId { get; init; }
        public Guid LayerId { get; init; }
        public ushort ContentKind { get; init; }
        public float Mass { get; init; }
        public float ContributedEnergy { get; init; }
        public Vector4 ColorHue { get; init; }
        public long TimestampTicks { get; init; }
    }

    /// <summary>
    /// Immutable cell-ledger value. Legacy Col/Row, FieldEnergy, and CompositeColor
    /// accessors remain available to the renderer while hot source provenance is
    /// carried through four inline source slots.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public readonly record struct CellLedgerEntry
    {
        public const float BaselineEnergy = 0.05f;

        public GridCellPosition Position { get; init; }
        public Guid LayerId { get; init; }
        public float TotalEnergy { get; init; }
        public float FieldEnergy => TotalEnergy;
        public Color CompositeColor { get; init; }
        public Vector4 PrimaryHue { get; init; }
        public int SourceCount { get; init; }

        public FieldSourceMetadata InlineSource0 { get; init; }
        public FieldSourceMetadata InlineSource1 { get; init; }
        public FieldSourceMetadata InlineSource2 { get; init; }
        public FieldSourceMetadata InlineSource3 { get; init; }

        // Compatibility aliases retained for existing renderer callers.
        public int Col => Position.X;
        public int Row => Position.Y;

        public CellLedgerEntry(int col, int row)
        {
            Position = new GridCellPosition(col, row);
            LayerId = Guid.Empty;
            TotalEnergy = BaselineEnergy;
            CompositeColor = Colors.SurfaceGrid;
            PrimaryHue = ToNormalizedHue(Colors.SurfaceGrid);
            SourceCount = 0;
            InlineSource0 = default;
            InlineSource1 = default;
            InlineSource2 = default;
            InlineSource3 = default;
        }

        private static Vector4 ToNormalizedHue(Color color) => new(
            color.R / 255f,
            color.G / 255f,
            color.B / 255f,
            color.A / 255f);
    }

    /// <summary>
    /// Subscriber seam for single-cell and region-batch field updates.
    /// The legacy overload remains a default adapter for existing subscribers.
    /// </summary>
    public interface IFieldSubscriber
    {
        string SubscriberId => GetType().FullName ?? GetType().Name;

        void OnCellFieldUpdated(in CellLedgerEntry entry)
        {
            OnCellFieldUpdated(entry.Col, entry.Row, entry);
        }

        void OnCellFieldUpdated(int col, int row, CellLedgerEntry entry)
        {
        }

        void OnRegionFieldBatchUpdated(ReadOnlySpan<CellLedgerEntry> regionEntries)
        {
            for (int i = 0; i < regionEntries.Length; i++)
            {
                OnCellFieldUpdated(in regionEntries[i]);
            }
        }
    }

    /// <summary>
    /// Computes the additive energy-weighted hue for a cell and its render alpha.
    /// Hues are summed and channel-clamped; no midpoint colour is computed.
    /// </summary>
    public sealed class AuraHeatmapSubscriber : IFieldSubscriber
    {
        public string SubscriberId => "subscriber.aura-heatmap";

        public CellLedgerEntry ApplyCompositeColor(in CellLedgerEntry entry)
        {
            return entry with { CompositeColor = CalculateCompositeColor(entry) };
        }

        public Color CalculateCompositeColor(in CellLedgerEntry entry)
        {
            if (entry.SourceCount == 0)
            {
                return Colors.SurfaceGrid;
            }

            double totalWeight = 0.0;
            double red = 0.0;
            double green = 0.0;
            double blue = 0.0;

            Accumulate(entry.InlineSource0, ref totalWeight, ref red, ref green, ref blue);
            Accumulate(entry.InlineSource1, ref totalWeight, ref red, ref green, ref blue);
            Accumulate(entry.InlineSource2, ref totalWeight, ref red, ref green, ref blue);
            Accumulate(entry.InlineSource3, ref totalWeight, ref red, ref green, ref blue);

            if (totalWeight <= 0.0)
            {
                return Colors.SurfaceGrid;
            }

            double alpha = Math.Clamp(
                entry.FieldEnergy * Tokens.FieldGain,
                Tokens.FieldAlphaMin,
                Tokens.FieldAlphaMax);

            return Color.FromArgb(
                ToByte(alpha * byte.MaxValue),
                ToByte(red),
                ToByte(green),
                ToByte(blue));
        }

        public void OnCellFieldUpdated(in CellLedgerEntry entry)
        {
            // The engine applies the returned value before publishing it, keeping
            // this subscriber pure at the value-object seam.
        }

        public void OnRegionFieldBatchUpdated(ReadOnlySpan<CellLedgerEntry> regionEntries)
        {
            // Composite colors are applied while each entry is constructed.
        }

        private static byte ToByte(double value) => (byte)Math.Clamp(Math.Round(value), 0.0, 255.0);

        private static void Accumulate(
            FieldSourceMetadata source,
            ref double totalWeight,
            ref double red,
            ref double green,
            ref double blue)
        {
            double weight = source.ContributedEnergy;
            if (weight <= 0.0)
            {
                return;
            }

            totalWeight += weight;
            red += source.ColorHue.X * byte.MaxValue * weight;
            green += source.ColorHue.Y * byte.MaxValue * weight;
            blue += source.ColorHue.Z * byte.MaxValue * weight;
        }
    }

    /// <summary>
    /// Detects energy boundaries and tracks saturated versus perimeter cells.
    /// </summary>
    public sealed class PerimeterRingSubscriber : IFieldSubscriber
    {
        public const double PerimeterThresholdEnergy = 0.15;

        public string SubscriberId => "subscriber.perimeter-ring";
        public HashSet<(int col, int row)> SaturatedCells { get; } = new();
        public HashSet<(int col, int row)> PerimeterCells { get; } = new();

        public void OnCellFieldUpdated(in CellLedgerEntry entry)
        {
            var key = (entry.Col, entry.Row);
            if (entry.FieldEnergy >= PerimeterThresholdEnergy)
            {
                SaturatedCells.Add(key);
            }
            else
            {
                SaturatedCells.Remove(key);
                PerimeterCells.Remove(key);
            }
        }

        public void RecomputePerimeter()
        {
            PerimeterCells.Clear();
            foreach (var (c, r) in SaturatedCells)
            {
                bool isEdge = !SaturatedCells.Contains((c - 1, r)) ||
                              !SaturatedCells.Contains((c + 1, r)) ||
                              !SaturatedCells.Contains((c, r - 1)) ||
                              !SaturatedCells.Contains((c, r + 1));
                if (isEdge)
                {
                    PerimeterCells.Add((c, r));
                }
            }
        }
    }

    /// <summary>
    /// Aggregates metadata across saturated cells for spatial annotation queries.
    /// </summary>
    public sealed class AnnotationMetadataSubscriber : IFieldSubscriber
    {
        public const double QualificationEnergy = 1.0;
        public const int SupportedCellQualification = 24;
        public string SubscriberId => "subscriber.annotation-metadata";
        public Dictionary<(int col, int row), List<CellMetadataSource>> AggregatedMetadata { get; } = new();
        public HashSet<(int col, int row)> QualifiedCells { get; } = new();
        public event Action<IReadOnlyList<CellLedgerEntry>>? QualifiedRegionDetected;

        public void OnCellFieldUpdated(in CellLedgerEntry entry)
        {
            var key = (entry.Col, entry.Row);
            if (entry.SourceCount >= 2 && entry.FieldEnergy >= QualificationEnergy)
            {
                QualifiedCells.Add(key);
            }
            else
            {
                QualifiedCells.Remove(key);
            }

            if (entry.SourceCount > 0)
            {
                var sources = new List<CellMetadataSource>(entry.SourceCount);
                AddSource(sources, entry.InlineSource0);
                AddSource(sources, entry.InlineSource1);
                AddSource(sources, entry.InlineSource2);
                AddSource(sources, entry.InlineSource3);
                AggregatedMetadata[key] = sources;
            }
            else
            {
                AggregatedMetadata.Remove(key);
            }
        }

        public void OnRegionFieldBatchUpdated(ReadOnlySpan<CellLedgerEntry> regionEntries)
        {
            var qualified = new List<CellLedgerEntry>();
            for (int i = 0; i < regionEntries.Length; i++)
            {
                OnCellFieldUpdated(in regionEntries[i]);
                if (regionEntries[i].SourceCount >= 2 && regionEntries[i].FieldEnergy >= QualificationEnergy)
                {
                    qualified.Add(regionEntries[i]);
                }
            }

            if (qualified.Count >= SupportedCellQualification)
            {
                QualifiedRegionDetected?.Invoke(qualified);
            }
        }

        private static void AddSource(List<CellMetadataSource> target, FieldSourceMetadata source)
        {
            if (source.ContributedEnergy <= 0.0 || source.ContentId == Guid.Empty)
            {
                return;
            }

            target.Add(new CellMetadataSource
            {
                ContentId = source.ContentId.ToString("N"),
                TextSnippet = source.ContentId.ToString("N"),
                LayerId = source.LayerId == Guid.Empty ? "Layer0" : source.LayerId.ToString("N"),
                ContentKind = source.ContentKind,
                Mass = source.Mass,
                Weight = source.ContributedEnergy,
                SourceColor = Color.FromArgb(
                    ToByte(source.ColorHue.W * byte.MaxValue),
                    ToByte(source.ColorHue.X * byte.MaxValue),
                    ToByte(source.ColorHue.Y * byte.MaxValue),
                    ToByte(source.ColorHue.Z * byte.MaxValue))
            });
        }

        private static byte ToByte(float value) => (byte)Math.Clamp(Math.Round(value), 0.0, 255.0);

        public List<CellMetadataSource> QueryRegion(int minCol, int maxCol, int minRow, int maxRow)
        {
            var results = new List<CellMetadataSource>();
            var seenIds = new HashSet<string>();

            for (int c = minCol; c <= maxCol; c++)
            {
                for (int r = minRow; r <= maxRow; r++)
                {
                    if (AggregatedMetadata.TryGetValue((c, r), out var list))
                    {
                        foreach (var source in list)
                        {
                            if (seenIds.Add(source.ContentId))
                            {
                                results.Add(source);
                            }
                        }
                    }
                }
            }

            return results;
        }
    }

    /// <summary>
    /// Computes and publishes bounded spatial field entries.
    /// </summary>
    public sealed class FieldLedgerEngine
    {
        private readonly Dictionary<(int col, int row), CellLedgerEntry> _ledger = new();
        private readonly List<IFieldSubscriber> _subscribers = new();

        public const int MaxLayerPermeabilityDepth = 3;
        public const double LayerPermeabilityDecay = 0.5;

        /// <summary>
        /// Active layer used when computing the next field snapshot.
        /// </summary>
        public int ActiveLayerId { get; set; }

        /// <summary>
        /// Resolves stack distance between source and target layers. The default
        /// adapter supports the base integer layer model; the canvas supplies the
        /// ordered-stack adapter when layers are present.
        /// </summary>
        public Func<int, int, int> LayerDeltaResolver { get; set; } =
            static (sourceLayerId, targetLayerId) => Math.Abs(sourceLayerId - targetLayerId);

        public AuraHeatmapSubscriber HeatmapSubscriber { get; } = new();
        public PerimeterRingSubscriber PerimeterSubscriber { get; } = new();
        public AnnotationMetadataSubscriber AnnotationSubscriber { get; } = new();

        /// <summary>
        /// Produces the canonical visible aura cell set shared by preparation and
        /// rendering. The envelope is bounded for work, then trimmed to Euclidean
        /// distance so the culling rule and the field equation agree at the corners.
        /// </summary>
        public static HashSet<(int col, int row)> GetAuraCells(
            IEnumerable<GridContentItem> items,
            int minCol,
            int maxCol,
            int minRow,
            int maxRow)
        {
            var auraCells = new HashSet<(int col, int row)>();
            int radius = Tokens.MaxCullingRadiusCells;
            double radiusSquared = radius * radius;

            foreach (var item in items)
            {
                int itemMinX = Math.Max(minCol, item.CellX - radius);
                int itemMaxX = Math.Min(maxCol, item.CellX + item.CellWidth - 1 + radius);
                int itemMinY = Math.Max(minRow, item.CellY - radius);
                int itemMaxY = Math.Min(maxRow, item.CellY + item.CellHeight - 1 + radius);

                for (int col = itemMinX; col <= itemMaxX; col++)
                {
                    for (int row = itemMinY; row <= itemMaxY; row++)
                    {
                        if (DistanceToFootprintSquared(item, col, row) <= radiusSquared)
                        {
                            auraCells.Add((col, row));
                        }
                    }
                }
            }

            return auraCells;
        }

        public FieldLedgerEngine()
        {
            RegisterSubscriber(HeatmapSubscriber);
            RegisterSubscriber(PerimeterSubscriber);
            RegisterSubscriber(AnnotationSubscriber);
        }

        public void RegisterSubscriber(IFieldSubscriber subscriber)
        {
            if (!_subscribers.Contains(subscriber))
            {
                _subscribers.Add(subscriber);
            }
        }

        public void UnregisterSubscriber(IFieldSubscriber subscriber) => _subscribers.Remove(subscriber);

        /// <summary>
        /// Spatial lookup obtaining the value for (col, row).
        /// </summary>
        public CellLedgerEntry GetCellLedger(int col, int row)
        {
            return _ledger.TryGetValue((col, row), out var entry)
                ? entry
                : new CellLedgerEntry(col, row);
        }

        public void Clear()
        {
            _ledger.Clear();
            PerimeterSubscriber.SaturatedCells.Clear();
            PerimeterSubscriber.PerimeterCells.Clear();
            AnnotationSubscriber.AggregatedMetadata.Clear();
        }

        /// <summary>
        /// Recalculates a rectangular region and publishes one batch update.
        /// </summary>
        public void RecalculateField(IEnumerable<GridContentItem> items, int minCol, int maxCol, int minRow, int maxRow)
        {
            ResetSnapshot();
            var itemList = items.ToList();
            int width = Math.Max(0, maxCol - minCol + 1);
            int height = Math.Max(0, maxRow - minRow + 1);
            var updatedEntries = new CellLedgerEntry[width * height];
            int entryIndex = 0;

            for (int c = minCol; c <= maxCol; c++)
            {
                for (int r = minRow; r <= maxRow; r++)
                {
                    updatedEntries[entryIndex++] = UpdateCellCore(c, r, itemList);
                }
            }

            NotifyBatch(updatedEntries);
            PerimeterSubscriber.RecomputePerimeter();
        }

        /// <summary>
        /// Recalculates the specified active aura cells and publishes one batch update.
        /// </summary>
        public void RecalculateField(IEnumerable<GridContentItem> items, IEnumerable<(int col, int row)> activeCells)
        {
            ResetSnapshot();
            var itemList = items as List<GridContentItem> ?? items.ToList();
            if (activeCells is ICollection<(int col, int row)> cellCollection)
            {
                var updatedEntries = new CellLedgerEntry[cellCollection.Count];
                int entryIndex = 0;
                foreach (var (c, r) in cellCollection)
                {
                    updatedEntries[entryIndex++] = UpdateCellCore(c, r, itemList);
                }

                NotifyBatch(updatedEntries);
            }
            else
            {
                var updatedEntries = new List<CellLedgerEntry>();
                foreach (var (c, r) in activeCells)
                {
                    updatedEntries.Add(UpdateCellCore(c, r, itemList));
                }

                NotifyBatch(updatedEntries.ToArray());
            }
            PerimeterSubscriber.RecomputePerimeter();
        }

        /// <summary>
        /// Updates one cell and publishes a single-cell notification.
        /// </summary>
        public void UpdateCell(int col, int row, IEnumerable<GridContentItem> items)
        {
            var entry = UpdateCellCore(col, row, items);
            NotifySingle(entry);
            PerimeterSubscriber.RecomputePerimeter();
        }

        private CellLedgerEntry UpdateCellCore(int col, int row, IEnumerable<GridContentItem> items)
        {
            var sourceMetadata = new Dictionary<string, CellMetadataSource>();
            double accumulatedEnergy = CellLedgerEntry.BaselineEnergy;

            foreach (var item in items)
            {
                double dx = 0.0;
                if (col < item.CellX)
                {
                    dx = item.CellX - col;
                }
                else if (col >= item.CellX + item.CellWidth)
                {
                    dx = col - (item.CellX + item.CellWidth - 1);
                }

                double dy = 0.0;
                if (row < item.CellY)
                {
                    dy = item.CellY - row;
                }
                else if (row >= item.CellY + item.CellHeight)
                {
                    dy = row - (item.CellY + item.CellHeight - 1);
                }

                // ADR-003's axis-aligned bounding-box culling envelope.
                if (dx > Tokens.MaxCullingRadiusCells || dy > Tokens.MaxCullingRadiusCells)
                {
                    continue;
                }

                int layerDelta = LayerDeltaResolver(item.LayerId, ActiveLayerId);
                if (layerDelta > MaxLayerPermeabilityDepth)
                {
                    continue;
                }

                double distSq = dx * dx + dy * dy;
                if (distSq > Tokens.MaxCullingRadiusCells * Tokens.MaxCullingRadiusCells)
                {
                    continue;
                }
                double mass = GetBaseMass(item);
                double layerAttenuation = Math.Pow(LayerPermeabilityDecay, layerDelta);
                double weight = (mass / (1.0 + 0.4 * distSq)) * layerAttenuation;

                if (weight <= 0.001)
                {
                    continue;
                }

                accumulatedEnergy += weight;
                Color hue = Color.Parse(item.FieldHueHex);
                string contentId = item.Id;
                string snippet = item switch
                {
                    GridNote note => note.Text.Length > 30 ? note.Text.Substring(0, 30) + "..." : note.Text,
                    GridDocument document => document.Title,
                    GridImage image => System.IO.Path.GetFileName(image.FilePath),
                    _ => item.Id
                };

                sourceMetadata[contentId] = new CellMetadataSource
                {
                    ContentId = contentId,
                    TextSnippet = snippet,
                    LayerId = GetLayerGuid(item.LayerId).ToString("N"),
                    ContentKind = (ushort)item.Kind,
                    Mass = mass,
                    Weight = weight,
                    SourceColor = hue
                };
            }

            var firstSourceColor = sourceMetadata.Values.FirstOrDefault()?.SourceColor ?? Colors.SurfaceGrid;
            var entry = new CellLedgerEntry(col, row)
            {
                LayerId = GetLayerGuid(ActiveLayerId),
                TotalEnergy = (float)accumulatedEnergy,
                SourceCount = Math.Min(sourceMetadata.Count, 4),
                PrimaryHue = sourceMetadata.Count == 0
                    ? ToNormalizedHue(Colors.SurfaceGrid)
                    : ToNormalizedHue(firstSourceColor)
            };

            var inlineSources = sourceMetadata.Values.Take(4).Select(ToInlineSource).ToArray();
            entry = entry with
            {
                InlineSource0 = inlineSources.Length > 0 ? inlineSources[0] : default,
                InlineSource1 = inlineSources.Length > 1 ? inlineSources[1] : default,
                InlineSource2 = inlineSources.Length > 2 ? inlineSources[2] : default,
                InlineSource3 = inlineSources.Length > 3 ? inlineSources[3] : default
            };

            entry = HeatmapSubscriber.ApplyCompositeColor(entry);
            _ledger[(col, row)] = entry;
            return entry;
        }

        private static double GetBaseMass(GridContentItem item) => item.Kind switch
        {
            ContentKind.Note => 1.0,
            ContentKind.Document => 2.5,
            ContentKind.Image => 4.0,
            _ => 1.0
        };

        private static double DistanceToFootprintSquared(GridContentItem item, int col, int row)
        {
            double dx = col < item.CellX
                ? item.CellX - col
                : col >= item.CellX + item.CellWidth
                    ? col - (item.CellX + item.CellWidth - 1)
                    : 0;
            double dy = row < item.CellY
                ? item.CellY - row
                : row >= item.CellY + item.CellHeight
                    ? row - (item.CellY + item.CellHeight - 1)
                    : 0;
            return dx * dx + dy * dy;
        }

        private void ResetSnapshot() => Clear();

        private void NotifySingle(CellLedgerEntry entry)
        {
            foreach (var subscriber in _subscribers)
            {
                subscriber.OnCellFieldUpdated(in entry);
            }
        }

        private void NotifyBatch(CellLedgerEntry[] entries)
        {
            if (entries.Length == 0)
            {
                return;
            }

            ReadOnlySpan<CellLedgerEntry> batch = entries;
            foreach (var subscriber in _subscribers)
            {
                subscriber.OnRegionFieldBatchUpdated(batch);
            }
        }

        private static FieldSourceMetadata ToInlineSource(CellMetadataSource source)
        {
            return new FieldSourceMetadata
            {
                ContentId = TryParseGuid(source.ContentId),
                LayerId = TryParseGuid(source.LayerId),
                ContentKind = source.ContentKind,
                Mass = (float)source.Mass,
                ContributedEnergy = (float)source.Weight,
                ColorHue = ToNormalizedHue(source.SourceColor),
                TimestampTicks = DateTime.UtcNow.Ticks
            };
        }

        private static Guid TryParseGuid(string value) => Guid.TryParse(value, out var result) ? result : Guid.Empty;

        private static Guid GetLayerGuid(int layerId) => new(layerId, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        private static Vector4 ToNormalizedHue(Color color) => new(
            color.R / 255f,
            color.G / 255f,
            color.B / 255f,
            color.A / 255f);

        /// <summary>
        /// Spatial query gathering metadata sources for cells intersecting cellBounds.
        /// </summary>
        public List<CellMetadataSource> QueryMetadataInRegion(Rect cellBounds)
        {
            int minCol = (int)Math.Floor(cellBounds.X);
            int maxCol = (int)Math.Ceiling(cellBounds.Right);
            int minRow = (int)Math.Floor(cellBounds.Y);
            int maxRow = (int)Math.Ceiling(cellBounds.Bottom);

            return AnnotationSubscriber.QueryRegion(minCol, maxCol, minRow, maxRow);
        }

        public int GetTotalMetadataSourcesCount() =>
            AnnotationSubscriber.AggregatedMetadata.Values.Sum(list => list.Count);
    }
}
