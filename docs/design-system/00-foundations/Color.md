---
type: design-system-foundation
status: active
date: 2026-08-09
tags: [grove, design-system, color]
---

# Colour

How colour is chosen and combined. `00-foundations/Tokens.md` holds the
values; this document holds the rules that decide which one is correct.

Grove's palette is small on purpose. Near-black spatial surfaces, one warm
high-contrast ink, one paper pair, four signals, five role hues, and three
authored Note colours. Nothing else exists, and adding an accent for a single
screen fails review.

## The two worlds

Grove has a spatial world and a print world, and a surface belongs to exactly
one.

**Spatial surfaces** are the Grid and its chrome: `--surface-grid`
for the field, `--surface-chrome` for anything composed over it, and
`--surface-nested` for a surface inside another surface. They carry `--ink`.

**Print surfaces** are where reading and writing happen: `--surface-page`
carrying `--paper-ink`. A Document on the Grid, an annotation edition, and a
writing surface are all paper, and paper is what makes their kind legible
before a word is read.

The rule that decides: **if a person reads continuous prose or writes it, the
surface is paper.** If they orient, select, arrange, or operate, the surface
is spatial. A surface that tries to be both is the defect that produces dark
reading panes.

There are exactly three dark steps. A fourth is a defect: depth in Grove comes
from tonal separation, restrained borders, and position, and a fourth step
means a surface has been stacked where it should have been placed.

## The four signals

Four signals, four jobs, no overlap. A signal used for a second job has
stopped being a signal, because a person can no longer read it without
context.

| Signal | Means | Never means |
| --- | --- | --- |
| `--signal-interaction` | This is what you are working on. | A category, a warning, a result, a link, a brand accent. |
| `--signal-active-work` | A gesture is open right now. | A result, a warning, a category, a highlight. |
| `--signal-refusal` | This cannot happen. | Danger, severity, deletion, emphasis, a required field. |
| `--signal-authored-context` | Authored context is present. | A colour a person picked, a tag, a group. |

`10-grammar/Signal-roles.md` owns how each is drawn and when it hands over to
another. Two rules belong here because they are about colour itself:

- **A signal never appears without its second carrier.** Refusal is hue plus
  hatching plus a sentence; selection is hue plus an outline outside the edge
  plus brightened cells. `00-foundations/Accessibility.md` holds the audit.
- **A signal is never a surface fill.** Signals outline, mark, and tint the
  field; a surface filled with a signal colour turns a transient state into a
  place.

## Role hues

Five hues identify operation families: Tool, View, Layer, Edit, and Slate.
They tell a person which kind of operation a surface belongs to, and they are
signals about the interface, never categories a person authors.

Each hue has a fill and a border value, and the pairing is fixed: the fill
identifies, the border contains. Use the fill for a header identity mark, a
key cap, or a one-line accent; use the border for the containment edge of the
surface that hue belongs to. Never fill a whole surface with a role hue — the
surface is `--surface-chrome`, and the hue is how it identifies itself.

A role hue never carries state. A Slate that is focused, refused, or busy says
so with a signal, not by shifting its identity hue.

## Authored Note colours

A Note carries one of three fills. Three, not a palette, so a colour stays
something a person can remember and mean.

The fill is identity, and identity is what the Note radiates: the presence a
Note casts into the cells around it is **its own fill**, never a brightened
alias of it. A Note may not radiate the Anchor or Invalid triplets, because a
signal that also means "someone picked this colour" is no longer readable as a
signal. This resolves a disagreement in the catalogue in favour of the
whole-field rendering, which casts `--c-note-violet` as its own value.

A Note's text is `#F4F4F2` on all three fills, and every pairing clears the
contrast floor. A fourth Note colour is legal only if its text pairing clears
4.5:1 and the fill clears 3:1 against `--surface-grid`.

Colour is never meaning a person did not author. A shared cell in the field
means two things sit near each other and nothing more — no link, no grouping,
no relationship is implied, and none is stored.

## Choosing a colour

In order, stop at the first that applies:

1. Is this text or an edge on a known surface? Take the step from the ink ramp
   that `00-foundations/Accessibility.md` permits for that use.
2. Is this one of the four things a signal means? Use that signal, with its
   second carrier.
3. Does this identify an operation family? Use that role hue, fill or border
   as the pairing requires.
4. Is this a Note a person coloured? Use their choice, and radiate it.
5. Otherwise it has no colour. Use ink, space, and position.

There is no sixth branch. A need that reaches the end of this list is a design
that has not decided what it is saying.

## Legal pairings

Reading text: `--text-primary` on any spatial surface, `--text-page` on paper.
Supporting text: `--text-secondary`. Metadata: `--text-meta`, which sits at
the contrast floor and is the quietest ink legal for text. Unavailable text:
`--text-unavailable`, which is exempt from the floor and always pairs with a
border and a retained position.

Borders: `--edge-hairline` separates groups inside one surface,
`--edge-quiet` is the default containment edge, `--edge-found` is an edge a
person must be able to locate. None of the three may carry meaning on its own;
a meaning-bearing edge uses a signal.

## Refusals

- **No new accent.** A screen that needs a colour the system does not have is
  a screen that has not identified what it is communicating.
- **No hue as the only carrier.** Every state pairs hue with structure,
  position, or words.
- **No hue for relationships a person did not author.** Proximity in the field
  is not a group, and it is never coloured as one.
- **No signal as a surface fill.** A transient state may not become a place.
- **No role hue carrying state.** Identity and state are separate axes.
- **No gradient on a product surface.** Grove's surfaces are flat fills; a
  gradient implies a light source the Grid does not have.
- **No colour-only distinction between two kinds of content.** Kind is legible
  from form — paper against authored fill, display title against plain text —
  before colour is read at all.
