---
type: canonical-domain-concept
status: accepted
authority: source-of-truth
concept: Memory
date: 2026-08-12
---

# Memory

## Definition

A Memory is an immutable semantic record with a stable identity, payload, and
version lineage. It is the thing a person remembers, keeps, searches for, and
reuses across contexts.

A Memory has no spatial state. It is not placed or unplaced. It does not own
Content, Placement, or Anchor records.

## What a Memory contains

A Memory contains only semantic-record data:

- a stable `MemoryId`;
- its immutable payload and payload kind;
- content hash and integrity identity;
- version lineage (`ParentMemoryId`, root identity, and generation);
- creation and update timestamps.

The record does not contain grid coordinates, a footprint, a Grid Layer, an
Anchor collection, or a placement state.

## Relationships

Content points to Memory through a foreign-key relationship:

```text
Content.MemoryId -> Memory.MemoryId
```

The direction matters. A Memory can be found without loading Content. Content
can be queried for a Memory without changing the Memory. The absence or
presence of Content never changes what the Memory is.

Anchor labels are attached to Content. Search may join Content and Anchor back
to Memory through `MemoryId`, but no Anchor is stored inside the Memory record.

## How Memories are formed

### Direct capture

Quick Note commits a new Memory directly. This path does not require a Grid
destination and does not create Content as a side effect.

### New source placement

When a person places a new source that has no Memory, Grove commits the Memory
first, then creates Content whose `MemoryId` references the new record. The
placement action creates Content; it does not place the Memory.

### Existing Memory placement

When a person chooses an existing Memory and places it on the Grid, Grove
creates a new Content instance with the existing `MemoryId`. The Memory is not
copied, reclassified, or mutated.

### Editing

Memory payloads are immutable. When a person edits Content, Grove commits a
new Memory version whose parent is the previous Memory. That Content then
references the new version. Other Content instances continue to reference the
previous version until they are independently edited.

## Lifecycle

A Memory may exist with zero Content instances, one Content instance, or many
Content instances. These are relationship counts, not Memory states.

Removing Content removes the Content's Placement and any Content-side Anchor.
It does not delete, detach, downgrade, or otherwise alter the Memory. The
Memory remains searchable and can receive new Content later.

Moving or resizing Content changes its spatial record only. Relabelling Content
changes its Anchor only. Neither action changes Memory payload or identity.

## User outcomes

- A person can keep a thought without placing it on the Grid.
- A person can reuse one semantic record in many spatial contexts.
- A person can remove every Content instance and still find the Memory later.
- A person can edit one Content instance without silently rewriting other
  instances of the earlier Memory version.

## Boundary decisions

| Tension | Decision |
| --- | --- |
| Memory vs Content | Memory is semantic; Content is spatial. |
| Memory vs Placement | Placement belongs to Content and never becomes Memory state. |
| Memory vs Anchor | Anchor is authored Content context, not semantic payload. |
| Memory vs Aura | Content emits Aura; Memory does not emit a field by existing. |
| Memory vs search context | Search joins outward from Content/Anchor to Memory. |

## Non-canonical language

Do not describe a Memory as placed, unplaced, positioned, anchored, or located.
Describe the related Content instance instead.
