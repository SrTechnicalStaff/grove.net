---
type: canonical-domain-concept
status: accepted
authority: source-of-truth
concept: Memory Search
date: 2026-08-12
---

# Memory Search

## Definition

Memory Search is the product behavior that turns a person's words into an
ordered set of Memory records, using Content-side context and spatial
evidence to improve recall.

It searches Memories. It does not search a gallery of Content representatives
and it does not require a Memory to have Content.

## Query example

For a query such as “Greg's retirement,” Grove may match:

- an Anchor label containing “Greg” or “retirement”;
- a Memory title containing one of those terms;
- Memory payload text containing one of those terms;
- fuzzy variants of those terms.

The result is the Memory record. Any matching Anchor and associated Content
are explanation and context for that result.

## Matching hierarchy

The semantic matching hierarchy is:

1. Content-side Anchor labels and authored context;
2. Memory title and identity fields;
3. Memory payload content.

Matching is fuzzy within each level. A lower level can still return a Memory
when no higher-level match exists. The level is retained so the result can be
explained and consistently ordered.

The relationship traversal is:

```text
query -> Anchor -> Content -> MemoryId -> Memory
query -> Memory title
query -> Memory payload
```

The first path is an outward join from Content. It is not a Memory-owned
Anchor lookup.

## Spatial recall ranking

After semantic candidates are found, the Field Ledger adds spatial evidence.
For each candidate Memory, Grove considers the Content instances associated
with that Memory and their local relationships to other Content.

The spatial evidence has this order of strength:

1. Aura overlap between Content fields;
2. saturation and density within the overlapping field;
3. proximity through a Gap, using a weaker distance-decaying tail.

The result ordering is lexicographic: semantic match level and fuzzy match
quality establish relevance first, then spatial recall weight orders otherwise
comparable Memories. This prevents a nearby weak match from outranking a
strong direct Anchor match merely because it is close on the Grid.

Spatial recall weight is derived relationship data. It is not stored on
Memory, does not become part of the Memory payload, and does not require the
Memory to have Content.

## Zero-Content Memories

A Memory with no Content can match through title or payload search. It remains
in the result set and receives no spatial bonus. It is never classified as an
unplaced Memory because that is not a domain state.

## Search and mutation

Searching is read-only. It does not create Content, create Anchors, move the
camera, alter Aura, or mutate Memory records. Choosing to place a result is a
separate action that creates Content referencing the selected Memory.

## Boundary decisions

| Tension | Decision |
| --- | --- |
| Search vs Memory Slate | Memory Slate consumes Memory Search results; its cards are Memory records. |
| Search vs Content | Content supplies context and spatial evidence, not the returned identity. |
| Search vs Anchor | Anchor is the highest-priority contextual match and remains Content-side. |
| Search vs Field Ledger | Field Ledger supplies derived ranking evidence only. |
| Search vs placement | A search result can be placed; searching itself never places it. |
