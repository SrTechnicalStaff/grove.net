---
type: product-outcome
status: implemented-verified
date: 2026-08-08
owner: "Grove Product"
roadmap: "[[../../roadmaps/Grid Plane roadmap]]"
source_notes:
  - "[[../../raw/original-notes/Grove controls]]"
  - "[[../../raw/original-notes/Grove at a distance]]"
---

# Navigate the Grid

## Purpose

Define how a person moves through the Grid and changes scale without losing the
ability to act on the Content they came to see.

## Background

Navigation combines pan, focal zoom, framing, coordinate conversion, and the
current Layer. The browser slice supports movement, focal zoom, framing, and
recoverable Camera state. The wider 1%–1000% scale contract is completed
separately by the Camera zoom task.

## Outcome

> I can move through the Grid, change its scale, and stay oriented enough to act deliberately wherever I arrive.

## Behavior scenarios

### Move through the Grid

I pan to another region and the Grid, Content, and cursor remain aligned.

### Change scale

I zoom around the point I am examining rather than losing the area under my
attention.

### Frame the current Layer

I frame the current Layer and see its Content without changing the underlying
Camera state unexpectedly.

### Recover orientation

After a large movement or reload, the focal position and scale remain explicit
enough for me to continue working.

## Context

Camera owns view position and scale. Grid owns cell projection. Layers determine
which Content is visible. Distant representation and Annotation are consumers
of navigation, not Camera responsibilities.

## Decisions

- Navigation must preserve focal continuity.
- Framing is scoped to the current Layer.
- Camera and Information Plane state remain separate.

## Non-goals

- Spatial retrieval.
- Annotation generation or distant reading.
- Layer CRUD or Content transfer.

## Evidence

- Current slice: `js/workspace/camera.js`, `qa/scenarios/navigate-grid.json`
- Discovery: [[Camera zoom and distant representation]]
- Decision: [[../../decisions/Grid navigation focal continuity]]
- Wireframe: [[../../ux/wireframes/Navigate the Grid]]
