---
type: design-system-component
status: active
date: 2026-08-09
component: Placement preview
plane: grid
surface_class: placement
tags: [grove, design-system, component]
---

# Placement preview

The cells a Note, a Document, or a picture will occupy, drawn under the pointer
while a person is still deciding.

## Anatomy

Five parts. The preview holds no content, so it has no body, no measure, and no
padding of its own.

| Part | Required | Value |
| --- | --- | --- |
| Footprint fill | yes | `--signal-interaction` at `0.06`; `--signal-refusal` at `0.12` when refused. Flat, no gradient, no shadow. |
| Inset stroke | yes | `2px` inset, `--signal-interaction` at `0.60`; `--signal-refusal` at `0.85` when refused. Drawn inside the footprint so the preview never grows past the cells it promises. |
| Dashed edge | yes | `1px` dashed on all four sides, `--signal-interaction` at `0.80`; `--signal-refusal` at `0.90` when refused. |
| Origin marker | yes, when the gesture has a source already placed on this Layer | `1px` dashed, `--ink` at `--ink-faint`, on the source's exact cells. No fill. |
| Origin content | yes, with the origin marker | The source's own drawing, unchanged, held at `--ink-faint`'s alpha as an opacity. |

The refusal hatch on the cells that cannot take the footprint is the mark owned
by `00-foundations/Marks.md` and is drawn beneath this component, not by it.

**Containment edge.** The dashed edge is the containment edge, and it is the one
part that may never be dropped, because it is the only thing in the frame that
says the footprint is a promise rather than a fact.

**Why this component owns six alphas.** `0.06`, `0.60`, and `0.80`, and their
refused counterparts `0.12`, `0.85`, and `0.90`, are component figures rather
than ink-ramp steps: the three layers must stay separable by
weight at every camera scale, and the ramp has no step between `--ink-secondary`
and `--ink-primary` fine enough to keep the inset stroke and the dashed edge
distinct. The refused set is composed heavier throughout because
`--signal-refusal` reaches 5.63 on canvas against `--signal-interaction`'s 9.50,
so equal alphas would draw a quieter refusal than a permission.

**Corrections to the evidence.** Deck 05 draws the origin marker at `0.28`;
`--ink-faint` wins, because it is the ramp step that means spent.

## Geometry

- **Footprint** — exactly the cells the content will occupy, never a bounding
  box and never one cell more. The procedure: take the footprint of the content
  being placed, in cells, unchanged by the gesture; take the cell under the
  pointer; translate the footprint so the cell the gesture was pressed on sits
  under the pointer. For a placement with no source on the field — a Memory sent
  from a Slate, a paste, a Trace drop-off, an imported picture — the footprint's
  top-left cell sits under the pointer, because there is no grab offset to keep.
  Two readers running this on the same frame get the same cells.
- **Growth** — none. The preview carries no content, so nothing can outgrow it;
  its extent changes only when the promised footprint changes, and only a resize
  gesture changes that. This is the one place Grove's growth answer does not
  apply, because there is nothing inside to make room for.
- **Measure** — none. The preview draws no type.
- **Alignment** — all four edges land on major grid lines at every zoom.
- **Multiple placements** — one preview per placement being moved, each on its
  own destination cells. A single box around the group would promise cells that
  stay empty, and a repeated figure would make the preview a Grid readout.
- **Draw order** — above content and above the refusal hatch, below the Grid
  cursor and its trail. The marquee never shares a frame with a preview, because
  a sweep has no subject, so no order between the two is fixed.

## States

The preview exists only while a placement gesture is open. Seven of the nine
states are answered by not drawing it, and that is the answer, not an omission.

