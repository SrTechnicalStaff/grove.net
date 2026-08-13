---
status: "PARTIAL — verified multi-type resize and refusal feedback seam"
---

# ADR-051: Interactive Resize Engine and Cell Alignment System

| Property | Value |
| :--- | :--- |
| **Status** | PARTIAL — verified multi-type resize and refusal feedback seam |
| **Date** | 2026-08-12 |
| **Area** | Spatial Grid Engine / Transform & Resize Subsystem |
| **Target Runtime** | C# 13 / .NET 9 / Avalonia 11.2.5 / SkiaSharp 3.x |
| **Authors** | Chief Product Definition Architect |

---

## 1. Executive Context & Architectural Principles

Placement footprints on the Grove v9 Spatial Grid (`Note`, `Document`, `Picture`) exist as discrete cell-aligned rectangles on Plane 0. Unlike freeform vector canvas tools that allow continuous floating pixel dimensions, Grove enforces strict **Discrete Cell Alignment**: every content footprint must be an integral cell rectangle constrained between $1 \times 1$ world cells ($220 \times 220\text{ DIPs}$) and $8 \times 8$ world cells ($1760 \times 1760\text{ DIPs}$).

### Key Architectural Drivers:
1. **Interactive Corner Drag-to-Resize**: Active selections expose 4 corner resize affordances. Dragging a corner handle dynamically adjusts the footprint boundary in discrete cell increments while maintaining cell alignment.
2. **Strict Region Collision Checks (`IsRegionFree`)**: Before committing a resize operation or rendering a valid placement preview, the engine performs real-time region collision queries against spatial R-Tree indices. A resize into an occupied spatial region triggers immediate refusal state rendering.
3. **Discrete Metric Constraints**: Dimensions are strictly bounded: $W_{\text{cells}} \in [1, 8]$, $H_{\text{cells}} \in [1, 8]$. Sub-cell rounding uses half-cell hysteresis snapping to prevent jitter during pointer motion.
4. **Compositor Ghost Previewing**: During drag operations, the original item remains rendered in place while a footprint projection ghost (`--signal-interaction` at 6% fill / 2px inset edge or `--signal-refusal` at 12% fill / 45° hatching) follows the active resize handle.

---

## 2. Spatial Geometry & Collision Mathematics

```
           X_origin                     X_origin + W_new
  Y_origin +----------------------------+ (NE Handle)
           | Existing Content           |
           | Footprint                  |
           |                            |
Y_origin   +----------------------------+
+ H_new    (NW)                         (SE Active Drag Handle)
                                           \
                                            +---> Snap to discrete cell boundary
                                                  Validate IsRegionFree(Rect)
```

### 2.1 Corner Handle Hit Geometry

Let $R_{\text{world}} = [x_0, y_0, x_1, y_1]$ be the world-space bounding box of the active content item. The four corner handle centers $H_k \in \{NW, NE, SE, SW\}$ in world space are:

$$H_{NW} = (x_0, y_0), \quad H_{NE} = (x_1, y_0), \quad H_{SE} = (x_1, y_1), \quad H_{SW} = (x_0, y_1)$$

Given screen-space pointer position $P_{\text{screen}}$, camera transform $T_{\text{camera}}$, and handle hit radius $r_{\text{hit}} = 12.0\text{ px}$:

$$\text{HitTest}(P_{\text{screen}}, H_k) = \mathbb{I}\left( \left\| P_{\text{screen}} - T_{\text{camera}}(H_k) \right\| \le r_{\text{hit}} \right)$$

### 2.2 Discrete Snap-to-Cell Delta Mathematics

Let $P_{\text{drag\_world}} = T_{\text{camera}}^{-1}(P_{\text{screen}})$ be the active world pointer position during a SE corner drag. The unconstrained candidate width $W_{\text{raw}}$ and height $H_{\text{raw}}$ in fractional cells are:

$$W_{\text{raw}} = \frac{P_{\text{drag\_world}}.X - x_0}{P_{\text{cell}}}, \quad H_{\text{raw}} = \frac{P_{\text{drag\_world}}.Y - y_0}{P_{\text{cell}}}$$

