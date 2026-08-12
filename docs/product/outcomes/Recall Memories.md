---
type: product-outcome
status: discovery
date: 2026-08-08
owner: "Grove Product"
roadmap: "[[../../roadmaps/Memory roadmap]]"
source_notes:
  - "[[../../raw/original-notes/Grove - notes]]"
  - "[[../../raw/original-notes/Grove - information layer]]"
  - "[[../../raw/original-notes/Grove - Annotations UX]]"
---

# Recall memories

## Purpose

Define the separate context-based lookup journey for finding a Memory after the
person has provided a clue, without making Content or the Grid the default
access path.

## Background

Memory Slate is the primary browse-first gallery. Recall is a separate journey
for a person who knows context associated with the Memory they want. That
context starts with authored Anchors and titles; supported payload terms may be
a secondary path by form, but they do not replace gallery browsing as the
default. Because relationship evidence comes from the Field ledger, a recall
can surface Memories related to the one the person meant.

## Outcome

> I can look up a Memory by the context behind it and see the memories around it, so I find the one I meant without knowing where it sits.

## Behavior scenarios

### Look up by authored context

I give an Anchor, title, or other context I deliberately supplied. The Memory I
meant surfaces first, and Memories near it in shared evidence come along with
it.

### Look up by title

I name a Memory by its title, derived or given, and Grove matches it before
falling through to body text.

### Use payload terms secondarily

For forms that carry readable text, the Memory's content can refine a lookup
after authored context has been considered. This is not the primary Memory
Slate entry path.

### Handle unplaced Memories

An unplaced Memory is as findable as a placed one; no Grid location is needed.

## Form differences

Text and image Memories expose different lookup models. A text Memory is
searchable through title and words; an image Memory is looked up through its
derived or given title and its Anchors. These differences are recorded here and
in the Content and Memory model rather than flattened into one mechanic.

## Context

Memory owns identity. Anchors are deliberate context holders. Recall is an
optional context path beside the browse-first Memory Slate gallery; the Field
surfaces related Memories when it has attributable evidence. This outcome
never promises a Grid; the Memory may be unplaced.

## Decisions

- Lookup resolves through authored Anchors and titles first; supported payload
  terms are secondary and form-dependent.
- Result sets include Memories surfaced by shared Field evidence.
- A lookup never forces a Grid destination.

## Non-goals

- Making recall identical to Memory Slate gallery browsing.
- Command-palette access as the first interface.
- A return-to-Grid promise from a recall action.
- Field semantics beyond attributable shared evidence until the ledger is in
  place.

## Evidence

- Discovery: [[../../discovery/Spatial retrieval and Memory recall]]
- Wireframe: [[../../ux/wireframes/Recall memories]]
- Reference: [[../../reference/Content and Memory model]]
