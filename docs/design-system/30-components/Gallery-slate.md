---
type: design-system-component
status: active
date: 2026-08-09
component: Gallery
plane: hud
surface_class: slate
tags: [grove, design-system, component]
---

# Gallery

Every picture a person has brought into Grove, on one wall, each one whole and
at the shape it actually is.

## Anatomy

Every visible part, named, with the token that gives it its value. A part with
no token is a part with no contract.

| Part | Required | Value |
| --- | --- | --- |
| Frame | yes | The Slate frame of `10-grammar/Surface-classes.md`: fill `--surface-chrome`, opaque; containment edge 1px in the Slate role edge; `--r-sm`; padding `--sp-lg` on all four sides; no shadow and no rounded corner, because a Slate fills the viewport edge to edge. Type role: none. |
| Identity | yes | The word `Gallery` in `--f-display` at `--t-title-small`, `--tr-label`, uppercase, ink `--k-slate`. No fill, no edge, no padding. `--sp-lg` beneath it. |
| Facet row | yes | Three text controls on one line, `--sp-lg` apart, starting `--sp-lg` after the identity. Each is `--f-ui` at `--t-caption`, as written, no fill and no edge; resting ink `--text-meta`, chosen ink `--text-primary`. Padding `--sp-xs` beneath the label, above a 1px underline: transparent at rest, `--edge-control` when chosen. |
| Count | yes | Pushed to the right end of the header line. `--f-mono` at `--t-label`, uppercase, `--tr-label`, ink `--text-meta`. No fill, no edge, no padding. |
| Wall | yes | The masonry columns. Fill: none — the Slate's own fill shows through. No edge, no padding, no radius, no type role of its own. Gutter `--sp-sm` between columns and between the cards in a column. |
| Frame box | yes | One picture at its intrinsic proportions, filling the column width exactly. Fill: the source. Padding: none. `--r-sm`, because `00-foundations/Shape.md` names a picture frame held inside a Slate under the chrome radius. |
| Frame containment edge | yes | 1px `--edge-quiet`, drawn inset on the frame's own box so it never insets the source. This is the whole of the frame treatment. |
| Identity line | yes | Beneath the frame, `--sp-sm` below it. `--f-mono` at `--t-label`, `--lh-ui`, ink `--text-meta`, as written. Three fields separated by ` · `: the picture's own name, its kind, its date. No fill, no edge, no padding. |
| Selection outline | Selected only | `2px` solid `--signal-interaction` at full strength, `3px` outside the frame box, taking the frame's `--r-sm`. |
| Focus ring | Focused only | `--focus-ring` at `--focus-ring-offset` outside the frame box, or outside the selection outline's outer edge when both are true. |
| Anchor diamond | Anchored only | The anchor diamond of `00-foundations/Marks.md`: a `9 × 9px` square rotated 45°, filled `--signal-authored-context`, its bounding box offset `-4px` on both axes from the frame's top-left corner. |
| Unavailable frame | Only when the source will not decode | The frame box at its recorded proportions, fill `--surface-nested`, 1px `--edge-found`, `--r-sm`, padding `--sp-md`, holding one sentence in `--f-ui` at `--t-caption` ink `--text-secondary` and the Retry control beneath it. |
| Retry control | Only inside an unavailable frame | `--f-ui` at `--t-caption`, ink `--text-primary`, 1px `--edge-control`, `--r-sm`, padding `--sp-sm` `--sp-md`, no fill. |
| Empty line | Only when the wall holds nothing | `--f-ui` at `--t-lead` weight 400, ink `--text-primary`, centred in the wall's box. |
| Import action | Only when the wall holds nothing | The one primary action on the surface: `--signal-interaction` fill, label `--c-paper-ink` in `--f-ui` at `--t-caption`, `--r-sm`, padding `--sp-sm` `--sp-md`, `--sp-xl` beneath the empty line. |
| Scroll cue | Only when the wall is taller than its box | A `3px` track in `--ink` at `--ink-whisper` with a thumb in `--ink` at `--ink-quiet`, both `--r-sm`, inset `--sp-sm` from the frame's right edge. |

Two figures this component needs have no token, and both should have one
because more than one surface uses them.

- **`--wall-column-min` `220px`** — the comfortable minimum column width in a
  masonry collection. Geometry below measures it out of the decks; the Gallery
  wall and the Memory Slate's wall both consume it, so it belongs in
  `00-foundations/Tokens.md` and not here.
