---
type: design-system-component
status: active
date: 2026-08-10
component: Local editor
plane: information
surface_class: local-editor
tags: [grove, design-system, component]
---

# Local editor

A small frame that opens beside a Note or a Document so its words can be
changed without leaving the thing itself in view.

## Anatomy

Every visible part, named, with the token that gives it its value. A part with
no token is a part with no contract.

| Part | Required | Value |
| --- | --- | --- |
| Frame | yes | `--surface-chrome`, opaque, `--r-sm` on all four corners, `--sp-md` padding on every side, `--shadow-local` as a single layer. |
| Role border | yes | `1px` solid `--k-edit-b`. It is the frame's only role signal and it never changes with state. |
| Label line | yes | The thing's own name, in `--f-mono` at `--t-label`, `--tr-wide`, uppercase, `--text-meta`. One line, left of the label row. |
| Unsaved line | no | The words `Unsaved changes` in `--f-mono` at `--t-label`, `--tr-wide`, uppercase, `--k-edit`. Right of the label row, present only while the draft differs from what is stored. |
| Draft field | yes | `--f-ui` at weight 400, `--t-body`, `--lh-reading`, `--text-primary`. No fill of its own, no border, no radius: it is the frame's body. |
| Caret | yes | The platform's own insertion caret, coloured `rgb(var(--ink) / var(--ink-full))`. Grove draws no caret of its own. |
| Escalation route | no | `Open in Writing Slate` in `--f-ui` at `--t-caption`, `--text-meta`, text only. Left end of the footer row. |
| Key hint | yes | The key that fires the action beside it, in `--f-mono` at `--t-micro`, `--tr-mono`, uppercase, `--text-meta`. Sits `--sp-sm` before its action, outside the action's own box. |
| Quiet action | yes | `Cancel` in `--f-ui` at weight 400, `--t-caption`, `--text-primary`. Text only — no fill, no border. Its hit area insets `--sp-xs` top and bottom and `--sp-sm` at the sides and draws nothing. |
| Committing action | yes | `Save` in `--f-ui` at weight 400, `--t-caption`, on a `--signal-interaction` fill with a `--c-paper-ink` label, `--r-sm`, `--sp-xs` top and bottom, `--sp-sm` at the sides. The one primary action on the frame. |
| Inline confirm | no | The row owned by `10-grammar/Surface-classes.md`: `--surface-nested`, `--r-sm`, `1px` `--edge-hairline`, `--sp-sm` top and bottom, `--sp-md` at the sides, no shadow. Above the footer. |
| Refusal row | no | The same row with its border in `--signal-refusal`, carrying the sentence, a dismissal, and a retry. |

Rhythm inside the frame is the space scale, per `10-grammar/Surface-classes.md`
rule 9: `--sp-md` between the label row and the draft, `--sp-md` between the
draft and the footer, `--sp-md` between the two actions so the pair does not
read as one control, and `--sp-sm` between a key hint and the action it names.

The draft sets directly on the frame's fill. A boxed field would be a second
tonal step inside a frame that has already taken one, which
`00-foundations/Elevation-and-depth.md` caps, and it would put a border between
a person and their own words.

Two values this component owns, both with reasons. The frame's width is
`--measure-reading` plus `--sp-md` at each side, because Grove already names one
reading column and a draft a person will read back on the Grid should wrap
at the same rhythm as everything else Grove sets. The caret takes `--ink-full`
because it is where keyboard attention is, and `00-foundations/Tokens.md`
reserves the top of the ink ramp for exactly that; the semantic name that should
carry it is `--text-caret`, which the token table does not yet declare, so the
caret is written against the core ramp until it does.

`10-grammar/Surface-classes.md` calls the role border `--c-edit-edge`. No such
name exists in `00-foundations/Tokens.md` or `css/tokens.css`; the value is
`#7A3F3A` and the table carries it as `--k-edit-b`, which is the name used here.
The semantic alias should be added to the token table, and until it is the core
name is the only one a stylesheet can spend.

The frame has no title, no header bar, no toolbar, no tab strip, no close
control, no metadata line, and no second naming line. The label line is the
whole of what Grove says about the thing being edited.

## Geometry

