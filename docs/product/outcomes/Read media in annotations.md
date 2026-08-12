---
type: product-outcome
status: discovery
date: 2026-08-07
owner: "Grove Product"
roadmap: "[[../../roadmaps/Annotation roadmap]]"
source_notes:
  - "[[../../raw/original-notes/Grove - Annotations UX]]"
---

# Read media in annotations

## Purpose

Define the supported media forms inside an Annotation publication.

## Background

The first Memory and Content scope supports Notes, Documents, regular Images,
and animated GIFs. Annotation composes those forms without flattening them or
pretending that unsupported audio/video/PDF/EPUB support exists.

## Outcome

> I can read the words and see the images in a related set without losing their original form, so the reading stays clear when the source forms are mixed.

## Behavior scenarios

### Read a Note

The Note remains readable as text and keeps its source context.

### Read a Document

The Document keeps its title/body structure and can continue across pages when
the selected template requires it.

### View an Image

The Image keeps its complete frame and can be selected for an explicit Image
Viewer handoff without cropping or recompressing it.

### View a GIF

The GIF preserves animation as a visual form with transient playback state; it
does not become a video workflow.

## Context

Payload and Memory identity come from `js/core/model.js` and `js/core/state.js`;
the current Grid renderer handles Notes, Documents, and Images in
`js/workspace/materials.js`. Annotation rendering is a new Information Plane
consumer. Writing Slate is a HUD destination and Image Viewer is an
Information Plane local editor; neither owns Annotation composition.

## Decisions

- Notes and Documents are text forms; Images and GIFs are visual forms inside
  the current mixed-media vertical.
- Complete frame/text fidelity is required; source mutation is not part of
  reading.

## Non-goals

- Audio or video.
- PDF or EPUB browse models.
- Editing a Memory inside the generated Annotation page.

## Evidence

- Wireframe: [[../../ux/wireframes/annotations/mixed-media/Annotation media forms]]
- Discovery: [[../../discovery/Annotation template thresholds and media forms]]
- Decision: [[../../decisions/Annotation source routing and media boundary]]
- Task: [[../../engineering/tasks/T-AN07 Edition media fidelity]]
- Evidence: [[../../engineering/evidence/T-AN07 Edition media fidelity]]
- Browser journeys: `qa/scenarios/annotation-brochure.json`,
  `qa/scenarios/annotation-pamphlet.json`,
  `qa/scenarios/annotation-magazine.json`,
  `qa/scenarios/annotation-media-gif.json`, and
  `qa/scenarios/annotation-media-gif-playing.json`