Applying half-cell rounding with bounds clamping $[1, 8]$:

$$W_{\text{target}} = \text{clamp}\left( \left\lfloor W_{\text{raw}} + 0.5 \right\rfloor, \, 1, \, 8 \right)$$
$$H_{\text{target}} = \text{clamp}\left( \left\lfloor H_{\text{raw}} + 0.5 \right\rfloor, \, 1, \, 8 \right)$$

Candidate cell bounds rectangle $R_{\text{candidate}}$:

$$R_{\text{candidate}} = \left[ C_{x, 0}, \, C_{y, 0}, \, W_{\text{target}}, \, H_{\text{target}} \right]$$

### 2.3 Region Collision Checking (`IsRegionFree`)

Let $\mathcal{O}_{\text{grid}}$ be the spatial index containing all item footprints $I_i = [X_i, Y_i, W_i, H_i]$ on the selected Grid Layer.
The region collision validation function $\text{IsRegionFree}(R_{\text{candidate}}, \text{ItemID}_{\text{active}})$ is defined as:

$$\text{IsRegionFree}(R_{\text{candidate}}, \text{ID}_{\text{active}}) = \bigwedge_{I_i \in \mathcal{O}_{\text{layer}} \setminus \{\text{ID}_{\text{active}}\}} \left( R_{\text{candidate}} \cap I_i = \varnothing \right)$$

Where rectangle intersection $A \cap B \neq \varnothing$ holds if and only if:

$$(A.X < B.X + B.W) \land (A.X + A.W > B.X) \land (A.Y < B.Y + B.H) \land (A.Y + A.H > B.Y)$$

---

## 3. C# 13 Type Interfaces & State Pipeline

```csharp
namespace Grove.SpatialGrid.Resize;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Grove.SpatialGrid.Cursor;

public enum ResizeHandleLocation : byte
{
    None = 0,
    NorthWest = 1,
    NorthEast = 2,
    SouthEast = 3,
    SouthWest = 4
}

public readonly record struct SpatialRegion(int X, int Y, int Width, int Height)
{
    public bool Intersects(SpatialRegion other) =>
        X < other.X + other.Width && X + Width > other.X &&
        Y < other.Y + other.Height && Y + Height > other.Y;
}

public enum ResizeStatus : byte
{
    ValidPendingCommit = 0,
    RefusedCollision = 1,
    RefusedOutOfBounds = 2
}

public readonly record struct ResizeState(
    Guid TargetItemId,
    SpatialRegion OriginalRegion,
    SpatialRegion TargetRegion,
    ResizeHandleLocation ActiveHandle,
    ResizeStatus Status
);

public interface ISpatialResizeService
{
    ResizeState? ActiveResizeOperation { get; }
    event Action<ResizeState>? ResizeStateChanged;

    bool IsRegionFree(SpatialRegion region, Guid ignoreItemId, Guid layerId);
    
    bool BeginResize(Guid itemId, ResizeHandleLocation handle, CellCoordinate initialPointerCell);
    ResizeState UpdateResize(CellCoordinate currentPointerCell);
    bool CommitResize();
    void CancelResize();
}
```

---

## 4. Avalonia 11.2.5 & SkiaSharp Rendering Pipeline Integration

### 4.1 Handle Affordance & Ghost Preview Custom Draw

