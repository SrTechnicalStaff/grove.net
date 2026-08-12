# UAT-ADR-004: Field Ledger Aura Surface Visualization, Sub-Cell Bleed Suppression & Dynamic Merging Contour Outlines

| Metadata Field    | Value                                                                     |
| ----------------- | ------------------------------------------------------------------------- |
| **Status**        | Proposed / In Review                                                      |
| **Date**          | 2026-08-12                                                                |
| **Authors**       | Senior Technical Staff / Grove Architecture Group                         |
| **Classification**| Architectural Decision Record (ADR) / User Acceptance Testing (UAT) Spec  |
| **Target Seam**   | [FieldLedgerEngine.cs](file:///C:/dev/grove-v9/src/GroveApp/Engine/FieldLedgerEngine.cs), [FieldLedgerModule.cs](file:///C:/dev/grove-v9/src/GroveApp/Engine/FieldLedgerModule.cs) |

---

## 1. Domain Separation & Context

In Grove v9, the **Field Ledger Engine** ([FieldLedgerEngine.cs](file:///C:/dev/grove-v9/src/GroveApp/Engine/FieldLedgerEngine.cs)) manages spatial energy accumulation fields governed by:

\[
E = \frac{M}{1 + 0.4 \cdot d^2}
\]

where \(M\) represents item mass and \(d\) is spatial distance in grid units. The visual representation of these energy fields is rendered by [FieldLedgerModule.cs](file:///C:/dev/grove-v9/src/GroveApp/Engine/FieldLedgerModule.cs).

User Acceptance Testing (UAT) identified two visual deficiencies in aura field rendering:
1. Aura heatmap fills exposed underlying minor black subdivision lines through the color fill, creating a fractured, noisy surface appearance.
2. Aura fields lacked dynamic perimeter contour outlines ("drawing outline technique"), failing to render a clean, unified boundary hull when multiple energy fields combined or collided.

This document defines the normative specification for clean aura surface rendering and dynamic merged perimeter contour outlines.

---

## 2. Root Cause Analysis (RCA) of Sub-Cell Line Bleed

### 2.1 Render Order & Subdivision Grid Bleed

In [GridCanvasControl.cs](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs#L924-L926), render order passes execute in the following sequence:

```csharp
// 1. Field Ledger Module: Heatmaps & Aura
_fieldLedgerModule.RenderFieldLedger(context, WorldToScreen, CellSize, Zoom, minCellX, maxCellX, minCellY, maxCellY, FieldEngine, Items);

// 2. Grid Line Module: Major 220px, Minor 44px Subdivisions
_gridLineModule.RenderGridLines(context, WorldToScreen, CellSize, Zoom, minCellX, maxCellX, minCellY, maxCellY);
```

#### Empirical Cause
Because `RenderGridLines` was drawn *after* `RenderFieldLedger`, minor 44px grid subdivision lines (`#161618`) were painted directly on top of the translucent aura energy fills. This exposed black sub-cell grid lines running through the interior of active energy fields, fracturing the visual surface.

---

## 3. Normative Technical Specifications

### 3.1 Sub-Cell Line Bleed Suppression

```
+------------------------------------+
|  Clean Aura Surface (220px Major)  |
|                                    |  <-- Sub-cell 44px minor lines
|  (Solid, contiguous energy fill)   |      are SUPPRESSED underneath
|                                    |      active aura cells
+------------------------------------+
```

1. **Contiguous Surface Rendering**: `FieldLedgerModule` must paint aura energy fills as solid, contiguous regions for the active cell tier size.
2. **Subdivision Line Masking**: `GridLineModule` must clip or suppress minor subdivision lines (44px) inside cells where aura energy alpha exceeds zero (\(E > 0\)). This ensures clean, un-fractured aura surfaces.

---

### 3.2 Dynamic Merging Contour Outline Hulls

```
Combined Energy Field Contour Hull:
+------------------------------------+
| Energy Aura Field 1                |
|                                    +--------------------+
| (No internal subdivision bleed)    | Energy Aura Field 2|
+------------------------------------+                    |
| Shared Merged Perimeter Outline    (Dynamic Fusion Hull)|
+---------------------------------------------------------+
```

#### 3.2.1 Boundary Segment Detection
`FieldLedgerEngine` evaluates the topological adjacency of active aura cells in the ledger. For each cell \((c_x, c_y)\) with active energy (\(E > 0\)), its four edges (North, South, East, West) are evaluated:
- An edge is classified as a **Perimeter Segment** if the adjacent neighbor cell in that direction has zero aura energy (\(E = 0\)).
- An edge is classified as an **Internal Shared Edge** if the adjacent neighbor cell also has active aura energy (\(E > 0\)).

#### 3.2.2 Dynamic Fusion Contour Hull
- Internal Shared Edges omit perimeter strokes entirely.
- Perimeter Segments are joined into a continuous, merged vector contour hull encompassing the combined energy field.
- When two independent aura fields expand or move closer together until their cells touch, their boundary segments dynamically update, dissolving internal edges and uniting into a single continuous outer boundary contour ("drawing outline technique").

#### 3.2.3 Contour Stroke Styling
Perimeter contour hulls paint with standard normative design system tokens:
- **Stroke Weight**: 1.5px (`Tokens.StrokeContainment`).
- **Stroke Color**: `--signal-interaction` (`#96B6F8`) for active fields, `--k-edit-b` (`#7A3F3A`) for static fields.
- **Dash Style**: Solid vector stroke with anti-aliased edge smoothing.

---

## 4. Seam Architecture & Module Boundaries

- **[FieldLedgerEngine.cs](file:///C:/dev/grove-v9/src/GroveApp/Engine/FieldLedgerEngine.cs)**: Calculates energy values \(E\), queries active aura cell regions, and extracts perimeter boundary segments for contour hull construction.
- **[FieldLedgerModule.cs](file:///C:/dev/grove-v9/src/GroveApp/Engine/FieldLedgerModule.cs)**: Paints contiguous aura surface fills and draws dynamic perimeter contour hulls on the Skia drawing context.

---

## 5. UAT Acceptance Criteria & Verification Plan

1. **Sub-Cell Bleed Suppression Verification**: Place a note on the grid. Inspect the surrounding aura heatmap at 200% zoom. Verify zero black minor subdivision lines (44px) are visible inside the colored aura fill.
2. **Dynamic Contour Hull Verification**: Place two notes 3 cells apart. Gradually move them closer until their aura fields overlap. Verify their individual perimeter outlines dynamically dissolve internal shared borders and merge into a single continuous outer outline.
