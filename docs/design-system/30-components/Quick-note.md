---
type: design-system-component
status: active
date: 2026-08-09
component: Quick Note
plane: information
surface_class: slate
tags: [grove, design-system, component]
---

# Quick Note

A place to write a Note down the moment it arrives, before it has anywhere to
go, with everything already written kept whole beneath it.

## Anatomy

Every visible part, named, with the token that gives it its value. A part with
no token is a part with no contract.

| Part | Required | Value |
| --- | --- | --- |
| Frame | yes | `--surface-chrome`, opaque, `--r-sm`, `--sp-lg` padding on every side, `--shadow-local`. |
| Frame border | yes | `1px` `--k-slate-b`. |
| Identity line | yes | `Quick Note` in `--f-display` at `--t-title-small`, `--tr-label`, `--lh-tight`, uppercase, ink `--c-slate`, with `--sp-lg` beneath it. |
| Header control | yes | `Close` at the right of the header: `--f-ui` at `--t-caption`, `--text-secondary`, `1px` `--edge-found`, `--r-sm`, `--sp-sm` top and bottom and `--sp-md` at the sides. |
| Capture field | yes | `--surface-nested`, `--r-sm`, `1px` `--edge-quiet`, `--sp-md` padding on every side; text in `--f-ui` at weight 400, `--t-lead`, `--lh-reading`, `--text-primary`. |
| Caret | yes | `2px` wide, `1.05em` tall, filled `--signal-interaction`. Present only while the capture field holds focus. |
| Hint line | yes | Beneath the capture field with `--sp-sm` above it; each hint is a key and its words in `--f-mono` at `--t-label`, `--tr-mono`, `--lh-ui`, `--text-meta`, one line, `--sp-md` between hints. |
| Composer action row | no | One row at the foot of the capture field, `--sp-md` above it: the quiet action then the committing action, `--sp-sm` apart. Present only while the capture field holds a saved Note pulled back for editing. |
| Quiet action | no | Text only — no fill, no border — `--f-ui` at `--t-caption`, `--text-primary`. |
| Committing action | no | `--signal-interaction` fill, label `--c-paper-ink` in `--f-ui` at `--t-caption` weight 500, `--r-sm`, `--sp-sm` top and bottom and `--sp-md` at the sides. |
| Feed | no | The Notes saved since the surface opened, newest first, `--sp-md` beneath the hint line, `--sp-sm` between items. Absent until the first Note is saved. |
| Feed item | no | `--surface-nested`, `--r-sm`, `1px` `--edge-hairline`, `--sp-sm` top and bottom and `--sp-md` at the sides. |
| Item timestamp | no | Left of the item's first row, in `--f-mono` at `--t-label`, `--tr-mono`, `--lh-ui`, `--text-meta`. |
| Item actions | no | `Edit` then `Discard`, right of the item's first row, `--f-ui` at `--t-caption`, `--text-secondary`, `--sp-md` apart, `--sp-sm` beneath the row. |
| Item text | no | The whole Note, every line, in `--f-ui` at weight 400, `--t-dense`, `--lh-ui`, `--text-primary`. |
| Anchor diamond | no | `Marks.md` geometry — a `9 × 9px` square rotated 45°, bounding box offset `-4px` on both axes from the item's top-left corner — filled `--signal-authored-context`. Present on Anchored only. |

The frame is opaque over its own footprint and draws nothing beyond it. There
is no second surface behind it, no wash, no tint, and no blur: `--surface-chrome`
sits directly on whatever the Grid is drawing, and the Grid keeps
drawing.

Two values in this table are decided here because no other component shares
them. The capture field's column is `56ch`, given under **Geometry**. The
caret's `1.05em` height is the field's own, because a caret shorter than the
line looks like a mark and a caret taller than it touches the line above.

`--k-slate-b` is named by `10-grammar/Surface-classes.md` and carries no row
in `00-foundations/Tokens.md`. It resolves to `--k-slate` at `0.46`, and
`00-foundations/Tokens.md` should carry that row: `0.46` is the lowest alpha at
which the border clears `3:1` against `--surface-grid`, and this border is
the only thing saying where the surface ends against a Grid that stays
live. The catalogue's `0.35` reaches `2.27:1` and migrates.

