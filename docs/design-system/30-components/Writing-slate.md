---
type: design-system-component
status: active
date: 2026-08-10
component: Writing Slate
plane: hud
surface_class: slate
tags: [grove, design-system, component]
---

# Writing Slate

Where a person reads a Note or a Document whole and writes it through to the
end, on a page with a fixed measure and one way to commit what they wrote.

## Anatomy

Every visible part, named, with the token that gives it its value. A part with
no token is a part with no contract.

| Part | Required | Value |
| --- | --- | --- |
| Frame | yes | `--surface-chrome`, opaque; `--r-sm`; 1px `--k-slate-b`; `--sp-lg` padding on all four sides; no shadow and no rounded corner, because a Slate fills the viewport edge to edge. |
| Identity line | yes | `Writing` in `--f-display` 500, `--t-title-small`, `--tr-label`, uppercase, ink `--k-slate`; `--sp-lg` beneath it. |
| Changed-state line | no | The words `Unsaved changes` in `--f-ui` 400, `--t-caption`, `--text-secondary`, preceded by a `6px` `--r-round` dot in `--k-edit` with `--sp-sm` between them. Present while the draft differs from the saved revision. |
| Quiet action | no | `Discard changes`, text only — no fill, no border — `--f-ui` 400, `--t-caption`, `--text-primary`, padding `--sp-xs` `--sp-md`. Present while the draft differs from the saved revision. |
| Primary action | no | `Save` on a `--signal-interaction` fill, label `--c-paper-ink`, `--f-ui` 500, `--t-caption`, `--r-sm`, padding `--sp-xs` `--sp-md`. Present while the draft differs from the saved revision. |
| Close | yes | The word `Close` in `--f-ui` 400, `--t-caption`, `--text-secondary`, with `Esc` beside it in `--f-mono` 500, `--t-micro`, `--tr-wide`, `--text-meta`. |
| Header separator | no | 1px `--edge-hairline`, `--sp-md` tall, between the changed-state group and the host's own commands. |
| Page | yes | `--surface-page`; `--r-none`; a 1px inset containment edge in `--edge-on-page`; `--sp-xl` margin on all four sides; interior width `65ch`; no page texture. |
| Title | no | `--f-ui` 500, `--t-title`, `--lh-tight`, `--tr-title`, the source's own case, ink `--c-paper-ink` at full strength. Drawn on the first page only. |
| Title rule | no | 1px in `--paper-rule`, the page's full interior width, `--sp-md` clear above and `--sp-lg` clear below. Drawn only when a title is drawn. |
| Body | yes | `--f-ui` 400, `--t-body`, `--lh-reading`, ink `--text-page`, measure `65ch`, `--sp-md` between paragraphs. |
| Source-carried heading | no | `--f-ui` 500, `--t-title-small`, `--tr-title`, the source's own case, ink `--c-paper-ink`; `--sp-lg` above and `--sp-xs` below. |
| Anchor ribbon | no | `00-foundations/Marks.md` geometry, filled `--signal-authored-context`, its left edge `--sp-md` from the page's left edge, standing wholly above the page's top edge. Present on Anchored only. |
| Folio | no | The page's own number in `--f-mono` 500, `--t-label`, `--tr-label`, ink `--paper-label`, at the foot of the page against its left interior edge. Present only when the piece runs to more than one page. |
| Caret | no | A `2px` solid stroke in `--c-paper-ink`, one line box tall, at the insertion point. Present while the body holds keyboard focus. |
| Selection band | no | `--signal-interaction` behind the selected run, with the run's glyphs at `--c-paper-ink` full strength. |
| Hint line | no | One row beneath the page in `--f-mono` 500, `--t-label`, `--tr-mono`, ink `--text-meta`, items `--sp-md` apart. Present while the body holds keyboard focus. |
| Metadata line | yes | Beneath the page and aligned to its outer edges: the kind word left, the revision line right, both `--f-mono` 500, `--t-label`, `--tr-label`, uppercase, ink `--text-meta`; `--sp-md` above it. |
| Inline confirm | no | `10-grammar/Surface-classes.md` anatomy: `--surface-nested`, `--r-sm`, 1px `--edge-hairline`, `--sp-sm` top and bottom, `--sp-md` at the sides, no shadow. Drawn directly beneath the header. |
| Scroll indicator | no | A `--sp-xs` track in `--ink-hairline` with a thumb in `--ink-quiet`, both `--r-sm`, `--sp-sm` inside the frame's right padding edge. Present while the pointer is within the surface and the piece runs to more than one page. |

