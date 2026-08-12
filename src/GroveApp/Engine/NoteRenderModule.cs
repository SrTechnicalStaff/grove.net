using System;
using System.Collections.Generic;
using System.Globalization;

using Avalonia;
using Avalonia.Media;
using GroveApp.DesignSystem;
using GroveApp.Models;
using Colors = GroveApp.DesignSystem.Colors;

namespace GroveApp.Engine
{
    /// <summary>
    /// Engine module for rendering notes matching docs/design_catalogue/src/grid-plane/02-note.html
    /// and Note.md specifications exactly.
    /// </summary>
    public class NoteRenderModule
    {
        public void RenderNotes(
            DrawingContext context,
            Func<Point, Point> worldToScreen,
            double cellSize,
            double zoom,
            int minCellX,
            int maxCellX,
            int minCellY,
            int maxCellY,
            IEnumerable<GridNote> notes,
            GridNote? selectedNote,
            GridNote? hoveredNote)
        {
            double projectedCellSize = cellSize * zoom;

            foreach (var note in notes)
            {
                if (note.CellX + note.SizeCells < minCellX || note.CellX > maxCellX ||
                    note.CellY + note.SizeCells < minCellY || note.CellY > maxCellY)
                {
                    continue;
                }

                Point startScreen = worldToScreen(new Point(note.CellX * cellSize, note.CellY * cellSize));
                Point endScreen = worldToScreen(new Point((note.CellX + note.SizeCells) * cellSize, (note.CellY + note.SizeCells) * cellSize));

                double rectW = endScreen.X - startScreen.X;
                double rectH = endScreen.Y - startScreen.Y;
                Rect noteRect = new Rect(startScreen.X, startScreen.Y, rectW, rectH);

                bool isSelected = note.IsSelected || note == selectedNote;
                bool isHovered = note.IsHovered || note == hoveredNote;

                // -------------------------------------------------------------
                // Distance Tier Representation Logic (Note.md)
                // -------------------------------------------------------------
                if (projectedCellSize < Tokens.TierStandinPromote) // < 28px: Stand-in Tier
                {
                    RenderStandInTier(context, note, noteRect, zoom, isSelected);
                }
                else if (projectedCellSize < Tokens.TierDetailPromote) // 28px <= size < 72px: Stepped Tier
                {
                    RenderSteppedTier(context, note, noteRect, zoom, isSelected);
                }
                else // >= 72px: Working Tier
                {
                    RenderWorkingTier(context, note, noteRect, zoom, isSelected, isHovered);
                }
            }
        }

        private void RenderWorkingTier(
            DrawingContext context,
            GridNote note,
            Rect noteRect,
            double zoom,
            bool isSelected,
            bool isHovered)
        {
            // 1. Authored Fill (Opaque flat fill: Violet #6E62A6, Clay #B0524E, Slate Blue #4E6E9C)
            var fillBrush = new SolidColorBrush(Color.Parse(note.FillHex));
            context.FillRectangle(fillBrush, noteRect);

            // 2. 1px Inset Containment Edge in #6E6E6A / --edge-on-color
            var insetEdgePen = new Pen(Colors.ContainmentEdgeBrush, Tokens.StrokeContainment);
            context.DrawRectangle(null, insetEdgePen, noteRect.Deflate(0.5));

            // 3. Selection Outline (2px --signal-interaction #96B6F8 offset by 3px per Shape.md)
            if (isSelected)
            {
                Rect selRect = noteRect.Inflate(3.0 * zoom);
                var selectionPen = new Pen(Colors.SignalInteractionBrush, Tokens.StrokeState * Math.Max(0.8, zoom));
                context.DrawRectangle(null, selectionPen, selRect);
            }

            // 4. Anchor Ribbon (Tab notched at foot, filled #9E8CEA)
            if (note.IsAnchored)
            {
                RenderAnchorRibbon(context, noteRect, zoom);
            }

            // 5. Rich Text Block (#F4F4F2 ink, formatted via RichTextEngine: bold, italic, code, headings, bullets, colors)
            if (!string.IsNullOrEmpty(note.Text))
            {
                double padTop = Tokens.SpaceMd;
                double padLeft = 15.0;

                double noteWorldW = noteRect.Width / zoom;
                double noteWorldH = noteRect.Height / zoom;
                double maxWidth = Math.Max(10.0, noteWorldW - (padLeft * 2.0));
                double maxHeight = Math.Max(10.0, noteWorldH - (padTop * 2.0));

                using (context.PushTransform(Matrix.CreateScale(zoom, zoom) * Matrix.CreateTranslation(noteRect.X, noteRect.Y)))
                {
                    Point textPos = new Point(padLeft, padTop);
                    var layout = RichTextEngine.CreateLayout(
                        note.Text,
                        Typography.SizeBody,
                        Colors.NoteTextBrush,
                        maxWidth,
                        maxHeight
                    );

                    RichTextEngine.Render(context, textPos, layout);
                }
            }

            // 6. Focus / Selection Resize Affordance Corner
            if (isHovered || isSelected)
            {
                RenderResizeCorner(context, noteRect, zoom);
            }
        }

