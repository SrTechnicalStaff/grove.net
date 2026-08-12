---
type: product-outcome
status: discovery
date: 2026-08-07
owner: "Grove Product"
roadmap: "[[../../roadmaps/Annotation roadmap]]"
---

# Follow annotation source routes

## Purpose

Define deliberate navigation from an Information Plane Annotation item to the
Grid, the canonical Memory record, or the appropriate focused surface.

## Background

An Annotation item may be read locally, returned to its placement, opened as
the core/original Memory, or opened in a form-specific focused surface. These
are different journeys. A placed Content instance is not the Memory, and
opening a Memory does not imply that the person wants to find it on the Grid.

## Outcome

> I can choose whether to revisit a piece where I placed it, open the original record, or focus it in the right viewer, so I can continue with the context I need.

## Behavior scenarios

### Return to Grid placement

I choose Grid placement and Grove hands the selected source to the Grid
Placement/navigation function. The route identifies Layer and cell context but
does not make the Annotation or Camera own that state.

### Open core Memory

I choose Core Memory and Memory Slate opens the canonical record, whether or
not the Memory is currently placed. Multiple placements remain a separate
placement fact.

### Open a form-specific surface

Notes and Documents can open Writing Slate; Images and GIFs can open Image
Viewer. The destination owns focus and presentation in its declared plane,
while Memory remains the source record.

### Return to the Annotation

Closing the destination returns to the invoking Annotation or its source route
when that route still exists; it does not create a second Annotation.

## Context

The current placement handoff pattern in `js/mechanics/placement.js` and the
separation between Memory and Placement in `js/core/model.js` are the relevant
precedents. `js/ui/hud-plane.js` and the Writing Slate outcome define HUD
ownership; the Image Viewer outcome and Information Plane model define local
viewer ownership. A future route dispatcher must use explicit
feature boundaries rather than event leakage across `js/interactions/plane-router.js`.

## Decisions

- Source destinations are explicit actions, not inferred from the current
  plane.
- Core Memory is a canonical-record route; Grid placement is a spatial route;
  Writing Slate/Image Viewer are focused form routes in their declared planes.
- No route silently moves the Camera, mutates Placement, or creates Memory.

## Non-goals

- Command-palette-first annotation discovery.
- Automatic opening of Grid, Memory Slate, or a slate after selection.
- Editing a Memory from the generated Annotation edition itself.

## Evidence

- Wireframe: [[../../ux/wireframes/annotations/source-routes/Annotation source routes]]
- Discovery: [[../../discovery/Annotation source routes and plane handoffs]]
- Decision: [[../../decisions/Annotation source routing and media boundary]]
- Task: [[../../engineering/tasks/T-AN09 Annotation source routes]]
- Evidence: [[../../engineering/evidence/T-AN09 Annotation source routes]]
- Browser journey: `qa/scenarios/annotation-source-routes.json`
