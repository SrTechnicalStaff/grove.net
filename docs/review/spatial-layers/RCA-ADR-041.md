# RCA Ledger: ADR-041 — Vertical Aura Permeability and Attenuation Physics

## 1. Executive Metadata

| Metadata Field | Value |
| :--- | :--- |
| **ADR ID** | ADR-041 |
| **ADR Title** | Vertical Aura Permeability and Attenuation Physics |
| **Category** | Spatial Layers (`docs/specs/spatial-layers/`) |
| **Claimed Status in Spec Header** | IMPLEMENTED - AWAITING USER REVIEW |
| **Verified Status (User-Observable)** | **FALSE CLAIM — PARTIALLY IMPLEMENTED (15% Implemented, Physics & Rendering Missing)** |
| **Audit Date** | 2026-08-12 |
| **Target Runtime** | C# 13 / .NET 9 / SkiaSharp |
| **Auditor** | Principal AI Systems Architect & Product Auditor |

---

## 2. Normative Specification Requirement Inventory

| ID | Requirement Category | Symbol / Contract Name | Normative Specification Requirement |
| :--- | :--- | :--- | :--- |
| **REQ-041-01** | Gravitational Energy | Inter-Layer Energy $E_i(d, \Delta L)$ | $E_i(d, \Delta L) = \frac{M_i}{1 + 0.4 \cdot d^2} \cdot \gamma^{|\Delta L|}$ where $\gamma = 0.5$ and $|\Delta L| = |L_{\text{target}} - L_{\text{source}}|$. |
| **REQ-041-02** | Composite Cell Field | Total Energy $E_{\text{total}}(c, L_{\text{active}})$ | $E_{\text{total}}(c, L_{\text{active}}) = E_0 + \sum_{i} \left( \frac{M_i}{1 + 0.4 \cdot d_{i,c}^2} \cdot (0.5)^{|L_{\text{active}} - L_i|} \right)$ with baseline $E_0 = 0.05$. |
| **REQ-041-03** | Color Saturation | Energy-Weighted Color Composite | $C_{\text{cell}}(c) = \sum w_i(c) \cdot C_i$ where $w_i(c) = \frac{E_i(d_{i,c}, \Delta L_i)}{E_{\text{total}}(c) - E_0}$ in linear RGB space. |
| **REQ-041-04** | Dynamic Opacity | Logarithmic Alpha Mapping | $\alpha_{\text{cell}}(c) = \min \left( 1.0, \; \alpha_{\text{base}} + \beta \cdot \ln(1.0 + E_{\text{total}}(c)) \right)$ with $\alpha_{\text{base}} = 0.08$ and $\beta = 0.42$. |
| **REQ-041-05** | 3D Spatial Culling | Effective Influence Bounds | Spatial cutoff $d_{\text{cull}} = 6$ cells and vertical cutoff $|\Delta L|_{\text{cull}} = 3$ layers ($E_{\text{threshold}} = 0.15$). Culls $>98\%$ of unneeded calculations. |
| **REQ-041-06** | Field Source Struct | `FieldSource` Record Struct | Record struct (`PlacementId`, `OriginX`, `OriginY`, `Width`, `Height`, `LayerStackIndex`, `Mass`, `Color`). |
| **REQ-041-07** | SIMD Physics Engine | `VerticalAuraPermeabilityEngine` | High-performance SIMD-accelerated vertical aura permeability calculator executing `CalculateCellEnergy`. |
| **REQ-041-08** | Perimeter Isoline Ring | `RenderPerimeterContainmentRings` | Skia Sharp stroke rendering of $1.5\text{px}$ perimeter containment rings (`--field-perimeter-width` token) for cells with $E \ge 0.15$. |
| **REQ-041-09** | Paper Towel Analogy | Saturation Model | Saturation model blending color hues from content + saturating field hues from layers above and below. |
| **REQ-041-10** | Cascade Attenuation | Attenuation Matrix | Attenuation cascade: $\Delta L=0 \to 1.0$, $\Delta L=1 \to 0.5$, $\Delta L=2 \to 0.25$, $\Delta L=3 \to 0.125$, $\Delta L \ge 4 \to 0.0625$ (unpainted). |

---

## 3. Codebase Reality & Line-by-Line Evidence

### 3.1 Symbol Existence & Implementation Audit

| Symbol / Contract Name | Expected Location | Actual Status in `src/GroveApp/` | Line-by-Line Evidence |
| :--- | :--- | :--- | :--- |
| `VerticalAuraPermeabilityEngine`| `src/GroveApp/Engine/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. |
| `FieldSource` | `src/GroveApp/Engine/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. |
| `GetPermeability` Scalar Method| `src/GroveApp/Engine/` | **PARTIALLY IMPLEMENTED** | Implemented in [`SpatialLayerStack.cs:L111-L120`](file:///C:/dev/grove-v9/src/GroveApp/Engine/SpatialLayerStack.cs#L111) as scalar `Math.Pow(0.5, Math.Abs(sourceIndex - targetIndex))`. |
| Energy-Weighted Color Composite | `src/GroveApp/Engine/` | **0% Implemented (MISSING LOGIC)** | [`FieldLedgerEngine.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/FieldLedgerEngine.cs) calculates 2D cell energy on the active layer only; cross-layer energy-weighted RGB color blending is absent. |
| Logarithmic Alpha Mapping | `src/GroveApp/Engine/` | **0% Implemented (MISSING LOGIC)** | Dynamic log-alpha opacity mapping $\alpha_{\text{cell}}(c)$ is absent. |
| 3D Spatial Culling ($d \le 6, \Delta L \le 3$)| `src/GroveApp/Engine/` | **0% Implemented (MISSING LOGIC)** | No 3D bounding region culling exists in field ledger calculations. |

---

## 4. Standards & Visual Plane Seam Audit

### 4.1 Visual Plane Separation (Plane 0 vs Layer 1 vs Plane 2)
- **Plane 0 (Spatial Grid Canvas)**: Aura heatmap rendering on [`GridCanvasControl.cs`](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs#L180) evaluates only 2D distances on the active layer. Energy cast from inactive layers above or below is not accumulated or color-blended into active cells.

### 4.2 Code Smells & Architectural Violations
1. **False Claim in Spec Header**: The spec header states `status: "IMPLEMENTED - AWAITING USER REVIEW"`, yet `VerticalAuraPermeabilityEngine.cs` does not exist, and color blending / log-alpha mapping physics are unwritten.
2. **2D Single-Layer Fallback**: The energy field engine operates strictly in 2D on the active layer, completely ignoring the 3D paper-towel saturation physics mandated by ADR-041.

---

## 5. Root Cause Analysis (RCA)

### 5.1 Primary Root Cause
The vertical aura permeability physics engine was falsely marked as implemented in the spec header. The codebase contains only a primitive scalar method `GetPermeability` inside `SpatialLayerStack.cs`, but lacks the actual `VerticalAuraPermeabilityEngine` SIMD class, color blending formulas, and 3D culling algorithms.

### 5.2 Failure Chain
1. **Unverifiable Status Marking**: Spec status was updated without verifying the existence of `VerticalAuraPermeabilityEngine.cs`.
2. **Engine Omission**: `FieldLedgerEngine.cs` was built for 2D single-layer cell energy calculations and was never upgraded to process 3D vertical layer permeability.
