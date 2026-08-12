---
type: design-system-foundation
status: active
date: 2026-08-09
tags: [grove, design-system, motion]
---

# Motion

Grove animates to answer a person, never to entertain one. Every transition
has a job, a step from the duration scale, and one of two curves. A transition
that cannot name its job is deleted, not retimed.

`Tokens.md` owns the values. This document owns which transition takes which
step, which curve it may use, what motion is forbidden to do, and what happens
when a person asks for less of it.

## The three jobs

Motion does exactly three things. There is no fourth.

| Job | What it means | Where it appears |
| --- | --- | --- |
| Confirm an action | The person did something and the interface says so once. | Press acknowledgement, placement confirmation, the single expanding mark after a committed sweep. |
| Carry an object | Something arrives, leaves, or is exchanged for another form of itself. | A Note arriving in its cells, a Document removed, a stand-in becoming a stepped form. |
| Settle a surface | Chrome that serves the hand appears or leaves with the hand. | A resize corner, an edit affordance, a flyout, a reading marker's line. |

Motion that decorates, attracts, teaches, entertains, fills waiting time, or
carries a meaning the settled frame does not already carry is a defect. The
test is one sentence: if the frame before the transition and the frame after it
were shown side by side with no movement between them, the person must lose
nothing.

## Two kinds of movement, and one that is not Grove's

Only the first kind takes a duration token.

| Kind | Driven by | Duration | Stops when |
| --- | --- | --- | --- |
| Transition | A discrete change between two settled appearances. | One of the six steps. Runs once. | It finishes. |
| Continuous rendering | The hand or the camera, redrawn each frame. | None. A duration token on it is a defect. | The input stops. |
| Authored animation | The content's own source. | The source's. | The person pauses it. |

Continuous rendering is the Grid cursor head and its spent-cell decay, the
field response beneath a placement, and grid tier ink as line spacing changes.
Each depicts a current value rather than a change over time, so it never starts
on its own and never continues after the hand does.

Authored animation is a picture whose source animates. It plays inside its own
frame, at its own rate, and the cells around it are still. It is content, not
interface motion, and the prohibitions below do not bind it.

## The six steps

Every Grove transition is one of six. A seventh step is a defect; so is a raw
millisecond value in product CSS.

| Step | The transition it names | Curve |
| --- | --- | --- |
| `--d-press` | Pointer-down acknowledgement: the pressed appearance is taken on press and released on release, both over this step. | `--ease` |
| `--d-fade` | Chrome answering the hand: a resize corner, an edit affordance, a marker line, a flyout, a menu, a local editor — in on approach or open, out on leave or dismiss. | `--ease` |
| `--d-swap` | Exchanging one representation for another in the same place, at the same extent: a stand-in becoming a stepped form, a Pending form replaced by its result, one pane's content replaced by another. | `--ease` |
| `--d-exit` | Content leaving the Grid: a placement removed, a Memory withdrawn from the Grid. | `--ease` |
| `--d-place` | Arrival and placement confirmation: content committing to its cells, a Memory returning to the Grid, content brought to the hand. | `--overshoot` |
| `--d-sweep` | One expanding mark, drawn once and gone, that confirms a completed gesture at the point it happened. | `--ease` |

The governing distinction between the content steps and the chrome step:
**chrome uses `--d-fade` in both directions; content uses `--d-place` to arrive
and `--d-exit` to leave.** A flyout closing is not an exit, because nothing left
the Grid.

Durations are declared in milliseconds. A duration written in seconds is a
defect even when the value is correct, because a scale expressed in two units
cannot be read at a glance.

## Choosing a step

Ask in this order and stop at the first yes.

1. Does it acknowledge a pointer press? `--d-press`.
2. Does chrome appear or leave because the hand came near, opened it, or left? `--d-fade`.
3. Does one form replace another in the same position at the same extent? `--d-swap`.
4. Is content leaving the Grid? `--d-exit`.
5. Is content arriving, committing to cells, or being brought to the hand? `--d-place`.
6. Is it a single expanding mark confirming a gesture that has already completed? `--d-sweep`.
7. None of the above? The transition has no job. Do not animate; change the frame.

Question seven is the answer whenever the canon is silent. A new transition is
never invented for a new component; the component either fits a step or settles
without motion.

## Properties that may move

Transitions animate `opacity`, `transform`, and colour only. Animating
`width`, `height`, `top`, `left`, `margin`, `padding`, or any other layout
property is a defect, because no state may change a component's footprint
(`10-grammar/States.md`) and layout animation cannot hold a frame budget.

Colour may be animated only where a mark changes role in place and holds the
new role — the engagement drawing turning to the refusal role, for example. A
colour that animates away from its start value and returns to it is a flash and
is forbidden.

## The two curves

`--ease` carries every transition in Grove.

`--overshoot` is reserved for placement confirmation and arrival. It pairs
**only** with `--d-place`; overshoot at any other duration is a defect, and
this is the single rule a check needs to assert.

