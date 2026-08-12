---
type: product-outcome
status: discovery
date: 2026-08-08
owner: "Grove Product"
roadmap: "[[../../roadmaps/Feature roadmap]]"
---

# Act from the context menu

## Purpose

Give every right-click a menu that understands what was clicked, so acting on
something never requires leaving it.

## Background

Right-click has been deliberately reserved since the plane-controls work: it
must never mutate Content until a real context menu exists. Grove now has the
seam to build it properly — every surface declares its plane and contract in
the surface registry, so a menu can be routed from the hit target instead of
being one global list. The menu is plane-aware: a placement on the Grid, a
card in a slate, a local editor, and empty Grid space each present their own
short, fitting action set.

## Outcome

> I can right-click anything and get a short menu of exactly the actions that fit what I clicked, so I can act where my attention already is.

## Behavior scenarios

### A menu that matches the target

When I right-click placed Content, I see that Content's actions — open, edit
in its form's editor, cut, copy, duplicate, trace, anchor, remove. When I
right-click empty Grid space, I see space actions — paste, create here, frame
the Layer. When I right-click a slate card or a local editor, I see that
surface's actions. Nothing generic, nothing that cannot run here.

### Submenus for destinations

When an action has destinations — open in, move to Layer — a submenu lists
them, and hovering or arrow keys reveal it without committing anything.

### Nothing happens by accident

When the menu opens, nothing mutates. Escape or clicking away closes it with
no effect, and destructive actions are visually distinct and never adjacent
to safe defaults.

### The same grammar everywhere

When I use the menu on any plane, opening, navigating, and dismissing feel
identical; only the actions change.

## Context

Menu contents are routed through the surface contract registry and the plane
router; the menu itself is a neutral surface that requests actions from the
hit target's owner and never reaches into another plane's state. Single-key
activation is owned by [[Use menu quick keys]]. Routing, content sets, and
HIG-guided structure live in
[[../../discovery/Context menu and plane-aware actions]].

## Decisions

- The context menu is plane-aware by routing, not by special cases: the hit
  target's surface contract supplies the action set.
- Opening the menu is side-effect free; only an explicit activation acts.
- Menus are short: actions that cannot apply are omitted, not disabled rows
  without explanation.
- Right-click remains non-mutating until this outcome lands; the reservation
  is honored, then fulfilled.

## Non-goals

- A global command palette or menu bar.
- Long-press/touch menus in this release.
- Replacing existing keybinds or plane controls; the menu is an additional
  route to the same mechanics.

## Evidence

- Wireframe: [[../../ux/wireframes/context-menu/Context menu]]
- Discovery: [[../../discovery/Context menu and plane-aware actions]]
- Decision: [[../../decisions/Slates and local editors]]
- Engineering Task: [[../../engineering/tasks/T-CM01 Plane-aware context menu]]
- Evidence: [[../../engineering/evidence/T-CM01 Plane-aware context menu]]
