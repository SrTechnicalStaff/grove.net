---
type: design-system-grammar
status: active
date: 2026-08-09
tags: [grove, design-system, surface-classes]
---

# Surface classes

Everything in Grove that is drawn above the Grid belongs to one of four
classes: **Slate**, **local editor**, **menu or flyout**, and **inline
confirm**. Each class has one anatomy, one way in, and one way out.

Grove has no generic modal, no scrim, and no dialog box. The inline confirm is
the replacement for all three.

**The closure rule.** The four classes are the whole set. An overlay that does
not fit one is a defect in that overlay, not a new pattern — it does not ship,
and no fifth class is created to receive it.

## The four classes

| | Slate | Local editor | Menu / flyout | Inline confirm |
| --- | --- | --- | --- | --- |
| Plane | HUD | Information | Any — neutral chrome | The plane of the surface that raised it |
| Fill | `--surface-chrome` | `--surface-chrome` | `--surface-chrome` | `--surface-nested` |
| Border | 1px `--k-slate-b` | 1px in the operation role edge | 1px `--edge-quiet` | 1px `--edge-hairline` |
| Radius | `--r-sm` | `--r-sm` | `--r-sm` | `--r-sm` |
| Padding | `--sp-lg` | `--sp-md` | `--sp-xs` top and bottom, none at the sides | `--sp-sm` top and bottom, `--sp-md` at the sides |
| Shadow | None | `--shadow-local` | `--shadow-local` | None |
| Anchored to | The viewport, composed by the Slate host | Its source, beside it | The pointer or the control that opened it | The region of the surface that asked |

Every fill is opaque. Translucency, backdrop blur, and inset highlight lines
are refused in all four classes, because a surface that lets the Grid
bleed through stops being readable at the moment it matters most.

An inline confirm carries no shadow: it is inside a surface that already
separated itself, and a second shadow would claim a second layer of depth.

## Shared rules

1. **Opening is side-effect free.** Opening any of the four saves nothing,
   selects nothing, moves no camera, and commits no change; the first durable
   change is the action a person chooses.
2. **Escape dismisses the topmost surface, and only that one.** Escape never
   reaches the Grid while any class is open.
3. **Radius is always `--r-sm`.** There is no per-class radius and no
   asymmetric corner.
4. **Borders contain and locate; shadows only separate.** A border is 1px, a
   shadow is one of the two depth tokens, and a glow is neither.
5. **Nothing floats centre-screen detached from what it serves.** Every
   surface is composed by the Slate host, anchored beside its source, anchored
   at its invoking point, or inside the surface that asked.
6. **No scrim, ever.** The Grid behind stays fully lit, legible, and
   live, because dimming claims an interruption Grove does not make.
7. **Focus is always returned by name.** Each class below states where; a
   surface that closes and leaves focus nowhere is a defect.
8. **One primary action per surface.** Every other exit is quiet text.
9. **Internal rhythm uses the space scale.** A class never introduces an
   off-scale gap to make a layout fit.

## Slate

A Slate is a viewport-fixed working surface with no single source: it holds a
body of work — Memories, a Gallery, a Document being written, captured Notes —
rather than serving one piece of content.

### Anatomy

| Part | Rule |
| --- | --- |
| Frame | `--surface-chrome`, opaque, `--r-none`, filling the viewport edge to edge, `--sp-lg` padding on every side, no shadow. A border is drawn only on an edge shared with another pane. |
| Border | `--k-slate-b` for a single pane; `--signal-interaction` for the active pane and `--edge-hairline` for the inactive pane when two are open. |
| Header | The surface's own product name in `--f-display` at `--t-title-small`, `--tr-label`, uppercase, in `--c-slate`, with `--sp-lg` beneath it. |
| Header controls | Quiet bordered controls at the right of the header — the host's commands only. |
| Body | Content in neutral ink. The Slate hue appears in the header and nowhere else. |

The Slate role hue is the class's identity mark, so every Slate header carries
it. The Quick Note capture deck sets its header in `--text-primary`; that deck
is corrected, because a Slate that drops the hue loses the one signal that
says which class it is.

A composed Slate carries no shadow. It fills the viewport edge to edge, so
there is nothing beside it to separate from and nothing for a shadow to fall
on. The Slate anatomy deck renders its plates flat, and that is correct.

### May contain

One identity, the host's commands, and content. It may not contain a second
title, a tab bar, a footer toolbar, a status bar, or a nested Slate.

| | |
| --- | --- |
| Opens | An explicit action — a declared key or a chosen row. The host composes it Full, Left, or Right; two panes is the maximum. |
| Dismisses | Close in the header, or Escape. A menu open inside it dismisses first. |
| Focus returns to | The surface that invoked it. Closing one pane leaves the peer untouched; closing the last returns to the invoking surface, or to the Grid cursor at its last cell when that surface is gone — a decision, because the Grid is the only surface always present. |

