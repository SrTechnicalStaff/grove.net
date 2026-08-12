---
type: design-system-component
status: active
date: 2026-08-09
component: Grid cursor
plane: grid
surface_class: chrome
tags: [grove, design-system, component]
---

# Grid cursor

The bright outline sitting on the addressed cell or the complete footprint of
the Content under it, with the fading path of cells behind it showing where it
came from.

## Anatomy

Every visible part, named, with the token that gives it its value. A part with
no token is a part with no contract.

| Part | Required | Value |
| --- | --- | --- |
| Head | yes | One cell on empty field; the complete rectangular footprint of the addressed Placement when the cursor is over Content. Every covered cell is in the same head state. |
| Head fill | yes | `--ink` at `--cursor-steady` × `--cursor-fill-gain`. The product is `0.132`; the component owns the figure because it is the result of two tokens and not a token itself. |
| Head ring | yes | `--cursor-ring` in `--ink` at `--cursor-ring-ink`, drawn as an inset border on the cell's own box; while a middle-pointer pan is held, the same role hue is drawn as a `2px` dashed border on the same box. |
| Spent cell | no | One cell, `--ink` at that cell's decayed energy × `--cursor-fill-gain`, no ring, no edge. |
| Footprint projection | no | Every whole cell the armed operation would take, drawn at the head's cell. |
| Type | none | The cursor sets no type, carries no label, and states no coordinate. |

The containment edge is the ring, and it is drawn inset so the head never
reaches past the cell it addresses — an outset ring would cover the neighbouring
cell and make the address ambiguous. `00-foundations/Elevation-and-depth.md`
fixes an inset shadow as the correct technique for exactly this edge. The head
has no interior padding, because a cell has no interior spacing.

The head takes `--r-none` rather than the chrome radius, because its four edges
must land on the grid's own major lines and a radius lifts the corners off them.

The projection's treatment is the placement preview's, not the cursor's: deck
`07-selection-placement.html` sets it at `--signal-interaction` fill `0.06`, a
`2px` inset edge at `0.60`, and a `1px` dashed edge at `0.80`, turning to
`--signal-refusal` fill `0.12` and inset edge `0.85` when the footprint cannot
land. This specification owns only which cells the projection covers, that it
appears when an operation arms and leaves when it ends, and that it carries no
words.

### Hue

One hue drives the ring, the head fill, and the spent cells together. At rest it
is `--ink`. While an operation is armed or open it is the role hue of that
operation's family, taken as the fill value rather than the border value,
because the border values fall below the 3:1 non-text floor on canvas.

| While | Hue |
| --- | --- |
| Nothing armed | `--ink` |
| Armed to put something down — new content, or Copy, Cut, Duplicate, Paste | `--k-tool` |
| Moving or resizing a placement | `--k-edit` |
| Trace | `--k-layer` |

Nothing else recolours the cursor. A refusal never reaches the head: it is drawn
on the projection and on the blocked cells, because refusal is about the
footprint and the head is only the address. Copy, Cut, and Duplicate take the
identical role hue and no variant of it, because each role hue has exactly one
value and a shade a person must read is not a signal.

## Geometry

- **Footprint** — one cell on empty field. Take the pointer's position in world
  coordinates, divide each axis by `--grid-cell`, and floor both results. When
  that cell belongs to a Placement, the head covers the Placement's complete
  width and height. A keyboard move adds `±1` to the addressed cell and keeps
  the complete head while the address remains inside the Placement.
- **Growth** — the head is one cell on empty field and the complete Placement
  footprint on Content. It never shows a partial Placement.
- **Measure** — none. The cursor sets no type.
- **Alignment** — all four edges land on major lines by construction, at every
  camera scale, with grid lines shown or hidden.

Extent is in cells; stroke weight is in screen pixels. The head is exactly one
cell under the camera and never counter-scales, and the ring holds
`--cursor-ring` on screen, following the line system's own rule that a tier's
pitch is in cells while its weight is one screen pixel. The ring is clamped to a
quarter of the projected cell size, because a ring wider than that fills the
cell it exists to outline.

Deck 01 draws the ring twice and the two disagree: the main stage projects it
through a `scale(0.75)` stage transform to `1.5px`, and the zoom strip holds it
at `2px` at cell `110`, cell `44`, and cell `13.2`. The zoom strip wins, because
it is the panel that states the rule and the main stage applies one transform to
a whole illustration.

