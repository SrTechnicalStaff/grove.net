# Root Cause Analysis (RCA) Ledger: ADR-071

| Property | Value |
| :--- | :--- |
| **ADR ID** | [ADR-071](file:///C:/dev/grove-v9/docs/specs/hud-feedback/ADR-071-Layer-Creation-And-Insertion-Feedback-Effects.md) |
| **Title** | Layer Creation and Insertion Feedback Effects |
| **Category** | HUD Feedback (`hud-feedback`) |
| **Claimed Status** | `IMPLEMENTED - AWAITING USER REVIEW` |
| **Verified Status** | `PARTIALLY IMPLEMENTED (NON-COMPLIANT LIVE UI)` |
| **Audit Date** | 2026-08-12 |

---

## 1. Executive Metadata & Audit Summary

- **Claimed Implementation Files**: [`Motion.cs`](file:///C:/dev/grove-v9/src/GroveApp/DesignSystem/Motion.cs), [`GridCanvasControl.cs`](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs)
- **Verified Runtime Reality**: `Motion.cs` defines the normative motion duration tokens (`PressDurationMs`, `FadeDurationMs`, `SwapDurationMs`, `ExitDurationMs`, `PlaceDurationMs`, `SweepDurationMs`), cubic Bezier curve evaluation helpers (`EvaluateEase`, `EvaluateOvershoot`), and reduced motion accessibility flag (`IsReducedMotionEnabled`). However, the runtime feedback animation controllers and Skia draw operations mandated by ADR-071 are **0% Implemented**. The classes `FlashSweepDrawOperation` (Plane 0 Skia custom draw operation executing 480ms radial flash sweep and aura pulse wave $E(r,t)$) and `LayerFeedbackAnimationController` (orchestrating Plane 0 canvas sweep and Plane 2 Layer Manager overlay row insertion animation) do **NOT** exist anywhere in `src/GroveApp/`. When a new layer is created or inserted, zero feedback animations run across Plane 0 or Plane 2 in live UI.

---

## 2. Normative Specification Requirement Inventory

| Requirement ID | Spec Requirement / Symbol Name | Target Specification Details |
| :--- | :--- | :--- |
| `REQ-071-01` | Plane 0 Canvas Radial Flash Sweep | 2D radial flash sweep and aura pulse expanding from insertion origin on Plane 0 governed by `--d-sweep` (`480ms`) with `--ease` curve ($R(t) = R_{\max} \cdot f_{\text{ease}}(t / T_{\text{sweep}})$). |
| `REQ-071-02` | Plane 2 Layer Manager Row Insertion | Vertical sliding insertion and height expansion animation inside the Layer Manager overlay stack list on Plane 2 governed by `--d-place` (`280ms`) with `--overshoot` curve. |
| `REQ-071-03` | Motion Token Scale Strictness | Durations MUST use defined tokens: `--d-press` (`90ms`), `--d-fade` (`120ms`), `--d-swap` (`160ms`), `--d-exit` (`200ms`), `--d-place` (`280ms`), `--d-sweep` (`480ms`). Raw millisecond values forbidden. |
| `REQ-071-04` | Cubic Bezier Curve Models | `--ease` = `cubic-bezier(0.25, 0.1, 0.25, 1.0)`, `--overshoot` = `cubic-bezier(0.2, 1.25, 0.3, 1.0)`. |
| `REQ-071-05` | Flash Sweep Opacity & Pulse Math | Opacity decay $A(t) = A_{\max} \cdot (1 - t/T_{\text{sweep}})^2$ ($A_{\max} = 0.35$); Aura pulse wave $E(r,t) = E_{\text{peak}} \exp\left(-\frac{(r - v_{\text{wave}} t)^2}{2\sigma^2}\right) (1 - t/T_{\text{sweep}})$. |
| `REQ-071-06` | Reduced Motion Compliance | When `prefers-reduced-motion` / `IsReducedMotionEnabled` is true, all durations collapse to `0ms`; flash sweep is bypassed and insertion occurs instantly. |
| `REQ-071-07` | Skia Custom Draw Operation | Class `FlashSweepDrawOperation : ICustomDrawOperation` executing Skia radial gradient sweep (`SKShader.CreateRadialGradient`) on Plane 0. |
| `REQ-071-08` | Animation Controller Class | Sealed class `LayerFeedbackAnimationController` with `TriggerLayerCreationFeedback()` and `BuildLayerRowInsertionAnimation()`. |

---

## 3. Codebase Reality & Line-by-Line Evidence

| Requirement ID | Codebase Symbol / Location | Implementation Status & Evidence |
| :--- | :--- | :--- |
| `REQ-071-01` | `src/GroveApp/` | **0% Implemented**: Plane 0 radial flash sweep animation is completely missing. Creating or switching layers in [`SpatialLayerStack.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/SpatialLayerStack.cs) triggers no radial wave on the canvas. |
| `REQ-071-02` | `src/GroveApp/` | **0% Implemented**: Plane 2 Layer Manager overlay row height expansion (`0px -> 36px`) animation is missing. |
| `REQ-071-03` | [`Motion.cs:10-24`](file:///C:/dev/grove-v9/src/GroveApp/DesignSystem/Motion.cs#L10-L24) | **IMPLEMENTED**: All duration constants (`PressDurationMs` 90, `FadeDurationMs` 120, `SwapDurationMs` 160, `ExitDurationMs` 200, `PlaceDurationMs` 280, `SweepDurationMs` 480) and `TimeSpan` properties exist. |
| `REQ-071-04` | [`Motion.cs:37-84`](file:///C:/dev/grove-v9/src/GroveApp/DesignSystem/Motion.cs#L37-L84) | **IMPLEMENTED**: `EvaluateCubicBezier`, `EvaluateEase` (0.25, 0.1, 0.25, 1.0), and `EvaluateOvershoot` (0.2, 1.25, 0.3, 1.0) exist and function correctly. |
| `REQ-071-05` | `src/GroveApp/` | **0% Implemented**: Flash sweep opacity decay $A(t)$ and radial energy wave equation $E(r,t)$ do **NOT** exist in any rendering file. |
| `REQ-071-06` | [`Motion.cs:30-32`](file:///C:/dev/grove-v9/src/GroveApp/DesignSystem/Motion.cs#L30-L32) | **IMPLEMENTED**: `IsReducedMotionEnabled` flag and `GetDurationMs()` correctly collapse token durations to 0.0 ms. |
| `REQ-071-07` | `src/GroveApp/` | **0% Implemented**: Class `FlashSweepDrawOperation` does **NOT** exist anywhere in the repository. |
| `REQ-071-08` | `src/GroveApp/` | **0% Implemented**: Class `LayerFeedbackAnimationController` does **NOT** exist anywhere in the repository. |

---

## 4. Standards & Visual Plane Seam Audit

- **Visual Plane Isolation (Plane 0 vs Plane 1 vs Plane 2)**:
  - Spec mandates dual-plane feedback execution: Plane 0 Skia custom draw operation for spatial canvas wave + Plane 2 Avalonia animation for Layer Manager overlay row insertion.
  - Codebase reality: Neither Plane 0 nor Plane 2 feedback mechanisms are wired in live application code.
- **Untriggered Motion Infrastructure**:
  - While `Motion.cs` contains math and token definitions, zero application code instantiates animations or dispatches draw operations when layers are created or activated.

---

## 5. Root Cause Analysis

### 5.1 Why Gaps Exist Between Claimed Status and Interactive UI Reality
1. **Foundation Tokens Written, Execution Classes Omitted**: The implementation team authored the design system token file (`Motion.cs`) containing duration values and cubic Bezier evaluators, but stopped short of creating the runtime animation execution classes (`FlashSweepDrawOperation` and `LayerFeedbackAnimationController`).
2. **Missing Integration in Layer Stack Events**: In [`SpatialLayerStack.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/SpatialLayerStack.cs) and [`GridCanvasControl.cs`](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs), layer creation and navigation events change integer layer IDs directly without raising or dispatching visual feedback animation requests to the rendering pipeline.
3. **Falsified Roadmap Status**: `ADR-ROADMAP.md` claimed ADR-071 was implemented via `Motion.cs` and `GridCanvasControl.cs`. In reality, only the token constants in `Motion.cs` were written, while 100% of the runtime visual feedback effects specified in ADR-071 remain missing.
