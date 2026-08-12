using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using SkiaSharp;

namespace GroveApp.Controls;

public enum VisualPlaneType
{
    Plane0_SpatialGrid = 0,
    Plane1_InformationPlane = 1,
    Plane2_HUDPlane = 2
}

public interface IPlaneView
{
    VisualPlaneType PlaneType { get; }
    int CompositorZIndex { get; }
    bool HandlesPointerInput(Point viewportPoint);
    void RenderPlane(SKCanvas canvas, Size viewportSize);
    bool DispatchPointerEvent(PointerEventArgs e, Point viewportPoint);
}

public interface IPlaneCompositor
{
    void RegisterPlaneView(IPlaneView planeView);
    void RenderAllPlanes(SKCanvas canvas, Size viewportSize);
    bool RoutePointerEvent(PointerEventArgs e, Point viewportPoint);
}

/// <summary>
/// Avalonia adapter for Grove's three-plane visual and input seam. The engine
/// remains UI-neutral; this control owns child z bands and pass-through routing.
/// </summary>
public sealed class ThreePlaneVisualCompositorContainer : Panel, IPlaneCompositor
{
    // Plane bands follow the design-system plane contracts: 10 / 300 / 400.
    // Plane 0 remains lowest; Information and HUD stay independently above it.
    public const int Plane0ZIndex = 10;
    public const int Plane1ZIndex = 300;
    public const int Plane2ZIndex = 400;

    private readonly List<IPlaneView>[] _planes = new[]
    {
        new List<IPlaneView>(), new List<IPlaneView>(), new List<IPlaneView>()
    };

    public void RegisterPlaneView(IPlaneView planeView)
    {
        ArgumentNullException.ThrowIfNull(planeView);
        int index = (int)planeView.PlaneType;
        if ((uint)index >= _planes.Length) throw new ArgumentOutOfRangeException(nameof(planeView));
        _planes[index].Add(planeView);
        if (planeView is ControlPlaneView controlView)
        {
            controlView.Control.ZIndex = planeView.CompositorZIndex;
        }
    }

    public void RenderAllPlanes(SKCanvas canvas, Size viewportSize)
    {
        ArgumentNullException.ThrowIfNull(canvas);

        // The explicit render seam is used by hosts that own an SKCanvas. The
        // running Avalonia host uses the same plane adapters through Panel
        // child composition; ControlPlaneView documents and dispatches that
        // path instead of pretending an Avalonia Control can be drawn into an
        // unrelated SKCanvas.
        for (int index = 0; index < _planes.Length; index++)
        {
            foreach (IPlaneView plane in _planes[index])
            {
                plane.RenderPlane(canvas, viewportSize);
            }
        }
    }

    public bool RoutePointerEvent(PointerEventArgs e, Point viewportPoint)
    {
        // Plane 0 remains the natural child recipient. Higher planes are
        // evaluated top-down and consume only when a visible frame is hit.
        for (int index = _planes.Length - 1; index >= 1; index--)
        {
            foreach (IPlaneView plane in _planes[index])
            {
                if (plane.HandlesPointerInput(viewportPoint) &&
                    plane.DispatchPointerEvent(e, viewportPoint))
                {
                    return true;
                }
            }
        }

        foreach (IPlaneView plane in _planes[0])
        {
            if (plane.HandlesPointerInput(viewportPoint) &&
                plane.DispatchPointerEvent(e, viewportPoint))
            {
                return true;
            }
        }

        return false;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (RoutePointerEvent(e, e.GetPosition(this))) e.Handled = true;
        base.OnPointerPressed(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (RoutePointerEvent(e, e.GetPosition(this))) e.Handled = true;
        base.OnPointerReleased(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (RoutePointerEvent(e, e.GetPosition(this))) e.Handled = true;
        base.OnPointerMoved(e);
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        if (RoutePointerEvent(e, e.GetPosition(this))) e.Handled = true;
        base.OnPointerWheelChanged(e);
    }

    public sealed class ControlPlaneView : IPlaneView
    {
        private readonly Action<SKCanvas, Size>? _renderAdapter;
        private readonly Func<PointerEventArgs, Point, bool>? _pointerDispatchAdapter;

        public ControlPlaneView(
            Control control,
            VisualPlaneType planeType,
            int compositorZIndex,
            Action<SKCanvas, Size>? renderAdapter = null,
            Func<PointerEventArgs, Point, bool>? pointerDispatchAdapter = null)
        {
            Control = control ?? throw new ArgumentNullException(nameof(control));
            PlaneType = planeType;
            CompositorZIndex = compositorZIndex;
            _renderAdapter = renderAdapter;
            _pointerDispatchAdapter = pointerDispatchAdapter;
        }

        public Control Control { get; }
        public VisualPlaneType PlaneType { get; }
        public int CompositorZIndex { get; }

        public bool HandlesPointerInput(Point viewportPoint) =>
            Control.IsVisible && Control.IsHitTestVisible && Control.Bounds.Contains(viewportPoint);

        public void RenderPlane(SKCanvas canvas, Size viewportSize)
        {
            if (_renderAdapter is not null)
            {
                _renderAdapter(canvas, viewportSize);
                return;
            }

            // Avalonia child composition is the live implementation for the
            // desktop host. Invalidate the child so its own render pass is
            // scheduled; this is an explicit adapter dispatch, not a claim
            // that Avalonia's retained Control can render directly on canvas.
            Control.InvalidateVisual();
        }

        public bool DispatchPointerEvent(PointerEventArgs e, Point viewportPoint)
        {
            if (_pointerDispatchAdapter is not null)
            {
                return _pointerDispatchAdapter(e, viewportPoint);
            }

            // Routed Avalonia input has already reached the child before the
            // container receives its bubbling notification. Confirm that the
            // child is the routed source (or one of its descendants) so the
            // compositor consumes only the plane that actually owns input.
            return e.Source is Visual source && IsWithinControl(source);
        }

        private bool IsWithinControl(Visual source)
        {
            for (Visual? current = source; current is not null; current = current.GetVisualParent())
            {
                if (ReferenceEquals(current, Control))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