| State | Appearance | Notes |
| --- | --- | --- |
| Rest | Not drawn. | The Grid at rest carries content, lines, presence, and the cursor, and nothing else. |
| Approached | Not drawn. | Approach adds chrome to a placement; a preview is not a target for the hand. |
| Focused | Not drawn, and no ring. | Keyboard attention on the Grid is the Grid cursor, which already says which cell holds it. |
| Selected | Not drawn. | Selection belongs to placed content; a promise cannot be selected. |
| Engaged | Fill, inset stroke, and dashed edge in `--signal-interaction`; the origin marker on the source cells. | The only state in which this component is drawn. |
| Pending | No change from Engaged. | Validity is answered on the same frame the footprint enters a cell, so there is nothing to wait for and no pending form to hold. |
| Refused | The same four layers in `--signal-refusal` at the refused alphas, the refusal hatch on the blocked cells beneath, and one plain sentence in the strip beside the footprint. | The footprint does not move, resize, or snap away; refusal answers where the hand is. |
| Unavailable | Not drawn. | A gesture that cannot start draws nothing; unavailability is a condition on a control, and this is not a control. |
| Anchored | Not drawn, and no anchor mark. | Anchored is durable and authored; a preview is neither, even when the content it promises carries an Anchor. |

## Behaviour

- **Pointer** — pressing on a placement arms the move and draws nothing; the
  preview and the origin marker appear on the first frame the pointer enters a
  different cell, and the preview redraws on every cell boundary it crosses.
  Release commits at the previewed cells when the footprint is valid. Release on
  a refused footprint commits nothing and removes the drawing, because a refusal
  answered live has already been read.
- **Keyboard** — the preview answers no key of its own; it follows the Grid
  cursor's armed footprint, and every key belongs to the Grid or to the open
  session, per `docs/reference/Keybind map.md`: left click places, `Escape`
  cancels the gesture or clears the mode, `R` enters or leaves Resize, `T`
  starts or ends Trace, `M` opens moving content between Layers, `I` opens
  picture import, `Ctrl/Cmd+V` arms Paste, and `Ctrl/Cmd+Enter` commits an open
  Transfer, Trace, or Clipboard session. Focus never arrives on the preview and
  never leaves through it.
- **Focus order** — none. The preview holds no focusable part, so it adds
  nothing to the order around it.
- **Escape** — removes the preview and the origin marker on the same frame and
  restores the source to full strength. Attention returns to the Grid cursor at
  the cell it held; where the gesture began in a Slate, the invoking Slate is the
  return surface and `docs/ux/wireframes/Place a Memory.md` owns that hand-back.
- **Commit and cancel** — the release, or the session's `Ctrl/Cmd+Enter`, is the
  first and only durable change in the gesture. Cancel is `Escape`, a release on
  a refused footprint, or a cancelled pointer; each leaves the Grid exactly
  as it was, so there is nothing to undo. The arrival of the committed placement
  runs over `--d-place` on `--overshoot` and belongs to the placement, not here.

## Motion

| Transition | Duration | Curve | Reduced motion |
| --- | --- | --- | --- |
| Preview and origin marker appear | None — drawn on the frame the footprint first changes cell | None | No change; already immediate |
| Preview moves to the next cell | None — redrawn on the frame the pointer crosses the boundary | None | No change; already immediate |
| Valid to refused, and back | None — recoloured on the frame the footprint enters the cell | None | No change; already immediate |
| Preview and origin marker leave | None — removed, never faded | None | No change; already immediate |

Every row is immediate on purpose. A tween on movement would put the drawing
behind the hand, a tween on validity would report the previous cell's answer, and
a fade on removal would read as a gesture still open. The shipped preview runs
`ghostpulse` at `1.5s` and `ghostblink` at `0.5s`, both `infinite`, in
`css/interactions.css`; both break Law 5 and both are removed rather than
retimed.

## Distance

| Tier | Sheds | Keeps |
| --- | --- | --- |
| Working | Nothing. | Fill, inset stroke, dashed edge, origin marker. |
| Stepped | Nothing. | Fill, inset stroke, dashed edge, origin marker. |
| Stand-in | The inset stroke. | Fill, dashed edge, origin marker. |

