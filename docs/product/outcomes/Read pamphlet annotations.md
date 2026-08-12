---
type: product-outcome
status: discovery
date: 2026-08-07
owner: "Grove Product"
roadmap: "[[../../roadmaps/Annotation roadmap]]"
---

# Read pamphlet annotations

## Purpose

Define the mixed-content reading for a moderate Annotation cluster.

## Background

Pamphlet is the middle publication form. It gives a mixed set of text and
visual Content enough room to remain recognizable without forcing a dense
magazine sequence.

## Outcome

> I can read a moderate mixed set across a compact spread, so text and images stay connected instead of competing for space.

## Behavior scenarios

### Open a mixed spread

The pamphlet opens locally with a deliberate spread or short sequence that
keeps text columns and image frames legible together.

### Follow a source item

Selecting an item preserves its source identity and offers the source route
without collapsing the rest of the reading unexpectedly.

### Grow beyond the spread

When the set exceeds the pamphlet threshold, Grove adds pages or promotes it to
magazine rather than compressing the text section.

## Context

Pamphlet is owned by Annotation composition. It reads the current Content
forms supplied by the Memory-backed model but never creates a new Content or
Memory record. Source-local presentation is provided by the Information Plane.

## Decisions

- Mixed text/image content remains visibly mixed.
- Page footers expose Layer/source context while preserving local reading.
- Promotion is deterministic for the same source set and threshold fixture.

## Non-goals

- Semantic reordering or automatic summarization.
- Direct editing inside the Annotation publication.
- Audio, video, PDF, or EPUB.

## Evidence

- Wireframe: [[../../ux/wireframes/annotations/mixed-media/Annotation pamphlet]]
- Discovery: [[../../discovery/Annotation template thresholds and media forms]]
- Reference: [[../../reference/Content and Memory model]]
- Task: [[../../engineering/tasks/T-AN05 Pamphlet spread form]]
- Evidence: [[../../engineering/evidence/T-AN05 Pamphlet spread form]]
- Browser journey: `qa/scenarios/annotation-pamphlet.json`
