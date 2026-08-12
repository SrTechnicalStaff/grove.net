---
type: product-outcome
status: implemented-verified
date: 2026-08-07
owner: "Grove Product"
roadmap: "[[../../roadmaps/Grid Plane roadmap]]"
---

# Place content

## Purpose

Define the result of putting a new piece of Content onto the Grid.

## Background

Content previously required a deliberate Grid gesture, but the result was only
useful when the chosen cell, Layer, footprint, and collision state were clear
before commit.

## Outcome

> I can place a piece of Content on the Grid at the exact cell and Layer I choose, so it stays where I put it.

## Behavior scenarios

### Place new Content

I choose a Content type, point to an open Grid location, see the complete
footprint preview, and commit the placement. Grove creates the Content and its
Placement at the previewed cells on the current Layer.

### Refuse a collision

When the requested cells overlap existing Content, Grove shows the placement as
invalid and commits nothing.

### Cancel before commit

When I cancel a preview, Grove removes the provisional state and leaves no empty
Content behind.

### Reopen the application

After reload, the Content returns at the same Layer, cells, footprint, and
presentation.

## Context

Grid owns cells and occupancy. Placement owns the address and footprint. Layer
owns semantic depth. Memory owns the durable identity behind the Content form.

## Decisions

- Preview and commit use the same collision rule.
- A completed placement is one durable transition.
- A cancelled or invalid preview is not durable state.

## Non-goals

- Full Note, Document, or Image editing.
- Moving or resizing established Content.
- Automatic collision rearrangement.

## Evidence

- Model: [[../../reference/Grid and Placement model]]
- Identity: [[../../reference/Content and Memory model]]
- Browser journey: `qa/scenarios/place-content.json`
- Implementation state: [[../../engineering/Implementation status]]
