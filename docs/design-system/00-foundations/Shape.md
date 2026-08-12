---
type: design-system-foundation
status: active
date: 2026-08-09
tags: [grove, design-system, shape]
---

# Shape and edges

Grove is made of rectangles. Two radii cover everything a person sees, one
scoped exception exists for marks that denote a point, and an edge is drawn
only when it contains something, carries attention, or refuses an action.

## The corner language

| Token | Applies to | Never applies to |
| --- | --- | --- |
| `--r-none` | Everything measured in cells: placement footprints, lit field cells, the field perimeter, the cursor head, refusal hatching, placement previews, stand-ins, and any paper reading page. | Chrome. |
| `--r-sm` | Every piece of chrome: Slates, flyouts, menu rows, local editors, inline confirm bars, inputs, buttons, badges, key caps, scroll cues, and picture frames held inside a Slate. | Anything on the Grid. |
| `--r-round` | Point markers only, under the scoped exception below. | Anything holding content, anything a person reads, and any control that can carry a label. |

- Chrome carries `--r-sm` on all four corners. An asymmetric radius such as
  `2px 2px 0 0` is a defect, because a rounded head on a square body reads as
  two joined objects rather than one surface.
- No fourth radius exists. A value outside this table is a defect wherever it
  appears, including inside a rendering, a mockup, or a catalogue deck.
- Radius is never a state. No state adds, removes, or changes a corner; states
  are drawn as outlines and fills, and a corner that moves would change the
  component's extent, which `States.md` forbids.
- A container never clips its content to a radius. Clipping rounds the corners
  off the content itself, and Grove never crops what a person placed.
- Paper is square. A page in a reading edition, a placed Document, and a
  printed stand-in all use `--r-none`, because a page has square corners.

### The scoped exception

`--r-round` is admissible for exactly three things:

| Mark | What it is |
| --- | --- |
| A point marker on content | A mark whose entire meaning is "here", carrying no text and no content — an annotation marker and its centre. |
| A state dot | A small dot reporting one condition beside a label it does not contain. |
| An expanding acknowledgement | The single ring that radiates from the point a person acted on, over `--d-sweep`. |

Everything else circular is a defect. A control that opens something, holds a
glyph, or could carry a label is chrome and takes `--r-sm`, however small it
is — size does not convert a control into a mark.

## Placements on the Grid are never rounded

A placement's corners are square, at every distance, in every state, in every
form. This holds for the Note, the placed Document, the picture, every
stand-in that replaces them at distance, the placement preview, the origin
footprint held during a move, and the lit cells and perimeter of the presence
a placement casts.

The reason is geometric, not stylistic: a footprint is whole cells and every
edge lands on a major line, so a rounded corner would pull content off the
line the person aligned it to.

**Contradiction, ruled.** The image, selection, and distance decks draw the
picture frame with the chrome radius (`border-radius: var(--r)`), and the
presence-fields deck rounds the placement preview. The Note and Document
decks, the stand-in forms in the distance deck, the selection deck's own
preview, and the shipped runtime all draw these square. The square reading
wins: `--r-none` is normative for every placement, the picture included, and
those four deck renderings are corrected.

## What a border is for

A border does one of three jobs. A border doing none of them is removed.

| Job | Drawn as | Token |
| --- | --- | --- |
| Containment — this surface ends here | 1px, on the surface's own box | `--edge-quiet`, or `--paper-edge` on paper |
| Containment, locatable — an edge a person must be able to find | 1px | `--edge-found` |
| Separation — groups within one surface | 1px, no radius, no box | `--edge-hairline` |
| Focus — keyboard attention is here | 2px ring, outside, `2px` offset, `:focus-visible` only | `--signal-interaction` at `--ink-full` |
| Selection — this is what you are working on | 2px outline, outside, `3px` offset | `--signal-interaction` |
| Refusal — this cannot happen | 1px on the refused thing, with 45° hatching over the region | `--signal-refusal` |
| Active gesture — a sweep is open now | `--field-perimeter-width`, with a low-alpha fill | `--signal-active-work` |

