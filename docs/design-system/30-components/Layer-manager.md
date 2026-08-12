---
type: design-system-component
status: active
date: 2026-08-09
component: Layers
plane: hud
surface_class: slate
tags: [grove, design-system, component]
---

# Layers

The whole stack of Layers, in order, with the one a person is working on marked
and every Layer named, added, reordered, renamed, or removed from its own row.

## Anatomy

Every visible part, named, with the token that gives it its value. A part with
no token is a part with no contract.

| Part | Required | Value |
| --- | --- | --- |
| Frame | yes | `--surface-chrome`, opaque; `--r-sm`; `1px` `--k-slate-b`; `--sp-lg` padding on every side; no shadow and no rounded corner, because a Slate fills the viewport edge to edge. |
| Identity line | yes | `Layers` in `--f-display` at weight 500, `--t-title-small`, `--tr-label`, uppercase, ink `--k-slate`; `--sp-lg` beneath it. |
| Close control | yes | At the right of the identity row. Label in `--f-ui` at `--t-caption`, `--text-primary`; `1px` `--edge-quiet`; `--r-sm`; padding `--sp-xs` top and bottom, `--sp-md` at the sides. |
| Stack count | yes | `--f-mono` at weight 500, `--t-label`, `--tr-label`, uppercase, `--text-meta`; `--sp-sm` beneath it. |
| Filter field | yes | Full width of the padding box; fill `--surface-nested`; `1px` `--edge-quiet`; `--r-sm`; padding `--sp-sm`; text `--f-ui` at `--t-dense`, `--lh-ui`, `--text-primary`; placeholder `--text-meta`; `--sp-md` beneath it. |
| Stack list | yes | One column, top of the stack first, `--sp-xs` between rows. No fill, no border, no separator; it takes the height left after the parts above it and scrolls within that height. |
| Row | yes | Two columns — the label token, then the name — with `--sp-sm` between them; padding `--sp-sm` top and bottom and none at the sides, so a row's outline reaches the frame's padding box. |
| Row label token | yes | The Layer's own label in `--f-mono` at weight 500, `--t-label`, `--tr-mono`, `--text-meta`, in a fixed `4ch` column. |
| Row name | yes | The name a person wrote, in `--f-ui` at 400, `--t-dense`, `--lh-ui`, `--text-secondary`; `--text-primary` on the current Layer's row. It wraps at `--measure-reading` and never truncates. |
| Rename field | no | Replaces the row name in place: fill `--surface-nested`; `1px` `--edge-found`; `--r-sm`; padding `--sp-xs`; text `--f-ui` at `--t-dense`, `--text-primary`. Present only while a rename is open. |
| Row menu | no | The menu class of `10-grammar/Surface-classes.md`, opened at the invoking point on one row. Rows in four groups: `Rename`; `Add above`, `Add below`; `Move up`, `Move down`; `Remove`. |
| Inline confirm | no | The inline confirm class, drawn full width of the list directly beneath the row it is about, pushing later rows down. Present only while a removal is being answered. |

The `4ch` label column is this component's own figure: `B01` is the longest
label Grove mints, so four characters is the narrowest column in which every
name starts on the same line, and a column measured in `ch` follows the type
rather than the pane.

The surface carries no toolbar, no footer, no second title, no status line, and
no control that is not either a host command in the header or a row in a menu
opened on a Layer. Every command in it targets one Layer, so every command
lives on that Layer's row.

**The Layer role hue does not appear here.** `--k-layer` identifies the Layer
family of *controls* — it is on the Layer key caps in the Controls HUD, which
is its only use in the product — while the identity of a composed surface is
the Slate hue, which `10-grammar/Surface-classes.md` fixes for every Slate
header. The shipped surface paints seven declarations in the Tool hue
`#b3a9e0` / `#5b5288`, which is neither.

## Geometry

- **Footprint** — this surface takes no cells. The Slate host composes it Full,
  Left, or Right, and the pane's extent is the host's. Executed as a procedure:
  the frame fills the pane; the identity row, the count, and the filter field
  each take their own height at the top of the padding box in that order; the
  list takes every remaining pixel of height and scrolls inside it. The same
  pane produces the same layout twice, because nothing in the procedure reads
  the camera, the stack's length, or the filter.
- **Growth** — the list scrolls, which is what a composed surface does instead
  of shortening what it carries. A name wraps and its row grows taller; the
  surface never sets a name smaller, never clips it, and never gives it an
  ellipsis. The surface itself never drags, never resizes, and never grows past
  its pane.