## States

All nine. `—` is not an answer; write "No change from Rest" where that is
true.

| State | Appearance | Notes |
| --- | --- | --- |
| Rest | The head on its cell: `--ink` fill at `0.132` under a `--cursor-ring` ring at `--cursor-ring-ink`. No spent cells, no projection. | The trail has drained; a resting Grid is a still image. |
| Approached | No change from Rest. | The cursor is the pointer, so nothing can approach it. |
| Focused | No change from Rest. | The cursor is keyboard attention on this plane, so it never wears a focus ring of its own and no placement draws a second one. |
| Selected | Never reached. | The cursor takes no pointer events and is not hit-testable, so it cannot be selected. |
| Engaged | The ring, head fill, and spent cells take the operation's role hue at the same alphas, and the footprint projection is drawn at the head's cell. | Weight, geometry, and alpha never change with the hue. |
| Pending | No change from Rest. | The cursor draws the hand's current cell and waits for nothing, so it has no pending form. |
| Refused | No change to the head. The projection turns `--signal-refusal` and the cells that cannot take the operation are hatched at 45°. | The one plain sentence sits beside the footprint, inside the surface that asked. |
| Unavailable | Not drawn. The cursor's cell is held and it returns there when attention does. | Drawing an address while attention is on a surface above the Grid states something false. |
| Anchored | Never reached. | Anchored is a durable property of authored content, and the cursor is not content. |

## Behaviour

- **Pointer** — the cursor takes the cell the pointer's position falls in, on
  the input frame, and commits nothing by arriving there. It takes no pointer
  events itself; a press at its cell is answered by the Grid, and the
  Keybind map owns what that press does. Through a camera pan the cursor holds
  its cell and travels with the field. Its ring changes to the held form while
  the middle pointer is down, and it resumes the pointer's cell when the pan
  ends, because a pan changes what is in view and never what is addressed. Under
  a wheel zoom the cell is invariant by construction, since the zoom is anchored
  at the pointer.
- **Keyboard** — the arrow keys move the cursor one cell per press in that
  direction; `Enter` performs at the cursor's cell what a left click performs
  there; `Escape` cancels the open gesture and leaves the cursor on its cell.
  This is a decision, because `docs/reference/Keybind map.md` assigns the cursor
  no key while `00-foundations/Accessibility.md` requires every action to be
  reachable from the keyboard, and the arrow keys are the desktop convention for
  a cell cursor. When a keyboard move would take the cursor outside the
  viewport, the camera pans by whole cells to keep it one cell inside the edge,
  because a cursor off screen stops saying where attention is.
- **Focus order** — the cursor has no focusable parts and is not in the tab
  order, because it is the plane's own attention mark and a second tab stop
  would be a second model of the same thing.
- **Escape** — Escape with nothing above the Grid cancels the active
  gesture or mode; the cursor stays on its cell and returns to `--ink`. Escape
  closing the last surface above the Grid returns attention to the cursor
  at its held cell.
- **Commit and cancel** — the cursor commits nothing and there is nothing to
  undo. Its cell is view state: it is never written to a Memory, a Placement, or
  a Layer, and it does not survive a reload.

Which operation a key or a press starts belongs to
`docs/reference/Keybind map.md` and to the wireframes that own those journeys.

## Motion

| Transition | Duration | Curve | Reduced motion |
| --- | --- | --- | --- |
| Head follows the pointer or an arrow key | None | None | Unchanged; continuous rendering continues. |
| Spent cell decays by `--cursor-trail-decay` each rendered frame, and is dropped below `--cursor-trail-min` | None | None | Skipped; the cell clears on the frame the cursor leaves it. |
| Ring, fill, and spent cells take or drop a role hue | None | None | Unchanged; the hue changes in place on the frame the operation arms or ends. |

No duration token appears anywhere on this component, because
`00-foundations/Motion.md` classes the head and its decay as continuous
rendering and a duration token on continuous rendering is a defect.

A cell is deposited at `--cursor-steady` and decays from there, so eighteen
frames after the head leaves a cell the cell is gone — `125 ms` at the `144 Hz`
floor. The shipped prototype deposits energy scaled by pointer speed, with a
floor below which a slow move deposits nothing; deck 01 renders two spent cells
at `0.111` and `0.093`, exactly one and two decay steps below the resting
`0.132`, and the deck wins, because a speed-scaled trail makes one path read two
ways and a careful slow move would leave no trail at all.