- **Footprint** — the frame is `--measure-reading` wide plus `--sp-md` at each
  side. Its height is the label row, then `--sp-md`, then the draft field, then
  `--sp-md`, then the footer row, inside `--sp-md` of padding top and bottom.
- **Growth** — the draft field opens at eight lines of `--t-body` at
  `--lh-reading` and grows one line at a time as the draft grows, until the
  frame would come within `--sp-md` of the viewport's top or bottom edge. From
  there the frame stops growing and the draft field scrolls. Grove's answer is
  normally that the footprint grows; this component differs because it is chrome
  and not a footprint on the Grid, and `00-foundations/Principles.md` Law 9
  permits a chrome surface to scroll. Nothing is shortened, summarised, or set
  smaller at any length.
- **Measure** — `--measure-reading`. The column is expressed in `ch`, so it
  holds the same count of characters at every interface text size.
- **Alignment** — the frame lands on no grid line, because it is chrome in
  screen space and not a placement. Its outer edge sits `--sp-md` clear of the
  source's projected footprint on the side it took, and its vertical centre sits
  on the source's projected vertical centre.

**The anchor procedure.** Executed once, on the frame the editor opens, against
the source's footprint in screen coordinates. Four candidate positions are tried
in order, each with a `--sp-md` gap from the source: right, left, below, above.
The first whose frame lies wholly inside the viewport less `--sp-md` on every
side wins. Where none does, the fourth is taken and the frame is pushed inside
the viewport margin — a source larger than the viewport is a camera the person
has to move, and Grove moves no camera on their behalf. The procedure reads the
source rectangle and the viewport and nothing else, so it yields the same answer
twice.

**The frame flips; it never slides.** When the preferred side does not fit, the
frame moves to the opposite side of the source whole. Sliding the frame along
its axis to fit is refused, because the distance it slides is exactly the
distance it spends covering the thing it was opened to serve.

**The frame follows its source without scaling.** Its side is selected once
when it opens. The projection bridge reprojects its anchor when Camera geometry
changes; the frame retains the same width, type, padding, and internal state.
It does not re-flip or clamp. If the source leaves the viewport, the frame leaves
with it. Viewport resizing uses the same source projection and does not rescue
the frame independently
— it is clamped, never re-derived.

## States

All nine.

| State | Appearance | Notes |
| --- | --- | --- |
| Rest | The frame, the label line, the whole draft, and the footer. The committing action is Unavailable while the draft matches what is stored. | The frame at rest is an open editor with nothing typed into it yet; it is not an absence. |
| Approached | No change from Rest. | The frame was summoned, so it has nothing to reveal to a pointer, and every action is present from the first frame. |
| Focused | The part holding `:focus-visible` takes `--focus-ring` at `--focus-ring-offset`, drawn outside that part. | The frame itself never takes a ring, and the role border never changes to signal focus. When the editor is opened by pointer, focus lands in the draft without a ring and the caret is the mark. |
| Selected | The frame is never Selected. Text selected inside the draft takes a `--signal-interaction` fill with `--c-paper-ink` ink. | Selection names content a person is working on; this frame is chrome. The source keeps whatever selection it had, unchanged, the whole time. |
| Engaged | A press on an action takes the pressed appearance over `--d-press`. A drag inside the draft extends the text selection as continuous rendering. | No marquee is drawn: `--signal-active-work` belongs to a sweep on the Grid, and a text selection already has its own drawing. |
| Pending | While a save is in flight the committing action goes Unavailable and the draft stays exactly as typed. | Never blank, never a spinner. The draft is the most complete form the frame has and it holds it. |
| Refused | A save that could not be written raises the refusal row above the footer: the same row with its border in `--signal-refusal`, one plain sentence, a dismissal, and a retry. The draft is untouched and the frame stays open. | `30-components/Refusal.md` hands refusal inside a local editor to `10-grammar/Surface-classes.md`, and the frame draws no hatch — nothing on the Grid was refused. |
| Unavailable | The frame is never Unavailable. The committing action is `--text-unavailable` on a `1px` `--edge-hairline` border, in place, while the draft matches what is stored and while a save is in flight. | An action that vanished on a clean draft would teach a person that the editor loses a capability it still has. |
| Anchored | The frame carries no anchor mark. | Anchored is a property of content, and the mark stays on the source, which is in view the whole time. |

