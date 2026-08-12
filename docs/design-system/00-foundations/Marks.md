---
type: design-system-foundation
status: active
date: 2026-08-09
tags: [grove, design-system, marks]
---

# Marks

A mark is a non-text shape that carries meaning on its own. Grove has seven of
them and no more. The set is closed: a shape that is not on this list is a
defect, not a new pattern.

Marks are not decoration and not iconography. **Grove has no icon set.** There
is no glyph font, no picture language, no symbol library, and no plan for one.
A row in a menu is a word; a state is geometry plus a word; a kind is a word in
monospace. A reader never decodes a picture to find out what something is.

## The set

| Mark | Shape | Carries | Attaches to |
| --- | --- | --- | --- |
| Anchor ribbon | Tab notched at its foot | Authored context is present | The top edge of a form with a head |
| Anchor diamond | Square rotated 45° | Authored context is present | The top-left corner of a complete frame |
| Kind badge | Monospace word in a hairline box | The kind of a frame its own picture cannot declare | The top-right corner of a frame |
| Refusal hatch | 45° stripes | This region cannot take the operation | Grid cells, never content |
| Resize corner | Two arms meeting at a right angle | This extent can be dragged | The bottom-right corner of a placement |
| Perimeter ring | 1px line on outward cell edges | Where an occupied region ends | Cells of the field |
| Point marker | Ring with a solid core | Something is here, at this point | A position in view, in screen space |

## Reading a mark

Every mark is geometry first. Remove all colour and each one still reads: the
ribbon is a tab, the diamond is a rotated square, the badge is a boxed word,
the hatch is stripes, the resize corner is two arms, the perimeter is a line on
one side of a cell, the point marker is a ring. Hue confirms a mark; it never
carries it alone, so a colour-blind read lands with no loss.

Four rules bind every mark without exception.

| Rule | Why |
| --- | --- |
| A mark never changes a component's footprint, position, or extent. | Marks are drawn at or outside an edge, so nothing reflows when one appears. |
| A mark is never a control. Only the resize corner takes a pointer, and it takes only a drag. | A mark that can be clicked stops being a statement and becomes chrome. |
| A mark carries no number, count, score, ordinal, or status word. | The moment a mark holds a value it becomes a dashboard reading, and the answer belongs in the thing the mark points at. |
| A mark never animates, pulses, loops, or glows. The only transition on any mark is the resize corner's fade over `--d-fade`, which becomes an immediate change under reduced motion. | A mark states a fact; a fact does not move. |

Marks are never authored. A person cannot add, remove, recolour, or reposition
one, and no mark ever means "a person picked this".

## Anchor ribbon

**Geometry.** `12 × 22px`, clipped to
`polygon(0 0, 100% 0, 100% 100%, 50% 72%, 0 100%)` — a straight-sided tab with
a shallow V cut into its foot.

**Token.** Filled `--signal-authored-context`, flat, no border, no shadow.

**Where it attaches.** Hanging from the placement's top edge, `5px` above the
edge and `17px` down over the head, with its left edge `--sp-md` from the
placement's left edge. The offset is fixed for every form; the Document deck
renders `20px` and the Note deck `16px`, and `16px` wins because it is on the
spacing scale and `20px` is on nothing.

**Never.** Never over authored text. Never on a frame whose picture reaches its
own edge — that form takes the diamond. Never a second ribbon on one placement.
Never any hue but the authored-context signal.

## Anchor diamond

**Geometry.** A `9 × 9px` square rotated 45°.

**Token.** Filled `--signal-authored-context`, flat, no border, no shadow.

**Where it attaches.** Its bounding box is offset `-4px` on both axes from the
frame's top-left corner, putting the mark's centre on the corner point and the
bulk of its mass outside the picture. It never moves inboard.

**Never.** Never anywhere but the top-left corner. Never scaled to the frame.
Never paired with a ribbon on the same placement. Never over the composed part
of a picture.

## Choosing between the anchor marks

