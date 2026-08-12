# Root Cause Analysis (RCA) Ledger: ADR-070

| Property | Value |
| :--- | :--- |
| **ADR ID** | [ADR-070](file:///C:/dev/grove-v9/docs/specs/hud-feedback/ADR-070-HUD-Spatial-Watermark-And-Layer-Identity.md) |
| **Title** | HUD Spatial Watermark and Active Layer Identity |
| **Category** | HUD Feedback (`hud-feedback`) |
| **Claimed Status** | `IMPLEMENTED - AWAITING USER REVIEW` |
| **Verified Status** | `NOT IMPLEMENTED / 0% LIVE UI` |
| **Audit Date** | 2026-08-12 |

---

## 1. Executive Metadata & Audit Summary

- **Claimed Implementation Files**: [`GridCanvasControl.cs`](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs)
- **Verified Runtime Reality**: The bottom-right HUD Spatial Watermark and Active Layer Identity control specified by ADR-070 is **0% Implemented**. The C# 13 services and controls (`ISpatialWatermarkService`, `HudWatermarkViewModel`, `HudSpatialWatermarkControl`), structural layout elements W-01 through W-07 (brand token `GROVE v9`, scale readout `100%`, active layer label token `01`, layer display name, inline rename field `PART_RenameTextBox`), pointer passthrough rules, and double-click / `F2` inline renaming state machine do **NOT** exist anywhere in `src/GroveApp/`. What actually exists is a simple, static horizontal telemetry bar embedded at the bottom of [`MainWindow.axaml:24-77`](file:///C:/dev/grove-v9/src/GroveApp/MainWindow.axaml#L24-L77) rendering static strings (`CELL: (0, 0)`, `ZOOM: 100%`, `NOTES: 3`, `LEDGER METADATA: 0`).

---

## 2. Normative Specification Requirement Inventory

| Requirement ID | Spec Requirement / Symbol Name | Target Specification Details |
| :--- | :--- | :--- |
| `REQ-070-01` | Plane 2 HUD Anchoring | Positioned in bottom-right corner of Plane 2 HUD Overlay (`Right: 16px`, `Bottom: 16px`) at `ZIndex = 300`, unscaled by camera transform $T(x,y,s)$. |
| `REQ-070-02` | Pointer Passthrough Rules | Non-interactive elements (W-02 brand `GROVE v9`, W-03 zoom readout) are `IsHitTestVisible = false` (pass clicks to canvas); active layer pill is interactive. |
| `REQ-070-03` | W-02 Brand Token | `56px` fixed width, `--f-mono` 500 9px (`--t-micro`), `--tr-wide` (0.16em), `--ink-whisper` (`0.04`). |
| `REQ-070-04` | W-03 Camera Scale Readout | `44px` fixed width, `--f-mono` 500 9px (`--t-micro`), `--tr-mono` (0.08em), `--ink-tertiary` (`0.51`). |
| `REQ-070-05` | W-05 Active Layer Label Token | `32px` fixed width, `--f-mono` 500 11px (`--t-label`), `--tr-mono` (0.08em), `--k-layer` (`#E2A6C6`), border `--k-layer-b` (`#8A3F63`). |
| `REQ-070-06` | W-06 Active Layer Name Block | Auto width (`120px`–`240px`), `--f-ui` 400 13px (`--t-dense`), `--ink-primary` (`0.82`). Double-click or `F2` triggers inline rename. |
| `REQ-070-07` | W-07 Inline Rename Field | Overlays W-06, `--f-ui` 400 13px (`--t-dense`), background `--surface-nested` (`#1C1C20`), border `--edge-found`. Auto-selects text on focus. |
| `REQ-070-08` | Inline Rename State Machine | State machine: Rest -> Edit -> Committed / Cancelled. Duplicate name validation refusal (`--c-invalid`), `Enter` commit, `Escape` cancel. |
| `REQ-070-09` | C# 13 Types & Services | Namespace `Grove.Core.Spatial` & `Grove.UI.Controls`, records `SpatialCameraState`, `SpatialLayerIdentity`, `LayerRenameResult`, service `ISpatialWatermarkService`, ViewModel `HudWatermarkViewModel`, control `HudSpatialWatermarkControl`. |

---

## 3. Codebase Reality & Line-by-Line Evidence

| Requirement ID | Codebase Symbol / Location | Implementation Status & Evidence |
| :--- | :--- | :--- |
| `REQ-070-01` | [`MainWindow.axaml:25-28`](file:///C:/dev/grove-v9/src/GroveApp/MainWindow.axaml#L25-L28) | **NON-COMPLIANT**: Implemented as a full-width static `Border` height 32px (`HorizontalAlignment="Stretch"`), NOT a bottom-right viewport cell margin control (`Right: 16px`, `Bottom: 16px`) at `ZIndex = 300`. |
| `REQ-070-02` | `MainWindow.axaml:29-76` | **NON-COMPLIANT**: Pointer hit-testing policy is unmanaged; no `IsHitTestVisible="False"` passthrough rules are defined for brand token or scale readout. |
| `REQ-070-03` | `src/GroveApp/` | **0% Implemented**: Brand Token W-02 (`GROVE v9`, `--f-mono` 9px, `--ink-whisper`) does **NOT** exist anywhere in the user interface. |
| `REQ-070-04` | [`MainWindow.axaml:42-46`](file:///C:/dev/grove-v9/src/GroveApp/MainWindow.axaml#L42-L46) | **PARTIAL**: Scale readout is rendered as static text `ZOOM: 100%` inside a horizontal telemetry strip, missing W-03 tokens and formatting specifications. |
| `REQ-070-05` | `src/GroveApp/` | **0% Implemented**: W-05 Active Layer Label Token (`01`, `#E2A6C6`, border `#8A3F63`) does **NOT** exist anywhere in the user interface. |
| `REQ-070-06` | `src/GroveApp/` | **0% Implemented**: W-06 Active Layer Name Block (`Layer 01 - Working Surface`) does **NOT** exist anywhere in the user interface. |
| `REQ-070-07` | `src/GroveApp/` | **0% Implemented**: W-07 Inline Rename Field (`PART_RenameTextBox`) does **NOT** exist. Double-clicking or pressing `F2` on layer telemetry does nothing. |
| `REQ-070-08` | `src/GroveApp/` | **0% Implemented**: Inline renaming state machine, duplicate name validation refusal (`--c-invalid`), `Enter` commit, and `Escape` cancel do **NOT** exist. |
| `REQ-070-09` | `src/GroveApp/` | **0% Implemented**: `SpatialCameraState`, `SpatialLayerIdentity`, `LayerRenameResult`, `ISpatialWatermarkService`, `HudWatermarkViewModel`, and `HudSpatialWatermarkControl` do **NOT** exist anywhere in the repository. |

---

## 4. Standards & Visual Plane Seam Audit

- **Visual Plane Isolation (Plane 0 vs Layer 1 vs Plane 2)**:
  - Spec mandates a dedicated Plane 2 HUD Overlay control (`ZIndex = 300`) anchored to bottom-right viewport coordinates (`Right: 16px`, `Bottom: 16px`).
  - Codebase reality: A full-width `Border` (height 32px) is stretched across the bottom of `MainWindow.axaml` as a static status bar.
- **Interactive Layer Identity Seam Failure**:
  - The watermark is intended to serve as a live reactive link to `ISpatialLayerStateService.ActiveLayer`, allowing users to rename layers directly from the HUD. The existing telemetry strip has zero layer awareness and zero interactivity.

---

## 5. Root Cause Analysis

### 5.1 Why Gaps Exist Between Claimed Status and Interactive UI Reality
1. **Placeholder Telemetry Strip Substituted for Watermark Control**: Instead of building the dedicated `HudSpatialWatermarkControl` and its underlying `ISpatialWatermarkService` / `HudWatermarkViewModel` infrastructure, the team left a static 32px bottom telemetry strip in `MainWindow.axaml`.
2. **Missing Active Layer Identity & Inline Editor**: Sections 4, 5, and 6 of ADR-070 specified a complete inline renaming state machine with text selection, duplicate name validation refusal, and layer state stream synchronization. None of this domain logic or UI control templates were authored.
3. **Falsified Audit Status**: `ADR-ROADMAP.md` claimed ADR-070 was implemented via `GridCanvasControl.cs`. In truth, `GridCanvasControl.cs` contains zero watermark control code, and `MainWindow.axaml` contains only static telemetry text.
