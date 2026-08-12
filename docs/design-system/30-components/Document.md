---
type: design-system-component
status: active
date: 2026-08-10
component: Document
plane: grid
surface_class: placement
tags: [grove, design-system, component]
---

# Document

A piece of writing placed on the Grid, shown as its front page: the one
thing a person puts down that is made of paper.

## Anatomy

Every visible part, named, with the token that gives it its value. A part with
no token is a part with no contract.

| Part | Required | Value |
| --- | --- | --- |
| Page | yes | Fill `--surface-page`; corners `--r-none`; a 1px inset containment edge in `--paper-edge`; padding `26px 28px`. |
| Page texture | yes | Horizontal and vertical 1px rules at `44px` in `--paper-texture-minor`, and at `220px` in `--paper-texture-major`, drawn in page space beneath every zone. |
| Front matter | no | `--f-mono` 600, `--t-label`, `--tr-caps`, uppercase, ink `--paper-label`. One line, full interior width. |
| Display title | yes | `--f-display` 500, `--t-display`, `--lh-tight`, `--tr-title`, uppercase, ink `--c-paper-ink` at full strength; `10px` clear above it. Full interior width, wrapping freely. |
| Full rule | no | 1px in `--paper-rule`, full interior width, `18px` clear above and below. |
| Abstract | no | `--f-ui` 400, `--t-body`, `--lh-reading`, ink `--paper-body`, width `--measure-reading`. |
| Tight rule | no | 1px in `--paper-rule`, `56px` wide, `--sp-md` above and `12px` below. |
| Closing line | no | `--f-ui` 400, `--t-dense`, `--lh-reading`, ink `--paper-body`, width `--measure-reading`. |
| Spacer | yes | Takes all remaining interior height; minimum `--sp-md`. |
| Title block | no | A row of up to two cells at the foot, aligned to the right interior edge, boxed in 1px `--paper-border` with a 1px `--paper-border` divider between cells; each cell `7px 13px` padding, `--f-mono`, `--t-label`, `--tr-label`, uppercase, ink `--paper-label`. |
| Resize corner | no | `00-foundations/Marks.md`, drawn in `--paper-strong`. Present on Approached and Selected only. |
| Anchor ribbon | no | `00-foundations/Marks.md`, filled `--signal-authored-context`, its left edge `--sp-md` from the page's left edge. Present on Anchored only. |

The containment edge is the 1px inset edge in `--paper-edge` and nothing else:
a placed Document has no border drawn outside its footprint, no radius, and no
shadow, because a frame around content turns the Grid into a card layout.
The fill is `--surface-page` over the whole footprint, and it is the only light
surface a person places. Padding is `26px 28px` on all four sides; this is a
component figure rather than a step on the space scale because it is a page
margin set by the paper's proportions, not chrome rhythm between controls, and
`00-foundations/Tokens.md` names it as this component's own value. The `10px`
above the title, the `18px` around the full rule, the `12px` below the tight
rule, the `56px` tight-rule width, and the `7px 13px` title-block cell are
component figures for the same reason: they are optical clearances between two
specific type roles on paper, and the space scale has no step that lands on
them.

Five zones carry the page — front matter, title, abstract, closing line, title
block — and the two rules and the spacer are the joints between them. The
source note fixes this shape: a title, front matter in place of an author, the
equivalent of an abstract and the equivalent of a conclusion, at a length that
is representative rather than legible.

Where a Document has no written title, the title zone shows the document's own
file name, because the title is this component's accessible name and Grove
never writes a placeholder onto a person's page. A rule is drawn only when the
zone beneath it is drawn, because a rule above nothing is a mark for absent
content.

Three inks in the corpus are corrected here. The front matter is
`--paper-label`, not the `0.48` both the deck and `css/materials.css` render,
because `0.48` reaches only 3.09:1 on paper and `00-foundations/Tokens.md`
already fixes the separation as a type role — mono, uppercase, `--t-label`,
`--tr-caps` — rather than an ink step. The closing line is `--paper-body`, not
the deck's `0.50` at 3.28:1, so its demotion is carried by `--t-dense` and the
tight rule instead. Both title-block cells are `--paper-label`, not `0.68` and
`0.50`, because two unequal off-ramp alphas rank one authored field above the
other while the divider already separates them.

The page draws every required zone. Optional zones draw only when their authored
content exists; an absent zone creates no placeholder and no replacement label.

## Geometry

