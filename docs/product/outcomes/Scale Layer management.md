---
type: product-outcome
status: implemented-verified
date: 2026-08-08
owner: Grove Product
roadmap: "[[../../roadmaps/Grid Plane roadmap]]"
source_notes:
  - "[[../../raw/original-notes/Grove controls]]"
---

# Scale Layer management

## Purpose

Define how Layer management remains usable when the Grid contains a large
number of Layers.

## Background

The Layer manager uses a compact, scrollable stack with count context and
search so a larger stack does not require scanning every row or losing the
current position.

## Outcome

> I can find and manage the Layer I need in a large stack without scanning every Layer one by one.

## Behavior scenarios

### Find a Layer

I can narrow or jump through the Layer stack and see the current Layer in
context before I act.

### Keep my place

After creating, renaming, moving, or returning from a Grid action, the manager
returns me to the relevant Layer rather than resetting the list.

### Manage a result

Search or filtering does not hide the destination preview or make a Layer
operation ambiguous.

## Context

This is a scale extension to Layer CRUD, not a change to Layer identity,
topology, or placement behavior.

## Decisions

- Large-stack navigation remains separate from the first CRUD slice.
- The CRUD manager must leave room for a future search, filter, or jump affordance.
- Scale navigation is deferred to Later under [[../../decisions/Layer navigator scale deferral]].

## Non-goals

- Layer groups or nested hierarchy.
- Saved searches or alternate Layer taxonomies.
- Changing Grid or Camera behavior.

## Evidence

- Discovery: [[../../discovery/Layer navigator at scale]]
- Reference: [[../../reference/Grid and Placement model]]
- Decision: [[../../decisions/Layer navigator scale deferral]]
- Wireframe: [[../../ux/wireframes/Scale Layer management]]
