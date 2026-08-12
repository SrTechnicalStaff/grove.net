---
type: design-system-foundation
status: active
date: 2026-08-09
tags: [grove, design-system, principles]
---

# Principles

Nine laws. Every other document in this system is an expansion of them, and no
plane, component, or deck may contradict one. A law is never traded away for a
single screen: where a design cannot be built without breaking one, the design
is wrong.

Each law has a statement, a reason, and a test. The test is written so one
reviewer can apply it to a screenshot or a thirty-second recording and get the
same answer as the next reviewer.

| # | Law |
| --- | --- |
| 1 | Content is the hero. |
| 2 | The field orients, it never packages. |
| 3 | Chrome answers the hand and leaves. |
| 4 | Distance simplifies, it never subtracts. |
| 5 | Nothing loops. |
| 6 | Colour is never the only carrier. |
| 7 | The Grid stays lit, and nothing floats free. |
| 8 | Footprints are whole cells. |
| 9 | Authored content is never made to fit. |

## 1 · Content is the hero

**Statement.** On every screen authored content is the most complete and the
most present thing, and chrome ink never exceeds `--ink-secondary` on a
surface carrying `--ink-primary` content. Two marks are exempt because they
answer the person rather than decorate the screen: the focus ring and the Grid
cursor, which `10-grammar/States.md` fixes at `--ink-full`.

**Reason.** Grove exists to keep authored information visible, so chrome that
outranks content inverts the product.

**Test.** Squint at the frame: if the first thing that reads is a control, a
label, a border, or a badge rather than authored content, the frame fails. A
surface title set larger than the content it introduces is hierarchy, not a
breach; ink is the measure here, not size.

## 2 · The field orients, it never packages

**Statement.** Grid lines, presence, and the cursor exist to say where a person
is; they never frame, enclose, group, label, or decorate the content sitting on
them.

**Reason.** A field that draws containers around content becomes a card layout,
and then position stops carrying meaning.

**Test.** Turn grid lines off — every placement, footprint, preview, selection,
and presence cell must be pixel-identical, and nothing may lose its grouping.

## 3 · Chrome answers the hand and leaves

**Statement.** Chrome is absent at rest and arrives when it is called for.
`10-grammar/States.md` owns how Rest and Approached are drawn, and it is not
restated here.

**Reason.** Chrome that never leaves charges the whole Grid, permanently,
for an occasional action.

**Test.** Take the pointer off the Grid and wait: anything still drawn over
the field that is not content, grid lines, presence, or the cursor is a defect.

## 4 · Distance simplifies, it never subtracts

**Statement.** As the camera pulls back a placement sheds chrome first, then
surface treatment, then detail, while keeping its true position, its exact
cells, and a recognisable kind — nothing is hidden, moved, merged, counted, or
summarised.

**Reason.** The camera is a lens, so pulling back changes how much is seen and
never what exists.

**Test.** Compare the Grid at working zoom and at the furthest zoom: the
number of placements, their positions, and their kinds must match, with no
cluster bubble, count badge, or empty cell anywhere.

## 5 · Nothing loops

**Statement.** Every animation is a single transition that starts on an event
and ends; nothing pulses, blinks, breathes, spins, marches, or repeats while
waiting, including placement previews and anything not yet arrived.

**Reason.** A repeating animation claims attention forever for information that
was fully delivered the first time.

**Test.** Record an untouched screen for thirty seconds with nothing pending —
a correct screen is a still image. A single arrival swap over `--d-swap` is a
transition, not a loop.

## 6 · Colour is never the only carrier

**Statement.** No meaning is carried by hue alone.
`10-grammar/States.md` fixes the carrier rule and it is not restated here.

**Reason.** A Grid whose meaning collapses in greyscale is unreadable to a
large minority of the people using it.

**Test.** View the frame in greyscale: every distinction nameable in colour must
still be nameable, and every refusal must still read.

