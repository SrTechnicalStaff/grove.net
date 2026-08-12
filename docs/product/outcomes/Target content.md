---
type: product-outcome
status: implemented-verified
date: 2026-08-07
owner: "Grove Product"
roadmap: "[[../../roadmaps/Grid Plane roadmap]]"
---

# Target content

## Purpose

Define how a person selects the Content they are about to work with.

## Background

Grid operations need a visible target set before a later operation can act. A
partial marquee or an invisible selection would make the next action ambiguous.

## Outcome

> I can select one or more pieces of Content on the current Layer and see exactly what I selected before I act on it.

## Behavior scenarios

### Select one piece

Clicking a Content item selects it and clears the previous selection unless a
multi-selection modifier is active.

### Select several pieces

Modifier-click and full-footprint Marquee add complete Content items to the
selection while leaving partial or hidden items out.

### Clear the selection

Empty-space click, Escape, or a Layer change clears the selection according to
the active interaction rule.

### Inspect the selection

The selected Content and its contributing field presence remain visibly marked;
the selection is not persisted as durable Content state.

## Context

Selection is temporary state used by later operations. Placement supplies the
footprint, Grid supplies the region, and Layer limits the target set.

## Decisions

- Marquee requires complete-footprint containment.
- Selection never changes Content, Placement, or field facts.
- Selection feedback represents the actual target set.

## Non-goals

- Moving, resizing, deleting, copying, or tracing Content.
- Saved or named selections.
- Cross-Layer selection.

## Evidence

- Model: [[../../reference/Grid and Placement model]]
- Browser journey: `qa/scenarios/target-content.json`
- Implementation state: [[../../engineering/Implementation status]]
