---
type: design-system-component
status: active
authority: derived-from-domain-model
source_of_truth: ../../domain/Memory-Slate.md
date: 2026-08-10
component: Memory Slate
plane: hud
surface_class: slate
tags: [grove, design-system, component]
---

# Memory Slate

Every MemoryRecord made in Grove, whether it currently has Content or not,
gathered in one place so a person can look through it and pick one out. The
surface names itself `Memories`, and that is the only name it shows.

## Identity contract

- The gallery source is the Memory ledger, never a collection of Content
  representatives.
- The gallery has one card per MemoryRecord. Content and Anchors are metadata
  on that card, not additional cards.
- A Memory referenced by 50 Content instances appears once. Its card may show
  derived Content/Anchor context, but those relationships are not owned by the
  Memory record.
- A Memory with zero Content instances appears normally. It is not omitted,
  disabled, or represented by an empty Grid card.
- `Place` creates a new Content instance referencing the selected Memory. An
  Anchor is created only when the user supplies authored Content context. It
  never creates a second Memory merely because the destination is different.
- The Memory Slate is a browse and handoff surface. Text editing belongs to
  Writing Slate; image inspection belongs to Gallery Slate. Opening a record
  does not mutate the Memory or create Content.

## Anatomy

Every visible part, named, with the token that gives it its value. A part with
no token is a part with no contract.

| Part | Required | Value |
| --- | --- | --- |
| Frame, identity line, header controls | yes | The Slate class of `10-grammar/Surface-classes.md`, unchanged, and not restated here. The identity line reads `Memories`, and the header carries the host's commands only. |
| Search field | yes, unless nothing is kept | The body's full width. Fill `--surface-nested`; 1px `--edge-control`; `--r-sm`; padding `--sp-sm` top and bottom and `--sp-md` at the sides. Typed text `--f-ui` 400 at `--t-body`, `--lh-ui`, `--text-primary`; the placeholder the same type at `--text-meta`. `--sp-lg` below the identity line. |
| Filter row | yes, unless nothing is kept | Four controls on one line, `--sp-lg` apart, aligned to the body's left interior edge. `--sp-md` below the search field, `--sp-lg` above the gallery. |
| Filter, resting | yes | `--f-ui` 400 at `--t-caption`, the label's own case, `--text-meta`, with `--sp-sm` of clear beneath the label and no rule. |
| Filter, on | yes | The same type at `--text-secondary` over a 1px rule in `--edge-control`, the rule running the label's width. Exactly one filter is on. |
| Gallery | yes | Masonry columns of cards. Column gutter and the gap between cards in a column are both `--sp-sm`. No column rule, no card container, no row alignment. |
| Note card | yes | Fill `--surface-nested`; 1px `--edge-hairline`; `--r-sm`; padding `--sp-md`. The Note's complete text in `--f-ui` 400 at `--t-body`, `--lh-ui`, `--text-primary`. |
| Picture card | yes | The complete source at its intrinsic proportions, corners `--r-sm`. No fill, no padding, no mat, no edge, and nothing drawn over it. |
| Document card | yes | Fill `--surface-page`; `--r-sm`; padding `--sp-md`. Title `--f-ui` 500 at `--t-body`, `--tr-title`, `--lh-ui`, ink `--c-paper-ink`, in the source's own case. Front matter `--f-mono` 500 at `--t-label`, `--tr-caps`, uppercase, ink `--paper-label`, `--sp-xs` beneath the title. Abstract `--f-ui` 400 at `--t-dense`, `--lh-reading`, ink `--paper-body`, `--sp-sm` beneath the front matter, complete. |
| Card metadata line | yes | One line, `--f-mono` 500 at `--t-label`, uppercase by `--tr-label`, ink `--text-meta`, `--sp-sm` beneath the card. It identifies payload form and the number of Content instances. Never over a card, never wrapped. |
| Selection outline | Selected only | `2px` `--signal-interaction`, offset `3px` outside the card's box, drawn outside so no card moves. |
| Focus ring | Focused only | `--focus-ring` at `--focus-ring-offset`; at `6px` offset when the card is also Selected, which leaves a 1px gap outside the selection outline. |
| Anchor mark | Anchored only | The ribbon of `00-foundations/Marks.md` on a Note card and a Document card, its left edge `--sp-md` from the card's left edge; the diamond on a picture card, offset `-4px` on both axes from the frame's top-left corner. |
| Unavailable frame | Only when a picture cannot be shown | The frame's box at the picture's own proportions, fill `--surface-nested`, 1px `--edge-found`, `--r-sm`; one sentence in `--f-ui` 400 at `--t-caption`, `--text-secondary`, centred at `--measure-reading` or narrower; the quiet control `Retry` `--sp-sm` beneath it. No refusal hue. |
| Quiet control | yes | `--f-ui` 400 at `--t-caption`, `--text-secondary`; 1px `--edge-control`; `--r-sm`; padding `--sp-sm` top and bottom, `--sp-md` at the sides. Carries `Back`, `Show all`, and `Retry`. |
| Primary action | Record only | `Open` — routes text to Writing Slate and media to Gallery Slate. Fill `--signal-interaction`, label `--c-paper-ink` in `--f-ui` 400 at `--t-caption`, `--r-sm`, the same padding as the quiet control, no border. One per record surface. |
| Record: canonical Memory | Record only | The Memory at the largest size the body allows, complete and at its own proportions, with the card's metadata line beneath it extended by the picture's dimensions and file size in the same type role. |
| Record: title | Record only | The Memory's own name in `--f-ui` 500 at `--t-title-small`, `--tr-title`, `--lh-ui`, `--text-primary`, in the source's own case. |
| Record: description | Record only | The words a person wrote about it, `--f-ui` 400 at `--t-dense`, `--lh-reading`, `--text-secondary`, at `--measure-reading`, `--sp-sm` beneath the title. |
| Record: separator | Record only | 1px `--edge-hairline`, the detail column's full width, `--sp-lg` above and below. |
| Record: provenance | Record only | Rows `--sp-sm` apart. Label `--f-mono` 500 at `--t-label`, `--tr-label`, uppercase, `--text-meta`, in a column as wide as its longest label; value `--f-ui` 400 at `--t-caption`, `--text-secondary`, `--sp-md` after the label column. An Anchor's value takes `--signal-authored-context` and is preceded by the anchor diamond at `--sp-sm` clear. |