The surface has no tab bar, no field labels, no section headings, no footer
toolbar, no status bar, no destination control, and no control that is present
while it has nothing to act on.

## Geometry

- **Footprint** — none in cells. This surface is not addressed on the Grid and
  takes no cells, so `00-foundations/Principles.md` Law 8 does not reach it.
  Its width is executed as a procedure: set the capture field's column to
  `56ch` at `--t-lead`; add `--sp-md` at each side for the field's own padding;
  add `--sp-lg` at each side for the frame's padding; if the result exceeds the
  viewport width less `--sp-md` at each side, take that instead and let the
  column narrow with it. Its height is the height of its content, capped at the
  viewport height less `--sp-md` at the top and bottom. The same viewport
  yields the same box every time, because nothing in the procedure reads the
  camera, the Layer, or what is on the field.
- **Growth** — the capture field grows one line at a time as the draft grows,
  and it takes that room from the feed. When the surface reaches its height cap
  the feed's height gives way first and the feed scrolls; only when the feed is
  gone does the capture field scroll to hold the caret in view. A composed
  surface may scroll and a person must always see the line they are writing, so
  the field is the last thing that gives.
- **Measure** — `56ch` on the capture field. The catalogue's unconstrained
  rendering sets a `576px` column, which is 54 characters at the size that deck
  sets, and the shipped field measures 57; `56ch` sits between the two.
  `--measure-reading` does not bind here, because `34ch` is a reading column and
  this is a field a person writes a paragraph into, and the `65ch` of a surface
  given over to writing does not bind either, because
  `00-foundations/Typography.md` assigns that value to that surface alone.
- **Alignment** — centred on the viewport on both axes, and on no grid line.
  `00-foundations/Elevation-and-depth.md` licenses exactly this centring for a
  capture surface with no source, and the viewport inset of `--sp-md` on every
  side keeps the Grid visible on all four sides.

### Why this is fixed to the viewport, and why it is not a modal

A surface may be fixed to the viewport only where all four of these hold. Quick
Note is the only surface in Grove that meets them.

| Condition | How this surface meets it |
| --- | --- |
| Summoned by a declared key | `N`, from `docs/reference/Keybind map.md`. Nothing opens it by approach, selection, or camera movement. |
| Serves the whole Grid and names no source | A Note written here has no placement, no Layer, and no cells; there is nothing for the surface to sit beside. |
| Opaque over its own footprint and nothing beyond it | `--surface-chrome` at full opacity inside its border, and no drawing of any kind outside it. |
| Leaves everything outside its frame lit, legible, hit-testable, and reachable | The Grid behind is undimmed, unblurred, untinted, and answers the pointer while the surface is open. |

It is not a modal, and the difference is stated as four things it does not do.
It **dims nothing** — there is no scrim, wash, tint, blur, or desaturation
between it and the Grid, in any alpha, over any area. It **blocks
nothing** — a pointer press outside the frame acts on the Grid exactly as
it would with the surface closed, moves focus there, and leaves the surface
open with its draft intact. It **claims nothing** — it carries no `aria-modal`,
so assistive technology is never told the Grid is inert, because it is
not. And it **holds nothing hostage** — Escape peels one layer and hands focus
back by name.

`docs/reference/Keybind map.md` calls this surface modal. That word there names
key ownership — the keys pressed inside the surface belong to the surface and
reach no further — and it licenses no scrim, no dimming, and no assertion that
the rest of the Grid has stopped.

It is also visually distinct from every surface anchored to a source, and the
distinction is carried by two parts at once. A local editor takes an operation
role edge — `--c-edit-edge` where it writes, `--c-view-edge` where it only
looks — and sits beside the thing it serves; this surface takes `--k-slate-b`
and `--c-slate` on its identity line, and sits in the middle of the viewport.
An operation tint means *this belongs to something over there*; the Slate edge
means *this belongs to the Grid*. A person reads which one they are
looking at from the border alone, without reading a word.

