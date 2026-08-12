---
type: derived-outcome
status: active
date: 2026-08-11
tags: [grove, derived-outcome, selection, marquee, signal-roles, non-destructive]
---

# Product Outcome: Non Destructive Work Selection And Batch Inspection

> **Outcome Statement**: 
> "I want to highlight and group multiple items across my workspace without altering their appearance or changing their underlying content."

---

## 1. Human Behavior Shift

### Previous Behavior (Status Quo)
In conventional design and whiteboard software, selecting an item restyles the content itself—recoloring text, adding heavy fill washes, or overlaying intrusive resize handles across card content. Multi-selection often modifies file metadata or groups items permanently into rigid frames, forcing users to ungroup items manually later.

### Transformed Behavior
Users select single or multiple items using click or marquee sweep gestures. Selected items maintain their complete visual integrity; an interaction outline (`#96B6F8`) runs 2px outside the content edge without obscuring text or images. Nearby grid cells brighten to reflect structural focus. Marquee drags display a temporary amber sweep line (`#E8B964`) during travel that seamlessly hands over to blue interaction outlines upon release. Selection is purely a temporary working state and is never written to card storage or file metadata.

---

## 2. Underlying Product Architecture & Primitives

This non-destructive behavior is derived from the Grove Design Catalogue (`grid-plane/07-selection-placement.html`) and rests on:

- **[Content](../definitions/Content.md)**: Remains visually un-tinted and un-truncated during selection, preserving pure authoring colors and boundaries.
- **[Grid](../definitions/Grid.md)**: Brightens surrounding cell field lines in response to selection without modifying cell content or spatial coordinates.
- **[Aura](../definitions/Aura.md)**: Intensifies presence field cells around selected footprints without altering core aura hues.

---

## 3. Behavioral Verification Scenarios

### Scenario 1: Marquee Selection Sweep
- **Given** a cluster of notes, documents, and images on the grid plane,
- **When** the user drags a marquee sweep across the cluster,
- **Then** caught items highlight immediately in the blue interaction role (`#96B6F8`) while the open marquee rectangle displays in amber (`#E8B964`).

### Scenario 2: Preserving Content Appearance During Selection
- **Given** an image and a paper document are selected,
- **When** the user inspects the selection state,
- **Then** the blue outline sits outside the card borders and cell fields brighten around them, while the paper texture, image pixels, and document typography remain 100% un-tinted.
