---
type: canonical-domain-concept
status: accepted
authority: source-of-truth
concept: Memory Slate
date: 2026-08-12
---

# Memory Slate

## Definition

Memory Slate is the masonry-style gallery of Memory records on the HUD Plane.
It is a full-viewport or half-viewport sub-application, never a floating card
or a Grid placement.

Its resting state is an edge-to-edge river of Memory content. It has no visible
title, header, footer, permanent search field, filter row, outer padding, or
application-authored introduction.

## Source and identity

The gallery enumerates the Memory ledger directly. It renders one card per
Memory record, regardless of whether that record has zero, one, or many Content
instances. Content and Content-side Anchors are derived context on the card.

A Memory referenced by fifty Content instances still appears once. A Memory
with no Content still appears normally. The gallery must not group a list of
Content representatives and infer that list is the Memory store.

## Search and actions

Memory Slate uses Memory Search. Search returns Memory records using the
Content-side Anchor, title, payload, and spatial-recall contracts. A card may
open Writing Slate, open Gallery Slate, or offer Place. Place creates Content
with the card's `MemoryId`; it does not create a second Memory and does not
change the Memory's state.

Browsing is read-only. Editing belongs to Writing Slate and creates a Memory
Version. Any authored context supplied during placement creates an Anchor on
the new Content.

Memory Search is invoked only from inside Memory Slate. It is not a Grid
overlay and cannot be opened, seeded, or scoped by a Grid cell, selection,
camera location, or Grid Layer. A Grid-facing lookup searches Content instead.

The resting Memory Slate does not display a query field. Any invoked lookup
state is a separate interaction state and may not be approximated by permanent
search chrome.

## Boundary decisions

| Tension | Decision |
| --- | --- |
| Memory Slate vs Memory | Slate presents records; it does not own or mutate the ledger by browsing. |
| Memory Slate vs Content | Content is derived context, never the gallery's identity source. |
| Memory Slate vs Anchor | Anchors explain a card through Content; they are not Memory fields. |
| Memory Slate vs Grid | Place is an explicit action that creates Content on the Grid. |
| Memory lookup vs Grid lookup | Memory lookup is exclusive to Memory Slate; Grid lookup returns Content. |
| Memory Slate vs Writing Slate | Gallery browsing and rich authoring are separate responsibilities. |
