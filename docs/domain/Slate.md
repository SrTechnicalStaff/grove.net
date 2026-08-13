---
type: canonical-domain-concept
status: accepted
authority: source-of-truth
concept: Slate
date: 2026-08-12
---

# Slate

## Definition

A Slate is a named sub-application surface on the HUD Plane. A Slate takes
either the full viewport or one half of it. It is not a floating card, hover
surface, footer, or spatial Grid object.

## Named Slates

- **Memory Slate** is a masonry gallery of every Memory record. It may show
  derived Content and Anchor context, but its source is the Memory ledger.
- **Writing Slate** is the long-form authoring surface for rich text and
  document-like editing. It edits semantic records through Memory Version
  creation and does not use a fake collection of containers as an editor.
- **Gallery Slate** is the focused media browsing and inspection surface.

Quick Note is a capture flow, not a Memory Slate. The Layer Manager is an
operational HUD surface, not a Slate.

## Lifecycle

Opening a Slate changes the active HUD projection and focus route. It does not
move the camera, change Grid Layer state, create Content, or mutate a Memory
by merely opening or browsing it. Any commit operation is defined by the
underlying Memory, Content, or Anchor contract.

## Boundary decisions

| Tension | Decision |
| --- | --- |
| Slate vs Overlay | Slate is a named sub-application; Overlay is transient chrome. |
| Slate vs Plane | Slate is hosted on the HUD Plane; it is not another Plane. |
| Memory Slate vs Content | Memory Slate enumerates Memories directly and derives Content context. |
| Writing Slate vs Memory | Editing commits a new Memory Version; the Slate does not mutate an old record in place. |
