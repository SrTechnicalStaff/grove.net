using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using GroveApp.DesignSystem;
using GroveApp.Engine;
using SkiaSharp;

namespace GroveApp.Controls;

/// <summary>
/// Skia adapter for the engine's ordered, merged perimeter topology. The
/// engine exposes only integer grid-space edges; camera projection and pixels
/// belong to this Plane 0 rendering seam.
/// </summary>
public sealed class PerimeterContourDrawOperation : ICustomDrawOperation
{
    private readonly Rect _bounds;
    private readonly IReadOnlyList<PerimeterContourEdge> _edges;
    private readonly Matrix _cameraTransform;
    private readonly double _cellSize;
    private readonly SKColor _color;

    public PerimeterContourDrawOperation(
        Rect bounds,
        IReadOnlyList<PerimeterContourEdge> edges,
        Matrix cameraTransform,
        double cellSize,
        Color color,
        double alpha)
    {
        _bounds = bounds;
        _edges = edges;
        _cameraTransform = cameraTransform;
        _cellSize = cellSize;
        _color = new SKColor(
            color.R,
            color.G,
            color.B,
            (byte)Math.Clamp(Math.Round(alpha * byte.MaxValue), 0, byte.MaxValue));
    }

    public Rect Bounds => _bounds;

    public bool HitTest(Point p) => false;

    public bool Equals(ICustomDrawOperation? other) =>
        other is PerimeterContourDrawOperation operation &&
        _bounds == operation._bounds &&
        _edges.Equals(operation._edges) &&
        _cameraTransform.Equals(operation._cameraTransform) &&
        Math.Abs(_cellSize - operation._cellSize) < 0.000001 &&
        _color == operation._color;

    public void Dispose()
    {
    }

    public void Render(ImmediateDrawingContext context)
    {
        if (_edges.Count == 0)
        {
            return;
        }

        var feature = context.TryGetFeature(typeof(ISkiaSharpApiLeaseFeature)) as ISkiaSharpApiLeaseFeature;
        if (feature is null)
        {
            return;
        }

        using var lease = feature.Lease();
        using var path = new SKPath();
        using var paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = (float)Tokens.FieldPerimeterWidth,
            IsAntialias = true,
            Color = _color
        };

        BuildPath(path);
        lease.SkCanvas.DrawPath(path, paint);
    }

    private void BuildPath(SKPath path)
    {
        (int X, int Y)? activeLoopStart = null;
        (int X, int Y)? previousEnd = null;

        foreach (PerimeterContourEdge edge in _edges)
        {
            if (previousEnd is null || previousEnd.Value != edge.Start)
            {
                if (activeLoopStart is not null)
                {
                    path.Close();
                }

                Point start = ToScreen(edge.Start);
                path.MoveTo((float)start.X, (float)start.Y);
                activeLoopStart = edge.Start;
            }

            Point end = ToScreen(edge.End);
            path.LineTo((float)end.X, (float)end.Y);
            previousEnd = edge.End;

            if (edge.End == activeLoopStart)
            {
                path.Close();
                activeLoopStart = null;
                previousEnd = null;
            }
        }

        if (activeLoopStart is not null)
        {
            path.Close();
        }
    }

    private Point ToScreen((int X, int Y) cellPoint) =>
        _cameraTransform.Transform(new Point(cellPoint.X * _cellSize, cellPoint.Y * _cellSize));
}
