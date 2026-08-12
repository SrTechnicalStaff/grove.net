# Root Cause Analysis (RCA) Ledger: ADR-001

## 1. Executive Metadata

| Property | Value |
| :--- | :--- |
| **ADR ID** | ADR-001 |
| **ADR Title** | Spatial Grid Plane Architecture |
| **Category** | Spatial Grid Engine / Skia Rendering Pipeline |
| **Claimed Status** | IMPLEMENTED - AWAITING USER REVIEW |
| **Verified Status** | PARTIALLY IMPLEMENTED (Grid lines & fading active in live UI; Supercell tier, Skia native pipeline, and pixel hairline snapping missing) |
| **Audited Runtime Target** | C# 13 / .NET 9.0 / Avalonia UI 11.2.5 |
| **Audit Date** | 2026-08-12 |

---

## 2. Normative Specification Requirement Inventory

| Spec ID | Requirement / Contract Description | Target Type / Symbol | Specified Value / Formula |
| :--- | :--- | :--- | :--- |
| **REQ-001-01** | Plane 0 Spatial Grid Base Canvas | Canvas Background | `#0E0E10` (`--c-base`) |
| **REQ-001-02** | Major Grid Cell Pitch | Spatial Pitch | $P_{\text{major}} = 220.0\text{ DIPs}$ |
| **REQ-001-03** | Minor Grid Subdivision Pitch | Spatial Pitch | $P_{\text{minor}} = 44.0\text{ DIPs}$ ($P_{\text{major}} / 5$) |
| **REQ-001-04** | Supercell Pitch | Spatial Pitch | $P_{\text{super}} = 1100.0\text{ DIPs}$ ($5 \times P_{\text{major}}$) |
| **REQ-001-05** | Universal Ink Opacity Fade Formula | Equation | $\text{ink}(S) = \text{clamp}\left(\frac{S - 6}{8}, 0.0, 1.0\right)$ |
| **REQ-001-06** | Fade Start Threshold | Token | $S_{\text{start}} = 6.0\text{ px}$ (`--grid-fade-start`) |
| **REQ-001-07** | Fade End Threshold | Token | $S_{\text{end}} = 14.0\text{ px}$ (`--grid-fade-end`) |
| **REQ-001-08** | Minor Line Color Token | Color Token | `#161618` (`--c-grid-min` / `--grid-minor-ink`) |
| **REQ-001-09** | Major Line Color Token | Color Token | `#242428` (`--c-grid-maj` / `--grid-major-ink`) |
| **REQ-001-10** | Line Weight Invariant | Layout Constraint | Fixed $1.0\text{ DIP}$ across zoom $s \in [0.01, 10.0]$ |
| **REQ-001-11** | Skia Native Execution Pipeline | Rendering Primitive | SkiaSharp `CustomDrawOperation` / `SKCanvas` |
| **REQ-001-12** | Pixel Snapping Formula | Snapping Equation | $x_{\text{snap}} = \frac{\lfloor x_{\text{screen}} \cdot \text{DPI} \rceil}{\text{DPI}}$ |
| **REQ-001-13** | Hairline Paint Config | `SKPaint` Parameters | `IsAntialias = false`, `StrokeWidth = 1.0f` |
| **REQ-001-14** | Struct Contract | `SpatialGridGeometryConfig` | Immutable record struct containing pitches and ink equation |
| **REQ-001-15** | Renderer Contract | `ISpatialGridRenderer` | Interface for Plane 0 grid rendering |
| **REQ-001-16** | Skia Renderer Implementation | `SkiaSpatialGridRenderer` | Sealed class implementing batched path rendering |
| **REQ-001-17** | Line Visibility Persisted Keybind | Key Interaction | `Key.G` toggles lines without altering snap/cursor |

---

## 3. Codebase Reality & Line-by-Line Evidence

### 3.1 Implemented Artifacts

