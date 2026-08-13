---
type: product-definition
status: derived
authority: derived-from-domain-model
source_of_truth: ../../domain/Grid.md
version: v9
date: 2026-08-11
tags: [grove, product-definition, grid, spacetime, cell-continuum]
---

# Product Definition: Grid

> **What is the grid?**
> The **Grid** is the continuous 2D spatial substrate and coordinate system of Grove. It acts as a digital spacetime continuum where every cell holds an innate, non-zero information value, establishing spatial bearing, proximity relationships, and field propagation across all placed work.

---

## 1. Core Essence ("What is the grid?")

- **Canonical Statement**: The Grid is the fundamental 2D spatial continuum that underlies Grove's placed content. It provides the metric coordinate space `(x, y)` in which Content is positioned, Auras propagate, and spatial proximity creates organic semantic relationships.
- **Primary Function**: The Grid replaces traditional hierarchical file systems and tree views with spatial physics. Simply placing Content on the Grid establishes its bearing relative to all other content, treating location and distance as primary carriers of meaning.
- **Mental / Physical Model**: 
  1. *Spacetime Continuum*: Like physical spacetime, the Grid itself is not empty null space; every cell has intrinsic information value. Content creates mass, casting Aura fields that curve local space.
  2. *Microsoft Excel Cell Grid*: Grid cells, selection borders, cursor markers, and marquee dynamics draw directly from cell spreadsheet interactions—the world's most intuitive spatial manipulation model.

---

## 2. Fundamental Invariants & System Properties

1. **Strict 2D Geometry**: The Grid is strictly two-dimensional `(x, y)`. Depth is achieved by stacking 2D coordinate fields across literal **Grid Layers**.
2. **Innate Non-Zero Value**: No cell on the Grid has a value of zero. The simple existence of a spatial coordinate holds structural value for positioning and context.
3. **Implicit Proximity Bearing**: Relationships between Content items are innate; proximity on the Grid automatically establishes cognitive context without needing explicit hyperlinking or tag schemas.
4. **Excel-Class Cell Interactions**: Cell selection, border accents, armed cursor markers, and range marquee behaviors mirror spreadsheet cell interaction patterns.
5. **Deliberate & Frictionless Movement**: Moving Content across Grid cells is tuned to be strictly deterministic—neither "slippery" (uncontrolled momentum) nor "sticky" (excessive snapping resistance).

### Data & State Schema
- **State Ownership**: Core Grid Spatial Engine (`GridSpatialEngine`).
- **Grid Coordinates**:
  - `cell_x`, `cell_y`: Signed integer cell index.
  - `cell_size`: Pixel dimension constant per zoom level.
  - `occupancy_table`: Map of `(x, y, layer_id) -> content_id`.
  - `field_ledger_table`: Map of `(x, y) -> CellFieldLedgerRecord`.
- **Persistence Boundary**: Layout state saved to the Grove spatial config (`.grove/spatial_grid.json`).

---

## 3. Intersectionality Matrix

| Primitive | Intersection & Relational Rules |
| :--- | :--- |
| **Grid** | *Self-Intersection*: The Grid provides the infinite 2D coordinate space where cells neighbor one another, establishing metric distances and cell boundaries. |
| **Memory** | The Grid gives spatial context to Content. Placing a Memory creates Content with a `MemoryId` at specific `(x, y)` cell coordinates; the Memory itself gains no spatial state. |
| **Content** | Content occupies a bounded rectangle of Grid cells. Content movement, resizing, and collision detection occur directly on Grid cell coordinates. |
| **Aura** | Aura radiates outward from Content across Grid cells. The Field Ledger records Aura intensity and source metadata inside each Grid cell. |
| **Grid Layer** | Stacking 2D coordinate fields along the depth axis forms Grid Layers. Each Grid Layer has an identical cell coordinate grid, enabling precise vertical cell alignment during Content tracing. |
| **Annotation** | Annotations exist on the Information Plane, floating above the Grid; they query Grid cell coordinates and center on the highest Aura concentration on the Grid. |
| **Blip** | Blips anchor to specific Grid cell coordinates where dense Content clusters and Aura overlaps reach local maximum thresholds. |
| **Slates** | Writing Slate, Memory Slate, and Gallery Slate are viewport-fixed surfaces above the Grid; opening one suspends active Grid interaction while keeping Grid layout intact underneath. |

---

## 4. User Interaction & Camera Dynamics

- **Cursor Armed States (`1` key cycle)**:
  - Cursor interaction with Grid cells relies on clear accents, border highlights, and color coding (Trace, Resize, Copy, Cut, Duplicate) without cluttered text badges.
  - Marquee selection requires covering all cells occupied by Content to register selection.
- **Camera Viewport & Zoom**:
  - The camera acts as a lens moving over the 2D Grid plane.
  - Panning across the Grid is fluid and frictionless.
  - Quiet grid lines: Grid line visibility scales dynamically with camera distance to prevent visual clutter.
- **Navigation Controls**:
  - Spatial movement across cells is controlled via keyboard directional binds and mouse drag.

---

## 5. Derived Outcomes Mapping

- **[Spatial Association Without Forcing Structure](../derived-outcomes/Spatial%20Association%20Without%20Forcing%20Structure.md)**: Users organize work naturally by placing items near each other on the 2D Grid.
- **[Move Through A Grown Grid](../derived-outcomes/Spatial%20Association%20Without%20Forcing%20Structure.md)**: Users navigate sprawling knowledge bases through intuitive 2D spatial movement.
- **[Quiet The Grid Lines](../derived-outcomes/Spatial%20Association%20Without%20Forcing%20Structure.md)**: The spatial field stays visually clean and unobtrusive regardless of scale.
