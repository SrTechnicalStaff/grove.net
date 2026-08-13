---
type: design-system-component
status: active
date: 2026-08-10
component: Inline confirm
plane: any
surface_class: inline-confirm
tags: [grove, design-system, component]
---

# Inline confirm

The one question Grove asks before something a person wrote is gone for good,
put in a row inside the surface that raised it, beside the work it is about.

This is why Grove has no dialog box. Every other product would open a window
here; Grove attaches a row instead, and the Grid behind it stays lit,
readable, and clickable the whole time.

The component belongs to no plane of its own. It takes the plane of the surface
that raised it, and it is drawn identically on all three, because the question
is about the work rather than about where the work is being done.

## Anatomy

| Part | Required | Value |
| --- | --- | --- |
| Frame | yes | Full width of the region it attaches to, `--surface-nested`, `--r-sm`, `1px` `--edge-hairline`, `--sp-sm` top and bottom, `--sp-md` at the sides, no shadow. |
| Question | yes | Left in the frame. `--f-ui` at weight 400, `--t-dense`, `--lh-ui`, `--text-primary`. One sentence ending in a question mark. |
| Gap between the question and the actions | yes | `--sp-md`. |
| Quiet action | yes | Text only — no fill, no border. `--f-ui` at weight 400, `--t-caption`, `--text-primary`, padding `--sp-xs` top and bottom and `--sp-sm` at the sides. First of the two. |
| Committing action, non-destructive | yes, when the commit is not destructive | `--signal-interaction` fill, label `--c-paper-ink`, `--f-ui` at weight 400, `--t-caption`, `--r-sm`, padding `--sp-xs` top and bottom and `--sp-sm` at the sides. Second of the two. |
| Committing action, destructive | yes, when the commit destroys authored work | The same box with a `--signal-refusal` fill. The only place in Grove where refusal appears as a fill, and it leaves with the question. |
| Gap between the two actions | yes | `--sp-sm`. |
| Gap between the frame and the part it hangs from | yes | `--sp-md`. |
| Refusal border | no | `1px` `--signal-refusal` at full strength, replacing `--edge-hairline` on the same frame. Present on Refused only. |
| Shadow | no — refused | None. The confirm sits inside a surface that already separated itself, and a second shadow would claim a second layer of depth. |
| Icon, glyph, or alert mark | no — refused | None. The question is the words. |

The frame takes the third dark step because it is a surface inside a surface,
and the tone step plus the hairline plus its attachment to the region that
asked are together what stop the question reading as part of the content it is
about.

The hairline is the correct edge here even though
`00-foundations/Accessibility.md` withholds it from a meaning-bearing edge: the
confirm never floats, so its border separates a group inside one surface rather
than containing a surface against the Grid, which is the job
`00-foundations/Shape.md` assigns `--edge-hairline` exactly.

The committing action's fill is a control fill, not a surface fill.
`00-foundations/Color.md` refuses a signal as a surface fill and
`10-grammar/Signal-roles.md` draws the one primary action and the one
destructive control as solid fills; both hold, and together they are why the
frame itself is never tinted while the action inside it is.

The action label sits one size step below the question. All three catalogue
renderings set it that way — `12`/`11`, `12.5`/`11.5`, and `12.5`/`12` — and
`00-foundations/Typography.md` gives `--t-dense` to dense interface text and
`--t-caption` to a quiet control, so the two tokens land where the decks already
put the two sizes.

## Geometry

- **Footprint** — none in cells. The confirm is chrome inside a surface and is
  measured in that surface's content width, never in cells and never against
  the viewport.
- **Attachment** — executed as a procedure: find the control that raised the
  question; if that control sits in the surface's header, attach the frame
  directly beneath the header, `--sp-md` below its bottom edge; if it sits in
  the surface's footer, attach the frame directly above the footer, `--sp-md`
  above its top edge; if a key raised the question and no control was pressed,
  attach to whichever of the two regions carries that surface's own exit. The
  frame never attaches between two pieces of content. This is why the local
  editor deck draws the confirm at the foot of an editor whose actions sit on a
  footer row and the writing deck draws it under a header whose actions sit in
  the header: both are correct under one rule.
- **Growth** — the frame grows taller as the question or the actions need more
  room, and the surface's content moves down to make space. It never scrolls
  inside itself, never truncates the question, and never shortens a label.