**The page is paper.** `00-foundations/Color.md` decides it in one sentence — if
a person reads continuous prose or writes it, the surface is paper — and names
the dark reading pane as the defect that rule exists to prevent. Deck 04 renders
its whole reading column on `--surface-chrome` in `--c-text`, and it is
corrected: the frame around the page is chrome, the page inside it is
`--surface-page` carrying `--text-page`, and the reading and writing voice is
the same page voice a placed Document already uses. Counted across the
catalogue, twelve of twenty-one decks draw a reading page on `--surface-page`
and the three reading editions on the Information Plane draw every page that
way; deck 04 and deck 01-local-editors are the only surfaces that set continuous
prose on a dark fill, and a local editor's draft sits beside a Note whose own
text is on an authored fill, which is a different case.

Three values are design and are recorded rather than replaced. Frame padding is
`--sp-lg` in four HUD decks out of four, which is the Slate class figure and
changes nothing. The Slate hue on a display-type uppercase identity line is four
of four. `Unsaved changes` as words paired with a dot in `--k-edit` is what deck
04 renders and what `10-grammar/Signal-roles.md` fixes in its own table —
unsaved work is the Edit role hue and the words, never the refusal hue — so the
build's `.writing-slate[data-state-id="WS-02"] .writing-slate__status` in
`css/slate.css:187`, which sets that line in `--c-invalid`, is the drift.

Four figures resolve onto the scales. The identity line is `12px` at `0.14em`
in deck 04, `15px` at `0.14em` in deck 03, `16px` at `0.14em` in deck 01, and
`20px` at `0.12em` in deck 02; `00-foundations/Typography.md` already ruled the
off-scale sizes upward to `--t-title-small`, and `--tr-label` takes the
`0.13`–`0.15` band by the token table's own replacement rule, so deck 02's
setting is the one the other three resolve to. The body is `16px` on a `1.6`
line, which is `--t-body` on `--lh-reading`. The title is `26px` in UI 600 at
`-0.01em`, which Typography resolves to `--t-title` in UI 500 at `--tr-title`,
because Grove sets no 600 and has no negative tracking. The frame border is
`--k-slate` at `0.30` in deck 04, which composites to `#4D4752` and reaches only
2.15:1 against the Grid; the Slate role's declared border value reaches
3.08:1, so the border is that value and the deck is corrected.

`--k-slate-b` is the name `10-grammar/Surface-classes.md` uses for that
border and `00-foundations/Tokens.md` carries no row for it. The value is
`--k-slate-b`; the token table should add `--k-slate-b` as the semantic name
resolving to it, because a semantic name the grammar already spends is a
contract with no value behind it.

Three parts deck 04 draws are removed. The `‹` at the left of the header is a
second way out beside `Close`, and the deck's own annotation fixes one way out;
`00-foundations/Marks.md` also refuses a chevron outright. The `×` on the close
control is a cross drawn as a mark rather than written as a word, which the same
document refuses, so the control is the word. The `Document` chip in the header
is the kind badge, and Marks fixes both halves of that question: a badge is never
drawn on a form that says what it is in its own text, and where a metadata line
already exists the kind is a word in that line. The metadata line beneath the
page keeps the kind word and the header carries none.

The page has no texture. A placed Document rules its paper at `44px` and `220px`
so the sheet still reads as paper when the camera is far away; here the page is
read at full size with a caret in it, where the fill and the ink already say
paper and a ruled ground would sit under every line a person is writing.

## Geometry