## States

All nine.

| State | Appearance | Notes |
| --- | --- | --- |
| Rest | The frame, its border and shadow, the identity line, `Close`, the capture field on `--edge-quiet`, the hint line, and every saved Note in the feed complete. Nothing else. | The anchor diamond is present here on an item whose Note carries authored context, because Anchored is a property and not an interaction. |
| Approached | The feed item under the pointer takes `--edge-found` in place of `--edge-hairline`. | Approach adds a findable edge and changes no ink: `00-foundations/Principles.md` Law 1 caps chrome ink at `--ink-secondary` on a surface carrying `--ink-primary` content, so a hovered action may not brighten. |
| Focused | `--focus-ring` at `--focus-ring-offset`, drawn outside the focused part on `:focus-visible`. The capture field additionally takes `--signal-interaction` on its own border, and draws the caret. | Both are drawn; `10-grammar/States.md` fixes that the ring is never suppressed and is drawn on top of any other state's outline. |
| Selected | Not reachable. Nothing on this surface is selectable. | Choosing a saved Note moves it into the capture field rather than marking it, so there is no state between looking at a Note and editing it. |
| Engaged | The capture field holds a saved Note pulled back for editing: that item leaves the feed, the field carries its whole text, the action row appears, and the hint line changes to the two keys that leave the edit. Pointer-down on any control takes the pressed appearance over `--d-press`. | Nothing durable changes while the edit is open; the saved Note is unchanged until the change is committed, and leaving the edit restores it exactly. |
| Pending | The draft stays exactly as typed and stays editable, the caret keeps its position, and the committing action goes `--text-unavailable` on a `--edge-hairline` border. | Nothing is blanked and nothing spins; a person may keep typing while a save is in flight, because input is never held. |
| Refused | The inline confirm's refusal form appears above the capture field: full width of the field's region, `--surface-nested`, `--r-sm`, `1px` `--signal-refusal`, one sentence left, a retry and a dismissal right. The draft is untouched. | `30-components/Refusal.md` sends a refusal raised inside a Slate to this form; the 45° hatch belongs to Grid cells and is drawn nowhere on this surface. |
| Unavailable | The committing action while the capture field holds no text; `Edit` and `Discard` on every other saved item while the field holds an edit. `--text-unavailable` on a `--edge-hairline` border, in place. | There is one editing place on this surface and it is the capture field, so the other items keep their actions in position and say they cannot be reached now. |
| Anchored | The anchor diamond sits outside the feed item's top-left corner, and the item is otherwise untouched. | The diamond rather than the ribbon, because the item reserves only `--sp-sm` at its top edge and a ribbon's `17px` descent would land on the timestamp; `00-foundations/Marks.md`'s own test — does the form reserve margin at its top edge — answers diamond. |

Combination follows `10-grammar/States.md` without exception. Focused plus
Engaged keeps the ring outside the capture field's interaction border with a
visible gap.

## Behaviour

- **Pointer** — a click in the capture field places the caret and commits on
  pointer-up. A click on `Edit` moves that Note's whole text into the capture
  field and removes the item from the feed, committing on pointer-up. A click
  on `Discard` raises the inline confirm on that item's own region. A click on
  the committing action saves. A click on the quiet action leaves the edit and
  returns the Note to the feed unchanged. A click on `Close` dismisses the
  surface. A click anywhere outside the frame acts on the Grid and leaves
  the surface open.
- **Keyboard** — `N` opens the surface from the Grid. `Enter` inserts a
  line break, because a Note is multiline by nature and committing one is never
  an accident of typing. `Shift+Enter` saves the Note and returns an empty
  capture field with the caret in it. `Ctrl/Cmd+Enter` saves the Note, closes
  the surface, and arms placement with that Note. `Escape` peels one layer.
  `Tab` and `Shift+Tab` move focus within the surface. Every one of these is
  `docs/reference/Keybind map.md`, and none is redefined here. While the capture
  field holds an edit of a saved Note, `Ctrl/Cmd+Enter` commits the change to
  the same Memory and returns it to the feed, and `Shift+Enter` is unavailable:
  a Note that already exists cannot be placed twice by one chord, and there is
  no next Note to start from an edit.
