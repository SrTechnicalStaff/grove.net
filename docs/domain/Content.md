---
type: canonical-domain-concept
status: accepted
authority: source-of-truth
concept: Content
date: 2026-08-12
---

# Content

## Definition

Content is a placed spatial instance of exactly one Memory. It is the Grid
object a person can see, select, move, resize, trace, delete, and place near
other Content.

Content is not a second Memory and is not a state that Memory enters. Multiple
Content instances may reference the same Memory.

## What Content contains

Content owns the facts needed for spatial work:

- a stable `ContentId`;
- `MemoryId`, the foreign key to its Memory;
- grid origin and footprint;
- Grid Layer identity;
- content kind and render properties;
- optional Content-side `AnchorId` and label metadata.

Content always has spatial state. A Content record without a valid Memory is
invalid.

## Creation

Content is created by a placement operation:

1. resolve an existing Memory, or commit a new Memory for an unbound source;
2. validate the destination cells and Grid Layer;
3. create Content with the resolved `MemoryId`;
4. apply Placement facts;
5. optionally create a Content-side Anchor when authored context is supplied.

The Memory record remains unchanged in both existing-Memory and new-source
placement.

## Operations

### Move and resize

Moving or resizing Content updates its Placement and spatial index. Its
`MemoryId` remains unchanged. Its Aura is recalculated because its spatial
source geometry changed.

### Trace or reuse

Tracing creates another Content instance that references the selected Memory.
The new Content may use a different Grid Layer, footprint, or Anchor. No
semantic payload is duplicated merely because the spatial representation is
duplicated.

### Edit

Editing Content commits a new immutable Memory version and updates that
Content's `MemoryId` to the new version. This is the one Content operation that
changes its semantic reference; it does not mutate the prior Memory record.

### Delete

Deleting Content removes the Content, its Placement, and its Content-side
Anchor. The referenced Memory remains available.

## Field participation

Content is the sole spatial source for Aura and Field Ledger contributions.
The Field Ledger reads Content geometry and semantic references to produce
cell evidence. It may retain lightweight source identifiers for lookup, but it
does not turn Content into Memory state.

## Search participation

Content contributes search context through its Anchor labels and spatial
relationships. A search result remains a Memory result. Content is evidence
used to find or rank that result, not the returned semantic record itself.

## Boundary decisions

| Tension | Decision |
| --- | --- |
| Content vs Memory | Content is spatial; Memory is semantic. |
| Content vs Placement | Content owns Placement facts. |
| Content vs Anchor | Anchor is optional metadata attached to Content. |
| Content vs Field Ledger | Content emits spatial evidence; the ledger does not own Content. |
| Content vs Memory Slate | Slate enumerates Memories and derives Content context; it does not use Content representatives as its data source. |
