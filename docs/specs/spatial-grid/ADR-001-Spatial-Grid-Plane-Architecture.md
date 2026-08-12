---
status: "IMPLEMENTED - AWAITING USER REVIEW"
---

# ADR-001: Spatial Grid Plane Architecture

| Property | Value |
| :--- | :--- |
| **Status** | IMPLEMENTED - AWAITING USER REVIEW |
| **Date** | 2026-08-12 |
| **Area** | Spatial Grid Engine / Skia Rendering Pipeline |
| **Target Runtime** | C# 13 / .NET 9 / Avalonia 11.2.5 / SkiaSharp |
| **Authors** | Chief Product Definition Architect |

---

## 1. Context & Architectural Drivers

Grove v9 requires a high-performance continuous 2D spatial grid plane acting as Plane 0 (the lowest layer in the three-plane visual stack). The spatial grid provides absolute coordinate anchor points for placed content (`Note`, `Document`, `Picture`) and aura fields. 

Key architectural requirements:
1. **Infinite Spatial Geometry**: Strict cell-based spatial discretization with no origin highlights, arbitrary coordinate boundaries, or snapping anomalies.
2. **Multi-Scale Viewport Rendering**: Smooth scaling across a zoom range of $1\%$ ($0.01\times$) to $1000\%$ ($10.0\times$).
3. **Continuous 3-Tier Line Fading**: Grid lines must fade continuously based on screen-projected line pitch without visual popping, discrete zoom steps, or snap transitions.
4. **Skia Execution Efficiency**: Sub-millisecond GPU draw execution using SkiaSharp within Avalonia 11.2.5 `CustomDrawOperation` primitives.

---

## 2. Spatial Geometry Metrics & Tokens

The grid geometry consists of three nested spatial hierarchy tiers:

| Tier | World Pitch ($P_{\text{world}}$) | Subdivisions / Multiplier | Nominal Ink Token | Nominal Hex / Value |
| :--- | :--- | :--- | :--- | :--- |
| **Minor** | $44.0\text{ px}$ | $P_{\text{major}} / 5$ | `--grid-minor-ink` | `#161618` (`--c-grid-min`) |
| **Major** | $220.0\text{ px}$ | $1.0\text{ cell}$ | `--grid-major-ink` | `#242428` (`--c-grid-maj`) |
| **Supercell** | $1100.0\text{ px}$ | $5.0\text{ cells}$ | `--grid-major-ink` | `#242428` (`--c-grid-maj`) |

### Core Mathematical Constraints
1. Major Cell Pitch: $P_{\text{major}} = 220.0\text{ DIPs}$.
2. Minor Subdivision Pitch: $P_{\text{minor}} = \frac{P_{\text{major}}}{5} = 44.0\text{ DIPs}$.
3. Supercell Pitch: $P_{\text{super}} = 5 \times P_{\text{major}} = 1100.0\text{ DIPs}$.
4. Line Weight: Fixed at exactly $1.0\text{ DIP}$ across all zoom scales $s \in [0.01, 10.0]$. Lines never thicken or shrink with camera transform.

---

## 3. Continuous 3-Tier Grid Line Fading Mechanics

### 3.1 Screen Spacing Formula
For any grid tier with world pitch $P_{\text{world}}$, the projected screen spacing $S_{\text{screen}}$ under zoom scale $s$ is:

$$S_{\text{screen}} = P_{\text{world}} \cdot s$$

### 3.2 Ink Transparency Equation
The continuous ink opacity coefficient $\text{ink} \in [0.0, 1.0]$ is governed by the universal fade curve:

$$\text{ink}(S_{\text{screen}}) = \text{clamp}\left( \frac{S_{\text{screen}} - S_{\text{start}}}{S_{\text{end}} - S_{\text{start}}}, 0.0, 1.0 \right) = \text{clamp}\left( \frac{S_{\text{screen}} - 6}{8}, 0.0, 1.0 \right)$$

