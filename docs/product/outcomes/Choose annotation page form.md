---
type: product-outcome
status: discovery
date: 2026-08-07
owner: "Grove Product"
roadmap: "[[../../roadmaps/Annotation roadmap]]"
source_notes:
  - "[[../../raw/original-notes/Grove - Annotations UX]]"
---

# Choose annotation page form

## Purpose

Define how the one accepted mixed-media reading changes form as related Content
crosses amount and concentration thresholds.

## Background

Annotation is an organized reading, not a fixed card. The source notes describe
three editorial forms—brochure, pamphlet, and magazine—inside one mixed-media
vertical. Exact text/image ratios and source-mass thresholds are not yet
product constants; the person should experience a clear form transition rather
than a mysterious resize.

## Outcome

> I can read related pieces in a page shape that matches how much text and imagery they contain, so nothing is cramped, padded, cut off, or needlessly shrunk.

## Behavior scenarios

### Resolve a light cluster

A small or light set resolves to the brochure form and remains readable as one
complete page.

### Resolve a mixed cluster

A moderate set with a useful text/image balance resolves to the pamphlet form
and keeps the relationship between image and text visible.

### Resolve a dense cluster

A large or dense set resolves to the magazine form and continues across pages
instead of shrinking the content into an unreadable block.

### Cross a threshold

When related Content grows beyond the current mixed-media form, Grove promotes
the reading to the next form or adds pages. It never silently truncates or
overflows. The same threshold must be checked again when Content is removed,
added, edited, or changes image resolution.

## Context

The resolver is new annotation behavior. It consumes Content type and payload
facts from the Memory-backed model (`js/core/model.js`, `js/core/state.js`) and
placement/field facts from `js/mechanics/field-kernel.js`; it does not mutate
those systems or derive a new Memory identity.

## Decisions

- Brochure, pamphlet, and magazine are named forms in one mixed-media vertical,
  not three verticals or implementation breakpoints.
- Thresholds are qualitative in the first outcome definition; numeric ratios,
  image-demand weights, and source-mass gates require fixtures and evidence
  before becoming stable decisions.
- A form change preserves source order, provenance, and complete content.

## Non-goals

- A user-selectable template editor.
- Numeric threshold controls exposed to the person.
- Automatic summarization, reordering for semantic meaning, or source mutation.

## Evidence

- Wireframe: [[../../ux/wireframes/annotations/mixed-media/Annotation page forms]]
- Discovery: [[../../discovery/Annotation template thresholds and media forms]]
- Reference: [[../../reference/Information Plane and Annotation model]]
- Reference: [[../../reference/Content concentration and layout model]]
- Task: [[../../engineering/tasks/T-AN03 Mixed-media form resolver]]
- Evidence: [[../../engineering/evidence/T-AN03 Mixed-media form resolver]]