- **Footprint** — the Slate host composes this surface as Full, Left, or Right,
  and the surface takes that pane whole. Executed as a procedure: take the
  pane's content box; subtract `--sp-lg` of frame padding on each side; place
  the header row at the top and the metadata line at the foot; the page column
  is `65ch` plus `2 × --sp-xl`, centred horizontally in what remains. The host
  composes a pair only where each pane's content box holds that column plus
  `2 × --sp-lg`; below that it composes Full, because the measure is fixed and a
  pane narrower than its page would have to rewrap or clip, and both are refused.
- **Growth** — the surface never grows and the page never grows. Text that
  outruns the page flows onto the next page and the page stack scrolls inside
  the frame; `00-foundations/Principles.md` Law 9 fixes exactly this pair — a
  composed surface may scroll because it is chrome, and a page may paginate
  because it is a page. Nothing is shortened, faded, summarised, or set smaller
  to avoid either.
- **Measure** — `65ch`, on the title, the body, and any source-carried heading.
  `00-foundations/Typography.md` assigns that figure to the single column on a
  surface given over to writing and states that it belongs to this
  specification, because no other component shares it. Deck 04 draws a `520px`
  column at its `16px` setting and annotates it as about sixty-five characters;
  the annotation is the contract and `ch` is the unit, because a column measured
  in pixels changes its reading when a window is dragged.
- **Page height** — the page holds a whole number of body lines. Take the
  interior height left after the header, the metadata line, and the `--sp-md`
  above it; subtract the page's `2 × --sp-xl` margin and, where the piece runs
  past one page, the folio line and the `--sp-md` above it; divide by
  `--t-body × --lh-reading` and take the whole number below. Where that number
  is under eight the host composes Full instead, because a page of fewer than
  eight lines is a card and this surface exists so that writing is not done on a
  card. Consecutive pages are separated by `--sp-md` of the frame's own fill, so
  the seam reads as a page break rather than a gap in the text.
- **Alignment** — the page column is centred in the frame's content box; the
  metadata line aligns to the page's left and right outer edges; the title, the
  title rule, the body, and the folio align to the page's interior edges. A
  change in the surface's height changes how many pages the piece takes and
  never where a line breaks, because the measure is fixed in characters.

## States

All nine.

| State | Appearance | Notes |
| --- | --- | --- |
| Rest | The frame, the identity line, `Close`, the page, and the metadata line. No commitments, no hint line, no scroll indicator, no confirm. | This is the surface a person meets on opening and returns to after a commit; the deck's reading anatomy is this state. |
| Approached | The scroll indicator fades in over `--d-fade` when the piece runs to more than one page. | Nothing else answers the pointer, and the page is never restyled. |
| Focused | Every focusable part takes `--focus-ring` at `--focus-ring-offset` on `:focus-visible`. With the body focused the caret is drawn at the insertion point and the hint line is present. | The frame itself never draws a ring; focus belongs to the part that holds it. |
| Selected | The surface is never Selected. Selection inside it is a run of the person's own text: `--signal-interaction` behind the run with the run's glyphs at `--c-paper-ink`. | A Slate is composed, not chosen, so the interaction signal here names text rather than the surface. |
| Engaged | A pointer drag through the text extends the run live and the caret sits at the drag's moving end. Pointer-down on a control takes the pressed appearance over `--d-press`. | The gesture has a subject, so it draws in `--signal-interaction`; `--signal-active-work` belongs to a sweep and never appears here. |
| Pending | The piece asked for and not yet arrived holds the frame, the identity line, and the metadata line, and shows the fullest form it already has. While a commit is in flight the draft stays exactly as typed and `Save` goes Unavailable. | Never blank, never a spinner, never a progress bar. The arrived text replaces the pending form in place over `--d-swap`. |
| Refused | A commit that did not land raises the inline confirm's refusal form directly beneath the header: the same row with its border in `--signal-refusal`, one sentence, `Keep editing` quiet and `Try again` committing. The changed-state line keeps saying `Unsaved changes`, because it is still true. | Refusal's structure carrier here is the bordered row, not hatching: `00-foundations/Marks.md` puts the 45° hatch on Grid cells and never inside a Slate. |
| Unavailable | `Save` is `--text-unavailable` on a `--edge-hairline` border, in place, while the draft matches the saved revision, while the body is empty, and while a commit is in flight. Where the piece is gone the page is replaced by one sentence and `Close`. | An action that vanished would teach a person the surface lost a capability it still has; `Save` on an empty body is Unavailable because emptying a piece is not a revision of it. |
| Anchored | The anchor ribbon stands at the page's top edge, `--sp-md` from its left edge. | Durable and authored; it is the same mark the placed front page carries, so the property does not disappear where a person looks closest. |

