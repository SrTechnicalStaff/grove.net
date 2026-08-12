---
type: product-outcome
status: discovery
date: 2026-08-08
owner: "Grove Product"
roadmap: "[[../../roadmaps/Agent roadmap]]"
---

# Set up an agent

## Purpose

Make creating an agent a one-pass act: identity, power, effort, and conduct
chosen together, so collaborators are cheap to add and safe by default.

## Background

Agents arrive through an open protocol seam, so Grove can offer many
providers behind one setup surface. Setup composes a name, what runs it, the
model and its effort as a single choice (never two separate dials), tool
grants, standing instructions, and a trust posture. Configuration lives in
[[../../discovery/Grove agents and grid multiplayer]] until its decisions
close.

## Outcome

> I can set up a new agent in one pass — what powers it, how hard it thinks, and how it should behave — so adding a collaborator takes a minute instead of a project.

## Behavior scenarios

### One pass, one card

When I create an agent from Agent Slate, one focused surface asks for a
name, what runs it, one combined model-and-effort choice, and how it should
behave; finishing gives me a ready collaborator.

### Safe by default

When I finish setup without touching advanced choices, my new agent starts
with the cautious trust posture: it proposes changes for my acceptance
rather than committing directly.

### Change it later

When I reopen a configured agent, the same card edits in place; changes
apply to its next turn, never retroactively.

### Power is one choice

When I pick what powers an agent, the model and its effort read as one
selection, the way the discovery decided.

## Context

Setup lives in Agent Slate on the HUD. Provider processes, credentials,
and runtime belong to the desktop shell. Trust-ladder and tool-grant details
remain open in [[../../discovery/Grove agents and grid multiplayer]].

## Decisions

- Model and effort are one selection, never separate dials.
- New agents default to the propose-first trust posture.
- Setup writes configuration only; no agent acts during setup.

## Non-goals

- Building or training models; Grove hosts collaborators, not labs.
- Per-message model switching mid-conversation.
- Any setup step that requires editing files by hand.

## Evidence

- Wireframe: [[../../ux/wireframes/agents/Agent slate]]
- Discovery: [[../../discovery/Grove agents and grid multiplayer]]
