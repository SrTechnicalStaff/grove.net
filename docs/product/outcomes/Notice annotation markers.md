---
type: product-outcome
status: discovery
date: 2026-08-07
owner: "Grove Product"
roadmap: "[[../../roadmaps/Annotation roadmap]]"
source_notes:
  - "[[../../raw/original-notes/Grove at a distance]]"
  - "[[../../raw/original-notes/Grove - Annotations UX]]"
---

# Notice annotation markers

## Purpose

Define the distance-to-engagement journey for field and local-cluster
annotations on the Information Plane.

## Background

At distance, Content is intentionally illegible. Aura fields and their
information density provide a bird's-eye cue, while a marker gives the person
an intentional way to inspect a field or a dense local cluster. The marker is
not a raw field or ledger inspector and does not become Grid or Camera state.

## Outcome

> I can spot when enough nearby Content has gathered to make a useful reading, so I know when to open an annotation instead of chasing isolated pieces.

## Behavior scenarios

### See an idle marker

When a qualifying field is visible at a distance, Grove shows a quiet blip or
dot near the information-dense area without covering the Grid.

### Engage a marker

When I hover near a marker, a staircase or escalator line reveals the route and
the marker identifies whether it represents the whole field or a local cluster.

### Read the compact cue

The active marker shows an Anchor label when one exists, a title when one
exists, and a short fallback only when neither exists. Images show a thumbnail
and title; text remains a text cue.

### Refuse a weak threshold

When a field or cluster does not meet the current qualification threshold,
Grove does not invent a marker or expose raw saturation details.

## Context

The existing Field kernel (`js/mechanics/field-kernel.js`) and visible field
projection (`js/mechanics/presence-field.js`) provide the spatial facts. A
future marker controller will consume those facts and open an Information
Plane surface through `js/features/information-plane.js` and
`js/ui/surface-controller.js`; it must not subscribe to Camera transforms.

## Decisions

- Field-level and local-cluster markers are distinct threshold states.
- One marker can be active at a time; marker identity includes its source
  Content set and field/cluster provenance.
- Raw cell, ledger, and kernel values remain engineering evidence, not UI.

## Non-goals

- A fixed screen-centered modal.
- Automatic semantic grouping or topic inference.
- A command palette as the primary annotation entry.
- Audio, video, PDF, or EPUB markers in the first release.

## Evidence

- Wireframe: [[../../ux/wireframes/annotations/markers/Annotation markers and thresholds]]
- Discovery: [[../../discovery/Annotation markers and publication]]
- Reference: [[../../reference/Field and relationship model]]
- Code: `js/mechanics/field-kernel.js`, `js/mechanics/presence-field.js`, `js/ui/surface-controller.js`
- Task: [[../../engineering/tasks/T-AN01 Annotation concentration ledger and markers]]
- Evidence: [[../../engineering/evidence/T-AN01 Annotation concentration ledger and markers]]
