---
type: derived-outcome
status: active
date: 2026-08-11
tags: [grove, derived-outcome, menu, context-actions, keyboard-shortcuts, refusal]
---

# Product Outcome: Contextual Action Execution Without Dialog Interruption

> **Outcome Statement**: 
> "I want to act on any item or open space immediately using keyboard shortcuts, without navigating nested menus or being blocked by pop-up alerts."

---

## 1. Human Behavior Shift

### Previous Behavior (Status Quo)
In conventional desktop and web applications, performing actions on an asset requires clicking through multi-level hierarchical context menus or navigating modal dialog pop-ups. Destructive operations (like deletion) often sit adjacent to common edit options, causing accidental trigger errors. Unusable choices appear grayed-out, confusing users about why an action is unavailable, while confirmation alerts dim the workspace and break momentum.

### Transformed Behavior
Users invoke clean, single-level context menus directly at the point of action. Each menu lists only valid, supported verbs in plain language paired with single-stroke keyboard shortcuts. Unusable options are omitted entirely rather than grayed out. Destructive actions are isolated in dedicated groups at the bottom, behind hairline separators. Confirmations and explanations appear inline within the menu itself, keeping the surrounding workspace fully lit, responsive, and uninterrupted.

---

## 2. Underlying Product Architecture & Primitives

This execution model is derived from the Grove Design Catalogue (`01-menus.html` and `grid-plane/07-selection-placement.html`) and rests on the following primitives:

- **[Grid](../definitions/Grid.md)**: Serves as the active canvas substrate where context menus open over content or space without dimming the field or placing modal scrims.
- **[Content](../definitions/Content.md)**: Supplies target-specific action sets (`CM-01` for content, `CM-02` for space, `CM-04` for surfaces), ensuring menus name only what the target actually supports.
- **[Slate](../definitions/Slate.md)**: Integrates surface-owned menu actions within HUD slates without introducing external dialog windows.

---

## 3. Behavioral Verification Scenarios

### Scenario 1: Immediate Keyboard Action Execution
- **Given** a user opens a context menu over a placed document on the spatial grid,
- **When** they press the single-key shortcut `W`,
- **Then** the document immediately opens in the Writing Slate without requiring chord keys (`Ctrl` or `Cmd`) or mouse navigation.

### Scenario 2: Inline Confirmation Without Screen Scrims
- **Given** a user initiates a destructive removal or attempts to close an unsaved local editor,
- **When** the system requests confirmation,
- **Then** the confirmation question and actions render directly inside the menu frame, leaving the surrounding workspace completely visible and live.