Where:
- $S_{\text{start}} = 6.0\text{ px}$ (`--grid-fade-start`): Threshold below which grid lines are completely invisible ($\text{ink} = 0.0$).
- $S_{\text{end}} = 14.0\text{ px}$ (`--grid-fade-end`): Threshold at or above which grid lines render at $100\%$ nominal alpha ($\text{ink} = 1.0$).

### 3.3 Composite Tier Alpha Calculation
The final RGBA color for rendering tier $T \in \{\text{Minor}, \text{Major}, \text{Supercell}\}$ is:

$$\alpha_{\text{final}, T} = \text{BaseAlpha}_T \times \text{ink}(P_{\text{world}, T} \cdot s) \times V_{\text{pref}}$$

Where $V_{\text{pref}} \in \{0.0, 1.0\}$ represents the user view preference (`Show lines` / `Hide lines`).

### 3.4 Zoom Boundary Transition Table

| Camera Zoom ($s$) | Minor Spacing ($S_{\text{minor}}$) | Minor Ink ($\text{ink}_{\text{minor}}$) | Major Spacing ($S_{\text{major}}$) | Major Ink ($\text{ink}_{\text{major}}$) | Supercell Spacing ($S_{\text{super}}$) | Supercell Ink ($\text{ink}_{\text{super}}$) |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **1000.0%** ($10.0$) | $440.0\text{px}$ | **1.000** | $2200.0\text{px}$ | **1.000** | $11000.0\text{px}$ | **1.000** |
| **100.0%** ($1.0$) | $44.0\text{px}$ | **1.000** | $220.0\text{px}$ | **1.000** | $1100.0\text{px}$ | **1.000** |
| **31.82%** ($0.3182$) | $14.0\text{px}$ | **1.000** | $70.0\text{px}$ | **1.000** | $350.0\text{px}$ | **1.000** |
| **25.00%** ($0.2500$) | $11.0\text{px}$ | **0.625** | $55.0\text{px}$ | **1.000** | $275.0\text{px}$ | **1.000** |
| **20.00%** ($0.2000$) | $8.8\text{px}$ | **0.350** | $44.0\text{px}$ | **1.000** | $220.0\text{px}$ | **1.000** |
| **13.64%** ($0.1364$) | $6.0\text{px}$ | **0.000** | $30.0\text{px}$ | **1.000** | $150.0\text{px}$ | **1.000** |
| **6.36%** ($0.0636$) | $2.8\text{px}$ | **0.000** | $14.0\text{px}$ | **1.000** | $70.0\text{px}$ | **1.000** |
| **4.00%** ($0.0400$) | $1.8\text{px}$ | **0.000** | $8.8\text{px}$ | **0.350** | $44.0\text{px}$ | **1.000** |
| **2.73%** ($0.0273$) | $1.2\text{px}$ | **0.000** | $6.0\text{px}$ | **0.000** | $30.0\text{px}$ | **1.000** |
| **1.27%** ($0.0127$) | $0.6\text{px}$ | **0.000** | $2.8\text{px}$ | **0.000** | $14.0\text{px}$ | **1.000** |
| **1.00%** ($0.0100$) | $0.4\text{px}$ | **0.000** | $2.2\text{px}$ | **0.000** | $11.0\text{px}$ | **0.625** |

---

## 4. Skia Rendering Pipeline Specification

### 4.1 Coordinate Space & Viewport Culling
Given screen viewport bounds $V = [0, 0, W_{\text{screen}}, H_{\text{screen}}]$ and camera affine matrix $T$:

$$\begin{bmatrix} x_{\text{world}} \\ y_{\text{world}} \\ 1 \end{bmatrix} = T^{-1} \begin{bmatrix} x_{\text{screen}} \\ y_{\text{screen}} \\ 1 \end{bmatrix}$$

