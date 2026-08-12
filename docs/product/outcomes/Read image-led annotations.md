---
type: product-outcome
status: discovery
date: 2026-08-07
owner: "Grove Product"
roadmap: "[[../../roadmaps/Annotation roadmap]]"
---

# Read image-led annotations

## Purpose

Define the future image-led Annotation vertical for source sets whose readable
demand is primarily complete visual frames rather than writing.

## Background

The implemented [Now] Annotation vertical is mixed-media: brochure, pamphlet,
and magazine are its three forms. Image-heavy material needs a different
promise: images keep their visual relationships and enough display area to
remain understandable. The accepted forms are Tier 01 Gallery, Tier 02 Contact
sheet, and Tier 03 Image edition; all use landscape reading units and complete
frames.

## Outcome

> I can read an image-heavy Source set with enough room to see each complete image, so the visual relationship stays clear instead of becoming a grid of tiny crops.

## Behavior scenarios

### Light image demand

When a qualified source set is image-led and `1.0 ≤ D < 1.75`, Grove uses a
landscape Gallery page with one leading complete frame. Captions and supporting
text remain secondary to the image.

### Moderate image demand

When `1.75 ≤ D < 3.0`, Grove uses a landscape Contact sheet spread with varied
aspect ratios. Each frame remains complete; a layout yields space before it
crops or compresses an image.

### Dense image demand

When `D ≥ 3.0`, Grove uses a landscape Image edition with larger frames
continuing across pages. It preserves the visual sequence instead of reducing
every source to a small card.

### Source changes

When an image is added, removed, replaced, resized, or changes aspect ratio or
role, or the usable viewport changes, the reading recalculates its form and
page count while preserving source identity and complete frames.

## Context

The accepted calculation and form map live in [[../../decisions/Annotation vertical tiers and landscape forms]] and [[../../discovery/Annotation demand and vertical routing]]. The image-led contract lives in [[../../discovery/Annotation image-led vertical]] and [[../../reference/Content concentration and layout model]]. The current codebase has no image-led resolver or publication engine. Image Viewer remains the dedicated single-image Information Plane local editor; this outcome concerns a related collection reading only.

## Decisions

- Image-led routing is [Next] discovery, not current implementation.
- The accepted band is `image demand share ≥ 0.70` with no other supported
  media demand; implementation still requires fixture verification.
- Tier 01 is Gallery, Tier 02 is Contact sheet, and Tier 03 is Image edition.
  Aspect-family and narrow-width behavior still require fixture review.
- Image demand is based on intrinsic resolution, aspect ratio, role, target
  area, and fidelity reserve; file bytes are not a visual-density shortcut.
- Complete frames are the fidelity contract. Supporting text yields space and
  pagination absorbs overflow.

## Non-goals

- Changing Memory identity, revisions, or Content payload.
- Renaming or replacing the mixed-media brochure, pamphlet, or magazine forms.
- Image editing, export, video, audio, or a user-facing ratio/template editor.
- Making Memory Slate, Image Viewer, the Grid, or Camera responsible for
  collection composition.

## Evidence

- Wireframe: [[../../ux/wireframes/annotations/image-led/Annotation image vertical]]
- Decision: [[../../decisions/Annotation vertical tiers and landscape forms]]
- Image-led discovery: [[../../discovery/Annotation image-led vertical]]
- Calculation: [[../../discovery/Annotation demand and vertical routing]]
- Reference: [[../../reference/Content concentration and layout model]]
- Decision: [[../../decisions/Content concentration and layout rules]]
