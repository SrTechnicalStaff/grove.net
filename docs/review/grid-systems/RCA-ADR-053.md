# Root Cause Analysis (RCA) Ledger: ADR-053

## 1. Executive Metadata

| Property | Value |
| :--- | :--- |
| **ADR ID** | `ADR-053` |
| **Title** | Spatial CRUD Operations, Selection State Machine, and Marquee Sweep |
| **Category** | Grid Systems (`docs/specs/grid-systems/`) |
| **Claimed Spec Status** | `IMPLEMENTED - AWAITING USER REVIEW` |
| **Verified Interactive Status** | `PARTIALLY IMPLEMENTED (AD-HOC IN-CONTROL)` |
| **Audit Date** | 2026-08-12 |
| **Target Runtime** | C# 13 / .NET 9 / Avalonia 11.2.5 / SkiaSharp 3.x |
| **Authors** | Chief Product Definition Architect & AI Systems Audit Team |

---

## 2. Normative Specification Requirement Inventory

The table below catalogs every explicit architectural requirement, mathematical formulation, interface contract, and visual token specified in [ADR-053](file:///C:/dev/grove-v9/docs/specs/grid-systems/ADR-053-Spatial-CRUD-Operations-And-Selection.md):

| ID | Requirement Category | Normative Spec Requirement | Target Symbol / Token |
| :--- | :--- | :--- | :--- |
| **REQ-053-1** | Service Contracts | `ISelectionService` and `ISpatialCrudService` interfaces managing selection sets, marquee sweeps, spatial CRUD, and clipboards. | `Grove.SpatialGrid.Operations.ISelectionService`, `ISpatialCrudService` |
| **REQ-053-2** | Data Contracts | `SpatialClipboardItemPayload` record and `SpatialClipboardContainer` record. | `SpatialClipboardItemPayload`, `SpatialClipboardContainer` |
| **REQ-053-3** | Compositor Execution | Custom Skia draw operation `MarqueeDrawOperation` implementing Avalonia `ICustomDrawOperation` for zero-allocation marquee rendering. | `Grove.SpatialGrid.Rendering.MarqueeDrawOperation` |
| **REQ-053-4** | Marquee Cell Bounds | Marquee box bounds snap to whole cell indices $[C_{x,0}, C_{y,0}] \times [C_{x,1}, C_{y,1}]$ covering world coordinates $P_{\text{start}}$ to $P_{\text{curr}}$. | Marquee Cell Rectangle Computation |
| **REQ-053-5** | AABB Intersection Query | Spatial intersection query $\text{Select}(I_k) = (X_{\text{min}} < X_k + W_k) \land \dots$ accumulating qualifying items into active selection set. | Axis-Aligned Bounding Box Intersection |
| **REQ-053-6** | Keyboard Matrix | Matrix execution (`N` create Note, `A` toggle anchor, `Del`/`Backspace` delete, `Esc` clear selection, `Ctrl+C`/`Ctrl+V` spatial copy/paste). | Keyboard Shortcut Execution Matrix |
| **REQ-053-7** | 3 Signal Roles | Interaction (`#96B6F8` outline + soft glow `0 0 24px 6px`), Marquee (`#E8B964` active sweep work), Invalid/Refusal (`#E2625C` / `#F06543`). | 3 Distinct Signal Roles |
| **REQ-053-8** | Placement Footprint Preview | Placement previews render 1px dashed edge (`rgba(150,182,248,0.80)`), 6% fill (`0.06`), and monospaced corner size label (`3 × 3`). | Placement Footprint Preview Contract |
| **REQ-053-9** | Refusal Invariants | Selection fill wash refusal (no paint wash over content), solid preview refusal, and center-screen alert dialog refusal. | Selection & Placement Refusal Invariants |

---

## 3. Codebase Reality & Line-by-Line Evidence

### 3.1 Implemented & Partially Implemented Symbols

- **Marquee Selection Sweep**:
  - `GridCanvasControl.cs` [L461-L466, L851-L890](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs#L461) handles pointer drag on empty canvas and evaluates item intersection.
  - `RenderMarqueeSelection` in `GridCanvasControl.cs` [L996-L1030](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs#L996) draws marquee rectangle onto Avalonia `DrawingContext`.
- **Keyboard Shortcuts**:
  - `KeybindModule.cs` [L135-L210](file:///C:/dev/grove-v9/src/GroveApp/Engine/KeybindModule.cs#L135-L210) handles `N` (create note), `Del`/`Backspace` (delete), `Esc` (deselect), `Ctrl+C`/`Ctrl+V` (clipboard via `NativeClipboardService.cs`).

### 3.2 0% Implemented & Deviated Symbols

- **`ISelectionService` & `ISpatialCrudService` Interfaces**: **0% Implemented**. Missing namespaces `Grove.SpatialGrid.Operations` and interfaces `ISelectionService`, `ISpatialCrudService`. Selection state is managed directly as a private `List<GridContentItem>` inside `GridCanvasControl.cs` [L136](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs#L136).
- **`SpatialClipboardContainer` & `SpatialClipboardItemPayload` Records**: **0% Implemented**. Clipboard serialization relies on generic text strings in `NativeClipboardService.cs` rather than structured spatial memory records.
- **`MarqueeDrawOperation`**: **0% Implemented**. Missing custom Skia draw operation class `Grove.SpatialGrid.Rendering.MarqueeDrawOperation`. Drawing occurs directly on Avalonia `DrawingContext`.
- **3 Signal Roles Handover (`#96B6F8` vs `#E8B964` vs `#E2625C`)**: **0% Implemented**.
  - While marquee dragging, color transitions and explicit handover from amber `#E8B964` to interaction accent `#96B6F8` are not governed by a signal role matrix.
  - Soft glow `0 0 24px 6px rgb(150 182 248 / 0.45)` and structural field cell brightening (`rgba(150,182,248,0.13)`) are 0% Implemented.
- **Monospaced Corner Size Label (`3 × 3`)**: **0% Implemented**. Previews do not render the monospaced footprint dimension label in the corner.
- **Point-of-Action Refusal Strip Toolbar**: **0% Implemented**. No refusal strip toolbar ("This space is occupied") is displayed during invalid spatial CRUD actions.

---

## 4. Standards & Visual Plane Seam Audit

1. **Plane 0 Composition Seam**:
   - Marquee drawing and footprint previews are rendered directly inside `GridCanvasControl.OnRender` instead of being partitioned into custom Skia draw passes.
2. **Design System Tokens Alignment**:
   - `Colors.cs` defines `MarqueeHex = "#E8B964"`, but signal roles (Interaction vs Marquee vs Refusal) are applied inconsistently without role-based brush lookup managers.
3. **Code Smells & Architectural Violations**:
   - Selection state management is duplicated across `GridCanvasControl.cs`, `KeybindModule.cs`, and `NativeClipboardService.cs` without a central `ISelectionService` acting as single source of truth.

---

## 5. Root Cause Analysis (RCA)

### Why claimed status (`IMPLEMENTED - AWAITING USER REVIEW`) diverges from actual interactive reality:

1. **Monolithic Control State Machine**:
   - Instead of creating `ISelectionService` and `ISpatialCrudService` modules, all selection tracking was placed directly in `GridCanvasControl`. This achieved basic click-to-select and drag-marquee functionality but violated decoupled architecture principles.
2. **Omission of Signal Role Matrix**:
   - The design spec established strict signal role separation (`#96B6F8`, `#E8B964`, `#E2625C`). The implementation hardcoded ad-hoc colors into drawing routines, skipping soft glow shaders and perimeter inset calculations.
3. **Missing Footprint Size Labels**:
   - Corner size readouts (`3 × 3`) were omitted from preview rendering routines because typography rendering inside Skia draw operations was not wired up for transient preview ghosts.
4. **Clipboard Schema Simplification**:
   - The author used basic string JSON serialization in `NativeClipboardService` rather than implementing the rich `SpatialClipboardContainer` payload schema with relative cell offsets and memory provenance bindings.
