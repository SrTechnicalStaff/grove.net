---
type: design-system-component
status: active
date: 2026-08-10
component: Operation bar
plane: information
surface_class: local-editor
tags: [grove, design-system, component]
---

# Operation bar

The strip that stays beside the work while something is in hand — a Trace being
carried, Memories on their way to another Layer, content waiting to be placed,
a picture waiting to land — saying what is held and offering the one action
that finishes it.

Four operations wear this one form: a Trace, a move to another Layer, a copy,
cut or duplicate, and a picture coming in. Nothing else does. An operation that
does not fit this anatomy is not given a fifth bar; it is the operation that is
wrong.

## Anatomy

Every visible part, named, with the token that gives it its value. A part with
no token is a part with no contract.

| Part | Required | Value |
| --- | --- | --- |
| Frame | yes | `--surface-chrome`, opaque, `--r-sm`, `--sp-md` padding on every side, `--shadow-local`. |
| Role edge | yes | `1px` in the family's border token: `--k-layer-b` for a Trace and a move, `--k-edit-b` for a copy, cut, duplicate, and a picture coming in. The edge contains; it never changes to signal focus, selection, or refusal. |
| Name line | yes | The operation's own name in `--f-mono` at weight 500, `--t-label`, `--tr-label`, uppercase, in the family's fill token — `--k-layer` or `--k-edit`. One line, never wrapped. Top of the frame. |
| Sentence | yes | One sentence in `--f-ui` at weight 400, `--t-dense`, `--lh-ui`, `--text-secondary`, set to `--measure-reading`. `--sp-sm` below the name line. |
| Action row | yes | One row, actions right-aligned, `--sp-sm` between them, `--sp-md` above the row. Order left to right: the quiet action, then the committing action. |
| Committing action | yes | `--signal-interaction` fill, label in `--c-paper-ink`, `--f-ui` at weight 400, `--t-caption`, sentence case, `--r-sm`, `--sp-sm` padding on every side. Exactly one per bar. |
| Quiet action | yes | Text only — no fill, no border — `--text-primary`, `--f-ui` at weight 400, `--t-caption`, sentence case, the same padding box as the committing action so the two sit on one baseline. |
| Key hint | no | Beside its own action's label, `--sp-xs` after it, in `--f-mono` at weight 500, `--t-micro`, `--tr-wide`, `--text-meta`. |

The frame is the local editor of `10-grammar/Surface-classes.md` reduced to its
label, one sentence, and its control strip: an operation has no draft field and
no viewing stage, so the two parts a local editor may hold in the middle are
simply absent. Every other rule of the class binds unchanged.

**The role edge and the name line together identify the family, and neither
does it alone.** The edge is the containment edge the class already requires,
and the fill token on the name line is the identity mark `00-foundations/Color.md`
assigns to a role fill; the border tokens reach only `2.56:1` and `2.24:1`
against `--surface-chrome`, so an edge on its own would be a hue nobody can
read. The name line is also the carrier that survives greyscale, because it is
a word.

**Which family each operation belongs to.** A Trace and a move both exist only
to cross Layers, so both take Layer. A copy, a cut, a duplicate, and a picture
coming in all add or remove content on the Layer a person is standing on, so
all four take Edit. Two operations sharing a hue is the point: the hue names the
family and the name line names which one is open.

Measured across the four bars in `css/information-plane.css` — 40 rule sets in
all — the shipped form already agrees on most of this anatomy. All four carry a
`1px` role-tinted edge, a name line in the family hue set in monospace and
uppercase, a sentence line, an action row, a filled committing action with a
near-`--c-paper-ink` label, and a quiet action drawn as text on a transparent
ground. That agreement is the design and it is recorded here. Five values are
corrected against it and each is named in **Sources**: the frame's fill and
blur, the frame's missing radius, the sentence's family and ink, the committing
action's fill, and the count of actions.

The bar carries no title bar, no close glyph, no destination picker, no input
field, no progress figure, no count badge, and no control that outlives the
operation.