One question decides it: does the form reserve margin at its top edge?

| Form | Mark | Reason |
| --- | --- | --- |
| Note | Ribbon | Its head is padding, so a tab has somewhere to sit. |
| Document | Ribbon | Its head is the title block's margin. |
| Image | Diamond | The picture reaches every edge; a tab would sit on the subject. |
| GIF | Diamond | Same frame, same rule. |

A form added later follows the same test: margin at the top edge takes the
ribbon, edge-to-edge content takes the diamond. There is no third answer and no
form that takes both.

## Kind badge

The badge has one job: **name the kind of a frame that its own picture cannot
declare.** A still and a moving picture look identical while the moving one is
paused, so the moving one says `GIF`. That is the whole job.

**Geometry.** A word in `--f-mono` at `--t-micro`, uppercase, tracked
`--tr-wide`, ink `--ink-primary` because it sits over arbitrary photography at
the smallest size in the scale. Padding `2px 6px`, a `1px` `--edge-quiet`
border, `--r-sm` corners, and a `--c-canvas` fill at `0.72` alpha so the word
survives a light picture beneath it.

**Where it attaches.** The frame's top-right corner, inset `--sp-sm` on both
axes. Where a frame already has a metadata line beside or beneath it, the kind
is a word in that line and no badge is drawn on the frame; the badge exists
only where there is nowhere else for the word to go. The Gallery deck sets the
badge at `--t-micro`'s replaced `8px` and the frame decks at `9px`; the token
table's `--t-micro` settles it.

**Never.** Never a control, never focusable, never a filter, never a link.
Never more than four characters, because a longer word stops being a mark and
starts covering the picture. Never a count, a size, a date, a state, or a
status. Never on a Note, a Document, or any form that can say what it is in its
own text. Never more than one per frame.

## Refusal hatch

**Geometry.** `repeating-linear-gradient(45deg, …)` with a `4px` band and a
`12px` period — band on, `8px` off, forever. The angle is `45deg` measured the
CSS way and it is the same everywhere; a second angle would read as a second
meaning. The blocked region also carries a `1px` inset edge so its extent is
exact.

**Token.** Bands in `--signal-refusal` at `0.22` alpha, inset edge in
`--signal-refusal` at `0.45` alpha.

**Where it attaches.** The Grid cells that cannot accept the operation, drawn
over the field and under the placement preview. It covers whole cells only.

**Never.** Never on a placement's own surface, never on paper, never inside a
Slate or a local editor, and never on a control. Never at any angle but 45°,
and never at a second repeat — the 135° stripes in the catalogue are
photographic stand-in texture and annotation-layer shading, not this mark.
Never alone: refusal is hatch plus `--signal-refusal` plus one plain sentence,
per the state model.

## Resize corner

**Geometry.** An `18 × 18px` pointer target with the drawn mark inset `3px`
from its right and bottom edges: two `10px` arms of `2px` weight meeting at the
corner. The target is offset `-2px` on both axes so the arms sit on the
placement's edge rather than inside its content.

**Token.** `--paper-strong` on paper forms and `--ink-primary` on dark forms —
the token table reserves `--paper-strong` for this mark, and `--ink-primary` is
its match on the dark ramp. The runtime's `0.55` on paper is below both and is
corrected to `--paper-strong`.

**Where it attaches.** The bottom-right corner of a placement, and only there.
It is absent at Rest, appears on Approached and on Selected over `--d-fade`,
and leaves with the pointer.

**Never.** Never in the tab order — resize from the keyboard is the cursor's
armed state, so a second keyboard path would be a second model of the same
action. Never on all four corners, never on an edge, never a handle a person
can drag to move rather than resize. Never present below the stepped
representation tier.

## Perimeter ring

**Geometry.** A line of `--field-perimeter-width`, drawn inset on a cell's own
box and only on the edges where the neighbouring cell falls outside the
occupied region. The result is the exact contour of the region, hard-edged, on
cell boundaries.

