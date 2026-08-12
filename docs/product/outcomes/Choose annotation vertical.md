---
type: product-outcome
status: discovery
date: 2026-08-07
owner: "Grove Product"
roadmap: "[[../../roadmaps/Annotation roadmap]]"
source_notes:
  - "[[../../raw/original-notes/Grove - Annotations UX]]"
---

# Choose annotation vertical

## Purpose

Define the future routing between mixed-media, text-led, and image-led reading
grammars without treating the current mixed-media forms as separate verticals.

## Background

Grove has one implemented mixed-media vertical whose forms are brochure,
pamphlet, and magazine. The accepted design contract also defines separate
text-led and image-led grammars. Their routing uses measured demand and
explicit ratios rather than the presence of one image or a character-count
shortcut.

## Outcome

> I can read text-heavy, image-heavy, or mixed Content in a layout made for that balance, so each kind of content stays comfortable and complete.

## Behavior scenarios

### Stay in the mixed vertical

When text and image demand both contribute meaningfully, the reading remains in
the mixed-media vertical and chooses brochure, pamphlet, or magazine by source
mass.

### Route text-led material

When image demand is `≤ 0.25` of total demand, the reading uses the text-led
grammar rather than forcing long writing into image-led slots. Tier 01 is a
Bulletin, Tier 02 is a Berliner/compact spread, and Tier 03 is a Broadsheet
with threaded newspaper columns and additional landscape pages.

### Route image-led material

When image demand is `≥ 0.70` of total demand, the reading uses the image-led
grammar. Tier 01 is a Gallery, Tier 02 is a Contact sheet, and Tier 03 is an
Image edition with large complete frames and supporting text.

### Preserve the source

Changing vertical changes only presentation. Memory identity, Content payload,
Placement, source order, and source routes remain unchanged.

## Context

The accepted calculation and form map live in [[../../decisions/Annotation vertical tiers and landscape forms]] and [[../../discovery/Annotation demand and vertical routing]]. The vertical-specific questions live in [[../../discovery/Annotation text-led vertical]] and [[../../discovery/Annotation image-led vertical]]. No text-led or image-led resolver exists in the current codebase.

## Decisions

- Mixed-media is the only implemented [Now] vertical; the three-vertical
  calculation and form map are accepted for [Next] implementation.
- Each renderer requires separate ratio fixtures, page geometry, and
  acceptance evidence under the accepted contract.
- Image demand uses intrinsic resolution, aspect ratio, role, and target area;
  file bytes do not route a vertical.

## Non-goals

- Renaming brochure, pamphlet, or magazine as separate verticals.
- Adding audio or video routing to the current release.
- Letting a vertical choice change Memory identity or Placement.

## Evidence

- Decision: [[../../decisions/Annotation vertical tiers and landscape forms]]
- Calculation: [[../../discovery/Annotation demand and vertical routing]]
- Text-led discovery: [[../../discovery/Annotation text-led vertical]]
- Image-led discovery: [[../../discovery/Annotation image-led vertical]]
- Reference: [[../../reference/Content concentration and layout model]]
- Decision: [[../../decisions/Content concentration and layout rules]]
