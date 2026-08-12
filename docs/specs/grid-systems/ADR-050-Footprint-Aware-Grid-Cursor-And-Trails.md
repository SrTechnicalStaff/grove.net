# ADR-050: Footprint-Aware Grid Cursor and Spent-Cell Trail Decay System

| Property | Value |
| :--- | :--- |
| **Status** | Accepted |
| **Date** | 2026-08-12 |
| **Area** | Spatial Grid Engine / Cursor Interaction & Render Subsystem |
| **Target Runtime** | C# 13 / .NET 9 / Avalonia 11.2.5 / SkiaSharp 3.x |
| **Authors** | Chief Product Definition Architect |

---

## 1. Executive Context & Architectural Principles

In Grove v9, keyboard and pointer spatial focus on Plane 0 (Spatial Grid Canvas) is unified into a single primitive: the **Grid Cursor**. Unlike traditional desktop OS environments that render an unscaled screen-space mouse arrow, Grove eliminates the system arrow pointer over the grid canvas in favor of a spatial, footprint-aware cell cursor.

### Key Architectural Requirements:
1. **Dynamic Footprint Expansion**: On empty cells, the cursor head occupies exactly $1 \times 1$ world cell ($220.0\text{ DIPs} \times 220.0\text{ DIPs}$). When addressing an existing placement (`Note`, `Document`, `Picture`), the head dynamically expands to match the content's full $N \times M$ grid cell footprint.
2. **Visual Tokens & Inset Edge Geometry**: The cursor head is rendered with a 22% fill gain (`--cursor-fill-gain` = 0.22, base ink `#F4F4F2`) and an inset 2px ring (`--cursor-ring` = 2px, opacity 0.88). Outset strokes are strictly forbidden to eliminate cell boundary ambiguity.
3. **18-Step Spent-Cell Decay Physics**: As the cursor translates across cells, it deposits kinetic energy into traversed cells. Spent cells decay exponentially per frame over exactly 18 frames ($\gamma = 0.84$) before being pruned below a threshold of $0.03$.
4. **Zero-Latency Compositor Execution**: Continuous cursor position updates and trail decay calculations execute on the Avalonia 11.2.5 render pipeline within custom Skia `CustomDrawOperation` calls, running at native display refresh rates (60Hz–144Hz) with zero garbage collection allocations on the render loop.

---

## 2. Spatial Cursor Geometry & Mathematical Formulations

```
+-------------------------------------------------------------------+
|                        Spatial Grid Canvas                        |
|                                                                   |
|   (Cell C_x, C_y-2)       (Cell C_x, C_y-1)      (Cell C_x, C_y)   |
|   +---------------+       +---------------+      +---------------+|
|   | Spent Cell t-2| ----> | Spent Cell t-1| ---> |  Cursor Head  ||
|   | E_2 = 0.093   |       | E_1 = 0.111   |      | E_0 = 0.60    ||
|   | Fill: 2.0%    |       | Fill: 2.4%    |      | Fill: 22.0%   ||
|   +---------------+       +---------------+      | Ring: 2px Inset||
|                                                  +---------------+|
+-------------------------------------------------------------------+
```

### 2.1 Cell Index Mapping & Pointer Projection

Let $P_{\text{world}} = (x_{\text{world}}, y_{\text{world}})$ be the world coordinate derived from viewport point $P_{\text{screen}}$ via camera transform $T_{\text{camera}}^{-1}$. The addressed cell coordinate $(C_x, C_y) \in \mathbb{Z}^2$ is:

$$C_x = \left\lfloor \frac{x_{\text{world}}}{P_{\text{cell}}} \right\rfloor, \quad C_y = \left\lfloor \frac{y_{\text{world}}}{P_{\text{cell}}} \right\rfloor$$

where $P_{\text{cell}} = 220.0\text{ DIPs}$.

### 2.2 Footprint Expansion Math

Let $S(C_x, C_y)$ be the spatial occupation lookup for cell $(C_x, C_y)$. The head footprint bounds $R_{\text{head}} = [X_{\text{origin}}, Y_{\text{origin}}, W_{\text{cells}}, H_{\text{cells}}]$ are evaluated as:

$$R_{\text{head}} = \begin{cases} 
[C_x, C_y, 1, 1] & \text{if } S(C_x, C_y) = \varnothing \\
[X_{\text{item}}, Y_{\text{item}}, W_{\text{item}}, H_{\text{item}}] & \text{if } S(C_x, C_y) = \text{Item}_{\text{id}} 
\end{cases}$$