- **Narrow layout** — when the question and both actions cannot share one line,
  the question takes the first line and the two actions right-align on the
  second, inside the same frame, in the same order.
- **Measure** — none. The question is one clause, so it sets no reading column
  and `--measure-reading` does not bind; a single clause never becomes a column.
- **Alignment** — the frame's left and right edges land on the content edges of
  the region it attaches to, so it is exactly as wide as the thing it hangs
  from. The actions align to the frame's right content edge; the question
  aligns to its left.

## States

All nine.

| State | Appearance | Notes |
| --- | --- | --- |
| Rest | The settled row: the question left, the quiet action and then the committing action right, on `--surface-nested` inside a hairline frame. | The component has no appearance before it is raised; it exists only while a question is open, and it is drawn nowhere at rest on the surface. |
| Approached | No change from Rest. | Both actions are already fully drawn, and `10-grammar/Signal-roles.md` gives the hover highlight to a menu row alone, so a second hover grammar would teach two answers to one gesture. |
| Focused | The focused action takes `--focus-ring` at `--focus-ring-offset`, drawn outside its own box. | Focus arrives on the quiet action when the confirm opens; the frame itself never takes a ring, because a frame that cannot be pressed is not a target. |
| Selected | Not reachable. | Selection names placed content a person is working on; a question's two actions are commands, and a command is chosen rather than selected. |
| Engaged | Pointer-down on either action takes the pressed appearance over `--d-press` and releases it on release. | Nothing durable changes while the button is down; releasing off the action commits nothing and restores Rest. |
| Pending | Not reachable. | The confirm closes on the frame either action is pressed, and the surface that asked holds any pending form, because a question left open after it was answered asks twice. |
| Refused | The same frame with its border in `--signal-refusal` at full strength, one sentence stating the condition in place of the question, and a quiet action beside a retry. Never a destructive pair. | The commit that failed is what is being reported, so the row that raised it is the row that answers; no second surface, no strip, no banner. |
| Unavailable | Not reachable. | A confirm opens only on an attempt that can still be completed and closes on the answer, so neither action is ever present-but-unusable; a question whose commit cannot run was raised in error. |
| Anchored | Not reachable. | Anchored is authored context carried by Content, and a question is neither. |

Combination follows `10-grammar/States.md` without exception. Focused plus
Engaged is both: the ring stays outside the pressed box.

## Behaviour

- **Pointer** — a click on either action commits on pointer-up over that action.
  Pointer-down anywhere on an action takes the pressed appearance; dragging off
  before release commits nothing. A click outside the confirm does nothing at
  all — it neither dismisses nor answers, because dismissing a question by
  clicking elsewhere answers it without saying which answer was given. A click
  on the Grid behind the surface reaches the Grid normally.
- **Keyboard** — `Escape` chooses the quiet action, closes the confirm, and
  leaves the surface open; this is the inline-confirm row of the escape order in
  `10-grammar/Surface-classes.md` and it is not restated here. `Enter` fires the
  action that holds focus and nothing else, so `Enter` reaches a destructive
  commit only when a person has deliberately moved focus onto it. `Tab` and
  `Shift+Tab` move between the two actions and cycle within them while the
  confirm is open, because `00-foundations/Accessibility.md` requires focus to
  stay in the open surface. No other key is answered, and the keys that leave
  the surface are not swallowed.
- **Focus order** — two focusable parts, in this order: the quiet action, then
  the committing action. The question is not focusable, because it names the
  choice rather than offering one.
- **Escape** — chooses the quiet action and returns focus to the control that
  raised the confirm. Escape never closes the surface while a confirm is open,
  and it never reaches the Grid.
- **Commit and cancel** — the quiet action makes nothing durable and restores
  the surface exactly as it was, caret and scroll position included. The
  committing action performs the named change once and closes the confirm; where
  that change also closes the surface, focus follows that surface's own return
  path. There is no third outcome.

Which attempt raises a confirm belongs to the wireframe that owns the attempt.
This specification owns only what the confirm looks like and how it answers.

## Motion

| Transition | Duration | Curve | Reduced motion |
| --- | --- | --- | --- |
| The confirm arriving | `--d-fade` | `--ease` | Present at the identical threshold, with no fade. |
| The confirm leaving on either action or on Escape | `--d-fade` | `--ease` | Gone at the identical threshold, with no fade. |
| Pointer-down acknowledgement on either action | `--d-press` | `--ease` | The pressed appearance applied on press and removed on release, with no scaling. |
| The frame turning to the refusal border | None — immediate | None | Identical; there is nothing to reduce. |

