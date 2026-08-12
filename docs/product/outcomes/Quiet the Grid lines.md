---
type: product-outcome
status: discovery
date: 2026-08-07
owner: "Grove Product"
roadmap: "[[../../roadmaps/Grid Plane roadmap]]"
---

# Quiet the Grid lines

## Purpose

Give the person direct control over Grid line visibility, so the field can be
visually calm on demand while the Grid itself never stops working.

## Background

Grid lines are currently always drawn. Minor lines fade as their spacing
collapses, but major lines render at every scale, which at far zoom produces
a dense line field that is hard on the eyes and costly to draw. Line
visibility is pure presentation: cells, snapping, placement, selection, and
every other Grid fact exist whether or not the lines are shown. The line
hierarchy and its drawing cost are examined in
[[../../discovery/Grid performance and representation at scale]].

## Outcome

> I can show or hide the Grid lines whenever I choose while every Grid behavior keeps working, so the field can be visually calm without ever being less of a Grid.

## Behavior scenarios

### Hide the lines

Given any view of the Grid, when I turn Grid lines off, Grove removes the
line rendering while cells, snapping, placement, and selection behave
identically, so the surface calms down without the Grid losing any ability.

### Show the lines

Given hidden lines, when I turn Grid lines on, Grove restores the same line
hierarchy at the current scale immediately, so the structure returns the
moment I want it.

### The choice persists

Given a visibility choice, when I reload the application, Grove restores the
Grid lines to the state I chose, so calm is a setting, not a ritual.

### Legibility at distance

Given visible lines at far zoom, when line spacing collapses below
legibility, Grove fades the crowded line levels rather than drawing a dense
field of them, so distance never turns the Grid into visual noise even with
lines on.

## Context

Line rendering is owned by the Grid Plane's visual hierarchy; the toggle is a
presentation control and writes no product fact. Placement, snapping, and
addressing are owned by their existing outcomes and remain unconditional.
Drawing-cost and fade hypotheses live in
[[../../discovery/Grid performance and representation at scale]].

## Decisions

- The Grid is always functionally on; visibility governs rendering only.
- Line visibility is view preference, persisted with view state, not a
  product fact on any Layer or Placement.
- The fade of crowded line levels applies whenever lines are visible; the
  toggle and the fade are one hierarchy, not two systems.
- [[../../decisions/Grid line visibility control]] accepts the Grid Plane `G`
  binding `grid-toggle-lines` as the view-only visibility control.

## Non-goals

- Disabling snapping, cell addressing, or any Grid behavior with the lines.
- Per-Layer or per-area line visibility.
- Styling controls beyond visibility.

## Evidence

- Wireframe: [[../../ux/wireframes/grid-scale/Grid line visibility]]
- Discovery: [[../../discovery/Grid performance and representation at scale]]
- QA or implementation evidence: toggle journey with identical placement
  behavior in both states: [[../../engineering/evidence/T-G12 Grid line
  hierarchy and visibility]].