World-space rectangular bounds $B_{\text{world}} = [x_0, y_0, x_1, y_1]$ for the cursor head:

$$x_0 = X_{\text{origin}} \cdot P_{\text{cell}}, \quad y_0 = Y_{\text{origin}} \cdot P_{\text{cell}}$$
$$x_1 = (X_{\text{origin}} + W_{\text{cells}}) \cdot P_{\text{cell}}, \quad y_1 = (Y_{\text{origin}} + H_{\text{cells}}) \cdot P_{\text{cell}}$$

### 2.3 Head Fill and Inset Ring Specification

The screen-projected head rectangle $B_{\text{screen}} = T_{\text{camera}}(B_{\text{world}})$.
The stroke of the inset ring is defined as $w_{\text{ring}} = 2.0\text{ px}$ on screen space. The inset geometry rectangle $B_{\text{inset}}$ for drawing the 2px stroke is:

$$B_{\text{inset}} = \left[ x_{\text{screen}, 0} + \frac{w_{\text{ring}}}{2}, \, y_{\text{screen}, 0} + \frac{w_{\text{ring}}}{2}, \, x_{\text{screen}, 1} - \frac{w_{\text{ring}}}{2}, \, y_{\text{screen}, 1} - \frac{w_{\text{ring}}}{2} \right]$$

The composite fill opacity $\alpha_{\text{head\_fill}}$ and ring opacity $\alpha_{\text{head\_ring}}$ are:

