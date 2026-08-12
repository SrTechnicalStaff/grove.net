# RCA Ledger: ADR-043 — Spatial Layer Manager UI and Controls

## 1. Executive Metadata

| Metadata Field | Value |
| :--- | :--- |
| **ADR ID** | ADR-043 |
| **ADR Title** | Spatial Layer Manager UI and Controls |
| **Category** | Spatial Layers (`docs/specs/spatial-layers/`) |
| **Claimed Status in Spec Header** | IMPLEMENTED - AWAITING USER REVIEW |
| **Verified Status (User-Observable)** | **FALSE CLAIM — 0% Implemented (NOT IMPLEMENTED / NON-FUNCTIONAL IN LIVE UI)** |
| **Audit Date** | 2026-08-12 |
| **Target Runtime** | C# 13 / .NET 9 / Avalonia 11.2.5 |
| **Auditor** | Principal AI Systems Architect & Product Auditor |

---

## 2. Normative Specification Requirement Inventory

| ID | Requirement Category | Symbol / Contract Name | Normative Specification Requirement |
| :--- | :--- | :--- | :--- |
| **REQ-043-01** | Plane 2 HUD Slate | `LayerManagerSlate.axaml` | Viewport-fixed Plane 2 HUD Slate composed full height or docked to screen edge (`--surface-chrome` `#161618` background, `1px` border `#2D2D2A`). |
| **REQ-043-02** | Monospaced Label Column| `4ch` Fixed Column | Layer labels (`01`, `02`, `B01`) rendered in a fixed `4ch` monospaced column (`--f-mono` `--t-label`) for vertical alignment. |
| **REQ-043-03** | Stack Count Header | Stack Counter Display | Header displaying total/filtered layer count (e.g. `12 LAYERS` or `3 OF 12 LAYERS`). |
| **REQ-043-04** | Search Filter | Filter Input Field | Full-width search box with placeholder `Watermark="Find a Layer"`. |
| **REQ-043-05** | Visibility Toggle | Eye Icon Control | Row button toggling layer visibility (`IsVisible`). Dimmed when layer is hidden. |
| **REQ-043-06** | Color Swatches | Layer Hue Swatch | `12x12px` circular swatch (`--r-full`) selecting from canonical design hues (`#4E6E9C`, `#B0524E`, `#6E62A6`, `#B08D4E`, `#4EB07B`). |
| **REQ-043-07** | Inline Confirmation | Destructive Removal Flow | Inline confirmation panel opening beneath affected row without modal scrims: `"Remove Layer {Name} and move what is on it to Layer {Dest}?"`. |
| **REQ-043-08** | Refusal State | Occupancy Refusal | Transitions confirm panel to inline refusal state when target cells are occupied: `"That space is occupied on Layer {Dest}."`. |
| **REQ-043-09** | Selection Synchronization| Active Row Outline | Active layer row carries a `2px` `--signal-interaction` selection outline offset `3px` outside its bounds. |
| **REQ-043-10** | Roving Focus & Keys | Keyboard Accessibility | Arrow keys (`Up`/`Down`) move list roving focus; `Home`/`End` jump to stack ends; `Escape` peels open confirms/filters/Slate. |

---

## 3. Codebase Reality & Line-by-Line Evidence

### 3.1 Symbol Existence & Implementation Audit

A codebase-wide search across [`src/GroveApp/`](file:///C:/dev/grove-v9/src/GroveApp/) demonstrates that zero UI components from ADR-043 exist:

| Symbol / Class Name | Expected Location | Actual Status in `src/GroveApp/` | Line-by-Line Evidence |
| :--- | :--- | :--- | :--- |
| `LayerManagerSlate.axaml` | `src/GroveApp/Controls/` | **0% Implemented (MISSING FILE)** | File does not exist. |
| `LayerManagerSlate.axaml.cs`| `src/GroveApp/Controls/` | **0% Implemented (MISSING FILE)** | File does not exist. |
| `LayerManagerViewModel` | `src/GroveApp/ViewModels/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. |
| `LayerRowItemViewModel` | `src/GroveApp/ViewModels/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. |
| Fixed `4ch` Label Column | `src/GroveApp/Controls/` | **0% Implemented (MISSING CONTROL)** | No monospaced label column control exists. |
| Inline Removal Confirmation | `src/GroveApp/Controls/` | **0% Implemented (MISSING CONTROL)** | No inline removal confirmation panel exists. |

---

## 4. Standards & Visual Plane Seam Audit

### 4.1 Visual Plane Separation (Plane 0 vs Layer 1 vs Plane 2)
- **Plane 2 (HUD Slate Plane)**: Completely non-existent. There is no Spatial Layer Manager Slate on Plane 2, leaving users unable to visually inspect, filter, reorder, or delete layers in the UI.

### 4.2 Code Smells & Architectural Violations
1. **False Claim in Spec Header**: The spec header explicitly states `status: "IMPLEMENTED - AWAITING USER REVIEW"`, yet the entire UI implementation (`LayerManagerSlate.axaml`, `LayerManagerViewModel.cs`, `LayerRowItemViewModel.cs`) is 0% built.
2. **Missing UI Controls**: The application provides no visual interface for layer management whatsoever.

---

## 5. Root Cause Analysis (RCA)

### 5.1 Primary Root Cause
ADR-043 was falsely marked as `"IMPLEMENTED - AWAITING USER REVIEW"` in its document header, even though no XAML view or ViewModel was ever created in `src/GroveApp/`.

### 5.2 Failure Chain
1. **False Documentation Marking**: The ADR document status was set to implemented without authoring the corresponding XAML files.
2. **Missing Plane 2 Slate Infrastructure**: Because Plane 2 HUD Slate hosting was never built (ADR-031), there was no container to host `LayerManagerSlate.axaml`.