## Geometry

- **Footprint** — the bar occupies no cells; it is chrome at screen scale. The
  projection bridge reprojects its source anchor without scaling the bar. Its width is a procedure with one answer:
  the sentence's content box is `--measure-reading`, and the frame is that plus
  `--sp-md` on each side. Every operation bar is that width, whatever it holds,
  which is what makes four operations read as one thing. The shipped widths are
  `360px`, `300px`, `320px`, and `300px`; they are replaced, because a width set
  per operation makes the family look like four unrelated surfaces.
- **Growth** — the sentence wraps within its measure and the frame grows taller;
  when the two actions and their key hints cannot sit on one line, the row wraps
  and right-aligns on the second line inside the same frame, which is the narrow
  layout `10-grammar/Surface-classes.md` fixes for an action row. Nothing is
  ever shortened, ellipsized, or set smaller.
- **Measure** — `--measure-reading` on the sentence. The name line is a label,
  is one line, and never wraps.
- **Alignment** — the bar lands on no grid line. Its near edge sits `--sp-lg`
  clear of the source footprint's edge and is vertically centred on it. The
  opening side is derived once from the exact source. Pan and zoom move the bar
  with that source without scaling it. It does not re-flip or clamp after open
  and may leave the viewport with the source.

**The source, stated as a procedure.** For a Trace, a move, and a copy, cut or
duplicate, the source is the bounding rectangle of the selected placements at
the moment the operation opens. For a picture coming in, the source is the cell
the file was dropped on, or the centre cell of the view when the operation came
from a key. Every operation therefore has a Grid address before its bar is
drawn, and the bar is source-anchored in all four cases — there is no
viewport-fixed operation bar, and none is centred.

## States

All nine.

| State | Appearance | Notes |
| --- | --- | --- |
| Rest | Absent. The bar has no resting appearance, because it exists only while an operation is open. | This is the whole of Law 3 for this component: nothing about an open operation is drawn before one is opened, and nothing is left behind after one ends. |
| Approached | No change from Rest. | A surface a person opened is not chrome answering the hand, so approach adds nothing and the pointer raises no interaction signal anywhere in the bar. |
| Focused | The action holding keyboard attention takes `--focus-ring` at `--focus-ring-offset`, drawn outside the action and never around the frame. | Focus is on a part, never on the frame; the frame is not focusable and takes no ring. |
| Selected | The bar is never Selected. | Selection names content a person is working on; the placements the operation holds keep their own outline and field response on the Grid, unchanged, for the whole life of the bar. |
| Engaged | The bar does not change. While the pointer is choosing cells, the preview draws on the Grid in `--signal-interaction`; pointer-down on an action takes the pressed appearance over `--d-press`. | The preview belongs to `30-components/Placement-preview.md` and is not restated here. |
| Pending | The sentence and the frame hold exactly as they are and the committing action goes Unavailable until the result arrives. | The bar is never blank, never carries a spinner, a bar, a percentage, or a wait message: the fullest form it already has is its own sentence. |
| Refused | The cells that cannot take the operation hatch at 45° and the refused footprint takes the refusal hue on the Grid; the bar's own sentence becomes the refusal sentence and the committing action goes present and unavailable. The frame keeps its role edge. | The bar raises no second strip and no inline confirm: it is already a strip beside the work carrying a sentence and the committing action, and `30-components/Refusal.md`'s strip would be a second copy of a surface that is open. The frame is never tinted refusal, because the quiet action must not look dangerous. |
| Unavailable | The committing action alone: `--text-unavailable` on a `1px` `--edge-hairline` border, in place, at the same size. | The whole bar is never Unavailable — a bar with nothing available is an operation that should have ended. The quiet action is available for the whole life of the bar, without exception. |
| Anchored | Never. | Anchored is a durable property of authored content; an operation is not authored, nothing about it persists, and the Memories it holds carry their own anchor marks on the Grid. |

