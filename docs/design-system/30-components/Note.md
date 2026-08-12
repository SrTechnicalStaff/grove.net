---
type: design-system-component
status: active
date: 2026-08-09
component: Note
plane: grid
surface_class: placement
tags: [grove, design-system, component]
---

# Note

A thought a person wrote, placed in its own cells and set in full on the
colour they chose for it.

## Anatomy

Every visible part, named, with the token that gives it its value. A part with
no token is a part with no contract.

| Part | Required | Value |
| --- | --- | --- |
| Footprint box | yes | `n × --grid-cell` on both axes, `--r-none`. Content on the Grid is never rounded. |
| Authored fill | yes | One of `--c-note-violet`, `--c-note-clay`, `--c-note-slate-blue`. Opaque, flat, edge to edge. |
| Containment edge | yes | `1px` inset in `--edge-on-color`. |
| Text block | yes | `--f-ui` at weight 500, `--t-body`, `--lh-snug`, ink `#F4F4F2`, aligned to the top of the content box and filling its width. |
| Padding | yes | `16px 15px`. |
| Resize corner | no | `Marks.md` geometry, ink `--ink-primary`. Present on Approached and Selected only. |
| Edit affordance | no | Inset `--sp-sm` from the top and right edges; `--c-base` at `0.72`; `1px` border `#F4F4F2` at `--ink-secondary`; `--r-sm`; label `Edit` in `--f-mono` at weight 500, `--t-micro`, `--tr-mono`, uppercase, ink `#F4F4F2`; padding `4px 6px`. Present on Approached, Focused and Selected only. |
| Anchor ribbon | no | `Marks.md` ribbon, filled `--signal-authored-context`, left edge `--sp-md` from the Note's left edge, `5px` above the top edge and `17px` down over the head. Present on Anchored only. |
| Presence it casts | yes | The Note's own fill as a triplet — `--c-note-violet-field`, `--c-note-clay-field`, or `--c-note-slate-blue-field` — under `--field-gain`, `--field-alpha-min` and `--field-alpha-max`; `--signal-authored-context` instead when the Note is Anchored. |
| Perimeter ring | yes | `--field-perimeter-width` at `--field-perimeter-ink` in the Note's own presence hue, rising to `--field-perimeter-selected` when Selected. |

The containment edge is inset on the Note's own box, so no state changes the
footprint. The fill is the containment: it clears 3:1 against
`--surface-grid` on all three colours, which is why the inset edge is
decorative surface treatment rather than a meaning-bearing edge under
`Accessibility.md`. It uses `--edge-on-color` rather than a step of the `--ink`
ramp because the warm neutral muddies against a saturated fill.

Three figures are this component's own. `16px 15px` padding is assigned to the
Note by `00-foundations/Tokens.md` and shared with nothing else: the extra
vertical inset holds the first line's cap height off the top edge while the
tighter horizontal inset buys measure inside a `220px` cell. `#F4F4F2` is the
Note's ink, owned here because the dark ink ramp composes over
`--surface-grid` and this text sits on an authored fill;
`Accessibility.md` fixes its ratios against all three colours. `0.72` on the
edit affordance's fill is `Marks.md`'s figure for a word that must survive an
arbitrary fill beneath it, and it is used here for the same reason.

The Note has no title bar, no metadata line, no caption, no kind badge, and no
control that is present at rest.

## Geometry

- **Footprint** — the smallest whole-cell square that holds every word.
  Executed as a procedure: set the text in `--f-ui` 500 at `--t-body` with
  `--lh-snug`, wrapping to the content box; take `n = 1`; compute the content
  box as `n × --grid-cell − 30px` wide by `n × --grid-cell − 32px` high; if the
  set text overflows either axis, increment `n` and repeat; the first `n` that
  does not overflow is the footprint, `n × n` cells. The same text yields the
  same `n` every time, because nothing in the procedure reads the camera, the
  Layer, or the neighbours.
- **Growth** — the footprint grows and the type never changes. The solve runs
  on every committed change to the text and produces the minimum `n`; the
  placed footprint is the larger of that minimum and the extent a person last
  set explicitly, so a Note a person made roomy stays roomy and a Note whose
  text outgrew its square steps up. A resize below the solved minimum is
  Refused rather than clamped, because silently ignoring a drag teaches a
  person that the corner does nothing.
