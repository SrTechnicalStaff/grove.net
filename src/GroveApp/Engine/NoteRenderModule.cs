using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using GroveApp.DesignSystem;
using GroveApp.Models;
using Colors = GroveApp.DesignSystem.Colors;

namespace GroveApp.Engine
{
    /// <summary>
    /// Engine module for rendering Notes, Documents, and Images on the spatial Grid.
    /// Strictly enforces ADR-010, ADR-011, ADR-012 physical geometry and styling rules.
    /// </summary>
    public class NoteRenderModule
    {
        public void RenderContentItems(
            DrawingContext context,
            Func<Point, Point> worldToScreen,
            double cellSize,
            double zoom,
            int minCellX,
            int maxCellX,
            int minCellY,
            int maxCellY,
            IEnumerable<GridContentItem> items,
            GridContentItem? selectedItem,
            GridContentItem? hoveredItem)
        {
            double projectedCellSize = cellSize * zoom;

            foreach (var item in items)
            {
                if (item.CellX + item.CellWidth < minCellX || item.CellX > maxCellX ||
                    item.CellY + item.CellHeight < minCellY || item.CellY > maxCellY)
                {
                    continue;
                }

                bool isSelected = item.IsSelected || item == selectedItem;
                bool isHovered = item.IsHovered || item == hoveredItem;

                if (item is GridNote note)
                {
                    RenderNote(context, worldToScreen, cellSize, zoom, projectedCellSize, note, isSelected, isHovered);
                }
                else if (item is GridDocument doc)
                {
                    RenderDocument(context, worldToScreen, cellSize, zoom, doc, isSelected, isHovered);
                }
                else if (item is GridImage img)
                {
                    RenderImage(context, worldToScreen, cellSize, zoom, img, isSelected, isHovered);
                }
            }
        }

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
            RenderContentItems(context, worldToScreen, cellSize, zoom, minCellX, maxCellX, minCellY, maxCellY, notes, selectedNote, hoveredNote);
        }

