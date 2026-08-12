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
    /// Engine module for rendering prepared field-ledger cell fills and cell-presence perimeter rings.
    /// </summary>
    public class FieldLedgerModule
    {
        /// <summary>
        /// Renders prepared field-ledger cell fills and perimeter rings.
        /// Applies spatial aura radius culling (d &lt;= Tokens.MaxCullingRadiusCells)
        /// and LOD distance shedding when projected cell size &lt; Tokens.GridFadeStart.
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
            // Enforce LOD Distance Shedding: Shed cell field calculations & cell fill matrix quads when projected cell size < 6px (Tokens.GridFadeStart)
            bool enableCellFills = projectedCellSize >= Tokens.GridFadeStart;

            if (enableCellFills)
            {
                var activeAuraCells = FieldLedgerEngine.GetAuraCells(
                    itemList, minCellX, maxCellX, minCellY, maxCellY);

                if (activeAuraCells.Count > 0)
                {
                    // Aura rendering is discrete: each prepared active cell receives its own fill.
                    foreach (var (cx, cy) in activeAuraCells)
                    {
                        CellLedgerEntry entry = fieldEngine.GetCellLedger(cx, cy);
                        if (entry.FieldEnergy > CellLedgerEntry.BaselineEnergy &&
                            entry.CompositeColor != Colors.SurfaceGrid)
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

            // Cell presence perimeter rings (--field-perimeter-width = 1.5px), shed below 3px.
            if (projectedCellSize >= 3.0)
            {
                foreach (var (cx, cy) in fieldEngine.PerimeterSubscriber.PerimeterCells)
                {
                    if (cx < minCellX || cx > maxCellX || cy < minCellY || cy > maxCellY)
                    {
                        continue;
                    }

                    CellLedgerEntry entry = fieldEngine.GetCellLedger(cx, cy);
                    if (entry.FieldEnergy < PerimeterRingSubscriber.PerimeterThresholdEnergy)
                    {
                        continue;
                    }

                    double alpha = Tokens.FieldPerimeterInk;
                    Color lineClr = Color.FromArgb(
                        (byte)(255 * alpha),
                        entry.CompositeColor.R,
                        entry.CompositeColor.G,
                        entry.CompositeColor.B);
                    var perimeterPen = new Pen(new SolidColorBrush(lineClr), Tokens.FieldPerimeterWidth * Math.Max(0.5, zoom));

                    Point topLeftScreen = worldToScreen(new Point(cx * cellSize, cy * cellSize));
                    Point bottomRightScreen = worldToScreen(new Point((cx + 1) * cellSize, (cy + 1) * cellSize));

                    // Draw only the exposed edges of the saturated region. This
                    // keeps adjacent cells as one readable contour and never
                    // paints an interior cross-line between neighboring cells.
                    Point pTL = topLeftScreen;
                    Point pTR = new Point(bottomRightScreen.X, topLeftScreen.Y);
                    Point pBL = new Point(topLeftScreen.X, bottomRightScreen.Y);
                    Point pBR = bottomRightScreen;

                    var saturated = fieldEngine.PerimeterSubscriber.SaturatedCells;
                    if (!saturated.Contains((cx, cy - 1))) context.DrawLine(perimeterPen, pTL, pTR);
                    if (!saturated.Contains((cx + 1, cy))) context.DrawLine(perimeterPen, pTR, pBR);
                    if (!saturated.Contains((cx, cy + 1))) context.DrawLine(perimeterPen, pBR, pBL);
                    if (!saturated.Contains((cx - 1, cy))) context.DrawLine(perimeterPen, pBL, pTL);
                }
            }
        }
    }
}
