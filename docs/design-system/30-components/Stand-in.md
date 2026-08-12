---
type: design-system-component
status: active
date: 2026-08-09
component: Stand-in
plane: grid
surface_class: placement
tags: [grove, design-system, component]
---

# Stand-in

What a placed Note, Document, or picture becomes when the camera is far enough
back that its words can no longer be read: a block in that thing's own colour,
covering exactly the cells it occupies, still the thing itself.

## Anatomy

A stand-in has one part and one set of marks inside it. The block is the
placement's footprint; every figure below is a proportion of that footprint,
because the field is measured in cells and a distance on the field written in
screen pixels is a defect.

| Part | Required | Value |
| --- | --- | --- |
| Block | yes | The placement's exact cells. Corners `--r-none`. No shadow, no padding, no inset margin. |
| Fill — Note | yes | The Note's authored fill: `--c-note-violet`, `--c-note-clay`, or `--c-note-slate-blue`. |
| Fill — Document, picture | yes | `--surface-page`. |
| Inset edge — Note | yes | 1px `--edge-on-color`, drawn `box-shadow: inset 0 0 0 1px` so the block's box stays exact cells. |
| Inset edge — Document, picture | yes | 1px `--paper-border`, drawn the same way. |
| Note strokes | yes | Three strokes in `--ink-primary`, left inset `14%`, tops `28%` `48%` `68%`, widths `66%` `52%` `60%`, weight `5%` of the block's shorter side. |
| Sheet head bar | yes | One bar in `--c-paper-ink` at `--paper-body`, left `14.6%`, top `20.8%`, width `70.8%`, height `4.2%`. |
| Sheet body rules | yes | Four rules in `--c-paper-ink` at `--paper-body`, left `14.6%`, tops `37.5%` `49.0%` `60.4%` `71.9%`, widths `70.8%` `52.1%` `70.8%` `52.1%`, height `2.1%`. |
| Picture plate | yes | `--c-paper-ink` at `--paper-rule`, inset `9%` left, right and top and `17%` bottom. |
| Picture figure mark | yes | Filled `--c-paper-ink` at `--paper-body`, inside the plate, stretched to it: polygon `4,66 32,28 50,46 68,18 92,66` and circle `cx 72 cy 16 r 7` in a `0 0 96 78` box. |
| Anchor mark | when Anchored | The ribbon or the rotated square owned by `00-foundations/Marks.md`, at its stated pixel geometry, unscaled. |
| Type | — | None. A stand-in sets no type at any size; that is what this tier is. |

Every percentage is a component value because it is a proportion of one
placement's footprint and no other component shares it. The percentages come
from the decks: the sheet from the `96px` snapshot in the Document deck, the
Note strokes and the picture figure from the Distance and Image decks.

A mark is drawn only while its own thickness reaches one screen pixel. Below
that the fill alone carries kind, which is the floor the tier grammar already
sets.

## Geometry

- **Footprint** — the placement's, unchanged, inherited rather than computed.
  The procedure: take the placement's cell rectangle; multiply its column, row,
  width and height by `--grid-cell`; multiply by the camera scale; translate by
  the camera. Draw that rectangle. There is no rounding to whole screen pixels,
  no minimum size, no sub-cell offset, and no snap-to-screen, so the same
  placement drawn twice at the same camera lands on the same sub-pixel edges.
- **Growth** — nothing here can outgrow the block, because the block carries no
  authored content. When authored content grows the placement's footprint, the
  footprint grows and the block redraws to match on the next frame.
- **Measure** — none. A stand-in sets no type, so no `ch` measure applies to it.
- **Alignment** — all four edges land on the major lines bounding the
  placement's cells, at every scale. A block whose edge falls off a major line
  is a defect in the projection, never a rounding the block applies.

A footprint is a whole-cell rectangle, not a square. A picture's block is as
wide and as tall as its picture's cells, so a 5 × 3 picture is a 5 × 3 block.

## States

