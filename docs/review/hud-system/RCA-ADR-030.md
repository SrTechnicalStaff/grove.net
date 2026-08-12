# RCA Ledger: ADR-030 — Three-Plane Compositor Architecture Validation

## 1. Executive Metadata

| Metadata Field | Value |
| :--- | :--- |
| **ADR ID** | ADR-030 |
| **ADR Title** | Three-Plane Compositor Architecture Validation |
| **Category** | HUD System (`docs/specs/hud-system/`) |
| **Claimed Status in Spec Header** | Accepted |
| **Verified Status (User-Observable)** | **PARTIALLY IMPLEMENTED (30% Implemented, Structural Seam Defect in Live UI)** |
| **Audit Date** | 2026-08-12 |
| **Target Runtime** | C# 13 / .NET 9 / Avalonia 11.2.5 / SkiaSharp |
| **Auditor** | Principal AI Systems Architect & Product Auditor |

---

## 2. Normative Specification Requirement Inventory

| ID | Requirement Category | Symbol / Contract Name | Normative Specification Requirement |
| :--- | :--- | :--- | :--- |
| **REQ-030-01** | Root Panel | `ThreePlaneCompositorHost` | Root Avalonia Panel orchestrating the native 3-plane GPU compositor hierarchy with strict `ZIndex` layers. |
| **REQ-030-02** | Visual Plane 0 | `Plane0_SpatialGridCanvas` | Camera-projected infinite grid control (`ZIndex = 100`) using affine-transformed world coordinates $P_{\text{screen}} = T_{\text{camera}}(P_{\text{world}})$. |
| **REQ-030-03** | Visual Plane 1 | `Plane1_InformationLayer` | World-anchored editorial canvas (`ZIndex = 200`) using position-anchored screen coordinates $P_{\text{screen}} = T_{\text{translation}}(P_{\text{world\_origin}})$. |
| **REQ-030-04** | Visual Plane 2 | `Plane2_HudSlatePlane` | Viewport-fixed HUD Panel (`ZIndex = 300`) hosting Slates (Memory, Gallery, Writing, Layer Manager) using absolute viewport coordinates. |
| **REQ-030-05** | Skia Drawing Context | `SpatialCanvasCustomDrawOperation` | Direct Skia Sharp rendering via Avalonia `ICustomDrawOperation` and `ISkiaSharpApiLeaseFeature`. |
| **REQ-030-06** | Pass-Through Input | Scrim-Free Input Routing | Non-obscuringScrim-free pass-through policy. Unbound pointer events hit-test transparently down the 3-plane stack. |
| **REQ-030-07** | Input Pipeline | Top-Down Event Dispatch | Pointer events enter via Avalonia window loop and pass Plane 2 $\to$ Plane 1 $\to$ Plane 0. |
| **REQ-030-08** | Coordinate Decoupling | Affine Transform Isolation | Text scale and control bounds on Plane 1 and Plane 2 remain invariant under camera zoom $s$. |
| **REQ-030-09** | Zero Level-Lifting | Z-Index Preservation | Plane 0 NEVER mutates z-index or rendering layer to obscure or compete with Plane 1 or Plane 2. |
| **REQ-030-10** | Compositor Strategy | Option C Avalonia Stack | Host all 3 planes inside a single `TopLevel` window using Avalonia's native GPU compositor. |

---

## 3. Codebase Reality & Line-by-Line Evidence

### 3.1 Symbol Existence & Implementation Audit

| Symbol / Class Name | Expected Location | Actual Status in `src/GroveApp/` | Line-by-Line Evidence |
| :--- | :--- | :--- | :--- |
| `ThreePlaneCompositorHost` | `src/GroveApp/Controls/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. [`MainWindow.axaml:L13-L78`](file:///C:/dev/grove-v9/src/GroveApp/MainWindow.axaml#L13) uses a standard Avalonia `<Grid>` container. |
| `Plane0SpatialCanvasControl` | `src/GroveApp/Controls/` | **PARTIALLY IMPLEMENTED** | Implemented directly inside [`GridCanvasControl.cs`](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs#L1). |
| `InformationLayerCanvas` | `src/GroveApp/Controls/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. Overlays are placed directly in `MainWindow.axaml` grid. |
| `HudOverlayPanel` | `src/GroveApp/Controls/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. No dedicated Plane 2 HUD overlay panel (`ZIndex = 300`) exists. |
| `SpatialCanvasCustomDrawOperation` | `src/GroveApp/Controls/` | **PARTIALLY IMPLEMENTED** | `GridCanvasControl.Render` ([`GridCanvasControl.cs:L154-L180`](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs#L154)) draws to Skia canvas via custom draw operation, but lacks explicit plane ordering host. |

### 3.2 Main Window Structural Breakdown in `MainWindow.axaml`

```xml
<!-- MainWindow.axaml (Lines 13-23) -->
<!-- Actual codebase reality: Plain Grid without ThreePlaneCompositorHost or explicit ZIndex ordering -->
<Grid>
    <!-- Plane 0: Spatial Grid Canvas -->
    <controls:GridCanvasControl x:Name="CanvasControl" HorizontalAlignment="Stretch" VerticalAlignment="Stretch" />

    <!-- Plane 1: Local Editor & Quick Note Overlays -->
    <controls:LocalEditorOverlay x:Name="LocalEditor" IsVisible="False" VerticalAlignment="Top" Background="Transparent" />
    <controls:QuickNoteOverlay x:Name="QuickNote" IsVisible="False" HorizontalAlignment="Center" VerticalAlignment="Center" Background="Transparent" />

    <!-- NO Plane 2 HUD Overlay Panel (ZIndex = 300) exists -->
</Grid>
```

---

## 4. Standards & Visual Plane Seam Audit

### 4.1 Visual Plane Separation (Plane 0 vs Layer 1 vs Plane 2)
- **Plane 0**: `GridCanvasControl` renders Skia graphics, but because it is hosted inside a plain `<Grid>`, visual plane layering relies on ambient XAML child declaration order rather than structured `ThreePlaneCompositorHost` compositor nodes with explicit `ZIndex` properties (`100`, `200`, `300`).
- **Layer 1**: `LocalEditorOverlay` and `QuickNoteOverlay` are rendered, but `InformationLayerCanvas` container is missing.
- **Plane 2**: `HudOverlayPanel` (`ZIndex = 300`) is 100% missing. HUD Slates cannot be mounted.

### 4.2 Code Smells & Architectural Violations
1. **Ad-Hoc Layout Stack**: Using a generic Avalonia `Grid` instead of a dedicated `ThreePlaneCompositorHost` breaks visual encapsulation and prevents top-down pass-through input routing validation.

---

## 5. Root Cause Analysis (RCA)

### 5.1 Primary Root Cause
The architectural specification in ADR-030 defined `ThreePlaneCompositorHost` as the target host panel. However, during initial application scaffolding, `MainWindow.axaml` was created with a basic `<Grid>` to quickly display `GridCanvasControl`. The creation of `ThreePlaneCompositorHost.cs` was skipped.

### 5.2 Failure Chain
1. **Scaffolding Shortcut**: `<Grid>` was used as a placeholder in `MainWindow.axaml` and never replaced.
2. **Missing Subsystem Components**: `ThreePlaneCompositorHost.cs`, `InformationLayerCanvas`, and `HudOverlayPanel` were omitted from `src/GroveApp/Controls/`.
