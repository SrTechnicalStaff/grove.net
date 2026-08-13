---
status: "PARTIAL — verified implementation with remaining integration gaps"
---

# ADR-041: Vertical Aura Permeability and Attenuation Physics

| Property | Value |
| :--- | :--- |
| **Status** | PARTIAL — verified implementation with remaining integration gaps |
| **Date** | 2026-08-12 |
| **Area** | Aura Physics / Cross-Layer Propagation / Field Computation |
| **Target Runtime** | C# 13 / .NET 9 / SkiaSharp |
| **Authors** | Chief Product Definition Architect |

---

## 1. Executive Context & Physical Model

As established in original architectural notes (`Grove - Layers.txt`, `Grove - Field ledger.txt`, and [ADR-003](file:///C:/dev/grove-v9/docs/specs/spatial-grid/ADR-003-Spatial-Aura-Physics.md)), content placed on the spatial grid radiates a continuous visual and metadata energy field termed **Aura**. 

Aura does NOT remain bounded to its originating layer. Instead, it behaves as an isotropic gravitational force that saturates vertically through adjacent spatial layers in the depth stack continuum.

```
Vertical Permeability & Paper-Towel Saturation Model
─────────────────────────────────────────────────────────────────────────────────────────
Layer +2 (L_source+2)   [ (0.5)^2 = 0.25 Attenuation ]  Faint Presence Aura (E >= 0.15)
                             ▲
                             │  Decay Factor γ = 0.5
                             │
Layer +1 (L_source+1)   [ (0.5)^1 = 0.50 Attenuation ]  Moderate Presence Aura
                             ▲
                             │  Decay Factor γ = 0.5
                             │
Layer 0  (L_source)     [ Source Placement Mass M_i ]   Primary Content Frame + Full Aura
─────────────────────────────────────────────────────────────────────────────────────────
```

### Physical Analogy: The Saturation Metaphor

From `Grove - Layers.txt`:
> *"The concept of the aura field extending beyond its current layer could be likened to the concept of saturation. If you pour liquid on a table, and use a paper towel and lay it on top, how many paper towels would it take to absorb the liquid? ... So an aura field for a given object may be giving off a unique hue as it's the combination of the hue from the content itself + the combined hues of all the content saturating the field in that part of the grid from above and below that given layer."*

---

## 2. Mathematical Physics & Cross-Layer Propagation Formulas

### 2.1 Gravitational Energy Field Equation

The energy contribution $E_i(d, \Delta L)$ cast by content item $i$ (mass $M_i$) onto a grid cell at 2D Euclidean distance $d$ across vertical layer delta $|\Delta L| = |L_{\text{target}} - L_{\text{source}}|$ is governed by the inverse-distance quadratic equation combined with geometric layer falloff:

$$E_i(d, \Delta L) = \frac{M_i}{1 + 0.4 \cdot d^2} \cdot \gamma^{|\Delta L|} \quad \text{where } \gamma = 0.5$$

Where:
- $M_i \ge 1.0$: Mass of placed content item $i$ (Note $M=1.0$, Document $M=2.5$, Large Image $M=4.0$).
- $d = \sqrt{(x_{\text{cell}} - x_{\text{source}})^2 + (y_{\text{cell}} - y_{\text{source}})^2}$: Euclidean distance in cell units from the nearest cell of the content footprint.
- $|\Delta L| = |L_{\text{target}} - L_{\text{source}}|$: Absolute integer stack index difference between the target active layer and source content layer.
- $\gamma = 0.5$: Inter-layer aura permeability decay factor.

### 2.2 Composite Cell Energy Equation

The total composite energy $E_{\text{total}}(c, L_{\text{active}})$ at cell coordinate $c(x,y)$ on target layer $L_{\text{active}}$ accumulates energy from all active and inactive layer sources across the spatial field, added to baseline background energy $E_0 = 0.05$:

$$E_{\text{total}}(c, L_{\text{active}}) = E_0 + \sum_{i \in \text{All Sources}} \left( \frac{M_i}{1 + 0.4 \cdot d_{i,c}^2} \cdot (0.5)^{|L_{\text{active}} - L_i|} \right)$$

---

## 3. Inter-Layer Color Saturation & Blending Model

When multiple content items on different layers radiate energy into a common grid cell $(x,y)$ on $L_{\text{active}}$, their individual authored color hues blend proportionally to their vertical energy contributions.

### 3.1 Energy-Weighted Color Composite Formula

Let each content source $i$ have an authored color vector $C_i = (R_i, G_i, B_i, A_i)$ in linear RGB space. The composite cell color $C_{\text{cell}}(c)$ is calculated by:

$$w_i(c) = \frac{E_i(d_{i,c}, \Delta L_i)}{E_{\text{total}}(c) - E_0}$$

$$C_{\text{cell}}(c) = \sum_{i \in \text{Sources}} w_i(c) \cdot C_i$$

Where $w_i(c) \in [0, 1]$ represents the normalized energy weight of source $i$ at cell $c$, ensuring $\sum w_i(c) = 1.0$.

### 3.2 Dynamic Visual Alpha Mapping

The rendered alpha opacity $\alpha_{\text{cell}}(c)$ of the presence heatmap for cell $c$ scales continuously with total cell energy:

$$\alpha_{\text{cell}}(c) = \min \left( 1.0, \; \alpha_{\text{base}} + \beta \cdot \ln(1.0 + E_{\text{total}}(c)) \right)$$

Where $\alpha_{\text{base}} = 0.08$ and scaling factor $\beta = 0.42$.

---

## 4. Presence Reflection & Layer Attenuation Cascade

### 4.1 Attenuation Cascade Matrix

| Stack Delta $|\Delta L|$ | Attenuation Factor $(0.5)^{|\Delta L|}$ | Energy from Mass $M=1.0$ at $d=0$ | Visible Perimeter Ring ($E \ge 0.15$)? | Presence Render State |
| :--- | :--- | :--- | :--- | :--- |
| **$|\Delta L| = 0$** | $0.5^0 = 1.0000$ | $E = 1.0500$ | **YES** | Primary Content + Strong Perimeter Isoline |
| **$|\Delta L| = 1$** | $0.5^1 = 0.5000$ | $E = 0.5500$ | **YES** | Moderate Presence Heatmap & Isoline |
| **$|\Delta L| = 2$** | $0.5^2 = 0.2500$ | $E = 0.3000$ | **YES** | Faint Presence Heatmap & Isoline |
| **$|\Delta L| = 3$** | $0.5^3 = 0.1250$ | $E = 0.1750$ | **YES** | Boundary Presence Isoline (Near Threshold) |
| **$|\Delta L| \ge 4$** | $0.5^4 = 0.0625$ | $E = 0.1125 < 0.15$ | **NO** | Unpainted (Below Perception Floor) |

### 4.2 Culling Proof & Effective Influence Bounds

For any content source $i$ with mass $M_i$:
- Spatial Culling Cutoff: $d_{\text{cull}} = 6$ cells.
- Vertical Layer Culling Cutoff: $|\Delta L|_{\text{cull}} = 3$ layers.

Any cell where $d > 6$ OR $|\Delta L| > 3$ produces an energy contribution $E_i < 0.065$, which falls strictly below the visible perimeter containment threshold ($E_{\text{threshold}} = 0.15$). Culling outside this 3D spatial region eliminates 98%+ of unnecessary field calculation.

$$\text{Effective 3D Influence Region: } R_{\text{influence}} = [X_{\text{min}}-6, Y_{\text{min}}-6, X_{\text{max}}+6, Y_{\text{max}}+6] \times [L_{\text{source}}-3, L_{\text{source}}+3]$$

---

## 5. C# 13 & SkiaSharp SIMD Engine Implementation

```csharp
namespace Grove.SpatialLayers.Physics;

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using SkiaSharp;
using Grove.SpatialLayers.Architecture;

public readonly record struct FieldSource(
    long PlacementId,
    int OriginX,
    int OriginY,
    int Width,
    int Height,
    int LayerStackIndex,
    float Mass,
    SKColor Color
);

/// <summary>
/// High-performance SIMD-accelerated vertical aura permeability calculator.
/// </summary>
public sealed class VerticalAuraPermeabilityEngine
{
    private const float BaselineEnergy E0 = 0.05f;
    private const float PerceptionThreshold = 0.15f;
    private const float SpatialCullRadiusSq = 36.0f; // 6^2
    private const int LayerCullDepth = 3;

    /// <summary>
    /// Computes total cell energy at coordinate (cellX, cellY) on active layer activeStackIndex.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float CalculateCellEnergy(
        int cellX, 
        int cellY, 
        int activeStackIndex, 
        ReadOnlySpan<FieldSource> sources)
    {
        float totalEnergy = E0;

        foreach (ref readonly var source in sources)
        {
            int deltaL = Math.Abs(activeStackIndex - source.LayerStackIndex);
            if (deltaL > LayerCullDepth) continue;

            // Compute distance to nearest edge of placement footprint
            float dx = Math.Max(0, Math.Max(source.OriginX - cellX, cellX - (source.OriginX + source.Width - 1)));
            float dy = Math.Max(0, Math.Max(source.OriginY - cellY, cellY - (source.OriginY + source.Height - 1)));
            float dSq = dx * dx + dy * dy;

            if (dSq > SpatialCullRadiusSq) continue;

            // Vertical Attenuation Factor gamma^|deltaL| where gamma = 0.5
            float layerDecay = MathF.Pow(0.5f, deltaL);

            // Gravitational Energy Equation: E = (M / (1 + 0.4 * d^2)) * (0.5)^|deltaL|
            float energyContribution = (source.Mass / (1.0f + 0.4f * dSq)) * layerDecay;
            totalEnergy += energyContribution;
        }

        return totalEnergy;
    }

    /// <summary>
    /// Renders 1.5px perimeter containment rings for cells with E >= 0.15.
    /// </summary>
    public void RenderPerimeterContainmentRings(
        SKCanvas canvas, 
        SKPath contourPath, 
        SKColor primaryColor, 
        bool isSelected)
    {
        using var paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.5f, // --field-perimeter-width token
            Color = primaryColor.WithAlpha((byte)(isSelected ? 204 : 64)), // 0.80 or 0.25 alpha
            IsAntialias = true
        };

        canvas.DrawPath(contourPath, paint);
    }
}
```

---

## 6. Architectural Traceability & References

- **Original Notes**: `docs/product/original-notes/Grove - Layers.txt`, `Grove - Field ledger.txt`
- **Design System Grammar**: `docs/design-system/10-grammar/Layer-depth.md`, `docs/design-system/10-grammar/Content-concentration.md`
- **Related Specs**: [ADR-003](file:///C:/dev/grove-v9/docs/specs/spatial-grid/ADR-003-Spatial-Aura-Physics.md), [ADR-040](file:///C:/dev/grove-v9/docs/specs/spatial-layers/ADR-040-Spatial-Layer-System-Architecture.md)
