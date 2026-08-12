---
type: design-system-foundation
status: active
date: 2026-08-09
tags: [grove, design-system, accessibility]
---

# Accessibility

Grove is a dark, spatial Grid that carries a great deal of meaning in
position and hue. That makes accessibility a structural requirement rather
than a finishing pass: a Grid whose meaning collapses without colour, or
without a pointer, is not a quieter product — it is an unusable one.

Every rule here is binding on every component. `10-grammar/States.md` owns how
states are drawn; this document owns what they must satisfy.

## Contrast

Grove targets WCAG 2.2 AA: **4.5:1** for text, **3:1** for the non-text parts
that carry meaning — signal outlines, focus rings, borders that establish
containment, and marks.

Grove sets no type at or above 24px in a role that would qualify for the
large-text allowance, so the 3:1 large-text threshold never applies to text.

### Ink on dark surfaces

Measured against `--c-base`, `--c-grid-min`, and `--c-surface-raised`.

| Step | Alpha | On canvas | On chrome | On nested | Legal for |
| --- | --- | --- | --- | --- | --- |
| `--ink-full` | `1` | 16.03 | 15.02 | 14.12 | Anything. |
| `--ink-primary` | `0.82` | 10.83 | 10.36 | 9.85 | Anything. |
| `--ink-secondary` | `0.62` | 6.53 | 6.35 | 6.19 | Anything. |
| `--ink-tertiary` | `0.51` | 4.75 | 4.71 | 4.61 | Text, at the AA floor. |
| `--ink-faint` | `0.30` | 2.39 | 2.46 | 2.47 | Unavailable text only. |
| `--ink-quiet` | `0.22` | 1.81 | 1.89 | 1.89 | Decorative borders. Never text, never a meaning-bearing edge. |
| `--ink-edge` | `0.16` | 1.49 | 1.54 | 1.57 | Decorative borders only. |
| `--ink-hairline` | `0.10` | 1.24 | 1.28 | 1.31 | Group separators only. |

`--ink-faint` is the one deliberate exception. Unavailable text is exempt from
the contrast minimum under WCAG, and Grove pairs it with a border and a
retained position so the state never depends on the low contrast alone.

An edge that carries meaning — a containment edge a person must find, a
selection or refusal outline — may not use `--ink-quiet`, `--ink-edge`, or
`--ink-hairline`. It uses a signal colour, all of which clear 3:1 by a wide
margin.

### Ink on paper surfaces

| Step | Alpha | On paper | Legal for |
| --- | --- | --- | --- |
| `--paper-strong` | `0.82` | 9.36 | Anything. |
| `--paper-body` | `0.62` | 4.75 | Text. |
| `--paper-label` | `0.62` | 4.75 | Text. |
| `--paper-border` | `0.28` | 1.82 | Drawn boxes and stand-in edges. Never text. |
| `--paper-rule` | `0.16` | 1.39 | Horizontal rules. Never text. |

### Signals

| Signal | On canvas | On chrome |
| --- | --- | --- |
| `--signal-interaction` | 9.50 | 8.90 |
| `--signal-active-work` | 10.61 | 9.94 |
| `--signal-refusal` | 5.63 | 5.28 |
| `--signal-authored-context` | 6.79 | 6.36 |

All four clear 4.5:1 as text and 3:1 as a mark on both dark surfaces.

### Authored Note colours

A Note sets `#F4F4F2` on its own fill.

| Fill | Text on fill | Fill on canvas |
| --- | --- | --- |
| `--c-note-violet` | 4.81 | 3.64 |
| `--c-note-clay` | 4.58 | 3.82 |
| `--c-note-slate-blue` | 4.73 | 3.70 |

All three clear 4.5:1 for the Note's own text and 3:1 as a surface against the
Grid. A fourth Note colour is legal only if it clears both.

## Without colour

Every state that uses hue carries a second, non-hue carrier. This table is the
audit; a component that cannot fill its row has a defect.