A Slate never floats free, never drags, never resizes the Grid viewport, and
never dims what is behind it.

**A fixed capture surface is a Slate.** It is viewport-fixed and has no source,
which is the definition of the class; it differs from a local editor by having
no role-tinted border, and that difference is the visual separation the
contract root requires between capture and source-anchored chrome.

## Local editor

A local editor serves exactly one identified piece of content and opens beside
it, so the source stays visible the whole time.

### Anatomy

| Part | Rule |
| --- | --- |
| Frame | `--surface-chrome`, `--r-sm`, `--sp-md` padding, `--shadow-local`. |
| Border | 1px in the operation role edge: `--c-edit-edge` for a surface that writes, `--c-view-edge` for a surface that only looks. The border is the frame's only role signal. |
| Label | One quiet line naming the thing — `Note`, a file name — in `--f-mono` at `--t-label`, `--tr-wide`, `--ink-tertiary`. |
| Body | The draft or the viewing stage, in `--f-ui`, brighter than any chrome around it. |
| Footer | Actions on one row, with their key hints beside them in `--f-mono` at `--t-micro`, `--ink-tertiary`. |

The role border never changes to signal focus; keyboard focus is the ring
owned by `States.md`, drawn outside the frame.

A reading of gathered content is a viewer and takes `--c-view-edge`. The
annotation decks draw those frames with an interaction-tinted border; they are
corrected, because `--signal-interaction` means *this is what you are working
on* and a permanent interaction border makes focus and selection unreadable.

### May contain

One draft field or one viewing stage, one control strip, one inline confirm,
and one quiet escalation route. It may not contain a formatting toolbar, a
display-type title, a second nested editor, or a destination picker.

| | |
| --- | --- |
| Opens | Choosing an action on the content it serves. It opens beside the source, never over it, never centred. |
| Dismisses | Escape or the quiet action. On a clean draft it closes silently; on a dirty draft Escape raises the inline confirm instead of closing. |
| Focus returns to | The source it served — the placement, the frame, the reading — with its selection and scroll position exactly as they were. |

One local editor may open above another when a viewer is opened from a reading;
each Escape closes one of them. A third is a defect.

## Menu and flyout

A menu is a short list of actions for one target. It is neutral chrome: it
carries no role hue, because the target is what has a role, not the list.

### Anatomy

| Part | Rule |
| --- | --- |
| Frame | `--surface-chrome`, `--r-sm`, 1px `--edge-quiet`, `--shadow-local`, `--sp-xs` top and bottom, no horizontal padding so a row's highlight reaches the frame edge. |
| Row | Label left in `--f-ui` at `--t-dense`; one unmodified key right in `--f-mono` at `--ink-tertiary`. `--sp-sm` top and bottom, `--sp-md` at the sides. |
| Highlight | The row under the pointer or the keyboard fills in `--signal-interaction`; pointer and keyboard light the identical row. |
| Group separator | 1px `--edge-hairline`, `--sp-xs` above and below. Grouping is the only hierarchy a menu has. |
| Destructive row | Label in `--signal-refusal`, alone in the last group, never adjacent to the default row. |
| Flyout | Same cloth. Destinations are numbered `1`–`n`, the digit left in `--f-mono`; the digit is the quick key, so the row carries no second one. |

A menu has no title: the thing that was clicked is the title. It has no icons,
no checkbox column, and no row that wraps or truncates.

### Rows that are absent and rows that are grey

An operation the target does not support is **absent** — it was never one of
that target's actions. An operation the target supports but cannot perform at
this moment is **present and unavailable**, drawn in `--text-unavailable` in
its usual position. This resolves the menus deck's "omitted, never grayed"
against the state model's rule that an action must not vanish: the deck is
describing list construction, the state model is describing a listed action.

A target with no actions opens nothing. A menu of one filler row never opens.

| | |
| --- | --- |
| Opens | A context gesture on a target, or a control that declares a menu. It opens at the invoking point and flips to stay wholly on screen. |
| Dismisses | Escape, a click outside, or choosing a row. `←` closes a flyout only and returns to its held parent row. |
| Focus returns to | The target the menu was opened on, or the control that opened it. |

One flyout level, ever: a list too deep for one level is navigation, and
navigation is not a menu's job. A menu never scrolls — a list that cannot fit
on screen is the wrong list for that target.

## Inline confirm

The inline confirm is Grove's replacement for the dialog box. It is a row
inside the surface that raised the question, attached to the region that
raised it: under the header whose control was pressed, or above the footer
whose action was pressed.

### Anatomy

