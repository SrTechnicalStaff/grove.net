---
type: product-outcome
status: discovery
date: 2026-08-07
owner: "Grove Product"
roadmap: "[[../../roadmaps/Annotation roadmap]]"
---

# Read brochure annotations

## Purpose

Define the complete one-page reading for a light Content cluster.

## Background

Brochure is the smallest Annotation publication form. It is useful when a
field or cluster contains a small amount of related Notes, Documents, Images,
or GIFs. It should feel intentional, not like an under-filled magazine.

## Outcome

> I can read a small mixed set on one calm page, so a light cluster is useful without a heavy publication layout.

## Behavior scenarios

### Open a light reading

The brochure opens locally beside the source and presents the complete small
set in one page.

### Keep source boundaries

Each item retains its form, source label, Layer/footer context, and deliberate
route back to the source.

### Grow beyond one page

If the set no longer fits, Grove promotes the reading to pamphlet or magazine
before any clipping occurs.

## Context

Brochure is an Annotation presentation state. Content remains the current
Memory payload and Placement remains Grid-owned. The local surface follows the
Information Plane contract in `js/ui/surface-controller.js`.

## Decisions

- One complete page is the default brochure state.
- No content is truncated to preserve the brochure shape.
- A footer identifies source Layer or placement context without making the
  footer a second relationship model.

## Non-goals

- A brochure template editor.
- Fixed screen publication detached from source locality.
- Audio, video, PDF, or EPUB.

## Evidence

- Wireframe: [[../../ux/wireframes/annotations/mixed-media/Annotation brochure]]
- Discovery: [[../../discovery/Annotation template thresholds and media forms]]
- Reference: [[../../reference/Information Plane and Annotation model]]
- Task: [[../../engineering/tasks/T-AN04 Brochure page form]]
- Evidence: [[../../engineering/evidence/T-AN04 Brochure page form]]
- Browser journey: `qa/scenarios/annotation-brochure.json`
