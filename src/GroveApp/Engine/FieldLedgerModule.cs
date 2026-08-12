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
        /// Calculates the gravitational presence accumulation color for cell (cx, cy) from all notes.
        /// Restricts calculations strictly to active aura envelopes (d <= 6 cells).
        /// </summary>
        public Color CalculateCellFieldColor(int cx, int cy, IEnumerable<GridNote> notes)
        {
            Color baseColor = Colors.SurfaceGrid;
            double r = baseColor.R;
            double g = baseColor.G;
            double b = baseColor.B;

            foreach (var note in notes)
            {
                double dx = 0.0;
                if (cx < note.CellX)
                    dx = note.CellX - cx;
                else if (cx >= note.CellX + note.SizeCells)
                    dx = cx - (note.CellX + note.SizeCells - 1);

                double dy = 0.0;
                if (cy < note.CellY)
                    dy = note.CellY - cy;
                else if (cy >= note.CellY + note.SizeCells)
                    dy = cy - (note.CellY + note.SizeCells - 1);

                // Spatial aura bounding-box culling: d <= 6 cells
                if (dx > 6 || dy > 6) continue;

                double distSq = dx * dx + dy * dy;

                // Field gravity equation: FieldWeight = FieldGain / (1 + 0.4 * distSq)
                double fieldWeight = Tokens.FieldGain / (1.0 + 0.4 * distSq);
                Color noteFieldColor = Color.Parse(note.FieldHueHex);

                r += noteFieldColor.R * fieldWeight;
                g += noteFieldColor.G * fieldWeight;
                b += noteFieldColor.B * fieldWeight;
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
            IEnumerable<GridNote> notes)
        {
            var noteList = notes as List<GridNote> ?? new List<GridNote>(notes);
            if (noteList.Count == 0) return;

            double projectedCellSize = cellSize * zoom;
            const int auraRadius = 6;

            // Enforce LOD Distance Shedding: Shed cell field calculations & cell fill matrix quads when projected cell size < 6px (Tokens.GridFadeStart)
            bool enableCellFills = projectedCellSize >= Tokens.GridFadeStart;

            if (enableCellFills)
            {
                // Spatial Aura Bounding-Box Culling (d <= 6 cells):
                // Collect unique active aura envelope cells across all notes in the viewport
                var activeAuraCells = new HashSet<(int col, int row)>();

                foreach (var note in noteList)
                {
                    int nMinX = Math.Max(minCellX, note.CellX - auraRadius);
                    int nMaxX = Math.Min(maxCellX, note.CellX + note.SizeCells - 1 + auraRadius);
                    int nMinY = Math.Max(minCellY, note.CellY - auraRadius);
                    int nMaxY = Math.Min(maxCellY, note.CellY + note.SizeCells - 1 + auraRadius);

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
                    // 0. Recalculate Field Ledger ONLY for active aura envelope cells (O(Active Aura Envelopes) instead of O(Viewport Area))
                    fieldEngine.RecalculateField(noteList, activeAuraCells);

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

            // 2. Layer 1: Gravitational Atmosphere Radial Glows around Note Footprints (LOD Shedding when projected cell size < 6px)
            if (projectedCellSize >= Tokens.GridFadeStart)
            {
                foreach (var note in noteList)
                {
                    if (note.CellX + note.SizeCells + auraRadius < minCellX || note.CellX - auraRadius > maxCellX ||
                        note.CellY + note.SizeCells + auraRadius < minCellY || note.CellY - auraRadius > maxCellY)
                    {
                        continue;
                    }

                    Point noteStartWorld = new Point(note.CellX * cellSize, note.CellY * cellSize);
                    Point noteEndWorld = new Point((note.CellX + note.SizeCells) * cellSize, (note.CellY + note.SizeCells) * cellSize);
                    Point startScreen = worldToScreen(noteStartWorld);
                    Point endScreen = worldToScreen(noteEndWorld);

                    double noteW = endScreen.X - startScreen.X;
                    double noteH = endScreen.Y - startScreen.Y;
                    Point center = new Point(startScreen.X + noteW / 2.0, startScreen.Y + noteH / 2.0);

                    double auraRadiusPx = Math.Max(noteW, noteH) * 1.6;
                    Color fieldHue = Color.Parse(note.FieldHueHex);
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
                foreach (var note in noteList)
                {
                    if (note.CellX + note.SizeCells < minCellX || note.CellX > maxCellX ||
                        note.CellY + note.SizeCells < minCellY || note.CellY > maxCellY)
                    {
                        continue;
                    }

                    Color perimeterHue = Color.Parse(note.FieldHueHex);
                    double alpha = note.IsSelected ? Tokens.FieldPerimeterSelected : Tokens.FieldPerimeterInk;
                    Color lineClr = Color.FromArgb((byte)(255 * alpha), perimeterHue.R, perimeterHue.G, perimeterHue.B);
                    var perimeterPen = new Pen(new SolidColorBrush(lineClr), Tokens.FieldPerimeterWidth * Math.Max(0.5, zoom));

                    int startX = note.CellX;
                    int endX = note.CellX + note.SizeCells - 1;
                    int startY = note.CellY;
                    int endY = note.CellY + note.SizeCells - 1;

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
