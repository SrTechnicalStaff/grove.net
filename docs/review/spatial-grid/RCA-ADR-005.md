# Root Cause Analysis (RCA) Ledger: ADR-005

## 1. Executive Metadata

| Property | Value |
| :--- | :--- |
| **ADR ID** | ADR-005 |
| **ADR Title** | Camera Affine Transform Engine |
| **Category** | 2D Camera Engine / Viewport Mathematics |
| **Claimed Status** | IMPLEMENTED - AWAITING USER REVIEW |
| **Verified Status** | IMPLEMENTED & INTERACTIVE IN LIVE UI (Focal-point zoom, pan, affine math active; VSync spring dampening physics and formal interface missing) |
| **Audited Runtime Target** | C# 13 / .NET 9.0 / Avalonia UI 11.2.5 |
| **Audit Date** | 2026-08-12 |

---

## 2. Normative Specification Requirement Inventory

| Spec ID | Requirement / Contract Description | Target Type / Symbol | Specified Value / Formula |
| :--- | :--- | :--- | :--- |
| **REQ-005-01** | Forward Affine Matrix Formula | Matrix Equation | $T = \begin{bmatrix} s & 0 & T_x \\ 0 & s & T_y \\ 0 & 0 & 1 \end{bmatrix}$ |
| **REQ-005-02** | Inverse Affine Matrix Formula | Matrix Equation | $T^{-1} = \begin{bmatrix} 1/s & 0 & -T_x/s \\ 0 & 1/s & -T_y/s \\ 0 & 0 & 1 \end{bmatrix}$ |
| **REQ-005-03** | Focal-Point Zoom Operator | Zoom Equation | $T'_x = x_c - \frac{s'}{s}(x_c - T_x)$, $T'_y = y_c - \frac{s'}{s}(y_c - T_y)$ |
| **REQ-005-04** | Zoom Scale Bounds | Scale Range | $s \in [0.01, 10.0]$ ($1\%$ to $1000\%$) |
| **REQ-005-05** | Viewport Culling Bounds | World Bounds | $R_w = \left[ \frac{-T_x}{s}, \frac{-T_y}{s}, \frac{W - T_x}{s}, \frac{H - T_y}{s} \right]$ |
| **REQ-005-06** | Visible Cell Index Range | Cell Range | $X_{\text{cell, min}} = \lfloor \frac{-T_x}{s \cdot P} \rfloor$, $X_{\text{cell, max}} = \lceil \frac{W - T_x}{s \cdot P} \rceil$ |
| **REQ-005-07** | 120Hz-240Hz VSync Loop | Render Loop | `CompositionTarget.Rendering` frame update loop |
| **REQ-005-08** | Dampened Inertia Physics | Physics Math | `Position = Lerp(Position, TargetPosition, Factor)` |
| **REQ-005-09** | Immutable Camera State Record | `CameraState` | Record struct with `Translation`, `Scale`, `TransformMatrix`, `InverseMatrix` |
| **REQ-005-10** | Camera Engine Interface | `ICameraEngine` | Interface with `PanBy`, `ZoomAt`, `WorldToScreen`, `ScreenToWorld`, `GetVisibleWorldBounds` |
| **REQ-005-11** | Camera Engine Implementation | `CameraTransformEngine` | Class implementing `ICameraEngine` with matrix math |

---

## 3. Codebase Reality & Line-by-Line Evidence

### 3.1 Implemented Artifacts

- **Core Camera Affine Engine**: Implemented in [`src/GroveApp/Engine/CameraModule.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/CameraModule.cs#L1-L104).
  - Lines 13-14: MinZoom ($0.01$) and MaxZoom ($10.0$).
  - Lines 24-30: `WorldToScreen` transform: $P_{\text{screen}} = P_{\text{world}} \cdot s + T$.
  - Lines 36-43: `ScreenToWorld` inverse transform: $P_{\text{world}} = (P_{\text{screen}} - T) / s$.
  - Lines 85-93: `ZoomAt` focal-point zoom operator matching exact mathematical specification ($T'_x = x_c - \frac{s'}{s}(x_c - T_x)$).
  - Lines 98-101: `GetTransformMatrix()` returning 2D scale/translation matrix.
- **Interactive UI Integration**: Fully wired in [`src/GroveApp/Controls/GridCanvasControl.cs`](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs#L41-L86).
  - Mouse wheel zoom (`OnPointerWheelChanged` [L612-L623]): Invokes `Camera.ZoomAt(e.GetPosition(this), zoomFactor)`.
  - Middle/Right mouse drag panning (`OnPointerMoved` [L391-L400]): Updates `CameraX` and `CameraY`.
  - Double click cell coordinate translation: Maps click screen points to cell indices.

### 3.2 Missing / Non-Conforming Features (0% Implemented)

- **`CameraState` Struct**: **0% Implemented**. Missing record struct `Grove.SpatialGrid.Camera.CameraState`. Camera properties (`CameraX`, `CameraY`, `Zoom`) are exposed as individual mutable primitive properties on `CameraModule`.
- **`ICameraEngine` Interface**: **0% Implemented**. Missing interface `ICameraEngine`.
- **Class Naming Divergence**: `CameraTransformEngine` was renamed to `CameraModule`.
- **VSync Inertia Spring Dampening**: **0% Implemented**. Missing `Vector2.Lerp` dampened spring physics on `CompositionTarget.Rendering`. Zoom and pan interactions respond instantaneously without smooth exponential dampening decay.

---

## 4. Standards & Visual Plane Seam Audit

- **Visual Plane Compliance**: Complies with Plane 0 requirements. Transforms Plane 0 grid coordinates smoothly while Plane 1 overlays and Plane 2 HUD elements anchor correctly.
- **Token Integrity**: Respects scale range tokens (`0.01` to `10.0`) and cell pitch tokens (`220.0`).
- **Architectural Seams**: `CameraModule` is completely decoupled from content, text, and UI controls, hiding all 2D affine transformation math behind clean methods.

---

## 5. Root Cause Analysis

### Why claimed status (`IMPLEMENTED - AWAITING USER REVIEW`) diverges from actual interactive reality:

1. **Successful Interactive Math**: The core focal-point zoom and camera panning equations were implemented cleanly and fully validated in interactive user tests.
2. **Simplified Direct Step Panning vs Spring Physics**: Dampened inertia spring physics (`Vector2.Lerp`) were omitted to avoid complex frame velocity tracking, opting instead for instant 1:1 mouse tracking.
3. **Class Renaming**: `CameraTransformEngine` was simplified to `CameraModule` during early refactoring without updating the ADR class contract.
