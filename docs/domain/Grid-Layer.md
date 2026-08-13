---
type: canonical-domain-concept
status: accepted
authority: source-of-truth
concept: Grid Layer
date: 2026-08-12
---

# Grid Layer

## Definition

A Grid Layer is a literal depth member of the Grid. Every Grid Layer shares
the same horizontal cell coordinate system, so the same `(x, y)` coordinate
can be occupied independently on multiple Grid Layers.

The word **Layer** is reserved for this Grid concept. The visual composition
surfaces are Planes, and their internal groupings are Tiers.

## State

A Grid Layer has:

- a stable Grid Layer identity and relative depth;
- optional user-facing name or label;
- Content whose Placement belongs to that Grid Layer;
- active/inactive viewing state controlled by the Grid navigation context.

The ground Grid Layer is the base reference. Additional Grid Layers may be
created above or below it. Their labels and ordering are presentation of depth,
not semantic meaning assigned to the Memories they contain.

## Vertical field behavior

Aura may permeate between Grid Layers. A Content source retains its native
Grid Layer, while its field can contribute to cells on other Grid Layers with
the configured vertical decay. A cross-Grid-Layer field contribution does not
move Content and does not create another Content instance.

Perimeter contours are same-Grid-Layer geometry: a contour describes the
boundary of Content on the active Grid Layer only. Field energy from other Grid
Layers may affect saturation, but it must not create a contour that persists
through those Grid Layers.

## Navigation and mutation

Changing the active Grid Layer changes the spatial viewing context. It does
not change Memory identity, Content identity, or Placement state. Creating,
deleting, reordering, or renaming a Grid Layer changes the Grid composition,
not the semantic records.

## Boundary decisions

| Tension | Decision |
| --- | --- |
| Grid Layer vs Plane | A Grid Layer is spatial depth; a Plane is visual composition. |
| Grid Layer vs Tier | A Grid Layer is data/spatial state; a Tier is a visual or representation grouping. |
| Grid Layer vs Content | Content belongs to one Grid Layer through Placement. |
| Grid Layer vs Aura | Aura can cross Grid Layer boundaries; Content cannot silently change Grid Layer. |
| Grid Layer vs Anchor | Anchor is authored context on Content, not a Grid Layer marker. |
