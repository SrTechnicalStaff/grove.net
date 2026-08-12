---
type: product-outcome
status: implemented-verified
date: 2026-08-08
owner: "Grove Product"
roadmap: "[[../../roadmaps/Grid Plane roadmap]]"
source_notes:
  - "[[../../raw/original-notes/Tracing]]"
---

# Trace content

## Purpose

Define how one piece of Content can appear across multiple Grid Layers while
each placement carries the context of where it was put.

## Background

Trace is a sustained placement activity. A person selects Content, moves the
trace preview between Layers while keeping its cell position, and chooses when
to drop a placement onto a Layer. The original placement remains available and
the trace session continues until explicitly ended.

## Outcome

> I can place the same Content on multiple Grid Layers and give each placement its own context without recreating the Content.

## Behavior scenarios

### Start a trace

I select one or more placed Content items and enter Trace. The selected set
remains available while I navigate between Layers.

### Preview another Layer

I move to another Layer and see the selected Content at the same Grid cells.
Collision is visible as a condition of the preview but does not prevent me from
continuing to another Layer.

### Drop off a trace

I choose Drop off on the current Layer. Placement collision rules apply at that
moment; if valid, the placement is committed and the Trace session remains
active.

### Add to the trace

While tracing, I can add Content from another Layer to the active set. The
selected set updates without ending the Trace session.

### Contextualize a placement

Each committed traced Placement can carry its own contextual Anchor or other
placement-local explanation while the underlying Content remains shared.

### End or cancel

End Trace keeps committed placements. Cancel removes only the uncommitted
preview and leaves all committed placements unchanged.

## Context

Trace differs from ordinary Layer transfer, which moves one Placement. It also
differs from Copy, Cut, Duplicate, and Paste, which operate on independent
Content results. Trace preserves one Content identity across multiple
Placements.

## Decisions

- Trace adds Placements to existing Content rather than creating a new Content
  identity.
- Grid cell position is held constant while the trace preview moves between
  Layers.
- Collision is authoritative on Drop off, not on preview navigation.
- Trace remains active after a successful Drop off until explicitly ended.

## Non-goals

- Automatic semantic interpretation of context.
- New Content identities from tracing.
- Clipboard synchronization with other applications.
- Batch Trace without an explicit active selection.

## Evidence

- Source intent: [[../../raw/original-notes/Tracing]]
- Decision boundary: [[../../decisions/Trace and layer transfer boundaries]]
- Session contract: [[../../decisions/Trace session contract]]
- Discovery: [[../../discovery/Trace across Layers]]
- Wireframe: [[../../ux/wireframes/Trace across Layers]]