The frame, the identity line, and the header controls belong to
`10-grammar/Surface-classes.md`; this component adds no header control and no
second identity. `--edge-control` is `--ink` at `--ink-tertiary`: the token table
carries no border value that clears the 3:1 floor
`00-foundations/Accessibility.md` sets for a control boundary a person must
find — `--edge-hairline`, `--edge-quiet`, and `--edge-found` all fall below it —
so `00-foundations/Tokens.md` should carry `--edge-control` at that step and the
search field, the filter rule, and every quiet control spend it.
`--k-slate-b`, which `Surface-classes.md` names and the token table does not
carry, measures `--k-slate` at `0.35`: six deck frames render that value
(`00-design-language.html:178`, `01-menus.html:237` and `:327`,
`hud/02-memory-slate.html:44`, `information-plane/02-image-viewer.html:384`,
`information-plane/08-annotation-routes.html:173`) against one at `0.28`
(`hud/01-slate-anatomy.html:43`), so `0.35` is the design and `0.28` is drift.

The `6px` focus offset is this component's own figure. The selection outline is
`2px` at a `3px` offset, so its outer edge stands `5px` clear of the card;
`10-grammar/States.md` requires the focus ring to sit outside that outline with
a visible gap, and `6px` is the first offset that leaves one.

A card is not a box. The Note card is a raised surface because text needs one;
the picture card is the picture, and the Document card is paper. There is no
uniform card container, no title row, no icon, no kind glyph, and no control
that is present at rest.

## Geometry

- **Footprint** — the pane's footprint is the host's: Full, Left, or Right, per
  `10-grammar/Surface-classes.md`. What this component decides is the column
  count, as a procedure. (1) `body` is the pane's interior width, its width less
  `--sp-lg` on each side. (2) `col` is `--measure-reading` rendered at `--t-body`
  in `--f-ui`, plus `2 × --sp-md` for a card's padding. (3) `g` is `--sp-sm`.
  (4) `n = max(1, floor((body + g) ÷ (col + g)))`. (5) Every column is
  `(body − (n − 1) × g) ÷ n` wide. The same pane width yields the same `n` every
  time, because nothing in the procedure reads the content.
