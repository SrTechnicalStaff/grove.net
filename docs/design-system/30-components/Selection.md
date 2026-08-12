---
type: design-system-component
status: active
date: 2026-08-09
component: Selection
plane: grid
surface_class: placement
tags: [grove, design-system, component]
---

# Selection

What a person is working on right now, drawn entirely outside the thing itself,
together with the rectangle they drag across the Grid to pick things out.

## Anatomy

| Part | Required | Value |
| --- | --- | --- |
| Outline | yes | `2px` solid `--signal-interaction` at full strength, drawn `3px` outside the content edge. |
| Field ring | yes | Lit cells one cell deep around the footprint, in `--c-select`, hard-edged, at `0.13` on a cell that shares an edge with the footprint and `0.06` on a cell that meets it only at a corner. |
| Region perimeter | no — only where the footprint sits in an occupied field region | `--field-perimeter-width` at `--field-perimeter-selected`, in the region's own hue. |
| Content surface | yes | Unchanged. No fill, tint, wash, dim, border, radius, or opacity is applied to the placement. |
| Resize corner | yes | The mark `00-foundations/Marks.md` specifies, at `--ink-primary` on a dark form and `--paper-strong` on paper. |
| Sweep rectangle | no — only while a sweep is open | `1.5px` solid `--signal-active-work` over a fill of `--c-marquee` at `0.08`. |

The outline is the containment edge and it contains nothing: it is offset clear
of the placement so the gap between them is what reads, and it is the only part
of this component that is a line. No part carries a fill over content, no part
takes padding, and no part sets type, because a selection that carried a word
would be a count and `10-grammar/Signal-roles.md` forbids one.

`3px` is this component's own figure and not a token. The offset is the second
carrier of the state, so it belongs to the drawing rather than to the spacing
scale, which governs rhythm between things. `10-grammar/States.md` fixes it, and
deck 07, the image deck, and the distance deck all render it; deck 01, deck 02,
and deck 03 render `outline-offset: 0`, and they are corrected, because an
outline flush to the edge reads as a border the placement owns and loses the gap
the state depends on.

`0.13` and `0.06` are this component's own figures because no other component
draws a ring around a footprint. Both sit inside `--field-alpha-min` and
`--field-alpha-max`, so the ring never outshines content. The corner-only cell
takes the lower value because it touches the footprint at a point rather than
along an edge. Slide 02 of deck 07 renders both; slide 03 of the same deck
renders `0.11` and omits the corner cells, and slide 02 wins, because it is the
slide whose subject is the anatomy.

The outline is drawn at full-strength `--signal-interaction`. Deck 01, deck 02,
deck 03, and `css/materials.css` draw it at `0.9`; they are corrected, because
`10-grammar/Signal-roles.md` gives each signal exactly one value and a dimmed
variant would read as a weaker selection.

No glow, halo, or drop shadow is drawn on a selected placement. Deck 01, deck
02, deck 03, and `css/materials.css` add `0 0 24px 6px` in the interaction hue;
all four are corrected, because `00-foundations/Tokens.md` declares two shadows
and states that a glow is neither.

The region perimeter rises in alpha and never changes hue or width. Deck 02 and
deck 03 draw the selected perimeter at `0.30` in the interaction hue; they are
corrected, because the token table sets `--field-perimeter-selected` and
`00-foundations/Marks.md` fixes the perimeter in the region's own hue so a
selection accents a region without repainting it.

The outline is drawn above every placement, so a selected placement beside a
neighbour is never clipped by it; the state has to stay readable exactly where
placements sit closest.

## Geometry

- **Footprint** — selection has none of its own and changes none. The lit ring
  is solved by procedure: take the set of cells the placement occupies, then
  light every cell outside that set which shares an edge or a corner with a cell
  inside it; edge-sharers take `0.13` and corner-only-sharers take `0.06`. Where
  the rings of two selected placements overlap, the cell takes the higher of the
  two values and never their sum, because a summed cell would encode how many
  things are selected and that is a count.
- **Growth** — the footprint grows and the ring re-solves around it on the same
  frame. Selection never grows, shrinks, or moves anything.
- **Measure** — none. No part of this component sets type.
- **Alignment** — every lit cell has its four edges on major grid lines. The
  outline is the one part not on a line: it sits `3px` outside the content edge
  in screen pixels, and it holds `2px` and `3px` at every camera scale, exactly
  as the anchor marks and the perimeter ring hold theirs. That is not
  counter-scaling, because the outline claims no footprint and never changes
  which cells the placement covers; the distance deck renders it unscaled on a
  stand-in.