- **Measure** — the name column is bounded by `--measure-reading`, so a long
  name wraps at reading rhythm rather than at whatever width the host gave the
  pane.
- **Alignment** — every part's left edge lands on the frame's `--sp-lg` padding
  box, and the Close control's right edge lands on its right side. Names start
  on one left edge because the label column is fixed, so the stack reads as a
  single column of names with an identifier beside it.

## States

All nine.

| State | Appearance | Notes |
| --- | --- | --- |
| Rest | The frame, the identity line, the Close control, the count, the filter field, and the stack with the current Layer's row marked. No menu, no rename field, no confirm. | The surface is summoned deliberately and holds nothing that arrives on its own. |
| Approached | No change from Rest. | A row does not light under the pointer: `10-grammar/Signal-roles.md` raises no interaction signal on hover outside an open menu, where pointer and keyboard light the identical row. |
| Focused | The focused part takes `--focus-ring` at `--focus-ring-offset`. On the current Layer's row the ring is drawn at `7px` instead, so it sits outside that row's selection outline with a visible gap. | `7px` is this component's figure: the selection outline runs from `3px` to `5px` outside the row, and `7px` is the first offset that clears it by the `2px` the state model calls a visible gap. |
| Selected | The current Layer's row carries a `2px` `--signal-interaction` outline offset `3px` outside its box, and its name goes `--text-primary`. | Exactly one row is ever marked. The ink step is reading hierarchy; the outline's offset is the carrier that survives without hue. |
| Engaged | Pointer-down on a row, a menu row, or a control takes the pressed appearance over `--d-press`. An open rename field holds the caret and every typed character lands on the input frame. | There is no drag in this surface, so no gesture can be left half-open. |
| Pending | No change from Rest. Every Layer command resolves on the frame it is asked for. | If a stack ever arrives from storage the list holds the rows it already has; it is never blank and never carries a progress mark. |
| Refused | The inline confirm opens in its refusal form beneath the row: its frame border goes `--signal-refusal`, it states the condition in one sentence, `Keep it` stays available, and `Remove` is present and unavailable. | No hatch: `00-foundations/Marks.md` keeps the refusal hatch on Grid cells and never inside a Slate. The refusal form carries the dismissal and the unavailable committing action rather than a retry, because a second attempt at the same move would be answered identically and `10-grammar/States.md` requires the action that would commit to stay visible. |
| Unavailable | `Move up`, `Move down`, and `Remove` are drawn in `--text-unavailable` in their usual position in the menu when they cannot act. | `10-grammar/Surface-classes.md` narrows the state model for a menu row: the row keeps its place and takes the faint ink, and no border is added inside a menu that has none. |
| Anchored | Not reachable. A person writes an Anchor onto a Memory or a placement, never onto a Layer, so nothing in this surface carries authored context. | The authored-context signal never appears here. |

Combination follows `10-grammar/States.md` without exception. Focused plus
Selected is the one combination this surface draws often, and the two are told
apart by which line sits further out.

## Behaviour

- **Pointer** — a click on a row makes that Layer the current Layer, committing
  on pointer-up over the row, and clears the placement selection, because a
  selection on a Layer a person has left is a selection they cannot see. A
  context gesture on a row opens that row's menu at the invoking point; opening
  it changes nothing. Choosing a menu row acts on the Layer the menu was opened
  on. Typing in the filter field narrows the list on each keystroke. Close
  dismisses the surface. There is no drag anywhere in this surface, and no
  double-click gesture.
- **Keyboard** — `L` opens this surface and moves focus into it; pressed while
  it is open, `L` returns focus to the current Layer's row. `↑` and `↓` move
  roving focus one row, `Home` and `End` move it to the top and the bottom of
  the list, and `Enter` makes the focused Layer current. `Ctrl/Cmd+N` opens the
  rename field on the current Layer's row. `Shift+[` and `Shift+]` add a Layer
  at the bottom and the top of the stack; `Ctrl/Cmd+[` and `Ctrl/Cmd+]` add one
  immediately below and above the current Layer; `Alt+[` and `Alt+]` reorder the
  current Layer within its side of the stack; `[` and `]` traverse to the
  previous and next Layer. Every one of those is
  `docs/reference/Keybind map.md`, and none is redefined here. While the filter
  field or a rename field holds the caret, `Shift+[` and `Shift+]` type their
  characters and the modified bracket keys still act, because a surface that
  takes text input does not swallow the keys that leave it.
