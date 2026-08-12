# ADR-005: Camera Affine Transform Engine

| Property | Value |
| :--- | :--- |
| **Status** | Accepted |
| **Date** | 2026-08-12 |
| **Area** | 2D Camera Engine / Viewport Mathematics |
| **Target Runtime** | C# 13 / .NET 9 / Avalonia 11.2.5 CompositionTarget VSync |
| **Authors** | Chief Product Definition Architect |

---

## 1. Context & Architectural Requirements

The Grove v9 spatial canvas relies on an independent, decoupled 2D Camera Engine responsible for panning and zooming across an infinite 2D plane.

Key architectural requirements:
1. **Decoupled 2D Affine Transformation**: Coordinate transforms must be encapsulated in a single 2D affine matrix $T(x, y, s)$.
2. **Focal-Point Zooming**: Zoom operations must pivot seamlessly around the current mouse cursor location $(x_c, y_c)$.
3. **High Refresh Rate GPU VSync**: Smooth 120Hz-240Hz animation loop execution synchronized with display VSync using `CompositionTarget.Rendering` or Skia frame loops.
4. **Viewport Culling Precision**: Fast extraction of visible world bounds to clip out-of-frame grid rendering.

---

## 2. Mathematical Formalism & Affine Matrix Engine

### 2.1 Forward Transformation Matrix ($T$)
The 2D affine matrix $T$ projects world coordinates $(x_w, y_w)$ to screen coordinates $(x_s, y_s)$ given camera scale $s \in [0.01, 10.0]$ and screen translation offset $(T_x, T_y)$:

$$\begin{bmatrix} x_s \\ y_s \\ 1 \end{bmatrix} = T \begin{bmatrix} x_w \\ y_w \\ 1 \end{bmatrix} = \begin{bmatrix} s & 0 & T_x \\ 0 & s & T_y \\ 0 & 0 & 1 \end{bmatrix} \begin{bmatrix} x_w \\ y_w \\ 1 \end{bmatrix} = \begin{bmatrix} s \cdot x_w + T_x \\ s \cdot y_w + T_y \\ 1 \end{bmatrix}$$

### 2.2 Inverse Transformation Matrix ($T^{-1}$)
The inverse matrix $T^{-1}$ unprojects screen coordinates $(x_s, y_s)$ back to spatial world coordinates $(x_w, y_w)$:

$$\begin{bmatrix} x_w \\ y_w \\ 1 \end{bmatrix} = T^{-1} \begin{bmatrix} x_s \\ y_s \\ 1 \end{bmatrix} = \begin{bmatrix} \frac{1}{s} & 0 & -\frac{T_x}{s} \\ 0 & \frac{1}{s} & -\frac{T_y}{s} \\ 0 & 0 & 1 \end{bmatrix} \begin{bmatrix} x_s \\ y_s \\ 1 \end{bmatrix} = \begin{bmatrix} \frac{x_s - T_x}{s} \\ \frac{y_s - T_y}{s} \\ 1 \end{bmatrix}$$

### 2.3 Focal-Point Zoom Operator
When zooming from scale $s$ to scale $s'$ anchored at screen cursor position $(x_c, y_c)$, the world coordinate under the cursor must remain stationary:

$$\frac{x_c - T_x}{s} = \frac{x_c - T'_x}{s'}$$

Solving for new translation offsets $(T'_x, T'_y)$:

$$T'_x = x_c - \frac{s'}{s} (x_c - T_x)$$

$$T'_y = y_c - \frac{s'}{s} (y_c - T_y)$$

---

## 3. Viewport Culling Math

Given screen viewport size $W \times H$ (screen rectangle $R_s = [0, 0, W, H]$):

$$\text{World Bounds } R_w = \left[ \frac{-T_x}{s}, \; \frac{-T_y}{s}, \; \frac{W - T_x}{s}, \; \frac{H - T_y}{s} \right]$$

### Visible Cell Index Bounds
For major cell pitch $P_{\text{major}} = 220.0\text{px}$:

$$X_{\text{cell, min}} = \left\lfloor \frac{-T_x}{s \cdot P_{\text{major}}} \right\rfloor, \quad X_{\text{cell, max}} = \left\lceil \frac{W - T_x}{s \cdot P_{\text{major}}} \right\rceil$$

$$Y_{\text{cell, min}} = \left\lfloor \frac{-T_y}{s \cdot P_{\text{major}}} \right\rfloor, \quad Y_{\text{cell, max}} = \left\lceil \frac{H - T_y}{s \cdot P_{\text{major}}} \right\rceil$$

