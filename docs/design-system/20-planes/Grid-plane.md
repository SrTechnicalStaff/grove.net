---
type: design-system-plane
status: active
date: 2026-08-09
tags: [grove, design-system, grid]
---

# Grid Plane

The plane a person places things on. It is the only plane that holds durable
spatial content, and the only one the camera moves.

Everything here is addressed in cells and scales with the camera. If a thing
keeps a fixed size on screen while the camera moves, it does not belong to
this plane.

## Runtime contract

| Property | Value |
| --- | --- |
| Plane order | `0` — lowest |
| CSS compositor band | `z-index: 10` |
| Coordinate system | Grid cells projected by Camera |
| Camera relationship | Grid projection only |
| Surface relationship | Grid owns no Information or HUD surface DOM |
| Keyboard ownership | `InputCoordinator` bindings declared `plane: "grid"` |
| Pointer ownership | Grid root and Grid-owned descendants |

Grid never raises its compositor level to compete with Information or HUD.

## What it owns

| Owns | Does not own |
| --- | --- |
| Placed Notes, Documents, and pictures. | Reading a Document through to the end. |
| The line hierarchy and its fade. | Editing text in place. |
| Presence — the per-cell field cast by everything placed, on every Layer. | Browsing Memories that are not placed. |
| The Grid cursor, its footprint projection, and its trail. | Any surface a person opens deliberately. |
| Selection, the marquee sweep, and placement previews. | Composed arrangements of panes. |
| Representation at distance, including stand-ins. | Capture that is not yet placed. |
| Refusal of a placement, and the strip that states it. | |

## Chrome budget

At rest, with the pointer off the Grid, the only things drawn are:
placed content, grid lines, presence, and the Grid cursor. Nothing else.

A frame that shows anything more at rest has a defect. This is the strictest
budget of the three planes, and it is what makes the Grid readable as a
place rather than an application window.

Chrome that serves a placement — a resize corner, an edit affordance —
belongs to that placement, appears on approach, and leaves with the pointer.
Chrome that serves an operation arrives when the operation is called and
leaves when it ends. Nothing parks here.

## What a person sees

**Content is the brightest thing.** The field sits one step off canvas so
every placement is brighter than the ground beneath it. Kind is legible before
a word is read: paper against authored colour, a display title against plain
text, a complete frame against both.

**Position carries meaning, and nothing else does.** Two placements near each
other are near each other. The field brightens where things gather, and that
is the whole of what it says — no grouping, no link, no count.

**The whole Grid is always present.** Pulling back simplifies how a
placement is drawn and never whether it exists. At the furthest zoom every
placement still occupies its exact cells in a kind-recognisable form.

## Geometry

Every footprint is a whole number of cells on both axes, with every edge on a
major line. A Note solves to the smallest whole-cell square that holds its
text; every other kind takes the whole-cell rectangle its content needs.

Nothing has a minimum screen size, a sub-cell offset, or a snap-to-screen
rule. A placement's size on screen is its footprint under the camera, and that
is the only thing that decides it.

`00-foundations/Space-and-grid.md` holds the line system and the fade;
`10-grammar/Representation-tiers.md` holds what sheds and when.

## Layers

A person works on one Layer at a time. That Layer's content is drawn in full.

Every other Layer within reach contributes **presence only** — lit cells in
its content's hue, in the same cell system, with no frames and no readable
text. A hundred Layers accumulate as atmosphere, never as clutter. Drawing a
faint frame from another Layer is a defect: it shows a person something that
is not where they are working.

## Refusal

A refused placement is answered where it was attempted. The preview turns to
the refusal role, the cells that cannot take it are hatched, and one plain
sentence sits beside the footprint with the committing action present and
unavailable.

Nothing is half-done. The source keeps its place, the Grid is exactly as
it was, and there is nothing to undo.

## Boundaries with the other planes

A placement is the source for surfaces on other planes, and never opens one by
itself. Approaching, selecting, or moving a placement never opens anything.

- Reading a Document through, or writing one, is a composed arrangement and
  belongs to `HUD.md`.
- Editing a Note's text in place, viewing a picture at full size, and reading
  a nearby annotation are local and belong to `Information-plane.md`.
- Both are entered deliberately, and both return focus to the placement they
  came from.

The Grid never dims for either. A surface on another plane is opaque over
its own footprint and nothing more; everything outside it stays lit, readable,
and live.

## Refusals

- **No container per placement.** A frame, border, and shadow around every
  piece of content turns the field into a card layout.
- **No chrome parked on the field.** Tools arrive when called and leave when
  the work is done.
- **No counter-scaling.** Nothing holds a fixed screen size while the camera
  moves.
- **No aggregation at distance.** Three placements are three marks at three
  positions, never a bubble with a count.
- **No empty cell.** When a form is not ready, the coarser form holds the
  footprint.
- **No second grid.** One line system, one fade rule; hiding lines is a view
  preference that removes no behaviour and unlocks none.
