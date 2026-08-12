---
type: design-system-component
status: active
date: 2026-08-09
component: Grid lines
plane: grid
surface_class: chrome
tags: [grove, design-system, component]
---

# Grid lines

The quiet ruling a person places things on: three spacings in two inks, faint
enough to orient by and never bright enough to read.

## Anatomy

Every visible part, named, with the token that gives it its value. A part with
no token is a part with no contract.

| Part | Required | Value |
| --- | --- | --- |
| Minor line | yes | Pitch `--grid-cell` ÷ `--grid-subdivisions` in world units — `44px` at `100%`. Ink `--grid-minor-ink`, multiplied by the computed fade alpha. |
| Major line | yes | Pitch `--grid-cell` in world units — `220px` at `100%`. Ink `--grid-major-ink`, multiplied by the computed fade alpha. |
| Supercell line | yes | Pitch `--grid-cell` × `--grid-supercell` in world units — `1100px` at `100%`. Ink `--grid-major-ink`, multiplied by the computed fade alpha. |
| Line weight | yes | `1` device-independent pixel on every part, at every zoom. Not a token: it is the only weight the system has, and a token would imply a second one exists. |
| Ground between lines | yes | `--surface-grid`, drawn by the Grid and never repainted here — the two inks are one step off canvas so content is always brighter than the field beneath it. |
| Origin line | no | None. The lines through the origin are drawn exactly like every other line of their spacing, because a marked origin privileges one place on a field where every place is addressable. |
| Fourth spacing, second ink, axis, ruler, coordinate, label | no | None, ever. |

Containment edge: none — a line contains nothing, and the three parts draw no
box, corner, or terminus. Fill: none — each part is a stroke, so the ground
shows between lines unaltered. Padding: none — the parts have no interior.
Type role: none — the field carries no type at any zoom, so no type token
applies.

Two inks, not three. A supercell line is not thicker, firmer, or brighter than
a major line; only its survival at distance distinguishes it, because weight
used to rank a spacing would read as containment.

## Geometry

- **Footprint** — the visible field, recomputed from the camera every frame.
  Executed in order: (1) take the camera's world bounds for the viewport;
  (2) for each part, `spacing = world pitch × camera scale`; (3) compute its
  ink by the rule below; (4) drop any part whose ink is `0`; (5) for each part
  that survives, emit one line at every integer multiple of its world pitch
  inside the bounds, extended one pitch past each edge; (6) assign each world
  coordinate to the coarsest part that owns it and to no other — a multiple of
  the supercell pitch is a supercell line only, a multiple of the cell that is
  not is a major line only, every remaining fifth is a minor line only;
  (7) project each line to screen and snap it to the nearest half device pixel
  so a one-pixel stroke lands crisp. Two readers executing this produce the
  same set.
- **Growth** — the drawn set grows with the viewport and with pulling back, and
  the fade is what bounds it: no part is emitted once its spacing reaches
  `--grid-fade-start`, so lines per axis never exceed the viewport dimension
  divided by `6`, plus the two drawn beyond each edge. The fade is a density
  guard as much as a legibility one.
- **Measure** — none. This component sets no type.
- **Alignment** — every major line sits on an integer multiple of `--grid-cell`
  in world space, and every footprint edge lands on one. Minor lines sit on
  fifths of a cell and are a reading aid only; only major lines are snap
  targets. Supercell lines sit on every fifth major line.

### The fade rule

One rule sets all three parts. There are no per-part constants and no zoom
steps, so nothing ever pops.

```text
spacing = pitch_world × zoom                       // device-independent screen pixels
ink     = clamp((spacing − 6) ÷ 8, 0, 1)           // --grid-fade-start 6, --grid-fade-end 14
alpha   = ink                                       // multiplied into that part's own ink
```

A part is invisible at or below `--grid-fade-start` and at full ink at or above
`--grid-fade-end`.

### Zoom, screen spacing, and ink