- **`--edge-control` — `--ink` at `--ink-tertiary`** — the boundary of a
  control a person is asked to press, and of a 1px mark that carries meaning.
  `--edge-hairline`, `--edge-quiet`, and `--edge-found` reach 1.28, 1.54, and
  1.89 on `--surface-chrome`, all below the 3:1 floor
  `00-foundations/Accessibility.md` sets for a non-text part that carries
  meaning; `--ink-tertiary` is the first step above it at 4.71.

`3px` on the scroll cue is the Slate class's own furniture, rendered at that
width by both HUD decks; it is recorded here because the wall is what scrolls,
and it belongs to the Slate class rather than to this component.

The wall draws no container around a card. There is no border, no fill, and no
padding wrapping frame plus identity line, because `Shape.md` already rules the
shipped `.gallery-card` wrapper out: in a masonry collection the item's edge is
the frame of the picture itself.

## Geometry

- **Footprint** — the Slate is fixed to the viewport, inset at least `--sp-md`
  from every viewport edge, and composed Full, Left, or Right by the Slate
  host. It occupies no cells and answers no camera.

- **Column count** — executable, from the wall's content width `W` and the
  gutter `g` = `--sp-sm`:

  ```text
  n = max(1, floor((W + g) ÷ (--wall-column-min + g)))
  columnWidth = (W − (n − 1) × g) ÷ n
  ```

  `W` is the Slate's outer width less its 1px border on each side and `--sp-lg`
  on each side. Every column is the same width; the wall reserves no extra
  padding for the scroll cue, because the cue is inset `--sp-sm` from the
  frame's outer edge and therefore already sits inside the Slate's own padding.

  Below the minimum there is one column and the frame narrows with it. The
  frame is never cropped and never boxed, because Law 9 answers the content
  and never the container.

  The decks measure `--wall-column-min` out to a single value. A full-width
  Gallery is `856px` wide and renders three columns; the Memory Slate's wall is
  `836px` wide and renders three; a half-width Gallery pane is `420px` and
  renders one. Those three counts hold for a minimum of `200`, `220`, or
  `240px` and for nothing outside that band: `180px` returns four, four, and
  two, and `260px` returns three, two, and one. Inside the band the shipped
  product already uses one value — `css/slate.css:112` sets the Memory Slate's
  masonry to `column-width:220px` — so `220px` is the design and
  `css/slate.css:136`'s `minmax(180px,1fr)` on the Gallery wall is the drift.

- **Column assignment** — executable, and it reproduces the deck exactly:

  1. Order the pictures newest first, by the Memory's arrival order.
  2. Take each in turn and append it to the column whose current height is
     smallest; on a tie take the leftmost.
  3. A card's height is `columnWidth × sourceHeight ÷ sourceWidth`, plus
     `--sp-sm`, plus one identity line at `--t-label` on `--lh-ui`.
  4. A column's height is the sum of its cards' heights plus `--sp-sm` between
     each pair.

  Run against the seven frames of `hud/03-gallery-slate.html` slide 02 at
  `W = 806`, the procedure returns `n = 3`, `columnWidth = 263.3px`, and the
  columns `Pier at low tide · Ridge line`, `Studio window · Fern detail`,
  `Harbor at dusk · Loop study · Coast road` — the deck's own three columns,
  card for card and in that order. The identity line is what makes it land: the
  sixth frame reaches the first column only because each card carries `32.5px`
  of line and gutter above the next, and a rule that ignored the line would put
  it in the third.

- **Growth** — the wall grows downward and the Slate scrolls, because a Slate
  is chrome rather than a footprint on the field. No frame shrinks, no frame is
  cropped, and no denser layout is substituted when the collection gets large.
  One picture and ten thousand use the same wall.

- **Measure** — None. The Gallery sets no column of continuous text: the
  identity line is a label, and `--measure-reading` binds reading columns.

- **Alignment** — the first column's left edge and the last column's right edge
  land on the Slate's content box. Cards align to the top of their column and
  the wall is top-aligned in its box, so a short collection sits under the
  header rather than centred in the pane. Nothing here aligns to a grid line,
  because nothing here is addressed in cells.

- **Frame proportions are known before the picture is** — a frame's box is
  computed from the source's recorded width and height, carried on the Memory
  from import, never from decoding the pixels. A picture arriving therefore
  never moves the wall.

## States

All nine, for the wall and its cards.

