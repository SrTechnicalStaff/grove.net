---
type: product-outcome
status: implemented-verified
date: 2026-08-07
owner: "Grove Product"
roadmap: "[[../../roadmaps/Product Roadmap]]"
---

# Use plane controls

## Purpose

Define the predictable control boundary across Grove's Grid, Information, and
HUD planes.

## Background

Grove has three surfaces with different responsibilities. A person needs to
know which controls belong to the surface they are using and must be able to
use a control without accidentally starting a gesture in another plane.

## Outcome

> I can see which controls belong to the Grid, Information, and HUD planes and use them without triggering actions in another plane.

## Behavior scenarios

### Read the controls

I open Controls and see the active keybinds grouped by Grid Plane, Information
Plane, HUD Plane, and any surface-specific scope.

### Use a Grid control

I use a Grid keybind or gesture and Grove changes Grid state only. A nearby
control or surface does not receive the Grid gesture.

### Use an Information Plane surface

I write or act in a local surface and the Grid does not select, pan, zoom, or
start a Grid gesture from that input.

### Use a HUD control

I open a fixed global surface and the Camera, Grid selection, and Placement
geometry remain unchanged until an explicit product action requests a change.

### Recover

Escape, cancellation, and reload leave each plane's state at a recoverable
boundary without a hidden cross-plane mutation.

## Context

The three-plane architecture is defined by
[[../../decisions/Camera observer and Grid projection boundary]].
The canonical control inventory is [[../../reference/Keybind map]].

## Decisions

- The Controls HUD and runtime keyboard routing use one canonical keybind
  registry.
- Keybinds use `Ctrl/Cmd` notation and physical-key normalization.
- Plane boundaries are input boundaries, not just visual stacking.

## Non-goals

- Creating a generic command palette.
- Defining the Content context menu; that requires its own Product Outcome and
  discovery brief.
- Changing Memory, Placement, Layer, or Annotation product meaning.

## Evidence

- Decision: [[../../decisions/Camera observer and Grid projection boundary]]
- Reference: [[../../reference/Keybind map]]
- Engineering plan: [[../../engineering/Plane boundary stabilization plan]]
