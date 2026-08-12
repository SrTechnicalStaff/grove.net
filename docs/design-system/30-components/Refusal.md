---
type: design-system-component
status: active
date: 2026-08-10
component: Refusal
plane: grid
surface_class: chrome
tags: [grove, design-system, component]
---

# Refusal

How Grove says no: the footprint under the hand turns, the cells that cannot
take it are hatched, and one plain sentence sits beside them with the action
that would have committed present and unavailable.

Refusal is drawn only while a gesture is open. It is the answer to an attempt,
never a standing condition, and it is carried by three parts at once so it
reads with no colour at all.

Refusal raised inside a Slate or a local editor is the inline confirm's refusal
form and belongs to `10-grammar/Surface-classes.md`. This specification owns
refusal on the Grid only.

## Anatomy

| Part | Required | Value |
| --- | --- | --- |
| Refused footprint | yes | The placement preview's own geometry with its hue changed to `--signal-refusal`: fill at `0.12`, a `2px` inset edge at `0.85`, and a `1px` dashed containment border at `0.90`. Three alphas over the one `--c-invalid` triplet, which is how `00-foundations/Tokens.md` composes every signal; they are not variants of the hue and no second refusal value exists. |
| Blocked cells | yes | The refusal hatch of `00-foundations/Marks.md`: `repeating-linear-gradient(45deg, …)`, a `4px` band on a `12px` period — band on, `8px` off — in `--signal-refusal` at `0.22`, with a `1px` inset edge in `--signal-refusal` at `0.45` so the blocked extent is exact. Drawn over the field and under the refused footprint, on whole cells only, with no fill of its own. |
| Strip | yes | `--surface-chrome`, opaque, `--r-sm`, one `1px` `--edge-quiet` containment border, `--sp-sm` top and bottom, `--sp-md` at the sides, `--shadow-local`. |
| Sentence | yes | `--f-ui` at `--t-caption`, `--text-primary`, one line, left in the strip. |
| Committing action | yes | `--f-ui` at `--t-caption`, `--text-unavailable` on a `1px` `--edge-hairline` border, `--r-sm`, `--sp-xs` top and bottom and `--sp-sm` at the sides, right in the strip. |
| Gap between the sentence and the action | yes | `--sp-md`. |
| Quiet action | no — refused | None. Releasing already ends the gesture at no cost, so a second control would be chrome for something the hand already does. |

The strip is the only part with a fill, a containment edge, or padding: the
refused footprint and the blocked cells are additive drawing over the field
and add no surface of their own.

Line weights and the hatch pitch are screen-space figures and do not scale with
the camera; every region — the footprint, the blocked cells — is measured in
cells and scales with it. This is the same split `00-foundations/Marks.md`
applies to every mark, and it is what keeps the hatch legible when the
footprint beneath it is a few pixels wide.

Three deck figures are corrected because they sit on no scale. The strip's
`11px 13px` padding becomes `--sp-sm` and `--sp-md`, its `18px` inner gap
becomes `--sp-md`, and the action's `5px 10px` becomes `--sp-xs` and `--sp-sm`;
Grove's space scale governs rhythm and the rendered figures predate it. The
strip's `1px` border moves from `--edge-hairline` to `--edge-quiet`, because a
surface sitting directly on the field needs an edge that contains rather than
one that separates groups inside a surface. The sentence's `#EAEAEA` becomes
`--text-primary` and the deck's removed quiet action was set at an alpha of
`0.72`, which is not one of the nine ink steps.

The refused footprint carries no word in its corner. The deck sets `Occupied`
there and it is corrected, because the strip already carries the words and a
label inside the footprint lands on whatever the footprint covers, where
`--signal-refusal` reaches only `3.14:1` on `--c-paper`. The footprint keeps
the size figure its own specification gives it, and that figure takes the
refusal hue with the rest of the drawing.

## Geometry

- **Footprint** — Refusal has none of its own. The hatched region is computed:
  take the attempted footprint's cell rectangle; for each cell in it, mark the
  cell blocked when any placement on the current Layer occupies it; the hatched
  region is exactly the set of blocked cells. It is the intersection, never the
  whole footprint — the catalogue's four-cell-wide attempt over a Document
  hatches the two columns that overlap and no more.
- **Growth** — Nothing grows and nothing shrinks. The attempted footprint keeps
  the extent the person is holding, because resizing it to one that fits would
  answer a question they did not ask. The strip grows with its sentence and its
  action and never truncates.
- **Measure** — None. The strip carries one clause on one line, so it sets no
  reading column and `--measure-reading` does not apply.