- **Growth** — the pane scrolls. Columns never narrow past `col`; they drop in
  number, one at a time, and the measure holds. No card shrinks, crops,
  summarises, or clips to avoid a scroll, because the surface is chrome and the
  cards carry authored work.
- **Measure** — `--measure-reading` on a Note card's text, on a Document card's
  abstract, on the record's description, and on the empty state's first line.
  The metadata line, the filters, and the provenance rows are single lines and
  take no measure. The deck's `~250px` column minimum is the same figure
  expressed in pixels, which `00-foundations/Typography.md` refuses; step 2
  above is that minimum stated in `ch`.
- **Alignment** — the identity line, the search field, the filter row, and the
  first column all start on the body's left interior edge; the header controls
  end on the right interior edge. Cards are placed in gallery order, each into
  the column with the least accumulated height and ties to the leftmost, so
  column bottoms stay ragged and the order stays readable. Gallery order is most
  recently kept first, which is the only order a person can predict without
  reading and the one the corpus leaves open.

## States

All nine.

| State | Appearance | Notes |
| --- | --- | --- |
| Rest | The identity line, the search field, the filter row with `All` on, and the cards. No control on any card. | The surface at rest is a wall of content and four quiet words. |
| Approached | No change from Rest. | A card carries no chrome that serves the hand, and `10-grammar/Signal-roles.md` raises no interaction accent on hover outside an open menu. |
| Focused | The focused card takes `--focus-ring` at `--focus-ring-offset`; the search field, a filter, and a control take the same ring on `:focus-visible`. | Focus is never suppressed, and the gallery holds one tab stop with a roving position. |
| Selected | The selected card takes a `2px` `--signal-interaction` outline offset `3px` outside its box, and its metadata line rises from `--text-meta` to `--text-secondary`. | One card at a time. The card's own surface, text, picture, and paper are never tinted, dimmed, washed, or restyled. Selection is never stored. |
| Engaged | Selection commits on pointer-down, so the outline drawn on that frame is the press acknowledgement and no separate pressed appearance exists. | There is no drag, no sweep, and no resize inside this surface, so no gesture stays open across frames. |
| Pending | A card whose picture has not decoded holds its box at the picture's own proportions in `--surface-nested`, with its metadata line already drawn. | Never blank, never a spinner. The picture replaces the box in place over `--d-swap`, at the same extent. |
| Refused | Never drawn here. | Nothing inside this surface can be refused: narrowing that catches nothing is a designed result, a picture that will not decode is Unavailable, and a placement that cannot land is answered on the Grid where the cells are, with this surface unchanged. |
| Unavailable | A picture that cannot be shown keeps its card, its proportions, and its metadata line, and shows one sentence and `Retry` inside its frame on a `--edge-found` border. A filter with no Memories of that form is `--text-unavailable` on a `--edge-hairline` border, in place. | `10-grammar/Signal-roles.md` already rules that content which will not load carries no refusal hue, because nothing was refused. |
| Anchored | The card carries the anchor ribbon or the anchor diamond by form, and the record's provenance carries the Anchor's own words in `--signal-authored-context` behind an anchor diamond. | Durable and authored. Geometry carries it, so a colour-blind read still lands. |

Combination follows `10-grammar/States.md` without exception. Focused plus
Selected draws both lines, the focus ring outside the selection outline at the
`6px` offset above.

## Behaviour

- **Pointer** — a single click on a card selects it, committing on pointer-down
  so the hand is never held; clicking another card moves the selection, and
  clicking the gallery's empty space clears it. A double-click on a card opens
  its record — a decision, because `30-components/Picture.md` and
  `30-components/Document.md` already open content that way and a second route
  would be a second model of one action. Clicking a filter turns it on and turns
  the previous one off. Clicking the search field places the caret. `Retry`,
  `Show all`, `Back`, `Open`, and `Place` commit on release over the control.
