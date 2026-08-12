#nullable enable

using System;
using Avalonia.Media;
using GroveApp.DesignSystem;
using Colors = GroveApp.DesignSystem.Colors;

namespace GroveApp.Engine;

public readonly record struct LayerInsertionFeedbackOrigin(double X, double Y);

public enum LayerFeedbackPhase
{
    Idle,
    Running,
    Completed
}

/// <summary>
/// Immutable render input produced by <see cref="LayerFeedbackAnimationController"/>.
/// A renderer can consume this state for Plane 0 and Plane 2 without giving the
/// controller access to the field ledger or content collections.
/// </summary>
public readonly record struct LayerFeedbackAnimationState(
    LayerFeedbackPhase Phase,
    LayerInsertionFeedbackOrigin Origin,
    TimeSpan Elapsed,
    TimeSpan SweepDuration,
    TimeSpan RowDuration,
    double SweepProgress,
    double RowProgress,
    double SweepOpacity,
    bool IsReducedMotion,
    Color SweepColor,
    Color LayerRowColor)
{
    public bool IsActive => Phase == LayerFeedbackPhase.Running;

    public bool IsCompleted => Phase == LayerFeedbackPhase.Completed;
}

/// <summary>
/// Deep module for insertion feedback timing. It owns the time model and motion policy;
/// the external interface is only Begin, Advance, Reset, and the immutable state event.
/// It deliberately has no ledger dependency and never mutates spatial field state.
/// </summary>
public sealed class LayerFeedbackAnimationController
{
    private LayerInsertionFeedbackOrigin _origin;
    private TimeSpan _elapsed;
    private TimeSpan _sweepDuration;
    private TimeSpan _rowDuration;
    private bool _isReducedMotion;
    private LayerFeedbackAnimationState _state;

    public LayerFeedbackAnimationController()
    {
        _state = CreateState(LayerFeedbackPhase.Idle);
    }

    public event Action<LayerFeedbackAnimationState>? StateChanged;

    public LayerFeedbackAnimationState State => _state;

    public LayerFeedbackAnimationState Begin(
        LayerInsertionFeedbackOrigin origin,
        bool prefersReducedMotion = false)
    {
        _origin = origin;
        _elapsed = TimeSpan.Zero;
        _isReducedMotion = prefersReducedMotion || Motion.IsReducedMotionEnabled;
        _sweepDuration = ResolveDuration(Motion.SweepDuration, _isReducedMotion);
        _rowDuration = ResolveDuration(Motion.PlaceDuration, _isReducedMotion);

        var totalDuration = Max(_sweepDuration, _rowDuration);
        if (totalDuration == TimeSpan.Zero)
        {
            _elapsed = totalDuration;
            return Publish(LayerFeedbackPhase.Completed);
        }

        return Publish(LayerFeedbackPhase.Running);
    }

    public LayerFeedbackAnimationState Advance(TimeSpan elapsed)
    {
        if (elapsed < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(elapsed), "Animation time cannot move backwards.");
        }

        if (_state.Phase != LayerFeedbackPhase.Running)
        {
            return _state;
        }

        var totalDuration = Max(_sweepDuration, _rowDuration);
        _elapsed = Min(totalDuration, _elapsed + elapsed);
        var phase = _elapsed >= totalDuration
            ? LayerFeedbackPhase.Completed
            : LayerFeedbackPhase.Running;

        return Publish(phase);
    }

    public LayerFeedbackAnimationState Complete()
    {
        _elapsed = Max(_sweepDuration, _rowDuration);
        return Publish(LayerFeedbackPhase.Completed);
    }

    public LayerFeedbackAnimationState Reset()
    {
        _origin = default;
        _elapsed = TimeSpan.Zero;
        _sweepDuration = TimeSpan.Zero;
        _rowDuration = TimeSpan.Zero;
        _isReducedMotion = false;
        return Publish(LayerFeedbackPhase.Idle);
    }

    private LayerFeedbackAnimationState Publish(LayerFeedbackPhase phase)
    {
        _state = CreateState(phase);
        StateChanged?.Invoke(_state);
        return _state;
    }

    private LayerFeedbackAnimationState CreateState(LayerFeedbackPhase phase)
    {
        var sweepLinearProgress = Progress(_elapsed, _sweepDuration);
        var rowLinearProgress = Progress(_elapsed, _rowDuration);

        return new LayerFeedbackAnimationState(
            Phase: phase,
            Origin: _origin,
            Elapsed: _elapsed,
            SweepDuration: _sweepDuration,
            RowDuration: _rowDuration,
            SweepProgress: Motion.EvaluateEase(sweepLinearProgress),
            RowProgress: Motion.EvaluateOvershoot(rowLinearProgress),
            SweepOpacity: 0.35 * Math.Pow(1.0 - sweepLinearProgress, 2.0),
            IsReducedMotion: _isReducedMotion,
            SweepColor: Colors.LayerFill,
            LayerRowColor: Colors.LayerFill);
    }

    private static TimeSpan ResolveDuration(TimeSpan tokenDuration, bool reducedMotion)
    {
        return reducedMotion ? TimeSpan.Zero : tokenDuration;
    }

    private static double Progress(TimeSpan elapsed, TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
        {
            return 1.0;
        }

        return Math.Clamp(elapsed.TotalMilliseconds / duration.TotalMilliseconds, 0.0, 1.0);
    }

    private static TimeSpan Max(TimeSpan left, TimeSpan right) =>
        left >= right ? left : right;

    private static TimeSpan Min(TimeSpan left, TimeSpan right) =>
        left <= right ? left : right;
}