- **Footprint** — a whole-cell rectangle, solved by procedure. (1) Set the page
  at the type roles above, with the abstract and the closing line at
  `--measure-reading`. (2) Width is the smallest whole number of cells `w`
  where `w × --grid-cell − 56px` is at least the rendered width of
  `--measure-reading` at `--t-body` in `--f-ui`; at `--grid-cell` `220px` a
  single cell leaves `164px` of interior and never holds it, so `w` is 2 or
  more. (3) Height is the smallest whole number of cells `h` where
  `h × --grid-cell − 52px` holds every drawn zone at its specified type and
  spacing with the spacer at `--sp-md`. (4) A person may resize to any larger
  whole-cell rectangle; a resize below the solved minimum is Refused. Square is
  not required — the catalogue renders both a 2×2 and a 4×5 Document, and both
  are correct.
- **Growth** — the footprint grows. Added content re-runs step 3 and the height
  takes the next whole cell; the width changes only by an explicit resize,
  because a page that widened itself would rewrap text a person had already
  read. Type size never changes, and nothing is clipped, scrolled, or
  summarised to fit.
- **Measure** — `--measure-reading` on the abstract and the closing line. The
  front matter, the title, and the title block are single lines that take the
  full interior width, because a measure applied to a title wraps it against
  its own `--lh-tight` leading for no reading benefit.
- **Alignment** — all four footprint edges land on major grid lines. The page's
  content box is inset from those edges by the `26px 28px` padding; the title
  block is aligned to the right interior edge and the spacer pushes it to the
  bottom one, so a short Document reads as a finished page rather than an
  unfinished one.

## States

All nine. `—` is not an answer; write "No change from Rest" where that is
true.

| State | Appearance | Notes |
| --- | --- | --- |
| Rest | Paper fill, texture, inset edge, and the drawn zones. Nothing else. | No title bar, no toolbar, no kind badge, no outline. |
| Approached | The resize corner fades in at the bottom-right over `--d-fade`. | Nothing on the page itself changes; approach adds and never restyles. |
| Focused | No change from Rest. | Keyboard attention is the Grid cursor at the focused cell; the page draws no second ring. |
| Selected | A `2px` `--signal-interaction` outline offset `3px` outside the page edge, the cells around the footprint brightened, and the resize corner present. | The page is never tinted, dimmed, washed, or restyled. |
| Engaged | The previewed footprint is drawn in `--signal-interaction` while a move or a resize is open; the page keeps its place until release. | A placement preview is never `--signal-active-work`; amber belongs to a sweep, which has no subject. |
| Pending | The footprint holds, showing the fullest form already available — the stand-in when nothing fuller exists. | Never blank, never a spinner. The arriving form replaces it in place over `--d-swap`. |
| Refused | The previewed footprint takes a 1px inset edge in `--signal-refusal`, the cells that cannot accept it are hatched at 45°, and one plain sentence sits beside the footprint with the committing action present and unavailable. | The Document's own paper is never hatched and never tinted; the hatch covers whole cells of the field. |
| Unavailable | Never drawn on a placed Document. | A Document whose content has not arrived is Pending, and an attempt that is answered is Refused; a placed Document is otherwise always actionable. |
| Anchored | The anchor ribbon hangs from the page's top edge, and the presence the footprint casts takes `--signal-authored-context` in place of the neutral ink. | Durable and authored. Geometry carries it, so a colour-blind read still lands. |

## Behaviour

- **Pointer** — pressing anywhere on the page selects it, and selection commits
  on pointer-down so the hand is never held. Dragging from the page moves it;
  the move previews live in whole cells and commits on release, and only when
  every destination cell on the Layer is free. Dragging the resize corner
  changes the footprint in whole cells and commits on release. Double-clicking
  the page opens it for writing — a decision, because desktop convention makes
  double-click the open gesture and the corpus fixes no other pointer route to
  a deliberate open. A context gesture opens the content menu on the Document.
- **Keyboard** — this component declares no key of its own. The keys that act
  on a selected Document are the Grid keys in
  `docs/reference/Keybind map.md`: `R` to enter and leave Resize, `A` to open
  the Anchor editor, `Ctrl/Cmd+C`, `Ctrl/Cmd+X`, `Ctrl/Cmd+D`, `Delete` and
  `Backspace`, `Ctrl/Cmd+Z`, and `Escape` to cancel the open gesture.
  `Shift+F10` and the Menu key open the content menu on the selected Document —
  a decision, because `00-foundations/Accessibility.md` requires a keyboard
  route to every action and this is the desktop convention for reaching a
  context menu; the keybind map gains the row in the same change.
