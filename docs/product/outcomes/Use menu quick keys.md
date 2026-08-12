---
type: product-outcome
status: discovery
date: 2026-08-08
owner: "Grove Product"
roadmap: "[[../../roadmaps/Feature roadmap]]"
---

# Use menu quick keys

## Purpose

Make the context menu a speed surface: every action shows the single key that
runs it, so the menu teaches its own shortcuts and frequent actions become
reflexes.

## Background

Grove's keybind registry is plane-grouped and presentation-aware, but its
bindings are global-scope chords. The context menu adds a second, gentler
tier: while a menu is open, each visible action exposes one plain keystroke —
right-click, then a single letter or digit — shown beside its label exactly
where desktop menus show accelerators. Submenu destinations number
themselves. The keys exist only while the menu is open, so they never collide
with Grid, editor, or slate bindings.

## Outcome

> I can press the single key shown beside a menu action to run it instantly, so frequent actions become quick reflexes instead of menu trips.

## Behavior scenarios

### See the key with the action

When the menu opens, every action row shows its quick key at the row's end,
quiet but readable; submenu items show digits in listed order.

### Act without aiming

When the menu is open and I press a shown key, that action runs exactly as if
clicked — same confirmation rules, same refusals — and the menu closes.

### Keys are scoped to the open menu

When no menu is open, those same keystrokes mean whatever the active plane
says they mean; a menu key never leaks into a global binding or steals typing
focus from an editor.

### Learn by using

When I use the same action repeatedly, the visible pairing of label and key
teaches the reflex — right-click, letter — without a settings screen or
memorization effort.

## Context

Quick keys extend the canonical keybind registry with a menu scope rather
than a parallel system; the registry remains the single mirror of
`docs/reference/Keybind map.md`. Key assignment rules, collision policy, and
localization questions live in
[[../../discovery/Context menu and plane-aware actions]]. The menu surface
itself is owned by [[Act from the context menu]].

## Decisions

- Quick keys are single strokes without modifiers, active only while their
  menu is open.
- Every menu action has a visible quick key; unlabeled actions do not ship.
- Key-to-action assignment is stable per target kind, so reflexes transfer
  between sessions and surfaces.
- Activation by key and by pointer are the same code path with the same
  guards.

## Non-goals

- New global chords or changes to existing plane bindings.
- Configurable key remapping in this release.
- Sequences or multi-key combos; one stroke per action.

## Evidence

- Wireframe: [[../../ux/wireframes/context-menu/Context menu]]
- Discovery: [[../../discovery/Context menu and plane-aware actions]]
- Reference: [[../../reference/Keybind map]]
- Engineering Task: [[../../engineering/tasks/T-CM01 Plane-aware context menu]]
- Evidence: [[../../engineering/evidence/T-CM01 Plane-aware context menu]]
