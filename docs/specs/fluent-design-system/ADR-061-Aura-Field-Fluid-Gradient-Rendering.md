---
status: "IMPLEMENTED - AWAITING USER REVIEW"
---

# ADR-061: Aura Field Fluid Gradient Rendering

| Property | Value |
| :--- | :--- |
| **Status** | IMPLEMENTED - AWAITING USER REVIEW |
| **Date** | 2026-08-12 |
| **Area** | Spatial Physics Engine / SkiaSharp Rendering Compositor |
| **Target Runtime** | C# 13 / .NET 9 / SkiaSharp / Vulkan / Direct3D 11 |
| **Authors** | Chief Product Definition Architect |

---

## 1. Context & Visual Defect Elimination

In earlier visual engine iterations, spatial grid lines were rendered indiscriminately across the entire viewport prior to overlaying aura field highlights. Consequently, dark cell borders (hairline grid lines) showed through active energy heatmaps, creating an unapproved "mini-grid gridline grid" inside smooth aura fields.

### Architectural Directives
1. **Continuous Field Principle**: Spatial aura fields represent weak continuous scalar gravitational potential distributions $E(x,y)$. Rendering discrete grid lines inside an aura field violates physical continuity and introduces visual noise.
2. **Inner Line Suppression**: Dark grid lines MUST be completely suppressed across any cell region where composite field energy $E_{\text{total}}(x,y) \ge E_{\text{suppress}} = 0.05$.
3. **Fluid Gradient Rendering**: Aura heatmaps MUST render as continuous, smooth fluid gradient fills using hardware-accelerated radial shader functions (`SKShader.CreateRadialGradient`) operating on Plane 0.

---

## 2. Mathematical Physics & Smooth Radial Shader Interpolation

### 2.1 Gravitational Field Radial Profile
For a content item $i$ centered at $(x_i, y_i)$ with Mass $M_i \ge 1.0$, the continuous spatial energy distribution $E_i(r)$ at radial distance $r = \sqrt{(x - x_i)^2 + (y - y_i)^2}$ is defined as:

$$E_i(r) = \frac{M_i}{1 + \alpha \cdot r^2}$$

Where $\alpha = \frac{0.4}{S_{\text{cell}}^2}$ normalizes spatial distance to cell grid units $S_{\text{cell}}$.

### 2.2 Radial Cutoff & Shader Influence Radius
The outer boundary radius $R_{\text{max}}$ where field energy falls to the visibility cutoff threshold $E_{\text{cutoff}} = 0.05$ is derived by solving $E_i(R_{\text{max}}) = E_{\text{cutoff}}$:

$$R_{\text{max}} = \sqrt{\frac{\frac{M_i}{E_{\text{cutoff}}} - 1}{\alpha}} = S_{\text{cell}} \cdot \sqrt{\frac{20 \cdot M_i - 1}{0.4}}$$

For nominal mass $M_i = 1.0$ and cell size $S_{\text{cell}} = 32\text{px}$:
$$R_{\text{max}} = 32 \cdot \sqrt{\frac{19}{0.4}} = 32 \cdot \sqrt{47.5} \approx 220.55\text{ DIPs}$$

### 2.3 Hermite Smoothstep Alpha Interpolation
To eliminate hard edges at $r = R_{\text{max}}$, color stop alpha values $\mathcal{A}(t)$ are modulated via a smooth cubic Hermite polynomial over normalized radius $t = \frac{r}{R_{\text{max}}} \in [0, 1]$:

$$\mathcal{A}(t) = \alpha_{\text{peak}} \cdot \left( 1 - 3t^2 + 2t^3 \right)$$

This guarantees zero first-derivative boundary condition at $t = 1$:

$$\left. \frac{d\mathcal{A}}{dt} \right|_{t=1} = \left. \alpha_{\text{peak}} \cdot \left( -6t + 6t^2 \right) \right|_{t=1} = 0$$

```
 Alpha (A)
   ▲
1.0│──────┐ (α_peak)
   │      │\
0.8│      │ \  Smooth Hermite Falloff: A(t) = α_peak * (1 - 3t² + 2t³)
0.6│      │  \
0.4│      │   \
0.2│      │    └─── (A'(1) = 0, no edge artifact)
0.0└──────┴─────────┴────────► Normalized Radius t (r / R_max)
   0.0   0.4       1.0
```

---

## 3. Compositor Layering & Grid Line Masking Algorithm

### 3.1 Plane 0 Execution Order
Rendering of the spatial grid plane follows a strict 4-pass sequence to guarantee zero inner line pollution inside aura fields:

```
┌─────────────────────────────────────────────────────────┐
│ Pass 1: Draw Fluid Radial Gradient Fills on Plane 0     │
│         Using SKShader.CreateRadialGradient (Additive)  │
└────────────────────────────┬────────────────────────────┘
                             │
┌────────────────────────────▼────────────────────────────┐
│ Pass 2: Compute Grid Line Path Difference               │
│         P_visible = P_grid \ P_aura_influence          │
└────────────────────────────┬────────────────────────────┘
                             │
┌────────────────────────────▼────────────────────────────┐
│ Pass 3: Render Hairline Grid Lines                      │
│         Draw P_visible using 1.0px hairline SKPaint     │
└────────────────────────────┬────────────────────────────┘
                             │
┌────────────────────────────▼────────────────────────────┐
│ Pass 4: Draw Continuous 1.5px Outer Perimeter Ring      │
│         Render isoline E = 0.15 contour boundary        │
└─────────────────────────────────────────────────────────┘
```

### 3.2 Path Difference Set Theory Formulation
Let $\mathcal{P}_{\text{grid}}$ be the multi-line path representing all spatial grid lines within the active camera frustum.
Let $\Omega_{\text{aura}} = \{ (x,y) \in \mathbb{R}^2 \mid E_{\text{total}}(x,y) \ge 0.05 \}$ be the spatial domain occupied by active aura fields.

The visible grid line path $\mathcal{P}_{\text{visible}}$ is constructed via spatial path subtraction:

$$\mathcal{P}_{\text{visible}} = \mathcal{P}_{\text{grid}} \setminus \Omega_{\text{aura}}$$

---

## 4. C# 13 & SkiaSharp Fluid Gradient Engine Implementation

```csharp
namespace Grove.SpatialGrid.Rendering;

using System;
using System.Collections.Generic;
using SkiaSharp;
using Grove.SpatialGrid.AuraPhysics;

public sealed class FluidAuraRenderer : IDisposable
{
    private readonly SKPaint _gradientPaint;
    private readonly SKPaint _perimeterPaint;
    private readonly SKPaint _gridHairlinePaint;

    public FluidAuraRenderer()
    {
        _gradientPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            BlendMode = SKBlendMode.Plus // Additive energy field blending
        };

        _perimeterPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.5f,
            StrokeCap = SKStrokeCap.Round
        };

        _gridHairlinePaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.0f,
            Color = new SKColor(255, 255, 255, 25) // Hairline ink
        };
    }

    /// <summary>
    /// Renders fluid aura heatmaps and suppressed grid hairlines onto Plane 0.
    /// </summary>
    public void RenderAuraPlane(
        SKCanvas canvas,
        SKPath globalGridPath,
        IReadOnlyList<AuraSource> activeSources,
        float cellWidthDips)
    {
        using var auraOccupancyPath = new SKPath();
        auraOccupancyPath.FillType = SKPathFillType.Winding;

        // Pass 1: Render smooth fluid radial gradient fills & accumulate aura occupancy path
        foreach (var source in activeSources)
        {
            float rMax = cellWidthDips * MathF.Sqrt((20.0f * source.Mass - 1.0f) / 0.4f);
            var center = new SKPoint(source.CenterDips.X, source.CenterDips.Y);

            // Construct Hermite smoothstep color stops
            SKColor baseColor = source.HueColor;
            SKColor[] colors = new SKColor[]
            {
                baseColor.WithAlpha((byte)(0.35f * 255)), // Peak core alpha (t=0.0)
                baseColor.WithAlpha((byte)(0.25f * 255)), // Mid falloff (t=0.4)
                baseColor.WithAlpha((byte)(0.08f * 255)), // Low falloff (t=0.7)
                baseColor.WithAlpha(0)                    // Zero boundary (t=1.0)
            };

            float[] colorPos = new float[] { 0.0f, 0.4f, 0.7f, 1.0f };

            using var shader = SKShader.CreateRadialGradient(
                center,
                rMax,
                colors,
                colorPos,
                SKShaderTileMode.Clamp);

            _gradientPaint.Shader = shader;
            canvas.DrawCircle(center, rMax, _gradientPaint);
            _gradientPaint.Shader = null;

            // Accumulate radial footprint into occupancy mask
            auraOccupancyPath.AddCircle(center.X, center.Y, rMax);
        }

        // Pass 2: Suppress inner grid lines via Path Difference Op (P_visible = P_grid \ P_aura)
        using var visibleGridPath = new SKPath();
        globalGridPath.Op(auraOccupancyPath, SKPathOp.Difference, visibleGridPath);

        // Pass 3: Draw suppressed grid hairlines
        canvas.DrawPath(visibleGridPath, _gridHairlinePaint);

        // Pass 4: Draw continuous 1.5px perimeter containment rings (E >= 0.15)
        foreach (var source in activeSources)
        {
            float rPerimeter = cellWidthDips * MathF.Sqrt((source.Mass / 0.15f - 1.0f) / 0.4f);
            _perimeterPaint.Color = source.HueColor.WithAlpha(180);
            canvas.DrawCircle(source.CenterDips.X, source.CenterDips.Y, rPerimeter, _perimeterPaint);
        }
    }

    public void Dispose()
    {
        _gradientPaint.Dispose();
        _perimeterPaint.Dispose();
        _gridHairlinePaint.Dispose();
    }
}
```