- **Major/Minor Grid Line Fading & Rendering**: Implemented in [`src/GroveApp/Engine/GridLineModule.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/GridLineModule.cs#L1-L140).
  - Lines 40-42: Minor line pitch $44.0\text{px}$ (`Tokens.MinorCellSize`) and Major line pitch $220.0\text{px}$ (`Tokens.GridCell`).
  - Lines 70-76: Opacity fade curve implementation: `CalculateTierAlpha(spacing, baseAlpha)` using $S_{\text{start}} = 6.0$ and $S_{\text{end}} = 14.0$.
  - Lines 105-135: Viewport line loop drawing vertical and horizontal major/minor grid lines onto Avalonia `DrawingContext`.
- **Canvas Integration**: Implemented in [`src/GroveApp/Controls/GridCanvasControl.cs`](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs#L913-L914), invoking `_gridLineModule.RenderGridLines(...)` during Plane 0 render pass.
- **Design Tokens**: Defined in [`src/GroveApp/DesignSystem/Tokens.cs`](file:///C:/dev/grove-v9/src/GroveApp/DesignSystem/Tokens.cs#L12-L28) (`GridCell = 220.0`, `MinorCellSize = 44.0`, `GridFadeStart = 6.0`, `GridFadeEnd = 14.0`) and [`src/GroveApp/DesignSystem/Colors.cs`](file:///C:/dev/grove-v9/src/GroveApp/DesignSystem/Colors.cs#L22-L25) (`SurfaceGrid = #0E0E10`, `GridMin = #161618`, `GridMaj = #242428`).

### 3.2 Missing / Non-Conforming Features (0% Implemented)

- **Supercell $1100.0\text{px}$ Tier**: **0% Implemented**. Missing symbol `SupercellPitchWorld` / `SupercellCellSize`. [`GridLineModule.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/GridLineModule.cs) only calculates and draws two tiers (Minor $44\text{px}$ and Major $220\text{px}$). At camera scale $s \le 0.0273$ ($2.73\%$), grid lines completely vanish because no supercell lines are rendered.
- **`SpatialGridGeometryConfig` Struct**: **0% Implemented**. Missing struct `Grove.SpatialGrid.Architecture.SpatialGridGeometryConfig`. Constants are split ad-hoc between `Tokens.cs` and `GridLineModule.cs`.
- **`ISpatialGridRenderer` & `SkiaSpatialGridRenderer`**: **0% Implemented**. Missing interface `ISpatialGridRenderer` and Skia class `SkiaSpatialGridRenderer`. Rendering is executed via Avalonia `DrawingContext` vector drawing rather than GPU SkiaSharp `SKCanvas.DrawPath`.
- **Subpixel Hairline Pixel Snapping & Non-Antialiased Lines**: **0% Implemented**. Missing formula $x_{\text{snap}} = \frac{\lfloor x \cdot \text{DPI} \rceil}{\text{DPI}}$ and `IsAntialias = false` stroke configuration. Lines rely on standard Avalonia antialiasing, causing minor subpixel blur at non-integer camera offsets.

---

## 4. Standards & Visual Plane Seam Audit

- **Visual Plane Compliance**: Complies with Plane 0 bottom-layer placement. Rendered first inside `GridCanvasControl.Render()` before content items and overlays.
- **Token Integrity**: Fully compliant with normative color tokens (`Colors.SurfaceGrid`, `Colors.GridMin`, `Colors.GridMaj`) and size tokens (`Tokens.GridCell`, `Tokens.MinorCellSize`). No hardcoded magic hex strings found in grid rendering.
- **Architectural Seams**: Decoupled into `GridLineModule.cs`, avoiding direct state mutation inside `GridCanvasControl.cs`.

---

## 5. Root Cause Analysis

### Why claimed status (`IMPLEMENTED - AWAITING USER REVIEW`) diverges from actual interactive reality:

1. **Premature Approval of Partial Module**: The ADR claimed full implementation after `GridLineModule.cs` succeeded in drawing 2-tier grid lines ($44\text{px}$ and $220\text{px}$). The 3rd tier (Supercell $1100\text{px}$) was omitted during initial engine construction without updating the spec status.
2. **Avalonia DrawingContext Abstraction vs Native Skia Pipeline**: The specification required low-level GPU SkiaSharp `SKCanvas` rendering (`SkiaSpatialGridRenderer`). To accelerate UI integration with Avalonia 11.2.5, developers implemented line drawing using Avalonia's high-level `DrawingContext.DrawLine()` API instead of a custom Skia `CustomDrawOperation`. This bypassed `IsAntialias = false` and exact DPI pixel snapping.
