---
type: design-system-component
status: active
date: 2026-08-09
component: Presence
plane: grid
surface_class: chrome
tags: [grove, design-system, component]
---

# Presence

The light every placed thing casts into the cells around it, so a person can
see where their work gathers without reading a word.

## Anatomy

Presence is drawn on the field, beneath everything a person placed. It is not
one of the four surface classes; it never floats, never opens, and never
closes, so the closure rule in `10-grammar/Surface-classes.md` is untouched.

| Part | Required | Value |
| --- | --- | --- |
| Lit cell | yes | Fills exactly one cell, inset `0` on all four sides, at `--r-none`. Paint only: no border, no inset edge, no shadow. |
| Cell alpha | yes | `clamp(value, --field-alpha-min, --field-alpha-max)`, where `value` is the summed per-source strength multiplied by `--field-gain`. |
| Cell hue | yes | The radiating content's own value: `--c-note-violet-field`, `--c-note-clay-field`, or `--c-note-slate-blue-field` for a Note; `--signal-authored-context` where the placement is Anchored; the `--ink` triplet for a Document or an Image. |
| Cell boundary | yes | No paint. A boundary is the edge of the cell's own box, hard at every zoom; there is no stroke, no feather, and no shared pixel with the neighbour. |
| Unlit cell | yes | `--surface-grid`, untouched. An unlit cell is not painted at a floor value. |
| Perimeter ring | yes, on an occupied region | `--field-perimeter-width` in the region's own hue at `--field-perimeter-ink`, rising to `--field-perimeter-selected` while the region is selected. Geometry is owned by `00-foundations/Marks.md`. |
| Type | none | Presence sets no type at any zoom. |

The containment edge is none: presence contains nothing, and a border around a
lit region would make the field a container. The perimeter ring is a mark that
restates where a region ends, not an edge that encloses it.

The inset figure `0` is this component's own and is not a token, because every
other inset in Grove is measured on the spacing scale in screen pixels and a
lit cell is measured in cells; a lit cell that stopped short of its own edge
would put an unpainted seam on a boundary the field uses as its pixel.

### The value of a cell

```text
per source, per cell
  distance     = Euclidean cell distance to the nearest cell of the footprint
  contribution = peak × exp(−layerDistance × 0.55) × exp(−distance ÷ falloffLength)
  strength     = fieldVisibleStrength(contribution, coverage)

per cell
  value        = Σ  strength × --field-gain      // over every source reaching the cell
  alpha        = clamp(value, --field-alpha-min, --field-alpha-max)
```

`peak`, `falloffLength`, and `coverage` are derived from the footprint by
`src/domain/field.ts` and are versioned by `FIELD_MODEL_VERSION`. They are
field mathematics, not visual values, which is why they are not in the token
table: the only figures a designer may change are `--field-gain`,
`--field-alpha-min`, and `--field-alpha-max`.

`--field-alpha-min` is a floor on a lit cell, never a fill on an unlit one: a
cell whose value falls below it is dropped from the lit set, and a cell that
survives is never painted fainter than the floor. `--field-alpha-max` is the
ceiling that keeps the field quieter than the content it announces, and it
binds after summation, which is the only place summation can breach it.

### Hue sums, it does not blend

A cell holds one contribution per source, each in that source's own hue at
that source's own alpha, composited in source order. No midpoint colour is
computed, stored, or painted, so a violet Note and a clay Note sharing a cell
each stay readable in it and removing one removes exactly its contribution.
Where a placement is Anchored its contribution is `--signal-authored-context`
at its one fixed value, so authored context stays recognisable inside a mixed
cell.

A shared cell means two things sit near each other. No link, group, order, or
relationship is implied by it, and none is stored.

### Derived, never stored

Presence is recomputed from what sits on the field. It is never written to a
Layer, a Placement, or a Content payload, and it is never a revision. Remove
the content and the field has nothing left to hold. The per-cell source
record that annotations query is a read of the same computation, not a saved
picture of it.

## Geometry

- **Footprint** — presence has no footprint of its own; its extent is the
  union of every cell any source reaches. Executable procedure: for each
  Layer within the declared rendering falloff of the current Layer, and each
  placement on it, take `fieldProfile` from the placement's footprint, take
  `fieldReach` for that profile at that Layer distance, and mark every cell
  within that reach; the lit set is the union of those marks, with each cell's
  alpha computed as above. Run twice, get the same set.
- **Growth** — reach grows with the footprint, because `falloffLength` grows
  with the square root of the footprint's area. A larger placement lights more
  cells; longer text inside an unchanged footprint lights none.
- **Measure** — none. Presence sets no type, so it has no reading width.
- **Alignment** — every lit cell's four edges land on major grid lines. The
  perimeter ring is drawn inset on the cell's own box, on the outward edges
  only, so the contour is the region's exact shape.

### Falloff, ring by ring

1. `n` is the Chebyshev distance in cells from the cell to the nearest cell of
   the footprint. Ring `n` is every cell at that distance.
