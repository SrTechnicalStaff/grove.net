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
    /// Engine module for grid cursor rendering with inset 2px ring in #F4F4F2, 22% fill,
    /// footprint expansion over all content types, and 18-step spent cell trail decay.
    /// </summary>
    public class CursorRenderModule
    {
        /// <summary>
        /// Updates the 18-step spent cell trail decay physics.
        /// </summary>
        public bool DecayTrail(List<SpentCell> spentCells)
        {
            bool needsRedraw = false;
            for (int i = spentCells.Count - 1; i >= 0; i--)
            {
                spentCells[i].Energy *= Tokens.CursorTrailDecay;
                if (spentCells[i].Energy <= Tokens.CursorTrailMin)
                {
                    spentCells.RemoveAt(i);
                }
                needsRedraw = true;
            }
            return needsRedraw;
        }

        /// <summary>
        /// Registers spent cell position when cursor transitions between grid cells.
        /// Maintains 18-step cap per design system specs.
        /// </summary>
        public void RegisterCellTransition(List<SpentCell> spentCells, int lastCellX, int lastCellY)
        {
            if (lastCellX != int.MinValue && lastCellY != int.MinValue)
            {
                spentCells.Add(new SpentCell(lastCellX, lastCellY));
                while (spentCells.Count > Tokens.CursorTrailMaxSteps)
                {
                    spentCells.RemoveAt(0);
                }
            }
        }

        /// <summary>
        /// Renders the 18-step spent cell trail decay on the grid canvas.
        /// </summary>
        public void RenderSpentTrail(
            DrawingContext context,
            Func<Point, Point> worldToScreen,
            double cellSize,
            double zoom,
            IEnumerable<SpentCell> spentCells)
        {
            foreach (var spent in spentCells)
            {
                Point startWorld = new Point(spent.CellX * cellSize, spent.CellY * cellSize);
                Point startScreen = worldToScreen(startWorld);
                double sizeScreen = cellSize * zoom;

                Rect cellRect = new Rect(startScreen.X, startScreen.Y, sizeScreen, sizeScreen);
                byte alpha = (byte)(255 * Tokens.InkQuiet * spent.Energy);
                var trailBrush = new SolidColorBrush(Color.FromArgb(alpha, Colors.NoteText.R, Colors.NoteText.G, Colors.NoteText.B));
                context.FillRectangle(trailBrush, cellRect);
            }
        }

        /// <summary>
        /// Renders the primary grid cursor with inset 2px ring in #F4F4F2, 22% fill,
        /// and footprint expansion over the target content footprint.
        /// </summary>
        public void RenderGridCursor(
            DrawingContext context,
            Func<Point, Point> worldToScreen,
            double cellSize,
            double zoom,
            int cursorCellX,
            int cursorCellY,
            GridContentItem? targetItem)
        {
            int startCellX = targetItem?.CellX ?? cursorCellX;
            int startCellY = targetItem?.CellY ?? cursorCellY;
            int spanWidth = targetItem?.CellWidth ?? 1;
            int spanHeight = targetItem?.CellHeight ?? 1;

            Point startWorld = new Point(startCellX * cellSize, startCellY * cellSize);
            Point endWorld = new Point(
                (startCellX + spanWidth) * cellSize,
                (startCellY + spanHeight) * cellSize);

            Point startScreen = worldToScreen(startWorld);
            Point endScreen = worldToScreen(endWorld);

            double curW = endScreen.X - startScreen.X;
            double curH = endScreen.Y - startScreen.Y;
            Rect cursorRect = new Rect(startScreen.X, startScreen.Y, curW, curH);

            // 22% fill (#F4F4F2 ink with Tokens.CursorFillGain 0.22 alpha)
            byte fillAlpha = (byte)(255 * Tokens.CursorFillGain);
            var headFillBrush = new SolidColorBrush(Color.FromArgb(fillAlpha, Colors.NoteText.R, Colors.NoteText.G, Colors.NoteText.B));
            context.FillRectangle(headFillBrush, cursorRect);

            // Inset 2px ring in #F4F4F2 (Tokens.CursorRingInk = 0.88 alpha)
            byte ringAlpha = (byte)(255 * Tokens.CursorRingInk);
            var ringPen = new Pen(new SolidColorBrush(Color.FromArgb(ringAlpha, Colors.NoteText.R, Colors.NoteText.G, Colors.NoteText.B)), Tokens.StrokeCursorRing * Math.Max(0.5, zoom));
            context.DrawRectangle(null, ringPen, cursorRect.Deflate(1.0));
        }
    }
}
