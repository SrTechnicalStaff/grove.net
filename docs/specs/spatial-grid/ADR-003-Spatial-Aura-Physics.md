---
status: "PARTIAL — verified aura physics and discrete rendering"
---

# ADR-003: Spatial Aura Physics

| Property | Value |
| :--- | :--- |
| **Status** | PARTIAL — verified aura physics and discrete rendering |
| **Date** | 2026-08-12 |
| **Area** | Aura Field Dynamics / Spatial Physics Engine |
| **Target Runtime** | C# 13 / .NET 9 / SkiaSharp |
| **Authors** | Chief Product Definition Architect |

---

## 1. Context & Physical Model

As defined in original architectural notes (`Grove - Field ledger.txt`, `Grove - Layers.txt`), content on the spatial grid radiates a visual and metadata energy field termed **Aura**. Aura behaves as a weak continuous physical force analogous to gravity in space-time.

Key properties:
1. Every placed content item (`Note`, `Document`, `Picture`) possesses a Mass $M \ge 1.0$.
2. Content energy attenuates continuously across spatial distance $d$.
3. Energy saturates vertically across adjacent layers via a layer-depth decay factor.
4. Cells reaching isoline energy threshold $E \ge 0.15$ form distinct visual perimeter containment rings.

---

## 2. Mathematical Physics Formulas

### 2.1 Gravitational Field Equation
The field energy contribution $E_i(d)$ cast by content item $i$ with mass $M_i$ onto a grid cell at distance $d$ is governed by the inverse-distance quadratic equation:

$$E_i(d) = \frac{M_i}{1 + 0.4 \cdot d^2}$$

Where:
- $M_i \ge 1.0$: Mass of content item $i$ (e.g., Note $M=1.0$, Document $M=2.5$, Large Image $M=4.0$).
- $d = \sqrt{\Delta x^2 + \Delta y^2}$: Euclidean distance in cell units from the nearest cell of the content footprint.

### 2.2 Inter-Layer Saturation & Depth Falloff
When energy radiates across layer depths, the inter-layer decay factor $\gamma = 0.5$ attenuates energy per layer delta $|\Delta L| = |L_{\text{target}} - L_{\text{source}}|$:

$$E_i(d, \Delta L) = \frac{M_i}{1 + 0.4 \cdot d^2} \cdot \left(0.5\right)^{|\Delta L|}$$

### 2.3 Total Cell Energy Equation
The composite energy $E_{\text{total}}(c)$ for any cell $c$ at position $(x,y)$ on layer $L$ combines the non-zero baseline energy $E_0 = 0.05$ with all active spatial sources:

$$E_{\text{total}}(c) = E_0 + \sum_{i \in \text{Sources}} \left( \frac{M_i}{1 + 0.4 \cdot d_{i,c}^2} \cdot \left(0.5\right)^{|L - L_i|} \right)$$

---

## 3. Spatial Bounding-Box Culling Algorithm ($d \le 6$)

### 3.1 Culling Proof & Cutoff Threshold
Evaluating infinite distance across a continuous grid is computationally intractable. We define a strict bounding box culling cutoff at cell radius $d_{\text{cull}} = 6$:

$$\text{At } d = 6: \quad E(6) = \frac{M}{1 + 0.4(36)} = \frac{M}{1 + 14.4} = \frac{M}{15.4} \approx 0.06493 \cdot M$$

For nominal content mass $M = 1.0$, $E(6) \approx 0.065$, which falls below the visible perception threshold and does not trigger perimeter isolines ($E_{\text{threshold}} = 0.15$).

### 3.2 Bounding-Box Region Constraints
For a content footprint spanning cell rectangle $[X_{\text{min}}, Y_{\text{min}}, X_{\text{max}}, Y_{\text{max}}]$ on layer $L$:
- Effective spatial influence bounding box:

$$R_{\text{influence}} = \left[ X_{\text{min}} - 6, \; Y_{\text{min}} - 6, \; X_{\text{max}} + 6, \; Y_{\text{max}} + 6 \right]$$

- Neighborhood grid dimensions: Maximum $(W_{\text{footprint}} + 12) \times (H_{\text{footprint}} + 12)$ cells.
- Any cell outside $R_{\text{influence}}$ receives zero field calculation from content $i$.

---

## 4. 1.5px Perimeter Containment Ring Specification

