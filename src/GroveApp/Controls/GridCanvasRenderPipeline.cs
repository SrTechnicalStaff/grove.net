using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using GroveApp.DesignSystem;
using GroveApp.Engine;
using GroveApp.Models;
using Colors = GroveApp.DesignSystem.Colors;

namespace GroveApp.Controls;

public readonly record struct GridCanvasRenderFrame(
    Size ViewportSize,
    Matrix CameraTransform,
    double CellSize,
    double Zoom,
    int MinCellX,
    int MaxCellX,
    int MinCellY,
    int MaxCellY,
    double RenderScaling,
    bool GridLinesVisible,
    FieldLedgerEngine FieldEngine,
    IReadOnlyList<GridContentItem> VisibleItems,
    IReadOnlyList<GridContentItem> ActiveItems,
    IReadOnlyList<GridContentItem> InactiveItems,
    GridContentItem? SelectedItem,
    GridContentItem? HoveredItem,
    Func<Point, Point> WorldToScreen);

public sealed class GridCanvasRenderPipeline
{
    private readonly FieldLedgerRenderModule _fieldLedgerRenderer = new();
    private readonly NoteRenderModule _contentRenderer = new();
    public GridCanvasRenderPipeline() { }

    public void Render(DrawingContext context, GridCanvasRenderFrame frame)
    {
        context.FillRectangle(Colors.SurfaceGridBrush, new Rect(frame.ViewportSize));

        _fieldLedgerRenderer.RenderAuraFills(
            context,
            frame.CameraTransform,
            frame.CellSize,
            frame.Zoom,
            frame.MinCellX,
            frame.MaxCellX,
            frame.MinCellY,
            frame.MaxCellY,
            frame.FieldEngine,
            frame.VisibleItems);

        IReadOnlyList<PerimeterContourEdge> contourEdges = _fieldLedgerRenderer.GetPerimeterEdges(
            frame.FieldEngine,
            frame.MinCellX,
            frame.MaxCellX,
            frame.MinCellY,
            frame.MaxCellY);
        if (contourEdges.Count > 0)
        {
            Color contourHue = FieldLedgerRenderModule.ResolveContourHue(frame.FieldEngine);
            double contourAlpha = frame.SelectedItem is not null
                ? Tokens.FieldPerimeterSelected
                : Tokens.FieldPerimeterInk;
            context.Custom(new PerimeterContourDrawOperation(
                new Rect(frame.ViewportSize),
                contourEdges,
                frame.CameraTransform,
                frame.CellSize,
                contourHue,
                contourAlpha));
        }

        context.Custom(new GridLineDrawOperation(
            new Rect(frame.ViewportSize),
            frame.MinCellX,
            frame.MaxCellX,
            frame.MinCellY,
            frame.MaxCellY,
            frame.CellSize,
            frame.CameraTransform,
            frame.RenderScaling,
            frame.GridLinesVisible,
            frame.FieldEngine.CurrentVisibleAuraCells));

        _contentRenderer.RenderContentItems(
            context,
            frame.WorldToScreen,
            frame.CellSize,
            frame.Zoom,
            frame.MinCellX,
            frame.MaxCellX,
            frame.MinCellY,
            frame.MaxCellY,
            frame.ActiveItems,
            frame.SelectedItem,
            frame.HoveredItem);

        foreach (GridContentItem item in frame.InactiveItems)
        {
            _contentRenderer.RenderGhostOutline(
                context,
                frame.WorldToScreen,
                frame.CellSize,
                frame.Zoom,
                frame.MinCellX,
                frame.MaxCellX,
                frame.MinCellY,
                frame.MaxCellY,
                item,
                Tokens.InactiveGhostOpacity);
        }

    }
}