- **Measure** — the Note sets type and sets no `ch` clamp; the line is bounded
  by the content box, which is `190px` at `n = 1`. `--measure-reading` does not
  bind here, because the square solver already couples line length to the
  amount of text and a `34ch` clamp inside the square would force a taller
  square for the same words while leaving a dead column beside them.
- **Alignment** — all four edges land on major lines. The origin corner sits on
  the top-left major line of its cell and the opposite corner on the major line
  `n` cells away on both axes; both sides are equal, always.

## States

All nine.

| State | Appearance | Notes |
| --- | --- | --- |
| Rest | Authored fill, inset containment edge, the whole text. Nothing else. | The anchor ribbon is present here when the Note is Anchored, because Anchored is a property and not an interaction. |
| Approached | The resize corner and the edit affordance fade in over `--d-fade`. | The fill, the edge and the text are untouched; approach adds and never restyles. |
| Focused | The Grid cursor sits on the Note's cells; the Note draws no ring. The edit affordance is present, because a keyboard hand reaches a Note the way a pointer does. | When the edit affordance itself holds keyboard focus it takes `--focus-ring` at `--focus-ring-offset`, drawn outside the affordance and not around the Note. |
| Selected | A `2px` `--signal-interaction` outline offset `3px` outside the containment edge; the perimeter ring on the occupied region rises to `--field-perimeter-selected` in the region's own hue; the resize corner and edit affordance are present. | The fill, the edge and the text are never tinted, dimmed, washed, or restyled. Selection is never stored on the Note. |
| Engaged | While a move or a resize is open, the footprint under the pointer is drawn in `--signal-interaction` and the Note keeps its place until release. Pointer-down takes the pressed appearance over `--d-press`. | Nothing durable changes while the gesture is open; releasing early costs nothing. |
| Pending | The Note holds its footprint and the most complete form it already has while a fuller form is queued. | A Note is never blank and never carries a progress mark; the stand-in is the pending form. |
| Refused | The refused footprint takes `--signal-refusal` as an inset edge and a faint fill, the cells that cannot accept the operation hatch at 45°, and one plain sentence sits beside the footprint. | The Note's own surface is not restyled and nothing moves; the sentence belongs to the Grid's refusal strip and is named there. |
| Unavailable | The Note's surface is never Unavailable — a placed Note is always readable, selectable, and movable. The edit affordance goes `--text-unavailable` on a `--edge-hairline` border, in place, while a local surface is already open on this Note. | An affordance that vanished would teach a person that the Note lost a capability it still has. |
| Anchored | The ribbon hangs from the top edge and the presence the Note casts takes `--signal-authored-context` instead of the Note's fill hue. | The ribbon's `17px` descent ends inside the `16px` top padding and the first line's half-leading, so it never touches a glyph. |

Combination follows `10-grammar/States.md` without exception. Selected plus
Anchored keeps the ribbon and gives the field the anchor hue, not the
interaction hue.

## Behaviour

- **Pointer** — a click on the body selects the Note, committing on pointer-up
  over it. A drag from the body moves the footprint, committing on release. A
  drag on the resize corner changes `n`, committing on release, and refuses
  below the solved minimum. A click on the edit affordance opens the Note's
  local editor beside it. A context gesture opens the Note's menu at the
  invoking point. There is no double-click gesture, because a hidden gesture
  has no keyboard equal.
- **Keyboard** — `R` enters and leaves Resize on the selected Note; `2`–`4`
  choose its colour; `1` cycles the active placement form; `A` opens the Anchor
  editor for it; `Delete` and `Backspace` remove it after explicit targeting;
  `Escape` cancels the open gesture. `Enter` on the focused edit affordance
  opens the Note's local editor. Every one of these is `docs/reference/Keybind map.md`,
  and none is redefined here. Focus arrives on the edit affordance and leaves
  to the next placement's; opening the editor moves focus into it.
- **Focus order** — one focusable part: the edit affordance. The resize corner
  is never in the tab order, because resize from the keyboard is `R` and a
  second keyboard path would be a second model of one action.
- **Escape** — Escape cancels the open move or resize and restores the
  footprint and position that existed before it. Escape never dismisses a Note
  and never reaches the Grid while a surface is open above it.
