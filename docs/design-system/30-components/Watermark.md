---
type: design-system-component
status: active
date: 2026-08-09
component: Layer mark
plane: grid
surface_class: chrome
tags: [grove, design-system, component]
---

# Layer mark

The number and name of the Layer being worked on, set large and very quiet in
the corner of the Grid, so a person can see where they are without asking
for it.

## Anatomy

Every visible part, named, with the token that gives it its value. A part with
no token is a part with no contract.

| Part | Required | Value |
| --- | --- | --- |
| Numeral | yes | The Layer's own label — `01`, or a side letter and two padded digits. `--f-display` at weight 500, `208px`, `--lh-tight`, `--tr-title`, ink `--ink-whisper`. |
| Name line | no | The Layer's name as a person wrote it, uppercased by type treatment. `--f-display` at weight 500, `--t-body`, `--lh-tight`, `--tr-caps`, ink `--ink-edge`. Absent, not blank, when the Layer has no name. |
| Gap between the two lines | yes | `--sp-xs`. |
| Optical inset on the name | yes | `3px` at the right — `--tr-caps` × `--t-body` — cancelling the trailing letter-space that tracking adds after the last capital, so the name's final glyph lands on the numeral's right edge. |
| Position | yes | Inset `--sp-xl` from the right edge of the viewport and `--sp-md` from the bottom, fixed to the viewport and unaffected by the camera. |
| Containment edge | no | None, ever. |
| Fill | no | None, ever. The Grid shows through both lines unaltered. |
| Padding | no | None. The mark has no interior. |
| Shadow | no | None. Grove has two shadows and both separate a surface; this is not a surface. |

Both lines are right-aligned on one edge, which is what makes the pair read as
one mark rather than as two labels.

`208px` is this component's own figure and no token carries it.
**`--t-watermark` should**, declared in `00-foundations/Tokens.md` beside the
nine reading sizes rather than inside them, because the numeral is not read.
The nine sizes answer the question "how big is this text for the job of being
read"; this mark is never read at `1.08:1`, and `00-foundations/Tokens.md`
already anticipates it by naming `--ink-whisper`'s use as *watermark scale,
ambient marks* — an ink beneath every step the legality table admits, reserved
for exactly this job. The exemption is bought by the ink and extends no
further: a mark set above `--ink-whisper` is text and takes one of the nine.

The `3px` optical inset is a component value because no other component sets a
right-aligned tracked capital against a second line's edge.

The mark has no border, no box, no plate, no icon, no separator, and no second
figure of any kind.

### Why the Grid carries a standing mark at all

`20-planes/Grid-plane.md` sets the strictest chrome budget in the system: at
rest, with the pointer away, only placed content, grid lines, presence, and the
Grid cursor are drawn. This mark is the one standing exception, and it is
bought three ways.

**It answers the one question position cannot.** Every other fact on the
Grid is carried by where a thing is. A Layer is the exception — two Layers
occupy the same coordinates, and presence from the others arrives as atmosphere
with no frames and no readable text — so nothing in the field's geometry says
which Layer is the one drawn in full. The mark states that, and states nothing
else.

**It is drawn over the field and not in it.** It keeps a fixed size while the
camera moves, holds no cells, casts no presence, and takes no hit test, so
every placement, footprint, preview, snap target, and line is pixel-identical
whether the mark is drawn or not. The budget is spent by things a person could
mistake for something placed; this cannot be mistaken for one.

**It is quieter than the field it sits on.** The numeral reaches `1.08:1`
against `--surface-grid` and the minor grid line reaches `1.07:1` — the
mark is field-strength, not chrome-strength, and standing still is what that
buys. A mark that met the `3:1` non-text minimum would outrank the content
sitting on the Grid and break Law 1.

### What it must never become

A status line. The moment the mark carries a count of placements, a zoom
readout, a coordinate, a saved-at time, a person's name, or a condition, it has
stopped naming where a person is and started reporting on the Grid, and
`30-components/Grid-lines.md` already refuses a number on the field for that
reason. Its content is fixed: the Layer's label, and the Layer's name if there
is one.

## Geometry

- **Footprint** — screen pixels, not cells, because the mark is not a
  placement. Executed as a procedure: (1) set the numeral in `--f-display` 500
  at `208px` with `--lh-tight`, giving a box `203.84px` high; (2) if the Layer
  has a name, set it in `--f-display` 500 at `--t-body` with `--lh-tight`,
  giving a box `14.7px` high, and add `--sp-xs` between the two boxes;
  (3) right-align both boxes on one edge, insetting the name a further `3px`;
  (4) place that edge `--sp-xl` from the right of the viewport and the foot of
  the numeral `--sp-md` from the bottom. The height is `222.54px` with a name
  and `203.84px` without. Two readers executing this produce the same box.