$$\alpha_{\text{head\_fill}} = \text{clamp}\left( E_{\text{steady}} \times \text{gain}_{\text{fill}}, \, 0.0, \, 1.0 \right) = 0.60 \times 0.22 = 0.132 \quad (13.2\% \text{ nominal, up to } 22.0\% \text{ armed})$$
$$\alpha_{\text{head\_ring}} = 0.88 \quad (\text{color } \#\text{F4F4F2})$$

### 2.4 18-Step Spent-Cell Trail Decay Recurrence Relation

When the cursor transitions from cell $A$ to cell $B$, cell $A$ is registered as a spent trail cell with initial kinetic energy $E_0 = E_{\text{steady}} = 0.60$.
For each rendered frame $t \ge 0$, energy decays exponentially:

$$E_{t+1} = E_t \cdot \gamma_{\text{decay}}$$

where $\gamma_{\text{decay}} = 0.84$ (`--cursor-trail-decay`).
A spent cell is pruned from the render queue on frame $t$ when:

$$E_t < E_{\text{min}}$$

where $E_{\text{min}} = 0.03$ (`--cursor-trail-min`).

#### Exact Frame Lifetime Derivation:
$$E_t = E_0 \cdot (0.84)^t < 0.03 \implies 0.60 \cdot (0.84)^t < 0.03 \implies (0.84)^t < 0.05$$
$$t \cdot \ln(0.84) < \ln(0.05) \implies t \cdot (-0.17435) < -2.9957 \implies t > \frac{2.9957}{0.17435} \approx 17.18 \text{ frames}$$

Thus, at frame $t = 18$, $E_{18} = 0.60 \cdot (0.84)^{18} \approx 0.0264 < 0.03$. The trail cell persists for **exactly 18 frames**.

---

## 3. C# 13 System Architecture & Interface Contracts

```csharp
namespace Grove.SpatialGrid.Cursor;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Avalonia;
using SkiaSharp;

public enum CursorRole : byte
{
    Default = 0,
    ToolPlacement = 1,  // --k-tool (#3B82F6)
    EditTransform = 2,  // --k-edit (#F59E0B)
    LayerTrace    = 3   // --k-layer (#10B981)
}

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public readonly record struct CellCoordinate(int X, int Y)
{
    public static CellCoordinate Zero => new(0, 0);
}

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public readonly record struct FootprintBounds(int OriginX, int OriginY, int WidthCells, int HeightCells)
{
    public bool Contains(CellCoordinate cell) =>
        cell.X >= OriginX && cell.X < OriginX + WidthCells &&
        cell.Y >= OriginY && cell.Y < OriginY + HeightCells;
}

public readonly record struct SpentCellSegment(
    CellCoordinate Cell,
    float KineticEnergy,
    byte FrameAge
);

public sealed record GridCursorState(
    CellCoordinate AddressedCell,
    FootprintBounds ActiveFootprint,
    CursorRole ActiveRole,
    bool IsMiddlePanActive,
    IReadOnlyList<SpentCellSegment> SpentTrail
);

public interface IGridCursorService
{
    GridCursorState CurrentState { get; }
    event Action<GridCursorState>? CursorStateChanged;

    void MoveToCell(CellCoordinate targetCell);
    void SetPointerPosition(Point worldPosition);
    void ArmRole(CursorRole role);
    void DisarmRole();
    void TickRenderFrame();
}
```

---

## 4. Avalonia 11.2.5 / SkiaSharp Rendering Pipeline Integration

### 4.1 Custom Skia Draw Operation

```csharp
namespace Grove.SpatialGrid.Rendering;

using Avalonia.MicroCom;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Grove.SpatialGrid.Cursor;
using SkiaSharp;

public sealed class GridCursorDrawOperation : ICustomDrawOperation
{
    private readonly Rect _bounds;
    private readonly GridCursorState _state;
    private readonly Matrix3x3 _cameraTransform;
    private readonly float _cellPitchDip;

    public GridCursorDrawOperation(
        Rect bounds, 
        GridCursorState state, 
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

    public bool HitTest(Point p) => false; // Cursor is never hit-testable

    public void Render(ImmediateDrawingContext context)
    {
        var skiaContext = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
        if (skiaContext is null) return;

        using var lease = skiaContext.Lease();
        var canvas = lease.SkCanvas;

        canvas.Save();
        canvas.SetMatrix(_cameraTransform);

        // 1. Render Spent Trail Cells
        RenderSpentTrail(canvas, _state.SpentTrail, _cellPitchDip);

        // 2. Render Cursor Head
        RenderCursorHead(canvas, _state, _cellPitchDip);

        canvas.Restore();
    }

    private static void RenderSpentTrail(SKCanvas canvas, IReadOnlyList<SpentCellSegment> trail, float pitch)
    {
        using var fillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            IsAntialias = false
        };

        foreach (var segment in trail)
        {
            float alpha = segment.KineticEnergy * 0.22f; // Fill gain
            fillPaint.Color = SKColor.Parse("#F4F4F2").WithAlpha((byte)(alpha * 255));

            var rect = SKRect.Create(
                segment.Cell.X * pitch, 
                segment.Cell.Y * pitch, 
                pitch, 
                pitch
            );
            canvas.DrawRect(rect, fillPaint);
        }
    }

    private static void RenderCursorHead(SKCanvas canvas, GridCursorState state, float pitch)
    {
        var fp = state.ActiveFootprint;
        var worldRect = SKRect.Create(
            fp.OriginX * pitch,
            fp.OriginY * pitch,
            fp.WidthCells * pitch,
            fp.HeightCells * pitch
        );

        SKColor baseColor = state.ActiveRole switch
        {
            CursorRole.ToolPlacement => SKColor.Parse("#3B82F6"),
            CursorRole.EditTransform => SKColor.Parse("#F59E0B"),
            CursorRole.LayerTrace    => SKColor.Parse("#10B981"),
            _                        => SKColor.Parse("#F4F4F2")
        };

        // Fill: 22% maximum / 13.2% steady
        using var fillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = baseColor.WithAlpha((byte)(0.22f * 255)),
            IsAntialias = false
        };
        canvas.DrawRect(worldRect, fillPaint);

        // Inset 2px Ring
        using var ringPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2.0f / canvas.TotalMatrix.ScaleX, // Keep exactly 2px on screen
            Color = baseColor.WithAlpha((byte)(0.88f * 255)),
            IsAntialias = true
        };

        // Inset stroke adjustment
        float halfStroke = 1.0f / canvas.TotalMatrix.ScaleX;
        var insetRect = new SKRect(
            worldRect.Left + halfStroke,
            worldRect.Top + halfStroke,
            worldRect.Right - halfStroke,
            worldRect.Bottom - halfStroke
        );

        canvas.DrawRect(insetRect, ringPaint);
    }
}
```

---

## 5. Architectural Invariants & Refusal Assertions

1. **Zero Outset Border Violation**: The cursor head ring MUST be drawn inset. Under no circumstances may stroke rendering expand into neighboring cell space.
2. **Strict Non-Hit-Testability**: `HitTest` on the cursor draw operation returns `false` invariant. Pointer events pass through to underlying spatial content or grid canvas.
3. **No Screen-Fixed Pointer Floating**: The cursor head position MUST be strictly derived from grid cell coordinates. Screen-space mouse pointers floating independently above Plane 0 without cell snapping are illegal.
4. **Reduced-Motion Compliance**: When `SystemAnimations.IsEnabled` is false, the spent cell trail is suppressed entirely ($\gamma_{\text{decay}} = 0$). Cells clear instantaneously when the head departs.