| State | Appearance | Notes |
| --- | --- | --- |
| Rest | Fill, inset edge, kind marks. Nothing else. | The whole component at rest, and the majority of what a far camera shows. |
| Approached | No change from Rest. | Chrome has already been shed two rungs earlier, and a target this small cannot be reached by hand. |
| Focused | No change to the block. The Grid cursor sits on the focused cell. | The cursor is measured in cells and shrinks with the field, so a second ring would be the only thing on screen not obeying the camera. |
| Selected | A `2px` `--signal-interaction` outline offset `3px` outside the block, and the cells around the footprint brighten. The fill, edge and marks are untouched. | Identical to a working form's selection, because a stand-in is the placement and selection never reads as a different thing at a different distance. |
| Engaged | The open gesture's role drawn on the footprint, unchanged from the working form: `--signal-interaction` for a move, resize, or placement preview. | The block itself keeps its rest drawing at the origin while a move is open, so nothing durable appears to have happened. |
| Pending | No change from Rest. The stand-in is the pending form. | Holding the coarse form is the whole answer to Pending on the field; there is nothing further to show. |
| Refused | `--signal-refusal` replaces the edge hue in place, and the cells that cannot take the operation hatch at 45°. | The sentence stays in the surface that asked, because a block a few pixels across has no room for words. |
| Unavailable | Fill `--text-unavailable`, inset edge `--edge-hairline`, kind marks dropped, position and extent held. | Kind is not lost to distance here: Unavailable is a standing condition of the application, and a mark on a fill this faint cannot be read anyway. |
| Anchored | The anchor ribbon or the rotated square at its stated pixel geometry, unscaled, and the presence beneath the block in `--signal-authored-context`. | Two carriers, geometry and presence, so authored context survives both greyscale and a block one pixel wide. |

Precedence between states, and what each state may add as a mark, are already
fixed; a stand-in introduces no exception to either.

## Behaviour

- **Pointer** — the block is hit-tested on its exact cells and answers on the
  input frame. A press takes the pressed appearance over `--d-press`; release
  inside selects, exactly as on a working form. A drag from inside the block
  moves the placement. There is no resize corner at this tier, so a drag never
  resizes here. Approach commits nothing and opens nothing.
- **Keyboard** — the component answers no key of its own. Every Grid Plane
  binding in `docs/reference/Keybind map.md` acts on a stand-in with no
  exception, including `R` for resize, which is the only resize route at this
  tier. Focus arrives as the Grid cursor at the block's cell and leaves the same
  way; the tier is not a mode and adds no binding.
- **Focus order** — one stop, the block. It contains no focusable part, so
  `Tab` never enters it.
- **Escape** — cancels the open Grid gesture and leaves the block exactly as it
  was. Escape never dismisses a stand-in, because a representation is not a
  surface and there is nothing to close.
- **Commit and cancel** — a stand-in commits nothing and is never written. It is
  derived and disposable: never a revision, and never what a Memory or a Trace
  refers to. Every durable change reached through it belongs to the placement
  and is undone on the placement.

Which gesture a person is running, and what it does to the Grid, belongs to
the owning wireframe for that gesture.

## Motion

| Transition | Duration | Curve | Reduced motion |
| --- | --- | --- | --- |
| Promotion: block exchanged for the stepped form in place | `--d-swap` | `--ease` | The stepped form replaces the block on the same frame, at the identical `--tier-standin-promote` threshold |
| Demotion: stepped form exchanged for the block | None — immediate | — | Unchanged; drawing less is always available this frame |
| Press acknowledgement | `--d-press` | `--ease` | The pressed appearance applied on press and removed on release, with no scaling |
| Selection outline appearing or leaving | None | — | Unchanged; an outline is drawn on the frame selection changes |
| The block's marks redrawn after its placement's content changes | None | — | Unchanged; a redraw is not a transition |

Nothing loops. Nothing idles. Nothing pulses or blinks. A queued promotion draws
no progress of any kind: the block is still while it waits, and promotion never
blocks the camera.

## Distance

This component is the floor of the shedding order, so its rows read as what it
gives up to become the fuller form above it and what it keeps at its own tier.

| Tier | Sheds | Keeps |
| --- | --- | --- |
| Working | Itself, in one exchange over `--d-swap`; a stand-in reaches this tier only through the stepped form, one rung at a time. | Nothing of its own. Identity, position and extent pass to the working form unchanged, and nothing moves to arrive. |
| Stepped | Itself, in the same exchange. | The same three facts, handed over in place. |
| Stand-in | Nothing further. There is no fourth rung. | Fill, inset edge, kind marks, exact cells, presence, the anchor mark, the perimeter ring, and the refusal hatch. |

The kind-coded forms are exact, and no two of them collapse into one glyph:

- **Sheet form** — paper, a ruled head bar, four body rules. A page reads as a
  page.
- **Note form** — the authored fill the person chose, under three light strokes.
  Colour is the fastest carrier and it is the one a person picked.
- **Picture form** — a paper frame with a plate inside it and a filled figure
  mark on the plate. The deeper bottom margin is the mass the caption row holds
  in the working form, so the frame reads the same at both distances.

