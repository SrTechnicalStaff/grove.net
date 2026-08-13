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
The original implementation registered `(_lastCursorCellX, _lastCursorCellY)` into `SpentCells` whenever a base cursor coordinate changed. That was the defect: it made the trail a second, base-cell cursor model. The corrected implementation keeps the previous resolved `CursorDescriptor` and registers its complete world footprint only when that descriptor footprint changes:

```csharp
_cursorModel.Apply(resolvedDescriptor);
```

`CanonicalCursorTrailModel.Apply` deposits the previous descriptor's `WorldOrigin` and `WorldExtent`, preserving 44px, 220px, 1100px, content, and armed-tool footprints as one model.

The previous diagram showed four internal 1×1 trail boxes. That diagram is
intentionally removed: the corrected trail is one world-space rectangle equal
to the previous descriptor footprint.

The descriptor comparison suppresses movement within one occupied footprint. When the pointer leaves it, the full previous footprint is deposited as one world-space trail entity, so internal base-cell boundaries cannot create a second 1x1 cursor trail.

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
When hovering over empty canvas space, `CursorRenderModule` resolves one `CursorDescriptor` from the active grid tier. Camera scale selects the visible tier; the tier pitches themselves remain grid constants:

```csharp
public CursorDescriptor ResolveCursorDescriptor(
    Point worldPt,
    double zoom,
    GridContentItem? targetItem)
{
    if (targetItem != null)
    {
        return CreateDescriptor(
            targetItem.CellX * Tokens.GridCell,
            targetItem.CellY * Tokens.GridCell,
            targetItem.CellWidth * Tokens.GridCell,
            targetItem.CellHeight * Tokens.GridCell,
            CursorFootprintKind.Content,
            zoom);
    }

    double stepSize = zoom switch
    {
        >= 0.5 => Tokens.MinorCellSize, // 44px
        >= 0.1 => Tokens.GridCell,      // 220px
        _ => Tokens.SupercellPitch     // 1100px
    };

    double originX = Math.Floor(worldPt.X / stepSize) * stepSize;
    double originY = Math.Floor(worldPt.Y / stepSize) * stepSize;
    return CreateDescriptor(
        originX, originY, stepSize, stepSize,
        CursorFootprintKind.MinorGrid, zoom);
}
```

The cursor remains clear, legible, and visually proportional across all zoom scales from 1% to 1000%.

---

### 3.2 Footprint-Aware Spent Trail Filtering

#### 3.2.1 Internal Footprint Transition Suppression
`CanonicalCursorTrailModel.Apply` compares the previous and current resolved descriptor footprints. If their world origin and extent are unchanged, spent trail registration is suppressed.

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
