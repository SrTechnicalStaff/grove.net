# RCA Ledger: ADR-032 — Spatial Layer Manager and Navigation Specifications

## 1. Executive Metadata

| Metadata Field | Value |
| :--- | :--- |
| **ADR ID** | ADR-032 |
| **ADR Title** | Spatial Layer Manager and Navigation Specifications |
| **Category** | HUD System (`docs/specs/hud-system/`) |
| **Claimed Status in Spec Header** | Accepted |
| **Verified Status (User-Observable)** | **PARTIALLY IMPLEMENTED (15% Implemented, Non-Functional UI / Broken Seams)** |
| **Audit Date** | 2026-08-12 |
| **Target Runtime** | C# 13 / .NET 9 / Avalonia 11.2.5 |
| **Auditor** | Principal AI Systems Architect & Product Auditor |

---

## 2. Normative Specification Requirement Inventory

| ID | Requirement Category | Symbol / Contract Name | Normative Specification Requirement |
| :--- | :--- | :--- | :--- |
| **REQ-032-01** | Stack Model | Unbounded Layer Stack | Unbounded spatial depth stack ($L_0, L_1, \dots, L_n$) with permanent base layer `01`. |
| **REQ-032-02** | Side-Coded Labeling | Stable Label Tokens | Minting stable side-coded labels (`01`, `02`, `03` above base layer; `B01`, `B02` below base layer) that remain fixed during layer reordering. |
| **REQ-032-03** | Render Policy: Active | Active Layer Full Render | Active layer renders full content footprints (text, images, interactive controls, selection marquees). |
| **REQ-032-04** | Render Policy: Inactive | Inactive Presence Render | Inactive layers render presence heatmaps and aura isoline fields ONLY. Content frames and text details are suppressed. |
| **REQ-032-05** | Energy Decay Formula | Vertical Decay $\gamma = 0.5$ | Vertical aura saturation formula $E_i(d, \Delta L) = \frac{M_i}{1 + 0.4 d^2} \cdot (0.5)^{|\Delta L|}$. |
| **REQ-032-06** | Navigation Keybind: Base | `[` / `]` Keys | Key `[` traverses active layer down; key `]` traverses active layer up. |
| **REQ-032-07** | Navigation Keybind: Add | `Shift+[` / `Shift+]` Keys | `Shift+[` creates layer at bottom of stack (`B0x`); `Shift+]` creates layer at top of stack (`0x`). |
| **REQ-032-08** | Navigation Keybind: Insert| `Ctrl+[` / `Ctrl+]` Keys | `Ctrl+[` creates layer below active; `Ctrl+]` creates layer above active. |
| **REQ-032-09** | Navigation Keybind: Reorder| `Alt+[` / `Alt+]` Keys | `Alt+[` reorders active layer down; `Alt+]` reorders active layer up. Preserves stable labels. |
| **REQ-032-10** | Layer Removal Flow | Content Migration & Refusal | Deleting a layer migrates content footprints to adjacent layer. If destination cells are occupied, triggers refusal `"That space is occupied on Layer {dest}."`. |

---

## 3. Codebase Reality & Line-by-Line Evidence

### 3.1 Symbol Existence & Implementation Audit

| Symbol / Contract Name | Expected Location | Actual Status in `src/GroveApp/` | Line-by-Line Evidence |
| :--- | :--- | :--- | :--- |
| `SpatialLayerStack` | `src/GroveApp/Engine/` | **PARTIALLY IMPLEMENTED** | Implemented in [`SpatialLayerStack.cs:L28-L165`](file:///C:/dev/grove-v9/src/GroveApp/Engine/SpatialLayerStack.cs#L28). Has basic `AddLayerAbove`, `AddLayerBelow`, `SetActiveLayer`, `Navigate`. |
| `ISpatialLayerPermeability` | `src/GroveApp/Engine/` | **PARTIALLY IMPLEMENTED** | Implemented in [`SpatialLayerStack.cs:L17-L22`](file:///C:/dev/grove-v9/src/GroveApp/Engine/SpatialLayerStack.cs#L17). `GetPermeability` calculates `Math.Pow(0.5, Math.Abs(source - target))`. |
| `[` / `]` Keybinds | `src/GroveApp/Engine/` | **PARTIALLY IMPLEMENTED** | Implemented in [`KeybindModule.cs:L271-L282`](file:///C:/dev/grove-v9/src/GroveApp/Engine/KeybindModule.cs#L271). Calls `canvas.NavigateLayer(-1)` and `(1)`. |
| `Shift+[` / `Shift+]` | `src/GroveApp/Engine/` | **0% Implemented (MISSING KEYBIND)** | 0 occurrences in `KeybindModule.cs`. |
| `Ctrl+[` / `Ctrl+]` | `src/GroveApp/Engine/` | **0% Implemented (MISSING KEYBIND)** | 0 occurrences in `KeybindModule.cs`. |
| `Alt+[` / `Alt+]` | `src/GroveApp/Engine/` | **0% Implemented (MISSING KEYBIND)** | 0 occurrences in `KeybindModule.cs`. |
| `L` Keybind (Toggle Manager) | `src/GroveApp/Engine/` | **0% Implemented (MISSING KEYBIND)** | 0 occurrences in `KeybindModule.cs`. |
| Placement Migration Engine | `src/GroveApp/Engine/` | **0% Implemented (MISSING SYMBOL)** | No placement migration or cell collision checking exists on layer removal. |
| Layer Manager HUD Slate UI | `src/GroveApp/Controls/` | **0% Implemented (MISSING SYMBOL)** | No Plane 2 HUD Slate for Layer Manager exists. |

---

## 4. Standards & Visual Plane Seam Audit

### 4.1 Visual Plane Separation (Plane 0 vs Layer 1 vs Plane 2)
- **Plane 0**: `GridCanvasControl` references `SpatialLayerStack` to switch active layers, but inactive layers do NOT render presence heatmaps or isoline fields according to the $\gamma=0.5$ attenuation formula.
- **Plane 2**: The Layer Manager HUD Slate UI on Plane 2 is 100% missing. Users cannot visually manage, reorder, or inspect layers in a Slate interface.

### 4.2 Code Smells & Architectural Violations
1. **Incomplete Keyboard Navigation**: Only un-modified `[` and `]` are handled in `KeybindModule.cs`. All modifier shortcuts (`Shift`, `Ctrl`, `Alt`) are ignored.
2. **Missing Layer Removal Validation**: Deleting a layer has no collision validation or refusal prompt.

---

## 5. Root Cause Analysis (RCA)

### 5.1 Primary Root Cause
The core backend data structure `SpatialLayerStack.cs` was written with basic layer insertion and navigation methods, but the HUD UI controls (Plane 2 Layer Manager Slate), extended modifier keybinds (`Shift`/`Ctrl`/`Alt`), and content migration engine were never built.

### 5.2 Failure Chain
1. **Partial Backend Delivery**: `SpatialLayerStack.cs` provided minimal layer storage, but was never expanded to support placement migration or cell collision checking.
2. **Keybind Omission**: Modifier key handling in `KeybindModule.cs` was left incomplete.
3. **Missing UI**: No Avalonia control was created for the Layer Manager Slate on Plane 2.