- **Focus order** — Close, the filter field, then the list, which is one tab
  stop with roving focus inside it. Opening the surface puts focus on the
  current Layer's row, because that is where the person already is and it is
  what every command targets.
- **Escape** — one layer per press, in this order: a rename field restores the
  previous name and closes; an open menu closes and focus returns to its row; an
  open confirm chooses `Keep it` and focus returns to the row; a filter with
  text in it clears and the list returns to the current Layer's row; otherwise
  the surface closes and focus returns to whatever opened it. A filter is a
  narrowing a person added, so removing it is peeling one layer.
- **Commit and cancel** — the current Layer changes on pointer-up or on `Enter`.
  A name becomes durable on `Enter` in the rename field, and `Escape` restores
  the previous name at no cost. A created Layer and a reorder are durable on the
  action that asked for them. A removal is durable only on `Remove` in the
  confirm, and on an empty Layer it is durable on the menu row itself. Every one
  of these is a product transition and `Ctrl/Cmd+Z` undoes it.

**Renaming raises no question.** Escape on a rename field with unsaved
characters restores the previous name and says nothing: a question about one
line of typing that can be retyped teaches a person to stop reading questions.

**Adding.** `Add above` inserts a Layer immediately above the row's Layer and
`Add below` immediately below it. A Layer added above `01` takes the next
unused number from `02` upward; one added below takes `B` and the next unused
number from `B01` downward. A label is a stable identity and not a position, so
reordering never renumbers a row. A new Layer becomes the current Layer, is
scrolled into view, and carries no name until a person writes one.

The surface offers two creation actions and not four: adding at the top of the
stack is `Add above` on the top row and adding at the bottom is `Add below` on
the bottom row. The four keys stay, because a key acts with no row under the
pointer.

**Reordering.** `Move up` and `Move down` exchange the row with its neighbour
on the same side of `01`. Both are present and unavailable on `01` itself and
at each end of a side, because whether the move can happen is knowable before
the attempt and a standing condition is Unavailable, never Refused.

**Removing, and what is on the Layer.** Removing a Layer never removes what is
placed on it. Everything on the removed Layer moves to the Layer that closes
over the gap — the next Layer down the stack, or the next one up when the
removed Layer is at the bottom — and the confirm names that Layer before
anything commits. `Remove` is present and unavailable on `01` and on the only
Layer in the stack. On a Layer holding nothing, `Remove` commits on the menu
row and raises no question, because nothing a person placed changes. On a Layer
holding placed work, `Remove` opens the confirm beneath the row; if what is on
the Layer would land where the destination is already occupied, the confirm
opens in its refusal form and nothing moves.

**Anchored scrolling.** The list holds the current Layer's row at a fixed
distance from the top of its own scroll box while rows arrive above or leave
above it, so nothing a person is reading moves under the pointer when the stack
changes. Opening the surface, clearing the filter, and any change of the current
Layer scroll that row fully into view. The scroll never animates.

**Filtering at scale.** The filter matches the label token and the name as a
plain case-insensitive substring, in stack order, with no ranking and no
highlight of the matched run — no signal answers "this text matched", and
`10-grammar/Signal-roles.md` forbids inventing one. The current Layer's row is
always listed whether or not it matches, so the list is never empty, the
surface can always say where a person is, and no empty state exists to design.
Filtering never reorders: stack order is the only order this surface has.

Camera framing of the current Layer, transfer of placed work between Layers,
and how presence from one Layer saturates its neighbours belong to the
Grid and the field, not to this surface.

## Motion

| Transition | Duration | Curve | Reduced motion |
| --- | --- | --- | --- |
| The surface arriving on open | `--d-fade` | `--ease` | Present at the identical threshold, with no fade. |
| The surface leaving on Close or Escape | `--d-fade` | `--ease` | Absent on the frame the dismissal commits. |
| A row's menu arriving and leaving | `--d-fade` | `--ease` | Present or absent at the identical threshold. |
| The inline confirm arriving and leaving | `--d-fade` | `--ease` | Present or absent at the identical threshold. |
| Pointer-down acknowledgement on a row, a menu row, or a control | `--d-press` | `--ease` | The pressed appearance applied on press and removed on release, with no scaling. |

`00-foundations/Motion.md` names the curves and owns which step a transition
takes; this surface uses only the two steps above, because no other step names
anything it does.