| Permitted use of `--overshoot` | Job |
| --- | --- |
| Content commits to its cells | Placement confirmation. |
| A Memory returns to the Grid | Arrival. |
| Content is brought to the hand | Arrival. |

Nothing else. Not exit, not swap, not fade, not press, not the sweep mark, and
never on a hover, a focus ring, or a menu.

Overshoot is drawn on the arriving content's own visual, which may exceed its
footprint for the length of the transition only. The footprint does not move,
neighbours do not shift, and hit-testing lands on the true cells from the first
frame.

## What motion may never do

Each line is written so a check can assert it.

- **Never loop.** Every Grove transition runs once. An iteration count above one
  on anything Grove draws is a defect.
- **Never pulse, breathe, throb, blink, flash, shimmer, or march.** A moving
  edge competes with the hand that is moving over it.
- **Never idle.** Nothing moves while the person is doing nothing. An interface
  at rest is a still image.
- **Never spin.** There is no spinner, progress ring, skeleton shimmer, or
  indeterminate bar anywhere in Grove; a waiting component holds its most
  complete form.
- **Never seek attention.** No entrance stagger, no bounce on hover, no
  scroll-linked reveal, no parallax, no motion that arrives unrequested to point
  at something.
- **Never move the camera on its own.** Pan and zoom track the input frame by
  frame, and a command that changes the view lands in one step with no travel,
  because a camera that flies makes a person wait to find out where they are.
- **Never carry meaning alone.** If the transition is the only thing that said
  it, the transition is not the problem — the settled frame is.
- **Never animate a focus ring or a selection outline.** Both appear on the
  frame attention or selection changes, because a person tabbing quickly must
  see where they are without waiting.
- **Never animate a refusal.** Refusal is hue, structure, and words held for as
  long as the condition holds, never a mark that appears and returns.
- **Never animate a placement preview.** The preview holds one settled
  appearance and changes hue and hatch in place as validity changes.

## Input is never held hostage

A transition is something the interface is drawing. It is never something the
person is waiting for.

- Every gesture acts on the input event, not on a transition's completion:
  hit-testing, pointer capture, key handling, caret placement, typed characters,
  commit, cancel, and camera response.
- Pressing again mid-transition retargets immediately. The new transition starts
  from the currently drawn position, not from the old start value, so nothing
  snaps backwards.
- Escape dismisses during a transition, at the moment it is pressed.
- No transition may remove pointer or keyboard reach from anything a person can
  act on. Marks that are drawn and gone — the expanding sweep mark, the arrival
  overshoot — take no input at any point in their life.
- Nothing is queued behind a transition. Two actions in quick succession produce
  two results in order, whatever the drawing is doing.
- A transition never gates a durable write, and a durable write never waits for
  one.

## The one-frame rule

A component may lag one frame in what it draws around itself. It may never lag
in what it does with the hand. The budget is one frame at the display's native
refresh rate, with 144 Hz as the floor.

| May settle one frame late | Must answer on the input frame |
| --- | --- |
| The field response beneath a placement. | Hit-testing and pointer capture. |
| The perimeter ring on an occupied region. | The dragged object's position under the pointer. |
| The presence a placement casts. | The caret and every typed character. |
| Grid tier ink after a zoom step. | Every key the component answers, including Escape. |
| A stand-in's snapshot refresh. | The focus ring and the selection outline. |
| Promotion from a distant form to the working form. | Pan and zoom. |

Promotion is asynchronous and never blocks camera motion; the true form takes
over with the identity, position, and extent the distant form promised.

## Motion and the nine states

`10-grammar/States.md` owns the states. This table says only what moves when
each is entered, and nothing moves that is not listed.

| State entered | What moves | Step |
| --- | --- | --- |
| Rest | Chrome that served the hand leaves. | `--d-fade` |
| Approached | Chrome that serves the hand arrives. | `--d-fade` |
| Focused | Nothing. | — |
| Selected | Nothing. The outline is drawn on the frame selection changes; the field response may settle one frame later. | — |
| Engaged | Nothing on entry. The drawing tracks the hand as continuous rendering. | — |
| Pending | Nothing on entry. | — |
| Pending → result | The result replaces the pending form in place. | `--d-swap` |
| Refused | Nothing. | — |
| Unavailable | Nothing. | — |
| Anchored | Nothing. | — |

A pointer press adds `--d-press` over whatever state is current, and releases
it on the same step. Press acknowledgement is never omitted and never extended
into a hold.

## Reduced motion

Under a reduced-motion preference every duration token resolves to zero. That
is the whole mechanism; there is no separate reduced-motion design.