- **Commit and cancel** — the text becomes durable on `Ctrl/Cmd+Enter` in the
  Note's local editor; the colour on the key press; the footprint and the
  position on release of the gesture. Cancelling any gesture restores the prior
  state at no cost. Removing a Note raises the inline confirm owned by
  `10-grammar/Surface-classes.md`.

Editing the Note's text, reading a nearby annotation on it, and the removal
confirm belong to the surfaces that own them. Placement, transfer between
Layers, and the marquee sweep belong to the Grid.

## Motion

| Transition | Duration | Curve | Reduced motion |
| --- | --- | --- | --- |
| Resize corner and edit affordance arriving on approach or focus | `--d-fade` | `--ease` | Present at the identical threshold, with no fade. |
| Resize corner and edit affordance leaving with the pointer | `--d-fade` | `--ease` | Absent at the identical threshold. |
| Pointer-down acknowledgement | `--d-press` | `--ease` | The pressed appearance applied on press and removed on release, with no scaling. |
| Stand-in exchanged for the stepped form, stepped for the working form | `--d-swap` | `--ease` | The new form in place on the same frame, at the same threshold. |
| The Note committing to its cells | `--d-place` | `--overshoot` | At full size in its true cells on the frame the placement commits. |
| The Note leaving the Grid | `--d-exit` | `--ease` | Gone on the frame the removal commits. |

`00-foundations/Motion.md` names the curves `--ease` and `--overshoot`
and `00-foundations/Tokens.md` projects the same two values as `--ease` and
`--overshoot`; this specification uses Motion's names, because Motion owns
which curve a transition takes.

The selection outline, the focus ring, the anchor ribbon, the authored fill,
and the text never animate. Nothing loops. Nothing idles. Nothing pulses or
blinks.

## Distance

Thresholds are on projected cell size and belong to
`10-grammar/Representation-tiers.md`.

| Tier | Sheds | Keeps |
| --- | --- | --- |
| Working | The resize corner and the edit affordance, once projected cell size falls below `--tier-detail-promote`. | The authored fill, the inset containment edge, every word at `--t-body`, the ribbon when Anchored, presence, position, extent. |
| Stepped | The inset containment edge. | The authored fill, **every word still set at `--t-body`** — the true text, merely small, never a summary and never a substituted mass — the ribbon when Anchored, presence, position, extent. |
| Stand-in | The set type. | One block of the authored fill covering the exact footprint, a `1px` `--edge-on-color` inset edge, and the kind mark below. |

A Note keeps its own words at the stepped tier. Deck 02 renders it that way at
`0.24` scale and annotates it as the true text, and the shipped product agrees
— `.obj.representation-stepped` drops the inset edge and the chrome and leaves
`.sticky .note` drawn. Type at that projection falls below four pixels and
reads as texture rather than words, and that texture is honest: it is the
Note's own length and shape, not a drawn approximation of it. Substituting a
block of mass would be a summary, which Law 9 refuses.

This is where a Note and a Document part company. A Document's front page is
composed of parts that keep their meaning as masses, so it steps down to its
bones. A Note is nothing but its text, so it has no bones to step down to.

**The kind-coded stand-in.** The block keeps the Note's authored fill, because
below the size at which a mark can be drawn the fill is the only thing left
carrying kind. Over it sit three light strokes in `--ink-primary`: inset `14%`
from the left edge, at `26%`, `46%` and `66%` of the block's height, each
`2px` tall or `5%` of the block's height, whichever is greater; the first and
third run `60%` of the block's width and the second runs `46%`, so the mark
reads as written lines rather than a stack of rules. Below the size at which
those strokes can be drawn, the fill alone stands.

The stand-in is the Note. It is hit-testable, selectable, wears the same
outline and the same field response as the working form, and takes no geometry
of its own. Presence, position, and extent never shed at any tier.

## Accessibility

- **Role and name** — the Note is an option within the Grid's list of
  placed content, and its accessible name is the text a person wrote, verbatim.
  The word `Note` as an accessible name is a defect: it replaces the one thing
  that distinguishes this Note from every other.
- **Contrast** — `#F4F4F2` on the authored fill reaches 4.81 on
  `--c-note-violet`, 4.58 on `--c-note-clay`, and 4.73 on
  `--c-note-slate-blue`; each fill reaches 3.64, 3.82, and 3.70 against
  `--surface-grid`. The resize corner at `--ink-primary` reaches 3.63,
  3.35, and 3.52 on the three fills. The edit affordance's label reaches at
  least 13:1 on the affordance's own fill, and its border clears 3:1 against
  that fill, which is what defines the control's boundary. `--signal-interaction`
  reaches 9.50 on canvas. The inset containment edge carries no meaning and is
  exempt, because the fill carries extent.
