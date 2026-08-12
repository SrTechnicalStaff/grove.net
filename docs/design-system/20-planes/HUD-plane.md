---
type: design-system-plane
status: active
date: 2026-08-09
tags: [grove, design-system, hud]
---

# HUD Plane

The plane for work that needs room. A composed surface is summoned
deliberately, arranges one or more panes, and serves the whole Grid
rather than one placement.

Everything here is fixed to the viewport and unaffected by the camera. Nothing
here is addressed in cells.

## Runtime contract

| Property | Value |
| --- | --- |
| Plane order | `2` — highest |
| CSS compositor band | `z-index: 400` |
| Coordinate system | Viewport pixels |
| Camera relationship | None |
| Anchor | Rejected; HUD surfaces have no Information locality |
| Surface host | Shared `SurfaceCoordinator` |
| Keyboard ownership | `InputCoordinator` bindings declared `plane: "hud"` |
| Root pointer behavior | Transparent outside active surfaces; decorative watermark numerals never capture pointer input |

HUD remains above Information regardless of open sequence. A HUD surface never
resizes the Grid viewport and never reads or writes Camera state.

## What it owns

| Owns | Does not own |
| --- | --- |
| Reading and writing a Document through to the end. | Placed content. |
| Browsing Memories, placed or not. | Editing beside a placement. |
| Browsing a collection of pictures. | Anything that must stay next to its source. |
| Managing Layers. | Presence, the cursor, or the field. |
| Any arrangement of more than one pane. | |

## Composition

A composed surface is built by one host, never assembled ad hoc. It has one
header carrying its identity in the Slate hue, one body of panes, and one
return path.

It never floats free and it never dims the Grid behind it. Its fill is
opaque over its own footprint, `--r-sm`, one quiet 1px border, generous
padding, and no shadow — it meets the screen at every edge, so there is nothing beside it to separate from. Everything outside it stays lit,
readable, and live — a person can read a Note on the field while a composed
surface is open, and click it.

Panes inside a surface use `--surface-nested`. That is the third and last dark
step; a pane inside a pane is a composition that has not decided its hierarchy.

## Room to work

A composed surface exists because the work needs space. It is generous by
default: content-first, with padding from the upper end of the scale, and
never squeezed to look compact.

A surface may scroll, because it is chrome rather than a footprint on the
field. It may never shorten, summarise, or shrink what it carries to avoid
scrolling. A reading page inside it paginates rather than truncating.

## Collections

Collections are masonry, never a grid of crops. Frames keep their intrinsic
proportions and complete edges, columns balance around a comfortable minimum
width, and gutters use `--sp-sm`.

Card metadata is quiet monospace beneath or beside the frame, never overlaid
on it. Selection is an interaction outline plus structure, applied outside the
frame, so the picture itself is never restyled.

An empty collection is a designed state with a next step, not a blank surface.

## Entering and leaving

A composed surface is always entered deliberately — a key, or an action a
person chose. Approaching, selecting, or moving a placement never opens one.

Opening is side-effect free: nothing is created, moved, or committed by the
act of looking. Escape dismisses the topmost surface first and only that
surface, and focus returns to whatever opened it.

When a composed surface acts on a placement, it hands off rather than reaching
across: it names the Memory, Content, or Placement it wants and lets the
Grid answer. It never draws on the field, moves the camera, or writes
another plane's state.

## Refusals

- **No scrim, no modal.** The Grid stays lit and live behind every
  composed surface.
- **No surface that opens itself.** Nothing appears because a person moved a
  pointer or selected something.
- **No crop to a uniform tile.** Masonry preserves complete frames.
- **No metadata over a picture.** Details live off the frame.
- **No fourth dark step.** Surface, nested, and that is the end of the stack.
- **No competing scene.** A composed surface separates itself from the
  Grid; it does not replace it.
