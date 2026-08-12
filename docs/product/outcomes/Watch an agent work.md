---
type: product-outcome
status: discovery
date: 2026-08-08
owner: "Grove Product"
roadmap: "[[../../roadmaps/Agent roadmap]]"
---

# Watch an agent work

## Purpose

Make oversight spatial: observing an agent means going to where it works and
watching changes land, with the full session record available when depth is
wanted.

## Background

Grove separates three kinds of observation: presence (seeing where an agent
is focused, owned by [[Recognize agent presence]]), watching (standing on
its Layer as its moves land live), and the audit record (the complete
machine exchange behind those moves, readable but never the primary
interface). Watching must feel like watching a colleague on the shared
field, not tailing output.

## Outcome

> I can jump to where an agent is working and watch its changes land live, with the full record there when I want it, so oversight feels like watching a colleague, not reading a log.

## Behavior scenarios

### Go to the work

When I choose an agent's location from the roster or its presence marker,
the view travels to that Layer and region, and I watch as an observer —
my selection and tools remain mine.

### Changes land visibly

When an agent commits an action while I watch, the change lands with the
same visible feedback my own actions get, attributed to that agent.

### Depth on demand

When I want the full story, I open that agent's session record and read the
complete exchange behind each action — separate from the field, never
overlaid on it.

### Watching never blocks

When I watch, work continues: mine elsewhere on the field, and that agent's
in front of me. Watching adds no pauses and asks no permission.

## Context

Travel uses ordinary Grid navigation; the record view lives with Agent
Slate. What the record shows and how actions link to it remain open in
[[../../discovery/Grove agents and grid multiplayer]].

## Decisions

- Watching is observation only; it never suspends or alters an agent's turn.
- Action feedback for agents reuses the same visible grammar as human
  actions, plus attribution.
- The session record is reachable from both roster and watched actions, and
  never renders on the Grid itself.

## Non-goals

- A picture-in-picture or split "agent screen"; the field itself is the
  view.
- Streaming raw machine exchange onto any plane.
- Replay or time-travel in this slice.

## Evidence

- Wireframe: [[../../ux/wireframes/agents/Agent presence]]
- Discovery: [[../../discovery/Grove agents and grid multiplayer]]
