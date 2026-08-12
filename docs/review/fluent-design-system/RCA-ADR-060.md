# Root Cause Analysis (RCA) Ledger: ADR-060

| Property | Value |
| :--- | :--- |
| **ADR ID** | [ADR-060](file:///C:/dev/grove-v9/docs/specs/fluent-design-system/ADR-060-Fluent-Local-Editor-Notepad-Design.md) |
| **Title** | Fluent Local Editor Notepad Design |
| **Category** | Native Fluent Design (`fluent-design-system`) |
| **Claimed Status** | `IMPLEMENTED - AWAITING USER REVIEW` |
| **Verified Status** | `PARTIALLY IMPLEMENTED (NON-COMPLIANT LIVE UI)` |
| **Audit Date** | 2026-08-12 |

---

## 1. Executive Metadata & Audit Summary

- **Claimed Implementation Files**: [`LocalEditorOverlay.axaml.cs`](file:///C:/dev/grove-v9/src/GroveApp/Controls/LocalEditorOverlay.axaml.cs), [`QuickNoteOverlay.axaml.cs`](file:///C:/dev/grove-v9/src/GroveApp/Controls/QuickNoteOverlay.axaml.cs)
- **Verified Runtime Reality**: `LocalEditorOverlay` exists as an in-canvas `UserControl` overlay on Layer 1 of the spatial workspace. While it provides basic draft editing, tabbed document switching, and RAW/WYSIWYG toggling via `Ctrl+E`, it completely deviates from the normative WinUI 3 / Windows 11 Notepad frame topology mandated by ADR-060. It is **not** hosted inside a standalone `fa:AppWindow` / `FluentWindow` with DWM backdrops (`MicaAlt` / `DesktopAcrylic`), uses incorrect corner radii and border colors, omits mandatory C# viewmodel interfaces, and relies on ad-hoc buttons rather than FluentAvalonia `ui:Segmented` controls.

---

## 2. Normative Specification Requirement Inventory

| Requirement ID | Spec Requirement / Symbol Name | Target Specification Details |
| :--- | :--- | :--- |
| `REQ-060-01` | Window Topology Host | Hosted inside a standalone `fa:AppWindow` / `FluentWindow` with native DWM backdrops (`MicaAlt` or `DesktopAcrylic`). |
| `REQ-060-02` | Content Titlebar Extension | Native Fluent titlebar with seamless content extension (`ExtendsContentIntoTitleBar = true`) and lightweight tabbed navigation (`TabView`). |
| `REQ-060-03` | Frame Geometry & Radius | Standardized Fluent geometry: outer corner radius `--r-md` (`8px` / `CornerRadius="8"`). |
| `REQ-060-04` | Role Border Styling | Outer border stroke defined by role border `--k-edit-b` (`1px solid #7A3F3A`). |
| `REQ-060-05` | Reading Measure Constraint | Body reading column constrained by `--measure-reading` (`68ch` / `640px` / `MaxWidth="640"`). |
| `REQ-060-06` | Dual-Mode Editor Toggle | Zero-latency dual-mode Markdown engine featuring RAW monospace (`Cascadia Code`) and WYSIWYG (`Segoe UI Variable Text`) toggling via `Ctrl+E` or `ui:Segmented`. |
| `REQ-060-07` | Dual Viewport Sync Formula | Character offset mapping ratio $y_{\text{target}} = y_{\text{source}} \cdot \frac{H_{\text{target\_content}}}{H_{\text{source\_content}}}$. |
| `REQ-060-08` | C# 13 Types & Models | Namespace `Grove.UI.FluentDesign.LocalEditor`, Enum `LocalEditorMode`, Interface `ILocalEditorViewModel`, Record `LocalEditorConfig`, Window `LocalEditorWindow`. |
| `REQ-060-09` | Segmented Control UI | `ui:Segmented` control hosting `ui:SegmentedItem` ("RAW", "PREVIEW"). |

---

## 3. Codebase Reality & Line-by-Line Evidence

| Requirement ID | Codebase Symbol / Location | Implementation Status & Evidence |
| :--- | :--- | :--- |
| `REQ-060-01` | `LocalEditorOverlay.axaml:1-6` | **NON-COMPLIANT**: Implemented as `<UserControl>` overlaid directly inside `MainWindow.axaml` (line 19), NOT a standalone `fa:AppWindow` or `FluentWindow`. |
| `REQ-060-02` | `LocalEditorOverlay.axaml:23-86` | **PARTIAL**: Implemented as an in-control `Border` grid row (height 40px) simulating a titlebar header with tab strip, NOT a native DWM titlebar extension. |
| `REQ-060-03` | `LocalEditorOverlay.axaml:16` | **NON-COMPLIANT**: Uses `CornerRadius="{x:Static ds:Tokens.CornerRadiusSm}"` (`4px`), violating the normative `--r-md` (`8px`) spec contract. |
| `REQ-060-04` | `LocalEditorOverlay.axaml:14` | **NON-COMPLIANT**: Uses `BorderBrush="{x:Static ds:Colors.EdgeQuietBrush}"` (`#242428`), violating the normative role border `--k-edit-b` (`#7A3F3A`) stroke contract. |
| `REQ-060-05` | `LocalEditorOverlay.axaml:10` | **NON-COMPLIANT**: Hardcoded width `Width="580"`, missing the `--measure-reading` (`640px` / `68ch`) reading column constraint. |
| `REQ-060-06` | [`LocalEditorOverlay.axaml.cs:230-255`](file:///C:/dev/grove-v9/src/GroveApp/Controls/LocalEditorOverlay.axaml.cs#L230-L255) | **IMPLEMENTED**: `ToggleWysiwygMode()` toggles visibility between `TxtDraft` (TextBox) and `WysiwygScrollViewer` (`RichTextPreviewControl`) via `Ctrl+E` (lines 323–327). |
| `REQ-060-07` | `LocalEditorOverlay.axaml.cs` | **0% Implemented**: Dual viewport scroll ratio synchronization formula $y_{\text{target}} = y_{\text{source}} \cdot \frac{H_{\text{target}}}{H_{\text{source}}}$ is completely missing. Switching modes resets viewport scroll positions independently. |
| `REQ-060-08` | `src/GroveApp/` | **0% Implemented**: Namespace `Grove.UI.FluentDesign.LocalEditor`, Enum `LocalEditorMode`, Interface `ILocalEditorViewModel`, Record `LocalEditorConfig`, and class `LocalEditorWindow` do **NOT** exist anywhere in the repository. |
| `REQ-060-09` | `LocalEditorOverlay.axaml:61-83` | **NON-COMPLIANT**: Implemented using a custom `<Button Name="BtnModeToggle">` containing nested `TextBlock` elements rather than the normative `ui:Segmented` / `ui:SegmentedItem` FluentAvalonia control. |

---

## 4. Standards & Visual Plane Seam Audit

- **Visual Plane Isolation (Plane 0 vs Layer 1 vs Plane 2)**:
  - Spec mandates a windowed Notepad surface using `fa:AppWindow` anchored to Layer 1 of the visual plane hierarchy.
  - Codebase reality: `LocalEditorOverlay` is rendered as an in-canvas `UserControl` inside `MainWindow.axaml` (Layer 1 overlay). It translates pointer events directly back to the canvas when clicking outside bounds ([`LocalEditorOverlay.axaml.cs:403-456`](file:///C:/dev/grove-v9/src/GroveApp/Controls/LocalEditorOverlay.axaml.cs#L403-L456)).
- **Design System Tokens & Styling**:
  - `Tokens.CornerRadiusSm` (`4px`) is incorrectly applied to `FrameBorder` instead of `--r-md` (`8px`).
  - Color `#242428` is used for the outer border brush instead of `--k-edit-b` (`#7A3F3A`).
- **Code Smells & Architectural Drift**:
  - Tight coupling to `MainWindow` and `GridCanvasControl` direct event handlers rather than decoupled ViewModels adhering to `ILocalEditorViewModel`.
  - Missing strongly-typed `LocalEditorConfig` configuration record.

---

## 5. Root Cause Analysis

### 5.1 Why Gaps Exist Between Claimed Status and Interactive UI Reality
1. **Shortcut to Canvas Overlay vs Window System**: The implementation team opted to construct `LocalEditorOverlay` as an in-canvas Avalonia `UserControl` directly embedded inside `MainWindow` rather than creating a true standalone `AppWindow` / `FluentWindow`. This bypassed DWM backdrop composition, titlebar extension logic, and windowing management.
2. **Ad-hoc Custom Control vs FluentAvalonia Primitive Standard**: Instead of incorporating `FluentAvalonia.UI.Controls.Segmented` as specified in Section 5 of ADR-060, the developers built a basic `Button` with manual text updating (`WYSIWYG` vs `RAW`), ignoring Fluent Design System component standards.
3. **Omission of Specification Contracts**: The formal C# 13 contracts (`ILocalEditorViewModel`, `LocalEditorConfig`, `LocalEditorMode`) specified in Section 4 were never authored. The control logic was written directly inside code-behind (`LocalEditorOverlay.axaml.cs`), leading to missing scroll synchronization math and hardcoded frame bounds (`580px` vs `640px` / `68ch`).
