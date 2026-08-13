---
status: accepted
authority: domain-model-integration-decision
date: 2026-08-12
area: Memory system / Content / Anchoring / Spatial recall
---

# ADR-023: Memory, Content, and Anchor ownership

The individual canonical definitions are authoritative for each concept:
[`Memory`](../../domain/Memory.md), [`Content`](../../domain/Content.md),
[`Placement`](../../domain/Placement.md), [`Anchor`](../../domain/Anchor.md),
and [`Memory Search`](../../domain/Memory-Search.md). This ADR records only
the relationship decision that connects them.

## Decision

Grove keeps the semantic record and the spatial instance separate.

- `Memory` is an immutable semantic record with identity, payload, and version
  lineage.
- `Content` is a placed instance of one Memory and stores the `MemoryId`
  foreign key.
- `Placement` is spatial state owned by Content.
- `Anchor` is an optional authored label/context relationship attached to
  Content.
- Memory does not own, contain, or mutate Content, Placement, or Anchor
  records.

The relationship direction is:

```text
Content.MemoryId -> Memory.MemoryId
Content.AnchorId -> Content-side Anchor
```

There is no placed/unplaced Memory state.

## Formation and placement

Creating a Memory commits a semantic record and does not require Content.
Quick Note is a direct Memory-creation path.

Placing an existing Memory creates Content and assigns its `MemoryId`. Placing
a new source first creates its Memory and then creates Content. Placement never
mutates the Memory into a placed state.

Deleting Content deletes only its spatial and label relationships. The Memory
remains searchable and can receive another Content instance later.

## Lookup contract

Memory search returns Memory records and performs an outward join to Content
and Content-side Anchors. Fuzzy matching priority is:

1. Anchor labels/context;
2. Memory titles and identity fields;
3. Memory payload content.

The Field Ledger adds spatial recall weight from Content and cells. Aura
overlap and saturation are stronger evidence than the distance-decaying tail
through nearby empty gaps. Spatial weight is derived ranking data and never a
Memory field.

## Consequences

- `MemoryRecord` must not expose an Anchor collection as ownership.
- Anchor persistence must be Content-side or in a separate relation store
  keyed by Content identity and must retain `MemoryId` only as a join key.
- `IMemoryLedger` stores semantic records only and must not expose Anchor
  mutation methods.
- Memory Slate must enumerate the Memory ledger directly, including records
  with no Content, and derive Content/Anchor context through indexes.
- Search cannot be implemented as a Content representative list grouped by
  Memory identity.
- Field Ledger integration must provide evidence for search ranking without
  changing Memory records.

## Rejected language

The following are non-canonical and must not appear in domain contracts:

- placed Memory;
- unplaced Memory;
- Memory placement;
- Memory owns Anchors;
- placing a Memory changes the Memory's state.

The UI may describe the user action as “Place,” but the resulting domain
mutation is “create Content referencing Memory.”
