---
type: design-system-component
status: active
date: 2026-08-09
component: Picture
plane: grid
surface_class: placement
tags: [grove, design-system, component]
---

# Picture

A picture a person placed, shown whole, at the size and shape it actually is.

## Anatomy

Every visible part, named, with the token that gives it its value. A part with
no token is a part with no contract.

| Part | Required | Value |
| --- | --- | --- |
| Frame | yes | The complete source at its intrinsic proportions, filling the footprint exactly. Fill: none — the source is the fill. Padding: none. Radius `--r-none`. Type role: none. |
| Containment edge | yes | 1px `--edge-quiet`, drawn on the frame's own box so it never insets the source. This is the entire frame treatment. |
| Kind badge | Only when the source animates | The kind badge of `00-foundations/Marks.md`, carrying the word `GIF`, at the frame's top-right corner inset `--sp-sm` on both axes. |
| Anchor diamond | Only when anchored | The anchor diamond of `Marks.md`: a `9 × 9px` square rotated 45°, filled `--signal-authored-context`, its bounding box offset `-4px` on both axes from the frame's top-left corner. |
| Resize corner | Approached and Selected only | The resize corner of `Marks.md`, drawn in `--ink-primary` because the mark sits over arbitrary photography and the dark ramp is the value that reads there. |
| Selection outline | Selected only | `2px` `--signal-interaction`, offset `3px` outside the frame edge. |
| Presence | yes | The occupied cells and their falloff, in `--ink` at the field alphas; `--signal-authored-context` instead when the picture is anchored, because a picture has no authored fill of its own and the field never invents a hue for it. |

The frame has no fill, no padding, no inset, and no type role: the source is the
whole of it. Every figure above is a token or a geometry `Marks.md` already
owns, so this anatomy introduces no component value. The one component value
Picture owns is the source-pixels-per-cell divisor, and it lives in Geometry
because it decides extent rather than appearance.

Deck 01 draws a placed picture as a paper mat holding a grey area and a caption
row reading `Image` and `1×1`; deck 04 draws the complete frame with a quiet
1px edge and nothing else. **Deck 04 wins and deck 01 is corrected**, because a
mat and a caption make chrome the first thing that reads on a frame whose
content is the picture, and `10-grammar/Copy.md` already names `Image` as a
caption under a picture as a violation.

## Geometry

- **Footprint** — a whole-cell rectangle whose proportions are the source's own.
  Run this and it returns the same answer twice:

  1. Read the source's intrinsic width and height in pixels; call the larger
     `long` and the smaller `short`.
  2. `L = max(1, ceil(long ÷ 256))` — cells on the long axis.
  3. `S = max(1, round(L × short ÷ long))` — cells on the short axis.
  4. The footprint is `L` cells on the source's long axis and `S` cells on its
     short axis, in the source's own orientation.

  `256` is a component value and not a token: it is the only figure in Grove
  that converts a raster measure into cells, and `src/domain/image-footprint.ts`
  already resolves against it.

  The deck's four aspect families are the test cases, and the procedure returns
  each of them exactly:

  | Source | Footprint | Deck evidence |
  | --- | --- | --- |
  | `1200 × 1700` portrait | 5 × 7 | `04-image.html`, placed form, `300 × 420` at a `60px` cell |
  | `700 × 900` portrait | 3 × 4 | `04-image.html`, aspect families, `120 × 160` at a `40px` cell |
  | `900 × 700` landscape | 4 × 3 | `04-image.html`, aspect families, `160 × 120` at a `40px` cell |
  | `700 × 700` square | 3 × 3 | `04-image.html`, aspect families, `120 × 120` at a `40px` cell |
  | `2000 × 500` panorama | 8 × 2 | `04-image.html`, aspect families, `320 × 80` at a `40px` cell |

  A square footprint is a result the procedure can return, never a shape it is
  given. Step 3 is what forbids the letterbox: a proportion that will not sit in
  a footprint changes the footprint, and never the frame.

- **Growth** — a picture cannot outgrow its footprint, because the footprint was
  derived from the source and the source does not change. The only thing that
  changes extent is a resize, and a resize moves the long axis by whole cells
  while step 3 recomputes the short axis, so the proportion survives every size
  and no bar is ever introduced.
- **Measure** — None. The frame sets no type; the only word it can carry is the
  kind badge, and that is a mark.
- **Alignment** — all four footprint edges land on major grid lines, and the
  source's four edges are those same edges, because nothing sits between the
  picture and the line.

## States

All nine. `—` is not an answer; write "No change from Rest" where that is
true.

