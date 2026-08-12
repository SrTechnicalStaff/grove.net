---
type: product-outcome
status: implemented-verified
date: 2026-08-08
owner: "Grove Product"
roadmap: "[[../../roadmaps/Grid Plane roadmap]]"
source_notes:
  - "[[../../raw/original-notes/Grove at a distance]]"
---

# Control Camera zoom

## Purpose

Define the full Camera lens and the representations that make distant work
understandable.

## Background

Grove's lens spans from 1% to 1000% with focal continuity. Content follows
ordinary Grid projection while readable, then uses the accepted stepped and
distant fidelity tiers without changing its Placement facts. Broader
scene-meaning grammar remains Discovery-owned.

## Outcome

> I can zoom the Grid from 1% to 1000% and stay oriented as the view changes scale.

## Behavior scenarios

### Zoom in and out

I change scale around the point under attention and the focal Content remains
reachable.

### Cross the representation thresholds

As Content becomes distant, Grove changes its visual representation without
losing the route back to the underlying Content.

### Return from distance

I can identify a distant region and return to a usable Content-level view.

## Context

Camera contains only view position, scale, and coordinate math. Grid owns the
only projection. Annotation or overview features may later derive facts from
Grid extent, but no plane subscribes to Camera and Camera does not create or
select a representation.

## Decisions

- The accepted range is 1% through 1000%.
- Focal continuity is required at every scale.
- Content follows natural Grid scale while readable; accepted fidelity tiers
  preserve the same cell footprint and source route at distance.
- Representation remains derived Grid projection state outside Camera and does
  not become a second Content store.

## Non-goals

- Spatial retrieval.
- Annotation publication rules.
- Changing Grid cell geometry.

## Evidence

- Discovery: [[Camera zoom and distant representation]]
- Implemented slice: pure Camera math and natural Grid projection in
  `js/workspace/camera.js`, `js/workspace/grid-renderer.js`, and
  `tests/camera.test.mjs`
- Verified task: [[../../engineering/tasks/T-G07 Control Camera zoom]]
- Evidence: [[../../engineering/evidence/T-G07 Control Camera zoom]]
- Browser journey: `qa/scenarios/control-camera-zoom.json`
- Decision: [[../../decisions/Camera zoom range and representation]]
- Projection decision: [[../../decisions/Camera observer and Grid projection boundary]]
- Wireframe: [[../../ux/wireframes/Control Camera zoom]]
