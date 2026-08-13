# Root Cause Analysis (RCA) Ledger: ADR-063

| Property | Value |
| :--- | :--- |
| **ADR ID** | [ADR-063](file:///C:/dev/grove-v9/docs/specs/fluent-design-system/ADR-063-FluentAvalonia-Mica-Acrylic-Backdrop-System.md) |
| **Title** | FluentAvalonia Mica and Acrylic Backdrop System |
| **Category** | Native Fluent Design (`fluent-design-system`) |
| **Claimed Status** | `IMPLEMENTED - AWAITING USER REVIEW` |
| **Verified Status** | `PARTIALLY IMPLEMENTED (NON-COMPLIANT LIVE UI)` |
| **Audit Date** | 2026-08-12 |

---

## 1. Executive Metadata & Audit Summary

- **Claimed Implementation Files**: [`MainWindow.axaml.cs`](file:///C:/dev/grove-v9/src/GroveApp/MainWindow.axaml.cs)
- **Verified Runtime Reality**: `MainWindow.axaml.cs` configures basic titlebar content extension (`TitleBar.ExtendsContentIntoTitleBar = true`) and sets Avalonia's built-in `TransparencyLevelHint` array to hint at Mica/Acrylic availability. However, the native Windows 11 Desktop Window Manager (DWM) interop layer and 4-tier glassmorphic elevation hierarchy specified in ADR-063 are **0% Implemented**. The classes `FluentWindowBackdropManager`, `NativeDwmApi`, enum `GroveBackdropType`, native Win32 `dwmapi.dll` P/Invoke calls (`DwmSetWindowAttribute`), optical blur and tint opacity scaling formulas ($R_{\text{blur}}(z) = 10 + 10z$, $\alpha_{\text{tint}}(z) = 0.85(1 - 0.12z)$), and FluentAvalonia backdrop controllers (`MicaController`, `AcrylicController`) do **NOT** exist anywhere in `src/GroveApp/`.

---

## 2. Normative Specification Requirement Inventory

| Requirement ID | Spec Requirement / Symbol Name | Target Specification Details |
| :--- | :--- | :--- |
| `REQ-063-01` | Material Standardization | Consume native Windows 11 DWM materials (`Mica`, `MicaAlt`, `DesktopAcrylic`, `InAppAcrylic`) provided by FluentAvalonia `MicaController` and `AcrylicController`. |
| `REQ-063-02` | Win32 DWM Immersive Dark Interop | Titlebar content extension (`ExtendsContentIntoTitleBar = true`) and theme state synchronization via Win32 DWM interop attributes (`DWMWA_USE_IMMERSIVE_DARK_MODE`). |
| `REQ-063-03` | Glassmorphic Elevation Hierarchy | Surface depths $z \in \{0, 1, 2, 3\}$ defining distinct backdrop materials ($z=0$ `MicaBase`, $z=1$ `MicaAlt`, $z=2$ `DesktopAcrylic`, $z=3$ `InAppAcrylic`). |
| `REQ-063-04` | Optical Blur & Tint Formulas | Blur radius $R_{\text{blur}}(z) = 10 + 10z$ DIPs and tint opacity $\alpha_{\text{tint}}(z) = 0.85 \cdot (1 - 0.12z)$ for elevation steps $z \in [0, 3]$. |
| `REQ-063-05` | Platform Fallback Integrity | Degrade gracefully on unsupported OS / software rendering to semi-transparent Avalonia acrylic brushes or solid dark surface fills (`#1E1E1E`). |
| `REQ-063-06` | Win32 DWM API P/Invoke Class | Class `NativeDwmApi` with `DwmSetWindowAttribute()`, `DWMWINDOWATTRIBUTE`, `DWM_SYSTEMBACKDROP_TYPE`, `DWM_WINDOW_CORNER_PREFERENCE`. |
| `REQ-063-07` | C# 13 Backdrop Manager Class | Sealed class `FluentWindowBackdropManager` in namespace `Grove.UI.FluentDesign.Backdrops` with enum `GroveBackdropType` and method `ApplyBackdrop()`. |

---

## 3. Codebase Reality & Line-by-Line Evidence

| Requirement ID | Codebase Symbol / Location | Implementation Status & Evidence |
| :--- | :--- | :--- |
| `REQ-063-01` | [`MainWindow.axaml.cs:33-39`](file:///C:/dev/grove-v9/src/GroveApp/MainWindow.axaml.cs#L33-L39) | **PARTIAL**: Standard Avalonia `TransparencyLevelHint` array is set (`Mica`, `AcrylicBlur`, `Blur`, `None`), but no FluentAvalonia `MicaController` or `AcrylicController` instances are constructed or configured. |
| `REQ-063-02` | [`MainWindow.axaml.cs:42-50`](file:///C:/dev/grove-v9/src/GroveApp/MainWindow.axaml.cs#L42-L50) | **PARTIAL**: `TitleBar.ExtendsContentIntoTitleBar = true` and caption button colors are set, but native `DWMWA_USE_IMMERSIVE_DARK_MODE` interop via `dwmapi.dll` is missing. |
| `REQ-063-03` | `src/GroveApp/` | **0% Implemented**: Surface material elevation hierarchy $z \in \{0, 1, 2, 3\}$ ($z=0$ `MicaBase`, $z=1$ `MicaAlt`, $z=2$ `DesktopAcrylic`, $z=3$ `InAppAcrylic`) is completely absent. |
| `REQ-063-04` | `src/GroveApp/` | **0% Implemented**: Mathematical formulas for $R_{\text{blur}}(z)$ and $\alpha_{\text{tint}}(z)$ do **NOT** exist in any file. |
| `REQ-063-05` | [`MainWindow.axaml:11`](file:///C:/dev/grove-v9/src/GroveApp/MainWindow.axaml#L11) | **PARTIAL**: Window background falls back to static `Background="{x:Static ds:Colors.SurfaceGridHex}"` (`#0E0E10`), but dynamic platform capability checks are absent. |
| `REQ-063-06` | `src/GroveApp/` | **0% Implemented**: Native Win32 class `NativeDwmApi`, `DwmSetWindowAttribute()`, `DWMWINDOWATTRIBUTE`, `DWM_SYSTEMBACKDROP_TYPE`, and `DWM_WINDOW_CORNER_PREFERENCE` do **NOT** exist anywhere in the repository. |
| `REQ-063-07` | `src/GroveApp/` | **0% Implemented**: Namespace `Grove.UI.FluentDesign.Backdrops`, class `FluentWindowBackdropManager`, and enum `GroveBackdropType` do **NOT** exist anywhere in the repository. |

---

## 4. Standards & Visual Plane Seam Audit

- **Visual Plane Isolation (Plane 0 vs Plane 1 vs Plane 2)**:
  - Spec mandates distinct glassmorphic material assignments across window planes ($z=0$ Main Grid Window `MicaBase`, $z=1$ Local Editor `MicaAlt`, $z=2$ HUD Slates `DesktopAcrylic`, $z=3$ Context Menus `InAppAcrylic`).
  - Codebase reality: All window surfaces render over flat hex backgrounds (`#0E0E10`, `#161618`) without DWM material distinction or optical blur separation.
- **Missing Win32 DWM Corner & Dark Mode Interop**:
  - Without calling `DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ...)` and `DWMWA_WINDOW_CORNER_PREFERENCE`, Windows 11 DWM non-client frame chrome does not properly adapt to the app's dark theme tokens.

---

## 5. Root Cause Analysis

### 5.1 Why Gaps Exist Between Claimed Status and Interactive UI Reality
1. **High-Level Avalonia Hints vs Native Win32 DWM P/Invoke**: The developers relied on Avalonia's built-in `TransparencyLevelHint` property in `MainWindow.axaml.cs` and assumed it fully satisfied the spec. They did not implement the native Win32 `dwmapi.dll` P/Invoke interop specified in Section 3 of ADR-063.
2. **Missing Backdrop Manager Subsystem**: Section 4 of ADR-063 defined `FluentWindowBackdropManager` and `GroveBackdropType` to orchestrate material selection across window depth steps $z \in [0,3]$. This manager class was never created, leaving secondary windows and HUD overlays without glassmorphic backdrops.
3. **Falsified Audit Reporting**: `ADR-ROADMAP.md` marked ADR-063 as `IMPLEMENTED - AWAITING USER REVIEW` based solely on 8 lines of code in `MainWindow.axaml.cs`, ignoring the entire normative DWM interop and elevation matrix specification.
