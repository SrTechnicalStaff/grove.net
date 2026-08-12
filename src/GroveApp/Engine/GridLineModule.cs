using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using GroveApp.DesignSystem;
using SkiaSharp;
using Colors = GroveApp.DesignSystem.Colors;

namespace GroveApp.Engine;

public readonly record struct SpatialGridGeometryConfig(
    double CellSize,
    int Subdivisions,
    int SupercellMultiplier)
{
    public static SpatialGridGeometryConfig Default => new(
        Tokens.GridCell,
        Tokens.GridSubdivisions,
        Tokens.GridSupercell);

    public double MinorCellSize => CellSize / Math.Max(1, Subdivisions);
    public double SupercellPitch => CellSize * Math.Max(1, SupercellMultiplier);
}

/// <summary>
/// Compatibility adapter for callers that still use Avalonia line drawing.
/// Plane 0 uses <see cref="GridLineDrawOperation"/> for its live render seam.
/// </summary>
public class GridLineModule
{
    public void RenderGridLines(
        DrawingContext context,
        Func<Point, Point> worldToScreen,
        double cellSize,
        double zoom,
        int minCellX,
        int maxCellX,
        int minCellY,
        int maxCellY,
        SpatialGridGeometryConfig? geometry = null,
        double renderScaling = 1.0)
    {
        SpatialGridGeometryConfig grid = geometry ?? SpatialGridGeometryConfig.Default with { CellSize = cellSize };
        double minorSpacing = grid.MinorCellSize * zoom;
        double majorSpacing = grid.CellSize * zoom;
        double superSpacing = grid.SupercellPitch * zoom;
        double minorAlpha = CalculateAlpha(minorSpacing);
        double majorAlpha = CalculateAlpha(majorSpacing);
        double superAlpha = CalculateAlpha(superSpacing);
        if (minorAlpha <= 0.001 && majorAlpha <= 0.001 && superAlpha <= 0.001) return;

        Point topLeft = worldToScreen(new Point(minCellX * cellSize, minCellY * cellSize));
        Point bottomRight = worldToScreen(new Point((maxCellX + 1) * cellSize, (maxCellY + 1) * cellSize));
        if (minorAlpha > 0.001)
        {
            var baseColor = Color.Parse(Colors.GridMinorLineHex);
            var pen = new Pen(new SolidColorBrush(Color.FromArgb(Alpha(minorAlpha), baseColor.R, baseColor.G, baseColor.B)), Tokens.StrokeHairline);
            int minMinor = minCellX * grid.Subdivisions;
            int maxMinor = maxCellX * grid.Subdivisions;
            for (int index = minMinor; index <= maxMinor; index++)
            {
                if (index % grid.Subdivisions == 0) continue;
                double screenX = Snap(worldToScreen(new Point(index * grid.MinorCellSize, 0)).X, renderScaling);
                context.DrawLine(pen, new Point(screenX, topLeft.Y), new Point(screenX, bottomRight.Y));
            }
            for (int index = minMinor; index <= maxMinor; index++)
            {
                if (index % grid.Subdivisions == 0) continue;
                double screenY = Snap(worldToScreen(new Point(0, index * grid.MinorCellSize)).Y, renderScaling);
                context.DrawLine(pen, new Point(topLeft.X, screenY), new Point(bottomRight.X, screenY));
            }
        }

        DrawAvaloniaTier(context, worldToScreen, minCellX, maxCellX, minCellY, maxCellY,
            grid.CellSize, cellSize, grid.SupercellMultiplier, majorAlpha, Colors.GridMajorLineHex, renderScaling);
        DrawAvaloniaTier(context, worldToScreen, minCellX, maxCellX, minCellY, maxCellY,
            grid.SupercellPitch, cellSize, 1, superAlpha, Colors.GridMajorLineHex, renderScaling);
    }

