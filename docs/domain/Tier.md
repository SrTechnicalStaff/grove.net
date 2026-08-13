---
type: canonical-domain-concept
status: accepted
authority: source-of-truth
concept: Tier
date: 2026-08-12
---

# Tier

## Definition

A Tier is a named grouping within a Plane or a representation level used to
organize what is visible at a given scale. It is a presentation concept, not a
semantic or spatial ownership relation.

Examples include the HUD, Information, and Grid tiers when a product surface
needs to describe composition order, and recursive Grid representation tiers
when the camera changes scale.

## Rules

- a Tier may contain projections of many domain objects;
- changing Tiers does not create, delete, move, or version a Memory;
- a Tier must not be used as a synonym for Grid Layer;
- a Tier's visibility or LOD policy must not become persisted semantic state.

## Boundary decisions

| Tension | Decision |
| --- | --- |
| Tier vs Grid Layer | Tier is presentation; Grid Layer is spatial depth. |
| Tier vs Plane | A Plane hosts or orders Tiers; a Tier does not replace a Plane. |
| Tier vs Content | Content may be represented in a Tier but is not owned by it. |