- **Growth** — none. The mark does not grow, and it is the one thing in Grove
  for which that is correct: it holds no authored content, so Law 9 has nothing
  to protect. A Layer name longer than the room available runs to one line at
  its stated size and is clipped by the viewport edge, which loses nothing
  because the Layer list carries the same name in full at full contrast. A
  second line is refused, because a wrapped tracked capital has become prose in
  the wrong dress.
- **Measure** — none. Neither line is a column of continuous text; each is one
  line that never wraps.
- **Alignment** — the mark lands on no grid line and is not required to. It is
  measured from the viewport's right and bottom edges, because it belongs to
  the view rather than to a position in the Grid, and a mark snapped to a
  cell would claim a place on a field where it occupies none.

## States

All nine.

| State | Appearance | Notes |
| --- | --- | --- |
| Rest | The numeral, and the name above it when the Layer has one, at their stated inks in the corner. | Rest is the only appearance this component has. Every other row below is a consequence of that. |
| Approached | No change from Rest. | The mark takes no pointer at any moment, so there is nothing for the hand to be near. |
| Focused | Not reachable. | No part is focusable. The mark offers no action, so focus on it would land on something that does nothing, which `00-foundations/Accessibility.md` forbids. |
| Selected | No change from Rest. | The mark is not content and holds no cells, so there is nothing to select and no outline to wear. |
| Engaged | No change from Rest. | No gesture opens on the mark, and a gesture open elsewhere never restyles it, so the corner never appears to move under a hand. |
| Pending | No change from Rest. | The Layer's label and name are read from the view record and are never awaited, so there is nothing to hold a place for. |
| Refused | No change from Rest. | A refused Layer operation is answered in the surface that asked, with hue, structure, and words held while the condition holds. Nothing is drawn here, and nothing flashes. |
| Unavailable | Not reachable. | The mark has no condition in which it is present but cannot be acted on, because it is never acted on. |
| Anchored | No change from Rest. | Anchored is a property of a placement; a Layer carries no authored context of its own. |

Combination is trivial: eight of the nine draw the resting appearance, so no
two states ever contend for the same region.

## Behaviour

- **Pointer** — none, at any moment. The mark takes no hit test, so a click,
  drag, context gesture, or wheel over it reaches the Grid beneath,
  unchanged and at the same instant. It has no pressed appearance, because
  there is nothing to press.
- **Keyboard** — none. The mark answers no key. `[` and `]` traverse Layers and
  the mark follows what they change; `Ctrl/Cmd+N` in the Layer list renames the
  current Layer. Both are `docs/reference/Keybind map.md` and neither is
  redefined here.
- **Focus order** — none. This component has no focusable parts, and focus
  never returns here from a surface it did not open.
- **Escape** — nothing. The mark opens nothing, so there is nothing for Escape
  to peel.
- **Commit and cancel** — none. The mark writes no product fact, creates no
  undo entry, and has nothing to cancel.

**On a Layer change.** The numeral and the name are replaced in the frame the
current Layer changes, in the same frame the Grid's content changes, with
no transition in either line. Where the new Layer has no name the name line is
removed rather than emptied, so the numeral sits at the same distance from the
bottom edge on every Layer.

Traversing, creating, reordering, renaming, and removing a Layer belong to the
Layer list. Naming a Layer belongs to the surface that takes the name. This
component reads the result and draws it.

## Motion

| Transition | Duration | Curve | Reduced motion |
| --- | --- | --- | --- |
| The numeral and the name on a Layer change | `0` | None | No change. The swap is already immediate. |
| A refused Layer operation | None | None | No change. Nothing is drawn here, in either preference. |

Nothing in this component transitions. A Layer change is not `--d-swap`: that
step exchanges two representations of one thing at one extent, and these are
two different Layers. A cross-fade here would make switching Layers feel slower
than it is while the content behind it has already changed, and
`00-foundations/Motion.md`'s seventh question answers the rest — a transition
that cannot name its job is not drawn.

Nothing loops. Nothing idles. Nothing pulses or blinks.

## Distance

Thresholds are on projected cell size and belong to
`10-grammar/Representation-tiers.md`.

| Tier | Sheds | Keeps |
| --- | --- | --- |
| Working | Nothing. | Both lines, at their stated figures. |
| Stepped | Nothing. | Both lines, at their stated figures. |
| Stand-in | Nothing. | Both lines, at their stated figures. |

The mark reads identically at every tier and at every camera scale, because it
is not drawn in the Grid's coordinate space. It has no stand-in and no
kind-coded form: it is not a placement, holds no identity to code, and at the
zoom floor it is the only thing on screen still saying which Layer the field
belongs to.

This does not breach the refusal of counter-scaling. That refusal binds what is
drawn in Grid coordinates — placements, previews, the Grid cursor, the presence
field — and `10-grammar/Representation-tiers.md` grants the same carve-out to
the reading marker on the same reasoning: this mark cannot be selected, never
becomes a placement, and lies about no position because it claims none.

## Accessibility

