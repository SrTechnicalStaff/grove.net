---
type: product-outcome
status: discovery
date: 2026-08-08
owner: "Grove Product"
roadmap: "[[../../roadmaps/Feature roadmap]]"
---

# Browse Gallery Slate

## Purpose

Define the dedicated image gallery slate so visual material has a browse-first
home of its own, instead of hiding inside the general Memory gallery or
requiring a Grid hunt.

## Background

Every Image and GIF in Grove is a Memory backed by a durable Asset, but there
is no surface that shows the vault's visual material together. Memory Slate
browses all forms with form-appropriate cards; the Image Viewer presents one
image. The gap between them — seeing all images as images — was explicitly
deferred by the Image Viewer outcome and is now owned here. Gallery Slate is
a HUD slate per [[../../decisions/Slates and local editors]]: composable in
the Slate host beside Memory Slate or Writing Slate.

## Outcome

> I can open Gallery Slate and browse every image I have brought into Grove in one place, so I can find a visual by looking instead of remembering where I put it.

## Behavior scenarios

### Open the gallery

When I open Gallery Slate from its keybind, CTA, or the Slate host, I see
every Image and GIF as a complete-frame thumbnail wall, newest first, with
quiet identity metadata.

### Recognize by looking

When I scan the wall, frames keep their aspect ratios and animate only where
the source animates, so recognition works the way visual memory works.

### Select and act

When I select an image, the selection is visible and explicit actions become
available — open in the Image Viewer, reveal a placement, or open the Memory
record — without any action firing from selection alone.

### Compose with other slates

When I pair Gallery Slate with Memory Slate or Writing Slate in the host,
each pane keeps independent scroll, selection, and filter state.

## Context

Gallery Slate reads canonical Memories and Assets; it stores nothing of its
own. The Image Viewer remains the single-image local editor on the
Information Plane; Gallery Slate is the collection surface on the HUD. The
Slate host owns composition per [[Compose Slate composition]]. Open facets and
entry details live in [[../../discovery/Local editors and slate taxonomy]].

## Decisions

- Gallery Slate is a slate: HUD Plane, host-composable, camera-blind, per
  [[../../decisions/Slates and local editors]].
- Complete frames only: thumbnails preserve aspect ratio and are never
  cropped into uniform tiles.
- Selection and action are separate visible steps.
- The gallery is a view over Memories and Assets, never a second store.

## Non-goals

- Image editing, export, or annotation.
- Replacing Memory Slate's all-forms browsing or context recall.
- Becoming the single-image viewer; that is the Image Viewer's journey.
- Audio or video media.

## Evidence

- Wireframe: [[../../ux/wireframes/gallery-slate/Gallery Slate]]
- Discovery: [[../../discovery/Local editors and slate taxonomy]]
- Decision: [[../../decisions/Slates and local editors]]
