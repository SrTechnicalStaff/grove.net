---
type: product-outcome
status: discovery
date: 2026-08-07
owner: "Grove Product"
roadmap: "[[../../roadmaps/Grid Plane roadmap]]"
---

# Feel presence across many Layers

## Purpose

Keep the presence field ambient and truthful as the Layer stack grows tall,
so cross-Layer awareness scales with the vault instead of throttling it.

## Background

The presence field currently rebuilds by sampling every placement on every
Layer and materializing a document node per lit cell. At a tall stack with
broad fields, that rebuild becomes the most expensive object in the
application — and it runs during interaction. The field is a scalar surface
by nature; the chunked accumulation that renders it as one inexpensive
texture is hypothesized in
[[../../discovery/Grid performance and representation at scale]].

## Outcome

> I can feel where work has gathered across a tall stack of Layers without the field slowing the Grid, so presence stays ambient instead of becoming a cost.

## Behavior scenarios

### A tall stack still glows

Given on the order of a hundred Layers with occupied cells and field reach,
when I view any Layer, Grove renders the accumulated presence of the whole
stack, so depth of organization enriches the field instead of being rationed.

### Presence during interaction

Given that same stack, when I move Content, change selection, or pan, Grove
keeps the field alive and current without the interaction stuttering, so
presence accompanies my work rather than interrupting it.

### Local change, local cost

Given a small edit on one Layer, when the field updates, Grove recomputes
only the region and Layers the change can reach, so a single placement never
triggers a whole-field rebuild.

### Faithful accumulation

Given overlapping fields from many Layers, when contributions accumulate,
Grove preserves the same brightness, coloring, and perimeter meaning the
field has today, so scale changes the cost of presence, never its truth.

## Context

Field semantics — reach, accumulation, coloring, and perimeter — are owned by
their existing field contracts and
[[../../discovery/Memory–Field relationships]]; this outcome owns their cost
at scale. Chunked accumulation and texture rendering are hypothesized in
[[../../discovery/Grid performance and representation at scale]].

## Decisions

- Field values are per-cell facts; rendering preserves cell quantization,
  hard cell boundaries, and perimeter meaning exactly — never a smoothed
  gradient.
- Field visual semantics are preserved exactly; the representation medium may
  change, the meaning may not.
- Field update cost is priced by affected chunks and reachable Layers, never
  by total vault size.

## Non-goals

- Changing field reach, accumulation math, or perimeter rules.
- Capping Layer count or field participation to protect performance.
- Persisting the rendered field; it remains derived state.

## Evidence

- Wireframe: [[../../ux/wireframes/grid-scale/Presence across Layers]]
- Discovery: [[../../discovery/Grid performance and representation at scale]]
- QA or implementation evidence: [[../../engineering/evidence/T-F02 Field
  stream and presence texture]].