- **Focus order** — `Close`, then the capture field, then the composer action
  row when it is present, then each feed item's `Edit` and `Discard` in the
  feed's order. Opening moves focus into the capture field, because that is
  what the surface exists for. Focus does not leave the surface while it is
  open; a pointer press outside it moves focus to the Grid, and the
  surface stays open.
- **Escape** — one layer per press. An open inline confirm chooses its quiet
  action. Otherwise, a capture field holding unsaved text raises
  `Discard unsaved changes?`. Otherwise the surface closes and focus returns to
  the Grid cursor at its last cell.
- **Commit and cancel** — a Note becomes a Memory on `Shift+Enter` or
  `Ctrl/Cmd+Enter`, and it is durable at that moment with no destination, no
  Layer, and no question asked. A change to a saved Note becomes durable on
  `Ctrl/Cmd+Enter` or on the committing action, on the same Memory, never as a
  second one. `Discard` on an item removes only that item's Memory and leaves
  every other item and the draft untouched. Closing the surface clears the feed
  and the empty capture field and nothing else; every Note saved here survives
  it.

Placement is a Grid gesture and belongs to `30-components/Placement-preview.md`:
this surface hands off by closing and arming it, and cancelling the placement
leaves the Note saved and unplaced. The wording and the action pair of every
confirm raised here belong to `10-grammar/Surface-classes.md`. Where a Memory
goes afterwards belongs to the Grid and to the surface that browses
Memories.

## Motion

| Transition | Duration | Curve | Reduced motion |
| --- | --- | --- | --- |
| The surface arriving | `--d-fade` | `--ease` | Present at the identical threshold, with no fade. |
| The surface leaving | `--d-fade` | `--ease` | Absent at the identical threshold. |
| The composer action row arriving when an edit opens and leaving when it closes | `--d-fade` | `--ease` | Present or absent at the identical threshold. |
| The inline confirm and the refusal form arriving and leaving | `--d-fade` | `--ease` | Present or absent at the identical threshold. |
| Pointer-down on `Close`, an item's action, or a composer action | `--d-press` | `--ease` | The pressed appearance applied on press and removed on release, with no scaling. |

`00-foundations/Motion.md` names the curves `--ease` and `--overshoot`
and `00-foundations/Tokens.md` projects the same two values as `--ease` and
`--overshoot`; this specification uses Motion's names, because Motion owns which
curve a transition takes. `--overshoot` appears nowhere on this surface,
because nothing here arrives in cells.

A saved Note appearing in the feed, the capture field emptying, and a discarded
item leaving are frame changes with no transition: nothing arrived in or left
the Grid, and the settled frame already says what happened. The focus
ring, the capture field's focused border, the caret, the refusal form's border,
and the anchor diamond never animate. Nothing loops. Nothing idles. Nothing
pulses or blinks.

## Distance

Thresholds are on projected cell size and belong to
`10-grammar/Representation-tiers.md`. This surface is fixed to the viewport,
takes no projected cell size, and reads identically at every camera scale.

| Tier | Sheds | Keeps |
| --- | --- | --- |
| Working | Nothing. | Every part, at the values in **Anatomy**. |
| Stepped | Nothing. | Every part, at the values in **Anatomy**. |
| Stand-in | Nothing. | Every part, at the values in **Anatomy**. |

What changes at distance is the Grid behind the surface, which sheds and
promotes exactly as it would with the surface closed, and stays fully lit and
fully live through every tier. That is the visible proof this surface is not a
modal: a person can watch the field simplify behind it while they type.

## Accessibility

- **Role and name** — the surface is `role="dialog"` with its accessible name
  taken from the identity line, `Quick Note`, and **no** `aria-modal`: the role
  says the surface is dismissible and holds its own focus order, and the absent
  `aria-modal` is what says the Grid behind it is still live. The capture
  field is a multi-line text control named `Note`. The feed is a list named
  `Notes`. Each item's accessible name is the text a person wrote, verbatim; the
  word `Note` as an item's accessible name is a defect, because it replaces the
  one thing that distinguishes that Note from every other. The refusal form is
  `role="status"` with `aria-live="polite"` and announces its sentence once.
