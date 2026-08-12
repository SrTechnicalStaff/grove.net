---
type: product-outcome
status: discovery
date: 2026-08-08
owner: "Grove Product"
roadmap: "[[../../roadmaps/Agent roadmap]]"
---

# Receive agent work as real content

## Purpose

Guarantee that what agents make is ordinary Grove Content: real Notes,
Documents, and Images on the Grid, with provenance, indistinguishable in
capability from what a person makes.

## Background

Agents create through the same commands a person's actions use, so their
output is canonical Memory with Placement — never chat transcripts, side
files, or a shadow store. Provenance records which agent made what. The
form set is open-ended by design: any form Grove supports later (and its
placement rules) is automatically within an agent's reach, because creation
goes through the same command layer.

## Outcome

> I can receive an agent's work as ordinary Notes, Documents, and Images placed on the Grid, so what agents make is as real and mine as what I make.

## Behavior scenarios

### Real forms, real cells

When an agent produces work, it lands as Notes, Documents, or Images with
ordinary cell footprints on the Layer it works — editable, traceable,
anchorable, movable by me like anything else.

### Provenance without clutter

When I inspect agent-made Content, its record tells me which agent made it
and in which session; on the field it looks like Content, not like a
notification.

### Proposals when I want them

When an agent's trust posture is propose-first, its work arrives as a
visible preview on the target cells that I accept or decline; accepting
commits it as ordinary Content, declining leaves no residue.

### Tomorrow's forms included

When Grove gains a new Content form, agents can produce it the day it
exists, because they create through the same commands with the same rules.

## Context

Creation, collision, and atomic commit rules are the existing command-layer
contracts. The proposal preview reuses the placement-preview grammar.
Provenance representation remains open in
[[../../discovery/Grove agents and grid multiplayer]].

## Decisions

- Agent output is canonical Memory with Placement; no shadow stores, no
  chat-only artifacts.
- Creation routes through the existing commands with the existing guards;
  new forms are automatically included.
- Provenance lives on the record; the field shows attribution through
  presence and inspection, not through badges on every card.

## Non-goals

- Agent-only content types or a separate "AI content" class.
- Automatic acceptance of proposals by timeout.
- Editing rights that differ from human-made Content.

## Evidence

- Wireframe: [[../../ux/wireframes/agents/Agent collaboration]]
- Discovery: [[../../discovery/Grove agents and grid multiplayer]]
