---
type: product-outcome
status: implemented-verified
date: 2026-08-07
owner: "Grove Product"
roadmap: "[[../../roadmaps/Product Roadmap]]"
---

# Trust a long session

## Purpose

Make saving and history invisible costs at any vault size, so a long working
session never pauses for persistence and never hesitates on undo.

## Background

Every committed change currently captures multiple complete copies of the
application state for history, and saving writes the whole state — history
and inline image payloads included — as one record. On an image-bearing vault
this makes saving the largest single cost in the application, felt as pauses
after gestures and commits. Entity-level persistence, operation-based
history, and media stored as referenced assets are hypothesized in
[[../../discovery/Grid performance and representation at scale]], extending
[[../../discovery/Persistence and operation history]].

## Outcome

> I can save continuously and undo instantly through a long session on a large Grid, so nothing I make is lost and nothing I did slows me down.

## Behavior scenarios

### Saving is never felt

Given a large vault with images, when I place, edit, move, or navigate, Grove
saves the change without any pause in the interaction, so persistence is
something I learn about only by never losing work.

### Undo is always instant

Given a session of hundreds of operations, when I undo or redo repeatedly,
Grove applies each step immediately regardless of vault size, so stepping
back through my work is as light as stepping forward.

### Recovery is complete

Given an interrupted session, when I reopen the application, Grove restores
my work, my viewpoint, and my recent history, so an interruption costs me
nothing I had committed.

### Images do not tax the session

Given a vault holding many images, when I work through a long session, Grove
stores each image once and saves changes without rewriting image data, so
visual richness never becomes persistence weight.

## Context

Atomicity and recovery boundaries are decided in
[[../../decisions/Persistence and operation history atomicity]] and remain
binding. The persistence engine hypotheses live in
[[../../discovery/Grid performance and representation at scale]] and
[[../../discovery/Persistence and operation history]]. This outcome is
cross-surface: every plane commits through the same persistence and history
system.

## Decisions

- Save cost is priced by the change, not the vault; history cost is priced by
  the operation, not the state.
- Media payloads are stored once as referenced assets; operations and
  snapshots reference, never embed, image data.
- Atomic commit and recovery guarantees are preserved exactly while the
  storage granularity changes.

## Non-goals

- Changing undo semantics, history depth promises, or atomicity boundaries.
- Cloud sync, multi-device, or desktop-shell storage; this outcome must hold
  in the browser.
- A user-facing save control; saving remains continuous and automatic.

## Evidence

- Wireframe: [[../../ux/wireframes/grid-scale/Session trust]]
- Discovery: [[../../discovery/Grid performance and representation at scale]]
- Discovery: [[../../discovery/Persistence and operation history]]
- Engineering task: [[../../engineering/tasks/T-PS01 Entity persistence and
  operation history]]
- Evidence: [[../../engineering/evidence/T-PS01 Entity persistence and
  operation history]]
- QA: [[../../engineering/QA evidence]] via
  `qa/scenarios/session-trust.json`.
