using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Media;
using GroveApp.DesignSystem;
using GroveApp.Engine;
using GroveApp.Models;
using GroveApp.Models.Interaction;
using Colors = GroveApp.DesignSystem.Colors;

namespace GroveApp.Controls;

public sealed class GridCanvasFeedbackRenderer
{
    private readonly CursorRenderModule _cursorRenderer;

    public Rect ResizeCancelBounds { get; private set; }

    public GridCanvasFeedbackRenderer(CursorRenderModule cursorRenderer)
    {
        _cursorRenderer = cursorRenderer ?? throw new ArgumentNullException(nameof(cursorRenderer));
    }

    public void RenderDropPreview(
        DrawingContext context,
        DropPlacementPreview preview,
        Func<Point, Point> worldToScreen)
    {
        Color signal = preview.IsValid ? Colors.SignalInteraction : Colors.SignalRefusal;
        _cursorRenderer.RenderDescriptor(
            context,
            worldToScreen,
            preview.Cursor,
            signal,
            32.0 / 255.0,
            signal,
            210.0 / 255.0);
    }

    public void RenderArmingGhost(
        DrawingContext context,
        GhostPlacementDescriptor ghost,
        CursorDescriptor cursor,
        bool rejectionPulse,
        Func<Point, Point> worldToScreen)
    {
        if (!ghost.IsVisible || cursor.Kind != CursorFootprintKind.ArmedTool)
        {
            return;
        }

        Color signal = ghost.IsValidRegion ? Colors.ToolAccent : Colors.ToolInvalid;
        double fillAlpha = rejectionPulse
            ? Math.Min(1.0, Tokens.GhostFillOpacity + 0.20)
            : Tokens.GhostFillOpacity;
        _cursorRenderer.RenderDescriptor(
            context,
            worldToScreen,
            cursor,
            signal,
            fillAlpha,
            signal,
            Tokens.GhostRingOpacity);
    }