Combination follows `10-grammar/States.md` without exception. Focused plus
Refused draws the ring on the retry action, outside the refusal row's border.

## Behaviour

- **Pointer** — a click on the quiet action closes the frame on a clean draft
  and raises the inline confirm on a dirty one, committing on pointer-up over
  it. A click on the committing action saves, committing on pointer-up over it.
  A click on the escalation route hands the current draft to the Writing Slate
  and closes the frame. A click anywhere in the draft places the caret; a drag
  extends the selection. A click outside the frame does nothing to the frame:
  the Grid behind stays live and a person reaching it is not asking to lose
  a draft.
- **Keyboard** — `Ctrl/Cmd+Enter` saves and `Escape` cancels, both owned by
  `docs/reference/Keybind map.md` and neither redefined here. `Tab` and
  `Shift+Tab` move focus within the frame and never out of it while it is open.
  `Enter` inside the draft inserts a line break, because the draft is authored
  text and a key that leaves a text field is a key that eats a paragraph. Focus
  arrives in the draft field with the caret at the end of the text, and leaves to
  the source when the frame closes.
- **Focus order** — draft field, escalation route, quiet action, committing
  action. When the inline confirm or the refusal row is open, its two actions
  come after the draft and before the footer, in the order the row draws them.
- **Escape** — one press peels one layer, per `10-grammar/Surface-classes.md`.
  With a confirm or a refusal row open, Escape chooses that row's quiet action
  and leaves the frame open. With a clean draft, Escape closes the frame
  silently. With a dirty draft, Escape raises the inline confirm rather than
  closing. Escape never reaches the Grid while the frame is open.
- **Commit and cancel** — `Save` is the only durable write. Typing commits
  nothing, closing commits nothing, and the escalation route commits nothing —
  it carries the draft, dirty state included, to the Writing Slate intact.
  Cancelling on a clean draft costs nothing; cancelling on a dirty draft asks
  first, and choosing `Discard` loses the draft and nothing else. A save that is
  refused leaves the draft whole and the frame open.

Dismissal returns focus to the source it served — the placement's edit
affordance, or the card the editor was opened from — with that source's
selection and scroll position exactly as they were. The source is never
restyled, never scrolled, and never moved by the frame opening or closing.

Two local surfaces are never open on the same source, and one local editor is
open at a time across the Grid. Opening an editor elsewhere while this one
holds a dirty draft asks the discard question here first. The journey through
those states is `docs/ux/wireframes/text-editor/Text editor.md`; it is named,
not restated.

## Motion

| Transition | Duration | Curve | Reduced motion |
| --- | --- | --- | --- |
| The frame arriving on open | `--d-fade` | `--ease` | Present at the identical threshold, with no fade. |
| The frame leaving on dismiss | `--d-fade` | `--ease` | Absent on the frame the dismissal commits. |
| The inline confirm or the refusal row arriving and leaving inside the frame | `--d-fade` | `--ease` | Present or absent at the identical threshold. |
| Pointer-down acknowledgement on an action | `--d-press` | `--ease` | The pressed appearance applied on press and removed on release, with no scaling. |

`00-foundations/Motion.md` names the curve `--ease` and
`00-foundations/Tokens.md` projects the same value as `--ease`; Motion's name is
used, because Motion owns which curve a transition takes.

The frame never slides in from an edge, never scales on open, and never moves
after it has opened. The role border, the label line, the unsaved line, the
draft, the caret, the focus ring, and the refusal row never animate. Nothing
loops. Nothing idles. Nothing pulses or blinks.

## Distance

Thresholds are on projected cell size and belong to
`10-grammar/Representation-tiers.md`.

| Tier | Sheds | Keeps |
| --- | --- | --- |
| Working | Nothing. | The whole frame at one size. |
| Stepped | Nothing. | The whole frame at one size. |
| Stand-in | Nothing. | The whole frame at one size. |

The frame holds no tier. It is chrome drawn in screen space, so the camera
changes what its source looks like and never what it looks like: at every zoom
the frame is the same width, the same type, and the same padding. A frame that
shed detail as its source receded would be answering the camera instead of the
person typing into it.