Combination follows `10-grammar/States.md` without exception. One confirm is
open at a time: a save that fails while the discard question is open answers
into the same row rather than raising a second.

## Behaviour

- **Pointer** — a click in the body places the caret at the nearest character
  boundary, committing on pointer-down so the hand is never held; a click in the
  page margin places it at the nearest line end. A drag extends the selection
  and commits on release. Double-click selects a word and triple-click selects a
  paragraph — the desktop convention, and both have the keyboard equal below, so
  neither is a gesture without a key. The wheel scrolls the page stack; the
  camera never moves while the pointer is over the surface. Clicking the frame
  outside the page moves nothing.
- **Keyboard** — typing inserts at the caret. `Ctrl/Cmd+Enter` commits, which is
  the row `docs/reference/Keybind map.md` already carries for a text surface.
  Arrow keys move the caret by character and by line; `Ctrl/Cmd+←` and
  `Ctrl/Cmd+→` by word; `Home` and `End` to the ends of the line;
  `Ctrl/Cmd+Home` and `Ctrl/Cmd+End` to the ends of the whole piece; `PageUp`
  and `PageDown` by one page of the stack. `Shift` with any of those extends the
  selection, and `Ctrl/Cmd+A` selects the whole text. `Ctrl/Cmd+Z` and
  `Ctrl/Cmd+Shift+Z` undo and redo inside the open draft. `Tab` moves focus out
  of the body and never inserts a character, because
  `00-foundations/Accessibility.md` forbids a text surface from swallowing the
  keys that leave it. `Escape` closes a clean surface and raises the confirm on a
  dirty one — a decision, because the same rule that protects a dirty draft in a
  local editor protects one here, and the keybind map gains that row in the same
  change. Grid keys do not run while focus is inside this surface.
- **Focus order** — the quiet action, `Save`, `Close`, then the body, wrapping.
  Focus arrives on the body when the surface opens, because the surface exists
  to be written in, and the header controls sit ahead of it in the order so a
  person tabbing forward from the body reaches the commitments first.
- **Escape** — Escape peels one layer per press: the inline confirm first,
  choosing its quiet action; then, on a dirty draft, the confirm again; then the
  surface. Escape reaches the Grid only when nothing is open above it.
- **Commit and cancel** — `Save` and `Ctrl/Cmd+Enter` are the only durable
  writes. A commit writes one new revision on the same Memory: no second Memory
  appears, nothing is renamed, no Placement is created or moved, and the camera
  does not move. `Discard changes` and a dirty `Escape` raise
  `Discard unsaved changes?`; choosing `Discard` restores the saved revision in
  place and clears the changed-state line, and choosing `Keep editing` returns
  the caret exactly where it was. Nothing is written on close, on blur, or on a
  timer.

**The return path.** `Close` and a clean `Escape` dismiss this pane only and
return focus to the surface that invoked it — the placed Document under the Grid
cursor, the Memory row that opened it, or the reading it was opened from — with
that surface's own selection and scroll position unchanged. Where the invoking
surface is gone, focus returns to the Grid cursor at its last cell. When a peer
pane is open, closing this one leaves the peer untouched.

