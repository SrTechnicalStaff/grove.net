---
type: design-system-plane
status: active
date: 2026-08-09
tags: [grove, design-system, information]
---

# Information Plane

The plane for surfaces that belong beside something. A local surface appears
next to the thing it serves, keeps that thing visible, and returns focus to it
when it closes.

Locality is the whole reason this plane exists. A surface that could sit
anywhere on screen without losing meaning belongs to `HUD.md` instead.

## Runtime contract

| Property | Value |
| --- | --- |
| Plane order | `1` — above Grid, below HUD |
| CSS compositor band | `z-index: 300` |
| Coordinate system | Source anchor projected into Information-local pixels |
| Camera relationship | Features: none; projection bridge: read-only observation |
| Anchor | Exact canonical source or ephemeral pointer origin |
| Surface host | Shared `SurfaceCoordinator` |
| Keyboard ownership | Information-origin events remain Information; named surface bindings precede ordinary plane bindings |
| Root pointer behavior | Transparent outside active surfaces |

The compositor never promotes Information above HUD because a surface opened
later. Pan and zoom reproject a local surface with its source without scaling
the surface.

## What it owns

| Owns | Does not own |
| --- | --- |
| Editing a Note's or a Document's text beside the placement. | Reading a Document through to the end. |
| Viewing a picture at its full size. | Browsing a collection. |
| Reading an annotation near where it was found, and returning to it. | Composed arrangements of panes. |
| | Capture before a source exists. |
| Bars that carry an open operation — a trace, a transfer, a clipboard, an import. | Durable content. |
| Menus and flyouts raised from a placement. | |

## Position

A local surface is placed against its exact source, not against the viewport.
Its opening side is selected once, using the source bounds, surface bounds, and
the component gap. That side remains fixed for the session.

The feature does not communicate with Camera or Grid projection. The separate
projection bridge reads source geometry and Camera state and supplies
Information-local pixels. Pan and zoom move the surface anchor with the source;
the surface's dimensions, type, controls, border, and internal layout remain
screen-constant.

An open local surface may leave the viewport with its source. It is not clamped,
re-flipped, rescued by a viewport marker, or promoted to HUD.

Two local surfaces are never open on the same source. Opening a second closes
the first.

Capture before a source exists is HUD content. Information has no viewport-fixed
exception.

## Anatomy

A local editor is a compact frame: `--surface-chrome` fill, `--r-sm`, a 1px
border tinted by its operation role — Edit for text surfaces, View for
viewers — `--sp-md` padding, and `--shadow-local` to separate it from the
Grid.

`10-grammar/Surface-classes.md` owns the full anatomy of each class. Two rules
belong here because they are about locality:

- **The opening side clears the source.** Later Camera movement preserves the
  selected relationship and may carry either source or surface offscreen.
- **Dismissal returns focus.** Escape closes the topmost surface and only
  that surface, and focus lands back on the placement it came from.

## Reading

Annotation editions read as print on paper: `--surface-page` fill,
`--text-page`, a readable measure in `ch`, steady line rhythm, and
baseline-aligned columns.

Editions paginate. They never truncate, crop, or set type below the reading
size. Edition chrome is minimal — page context in quiet monospace, source
routes as understated actions, and nothing that competes with the reading.

Page mechanics govern here: margins, columns, gutters, a baseline grid, and
threaded text. A reading surface is a page, and it behaves like one.

## Refusals

- **No scrim.** Nothing on this plane dims, blurs, or blocks the Grid.
  A full-viewport tint with content centred in it is a modal, whatever it is
  called.
- **No centre-screen surface anchored to a source.** A surface that serves one
  source sits beside it.
- **No viewport rescue.** A local surface is never clamped, re-flipped, or
  replaced by a fixed return marker after opening.
- **No surface that outlives its source.** If the placement is removed, the
  surface closes.
- **No second level of flyout.** One level; menus flip to stay on screen
  rather than cascading.
- **No dark reading pane.** Continuous prose is set on paper.
- **No scroll inside a placement's footprint.** Depth belongs to a surface a
  person opened, never hidden inside content on the field.
