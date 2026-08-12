---
type: product-outcome
status: discovery
date: 2026-08-08
owner: "Grove Product"
roadmap: "[[../../roadmaps/Feature roadmap]]"
---

# Edit a Note in place

## Purpose

Give quick Note changes a lightweight local surface, so the fast path for a
small edit matches the speed at which Notes are captured.

## Background

Notes are rapid-capture plain text, but editing one today means either
fighting a small card on the Grid or opening the full Writing Slate — a
surface built for long-form writing. The Text editor is the Information
Plane local editor from [[../../decisions/Slates and local editors]]: a small
focused surface beside the source with complete text, Save, and Cancel, and
nothing else. Writing Slate remains available for deliberate long-form work.

## Outcome

> I can edit a Note right where it lives in a small focused editor, so a quick change never requires opening a full writing surface.

## Behavior scenarios

### Open beside the source

When I invoke edit on a Note — from its placement, the context menu, or a
Memory action — the Text editor opens beside the source with the complete
text and a visible caret, while the source stays in view.

### Make the change

When I type, the draft is transient; Save commits one Memory revision through
the normal path, and Cancel discards only the draft.

### Leave cleanly

When I save or cancel, the editor dismisses and focus returns exactly where I
was; the Grid, Camera, and any open slates are unchanged.

### Escalate deliberately

When a quick edit turns into real writing, an explicit action hands the same
Note to Writing Slate without losing the draft.

## Context

The Text editor is camera-independent Information Plane locality per
[[../../decisions/Information Plane locality]]. Memory identity and revision
rules are unchanged. Writing Slate remains the rich-text surface; the Text
editor is the default local route for Note edits per
[[../../decisions/Slates and local editors]]. Scope questions live in
[[../../discovery/Local editors and slate taxonomy]].

## Decisions

- The Text editor is a local editor: Information Plane, content-attached,
  dismissed back to invoking focus.
- Plain text only: no rich toolbar, preview, or pane composition.
- Save is the only durable write; it advances one revision and preserves
  origin identity.
- Escalation to Writing Slate is explicit and carries the draft.

## Non-goals

- Document or Markdown editing; that remains Writing Slate.
- A second draft store or autosave beside the revision path.
- Replacing the provisional-Note capture flow on the Grid.

## Evidence

- Wireframe: [[../../ux/wireframes/text-editor/Text editor]]
- Discovery: [[../../discovery/Local editors and slate taxonomy]]
- Decision: [[../../decisions/Slates and local editors]]
