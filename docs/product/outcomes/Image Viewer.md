---
type: product-outcome
status: discovery
date: 2026-08-08
owner: "Grove Product"
roadmap: "[[../../roadmaps/Feature roadmap]]"
source_notes:
  - "[[../../raw/original-notes/Grove - information layer]]"
  - "[[../../raw/original-notes/Grove - notes]]"
---

# Image Viewer

## Purpose

Define the dedicated single-image viewing surface for regular Images and
animated GIFs without making Memory Slate or the Grid the viewer.

## Background

An Image is a complete visual payload. Image Viewer presents one selected
Image or GIF as an Information Plane local editor: a quiet surface beside its
source with the control parity of a desktop photos app — fit, zoom, pan, and
play/pause as appropriate. It can open from selected placed Content, a Memory
action, a Gallery Slate selection, or the context menu. It is not an image
gallery, a pixel editor, or a second record. Collection browsing belongs to
[[Browse Gallery Slate]].

## Outcome

> I can open an Image or GIF in a dedicated viewer and inspect its complete frame, so I can understand it without leaving the record or entering the Grid.

## Behavior scenarios

### Open one Image

I choose an Image from its invoking surface and Image Viewer opens the complete
frame with a clear Back and Close path.

### Inspect the frame

I can fit the frame or inspect it at a useful scale without cropping or
recompressing the source asset.

### Play a GIF

An animated GIF keeps its animation and exposes a simple play/pause control;
the viewer does not expand this into video support.

### Close and return

I close the viewer and return to the invoking surface. Memory Slate and Grid do
not open or change as a side effect.

## Context

Image Viewer is a dedicated local journey for one visual Memory. Memory owns
the Image identity and asset; the viewer owns presentation state. Memory
Slate is a gallery of records and Gallery Slate is the image collection; the
viewer is neither. The two-family surface taxonomy is decided in
[[../../decisions/Slates and local editors]]; the earlier HUD implementation
is superseded by the local Information Plane implementation and remains
historical migration context.

## Decisions

- Regular Images and animated GIFs share the Image Viewer contract.
- The viewer is a local editor: Information Plane, content-attached,
  camera-independent, dismissed back to the invoking focus, per
  [[../../decisions/Slates and local editors]].
- Controls carry desktop-photos-app parity — fit, zoom, pan, play/pause —
  and nothing that edits pixels.
- The first slice is inspect-only; editing is a later Image outcome.
- Complete frame fidelity is preserved.

## Non-goals

- Image Gallery or collection browsing.
- Image editing, annotation, or export.
- Memory Slate retrieval or context recall.
- Placement, Grid navigation, or Layer controls.
- Audio or video support.

## Evidence

- Wireframe: [[../../ux/wireframes/image-viewer/Image Viewer]]
- Discovery: [[../../discovery/Content forms and focused surfaces]]
- Reference: [[../../reference/Content and Memory model]]
