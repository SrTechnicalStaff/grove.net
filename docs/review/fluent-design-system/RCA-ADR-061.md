# Root Cause Analysis (RCA) Ledger: ADR-061

| Property | Value |
| :--- | :--- |
| **ADR ID** | [ADR-061](file:///C:/dev/grove-v9/docs/specs/fluent-design-system/ADR-061-Aura-Field-Fluid-Gradient-Rendering.md) |
| **Title** | Aura Field Fluid Gradient Rendering |
| **Category** | Native Fluent Design (`fluent-design-system`) |
| **Claimed Status** | `IMPLEMENTED - AWAITING USER REVIEW` |
| **Verified Status** | `NOT IMPLEMENTED / 0% LIVE UI` |
| **Audit Date** | 2026-08-12 |

---

## 1. Executive Metadata & Audit Summary

- **Claimed Implementation Files**: [`FieldLedgerEngine.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/FieldLedgerEngine.cs)
- **Verified Runtime Reality**: `FieldLedgerEngine.cs` implements discrete cell energy calculation and value object storage. However, the hardware-accelerated fluid radial gradient rendering engine mandated by ADR-061 is **0% Implemented**. The specified class `FluidAuraRenderer`, SkiaSharp radial gradient shaders (`SKShader.CreateRadialGradient`), Hermite smoothstep alpha falloff interpolation, additive blending (`SKBlendMode.Plus`), and 4-pass path-difference grid line masking ($\mathcal{P}_{\text{visible}} = \mathcal{P}_{\text{grid}} \setminus \Omega_{\text{aura}}$) do **NOT** exist anywhere in the codebase. Instead, [`FieldLedgerModule.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/FieldLedgerModule.cs) fills discrete 220px/44px cell squares with flat `SolidColorBrush` quads via Avalonia's `DrawingContext.FillRectangle()`, causing dark grid lines to show through active energy heatmaps in live UI.

---

## 2. Normative Specification Requirement Inventory

| Requirement ID | Spec Requirement / Symbol Name | Target Specification Details |
| :--- | :--- | :--- |
| `REQ-061-01` | Continuous Field Principle | Gravitational potential distribution $E(x,y) = \frac{M}{1 + \alpha r^2}$ rendered as a smooth continuous scalar field on Plane 0. |
| `REQ-061-02` | Inner Line Suppression Rule | Discrete grid lines MUST be completely suppressed across any cell region where composite field energy $E_{\text{total}}(x,y) \ge E_{\text{suppress}} = 0.05$. |
| `REQ-061-03` | Fluid Radial Shader Fills | Fluid heatmaps MUST render as continuous radial gradient fills using hardware-accelerated `SKShader.CreateRadialGradient` operating on Plane 0. |
| `REQ-061-04` | Hermite Smoothstep Alpha | Radial color stop alpha values $\mathcal{A}(t) = \alpha_{\text{peak}} \cdot (1 - 3t^2 + 2t^3)$ over normalized radius $t = r / R_{\text{max}} \in [0, 1]$, ensuring $\left. \frac{d\mathcal{A}}{dt} \right|_{t=1} = 0$. |
| `REQ-061-05` | 4-Pass Execution Sequence | Pass 1: Radial Gradient Fills; Pass 2: Compute Path Difference $\mathcal{P}_{\text{visible}} = \mathcal{P}_{\text{grid}} \setminus \Omega_{\text{aura}}$; Pass 3: Render Hairlines; Pass 4: Draw 1.5px Outer Perimeter Ring ($E \ge 0.15$). |
| `REQ-061-06` | Path Difference Formulation | $\mathcal{P}_{\text{visible}} = \mathcal{P}_{\text{grid}} \setminus \Omega_{\text{aura}}$ using `SKPath.Op(auraOccupancyPath, SKPathOp.Difference, visibleGridPath)`. |
| `REQ-061-07` | C# 13 Renderer Class | Class `FluidAuraRenderer` in namespace `Grove.SpatialGrid.Rendering` managing `_gradientPaint`, `_perimeterPaint`, `_gridHairlinePaint`, and `RenderAuraPlane()`. |

---

## 3. Codebase Reality & Line-by-Line Evidence

| Requirement ID | Codebase Symbol / Location | Implementation Status & Evidence |
| :--- | :--- | :--- |
| `REQ-061-01` | [`FieldLedgerEngine.cs:535-542`](file:///C:/dev/grove-v9/src/GroveApp/Engine/FieldLedgerEngine.cs#L535-L542) | **PARTIAL**: Energy physics weight formula $W = \frac{M}{1 + 0.4 \cdot d^2}$ is evaluated for discrete grid cell coordinates, but **not** rendered as a continuous field. |
| `REQ-061-02` | `FieldLedgerModule.cs` / `GridLineModule.cs` | **0% Implemented**: Inner line suppression is completely absent. Hairline grid lines are drawn unconditionally across the entire viewport by [`GridLineModule.cs:40-75`](file:///C:/dev/grove-v9/src/GroveApp/Engine/GridLineModule.cs#L40-L75), polluting energy heatmaps with dark grid lines. |
| `REQ-061-03` | `FieldLedgerModule.cs:48-63` | **0% Implemented**: No `SKShader.CreateRadialGradient` or radial shader exists. Rendered via discrete cell-by-cell `context.FillRectangle(cellBrush, cellRect)` using flat `SolidColorBrush`. |
| `REQ-061-04` | `src/GroveApp/` | **0% Implemented**: Hermite smoothstep polynomial interpolation $\mathcal{A}(t) = \alpha_{\text{peak}} (1 - 3t^2 + 2t^3)$ does **NOT** exist in any rendering file. |
| `REQ-061-05` | `GridCanvasControl.cs:911-915` | **NON-COMPLIANT**: Execution sequence is inverted and unmasked: `_fieldLedgerModule.RenderFieldLedger()` is drawn first (flat squares), followed by `_gridLineModule.RenderGridLines()` drawn directly over top without masking. |
| `REQ-061-06` | `src/GroveApp/` | **0% Implemented**: `SKPath.Op()` path difference subtraction (`Difference`) is completely missing. No spatial path subtraction is performed anywhere in the codebase. |
| `REQ-061-07` | `src/GroveApp/` | **0% Implemented**: Namespace `Grove.SpatialGrid.Rendering` and class `FluidAuraRenderer` do **NOT** exist anywhere in the repository. |

---

## 4. Standards & Visual Plane Seam Audit

- **Visual Plane Isolation (Plane 0 vs Layer 1 vs Plane 2)**:
  - Spec mandates Plane 0 Skia hardware vector rendering for fluid aura gradients with path-difference inner line masking.
  - Codebase reality: Avalonia `DrawingContext` high-level primitives (`FillRectangle`) are used on Plane 0 inside `FieldLedgerModule.cs`, missing GPU shader acceleration and path geometry operations.
- **Visual Defect Persistence**:
  - The exact visual defect described in Section 1 of ADR-061 ("mini-grid gridline grid inside active aura fields") remains present in live runtime rendering because grid lines are painted on top of aura cell fills without path subtraction.

---

## 5. Root Cause Analysis

### 5.1 Why Gaps Exist Between Claimed Status and Interactive UI Reality
1. **Fallback to Discrete Cell Fills**: Rather than writing a native SkiaSharp rendering module utilizing `SKShader` and `SKPath.Op`, the implementation team reused existing discrete grid cell iteration (`activeAuraCells`) and rendered each cell as a flat square using Avalonia's `DrawingContext.FillRectangle()`.
2. **Missing SkiaSharp Pipeline Integration**: The core engine was wired directly to Avalonia's immediate-mode drawing API (`DrawingContext`) rather than tapping into low-level SkiaSharp `SKCanvas` contexts. This prevented the use of hardware-accelerated radial shaders (`SKShader.CreateRadialGradient`) and path boolean operations (`SKPathOp.Difference`).
3. **Falsified Roadmap Status**: `ADR-ROADMAP.md` claimed `FluidAuraRenderer` was implemented via `FieldLedgerEngine.cs`. In truth, `FieldLedgerEngine.cs` only contains cell energy data lookup math, while the actual rendering class `FluidAuraRenderer` specified in Section 4 of ADR-061 was never created.
