---
type: product-outcome
status: discovery
date: 2026-08-07
owner: "Grove Product"
roadmap: "[[../../roadmaps/Annotation roadmap]]"
source_notes:
  - "[[../../raw/original-notes/Grove - Annotations UX]]"
  - "[[../../raw/original-notes/Grove - annotations]]"
  - "[[../../raw/original-notes/Grove at a distance]]"
---

# Read place-aware annotations

## Purpose

Define the complete annotation reading contract: how spatial density becomes a
local reading, how supported media is composed, and how every source remains
reachable without replacing the original Content.

## Background

Annotation is a generated reading surface, not a raw Field inspector or a
comment attached to a sentence. It groups related placed Content by field or
local cluster, uses the current mixed-media form family, preserves provenance,
and provides a route back to every source. Memory remains the canonical record;
Content remains a placed view of that record.

## Outcome

> I can open a complete reading of related pieces and return to each original piece, so exploring a cluster never hides or loses what it came from.

## Behavior scenarios

### Notice a place

At a distance, a marker identifies a meaningful field or local cluster without
covering the Grid in persistent UI.

### Choose a reading form

The marker opens the appropriate form in the current mixed-media vertical. A
light cluster can stay a brochure, a moderate cluster can use a pamphlet
spread, and a dense cluster can continue as a paginated magazine.

### Open a reading

I open the marker and receive a complete local edition whose layout responds to
the Content mix and amount. Notes, Documents, Images, and GIFs retain their
form instead of being flattened into one generic card.

### Preserve source truth

The edition does not truncate, summarize, or replace source Content. Every item
retains its Memory, Layer, and Placement route.

### Return to source

I choose whether to return to the original Grid placement, open the core
Memory record, or continue in the appropriate HUD slate. The choice does not
confuse a placement with the Memory or silently move the Camera.

### Keep a useful reading

I pin one local Annotation and can return to it after the source leaves view;
the pinned reading remains tied to its source route until I dismiss it.

## Context

The Information Plane owns local overlay behavior, not publication semantics.
The HUD Plane may host a deliberate handoff to Memory Slate or Writing Slate;
the Information Plane may host Image Viewer beside its source. Neither route
detaches the reading from its source route. Current
code already supplies the relevant boundaries: `js/mechanics/field-kernel.js`
and `js/mechanics/presence-field.js` expose field facts and projection;
`js/features/information-plane.js` and `js/ui/surface-controller.js` own local
surface locality, modality, and focus; `js/interactions/plane-router.js` keeps
plane input separate; `js/core/model.js` and `js/core/state.js` keep Memory
identity separate from `placements`; and `js/ui/hud-plane.js` owns fixed HUD
surface state. The annotation reader, marker controller, template resolver,
and cross-plane route dispatcher are not implemented yet.

## Decisions

- One local reader remains open until close, Escape, or explicit replacement.
- Complete pagination is preferred over truncation or overflow.
- A HUD entry route does not create a second Annotation or remove the local
  source route.
- Brochure, pamphlet, and magazine are forms in the one implemented mixed-media
  vertical. The accepted calculation also defines three tiers for text-led and
  image-led verticals; their renderers remain [Next] work. PDF, EPUB, audio,
  and video are outside the first release.
- A source action is explicit: Grid placement, core Memory, or the appropriate
  HUD slate. Annotation never performs an implicit Camera move or placement.

## Non-goals

- Raw Cell or saturation inspection.
- A second Content or Memory store.
- Fixed screen publication detached from source locality.
- Command-palette-first discovery, automatic summaries, and creating a new
  Memory from a highlighted excerpt. Those are separate functions or future
  outcomes.

## Evidence

- Discovery: [[Annotation markers and publication]]
- Reference: [[../../reference/Information Plane and Annotation model]]
- Decision: [[../../decisions/Annotation source routing and media boundary]]
- Wireframe: [[../../ux/wireframes/annotations/reading/Annotation reading]]
- Task: [[../../engineering/tasks/T-AN02 Local Annotation reader]]
- Evidence: [[../../engineering/evidence/T-AN02 Local Annotation reader]]
