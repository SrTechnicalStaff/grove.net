# Root Cause Analysis (RCA) Ledger: ADR-004

## 1. Executive Metadata

| Property | Value |
| :--- | :--- |
| **ADR ID** | ADR-004 |
| **ADR Title** | Three-Plane Visual Hierarchy |
| **Category** | Visual Architecture / Compositor & Input Pipeline |
| **Claimed Status** | IMPLEMENTED - AWAITING USER REVIEW |
| **Verified Status** | PARTIALLY IMPLEMENTED (Visual plane stacking present in XAML; formal compositor interfaces, unified top-down input router container, and z-index bands missing) |
| **Audited Runtime Target** | C# 13 / .NET 9.0 / Avalonia UI 11.2.5 |
| **Audit Date** | 2026-08-12 |

---

## 2. Normative Specification Requirement Inventory

| Spec ID | Requirement / Contract Description | Target Type / Symbol | Specified Value / Formula |
| :--- | :--- | :--- | :--- |
| **REQ-004-01** | Plane 0 Definition | Spatial Grid Canvas | `z-index: 10`, camera-projected world coordinates $(T(x,y,s))$ |
| **REQ-004-02** | Plane 1 Definition | Information Plane | `z-index: 300`, position-linked to spatial world, unscaled by camera zoom |
| **REQ-004-03** | Plane 2 Definition | HUD Plane | `z-index: 400`, fixed screen-space viewport coordinates |
| **REQ-004-04** | Zero Level-Lifting Rule | Architecture Law | Plane 0 NEVER raises compositor z-index to compete with Plane 1/2 |
| **REQ-004-05** | No Canvas Dimming Rule | Architecture Law | Opening overlays on Plane 1/2 NEVER dims or blurs Plane 0 rendering |
| **REQ-004-06** | Visual Plane Enum | `VisualPlaneType` | Enum: `Plane0_SpatialGrid = 0`, `Plane1_InformationPlane = 1`, `Plane2_HUDPlane = 2` |
| **REQ-004-07** | Plane View Interface | `IPlaneView` | Interface with `PlaneType`, `CompositorZIndex`, `HandlesPointerInput`, `RenderPlane` |
| **REQ-004-08** | Plane Compositor Interface | `IPlaneCompositor` | Interface with `RegisterPlaneView`, `RenderAllPlanes`, `RoutePointerEvent` |
| **REQ-004-09** | Unified Compositor Container | `ThreePlaneVisualCompositorContainer` | Custom Avalonia `Panel` enforcing top-down pointer dispatch |
| **REQ-004-10** | Top-Down Pointer Routing Protocol | Input Dispatch | Top-down evaluation: Plane 2 $\to$ Plane 1 $\to$ Plane 0 |
| **REQ-004-11** | Base Canvas Ground Fill | Color Token | Opaque `#0E0E10` (`--c-base` / `Colors.SurfaceGrid`) |
| **REQ-004-12** | Information Plane Chrome Fill | Color Token | `#161618` (`--surface-chrome`) over local footprint only |

---

## 3. Codebase Reality & Line-by-Line Evidence

### 3.1 Implemented Artifacts

- **Visual Plane stacking in XAML**: Implemented in [`src/GroveApp/MainWindow.axaml`](file:///C:/dev/grove-v9/src/GroveApp/MainWindow.axaml#L13-L78).
  - Lines 15: Plane 0 `GridCanvasControl` positioned as lowest Grid child.
  - Lines 19-22: Plane 1 `LocalEditorOverlay` and `QuickNoteOverlay` stacked in middle.
  - Lines 25-77: Plane 2 HUD telemetry border positioned at bottom (`VerticalAlignment="Bottom"`).
- **Non-Dimming Canvas**: Verified in [`src/GroveApp/MainWindow.axaml.cs`](file:///C:/dev/grove-v9/src/GroveApp/MainWindow.axaml.cs#L128-L150). Opening `LocalEditorOverlay` or `QuickNoteOverlay` leaves `GridCanvasControl` fully visible and rendering without dimming/blur backdrops.

### 3.2 Missing / Non-Conforming Features (0% Implemented)

- **`VisualPlaneType` Enum**: **0% Implemented**. Missing enum `Grove.SpatialGrid.VisualHierarchy.VisualPlaneType`.
- **`IPlaneView` & `IPlaneCompositor` Interfaces**: **0% Implemented**. Missing interfaces `IPlaneView` and `IPlaneCompositor`. Plane views do not implement a unified compositor contract.
- **`ThreePlaneVisualCompositorContainer` Panel**: **0% Implemented**. Missing class `ThreePlaneVisualCompositorContainer`. Visual stacking relies on standard Avalonia `Grid` child order without explicit z-index compositor bands.
- **Unified Top-Down Pointer Router**: **0% Implemented**. Pointer routing is handled via ad-hoc tunneling event handlers in [`src/GroveApp/MainWindow.axaml.cs`](file:///C:/dev/grove-v9/src/GroveApp/MainWindow.axaml.cs#L85-L93) and `GlobalFocusPrecedenceRouter.cs` rather than top-down hit testing (`Plane 2 -> Plane 1 -> Plane 0`) inside a compositor container.

---

## 4. Standards & Visual Plane Seam Audit

- **Visual Plane Separation**: The 3-plane visual separation is maintained in UI layout structure. Plane 0 canvas does not raise level.
- **Design Token Compliance**: Token colors (`#0E0E10` canvas ground, `#161618` overlay chrome) match specification tokens.
- **Architectural Seams**: Input routing relies on direct event handlers in `MainWindow.axaml.cs` instead of encapsulating routing logic inside a reusable `ThreePlaneVisualCompositorContainer`.

---

## 5. Root Cause Analysis

### Why claimed status (`IMPLEMENTED - AWAITING USER REVIEW`) diverges from actual interactive reality:

1. **XAML Grid Stacking Substituted for Custom Compositor**: In Avalonia UI, placing controls sequentially inside a standard `<Grid>` provides implicit visual z-ordering. The implementation team leveraged this native XAML behavior and marked ADR-004 complete without writing the formal C# compositor container (`ThreePlaneVisualCompositorContainer`).
2. **Ad-Hoc Event Handler Routing**: Instead of implementing top-down hit testing via `IPlaneCompositor.RoutePointerEvent()`, event routing was wired directly using Avalonia's `Tunnel` routing strategy in `MainWindow.axaml.cs`.