**The placed Document front page.** The front page and this surface are the same
piece at two distances. The front page composes zones from it — front matter,
the display title, the opening passage at `--measure-reading`, the closing line,
the title block — and this surface carries the piece whole, so no zone is
repeated here and no title block is drawn. The title is set differently in each
and both are correct: `00-foundations/Typography.md` keeps the front page's
uppercase `--t-display` because a placed sheet is read as an identity at a
glance, and keeps the source's own case in UI type here because a page being
written is read as words and the editing voice must equal the reading voice.
Committing re-renders the front page in place from the new revision; its
position never changes and its height re-solves by the growth rule in
`30-components/Document.md`. Front matter and the title block are fields of the
record and are edited where the record is shown, because a writing page is one
continuous text and labelled fields with an accept button are the form this
surface refuses.

The journey — which surfaces may open this one, and what happens after the
return — belongs to `docs/ux/wireframes/writing-slate/Writing Slate.md`.

## Motion

| Transition | Duration | Curve | Reduced motion |
| --- | --- | --- | --- |
| The surface opening and dismissing | `--d-fade` | `--ease` | Present or absent at the identical threshold. |
| The commitments arriving as the draft goes dirty, and leaving on commit | `--d-fade` | `--ease` | Present or absent at the identical threshold. |
| The hint line arriving with focus in the body and leaving with it | `--d-fade` | `--ease` | Present or absent at the identical threshold. |
| The scroll indicator arriving on approach and leaving with the pointer | `--d-fade` | `--ease` | Present or absent at the identical threshold. |
| The inline confirm arriving and dismissing | `--d-fade` | `--ease` | Present or absent at the identical threshold. |
| A pending piece replaced by the arrived text | `--d-swap` | `--ease` | The text in place on the same frame, at the same threshold. |
| Pointer-down acknowledgement on a control | `--d-press` | `--ease` | The pressed appearance applied on press and removed on release, with no scaling. |

`00-foundations/Motion.md` names the curves `--ease` and `--overshoot`;
`00-foundations/Tokens.md` projects the same two values as `--ease` and
`--overshoot`. `--overshoot` never appears on this surface, because it is
reserved for arrival in cells and nothing here arrives in cells.

**The caret does not blink.** It is a solid stroke that moves when the insertion
point moves and is otherwise still, because Law 5 admits no repeating animation
and a `2px` stroke at full paper ink is found without one. Scrolling the caret
back into view lands in one step with no travel. The page, the text, the title,
the selection band, the focus ring, and the anchor ribbon never animate. Nothing
loops. Nothing idles. Nothing pulses or blinks.

## Distance

Thresholds are on projected cell size and belong to
`10-grammar/Representation-tiers.md`.

| Tier | Sheds | Keeps |
| --- | --- | --- |
| Working | Nothing. | Everything in the Anatomy table. |
| Stepped | Nothing. | Everything in the Anatomy table. |
| Stand-in | Nothing. | Everything in the Anatomy table. |

This surface is fixed to the viewport and the camera does not reach it, so no
tier ever applies: `20-planes/HUD-plane.md` states that everything on this plane is
unaffected by the camera and addressed in nothing but the viewport. The camera
keeps moving behind an open surface, and the placements it moves change tier
under their own rules while this page does not change at all. Presence,
position, and extent are the field's facts, not this surface's, and it sheds
none of them because it holds none of them.

## Accessibility

- **Role and name** — the surface is a `region` whose accessible name is the
  piece's own title, verbatim; where the piece has no title the name is its file
  name. The page body is a multiline `textbox` taking the same name, because the
  piece is the thing being written into and a classifying label such as
  `Document body` replaces the one thing that distinguishes this piece from
  every other. `role="dialog"` is a defect here: Grove has no modal, the
  Grid behind stays live, and a dialog role tells assistive technology
  otherwise. The changed-state line and the refusal row are polite live regions;
  a commit, a refusal, and the arrival of a piece are announced, and nothing
  else is.