Combination follows `10-grammar/States.md` without exception. Focused plus
Unavailable on the same action draws both: the ring is never suppressed on an
action a person can still reach and read.

## Behaviour

- **Pointer** — a click on the committing action commits, on pointer-up over it.
  A click on the quiet action ends the operation, on pointer-up over it. For a
  copy, cut, or duplicate, a click on an open area of the Grid also
  commits, at the clicked cell; that gesture belongs to the Grid and the
  bar only reports it. There is no drag, no resize, and no double-click gesture
  anywhere in the bar.
- **Keyboard** — `Ctrl/Cmd+Enter` commits, wherever focus sits, until the
  operation ends. `Escape` ends the operation. `Tab` and `Shift+Tab` move focus
  between the two actions. `Enter` and `Space` fire the focused action. The keys
  that open each operation, and `A`, which adds the current selection to an open
  Trace, are `docs/reference/Keybind map.md`'s and none is redefined here. Focus
  arrives on the committing action when the bar opens, because a non-destructive
  commit is what the person came for, and leaves to the quiet action.
- **Focus order** — the committing action, then the quiet action. The frame, the
  name line, and the sentence are never in the tab order, because focus never
  lands on something that does nothing.
- **Escape** — Escape ends the operation and closes the bar, and only the bar.
  Nothing durable is undone, because nothing durable happened. Focus returns to
  the Grid cursor at the source's cells. A menu opened over the bar dismisses
  first, per the escape order in `10-grammar/Surface-classes.md`.
- **Commit and cancel** — the commit is durable on the committing action or on
  `Ctrl/Cmd+Enter`, and **the bar closes on the frame the commit lands**. A
  refused commit changes nothing, leaves the operation open, and turns the
  sentence into the refusal sentence. Escape and the quiet action both end the
  operation with nothing moved, copied, or placed, so neither raises an inline
  confirm — a question about work that can be repeated for free teaches a person
  to stop reading questions.

Opening a second operation ends the first. One operation is open at a time,
because two open operations both answering `Ctrl/Cmd+Enter` make the commit key
ambiguous and a person cannot see which one they aimed at.

Choosing which Layer a move goes to belongs to the numbered flyout in
`10-grammar/Surface-classes.md`; the destination is already chosen when the bar
opens. Selecting the content an operation holds, previewing the cells it will
take, and the arrival of the content itself belong to the Grid.

## Motion

| Transition | Duration | Curve | Reduced motion |
| --- | --- | --- | --- |
| The bar arriving when the operation opens | `--d-fade` | `--ease` | Present at the identical threshold, with no fade. |
| The bar leaving when the operation ends or commits | `--d-fade` | `--ease` | Absent on the frame the operation ends. |
| Pointer-down acknowledgement on an action | `--d-press` | `--ease` | The pressed appearance applied on press and removed on release, with no scaling. |

Three transitions, and no fourth. The name line, the sentence, the role edge,
the focus ring, and the change of the committing action to and from Unavailable
are all frame changes with no transition, because the settled frame carries the
whole meaning. The content committing to its cells is the placement's own
arrival over `--d-place`, owned by that placement.

Nothing loops. Nothing idles. Nothing pulses or blinks. The bar never delays its
own dismissal to show that a commit succeeded.

## Distance

Thresholds are on projected cell size and belong to
`10-grammar/Representation-tiers.md`.

| Tier | Sheds | Keeps |
| --- | --- | --- |
| Working | Nothing. | The whole bar at one size. |
| Stepped | Nothing. | The whole bar at one size. |
| Stand-in | Nothing. | The whole bar at one size. |

The bar is chrome on a local surface, drawn in screen space at one size at every
camera scale, so it has no representation tier of its own and sheds nothing.
What does shed is the source beneath it: the placements the operation holds and
the cells it will commit to step down with the field, and the bar keeps stating
what is in progress while they do. A bar that thinned as the camera pulled back
would hide the one sentence explaining why the Grid is in the state it is.

## Accessibility