The source keeps its own tier and sheds on its own schedule. Presence, position,
and extent are never shed; the source's stand-in is still the source, and the
frame stays anchored to where it was.

If the source is removed while the frame is open, the frame closes, per
`20-planes/Information-plane.md` — no surface outlives its source.

## Accessibility

- **Role and name** — the frame is a `dialog` without `aria-modal`, because the
  role says a surface took focus and can be dismissed while the absent property
  is what stops it claiming the interruption Grove refuses. Its accessible name
  is the label line, referenced rather than repeated, and the draft field takes
  its name from the same line. `Edit Note` and `Note text` as accessible names
  are defects: they invent strings for a line that already names the thing. The
  actions are buttons carrying their own words; the escalation route is a button
  and never a link.
- **Contrast** — the draft at `--text-primary` reaches 10.36 on
  `--surface-chrome`; the label line and the key hints at `--ink-tertiary` reach
  4.71; the escalation route at `--text-meta` reaches 4.71; the unsaved line in
  `--k-edit` reaches 9.14; the quiet action at `--text-primary` reaches 10.36;
  the committing action's `--c-paper-ink` label reaches 8.57 on its
  `--signal-interaction` fill, and that fill reaches 8.90 against the frame,
  which is what defines the control's boundary. `--signal-refusal` reaches 5.28
  on the frame. The role border at `--k-edit-b` reaches 2.29 on the frame and
  2.39 on `--surface-grid`; it carries no meaning under
  `00-foundations/Accessibility.md`, because the frame's separation is carried
  by tone, `--shadow-local`, and position, and the operation it names is carried
  by the frame's contents. The unavailable committing action at
  `--text-unavailable` reaches 2.46 and takes the exemption
  `00-foundations/Accessibility.md` grants unavailable text, paired with its
  border and its retained position.
- **Without colour** — Approached: no change, so nothing to read. Focused: a
  ring outside the focused part at a distinct offset. Selected: the frame never
  enters it; selected text is a filled run of a different shape from its
  surroundings. Engaged: the pressed appearance on the action under the hand.
  Pending: the committing action's border and retained position. Refused: the
  refusal row's own frame above the footer, plus one plain sentence. Unavailable:
  a retained position and a hairline border. Anchored: not this frame's state.
  Unsaved work is carried by the words, not by the hue: `Unsaved changes` reads
  with the colour removed, which is why no dot is drawn.
- **Forced colours** — the frame fill, the role border, the draft ink, the label
  and unsaved lines, the committing action's fill and label, the confirm's fill
  and border, and the refusal row's border are redeclared in system colours. What
  survives without redeclaration is the frame's position beside its source, the
  footer's order, the confirm and refusal rows as rows of their own, the focus
  ring's offset, and every word on the frame. `--shadow-local` does not survive,
  which is why the role border is mandatory rather than decorative here.
- **Text scaling** — everything on the frame is interface text and grows with
  it. The column is `--measure-reading`, so it holds 34 characters at every size
  and the frame's width grows with the type; the frame re-clamps inside the
  viewport margin, the footer wraps to a second row rather than clipping, and the
  draft field takes the height it needs before it scrolls. Nothing truncates, no
  horizontal scrollbar appears in the draft, and the frame never covers its
  source at any scale.
- **Reduced motion** — no change from the Motion table.

## Copy

Every user-visible string this frame can show.

| String | Where | Why it passes |
| --- | --- | --- |
| `Note` | The label line, for a Note. Rendered uppercase by `--tr-wide` rather than by writing capitals. | A product noun a person handles, and true after any rebuild. |
| `Unsaved changes` | The unsaved line, while the draft differs from what is stored. | States the condition in ordinary English, sentence case, no terminal punctuation. |
| `Cancel` | The quiet action in the footer. | Names the outcome of leaving without saving, in the word every desktop uses for it. |
| `Save` | The committing action in the footer, and the retry in the refusal row. | A verb for the action it performs. |
| `Open in Writing Slate` | The escalation route. | Names the act and the surface it opens; `Writing Slate` is a surface title, which `10-grammar/Copy.md` permits. |
| `Esc` | The key hint before the quiet action. | The key's own name. |
| `Ctrl+Enter` / `Cmd+Enter` | The key hint before the committing action, carrying the platform's own modifier name. | The keys' own names. |
| `Discard unsaved changes?` | The inline confirm's question. | Owned verbatim by `10-grammar/Surface-classes.md`. |
| `Keep editing` | The confirm's quiet action, and the refusal row's dismissal. | Names what continues. |
| `Discard` | The confirm's committing action. | Repeats the verb of the loss. |
| `This change is not saved.` | The refusal row's sentence. | One clause, present tense, stating the condition of the world rather than the operation, the attempt, or the person; it takes a full stop because `10-grammar/Copy.md` gives one to a full sentence in a strip. |

