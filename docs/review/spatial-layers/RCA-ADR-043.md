# RCA Ledger: ADR-043 — Spatial Layer Manager UI and Controls

## 1. Executive Metadata

| Metadata Field | Value |
| :--- | :--- |
| **ADR ID** | ADR-043 |
| **ADR Title** | Spatial Layer Manager UI and Controls |
| **Category** | Spatial Layers (`docs/specs/spatial-layers/`) |
| **Claimed Status in Spec Header** | IMPLEMENTED - AWAITING USER REVIEW |
| **Verified Status (User-Observable)** | **PARTIAL — overlay is mounted and interactive; accessibility and transfer-state details remain** |
| **Audit Date** | 2026-08-12 |
| **Target Runtime** | C# 13 / .NET 9 / Avalonia 11.2.5 |
| **Auditor** | Principal AI Systems Architect & Product Auditor |

---

## 2. Normative Specification Requirement Inventory

| ID | Requirement Category | Symbol / Contract Name | Normative Specification Requirement |
| :--- | :--- | :--- | :--- |
| **REQ-043-01** | Plane 2 HUD Overlay | `LayerManagerOverlay.axaml` | Viewport-fixed Plane 2 Layer Manager overlay composed full height or docked to screen edge (`--surface-chrome` `#161618` background, `1px` border `#2D2D2A`). |
| **REQ-043-02** | Monospaced Label Column| `4ch` Fixed Column | Layer labels (`01`, `02`, `B01`) rendered in a fixed `4ch` monospaced column (`--f-mono` `--t-label`) for vertical alignment. |
| **REQ-043-03** | Stack Count Header | Stack Counter Display | Header displaying total/filtered layer count (e.g. `12 LAYERS` or `3 OF 12 LAYERS`). |
| **REQ-043-04** | Search Filter | Filter Input Field | Full-width search box with placeholder `Watermark="Find a Layer"`. |
| **REQ-043-05** | Visibility Toggle | Eye Icon Control | Row button toggling layer visibility (`IsVisible`). Dimmed when layer is hidden. |
| **REQ-043-06** | Color Swatches | Layer Hue Swatch | `12x12px` circular swatch (`--r-full`) selecting from canonical design hues (`#4E6E9C`, `#B0524E`, `#6E62A6`, `#B08D4E`, `#4EB07B`). |
| **REQ-043-07** | Inline Confirmation | Destructive Removal Flow | Inline confirmation panel opening beneath affected row without modal scrims: `"Remove Layer {Name} and move what is on it to Layer {Dest}?"`. |
| **REQ-043-08** | Refusal State | Occupancy Refusal | Transitions confirm panel to inline refusal state when target cells are occupied: `"That space is occupied on Layer {Dest}."`. |
| **REQ-043-09** | Selection Synchronization| Active Row Outline | Active layer row carries a `2px` `--signal-interaction` selection outline offset `3px` outside its bounds. |
| **REQ-043-10** | Roving Focus & Keys | Keyboard Accessibility | Arrow keys (`Up`/`Down`) move list roving focus; `Home`/`End` jump to stack ends; `Escape` peels open confirms/filters/overlay. |

---

## 3. Codebase Reality & Line-by-Line Evidence

### 3.1 Symbol Existence & Implementation Audit

A codebase-wide search across [`src/GroveApp/`](file:///C:/dev/grove-v9/src/GroveApp/) demonstrates that zero UI components from ADR-043 exist:

| Symbol / Class Name | Expected Location | Actual Status in `src/GroveApp/` | Line-by-Line Evidence |
| :--- | :--- | :--- | :--- |
| `LayerManagerOverlay.axaml` | `src/GroveApp/Controls/` | **Implemented** | Layer Manager overlay is mounted in `MainWindow.axaml`. |
| `LayerManagerOverlay.axaml.cs`| `src/GroveApp/Controls/` | **Implemented** | Overlay owns layer list input and commands. |
| `LayerManagerViewModel` | `src/GroveApp/ViewModels/` | **Not used** | The code-behind overlay binds directly to `ISpatialLayerStateService`. |
| `LayerRowItemViewModel` | `src/GroveApp/ViewModels/` | **Not used** | Row projection is created from `SpatialLayerModel`. |
| Fixed `4ch` Label Column | `src/GroveApp/Controls/LayerManagerOverlay.axaml.cs` | **Implemented** | Monospaced padded label column is created per row. |
| Inline Removal Confirmation | `src/GroveApp/Controls/LayerManagerOverlay.axaml.cs` | **Implemented** | Delete confirmation and occupied-destination refusal remain inline. |

---

## 4. Standards & Visual Plane Seam Audit

### 4.1 Visual Plane Separation (Plane 0 vs Plane 1 vs Plane 2)
- **Plane 2 (HUD Overlay Plane)**: `LayerManagerOverlay` is registered at the HUD plane z-index and receives filtering, selection, visibility, lock, insertion, reorder, rename, and delete actions.

### 4.2 Code Smells & Architectural Violations
1. **Stale Claim in Spec Header**: The implementation is present, but the legacy Slate filename and the original view-model claim were inaccurate.
2. **Remaining UI Gap**: Layer transfer confirmation and per-layer color semantics require completion.

---

## 5. Root Cause Analysis (RCA)

### 5.1 Primary Root Cause
The original ADR-043 review predates the direct code-behind overlay now present in `src/GroveApp/`. The remaining mismatch is between the draft MVVM contract and the shipped overlay seam.

### 5.2 Failure Chain
1. **False Documentation Marking**: The ADR used a Slate name for a Layer Manager overlay.
2. **Resolved Plane 2 Hosting Gap**: The compositor now mounts `LayerManagerOverlay` in the HUD plane.
