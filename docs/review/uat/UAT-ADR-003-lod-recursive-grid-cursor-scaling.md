# UAT-ADR-003: Spatial Grid System, Recursive Cursor LOD Scaling & Trail Physics

| Metadata Field    | Value                                                                     |
| ----------------- | ------------------------------------------------------------------------- |
| **Status**        | Proposed / In Review                                                      |
| **Date**          | 2026-08-12                                                                |
| **Authors**       | Senior Technical Staff / Grove Architecture Group                         |
| **Classification**| Architectural Decision Record (ADR) / User Acceptance Testing (UAT) Spec  |
| **Target Seam**   | [CursorRenderModule.cs](file:///C:/dev/grove-v9/src/GroveApp/Engine/CursorRenderModule.cs), [GridLineModule.cs](file:///C:/dev/grove-v9/src/GroveApp/Engine/GridLineModule.cs) |

---

## 1. Domain Separation & Context

In Grove v9, the **Spatial Grid & Cursor System** is architecturally decoupled from the affine camera matrix engine. While `CameraModule` manages viewport transforms \(T(x,y,s)\), `GridLineModule` handles multi-scale grid line rendering (44px minor subdivisions, 220px major cells, 1100px supercells), and `CursorRenderModule` manages grid cursor geometry and spent trail decay physics.

User Acceptance Testing (UAT) identified two visual deficiencies:
1. When zooming out, the grid cursor shrank into an unreadable sub-pixel square rather than adapting to the visible grid tier.
2. Moving or panning over multi-cell occupied items (2x2 notes, 4x4 documents) caused individual internal 1x1 cells to blink or flicker.

This document defines the normative specification for LOD-recursive grid cursor scaling and footprint-aware trail physics.

---

## 2. Root Cause Analysis (RCA) of Occupied Cell Blinking

### 2.1 Unscoped Spent Trail Cell Registration

#### Symptom
Panning over a 2x2 or 4x4 occupied item causes individual internal 1x1 cells inside the item to flash or blink.

#### Empirical Analysis
In [GridCanvasControl.cs](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs#L485-L489), cell transition logic registers `(_lastCursorCellX, _lastCursorCellY)` into `SpentCells` whenever the cursor cell coordinate `(cx, cy)` changes:

```csharp
if (cx != _lastCursorCellX || cy != _lastCursorCellY)
{
    _cursorRenderModule.RegisterCellTransition(SpentCells, _lastCursorCellX, _lastCursorCellY);
    _lastCursorCellX = cx;
    _lastCursorCellY = cy;
}
```

In [CursorRenderModule.cs](file:///C:/dev/grove-v9/src/GroveApp/Engine/CursorRenderModule.cs#L40-L50), `RegisterCellTransition` adds `(_lastCursorCellX, _lastCursorCellY)` as a **single 1x1 cell entry**.

```
+------------------------------------+
|  Occupied Note (2x2 Footprint)     |
|  +----------------+----------------+  <-- Spent Trail registers 1x1
|  | Cell (0,0) [X] | Cell (1,0)     |      single-cell trail boxes inside
|  +----------------+----------------+      occupied 2x2 footprint as
|  | Cell (0,1)     | Cell (1,1)     |      camera pans across cells!
|  +----------------+----------------+
+------------------------------------+
```

Because `RegisterCellTransition` does not verify whether the cell transition occurs inside an occupied item's footprint, every internal cell boundary crossed registers a 1x1 spent cell trail box. [RenderSpentTrail](file:///C:/dev/grove-v9/src/GroveApp/Engine/CursorRenderModule.cs#L55-L73) then fills these individual 1x1 cells with decaying trail ink (`Tokens.InkQuiet * spent.Energy`), producing a flickering effect on individual cells inside the item.

---

## 3. Normative Technical Specifications

### 3.1 Recursive LOD Grid Cursor Cell Occupancy Scaling

#### 3.1.1 LOD Scale Tier Snapping
The grid cursor footprint must dynamically scale its cell occupancy to match the active Level-of-Detail (LOD) grid tier being rendered by [GridLineModule.cs](file:///C:/dev/grove-v9/src/GroveApp/Engine/GridLineModule.cs):

| Zoom Range (\(s\)) | Active Grid LOD Tier | Grid Pitch | Cursor Cell Occupancy | Ring Stroke Weight |
| :----------------- | :------------------- | :--------- | :-------------------- | :----------------- |
| \(s \ge 0.5\)      | Minor Subdivisions   | 44px       | Single Minor Cell     | 2.0px              |
| \(0.1 \le s < 0.5\)| Major Grid Pitch     | 220px      | Single Major Cell     | 1.5px              |
| \(s < 0.1\)        | Supercell Pitch      | 1100px     | Single Supercell      | 1.0px              |

#### 3.1.2 Footprint Calculation Math
When hovering over empty canvas space, `CursorRenderModule` calculates cursor geometry based on the active LOD step size:

```csharp
public (int startX, int startY, double widthPx, double heightPx) CalculateLODCursorBounds(
    Point worldPt,
    double zoom,
    GridContentItem? targetItem)
{
    if (targetItem != null)
    {
        return (targetItem.CellX, targetItem.CellY, targetItem.CellWidth * Tokens.GridCell, targetItem.CellHeight * Tokens.GridCell);
    }

    double stepSize = zoom switch
    {
        >= 0.5 => Tokens.MinorCellSize, // 44px
        >= 0.1 => Tokens.GridCell,      // 220px
        _ => Tokens.SupercellPitch     // 1100px
    };

    int cx = (int)Math.Floor(worldPt.X / stepSize);
    int cy = (int)Math.Floor(worldPt.Y / stepSize);
    return (cx, cy, stepSize, stepSize);
}
```

The cursor remains clear, legible, and visually proportional across all zoom scales from 1% to 1000%.

---

### 3.2 Footprint-Aware Spent Trail Filtering

#### 3.2.1 Internal Footprint Transition Suppression
`RegisterCellTransition` must inspect whether both `(_lastCursorCellX, _lastCursorCellY)` and `(currentCellX, currentCellY)` fall within the cell bounds of the same active `GridContentItem`. If both coordinates belong to the same item, spent cell trail registration is suppressed.

#### 3.2.2 Item Perimeter Trail Registration
When transitioning off an item onto empty grid space, the spent trail registers the full item footprint boundary as a single decay entity, preventing boundary flickering.

---

## 4. Seam Architecture & Module Boundaries

- **[CursorRenderModule.cs](file:///C:/dev/grove-v9/src/GroveApp/Engine/CursorRenderModule.cs)**: Owns grid cursor geometry calculation, LOD step snapping, 22% fill painting, 2px inset ring rendering, and 18-step trail decay physics.
- **[GridLineModule.cs](file:///C:/dev/grove-v9/src/GroveApp/Engine/GridLineModule.cs)**: Owns LOD alpha distance fading calculations for minor, major, and supercell grid lines.

---

## 5. UAT Acceptance Criteria & Verification Plan

1. **LOD Recursive Cursor Verification**: Zoom out to 5% (`Zoom = 0.05`). Verify the grid cursor automatically expands to snap to 1100px supercells instead of shrinking to a sub-pixel dot.
2. **Occupied Cell Blinking Verification**: Place a 4x4 document on the canvas. Pan mouse back and forth across the interior cells of the document. Verify zero individual 1x1 cell blinking or internal trail flashing occurs.
3. **Trail Decay Verification**: Move cursor rapidly across empty grid space at 100% zoom. Verify 18-step trail decays smoothly per `Tokens.CursorTrailDecay` (0.84).