| Transition | What the person gets instead |
| --- | --- |
| `--d-press` | The pressed appearance applied on press and removed on release, with no scaling. |
| `--d-fade` | Chrome present or absent at the same approach, open, and dismiss thresholds. |
| `--d-swap` | The new form in place of the old on the same frame, at the same threshold, with no cross-fade. |
| `--d-exit` | The content gone on the frame the removal commits. |
| `--d-place` | The content at full size in its true cells on the frame the placement commits. |
| `--d-sweep` | Nothing drawn, because the mark is pure expansion and what it confirms is already in the settled frame. |
| Authored animation | Opens paused on a complete frame, with playback offered. |

The contract around that table:

- **Identical thresholds.** A transition that ran at a threshold becomes a
  change at the same threshold — not at the moment the animation would have
  ended, and never on a timer.
- **Identical end state.** The settled frame is the same frame either way,
  pixel for pixel.
- **No substitutions.** A removed transition is never replaced by a delay, a
  shorter transition, a cross-fade, or a colour flash.
- **No change to anything else.** Geometry, footprints, hysteresis gaps,
  representation thresholds, and which states are reachable are all unchanged.
- **Continuous rendering continues,** because it depicts a current value rather
  than a change over time; the cursor head, the field response, and grid tier
  ink all behave as they do otherwise. The spent-cell decay tail is the one
  exception and is skipped, so a cell clears on the frame the cursor leaves it —
  the tail carries no meaning and is the only movement in that set that persists
  after the hand stops.
- **The preference is honoured live,** without a reload, following desktop
  convention for accessibility preferences.

## Shipped durations with no distinct job

Four durations exist in the product stylesheets and name no step. Each migrates
to the nearest step, and where the transition itself is forbidden it is deleted
rather than retimed.

| Shipped value | Where it appears | Migrates to | What happens to the transition |
| --- | --- | --- | --- |
| `320ms` | The refused-Layer colour flash on the watermark name. | `--d-place` | Deleted. A refusal that flashes and returns leaves its meaning only in the motion. |
| `360ms` | Content brought to the hand. | `--d-place` with `--overshoot` | Kept. Bringing content to the hand is arrival, and arrival is the one overshoot use. |
| `440ms` | The expanding ring after a committed gesture. | `--d-sweep` | Kept, unchanged in role. |
| `0.15s` | The hover response on a Slate's pane divider. | `--d-fade` | Kept. Chrome answering the hand is always the fade step, and the value is rewritten in milliseconds. |

`Tokens.md` names the first three; the fourth is listed here because it is
written in seconds and a millisecond scan does not find it.

Two shipped animations loop and are deleted outright, not retimed.

| Shipped animation | Why it is deleted |
| --- | --- |
| A 1.5s infinite pulse on the placement preview. | A preview under a moving hand must be still, so the hand is the only thing moving. |
| A 0.5s infinite blink on the refused preview. | Refusal is hue, hatch, and words held while the condition holds; a blink hides the refusal half the time. |

## Contradictions ruled

- **The catalogue against the build, on the placement preview.** The selection
  and placement deck draws the preview as a static dashed edge with a
  transparent fill; the shipped stylesheet pulses it and blinks its refused
  variant. The deck wins and the animations are deleted, because the canon
  forbids idle loops and the deck is the rendering of that rule.
- **The canon against itself, on looping.** The prohibition on looping motion
  and the picture decks' animating frames are both correct: the prohibition
  binds what Grove draws, and a picture whose source animates plays inside its
  own frame under the person's control.
- **The catalogue against the build, on the refusal flash.** The refusal
  grammar holds hue, structure, and words for the duration of the condition;
  the shipped stylesheet flashes the watermark name and returns it to rest. The
  grammar wins, because a mark that returns to rest has said nothing a person
  can go back and read.

## Conformance

`90-conformance/Checks.md` names the scripts. The assertions this document
makes checkable:

| Assertion | Source |
| --- | --- |
| Every duration in product CSS is one of the six step tokens. | `css/*.css` |
| No duration is written in seconds. | `css/*.css` |
| `--overshoot` appears only alongside `--d-place`. | `css/*.css` |
| No animation declares an iteration count above one, and none declares `infinite`. | `css/*.css` |
| No transition or animation names a layout property. | `css/*.css` |
| Every duration token resolves to zero under a reduced-motion preference. | `css/tokens.css` |

## Sources

- `DESIGN.md` — Motion.
- `docs/design_catalogue/src/00-design-language.html` — no meaning that lives only in motion.
- `docs/design_catalogue/src/grid-plane/06-distance.html` — arrival in place, promotion never blocking the camera.
- `docs/design_catalogue/src/grid-plane/07-selection-placement.html` — the settled placement preview.
- `docs/design_catalogue/src/information-plane/04-annotation-markers.html` — motion as courtesy, not message.
- `docs/design_catalogue/src/grid-plane/04-image.html`, `docs/design_catalogue/src/hud/03-gallery-slate.html` — authored animation inside its own frame.
- `docs/raw/original-notes/Grove at a distance.txt` — the camera is a lens.
- `docs/decisions/Grid navigation focal continuity.md` — the view changes without losing the focal point.
