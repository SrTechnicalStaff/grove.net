# Root Cause Analysis (RCA) Ledger: ADR-056

## 1. Executive Metadata

| Property | Value |
| :--- | :--- |
| **ADR ID** | `ADR-056` |
| **Title** | Interactive Resize Geometry, Type-Specific Footprint Solvers, and Affordance Rendering Engine |
| **Category** | Grid Systems (`docs/specs/grid-systems/`) |
| **Claimed Spec Status** | `IMPLEMENTED - AWAITING USER REVIEW` |
| **Verified Interactive Status** | `FAILS INTERACTIVE AUDIT (SUB-SET STUB)` |
| **Audit Date** | 2026-08-12 |
| **Target Runtime** | C# 13 / .NET 9 / Avalonia 11.2.5 / SkiaSharp 3.x |
| **Authors** | Chief Product Definition Architect & AI Systems Audit Team |

---

## 2. Normative Specification Requirement Inventory

The table below catalogs every explicit architectural requirement, mathematical formulation, interface contract, and visual token specified in [ADR-056](file:///C:/dev/grove-v9/docs/specs/grid-systems/ADR-056-Interactive-Resize-Geometry-And-Affordances.md):

| ID | Requirement Category | Normative Spec Requirement | Target Symbol / Token |
| :--- | :--- | :--- | :--- |
| **REQ-056-1** | Service Contract | `IInteractiveResizeEngine` interface managing resize drag session lifetime, ghost previews, validation, commit, and cancel. | `Grove.SpatialGrid.Resize.IInteractiveResizeEngine` |
| **REQ-056-2** | Solver Engine | `TypeSpecificFootprintSolver` static class solving Note ($n \times n$), Document ($2 \times 2..8 \times 8$), and Image (aspect ratio $L = \lceil\text{long}/256\rceil$). | `Grove.SpatialGrid.Resize.TypeSpecificFootprintSolver` |
| **REQ-056-3** | Data Contracts | `ResizeHandleHitTest` struct (`Location`, `ScreenCenterX`, `ScreenCenterY`, `TargetSizePixels = 12.0`, `ContainsPointer`). | `ResizeHandleHitTest` |
| **REQ-056-4** | Handle & Edge Geometry | 12px bottom-right corner target handle ($H_{\text{SE}}$) and 1.5px containment edge highlight in `#96B6F8` (`--accent-edge-highlight`). | 12px Corner Target & 1.5px Containment Edge |
| **REQ-056-5** | Cursor Hover States | Avalonia cursor transitions on hover (`SizeNWSE` over SE/NW handles, `SizeNESW` over NE/SW handles, `Arrow` default). | Pointer Cursor State Machine |
| **REQ-056-6** | Image Aspect Solver | Image aspect solver preserving principal axis $D = 256\text{px}$ divisor $L = \lceil\text{long}/256\rceil$ and secondary axis $S_{\text{derived}}$ during resize. | Image Aspect Ratio Interactive Solver |
| **REQ-056-7** | Document Bounds Solver | Document free cell pitch bounds solver constrained strictly between $2 \times 2$ cells ($440 \times 440\text{ DIPs}$) and $8 \times 8$ cells. | Document Free Cell Pitch Solver |
| **REQ-056-8** | Note Extent Solver | Note 1:1 square extent solver where width equals height ($n \times n$, $n \in [1, 8]$) using maximum axial displacement $\Delta C_{\text{max}}$. | Note Square Extent Solver |
| **REQ-056-9** | Refusal Hatching | Candidate footprint preview transitions to 12% refusal state (`rgba(226,98,92,0.12)`), $45^\circ$ diagonal cross-hatching, and inset border. | Collision Refusal Cross-Hatch Pattern |
| **REQ-056-10** | Refusal Strip | Point-of-action refusal strip toolbar ("This space is occupied" with disabled `Place` and active `Cancel`). Zero modal dialogs. | Point-of-Action Refusal Strip Toolbar |

---

## 3. Codebase Reality & Line-by-Line Evidence

### 3.1 Implemented & Partially Implemented Symbols

- **Basic SouthEast Resize Target**:
  - `GridCanvasControl.cs` [L524-L545](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs#L524-L545) detects pointer press near the bottom-right corner of a selected note.
- **`ImageFootprintResolver.cs`**:
  - [`Engine/ImageFootprintResolver.cs:L1-L60`](file:///C:/dev/grove-v9/src/GroveApp/Engine/ImageFootprintResolver.cs#L1-L60) implements initial footprint placement calculations ($L = \lceil\text{long}/256\rceil$), but is NOT wired to interactive resize gestures.

### 3.2 0% Implemented & Deviated Symbols

- **`IInteractiveResizeEngine` Interface**: **0% Implemented**. Missing namespace `Grove.SpatialGrid.Resize` and interface `IInteractiveResizeEngine`.
- **`TypeSpecificFootprintSolver` Static Class**: **0% Implemented**. Missing `TypeSpecificFootprintSolver` containing `SolveNoteSquare`, `SolveDocumentBounds`, and `SolveImageAspectRatio`.
- **`ResizeHandleHitTest` Struct**: **0% Implemented**.
- **Interactive Image Aspect Ratio Resize**: **0% Implemented**. `ImageFootprintResolver` is used only during initial placement, not during live drag-to-resize gestures. `GridImage` cannot be interactively resized.
- **Interactive Document Bounds Resize**: **0% Implemented**. `GridDocument` cannot be resized interactively.
- **Avalonia System Cursor Hover Transitions (`SizeNWSE` / `SizeNESW`)**: **0% Implemented**.
  - In `GridCanvasControl.cs` [L370-L400](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs#L370-L400), pointer movement over corner handles does NOT set `Cursor = StandardCursorType.SizeNWSE` or `SizeNESW`. The cursor remains `StandardCursorType.Arrow`.
- **4 Corner Handles ($NW, NE, SE, SW$)**: **0% Implemented**. Only SouthEast handle is partially supported for Notes. $NW, NE, SW$ handles do not exist.
- **Collision Refusal Cross-Hatch Pattern & Strip Toolbar**: **0% Implemented**. No cross-hatching or strip toolbar appears when resizing into an occupied region.

---

## 4. Standards & Visual Plane Seam Audit

1. **Plane 0 Composition Seam**:
   - Skia rendering methods `DrawResizeAffordances` and `DrawResizeGhostPreview` mandated by ADR-056 Section 5 are 0% Implemented as standalone modules. Affordances are partially drawn inside `NoteRenderModule.cs`.
2. **Design System Tokens Alignment**:
   - Edge highlight color `#96B6F8` (`--accent-edge-highlight`) is applied on note selection, but 12px corner target handle rendering and refusal colors `#F06543` / `#E2625C` are omitted.
3. **Code Smells & Architectural Violations**:
   - Missing Avalonia system cursor hover feedback makes resize handle hit targets non-discoverable to users.
   - Non-Note content types (`GridDocument`, `GridImage`) crash or ignore resize attempts due to hardcoded type casting in `GridCanvasControl`.

---

## 5. Root Cause Analysis (RCA)

### Why claimed status (`IMPLEMENTED - AWAITING USER REVIEW`) diverges from actual interactive reality:

1. **Partial Note Prototype Claimed as Complete Engine**:
   - The author wrote a quick single-corner resize snippet for `GridNote` and assumed it satisfied the interactive resize specification, ignoring type-specific solver contracts for Documents and Images.
2. **Missing System Cursor Binding**:
   - Pointer hover events were not connected to Avalonia's `Window.Cursor` property, resulting in a static arrow cursor over interactive corner handles.
3. **Static Image Resolver Bypassed for Dynamic Drag**:
   - `ImageFootprintResolver` was authored for static placement calculation. The developer failed to refactor it into `TypeSpecificFootprintSolver.SolveImageAspectRatio` for live drag tracking.
4. **Omission of Refusal Overlay Shaders**:
   - Skia diagonal hatching shaders and local refusal toolbars were skipped to avoid implementing complex canvas overlay passes.