- **Role and name** — the bar is a region, and its accessible name is its own
  name line, verbatim, by reference. `Trace session`, `Content clipboard`,
  `Move Content`, and `Import Image` are defects as accessible names: three
  carry architecture nouns and all four say something different from what is on
  screen.
- **Contrast** — the name line reaches `9.04:1` in `--k-layer` and `8.92:1` in
  `--k-edit` on `--surface-chrome`. The sentence at `--text-secondary` reaches
  `6.35:1`. The quiet action at `--text-primary` reaches `10.36:1`. The
  committing action's `--c-paper-ink` label reaches `8.57:1` on its
  `--signal-interaction` fill, and that fill reaches `8.90:1` against the frame,
  which is what defines the control's boundary. The focus ring reaches `8.90:1`.
  The role edge reaches `2.56:1` and `2.24:1` and is exempt, because it is a
  containment edge and the name line carries the family in words. The
  unavailable committing action at `--text-unavailable` reaches `2.46:1` and is
  the exemption `00-foundations/Accessibility.md` names, paired with its
  hairline border and its retained position.
- **Without colour** — Focused: a ring outside the action at a distinct offset.
  Pending: the committing action's hairline border and retained position.
  Refused: 45° hatching on the blocked cells and the sentence. Unavailable: the
  hairline border and the retained position. The family: the name line's word.
  The committing action: the filled box against two unfilled neighbours. Rest,
  Approached, Selected, Engaged, and Anchored draw nothing in the bar, so there
  is nothing for greyscale to lose.
- **Forced colours** — the frame fill, the role edge, the name line, the
  sentence, both actions, the focus ring, and the unavailable border are
  redeclared in system colours. What survives without redeclaration is the bar's
  position beside its source, the name line's word, the sentence, the order of
  the action row, the ring's offset, and the hatch on the blocked cells.
- **Text scaling** — every string in the bar is interface text and grows with
  it. The measure holds in `ch`, so the sentence takes more lines and the frame
  grows taller; the action row wraps to a second line inside the same frame; the
  name line grows on one line. Nothing truncates and no scrollbar appears inside
  the bar at any scale. The content the operation holds is measured in cells and
  does not scale with interface text.
- **Reduced motion** — no change from the Motion table.

## Copy

Every user-visible string this component can show.

**Name lines.** One word or two, sentence case, rendered uppercase by
`--tr-label` rather than by writing capitals.

| String | Where |
| --- | --- |
| `Trace` | A Trace being carried. |
| `Move` | Memories on their way to another Layer. |
| `Copy` | A copy waiting to be placed. |
| `Cut` | A cut waiting to be placed. |
| `Duplicate` | A duplicate waiting to be placed. |
| `Image` | A picture coming in. |

**Sentences.** One sentence, one pattern for all four operations: what is held,
then what finishes it. The count and the singular form are part of the string;
authored names — a Layer's name, a file's name — are carried verbatim.

| String | Where |
| --- | --- |
| `1 Memory is held and ready to drop off.` / `3 Memories are held and ready to drop off.` | A Trace. |
| `1 Memory is ready to move to Field Notes.` / `3 Memories are ready to move to Field Notes.` | A move, naming the Layer the person chose. |
| `1 Memory is ready to place.` / `3 Memories are ready to place.` | A copy, a cut, or a duplicate. |
| `Harbor at dusk.jpg is ready to place.` | A picture coming in, naming the file. |
| `This space is occupied.` | Any of the four, when the cells cannot take the operation. |
| `This picture cannot be read.` | A picture coming in, when the file will not decode. |

A Layer a person has not named is named in the sentence by its own label —
`01`, `02` — never by a generated title, because Grove never writes a name
nobody wrote.

**Actions.**

| String | Where |
| --- | --- |
| `Drop off` | The committing action of a Trace. |
| `Move` | The committing action of a move. |
| `Place` | The committing action of a copy, a cut, a duplicate, and a picture coming in. |
| `Cancel` | The quiet action of all four. |
| `Ctrl+Enter` / `⌘+Enter` | The key hint beside the committing action. |
| `Esc` | The key hint beside the quiet action. |

