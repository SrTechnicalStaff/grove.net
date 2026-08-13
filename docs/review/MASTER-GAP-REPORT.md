# Grove v9 — Master Architectural Gap & Work-to-Be-Done Report

| Property | Value |
| :--- | :--- |
| **Document Type** | Comprehensive Master Gap & Remediation Roadmap |
| **Source Basis** | 35 Standalone RCA Ledgers in [`docs/review/`](file:///C:/dev/grove-v9/docs/review/) |
| **Target Codebase** | [`src/GroveApp/`](file:///C:/dev/grove-v9/src/GroveApp/) (.NET 9 / C# 13 / Avalonia 11.2.5 / SkiaSharp 3.x) |
| **Audit Standard** | Strict User-Observable and Interactive Live UI Compliance |
| **Date** | 2026-08-12 |

> **Status note:** This document is the historical audit baseline, not a
> current implementation ledger. Several items below have since been repaired
> or moved behind explicit seams. See [`REMEDIATION-STATUS.md`](REMEDIATION-STATUS.md)
> and the ADR statuses for the current source-and-behavior result.

---

## 1. Executive Summary & Audit Baseline

An exhaustive audit of all 35 Architecture Decision Records (ADRs) across the 9 primary architectural subsystems of **Grove v9** was conducted against the executable application codebase in [`src/GroveApp/`](file:///C:/dev/grove-v9/src/GroveApp/).

### Master Status Distribution
- **Truly Implemented (Observable & Interactive)**: **12 ADRs (34.3%)** — *Core 2D Camera, Note Primitives, Document Reflow, Image Footprints, OS Drag-and-Drop, Clipboard Interop, Spent Cell Trails, Basic Snap Resize, Anchoring Pin, Selection Sweep, Keybind Router, Window Transparency Hints.*
- **Partially Implemented (Sub-Set Logic / UI Missing)**: **13 ADRs (37.1%)** — *Grid Lines, Field Ledger, Aura Physics, Three-Plane Compositor, Spatial Layer Stack, Permeability, Layer State, Footprint Cursor, Selection CRUD, Multi-Item Group Drag, Type Resize Solvers, Keybind Arming, Local Editor Overlay, Fluent Backdrops, Layer Feedback.*
- **False / Unimplemented (0% Interactive UI Implementation)**: **10 ADRs (28.6%)** — *Memory Model Ledger, Memory Lineage Version Tree, Memory Spatial R-Tree, Named Slate Window System, Spatial Context Menu, Spatial Layer Manager UI, Fluid Aura Shaders, Skia GPU Pipeline, HUD Spatial Watermark, Layer Creation Flash Sweep.*

---

## 2. Categorized Inventory of Missing Work to Be Done

This section lists every missing contract, class, interface, method, UI control, Skia draw operation, physics formula, animation, and token required to bring Grove v9 into 100% compliance with its architectural specifications.

---

### Subsystem 1: Spatial Grid (ADR-001 to ADR-005)

#### 1. Grid Line Engine ([`ADR-001`](file:///C:/dev/grove-v9/docs/specs/spatial-grid/ADR-001-Spatial-Grid-Plane-Architecture.md))
- [ ] **Supercell Grid Line Tier**: Implement the 1100px Supercell grid line rendering tier in [`GridLineModule.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/GridLineModule.cs).
- [ ] **Configuration Model**: Create `SpatialGridGeometryConfig` record struct replacing raw static fields.
- [ ] **Subpixel Hairline Snapping**: Implement DPI-aware pixel snapping math $x_{\text{snap}} = \frac{\lfloor x \cdot \text{DPI}\rfloor}{\text{DPI}}$ to eliminate subpixel grid line blur during camera translation.

#### 2. Field Ledger & Subscribers ([`ADR-002`](file:///C:/dev/grove-v9/docs/specs/spatial-grid/ADR-002-Field-Ledger-And-Subscribers.md))
- [ ] **Event-Driven Pub-Sub Architecture**: Create `FieldLedgerManager` to dispatch cell ledger mutations to field subscribers reactively instead of polling per render frame.
- [ ] **Perimeter Ring Subscriber**: Build `PerimeterRingSubscriber` rendering 1.5px isoline vector contour rings around contiguous cell regions where total field energy $E \ge 0.15$.
- [ ] **Annotation Metadata Subscriber**: Build `AnnotationMetadataSubscriber` to filter and qualify cross-layer note metadata and link indicators.

#### 3. Spatial Aura Physics ([`ADR-003`](file:///C:/dev/grove-v9/docs/specs/spatial-grid/ADR-003-Spatial-Aura-Physics.md))
- [ ] **View-Distance Representation Tiers**: Implement 5 formal representation tiers (`WV-00` Macro to `WV-04` Micro) with stand-in hysteresis zoom bounds to govern level-of-detail rendering.
- [ ] **Isoline Containment Rendering**: Wire dynamic isoline boundary paths on Plane 0 when energy field gradients cross threshold $\nabla E \ge 0.10$.

#### 4. Three-Plane Visual Compositor ([`ADR-004`](file:///C:/dev/grove-v9/docs/specs/spatial-grid/ADR-004-Three-Plane-Visual-Hierarchy.md))
- [ ] **Compositor Architecture**: Create `ThreePlaneVisualCompositorContainer`, `IPlaneView`, and `IPlaneCompositor` replacing raw Avalonia `<Grid>` layout stacking in [`MainWindow.axaml`](file:///C:/dev/grove-v9/src/GroveApp/MainWindow.axaml).
- [ ] **Top-Down Input Routing**: Implement deterministic top-down hit testing routing pointer events strictly from Plane 2 (HUD) $\rightarrow$ Plane 1 (Information) $\rightarrow$ Plane 0 (Canvas).

#### 5. Camera Affine Engine ([`ADR-005`](file:///C:/dev/grove-v9/docs/specs/spatial-grid/ADR-005-Camera-Affine-Transform-Engine.md))
- [ ] **VSync Inertia Physics**: Implement exponential spring-dampening inertia physics ($T(t) = T_0 \cdot e^{-\lambda t}$) for smooth camera pan and focal-point zoom release.

---

### Subsystem 2: Content Types (ADR-010 to ADR-014)

#### 1. Note Physical Geometry ([`ADR-010`](file:///C:/dev/grove-v9/docs/specs/content-types/ADR-010-Note-Physical-Geometry-And-Fills.md))
- [ ] **LOD Text Abbreviation Rules**: Implement strict text truncation and micro-pill rendering when camera zoom drops below $35\%$ (`WV-02`).

#### 2. Document Surface Engine ([`ADR-011`](file:///C:/dev/grove-v9/docs/specs/content-types/ADR-011-Document-Physical-Geometry-And-Reflow.md))
- [ ] **Interactive Document Editor**: Build dedicated `DocumentEditorOverlay.axaml` allowing users to edit Document Markdown AST text directly in live UI.
- [ ] **Page Pagination Controls**: Add interactive page turning buttons (`< Page X of Y >`) and column count toggles to `GridDocument` rendered surface on Plane 0.

#### 3. Image Resolution Engine ([`ADR-012`](file:///C:/dev/grove-v9/docs/specs/content-types/ADR-012-Image-Footprint-Resolution-Mapping.md))
- [ ] **Interactive Image Property Overlay**: Build image property HUD popover showing Effective PPI, natural dimensions, file size, aspect ratio lock toggle, and zero-crop alignment rules.

#### 4. Drag-and-Drop & Clipboard Interop ([`ADR-013`](file:///C:/dev/grove-v9/docs/specs/content-types/ADR-013-External-Drag-And-Drop-System.md), [`ADR-014`](file:///C:/dev/grove-v9/docs/specs/content-types/ADR-014-Native-Clipboard-HTML-And-RichText-Interop.md))
- [ ] **Drop Placement Hover Preview**: Render cell-aligned ghost placement box during active OS drag operation before mouse release.
- [ ] **Rich Clipboard Visual Paste Indicator**: Add visual HUD toast feedback when `CF_HTML` or bitmap data is pasted onto canvas.

---

### Subsystem 3: Memory System (ADR-020 to ADR-022)

#### 1. Immutable Memory Model ([`ADR-020`](file:///C:/dev/grove-v9/docs/specs/memory-system/ADR-020-Memory-Model-And-Immutable-Ledger.md))
- [ ] **`ContentHash` Value Object**: Implement SHA-256 binary content hashing for all grid items.
- [ ] **`MemoryRecord` Ledger Store**: Build time-ordered UUIDv7 memory record ledger tracking creation, edits, and deletions.
- [ ] **`MemoryAnchor` Disk Synchronization**: Build YAML frontmatter parser/serializer synchronizing grid anchors with local `.md` disk files (`anchors: [...]`).

#### 2. Memory Lineage & Version Tree ([`ADR-021`](file:///C:/dev/grove-v9/docs/specs/memory-system/ADR-021-Memory-Lineage-And-Version-Tree.md))
- [ ] **`MemoryVersionTree` DAG Topology**: Build Directed Acyclic Graph (DAG) tracking document edit histories.
- [ ] **Myers Diff Engine**: Implement Myers line/chunk diffing algorithm to compute version deltas.
- [ ] **LCA & 3-Way Merge**: Build Lowest Common Ancestor (LCA) tree search and 3-way merge resolution engine.

#### 3. Spatial R-Tree Index ([`ADR-022`](file:///C:/dev/grove-v9/docs/specs/memory-system/ADR-022-Memory-Spatial-RTree-Index.md))
- [ ] **`MemorySpatialRTree`**: Implement 2D R-Tree spatial bounding box index replacing $O(N)$ linear scans in [`GridCanvasControl.cs`](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs).
- [ ] **`SpatialQueryCache`**: Build LRU spatial query cache for viewport frustum culling.

---

### Subsystem 4: HUD System (ADR-030 to ADR-032)

#### 1. Three-Plane Compositor Host ([`ADR-030`](file:///C:/dev/grove-v9/docs/specs/hud-system/ADR-030-Three-Plane-Compositor-Architecture-Validation.md))
- [ ] **Compositor Control**: Implement `ThreePlaneCompositorHost` as the root window content view in [`MainWindow.axaml`](file:///C:/dev/grove-v9/src/GroveApp/MainWindow.axaml).

#### 2. Named Slate Window System ([`ADR-031`](file:///C:/dev/grove-v9/docs/specs/hud-system/ADR-031-Slate-Window-System-And-Anatomy.md))
- [ ] **Named-Slate frame control**: Build viewport-fixed container controls for Writing, Memory, and Gallery Slates adhering to design tokens (`--surface-chrome`, `1px` `--k-slate-b`).
- [ ] **Specialized Slates**: Build `WritingSlate`, `MemorySlate`, `GallerySlate`, and `MasonryGalleryPanel`.

#### 3. Spatial Layer Navigation ([`ADR-032`](file:///C:/dev/grove-v9/docs/specs/hud-system/ADR-032-Spatial-Layer-Manager-And-Navigation.md))
- [ ] **Extended Hotkeys**: Implement `Shift+[` and `Shift+]` for top/bottom layer jumping, and `Ctrl+Shift+L` for toggling the Layer Manager overlay.

---

### Subsystem 5: Spatial Layers (ADR-040 to ADR-043)

#### 1. Spatial Layer System Architecture ([`ADR-040`](file:///C:/dev/grove-v9/docs/specs/spatial-layers/ADR-040-Spatial-Layer-System-Architecture.md))
- [ ] **Strongly-Typed Layer Identifiers**: Create `LayerId` and `LayerLabel` value types (`01`, `02`, `B01`) replacing primitive `int` IDs.
- [ ] **Item Layer Migration**: Implement `LayerMigrationResult` struct and item migration pipeline for moving content items across spatial layers.

#### 2. Vertical Aura Permeability Physics ([`ADR-041`](file:///C:/dev/grove-v9/docs/specs/spatial-layers/ADR-041-Vertical-Aura-Permeability-And-Attenuation.md))
- [ ] **`VerticalAuraPermeabilityEngine`**: Build SIMD-accelerated permeability engine evaluating vertical aura attenuation $0.5^{|\Delta L|}$ across all 12 spatial layers simultaneously.
- [ ] **3D Spatial Layer Culling**: Implement depth-based z-index frustum culling for non-permeable layer content.

#### 3. Spatial Layer State & Activation ([`ADR-042`](file:///C:/dev/grove-v9/docs/specs/spatial-layers/ADR-042-Spatial-Layer-State-And-Activation.md))
- [ ] **`ILayerActivationManager`**: Build layer state machine supporting Active, Inactive, Solo ($\gamma=0$), and Locked operational states.
- [ ] **Ghost Presence Silhouettes**: Render low-opacity ghost presence outlines on Plane 0 for content items residing on inactive spatial layers.

#### 4. Spatial Layer Manager UI ([`ADR-043`](file:///C:/dev/grove-v9/docs/specs/spatial-layers/ADR-043-Spatial-Layer-Manager-UI-And-Controls.md))
- [ ] **Layer Manager overlay control**: Build viewport-fixed Plane 2 overlay control featuring:
  - Header row with stack count (`12 LAYERS`).
  - Search filter text field (`Find a Layer`).
  - 12 layer rows with fixed `4ch` monospaced labels (`B01`, `01`, `02`), layer display names, color indicator dots, visibility toggle buttons (`[Visible]`/`[Hidden]`), and active layer selection outline (`2px` `--signal-interaction`).
  - Inline destructive removal confirmation box (`"Remove Layer 02 and move what is on it to Layer 01? [Keep it] [Remove]"`).

---

### Subsystem 6: Grid Systems (ADR-050 to ADR-056)

#### 1. Footprint-Aware Grid Cursor ([`ADR-050`](file:///C:/dev/grove-v9/docs/specs/grid-systems/ADR-050-Footprint-Aware-Grid-Cursor-And-Trails.md))
- [ ] **`IGridCursorService` & Skia Draw Operation**: Implement `IGridCursorService` and `GridCursorDrawOperation` (`ICustomDrawOperation`) running on GPU compositor.
- [ ] **`CursorRole` Action Recoloring**: Dynamically recolor cursor ring based on active tool role (`#3B82F6` select, `#F59E0B` resize, `#10B981` arm).
- [ ] **Multi-Type Footprint Expansion**: Expand grid cursor footprint automatically when hovering over `GridDocument` and `GridImage` items.

#### 2. Interactive Resize Engine ([`ADR-051`](file:///C:/dev/grove-v9/docs/specs/grid-systems/ADR-051-Interactive-Resize-And-Cell-Alignment.md))
- [ ] **4-Corner Resize Handles**: Add Northwest ($NW$), Northeast ($NE$), and Southwest ($SW$) resize handles in addition to Southeast ($SE$).
- [ ] **`ISpatialResizeService` & Draw Operation**: Implement `ISpatialResizeService` and `ResizePreviewDrawOperation`.
- [ ] **Collision Refusal Feedback**: Render 45-degree diagonal cross-hatching (`#EF4444`) and point-of-action refusal strip toolbar (`"This space is occupied"`) when resize overlaps occupied cells.

#### 3. Content Anchor System ([`ADR-052`](file:///C:/dev/grove-v9/docs/specs/grid-systems/ADR-052-Content-Anchor-System-And-Memory-Binding.md))
- [ ] **`IMemoryAnchorService` & Anchor Ribbon**: Implement `IMemoryAnchorService` and `AnchorRibbonDrawOperation` rendering a $16 \times 24\text{px}$ inverted V-notch apex with centered 10pt serif `A` lettermark.
- [ ] **Refusal Signal on Drag**: Flash red refusal ring and camera jitter when user attempts to drag an anchored item without un-pinning first.

#### 4. Selection State Machine & CRUD ([`ADR-053`](file:///C:/dev/grove-v9/docs/specs/grid-systems/ADR-053-Spatial-CRUD-Operations-And-Selection.md))
- [ ] **`ISelectionService` & `ISpatialCrudService`**: Implement decoupled selection and CRUD service interfaces.
- [ ] **Marquee Size Readout**: Render monospaced cell count badge (e.g., `3 × 3`) at bottom-right of active marquee selection box.
- [ ] **`SpatialClipboardContainer`**: Create structured binary/JSON container format for spatial clipboard data.

#### 5. Spatial Context Menu System ([`ADR-054`](file:///C:/dev/grove-v9/docs/specs/grid-systems/ADR-054-Spatial-Context-Menu-System.md))
- [ ] **`ISpatialContextMenuService` & View**: Build `ISpatialContextMenuService` and `SpatialContextMenuOverlayView.axaml` composed on Plane 2.
- [ ] **Context Menu Interactions**: Wire right-click and long-press pointer events on Plane 0 to open a zero-modal context menu displaying: `Create Note (N)`, `Create Document (D)`, `Paste (Ctrl+V)`, `Anchor (A)`, `Delete (Del)`.

#### 6. Multi-Item Selection & Group Translation ([`ADR-055`](file:///C:/dev/grove-v9/docs/specs/grid-systems/ADR-055-Multi-Item-Selection-And-Group-Translation.md))
- [ ] **`ISpatialGroupTranslationEngine` & `SelectionQueue`**: Implement decoupled group translation engine and selection queue class.
- [ ] **Strict 50% Area Overlap Rule**: Enforce $\ge 50\%$ area overlap ratio $\Phi(I_k, M) \ge 0.50$ for marquee sweep selection.
- [ ] **Vacated Cells Spent Trail Emission**: Emit spent cell motion decay trails along the movement path of all items in a translated cluster ($\mathcal{V} = \mathcal{U}_{\text{source}} \setminus \mathcal{U}_{\text{target}}$).

#### 7. Type-Specific Footprint Solvers ([`ADR-056`](file:///C:/dev/grove-v9/docs/specs/grid-systems/ADR-056-Interactive-Resize-Geometry-And-Affordances.md))
- [ ] **`IInteractiveResizeEngine` & Solvers**: Build `IInteractiveResizeEngine` and `TypeSpecificFootprintSolver` static class supporting:
  - Document solver ($2\times 2$ to $8\times 8$ aspect ratio rules).
  - Image 256px scale divisor aspect ratio solver.
- [ ] **System Cursor Hover Transitions**: Trigger Avalonia system mouse cursor state changes (`SizeNWSE`, `SizeNESW`) when hovering over resize handle bounds.

---

### Subsystem 7: Keybind Arming (ADR-057, ADR-058)

#### 1. Tool Arming State Machine ([`ADR-057`](file:///C:/dev/grove-v9/docs/specs/keybind-arming/ADR-057-Keybind-Arming-And-Ghost-Placement.md))
- [ ] **`IToolArmingService` & Draw Operation**: Implement `IToolArmingService` interface and `GhostPlacementDrawOperation` Skia custom draw operation.
- [ ] **Visual Rejection Pulse**: Trigger 200ms red flash and camera jitter pulse when user clicks to place an armed item on an occupied cell.

#### 2. Global Keybind Precedence Router ([`ADR-058`](file:///C:/dev/grove-v9/docs/specs/keybind-arming/ADR-058-Global-Keybind-Focus-Precedence-Router.md))
- [ ] **Full Keybind Coverage**: Expand [`GlobalFocusPrecedenceRouter.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/GlobalFocusPrecedenceRouter.cs) to route all 11 global hotkeys (`Space`, `Ctrl+Enter`, `Ctrl+E`, `Tab`, `N`, `Shift+N`, `D`, `P`, `A`, `Del`, `Esc`, `Ctrl+C`, `Ctrl+V`, `[`, `]`) during the Tunneling phase.
- [ ] **Attached Properties**: Implement `FocusPrecedenceAttachedProperties` static class for XAML focus management.

---

### Subsystem 8: Native Fluent Design (ADR-060 to ADR-063)

#### 1. Fluent Local Editor Notepad ([`ADR-060`](file:///C:/dev/grove-v9/docs/specs/fluent-design-system/ADR-060-Fluent-Local-Editor-Notepad-Design.md))
- [ ] **Standalone Window Hosting**: Re-host `LocalEditorOverlay` as a standalone `fa:AppWindow` / `FluentWindow` rather than an in-canvas `UserControl`.
- [ ] **Design Token Compliance**: Update styling to 8px `--r-md` corner radius, role border `--k-edit-b` (`#7A3F3A`), `--measure-reading` (68ch / 640px) frame sizing, and `ui:Segmented` mode toggle control.
- [ ] **Dual-Viewport Scroll Synchronization**: Implement scroll sync equation $y_{\text{target}} = y_{\text{source}} \cdot \frac{H_{\text{target}}}{H_{\text{source}}}$.

#### 2. Fluid Aura Gradient Rendering ([`ADR-061`](file:///C:/dev/grove-v9/docs/specs/fluent-design-system/ADR-061-Aura-Field-Fluid-Gradient-Rendering.md))
- [ ] **`FluidAuraRenderer` Class**: Build `FluidAuraRenderer` evaluating SkiaSharp radial gradient shaders (`SKShader.CreateRadialGradient`).
- [ ] **Hermite Smoothstep Falloff**: Implement Hermite smoothstep alpha falloff $A(t) = \alpha_{\text{peak}} (1 - 3t^2 + 2t^3)$.
- [ ] **4-Pass Path Difference Line Suppression**: Execute 4-pass path difference subtraction ($\mathcal{P}_{\text{visible}} = \mathcal{P}_{\text{grid}} \setminus \Omega_{\text{aura}}$) to suppress dark grid lines beneath active energy heatmaps.

#### 3. Skia GPU Anti-Aliasing & Subpixel Typography ([`ADR-062`](file:///C:/dev/grove-v9/docs/specs/fluent-design-system/ADR-062-Skia-GPU-AntiAliasing-And-Subpixel-Typography.md))
- [ ] **`SkiaGpuPipelineManager` & Context**: Build `SkiaGpuPipelineManager`, `SkiaPaintDefaults`, `PathCacheKey` struct, and hardware `GRContext` GPU context bindings.
- [ ] **1/4 DIP Subpixel Quantization**: Implement 1/4 DIP subpixel text positioning math $\mathbf{x}_{\text{subpixel}} = \frac{\lfloor 4\mathbf{x} + 0.5\mathbf{1}\rfloor}{4}$ and `SKFontEdging.SubpixelAntialias`.
- [ ] **Vector Path Outlines & LRU Cache**: Render vector path outlines when zoom $s > 2.5$ and implement GPU LRU path cache.

#### 4. Native Windows 11 DWM Backdrops ([`ADR-063`](file:///C:/dev/grove-v9/docs/specs/fluent-design-system/ADR-063-FluentAvalonia-Mica-Acrylic-Backdrop-System.md))
- [ ] **Native DWM P/Invoke Interop**: Implement `NativeDwmApi` Win32 P/Invoke calls (`DwmSetWindowAttribute` for `DWMWA_USE_IMMERSIVE_DARK_MODE`).
- [ ] **Glassmorphic Material Scaling**: Implement optical blur & tint scaling formulas ($R_{\text{blur}}(z) = 10 + 10z$, $\alpha_{\text{tint}}(z) = 0.85(1 - 0.12z)$) and `FluentWindowBackdropManager`.

---

### Subsystem 9: HUD Feedback (ADR-070, ADR-071)

#### 1. HUD Spatial Watermark & Layer Identity ([`ADR-070`](file:///C:/dev/grove-v9/docs/specs/hud-feedback/ADR-070-HUD-Spatial-Watermark-And-Layer-Identity.md))
- [ ] **`HudSpatialWatermarkControl.axaml`**: Build viewport-fixed HUD control in bottom-right margin (`Right: 16px`, `Bottom: 16px`) containing:
  - Brand token `GROVE v9` (`W-02`).
  - Camera scale readout `100%` (`W-03`).
  - Active layer label token `01` in role ink `--k-layer` (`#E2A6C6`, `W-05`).
  - Active layer display name block (`W-06`).
  - Inline rename editor field (`W-07` `PART_RenameTextBox`).
- [ ] **Inline Rename & Validation State Machine**:
  - Double-click or `F2` triggers `W-07` inline text box with automatic focus capture.
  - Commit on `Enter` or `Blur`; cancel on `Escape`.
  - Duplicate name collision displays refusal ink `--c-invalid` (`#E2625C`) and blocks commit.
- [ ] **`ISpatialWatermarkService` & Ledger Sync**: Build reactive watermark service propagating layer name changes directly into `FieldLedgerEngine` and the Layer Manager overlay.

#### 2. Layer Creation & Insertion Feedback ([`ADR-071`](file:///C:/dev/grove-v9/docs/specs/hud-feedback/ADR-071-Layer-Creation-And-Insertion-Feedback-Effects.md))
- [ ] **`FlashSweepDrawOperation`**: Build Plane 0 GPU Skia custom draw operation (`ICustomDrawOperation`) rendering a 480ms radial flash sweep and Gaussian aura pulse wave $E(r,t) = E_{\text{peak}} \cdot \exp\left(-\frac{(r - v_{\text{wave}} t)^2}{2\sigma^2}\right)$.
- [ ] **`LayerFeedbackAnimationController`**: Build animation controller orchestrating Plane 0 canvas radial sweep (`480ms` `--ease`) and Plane 2 `LayerManager` stack row height expansion (`0px` $\rightarrow$ `36px`, `280ms` `--overshoot`).
- [ ] **OS Reduced-Motion Integration**: Collapse all motion duration tokens to `0ms` when OS `prefers-reduced-motion` is active.
- [ ] **Layer Creation Pipeline**: Wire `Ctrl+Shift+N` hotkey, Layer Manager overlay `[+ New Layer]` control, and context menu actions to dispatch the layer creation animation pipeline.

---

## 3. Recommended Phased Implementation Sequence

To execute this work systematically without introducing regression risks or breaking existing functional tests, implementation should proceed through 5 ordered phases:

```
+-----------------------------------------------------------------------------------+
| PHASE 1: Core System Contracts & Interfaces                                       |
| - Define missing interfaces: IGridCursorService, ISpatialResizeService,          |
|   ISpatialContextMenuService, IMemoryAnchorService, ISpatialLayerStateService,    |
|   ISpatialWatermarkService, IToolArmingService.                                   |
+-----------------------------------------------------------------------------------+
                                         |
                                         v
+-----------------------------------------------------------------------------------+
| PHASE 2: HUD Plane Controls & Named Slates (Plane 2)                              |
| - Build the Layer Manager overlay (ADR-043).                                      |
| - Build HudSpatialWatermarkControl.axaml with inline rename & validation (ADR-070).|
| - Build SpatialContextMenuOverlayView.axaml on right-click (ADR-054).            |
+-----------------------------------------------------------------------------------+
                                         |
                                         v
+-----------------------------------------------------------------------------------+
| PHASE 3: Skia GPU Custom Draw Operations & Motion (Plane 0)                      |
| - Build FlashSweepDrawOperation (480ms radial sweep & aura pulse wave) (ADR-071).  |
| - Build FluidAuraRenderer with SKShader radial gradients & line suppression (ADR-061).|
| - Build GridCursorDrawOperation & AnchorRibbonDrawOperation (ADR-050, ADR-052).  |
+-----------------------------------------------------------------------------------+
                                         |
                                         v
+-----------------------------------------------------------------------------------+
| PHASE 4: Memory System Ledger & Spatial R-Tree Index                              |
| - Build MemoryRecord UUIDv7 ledger & ContentHash SHA-256 digests (ADR-020).      |
| - Build MemorySpatialRTree 2D index replacing O(N) canvas scans (ADR-022).        |
| - Build MemoryVersionTree DAG & Myers 3-way merge engine (ADR-021).              |
+-----------------------------------------------------------------------------------+
                                         |
                                         v
+-----------------------------------------------------------------------------------+
| PHASE 5: Advanced Engineering & GPU Pipeline Polish                               |
| - Build SkiaGpuPipelineManager, 1/4 DIP subpixel math & path cache (ADR-062).    |
| - Re-host LocalEditorOverlay as standalone fa:AppWindow (ADR-060).               |
| - Implement Win32 NativeDwmApi Mica/Acrylic backdrop interop (ADR-063).           |
+-----------------------------------------------------------------------------------+
```

---

## 4. Verification & Sign-off Checklist

Before declaring any ADR fully implemented moving forward:
1. [ ] **User-Observable Verification**: Feature must be triggered interactively in the live UI with visible feedback.
2. [ ] **Contract Compliance**: All interfaces, structs, and events specified in the ADR must exist in code.
3. [ ] **Design System Compliance**: Styling must strictly reference tokens from `Colors.cs`, `Tokens.cs`, `Typography.cs`, and `Motion.cs`.
4. [ ] **Build Hygiene**: `dotnet build src/GroveApp/GroveApp.csproj` must compile with **0 Warnings and 0 Errors**.