1. Compute world viewport bounding rectangle $R_{\text{world}} = [x_{\text{min}}, y_{\text{min}}, x_{\text{max}}, y_{\text{max}}]$.
2. Determine start/end line indices for tier pitch $P$:
   $$i_{\text{start}} = \left\lfloor \frac{x_{\text{min}}}{P} \right\rfloor, \quad i_{\text{end}} = \left\lceil \frac{x_{\text{max}}}{P} \right\rceil$$
   $$j_{\text{start}} = \left\lfloor \frac{y_{\text{min}}}{P} \right\rfloor, \quad j_{\text{end}} = \left\lceil \frac{y_{\text{max}}}{P} \right\rceil$$

### 4.2 Pixel Snapping & Hairline Rendering
To prevent sub-pixel antialiasing blur, drawn screen lines must be snapped to exact physical pixel boundaries when rendering on GPU:

$$x_{\text{snap}} = \frac{\lfloor x_{\text{screen}} \cdot \text{DPI} \rceil}{\text{DPI}}$$

- Skia `SKPaint` configuration:
  - `IsAntialias = false` for axis-aligned grid hairlines.
  - `Style = SKPaintStyle.Stroke`.
  - `StrokeWidth = 1.0f`.

---

## 5. C# Type Definitions & Contracts

```csharp
namespace Grove.SpatialGrid.Architecture;

using System;
using SkiaSharp;

/// <summary>
/// Immutable configuration defining spatial grid geometry metrics and fading constants.
/// </summary>
public readonly record struct SpatialGridGeometryConfig
{
    public const float MajorCellPitchWorld = 220.0f;
    public const float MinorSubdivisionPitchWorld = 44.0f;
    public const float SupercellPitchWorld = 1100.0f;
    public const int SubdivisionsPerCell = 5;
    public const int CellsPerSupercell = 5;

    public const float FadeStartScreenPx = 6.0f;
    public const float FadeEndScreenPx = 14.0f;
    public const float MinZoomScale = 0.01f;   // 1%
    public const float MaxZoomScale = 10.0f;   // 1000%

    public static readonly SKColor MinorLineBaseColor = SKColor.Parse("#161618");
    public static readonly SKColor MajorLineBaseColor = SKColor.Parse("#242428");

    /// <summary>
    /// Calculates the continuous ink transparency coefficient for a given world pitch and zoom scale.
    /// Formula: ink = clamp((spacing - 6) / 8, 0, 1)
    /// </summary>
    public static float CalculateInk(float worldPitch, float zoomScale)
    {
        float spacing = worldPitch * zoomScale;
        float ink = (spacing - FadeStartScreenPx) / (FadeEndScreenPx - FadeStartScreenPx);
        return Math.Clamp(ink, 0.0f, 1.0f);
    }
}

/// <summary>
/// Interface for low-level Skia spatial grid rendering on Plane 0.
/// </summary>
public interface ISpatialGridRenderer
{
    void RenderGrid(
        SKCanvas canvas, 
        SKRect viewportScreen, 
        SKMatrix cameraMatrix, 
        bool linesVisible);
}

/// <summary>
/// High-performance Skia grid renderer utilizing batched path rendering.
/// </summary>
public sealed class SkiaSpatialGridRenderer : ISpatialGridRenderer
{
    private readonly SKPaint _minorPaint = new()
    {
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 1.0f,
        IsAntialias = false,
        Color = SpatialGridGeometryConfig.MinorLineBaseColor
    };

    private readonly SKPaint _majorPaint = new()
    {
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 1.0f,
        IsAntialias = false,
        Color = SpatialGridGeometryConfig.MajorLineBaseColor
    };

    public void RenderGrid(
        SKCanvas canvas, 
        SKRect viewportScreen, 
        SKMatrix cameraMatrix, 
        bool linesVisible)
    {
        if (!linesVisible) return;

        float zoomScale = cameraMatrix.ScaleX;
        float minorInk = SpatialGridGeometryConfig.CalculateInk(SpatialGridGeometryConfig.MinorSubdivisionPitchWorld, zoomScale);
        float majorInk = SpatialGridGeometryConfig.CalculateInk(SpatialGridGeometryConfig.MajorCellPitchWorld, zoomScale);
        float superInk = SpatialGridGeometryConfig.CalculateInk(SpatialGridGeometryConfig.SupercellPitchWorld, zoomScale);

        if (superInk <= 0.0f) return;

        if (!cameraMatrix.TryInvert(out SKMatrix inverseMatrix)) return;

        SKRect worldBounds = inverseMatrix.MapRect(viewportScreen);

        canvas.Save();
        canvas.SetMatrix(cameraMatrix);

        // Render Minor Grid Tier
        if (minorInk > 0.0f)
        {
            _minorPaint.Color = SpatialGridGeometryConfig.MinorLineBaseColor.WithAlpha((byte)(minorInk * 255));
            DrawGridTier(canvas, worldBounds, SpatialGridGeometryConfig.MinorSubdivisionPitchWorld, _minorPaint);
        }

        // Render Major Grid Tier
        if (majorInk > 0.0f)
        {
            _majorPaint.Color = SpatialGridGeometryConfig.MajorLineBaseColor.WithAlpha((byte)(majorInk * 255));
            DrawGridTier(canvas, worldBounds, SpatialGridGeometryConfig.MajorCellPitchWorld, _majorPaint);
        }

        // Render Supercell Tier
        if (superInk > 0.0f)
        {
            _majorPaint.Color = SpatialGridGeometryConfig.MajorLineBaseColor.WithAlpha((byte)(superInk * 255));
            DrawGridTier(canvas, worldBounds, SpatialGridGeometryConfig.SupercellPitchWorld, _majorPaint);
        }

        canvas.Restore();
    }

    private static void DrawGridTier(SKCanvas canvas, SKRect worldBounds, float pitch, SKPaint paint)
    {
        using var path = new SKPath();
        
        float startX = MathF.Floor(worldBounds.Left / pitch) * pitch;
        float endX = MathF.Ceiling(worldBounds.Right / pitch) * pitch;

        for (float x = startX; x <= endX; x += pitch)
        {
            path.MoveTo(x, worldBounds.Top);
            path.LineTo(x, worldBounds.Bottom);
        }

        float startY = MathF.Floor(worldBounds.Top / pitch) * pitch;
        float endY = MathF.Ceiling(worldBounds.Bottom / pitch) * pitch;

        for (float y = startY; y <= endY; y += pitch)
        {
            path.MoveTo(worldBounds.Left, y);
            path.LineTo(worldBounds.Right, y);
        }

        canvas.DrawPath(path, paint);
    }
}
```

