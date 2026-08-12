---
type: product-outcome
status: discovery
date: 2026-08-07
owner: "Grove Product"
roadmap: "[[../../roadmaps/Annotation roadmap]]"
---

# Read text-led annotations

## Purpose

Define the future text-led Annotation vertical for source sets whose readable
demand is primarily writing rather than imagery.

## Background

The implemented [Now] Annotation vertical is mixed-media: brochure, pamphlet,
and magazine are its three forms. Text-heavy material should not inherit those
image-bearing slots by default. The accepted text-led grammar has three
landscape forms: Tier 01 Bulletin, Tier 02 Berliner/compact, and Tier 03
Broadsheet with threaded newspaper columns and additional pages.

## Outcome

> I can read a mostly text-based Source set in a layout that keeps the writing comfortable and complete, so long passages do not get squeezed into image-shaped spaces.

## Behavior scenarios

### Light text demand

When a qualified source set is text-led and `1.0 ≤ D < 1.75`, Grove uses a
landscape Bulletin with a comfortable measure and no forced image slot.

### Moderate text demand

When `1.75 ≤ D < 3.0`, Grove uses a landscape Berliner/compact spread with
stable newspaper columns and a clear continuation path.

### Dense text demand

When `D ≥ 3.0`, Grove uses a landscape Broadsheet with threaded multi-column
text and additional pages. It keeps type readable instead of shrinking the
whole source to fit.

### Source changes

When text is edited, a source is added or removed, or the usable viewport
changes, the reading recalculates its form and page count while preserving
source order, identity, and the person's current source when possible.

## Context

The accepted calculation and form map live in [[../../decisions/Annotation vertical tiers and landscape forms]] and [[../../discovery/Annotation demand and vertical routing]]. The text-led contract lives in [[../../discovery/Annotation text-led vertical]] and [[../../reference/Content concentration and layout model]]. The current codebase has no text-led resolver or publication engine. Memory identity, Content payload, and Placement remain owned by their existing systems.

## Decisions

- Text-led routing is [Next] discovery, not current implementation.
- The accepted band is `image demand share ≤ 0.25` with no other supported
  media demand; implementation still requires fixture verification.
- Tier 01 is Bulletin, Tier 02 is Berliner/compact, and Tier 03 is Broadsheet.
  Column counts and narrow-width behavior still require fixture review.
- Pagination is the overflow contract. The reading may add pages but may not
  truncate, summarize, or force an image placeholder.

## Non-goals

- Changing Memory identity, revisions, or Content payload.
- Renaming or replacing the mixed-media brochure, pamphlet, or magazine forms.
- Audio, video, editing, export, or a user-facing ratio/template editor.
- Making the Grid or Camera responsible for text-led presentation.

## Evidence

- Wireframe: [[../../ux/wireframes/annotations/text-led/Annotation text vertical]]
- Decision: [[../../decisions/Annotation vertical tiers and landscape forms]]
- Text-led discovery: [[../../discovery/Annotation text-led vertical]]
- Calculation: [[../../discovery/Annotation demand and vertical routing]]
- Reference: [[../../reference/Content concentration and layout model]]
- Decision: [[../../decisions/Content concentration and layout rules]]
