---
type: product-outcome
status: discovery
date: 2026-08-08
owner: "Grove Product"
roadmap: "[[../../roadmaps/Product Roadmap]]"
source_notes:
  - "[[../../raw/original-notes/Grove - notes]]"
  - "[[../../raw/original-notes/Grove - information layer]]"
---

# Place a Memory

## Purpose

Define the cross-surface placement function that gives a Memory a Grid
location without making Placement part of Memory or Content ownership.

## Background

The Memory Slate shows a Memory in its recorded state, whether or not it has a
Placement. Placement is deliberate: a person chooses when a Memory should occupy
a Grid location. Starting Placement from the Slate enters the normal Grid
Placement journey; it never silently changes the view just because a Memory was
opened.

## Outcome

> I can take a Memory from the Slate and give it a place on the Grid, so where I keep it does not decide what it is.

## Behavior scenarios

### Start placement from the Slate

I choose a Memory and an explicit place action, and the Grid resolves a
footprint at a location I pick without blurring the Memory itself.

### Keep the record whole

Placing, moving, or leaving the footprint unplaced never edits the Memory or
creates a second record.

### Leave it unplaced

I can close the Slate and leave the Memory unplaced; it remains intact and
findable.

## Context

Memory owns identity. Placement is an independent spatial occurrence and a
general app function owned by the Grid journey. The Slate and the Grid are
separate surfaces; starting Placement from the Slate hands off explicitly to
the Grid Placement journey rather than navigating by default.

## Decisions

- Placement from the Slate is an explicit action with a destination.
- Placing never mutates Memory identity or current revision.
- Unplaced Memory remains durable and findable.

## Non-goals

- Automatic placement on open.
- Command-search access for this journey.
- Placement behavior that restyles or truncates the Memory.

## Evidence

- Discovery: [[Spatial retrieval and Memory recall]]
- Wireframe: [[../../ux/wireframes/Place a Memory]]
- Reference: [[../../reference/Content and Memory model]]
- Engineering Task: [[../../engineering/tasks/T-P01 Explicit Memory placement handoff]]
- Evidence: [[../../engineering/evidence/T-P01 Explicit Memory placement handoff]]
