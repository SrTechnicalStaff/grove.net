---
type: product-outcome
status: implemented-verified
date: 2026-08-07
owner: "Grove Product"
roadmap: "[[../../roadmaps/Grid Plane roadmap]]"
source_notes:
  - "[[../../raw/original-notes/Grove controls]]"
  - "[[../../raw/original-notes/Grove - Layers]]"
---

# Manage Layers

## Purpose

Define the complete CRUD journey for semantic Grid Layers.

## Background

People need to add depth around the Grid, insert a Layer between existing
Layers, reorder Layers, rename them, and remove them without changing the
Content already placed on the remaining Layers.

## Outcome

> I can create, insert, move, rename, and remove Grid Layers, allowing me to organize content as I see fit.

## Behavior scenarios

### Create an outer Layer

I use the approved edge-creation command to add a Layer above or below the
existing stack. The new Layer receives a stable identity and the current-Layer
result is explicit.

### Insert a Layer

I use the approved insertion command to add a Layer immediately above or below
the current Layer without changing the Content already placed on the Grid.

### Move a Layer

I reorder a Layer and its Content membership remains intact. The origin Layer
remains the non-deletable boundary of the stack.

### Rename a Layer

I rename a Layer without changing its stable identity or ordered position.

### Remove a Layer

I remove an empty Layer. Removing a non-empty Layer requires an explicit
Content-handling choice; it never silently deletes or relocates placed Content.

### Recover

Undo, Redo, and reload restore Layer identity, order, names, current Layer, and
Content membership.

## Context

Layers are semantic depth within one Grid, not separate boards or Camera worlds.
The original control notes define bracket navigation, Shift edge creation, and
Ctrl insertion. The current implementation is verified by the Layer Manager,
the canonical [[../../reference/Keybind map]], and the browser Layer journey.

## Decisions

- The origin Layer cannot be deleted or moved across the stack boundary.
- Creation, insertion, movement, renaming, and removal are distinct operations.
- Non-empty removal must name the explicit Content handling choice before
  implementation.

## Non-goals

- Layer folders, nesting, visibility permissions, or separate Camera views.
- Copy, Cut, Duplicate, Paste, or Trace.
- Content editing, placement, and retrieval.

## Evidence

- Discovery: [[Layer CRUD]]
- Wireframe: [[../../ux/wireframes/Manage Layers]]
- Current partial browser slice: `qa/scenarios/operate-layers.json`
- Implementation state: [[../../engineering/Implementation status]]