## States

| State | Appearance | Notes |
| --- | --- | --- |
| Rest | Nothing drawn. | An unselected placement is only itself. |
| Approached | No change from Selected, plus the resize corner over `--d-fade`. | Approach adds; it never restyles what is already there. |
| Focused | No change from Selected. | On the Grid keyboard attention is the Grid cursor, so a selected placement draws no second ring and the focus-versus-selection gap question never arises here. |
| Selected | Outline, field ring, untouched content surface, resize corner. | The three carriers together, so the state survives greyscale. |
| Engaged | While a sweep is open, a placement the rectangle wholly covers draws the full Selected appearance live; while a selected placement is moved or resized, that drawing travels with the preview in the interaction role. | `--signal-active-work` belongs to the rectangle and never touches a placement. |
| Pending | No change from Selected. | A stand-in wears the same outline and the same ring as a working form. |
| Refused | The refusal drawing replaces the selection outline on the refused footprint; the field ring is not drawn on hatched cells. | `10-grammar/States.md` puts Refused above Selected in precedence, and two edges on one region would ask a person which one answers. |
| Unavailable | No change from Selected. | The outline never dims, because a dimmed signal would read as a weaker selection. |
| Anchored | Outline unchanged; the field ring and the region perimeter take `--signal-authored-context` in place of the interaction hue. | The authored-context hue wins on the field wherever it is present, so a mixed cell still says a person wrote something down. |

## Behaviour

- **Pointer** — a click on a placement replaces the selection with that
  placement, committing on release. `Shift`-click adds a placement to the
  selection or removes it if it is already in. A click on empty field clears the
  selection; `Shift`-click on empty field leaves it alone. Press and drag from
  empty field opens a sweep: the rectangle appears on the first pointer move
  after the button goes down, follows the pointer exactly at any angle of
  travel, and is removed on the release frame. `Shift` held as the sweep opens
  adds what it catches to the standing selection and `Alt` removes it; the
  source notes name shift and alt as the bulk-selection technique, so both are
  canon rather than a convention borrowed here. Press and drag from a placement
  is a move and draws in the interaction role, never in amber.
- **Keyboard** — `Ctrl/Cmd+A` selects all Content on the current Layer.
  `Escape` cancels an open sweep and restores the selection that existed before
  it, and with no gesture open it clears the selection. `Delete` and
  `Backspace` remove selected Content. `A`, `T`, `M`, `R`, `1`, `2`–`4`,
  `Ctrl/Cmd+C`, `Ctrl/Cmd+X`, and `Ctrl/Cmd+D` all act on the selection;
  `docs/reference/Keybind map.md` owns every one of them and none is restated
  here. Keyboard attention on the Grid is the Grid cursor, and selecting
  neither moves it nor takes focus from it.
- **Focus order** — none. No part of this component is focusable, because a
  selection outline a person could tab to would be a control, and
  `00-foundations/Marks.md` refuses a mark that is one.
- **Escape** — with a sweep open, the rectangle is removed and the previous
  selection returns at no cost; with nothing open, the selection clears.
  Attention stays with the Grid cursor at its last cell either way.
- **Commit and cancel** — there is nothing to commit and nothing to undo.
  Selection lives only for as long as the work does: it is never written to a
  Memory, a Placement, or a Content payload, and after a reload nothing is
  selected. Traversing to another Layer clears it, because a selection names
  Content on the Layer being worked on, and an undo or redo clears it, because
  the placements it named may no longer be the placements that exist.

The moment a sweep catches a placement, and the order in which a person moves
from selecting to moving, transferring, or removing, belong to the owning
wireframes and are named rather than restated here.

## Motion

| Transition | Duration | Curve | Reduced motion |
| --- | --- | --- | --- |
| Outline and field ring appearing or leaving | none — drawn on the frame the selection changes | — | Identical; there is no transition to remove. |
| Resize corner appearing on Selected | `--d-fade` | `--ease` | Immediate appearance at the identical threshold. |
| Sweep rectangle appearing | none — drawn on the first frame the pointer moves | — | Identical. |
| Sweep rectangle leaving on release | none — removed on the release frame | — | Identical. |

The outline takes no fade because a sweep changes the selection on every cell it
crosses, and a fade would leave the answer behind the hand. The rectangle is
removed rather than faded, because a fading rectangle reads as a gesture still
open.

Nothing loops. Nothing idles. Nothing pulses or blinks.

## Distance

