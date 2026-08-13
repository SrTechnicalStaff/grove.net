---
type: canonical-domain-concept
status: accepted
authority: source-of-truth
concept: Grid
date: 2026-08-12
---

# Grid

## Definition

The Grid is Grove's persistent spatial field: an unbounded coordinate system
where Content can be positioned relative to other Content. It gives a person a
stable place to arrange, revisit, and relate Content without requiring folders,
tags, or a fixed document hierarchy.

The Grid is not the Memory store. It stores or references spatial Content
instances and their Placement facts. A Memory can exist without the Grid.

## What the Grid provides

- integer cell coordinates and a stable origin convention;
- a set of Grid Layers sharing the same horizontal coordinate system;
- occupancy and collision decisions for Content Placement;
- spatial navigation, selection, movement, and resizing;
- the cell evidence consumed by the Field Ledger.

## What the Grid does not provide

- semantic identity for a Memory;
- a placed/unplaced state for a Memory;
- ownership of Memory payload or version lineage;
- search results as a collection of spatial representatives;
- authored Anchor context on its own.

## Relationship to Content

Every visible semantic object on the Grid is Content. Content references one
Memory and owns the spatial facts that let the Grid render and manipulate it.
Placing an existing Memory therefore creates Content; it does not move the
Memory into the Grid.

## Relationship to the Field Ledger

The Grid supplies cell coordinates and Content geometry. The Field Ledger
derives energy and source evidence from that geometry. The Grid remains usable
when a field visualization is unavailable; field evidence is a derived service,
not the spatial identity of the Grid.

## Boundary decisions

| Tension | Decision |
| --- | --- |
| Grid vs Memory | Grid is spatial; Memory is semantic. |
| Grid vs Content | Grid hosts Content; Content is the placed object. |
| Grid vs Grid Layer | Grid Layers are the depth members of one Grid coordinate system. |
| Grid vs Plane | Grid is the spatial model; the Spatial Grid Plane is its visual projection. |
| Grid vs Slate | A Slate can browse or edit records without changing camera or cell state. |