```csharp
namespace Grove.SpatialGrid.Rendering;

using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Grove.SpatialGrid.Resize;
using SkiaSharp;

public sealed class ResizePreviewDrawOperation : ICustomDrawOperation
{
    private readonly Rect _bounds;
    private readonly ResizeState _state;
    private readonly Matrix3x3 _cameraTransform;
    private readonly float _cellPitchDip;

    public ResizePreviewDrawOperation(
        Rect bounds,
        ResizeState state,
        Matrix3x3 cameraTransform,
        float cellPitchDip = 220.0f)
    {
        _bounds = bounds;
        _state = state;
        _cameraTransform = cameraTransform;
        _cellPitchDip = cellPitchDip;
    }

    public Rect Bounds => _bounds;
    public void Dispose() { }
    public bool Equals(ICustomDrawOperation? other) => false;
    public bool HitTest(Point p) => false;

    public void Render(ImmediateDrawingContext context)
    {
        var skiaContext = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
        if (skiaContext is null) return;

        using var lease = skiaContext.Lease();
        var canvas = lease.SkCanvas;

        canvas.Save();
        canvas.SetMatrix(_cameraTransform);

        float pitch = _cellPitchDip;
        var r = _state.TargetRegion;
        var worldRect = SKRect.Create(r.X * pitch, r.Y * pitch, r.Width * pitch, r.Height * pitch);

        bool isValid = _state.Status == ResizeStatus.ValidPendingCommit;

        // 1. Footprint Projection Fill
        SKColor fillColor = isValid ? SKColor.Parse("#3B82F6").WithAlpha((byte)(0.06f * 255))
                                    : SKColor.Parse("#EF4444").WithAlpha((byte)(0.12f * 255));

        using (var fillPaint = new SKPaint { Style = SKPaintStyle.Fill, Color = fillColor })
        {
            canvas.DrawRect(worldRect, fillPaint);
        }

        // 2. Refusal Hatching Lines (45 degrees) if Refused
        if (!isValid)
        {
            RenderRefusalHatching(canvas, worldRect);
        }

        // 3. Footprint Projection Border
        SKColor strokeColor = isValid ? SKColor.Parse("#3B82F6").WithAlpha((byte)(0.80f * 255))
                                      : SKColor.Parse("#EF4444").WithAlpha((byte)(0.85f * 255));

        using (var strokePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2.0f / canvas.TotalMatrix.ScaleX,
            Color = strokeColor,
            PathEffect = isValid ? SKPathEffect.CreateDash(new float[] { 4.0f, 4.0f }, 0.0f) : null
        })
        {
            canvas.DrawRect(worldRect, strokePaint);
        }

        canvas.Restore();
    }

    private static void RenderRefusalHatching(SKCanvas canvas, SKRect bounds)
    {
        using var hatchPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.5f / canvas.TotalMatrix.ScaleX,
            Color = SKColor.Parse("#EF4444").WithAlpha((byte)(0.40f * 255)),
            IsAntialias = true
        };

        canvas.Save();
        canvas.ClipRect(bounds);
        float step = 20.0f;
        for (float x = bounds.Left - bounds.Height; x < bounds.Right; x += step)
        {
            canvas.DrawLine(x, bounds.Bottom, x + bounds.Height, bounds.Top, hatchPaint);
        }
        canvas.Restore();
    }
}
```

---

## 5. Architectural Invariants & Refusal Protocol

1. **Hard Size Limits**: Footprints MUST NOT be resized smaller than $1 \times 1$ cell ($220 \times 220\text{ DIPs}$) nor larger than $8 \times 8$ cells ($1760 \times 1760\text{ DIPs}$). Pointer movement beyond these limits is clamped.
2. **Collision Atomic Refusal**: Committing a resize operation when `IsRegionFree` returns `false` is strictly forbidden. The system rejects the commit, reverts target bounds to `OriginalRegion`, and emits an inline refusal signal.
3. **Non-Destructive Content Reflow**: Text inside `Document` or `Note` placements reflows dynamically according to updated cell width. Media elements inside `Picture` placements re-evaluate pixel density scale without altering raw file assets.
4. **Collision Refusal Cross-Hatching**: When candidate resize bounds overlap an occupied cell, the preview renders in the invalid role `#E2625C` / `#F06543` (12% fill) with $45^\circ$ diagonal cross-hatching (`repeating-linear-gradient(45deg, rgba(226,98,92,0.22) 0 4px, transparent 4px 12px)` and inset border `inset 0 0 0 1px rgba(226,98,92,0.45)`). Structure and hue convey refusal together.
5. **Local Point-of-Action Refusal Strip**: Refusal feedback is delivered via a local strip toolbar positioned beside the refused footprint ("This space is occupied" with disabled `Place` action). Center-screen modal alert dialogs and canvas dimming are strictly forbidden.