| State | Appearance | Notes |
| --- | --- | --- |
| Rest | The complete picture on its exact cells, with the 1px `--edge-quiet` edge. The kind badge if the source animates, the anchor diamond if it is anchored, and nothing else. | The frame carries no control, no caption, and no handle at rest. |
| Approached | The resize corner fades in at the bottom-right over `--d-fade`. | The picture is untouched; approach adds a mark outside the content and restyles nothing. |
| Focused | No change from Rest. | Keyboard attention on the Grid is the Grid cursor at the focused cell; the frame draws no second ring. |
| Selected | A `2px` `--signal-interaction` outline offset `3px` outside the frame edge, the cells around the footprint brightened hard-edged, and the resize corner present. | The picture is never tinted, dimmed, washed, or restyled to say it is selected. |
| Engaged | The moved or resized footprint previews in `--signal-interaction`; the picture keeps its place until release. | A move and a resize both have a subject, so neither is drawn in `--signal-active-work`. |
| Pending | The fullest form already decoded holds the exact footprint — the stand-in when nothing else exists yet. | Never blank, never a spinner; the arriving form replaces it in place over `--d-swap`. |
| Refused | The previewed footprint takes the refusal inset edge and the cells that cannot accept it hatch at 45°. | The sentence sits in the strip the Grid raises beside the footprint; the frame contributes no words. |
| Unavailable | The footprint is held by the figure stand-in in `--text-unavailable` on a `--edge-hairline` border, with no refusal hue. | Reached when the source is permanently unreadable; nothing was refused, so nothing is drawn in the refusal role. The route that retries is the one that opens the picture, because the Grid parks no control on the field. |
| Anchored | The `9 × 9px` diamond at the top-left corner, and the presence the picture casts in `--signal-authored-context`. | The picture reaches every edge, so it takes the diamond and never the ribbon. |

Deck 04 draws its selected frame without a resize corner. `Marks.md` gives
Selected the resize corner and **`Marks.md` wins**, because precedence runs
downward and a rendering is evidence rather than authority.

## Behaviour

- **Pointer** — left click selects, committing on release inside the frame.
  Press-and-drag moves the placement and commits on release; the source keeps
  its cells until then. Dragging the resize corner resizes and commits on
  release. Double-click opens the picture at full size — a decision, because
  desktop convention opens content on a double-click and the frame carries no
  control that could offer a second route.
- **Keyboard** — none of its own. Every key that reaches a selected picture is a
  Grid surface binding in `docs/reference/Keybind map.md`: `R` for Resize, `A`
  for the Anchor editor, the clipboard and transfer bindings, `Delete` and
  `Backspace` for removal, and `Escape` for cancel. Keyboard attention arrives
  as the Grid cursor at the footprint's top-left cell and leaves to the
  neighbouring cell in the direction pressed.
- **Focus order** — the frame exposes no tab stop and has no focusable parts.
  The resize corner is never in the tab order; resize from the keyboard is `R`.
- **Escape** — there is nothing on the frame to dismiss. Escape cancels an open
  move or resize and restores the footprint that existed before it, and focus
  stays with the Grid cursor.
- **Commit and cancel** — a move or a resize is durable on release, or on
  leaving Resize; Escape during either restores the previous footprint at no
  cost, because an open gesture changes nothing durable. Import is durable when
  the placement lands.

Import, refusal of a placement, and opening the picture at full size belong to
`docs/ux/wireframes/Import and place Images.md` and to
`20-planes/Information-plane.md`; they are named here and not restated.

## Motion

| Transition | Duration | Curve | Reduced motion |
| --- | --- | --- | --- |
| Arrival at its footprint | `--d-place` | `--overshoot` | Present at full extent at the identical threshold. |
| Resize corner in and out | `--d-fade` | `--ease` | Present or absent at the identical threshold. |
| Representation exchange | `--d-swap` | `--ease` | Immediate swap at the identical threshold. |
| Removal | `--d-exit` | `--ease` | Gone at the identical threshold. |
| Selection outline | None | None | No change. |
| Playback of an animated source | The source's own timing | The source's own timing | A complete still frame at the same footprint, same size, same place. |

Nothing loops. Nothing idles. Nothing pulses or blinks. An animated source's own
motion is the one exception and it is not Grove's: it is content a person
placed, it plays only inside the frame, and the cells around it are still.
Playback runs only in the working form, because below it there is no picture
left to play. Play and pause are transient and are never written with the
picture.

## Distance

How the component reads at each representation tier. Thresholds are on
projected cell size and belong to `10-grammar/Representation-tiers.md`; this
section says only what this component sheds and what it keeps.

| Tier | Sheds | Keeps |
| --- | --- | --- |
| Working | Nothing. The resize corner leaves at `--tier-detail-promote`, ahead of the tier itself. | The complete source, the 1px edge, the kind badge, the anchor diamond, the exact cells. |
| Stepped | The source's detail and the kind badge; playback stops. | The picture's own plate — its dominant tone as one flat fill across the exact footprint — the 1px edge, the anchor diamond, the exact cells. |
| Stand-in | The plate's role as the whole frame; the source is no longer drawn. | A `--surface-page` block on the exact footprint, an inset 1px `--paper-border` edge, the figure mark, the anchor diamond, presence. |

Presence, position, and extent are never shed.

