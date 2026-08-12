---
type: derived-outcome
status: active
date: 2026-08-11
tags: [grove, derived-outcome, annotation-routes, memory, grid-navigation]
---

# Product Outcome: Source Navigation From Editorial Syntheses

> **Outcome Statement**: 
> "I want to jump directly from a summary reading back to the original item or underlying master record without losing my place in the summary."

---

## 1. Human Behavior Shift

### Previous Behavior (Status Quo)
When reading compiled summaries or digests in standard productivity tools, clicking a source reference either navigates away from the summary entirely (losing reading position) or fails to show where the source document sits relative to surrounding work. Users are forced to manually search for referenced files in complex folder trees.

### Transformed Behavior
While reading a compiled broadsheet overview (such as a pamphlet or magazine annotation reading), users open a clean route menu on any individual item. Selecting "Show on the Grid" smoothly frames the item's precise spatial placement on its native layer while saving the reading state. A floating return marker appears on the canvas, allowing the user to return to their exact place in the reading with one click. Selecting "Open Record" opens the canonical Memory Slate without shifting camera position.

---

## 2. Underlying Product Architecture & Primitives

This navigation system is derived from the Grove Design Catalogue (`information-plane/08-annotation-routes.html`) and rests on:

- **[Annotation](../definitions/Annotation.md)**: Curates source-anchored editorial readings and provides route menus (`Show on the Grid`, `Open Record`, `Open in Writing Slate`) per item.
- **[Memory](../definitions/Memory.md)**: Opens the underlying canonical record in Memory Slate when selected, independent of grid coordinates.
- **[Grid](../definitions/Grid.md)**: Frames target placements when "Show on the Grid" is chosen and hosts the temporary "Return to Reading" marker.

---

## 3. Behavioral Verification Scenarios

### Scenario 1: Spatial Framing from an Editorial Reading
- **Given** a user is inspecting a multi-item summary broadsheet on the information plane,
- **When** they choose "Show on the Grid" from a note's route menu,
- **Then** the canvas frames the note's exact grid placement, highlights it with an interaction outline, and places a "Return to Reading" marker on screen.

### Scenario 2: Returning to Reading State
- **Given** a user has jumped to a grid placement from an editorial summary,
- **When** they click "Return to Reading",
- **Then** the canvas view restores the broadsheet summary at the exact scroll position and selected item state where they left off.