| Tier | Sheds | Keeps |
| --- | --- | --- |
| Working | Nothing. | Outline, field ring, region perimeter, resize corner. |
| Stepped | The resize corner, which goes with all hand chrome once projected cell size falls below `--tier-detail-promote`. | Outline at `2px` and `3px`, field ring, region perimeter. |
| Stand-in | Nothing further. | Outline at `2px` and `3px`, field ring, region perimeter. |

Selection changes nothing about which tier a placement holds, and no tier
changes what selection draws. A stand-in is the placement, so it wears the same
outline and the same ring as a working form; the distance deck renders exactly
that.

## Accessibility

- **Role and name** — selection is a property of a placement, not a component
  with a role of its own. Each placement is an option inside a multi-selectable
  list and reports its selection through `aria-selected`; its accessible name is
  the content a person authored, and no part of this component contributes to
  that name. Selection changes announce nothing: the drawing is the answer, and
  a live region repeating it would narrate the pointer.
- **Contrast** — no part of this component sets text. The outline carries the
  non-text requirement at `9.50` against canvas and `8.90` against chrome, well
  clear of `3:1`. The field ring is structure rather than an edge a person must
  find; its `0.13` and `0.06` are capped by `--field-alpha-max` precisely so the
  field never competes with content, and the outline carries the ratio for both.
- **Without colour** — the offset gap between the placement and a line that is
  not part of it, and the lit cells around the footprint, both survive
  greyscale. The sweep is carried by the rectangle's shape, which no placement,
  no chrome, and no other state ever draws.
- **Forced colours** — the outline is redeclared in a system highlight colour
  and the sweep rectangle in a system border colour. The field ring is canvas
  paint and does not survive, so the occupied region falls back to a
  system-coloured border, which is why the outline rather than the ring is the
  part that must be redeclared.
- **Text scaling** — nothing reflows and nothing grows. This component sets no
  type, and the outline and ring are measured in screen pixels and cells, so
  raising interface text leaves every selected placement exactly where and as
  large as it was.
- **Reduced motion** — no change from the Motion table.

## Copy

None.

No part of this component carries a word, a number, or a label. The count the
build parks on the Grid is refused below, and `10-grammar/Copy.md` already
records its correction.

## Refusals

- **A fill wash over the body** — a tint on the content restyles what a person
  wrote in order to say something about the pointer, hides an authored Note
  colour, and leaves a second selected placement nothing to look different by.
- **A selection count on the Grid** — a figure parked over the field says
  in a number what every selected placement already says in place, and it is
  chrome that never leaves.
- **A badge, ordinal, or marker on a selected placement** — many selected
  placements wear one grammar, and ranking them turns selection into a list.
- **A glow, halo, or drop shadow on a selected placement** — Grove has two
  shadows and both separate a surface; a glow claims a height the Grid does
  not have.
- **An outline drawn inside or flush to the content edge** — the gap is the
  second carrier, and without it the state depends on hue alone.
- **Amber on a placement, or amber anywhere after release** — the active-work
  signal says a hand is moving and nothing else, so a result drawn in it would
  make a person read a colour to learn whether their gesture had ended.
- **A sweep that catches a partly covered placement** — the live answer during
  the sweep must be the true answer, and partial cover would make release a
  guess.
- **A sweep rectangle snapped to cells** — the rectangle is a gesture, not a
  footprint, and quantizing it makes the hand feel like it is dragging content.
- **Dimming, blurring, or restyling unselected placements** — the Grid
  stays lit, and a selection that darkens everything else is a scrim drawn one
  placement at a time.
- **Selection written to a Memory, a Placement, or a Content payload** —
  interaction state that survives a reload stops being about what a person is
  doing now.
- **Selection pinning a representation tier** — distance and state are separate
  axes, and a selected placement that refuses to shed reports a distance it is
  not at.


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

- Catalogue deck: `docs/design_catalogue/src/grid-plane/07-selection-placement.html`
- Catalogue deck: `docs/design_catalogue/src/grid-plane/04-image.html`
- Catalogue deck: `docs/design_catalogue/src/grid-plane/06-distance.html`
- Catalogue deck: `docs/design_catalogue/src/grid-plane/01-the-grid.html`
- Catalogue deck: `docs/design_catalogue/src/grid-plane/02-note.html`
- Catalogue deck: `docs/design_catalogue/src/grid-plane/03-document.html`
- Source note: `docs/raw/original-notes/Grove - controlling content.txt`
- Reference: `docs/reference/Keybind map.md`
