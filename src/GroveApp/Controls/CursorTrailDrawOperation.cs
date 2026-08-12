using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using GroveApp.DesignSystem;
using GroveApp.Models;
using SkiaSharp;
using DesignColors = GroveApp.DesignSystem.Colors;

namespace GroveApp.Controls;

public sealed class CursorTrailDrawOperation : ICustomDrawOperation
{
    private readonly Rect _bounds;
    private readonly Matrix _cameraTransform;
    private readonly IReadOnlyList<SpentCell> _trail;
    private readonly long _trailVersion;

    public CursorTrailDrawOperation(
        Rect bounds,
        Matrix cameraTransform,
        IReadOnlyList<SpentCell> trail,
        long trailVersion)
    {
        _bounds = bounds;
        _cameraTransform = cameraTransform;
        _trail = trail ?? throw new ArgumentNullException(nameof(trail));
        _trailVersion = trailVersion;
    }

    public Rect Bounds => _bounds;

    public bool HitTest(Point p) => false;

    public bool Equals(ICustomDrawOperation? other) =>
        other is CursorTrailDrawOperation operation &&
        _bounds == operation._bounds &&
        _cameraTransform == operation._cameraTransform &&
        _trailVersion == operation._trailVersion &&
        ReferenceEquals(_trail, operation._trail);

    public void Dispose()
    {
    }

    public void Render(ImmediateDrawingContext context)
    {
        if (_trail.Count == 0)
        {
            return;
        }

        var feature = context.TryGetFeature(typeof(ISkiaSharpApiLeaseFeature)) as ISkiaSharpApiLeaseFeature;
        if (feature is null)
        {
            return;
        }

        using var lease = feature.Lease();
        using var paint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            IsAntialias = false,
            Color = new SKColor(DesignColors.NoteText.R, DesignColors.NoteText.G, DesignColors.NoteText.B)
        };

        foreach (SpentCell spent in _trail)
        {
            Point start = _cameraTransform.Transform(new Point(spent.WorldX, spent.WorldY));
            Point end = _cameraTransform.Transform(new Point(
                spent.WorldX + spent.WorldWidth,
                spent.WorldY + spent.WorldHeight));
            paint.Color = new SKColor(
                DesignColors.NoteText.R,
                DesignColors.NoteText.G,
                DesignColors.NoteText.B,
                ToAlpha(spent.Energy));
            lease.SkCanvas.DrawRect(
                new SKRect(
                    (float)start.X,
                    (float)start.Y,
                    (float)end.X,
                    (float)end.Y),
                paint);
        }
    }

    private static byte ToAlpha(double energy) =>
        (byte)Math.Clamp(
            Math.Round(Tokens.CursorFillGain * energy * byte.MaxValue),
            0,
            byte.MaxValue);
}
