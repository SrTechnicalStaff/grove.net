---
type: canonical-domain-concept
status: accepted
authority: source-of-truth
concept: Gap
date: 2026-08-12
---

# Gap

## Definition

A Gap is a sequence of Grid cells with no overlapping Content Aura that still
lies between or near Content. It is empty of Content and is not itself a
Memory, Anchor, or Aura source.

## Why a Gap matters

Nearby Content does not need to overlap Aura in order to be spatially related.
The empty cells between them preserve a weaker sense of distance. This lets
Grove recall nearby work without pretending that non-overlapping Content forms
the same saturated cluster.

## Search role

The Field Ledger supplies a distance-decaying Gap tail to Memory Search. The
tail is weaker than direct Aura overlap and saturation, and it must decrease as
the distance through empty cells increases.

Gap evidence can improve ordering among semantic matches. It cannot create a
Memory candidate, change a Memory payload, or make an empty cell appear to
contain Content.

## Boundary decisions

| Tension | Decision |
| --- | --- |
| Gap vs cell | A Gap is a relation across cells, not a new cell type. |
| Gap vs Aura | A Gap has no overlapping Aura; it carries only distance evidence. |
| Gap vs proximity | Gap distance is weaker than direct field overlap. |
| Gap vs viewport | Gap recall is not allowed to disappear solely at a viewport edge. |
