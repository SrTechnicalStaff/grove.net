---
type: canonical-domain-concept
status: accepted
authority: source-of-truth
concept: Anchor
date: 2026-08-12
---

# Anchor

## Definition

An Anchor is an optional authored label or context record attached to Content.
It tells a person why a particular Content instance matters where it is. It
may name a person, event, project phase, source, or local interpretation.

An Anchor is not the Memory payload, not a spatial coordinate by itself, and
not a property owned by Memory.

## Ownership and identity

An Anchor belongs to a Content-side relationship:

```text
Anchor.ContentId -> Content.ContentId
Content.MemoryId -> Memory.MemoryId
```

The Anchor may carry `MemoryId` as a denormalized lookup key, but that key is
join data. It does not make Memory the Anchor owner.

An Anchor has its own identity and may have authored label, context, and
provenance fields. It may be absent. One Content instance may have no Anchor
or one active Anchor according to the anchoring rules of the product.

## Creation and changes

Anchoring Content creates or updates the Content-side Anchor. It does not
create a Memory and does not edit Memory payload.

Moving or resizing Content updates the Anchor's associated spatial record when
needed, but does not change the label's semantic meaning. Removing Content
removes the Anchor relationship. The Memory remains.

## Search role

Anchor labels are the first search surface because they preserve local human
context. A fuzzy query may match an Anchor label even when the query words do
not occur in the Memory title or payload.

Search follows the relationship outward:

```text
query -> Anchor.label -> Anchor.ContentId -> Content.MemoryId -> Memory
```

The returned object is the Memory. The Anchor is the explanation for why that
Memory was found in a local context.

## Boundary decisions

| Tension | Decision |
| --- | --- |
| Anchor vs Memory | Anchor is local context; Memory is durable semantic payload. |
| Anchor vs Placement | Anchor labels Content; Placement gives Content geometry. |
| Anchor vs Content | Anchor is optional Content metadata, not Content itself. |
| Anchor vs search result | Anchor explains a Memory result but is not the result. |

## Non-canonical language

Do not say that a Memory owns, carries, or has an Anchor. Say that Content is
anchored, or that an Anchor labels Content associated with a Memory.
