---
type: product-outcome
status: implemented-verified
date: 2026-08-08
owner: "Grove Product"
roadmap: "[[../../roadmaps/Grid Plane roadmap]]"
source_notes:
  - "[[../../raw/original-notes/Grove - controlling content]]"
  - "[[../../raw/original-notes/Tracing]]"
---

# Control content

## Purpose

Define how a person copies, cuts, duplicates, and pastes Content with visible
consequences and recovery.

## Background

These operations change Content, Placement, or both. The person must see which
operation is armed, where the result will land, and whether the result is
independent from the source.

## Outcome

> I can copy, cut, duplicate, and paste selected Content and see where the result will land before I commit.

## Behavior scenarios

### Copy

I copy selected Content and retain the source unchanged while the copied
identity is available to paste.

### Cut

I cut selected Content, see the removal and armed paste state, and can recover
the source through Undo or cancellation.

### Duplicate

I duplicate Content and receive an independent Content result with deliberate
Placement.

### Paste

I preview the destination footprint and commit only when the result is valid.

## Context

Content identity, Placement geometry, selection, cursor preview, and history are
separate concerns. Copy and Duplicate are not aliases; Cut must not silently
delete the source before commit. Trace is the separate operation that preserves
one Content identity across multiple Layers.

## Decisions

- Every operation has an explicit armed state and visible preview.
- Invalid or cancelled operations make no durable change.
- Duplicate creates an independent Content identity.

## Non-goals

- Trace provenance.
- Automatic packing or collision rearrangement.
- Cross-application clipboard synchronization.

## Evidence

- Discovery: [[../../discovery/Content clipboard operations]]
- Decision: [[../../decisions/Content control identity boundaries]]
- Session contract: [[../../decisions/Clipboard session contract]]
- Reference: [[../../reference/Content and Memory model]]
- Wireframe: [[../../ux/wireframes/Content clipboard operations]]