The frame takes its full height on the frame it opens and only its opacity
transitions, because `00-foundations/Motion.md` forbids animating a layout
property; the content below it moves in one step.

`00-foundations/Motion.md` names the curve `--ease` and
`00-foundations/Tokens.md` projects the same value as `--ease`; Motion's name is
used above, because Motion owns which curve a transition takes.

The question, both action labels, the committing action's fill, the refusal
border, and the focus ring never animate. Nothing loops. Nothing idles. Nothing
pulses or blinks.

## Distance

| Tier | Sheds | Keeps |
| --- | --- | --- |
| Working | Nothing. | The frame, the question, both actions. |
| Stepped | Nothing. | All of it, unchanged and at the same size. |
| Stand-in | Nothing. | All of it, unchanged and at the same size. |

The confirm sheds nothing at any tier and changes no dimension with the camera,
because it is chrome drawn in screen space inside a surface that is not
addressed in cells. `10-grammar/Representation-tiers.md` fixes tiers for
placements on the field; a question about a placement is not a placement.

This component has no stand-in and no kind-coded form. Presence, position, and
extent belong to the content the question is about and are never touched by it.

## Accessibility

- **Role and name** — the frame is `role="group"` named by its question element.
  The question element is `role="status"` with `aria-live="polite"`, so the
  question is announced once when the confirm opens, which is the one case
  `00-foundations/Accessibility.md` permits announcing. Each action is a
  `button` whose accessible name is its own visible verb. `role="dialog"` and
  `aria-modal` are defects here: both assert an interruption Grove does not
  make, and `aria-modal` hides the live Grid from assistive technology
  while it is still fully usable by everyone else.
- **Contrast** — the question is `--text-primary` on `--surface-nested` at
  `9.85:1`. The quiet action's label is the same at `9.85:1`. The
  non-destructive committing action's label is `--c-paper-ink` on
  `--signal-interaction` at `8.58:1`, and its fill reaches `8.37:1` against the
  frame, which is what defines the control's boundary. The destructive
  committing action's label is `--c-paper-ink` on `--signal-refusal` at
  `5.08:1`, and its fill reaches `4.96:1` against the frame. The refusal border
  reaches `4.96:1`. The focus ring is `--signal-interaction` at `--ink-full`,
  `8.37:1` against the frame. The frame's hairline carries no meaning of its own
  and is exempt, because the tone step and the attachment carry containment.
- **Without colour** — Rest: the row's presence where nothing was before, plus
  the words. Focused: a ring outside the box at a distinct offset. Engaged: the
  pressed appearance. Refused: the words state the condition and the border
  changes on the frame that raised the commit. Quiet against committing: one is
  bare text and one is a filled rectangle, so the presence of the fill and not
  its hue tells them apart. Destructive against non-destructive: the label
  repeats the verb of the loss, which is the words carrier — the fill's hue is
  never the only thing saying that a commit is irreversible. There is no 45°
  hatching on the refusal form, because the hatch marks blocked cells and this
  row has none; hue, position, and words are its three carriers.
- **Forced colours** — the frame's fill and border, the question, both action
  labels, the committing action's fill, the refusal border, and the focus ring
  are redeclared in system colours. What survives without redeclaration is the
  row's position inside the surface that asked, the filled-against-bare
  difference between the two actions, the ring's offset, and every word.
- **Text scaling** — the question and both labels grow with interface text and
  the frame grows with them; at the width where they cannot share a line the
  narrow layout applies. Nothing truncates, no label wraps, and no scrollbar
  appears inside the frame at any scale.
- **Reduced motion** — no change from the Motion table.

## Copy

This component owns no fixed string. Every string it shows is written by the
surface that raises the question, and it is built by four rules:

1. The question names the loss in the words a person would use, in one
   sentence, ending in a question mark.
2. The committing action repeats the verb of the loss, so the label and the
   question cannot disagree.
3. The quiet action names what continues, never what is being avoided.
4. A destructive question names the thing it will destroy — the Note, the
   Layer, the Anchor — never `this item`, never `the selection`, and never a
   count standing in for the thing.