- **Role and name** — none. The mark is hidden from assistive technology rather
  than named, because it describes the Grid rather than its contents,
  which is the rule `00-foundations/Accessibility.md` already applies to the
  field, the grid lines, and the cursor trail. The Layer it names is reachable
  and announced by the Layer list, where the current Layer carries
  `aria-selected`. A Layer change is not announced: it is a change of view, and
  the content list beneath it changes at the same moment.
- **Contrast** — the numeral reaches `1.08:1` against `--surface-grid` and
  the name reaches `1.49:1`. Both are deliberately below the `4.5:1` text
  minimum and below the `3:1` non-text minimum, and both are exempt as
  decoration on one condition, stated as a refusal below: the mark carries no
  fact that is not also carried at full contrast in the Layer list. That
  exemption and the decision to hide the mark from assistive technology are the
  same decision — a mark holding a fact a person must find could not be hidden,
  and a mark that may be hidden is not holding one. Raising either line to the
  text floor would put the quietest thing on the Grid above the content it
  sits behind and break Law 1.
- **Without colour** — nothing is lost. No state here uses hue, both lines are
  neutral ink on the Grid, and the mark is legible in greyscale exactly as
  it is in colour, which is to say barely, by design.
- **Forced colours** — the mark is not drawn. Text is one of the things
  `00-foundations/Accessibility.md` redeclares in system colours, and
  redeclaring a `208px` numeral would take it from `1.08:1` to full system
  contrast and make the faintest mark on the Grid the loudest. Nothing is
  lost, because everything it says is in the Layer list, which is redeclared and
  stays readable. This is the one component in Grove whose correct behaviour in
  forced colours is absence.
- **Text scaling** — nothing changes. The mark's two figures are fixed and do
  not grow with interface text, because it carries no content to lose and
  doubling a `208px` mark would lay ambient paint across a quarter of the
  Grid. A person raising interface text to `200%` gets the Layer's name at
  double size in the Layer list, which is where it is read.
- **Reduced motion** — no change from the Motion table.

## Copy

**None.** This component shows no string Grove wrote.

The numeral is the Layer's own label and the name is the Layer's name as a
person wrote it. Neither is copy: Grove never edits, shortens, re-cases in the
text, prefixes, or adds a word to either, and the uppercase is applied by
`text-transform` with `--tr-caps` rather than by writing capitals.

An unnamed Layer shows no name line and no placeholder. `Untitled Layer`
belongs to the Layer list, where a row must be pickable and therefore must have
something to pick; here there is nothing to pick, and
`00-foundations/Typography.md` is absolute that Grove never generates a title
for something a person has not named.

The shipped accessible name `Rename current Layer` is removed with the pointer
route that motivated it. It named an action this component must not offer, and
`10-grammar/Copy.md` binds accessible names as strictly as visible ones.

## Refusals

- **Answering the pointer** — a mark that can be clicked has stopped being a
  statement and become chrome parked on the Grid, which is the one thing
  the field's budget refuses outright.
- **Carrying a fact the Layer list does not** — the low ink and the hiding from
  assistive technology are both licensed by the mark being redundant, so a fact
  that lives only here is unreadable and unreachable at once.
- **A count, a coordinate, a scale readout, a date, or any condition** — a
  figure on the field is a report about the Grid standing where the
  Grid should be.
- **A refusal drawn here** — refusal is hue, structure, and one plain sentence
  beside the thing refused, and a colour that flashes in the corner and returns
  to rest has said nothing a person can go back and read.
- **Any transition, at any duration** — no transition here can name one of the
  six jobs, and a mark that moves while a person is reading is competing with
  what they are reading.
- **Scaling with the camera** — the mark names the view, not a place in the
  Grid, and a Layer number that grows as a person zooms in claims a
  position in the field that it does not have.
- **Type driven by the viewport** — `clamp()`, `vw`, and fit-to-box scaling are
  refused everywhere in Grove, and a mark that resizes when a window is dragged
  makes the same Grid two different pictures on two machines.
- **A second display weight** — Grove sets one weight for display type, and a
  mark set at a weight nothing else uses reads as a second voice for no gain at
  an ink where the difference is barely visible.
- **A containment edge, a fill, a plate, or a shadow** — every one of them
  turns an ambient mark into a badge, and the Grid has no card layout.
- **A second standing mark** — one exception to the chrome budget is an
  exception, and two is a status bar being assembled a piece at a time.
- **Being announced** — a Layer change is a change of view, and
  `00-foundations/Accessibility.md` announces only what a person could not
  otherwise perceive.


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

- Foundation: `00-foundations/Tokens.md` — `--ink-whisper` is named for
- Foundation: `00-foundations/Motion.md` — the `320ms` refusal flash on this
- Foundation: `00-foundations/Accessibility.md` — marks that describe the
- Reference: `docs/reference/Keybind map.md` — `[` and `]` traverse Layers;