Nothing loops. Nothing idles. Nothing pulses or blinks.

## Distance

How the component reads at each representation tier. Thresholds are on
projected cell size and belong to `10-grammar/Representation-tiers.md`; this
section says only what this component sheds and what it keeps.

| Tier | Sheds | Keeps |
| --- | --- | --- |
| Working | Nothing | Head, ring, spent cells, and the footprint projection. |
| Stepped | Nothing | Head, ring, spent cells, and the footprint projection. |
| Stand-in | Nothing | Head, ring, spent cells, and the footprint projection. |

The cursor has no tiers. Its empty-field head is one cell and its occupied head
is the complete Placement footprint at every distance. It sheds nothing at any
distance, because the resize corner is gone below the stepped tier and the
cursor's occupied head remains the route to the full geometry.

## Accessibility

- **Role and name** — the cursor draws no accessible node and has no name. The
  Grid is the focusable element, and where the cursor's cell holds a
  placement, that placement's own accessible name is reported through
  `aria-activedescendant`; an empty cell has no descendant and reports nothing.
  This is a decision, because the fact worth naming is the authored content
  attention is on, never the numbers of a cell. The spent cells are hidden from
  assistive technology rather than named.
- **Contrast** — the ring reaches `12.5:1` on canvas in `--ink` at
  `--cursor-ring-ink`, `7.1:1` in `--k-tool`, `7.5:1` in `--k-edit`, and
  `7.6:1` in `--k-layer`; all four clear the 3:1 non-text floor. The head fill
  reaches `1.4:1` and carries no meaning on its own, so the floor does not bind
  it. The cursor sets no text.
- **Without colour** — the carrier is geometry: a whole-cell ring on cell
  boundaries, a silhouette drawn nowhere else in Grove. The armed hue is never
  the only carrier, because the footprint projection appears on the same frame
  the hue changes.
- **Forced colours** — the ring is redeclared in `Highlight` and survives. The
  head fill and the spent cells are paint, do not survive, and are not
  redeclared, because a system colour carries no alpha and a solid cell would
  hide whatever is beneath it.
- **Text scaling** — nothing changes. The cursor sets no type and is measured in
  cells, so raising interface text moves neither its size nor its position.
- **Reduced motion** — as the Motion table: the decay tail is skipped and a cell
  clears on the frame the cursor leaves it; the head is unchanged.

## Copy

None.

The cursor shows no string in any state, and its footprint projection carries
none either. Deck `07-selection-placement.html` labels the projection `3 × 3` in
one panel and `Occupied` in another; both are corrected, because the covered
cells already state the extent and the refusal sentence belongs beside the
footprint under the state model.

## Refusals

- **No fixed screen size** — the head is measured in cells under the camera,
  one cell on empty field or the complete occupied footprint; a cursor pinned to
  a screen size floats free of the field and lies about which cell it is on.
- **No selection outline and no glow** — it points, it does not hold, and a
  glow is not available at any edge in Grove.
- **No sub-cell position** — the cursor is never between cells and never at a
  pointer point, because the cell is the unit of address.
- **No loop** — it never pulses, blinks, breathes, or spins, including while an
  operation is armed and while a footprint cannot land.
- **No words, coordinates, count, or size figure** — the moment the cursor
  states a value it becomes a readout about the Grid instead of a position
  in it.
- **No second cursor** — one head, one addressed cell or one complete occupied
  footprint, at a time, on one plane.
- **No shadow and no blur** — the ring is a border drawn inset, and an inset
  shadow with a blur radius is refused everywhere.
- **No hit-testing** — it takes no pointer events, is never a drag target, and
  is never something a person can click.
- **No persistence** — its cell is never written to a Memory, a Placement, or a
  Layer.
- **No arrow** — the Grid hides the system pointer and the cursor is the
  only pointer it has; an arrow drawn over the field would address a point where
  everything else addresses a cell.


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

- Catalogue deck: `docs/design_catalogue/src/grid-plane/01-the-grid.html`
- Catalogue deck: `docs/design_catalogue/src/grid-plane/07-selection-placement.html`
- Source note: `docs/raw/original-notes/Grove - controlling content.txt`
- Source note: `docs/raw/original-notes/Grove controls.txt`
- Wireframe: `docs/ux/wireframes/Navigate the Grid.md`
- Reference: `docs/reference/Keybind map.md`.