        private void RenderNote(
            DrawingContext context,
            Func<Point, Point> worldToScreen,
            double cellSize,
            double zoom,
            double projectedCellSize,
            GridNote note,
            bool isSelected,
            bool isHovered)
        {
            Point startScreen = worldToScreen(new Point(note.CellX * cellSize, note.CellY * cellSize));
            Point endScreen = worldToScreen(new Point((note.CellX + note.SizeCells) * cellSize, (note.CellY + note.SizeCells) * cellSize));

            double rectW = endScreen.X - startScreen.X;
            double rectH = endScreen.Y - startScreen.Y;
            Rect noteRect = new Rect(startScreen.X, startScreen.Y, rectW, rectH);

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

        public void RenderDocument(
            DrawingContext context,
            Func<Point, Point> worldToScreen,
            double cellSize,
            double zoom,
            GridDocument doc,
            bool isSelected,
            bool isHovered)
        {
            Point startScreen = worldToScreen(new Point(doc.CellX * cellSize, doc.CellY * cellSize));
            Point endScreen = worldToScreen(new Point((doc.CellX + doc.CellWidth) * cellSize, (doc.CellY + doc.CellHeight) * cellSize));

            double rectW = endScreen.X - startScreen.X;
            double rectH = endScreen.Y - startScreen.Y;
            Rect docRect = new Rect(startScreen.X, startScreen.Y, rectW, rectH);

            // 1. Paper Fill (--surface-page #F5F5F5)
            context.FillRectangle(Colors.SurfacePageBrush, docRect);

            // 2. Page Texture (Rules at 44px minor / 220px major)
            RenderPageTexture(context, docRect, zoom);

            // 3. 1px Inset Edge (--paper-edge)
            var edgePen = new Pen(new SolidColorBrush(Color.FromArgb(25, 26, 26, 26)), 1.0);
            context.DrawRectangle(null, edgePen, docRect.Deflate(0.5));

            // 4. Multi-Column Reflow Rendering
            if (doc.AstDocument != null)
            {
                var layout = DocumentReflowEngine.Reflow(doc.AstDocument, doc.CellWidth, doc.CellHeight, doc.CurrentPage);
                using (context.PushTransform(Matrix.CreateScale(zoom, zoom) * Matrix.CreateTranslation(docRect.X, docRect.Y)))
                {
                    var textBrush = Colors.PaperInkBrush;
                    var typeface = new Typeface("Inter", FontStyle.Normal, FontWeight.Regular);

                    foreach (var slice in layout.Slices)
                    {
                        double yCursor = slice.Y;
                        for (int b = slice.StartBlockIndex; b <= slice.EndBlockIndex && b < doc.AstDocument.Blocks.Count; b++)
                        {
                            var block = doc.AstDocument.Blocks[b];
                            if (block is HeadingBlock heading)
                            {
                                var fmt = new FormattedText(
                                    heading.Text,
                                    CultureInfo.CurrentCulture,
                                    FlowDirection.LeftToRight,
                                    new Typeface("Inter", FontStyle.Normal, FontWeight.Bold),
                                    18.0,
                                    textBrush)
                                {
                                    MaxTextWidth = slice.Width
                                };
                                context.DrawText(fmt, new Point(slice.X, yCursor));
                                yCursor += fmt.Height + 8.0;
                            }
                            else if (block is ParagraphBlock para)
                            {
                                string text = string.Join("", para.Inlines.Select(i => i switch
                                {
                                    TextRunInline t => t.Text,
                                    FormattedInline f => f.Text,
                                    _ => ""
                                }));

                                var fmt = new FormattedText(
                                    text,
                                    CultureInfo.CurrentCulture,
                                    FlowDirection.LeftToRight,
                                    typeface,
                                    15.0,
                                    textBrush)
                                {
                                    MaxTextWidth = slice.Width,
                                    LineHeight = DocumentReflowEngine.LineHeightPx
                                };

                                context.DrawText(fmt, new Point(slice.X, yCursor));
                                yCursor += fmt.Height + 12.0;
                            }
                        }
                    }
                }
            }

            // 5. Selection Ring
            if (isSelected)
            {
                Rect selRect = docRect.Inflate(3.0 * zoom);
                var selectionPen = new Pen(Colors.SignalInteractionBrush, Tokens.StrokeState * Math.Max(0.8, zoom));
                context.DrawRectangle(null, selectionPen, selRect);
            }
        }

        private void RenderPageTexture(DrawingContext context, Rect rect, double zoom)
        {
            var minorPen = new Pen(new SolidColorBrush(Color.FromArgb(13, 26, 26, 26)), 1.0);
            var majorPen = new Pen(new SolidColorBrush(Color.FromArgb(20, 26, 26, 26)), 1.0);

            for (double x = 44.0 * zoom; x < rect.Width; x += 44.0 * zoom)
            {
                var pen = (Math.Abs((x / zoom) % 220.0) < 0.01) ? majorPen : minorPen;
                context.DrawLine(pen, new Point(rect.X + x, rect.Y), new Point(rect.X + x, rect.Y + rect.Height));
            }

            for (double y = 44.0 * zoom; y < rect.Height; y += 44.0 * zoom)
            {
                var pen = (Math.Abs((y / zoom) % 220.0) < 0.01) ? majorPen : minorPen;
                context.DrawLine(pen, new Point(rect.X, rect.Y + y), new Point(rect.X + rect.Width, rect.Y + y));
            }
        }

        public void RenderImage(
            DrawingContext context,
            Func<Point, Point> worldToScreen,
            double cellSize,
            double zoom,
            GridImage img,
            bool isSelected,
            bool isHovered)
        {
            Point startScreen = worldToScreen(new Point(img.CellX * cellSize, img.CellY * cellSize));
            Point endScreen = worldToScreen(new Point((img.CellX + img.CellWidth) * cellSize, (img.CellY + img.CellHeight) * cellSize));

            double rectW = endScreen.X - startScreen.X;
            double rectH = endScreen.Y - startScreen.Y;
            Rect imgRect = new Rect(startScreen.X, startScreen.Y, rectW, rectH);

            if (img.LoadedBitmap != null)
            {
                context.DrawImage(img.LoadedBitmap, new Rect(0, 0, img.LoadedBitmap.Size.Width, img.LoadedBitmap.Size.Height), imgRect);
            }
            else
            {
                context.FillRectangle(Colors.SurfaceNestedBrush, imgRect);
            }

            // 1px Quiet Edge (--edge-quiet)
            var edgePen = new Pen(Colors.EdgeQuietBrush, 1.0);
            context.DrawRectangle(null, edgePen, imgRect.Deflate(0.5));

            // GIF Badge
            if (img.IsAnimatedGif)
            {
                double badgeX = imgRect.Right - 32.0 * zoom;
                double badgeY = imgRect.Top + 8.0 * zoom;
                Rect bgRect = new Rect(badgeX, badgeY, 24 * zoom, 14 * zoom);
                context.FillRectangle(new SolidColorBrush(Color.FromArgb(200, 14, 14, 16)), bgRect);

                var fmt = new FormattedText(
                    "GIF",
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("JetBrains Mono", FontStyle.Normal, FontWeight.Bold),
                    Typography.SizeMicro * zoom,
                    Brushes.White);
                context.DrawText(fmt, new Point(badgeX + 2 * zoom, badgeY + 1 * zoom));
            }

            // Selection Ring
            if (isSelected)
            {
                Rect selRect = imgRect.Inflate(3.0 * zoom);
                var selectionPen = new Pen(Colors.SignalInteractionBrush, Tokens.StrokeState * Math.Max(0.8, zoom));
                context.DrawRectangle(null, selectionPen, selRect);
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
            var fillBrush = new SolidColorBrush(Color.Parse(note.FillHex));
            context.FillRectangle(fillBrush, noteRect);

            var insetEdgePen = new Pen(Colors.ContainmentEdgeBrush, Tokens.StrokeContainment);
            context.DrawRectangle(null, insetEdgePen, noteRect.Deflate(0.5));

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
                ctx.LineTo(new Point(rx + ribbonW / 2.0, ry + ribbonH * 0.72));
                ctx.LineTo(new Point(rx, ry + ribbonH));
                ctx.EndFigure(true);
            }

            context.DrawGeometry(Colors.SignalAuthoredContextBrush, null, geom);
        }

        private void RenderResizeCorner(DrawingContext context, Rect noteRect, double zoom)
        {
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