- **Alignment** — The blocked region's four edges land on major grid lines,
  since it is a set of whole cells. The strip's left edge aligns with the
  attempted footprint's left edge and its top edge sits `--sp-md` below the
  footprint's bottom edge, flipping above the footprint when the viewport's
  bottom edge is nearer than the strip's own height. The strip tracks the
  footprint as the hand keeps moving, so it never leaves the thing it is about.

## States

| State | Appearance | Notes |
| --- | --- | --- |
| Rest | Nothing is drawn. | There is no resting refusal; refusal exists only under an open gesture. |
| Approached | No change from Rest. | Approach adds nothing, because the pointer is already on the Grid and the gesture, not the pointer, raises the drawing. |
| Focused | No change from Rest. | Keyboard attention stays the Grid cursor at its cell; no part of this component takes a focus ring. |
| Selected | No change from Rest. | Selected placements keep their outline and their lit cells; refusal draws only on the attempted footprint and the blocked cells, and never repaints a presence region. |
| Engaged | The engagement drawing turns to the refusal role in place, the blocked cells hatch, and the strip appears. | `10-grammar/States.md` fixes this combination; the gesture stays open and the hand can keep moving. |
| Pending | Not reachable. | Validity is answered on the frame the footprint is tested, so no refusal is ever outstanding. |
| Refused | All three parts together: the footprint in the refusal role, the hatch on the blocked cells, the sentence and the unavailable action in the strip. | Any one part alone is a defect, not a lighter refusal. |
| Unavailable | The committing action in the strip, and nothing else. | It is `--text-unavailable` on `--edge-hairline` and keeps its position, so a person sees what they were reaching for. |
| Anchored | No change from Rest. | Placements carrying authored context keep their mark and their presence hue underneath; refusal never recolours a presence region. |

## Behaviour

- **Pointer** — While the button is down the footprint is tested on every cell
  the pointer crosses, and the three parts appear on the frame the footprint
  stops fitting and are removed on the frame it fits again. Releasing over a
  refused footprint commits nothing and ends the gesture, and every part is
  removed on the release frame. The strip takes no pointer input and the
  unavailable action does not answer a click, so no target appears under a
  moving hand.
- **Keyboard** — `Escape` ends the open gesture and removes all three parts,
  per the Grid Plane row in `docs/reference/Keybind map.md`. The keys that move
  the footprint keep moving it and the answer updates under them. No other key
  is answered, and no key dismisses the strip while the gesture continues.
- **Focus order** — None. No part of this component is focusable; focus stays
  on the Grid cursor for the whole gesture, because keyboard attention belongs
  on the cells being moved rather than on a control that cannot be used.
- **Escape** — `Escape` cancels the gesture and returns to the Grid cursor at
  the cell where the gesture began, which is the Grid row of the escape
  order in `10-grammar/Surface-classes.md`.
- **Commit and cancel** — Nothing durable changes at any point. A refused
  gesture writes nothing, moves nothing, and creates nothing; the source keeps
  its place and the Grid is exactly as it was. There is nothing to undo,
  and the first press of `Ctrl/Cmd+Z` after a refusal undoes whatever preceded
  the gesture.

When and from which gesture a refusal is reached belongs to the wireframe that
owns the gesture; this specification owns only what it looks like and how it
answers.

## Motion

| Transition | Duration | Curve | Reduced motion |
| --- | --- | --- | --- |
| The footprint turning to the refusal role | None — immediate | None | Identical; there is nothing to reduce |
| The hatch arriving on the blocked cells | None — immediate | None | Identical; there is nothing to reduce |
| The strip arriving | `--d-fade` | `--ease` | Appears at the identical threshold, with no fade |
| The strip leaving | `--d-fade` | `--ease` | Disappears at the identical threshold, with no fade |
| Every part leaving on release or `Escape` | None — immediate | None | Identical; there is nothing to reduce |

The footprint and the hatch change with no transition because the answer must
arrive under the hand, and a fade would still be reporting a cell the pointer
has already left.

Nothing loops. Nothing idles. Nothing pulses or blinks.

## Distance

| Tier | Sheds | Keeps |
| --- | --- | --- |
| Working | Nothing. | The footprint in the refusal role, the hatch, the strip. |
| Stepped | Nothing. | All three, unchanged; the hatch holds its `4px` band on its `12px` period because its pitch is screen-space. |
| Stand-in | Nothing. | The refusal hue on the block, the hatch on the cells, and the sentence in the strip, which is chrome and keeps its size. |

Refusal is the only drawing in Grove that sheds nothing at any tier, because it
is an answer to an attempt and an answer that thins with distance reports less
than happened. `10-grammar/Representation-tiers.md` already fixes the stand-in
row, and the two tiers above it inherit it.

## Accessibility

