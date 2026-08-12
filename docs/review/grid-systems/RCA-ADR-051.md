# Root Cause Analysis (RCA) Ledger: ADR-051

## 1. Executive Metadata

| Property | Value |
| :--- | :--- |
| **ADR ID** | `ADR-051` |
| **Title** | Interactive Resize Engine and Cell Alignment System |
| **Category** | Grid Systems (`docs/specs/grid-systems/`) |
| **Claimed Spec Status** | `IMPLEMENTED - AWAITING USER REVIEW` |
| **Verified Interactive Status** | `FAILS INTERACTIVE AUDIT (SUB-SET AD-HOC STUB)` |
| **Audit Date** | 2026-08-12 |
| **Target Runtime** | C# 13 / .NET 9 / Avalonia 11.2.5 / SkiaSharp 3.x |
| **Authors** | Chief Product Definition Architect & AI Systems Audit Team |

---

## 2. Normative Specification Requirement Inventory

The table below catalogs every explicit architectural requirement, mathematical formulation, interface contract, and visual token specified in [ADR-051](file:///C:/dev/grove-v9/docs/specs/grid-systems/ADR-051-Interactive-Resize-And-Cell-Alignment.md):

| ID | Requirement Category | Normative Spec Requirement | Target Symbol / Token |
| :--- | :--- | :--- | :--- |
| **REQ-051-1** | Service Contract | `ISpatialResizeService` interface managing `IsRegionFree`, `BeginResize`, `UpdateResize`, `CommitResize`, `CancelResize`, and state events. | `Grove.SpatialGrid.Resize.ISpatialResizeService` |
| **REQ-051-2** | Data Contracts | `ResizeHandleLocation` enum (`NorthWest`=1, `NorthEast`=2, `SouthEast`=3, `SouthWest`=4). | `Grove.SpatialGrid.Resize.ResizeHandleLocation` |
| **REQ-051-3** | Data Contracts | `SpatialRegion` struct (`X`, `Y`, `Width`, `Height`, `Intersects`), `ResizeStatus` enum, `ResizeState` record. | `SpatialRegion`, `ResizeStatus`, `ResizeState` |
| **REQ-051-4** | Compositor Execution | Custom Skia draw operation `ResizePreviewDrawOperation` implementing Avalonia `ICustomDrawOperation` for zero-allocation ghost rendering. | `Grove.SpatialGrid.Rendering.ResizePreviewDrawOperation` |
| **REQ-051-5** | Handle Hit Math | 4 corner resize handles with $12.0\text{ px}$ hit radius $r_{\text{hit}}$ centered at corner vertices $H_{NW}, H_{NE}, H_{SE}, H_{SW}$. | Corner Handle Hit Geometry |
| **REQ-051-6** | Discrete Cell Limits | Discrete bounds $W_{\text{cells}} \in [1, 8]$, $H_{\text{cells}} \in [1, 8]$ with half-cell rounding ($\lfloor W_{\text{raw}} + 0.5 \rfloor$). | Discrete Snap-to-Cell Delta Mathematics |
| **REQ-051-7** | Spatial Region Free | R-Tree region collision check `IsRegionFree(R_candidate, ID_active)` preventing overlapping placements. | `IsRegionFree` Collision Predicate |
| **REQ-051-8** | Refusal Hatching | $45^\circ$ diagonal cross-hatching (`repeating-linear-gradient`, 12% fill `#E2625C` / `#F06543`, inset border `inset 0 0 0 1px rgba(226,98,92,0.45)`). | Collision Refusal Cross-Hatching |
| **REQ-051-9** | Refusal Strip | Local point-of-action refusal strip toolbar beside refused footprint ("This space is occupied" with disabled `Place` action). Zero modal dialogs. | Point-of-Action Refusal Strip Toolbar |

---

## 3. Codebase Reality & Line-by-Line Evidence

### 3.1 Implemented & Partially Implemented Symbols

- **Ad-Hoc Corner Resize Logic**:
  - In `GridCanvasControl.cs` [L403-L435](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs#L403-L435), mouse movement checks `_isResizingNote` and updates `SizeCells` on a `GridNote`.
  - In `GridCanvasControl.cs` [L524-L545](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs#L524-L545), pointer press checks if pointer is near the bottom-right corner of the selected `GridNote` to initiate resize.

### 3.2 0% Implemented & Deviated Symbols

- **`ISpatialResizeService` Interface**: **0% Implemented**. Missing namespace `Grove.SpatialGrid.Resize` and interface `ISpatialResizeService`. No standalone service encapsulates resize transaction lifetime.
- **`ResizeHandleLocation` Enum**: **0% Implemented**. Missing enum `ResizeHandleLocation`. Only the SouthEast corner of notes is hardcoded; NorthWest, NorthEast, and SouthWest corner handles are completely non-existent.
- **`ResizePreviewDrawOperation`**: **0% Implemented**. Missing custom Skia draw operation class `Grove.SpatialGrid.Rendering.ResizePreviewDrawOperation`. No ghost preview is rendered during resize.
- **`SpatialRegion` & `ResizeState` Records**: **0% Implemented**. Missing types `SpatialRegion`, `ResizeStatus`, `ResizeState`.
- **Collision Refusal Cross-Hatching ($45^\circ$)**: **0% Implemented**. During note resize in `GridCanvasControl.cs`, collision checking against other items is completely omitted. Resizing a note into an occupied cell succeeds without refusal hatching or validation.
- **Point-of-Action Refusal Strip Toolbar**: **0% Implemented**. No local strip toolbar ("This space is occupied") exists in XAML or code.
- **Multi-Type Placement Support**: **0% Implemented**. Resize code in `GridCanvasControl.cs` cast-checks exclusively to `GridNote` [L405, L525](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs#L405). `GridDocument` and `GridImage` items cannot be resized interactively.

---

## 4. Standards & Visual Plane Seam Audit

1. **Plane 0 Composition Seam**:
   - Resize handles and edge highlights are drawn directly inside `NoteRenderModule.cs` [L240-L260](file:///C:/dev/grove-v9/src/GroveApp/Engine/NoteRenderModule.cs#L240-L260) on Plane 0 rather than being managed as interactive visual affordances by a dedicated resize subsystem.
2. **Design System Tokens Alignment**:
   - Token `#96B6F8` is used for selection outlines, but invalid role `#E2625C` / `#F06543` cross-hatch fill (`rgba(226,98,92,0.12)`) and inset borders (`inset 0 0 0 1px rgba(226,98,92,0.45)`) are absent.
3. **Code Smells & Architectural Violations**:
   - Resize logic directly mutates `GridNote.SizeCells` during pointer drag without transactional preview/commit/cancel rollback states.
   - `GridCanvasControl` tightly couples hit testing, pointer tracking, bounds calculations, and item mutation in a single 1000-line control file.

---

## 5. Root Cause Analysis (RCA)

### Why claimed status (`IMPLEMENTED - AWAITING USER REVIEW`) diverges from actual interactive reality:

1. **Primitive Prototype Equated to Architectural Spec**:
   - A primitive 1-corner drag implementation for `GridNote` was authored directly inside `GridCanvasControl` to demonstrate visual resizing. The author subsequently marked ADR-051 as "IMPLEMENTED" despite omitting 80% of the spec.
2. **Missing Region Collision Integration**:
   - The resize gesture was never hooked up to `IsRegionFree` spatial index queries. Consequently, users can resize items directly over existing grid content without collision checks or refusal feedback.
3. **Failure to Decouple Type-Specific Footprint Solvers**:
   - The spec required distinct geometric solver logic for Notes ($n \times n$), Documents ($2 \times 2$ to $8 \times 8$), and Images (aspect ratio formulas). Because no `ISpatialResizeService` was built, non-Note placements were left completely unresizable.
4. **Omission of Refusal Feedback UI**:
   - The refusal strip toolbar and Skia $45^\circ$ diagonal cross-hatch shaders required complex composition across Plane 0 and Plane 2, which was bypassed in favor of simple direct property mutation.
