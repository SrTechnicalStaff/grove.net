---
type: product-outcome
status: discovery
date: 2026-08-07
owner: "Grove Product"
roadmap: "[[../../roadmaps/Annotation roadmap]]"
---

# Keep annotation reading current

## Purpose

Define the live re-resolution behavior missing from Annotation publication.

## Background

An Annotation is a reading of the current related Content set. Adding or
removing a source, editing text, changing a revision, replacing an image, or
changing the usable screen ratio can change source mass and media balance. A
one-time form decision would leave the reading stale.

## Outcome

> I can keep reading after the Content changes because the page shape and page count adjust to the new amount of text and imagery without losing my place.

## Behavior scenarios

### Add or remove material

When a related Note, Document, Image, or GIF enters or leaves the set, Grove
recomputes concentration and promotes, demotes, or repaginates the reading when
the current threshold changes.

### Edit text

When a Note or Document gains headings, paragraphs, or other text, Grove
reflows the reading at the new rendered demand and continues text to another
page instead of shrinking or clipping it.

### Replace an image

When an image's intrinsic resolution or aspect ratio changes, Grove rechecks its
fidelity reserve and page fit while preserving the complete frame.

### Resize the surface

When the usable Information Plane surface changes from portrait to near-square
or landscape, Grove chooses the closest supported page geometry that still
meets readable text and image-fidelity floors.

### Preserve the person's place

After reflow, Grove retains source order, provenance, and the current source
identity whenever the new page sequence can support it. It never creates a new
Memory or silently summarizes the source.

## Context

This outcome requires a future concentration resolver and publication engine.
The current application owns the input boundaries but does not implement this
behavior. See [[../../reference/Content concentration and layout model]] and [[../../discovery/Annotation template thresholds and media forms]].

## Decisions

- Re-resolution runs on initial load and on source-set, revision, image, and
  usable-viewport changes.
- Pagination is the overflow contract; truncation and forced compaction are not
  acceptable fallbacks.
- Reflow is Annotation presentation state and does not mutate Memory, Content,
  Placement, Grid, or Camera.

## Non-goals

- A user-facing ratio or template control.
- Automatic summarization or semantic reordering.
- Camera movement or cross-plane subscriptions.

## Evidence

- Discovery: [[../../discovery/Annotation template thresholds and media forms]]
- Reference: [[../../reference/Content concentration and layout model]]
- Decision: [[../../decisions/Content concentration and layout rules]]