- **Role and name** — The strip is `role="status"` with `aria-live="polite"`,
  so the sentence is announced once when the refusal begins;
  `00-foundations/Accessibility.md` permits announcing a refusal and nothing
  else about this gesture is announced. Its accessible name is the sentence
  itself. The committing action is a button named by its own verb, carrying
  `aria-disabled="true"` rather than `disabled`, because the state model
  requires it to stay present and a disabled control is neither reachable nor
  announced. The footprint and the hatch are hidden from assistive technology,
  because the sentence carries the same fact in words and a hatched region
  would name cells a person cannot address.
- **Contrast** — The sentence is `--text-primary` on `--surface-chrome` at
  `10.36:1`. The unavailable action is `--text-unavailable` at `2.46:1`, the
  one exemption the foundations grant, and it is paired with a border and a
  retained position. The footprint's dashed border reaches `4.75:1` on
  `--c-base` and its inset edge `4.34:1`, so the meaning-bearing edge clears
  the `3:1` non-text minimum. The hatch bands reach `1.31:1` and the blocked
  region's inset edge `2.01:1` at the alphas `00-foundations/Marks.md` fixes;
  the hatch is therefore the structural carrier and never the one that carries
  the `3:1` obligation, which the footprint's edges and the sentence meet.
- **Without colour** — Refusal uses hue in every part, and every part has a
  second carrier: the footprint is a complete dashed rectangle standing where
  no content is, the blocked cells are `45°` stripes drawn nowhere else in
  Grove, and the strip is a sentence. Remove all colour and the refusal still
  reads three times over.
- **Forced colours** — The sentence, the unavailable action, the strip's border
  and the footprint's dashed border survive as text and borders, and the dash
  pattern survives as structure. The hatch is a gradient and does not survive,
  so the blocked region's `1px` inset edge is redeclared in the system mark
  colour and it is what states the blocked extent; the strip's fill, both
  borders, the text, and the refusal hue are redeclared in system colours.
- **Text scaling** — The strip's sentence and action grow with interface text
  and the strip grows with them; at the width where the sentence and the action
  cannot share a line, the sentence takes the first line and the action
  right-aligns on the second inside the same frame. Nothing truncates. The
  footprint, the blocked cells, and the hatch do not scale with interface text,
  because they are measured in cells and in screen pixels.
- **Reduced motion** — No change from the Motion table.

## Copy

One string.

**`This space is occupied.`**

It states the condition of the world rather than the operation, the attempt, or
the person: `10-grammar/Copy.md` names it as the correction for
`Placement rejected` and it carries no plane noun, no architecture noun, and no
process word. It is one clause in the present tense, it sits beside the thing
being refused, and it takes a full stop because Copy.md gives a full stop to a
full sentence in a strip. The catalogue renders it without the stop and is
corrected.

The unavailable action carries the committing verb of the open gesture, owned
by that gesture's specification; for a placement it reads `Place`, a verb with
no terminal punctuation. The deck's quiet `Cancel` is removed with the action
itself, and would fail Copy.md in any case by naming the question rather than
an outcome.

The shipped runtime writes three different sentences for this one condition —
`That area is occupied. Choose another Grid area.`,
`Those cells are occupied on this Layer.`, and
`That space is occupied on the destination Layer.` — each in a session
surface's status line rather than beside the footprint. All three are
corrected to the one sentence above; the second clause of the first is an
instruction a person does not need, and `Grid area` is machinery.

## Refusals

- **A scrim, or any dimming of the Grid** — the field behind the strip
  stays fully lit, legible, and live, because dimming claims an interruption
  Grove does not make.
- **A centre-screen alert, dialog, or floating confirmation** — an answer
  detached from the cells it is about takes the hand off the work and then asks
  for a click to give it back.
- **A partial commit** — nothing is half-placed, half-moved, or half-written,
  so there is never anything to undo after a refusal.
- **Moving or resizing the footprint to make it fit** — that answers a question
  the person did not ask and hides the one they did.
- **Closing the gesture on refusal** — the drawing stays and the hand keeps
  moving, because a gesture that ends on the first refused cell makes a person
  restart to try the cell beside it.
- **A blink, pulse, shake, or any repeating motion** — a repeating animation
  claims attention forever for information delivered on the first frame.
- **Removing the committing action instead of making it unavailable** — an
  action that vanishes teaches a person that the interface is unstable.
- **Restyling the source content to say the move will not land** — dimming or
  outlining authored content makes the refusal a property of the thing a person
  wrote.
- **A second refusal value, a caution level, or a warning short of refusal** —
  Grove has one refusal hue and no severity scale.
- **The refusal hue on a result that has not arrived or failed to arrive** —
  nothing was refused, so nothing refuses.


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
- Plane: `docs/design-system/20-planes/Grid-plane.md`
- Mark: `docs/design-system/00-foundations/Marks.md`
- Keys: `docs/reference/Keybind map.md`, Grid Plane row for `Escape`.
