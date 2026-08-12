# Grove v9 Architecture Decision Record (ADR) Master Roadmap

| Property | Value |
| :--- | :--- |
| **Document Type** | Architectural Master Roadmap & Implementation Index |
| **Author** | Chief Product Definition Architect |
| **Target System** | Grove v9 Continuous Spatial Workspace Engine |
| **Date** | 2026-08-12 |

---

## 1. Overview & Architectural Scope

This Master Roadmap tracks all 35 Architecture Decision Records (ADRs) across the 9 primary architectural subsystems of **Grove v9**. Each ADR establishes normative architectural rules, mathematical models, and implementation specifications for Grove's three-plane spatial workspace.

The historical implementation labels in this roadmap are not an implementation proof. The executable source and the verified ledgers in `docs/review/` are authoritative. ADRs are now treated as `IMPLEMENTED`, `PARTIAL`, `SUPERSEDED`, or `CONFLICTING` only after a source-and-behavior check; missing seams remain work items.

---

## 2. Master ADR Matrix

Below is the consolidated matrix tracking all 35 ADRs across the 9 defined categories:

| ADR ID | Title | Category | Status | Implementation File / Spec Link |
| :--- | :--- | :--- | :--- | :--- |
| [ADR-001](file:///C:/dev/grove-v9/docs/specs/spatial-grid/ADR-001-Spatial-Grid-Plane-Architecture.md) | Spatial Grid Plane Architecture | Spatial Grid | PARTIAL — verified grid and camera slice; compositor gaps remain | [`GridLineModule.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/GridLineModule.cs), [`GridCanvasControl.cs`](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs) |
| [ADR-002](file:///C:/dev/grove-v9/docs/specs/spatial-grid/ADR-002-Field-Ledger-And-Subscribers.md) | Field Ledger and Subscribers | Spatial Grid | PARTIAL — verified discrete ledger/subscriber slice; contour and qualification gaps remain | [`FieldLedgerEngine.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/FieldLedgerEngine.cs), [`FieldLedgerModule.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/FieldLedgerModule.cs) |
| [ADR-003](file:///C:/dev/grove-v9/docs/specs/spatial-grid/ADR-003-Spatial-Aura-Physics.md) | Spatial Aura Physics | Spatial Grid | PARTIAL — verified discrete aura physics; representation tiers remain | [`FieldLedgerEngine.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/FieldLedgerEngine.cs) |
| [ADR-004](file:///C:/dev/grove-v9/docs/specs/spatial-grid/ADR-004-Three-Plane-Visual-Hierarchy.md) | Three-Plane Visual Hierarchy | Spatial Grid | PARTIAL — verified visual stacking; compositor routing gaps remain | [`GridCanvasControl.cs`](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs), [`MainWindow.axaml.cs`](file:///C:/dev/grove-v9/src/GroveApp/MainWindow.axaml.cs) |
| [ADR-005](file:///C:/dev/grove-v9/docs/specs/spatial-grid/ADR-005-Camera-Affine-Transform-Engine.md) | Camera Affine Transform Engine | Spatial Grid | PARTIAL — verified affine camera; inertia gaps remain | [`CameraModule.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/CameraModule.cs) |
| [ADR-010](file:///C:/dev/grove-v9/docs/specs/content-types/ADR-010-Note-Physical-Geometry-And-Fills.md) | Note Physical Geometry, Authored Fills, and Grid Placement | Content Types | PARTIAL — verified note geometry/rendering; LOD gaps remain | [`NoteRenderModule.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/NoteRenderModule.cs), [`GridNote.cs`](file:///C:/dev/grove-v9/src/GroveApp/Models/GridNote.cs) |
| [ADR-011](file:///C:/dev/grove-v9/docs/specs/content-types/ADR-011-Document-Physical-Geometry-And-Reflow.md) | Document Physical Geometry, Page Texture, AST, and Multi-Column Reflow Engine | Content Types | PARTIAL — verified document editor seam; pagination controls remain | [`DocumentReflowEngine.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/DocumentReflowEngine.cs), [`GridDocument.cs`](file:///C:/dev/grove-v9/src/GroveApp/Models/GridDocument.cs) |
| [ADR-012](file:///C:/dev/grove-v9/docs/specs/content-types/ADR-012-Image-Footprint-Resolution-Mapping.md) | Image Footprint Resolution Mapping, Zero-Crop Rules, and Skia Bitmap Sampling | Content Types | PARTIAL — verified intrinsic footprint/ownership; property controls remain | [`ImageFootprintResolver.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/ImageFootprintResolver.cs), [`GridImage.cs`](file:///C:/dev/grove-v9/src/GroveApp/Models/GridImage.cs) |
| [ADR-013](file:///C:/dev/grove-v9/docs/specs/content-types/ADR-013-External-Drag-And-Drop-System.md) | External Drag-and-Drop System, ScreenToCell Coordinate Resolution, and Content Auto-Creation | Content Types | PARTIAL — verified ownership, preview, and rejection seam | [`ExternalDragDropHandler.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/ExternalDragDropHandler.cs) |
| [ADR-014](file:///C:/dev/grove-v9/docs/specs/content-types/ADR-014-Native-Clipboard-HTML-And-RichText-Interop.md) | Native Clipboard HTML and RichText Interoperability Specification | Content Types | PARTIAL — verified clipboard placement seam; feedback/format gaps remain | [`NativeClipboardService.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/NativeClipboardService.cs), [`RichTextEngine.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/RichTextEngine.cs) |
| [ADR-020](file:///C:/dev/grove-v9/docs/specs/memory-system/ADR-020-Memory-Model-And-Immutable-Ledger.md) | Memory Model and Immutable Ledger Architecture | Memory System | Normative / Accepted | [`Memory.md`](file:///C:/dev/grove-v9/docs/product/definitions/Memory.md) |
| [ADR-021](file:///C:/dev/grove-v9/docs/specs/memory-system/ADR-021-Memory-Lineage-And-Version-Tree.md) | Memory Lineage and Version Tree Architecture | Memory System | Normative / Accepted | [`ADR-020-Memory-Model-And-Immutable-Ledger.md`](file:///C:/dev/grove-v9/docs/specs/memory-system/ADR-020-Memory-Model-And-Immutable-Ledger.md) |
| [ADR-022](file:///C:/dev/grove-v9/docs/specs/memory-system/ADR-022-Memory-Spatial-RTree-Index.md) | Memory Spatial R-Tree Index Architecture | Memory System | Normative / Accepted | [`ADR-020-Memory-Model-And-Immutable-Ledger.md`](file:///C:/dev/grove-v9/docs/specs/memory-system/ADR-020-Memory-Model-And-Immutable-Ledger.md) |
| [ADR-030](file:///C:/dev/grove-v9/docs/specs/hud-system/ADR-030-Three-Plane-Compositor-Architecture-Validation.md) | Three-Plane Compositor Architecture Validation | HUD System | Accepted | [`MainWindow.axaml.cs`](file:///C:/dev/grove-v9/src/GroveApp/MainWindow.axaml.cs) |
| [ADR-031](file:///C:/dev/grove-v9/docs/specs/hud-system/ADR-031-Slate-Window-System-And-Anatomy.md) | Slate Window System and Anatomy Specifications | HUD System | Accepted | [`Slate.md`](file:///C:/dev/grove-v9/docs/product/definitions/Slate.md) |
| [ADR-032](file:///C:/dev/grove-v9/docs/specs/hud-system/ADR-032-Spatial-Layer-Manager-And-Navigation.md) | Spatial Layer Manager and Navigation Specifications | HUD System | Accepted | [`Layer.md`](file:///C:/dev/grove-v9/docs/product/definitions/Layer.md) |
| [ADR-040](file:///C:/dev/grove-v9/docs/specs/spatial-layers/ADR-040-Spatial-Layer-System-Architecture.md) | Spatial Layer System Architecture | Spatial Layers | PARTIAL — verified stack and migration seam; primitive-ID consumers remain | [`SpatialLayerStack.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/SpatialLayerStack.cs) |
| [ADR-041](file:///C:/dev/grove-v9/docs/specs/spatial-layers/ADR-041-Vertical-Aura-Permeability-And-Attenuation.md) | Vertical Aura Permeability and Attenuation Physics | Spatial Layers | PARTIAL — verified scalar attenuation; dedicated engine/culling remain | [`SpatialLayerStack.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/SpatialLayerStack.cs) |
| [ADR-042](file:///C:/dev/grove-v9/docs/specs/spatial-layers/ADR-042-Spatial-Layer-State-And-Activation.md) | Spatial Layer State and Activation | Spatial Layers | PARTIAL — verified visibility/lock/ghost slice; activation modes remain | [`SpatialLayerStack.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/SpatialLayerStack.cs) |
| [ADR-043](file:///C:/dev/grove-v9/docs/specs/spatial-layers/ADR-043-Spatial-Layer-Manager-UI-And-Controls.md) | Spatial Layer Manager UI and Controls | Spatial Layers | PARTIAL — verified slate controls; transfer confirmation/color semantics remain | [`LayerManagerSlate.axaml`](file:///C:/dev/grove-v9/src/GroveApp/Controls/LayerManagerSlate.axaml) |
| [ADR-050](file:///C:/dev/grove-v9/docs/specs/grid-systems/ADR-050-Footprint-Aware-Grid-Cursor-And-Trails.md) | Footprint-Aware Grid Cursor and Spent-Cell Trail Decay System | Grid Systems | PARTIAL — verified footprint cursor/trail behavior; draw-operation seam remains | [`CursorRenderModule.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/CursorRenderModule.cs), [`GridCursorTrail.cs`](file:///C:/dev/grove-v9/src/GroveApp/Models/GridCursorTrail.cs) |
| [ADR-051](file:///C:/dev/grove-v9/docs/specs/grid-systems/ADR-051-Interactive-Resize-And-Cell-Alignment.md) | Interactive Resize Engine and Cell Alignment System | Grid Systems | PARTIAL — verified multi-handle resize; refusal affordance remains | [`GridCanvasControl.cs`](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs) |
| [ADR-052](file:///C:/dev/grove-v9/docs/specs/grid-systems/ADR-052-Content-Anchor-System-And-Memory-Binding.md) | Content Anchor System and Memory Binding Architecture | Grid Systems | PARTIAL — verified memory anchor seam; ribbon/refusal affordance remains | [`GridContentItem.cs`](file:///C:/dev/grove-v9/src/GroveApp/Models/GridContentItem.cs) |
| [ADR-053](file:///C:/dev/grove-v9/docs/specs/grid-systems/ADR-053-Spatial-CRUD-Operations-And-Selection.md) | Spatial CRUD Operations, Selection State Machine, and Marquee Sweep | Grid Systems | PARTIAL — verified selection/clipboard contracts; readout/container remain | [`GridCanvasControl.cs`](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs) |
| [ADR-054](file:///C:/dev/grove-v9/docs/specs/grid-systems/ADR-054-Spatial-Context-Menu-System.md) | Spatial Context Menu System and Zero-Modal Pass-Through Architecture | Grid Systems | PARTIAL — verified right-click menu and commands; long-press remains | [`SpatialContextMenuOverlay.axaml`](file:///C:/dev/grove-v9/src/GroveApp/Controls/SpatialContextMenuOverlay.axaml) |
| [ADR-055](file:///C:/dev/grove-v9/docs/specs/grid-systems/ADR-055-Multi-Item-Selection-And-Group-Translation.md) | Multi-Item Selection Model, Cell-Aligned Marquee Sweep, and Multi-Type Group Translation Engine | Grid Systems | PARTIAL — verified rigid translation/trails; service seam remains | [`GridCanvasControl.cs`](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs) |
| [ADR-056](file:///C:/dev/grove-v9/docs/specs/grid-systems/ADR-056-Interactive-Resize-Geometry-And-Affordances.md) | Interactive Resize Geometry, Type-Specific Footprint Solvers, and Affordance Rendering Engine | Grid Systems | PARTIAL — verified type-specific solvers; cursor transition seam remains | [`GridCanvasControl.cs`](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs) |
| [ADR-057](file:///C:/dev/grove-v9/docs/specs/keybind-arming/ADR-057-Keybind-Arming-And-Ghost-Placement.md) | Keybind Arming State Machine and Ghost Placement Preview | Keybind Arming | PARTIAL — verified arming/ghost/rejection behavior; draw-operation seam remains | [`ToolArmingStateMachine.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/ToolArmingStateMachine.cs), [`GridCanvasControl.cs`](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs) |
| [ADR-058](file:///C:/dev/grove-v9/docs/specs/keybind-arming/ADR-058-Global-Keybind-Focus-Precedence-Router.md) | Global Keybind Focus Precedence Router Architecture | Keybind Arming | PARTIAL — verified focus router and host seam; full key matrix remains | [`GlobalFocusPrecedenceRouter.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/GlobalFocusPrecedenceRouter.cs), [`IKeybindHost.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/IKeybindHost.cs) |
| [ADR-060](file:///C:/dev/grove-v9/docs/specs/fluent-design-system/ADR-060-Fluent-Local-Editor-Notepad-Design.md) | Fluent Local Editor Notepad Design | Native Fluent Design | SUPERSEDED BY ADR-064 | [`FluentNotepadEditor.axaml`](file:///C:/dev/grove-v9/src/GroveApp/Controls/FluentNotepadEditor.axaml) |
| [ADR-061](file:///C:/dev/grove-v9/docs/specs/fluent-design-system/ADR-061-Aura-Field-Fluid-Gradient-Rendering.md) | Aura Field Fluid Gradient Rendering | Native Fluent Design | CONFLICTING / REFUSED BY ADR-002 | [`FieldLedgerModule.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/FieldLedgerModule.cs) |
| [ADR-062](file:///C:/dev/grove-v9/docs/specs/fluent-design-system/ADR-062-Skia-GPU-AntiAliasing-And-Subpixel-Typography.md) | Skia GPU Anti-Aliasing and Subpixel Typography | Native Fluent Design | PARTIAL — verified Avalonia text hints; GPU pipeline remains bounded | [`Typography.cs`](file:///C:/dev/grove-v9/src/GroveApp/DesignSystem/Typography.cs), [`GridCanvasControl.cs`](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs) |
| [ADR-063](file:///C:/dev/grove-v9/docs/specs/fluent-design-system/ADR-063-FluentAvalonia-Mica-Acrylic-Backdrop-System.md) | FluentAvalonia Mica and Acrylic Backdrop System | Native Fluent Design | PARTIAL — verified native dark-mode adapter; material controller remains | [`MainWindow.axaml.cs`](file:///C:/dev/grove-v9/src/GroveApp/MainWindow.axaml.cs) |
| [ADR-070](file:///C:/dev/grove-v9/docs/specs/hud-feedback/ADR-070-HUD-Spatial-Watermark-And-Layer-Identity.md) | HUD Spatial Watermark and Active Layer Identity | HUD Feedback | PARTIAL — verified watermark/rename seam; full ledger sync remains | [`HudSpatialWatermark.cs`](file:///C:/dev/grove-v9/src/GroveApp/Controls/HudSpatialWatermark.cs) |
| [ADR-071](file:///C:/dev/grove-v9/docs/specs/hud-feedback/ADR-071-Layer-Creation-And-Insertion-Feedback-Effects.md) | Layer Creation and Insertion Feedback Effects | HUD Feedback | PARTIAL — verified discrete insertion feedback; refused gradient sweep remains intentionally absent | [`LayerFeedbackAnimationController.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/LayerFeedbackAnimationController.cs), [`GridCanvasControl.cs`](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs) |

---

## 3. Subsystem Category Breakdown

### 3.1 Spatial Grid
Focuses on Plane 0 continuous 2D coordinate system, Skia GPU rendering pipeline, cell discretization, camera transformations, aura physics, and multi-plane visual stack composition.
- [ADR-001: Spatial Grid Plane Architecture](file:///C:/dev/grove-v9/docs/specs/spatial-grid/ADR-001-Spatial-Grid-Plane-Architecture.md)
- [ADR-002: Field Ledger and Subscribers](file:///C:/dev/grove-v9/docs/specs/spatial-grid/ADR-002-Field-Ledger-And-Subscribers.md)
- [ADR-003: Spatial Aura Physics](file:///C:/dev/grove-v9/docs/specs/spatial-grid/ADR-003-Spatial-Aura-Physics.md)
- [ADR-004: Three-Plane Visual Hierarchy](file:///C:/dev/grove-v9/docs/specs/spatial-grid/ADR-004-Three-Plane-Visual-Hierarchy.md)
- [ADR-005: Camera Affine Transform Engine](file:///C:/dev/grove-v9/docs/specs/spatial-grid/ADR-005-Camera-Affine-Transform-Engine.md)

### 3.2 Content Types
Defines physical geometry, cell footprint constraints, authoring tools, AST models, image resolution mappings, drag-and-drop parsing, and clipboard interoperability for core content primitives.
- [ADR-010: Note Physical Geometry, Authored Fills, and Grid Placement](file:///C:/dev/grove-v9/docs/specs/content-types/ADR-010-Note-Physical-Geometry-And-Fills.md)
- [ADR-011: Document Physical Geometry, Page Texture, AST, and Multi-Column Reflow Engine](file:///C:/dev/grove-v9/docs/specs/content-types/ADR-011-Document-Physical-Geometry-And-Reflow.md)
- [ADR-012: Image Footprint Resolution Mapping, Zero-Crop Rules, and Skia Bitmap Sampling](file:///C:/dev/grove-v9/docs/specs/content-types/ADR-012-Image-Footprint-Resolution-Mapping.md)
- [ADR-013: External Drag-and-Drop System, ScreenToCell Coordinate Resolution, and Content Auto-Creation](file:///C:/dev/grove-v9/docs/specs/content-types/ADR-013-External-Drag-And-Drop-System.md)
- [ADR-014: Native Clipboard HTML and RichText Interoperability Specification](file:///C:/dev/grove-v9/docs/specs/content-types/ADR-014-Native-Clipboard-HTML-And-RichText-Interop.md)

### 3.3 Memory System
Specifies the underlying semantic payload ledger, time-ordered UUIDv7 identification, SHA-256 deduplication, version lineage DAG trees, and multi-layer spatial R-Tree spatial indexing.
- [ADR-020: Memory Model and Immutable Ledger Architecture](file:///C:/dev/grove-v9/docs/specs/memory-system/ADR-020-Memory-Model-And-Immutable-Ledger.md)
- [ADR-021: Memory Lineage and Version Tree Architecture](file:///C:/dev/grove-v9/docs/specs/memory-system/ADR-021-Memory-Lineage-And-Version-Tree.md)
- [ADR-022: Memory Spatial R-Tree Index Architecture](file:///C:/dev/grove-v9/docs/specs/memory-system/ADR-022-Memory-Spatial-RTree-Index.md)

### 3.4 HUD System
Validates native Avalonia 11.2.5 GPU composition across Plane 0, Plane 1, and Plane 2, establishing Slate window anatomical rules and spatial layer navigation models.
- [ADR-030: Three-Plane Compositor Architecture Validation](file:///C:/dev/grove-v9/docs/specs/hud-system/ADR-030-Three-Plane-Compositor-Architecture-Validation.md)
- [ADR-031: Slate Window System and Anatomy Specifications](file:///C:/dev/grove-v9/docs/specs/hud-system/ADR-031-Slate-Window-System-And-Anatomy.md)
- [ADR-032: Spatial Layer Manager and Navigation Specifications](file:///C:/dev/grove-v9/docs/specs/hud-system/ADR-032-Spatial-Layer-Manager-And-Navigation.md)

### 3.5 Spatial Layers
Defines vertical layer ordering ($Z \in [-10, +10]$), spatial frequency bands, vertical aura permeability, layer activation state transitions, and HUD layer management controls.
- [ADR-040: Spatial Layer System Architecture](file:///C:/dev/grove-v9/docs/specs/spatial-layers/ADR-040-Spatial-Layer-System-Architecture.md)
- [ADR-041: Vertical Aura Permeability and Attenuation Physics](file:///C:/dev/grove-v9/docs/specs/spatial-layers/ADR-041-Vertical-Aura-Permeability-And-Attenuation.md)
- [ADR-042: Spatial Layer State and Activation](file:///C:/dev/grove-v9/docs/specs/spatial-layers/ADR-042-Spatial-Layer-State-And-Activation.md)
- [ADR-043: Spatial Layer Manager UI and Controls](file:///C:/dev/grove-v9/docs/specs/spatial-layers/ADR-043-Spatial-Layer-Manager-UI-And-Controls.md)

### 3.6 Grid Systems
Enforces footprint-aware cursor movement, spent-cell motion trail decay, cell-aligned resize mechanics, selection state machines, marquee sweeps, zero-modal context menus, and multi-type translation.
- [ADR-050: Footprint-Aware Grid Cursor and Spent-Cell Trail Decay System](file:///C:/dev/grove-v9/docs/specs/grid-systems/ADR-050-Footprint-Aware-Grid-Cursor-And-Trails.md)
- [ADR-051: Interactive Resize Engine and Cell Alignment System](file:///C:/dev/grove-v9/docs/specs/grid-systems/ADR-051-Interactive-Resize-And-Cell-Alignment.md)
- [ADR-052: Content Anchor System and Memory Binding Architecture](file:///C:/dev/grove-v9/docs/specs/grid-systems/ADR-052-Content-Anchor-System-And-Memory-Binding.md)
- [ADR-053: Spatial CRUD Operations, Selection State Machine, and Marquee Sweep](file:///C:/dev/grove-v9/docs/specs/grid-systems/ADR-053-Spatial-CRUD-Operations-And-Selection.md)
- [ADR-054: Spatial Context Menu System and Zero-Modal Pass-Through Architecture](file:///C:/dev/grove-v9/docs/specs/grid-systems/ADR-054-Spatial-Context-Menu-System.md)
- [ADR-055: Multi-Item Selection Model, Cell-Aligned Marquee Sweep, and Multi-Type Group Translation Engine](file:///C:/dev/grove-v9/docs/specs/grid-systems/ADR-055-Multi-Item-Selection-And-Group-Translation.md)
- [ADR-056: Interactive Resize Geometry, Type-Specific Footprint Solvers, and Affordance Rendering Engine](file:///C:/dev/grove-v9/docs/specs/grid-systems/ADR-056-Interactive-Resize-Geometry-And-Affordances.md)

### 3.7 Keybind Arming
Implements two-stage keybind arming (`N` Note, `D` Document, `P` Picture), ghost placement previews, cell collision validation, and deterministic input focus routing.
- [ADR-057: Keybind Arming State Machine and Ghost Placement Preview](file:///C:/dev/grove-v9/docs/specs/keybind-arming/ADR-057-Keybind-Arming-And-Ghost-Placement.md)
- [ADR-058: Global Keybind Focus Precedence Router Architecture](file:///C:/dev/grove-v9/docs/specs/keybind-arming/ADR-058-Global-Keybind-Focus-Precedence-Router.md)

### 3.8 Native Fluent Design
Delivers Windows 11 Fluent Design System aesthetics through Skia GPU subpixel rendering, fluid aura field heatmaps, local editor overlays, and FluentAvalonia Mica/Acrylic materials.
- [ADR-060: Fluent Local Editor Notepad Design](file:///C:/dev/grove-v9/docs/specs/fluent-design-system/ADR-060-Fluent-Local-Editor-Notepad-Design.md)
- [ADR-061: Aura Field Fluid Gradient Rendering](file:///C:/dev/grove-v9/docs/specs/fluent-design-system/ADR-061-Aura-Field-Fluid-Gradient-Rendering.md)
- [ADR-062: Skia GPU Anti-Aliasing and Subpixel Typography](file:///C:/dev/grove-v9/docs/specs/fluent-design-system/ADR-062-Skia-GPU-AntiAliasing-And-Subpixel-Typography.md)
- [ADR-063: FluentAvalonia Mica and Acrylic Backdrop System](file:///C:/dev/grove-v9/docs/specs/fluent-design-system/ADR-063-FluentAvalonia-Mica-Acrylic-Backdrop-System.md)

### 3.9 HUD Feedback
Provides dynamic visual feedback for active layer identity, spatial watermark overlays, and spring-damper motion physics for layer creation and insertion.
- [ADR-070: HUD Spatial Watermark and Active Layer Identity](file:///C:/dev/grove-v9/docs/specs/hud-feedback/ADR-070-HUD-Spatial-Watermark-And-Layer-Identity.md)
- [ADR-071: Layer Creation and Insertion Feedback Effects](file:///C:/dev/grove-v9/docs/specs/hud-feedback/ADR-071-Layer-Creation-And-Insertion-Feedback-Effects.md)
