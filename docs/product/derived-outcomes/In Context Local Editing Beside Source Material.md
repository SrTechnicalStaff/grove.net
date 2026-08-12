---
type: derived-outcome
status: active
date: 2026-08-11
tags: [grove, derived-outcome, local-editor, editing, image-viewer, spatial-context]
---

# Product Outcome: In Context Local Editing Beside Source Material

> **Outcome Statement**: 
> "I want to edit notes and view images directly beside their original placement on my canvas, so I never lose sight of what surrounds them."

---

## 1. Human Behavior Shift

### Previous Behavior (Status Quo)
In conventional software, editing a note or inspecting a picture opens a full-screen editor or centered modal box that completely covers the original card and its surrounding materials. Users lose visual contact with neighboring reference documents, forcing them to open and close windows repeatedly to check facts or copy details from nearby items.

### Transformed Behavior
Users double-click or select a note or image on the canvas to open a compact local editor or viewer directly beside the source item. The source item remains fully visible in place on the grid plane. The editor frame carries role-tinted borders (edit-tinted `#7A3F3A` for text, view-tinted `#3F5F8A` for media) and a quiet dirty-draft indicator. If a short edit grows into long-form writing, users escalate the draft to a dedicated Slate window seamlessly without losing edits.

---

## 2. Underlying Product Architecture & Primitives

This experience is derived from the Grove Design Catalogue (`information-plane/01-local-editors.html` and `information-plane/02-image-viewer.html`) and rests on the following primitives:

- **[Content](../definitions/Content.md)**: Serves as the anchored source material on the grid plane while the local editor opens beside it.
- **[Grid](../definitions/Grid.md)**: Maintains spatial positioning so local editors align anchored to source cell coordinates without obscuring neighboring cards.
- **[Slate](../definitions/Slate.md)**: Receives escalated long-form text drafts (`Open in Writing Slate`) when a local edit expands beyond quick notes.

---

## 3. Behavioral Verification Scenarios

### Scenario 1: Quick Text Edit Beside Spatial Source
- **Given** a user is viewing a note surrounded by research images on the grid plane,
- **When** they initiate a local text edit,
- **Then** a compact editor frame opens beside the note, keeping the note content and all surrounding research images fully visible.

### Scenario 2: Escalating Quick Edits to Long-Form Writing
- **Given** a user is typing a draft in a local editor and needs multi-paragraph formatting,
- **When** they click "Open in Writing Slate",
- **Then** the unsaved text transfers seamlessly into a dedicated Writing Slate HUD surface without losing any draft edits.
