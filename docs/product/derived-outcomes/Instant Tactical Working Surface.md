---
type: derived-outcome
status: active
date: 2026-08-11
tags: [grove, derived-outcome, slate, quick-capture, tactical-surface]
---

# Product Outcome: Instant Tactical Working Surface

> **Outcome Statement**: 
> "I want to immediately pull up an active document or reference item to work on directly, then dismiss it cleanly without cluttering my main arrangement."

---

## 1. Human Behavior Shift

### Previous Behavior (Status Quo)
When users need to quickly draft a note, review an image, or reference a document, traditional software forces them to either create a permanent canvas item (which clutters their organized layout) or switch to an external text editor/file browser window. Managing multiple floating windows breaks momentum and litters the desktop.

### Transformed Behavior
Users activate an instant tactical working surface (Slate) with a quick shortcut or gesture. The focused editor appears over their view instantly, allowing them to jot down notes, review media, or inspect reference data. Once finished, a single keystroke dismisses the surface, leaving their spatial canvas layout pristine and organized.

---

## 2. Underlying Product Architecture & Primitives

This workflow is supported by:

- **[Slate](../definitions/Slate.md)**: Acts as a screen-fixed, tactical modal window (Writing Slate, Memory Slate, Gallery Slate) that opens and closes instantly.
- **[Memory](../definitions/Memory.md)**: Persists all captured notes and edits to storage without requiring manual file placement on the grid.
- **[Content](../definitions/Content.md)**: Converts tactical notes into permanent spatial cards only when explicitly dropped onto the grid plane.

---

## 3. Behavioral Verification Scenarios

### Scenario 1: Quick Note Capture via Slate
- **Given** a user has an active spatial workspace open,
- **When** they invoke the quick Slate shortcut,
- **Then** a lightweight writing window opens instantly over the viewport, ready for immediate typing.

### Scenario 2: Clean Dismissal Without Layout Artifacts
- **Given** a user has finished capturing thoughts in a tactical Slate,
- **When** they press `Escape` or click close,
- **Then** the Slate window closes, the captured content is saved to memory, and the spatial canvas layout remains completely unchanged.