## 7 · The Grid stays lit, and nothing floats free

**Statement.** No surface dims, blurs, tints, or blocks the Grid behind it,
and no surface sits detached from the thing it serves; chrome fills are opaque
over their own footprint and nothing more. A surface's own separation shadow —
`--shadow-local` — is not a dim.

**Reason.** Grove has no modal because a person is never doing only one thing,
and a scrim is an assertion that they are.

**Test.** With any surface open, read and click a placement outside it — if
anything outside the surface is unreadable, blurred, or inert, the surface is
a modal.

A surface a person summons by key to capture or review, and which serves the
whole Grid rather than one placement, may be fixed to the viewport; it is
still opaque only over itself, still dims nothing, and still blocks nothing.
Every other surface sits beside its source, inside the surface that asked, or
in the composed arrangement that owns it.

## 8 · Footprints are whole cells

**Statement.** Every placement occupies a whole number of cells on both axes and
every footprint edge lands on a major grid line; there is no half cell, no
sub-cell offset, no snap-to-screen, and no minimum screen size.

**Reason.** The cell is the unit of position, presence, hit-testing, and
selection, so a placement not measured in cells cannot be reasoned about by any
of them.

**Test.** At any zoom, follow a placement's four edges — each must land on a
major line, and its stand-in at distance must cover the same cells.

Whole cells is the law; square is not. A Note's footprint is the smallest
whole-cell square that holds its text; every other kind takes the whole-cell
rectangle its content needs, because forcing a square would crop a picture and
break Law 9.

## 9 · Authored content is never made to fit

**Statement.** Authored text, pictures, and pages are never truncated,
ellipsized, cropped, cover-fitted, set below their reading size, or scrolled
inside their own footprint; when content outgrows its footprint, the footprint
grows.

**Reason.** A container that hides part of what a person wrote makes reading a
second step and makes the Grid lie about what is present.

**Test.** Look inside a placement for an ellipsis, a scrollbar, a cropped edge,
or type smaller than the reading size — any one of them fails.

A composed surface may scroll and a reading page may paginate, because a Slate
is chrome and a page is a page; neither is a footprint on the field, and neither
may shorten, summarise, or shrink what it carries.

## When two laws collide

| Collision | Ruling |
| --- | --- |
| Content outgrows a whole-cell footprint (8 vs 9) | The footprint grows to the next whole cell; type size never changes. |
| A picture's proportions do not fill a square (8 vs 9) | The footprint takes the whole-cell rectangle nearest the frame's proportions; the picture is never cropped. |
| An action needs chrome, content must stay loudest (1 vs 3) | Chrome fades in at approach, is drawn outside the content edge, and never restyles the content's own surface. |
| A placement is too small to draw its detail (1 vs 4) | It becomes a stand-in holding its own cells and its kind; it never disappears and never merges with a neighbour. |
| A surface must be read while work stays live (1 vs 7) | The surface is opaque over its own footprint only; everything outside stays at full brightness and stays live. |
| A state must be seen at once, nothing may loop (5 vs 6) | The settled frame carries the state in at least two of hue, structure, position, and words; motion only delivers it there. |
| The field would clarify by grouping content (2 vs 4) | Presence brightens the cells and nothing else; grouping is never drawn as an enclosure or a count. |

## Applying the laws

- A law binds the specification, the deck that renders it, and the build. Where
  a deck and a law disagree, the deck is corrected, because a rendering is
  evidence and never authority.
- A law is never satisfied by a preference, a setting, or a mode. An option that
  turns a law off is a defect in the design that needed the option.
- Where this system is silent, the reader resolves in the order the README sets
  and writes the resolution where the rule lives, with the reason on the same
  line. A resolution may narrow a law and may never weaken one.
- `10-grammar/States.md` owns how the nine states are drawn, and
  `00-foundations/Tokens.md` owns every value these laws are expressed in;
  neither is restated here, and neither may contradict a law.
