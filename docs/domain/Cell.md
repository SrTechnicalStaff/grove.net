---
type: canonical-domain-concept
status: accepted
authority: source-of-truth
concept: Cell
date: 2026-08-12
---

# Cell

## Definition

A Cell is one addressable unit of the Grid coordinate system. A cell is the
smallest unit used for Content occupancy, cursor placement, collision checks,
and Field Ledger evidence.

A cell is a coordinate and a derived state. It is not a container for a
Memory, and it does not own Content.

## Cell state

For a Grid Layer, a cell may have:

- zero or more occupying Content footprints, subject to collision rules;
- derived Field Ledger energy;
- zero or more contributing Content source references;
- Aura overlap or saturation evidence;
- Gap evidence when no Aura overlaps but nearby Content remains relevant.

Cell state is scoped to a Grid Layer for occupancy and contour decisions. Field
energy may include contributions from other Grid Layers according to vertical
permeability rules.

## Membrane rule

Cells are the membrane between spatial arrangement and semantic recall. They
carry evidence about where Content is and what nearby Content contributes, but
they never become semantic records and never rewrite a Memory.

## Boundary decisions

| Tension | Decision |
| --- | --- |
| Cell vs Content | A Content footprint occupies cells; a cell does not own Content. |
| Cell vs Aura | Aura is derived field evidence over cells. |
| Cell vs Gap | A Gap is a relation across cells, not a special Memory or Content type. |
| Cell vs Memory | A cell may identify Content that joins to Memory, but it never contains a Memory. |