---

## 4. 120Hz-240Hz VSync Synchronization Loop

To achieve liquid smooth rendering on modern 120Hz-240Hz displays, camera translation and zoom spring dampening are updated on every GPU frame callback via Avalonia `CompositionTarget.Rendering`.

```text
Display VSync Pulse (8.33ms for 120Hz / 4.16ms for 240Hz)
  ↓
CompositionTarget.Rendering Callback
  ↓
Update Dampened Inertia Physics:
  Position = Vector2.Lerp(Position, TargetPosition, DampeningFactor)
  Scale = MathF.Lerp(Scale, TargetScale, DampeningFactor)
  ↓
Recalculate Matrix T and Inverse T^{-1}
  ↓
Invalidate Plane 0 Skia Canvas Render Pass
```

---

## 5. C# Type Definitions & Engine Implementation

```csharp
namespace Grove.SpatialGrid.Camera;

using System;
using System.Numerics;
using Avalonia;
using SkiaSharp;

/// <summary>
/// Immutable snapshot of current camera transform state.
/// </summary>
public readonly record struct CameraState
{
    public Vector2 Translation { get; init; }
    public float Scale { get; init; }
    public Matrix3x2 TransformMatrix { get; init; }
    public Matrix3x2 InverseMatrix { get; init; }

    public CameraState(Vector2 translation, float scale)
    {
        Translation = translation;
        Scale = Math.Clamp(scale, 0.01f, 10.0f);

        TransformMatrix = new Matrix3x2(
            Scale, 0.0f,
            0.0f, Scale,
            Translation.X, Translation.Y);

        Matrix3x2.Invert(TransformMatrix, out Matrix3x2 inv);
        InverseMatrix = inv;
    }
}

public interface ICameraEngine
{
    CameraState CurrentState { get; }
    void PanBy(Vector2 deltaScreen);
    void ZoomAt(Point cursorScreen, float zoomDeltaFactor);
    Point WorldToScreen(Point worldPoint);
    Point ScreenToWorld(Point screenPoint);
    SKRect GetVisibleWorldBounds(Size viewportSize);
}

/// <summary>
/// High-performance decoupled 2D camera affine transform engine.
/// </summary>
public sealed class CameraTransformEngine : ICameraEngine
{
    private Vector2 _translation = Vector2.Zero;
    private float _scale = 1.0f; // 100%

    public CameraState CurrentState => new(_translation, _scale);

    public void PanBy(Vector2 deltaScreen)
    {
        _translation += deltaScreen;
    }

    public void ZoomAt(Point cursorScreen, float zoomDeltaFactor)
    {
        float oldScale = _scale;
        float newScale = Math.Clamp(oldScale * zoomDeltaFactor, 0.01f, 10.0f);

        if (MathF.Abs(newScale - oldScale) < 0.00001f) return;

        // Focal-point zoom algorithm around cursor:
        // T'_x = x_c - (s' / s) * (x_c - T_x)
        // T'_y = y_c - (s' / s) * (y_c - T_y)
        float scaleRatio = newScale / oldScale;
        float newTx = (float)cursorScreen.X - scaleRatio * ((float)cursorScreen.X - _translation.X);
        float newTy = (float)cursorScreen.Y - scaleRatio * ((float)cursorScreen.Y - _translation.Y);

        _scale = newScale;
        _translation = new Vector2(newTx, newTy);
    }

    public Point WorldToScreen(Point worldPoint)
    {
        float sx = _scale * (float)worldPoint.X + _translation.X;
        float sy = _scale * (float)worldPoint.Y + _translation.Y;
        return new Point(sx, sy);
    }

    public Point ScreenToWorld(Point screenPoint)
    {
        float wx = ((float)screenPoint.X - _translation.X) / _scale;
        float wy = ((float)screenPoint.Y - _translation.Y) / _scale;
        return new Point(wx, wy);
    }

    public SKRect GetVisibleWorldBounds(Size viewportSize)
    {
        float minX = -_translation.X / _scale;
        float minY = -_translation.Y / _scale;
        float maxX = ((float)viewportSize.Width - _translation.X) / _scale;
        float maxY = ((float)viewportSize.Height - _translation.Y) / _scale;

        return new SKRect(minX, minY, maxX, maxY);
    }

    public SKMatrix ToSkiaMatrix()
    {
        var skMat = SKMatrix.CreateIdentity();
        skMat.ScaleX = _scale;
        skMat.ScaleY = _scale;
        skMat.TransX = _translation.X;
        skMat.TransY = _translation.Y;
        return skMat;
    }
}
```
