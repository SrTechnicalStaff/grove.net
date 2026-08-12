---
type: product-outcome
status: implemented-verified
date: 2026-08-08
owner: "Grove Product"
roadmap: "[[../../roadmaps/Grid Plane roadmap]]"
source_notes:
  - "[[../../raw/original-notes/Grove - Layers]]"
  - "[[../../raw/original-notes/Grove controls]]"
  - "[[../../raw/original-notes/Tracing]]"
---

# Operate Layers

## Purpose

Define how a person moves Content between semantic Grid Layers without changing
what the Content is or where it is understood to be.

## Background

Layer CRUD establishes the stack. Transfer is separate from Trace: it changes
semantic depth for one existing Placement while preserving the placed Content,
its geometry, and recoverability.

## Outcome

> I can move Content between Grid Layers without losing its identity, position, or bearing.

## Behavior scenarios

### Transfer Content

I select Content, choose a destination Layer, see the destination before
commit, and move the Content as one reversible action.

### Preserve identity

The placed Content and its current context remain the same after transfer.

### Preserve geometry

The Content keeps its Grid position and footprint unless I deliberately change
them as part of the transfer.

### Recover

Undo, Redo, and reload restore the Layer membership and original geometry.

## Context

Layer topology owns semantic depth. Placement owns address and footprint.
Content owns the visible payload. Transfer changes Layer membership only.

## Decisions

- The protected origin Layer remains the permanent stack boundary.
- Transfer is atomic and explicit.
- Field derivation recalculates from the new Layer membership after commit.

## Non-goals

- Layer creation, insertion, rename, or removal.
- Copy, Cut, Duplicate, Paste, or Trace.
- Automatic reflow.

## Evidence

- Reference: [[../../reference/Grid and Placement model]]
- Discovery: [[../../discovery/Content transfer between Layers]]
- Decision: [[../../decisions/Grid transfer commit contract]]
- Wireframe: [[../../ux/wireframes/Content transfer between Layers]]