A Document's or a picture's label line carries the file name, which is authored
and not copy. The draft's contents are authored text: Grove never edits,
shortens, summarises, prefixes, re-cases, or adds a word of its own to them.

Everything else is **None**. The draft field carries no placeholder — an empty
Note opens with an empty field and a caret, and the label already says what is
being written, so `Write a thought…` is a string with no job.

## Refusals

- **Scaling with Camera motion** — source-relative position changes; local
  chrome dimensions and the caret geometry do not.
- **Clamping after open** — viewport rescue breaks source locality.
- **Sliding to fit rather than flipping** — the distance the frame slides is
  exactly the distance it spends covering the thing it was opened to serve.
- **Covering the source, or opening centre-screen** — the frame exists to change
  words while the words stay in view, and a frame over its own source has
  removed its own reason to exist.
- **A scrim, a dim, a blur, or a translucent fill** — the Grid behind stays
  lit and live, and a draft faded behind its own question cannot be read while a
  person decides whether to lose it.
- **A formatting toolbar, a preview pane, or a second nested editor** — chrome
  for long-form work makes a small frame claim work that escalates to the Writing
  Slate instead.
- **A display-type title** — the label line names the thing once, and a second
  naming line in display type outranks the words a person came to change.
- **A round dot for unsaved work** — `00-foundations/Marks.md` closes the mark
  set at seven and refuses a status dot, and `10-grammar/Signal-roles.md` fixes
  unsaved work as the Edit hue plus the words.
- **The refusal hue on unsaved work** — refusal answers an attempt; nothing has
  been attempted while a person is still typing.
- **A `--signal-interaction` border on the frame** — the interaction hue means
  *this is what you are working on*, and a permanent interaction border makes
  focus and selection unreadable.
- **A second filled action** — one primary action per surface, and two filled
  actions ask a person to rank them.
- **Autosave, or any durable write that is not `Save`** — typing that commits
  makes cancelling a lie.
- **Truncating, ellipsising, or setting the draft below `--t-body`** — the frame
  answers the text; the text is never disciplined by the frame.
- **Dismissing on a click outside** — the Grid is deliberately live behind
  the frame, and destroying a draft because a person reached it is a punishment
  for using the product as designed.
- **A menu that repeats the footer's actions** — two routes to one action is two
  models of it, and a person then has to learn which one this frame uses.
- **A glow, a bevel, an inset highlight, or a second shadow layer** — Grove has
  two shadows, no glow, and no lit edges.


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

- Catalogue deck: `docs/design_catalogue/src/information-plane/01-local-editors.html` — the contract for this component.
- Catalogue deck: `docs/design_catalogue/src/01-menus.html` — the same frame rendered beside a Note, and the `Edit` row that opens it.
- Catalogue deck: `docs/design_catalogue/src/information-plane/02-image-viewer.html` — the View-bordered sibling that fixes the one-variable rule.
- Source note: `docs/raw/original-notes/Grove - information layer.txt` — the local Information Plane surface sits above the Grid and the Grid does not know it exists.
- Decision: `docs/decisions/Slates and local editors.md` — local editors are content-attached, do not subscribe to the camera, and return focus on one dismissal.
- Wireframe: `docs/ux/wireframes/text-editor/Text editor.md` — the order a person moves through opening, drafting, saving, refusing, and escalating.
- Reference: `docs/reference/Keybind map.md` — `Ctrl/Cmd+Enter`, `Escape`, `Tab` and `Shift+Tab` on a local surface.