The inset stroke goes at the stand-in tier because two concentric
strokes inside a footprint that small read as one thick edge and lie about the
extent. The dashed edge never goes, because it is the whole of what separates a
promise from a fact. Position and extent never go: the preview covers its exact
cells at the projection's true size at every distance, and nothing about it is
pinned to a screen size.

## Accessibility

- **Role and name** — none. The preview is drawn geometry with no role, no name,
  and no place in the tab order, and it is hidden from assistive technology, the
  way the field, the lines, and the cursor trail are. The one fact a person
  cannot otherwise perceive — that the current footprint cannot be placed — is
  announced once per entry into refusal by the refusal strip's live region, which
  owns the sentence.
- **Contrast** — the inset stroke reads 4.05:1 valid and 4.35:1 refused, and
  the dashed edge 6.38:1 valid and 4.74:1 refused, all clear of the
  3:1 non-text minimum. The `0.06` and `0.12` fills carry no meaning alone and
  are exempt.
- **Without colour** — Engaged is carried by the dashed edge, which no placement
  or piece of chrome ever draws. Refused is
  carried by the 45° hatch on the blocked cells and the plain sentence in the
  strip, exactly as `00-foundations/Accessibility.md` requires. In greyscale a
  valid preview still reads as an empty dashed rectangle over lit cells, and a
  refused one still reads as a dashed rectangle over striped cells.
- **Forced colours** — the fill is paint and does not survive; the dashed edge,
  the inset stroke, the hatch, and the origin marker are redeclared in system
  colours, and the dashed pattern itself survives without redeclaration. This is
  why the dashed edge is mandatory: in forced colours it is frequently the only
  carrier of the promise left.
- **Text scaling** — nothing reflows. The preview draws no type, so raising
  interface text to 200% changes no preview size, position, or extent.
- **Reduced motion** — no change from the Motion table; every transition is
  already immediate.

## Copy

None.

The refusal sentence — `This space is occupied` — is drawn in the strip beside
the footprint and is owned by the Grid's refusal, not by this component.

Deck 07 replaces the footprint with the word `Occupied` in the corner when it is
refused; that is corrected. Refusal already carries hue, hatch, and one plain
sentence beside the footprint; the preview carries no second wording.

## Refusals

- **A solid preview identical to placed content** — two identical forms where one
  does not exist yet gives a person no way to tell which survives letting go
  except by committing and checking.
- **Any idle loop** — a pulse, a blink, a breath, a spin, or a marching dashed
  edge claims attention forever for a fact that was fully delivered the first
  frame.
- **Authored content inside the preview** — text, an authored fill, a picture, a
  title block, or a page texture makes the promise look like the thing.
- **A shadow or a glow on the preview** — depth says a thing is there, and the
  preview is a claim about cells that are still empty.
- **The active-work hue on a preview, including a Trace** — amber says a gesture
  has produced nothing yet; a preview already has a subject and takes the
  interaction signal.
- **Moving the source before release** — the origin keeps its cells and its
  content until the commit, so letting go early costs nothing.
- **Dimming, washing, or outlining the source content to report refusal** — the
  answer belongs on the cells that cannot take the footprint, not on the thing
  being moved.
- **A partial cell, a sub-cell offset, or a snap to screen pixels** — the
  footprint promised must be the footprint taken.
- **Counter-scaling to a fixed screen size** — a preview pinned at a constant
  size on screen lies about where and how big the placement will be.
- **A count, an ordinal, a coordinate readout, or a status word on the preview** —
  the covered cells already state the extent.
- **A focusable, hit-testable, or selectable preview** — it is a drawing, and a
  drawing that answers the pointer has become chrome.
- **Deferring refusal to release** — validity is answered on every cell the
  footprint crosses, so a refusal is never a surprise at the end.


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
- Catalogue deck: `docs/design_catalogue/src/grid-plane/05-presence-fields.html`
- Decision: `docs/decisions/Memory placement footprint and return.md`
- Wireframe: `docs/ux/wireframes/Place a Memory.md`
- Reference: `docs/reference/Keybind map.md`
