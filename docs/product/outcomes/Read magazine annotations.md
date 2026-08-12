---
type: product-outcome
status: discovery
date: 2026-08-07
owner: "Grove Product"
roadmap: "[[../../roadmaps/Annotation roadmap]]"
---

# Read magazine annotations

## Purpose

Define the dense, paginated reading for a large Annotation cluster.

## Background

Magazine is the largest initial publication form. It gives dense mixed Content
an ordered page sequence with room for complete text and images. The form is a
reading edition, not a second document or a replacement for the source
Memories.

## Outcome

> I can read a dense mixed set across ordered pages, so the Content stays complete without becoming a tiny wall of content.

## Behavior scenarios

### Open a dense reading

The magazine opens locally with a clear page sequence, page count, and source
context.

### Continue across pages

When a text section or media group does not fit, Grove continues on the next
page instead of shrinking, clipping, or hiding it.

### Return from a page

Every item keeps a deliberate route to its original placement, core Memory, or
form-specific HUD slate.

## Context

Magazine composition consumes field-local Content and threshold fixtures. The
Grid, Camera, and Memory model remain observers/owners of their own facts; the
reader owns only publication order and page state.

## Decisions

- Pagination is the overflow contract.
- Page footers include Layer/source context for every page.
- The first release supports Notes, Documents, Images, and GIFs only.

## Non-goals

- Exporting or editing the generated edition.
- PDF/EPUB browsing, audio, or video.
- A fixed HUD modal that loses the source route.

## Evidence

- Wireframe: [[../../ux/wireframes/annotations/mixed-media/Annotation magazine]]
- Discovery: [[../../discovery/Annotation template thresholds and media forms]]
- Reference: [[../../reference/Information Plane and Annotation model]]
- Task: [[../../engineering/tasks/T-AN06 Magazine edition form]]
- Evidence: [[../../engineering/evidence/T-AN06 Magazine edition form]]
- Browser journey: `qa/scenarios/annotation-magazine.json`
