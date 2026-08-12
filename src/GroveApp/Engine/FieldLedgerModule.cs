using System;
using System.Collections.Generic;

using Avalonia;
using Avalonia.Media;
using GroveApp.DesignSystem;
using GroveApp.Models;
using Colors = GroveApp.DesignSystem.Colors;

namespace GroveApp.Engine
{
    /// <summary>
    /// Engine module for high-precision gravitational presence accumulation per cell,
    /// cell background fills, multi-layer atmosphere, and cell presence perimeter rings.
    /// </summary>
    public class FieldLedgerModule
    {
        /// <summary>
        /// Calculates the gravitational presence accumulation color for cell (cx, cy) from all content items.
        /// Restricts calculations strictly to active aura envelopes (d <= 6 cells).
        /// </summary>
        public Color CalculateCellFieldColor(int cx, int cy, IEnumerable<GridContentItem> items)
        {
            Color baseColor = Colors.SurfaceGrid;
            double r = baseColor.R;
            double g = baseColor.G;
            double b = baseColor.B;

            foreach (var item in items)
            {
                double dx = 0.0;
                if (cx < item.CellX)
                    dx = item.CellX - cx;
                else if (cx >= item.CellX + item.CellWidth)
                    dx = cx - (item.CellX + item.CellWidth - 1);

                double dy = 0.0;
                if (cy < item.CellY)
                    dy = item.CellY - cy;
                else if (cy >= item.CellY + item.CellHeight)
                    dy = cy - (item.CellY + item.CellHeight - 1);

                // Spatial aura bounding-box culling: d <= 6 cells
                if (dx > 6 || dy > 6) continue;

                double distSq = dx * dx + dy * dy;

                // Field gravity equation: FieldWeight = (FieldGain * Mass) / (1 + 0.4 * distSq)
                double fieldWeight = (Tokens.FieldGain * item.Mass) / (1.0 + 0.4 * distSq);
                Color fieldColor = Color.Parse(item.FieldHueHex);

                r += fieldColor.R * fieldWeight;
                g += fieldColor.G * fieldWeight;
                b += fieldColor.B * fieldWeight;
            }

            byte finalR = (byte)Math.Clamp(r, 0, 255);
            byte finalG = (byte)Math.Clamp(g, 0, 255);
            byte finalB = (byte)Math.Clamp(b, 0, 255);

            return Color.FromRgb(finalR, finalG, finalB);
        }