Pitches at `100%`: minor `44px`, major `220px`, supercell `1100px`. The major
column is the projected cell size, which is also the figure the representation
thresholds read, so line ink and representation are computed from one number.

| Zoom | Minor | Ink | Major | Ink | Supercell | Ink |
| --- | --- | --- | --- | --- | --- | --- |
| 1000% | 440.0px | 1.00 | 2200.0px | 1.00 | 11000.0px | 1.00 |
| 100% | 44.0px | 1.00 | 220.0px | 1.00 | 1100.0px | 1.00 |
| 40% | 17.6px | 1.00 | 88.0px | 1.00 | 440.0px | 1.00 |
| 31.82% | 14.0px | 1.00 | 70.0px | 1.00 | 350.0px | 1.00 |
| 25% | 11.0px | 0.63 | 55.0px | 1.00 | 275.0px | 1.00 |
| 20% | 8.8px | 0.35 | 44.0px | 1.00 | 220.0px | 1.00 |
| 13.64% | 6.0px | 0.00 | 30.0px | 1.00 | 150.0px | 1.00 |
| 6.36% | 2.8px | 0.00 | 14.0px | 1.00 | 70.0px | 1.00 |
| 6% | 2.6px | 0.00 | 13.2px | 0.90 | 66.0px | 1.00 |
| 4% | 1.8px | 0.00 | 8.8px | 0.35 | 44.0px | 1.00 |
| 2.73% | 1.2px | 0.00 | 6.0px | 0.00 | 30.0px | 1.00 |
| 1.27% | 0.6px | 0.00 | 2.8px | 0.00 | 14.0px | 1.00 |
| 1% | 0.4px | 0.00 | 2.2px | 0.00 | 11.0px | 0.63 |

Five boundaries fall out of the curve: minor reaches full ink at `31.82%` and
zero at `13.64%`; major reaches full ink at `6.36%` and zero at `2.73%`;
supercell reaches full ink at `1.27%` and rests at `0.63` at the `1%` floor,
so the field is never a blank plane inside the product zoom range.

### Why one part always arrives as another leaves

The parts sit `--grid-supercell` apart — a factor of `5` between each spacing
and the next. A part is mid-fade only while its spacing is between
`--grid-fade-start` and `--grid-fade-end`, a zoom window of `14 ÷ 6 = 2.33×`,
which is strictly narrower than the `5×` gap between parts. Two consequences
follow and neither is a coincidence:

- **At most one part is ever mid-fade.** While the minor is fading — zoom
  `13.64%` to `31.82%` — the major spans `30px` to `70px` and is at full ink.
  While the major is fading — `2.73%` to `6.36%` — the minor is already at zero
  and the supercell spans `30px` to `70px` at full ink. The one part that can
  fade with nothing coarser behind it is the supercell, and it does not reach
  zero before the zoom floor.
- **The on-screen rhythm repeats every factor of `25`.** Each part is `5×` the
  pitch of the one below, so the spacing a part vacates is the spacing the next
  one up inherits two steps later: minor `44px` at `100%`, major `44px` at
  `20%`, supercell `44px` at `4%`. The first and last panels of the catalogue's
  fade sequence read the same, with twenty-five times more Grid in view.

This is the whole reason there is no second grid: one system, five apart,
hands the field from one part to the next without a gap and without a
collision.

### Visibility

Line visibility is one view preference on `G`, and it is the only thing the
preference touches.

| Property | Rule |
| --- | --- |
| Scope | One preference per person, applied wherever they are, because there is one view record per person. |
| Storage | Beside the view record, never on a Layer, a Placement, or content. |
| Granularity | All three parts together; there is no per-part toggle, because a per-part toggle is a second grid. |
| Relationship to the fade | Visibility multiplies the computed ink to zero and never replaces the curve, so turning lines back on shows exactly the parts the current zoom produces. |
| Default and recovery | Visible. A stored value that is not `false` reads as visible, so an unreadable preference repairs itself. |
| Undo | None. The toggle writes no product fact, so it creates no undo entry and undo never restores or removes lines. |

