---
type: product-outcome
status: discovery
date: 2026-08-07
owner: "Grove Product"
roadmap: "[[../../roadmaps/HUD Plane roadmap]]"
source_notes:
  - "[[../../raw/original-notes/Grove - information layer]]"
  - "[[../../raw/original-notes/Grove - Documents]]"
---

# Compose Slate composition

## Purpose

Define how multiple fixed-screen slates share the HUD without turning one
slate into another slate's navigation or state.

## Background

A person may need to browse Memories while writing, inspect an Image while
keeping the gallery available, or focus one slate alone. The HUD therefore
needs a single-slate full-width mode, a half-slate mode, and an explicit way to
assign independent slates to the left and right sides. A slate can start from
selected placed Content or a Memory record; neither source determines the
other pane.

## Outcome

> I can open a slate full-width, use it at half width, or pair it with another slate on the opposite side, so I can keep one view focused or work across memories and content without losing my place.

## Behavior scenarios

### Open one slate

I open Memory Slate, Writing Slate, or Image Viewer from a keybind, CTA, placed
Content handoff, or Memory action. It opens full-width by default with its own
navigation and dismissal state.

### Assign a second slate

I choose Add slate and select Left or Right. The assignment preview shows the
result before commit; an occupied side is never silently replaced.

### Work in two panes

I can browse Memory Slate on one side while editing in Writing Slate or
inspecting an Image in Image Viewer on the other. Each pane keeps its own
selection, scroll position, draft state, and return path.

### Collapse and restore

When the available width is too narrow, I collapse one pane into a preserved
tab or restore it to full-width. Collapsing does not close the slate or discard
its state.

### Close deliberately

I close one pane without changing the other, or dismiss the HUD host when no
slate remains. The Grid and the invoking surface do not navigate as a side
effect.

## Context

The Slate host is a HUD composition function. It owns pane assignment, sizing,
focus, and dismissal. Memory Slate owns gallery browsing; Writing Slate owns
focused text editing; Image Viewer owns single-asset inspection. All three
surfaces resolve the canonical Memory when opened from a placed Content
instance or directly from a Memory action.

## Decisions

- One slate opens full-width; adding a second slate requires an explicit
  left/right assignment.
- The two panes are peers. Selection in one pane never replaces or navigates
  the other pane.
- Narrow widths collapse a pane while preserving it for restoration.
- Pane controls expose the current side, expand/full, collapse, swap, and close
  actions without hiding the active slate's identity.
- Apple split-view guidance is used as a reference for adjacent panes,
  explicit selection, and width-aware collapse; Grove does not adopt Apple's
  hierarchy or platform-specific navigation model wholesale.

## Non-goals

- A third simultaneous slate in the first slice.
- A new Memory or Content store for pane state.
- Grid camera, cell, Layer, or Placement behavior inside the HUD host.
- Automatic replacement of an occupied side.
- Audio or video slates.

## Evidence

- Wireframe: [[../../ux/wireframes/multi-slate-hud/Multi-slate HUD]]
- Discovery: [[../../discovery/Slate composition and pane assignment]]
- Decision: [[../../decisions/Slate composition and pane assignment]]
- Architectural prerequisite: [[../../engineering/tasks/T-ARCH01 Surface contract registry and router]]
- Architectural evidence: [[../../engineering/evidence/T-ARCH01 Surface contract registry and router]]
- Apple reference: [Split views](https://developer.apple.com/design/human-interface-guidelines/split-views)