Rows do not animate in or out when a Layer is added or removed, the list's
scroll never animates, the rename field replaces the name on the frame the
rename opens, and the selection outline and the focus ring appear on the frame
attention or the current Layer changes. Nothing loops. Nothing idles. Nothing
pulses or blinks.

## Distance

Thresholds are on projected cell size and belong to
`10-grammar/Representation-tiers.md`.

| Tier | Sheds | Keeps |
| --- | --- | --- |
| Working | Nothing. | Everything. |
| Stepped | Nothing. | Everything. |
| Stand-in | Nothing. | Everything. |

This surface is fixed to the viewport and is not addressed in cells, so the
camera never reaches it and it holds no tier. It has no stand-in and no
kind-coded form, because it is not a placement and casts no presence. Pan and
zoom behind it change nothing about it, and it changes nothing about them.

## Accessibility

- **Role and name** — the surface is a labelled region whose accessible name is
  its identity line, `Layers`. The list is a listbox that takes the same name by
  reference, so the surface is named once rather than twice. Each row is an
  option whose accessible name is exactly the text of the row — its label token
  and the name a person wrote, in the order they are drawn. A Layer nobody has
  named is announced by its label alone; `Untitled Layer` is a defect, because
  Grove never generates a name for something a person left unnamed.
- **Contrast** — on `--surface-chrome`: the identity line in `--k-slate` reaches
  9.86, `--text-primary` 10.36, `--text-secondary` 6.35, `--text-meta` 4.71, the
  selection outline and the focus ring in `--signal-interaction` 8.90, and the
  refusal border and the destructive label in `--signal-refusal` 5.28. The
  destructive `Remove` in the confirm sets `--c-paper-ink` on a
  `--signal-refusal` fill at 5.08. `--text-unavailable` reaches 2.46 and is the
  documented exception, paired with a retained position. The frame's
  `--k-slate-b` border reaches 3.03 against `--surface-grid`, which is the
  surface it separates the Slate from and the ratio that carries containment,
  and 2.84 against the Slate's own fill. The confirm's `--edge-hairline` border
  is surface treatment rather than a meaning-bearing edge, because the
  `--surface-nested` fill against `--surface-chrome` is what states its extent.
- **Without colour** — Approached: nothing changes, so nothing is lost. Focused:
  a ring outside the part, at a distinct offset. Selected: an outline outside the
  row at a `3px` gap, which no other row has. Engaged: the pressed appearance.
  Pending: nothing changes. Refused: a bordered row that was not there before,
  sitting beneath the row it is about, carrying one plain sentence, with the
  committing action retained and faint. Unavailable: a retained position in the
  menu. Anchored: not reachable.
- **Forced colours** — the frame fill and border, the identity ink, both row
  inks, the label token, the filter field's fill and border, the selection
  outline, the focus ring, the confirm's fill and border, and the destructive
  action's fill and label are all redeclared in system colours. What survives
  without redeclaration is the selection outline's offset, the focus ring's
  larger offset, the fixed label column, the confirm's position beneath its row,
  and the retained position of an unavailable menu row.
- **Text scaling** — the identity line, the count, the filter field, and every
  row grow with interface text. A row grows taller and its name wraps within
  `--measure-reading`; the label column grows with the type because it is
  measured in `ch`. The list scrolls, which is what a composed surface is
  permitted to do. Nothing truncates at any scale, and no control group gains a
  scrollbar of its own.
- **Reduced motion** — no change from the Motion table.

## Copy

Every user-visible string this component can show.