- **Focus order** — none within the component. A placed Document has no
  focusable parts, because it is content rather than a control group, and focus
  never lands on a container that does nothing.
- **Escape** — nothing inside a Document dismisses. Escape on the Grid
  cancels the open move, resize, or placement gesture and returns attention to
  the Grid cursor at its last cell.
- **Commit and cancel** — a move or a resize is durable on release into a free
  footprint. Escape or a cancelled pointer during either restores the origin
  exactly, at no cost, because nothing durable changes while a gesture is open.
  `Ctrl/Cmd+Z` undoes a committed move, resize, or removal.

Opening a Document for writing, and everything that happens once it is open,
belongs to `docs/ux/wireframes/writing-slate/`. Placing a Document and
returning from it belong to `docs/ux/wireframes/Place a Memory.md`.

## Motion

| Transition | Duration | Curve | Reduced motion |
| --- | --- | --- | --- |
| Resize corner arriving on approach or selection | `--d-fade` | `--ease` | Present at the identical pointer threshold. |
| Resize corner leaving with the pointer | `--d-fade` | `--ease` | Absent at the identical pointer threshold. |
| Arrival on placement | `--d-place` | `--overshoot` | Drawn at its true footprint on the first frame. |
| Leaving the Grid | `--d-exit` | `--ease` | Gone on the first frame. |
| Exchanging one representation for another | `--d-swap` | `--ease` | Swapped at the identical threshold, in one frame. |

Nothing loops. Nothing idles. Nothing pulses or blinks. The page does not
scale, dip, or spring under a press, and no transition ends at anything but the
footprint's true extent, because no state may alter a placement's extent.

## Distance

How the component reads at each representation tier. Thresholds are on
projected cell size and belong to `10-grammar/Representation-tiers.md`; this
section says only what this component sheds and what it keeps.

| Tier | Sheds | Keeps |
| --- | --- | --- |
| Working | Nothing. | The whole front page, the page texture, the inset edge, and the resize corner on approach or selection. |
| Stepped | The resize corner, the inset edge, the page texture, and every word. | The paper fill, and each zone replaced in place by the mass it occupied — one bar for the front matter, two for the title, four for the abstract, one for the closing line — plus both rules, the spacer, and the title block as an empty bordered rectangle. |
| Stand-in | Every mass, both rules, and the title block. | The paper fill on the exact footprint, the inset `--paper-border` edge, and one ruled head bar above four body rules. |

Each stepped mass takes the ink of the type it replaced — `--paper-label` for
the front matter, `--c-paper-ink` for the title, `--paper-body` for the
abstract and the closing line — so the page's hierarchy survives the loss of
the words. The catalogue draws these masses at `0.78` and `0.42`; both are
corrected to the paper ramp, because the ramp is seven steps and a mass is not
a new kind of ink.

The stand-in has a kind-coded form and it is drawn in proportions of the
footprint, never at a fixed authoring size: a head bar from `18%` to `70%` of
the width, at `16%` of the height, `5%` tall with a `2px` minimum; then four
rules at `38%`, `52%`, `66%`, and `80%` of the height, each starting at `14%`
of the width, `62%`, `50%`, `60%` and `42%` wide, each `2px`. All five are
`--c-paper-ink`. The Document deck authors this mark at a fixed `96px` and
scales it; the Distance deck authors it in percentages and that is correct,
because a stand-in has no geometry of its own and a fixed authoring size is a
minimum size in disguise that would distort on a non-square footprint. The
head bar and rules are `--c-paper-ink` rather than the `0.44` and `0.40` both
decks render, because the grammar fixes this mark at full ink and it is the
last thing carrying kind.

Presence, position, and extent never shed. A Document casts its presence in the
neutral `--ink` triplet, not in paper, because a Document has no authored fill
and a paper-white field would outshine the content standing on it; the cells
are computed by the field's own rule, clamped between `--field-alpha-min` and
`--field-alpha-max`, and the occupied region carries the perimeter ring at
`--field-perimeter-ink`, rising to `--field-perimeter-selected` when selected.
Where the Document is Anchored the hue is `--signal-authored-context` instead,
in every tier including the stand-in.

## Accessibility