Hiding lines removes no behaviour and unlocks none. It is not a mode, it grants
no free placement, no sub-cell position, and no second geometry. With lines
hidden, every one of these is pixel-identical to lines shown: snapping and
every footprint edge; placement, move, and resize previews; refusal hatching;
the selection outline and the field response beneath it; the Grid cursor, its
whole-cell edges, and its trail; presence at every cell value and every hue;
and anchored marks and ribbons.

The Grid carries no on-screen indicator of the preference, because the
state is legible from the field itself.

## States

All nine. `—` is not an answer; write "No change from Rest" where that is
true.

| State | Appearance | Notes |
| --- | --- | --- |
| Rest | The parts the current zoom produces, at their computed ink. | Rest is the only appearance this component has; everything else is a function of the camera. |
| Approached | No change from Rest. | The lines take no pointer at any distance, so the hand has nothing to answer. |
| Focused | No change from Rest. | The lines are never focusable; keyboard attention on the field is the Grid cursor, which draws itself. |
| Selected | No change from Rest. | Selection brightens whole cells above the lines; the lines beneath keep their ink, because the field response is drawn by the field. |
| Engaged | No change from Rest. | An open gesture draws above the lines and never restyles them, so the field never appears to move under a hand. |
| Pending | No change from Rest. | Line geometry is computed from the camera each frame and is never awaited, so there is nothing to hold a place for. |
| Refused | No change from Rest. | The refusal hatch covers whole cells over the lines; the lines are unaltered beneath it so extent stays exact. |
| Unavailable | Not reachable. | The lines have no condition in which they are present but cannot be acted on, because they are never acted on. |
| Anchored | No change from Rest. | Anchored is a property of a placement; the field carries no authored context of its own. |

## Behaviour

- **Pointer** — none, at any moment. The line layer takes no gesture and no
  hit test; a click, drag, or wheel at the same point reaches the field
  beneath, unchanged and at the same instant.
- **Keyboard** — `G` on the field, unmodified, shows or hides the lines. It is
  the only key this component answers. `G` is answered by the field rather than
  by a focused part, because no part of this component is focusable; the key is
  ignored while typing or while a surface above the Grid holds input.
- **Focus order** — none. This component has no focusable parts.
- **Escape** — nothing. Escape never restores hidden lines, because visibility
  is a standing preference and not an open gesture.
- **Commit and cancel** — the toggle commits on key-down and is written to the
  view record; it has no cancel, because it changes nothing a person authored.
  Toggling during an open gesture applies on the next drawn frame and never
  interrupts the gesture.

Where the toggle appears in the list of Grid controls belongs to the
surface that lists them, not to this component.

## Motion

| Transition | Duration | Curve | Reduced motion |
| --- | --- | --- | --- |
| Ink as the camera moves | None | None | No change. The ink is a continuous function of camera scale, not a timed transition, so there is nothing to shorten. |
| Lines shown or hidden | `0` | None | No change. The swap is already immediate, because a fading field reads as the Grid moving rather than as a preference answering. |

Nothing loops. Nothing idles. Nothing pulses or blinks.

## Distance

How the component reads at each representation tier. Thresholds are on
projected cell size and belong to `10-grammar/Representation-tiers.md`; this
section says only what this component sheds and what it keeps. Projected cell
size is the major spacing, so the two systems read one number.

| Tier | Sheds | Keeps |
| --- | --- | --- |
| Working | Nothing. | All three parts. Across this band the minor runs from `0.65` at a `56px` projected cell to full ink at `72px` and above; major and supercell are at full ink throughout. |
| Stepped | The minor part, which reaches zero at a `30px` projected cell — inside this band, not at its edge. | Major and supercell, both at full ink for the whole band. |
| Stand-in | The major part, which reaches zero at a `6px` projected cell. | The supercell, at full ink down to a `2.8px` projected cell and never below `0.63` inside the product zoom range. |