- **Without colour** — Approached: chrome that was absent. Focused: the Grid
  cursor's ring. Selected: an outline outside the edge at a `3px` gap, plus the
  perimeter alpha rise. Engaged: the drawn footprint. Pending: the stand-in
  form, holding position and extent. Refused: 45° hatching and one plain
  sentence. Unavailable: a retained position and a hairline border. Anchored:
  the ribbon's silhouette. The three authored colours carry no meaning Grove
  assigns, so a greyscale read loses nothing Grove said and the whole thought
  is still legible.
- **Forced colours** — the authored fill, the text, the containment edge, the
  selection outline, the ribbon's fill, and the affordance's fill, border and
  label are redeclared in system colours. What survives without redeclaration
  is the ribbon's silhouette, the outline's offset, the resize corner's two
  arms, and the hatch. The presence the Note casts is canvas paint and does not
  survive, so the field falls back to a system-coloured border on the occupied
  region.
- **Text scaling** — the Note's own text does not scale with interface text: it
  is measured in cells and scales with the camera, so raising interface text
  disturbs neither the size nor the position of anything a person placed. The
  edit affordance's label is interface text, grows with it, and the affordance
  grows in place from the top-right corner without truncating or wrapping; it
  is drawn only while the hand or keyboard focus is on the Note, so the words
  beneath it are never hidden at rest.
- **Reduced motion** — no change from the Motion table.

## Copy

One string.

| String | Where | Why it passes |
| --- | --- | --- |
| `Edit` | The edit affordance's label, rendered uppercase by `--tr-mono` rather than by writing capitals. | A verb for an action, ordinary English, sentence case, no terminal punctuation, and true after any rebuild. |

Everything else is **None**. The Note's own text is authored content, not
copy: Grove never edits it, shortens it, summarises it, prefixes it, or adds a
word of its own to it. The refusal sentence beside a refused footprint belongs
to the Grid's refusal strip and is written there.

## Refusals

- **A title bar** — a header restates words the Note already is, and the first
  line of a Note is its own title.
- **An ellipsis** — an ellipsis withholds part of the thought inside a
  footprint that could have grown instead.
- **A persistent button row** — controls that never leave turn a thought into a
  card and charge the whole Grid, permanently, for an occasional action.
- **An interior scrollbar** — a scrollbar hides part of the thought inside the
  Note's own footprint; when the text grows, the footprint grows.
- **A non-square footprint** — a Note solves to a square, so a rectangle means
  the solver was overridden and the words no longer decide the size.
- **Type below `--t-body`** — the footprint answers the text; the text is never
  disciplined by the footprint.
- **A fourth authored fill** — three colours keep a colour a choice a person can
  remember, and a fourth is legal only when it clears both ratios in
  `00-foundations/Accessibility.md`.
- **A signal hue as the fill or as the presence** — a colour a person picked is
  content, and a signal that also means "a person picked this" has stopped
  being a signal.
- **A glow on selection** — Grove has two shadows and no glow, and a glow
  crosses cell boundaries that selection must respect.
- **A kind badge** — the badge names a frame whose picture cannot declare its
  kind, and a Note says what it is in its own words.
- **A count, a date, or any figure on the Note's surface** — a figure on the
  surface competes with the words for the one glance the Note exists to answer.


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

- Catalogue deck: `docs/design_catalogue/src/grid-plane/02-note.html` — the contract for this component.
- Catalogue deck: `docs/design_catalogue/src/grid-plane/01-the-grid.html` — the placed form at cell `220px`.
- Catalogue deck: `docs/design_catalogue/src/grid-plane/05-presence-fields.html` — cell-quantized presence and hue accumulation.
- Catalogue deck: `docs/design_catalogue/src/grid-plane/06-distance.html` — the shedding order and the kind-coded stand-in.
- Catalogue deck: `docs/design_catalogue/src/grid-plane/07-selection-placement.html` — the selection outline outside the edge.
- Source note: `docs/raw/original-notes/Grove - notes.txt` — a Note embodies a physical sticky note, and creating one is a single action.
- Reference: `docs/reference/Keybind map.md` — every key named above.
