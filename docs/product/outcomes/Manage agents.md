---
type: product-outcome
status: discovery
date: 2026-08-08
owner: "Grove Product"
roadmap: "[[../../roadmaps/Agent roadmap]]"
---

# Manage agents

## Purpose

Keep every agent accountable to one surface: what exists, what each is
doing, and the controls to pause, resume, or retire any of them instantly.

## Background

Agents run as long-lived collaborators with turns of their own, so the
roster must show truthful runtime state — resting, thinking, acting, waiting
for me — and expose control that always wins. Runtime processes belong to
the desktop shell; the roster is their product face.

## Outcome

> I can see every agent I have, what each one is doing, and pause, resume, or retire any of them at any moment, so I am always in charge of who works in my field.

## Behavior scenarios

### One truthful roster

When I open Agent Slate, every agent appears with its name, what powers
it, its current state, and where it is working.

### Control that always wins

When I pause an agent, it stops after its current action commits — never
mid-commit — and stays paused until I say otherwise; retiring an agent ends
its runtime and keeps everything it made.

### Follow the work

When a roster entry names where an agent is working, one action takes me
there to watch, per [[Watch an agent work]].

### The record is always there

When I open an agent's session record, I can read the complete exchange
that produced its actions, kept for audit even after retirement.

## Context

The roster lives in Agent Slate on the HUD. Pausing and retiring govern
runtime, never Content: everything an agent made remains ordinary Memory.
Runtime states and their transitions remain open in
[[../../discovery/Grove agents and grid multiplayer]].

## Decisions

- Pause takes effect at the next commit boundary, never mid-commit.
- Retirement ends runtime and deletes nothing an agent made.
- Roster state is truthful runtime state, not a decoration.

## Non-goals

- Scheduling or cron-style automation in this slice.
- Inter-agent management hierarchies.
- Hiding or archiving an agent's produced Content on retirement.

## Evidence

- Wireframe: [[../../ux/wireframes/agents/Agent slate]]
- Discovery: [[../../discovery/Grove agents and grid multiplayer]]