Every string passes `10-grammar/Copy.md`: verbs for the actions, plain English
elsewhere, sentence case, no terminal punctuation on a label and a full stop on
every sentence, no architecture noun, and no word that would need changing if
the product were rebuilt. `Cancel` is kept rather than replaced: the shipped
product uses it on all four bars and the design-language deck draws it as the
quiet action of a local surface, and the refusal of `Cancel` in
`10-grammar/Surface-classes.md` is scoped to a question raised by cancelling,
which no operation bar asks.

There is no success string, no progress string, no empty string, and no
placeholder. `Importing image…`, `Image placed.`, and `Trace dropped off on
this Layer.` are all removed: a result belongs in the Grid where it landed,
and a line that appears in order to leave is a toast wearing a bar's clothes.

## Refusals

- **Existing at rest** — a bar drawn when no operation is open charges the whole
  Grid, permanently, for something that happens occasionally.
- **A third action** — three choices mean the pair was wrong; a second way to
  end the operation, and a control for a step the keyboard already carries, both
  make the person rank actions that are not ranked.
- **A destination picker, a text field, or any other input** — a local surface
  may not carry one, and a destination is chosen in the flyout before the bar
  opens.
- **The role hue on the committing action** — the fill of the one action that
  commits is `--signal-interaction` everywhere in Grove, and a family hue there
  would make the commit look like an identity rather than an action.
- **The refusal hue on the frame** — the cells are what is refused; colouring
  the whole bar makes the quiet action look dangerous too.
- **A translucent fill or a backdrop blur** — both make the Grid an
  ingredient of the bar's own colour, and a blurred Grid is a scrim by
  another name.
- **A centre-screen position** — every operation has a Grid address before its
  bar is drawn, so a bar in the middle of the viewport has thrown away the one
  thing that says what it is about.
- **A spinner, a progress bar, a percentage, or a wait message** — a result that
  has not arrived is answered by holding the fullest form already present, which
  here is the bar's own sentence.
- **A success line, a toast, or a delayed dismissal** — the placed content is
  the result, and a bar that lingers to congratulate itself is chrome outliving
  its operation.
- **A count riding on the Grid** — the count belongs in the bar's sentence,
  where it is the subject, and nowhere else.
- **Two operation bars open at once** — two bars answering one commit key make
  the key ambiguous.
- **An inline confirm on cancelling** — nothing durable is lost, and a question
  about free work teaches a person to stop reading questions.


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

- Plane: `docs/design-system/20-planes/Information-plane.md` — this plane owns bars that carry an open operation.
- Grammar: `docs/design-system/10-grammar/Surface-classes.md` — the local editor's frame, label, footer, and action pair; the numbered destination flyout.
- Foundation: `docs/design-system/00-foundations/Color.md` — the role fill identifies and the role border contains.
- Foundation: `docs/design-system/00-foundations/Elevation-and-depth.md` — `--shadow-local` for any bar that floats beside the thing it is about; translucency and backdrop blur refused.
- Foundation: `docs/design-system/00-foundations/Space-and-grid.md` — `--sp-md` for a local surface's padding, `--sp-sm` between a quiet action and the primary beside it.
- Catalogue deck: `docs/design_catalogue/src/00-design-language.html` — the operation-family fill and border pairs; the local editor's footer drawn as UI-type actions with the committing one filled in the interaction accent.
- Catalogue deck: `docs/design_catalogue/src/01-menus.html` — `Move to` and its numbered Layer flyout.
- Catalogue deck: `docs/design_catalogue/src/information-plane/01-local-editors.html` — the class frame, the quiet mono label, the footer row, the key hint.
- Reference: `docs/reference/Keybind map.md` — `T`, `M`, `I`, `Ctrl/Cmd+C/X/D/V`, `A` on an open Trace, and `Ctrl/Cmd+Enter` staying live until the operation ends.