A containment edge may take a role hue instead of a neutral one — the local
editor's edge is tinted Edit for text surfaces and View for viewers, and a
Slate's edge is tinted Slate. Hue is the only per-role change: the width, the
radius, and the count of strokes never vary.

Three stroke widths exist and no fourth: 1px for containment and separation,
2px for state and for the cursor ring, and `--field-perimeter-width` for the
field perimeter and the active sweep.

### Never decoration

These are refused at any edge, on any plane:

| Refused | Reason |
| --- | --- |
| A glow, halo, or aura around a placement | The presence a placement casts is the field's job, per cell with hard boundaries; a soft ring around content competes with it and blurs the perimeter. |
| A second stroke — a double border, an inner ring inside an outer one | One edge, one stroke; a second stroke states nothing the first did not. |
| An inner top highlight (`inset 0 1px 0` in white) | It fakes a light source Grove does not have and reads as a bevel. |
| A bevel, emboss, or ridge | Same reason. |
| A gradient stroke or a gradient fill on chrome | A gradient may reproduce content and nothing else. |
| A border added to make a control look clickable | Position, spacing, and the hue role identify a control; an outline that means nothing at rest devalues the outlines that mean something. |

**Contradiction, ruled.** The Note and Document decks describe the selected
state as an outline "with a soft glow", and the shipped runtime draws
`box-shadow: 0 0 24px 6px` on `.obj.selected`. `Tokens.md` declares two
shadows and states that a glow is not available. The token table wins: a
selected placement is an outline plus the field response, and both the deck
annotations and the runtime rule are corrected.

## Inset edges and outlines

An edge that belongs to the thing is drawn **inside** its own bounds. An edge
that belongs to a state is drawn **outside** them. Nothing is drawn in
between.

An inset edge uses `box-shadow: inset 0 0 0 1px`, never a CSS border, because
a border adds to the box and a placement's box is exact cells. The shipped
Document sheet, picture figure, and Note all draw their edge this way, and
each drops it when distance sheds surface treatment.

An outline uses `outline` with a positive `outline-offset`, so it never covers
content and never moves it. An outline takes the radius of what it surrounds:
square around a placement, `--r-sm` around chrome.

| State | Edge treatment | Where |
| --- | --- | --- |
| Rest | The component's own containment edge, and nothing else. | Inside |
| Approached | Nothing at the edge. Approach adds chrome that serves the hand — the resize corner, an edit affordance — and leaves the edge alone. | — |
| Focused | 2px ring, `2px` offset, outside any selection outline with a visible gap. | Outside |
| Selected | 2px outline, `3px` offset, plus the field response. The component's own edge is unchanged. | Outside |
| Engaged | The sweep rectangle carries its own edge and fill; the placement being moved keeps its rest edge at the origin. | Outside |
| Pending | The same edge the settled form carries. A stand-in keeps a full edge. | Inside |
| Refused | The refusal hue replaces the edge hue in place, plus hatching across the region. | Inside |
| Unavailable | `--edge-hairline` on its own box, position held. | Inside |
| Anchored | The ribbon at the top edge, or the 9 × 9px square rotated 45° outside the top-left corner of a complete frame. | Outside |

A dashed edge means provisional and nothing else: the placement preview before
commit, and the origin footprint held while a move is open. A settled edge is
never dashed, so an empty collection carries words and one next step rather
than a dashed box.

The resize corner is two 2px strokes meeting at a right angle, never a rounded
arc and never a full box.

## One corner language per journey

Within one journey every chrome corner is `--r-sm` and every Grid corner is
`--r-none`. A journey that mixes corner languages is a defect, and mixing
takes three forms, all refused:

1. A control whose radius differs from the surface holding it.
2. A foreign radius imported from another system — any value outside the
   corner table.
3. Chrome rounding applied to something that lives on the Grid, or square
   corners applied to a floating surface to make it look Grid-attached.

Nesting the same radius is correct and expected: a picture frame at `--r-sm`
inside a Slate at `--r-sm` reads as one surface holding another.