    private static void DrawAvaloniaTier(
        DrawingContext context,
        Func<Point, Point> worldToScreen,
        int minCellX, int maxCellX, int minCellY, int maxCellY,
        double pitch, double cellSize, int skipMultiplier, double alpha, string colorHex, double renderScaling)
    {
        if (alpha <= 0.001) return;
        Point topLeft = worldToScreen(new Point(minCellX * cellSize, minCellY * cellSize));
        Point bottomRight = worldToScreen(new Point((maxCellX + 1) * cellSize, (maxCellY + 1) * cellSize));
        Color baseColor = Color.Parse(colorHex);
        var pen = new Pen(new SolidColorBrush(Color.FromArgb(Alpha(alpha), baseColor.R, baseColor.G, baseColor.B)), Tokens.StrokeHairline);
        int minX = (int)Math.Floor((minCellX * cellSize) / pitch);
        int maxX = (int)Math.Ceiling(((maxCellX + 1) * cellSize) / pitch);
        for (int index = minX; index <= maxX; index++)
        {
            if (skipMultiplier > 1 && index % skipMultiplier == 0) continue;
            double x = Snap(worldToScreen(new Point(index * pitch, 0)).X, renderScaling);
            context.DrawLine(pen, new Point(x, topLeft.Y), new Point(x, bottomRight.Y));
        }
        int minY = (int)Math.Floor((minCellY * cellSize) / pitch);
        int maxY = (int)Math.Ceiling(((maxCellY + 1) * cellSize) / pitch);
        for (int index = minY; index <= maxY; index++)
        {
            if (skipMultiplier > 1 && index % skipMultiplier == 0) continue;
            double y = Snap(worldToScreen(new Point(0, index * pitch)).Y, renderScaling);
            context.DrawLine(pen, new Point(topLeft.X, y), new Point(bottomRight.X, y));
        }
    }

    private static double CalculateAlpha(double spacing) =>
        Math.Clamp((spacing - Tokens.GridFadeStart) / (Tokens.GridFadeEnd - Tokens.GridFadeStart), 0.0, 1.0);

    private static byte Alpha(double value) => (byte)Math.Round(value * byte.MaxValue);
    private static double Snap(double value, double renderScaling) =>
        Math.Round(value * Math.Max(0.01, renderScaling)) / Math.Max(0.01, renderScaling);
}

/// <summary>
/// Skia Plane 0 grid pass. Minor segments are masked against the prepared
/// discrete aura-cell set; major and supercell orientation lines remain intact.
/// </summary>
public sealed class GridLineDrawOperation : ICustomDrawOperation
{
    private readonly Rect _bounds;
    private readonly int _minX, _maxX, _minY, _maxY;
    private readonly double _cellSize, _zoom, _renderScaling;
    private readonly Matrix _cameraTransform;
    private readonly bool _visible;
    private readonly IReadOnlySet<(int col, int row)> _auraCells;

    public GridLineDrawOperation(
        Rect bounds,
        int minX,
        int maxX,
        int minY,
        int maxY,
        double cellSize,
        Matrix cameraTransform,
        double renderScaling,
        bool visible,
        IReadOnlySet<(int col, int row)> auraCells)
    {
        _bounds = bounds;
        _minX = minX;
        _maxX = maxX;
        _minY = minY;
        _maxY = maxY;
        _cellSize = cellSize;
        _cameraTransform = cameraTransform;
        _zoom = cameraTransform.M11;
        _renderScaling = Math.Max(0.01, renderScaling);
        _visible = visible;
        _auraCells = auraCells;
    }

    public Rect Bounds => _bounds;
    public bool HitTest(Point p) => false;
    public bool Equals(ICustomDrawOperation? other) =>
        other is GridLineDrawOperation operation &&
        _bounds == operation._bounds &&
        _minX == operation._minX &&
        _maxX == operation._maxX &&
        _minY == operation._minY &&
        _maxY == operation._maxY &&
        Math.Abs(_cellSize - operation._cellSize) < 0.000001 &&
        _cameraTransform.Equals(operation._cameraTransform) &&
        Math.Abs(_renderScaling - operation._renderScaling) < 0.000001 &&
        _visible == operation._visible &&
        ReferenceEquals(_auraCells, operation._auraCells);
    public void Dispose() { }

    public void Render(ImmediateDrawingContext context)
    {
        if (!_visible) return;
        var feature = context.TryGetFeature(typeof(ISkiaSharpApiLeaseFeature)) as ISkiaSharpApiLeaseFeature;
        if (feature is null) return;
        using var lease = feature.Lease();
        SKCanvas canvas = lease.SkCanvas;
        using var minor = CreatePaint(Colors.GridMinorLineHex);
        using var major = CreatePaint(Colors.GridMajorLineHex);
        DrawMinorTier(canvas, minor);
        DrawMajorTier(canvas, major, _cellSize, Tokens.GridSupercell);
        DrawMajorTier(canvas, major, _cellSize * Tokens.GridSupercell, 1);
    }

