# Root Cause Analysis (RCA) Ledger: ADR-058

## 1. Executive Metadata

| Property | Value |
| :--- | :--- |
| **ADR ID** | `ADR-058` |
| **Title** | Global Keybind Focus Precedence Router Architecture |
| **Category** | Keybind Arming (`docs/specs/keybind-arming/`) |
| **Claimed Spec Status** | `IMPLEMENTED - AWAITING USER REVIEW` |
| **Verified Interactive Status** | `PARTIALLY IMPLEMENTED (DEVIATED SEAM)` |
| **Audit Date** | 2026-08-12 |
| **Target Runtime** | C# 13 / .NET 9 / Avalonia 11.2.5 / SkiaSharp 3.x |
| **Authors** | Chief Product Definition Architect & AI Systems Audit Team |

---

## 2. Normative Specification Requirement Inventory

The table below catalogs every explicit architectural requirement, mathematical formulation, interface contract, and visual token specified in [ADR-058](file:///C:/dev/grove-v9/docs/specs/keybind-arming/ADR-058-Global-Keybind-Focus-Precedence-Router.md):

| ID | Requirement Category | Normative Spec Requirement | Target Symbol / Token |
| :--- | :--- | :--- | :--- |
| **REQ-058-1** | Service Contract | `IFocusPrecedenceRouter` interface managing `CurrentContext`, `FocusContextChanged`, `KeybindRouted`, `RouteKeyEvent`, `ForcePlane0CanvasFocus`. | `Grove.Input.Precedence.IFocusPrecedenceRouter` |
| **REQ-058-2** | Data Contracts | `FocusPrecedenceLevel` enum (`FocusedTextBox`=0, `InformationOverlay`=1, `HUDPlane`=2, `Plane0Canvas`=3). | `FocusPrecedenceLevel` |
| **REQ-058-3** | Data Contracts | `KeyCombination` struct, `KeybindHandlingResult` enum (`Ignored`, `ConsumedByFocusedControl`, `DispatchedToSpatialGrid`, ...), `FocusContextInfo` record. | `KeyCombination`, `KeybindHandlingResult`, `FocusContextInfo` |
| **REQ-058-4** | Attached Properties | `FocusPrecedenceAttachedProperties` static class with `PrecedenceLevelProperty` attached property for Avalonia controls. | `FocusPrecedenceAttachedProperties` |
| **REQ-058-5** | Priority Pyramid | 4-tier precedence evaluation (Level 0 `FocusedTextBox` $\rightarrow$ Level 1 `InformationOverlay` $\rightarrow$ Level 2 `HUDPlane` $\rightarrow$ Level 3 `Plane0Canvas`). | 4-Tier Focus Context Precedence |
| **REQ-058-6** | Text Editing Isolation | When focused in text input (`FocusedTextBox`), single-key spatial hotkeys (`N`, `Shift+N`, `D`, `A`, `Space`, `Del`) MUST NOT activate spatial tools. | Strict Text Editing Isolation |
| **REQ-058-7** | Key Routing Matrix | Comprehensive matrix handling `Spacebar`, `Ctrl+Enter`, `Ctrl+E`, `Tab`, `N`, `Shift+N`, `D`, `A`, `Del`, `Esc`, `Ctrl+C`, `Ctrl+V`. | Comprehensive Keybind Routing Matrix |
| **REQ-058-8** | Tunneling Key Interception | Intercept key events at window root during `RoutingStrategies.Tunnel` phase before element-focused bubbling phase. | Tunneling-Phase Key Routing |

---

## 3. Codebase Reality & Line-by-Line Evidence

### 3.1 Implemented & Partially Implemented Symbols

- **`GlobalFocusPrecedenceRouter.cs`**:
  - Located at [`Engine/GlobalFocusPrecedenceRouter.cs:L8-L130`](file:///C:/dev/grove-v9/src/GroveApp/Engine/GlobalFocusPrecedenceRouter.cs#L8-L130).
  - Implements `FocusPrecedenceLevel` enum (`FocusedTextBox`=0, `InformationOverlay`=1, `HudPlane`=2, `Plane0Canvas`=3) [L8-L14](file:///C:/dev/grove-v9/src/GroveApp/Engine/GlobalFocusPrecedenceRouter.cs#L8-L14).
  - Implements `KeyCombination` struct [L16-L19](file:///C:/dev/grove-v9/src/GroveApp/Engine/GlobalFocusPrecedenceRouter.cs#L16-L19).
  - Implements `FocusContextInfo` record [L21-L23](file:///C:/dev/grove-v9/src/GroveApp/Engine/GlobalFocusPrecedenceRouter.cs#L21-L23).
  - Implements `IFocusPrecedenceRouter` interface [L25-L31](file:///C:/dev/grove-v9/src/GroveApp/Engine/GlobalFocusPrecedenceRouter.cs#L25-L31).
  - Registers tunneling key handler `_rootWindow.AddHandler(InputElement.KeyDownEvent, OnWindowKeyDownTunnel, RoutingStrategies.Tunnel)` [L61](file:///C:/dev/grove-v9/src/GroveApp/Engine/GlobalFocusPrecedenceRouter.cs#L61).

### 3.2 0% Implemented & Deviated Symbols

- **`KeybindHandlingResult` Enum**: **0% Implemented**. Missing enum `KeybindHandlingResult` (`Ignored`, `ConsumedByFocusedControl`, `DispatchedToSpatialGrid`, `DispatchedToOverlay`, `DispatchedToHUD`).
- **`FocusPrecedenceAttachedProperties` Static Class**: **0% Implemented**. Missing static class and attached property `PrecedenceLevelProperty`. Visual controls cannot declare their precedence level explicitly via XAML attached properties.
- **Incomplete Routing Matrix**:
  - In `GlobalFocusPrecedenceRouter.cs` [L84-L95](file:///C:/dev/grove-v9/src/GroveApp/Engine/GlobalFocusPrecedenceRouter.cs#L84-L95), the router only evaluates `Key.N`, `Key.D`, and `Key.Escape`.
  - Keys `Spacebar`, `Ctrl+Enter`, `Ctrl+E`, `Tab`, `A`, `Del`/`Backspace`, `Ctrl+C`, `Ctrl+V` are completely ignored by `RouteKeyEvent` during the Tunneling phase and bypass the router.
- **Deviated Delegation Architecture**:
  - In `GlobalFocusPrecedenceRouter.cs` [L42, L86](file:///C:/dev/grove-v9/src/GroveApp/Engine/GlobalFocusPrecedenceRouter.cs#L42, L86), the router relies on a delegate `Func<KeyCombination, bool> _routeSpatialKey` supplied by `MainWindow.axaml.cs` instead of maintaining native key handling rules.
- **Missing Record Properties**:
  - `FocusContextInfo` [L21-L23](file:///C:/dev/grove-v9/src/GroveApp/Engine/GlobalFocusPrecedenceRouter.cs#L21-L23) is missing `IsTextEditingActive` (bool) and `ActiveLayerId` (Guid?) properties mandated by spec Section 4.

---

## 4. Standards & Visual Plane Seam Audit

1. **Input Pipeline Seam**:
   - The precedence router correctly attaches to Avalonia's `RoutingStrategies.Tunnel` phase on the root window. However, because it only filters 3 keys (`N`, `D`, `Esc`), other spatial keys bubble directly down to controls, leading to key leakage.
2. **Design System & Focus Management**:
   - Without `FocusPrecedenceAttachedProperties`, HUD panels on Plane 1 and overlays on Plane 2 cannot declare their precedence level dynamically, forcing hardcoded `Func<bool>` boolean callbacks in constructor.
3. **Code Smells & Architectural Violations**:
   - `GlobalFocusPrecedenceRouter` is tightly coupled to delegate lambdas passed from `MainWindow.axaml.cs` rather than subscribing directly to engine services (`IToolArmingService`, `ISelectionService`).

---

## 5. Root Cause Analysis (RCA)

### Why claimed status (`IMPLEMENTED - AWAITING USER REVIEW`) diverges from actual interactive reality:

1. **Partial Key Filtering Scope**:
   - The router was created to solve the specific bug of typing `N` or `D` inside text boxes triggering tool arming. Once `N`, `D`, and `Esc` were wired up, development stopped, leaving the remaining 8 keys in the spec matrix unrouted.
2. **Omission of Attached Properties**:
   - Avalonia attached properties require registration boilerplate (`AvaloniaProperty.RegisterAttached`). The author opted for simple lambda callbacks (`_isInformationOverlayVisible()`) to quickly check overlay visibility, bypassing attached property infrastructure.
3. **Missing Handling Result Metrics**:
   - `KeybindHandlingResult` telemetry events were omitted because telemetry tracking for key routing was not connected to HUD status indicators on Plane 2.