| State | Appearance | Notes |
| --- | --- | --- |
| Rest | The wall of complete frames, each identity line at `--text-meta`. No control, badge, or outline on any frame. | The anchor diamond is present here on a picture that carries an Anchor, because Anchored is a property and not an interaction. |
| Approached | No change from Rest. | The wall's cards carry no chrome that serves the hand, and `10-grammar/Signal-roles.md` raises no interaction signal on hover outside an open menu. The shipped `.gallery-card:hover` border change is drift. |
| Focused | `--focus-ring` drawn `--focus-ring-offset` outside the frame box; when the card is also Selected the ring is offset `--focus-ring-offset` outside the selection outline's outer edge, which puts it `7px` from the frame. | Focus is never suppressed and is never animated. Moving focus moves selection, so the ring and the outline are concentric on the same card. |
| Selected | A `2px` `--signal-interaction` outline `3px` outside the frame box, and the identity line rises from `--text-meta` to `--text-primary`. | The picture is never dimmed, tinted, scaled, re-cropped, or restyled, and no neighbour changes. The ink rise is the structural second signal that stands where the Grid's field response would be, because a Slate has no cells to brighten. |
| Engaged | Pointer-down on a card takes the identity line to `--text-primary` over `--d-press` and releases it on the same step. | The wall opens no gesture: a card is never dragged, resized, or swept, so `--signal-active-work` never appears on this surface. |
| Pending | The frame holds its exact box, filled `--surface-nested`, with its identity line beneath; the picture replaces it in place over `--d-swap`. | Never blank, never a spinner. The box is already correct because the proportions come from the recorded dimensions. |
| Refused | Not reachable on this surface. | Selecting, faceting, scrolling, and all three routes either succeed or hand off; the surface that receives a route owns its own refusal. |
| Unavailable | A source that will not decode holds its box in `--surface-nested` on 1px `--edge-found`, carrying one sentence and the Retry control. The identity line beneath is unchanged. | No refusal hue: `Signal-roles.md` ruling 2 fixes this, because a load that has not arrived is a standing condition and not the answer to an attempt. `css/slate.css:140` borders it in the refusal hue and is corrected. |
| Anchored | The `9 × 9px` diamond rotated 45° at the frame's top-left corner, filled `--signal-authored-context`. | The Gallery draws it exactly as the Grid does, because authored context is durable and a wall that hid it would report less than the Grid holds. |

Combination follows `10-grammar/States.md` without exception. Focused plus
Selected keeps both, with the ring outside the outline and a visible gap.

## Behaviour

- **Pointer** — a single click on a card selects it, committing on pointer-up
  over the card; nothing opens, nothing moves, and the scroll position does not
  change. A context gesture on a card opens that card's menu at the invoking
  point. There is no double-click gesture: the menu is the only route forward,
  and a hidden gesture has no keyboard equal. A click on the Retry control asks
  for the source again. A click on a facet narrows the wall in place.
- **Keyboard** — `V` opens the Gallery, from the Grid surface or from the
  global HUD; `docs/reference/Keybind map.md` owns it and it is not redefined
  here. Inside the wall: `↑` and `↓` move to the previous and next frame in the
  same column; `←` and `→` move to the frame in the adjacent column whose top
  edge is nearest the current frame's top edge — a decision, because the wall
  has columns and no rows, so vertical motion follows a column and horizontal
  motion crosses one. `Home` and `End` reach the first and last frame in
  arrival order. `Enter` opens the focused card's menu at the card, and the
  menu's own quick keys act from there. Focus arrives on the first facet and
  reaches the wall on the first frame.
- **Focus order** — the three facets in reading order, then the wall as a
  single stop whose arrow keys move within it, then the frames' Retry controls
  in wall order where they exist. The count is not focusable, because it is a
  figure and not a control.
- **Escape** — Escape closes this Slate pane and focus returns to the peer
  pane, or to the surface that invoked the Slate. A menu open on a card
  dismisses first. Escape does not clear selection: `Surface-classes.md` peels
  one surface per press, selection is not a surface, and `States.md` already
  makes selection transient. `docs/ux/wireframes/gallery-slate/Gallery Slate.md`
  says Escape clears selection at GS-02 and is corrected.
- **Commit and cancel** — the Gallery commits nothing. Opening it is
  side-effect free, selection is never stored, and the facet is a view of the
  collection rather than a change to it. The three routes hand off; each
  destination owns its own commit, and closing one returns to this wall at the
  same scroll position with the same card selected.

Opening a picture at full size, revealing a placement on the Grid, and
reading a Memory's record belong to the surfaces that own them and to
`docs/ux/wireframes/gallery-slate/Gallery Slate.md`. Import belongs to
`docs/ux/wireframes/Import and place Images.md`.

## Motion