- **Keyboard** — `S` and `/` focus the search field, per
  `docs/reference/Keybind map.md`, and neither fires while a text field already
  holds focus. In the gallery: `↓` and `↑` move to the next and previous card in
  the same column; `→` and `←` move to the card in the neighbouring column whose
  vertical midpoint is nearest the current card's; `Home` and `End` move to the
  first and last card in gallery order. Arrows do not wrap, because a wrap makes
  the next press unpredictable in columns that hold different numbers of cards.
  `Space` selects the focused card. `Enter` opens its record. In the filter row,
  `←` and `→` move between filters and turn the one they land on on, which is
  the radio-group convention `00-foundations/Accessibility.md` inherits.
- **Focus order** — search field, then the filter row as one stop, then the
  gallery as one stop, then the record's `Back` and its two actions when the
  record is open. Focus arrives on the search field when the pane opens, because
  it is first in reading order and finding is what the surface is for. Opening a
  record moves focus to `Back`; returning from a record puts focus back on the
  card it was opened from, with the gallery's scroll position and selection
  exactly as they were.
- **Escape** — Escape peels one layer per press: with text in the search field
  and focus in it, Escape empties the field and keeps focus there; with the
  record open, Escape returns to the gallery; with a card selected, Escape
  clears the selection; with none of those true, Escape closes the pane and
  `10-grammar/Surface-classes.md` owns where focus lands. The first of those
  rows is added above `Slate pane` in that document's escape order, because a
  narrowing a person entered from the keyboard must be leavable from the
  keyboard.
- **Commit and cancel** — nothing this surface does is durable. The search text,
  the filter, the selection, and the scroll position are transient, are never
  written to a Memory, and do not survive the pane closing. `Place` hands off to
  the Grid and commits nothing until a destination is chosen there;
  cancelling that placement returns to this surface with the record unchanged.

Placing a Memory once the handoff has been made, and opening a Note, a Document,
or a picture once `Open` has been chosen, belong to
`docs/ux/wireframes/memory-slate/Memory Slate.md` and to the surfaces that
receive them. They are named here and not restated.

## Motion

| Transition | Duration | Curve | Reduced motion |
| --- | --- | --- | --- |
| The gallery exchanged for a record, and back | `--d-swap` | `--ease` | The new view in place on the same frame, at the identical threshold. |
| A picture arriving in a card that was holding its box | `--d-swap` | `--ease` | The picture in place on the frame it decodes. |
| A picture arriving in the record | `--d-swap` | `--ease` | The picture in place on the frame it decodes. |
| The gallery narrowing or widening as the search text or the filter changes | None | None | No change. |
| Selection outline, focus ring, filter rule | None | None | No change. |
| The pane opening and closing | Owned by `10-grammar/Surface-classes.md` | Owned there | Owned there. |

Narrowing is not animated. `00-foundations/Motion.md` asks what job a transition
does, and a set of cards changing membership answers none of the six: the frame
changes and that is the whole of it.

Nothing loops. Nothing idles. Nothing pulses or blinks. No card enters on a
stagger, no column reflows on a curve, and no result arrives with a highlight
that fades.

## Distance

Thresholds are on projected cell size and belong to
`10-grammar/Representation-tiers.md`.

| Tier | Sheds | Keeps |
| --- | --- | --- |
| Working | Nothing. | Everything above. |
| Stepped | Nothing. | Everything above. |
| Stand-in | Nothing. | Everything above. |

This surface holds no representation tier. It is fixed to the viewport, it is
addressed in no cells, and the camera never projects it, so a person who zooms
the Grid to one per cent sees this surface unchanged at every word. The
tier axis reaches the placements behind it and stops at its border.

The one distance rule that does bind: the pictures a card shows are the same
sources the Grid places, and this surface never substitutes a stand-in,
a thumbnail crop, or a reduced encoding for one. A picture too large to draw
yet is Pending, and Pending holds the box.

## Accessibility

- **Role and name** — the gallery is a `listbox` with single selection, named by
  the pane's identity line through `aria-labelledby` so the name is written
  once. Each card is an `option` carrying `aria-selected`. A card's accessible
  name is the Memory's own words: a Note's text verbatim, a Document's title,
  a picture's source name. `Note`, `Image`, and `Document` as an accessible name
  are defects, because they replace the one thing that tells this Memory from
  every other. The search field's accessible name is `Find a Memory`, the same
  words its placeholder carries, so the name survives the first keystroke. The
  filter row is a `radiogroup` named `Show`, and each filter is a `radio`.
