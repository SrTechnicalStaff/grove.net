# Root Cause Analysis (RCA) Ledger: ADR-003

## 1. Executive Metadata

| Property | Value |
| :--- | :--- |
| **ADR ID** | ADR-003 |
| **ADR Title** | Spatial Aura Physics |
| **Category** | Aura Field Dynamics / Spatial Physics Engine |
| **Claimed Status** | IMPLEMENTED - AWAITING USER REVIEW |
| **Verified Status** | PARTIALLY IMPLEMENTED (Gravitational math and layer attenuation active; 1.5px perimeter isoline rings and 5 view-distance representation tiers missing) |
| **Audited Runtime Target** | C# 13 / .NET 9.0 / Avalonia UI 11.2.5 |
| **Audit Date** | 2026-08-12 |

---

## 2. Normative Specification Requirement Inventory

| Spec ID | Requirement / Contract Description | Target Type / Symbol | Specified Value / Formula |
| :--- | :--- | :--- | :--- |
| **REQ-003-01** | Content Mass Values | Mass Property | Note $M = 1.0$, Document $M = 2.5$, Large Image $M = 4.0$ |
| **REQ-003-02** | Inverse-Distance Quadratic Formula | Field Equation | $E_i(d) = \frac{M_i}{1 + 0.4 \cdot d^2}$ |
| **REQ-003-03** | Inter-Layer Depth Falloff | Layer Equation | $E_i(d, \Delta L) = \frac{M_i}{1 + 0.4 \cdot d^2} \cdot (0.5)^{\vert\Delta L\vert}$ |
| **REQ-003-04** | Total Composite Energy Equation | Composite Formula | $E_{\text{total}}(c) = E_0 + \sum_{i} E_i(d_{i,c}, \Delta L_i)$ |
| **REQ-003-05** | Bounding-Box Culling Radius | Spatial Constraint | $d_{\text{cull}} = 6$ cells ($R_{\text{influence}} = [X_{\min}-6, Y_{\min}-6, X_{\max}+6, Y_{\max}+6]$) |
| **REQ-003-06** | Perimeter Containment Energy Threshold | Energy Threshold | $E_{\text{threshold}} = 0.15$ |
| **REQ-003-07** | Perimeter Ring Stroke Token | Stroke Token | `--field-perimeter-width`: $1.5\text{ DIPs}$ |
| **REQ-003-08** | Perimeter Ring Alpha Tokens | Alpha Tokens | `--field-perimeter-ink`: $0.25$ (unselected), $0.80$ (selected) |
| **REQ-003-09** | Physics Calculator Class | `AuraFieldCalculator` | Static class with `CalculateContribution()` & `DistanceToFootprint()` |
| **REQ-003-10** | Aura Physics Engine | `SpatialAuraPhysicsEngine` | Engine class executing aura recalculation & `GeneratePerimeterContourPath()` |
| **REQ-003-11** | Representation Tier WV-00 | Working Zoom | $S_{\text{cell}} \ge 72\text{px}$: Full content form, true typography, interactive chrome |
| **REQ-003-12** | Representation Tier WV-01 | Stepped Back | $56\text{px} > S_{\text{cell}} \ge 19\text{px}$: Chrome shed, text bones remain visible |
| **REQ-003-13** | Representation Tier WV-02 | Far Zoom Stand-in | $S_{\text{cell}} \le 18\text{px}$: Kind-coded stand-in (`.si-sheet`, `.si-note`, `.si-fig`) |
| **REQ-003-14** | Representation Tier WV-03/WV-04 | Approach & Arrived | Hysteresis cross-fade promotion (160ms transition) |
| **REQ-003-15** | Viewport Hysteresis Bands | Zoom Limits | Demote to stand-in at $S_{\text{cell}} \le 18\text{px}$; promote to page at $S_{\text{cell}} \ge 28\text{px}$ |
| **REQ-003-16** | Selection Interaction Outline | Selection Style | $2\text{px}$ outline `#96B6F8` (`--signal-interaction`), offset $3\text{px}$ outside content edge |

