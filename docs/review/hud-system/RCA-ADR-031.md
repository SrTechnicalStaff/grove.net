# RCA Ledger: ADR-031 — Slate Window System and Anatomy Specifications

## 1. Executive Metadata

| Metadata Field | Value |
| :--- | :--- |
| **ADR ID** | ADR-031 |
| **ADR Title** | Slate Window System and Anatomy Specifications |
| **Category** | HUD System (`docs/specs/hud-system/`) |
| **Claimed Status in Spec Header** | Accepted |
| **Verified Status (User-Observable)** | **0% Implemented (NOT IMPLEMENTED / NON-FUNCTIONAL IN LIVE UI)** |
| **Audit Date** | 2026-08-12 |
| **Target Runtime** | C# 13 / .NET 9 / Avalonia 11.2.5 |
| **Auditor** | Principal AI Systems Architect & Product Auditor |

---

## 2. Normative Specification Requirement Inventory

| ID | Requirement Category | Symbol / Contract Name | Normative Specification Requirement |
| :--- | :--- | :--- | :--- |
| **REQ-031-01** | Frame Container | `SlateFrameControl` | Viewport-fixed Avalonia `ContentControl` (`#161618` `--surface-chrome` fill, `1px` `#2D2D2A` `--edge-control` border, `4px` corner radius, `24px` `--sp-lg` padding). |
| **REQ-031-02** | Docking Mode | `SlateDockMode` Enum | Docking configuration enum (`FullViewport`, `LeftPane`, `RightPane`). |
| **REQ-031-03** | Interface Contract | `ISlateWindow` Interface | Slate interface declaring `IdentityTitle`, `DockMode`, `HandleEscapeKey()`, `OnSlateOpened()`, `OnSlateClosed()`. |
| **REQ-031-04** | Masonry Layout Panel | `MasonryGalleryPanel` Class | Custom Avalonia Panel executing dynamic masonry column calculation $n = \max\left(1, \left\lfloor \frac{W + g}{220 + g} \right\rfloor\right)$ ($g=12\text{px}$), placing items in shortest column. |
| **REQ-031-05** | Slate Anatomy: Writing | `WritingSlate` Control | Dedicated document viewing/authoring HUD Slate ($60\%$ viewport width default, `--measure-reading` $60-75\text{ch}$, paginated vertical scrolling, `--surface-page` fill). |
| **REQ-031-06** | Slate Anatomy: Memory | `MemorySlate` Control | Universal archive search Slate with full-width search input (`#101012` `--surface-nested`), filter buttons (`All`, `Notes`, `Documents`, `Images`), and masonry grid. |
| **REQ-031-07** | Slate Anatomy: Gallery | `GallerySlate` Control | Media archive Slate displaying images in intrinsic proportions, facet filter row (`All`, `GIFs`, `Placed`), identity lines (`Filename · Format · Date`), and image count header. |
| **REQ-031-08** | Slate Anatomy: Layers | `LayerManagerSlate` Control | Spatial layer stack management Slate on Plane 2. |
| **REQ-031-09** | Strict Tonal Hierarchy | 3 Dark Tonal Steps Rule | Maximum of 3 dark tonal steps (`#0E0E10` canvas $\to$ `#161618` chrome $\to$ `#101012` nested pane). A 4th step is strictly prohibited. |
| **REQ-031-10** | Escape Peeling | Escape Key Hierarchy | Pressing `Escape` peels exactly one layer in sequence: dismiss context menu $\to$ clear search filter $\to$ return to gallery view $\to$ dismiss topmost Slate. |

---

## 3. Codebase Reality & Line-by-Line Evidence

### 3.1 Symbol Existence & Implementation Audit

A complete search across [`src/GroveApp/`](file:///C:/dev/grove-v9/src/GroveApp/) verifies that zero classes, controls, or panel layouts from ADR-031 exist:

| Symbol / Class Name | Expected Location | Actual Status in `src/GroveApp/` | Line-by-Line Evidence |
| :--- | :--- | :--- | :--- |
| `SlateFrameControl` | `src/GroveApp/Controls/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. |
| `ISlateWindow` | `src/GroveApp/Engine/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. |
| `SlateDockMode` | `src/GroveApp/Engine/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. |
| `MasonryGalleryPanel` | `src/GroveApp/Controls/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. No masonry layout engine exists. |
| `WritingSlate` | `src/GroveApp/Controls/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. |
| `MemorySlate` | `src/GroveApp/Controls/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. |
| `GallerySlate` | `src/GroveApp/Controls/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. |
| `LayerManagerSlate` | `src/GroveApp/Controls/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. |

### 3.2 Analysis of `QuickNoteOverlay.axaml`

The existing [`QuickNoteOverlay.axaml`](file:///C:/dev/grove-v9/src/GroveApp/Controls/QuickNoteOverlay.axaml#L1) is a lightweight modal capture window centered on screen. It is NOT a Slate System:
- Lacks `SlateFrameControl` chrome and token structure.
- Lacks `ISlateWindow` interface implementation.
- Does not support docking modes (`FullViewport`, `LeftPane`, `RightPane`).
- Does not contain `WritingSlate`, `MemorySlate`, `GallerySlate`, or `LayerManagerSlate`.

---

## 4. Standards & Visual Plane Seam Audit

### 4.1 Visual Plane Separation (Plane 0 vs Layer 1 vs Plane 2)
- **Plane 2 (HUD Slate Plane)**: Entirely non-existent. There is no host container or window manager for HUD Slates on Plane 2.

### 4.2 Code Smells & Architectural Violations
1. **Absence of Viewport-Fixed Workspace Slates**: Users have no way to browse memory archives, view media galleries, or manage layers within generous viewport-fixed surfaces.

---

## 5. Root Cause Analysis (RCA)

### 5.1 Primary Root Cause
The Slate Window System defined in ADR-031 was never built. Development focused on Plane 0 canvas notes and Plane 1 local editor overlays, leaving Plane 2 HUD Slates unbuilt.

### 5.2 Failure Chain
1. **Unbuilt UI Components**: Neither `SlateFrameControl.cs` nor `MasonryGalleryPanel.cs` was authored.
2. **Missing Slate Controls**: `MemorySlate`, `GallerySlate`, `WritingSlate`, and `LayerManagerSlate` were never implemented in `src/GroveApp/Controls/`.