- **Contrast** — the identity line reaches 9.85 on `--surface-chrome`. Capture
  field text and item text at `--text-primary` reach 9.85 on `--surface-nested`;
  item actions at `--text-secondary` reach 6.19; the timestamp at `--text-meta`
  reaches 4.61. The hint line at `--text-meta` reaches 4.71 on
  `--surface-chrome`, and `Close` at `--text-secondary` reaches 6.35. The quiet
  action reaches 10.36 on `--surface-chrome`; the committing action's
  `--c-paper-ink` label reaches 8.47 on its `--signal-interaction` fill. The
  caret and the capture field's focused border reach at least 8.38 against both
  surfaces they touch. The refusal form's border reaches 4.96 on
  `--surface-nested`, and the anchor diamond 6.36 on `--surface-chrome`. The
  frame border at `--k-slate` `0.46` reaches 3.06 against
  `--surface-grid`, which is the floor for the edge that states this
  surface's extent. Three edges carry no meaning and are exempt: the feed
  item's `--edge-hairline`, which separates cards inside one surface;
  `Close`'s `--edge-found`, because the word identifies the control and the
  border only contains it; and the capture field's resting `--edge-quiet`,
  because the field's extent is carried by its tone step and its position
  directly beneath the header.
- **Without colour** — Approached: an edge that was quieter. Focused: a ring
  outside the edge at a distinct offset, and a caret that was absent. Selected:
  not reachable. Engaged: the item gone from the feed, the text in the capture
  field, and an action row that was absent. Pending: the committing action's
  retained position and hairline border. Refused: a bordered row that was not
  there, holding one plain sentence. Unavailable: a retained position and a
  hairline border. Anchored: the diamond's silhouette. The Slate edge and the
  identity line's hue are the surface's class mark and carry no state, so
  greyscale loses nothing Grove said.
- **Forced colours** — the frame fill and border, the identity line, the
  capture field's fill and both of its border states, the caret, every action
  label and the committing action's fill, the item fills and borders, and the
  diamond's fill are redeclared in system colours. What survives without
  redeclaration is the focus ring's offset, the diamond's silhouette, the
  refusal form's position above the field, the retained position of every
  unavailable action, and the words. Nothing on this surface is drawn as canvas
  paint, so nothing disappears silently.
- **Text scaling** — every part of this surface is interface text and grows
  with it. The frame grows to its maximum width and then wraps: the hint line
  wraps to as many lines as it needs and no hint ever truncates; the item's
  first row puts the timestamp on one line and the actions on the next; the
  action row puts its two actions on one line and wraps to two. The capture
  field keeps its `56ch` measure and takes more height. Nothing truncates and
  nothing ellipsises at any scale.
- **Reduced motion** — no change from the Motion table.

## Copy

| String | Where | Why it passes |
| --- | --- | --- |
| `Quick Note` | The identity line, rendered uppercase by `--tr-label` rather than by writing capitals. | Names the surface in ordinary English plus one product noun; true after any rebuild. |
| `Close` | The header control. | A verb for an action, sentence case, no terminal punctuation. |
| `Enter` · `Line break` | The hint line. | A key and what it does, in ordinary English. |
| `Shift+Enter` · `Save and start the next Note` | The hint line. | A key and what it does; `Note` is a product noun. |
| `Ctrl+Enter` · `Save and place` | The hint line; the modifier renders as the platform's own name. | A key and two verbs for what it does. |
| `Esc` · `Stop editing` | The hint line, while the capture field holds an edit. | A key and what it does. |
| `Edit` | An item's first action. | A verb for an action, sentence case, no terminal punctuation. |
| `Discard` | An item's second action. | A verb that names the outcome, not the mechanism. |
| `Stop editing` | The quiet action in the composer action row. | Names what it does and what continues; `Cancel` is refused because it names the question rather than the outcome. |
| `Save` | The committing action in the composer action row. | A verb for the one action that commits this surface. |
| `Discard unsaved changes?` · `Keep editing` · `Discard` | The inline confirm raised by leaving a capture field that holds unsaved text. | Verbatim from `10-grammar/Surface-classes.md`; the question names what is lost and the committing action repeats the verb. |
| `Discard this Note?` · `Keep it` · `Discard` | The inline confirm raised by `Discard` on an item. | Verbatim from `10-grammar/Surface-classes.md`. |
| `This Note is not saved.` · `Try again` · `Keep editing` | The refusal form, when a save could not be made durable. | States the condition in one clause in the present tense, blames nobody, takes a full stop as a sentence in a strip, and pairs a retry with a dismissal rather than a destructive pair. |