| Transition | Duration | Curve | Reduced motion |
| --- | --- | --- | --- |
| Pointer-down acknowledgement on a card | `--d-press` | `--ease` | The identity line at `--text-primary` on press and back on release, with no scaling. |
| The pending frame exchanged for the decoded picture | `--d-swap` | `--ease` | The picture in place of the fill on the same frame, at the same threshold, with no cross-fade. |
| The unavailable frame exchanged for the picture a retry produced | `--d-swap` | `--ease` | The picture in place on the frame the retry resolves. |
| A card's menu opening and dismissing | `--d-fade` | `--ease` | Present or absent at the identical threshold. |
| Playback of an animated source | The source's own timing | The source's own timing | One complete still frame at the same box; playback is offered where the picture opens, never as a control on the frame. |
| Selection outline, focus ring, facet underline | None | None | No change. |
| Column reflow when the pane's width changes | None | None | No change. |

The wall never staggers an entrance, never reveals on scroll, and never
animates a layout property — `00-foundations/Motion.md` forbids all three, and
a reflow is a width change rather than a transition. Nothing loops. Nothing
idles. Nothing pulses or blinks. Scrolling is continuous rendering and takes no
duration token.

## Distance

| Tier | Sheds | Keeps |
| --- | --- | --- |
| Working | Nothing. | Everything. |
| Stepped | Nothing. | Everything. |
| Stand-in | Nothing. | Everything. |

The representation tiers are a Grid axis, measured on projected cell size. This
Slate is fixed to the viewport, holds no Grid position, and the camera never
reaches it, so no tier applies and nothing sheds at any camera scale. A picture
that is also placed keeps whatever form the camera gives it on the Grid;
`30-components/Picture.md` owns that, and the wall draws the same source at the
column width whatever the Grid is doing.

Presence, position, and extent are the Grid's facts and the Gallery
neither casts nor changes them. Opening the Gallery moves no camera, lights no
cell, and selects nothing on the Grid.

## Accessibility

- **Role and name** — the wall is a single-select list of options and each card
  is one option, carrying `aria-selected`. The wall takes its accessible name
  from the header by reference, so the surface names itself once and no second
  string is authored. A card's accessible name is the picture's own name and
  its kind, in that order — `Harbor at dusk, JPG` — and its date is exposed as
  the option's description, because a name distinguishes and a date does not. A
  generic name such as `Image` is a defect.
- **Contrast** — on `--surface-chrome`: the identity line and the count at
  `--text-meta` 4.71:1; a resting facet at `--text-meta` 4.71:1; a chosen facet
  and the empty line at `--text-primary` 10.36:1; the selection outline and the
  focus ring at `--signal-interaction` 8.90:1; the chosen facet's underline at
  `--edge-control` 4.71:1. On `--surface-nested`: the unavailable sentence at
  `--text-secondary` 6.19:1 and the Retry control's border at `--edge-control`
  4.61:1. The Import action's `--c-paper-ink` label on its `--signal-interaction`
  fill reaches 8.57:1. The frame's 1px `--edge-quiet` edge is exempt from the
  3:1 floor because it carries no meaning: the picture's own pixels state its
  extent. The unavailable frame's `--edge-found` is exempt for the same reason —
  the state is carried by the sentence and the Retry, and the frame's tonal step
  states its extent.
- **Without colour** — Approached: nothing to read, because nothing changes.
  Focused: a ring outside the frame at a distinct offset. Selected: an outline
  outside the frame at a `3px` gap, plus the identity line's ink rise. Engaged:
  the same ink rise, taken and released with the press. Pending: the frame's
  box, held at its true proportions. Unavailable: a tonal step, a sentence, and
  a control where the picture was. Anchored: the rotated square's silhouette.
  Rest and Refused carry no hue. The chosen facet is carried by its underline as
  well as by its ink, and the underline occupies its 1px at rest in
  `transparent`, so choosing one never moves the row.
- **Forced colours** — redeclared in system colours: the Slate fill and its
  border, the header ink, the facet underline, the identity line, the selection
  outline, the focus ring, the anchor diamond's fill, the unavailable frame's
  fill and border, and the Import action's fill and label. What survives without
  redeclaration is the structural half of every state: the ring's offset, the
  outline's offset, the underline's presence, the diamond's silhouette, and the
  words. Pictures are rasters and survive unchanged.
- **Text scaling** — interface text scales to 200% and the wall answers by
  growing, never by clipping. The identity line breaks at its ` · ` separators
  onto further lines and never inside a field, so no monospace label wraps; the
  picture's own name is authored content and wraps when it must, because Law 9
  refuses to truncate it and a law outranks a type rule. Taller identity lines
  make cards taller and the assignment re-solves. The header wraps the facet row
  below the identity before anything truncates, and nothing truncates at any
  scale.
- **Reduced motion** — as the Motion table. An animated source opens on a
  complete still frame; playback is offered where the picture opens, because the
  wall parks no control on a frame.

## Copy