Presence, position, and extent are never shed: a part leaves at its full
geometry and never re-spaces, so pitch and phase are identical the moment it
returns. This component has no stand-in and no kind-coded form, because it is
not a placement — it is the ground a placement is measured against, and it
holds no identity to code.

## Accessibility

- **Role and name** — none. The line layer is presentation that describes the
  Grid rather than its contents, so it is hidden from assistive technology
  rather than named, per `00-foundations/Accessibility.md`.
- **Contrast** — no text. At full ink, `--grid-minor-ink` reaches `1.07:1` and
  `--grid-major-ink` reaches `1.25:1` against `--surface-grid`, falling
  toward `1.00:1` as the fade reduces alpha. Both are deliberately below the
  `3:1` non-text minimum, which applies to marks and edges that carry meaning:
  these carry none that a person must find, and a line that met `3:1` would
  outrank the content sitting on it and break Law 1.
- **Without colour** — the three parts are told apart by spacing alone, and two
  of them already share one ink, so hue carries nothing here and greyscale
  changes nothing.
- **Forced colours** — the lines are canvas paint and do not survive, and none
  is redeclared in system colours. Nothing is lost: `Accessibility.md` owes a
  system-coloured border on occupied regions, which is where extent lives, and
  redeclaring the field would put a high-contrast ruling over every placement.
- **Text scaling** — nothing changes. The component sets no type, and its
  pitches are measured in cells, so interface text at `200%` alters neither
  spacing nor weight.
- **Reduced motion** — no change from the Motion table.

## Copy

None. The field carries no copy at any zoom, in either visibility state, and
in every state above: orientation lives in geometry and ink alone.

The control that shows or hides the lines is not part of this component; its
two labels are fixed by `00-foundations/Space-and-grid.md` and owned by the
surface that lists Grid controls.

## Refusals

- **One uniform spacing** — a single pitch at a fixed ink turns distance into a
  moiré texture; the three parts and the fade exist so stepping back stays calm
  space.
- **Any second grid** — one line system answers every zoom, and a second would
  need a second snap target, at which point the cell stops being the unit of
  position.
- **A fourth spacing, or a per-part fade constant** — one rule sets all three,
  and a per-part constant is a second grid wearing the first one's tokens.
- **A firmer supercell** — the supercell is never thicker, brighter, or a
  different ink from a major line, because weight used to rank a spacing reads
  as containment.
- **Line weight that scales with the camera** — a thickening line makes zoom
  change what the field means rather than how much of it is in view.
- **A marked origin, axis, or centre** — a marked place privileges one position
  on a field where every position is addressable.
- **Any text on the field** — no coordinate, scale readout, ruler, cell count,
  or label at any zoom, because a number on the field is a report about the
  Grid standing where the Grid should be.
- **Grouping drawn from lines** — no tint, box, highlight, or heavier ruling
  around a run of cells; the field orients, it never packages.
- **Snapping to a minor line** — only major lines are snap targets, because the
  cell is the unit of every footprint.
- **Zoom steps or snap-to-zoom levels** — the fade is continuous across the
  whole range, and a stepped camera would make a part pop.
- **Hiding lines as a mode** — the preference removes no behaviour and unlocks
  none, and every listed behaviour is pixel-identical with lines off.
- **Answering a pointer** — the line layer is never hit-tested, because a line
  that can be clicked has become chrome parked on the field.


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

- Catalogue deck: `docs/design_catalogue/src/grid-plane/01-the-grid.html`
- Catalogue decks, corroborating: `docs/design_catalogue/src/grid-plane/02-note.html`, `03-document.html`
- Decision: `docs/decisions/Grid line visibility control.md` (D-GRID-11)
- Wireframe: `docs/ux/wireframes/grid-scale/Grid line visibility.md`
- Foundation: `docs/design-system/00-foundations/Space-and-grid.md`, `docs/design-system/00-foundations/Tokens.md`