2. A cell on ring `n` takes level `n`.
3. Where the horizontal and vertical offsets are **both** exactly `n` — the
   ring's four corner cells — the cell takes level `n + 1`, one step lower
   than the straight run beside it.
4. Levels continue outward until a level's alpha falls below
   `--field-alpha-min`; that level and everything beyond it is unlit.

Deck 05 renders four levels for a neutral source at `0.19`, `0.12`, `0.070`,
and `0.038`, with each ring's corners taking the next level down. The falloff
is computed per cell and never drawn as a halo, so the perimeter of a footprint
stays exact and the light begins at the neighbouring cell.

### Across Layers

A Layer other than the current one contributes presence and nothing else: lit
cells in its content's hue, in the same cell system, summing into the same
cells. No frame, no text, no outline, and no faint drawing of any kind crosses
from another Layer. Attenuation is `exp(−layerDistance × 0.55)`. Rendering may
drop contributions below the declared contribution floor, but Layer depth is
unbounded: a fifteenth Layer, or a deeper Layer, remains valid data and remains
navigable. Field visibility never rejects, relocates, or deletes its Content.

The perimeter ring is drawn only on a region occupied on the current Layer,
because a contour around a footprint a person cannot reach states an extent
they cannot act on.

### The occupied region

An occupied region is the footprint of one placement. Each placement draws its
own contour; two abutting placements never merge into one, because a contour
that spans two placements draws a group the field does not have.

## States

| State | Appearance | Notes |
| --- | --- | --- |
| Rest | Lit cells at their computed values, hard-edged, in their sources' hues; the perimeter ring at `--field-perimeter-ink` in the region's own hue. | The resting field is one of the four things the Grid draws with the pointer away. |
| Approached | No change from Rest. | Presence does not answer the hand; it is not hit-testable and adds nothing on approach. |
| Focused | No change from Rest. | Keyboard attention on the field is the Grid cursor; presence draws no second mark. |
| Selected | Presence values are unchanged. One ring of whole cells is added in `--signal-interaction`, summed into the same cells as any other contribution, and the region's perimeter rises to `--field-perimeter-selected` at unchanged hue and unchanged width. | Deck 07 renders that ring at `0.13` with its corners at `0.06`. One ring only: the response announces a footprint, not a mass. Where the placement is also Anchored the added ring is `--signal-authored-context`, because the anchor hue wins on the field. |
| Engaged | Destination cells light and origin cells dim as the footprint tracks the pointer. Recompute is local to the cells the move touches; the rest of the field is untouched. | Nothing durable changes while the gesture is open, and the field returns to the pre-gesture values on the frame after a cancel. |
| Pending | No change from Rest. | Presence is unconditional: a stand-in casts exactly the field its working form casts, because the field reads the footprint and never the drawing. |
| Refused | No change from Rest. | The refusal hatch is drawn over the blocked cells and above the field; presence takes no refusal hue, because the field reports what is there and refusal answers an attempt. |
| Unavailable | No change from Rest. | A placement that cannot be acted on still occupies its cells and still radiates. |
| Anchored | Every contribution from that placement is `--signal-authored-context`, at every ring and in every mixed cell. | The hue is fixed wherever authored context appears and is never a colour a person can pick. |

## Behaviour

- **Pointer** — presence answers no gesture. The field takes no pointer events
  and every gesture passes through it to the placement or the cell beneath.
  During a move the field recomputes locally on the cells the gesture touches;
  nothing durable changes until release, and the release frame is when the
  field's new values become the resting values.
- **Keyboard** — presence answers no key. `G` hides the grid lines, and
  presence is pixel-identical with lines shown and lines hidden.
- **Focus order** — none. Presence is never focusable and never enters the tab
  order.
- **Escape** — nothing. Presence has nothing to dismiss. Escape cancels the
  Grid gesture that was open, and the field follows the restored arrangement on
  the next frame.
- **Commit and cancel** — presence is never committed and never undone. The
  durable change belongs to the placement; the field is a consequence of it.

**Input never waits.** A frame may draw the field one frame behind the hand —
the deck shows the light still centred one cell behind a moving footprint — and
it may never delay hit-testing, pointer capture, the dragged footprint's
position, or any key the Grid answers. The trade is always taken in the
field's lag and never in input latency.

Which gesture opens, what it previews, and what it commits belong to the
owning wireframe and to the placement and selection specifications.

## Motion

| Transition | Duration | Curve | Reduced motion |
| --- | --- | --- | --- |
| A cell's value changing as a footprint moves, arrives, or leaves | None — continuous rendering | None | Unchanged: continuous rendering depicts a current value, not a change over time. |
| A hue entering or leaving a cell as a Layer enters or leaves reach | None — continuous rendering | None | Unchanged. |
| The perimeter ring appearing, moving, or leaving with its region | None — continuous rendering | None | Unchanged. |
| The perimeter alpha rising to `--field-perimeter-selected` and falling back | None — the alpha is redrawn on the frame selection changes | None | Unchanged. |

