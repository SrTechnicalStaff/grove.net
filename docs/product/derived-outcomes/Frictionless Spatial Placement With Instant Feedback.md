---
type: derived-outcome
status: active
date: 2026-08-11
tags: [grove, derived-outcome, placement, grid, preview, refusal]
---

# Product Outcome: Frictionless Spatial Placement With Instant Feedback

> **Outcome Statement**: 
> "I want to see exactly where and how my content will land before committing a move, with instant feedback if a location is unavailable."

---

## 1. Human Behavior Shift

### Previous Behavior (Status Quo)
When dragging cards or assets in traditional canvas software, users must guess whether an item will fit into a target space or overwrite existing work. Letting go of an item often results in unexpected overlapping, auto-reflowing layouts, or cryptic error pop-ups that require undoing the operation.

### Transformed Behavior
As users drag an item or preview a placement across the grid, the system renders a transparent, dashed footprint preview showing exact cell dimensions (`3×3`, `4×5`). The origin ghost stays in place until release, ensuring no state is altered during transit. If a space is occupied or invalid, the preview outline immediately shifts to the invalid signal role (`#E2625C`), blocked cells display structural diagonal hatching, and a plain inline sentence explains the refusal directly beside the pointer without popping up modal alerts.

---

## 2. Underlying Product Architecture & Primitives

This feedback loop is derived from the Grove Design Catalogue (`grid-plane/07-selection-placement.html`) and powered by:

- **[Grid](../definitions/Grid.md)**: Evaluates cell geometry and cell availability in real time, projecting exact cell boundaries at any camera zoom.
- **[Content](../definitions/Content.md)**: Projects complete footprint previews during movement without mutating underlying memory coordinates until pointer release.
- **[Aura](../definitions/Aura.md)**: Updates cell presence dynamically as the placement preview moves across grid coordinates.

---

## 3. Behavioral Verification Scenarios

### Scenario 1: Real-Time Footprint Preview During Drag
- **Given** a user is dragging a 2×2 note across the grid plane,
- **When** the cursor moves across cell coordinates,
- **Then** a dashed preview footprint moves in real time showing exact target cells and dimensions, while the original note remains untouched at its source position.

### Scenario 2: Clear Inline Refusal Explanation
- **Given** a user drags a document preview over cells that are already occupied by another card,
- **When** the footprint overlaps the occupied space,
- **Then** the preview outline turns red (`#E2625C`), blocked cells display diagonal hatch marks, and an inline status bar explains "This space is occupied" without interrupting camera movement.
