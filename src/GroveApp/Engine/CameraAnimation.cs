using System;
using Avalonia;
using GroveApp.DesignSystem;

namespace GroveApp.Engine;

/// <summary>
/// Deterministic duration-based camera settling. A new target starts from the
/// current rendered state, so repeated pan/zoom commands never use stale state.
/// </summary>
public sealed class CameraAnimation
{
    private Point _startPosition;
    private Point _targetPosition;
    private double _startScale;
    private double _targetScale;
    private TimeSpan _elapsed;
    private TimeSpan _duration;

    public bool IsActive { get; private set; }
    public Point TargetPosition => _targetPosition;
    public double TargetScale => _targetScale;

    public void Start(Point currentPosition, double currentScale, Point targetPosition, double targetScale, TimeSpan duration)
    {
        _startPosition = currentPosition;
        _targetPosition = targetPosition;
        _startScale = currentScale;
        _targetScale = targetScale;
        _elapsed = TimeSpan.Zero;
        _duration = duration < TimeSpan.Zero ? TimeSpan.Zero : duration;
        IsActive = _duration > TimeSpan.Zero &&
                   (Distance(_startPosition, _targetPosition) > 0.0001 || Math.Abs(_startScale - _targetScale) > 0.000001);
    }

    public void Cancel(Point currentPosition, double currentScale)
    {
        _startPosition = currentPosition;
        _targetPosition = currentPosition;
        _startScale = currentScale;
        _targetScale = currentScale;
        _elapsed = TimeSpan.Zero;
        _duration = TimeSpan.Zero;
        IsActive = false;
    }

    public bool Step(TimeSpan elapsed, ref Point position, ref double scale)
    {
        if (!IsActive) return false;

        TimeSpan safeElapsed = elapsed < TimeSpan.Zero ? TimeSpan.Zero : elapsed;
        _elapsed = _elapsed + safeElapsed;
        if (_elapsed >= _duration)
        {
            position = _targetPosition;
            scale = _targetScale;
            IsActive = false;
            return true;
        }

        double linearProgress = _duration == TimeSpan.Zero
            ? 1.0
            : Math.Clamp(_elapsed.TotalMilliseconds / _duration.TotalMilliseconds, 0.0, 1.0);
        double progress = Motion.EvaluateEase(linearProgress);
        position = new Point(
            Lerp(_startPosition.X, _targetPosition.X, progress),
            Lerp(_startPosition.Y, _targetPosition.Y, progress));
        scale = Lerp(_startScale, _targetScale, progress);
        return true;
    }

    private static double Lerp(double start, double end, double progress) => start + ((end - start) * progress);
    private static double Distance(Point a, Point b) => Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
}
