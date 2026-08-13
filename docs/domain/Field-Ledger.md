---
type: canonical-domain-concept
status: accepted
authority: source-of-truth
concept: Field Ledger
date: 2026-08-12
---

# Field Ledger

## Definition

The Field Ledger is the spatial evidence system that evaluates Content against
Grid cells. It records derived per-cell field and source metadata so Grove can
render presence and reason about local spatial relationships.

The Field Ledger is a bridge between Content and recall. It is not a Memory
store, does not own Content, and does not change semantic records.

## Inputs

The ledger reads:

- Content identity and `MemoryId`;
- Content kind and authored field hue;
- Placement geometry and Grid Layer;
- active cells and nearby Content;
- Grid-Layer permeability and field thresholds.

## Outputs

The ledger may produce:

- per-cell field energy;
- source contribution metadata;
- normalized hue and presence data;
- overlap and saturation evidence;
- same-Grid-Layer contour topology;
- spatial-recall evidence for Memory Search.

These are derived values. They are not persisted as Memory payload or identity.

The color output is an energy-weighted source sum. Normalization keeps the
aggregate inside the display range; it is not a midpoint blend and does not
discard source provenance.

## Relationship to cells

Cells are the membrane through which Content becomes spatial evidence. A cell
may hold Content directly, receive Aura from Content, or be an empty Gap that
still carries a weak distance relationship between nearby Content.

## Relationship to search

Memory Search first resolves semantic candidates. The Field Ledger then
provides spatial evidence from Content associated with those candidates and
nearby Content. Direct Aura overlap and saturation are stronger evidence than
the weaker Gap tail.

The search ledger is not limited by what is currently visible in the viewport.
Visual culling and semantic recall range are separate contracts.

## Boundary decisions

| Tension | Decision |
| --- | --- |
| Ledger vs Memory | Ledger derives evidence; Memory owns semantic identity. |
| Ledger vs Content | Ledger observes Content; Content remains the source object. |
| Ledger vs cell | Cell evidence is derived and may be recomputed. |
| Ledger vs rendering | Rendering consumes ledger output but does not define search meaning. |
| Ledger vs search | Search may use ledger evidence for ranking, never for Memory mutation. |
