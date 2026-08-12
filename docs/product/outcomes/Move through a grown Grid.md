---
type: product-outcome
status: discovery
date: 2026-08-07
owner: "Grove Product"
roadmap: "[[../../roadmaps/Grid Plane roadmap]]"
---

# Move through a grown Grid

## Purpose

Commit the Grid to staying fluid as the vault grows, so navigation quality is
a promise of the product rather than a property of small collections.

## Background

Pan and zoom currently redraw every grid line at every scale, keep a live
document node for every placement on the current Layer, and serialize the
complete application state after camera gestures. Each cost grows with the
vault, so movement that is smooth today degrades as work accumulates. The
engine changes that remove this coupling are hypothesized in
[[../../discovery/Grid performance and representation at scale]].

## Outcome

> I can pan and zoom a Grid that has grown large as smoothly as an empty one, so the amount I have gathered never makes moving through it feel heavy.

## Behavior scenarios

### Continuous pan across dense work

Given a Layer holding hundreds of placed items, when I pan across the field in
one continuous gesture, Grove keeps the motion smooth for the whole gesture,
so I can sweep across my work without stutter marking the dense areas.

### Continuous zoom through the full range

Given the same dense Layer, when I zoom continuously between the nearest and
farthest scales, Grove holds a steady frame rate through the whole range, so
changing altitude never feels like a cost I should avoid.

### Movement never waits on saving

Given any amount of placed work, when I finish a pan or zoom gesture, Grove
saves my viewpoint without pausing the view, so navigation never hitches after
the gesture ends.

### Grown vault, same feel

Given a vault that has grown to many Layers and thousands of placements, when
I navigate any Layer, Grove feels the same as it did when the vault was new,
so growth changes what I have, not how it moves.

## Context

Camera math and focal behavior are complete under
[[Control Camera zoom]] and [[Navigate the Grid]]; this outcome owns the
sustained smoothness of that navigation at scale. The engine hypotheses,
scale fixtures, and frame budgets live in
[[../../discovery/Grid performance and representation at scale]].

## Decisions

- Frame cost must be a function of what is visible and what changed, never of
  vault size; the specific budgets are fixture evidence, not user settings.
- Smoothness may not be bought by hiding, unloading, or relocating Content;
  simplification of representation is the only permitted response to scale.

## Non-goals

- Changing camera range, focal rules, or Grid geometry.
- A user-facing performance mode or quality setting.
- Desktop-shell dependencies; this outcome must hold in the browser.

## Evidence

- Wireframe: [[../../ux/wireframes/grid-scale/Grown Grid navigation]]
- Discovery: [[../../discovery/Grid performance and representation at scale]]
- QA or implementation evidence: synthetic-vault navigation traces recorded by
  the QA harness once slices exist.
