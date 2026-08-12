---
type: product-outcome
status: discovery
date: 2026-08-08
owner: "Grove Product"
roadmap: "[[../../roadmaps/HUD Plane roadmap]]"
source_notes:
  - "[[../../raw/original-notes/Grove - notes]]"
  - "[[../../raw/original-notes/Grove - information layer]]"
---

# Browse Memory Slate

## Purpose

Define the Memory Slate as a browse-first gallery and make its entry journey
usable without a Grid, a Placement, or a Content viewer being open first.

## Background

Memories are durable records independent of where they are shown or whether
they have a Placement. Memory Slate is the curated access surface for those
records: a person opens it from a keybind, call to action, or an explicit
placed-Content handoff, browses a gallery of Images, GIFs, Notes, and
Documents, and chooses what to revisit. The Slate is not a Grid view, a
content search box, or the owner of form-specific Content display.

## Outcome

> I can open Memory Slate and browse all the Memories I have created in a gallery, so I can choose one to revisit without searching through other surfaces.

## Behavior scenarios

### Open from a keybind or call to action

From any active surface, I use the Memory Slate keybind or its visible call to
action and the gallery opens without requiring a Grid or changing the surface
behind it.

### Browse every Memory

The gallery includes Memories whether or not they have a Placement. Cards show
the form and useful quiet metadata without turning spatial state into the
gallery's organizing principle.

### Narrow the gallery

I can use approved facets, saved filters, or form chips to narrow the gallery;
the primary journey remains browsing rather than searching the Content payload.

### Choose a Memory to revisit

I select a card and choose an explicit detail action to open its Memory detail
in the Slate. View content and Place later are explicit actions, not automatic
transitions from selection.

### Open from a placed source

I can invoke Memory Slate from selected placed Content and it resolves the
same canonical Memory record without making the Placement the gallery's
organizing principle.

### Close without a hidden destination

I close the Slate and return to the surface that invoked it. The gallery does
not open a Content viewer or navigate to the Grid as a side effect.

## Context

Memory Slate is a fixed HUD gallery over the canonical Memory records. It can
appear above any active surface, but it is not defined by the surface beneath
it. Memory owns the record; form-specific viewers own how an Image, Note, or
Document is displayed; Placement owns spatial occurrence.

## Decisions

- Keybind, call-to-action, and explicit placed-Content handoffs are first-class
  routes.
- Gallery browsing is primary; Content-payload lookup is a separate Recall
  Memories journey.
- Placed and unplaced status is visible metadata, not a gallery hierarchy.
- Selecting a Memory does not open a viewer or start Placement automatically.

## Non-goals

- Grid navigation, cell coordinates, Layers, Camera state, or Placement
  previews.
- Content-payload lookup as the primary way to enter or browse the Slate.
- Image viewing, text focus, Document editing, or other form-specific viewers.
- Audio and video Memories in this version.
- A second Memory or Content store.

## Evidence

- Wireframe: [[../../ux/wireframes/memory-slate/Memory Slate]]
- Discovery: [[../../discovery/Memory Slate gallery and entry]]
- Prototype reference: `baseline/raw/Memory-slate/prototype-23-memory-slate.md`