- **Contrast** — text: a Note card's text `--text-primary` on `--surface-nested`
  9.85:1; a Document card's title `--c-paper-ink` on `--surface-page` 15.97:1,
  its front matter and abstract 4.75:1; the metadata line `--text-meta` on
  `--surface-chrome` 4.71:1, rising to 6.35:1 when selected; the resting filter
  4.71:1 and the filter that is on 6.35:1; typed search text 10.36:1 and its
  placeholder 4.71:1; the identity line `--k-slate` on `--surface-chrome`
  9.84:1; a quiet control's label 6.35:1; the primary action's label
  `--c-paper-ink` on `--signal-interaction` 8.57:1; an Anchor's value
  `--signal-authored-context` 6.36:1. Non-text: the selection outline 8.90:1 and
  the focus ring 15.02:1 on `--surface-chrome`; every control edge
  `--edge-control` 4.71:1; the anchor diamond and ribbon 6.36:1; the paper card
  against the surface 16.56:1. The Note card's `--edge-hairline` border at
  1.28:1 and the `--surface-nested` fill at 1.06:1 against the surface are
  exempt, because the card's own text and the gutter carry its extent and
  neither line is meaning-bearing.
- **Without colour** — Approached: nothing, because nothing is added. Focused: a
  ring outside the card at a distinct offset. Selected: an outline at a `3px`
  offset plus the metadata line's rise. Engaged: the outline appearing on the
  press frame. Pending: the held box at the picture's own proportions.
  Refused: not drawn here. Unavailable: a retained box, a `--edge-found` border,
  one sentence, and a control. Anchored: the ribbon's notched silhouette or the
  diamond's rotated square. The filter that is on carries a rule beneath it, so
  the row reads with no colour at all.
- **Forced colours** — redeclared in system colours: every surface fill, the
  paper card and its ink, the card border, every control edge and fill, the
  selection outline, the focus ring, the anchor marks, and the identity line.
  What survives without redeclaration is the structural half of every state: the
  outline's offset, the focus ring's larger offset, the rule under the filter
  that is on, the anchor silhouettes, and the held box of a Pending or
  Unavailable card. Nothing on this surface is drawn as canvas paint, so nothing
  is lost.
- **Text scaling** — everything here is interface text and grows with it. The
  column minimum is expressed in `ch`, so columns widen with the type and drop
  in number rather than crowding the measure; the filter row wraps to a second
  line and the header controls wrap beneath the identity line; the pane scrolls.
  Nothing truncates and no scrollbar appears inside the search row or the filter
  row at any scale.
- **Reduced motion** — no change from the Motion table.

## Copy

Every user-visible string this surface can show.

| String | Where | Why it passes |
| --- | --- | --- |
| `Memories` | The identity line. | The product's own noun for what the surface holds, and true after any rebuild. |
| `Find a Memory` | The search field's placeholder and its accessible name. | A verb and a product noun; it names what the field does. |
| `Show` | The filter row's accessible name. | A verb, ordinary English, invisible on screen and audible in order. |
| `All` | The resting filter. | Ordinary English, no punctuation. |
| `Notes` | A filter. | A product noun. |
| `Documents` | A filter. | A product noun. |
| `Images` | A filter. | A product noun. |
| `Note` | A Note card's metadata line, rendered uppercase by `--tr-label`. | A product noun naming what a person is looking at. |
| `Document` | A Document card's metadata line, rendered uppercase by `--tr-label`. | A product noun naming what a person is looking at. |
| `JPG`, `PNG`, `GIF` | A picture card's metadata line, rendered uppercase by `--tr-label`. | The picture's own format, which `00-foundations/Typography.md` gives to monospace and `00-foundations/Marks.md` puts in the metadata line when there is one. |
| `Nothing kept yet.` | The first line of the empty gallery. | Names what is absent, in one sentence, with no apology and no possessive. |
| `Write a Note or bring in a picture.` | The second line of the empty gallery. | The action that changes what the first line said. |
| `Nothing here matches Documents.` | The no-results line when a filter is on and the search field is empty; the filter's own label is substituted. | States the condition rather than the failure, and never blames. |
| `Nothing here matches those words.` | The no-results line when the search field holds text. | The same, for the narrowing a person typed. |
| `Show all` | The control beside either no-results line. It empties the search field and returns the filter to `All`. | A verb for the one way back. |
| `This picture can't be shown right now.` | Inside a frame whose source cannot be drawn. | States the condition; nothing was refused, so nothing is named as a failure. |
| `Retry` | The control beneath that sentence. | A verb for the action. |
| `Back` | The record's return control. | A place a person is going, in one ordinary word. |
| `Added` | A provenance label. | Names the field in ordinary English. |
| `Form` | A provenance label. | Names the field in ordinary English. |
| `Anchor` | A provenance label. | A product noun. |
| `Open` | The record's primary action. | One verb, true for every form, and the record already shows which form it is. |
| `Place` | The record's quiet action. | The verb for what happens next. |