- **Contrast** — body `--text-page` on `--surface-page` 4.75:1; title and
  source-carried heading `--c-paper-ink` on `--surface-page` 15.97:1; folio
  `--paper-label` on `--surface-page` 4.75:1; identity line `--k-slate` on
  `--surface-chrome` 9.86:1; changed-state words `--text-secondary` 6.35:1 and
  the dot `--k-edit` 8.93:1, both on `--surface-chrome`; quiet action
  `--text-primary` 10.36:1; `Close`, the metadata line, and the key hint at
  `--text-meta` 4.71:1; `Save`'s label `--c-paper-ink` on `--signal-interaction`
  8.57:1; `Discard`'s label `--c-paper-ink` on `--signal-refusal` 5.09:1;
  selected text `--c-paper-ink` on the `--signal-interaction` band 8.57:1.
  Non-text: the page against the frame 16.58:1; the frame's border
  `--k-slate-b` against the Grid 3.08:1; the refusal row's border
  `--signal-refusal` against `--surface-nested` 4.96:1; the anchor ribbon
  `--signal-authored-context` against `--surface-chrome` 6.37:1. The page's
  inset edge at 1.22:1 and the title rule at 1.39:1 carry no meaning and are
  exempt, because the paper fill carries the page's extent. The selection band
  is a fill behind text rather than a boundary a person must locate, so the
  ratio that governs it is the ratio of the glyphs on it.
- **Without colour** — Approached: an indicator that was absent. Focused: a ring
  outside the part's edge, and the caret. Selected: the run's own extent, and
  the caret at its end. Engaged: the run growing under the hand. Pending: the
  fullest form already held, in the same place. Refused: a bordered row that was
  not there, one plain sentence, and `Save` still offered. Unavailable: a
  retained position and a hairline border. Anchored: the ribbon's notched
  silhouette. Unsaved work is carried by the words first and the Edit hue
  second, so a greyscale read loses nothing.
- **Forced colours** — the frame fill and border, the page fill and ink, the
  page's inset edge, the title rule, the focus ring, the selection band and its
  glyphs, both action fills, the refusal border, and the ribbon fill are
  redeclared in system colours. What survives without redeclaration is the
  ribbon's silhouette, the ring's offset, the confirm row's position beneath the
  header, the caret's stroke, and every word this surface writes. The `--k-edit`
  dot does not survive as a hue, and the words `Unsaved changes` beside it are
  why that costs nothing.
- **Text scaling** — this is interface text and it scales. The page grows with
  it, because `65ch` is tied to the type size, so raising interface text to 200%
  produces a wider page holding the same sixty-five characters per line and more
  pages in the stack; no line rewraps and nothing truncates. The header wraps to
  a second row before any control is clipped, and the metadata line's two halves
  stack rather than shortening.
- **Reduced motion** — no change from the Motion table.

## Copy

| String | Where | Why it passes |
| --- | --- | --- |
| `Writing` | The identity line, rendered uppercase by `--tr-label` rather than by writing capitals. | Names what the surface is for in ordinary English, and stays true after any rebuild. |
| `Unsaved changes` | The changed-state line. | Two ordinary words stating a condition; `10-grammar/Signal-roles.md` fixes this wording as the carrier for changed work. |
| `Save` | The primary action. | A verb for the action it performs, sentence case, no terminal punctuation. |
| `Discard changes` | The quiet action in the header. | Names the outcome rather than the mechanism, which is why it replaces `Cancel`. |
| `Close` | The one way out. | A verb for an action; the word replaces the `×` that `00-foundations/Marks.md` refuses. |
| `Esc` | The key hint beside `Close`. | A key name in monospace, not prose. |
| `Discard unsaved changes?` | The inline confirm's question. | Taken verbatim from the table in `10-grammar/Surface-classes.md`; it names what is lost and ends in a question mark. |
| `Keep editing` | The confirm's quiet action, and the failure row's. | Names what continues. |
| `Discard` | The confirm's committing action. | Repeats the verb of the loss. |
| `This writing is not saved.` | The failure row's sentence. | States the condition in the present tense, one clause, with no apology, no blame, and no possessive. |
| `Try again` | The failure row's committing action. | Names the act a person is offered. |
| `This writing is no longer here.` | In place of the page when the piece is gone. | Names what is absent, and `Close` is the one action beside it. |
| `# heading` `## subheading` `**bold**` `_italic_` `- list` `` `code` `` | The hint line. | Six syntax examples in ordinary words, each true of the text a person types, none a control. |
| `Note` `Document` | The kind word at the left of the metadata line. | Grove's own product nouns, in the metadata line where `00-foundations/Marks.md` puts the kind. |
| `Saved just now` | The revision line, immediately after a commit. | States when, in plain words. |
| `Edited` | The revision line, followed by the revision's date and time. | States when, in plain words; the date and time are figures, set with the monospace family's tabular figures so the line does not shift. |

