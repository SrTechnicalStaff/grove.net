---
type: canonical-domain-concept
status: accepted
authority: source-of-truth
concept: Placement
date: 2026-08-12
---

# Placement

## Definition

Placement is the act of creating or positioning Content on the Grid and the
spatial facts that result. It is never a state of Memory.

## Spatial facts

A Content Placement includes:

- grid cell origin;
- cell footprint width and height;
- Grid Layer identity;
- occupancy and collision state;
- spatial index identity used by the Grid and Field Ledger.

The Placement points to Content. Content points to Memory. There is no direct
Memory-to-Placement ownership relationship.

## Placement of an existing Memory

When a person chooses an existing Memory and a destination, Grove creates
Content with that Memory's `MemoryId`, validates the destination, and commits
the spatial facts. The Memory is unchanged.

## Placement of a new source

When a new source has no Memory, Grove commits the source as a Memory and then
creates Content referencing it. If validation fails before Content commit, the
new source must not be left with a fabricated or partially placed Content
record.

## Changes after placement

Moving, resizing, tracing, and deleting modify Content-side spatial records.
They do not convert a Memory between states. A later placement can create
another Content instance for the same Memory.

## Boundary decisions

| Tension | Decision |
| --- | --- |
| Placement vs Memory | Placement is spatial state; Memory has none. |
| Placement vs Content | Placement is owned by Content. |
| Placement vs Anchor | Anchor labels Content and may accompany Placement; it does not own geometry. |
| Placement vs Field Ledger | Placement changes cause field recalculation; the ledger observes the result. |