| State | Hue | Second carrier |
| --- | --- | --- |
| Rest | none | — |
| Approached | none | Chrome appears that was absent. |
| Focused | interaction | A ring outside the edge, at a distinct offset. |
| Selected | interaction | An outline outside the edge, plus brightened cells around the footprint. |
| Engaged | interaction or marquee | The drawn rectangle or footprint itself. |
| Pending | none | The stand-in form, holding position and extent. |
| Refused | refusal | 45° hatching on the blocked region, plus one plain sentence. |
| Unavailable | faint ink | A retained position and a hairline border. |
| Anchored | anchor | A ribbon or a rotated square, drawn as geometry. |

The test: render the frame in greyscale. Every distinction nameable in colour
must still be nameable, and every refusal must still read.

## Focus

Focus is always visible and is never suppressed. Removing a focus outline
without replacing it is a defect, regardless of how the component looks.

Off the Grid, focus is `--focus-ring` at `--focus-ring-offset`, applied on
`:focus-visible` only, drawn outside the component so it never covers content,
and drawn on top of any other state's outline. Focused and Selected must be
distinguishable when both are true: the focus ring sits outside the selection
outline with a visible gap.

On the Grid, keyboard attention is the Grid cursor at the focused cell. A
placement draws no second ring, because the cursor already says where
attention is, in cells, at the same size as everything else.

Focus order follows reading order within a surface. Opening a surface moves
focus into it; dismissing one returns focus to what opened it. Focus never
lands on a container that does nothing, and never leaves the open surface
while it is open.

## Keyboard

Every action is reachable from the keyboard. An action available only to a
pointer is a defect, not a convenience gap.

Grove's canonical key routing lives in `docs/reference/Keybind map.md`; this
document adds only the requirements every component inherits:

- Escape dismisses the topmost surface first, and only that surface.
- A surface that takes text input does not swallow the keys that leave it.
- A gesture that can be started from the keyboard can be cancelled from the
  keyboard, and cancelling restores the state before it began.
- No key sequence is required within a timing window.

## Naming

The accessible name of a placement is the content a person authored — the
Note's text, the Document's title, the picture's source name. A generic label
such as "Note" or "Image" as the accessible name is a defect: it replaces the
one thing that distinguishes this placement from every other.

Where content has no text, the name is the source name and the kind, in that
order. Decorative marks — the field, grid lines, the cursor trail — are hidden
from assistive technology rather than named, because they describe the
Grid rather than its contents.

Live regions announce only what a person could not otherwise perceive: a
refusal, a completed transfer, an arrival that changes what is present. Camera
movement, presence recomputation, and representation changes are not
announced.

## Forced colours

In a forced-colours mode the Grid keeps its structure and gives up its
palette. Grove redeclares, in system colours: text, surface fills, every
containment and focus edge, and the four signals.

What survives without redeclaration is the structural half of every state —
the ring outside the edge, the hatching on a blocked region, the ribbon, the
brightened cells, the plain sentence. This is the reason the second carrier is
mandatory: in forced colours it is frequently the only carrier left.

Backgrounds applied as images, gradients, or canvas paint do not survive.
Presence and the grid lines are drawn as paint, so in forced colours the field
falls back to a system-coloured border on occupied regions rather than
disappearing silently.

## Text scaling

Interface text scales to 200% without loss of content or function.

Chrome reflows: surfaces grow to their maximum width and then wrap, rows
become taller rather than clipped, and no surface introduces a scrollbar
inside a control group. Authored content on the Grid does not scale with
interface text — it is measured in cells and scales with the camera — so a
person raises interface text without disturbing the size or position of
anything they placed.

Nothing truncates at any scale. A label that cannot fit at 200% is a label
that was too long at 100%.

## Reduced motion

Under a reduced-motion preference every transition becomes an immediate state
change at the identical threshold. Nothing is delayed, nothing is skipped, and
no state becomes unreachable.

Meaning never lives only in the animation: the settled frame carries the same
information as the transition into it. An animated picture shows a complete
still frame at the same footprint.

## What is not yet asserted

Contrast, state carriers, focus visibility, keyboard reachability, and naming
are specified here and not yet checked by a script.
`90-conformance/Checks.md` records that gap and what would close it. A rule
being unchecked does not make it optional; it makes it the reviewer's job
until the check exists.