Everything else on this surface is authored: a Note's text, a Document's title,
front matter and abstract, a picture's source name and dimensions, a person's
own description, and the words of an Anchor. Grove never edits, shortens,
summarises, prefixes, or adds a word to any of them.

Four strings the corpus carries are corrected here. `View content` becomes
`Open`, because `10-grammar/Copy.md` records `Content` as an architecture noun
for the thing a person calls a Note, a Document, or a picture. `Place later`
becomes `Place`, because the action begins placement immediately and a label
that says later names a deferral it does not perform. `Every note, document, and
image you have kept — in one place.` is deleted with the surface it sat on, as a
sentence that narrates the concept. `Open Memory Slate (S)` is deleted with the
control it titled; `Slate` is a class noun and the surface is called `Memories`.

## Refusals

- **A resting invitation on the Grid** — a card that sits over the field
  advertising this surface is chrome that never leaves, which Principle 3
  refuses; the key is learned in the Controls HUD, where Grove lists its keys.
- **A title row on a card** — a heading above a Note's text restates the words
  it sits on, and a heading above a picture is a caption in a taller dress.
- **An ellipsis or a line clamp on a card** — a cut sentence promises text the
  card will never show, and the column absorbs the height instead.
- **A uniform tile grid** — cropping every frame to one rectangle throws away
  the sides of pictures a person kept.
- **Anything drawn over a frame** — metadata, a badge, a gradient, a scrim, or a
  play mark on a picture covers the picture with a report about it.
- **Selection as a tint, a fill, or a border swap** — restyling a card's own
  surface changes the content to say something about the person's attention.
- **A hover accent** — an interaction hue under the pointer means "this is what
  you are working on" before the person has chosen anything.
- **A result count** — a figure that repeats what the narrowed wall already
  shows, in the interface's own framing.
- **A relevance re-sort while a person types** — reordering moves the card they
  were reaching for.
- **A match highlight inside authored text** — `10-grammar/Signal-roles.md`
  names a search match as a use no signal answers, and a highlight restyles
  words a person wrote.
- **A second name for the surface** — one identity line, and no pane title, tab
  label, or heading that repeats it.
- **A search that matches Grove's own words** — matching field names, kinds, or
  identifiers returns results a person cannot explain.
- **A silent reset** — a narrowing that clears itself when it finds nothing
  teaches a person that the controls do not hold.
- **Opening anything as a side effect of selecting** — selection names what a
  person is working on and does nothing else.


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

- Catalogue deck: `docs/design_catalogue/src/hud/02-memory-slate.html` — the contract for this component.
- Catalogue deck: `docs/design_catalogue/src/hud/01-slate-anatomy.html` — the class the pane is composed as, and the metadata line naming a picture's format.
- Catalogue deck: `docs/design_catalogue/src/hud/03-gallery-slate.html` — narrowing belongs to the pane, not to the host.
- Catalogue deck: `docs/design_catalogue/src/00-design-language.html` — the surface border, and no meaning that lives only in motion.
- Wireframe: `docs/ux/wireframes/memory-slate/Memory Slate.md` — MS-00 to MS-08.
- Reference: `docs/reference/Keybind map.md` — `S` and `/`.
