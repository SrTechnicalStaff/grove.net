using System;
using System.Collections.Generic;
using System.Linq;
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
        public double Mass { get; set; } = Tokens.FieldGain;
        public double Weight { get; set; } = 0.0;
        public Color SourceColor { get; set; } = Colors.NoteViolet;
    }

    /// <summary>
    /// Cell Ledger Entry holding Col (Cell X), Row (Cell Y), FieldEnergy (accumulated aura gravity value, baseline 0.05),
    /// CompositeColor, and SourceMetadata mapping Content ID -> metadata source.
    /// </summary>
    public class CellLedgerEntry
    {
        public int Col { get; set; }
        public int Row { get; set; }
        public double FieldEnergy { get; set; } = 0.05; // Default non-zero baseline 0.05
        public Color CompositeColor { get; set; } = Colors.SurfaceGrid;
        public Dictionary<string, CellMetadataSource> SourceMetadata { get; } = new();

        public CellLedgerEntry(int col, int row)
        {
            Col = col;
            Row = row;
        }
    }

    /// <summary>
    /// Interface for field subscribers notified whenever a cell's field ledger entry is updated.
    /// </summary>
    public interface IFieldSubscriber
    {
        void OnCellFieldUpdated(int col, int row, CellLedgerEntry entry);
    }

    /// <summary>
    /// Computes cell fill composite colors based on surface grid background and accumulated note field weights.
    /// </summary>
    public class AuraHeatmapSubscriber : IFieldSubscriber
    {
        public void OnCellFieldUpdated(int col, int row, CellLedgerEntry entry)
        {
            Color baseColor = Colors.SurfaceGrid;
            double r = baseColor.R;
            double g = baseColor.G;
            double b = baseColor.B;

            foreach (var source in entry.SourceMetadata.Values)
            {
                r += source.SourceColor.R * source.Weight;
                g += source.SourceColor.G * source.Weight;
                b += source.SourceColor.B * source.Weight;
            }

            byte finalR = (byte)Math.Clamp(r, 0, 255);
            byte finalG = (byte)Math.Clamp(g, 0, 255);
            byte finalB = (byte)Math.Clamp(b, 0, 255);

            entry.CompositeColor = Color.FromRgb(finalR, finalG, finalB);
        }
    }

    /// <summary>
    /// Detects energy boundaries and tracks saturated vs. perimeter cells across field energy regions.
    /// </summary>
    public class PerimeterRingSubscriber : IFieldSubscriber
    {
        public HashSet<(int col, int row)> SaturatedCells { get; } = new();
        public HashSet<(int col, int row)> PerimeterCells { get; } = new();

        public void OnCellFieldUpdated(int col, int row, CellLedgerEntry entry)
        {
            var key = (col, row);
            if (entry.FieldEnergy > 0.051)
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
    public class AnnotationMetadataSubscriber : IFieldSubscriber
    {
        public Dictionary<(int col, int row), List<CellMetadataSource>> AggregatedMetadata { get; } = new();

        public void OnCellFieldUpdated(int col, int row, CellLedgerEntry entry)
        {
            var key = (col, row);
            if (entry.SourceMetadata.Count > 0)
            {
                AggregatedMetadata[key] = entry.SourceMetadata.Values.ToList();
            }
            else
            {
                AggregatedMetadata.Remove(key);
            }
        }

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
                        foreach (var src in list)
                        {
                            if (seenIds.Add(src.ContentId))
                            {
                                results.Add(src);
                            }
                        }
                    }
                }
            }
            return results;
        }
    }

    /// <summary>
    /// Core engine managing cell ledger entries, spatial lookups, field energy accumulation, and field subscribers.
    /// </summary>
    public class FieldLedgerEngine
    {
        private readonly Dictionary<(int col, int row), CellLedgerEntry> _ledger = new();
        private readonly List<IFieldSubscriber> _subscribers = new();

        public AuraHeatmapSubscriber HeatmapSubscriber { get; } = new();
        public PerimeterRingSubscriber PerimeterSubscriber { get; } = new();
        public AnnotationMetadataSubscriber AnnotationSubscriber { get; } = new();

        public FieldLedgerEngine()
        {
            RegisterSubscriber(HeatmapSubscriber);
            RegisterSubscriber(PerimeterSubscriber);
            RegisterSubscriber(AnnotationSubscriber);
        }

        public void RegisterSubscriber(IFieldSubscriber subscriber)
        {
            if (!_subscribers.Contains(subscriber))
                _subscribers.Add(subscriber);
        }

        public void UnregisterSubscriber(IFieldSubscriber subscriber)
        {
            _subscribers.Remove(subscriber);
        }

        /// <summary>
        /// Spatial lookup obtaining the cell ledger entry for (col, row).
        /// </summary>
        public CellLedgerEntry GetCellLedger(int col, int row)
        {
            if (_ledger.TryGetValue((col, row), out var entry))
            {
                return entry;
            }
            return new CellLedgerEntry(col, row)
            {
                FieldEnergy = 0.05,
                CompositeColor = Colors.SurfaceGrid
            };
        }

        /// <summary>
        /// Recalculates field energy, composite colors, and metadata sources across a bounding range of cells.
        /// </summary>
        public void RecalculateField(IEnumerable<GridContentItem> items, int minCol, int maxCol, int minRow, int maxRow)
        {
            var itemList = items.ToList();
            for (int c = minCol; c <= maxCol; c++)
            {
                for (int r = minRow; r <= maxRow; r++)
                {
                    UpdateCell(c, r, itemList);
                }
            }
            PerimeterSubscriber.RecomputePerimeter();
        }

        /// <summary>
        /// Recalculates field energy, composite colors, and metadata sources strictly for specified active aura envelope cells.
        /// Implements Spatial Aura Bounding-Box Culling (d <= 6 cells).
        /// </summary>
        public void RecalculateField(IEnumerable<GridContentItem> items, IEnumerable<(int col, int row)> activeCells)
        {
            var itemList = items as List<GridContentItem> ?? items.ToList();
            foreach (var (c, r) in activeCells)
            {
                UpdateCell(c, r, itemList);
            }
            PerimeterSubscriber.RecomputePerimeter();
        }

        /// <summary>
        /// Updates a single cell's accumulated energy, weight formula W_i = M_i / (1 + 0.4 * d_i^2), and notifies subscribers.
        /// </summary>
        public void UpdateCell(int col, int row, IEnumerable<GridContentItem> items)
        {
            var key = (col, row);
            if (!_ledger.TryGetValue(key, out var entry))
            {
                entry = new CellLedgerEntry(col, row);
                _ledger[key] = entry;
            }

            entry.SourceMetadata.Clear();
            double accumulatedEnergy = 0.05; // default non-zero baseline

            foreach (var item in items)
            {
                // Distance squared from cell (col, row) to item footprint
                double dx = 0.0;
                if (col < item.CellX)
                    dx = item.CellX - col;
                else if (col >= item.CellX + item.CellWidth)
                    dx = col - (item.CellX + item.CellWidth - 1);

                double dy = 0.0;
                if (row < item.CellY)
                    dy = item.CellY - row;
                else if (row >= item.CellY + item.CellHeight)
                    dy = row - (item.CellY + item.CellHeight - 1);

                // Spatial aura bounding-box culling check: d <= 6 cells
                if (dx > 6 || dy > 6) continue;

                double distSq = dx * dx + dy * dy;
                double mass = Tokens.FieldGain * item.Mass; // M_i = FieldGain * Mass (Note: 1.0, Doc: 2.5, Image: 4.0)
                double weight = mass / (1.0 + 0.4 * distSq); // W_i = M_i / (1 + 0.4 * d_i^2)

                if (weight > 0.001)
                {
                    accumulatedEnergy += weight;
                    string contentId = item.Id;
                    Color hue = Color.Parse(item.FieldHueHex);

                    string snippet = item switch
                    {
                        GridNote n => n.Text.Length > 30 ? n.Text.Substring(0, 30) + "..." : n.Text,
                        GridDocument d => d.Title,
                        GridImage img => System.IO.Path.GetFileName(img.FilePath),
                        _ => item.Id
                    };

                    entry.SourceMetadata[contentId] = new CellMetadataSource
                    {
                        ContentId = contentId,
                        TextSnippet = snippet,
                        LayerId = "Layer0",
                        Mass = mass,
                        Weight = weight,
                        SourceColor = hue
                    };
                }
            }

            entry.FieldEnergy = Math.Min(Tokens.FieldAlphaMax, accumulatedEnergy);

            foreach (var sub in _subscribers)
            {
                sub.OnCellFieldUpdated(col, row, entry);
            }
        }

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

        /// <summary>
        /// Returns total count of aggregated metadata sources across all saturated cells.
        /// </summary>
        public int GetTotalMetadataSourcesCount()
        {
            return AnnotationSubscriber.AggregatedMetadata.Values.Sum(list => list.Count);
        }
    }
}
