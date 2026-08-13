# RCA Ledger: ADR-042 — Spatial Layer State and Activation

## 1. Executive Metadata

| Metadata Field | Value |
| :--- | :--- |
| **ADR ID** | ADR-042 |
| **ADR Title** | Spatial Layer State and Activation |
| **Category** | Spatial Layers (`docs/specs/spatial-layers/`) |
| **Claimed Status in Spec Header** | IMPLEMENTED - AWAITING USER REVIEW |
| **Verified Status (User-Observable)** | **PARTIAL — activation, isolation, ghost presence, and modifier routing are wired; contract naming and full stack semantics remain** |
| **Audit Date** | 2026-08-12 |
| **Target Runtime** | C# 13 / .NET 9 / Avalonia 11.2.5 / SkiaSharp |
| **Auditor** | Principal AI Systems Architect & Product Auditor |

---

## 2. Normative Specification Requirement Inventory

| ID | Requirement Category | Symbol / Contract Name | Normative Specification Requirement |
| :--- | :--- | :--- | :--- |
| **REQ-042-01** | Layer State Machine | Active vs Inactive State | State machine enforcing exactly ONE active layer ($L_{\text{active}}$) rendering full content, while inactive layers ($L_{\text{inactive}}$) render presence heatmaps only. |
| **REQ-042-02** | Render Mode Enum | `LayerRenderMode` Enum | Mode enum (`ActiveFull`, `InactivePresenceOnly`, `IsolatedSolo`, `Hidden`). |
| **REQ-042-03** | Isolation Mode Rule | Layer Solo Mode ($\gamma = 0$) | When Isolation Mode is toggled on active layer $L_{\text{active}}$, $\gamma_{\text{inactive}} = 0.0$. Energy cast from inactive layers is suppressed. |
| **REQ-042-04** | Ghost Silhouettes | Ghost Footprint Specs | Inactive content items render a $1.0\text{px}$ hairline outline in authored hue at $\alpha = 0.15$ (15% opacity) with unpainted interior. |
| **REQ-042-05** | Service Interface | `ILayerActivationManager` | Interface declaring `ActiveLayerId`, `IsIsolationModeEnabled`, `ActivateLayer`, `ActivateNextLayer`, `ActivatePreviousLayer`, `ToggleIsolationMode`, `GetRenderMode`. |
| **REQ-042-06** | Keybind: Navigation | `[` / `]` Keys | Key `[` activates previous layer down; key `]` activates next layer up. |
| **REQ-042-07** | Keybind: Add Stack | `Shift+[` / `Shift+]` Keys | `Shift+[` creates layer at stack bottom (`B0x`); `Shift+]` creates layer at stack top (`0x`). |
| **REQ-042-08** | Keybind: Insert Layer| `Ctrl+[` / `Ctrl+]` Keys | `Ctrl+[` creates layer below current; `Ctrl+]` creates layer above current. |
| **REQ-042-09** | Keybind: Reorder Stack| `Alt+[` / `Alt+]` Keys | `Alt+[` reorders layer down; `Alt+]` reorders layer up. Stable labels remain invariant. |
| **REQ-042-10** | Keybind: Isolation | `Ctrl+I` / `Cmd+I` Key | Keyboard shortcut toggling Layer Isolation (Solo) Mode on active layer. |

---

## 3. Codebase Reality & Line-by-Line Evidence

### 3.1 Symbol Existence & Implementation Audit

| Symbol / Contract Name | Expected Location | Actual Status in `src/GroveApp/` | Line-by-Line Evidence |
| :--- | :--- | :--- | :--- |
| `LayerRenderMode` Enum | `src/GroveApp/Engine/LayerActivationManager.cs` | **Implemented** | Selects active, inactive, isolated, and hidden render modes. |
| `ILayerActivationManager` | `src/GroveApp/Engine/LayerActivationManager.cs` | **Implemented** | Owns isolation state and render-mode resolution. |
| `LayerActivationManager` | `src/GroveApp/Engine/LayerActivationManager.cs` | **Implemented** | Bound to layer-stack changes and canvas refresh. |
| Layer Isolation (Solo) Mode| `src/GroveApp/Engine/LayerActivationManager.cs` | **Implemented** | `Ctrl+I` suppresses inactive aura and content rendering. |
| Ghost Silhouettes Pass | `src/GroveApp/Engine/NoteRenderModule.cs` | **Implemented** | Inactive content renders a 15% discrete outline through `GridCanvasRenderPipeline`. |
| Modifier Layer Shortcuts | `src/GroveApp/Engine/KeybindModule.cs` | **Implemented** | Shift bracket jumps, Alt arrows reorder, and Ctrl+I isolates the active layer. |

---

## 4. Standards & Visual Plane Seam Audit

### 4.1 Visual Plane Separation (Plane 0 vs Plane 1 vs Plane 2)
- **Plane 0 (Spatial Grid Canvas)**: Inactive content remains non-interactive and renders discrete 15% outlines; inactive aura presence remains available outside isolation mode.
- **Key Routing**: `KeybindModule` routes navigation, isolation, insertion, and reorder actions through `IKeybindHost`.

### 4.2 Code Smells & Architectural Violations
1. **Contract Drift**: The original review predates the activation and feedback seams now present in source.
2. **Remaining Gap**: The public activation contract uses `ToggleIsolationMode` and `GetRenderMode`; it does not expose alternate `ActivateNextLayer` method names from the draft.

---

## 5. Root Cause Analysis (RCA)

### 5.1 Primary Root Cause
ADR-042 is partially implemented. The activation state machine and visual feedback exist, but the review ledger was not updated after those seams were added.

### 5.2 Failure Chain
1. **Stale Review Evidence**: The ledger described an earlier source state.
2. **Contract Naming Drift**: The implemented service uses the current engine vocabulary rather than the draft method names.