        /// <summary>
        /// Renders the complete multi-layer field ledger atmosphere, background fills, and perimeter rings.
        /// Applies Spatial Aura Bounding-Box Culling (d <= 6 cells) and LOD Distance Shedding when projected cell size < 6px.
        /// </summary>
        public void RenderFieldLedger(
            DrawingContext context,
            Func<Point, Point> worldToScreen,
            double cellSize,
            double zoom,
            int minCellX,
            int maxCellX,
            int minCellY,
            int maxCellY,
            FieldLedgerEngine fieldEngine,
            IEnumerable<GridContentItem> items)
        {
            var itemList = items as List<GridContentItem> ?? new List<GridContentItem>(items);
            if (itemList.Count == 0) return;

            double projectedCellSize = cellSize * zoom;
            const int auraRadius = 6;

            // Enforce LOD Distance Shedding: Shed cell field calculations & cell fill matrix quads when projected cell size < 6px (Tokens.GridFadeStart)
            bool enableCellFills = projectedCellSize >= Tokens.GridFadeStart;

            if (enableCellFills)
            {
                // Spatial Aura Bounding-Box Culling (d <= 6 cells):
                // Collect unique active aura envelope cells across all content items in the viewport
                var activeAuraCells = new HashSet<(int col, int row)>();

                foreach (var item in itemList)
                {
                    int nMinX = Math.Max(minCellX, item.CellX - auraRadius);
                    int nMaxX = Math.Min(maxCellX, item.CellX + item.CellWidth - 1 + auraRadius);
                    int nMinY = Math.Max(minCellY, item.CellY - auraRadius);
                    int nMaxY = Math.Min(maxCellY, item.CellY + item.CellHeight - 1 + auraRadius);

                    if (nMinX <= nMaxX && nMinY <= nMaxY)
                    {
                        for (int cx = nMinX; cx <= nMaxX; cx++)
                        {
                            for (int cy = nMinY; cy <= nMaxY; cy++)
                            {
                                activeAuraCells.Add((cx, cy));
                            }
                        }
                    }
                }

                if (activeAuraCells.Count > 0)
                {
                    // 0. Recalculate Field Ledger ONLY for active aura envelope cells
                    fieldEngine.RecalculateField(itemList, activeAuraCells);

                    // 1. Layer 0: High-Precision Cell Fill Matrix for active aura cells
                    foreach (var (cx, cy) in activeAuraCells)
                    {
                        CellLedgerEntry entry = fieldEngine.GetCellLedger(cx, cy);
                        if (entry.FieldEnergy > 0.051 && entry.CompositeColor != Colors.SurfaceGrid)
                        {
                            Point startScreen = worldToScreen(new Point(cx * cellSize, cy * cellSize));
                            double sizeScreen = cellSize * zoom;
                            Rect cellRect = new Rect(startScreen.X, startScreen.Y, sizeScreen, sizeScreen);

                            var cellBrush = new SolidColorBrush(entry.CompositeColor);
                            context.FillRectangle(cellBrush, cellRect);
                        }
                    }
                }
            }

            // 2. Layer 1: Gravitational Atmosphere Radial Glows around Item Footprints (LOD Shedding when projected cell size < 6px)
            if (projectedCellSize >= Tokens.GridFadeStart)
            {
                foreach (var item in itemList)
                {
                    if (item.CellX + item.CellWidth + auraRadius < minCellX || item.CellX - auraRadius > maxCellX ||
                        item.CellY + item.CellHeight + auraRadius < minCellY || item.CellY - auraRadius > maxCellY)
                    {
                        continue;
                    }

                    Point itemStartWorld = new Point(item.CellX * cellSize, item.CellY * cellSize);
                    Point itemEndWorld = new Point((item.CellX + item.CellWidth) * cellSize, (item.CellY + item.CellHeight) * cellSize);
                    Point startScreen = worldToScreen(itemStartWorld);
                    Point endScreen = worldToScreen(itemEndWorld);

                    double itemW = endScreen.X - startScreen.X;
                    double itemH = endScreen.Y - startScreen.Y;
                    Point center = new Point(startScreen.X + itemW / 2.0, startScreen.Y + itemH / 2.0);

                    double auraRadiusPx = Math.Max(itemW, itemH) * 1.6;
                    Color fieldHue = Color.Parse(item.FieldHueHex);
                    Color auraCenterColor = Color.FromArgb((byte)(255 * Tokens.FieldAlphaMax * 0.4), fieldHue.R, fieldHue.G, fieldHue.B);
                    Color auraOuterColor = Color.FromArgb(0, fieldHue.R, fieldHue.G, fieldHue.B);

                    var radialGradient = new RadialGradientBrush
                    {
                        Center = new RelativePoint(0.5, 0.5, RelativeUnit.Relative),
                        GradientOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative),
                        RadiusX = new RelativeScalar(0.5, RelativeUnit.Relative),
                        RadiusY = new RelativeScalar(0.5, RelativeUnit.Relative),
                        GradientStops = new GradientStops
                        {
                            new GradientStop(auraCenterColor, 0.0),
                            new GradientStop(auraOuterColor, 1.0)
                        }
                    };

                    Rect auraRect = new Rect(center.X - auraRadiusPx, center.Y - auraRadiusPx, auraRadiusPx * 2.0, auraRadiusPx * 2.0);
                    context.FillRectangle(radialGradient, auraRect);
                }
            }

            // 3. Layer 2: Cell Presence Perimeter Rings (--field-perimeter-width = 1.5px) (LOD Shedding below 3px)
            if (projectedCellSize >= 3.0)
            {
                foreach (var item in itemList)
                {
                    if (item.CellX + item.CellWidth < minCellX || item.CellX > maxCellX ||
                        item.CellY + item.CellHeight < minCellY || item.CellY > maxCellY)
                    {
                        continue;
                    }

                    Color perimeterHue = Color.Parse(item.FieldHueHex);
                    double alpha = item.IsSelected ? Tokens.FieldPerimeterSelected : Tokens.FieldPerimeterInk;
                    Color lineClr = Color.FromArgb((byte)(255 * alpha), perimeterHue.R, perimeterHue.G, perimeterHue.B);
                    var perimeterPen = new Pen(new SolidColorBrush(lineClr), Tokens.FieldPerimeterWidth * Math.Max(0.5, zoom));

                    int startX = item.CellX;
                    int endX = item.CellX + item.CellWidth - 1;
                    int startY = item.CellY;
                    int endY = item.CellY + item.CellHeight - 1;

                    Point topLeftScreen = worldToScreen(new Point(startX * cellSize, startY * cellSize));
                    Point bottomRightScreen = worldToScreen(new Point((endX + 1) * cellSize, (endY + 1) * cellSize));

                    // Outer perimeter edges of occupied region
                    Point pTL = topLeftScreen;
                    Point pTR = new Point(bottomRightScreen.X, topLeftScreen.Y);
                    Point pBL = new Point(topLeftScreen.X, bottomRightScreen.Y);
                    Point pBR = bottomRightScreen;

                    context.DrawLine(perimeterPen, pTL, pTR); // Top edge
                    context.DrawLine(perimeterPen, pTR, pBR); // Right edge
                    context.DrawLine(perimeterPen, pBR, pBL); // Bottom edge
                    context.DrawLine(perimeterPen, pBL, pTL); // Left edge
                }
            }
        }
    }
}