Cells whose composite energy $E_{\text{total}} \ge 0.15$ form an occupied energy domain. The boundary between occupied ($E \ge 0.15$) and unoccupied ($E < 0.15$) cells is rendered as a continuous containment ring.

### 4.1 Rendering Tokens & Parameters
- Token `--field-perimeter-width`: $1.5\text{ DIPs}$.
- Token `--field-perimeter-ink`: $0.25$ alpha (unselected region).
- Token `--field-perimeter-selected`: $0.80$ alpha (selected region).
- Hue: Rendered in the primary content's authored hue (e.g., Note Violet `#6E62A6`, Clay `#B0524E`, Slate Blue `#4E6E9C`).

### 4.2 Skia Contour Path Extraction
To render the 1.5px perimeter ring crisp on Plane 0:
1. Iterate over all cells in $R_{\text{influence}}$.
2. Identify edges separating a cell with $E \ge 0.15$ from an adjacent cell with $E < 0.15$.
3. Build a continuous `SKPath` along cell edges.
4. Draw using `SKPaint` with `StrokeWidth = 1.5f`, `Style = SKPaintStyle.Stroke`, `IsAntialias = true`.

---

## 5. C# Engine Implementation & Type Contracts

```csharp
namespace Grove.SpatialGrid.AuraPhysics;

using System;
using System.Numerics;
using SkiaSharp;
using Grove.SpatialGrid.FieldLedger;

/// <summary>
/// Core mathematical physics calculator for spatial aura fields.
/// </summary>
public static class AuraFieldCalculator
{
    public const float BaselineEnergy = 0.05f;
    public const float PerimeterThresholdEnergy = 0.15f;
    public const float PerimeterWidthPx = 1.5f;
    public const int MaxCullingRadiusCells = 6;
    public const float LayerDecayFactor = 0.5f;

    /// <summary>
    /// Computes spatial energy contribution: E = M / (1 + 0.4 * d^2) * (0.5)^|deltaL|
    /// </summary>
    public static float CalculateContribution(float mass, float distanceCells, int layerDelta)
    {
        if (distanceCells > MaxCullingRadiusCells) return 0.0f;

        float distanceAtten = 1.0f + 0.4f * (distanceCells * distanceCells);
        float spatialEnergy = mass / distanceAtten;

        if (layerDelta == 0) return spatialEnergy;

        float layerAtten = MathF.Pow(LayerDecayFactor, Math.Abs(layerDelta));
        return spatialEnergy * layerAtten;
    }

    /// <summary>
    /// Computes Chebyshev / Euclidean distance from point (cx, cy) to a rectangular footprint.
    /// </summary>
    public static float DistanceToFootprint(int cx, int cy, int fxMin, int fyMin, int fxMax, int fyMax)
    {
        int dx = Math.Max(0, Math.Max(fxMin - cx, cx - fxMax));
        int dy = Math.Max(0, Math.Max(fyMin - cy, cy - fyMax));
        return MathF.Sqrt(dx * dx + dy * dy);
    }
}

/// <summary>
/// Engine executing spatial aura field calculations and perimeter extraction.
/// </summary>
public sealed class SpatialAuraPhysicsEngine
{
    public void RecalculateContentAura(
        Guid contentId,
        float mass,
        GridCellPosition[] footprint,
        Guid sourceLayerId,
        Vector4 contentHue,
        FieldLedgerManager ledgerManager)
    {
        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;

        foreach (var pos in footprint)
        {
            if (pos.X < minX) minX = pos.X;
            if (pos.X > maxX) maxX = pos.X;
            if (pos.Y < minY) minY = pos.Y;
            if (pos.Y > maxY) maxY = pos.Y;
        }

        // Apply d <= 6 bounding-box culling constraint
        int startX = minX - AuraFieldCalculator.MaxCullingRadiusCells;
        int endX = maxX + AuraFieldCalculator.MaxCullingRadiusCells;
        int startY = minY - AuraFieldCalculator.MaxCullingRadiusCells;
        int endY = maxY + AuraFieldCalculator.MaxCullingRadiusCells;

        int totalCells = (endX - startX + 1) * (endY - startY + 1);
        Span<CellLedgerEntry> updatedEntries = new CellLedgerEntry[totalCells];
        int index = 0;

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                float distance = AuraFieldCalculator.DistanceToFootprint(x, y, minX, minY, maxX, maxY);
                float energy = AuraFieldCalculator.CalculateContribution(mass, distance, 0);

                var entry = new CellLedgerEntry(new GridCellPosition(x, y), sourceLayerId)
                {
                    TotalEnergy = AuraFieldCalculator.BaselineEnergy + energy,
                    PrimaryHue = contentHue,
                    SourceCount = 1,
                    InlineSource0 = new FieldSourceMetadata
                    {
                        ContentId = contentId,
                        LayerId = sourceLayerId,
                        Mass = mass,
                        ContributedEnergy = energy,
                        ColorHue = contentHue,
                        TimestampTicks = DateTime.UtcNow.Ticks
                    }
                };

                updatedEntries[index++] = entry;
            }
        }

        ledgerManager.BatchMutateRegion(updatedEntries);
    }

    public SKPath GeneratePerimeterContourPath(ReadOnlySpan<CellLedgerEntry> regionEntries, float cellPitchPx)
    {
        var path = new SKPath();
        
        foreach (ref readonly var entry in regionEntries)
        {
            if (entry.TotalEnergy >= AuraFieldCalculator.PerimeterThresholdEnergy)
            {
                float left = entry.Position.X * cellPitchPx;
                float top = entry.Position.Y * cellPitchPx;
                float right = left + cellPitchPx;
                float bottom = top + cellPitchPx;

                // Add cell boundary stroke segment
                path.AddRect(new SKRect(left, top, right, bottom));
            }
        }

        return path;
    }
}

---

## 6. Distance Representation Tiers & Selection Aura Integration

### 6.1 View Distance Shedding & Representation Tiers
As camera zoom $s$ changes, content representation transitions across 5 defined representation tiers (`WV-00` through `WV-04`) to preserve visual calm while keeping cell presence constant:
- **WV-00 Working Zoom ($S_{\text{cell}} \ge 72\text{px}$)**: Full content form, true text typography, authored color fills, hover response chrome available.
- **WV-01 Stepped Back ($56\text{px} > S_{\text{cell}} \ge 19\text{px}$)**: Interactive handles and edit affordances shed first. Surface textures (paper grain, inset rings) shed next while text bones (title mass, rules) remain visible.
- **WV-02 Far Zoom ($S_{\text{cell}} \le 18\text{px}$)**: Detail collapses into a kind-coded stand-in on the exact footprint bounds:
  - *Sheet (Document)*: Paper fill `#F5F5F5` with ruled header line and 4 text lines (`.si-sheet`).
  - *Note*: Authored color block (`#6E62A6`, `#B0524E`, `#4E6E9C`) with white line strokes (`.si-note`).
  - *Figure (Picture)*: Paper frame with simplified SVG mountain/sun geometry (`.si-fig`).