---

## 3. Codebase Reality & Line-by-Line Evidence

### 3.1 Implemented Artifacts

- **Gravitational Distance Quadratic Formula**: Implemented in [`src/GroveApp/Engine/FieldLedgerEngine.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/FieldLedgerEngine.cs#L220-L250).
  - Energy calculation $E = \frac{M}{1 + 0.4 \cdot d^2}$ correctly applies mass and distance attenuation.
  - Inter-layer decay multiplier $(0.5)^{|\Delta L|}$ is computed in `FieldLedgerEngine.cs` [L242-L248](file:///C:/dev/grove-v9/src/GroveApp/Engine/FieldLedgerEngine.cs#L242-L248).
  - Distance bounding box cutoff ($d \le 6$) enforced in `FieldLedgerEngine.cs` [L225].
- **Selection Outline**: Implemented in [`src/GroveApp/Engine/NoteRenderModule.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/NoteRenderModule.cs#L140-L160) ($2\text{px}$ stroke in `#96B6F8`, offset $3\text{px}$ outside content edge).

### 3.2 Missing / Non-Conforming Features (0% Implemented)

- **`AuraFieldCalculator` & `SpatialAuraPhysicsEngine`**: **0% Implemented**. Missing classes `Grove.SpatialGrid.AuraPhysics.AuraFieldCalculator` and `SpatialAuraPhysicsEngine`. Physics math was merged directly into `FieldLedgerEngine.cs` without extracting standalone physics abstractions.
- **1.5px Perimeter Containment Isolines (`GeneratePerimeterContourPath`)**: **0% Implemented**. Missing method `GeneratePerimeterContourPath`. Cells reaching $E \ge 0.15$ render standard rectangular heatmap tiles instead of continuous 1.5px contour isoline paths.
- **5 View-Distance Representation Tiers (`WV-00` through `WV-04`)**: **0% Implemented**. Missing representation tier state machine and stand-in renderers (`.si-sheet`, `.si-note`, `.si-fig`). At extreme far zoom ($s \le 0.05$), content cards continue to render full text elements or disappear entirely, rather than swapping to 96px footprint stand-in snapshots.
- **Hysteresis Band Controller**: **0% Implemented**. Missing zoom hysteresis thresholds ($18\text{px} \to 28\text{px}$ for stand-ins; $56\text{px} \to 72\text{px}$ for detail shedding).

---

## 4. Standards & Visual Plane Seam Audit

- **Visual Plane Separation**: Aura heatmap fills operate on Plane 0. Selection outlines correctly render on Plane 0 over target content footprints.
- **Design Token Compliance**: Selection color `#96B6F8` (`Colors.SignalInteraction`) and token stroke weights are strictly enforced.
- **Architectural Seams**: Physics equations are tightly coupled within `FieldLedgerEngine.cs`. Missing formal `AuraPhysicsEngine` module boundaries.

---

## 5. Root Cause Analysis

### Why claimed status (`IMPLEMENTED - AWAITING USER REVIEW`) diverges from actual interactive reality:

1. **Partial Physics Formula Completion**: Because the core inverse-quadratic formula $E = \frac{M}{1 + 0.4 \cdot d^2}$ was implemented inside `FieldLedgerEngine.cs` and visually verified via aura heatmaps, the ADR was declared implemented.
2. **Complexity of Isoline Contour Extraction**: Extracting continuous 1.5px vector isolines along grid cell edges (`GeneratePerimeterContourPath`) required marching-squares contour algorithms that were deferred and never completed.
3. **LOD Stand-in Pipeline Omission**: View-distance representation tiers (`WV-00` through `WV-04`) were viewed as visual optimization polish rather than core specification invariants, leaving stand-in demotion/promotion unwritten.
