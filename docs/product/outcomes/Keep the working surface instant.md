---
type: product-outcome
status: discovery
date: 2026-08-07
owner: "Grove Product"
roadmap: "[[../../roadmaps/Grid Plane roadmap]]"
---

# Keep the working surface instant

## Purpose

Guarantee that the Content a person is directly touching responds instantly,
no matter how large the surrounding vault has become.

## Background

Interactions currently pay costs proportional to the whole vault: hover and
selection scan every placement, moving Content rebuilds the presence field
from all Layers, and committing an edit serializes the complete application
state. The person feels the whole vault in their fingertips when they should
only feel the item they are touching. The engine changes that bound these
costs are hypothesized in
[[../../discovery/Grid performance and representation at scale]].

## Outcome

> I can edit and arrange what is in front of me with immediate response no matter how much exists elsewhere, so the piece I am touching never pays for the size of the whole.

## Behavior scenarios

### Typing stays immediate

Given a vault of any size, when I edit text in a Note or Document on the Grid,
Grove keeps every keystroke immediate, so writing feels the same in a full
vault as in an empty one.

### Dragging stays attached

Given hundreds of placements on the current Layer, when I move a selection
across the field, Grove keeps the moved Content attached to my pointer with
valid-drop feedback updating live, so the drag never lags behind my hand.

### Selection answers instantly

Given a dense Layer, when I sweep a marquee or click any item, Grove resolves
the selection immediately, so choosing what to act on is never slower than
deciding.

### Commits never freeze the field

Given any edit, move, or resize, when I commit it, Grove records and saves the
change without freezing the Grid, so finishing one action never delays
starting the next.

## Context

Selection, arrangement, and editing behaviors are owned by their existing
outcomes ([[Target content]], [[Arrange content]], [[Place content]]); this
outcome owns their responsiveness at scale. Bounded query, field, and
persistence costs are hypothesized in
[[../../discovery/Grid performance and representation at scale]].

## Decisions

- Interaction cost must be priced by the interaction's own footprint, not by
  vault size; queries, field updates, and saves touch only what changed.
- Responsiveness may not be bought by suspending presence, selection
  feedback, or validity checks; the behaviors stay complete and become cheap.

## Non-goals

- Changing any selection, arrangement, or editing behavior contract.
- Deferring correctness (collision, validity, atomicity) to gain speed.
- A user-facing performance setting.

## Evidence

- Wireframe: [[../../ux/wireframes/grid-scale/Instant working surface]]
- Discovery: [[../../discovery/Grid performance and representation at scale]]
- QA or implementation evidence: interaction-latency traces against synthetic
  vaults once slices exist.
