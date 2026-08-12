# Root Cause Analysis (RCA) Ledger: ADR-057

## 1. Executive Metadata

| Property | Value |
| :--- | :--- |
| **ADR ID** | `ADR-057` |
| **Title** | Keybind Arming State Machine and Ghost Placement Preview |
| **Category** | Keybind Arming (`docs/specs/keybind-arming/`) |
| **Claimed Spec Status** | `IMPLEMENTED - AWAITING USER REVIEW` |
| **Verified Interactive Status** | `PARTIALLY IMPLEMENTED (NON-CONFORMING SUB-SET)` |
| **Audit Date** | 2026-08-12 |
| **Target Runtime** | C# 13 / .NET 9 / Avalonia 11.2.5 / SkiaSharp 3.x |
| **Authors** | Chief Product Definition Architect & AI Systems Audit Team |

---

## 2. Normative Specification Requirement Inventory

The table below catalogs every explicit architectural requirement, mathematical formulation, interface contract, and visual token specified in [ADR-057](file:///C:/dev/grove-v9/docs/specs/keybind-arming/ADR-057-Keybind-Arming-And-Ghost-Placement.md):

| ID | Requirement Category | Normative Spec Requirement | Target Symbol / Token |
| :--- | :--- | :--- | :--- |
| **REQ-057-1** | Service Contract | `IToolArmingService` interface managing `CurrentState`, `ActiveGhostDescriptor`, `ArmTool`, `Disarm`, `UpdateCursorPosition`, `CommitPlacement`, and events. | `Grove.SpatialGrid.Arming.IToolArmingService` |
| **REQ-057-2** | Data Contracts | `ToolArmingState` enum (`Idle`=0, `ArmedNote`=1, `ArmedQuickNote`=2, `ArmedDocument`=3, `Placed`=4), `ArmableContentType` enum (`Note`, `QuickNote`, `Document`). | `ToolArmingState`, `ArmableContentType` |
| **REQ-057-3** | Data Contracts | `GhostPlacementDescriptor` struct/record and `PlacementCommitResult` record containing success status, placed memory ID, footprint, and auto-focus flag. | `GhostPlacementDescriptor`, `PlacementCommitResult` |
| **REQ-057-4** | Compositor Execution | Custom Skia draw operation `GhostPlacementDrawOperation` implementing Avalonia `ICustomDrawOperation` for zero-allocation ghost preview rendering. | `Grove.SpatialGrid.Arming.Rendering.GhostPlacementDrawOperation` |
| **REQ-057-5** | State Machine Matrix | Deterministic state transition matrix (`IDLE` $\rightarrow$ `ARMED_NOTE` / `ARMED_QUICKNOTE` / `ARMED_DOCUMENT` $\rightarrow$ `PLACED` $\rightarrow$ `IDLE`). | Tool Arming State Transition Matrix |
| **REQ-057-6** | 50% Alpha Inset Ghost | Ghost preview renders 50% alpha fill (`--ghost-fill-opacity` = 0.50) and 2px inset accent ring (`--ghost-ring-width` = 2.0px, opacity 0.88). Outset strokes forbidden. | `--ghost-fill-opacity`, `--ghost-ring-width` |
| **REQ-057-7** | Collision Validation Tint | Occupied cell switches ghost preview color from tool accent (`#3B82F6`) to collision refusal color (`#EF4444`). | Collision-Aware Validation Tinting |
| **REQ-057-8** | Visual Rejection Pulse | Clicking an invalid cell triggers a 200ms visual error shake/red flash boundary pulse without disarming the tool. | Visual Rejection Pulse Contract |
| **REQ-057-9** | QuickNote Focus Transfer | Immediate inline keyboard focus transfer to `FocusedTextBox` upon `ARMED_QUICKNOTE` placement commit. | Immediate Inline Focus Transfer |

---

## 3. Codebase Reality & Line-by-Line Evidence

### 3.1 Implemented & Partially Implemented Symbols

- **`ToolArmingStateMachine.cs`**:
  - Located at [`Engine/ToolArmingStateMachine.cs:L10-L162`](file:///C:/dev/grove-v9/src/GroveApp/Engine/ToolArmingStateMachine.cs#L10-L162).
  - Implements `ToolArmingState` enum (`Idle`, `ArmedNote`, `ArmedQuickNote`, `ArmedDocument`, `Placed`) [L10-L17](file:///C:/dev/grove-v9/src/GroveApp/Engine/ToolArmingStateMachine.cs#L10-L17).
  - Implements `ArmableContentType` enum [L19-L24](file:///C:/dev/grove-v9/src/GroveApp/Engine/ToolArmingStateMachine.cs#L19-L24).
  - Implements `GhostPlacementDescriptor` struct [L26-L36](file:///C:/dev/grove-v9/src/GroveApp/Engine/ToolArmingStateMachine.cs#L26-L36).
  - Implements methods `ArmTool`, `Disarm`, `UpdateCursorPosition`, `TryCommit`, `CompletePlacement`.
- **Grid Canvas Arming Integration**:
  - `GridCanvasControl.cs` [L34, L163, L676-L700, L804-L845](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs#L34) instantiates `ToolArmingStateMachine` and handles placement commit.
  - `RenderArmingGhost` in `GridCanvasControl.cs` [L962-L994](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs#L962) renders ghost box with blue/red brush.

### 3.2 0% Implemented & Deviated Symbols

- **`IToolArmingService` Interface**: **0% Implemented**. `ToolArmingStateMachine` does NOT implement `IToolArmingService` interface. The interface `IToolArmingService` is missing from the codebase.
- **`PlacementCommitResult` Record**: **0% Implemented**. Missing `PlacementCommitResult` record.
- **`GhostPlacementDrawOperation`**: **0% Implemented**. Missing custom Skia draw operation `Grove.SpatialGrid.Arming.Rendering.GhostPlacementDrawOperation`. Rendering occurs directly on Avalonia `DrawingContext` in `GridCanvasControl.cs` [L962](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs#L962).
- **Visual Rejection Pulse (200ms Red Flash/Shake)**: **0% Implemented**. When clicking an occupied cell while armed in `GridCanvasControl.cs` [L804-L808](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs#L804-L808), `TryCommit` returns `false` and nothing happens visually. The 200ms rejection pulse animation is completely missing.
- **Quick Note Immediate Focus Transfer**: **PARTIALLY IMPLEMENTED (FLAKY)**. `QuickNoteOverlay` is opened upon commit, but focus transfer to `FocusedTextBox` is incomplete and often leaves focus on canvas.

---

## 4. Standards & Visual Plane Seam Audit

1. **Plane 0 Composition Seam**:
   - Ghost previews float over Plane 0 grid cells. However, because `GhostPlacementDrawOperation` was not implemented as a Skia `ICustomDrawOperation`, ghost rendering runs on Avalonia's high-level `DrawingContext`, causing layout/paint allocations.
2. **Design System Tokens Alignment**:
   - Color `#3B82F6` (`--k-tool`) and `#EF4444` (`--k-invalid`) are used in `RenderArmingGhost`. However, exact 50% alpha fill (`0.50`) and 88% alpha ring opacity (`0.88`) are hardcoded rather than consuming design system motion/color tokens.
3. **Code Smells & Architectural Violations**:
   - `ToolArmingStateMachine` depends on a raw delegate `Func<CellCoordinate, int, int, int, bool> _isRegionFree` passed in constructor rather than injecting an `ISpatialIndexProvider` or `ISpatialResizeService`.

---

## 5. Root Cause Analysis (RCA)

### Why claimed status (`IMPLEMENTED - AWAITING USER REVIEW`) diverges from actual interactive reality:

1. **Interface Contract Omission**:
   - The developer built `ToolArmingStateMachine` as a standalone concrete C# class without extracting the spec-mandated `IToolArmingService` interface, breaking dependency injection and unit test mockability.
2. **Skia Compositor Bypass**:
   - Rather than creating `GhostPlacementDrawOperation` implementing `ICustomDrawOperation`, ghost drawing was placed directly into `GridCanvasControl.RenderArmingGhost`, introducing rendering overhead on the main UI thread.
3. **Omission of Rejection Pulse Animation**:
   - The 200ms error shake/red flash pulse required timer-based animation state management in `Motion.cs`. This was skipped, leaving invalid placement clicks completely silent.