| String | Where | Why it passes |
| --- | --- | --- |
| `Layers` | The identity line, rendered uppercase by `--tr-label` rather than by writing capitals; also the accessible name of the surface and of its list. | A Grove product noun naming a place, true after any rebuild. |
| `Close` | The host command in the header. | A verb for an action, ordinary English, sentence case, no terminal punctuation. |
| `12 Layers` | The count line, with `1 Layer` in the singular. | A figure and a product noun; it names what is present and nothing about how it is stored. |
| `4 of 12 Layers` | The count line while the filter holds text. | Says how much of the stack is listed, in ordinary English. |
| `Find a Layer` | The filter field's label and, in the same words, its placeholder, so the accessible name never depends on the placeholder. | A verb for an action on a product noun. |
| `Rename` | A row's menu. | A verb naming what happens, not the mechanism. |
| `Add above` | A row's menu. | A verb and a direction a person can see in the list. |
| `Add below` | A row's menu. | As above. |
| `Move up` | A row's menu. | A verb and a direction that matches the drawn order. |
| `Move down` | A row's menu. | As above. |
| `Remove` | A row's menu, alone in the last group in `--signal-refusal`; also the committing action in the confirm. | A verb naming the outcome; the committing action repeats the verb of the loss. |
| `Remove Layer B02 and move what is on it to Layer 01?` | The confirm's question, with the two labels taken from the Layers themselves. | One sentence ending in a question mark, naming the consequence and where the work goes; "what is on it" is true whatever is on it, where an enumeration could be wrong. |
| `Keep it` | The confirm's quiet action. | Names what continues, and is the wording `10-grammar/Surface-classes.md` already fixes for a removal. |
| `That space is occupied on Layer 01.` | The refusal form of the confirm, with the label taken from the destination Layer. | One clause, present tense, stating the condition rather than the failure, blaming neither the person nor the product. |

Everything else is **None**. A Layer's name is authored content, not copy:
Grove never edits it, shortens it, prefixes it, or supplies one when a person
has not written one.

## Refusals

- **A scrim, or any surface centred on the viewport** — Law 7 keeps the
  Grid lit and live, and a rename that darkens the whole screen claims an
  interruption Grove does not make.
- **A dialog for renaming** — a rename destroys nothing and is undoable, and
  `10-grammar/Surface-classes.md` refuses a question about a reversible action.
- **Removing a Layer that takes its content with it** — a Layer is a position in
  a stack, not a container that owns what stands on it, and Grove never destroys
  placed work as a side effect of tidying.
- **A status dot on the current row** — `00-foundations/Marks.md` refuses the
  status dot permanently, and a `10px` glow crosses the row boundary that the
  selection outline exists to state.
- **An ellipsis on a Layer's name** — the row grows taller instead, because a
  name a person wrote is never shortened to fit the pane it is listed in.
- **A generated name for an unnamed Layer** — a supplied title is a title nobody
  wrote, and the label already identifies the row.
- **A count of what is placed on a Layer, beside its row** — the row exists to
  name a Layer so a person can go to it, and a figure beside the name competes
  with the one glance the row answers.
- **A destination chooser inside the removal question** — one question takes one
  answer, and a picker in a confirm asks two.
- **Drag to reorder** — a drag has no keyboard equal, and the grip that would
  advertise it is a refused mark.
- **A role hue anywhere on this surface** — the Slate hue is the class's
  identity mark and it appears in the header and nowhere else.
- **A translucent fill or a backdrop blur** — every fill in all four surface
  classes is opaque, because a surface that lets the Grid bleed through
  stops being readable at the moment it matters most.
- **A toolbar, a footer, or a second title inside the surface** — a Slate holds
  one identity, the host's commands, and content; every other command belongs on
  the row it acts on.
- **A mark drawn on the Grid when a command cannot run** — a colour that
  flashes and returns leaves its meaning only in the motion, and an answer drawn
  outside the surface that asked is an answer nobody is looking at.
- **Reordering the list by anything but stack order** — the list is a picture of
  the stack, and a list sorted by name stops being one.


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

- Reference: `docs/reference/Grid and Placement model.md` — a protected first Layer, new Layers above or below it, stable side-coded labels that are not array indexes, and the requirement that creating, moving, renaming, and deleting a Layer preserve surviving Layers and their placed Content.
- Source note: `docs/raw/original-notes/Grove - Layers.txt` — a Layer is a position in a stack that the field reads through, not a container that owns content.
- Reference: `docs/reference/Keybind map.md` — `[`, `]`, `Shift+[`, `Shift+]`, `Ctrl/Cmd+[`, `Ctrl/Cmd+]`, `Alt+[`, `Alt+]`, `Ctrl/Cmd+N`, and the rule that no keyboard shortcut removes a Layer.
- Catalogue deck: `docs/design_catalogue/src/hud/01-slate-anatomy.html` — opaque fill, one quiet border, `2px` corner, `24px` padding, header identity in the Slate hue, no scrim, never floats free.
- Catalogue deck: `docs/design_catalogue/src/00-design-language.html` — no scrim and no centred dialog; confirmations are inline in the surface that asked.
- Catalogue deck: `docs/design_catalogue/src/01-menus.html` — menu row anatomy, grouping, the destructive row alone in the last group, and Escape peeling one layer.