`10-grammar/Surface-classes.md` already fixes the wording for closing an editor
with unsaved text, removing a placed Note, and discarding a saved Note. Three
further cases, worked:

| Situation | Question | Quiet | Committing |
| --- | --- | --- | --- |
| Removing a Layer that holds placed content | `Remove this Layer and everything placed on it?` | `Keep it` | `Remove` |
| Deleting an Anchor that Memories point at | `Delete this Anchor and everything written on it?` | `Keep it` | `Delete` |
| A save that did not land — the refusal form | `This Document is not saved.` | `Keep editing` | `Save again` |

Each passes `10-grammar/Copy.md`: `Layer`, `Anchor`, `Memory`, `Note`, and
`Document` are the product's own nouns, the rest is ordinary English, the labels
are verbs in sentence case with no terminal punctuation, and each would still be
true after a rebuild on any architecture. The refusal sentence takes a full stop
because it is a full sentence in a strip, states the condition rather than the
failure, and blames neither the person nor the product.

Strings the corpus currently shows, and what replaces them:

| Refused string | Why it fails | Correction |
| --- | --- | --- |
| `Drop changes` | Does not repeat the verb of the question, so the label and the question name two different acts. | `Discard` |
| `Stay here` | Names a place rather than what continues. | `Keep editing` |
| `Couldn't save your changes.` | Possessive about a person's work, and it names the failure rather than the condition. | `This Document is not saved.` |
| `Your writing is still here.` | Possessive about a person's work, and it reassures rather than states. | Nothing; the draft is visible and unharmed. |
| `Cancel` | Names the question rather than an outcome, and is unanswerable on a question raised by cancelling. | `Keep editing` |

`OK`, `Yes`, and `No` are refused for the same reason as `Cancel`: each answers
a question a person has to re-read the sentence to reconstruct.

## Refusals

- **A floating surface of any kind** — a window, a dialog, an alert, a sheet, or
  a popover detaches the question from the work it is about and then asks for a
  click to give the work back.
- **A scrim, dim, blur, or desaturation of anything outside the frame** — the
  Grid stays lit and live, because dimming claims an interruption Grove
  does not make.
- **Blocking the rest of the application** — no `inert`, no `aria-modal`, no
  invisible layer swallowing pointer events, and no focus trap reaching past the
  confirm's own two actions; a person is never doing only one thing.
- **A shadow** — the confirm is inside a surface that already separated itself,
  and a second shadow claims a second layer of depth Grove does not have.
- **Tinting the frame with the refusal hue** — colouring the whole row makes the
  safe path look dangerous too, and turns a transient state into a place.
- **A third action** — two outcomes is the whole set; a third choice means the
  question is wrong and must be split before it is asked.
- **Two filled actions** — a second fill asks the person to rank them, and then
  neither is primary.
- **`Enter` firing a destructive commit that does not hold focus** — an
  irreversible act reached by a key a person presses to move on is not a
  decision they made.
- **A confirm on a reversible action** — a question about work that can be
  undone teaches a person to stop reading questions.
- **A confirm on hover, on opening a surface, or to report success** — a
  question is raised by an attempt and by nothing else.
- **Two confirms open on one surface** — a second question stacked on the first
  makes neither answerable.
- **A checkbox that suppresses the question, a countdown, or a typed-name gate**
  — an option that turns off a safeguard is a defect in the design that needed
  the option.
- **Dimming, fading, or blurring the content the question is about** — the draft
  must stay readable while a person decides whether to lose it, and a fade at
  surface scale is a scrim by another name.
- **A hover fill on either action** — `10-grammar/Signal-roles.md` gives the
  hover highlight to menu rows alone, and both actions are already fully drawn.
- **An icon, alert glyph, or coloured bar down one edge** — the question is the
  words, and a mark beside them says nothing they do not.


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

- Catalogue deck: `docs/design_catalogue/src/information-plane/01-local-editors.html`
- Catalogue deck: `docs/design_catalogue/src/hud/04-writing-slate.html`
- Catalogue deck: `docs/design_catalogue/src/00-design-language.html`
- Grammar: `docs/design-system/10-grammar/Surface-classes.md`
- Component: `docs/design-system/30-components/Refusal.md`
- Keys: `docs/reference/Keybind map.md`, Text editor row — Escape cancels clean