---

## 6. Verification & Conformance Rules

1. **Continuous Line Fade Verification**: Assert that no tier pops or changes opacity discretely across camera scale delta $\Delta s = 0.0001$.
2. **Visual Budget Limit**: On Plane 0 at rest, only placed content, grid lines, presence aura heatmaps, and the grid cursor are permitted to draw.
3. **Line Snap Independence**: Toggling grid line visibility (`linesVisible = false`) MUST NOT alter coordinate snapping, content footprints, cursor geometry, or placement collision math.
4. **Canvas Ground & View Preference Contract**: The canonical spatial canvas background is fixed at `#0E0E10` (`--canvas`). The grid line visibility toggle (`Key.G`) is persisted per person (not per place). Toggling lines off removes grid lines only—snapping, placement previews, selection outlines, whole-cell cursor addressing, and presence fields remain pixel-identical.
5. **Architectural Grid Refusals**:
   - **Refused Single Uniform Line Tier**: Single fixed-ink line rendering without per-tier legibility fade is forbidden; un-faded fine lines turn distant viewports into moiré texture.
   - **Refused Container-per-Item Packaging**: Frames, borders, and drop shadows wrapped around content items are refused; lines orient spatial context, they do not package items as card layouts.
   - **Refused Parked Chrome**: Persistent toolbars anchored over Plane 0 are forbidden; tools arrive on approach and depart when work is complete.