## Accessibility

- **Role and name** — the stand-in is the same element as the placement and
  keeps that element's role. The accessible name is the authored content: the
  Note's text, the Document's title, the picture's source name. The block's own
  drawing is decorative and is hidden from assistive technology, and crossing a
  threshold announces nothing.
- **Contrast** — no text at any size, so 4.5:1 never applies. `--surface-page`
  on `--surface-grid` is 17.3:1. The Note fills on the Grid are
  3.64:1, 3.82:1 and 3.70:1. The Note strokes at `--ink-primary` on those three
  fills are 3.55:1, 3.36:1 and 3.49:1. The sheet head bar and body rules at
  `--paper-body` on `--surface-page` are 4.75:1. The picture figure mark at
  `--paper-body` on its plate is 4.2:1. Every meaning-bearing mark clears 3:1.
  The inset edge at `--paper-border` is 1.82:1 and is exempt because it
  separates two light surfaces and carries no meaning kind does not already
  carry.
- **Without colour** — kind is hue plus form: a head bar over four rules, three
  strokes, or a plate with a figure on it are three silhouettes, so greyscale
  still separates the three kinds. Selected is the offset outline and the
  brightened cells. Refused is the 45° hatch and the sentence beside it.
  Anchored is the ribbon or the rotated square.
- **Forced colours** — fills and marks are canvas paint and do not survive. The
  block redeclares, in system colours, a 1px edge on its exact footprint and one
  mark set per kind, so position, extent and kind all survive. The authored Note
  colour does not survive, because a forced-colours mode replaces every hue.
- **Text scaling** — nothing here scales. The block carries no interface text
  and is measured in cells, so raising interface text to 200% changes neither
  its size nor its position.
- **Reduced motion** — no change from the Motion table.

## Copy

None. A stand-in shows no string at any size: no file name, no kind word, no
count, no size, no coordinates, and no label. Its accessible name is the
placement's authored content, which this component does not write.

## Refusals

- **An empty cell** — a footprint drawn as nothing claims the Grid holds
  less than it holds.
- **A blank while a promotion is queued** — the block is the fallback, and a
  gap where content stands is a lie about what exists.
- **A spinner, progress ring, skeleton shimmer, or busy animation** — a progress
  mark on the field replaces content with a report about content.
- **Counter-scaling the block** — a block held at a fixed screen size floats
  free of the field and lies about where and how big it is.
- **Aggregation into a cluster or a count** — three placements are three blocks
  at three positions, and a bubble with a number replaces positions with
  arithmetic.
- **A minimum size, a sub-cell offset, a snap-to-screen rule, or rounding the
  block's edges to whole screen pixels** — each of them moves a placement to
  make it easier to draw.
- **One neutral block for every kind, a file-name card, or a file-type icon** —
  a stand-in states what the thing is, never what file it came from.
- **A signal hue or a role hue as a fill or a mark** — a stand-in is a placement
  at rest, so it is neither a state nor an operation.
- **A shadow** — a stand-in lives in the field, not above it.
- **Being pinned to a tier by selection, by kind, or by state** — every
  placement at the same distance is at the same distance.
- **Being written** — a stand-in is derived and disposable, and content is not.


## Design assertions

| ID | Assertion |
| --- | --- |
| C-01 | Every visible part has a named token or explicit component value. |
| C-02 | Footprint, growth, measure, alignment, and responsive rules are explicit. |
| C-03 | Rest, Approached, Focused, Selected, Engaged, Pending, Refused, Unavailable, and Anchored are explicit. |
| C-04 | Pointer, keyboard, focus, Escape, commit, cancel, persistence, and return behavior are explicit. |
| C-05 | Refusal and recovery preserve identity, provenance, source order, and unchanged durable state. |
| C-06 | Motion, reduced-motion behavior, camera relation, and distance shedding are explicit. |
| C-07 | Component-specific Grid, Information Plane, or HUD visual signatures are retained. |

## Sources

- Catalogue deck: `docs/design_catalogue/src/grid-plane/06-distance.html` — the contract.
- Catalogue decks: `docs/design_catalogue/src/grid-plane/02-note.html`, `03-document.html`, `04-image.html` — the per-kind forms.
- Source note: `docs/raw/original-notes/Grove at a distance.txt` — the camera is a lens, and the change in view is universal.
- Decision: `docs/decisions/Camera zoom range and representation.md` — 1% to 1000%, with identity and location retained at every value.
- Keys: `docs/reference/Keybind map.md`.
