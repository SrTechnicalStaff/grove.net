---
type: product-outcome
status: implemented-verified
date: 2026-08-08
owner: "Grove Product"
roadmap: "[[../../roadmaps/Grid Plane roadmap]]"
source_notes:
  - "[[../../raw/original-notes/Grove - controlling content]]"
---

# Import and place Images

## Purpose

Define how an Image becomes durable Content and occupies Grid space without
changing the source asset.

## Background

An Image drop crosses Content, Memory, and Grid. The source bytes remain
canonical while a deterministic footprint resolver turns pixel dimensions into
integral Grid cells.

## Outcome

> I can drop an Image onto the Grid and see its complete frame occupy a resolved footprint without shrinking or cropping it.

## Behavior scenarios

### Drop an Image

I drop a supported Image on an open cell and see the complete requested frame
before commit.

### Resolve the footprint

Grove derives an integral footprint from the Image dimensions and displays the
result without resampling the source.

### Refuse an invalid result

Collision, decode, or unsupported-format failure leaves no partial Memory,
Asset, or Placement behind.

### Reopen the Image

The source bytes, dimensions, descriptive metadata, and Placement remain
recoverable after reload.

## Context

Image owns Asset and descriptive metadata. Memory owns identity. Grid validates
cell occupancy and Placement geometry.

## Decisions

- Imported Images are snapshots owned by Grove.
- The complete frame is the default visual result.
- Footprint resolution is deterministic and tested.

## Non-goals

- Cropping, destructive editing, or silent recompression.
- Video, RAW, animated media, or SVG security policy.
- Hardcoded universal placement sizes.

## Evidence

- Discovery: [[Image drop and footprint]]
- Reference: [[../../reference/Content and Memory model]]
- Decision: [[../../decisions/Image frame and footprint fidelity]]
- Wireframe: [[../../ux/wireframes/Import and place Images]]
