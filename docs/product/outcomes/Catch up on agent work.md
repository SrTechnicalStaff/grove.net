---
type: product-outcome
status: discovery
date: 2026-08-08
owner: "Grove Product"
roadmap: "[[../../roadmaps/Agent roadmap]]"
---

# Catch up on agent work

## Purpose

Make returning effortless: everything agents did while attention was away,
walkable piece by piece on the Grid and replayable as motion, so absence
never costs orientation.

## Background

Operation history is ordered, attributed, and cell-addressed, and the
framing math already exists — so catching up can be spatial. The tour steps
through attributed operations, framing each in place; replay re-draws a
session as ghost motion using the preview grammar without touching state.
Both are read models over the same history that powers undo, per
[[../../decisions/Agent participation contracts]].

## Outcome

> I can step through everything agents did while I was away, framed piece by piece on the Grid, so returning never means wondering what changed.

## Behavior scenarios

### The tour

When I return, Agent Slate's activity view offers a tour; each step frames
one attributed change in place, with its feed line beside it, and I advance
or leave the tour at any point.

### Replay as motion

When I want the shape of a session rather than its steps, replay re-draws
it over the field as ghost motion — reads, moves, placements in order —
clearly previews, never commits.

### From tour to action

When a step shows something I want to change, I act immediately — undo that
operation, open the Content, or reply in its thread — without leaving the
tour's position.

### Nothing outside the record

When the tour ends, what I saw is exactly the operation record; there is no
second account of what happened.

## Context

Tour and replay are read-only derivations: framing uses ordinary Grid
navigation; ghost drawing uses the placement-preview grammar; entries link
to feeds, records, and threads. Cadence values are fixture-tuned under the
accepted contracts.

## Decisions

- Tour and replay derive from operation history only; they persist nothing
  and never mutate state.
- Every tour step is actionable in place (undo, open, reply).
- Replay motion is clearly ghost-grammar and reduced-motion safe.

## Non-goals

- Time-travel editing or branching history.
- Summaries that replace the record.
- Touring human operations (a later generalization if wanted).

## Evidence

- Wireframe: [[../../ux/wireframes/agents/Agent workflows]]
- Discovery: [[../../discovery/Agent action space and visibility]]
- Decision: [[../../decisions/Agent participation contracts]]
