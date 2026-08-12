---
type: product-outcome
status: implemented-verified
date: 2026-08-07
owner: "Grove Product"
roadmap: "[[../../roadmaps/Grid Plane roadmap]]"
---

# Arrange content

## Purpose

Define how a person changes the position or footprint of existing Content.

## Background

Arranging Content must preserve the intended selection, Grid geometry, and
unrelated Content. A blocked group must not leave a partial move behind.

## Outcome

> I can move or resize selected Content on the Grid while keeping other Content where it is.

## Behavior scenarios

### Move one item

I drag selected Content to open cells, see its complete footprint preview, and
commit the new address.

### Move a group

When several items are selected, Grove preserves their relative offsets and
commits the group as one change.

### Refuse a blocked result

When any destination cell is occupied, the complete preview is invalid and no
item moves.

### Resize Content

I enter Resize, see the new complete footprint, and commit only when every
requested cell is valid.

### Recover

Undo, Redo, cancellation, and reload preserve the last committed arrangement.

## Context

Selection identifies the target set. Placement owns address and footprint. Grid
owns occupancy and snapping. Aura Field recalculates after committed geometry.

## Decisions

- Move and Resize use the same preview and collision rules as commit.
- Group movement is atomic.
- No-op gestures do not enter history.

## Non-goals

- Moving Content between Layers.
- Trace, Duplicate, Copy, or Cut.
- Automatic layout or collision rearrangement.

## Evidence

- Model: [[../../reference/Grid and Placement model]]
- Persistence boundary: [[../../decisions/Persistence and operation history atomicity]]
- Browser journey: `qa/scenarios/arrange-content.json`
- Implementation evidence: [[../../engineering/evidence/T-F01 Versioned field kernel]]