The folio is a figure rather than a string: the page's own number, and nothing
else. A line naming the next page is refused here, because the pages of one
piece run in order and such a line would state what the order already says;
`README.md` resolves a silent convention to print and page mechanics, and a
bare folio is what a book uses.

Everything else is **None**. The piece's title, its headings, and its text are
authored content: Grove never edits, shortens, prefixes, summarises, or adds a
word of its own to any of them.

Four strings in the build are defects under `10-grammar/Copy.md`.
`index.html:235` names the surface `Slate surface`, which is an architecture
noun in an accessible name. `src/features/writing/writing-slate.ts:21` says
`This item is unavailable for writing.`, which classifies the piece and reports
the mechanism. Line 34 of the same file labels the title field
`Document title` and line 38 labels the body `Document body` or `Note text`,
which replace the piece's own name. Line 59 says
`Write something before saving.`, which instructs the person instead of stating
the condition; the correction is that `Save` is Unavailable on an empty body.

## Refusals

- **A dark reading pane** — a surface a person writes continuous prose on is
  paper, and a dark column for reading is the exact defect that rule names.
- **A formatting toolbar** — a row of B, I, U, and heading buttons is foreign
  chrome standing above the writing, and one quiet line of syntax hints does the
  same job without taking the eye off the page.
- **Labelled fields with an accept button** — a piece of writing is one
  continuous text on one measure, and a form of title, body, and tags with `OK`
  beneath it is a settings dialog wearing an editor's name.
- **A card-height viewport** — clipping the text and fading out the remainder
  hides what a person wrote inside a box that could have paginated instead.
- **A new editor accent** — `Save` is the interaction signal, identity is the
  Slate hue, changed work is the Edit hue, and a hue invented for this surface
  fails review wherever it appears.
- **Autosave, or any write on close, blur, or a timer** — a person who has not
  saved has not decided, and a background write takes that decision away.
- **A second Memory on commit** — a revision is a revision of the same piece;
  a copy left behind makes the Grid hold two of what a person wrote one of.
- **A word count, a character count, or a reading time** — a figure on the page
  is a dashboard reading competing with the words the page exists to carry.
- **A blinking caret** — Grove has no repeating animation, and a solid stroke at
  full paper ink is found without one.
- **A second title** — the surface names itself once and the piece carries its
  own title on the page; repeating either in the header is a title nobody wrote.
- **A measure that follows the window** — line length follows the type, so a
  wider surface produces the same reading and a narrower one produces more
  pages.
- **A scrim, a dim, or a blur behind the frame** — the Grid stays lit,
  legible, and clickable while a person writes.


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

- Catalogue deck: `docs/design_catalogue/src/hud/04-writing-slate.html` — the contract for this component.
- Catalogue deck: `docs/design_catalogue/src/hud/01-slate-anatomy.html` — the class frame, the header, and the no-scrim refusals.
- Catalogue deck: `docs/design_catalogue/src/hud/02-memory-slate.html` — the identity line at `20px`, which the other three decks resolve to.
- Catalogue deck: `docs/design_catalogue/src/information-plane/05-annotation-mixed-media.html` — pages, folios, and continuation as page mechanics.
- Catalogue deck: `docs/design_catalogue/src/information-plane/04-annotation-markers.html` — pages, never cuts.
- Wireframe: `docs/ux/wireframes/writing-slate/Writing Slate.md` — the journey and its return.
- Reference: `docs/reference/Keybind map.md` — `Ctrl/Cmd+Enter` commits; `Escape` dismisses.
