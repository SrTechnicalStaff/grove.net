---
type: canonical-domain-concept
status: accepted
authority: source-of-truth
concept: Spatial Recall Weight
date: 2026-08-12
---

# Spatial Recall Weight

## Definition

Spatial Recall Weight is derived evidence used to order otherwise comparable
Memory search results. It expresses how strongly the Grid's Content and cells
connect a matching Memory to the searched context.

It is not a property of Memory, Content, or Anchor. It belongs to a search
calculation or a temporary relationship between a result and the queried
spatial context.

## Evidence order

The weight is informed by these spatial signals, from strongest to weakest:

1. direct Content proximity and shared-cell relation;
2. Aura overlap and field saturation from nearby Content;
3. the distance-decaying tail through nearby Gaps where no Aura overlaps.

The Gap tail keeps nearby work discoverable without pretending that empty cells
are Content. It decays with distance and cannot outrank a materially stronger
direct relationship merely because a path crosses many empty cells.

Semantic match quality is evaluated before spatial ordering. Spatial evidence
orders comparable semantic matches; it does not turn an unrelated Memory into
a search hit.

## Stability

The value is recalculated when relevant Content, Placement, Aura, or query
state changes. It is not persisted into Memory and does not mutate search
records.

## Boundary decisions

| Tension | Decision |
| --- | --- |
| Weight vs Memory | Weight is derived ranking evidence, never Memory data. |
| Weight vs Aura | Aura is one input; the weight is the search interpretation of multiple inputs. |
| Weight vs Gap | Gap contributes a weaker tail and is not an Aura source. |
| Weight vs semantic match | Spatial evidence orders matching results; it does not replace matching. |