- **Role and name** — role `option` within the Grid's list of placements,
  with `aria-selected` tracking Selected. The accessible name is the Document's
  title; where it has none the name is its file name. A generic name such as
  `Document` is a defect, because it replaces the one thing that distinguishes
  this page from every other. The list itself is named
  `Notes, Documents and pictures on this Layer`; `index.html` line 39 names it
  `Content on current Layer`, which `10-grammar/Copy.md` already corrects.
- **Contrast** — display title `--c-paper-ink` on `--surface-page` 15.97:1;
  front matter, abstract, closing line, and both title-block cells
  `--paper-label` and `--paper-body` on `--surface-page` 4.75:1; resize corner
  `--paper-strong` on `--surface-page` 9.36:1. Non-text: the page against the
  Grid 17.71:1; the inset edge `--paper-edge` and the two rules
  `--paper-rule` at 1.39:1 and the title-block box `--paper-border` at 1.82:1,
  all legal as drawn boxes and rules and none of them meaning-bearing; the
  selection outline `--signal-interaction` 9.50:1 on canvas; the anchor ribbon
  `--signal-authored-context` 6.79:1 on canvas; the refusal edge and hatch
  `--signal-refusal` 5.63:1 on canvas.
- **Without colour** — Selected is the outline's offset outside the page edge
  plus the brightened cells; Engaged is the previewed footprint's own
  rectangle; Refused is 45° hatching plus one plain sentence; Anchored is the
  ribbon's notched silhouette; Pending is the stand-in form holding position
  and extent.
- **Forced colours** — the paper fill, the paper ink, the inset edge, both
  rules, the title-block box, the selection outline, the refusal edge, and the
  ribbon fill are redeclared in system colours. The page texture is a
  background image and does not survive; the page still reads as paper because
  its fill and ink pair is redeclared. Presence is canvas paint and falls back
  to a system-coloured border on the occupied region.
- **Text scaling** — nothing on the page scales with interface text. A placed
  Document is measured in cells and scales with the camera, so raising
  interface text to 200% leaves its size, position, and line breaks untouched.
  The abstract and closing line stay at `--measure-reading`, expressed in `ch`.
- **Reduced motion** — no change from the Motion table.

## Copy

None. Every string a placed Document shows is authored: its front matter, its
title, its abstract, its closing line, and up to two title-block cells. Grove
supplies no label, no placeholder, no caption, and no fallback.

| Content condition | Rule |
| --- | --- |
| Front matter absent | Omit the zone. |
| Title absent | Use the authored file name; never use a product-supplied placeholder. |
| Abstract absent | Omit the zone and its full rule. |
| Closing line absent | Omit the zone and its tight rule. |
| Title-block cell absent | Omit the cell; never synthesize an ordinal, type label, or status word. |

## Refusals

- **An ellipsis clip** — a cut sentence promises text the page will never show;
  a front page carries complete passages or fewer of them.
- **An interior scrollbar** — scrolling makes the placement a window with
  hidden depth and makes the Grid lie about what is present.
- **A filename card stand-in** — a filename proves only that a file exists and
  puts a dark container where a page belongs.
- **Type below its reading size** — shrinking type to fit is truncation with
  extra steps, and the footprint is what grows.
- **A kind badge or a kind word** — a Document says what it is in its own type
  and ink, so a badge covers content to repeat what is already legible.
- **A frame, a radius, a shadow, or a title bar** — a container per placement
  turns the field into a card layout and stops position carrying meaning.
- **Restyling the paper for a state** — selection, engagement, and refusal are
  drawn outside the content edge or on the field, never on the page.
- **A fixed on-screen size** — the page's size is its footprint under the
  camera; content pinned to the screen floats free of the field.


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

- Catalogue deck: `docs/design_catalogue/src/grid-plane/03-document.html`
- Catalogue deck: `docs/design_catalogue/src/grid-plane/01-the-grid.html`
- Catalogue deck: `docs/design_catalogue/src/grid-plane/05-presence-fields.html`
- Catalogue deck: `docs/design_catalogue/src/grid-plane/06-distance.html`
- Catalogue deck: `docs/design_catalogue/src/grid-plane/07-selection-placement.html`
- Source note: `docs/raw/original-notes/Grove - Documents.txt`
- Decision: `docs/decisions/Memory placement footprint and return.md`
- Decision: `docs/decisions/Camera zoom range and representation.md`
- Decision: `docs/decisions/Content concentration and layout rules.md`
- Wireframe: `docs/ux/wireframes/Place a Memory.md`
- Wireframe: `docs/ux/wireframes/writing-slate/`
