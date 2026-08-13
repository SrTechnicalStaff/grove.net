---
type: canonical-domain-concept
status: accepted
authority: source-of-truth
concept: Trace
date: 2026-08-12
---

# Trace

## Definition

Trace is the user operation that reuses a Memory in another spatial context.
It creates another Content instance with the same `MemoryId`, usually while
preserving the selected cell alignment during Grid-Layer navigation.

Trace is not a Memory Version and not a copy of the semantic payload. It is a
Content-creation operation. The new Content may receive its own Placement and
its own Content-side Anchor context.

## Operation

1. select existing Content or a Memory result;
2. carry the source Memory identity and intended footprint;
3. choose a destination Grid Layer and cells;
4. validate occupancy and collision rules;
5. commit new Content with the same `MemoryId`;
6. optionally author an Anchor for the new Content.

If the destination is refused, no Content is created and no Memory changes.
If the person edits the traced Content later, that edit creates a new Memory
Version for that Content according to the version contract.

## Relationship to copy and duplicate

Trace reuses a Memory identity. Copy or duplicate may create a new semantic
Memory when the product action explicitly requests independent content. The
distinction is semantic identity, not the number of visible Grid objects.

## Boundary decisions

| Tension | Decision |
| --- | --- |
| Trace vs Memory | Trace never changes the Memory record. |
| Trace vs Content | Trace creates Content. |
| Trace vs Memory Version | Spatial reuse is not semantic editing. |
| Trace vs Anchor | A new Content may receive its own authored context. |
| Trace vs Placement | Trace commits a new Placement after destination validation. |