## A frame around content is a card layout

The Grid is not a card layout. Drawing a frame, panel, tile, or persistent
chrome around placed content converts the field into a stack of cards and
destroys the spatial reading the Grid exists to give.

- A placement's only edge is its own inset edge. No wrapper, no title bar, no
  footer strip, no toolbar, no badge tray at rest.
- Several placements are never enclosed in a shared frame to group them.
  Grouping is expressed by position and by the presence the placements cast.
- No mat, drop shadow, rotation, or caption plate is added around a picture.
  A picture's only dress is its quiet 1px edge.
- Nothing overlays a frame. Size, name, and other details sit beside or beneath
  the frame, never on it.

The same rule binds collections inside chrome. In a masonry collection the
item's edge is the frame of the picture itself; metadata sits outside that
frame in quiet monospace, with no second container drawn around frame plus
metadata. The shipped `.gallery-card` and `.memory-card` wrappers — a border,
a fill, and padding around each item — are card-ification of a collection and
are removed; the catalogue's gallery rendering, which puts the edge and the
selection outline on the frame alone, is correct.

## Ornament is refused

| Refused form | What replaces it |
| --- | --- |
| A pill — a fully rounded chip, tag, or segmented toggle | A rectangle at `--r-sm` with the same label. |
| A rounded card | A rectangle at `--r-sm`, or no container at all. |
| A glossy gradient, sheen, or shine on a surface or control | A flat fill from the surface tokens. |
| A circular avatar, thumbnail, or icon button | A square frame at the same size, `--r-sm` for chrome and `--r-none` on the Grid. |
| An ornamental container — a box, panel, or rule containing nothing that needs containment | Nothing. Space separates. |
| A decorative divider, flourish, or bracket | `--edge-hairline`, or nothing. |
| A rounded badge or counter | A rectangle at `--r-sm`; a count reads as a figure, not a bubble. |

## The ten shipped radii

Ten distinct radius values are in the product stylesheets. Two are correct as
they stand; eight are defects.

| Value | Where it is used now | Migrates to |
| --- | --- | --- |
| `2px` | 45 sites across Slate panes, menu rows, inputs, buttons, badges, gallery and Memory cards, annotation reading controls, and the anchor editor fields. | `--r-sm`. Already correct; the token name replaces the literal. |
| `3px` | The context menu, its flyout, and the image viewer stage. | `--r-sm` |
| `5px` | The key cap in the fixed status readout. | `--r-sm` |
| `6px` | The Layer manager's search field, rows, buttons, and destination select; Memory card thumbnails and previews; the Memory recall anchor chip. | `--r-sm` |
| `7px` | The Gallery and Memory openers, the Slate rail tabs, the Layer manager toggle, the Memory query field and chips, the Memory input and filters. | `--r-sm` |
| `9px` | The Memory card container and the recall result block. | `--r-sm` |
| `11px` | The fixed status readout panel. | `--r-sm` |
| `12px` | The Layer manager panel. | `--r-sm` |
| `50%` | Six sites: the status dots, the Layer marker, the collapsed status pip, the expanding acknowledgement ring, the annotation marker, and its centre. | `--r-round` for the two annotation marks, the two dots, and the acknowledgement ring; `--r-sm` for the collapsed status pip, which is a control and not a mark. |
| `2px 2px 0 0` | The Slate pane header strip and the writing toolbar. | `--r-sm` on all four corners. |

Three shipped edge treatments migrate with them: `.obj.selected` and
`.obj.traced` move from `outline-offset: 0` to `3px` and drop their glow, and
the context menu drops its `inset 0 1px 0` white highlight.

## Deciding when this document is silent

Resolve in this order, and write the answer here rather than leaving it open:
an accepted Decision record; then the nearest established desktop convention;
then print and page mechanics, which govern any surface a person reads.
Applied to shape, the default answer is always the squarer one — Grove's
identity is architectural sharpness, so a corner in doubt is `--r-none` on the
Grid and `--r-sm` everywhere else.