Every user-visible string this component can show.

| String | Where | Why it passes |
| --- | --- | --- |
| `Gallery` | The header identity. | A product noun `10-grammar/Copy.md` names, and the surface's own name. |
| `All` | The first facet. | Ordinary English naming what is shown. |
| `GIFs` | The second facet. | The kind a person would say, in the plural. |
| `Placed` | The third facet. | Ordinary English naming where those pictures are, not how Grove stores them. |
| `128 Images` | The count, as `{n} Images` — `{n} GIFs` while the GIFs facet is chosen, because the count names the kind it counted. | A figure at `--text-meta`, which `Signal-roles.md` fixes as the answer for counts; `Image` is a product noun. |
| `Open picture` | The first menu row, alone above the group separator. | `Copy.md` fixes this exact string as the correction for `Open Image Viewer`. |
| `Show on the Grid` | The second menu row. | Names the act; it would still be true after any rebuild. |
| `Open Memory` | The third menu row. | A product noun and a verb; it replaces the deck's `Open record`, which names a mechanism. |
| `No Images yet` | The empty wall's one line. | Names what is absent, in the product's own noun, with no apology and no explanation. |
| `Import Images` | The empty wall's one action. | A verb for the one step that changes the absence. |
| `Nothing here matches GIFs.` | The line shown when a facet holds nothing, naming the chosen facet. | A full sentence about the world, taking a full stop, and it does not blame. |
| `Show all` | The quiet action beside that line. | Names the one way back; the facet never silently resets itself. |
| `This Image can't be shown right now.` | Inside an unavailable frame. | States the condition rather than the failure, and names nothing the person did. |
| `Retry` | The control beneath that sentence. | A verb for an action, and the word `Signal-roles.md` already uses for this state. |

The identity line's first field is the picture's own name and is authored
content, not copy: Grove never shortens it, re-cases it, generates it from a
file name, or adds a word to it. Its second and third fields are the source's
kind — `JPG`, `PNG`, `GIF` — and its date as a three-letter month and a day,
`Jun 02`, set in monospace so the figures stay tabular as the wall scrolls.

The menu's quick keys are `O`, `G`, and `M`, one unmodified key per row, bound
only while that menu is open.

## Refusals

- **A uniform tile grid** — cutting every frame to one shape throws away the
  sides of a picture, and the wall exists to show what was made.
- **A fixed-height frame box** — a box that every frame is fitted into pads
  panoramas and portraits with dead bars; in the wall the frame sets its own
  height and there is no box to fill.
- **A card container** — a border, a fill, and padding around frame plus
  identity line turns a collection into a stack of cards, and the item's edge is
  the frame of the picture itself.
- **Metadata over a frame** — a wash and a caption across the foot of a picture
  restyle it to hold text; details live off the frame.
- **Selection in the Slate hue, or any role hue** — identity and state are
  separate axes, and a surface that says "chosen" in its own identity colour has
  no colour left for what a person is working on.
- **Dimming anything** — no card is dimmed to emphasise another, no pane is
  dimmed to emphasise its peer, and nothing behind the Slate is dimmed at all.
- **A control on a frame that holds a picture** — a button riding on a picture
  makes chrome the first thing that reads; the Retry control appears only where
  there is no picture to cover.
- **A destructive row in a card's menu** — the Gallery browses and routes, and a
  wall that can permanently remove authored work turns looking into a risk.
- **A count, badge, or ordinal on a frame** — a figure on the picture is a
  report about the picture, and selection is already visible on every selected
  card.
- **A dashed empty box** — a dashed edge means provisional, so an empty
  collection carries words and one next step instead.
- **An entrance stagger or a reveal on scroll** — motion that arrives
  unrequested to point at content the person is already looking at.
- **A group, cluster, or order Grove inferred** — the wall is arrival order,
  newest first, and proximity on it means nothing a person did not do.


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

- Catalogue deck: `docs/design_catalogue/src/hud/03-gallery-slate.html` — the contract for this component: wall anatomy, selection, routes, composed panes, edge states, and four refusals.
- Catalogue deck: `docs/design_catalogue/src/hud/02-memory-slate.html` — the same masonry geometry, the header at `--t-title-small` on `--tr-label`, the quiet facet row, and the no-results and unavailable patterns.
- Catalogue deck: `docs/design_catalogue/src/hud/01-slate-anatomy.html` — the Slate frame this surface is built on.
- Wireframe: `docs/ux/wireframes/gallery-slate/Gallery Slate.md` — GS-00 to GS-05, and the rule that selection and routing are separate visible steps.
- Reference: `docs/reference/Keybind map.md` — `V` opens the Gallery.