- **WV-03 Approach (Promotion Pending)**: Crossing the promotion threshold queues true form loading; the distant stand-in holds the footprint position without blanking ("pending is never blank").
- **WV-04 Arrived (Promotion Complete)**: True form resolves via a 160ms cross-fade in place, matching exact spatial identity, position, and footprint extent.

### 6.2 Viewport Hysteresis Thresholds
To prevent visual flickering when camera position rests near a scale boundary, hysteresis bands are enforced:
- **Stand-in / Page Boundary**: Demote to stand-in at $S_{\text{cell}} \le 18\text{px}$; promote back to page at $S_{\text{cell}} \ge 28\text{px}$.
- **Detail Shedding Boundary**: Shed surface detail below $S_{\text{cell}} < 56\text{px}$; restore full detail at $S_{\text{cell}} \ge 72\text{px}$.

### 6.3 Selection State Aura Response
When a placement is selected:
1. **Interaction Outline**: A 2px outline in `#96B6F8` (`--signal-interaction`) is drawn $3\text{px}$ outside the content edge with a soft glow `0 0 24px 6px rgb(150 182 248 / 0.45)`.
2. **Structural Field Response**: Surrounding cells brighten in the interaction hue (`rgba(150,182,248,0.13)` for edge cells, `rgba(150,182,248,0.06)` for diagonal cells) with perimeter inset `inset 0 0 0 1.5px rgb(150 182 248 / 0.30)`.
3. **Selection Invariance**: Selection outline and field response operate identically across working forms and distant stand-ins.
```