Presence owns no duration token and no curve. It is redrawn each frame from
current values, so there is nothing to ease and nothing to skip. Nothing loops,
idles, pulses, or blinks; a field with no hand on it is a still image.

## Distance

| Tier | Sheds | Keeps |
| --- | --- | --- |
| Working | None | Every cell value, every hue, hard cell boundaries, and the perimeter ring. |
| Stepped | None | The same, unchanged. |
| Stand-in | None | The same, unchanged. |

Presence never sheds, and it has no stand-in: the field is one drawing at every
tier, because it is computed from footprints and a footprint does not simplify.
Nothing about presence counter-scales — a cell is a cell under the camera, at
every zoom.

The camera floor of `1%` puts the smallest projected cell at `2.2px`, so a cell
is never sub-pixel and no averaging across cells is ever required. Smoothing at
distance is refused for the same reason it is refused at working zoom: an
averaged pixel reports a value the field does not hold.

## Accessibility

- **Role and name** — none. The field is hidden from assistive technology
  rather than named, because it describes the Grid rather than its
  contents; the placements that cast it carry the authored names. The canvas
  it is drawn on is `aria-hidden="true"`.
- **Contrast** — presence carries no text, so no text ratio applies. The
  perimeter ring at `--field-perimeter-ink` in `--c-note-violet-field` measures
  `1.27:1` on `--surface-grid`, and `2.75:1` at
  `--field-perimeter-selected`. Both are deliberately below the `3:1` non-text
  minimum and both are exempt, because the ring restates an extent the
  placement already draws at full strength and selection is carried by the
  `2px` interaction outline on the placement itself. No fact in Grove is
  reachable only by resolving the field.
- **Without colour** — the second carrier is the lit cell: a hard-edged
  rectangle on cell boundaries, at a value distinct from canvas, plus the
  ring's contour. In greyscale the field still says where work gathers and how
  far a region extends. Hue in the field confirms which content is radiating
  and never carries a fact on its own; deck 05's "hue tells what kind"
  overstates it and is corrected, because kind is legible from a placement's
  form and authored context is legible from its ribbon or diamond.
- **Forced colours** — lit cells are paint and do not survive. The perimeter
  ring is redeclared in system colours at `--field-perimeter-width` on occupied
  regions, so extent still reads and the field does not disappear silently.
- **Text scaling** — nothing. Presence carries no text and does not scale with
  interface text; it is measured in cells and scales with the camera alone, so
  raising interface text to 200% moves no lit cell by a pixel.
- **Reduced motion** — no change from the Motion table.

## Copy

None. Presence shows no string at any zoom, in any state, on any Layer, and
carries no label, count, legend, key, or coordinate. The field carries no copy
and orientation lives in geometry and ink.

## Refusals

- **A smooth gradient blob** — a blur crosses cell boundaries, so presence
  stops being a per-cell value and the perimeter loses its meaning.
- **A feathered, rounded, or anti-aliased cell edge** — a soft edge promises a
  precision the measure does not have.
- **Ghost content from another Layer** — a frame, an outline, or readable text
  from another Layer shows a person something that is not where they are
  working, and turns a hundred Layers into noise.
- **A field louder than the content it announces** — presence bright enough to
  hide a Note has replaced the thing it exists to point at; this is what
  `--field-alpha-max` is for.
- **Presence drawn over content** — the field is always beneath every
  placement, never above one.
- **A blended midpoint hue** — averaging two sources into a third colour
  invents a value that belongs to neither and erases which content is there.
- **A signal hue radiating from a placement that is not Anchored** — a hue that
  also means "someone picked this colour" has stopped being a signal.
- **Stored presence** — a persisted field would survive content it no longer
  describes and start lying about what is on the Grid.
- **A merged contour across two placements** — one contour spanning two
  footprints draws a group the field does not have.
- **Presence as a control** — the field is never hit-tested, focusable,
  selectable, or draggable.
- **A count, legend, or scale on the field** — a value read off the field
  replaces looking at what is actually there.


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

- Catalogue deck: `docs/design_catalogue/src/grid-plane/05-presence-fields.html` — the contract for this component.
- Catalogue deck: `docs/design_catalogue/src/grid-plane/01-the-grid.html`
- Catalogue deck: `docs/design_catalogue/src/grid-plane/06-distance.html`
- Catalogue deck: `docs/design_catalogue/src/grid-plane/07-selection-placement.html`
- Source note: `docs/raw/original-notes/Grove - Field ledger.txt`
- Source note: `docs/raw/original-notes/Grove - field metadata.txt`
- Source note: `docs/raw/original-notes/Grove - Layers.txt`
- Source note: `docs/raw/original-notes/Grove at a distance.txt`
- Decision: none exists for the field; every resolution below is made here.
- Wireframe: none. Presence belongs to no journey; it is a consequence of every one.
