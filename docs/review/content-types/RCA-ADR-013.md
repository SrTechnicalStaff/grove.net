# Root Cause Analysis (RCA) Ledger: ADR-013

## 1. Executive Metadata

| Property | Value |
| :--- | :--- |
| **ADR ID** | ADR-013 |
| **ADR Title** | External Drag-and-Drop System, ScreenToCell Coordinate Resolution, and Content Auto-Creation |
| **Category** | System Interoperability / Spatial Drag-and-Drop Engine |
| **Claimed Status** | IMPLEMENTED - AWAITING USER REVIEW |
| **Verified Status** | IMPLEMENTED & INTERACTIVE IN LIVE UI (OS shell file drops, camera-aware ScreenToCell resolution, collision refusal, and extension mapping active) |
| **Audited Runtime Target** | C# 13 / .NET 9.0 / Avalonia UI 11.2.5 |
| **Audit Date** | 2026-08-12 |

---

## 2. Normative Specification Requirement Inventory

| Spec ID | Requirement / Contract Description | Target Type / Symbol | Specified Value / Formula |
| :--- | :--- | :--- | :--- |
| **REQ-013-01** | Zero-Modal Drop Rule | Interaction Law | Dragging valid OS files onto Grid immediately instantiates content without modal dialogs |
| **REQ-013-02** | Cell Quantization & Refusal | Placement Law | Footprint must land on integer cell bounds $(C_x, C_y)$. Occupied cells trigger refusal |
| **REQ-013-03** | World Coordinate Conversion | Spatial Math | $X_{\text{world}} = \frac{X_{\text{screen}} - O_x}{S}$, $Y_{\text{world}} = \frac{Y_{\text{screen}} - O_y}{S}$ |
| **REQ-013-04** | Cell Index Conversion | Spatial Math | $C_x = \left\lfloor \frac{X_{\text{screen}} - O_x}{220 S} \right\rfloor$, $C_y = \left\lfloor \frac{Y_{\text{screen}} - O_y}{220 S} \right\rfloor$ |
| **REQ-013-05** | Footprint Collision Check | Collision Math | $\text{IsFree} = \forall (c_x, c_y) \in \text{TargetRegion}, \text{Grid}[c_x, c_y] == \varnothing$ |
| **REQ-013-06** | Image Extension Family | File Mapping | `.png`, `.jpg`, `.jpeg`, `.webp`, `.gif` $\to$ **Picture** (`ImageFootprintResolver`) |
| **REQ-013-07** | Short Text Family | File Mapping | `.txt` ($< 500$ chars) $\to$ **Note** (`NoteGeometrySolver`) |
| **REQ-013-08** | Rich Prose Family | File Mapping | `.md`, `.txt` ($\ge 500$ chars) $\to$ **Document** (Min $2 \times 2$) |
| **REQ-013-09** | Data AST Family | File Mapping | `.json` $\to$ **Document** (JSON AST inside $2 \times 2$ Document) |
| **REQ-013-10** | Portable Doc Family | File Mapping | `.pdf` $\to$ **Document** (PDF Page AST inside $2 \times 3$ Document) |
| **REQ-013-11** | Spatial Coordinate Resolver | `SpatialCoordinateResolver` | Static class with `ScreenToCell(screenPoint, panOffset, zoomScale)` |
| **REQ-013-12** | Drag-and-Drop Handler Class | `ExternalDragDropHandler` | Class with `OnDragOver()` and `OnDropAsync()` integrating with OS shell |

---

## 3. Codebase Reality & Line-by-Line Evidence

### 3.1 Implemented Artifacts

- **Spatial Coordinate Resolver**: Implemented in [`src/GroveApp/Engine/ExternalDragDropHandler.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/ExternalDragDropHandler.cs#L16-L32).
  - Lines 20-31: `ScreenToCell` transforms raw cursor screen positions into quantized cell coordinates using camera pan offset and zoom scale.
- **OS Shell Drag-and-Drop Handler**: Implemented in [`src/GroveApp/Engine/ExternalDragDropHandler.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/ExternalDragDropHandler.cs#L38-L231).
  - Lines 51-56: `Attach()` registers Avalonia `DragDrop.DragOverEvent` and `DragDrop.DropEvent`.
  - Lines 58-116: `OnDragOver()` validates incoming OS file formats and checks collision via `_isRegionFreeChecker`. Sets `DragEffects.None` on collision.
  - Lines 118-149: `OnDropAsync()` processes file lists, calculates footprints, and commits placements onto the active spatial layer.
  - Lines 151-229: Deterministic extension dispatch creating Pictures (.png/.jpg/.gif), Notes (.txt < 500 chars), and Documents (.md/.json/.pdf).
- **Canvas Control Integration**: Attached in [`src/GroveApp/Controls/GridCanvasControl.cs`](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs#L223).

### 3.2 Missing / Non-Conforming Features (0% Implemented)

- **Dedicated Refusal Hatch Overlay**: During drag over an occupied cell region, `DragEffects.None` displays the OS refusal cursor, but a custom red canvas refusal hatch pattern (`--signal-refusal`) is not drawn on Plane 0.

---

## 4. Standards & Visual Plane Seam Audit

- **Visual Plane Separation**: Dropped placements land directly on Plane 0.
- **Design Token Compliance**: Coordinates snap cleanly to the canonical $220\text{px}$ cell grid (`Tokens.GridCell`).
- **Architectural Seams**: `ExternalDragDropHandler.cs` is cleanly decoupled from direct UI control state using delegate callbacks (`isRegionFreeChecker`, `onItemPlacedAsync`).

---

## 5. Root Cause Analysis

### Why claimed status (`IMPLEMENTED - AWAITING USER REVIEW`) matches actual interactive reality:

1. **Full OS Pipeline Integration**: Native OS file drag-and-drop was prioritized for desktop interop. Avalonia's `DragDrop` attached handlers were fully wired to the camera mathematics and spatial grid storage.
2. **Deterministic Dispatch**: File extension inspection was implemented thoroughly, supporting image footprint decoding via `ImageFootprintResolver` and text/document creation.
