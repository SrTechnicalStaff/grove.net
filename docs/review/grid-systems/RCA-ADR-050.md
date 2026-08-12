# Root Cause Analysis (RCA) Ledger: ADR-050

## 1. Executive Metadata

| Property | Value |
| :--- | :--- |
| **ADR ID** | `ADR-050` |
| **Title** | Footprint-Aware Grid Cursor and Spent-Cell Trail Decay System |
| **Category** | Grid Systems (`docs/specs/grid-systems/`) |
| **Claimed Spec Status** | `IMPLEMENTED - AWAITING USER REVIEW` |
| **Verified Interactive Status** | `PARTIALLY IMPLEMENTED (NON-CONFORMING SUB-SET)` |
| **Audit Date** | 2026-08-12 |
| **Target Runtime** | C# 13 / .NET 9 / Avalonia 11.2.5 / SkiaSharp 3.x |
| **Authors** | Chief Product Definition Architect & AI Systems Audit Team |

---

## 2. Normative Specification Requirement Inventory

The table below catalogs every explicit architectural requirement, mathematical formulation, interface contract, and visual token specified in [ADR-050](file:///C:/dev/grove-v9/docs/specs/grid-systems/ADR-050-Footprint-Aware-Grid-Cursor-And-Trails.md):

| ID | Requirement Category | Normative Spec Requirement | Target Symbol / Token |
| :--- | :--- | :--- | :--- |
| **REQ-050-1** | Service Contract | `IGridCursorService` interface managing state, cell moves, pointer projection, role arming, disarming, and frame ticking. | `Grove.SpatialGrid.Cursor.IGridCursorService` |
| **REQ-050-2** | Data Contracts | `CursorRole` enum (`Default`=0, `ToolPlacement`=1 `#3B82F6`, `EditTransform`=2 `#F59E0B`, `LayerTrace`=3 `#10B981`). | `Grove.SpatialGrid.Cursor.CursorRole` |
| **REQ-050-3** | Data Contracts | `CellCoordinate` pack=4 readonly record struct and `FootprintBounds` pack=4 readonly record struct with `Contains` helper. | `Grove.SpatialGrid.Cursor.CellCoordinate`, `FootprintBounds` |
| **REQ-050-4** | Data Contracts | `SpentCellSegment` readonly record struct (`Cell`, `KineticEnergy`, `FrameAge`) and `GridCursorState` immutable record. | `SpentCellSegment`, `GridCursorState` |
| **REQ-050-5** | Compositor Execution | Custom Skia draw operation `GridCursorDrawOperation` implementing Avalonia `ICustomDrawOperation` for zero-allocation render loop execution. | `Grove.SpatialGrid.Rendering.GridCursorDrawOperation` |
| **REQ-050-6** | Footprint Expansion | Cursor head dynamically expands to match target item footprint bounds ($N \times M$ cells for `Note`, `Document`, `Picture`). | Footprint Bounds $[X_{\text{origin}}, Y_{\text{origin}}, W_{\text{cells}}, H_{\text{cells}}]$ |
| **REQ-050-7** | Fill & Ring Geometry | Inset 2px ring (`--cursor-ring` = 2.0px, opacity 0.88, `#F4F4F2`) and 22% fill gain (`--cursor-fill-gain` = 0.22, 13.2% steady fill). | `--cursor-ring`, `--cursor-ring-ink`, `--cursor-fill-gain` |
| **REQ-050-8** | Trail Decay Physics | 18-step spent-cell exponential decay physics ($\gamma = 0.84$, initial $E_0 = 0.60$, pruned below threshold $E_{\text{min}} = 0.03$). | `--cursor-trail-decay`, `--cursor-trail-min`, `--cursor-trail-max-steps` |
| **REQ-050-9** | Action Recoloring | Neutral rest fill (`#F4F4F2`). Cursor recolors ONLY when actively placing (`#3B82F6`), moving/resizing (`#F59E0B`), or tracing (`#10B981`). | Neutral Rest & Action Recoloring Protocol |
| **REQ-050-10** | Addressing & Motion | Scale-independent cell addressing scaling across 1% to 1000% zoom levels (even when `linesVisible = false`) + Reduced-Motion compliance. | Scale-Independent Cell Addressing & Reduced-Motion |

---

## 3. Codebase Reality & Line-by-Line Evidence

### 3.1 Implemented & Partially Implemented Symbols

- **`Tokens.cs` Constants**:
  - `Tokens.StrokeCursorRing` = 2.0 ([Tokens.cs:L34](file:///C:/dev/grove-v9/src/GroveApp/DesignSystem/Tokens.cs#L34))
  - `Tokens.CursorRing` = 2.0, `Tokens.CursorRingInk` = 0.88, `Tokens.CursorFillGain` = 0.22, `Tokens.CursorSteady` = 0.6, `Tokens.CursorTrailDecay` = 0.84, `Tokens.CursorTrailMin` = 0.03, `Tokens.CursorTrailMaxSteps` = 18 ([Tokens.cs:L65-L71](file:///C:/dev/grove-v9/src/GroveApp/DesignSystem/Tokens.cs#L65-L71))
- **`CursorRenderModule.cs`**:
  - `DecayTrail(List<SpentCell> spentCells)` ([CursorRenderModule.cs:L21-L34](file:///C:/dev/grove-v9/src/GroveApp/Engine/CursorRenderModule.cs#L21-L34)): Implements exponential decay (`spentCells[i].Energy *= Tokens.CursorTrailDecay`) and pruning (`spentCells[i].Energy <= Tokens.CursorTrailMin`).
  - `RegisterCellTransition(...)` ([CursorRenderModule.cs:L40-L50](file:///C:/dev/grove-v9/src/GroveApp/Engine/CursorRenderModule.cs#L40-L50)): Registers spent cells and caps list length at `Tokens.CursorTrailMaxSteps` (18 steps).
  - `RenderGridCursor(...)` ([CursorRenderModule.cs:L79-L111](file:///C:/dev/grove-v9/src/GroveApp/Engine/CursorRenderModule.cs#L79-L111)): Draws cursor rectangle with 22% fill (`Tokens.CursorFillGain`) and inset 2px ring (`Tokens.CursorRingInk` = 0.88, `Tokens.StrokeCursorRing` = 2.0).

### 3.2 0% Implemented & Deviated Symbols

- **`IGridCursorService` Interface**: **0% Implemented**. Missing namespace `Grove.SpatialGrid.Cursor` and interface `IGridCursorService`. No service manages state transitions, pointer positions, or role arming.
- **`GridCursorDrawOperation`**: **0% Implemented**. Missing class `Grove.SpatialGrid.Rendering.GridCursorDrawOperation` implementing Avalonia's `ICustomDrawOperation`. Instead, `RenderSpentTrail` and `RenderGridCursor` draw directly using Avalonia's high-level `DrawingContext` ([CursorRenderModule.cs:L56, L79](file:///C:/dev/grove-v9/src/GroveApp/Engine/CursorRenderModule.cs#L56)), causing heap allocations during render ticks.
- **`CursorRole` Enum & Action Recoloring**: **0% Implemented**. Missing `CursorRole` enum (`Default`, `ToolPlacement`, `EditTransform`, `LayerTrace`). In `CursorRenderModule.cs` [L104, L109](file:///C:/dev/grove-v9/src/GroveApp/Engine/CursorRenderModule.cs#L104), the color is hardcoded to `#F4F4F2` (`Colors.NoteText`), completely ignoring action recoloring (`#3B82F6`, `#F59E0B`, `#10B981`).
- **`SpentCellSegment` Struct**: **0% Implemented**. Instead of the spec-mandated `SpentCellSegment` readonly record struct, `SpentCell` is implemented as a mutable class `public class SpentCell` in [`Models/GridCursorTrail.cs`](file:///C:/dev/grove-v9/src/GroveApp/Models/GridCursorTrail.cs).
- **Multi-Item Footprint Expansion**: **0% Implemented** for Documents and Images. In `CursorRenderModule.cs` [L88-L90](file:///C:/dev/grove-v9/src/GroveApp/Engine/CursorRenderModule.cs#L88-L90), footprint expansion checks `GridNote? targetNote`, completely failing to expand over `GridDocument` or `GridImage` placements.
- **Reduced-Motion Compliance**: **0% Implemented**. No check exists for `SystemAnimations.IsEnabled`.

---

## 4. Standards & Visual Plane Seam Audit

1. **Plane 0 Composition Seam**:
   - The grid cursor is rendered on Plane 0. However, because `GridCursorDrawOperation` was not implemented as a Skia `ICustomDrawOperation`, cursor rendering is intermingled with Avalonia's immediate `DrawingContext` in `GridCanvasControl.cs` [L920-L925](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs#L920-L925), violating the zero-GC compositor execution rule.
2. **Design System Tokens Alignment**:
   - `Tokens.cs` contains the correct numeric constants (`CursorRing` = 2.0, `CursorTrailDecay` = 0.84, `CursorFillGain` = 0.22).
   - However, color tokens `--k-tool` (`#3B82F6`), `--k-edit` (`#F59E0B`), and `--k-layer` (`#10B981`) are not bound to the cursor rendering module.
3. **Code Smells & Architectural Violations**:
   - Mutable reference type `SpentCell` is allocated on every cell transition, generating GC pressure during active canvas panning.
   - `GridCanvasControl` directly holds cursor state variables (`CursorCellX`, `CursorCellY`, `_lastCursorCellX`, `_lastCursorCellY`) in control code instead of consuming `IGridCursorService`.

---

## 5. Root Cause Analysis (RCA)

### Why claimed status (`IMPLEMENTED - AWAITING USER REVIEW`) diverges from actual interactive reality:

1. **Shallow Helper Isolation**:
   - The author implemented a lightweight helper class `CursorRenderModule` with basic decay loop math and hardcoded drawing routines to satisfy superficial visual rendering of a single cell ring.
2. **Bypassed Architecture for Immediate UI Convenience**:
   - The architecture specified a decoupled service `IGridCursorService` and a native Skia compositor operation `GridCursorDrawOperation`. To avoid implementing Avalonia `ICustomDrawOperation` infrastructure, rendering was wired directly into `GridCanvasControl.OnRender`.
3. **Incomplete Domain Model Integration**:
   - Footprint expansion was hardcoded against `GridNote`, neglecting the unified base type `GridContentItem` or interface abstractions required to expand over `GridDocument` and `GridImage`.
4. **Omission of Dynamic Role Recoloring**:
   - The state machine for tool arming was built separately without registering role changes (`CursorRole`) to the cursor renderer, leaving the cursor permanently locked in neutral `#F4F4F2`.
