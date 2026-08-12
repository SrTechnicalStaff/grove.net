---
type: product-outcome
status: implemented-verified
date: 2026-08-07
owner: "Grove Product"
roadmap: "[[../../roadmaps/Grid Plane roadmap]]"
source_notes:
  - "[[../../raw/original-notes/Grove - information layer]]"
---

# Anchor content

## Purpose

Define how a person gives Content their own contextual meaning without turning
that context into a mechanical tag system.

## Background

Content can be placed, moved, and revisited without carrying the person's own
reason for caring about it. An Anchor lets that context be written once and
shared deliberately.

## Outcome

> I can add my own context to Content so I can distinguish it by what it means to me instead of forcing it into a system category.

## Behavior scenarios

### Add context

I select Content and open the Anchor editor. The editor stays beside the Content
so the Content remains visible while I write multiline context.

### Reuse context

I can choose an existing Anchor and assign it to the selected Content without
creating a duplicate identity.

### Edit shared context

Editing an Anchor updates the shared context for its explicit assignments.

### Remove context

Unassigning removes the Anchor from one Content item. Deleting the Anchor removes
the Anchor from every assignment as one distinct operation.

### Cancel or recover

Escape cancels an unfinished draft. Committed Anchor changes survive reload and
Undo/Redo.

## Context

Anchor is contextual identity attached to Memory-backed Content or to one of
its Placements. It is not a Grid address, a traditional label, or an automatic
semantic relationship. A traced Placement may therefore carry a different
Anchor while the underlying Content identity remains shared.

## Decisions

- Context is multiline and the primary authored field.
- Existing Anchors are selected explicitly.
- Anchor hue is static Indigo and is not a form choice.
- The Information Plane presents the editor locally without owning Grid or
  Camera state.

## Non-goals

- Spatial retrieval UI.
- Annotation markers or readers.
- Automatic semantic relationships.

## Evidence

- Product boundary: [[../../decisions/Information Plane locality]]
- Stable terms: [[../../reference/Lexicon]]
- Discovery: [[Annotation markers and publication]]
- Browser journey: `qa/scenarios/anchor-content.json`
- Implementation state: [[../../engineering/Implementation status]]
