---
type: product-outcome
status: discovery
date: 2026-08-08
owner: "Grove Product"
roadmap: "[[../../roadmaps/Agent roadmap]]"
---

# Hear from agents

## Purpose

Keep the person informed of agent progress through quiet, well-timed
signals — finished, stalled, needs a decision — without surveillance or
noise.

## Background

Agents work while attention is elsewhere, so Grove needs a notification
grammar that respects its calm surfaces: no toasts raining over the field,
no modal interruptions. Signals concentrate in Agent Slate's attention
states and in each agent's presence mark; a decision request is a waiting
state that holds until answered, never a popup that steals focus.

## Outcome

> I can get a quiet signal when an agent finishes, stalls, or needs my decision, so I stay informed without watching over anyone's shoulder.

## Behavior scenarios

### Three signals, one grammar

When an agent finishes its task, stalls on a refusal, or needs my call, its
roster entry and presence mark carry the matching attention state; nothing
else in Grove changes.

### Decisions wait for me

When an agent needs my decision, the request waits in its waiting state
with the question and choices inline; nothing proceeds until I answer, and
nothing nags while I don't.

### Catch up in one place

When I return after time away, Agent Slate's activity view lists what
happened while I was gone, newest first, each entry leading to the work or
the record.

### Quiet is the default

When agents are simply working, Grove shows presence and nothing more; a
healthy field makes no announcement noise.

## Context

Attention states live on roster entries and presence marks; inline decision
requests follow the inline-confirm grammar. Desktop-level notification
(system tray, badges) belongs to shell discovery and is out of scope here.
Signal taxonomy remains open in
[[../../discovery/Grove agents and grid multiplayer]].

## Decisions

- No toasts, banners, or focus-stealing popups; attention states and inline
  requests are the whole grammar.
- A decision request blocks only its own agent's turn, never the person or
  other agents.
- The activity view is a read model over the operation record, not a second
  log.

## Non-goals

- Email, mobile push, or system notifications in this slice.
- Configurable notification rules engines.
- Unprompted agent messages outside anchored conversation.

## Evidence

- Wireframe: [[../../ux/wireframes/agents/Agent slate]]
- Discovery: [[../../discovery/Grove agents and grid multiplayer]]