The stand-in's kind-coded form is the **figure mark**, and it is exact: an inner
area inset `9%` from the left, right and top edges and `17%` from the bottom,
filled `--paper-ink` at `--paper-edge`; inside that area a ridge polygon and a
single disc, both `--paper-ink` at `--paper-border`. The insets are percentages
so the mark holds its proportions on a 3 × 4 footprint and on an 8 × 2 one
alike. Decks 04 and 06 draw the ridge in `--c-view` and the disc in
`--c-marquee`; both are corrected to the paper ramp, because
`10-grammar/Signal-roles.md` already rules that a stand-in glyph carries no
signal hue and no role hue.

`Representation-tiers.md` names the Image stand-in's fill "its own plate" and
its edge `--paper-border`, and decks 04 and 06 fill it `--surface-page`. **The
decks win**: `--paper-border` is an alpha over paper ink and reads only on a
light fill, so the grammar's own edge value fixes the fill. At this tier a
picture and a Document are told apart by their marks — a figure against a ruled
head bar — which is the kind coding the grammar requires.

## Accessibility

- **Role and name** — the placement is a selectable object inside the
  Grid's list of placed content, so it takes `role="option"` with
  `aria-selected`. Its accessible name is the source name followed by the kind:
  `sunrise-ridge.jpg, Image`. The kind word is legal here and illegal as a
  caption, because a caption rides on the frame and a name replaces nothing.
- **Contrast** — the frame carries one text part, the kind badge word, at
  `--ink-primary` over the badge's own canvas fill: 10.83:1. Non-text: the
  selection outline 9.50:1, the anchor diamond 6.79:1, and the refusal hatch
  5.63:1, all on canvas. The 1px `--edge-quiet` edge is exempt from the 3:1
  minimum because it is decorative: the picture's own pixels establish its
  extent and its cells are the contract, so a person locates the frame by the
  picture and never by the line.
- **Without colour** — Selected: the outline's `3px` offset and the brightened
  cells. Engaged: the previewed rectangle. Refused: 45° hatching and one plain
  sentence. Anchored: the rotated square's silhouette. Unavailable: the retained
  footprint and the hairline border. Pending: the stand-in holding position and
  extent. Approached: a mark that was absent. Focused: the Grid cursor's
  position.
- **Forced colours** — the source is a raster and survives. Redeclared in system
  colours: the containment edge, the selection outline, the anchor diamond, the
  kind badge's fill and border, and the refusal hatch. Presence is canvas paint
  and does not survive, so the occupied region falls back to a system-coloured
  border.
- **Text scaling** — nothing on the frame scales with interface text. The
  picture is measured in cells and scales with the camera; the kind badge is a
  mark at fixed pixel geometry. Nothing reflows and nothing truncates, because
  the frame holds no text to reflow.
- **Reduced motion** — no difference from the Motion table.

## Copy

| String | Where |
| --- | --- |
| `GIF` | The kind badge, and only when the source animates. |

That is every string a picture can show. The word is the kind badge's own, fixed
by `Marks.md`, and it passes `Copy.md` because it names what a person is looking
at rather than how Grove holds it. A second animated format enters through
`Marks.md`, not through this component.

The frame shows no file name, no dimensions, no footprint size, no format, no
byte count, and no title. `sunrise-ridge.jpg, Image` is an accessible name and
is never drawn.

## Refusals

- **Cover crop** — filling a footprint by throwing away the sides of a picture
  makes the Grid lie about what was placed.
- **Letterbox bars** — a bar above or beside a picture is footprint the picture
  does not occupy, and the footprint changes shape instead.
- **Square-forced footprint** — a square footprint that was not derived from the
  source can only be reached by cropping or by letterboxing, and both are
  refused.
- **Caption overlay** — a name-and-size bar across the foot of the frame covers
  the picture with a report about the picture.
- **Decorated card** — a paper mat, a radius, a drop shadow, a rotation, or an
  italic caption dresses a picture as a keepsake and turns the field into a card
  layout.
- **A control on the picture** — a button riding on the frame makes chrome the
  first thing that reads and has no keyboard path of its own.
- **A second mark** — a picture takes the diamond and never the ribbon, and
  never both.
- **Re-encoding to fit** — resolving a footprint never resamples, recompresses,
  or rewrites the source bytes.
- **A filename card at distance** — a stand-in states what the thing is, never
  what file it came from.
- **A scale change on press or on selection** — no state alters a placement's
  footprint, position, or extent.


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

- Catalogue deck: `docs/design_catalogue/src/grid-plane/04-image.html` — the frame contract, the aspect families, the animated-source badge, selection, the anchor diamond, distance, and the three refusals.
- Catalogue deck: `docs/design_catalogue/src/grid-plane/01-the-grid.html` — corrected: its mat-and-caption frame is superseded by deck 04.
- Catalogue deck: `docs/design_catalogue/src/grid-plane/06-distance.html` — the figure stand-in and the shedding order.
- Catalogue deck: `docs/design_catalogue/src/grid-plane/07-selection-placement.html` — the selection outline at `3px` offset and the refusal drawing.
- Source note: `docs/raw/original-notes/Grove - controlling content.txt`
- Source note: `docs/raw/original-notes/Grove at a distance.txt`
- Decision: `docs/decisions/Camera zoom range and representation.md`
- Wireframe: `docs/ux/wireframes/Import and place Images.md`