    public void RenderLayerFeedback(
        DrawingContext context,
        LayerFeedbackAnimationState state,
        double cellSize,
        double zoom,
        Func<Point, Point> worldToScreen,
        Func<int, int, Point> cellToWorld)
    {
        if (!state.IsActive)
        {
            return;
        }

        int radius = Math.Max(0, (int)Math.Round(state.SweepProgress * 4.0));
        byte alpha = (byte)Math.Clamp((int)Math.Round(state.SweepOpacity * 255.0), 0, 255);
        var pen = new Pen(new SolidColorBrush(Color.FromArgb(
            alpha,
            state.SweepColor.R,
            state.SweepColor.G,
            state.SweepColor.B)), Tokens.StrokeState);
        int originX = (int)Math.Round(state.Origin.X);
        int originY = (int)Math.Round(state.Origin.Y);

        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                if (Math.Max(Math.Abs(dx), Math.Abs(dy)) != radius)
                {
                    continue;
                }

                Point screen = worldToScreen(cellToWorld(originX + dx, originY + dy));
                var rect = new Rect(screen.X, screen.Y, cellSize * zoom, cellSize * zoom);
                context.DrawRectangle(null, pen, rect.Deflate(Tokens.StrokeState / 2.0));
            }
        }
    }

    public void RenderGroupDragPreview(
        DrawingContext context,
        bool isDragging,
        IReadOnlyList<GridContentItem> dragCluster,
        IReadOnlyDictionary<GridContentItem, (int cellX, int cellY)> initialPositions,
        int deltaX,
        int deltaY,
        bool isValid,
        double cellSize,
        double zoom,
        Func<Point, Point> worldToScreen,
        Func<int, int, Point> cellToWorld)
    {
        if (!isDragging || dragCluster.Count <= 1 || (deltaX == 0 && deltaY == 0))
        {
            return;
        }

        Color signal = isValid ? Colors.SignalInteraction : Colors.SignalRefusal;
        var fill = new SolidColorBrush(Color.FromArgb(38, signal.R, signal.G, signal.B));
        var pen = new Pen(new SolidColorBrush(Color.FromArgb(220, signal.R, signal.G, signal.B)), Tokens.StrokeState);

        foreach (GridContentItem item in dragCluster)
        {
            var origin = initialPositions[item];
            Point screen = worldToScreen(cellToWorld(origin.cellX + deltaX, origin.cellY + deltaY));
            var rect = new Rect(
                screen.X,
                screen.Y,
                item.CellWidth * cellSize * zoom,
                item.CellHeight * cellSize * zoom);
            context.FillRectangle(fill, rect);
            context.DrawRectangle(null, pen, rect.Deflate(Tokens.StrokeState / 2));
        }
    }

    public void RenderResizePreview(
        DrawingContext context,
        bool isResizing,
        bool candidateIsValid,
        SpatialRegion candidateFootprint,
        double cellSize,
        double zoom,
        Size viewportSize,
        Func<Point, Point> worldToScreen)
    {
        if (!isResizing || !candidateFootprint.IsValid)
        {
            ResizeCancelBounds = new Rect();
            return;
        }

        Point startScreen = worldToScreen(new Point(
            candidateFootprint.X * cellSize,
            candidateFootprint.Y * cellSize));
        Point endScreen = worldToScreen(new Point(
            candidateFootprint.Right * cellSize,
            candidateFootprint.Bottom * cellSize));
        Rect candidateBounds = new(
            startScreen.X,
            startScreen.Y,
            endScreen.X - startScreen.X,
            endScreen.Y - startScreen.Y);

        if (candidateIsValid)
        {
            Color interaction = Colors.SignalInteraction;
            context.FillRectangle(
                new SolidColorBrush(Color.FromArgb(15, interaction.R, interaction.G, interaction.B)),
                candidateBounds);
            context.DrawRectangle(
                null,
                new Pen(Colors.SignalInteractionBrush, Tokens.FieldPerimeterWidth),
                candidateBounds.Deflate(Tokens.FieldPerimeterWidth / 2.0));
            ResizeCancelBounds = new Rect();
            return;
        }

        Color refusal = Colors.SignalRefusal;
        context.FillRectangle(
            new SolidColorBrush(Color.FromArgb(31, refusal.R, refusal.G, refusal.B)),
            candidateBounds);
        using (context.PushClip(candidateBounds))
        {
            var hatchPen = new Pen(
                new SolidColorBrush(Color.FromArgb(102, refusal.R, refusal.G, refusal.B)),
                Math.Max(1.0, Tokens.FieldPerimeterWidth));
            double spacing = Math.Max(8.0, 12.0 * Math.Max(0.5, zoom));
            for (double start = candidateBounds.Left - candidateBounds.Height;
                 start < candidateBounds.Right;
                 start += spacing)
            {
                context.DrawLine(
                    hatchPen,
                    new Point(start, candidateBounds.Bottom),
                    new Point(start + candidateBounds.Height, candidateBounds.Top));
            }
        }

        context.DrawRectangle(
            null,
            new Pen(new SolidColorBrush(Color.FromArgb(115, refusal.R, refusal.G, refusal.B)), Tokens.FieldPerimeterWidth),
            candidateBounds.Deflate(Tokens.FieldPerimeterWidth / 2.0));

        double stripWidth = 246.0;
        double stripHeight = 24.0;
        double stripX = candidateBounds.Right + Tokens.SpaceXs;
        double stripY = candidateBounds.Top;
        if (stripX + stripWidth > viewportSize.Width)
        {
            stripX = Math.Max(0.0, candidateBounds.Left - stripWidth - Tokens.SpaceXs);
        }

        var strip = new Rect(stripX, stripY, stripWidth, stripHeight);
        double cancelWidth = 58.0;
        double placeWidth = 52.0;
        ResizeCancelBounds = new Rect(
            strip.Right - cancelWidth - placeWidth - Tokens.SpaceXs,
            strip.Top,
            cancelWidth,
            strip.Height);
        context.FillRectangle(
            new SolidColorBrush(Color.FromArgb(220, refusal.R, refusal.G, refusal.B)),
            strip);
        var text = new FormattedText(
            "This space is occupied",
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface(Typography.FontFamilyMono, FontStyle.Normal, FontWeight.Normal),
            Typography.SizeMicro,
            Colors.CPaperInkBrush)
        {
            MaxTextWidth = strip.Width - Tokens.SpaceSm
        };
        context.DrawText(text, new Point(strip.Left + Tokens.SpaceXs, strip.Top + Tokens.SpaceXs));

        Rect cancelRect = ResizeCancelBounds;
        Rect placeRect = new Rect(cancelRect.Right, strip.Top, placeWidth, strip.Height);
        context.DrawRectangle(
            new SolidColorBrush(Color.FromArgb(70, 255, 255, 255)),
            new Pen(new SolidColorBrush(Color.FromArgb(140, 255, 255, 255)), 1),
            cancelRect.Deflate(2));
        context.DrawRectangle(
            new SolidColorBrush(Color.FromArgb(25, 234, 234, 234)),
            new Pen(new SolidColorBrush(Color.FromArgb(60, 234, 234, 234)), 1),
            placeRect.Deflate(2));

        var cancelText = new FormattedText(
            "CANCEL",
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface(Typography.FontFamilyMono, FontStyle.Normal, FontWeight.Bold),
            Typography.SizeMicro,
            Colors.CPaperInkBrush);
        context.DrawText(cancelText, new Point(cancelRect.Left + Tokens.SpaceXs, cancelRect.Top + Tokens.SpaceXs));

        var placeText = new FormattedText(
            "PLACE",
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface(Typography.FontFamilyMono, FontStyle.Normal, FontWeight.Normal),
            Typography.SizeMicro,
            Colors.TextUnavailableBrush);
        context.DrawText(placeText, new Point(placeRect.Left + Tokens.SpaceXs, placeRect.Top + Tokens.SpaceXs));
    }

    public void RenderMarqueeSelection(
        DrawingContext context,
        bool isSelecting,
        Point startWorld,
        Point currentWorld,
        Func<Point, (int cellX, int cellY)> worldToCell,
        Func<Point, Point> worldToScreen,
        double cellSize)
    {
        if (!isSelecting)
        {
            return;
        }

        var startCell = worldToCell(startWorld);
        var currentCell = worldToCell(currentWorld);
        int minCellX = Math.Min(startCell.cellX, currentCell.cellX);
        int maxCellX = Math.Max(startCell.cellX, currentCell.cellX);
        int minCellY = Math.Min(startCell.cellY, currentCell.cellY);
        int maxCellY = Math.Max(startCell.cellY, currentCell.cellY);
        Point startScreen = worldToScreen(new Point(minCellX * cellSize, minCellY * cellSize));
        Point endScreen = worldToScreen(new Point((maxCellX + 1) * cellSize, (maxCellY + 1) * cellSize));
        var marqueeRect = new Rect(
            startScreen.X,
            startScreen.Y,
            endScreen.X - startScreen.X,
            endScreen.Y - startScreen.Y);
        Color marqueeColor = Colors.SignalActiveWork;
        var fillBrush = new SolidColorBrush(Color.FromArgb(25, marqueeColor.R, marqueeColor.G, marqueeColor.B));
        var borderPen = new Pen(Colors.SignalActiveWorkBrush, Tokens.FieldPerimeterWidth, new DashStyle(new[] { 4.0, 4.0 }, 0));
        context.FillRectangle(fillBrush, marqueeRect);
        context.DrawRectangle(null, borderPen, marqueeRect);
    }
}