| Part | Rule |
| --- | --- |
| Frame | Full width of its region, `--surface-nested`, `--r-sm`, 1px `--edge-hairline`, `--sp-sm` top and bottom, `--sp-md` at the sides, no shadow. |
| Question | Left, in `--f-ui` at `--t-dense`, `--text-primary`. One sentence, ending in a question mark. |
| Actions | Right, in this order: the quiet action, then the committing action. |
| Narrow layout | When the question and both actions cannot sit on one line without wrapping, the question takes the first line and the actions right-align on the second, inside the same frame. |
| Refusal form | The same row with its border in `--signal-refusal`, stating what happened; its actions are a retry and a dismissal, never a destructive pair. |

The frame is bordered and filled as a surface inside a surface. The local
editor deck draws the question with a bare hairline rule instead; that deck is
corrected, because the border is what stops the question reading as part of
the content it is about.

Content behind the confirm is never dimmed. The local editor deck fades its
draft to make room; that deck is corrected, because a fade at surface scale is
a scrim by another name, and the draft must stay readable while a person
decides whether to lose it.

### The action pair

Exactly two actions. A third choice means the question is wrong.

| Action | Drawn as | Rule |
| --- | --- | --- |
| Quiet | Text only — no fill, no border, `--text-primary` | Always the safe path, always the keyboard default. |
| Committing, non-destructive | `--signal-interaction` fill, `--c-paper-ink` label | The one primary action on the surface while the question is open. |
| Committing, destructive | `--signal-refusal` fill, `--c-paper-ink` label | The only place refusal appears as a fill; it leaves with the question. |

Two filled actions are refused: they ask the person to rank them. The bar
itself is never tinted with the refusal hue — colouring the whole row makes
the safe path look dangerous too.

Escape while a confirm is open chooses the quiet action and closes the confirm,
not the surface. Enter never fires a destructive action; a destructive commit
is a deliberate pointer or key press on that action, which is the desktop
convention for irreversible choices.

### Wording

The question names what is lost, in the person's own words. The committing
action repeats the verb of the loss. The quiet action names what continues.
`OK`, `Yes`, `No`, and `Cancel` are refused, because they name the question
rather than the outcome — and `Cancel` on a question raised by cancelling is
unanswerable.

| Situation | Question | Quiet | Committing |
| --- | --- | --- | --- |
| Closing an editor with unsaved text | Discard unsaved changes? | Keep editing | Discard |
| Removing a placed Note | Remove this Note from the Layer? | Keep it | Remove |
| Discarding a saved Note | Discard this Note? | Keep it | Discard |

| | |
| --- | --- |
| Opens | An attempt that would destroy authored work with no way back. Never on hover, never on opening, never to report success. |
| Dismisses | Choosing either action, or Escape, which chooses the quiet one. |
| Focus returns to | The control that raised it when the surface stays open; when the committing action closes the surface, focus follows that surface's own return path. |

One confirm per surface at a time. A reversible action never raises one — a
question about work that can be undone teaches a person to stop reading
questions.

## Escape order

Escape peels exactly one layer per press.

| Topmost open | Escape does | Focus lands on |
| --- | --- | --- |
| Flyout | Closes the flyout | Its held parent row |
| Menu | Closes the menu | The target it was opened on |
| Inline confirm | Chooses the quiet action | The control that raised it |
| Viewer opened from a reading | Closes the viewer; the reading stays open | The reading |
| Local editor, clean draft | Closes the editor | Its source |
| Local editor, dirty draft | Raises the inline confirm | The quiet action |
| Slate pane | Closes that pane | The peer pane, or the invoking surface |
| Nothing above the Grid | Cancels the active Grid gesture or mode | The Grid cursor |

## Classifying a surface

A reviewer applies this test in order. The first yes is the class.

1. Does it raise a question about an action the person just took inside a
   surface? → **Inline confirm**, inside that surface.
2. Is it a list of actions, opened on a target, from which the person picks
   one? → **Menu or flyout**.
3. Does it serve one identified piece of content that must stay visible while
   it is open? → **Local editor**, on the Information Plane.
4. Is it fixed to the viewport, holding a body of work with no single source?
   → **Slate**, composed by the Slate host on the HUD.
5. Two yeses means the surface is doing two jobs; split it and classify each
   half.
6. No yes means it is a defect. It does not ship, and no class is invented to
   hold it.

## Not classes

These never ship, whatever they are called.

| Pattern | Why it is refused |
| --- | --- |
| Centred modal, alert, or dialog box | Detaches the question from what it is about; the inline confirm replaces it. |
| Scrim or Grid dimming | Claims an interruption Grove does not make, and hides live work. |
| Toast, snackbar, or success banner | Meaning that lives only in something that leaves; a result belongs in the surface that produced it. |
| Draggable or resizable floating window | Position must be a consequence of what a surface serves, never a thing to manage. |
| Tooltip carrying an action | An action reachable only by hovering has no keyboard path. |
| Coach mark, tour, or onboarding overlay | Interface that has to be narrated is interface that failed. |
| Wizard or multi-step overlay | A sequence of questions is a task; tasks belong in a Slate. |
| A second flyout level | Navigation wearing menu cloth; the digits stop saying which list they pick from. |
