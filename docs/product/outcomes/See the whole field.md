---
type: product-outcome
status: discovery
date: 2026-08-07
owner: "Grove Product"
roadmap: "[[../../roadmaps/Grid Plane roadmap]]"
---

# See the whole field

## Purpose

Define how the Grid stays complete and honest when viewed from far away, so
distance simplifies what I see without ever subtracting from what exists.

## Background

Far zoom currently projects every placement as a full document node at tiny
scale: expensive to draw and too small to read. The missing behavior is a
simpler representation for distant Content — a pictorial stand-in that keeps
every placement present, positioned, and recognizable in kind. The
representation tiers and their thresholds are hypothesized in
[[../../discovery/Grid performance and representation at scale]], and the
scene-meaning questions remain open in
[[../../discovery/Camera zoom and distant representation]].

## Outcome

> I can pull far back and still see every placement present in a simpler form, so distance changes how much detail I see, never what exists.

## Behavior scenarios

### Everything stays present

Given a Layer with hundreds of placements, when I zoom far out, Grove shows
every placement in a simplified form at its true position and extent, so
nothing I placed disappears or moves because I chose distance.

### Simplified but recognizable

Given mixed Content across the field, when placements render in their distant
form, Grove keeps each one recognizable in kind and coloring, so I can tell
writing from imagery and mine apart before I commit to approaching.

### Approach restores detail

Given a distant view, when I zoom toward any area, Grove restores full detail
smoothly as items become readable, so moving closer feels like focusing, not
loading.

### Distance is never a lie

Given any distant view, when I return to a placement I saw from afar, Grove
shows the same item with the same identity and position the distant form
promised, so I can trust the overview as a map of the truth.

## Context

This outcome owns the simplified visual representation of distant Content.
[[Control Camera zoom]] owns the lens; scene-derived grammars for meaning at
distance remain discovery in
[[../../discovery/Camera zoom and distant representation]]; tier mechanics and
thresholds live in
[[../../discovery/Grid performance and representation at scale]].

## Decisions

- Every placement is always represented at every scale; fidelity is the only
  variable.
- A distant form occupies the same square cell footprint as the full form;
  representation never alters cell facts.
- Transitions between detailed and simplified forms must be visually stable
  during continuous zoom; flicker at a boundary scale is a defect.
- Distant forms are derived and disposable; they are never a second stored
  copy of Content.

## Non-goals

- Aggregating, clustering, or summarizing Content into new objects.
- Any change to Memory identity, Placement, or Layer facts.
- A user-facing level-of-detail control.

## Evidence

- Wireframe: [[../../ux/wireframes/grid-scale/Whole field view]]
- Discovery: [[../../discovery/Grid performance and representation at scale]]
- Discovery: [[../../discovery/Camera zoom and distant representation]]
- QA or implementation evidence: tier-transition captures once slices exist.