        private void RenderSteppedTier(
            DrawingContext context,
            GridNote note,
            Rect noteRect,
            double zoom,
            bool isSelected)
        {
            // Stepped Tier: Drops inset containment edge and edit affordances, keeps authored fill and full text set at --t-body
            var fillBrush = new SolidColorBrush(Color.Parse(note.FillHex));
            context.FillRectangle(fillBrush, noteRect);

            if (isSelected)
            {
                Rect selRect = noteRect.Inflate(3.0 * zoom);
                var selectionPen = new Pen(Colors.SignalInteractionBrush, Tokens.StrokeState * Math.Max(0.8, zoom));
                context.DrawRectangle(null, selectionPen, selRect);
            }

            if (note.IsAnchored)
            {
                RenderAnchorRibbon(context, noteRect, zoom);
            }

            if (!string.IsNullOrEmpty(note.Text))
            {
                double padTop = Tokens.SpaceMd;
                double padLeft = 15.0;

                double noteWorldW = noteRect.Width / zoom;
                double noteWorldH = noteRect.Height / zoom;
                double maxWidth = Math.Max(5.0, noteWorldW - (padLeft * 2.0));
                double maxHeight = Math.Max(5.0, noteWorldH - (padTop * 2.0));

                using (context.PushTransform(Matrix.CreateScale(zoom, zoom) * Matrix.CreateTranslation(noteRect.X, noteRect.Y)))
                {
                    Point textPos = new Point(padLeft, padTop);
                    var layout = RichTextEngine.CreateLayout(
                        note.Text,
                        Typography.SizeBody,
                        Colors.NoteTextBrush,
                        maxWidth,
                        maxHeight
                    );

                    RichTextEngine.Render(context, textPos, layout);
                }
            }
        }

        private void RenderStandInTier(
            DrawingContext context,
            GridNote note,
            Rect noteRect,
            double zoom,
            bool isSelected)
        {
            // Stand-in Tier: Authored fill block, 1px inset edge, and kind-coded written lines mark
            var fillBrush = new SolidColorBrush(Color.Parse(note.FillHex));
            context.FillRectangle(fillBrush, noteRect);

            var insetEdgePen = new Pen(Colors.EdgeOnColorBrush, Tokens.StrokeContainment);
            context.DrawRectangle(null, insetEdgePen, noteRect.Deflate(0.5));

            if (isSelected)
            {
                Rect selRect = noteRect.Inflate(3.0 * zoom);
                var selectionPen = new Pen(Colors.SignalInteractionBrush, Tokens.StrokeState * Math.Max(0.8, zoom));
                context.DrawRectangle(null, selectionPen, selRect);
            }

            // Kind-coded written lines mark: 3 light horizontal strokes in --ink-primary
            var markPen = new Pen(Colors.TextPrimaryBrush, Math.Max(1.0, 2.0 * zoom));
            double stroke1Y = noteRect.Y + noteRect.Height * 0.26;
            double stroke2Y = noteRect.Y + noteRect.Height * 0.46;
            double stroke3Y = noteRect.Y + noteRect.Height * 0.66;
            double strokeLeft = noteRect.X + noteRect.Width * 0.14;

            context.DrawLine(markPen, new Point(strokeLeft, stroke1Y), new Point(strokeLeft + noteRect.Width * 0.60, stroke1Y));
            context.DrawLine(markPen, new Point(strokeLeft, stroke2Y), new Point(strokeLeft + noteRect.Width * 0.46, stroke2Y));
            context.DrawLine(markPen, new Point(strokeLeft, stroke3Y), new Point(strokeLeft + noteRect.Width * 0.60, stroke3Y));
        }

        private void RenderAnchorRibbon(DrawingContext context, Rect noteRect, double zoom)
        {
            // Anchor ribbon tab geometry per Marks.md:
            // 12px x 22px tab, left edge inset 16px (--sp-md) from Note left edge,
            // hanging 5px above top edge and 17px down over head.
            double ribbonW = 12.0 * Math.Max(0.7, zoom);
            double ribbonH = 22.0 * Math.Max(0.7, zoom);
            double leftInset = 16.0 * Math.Max(0.7, zoom);
            double topHang = 5.0 * Math.Max(0.7, zoom);

            double rx = noteRect.X + leftInset;
            double ry = noteRect.Y - topHang;

            StreamGeometry geom = new StreamGeometry();
            using (StreamGeometryContext ctx = geom.Open())
            {
                ctx.BeginFigure(new Point(rx, ry), true);
                ctx.LineTo(new Point(rx + ribbonW, ry));
                ctx.LineTo(new Point(rx + ribbonW, ry + ribbonH));
                ctx.LineTo(new Point(rx + ribbonW / 2.0, ry + ribbonH * 0.72)); // Shallow V notch
                ctx.LineTo(new Point(rx, ry + ribbonH));
                ctx.EndFigure(true);
            }

            context.DrawGeometry(Colors.SignalAuthoredContextBrush, null, geom);
        }



        private void RenderResizeCorner(DrawingContext context, Rect noteRect, double zoom)
        {
            // Resize Corner per Marks.md:
            // 18x18px target at bottom-right corner, two 10px arms of 2px weight meeting at corner.
            double scale = Math.Max(0.7, zoom);
            double armLen = 10.0 * scale;
            double strokeW = Tokens.StrokeState * scale;

            var cornerPen = new Pen(Colors.TextPrimaryBrush, strokeW);

            Point br = new Point(noteRect.X + noteRect.Width - 3.0 * scale, noteRect.Y + noteRect.Height - 3.0 * scale);
            Point topArm = new Point(br.X, br.Y - armLen);
            Point leftArm = new Point(br.X - armLen, br.Y);

            context.DrawLine(cornerPen, topArm, br);
            context.DrawLine(cornerPen, leftArm, br);
        }
    }
}
