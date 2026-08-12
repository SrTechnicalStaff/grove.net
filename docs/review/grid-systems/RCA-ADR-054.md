# Root Cause Analysis (RCA) Ledger: ADR-054

## 1. Executive Metadata

| Property | Value |
| :--- | :--- |
| **ADR ID** | `ADR-054` |
| **Title** | Spatial Context Menu System and Zero-Modal Pass-Through Architecture |
| **Category** | Grid Systems (`docs/specs/grid-systems/`) |
| **Claimed Spec Status** | `IMPLEMENTED - AWAITING USER REVIEW` |
| **Verified Interactive Status** | `0% IMPLEMENTED (MISSING SUB-SYSTEM)` |
| **Audit Date** | 2026-08-12 |
| **Target Runtime** | C# 13 / .NET 9 / Avalonia 11.2.5 / SkiaSharp 3.x |
| **Authors** | Chief Product Definition Architect & AI Systems Audit Team |

---

## 2. Normative Specification Requirement Inventory

The table below catalogs every explicit architectural requirement, mathematical formulation, interface contract, and visual token specified in [ADR-054](file:///C:/dev/grove-v9/docs/specs/grid-systems/ADR-054-Spatial-Context-Menu-System.md):

| ID | Requirement Category | Normative Spec Requirement | Target Symbol / Token |
| :--- | :--- | :--- | :--- |
| **REQ-054-1** | Service Contract | `ISpatialContextMenuService` interface managing active menu state, opening, closing, pointer pass-through processing, and state change events. | `Grove.HUD.ContextMenu.ISpatialContextMenuService` |
| **REQ-054-2** | Data Contracts | `ContextMenuTargetType` enum (`EmptyCell`=0, `SinglePlacement`=1, `MultiSelection`=2), `ContextMenuTargetContext`, `ContextMenuItemViewModel`, `SpatialContextMenuModel`. | `ContextMenuTargetType`, `SpatialContextMenuModel` |
| **REQ-054-3** | UI Overlay Control | Plane 2 `SpatialContextMenuOverlayView` XAML control rendered in `OverlayLayer` canvas (`Width="220"`, `Background="#1C1C1E"`, `BorderBrush="#2C2C2E"`). | `Grove.HUD.Views.SpatialContextMenuOverlayView` |
| **REQ-054-4** | Zero-Modal Pass-Through | Zero backdrop scrim (`#000000` opacity = 0%) and zero canvas blur. Pointer events outside menu frame pass through directly to Plane 0. | Zero-Modal Pass-Through Architecture |
| **REQ-054-5** | Context-Sensitive Menu Models | Context evaluation generating targeted menu items: Empty Field (`N` Note, Document, `Ctrl+V` Paste), Single Item (`A` Anchor, `1` Trace, `Ctrl+C` Copy, `Del` Delete), Multi-Selection. | Context-Sensitive Target Resolution |
| **REQ-054-6** | Viewport Clamping Math | Position clamping algorithm $P_{\text{menu}} = (x_m, y_m)$ preventing popup clipping at screen bounds $[0, 0, W_v, H_v]$. | Screen Position Clamping Algorithm |
| **REQ-054-7** | Focus Return | Dismissing context menu automatically restores keyboard attention to Grid Cursor at `AddressedCell`. | Deterministic Focus Return |

---

## 3. Codebase Reality & Line-by-Line Evidence

### 3.1 Implemented & Partially Implemented Symbols

- **None**. Zero symbols, interfaces, or controls specified in ADR-054 exist in the codebase.

### 3.2 0% Implemented & Deviated Symbols

- **`ISpatialContextMenuService` Interface**: **0% Implemented**. Missing namespace `Grove.HUD.ContextMenu` and interface `ISpatialContextMenuService`.
- **Data Contracts (`SpatialContextMenuModel`, `ContextMenuItemViewModel`, `ContextMenuTargetContext`)**: **0% Implemented**. Missing all data records.
- **`SpatialContextMenuOverlayView.axaml`**: **0% Implemented**. File does not exist anywhere under `src/GroveApp/`.
- **Right-Click Interaction Seam**: **0% Implemented**.
  - In `GridCanvasControl.cs` [L499](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs#L499), right-click pointer events are intercepted ONLY to cancel active tool arming (`Arming.Disarm()`) or stop canvas panning (`_isPanning = false`).
  - Right-clicking empty canvas or selected items does NOT trigger any context menu opening logic.
- **Zero-Modal Pass-Through Canvas**: **0% Implemented**. No HUD context menu overlay exists on Plane 2.
- **Viewport Clamping Algorithm**: **0% Implemented**.

---

## 4. Standards & Visual Plane Seam Audit

1. **Plane 2 (HUD Plane) Integration**:
   - The specification mandates that context menus must be hosted on Plane 2 (`OverlayLayer` canvas) above Plane 0 (Grid Canvas) and Layer 1 (Editors). Plane 2 currently contains no context menu overlay views.
2. **Design System Tokens Alignment**:
   - Palette colors `#1C1C1E` (Frame Background), `#2C2C2E` (Border), `#F4F4F2` (Header Text), and `#8E8E93` (Gesture Text) were specified for context menu items but have 0% presence in XAML or C#.
3. **Code Smells & Architectural Violations**:
   - Right-click mouse events in `GridCanvasControl` swallow right-clicks as disarm/cancel triggers without delegating to a context menu service.

---

## 5. Root Cause Analysis (RCA)

### Why claimed status (`IMPLEMENTED - AWAITING USER REVIEW`) diverges from actual interactive reality:

1. **Total Subsystem Omission**:
   - The entire spatial context menu subsystem was completely omitted during codebase construction. Despite 0 lines of context menu code being written, the ADR spec status was incorrectly set to "IMPLEMENTED".
2. **Right-Click Pointer Hijacking**:
   - Pointer handlers in `GridCanvasControl` treated right-clicks exclusively as a gesture cancellation mechanism (`Disarm()`), preventing right-click events from reaching menu routing logic.
3. **Unbuilt Plane 2 HUD Overlays**:
   - Plane 2 overlay infrastructure was partially built for HUD bars and telemetry, but overlay containers for context menus and popup slates were skipped.