**Token.** `--field-perimeter-ink` in the region's own hue at rest, and
`--field-perimeter-selected` when the region is selected. Where the region
carries authored context, the hue is `--signal-authored-context`, not the
interaction hue.

**Where it attaches.** The outer cells of an occupied field region. One region,
one contour. A region inside a larger region draws its own contour and the
larger one keeps its own; neither is redrawn because the other exists.

**Never.** Never blurred, feathered, or softened — a gradient halo crosses cell
boundaries and destroys the meaning of the contour. Never rounded. Never drawn
on an interior edge. Never a different width when selected; only the alpha
changes, so the region's shape never appears to move. The catalogue renders the
resting perimeter at `0.30` and the token table sets `--field-perimeter-ink`;
the table wins, because it owns values.

## Point marker

**Geometry.** `14px` across, centred on its position, `--r-round`. A `1px` ring,
a translucent fill, and a solid core inset `3px`, with a soft halo outside the
ring. When engaged, the ring goes to full ink, the fill strengthens, and the
halo widens to `7px`; nothing else changes and the mark does not move.

**Token.** Ring, fill, core, and halo all in `--signal-interaction`; the
engaged ring goes to `--c-ink` at `--ink-full`.

**Where it attaches.** A position in view, in screen space. It does not scale
with the camera, does not sit in the Grid's coordinate space, and keeps its
size at every zoom.

**Never.** Never selectable, never hit-tested as content, never a placed
object, never something a person can move or delete. Never carries a count, a
score, a saturation reading, or a label. Never more than one engaged at a time
— moving to another candidate replaces the active one. Never blinks or pulses
for attention.

## Marks and the state model

Marks are the only geometry a state may add. The state model owns which states
exist; this table owns which mark each may draw.

| State | Mark it may add |
| --- | --- |
| Rest | None. A placement at rest carries the badge if its kind needs one and the anchor mark if it is anchored — nothing else. |
| Approached | Resize corner. |
| Focused | None. Focus is a ring, not a mark. |
| Selected | Resize corner, and the perimeter ring shifts to `--field-perimeter-selected`. |
| Engaged | None. The refusal hatch may appear beneath an open gesture, but it belongs to Refused. |
| Pending | None. A stand-in keeps whatever marks the working form carried. |
| Refused | Refusal hatch. |
| Unavailable | None. |
| Anchored | Ribbon or diamond, by form. |

No state adds two marks of the same kind, and no mark appears for a state not
listed here.

## Marks at distance

Marks shed in the same order as everything else: what serves the hand goes
first, what serves the eye stays longest.

| Mark | Below the stepped threshold | At stand-in size |
| --- | --- | --- |
| Resize corner | Gone | Gone |
| Kind badge | Gone | Gone |
| Anchor ribbon | Kept | Kept |
| Anchor diamond | Kept | Kept |
| Refusal hatch | Kept | Kept |
| Perimeter ring | Kept | Kept |
| Point marker | Kept, unscaled | Kept, unscaled |

The anchor marks and the perimeter ring survive to the smallest form because
authored context and region extent are the two facts distance may not hide.
They are drawn at their stated pixel geometry, unscaled, so they stay legible
when the placement beneath them is a few pixels wide.

## Closure

The set above is complete. A new shape does not join it by being drawn; it
joins by a change to this document, and only after the question "which existing
mark already answers this?" has a real answer of "none".

These are the shapes Grove refuses, permanently:

- Any icon, glyph, pictogram, or symbol font.
- A status dot, a coloured chip that means a category, or a traffic-light
  indicator.
- A count bubble, a numeric superscript, or an unread ring.
- A spinner, a progress ring, a skeleton shimmer, or any busy animation.
- A chevron, caret, or arrow used to imply a hidden panel.
- A drag grip of dots or lines.
- A checkmark or cross drawn as a mark rather than written as a word.
- A ribbon, corner fold, or banner used for anything but authored context.

Anything on that list that appears in a build is a defect with a name, and the
correct fix is to remove it, not to token it.
