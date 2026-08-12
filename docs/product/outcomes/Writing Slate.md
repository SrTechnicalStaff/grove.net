---
type: product-outcome
status: discovery
date: 2026-08-08
owner: "Grove Product"
roadmap: "[[../../roadmaps/HUD Plane roadmap]]"
source_notes:
  - "[[../../raw/original-notes/Grove - Documents]]"
  - "[[../../raw/original-notes/Grove - notes]]"
---

# Writing Slate

## Purpose

Define the dedicated writing surface for Notes and Documents without requiring
the person to edit inside a card, Memory Slate, or Grid context.

## Background

Writing Slate is a focused HUD editor for text Content. It can open from a
selected placed Note or Document or from a Memory record, then edits that
Memory's current text. It is an interface over the record, not a draft store
or a second identity. Saving uses the normal Memory revision path; the source
Memory updates in place without exposing the whole Memory workflow to the
person.

## Outcome

> I can open a Note or Document in Writing Slate and edit its complete text in one focused surface, so I can write without fighting a small card or the Grid.

## Behavior scenarios

### Open a text Memory

I choose a Note or Document from its invoking surface—whether a placed Content
instance or a Memory action—and Writing Slate opens with the complete current
text and a clear return path.

### Write with focus

I can read and edit the full text with a quiet, lightweight editor. The surface
does not require add-ons, a plugin system, Memory Slate, or a Grid location.

### Save the source Memory

I save and the selected Memory receives its next current revision through the
normal persistence path. No shadow copy or alternate Memory is created.

### Cancel or close

I cancel or close and return to the invoking surface. Unsaved edits do not
replace the source revision.

## Context

Writing Slate is a dedicated HUD journey for text forms. Memory owns identity
and revision; the editor owns focused presentation and draft state. Quick Note
creates Memories, Memory Slate browses them, and Writing Slate edits selected
text without becoming another Memory workflow.

## Decisions

- Writing Slate is fixed to the HUD and independent of Grid and Camera.
- Notes and Documents share the focused writing contract; form-specific
  differences remain visible.
- Save is explicit and commits through the existing Memory transition.
- The first version is lightweight plain-text/Markdown-oriented work with no
  add-ons or plugin marketplace.

## Non-goals

- Image or GIF viewing/editing.
- Memory Slate gallery browsing or context recall.
- Placement, Grid navigation, or Layer controls.
- Audio, video, or add-on/plugin support.
- A second draft store or Memory identity.

## Evidence

- Wireframe: [[../../ux/wireframes/writing-slate/Writing Slate]]
- Discovery: [[../../discovery/Content forms and focused surfaces]]
- Reference: [[../../reference/Content and Memory model]]