    private void DrawMinorTier(SKCanvas canvas, SKPaint paint)
    {
        double pitch = _cellSize / Tokens.GridSubdivisions;
        paint.Color = WithFade(ColorToSkia(Colors.GridMinorLineHex), pitch * _zoom);
        if (paint.Color.Alpha == 0) return;

        using var path = new SKPath();
        int minMinor = _minX * Tokens.GridSubdivisions;
        int maxMinor = _maxX * Tokens.GridSubdivisions;
        for (int index = minMinor; index <= maxMinor; index++)
        {
            if (index % Tokens.GridSubdivisions == 0) continue;
            double worldX = index * pitch;
            float screenX = Snap(TransformX(worldX));
            int cellX = (int)Math.Floor(worldX / _cellSize);
            for (int row = _minY; row <= _maxY; row++)
            {
                if (_auraCells.Contains((cellX, row))) continue;
                AddLine(path,
                    screenX,
                    Snap(TransformY(row * _cellSize)),
                    screenX,
                    Snap(TransformY((row + 1) * _cellSize)));
            }
        }

        for (int index = _minY * Tokens.GridSubdivisions; index <= _maxY * Tokens.GridSubdivisions; index++)
        {
            if (index % Tokens.GridSubdivisions == 0) continue;
            double worldY = index * pitch;
            float screenY = Snap(TransformY(worldY));
            int cellY = (int)Math.Floor(worldY / _cellSize);
            for (int col = _minX; col <= _maxX; col++)
            {
                if (_auraCells.Contains((col, cellY))) continue;
                AddLine(path,
                    Snap(TransformX(col * _cellSize)),
                    screenY,
                    Snap(TransformX((col + 1) * _cellSize)),
                    screenY);
            }
        }

        canvas.DrawPath(path, paint);
    }

    private void DrawMajorTier(SKCanvas canvas, SKPaint paint, double pitch, int skipMultiplier)
    {
        paint.Color = WithFade(ColorToSkia(Colors.GridMajorLineHex), pitch * _zoom);
        if (paint.Color.Alpha == 0) return;
        using var path = new SKPath();
        int minIndex = (int)Math.Floor((_minX * _cellSize) / pitch);
        int maxIndex = (int)Math.Ceiling(((_maxX + 1) * _cellSize) / pitch);
        for (int index = minIndex; index <= maxIndex; index++)
        {
            if (skipMultiplier > 1 && index % skipMultiplier == 0) continue;
            Point top = Transform(new Point(index * pitch, _minY * _cellSize));
            Point bottom = Transform(new Point(index * pitch, (_maxY + 1) * _cellSize));
            AddLine(path, Snap(top.X), Snap(top.Y), Snap(bottom.X), Snap(bottom.Y));
        }

        minIndex = (int)Math.Floor((_minY * _cellSize) / pitch);
        maxIndex = (int)Math.Ceiling(((_maxY + 1) * _cellSize) / pitch);
        for (int index = minIndex; index <= maxIndex; index++)
        {
            if (skipMultiplier > 1 && index % skipMultiplier == 0) continue;
            Point left = Transform(new Point(_minX * _cellSize, index * pitch));
            Point right = Transform(new Point((_maxX + 1) * _cellSize, index * pitch));
            AddLine(path, Snap(left.X), Snap(left.Y), Snap(right.X), Snap(right.Y));
        }
        canvas.DrawPath(path, paint);
    }

    private SKPaint CreatePaint(string colorHex) => new()
    {
        Style = SKPaintStyle.Stroke,
        StrokeWidth = (float)Tokens.StrokeHairline,
        IsAntialias = false,
        Color = SKColor.Parse(colorHex)
    };

    private SKColor WithFade(SKColor color, double spacing)
    {
        double alpha = Math.Clamp((spacing - Tokens.GridFadeStart) / (Tokens.GridFadeEnd - Tokens.GridFadeStart), 0.0, 1.0);
        return color.WithAlpha((byte)Math.Round(alpha * byte.MaxValue));
    }

    private float Snap(double value) => (float)(Math.Round(value * _renderScaling) / _renderScaling);
    private Point Transform(Point worldPoint) => _cameraTransform.Transform(worldPoint);
    private double TransformX(double worldX) => Transform(new Point(worldX, 0)).X;
    private double TransformY(double worldY) => Transform(new Point(0, worldY)).Y;
    private static void AddLine(SKPath path, float x1, float y1, float x2, float y2)
    {
        path.MoveTo(x1, y1);
        path.LineTo(x2, y2);
    }

    private static SKColor ColorToSkia(string value) => SKColor.Parse(value);
}