Everything else is **None**. A saved Note's own text is authored content and
never copy: Grove never shortens it, summarises it, prefixes it, or adds a word
of its own to it. The timestamp is a value, not copy — the time the Note was
saved, in the platform's own short form. The capture field carries no
placeholder, because a placeholder leaves the moment a person types while the
hint line stays, and the hints are already the next step an empty surface
offers. An empty feed carries no string, because the feed is absent until the
first Note is saved and an empty surface with a capture field and its hints is
already a designed state with a next step.

## Refusals

- **A scrim, a dim, a tint, or a backdrop blur** — the Grid behind this
  surface is live work, and dimming it claims an interruption Grove does not
  make.
- **`aria-modal`, or any assertion that the Grid is inert** — the
  Grid is reachable by pointer the whole time, so the claim would be
  false.
- **A translucent fill** — translucency makes the Grid an ingredient of
  this surface's own colour, and a capture field must read the same over a
  crowded Layer as over an empty one.
- **An operation role edge** — an Edit or View tint means the surface belongs to
  something on the field, and this one belongs to nothing.
- **A one-line preview row** — a row that hides its own text makes reading a
  second step, and the whole point of the feed is that a person never opens a
  Note to see what it says.
- **A count, a category chip, a tag, or a folder on an item** — a figure or a
  label riding on a saved thought competes with the words it is made of.
- **A destination control of any kind** — capture never asks where a thought
  belongs; a saved Note is durable with no destination and placement is a
  later, explicit act.
- **Two filled actions** — two primary actions ask a person to rank them, and
  neither is primary after that.
- **The shape of a settings sheet** — no labelled field rows, no section
  headings, no checkbox, toggle, radio, stepper, or select, and no `OK`, `Yes`,
  `No`, or `Cancel`. This surface has one field, and a surface that looks like a
  form teaches a person to fill it in rather than write in it.
- **A control that is present while it has nothing to act on** — the action row
  belongs to an edit, and drawing it over an empty field turns capture into
  ceremony.
- **A resize handle on the capture field** — the field already grows with the
  draft, and a handle makes the size of chrome a thing a person has to manage.
- **A second surface opened from this one** — nothing here opens a viewer, an
  editor, or a Slate; the only handoff is placement, and it closes this surface
  first.
- **A confirmation that a save happened** — the Note is in the feed, which is
  the result in the place that produced it; a banner that leaves would put the
  meaning in something that goes away.
- **A spinner, a progress bar, or a skeleton while a save is in flight** — the
  draft is the fullest form already available and it stays on screen.


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

- Catalogue deck: `docs/design_catalogue/src/information-plane/03-quick-note.html` — the contract for this component.
- Catalogue deck: `docs/design_catalogue/src/hud/01-slate-anatomy.html` — the Slate frame, its slate-tinted hairline, and its header controls.
- Catalogue deck: `docs/design_catalogue/src/hud/02-memory-slate.html` — the Slate edge at `--k-slate` `0.35` and the card inside a composed surface at `--t-dense`.
- Catalogue deck: `docs/design_catalogue/src/information-plane/01-local-editors.html` — the source-anchored frame this surface must not resemble.
- Reference: `docs/reference/Keybind map.md` — `N`, `Enter`, `Shift+Enter`, `Ctrl/Cmd+Enter`, `Escape`, `Tab`.
