---
type: design-system-foundation
status: active
date: 2026-08-09
tags: [grove, design-system, space-and-grid]
---

# Space and the cell

Grove measures in two units and never mixes them. Chrome is measured in screen
pixels on the five-step spacing scale. The field is measured in cells. A chrome
value expressed in cells, or a distance on the field expressed in pixels, is a
defect.

`Tokens.md` holds every figure named here. This page holds the rules for
choosing between them.

## Two measures

| Measure | Unit | Governs | Under zoom |
| --- | --- | --- | --- |
| Spacing scale | Screen pixels, five steps | Rhythm inside and between chrome surfaces | Fixed. Chrome never scales with the camera. |
| Cell | `--grid-cell` in world pixels | Every footprint, every field value, the cursor, every cell-bound preview | Scales exactly with the camera. |

Chrome that floats over the field still uses the spacing scale, because it
belongs to a surface and not to the field beneath it.

## The spacing scale

| Step | Use it for | Never |
| --- | --- | --- |
| `--sp-xs` | Inside one control: a label and the mark, count, or key hint that belongs to it. | Separating two controls. |
| `--sp-sm` | Between controls in one group; between a label and the field it names; gutters between frames in a collection. | Between content and a control that acts on it. |
| `--sp-md` | Padding of a local surface; between two stacked groups of the same kind in one surface. | Padding of a composed Slate. |
| `--sp-lg` | Padding of a composed Slate; separation between authored content and any control that acts on it; clear space on the content-facing side of the single primary action. | Inside a control group. |
| `--sp-xl` | Between two unrelated regions of one surface. | Any separation a border or hairline already makes. |

A spacing value that is not one of the five steps is a defect. The single
exception is a component's own anatomy figure, which lives in that component's
specification because no other component shares it.

Padding steps down one level at each depth: a surface padded `--sp-lg` holds
groups separated by `--sp-md` whose controls sit `--sp-sm` apart. The corpus is
silent on nesting, and this is the established desktop convention that lets
depth read without adding a border at every level.

Reading surfaces are outside this scale. Page margins, columns, gutters, and
baseline rhythm are set as page mechanics in the reading surface's own
specification, because print mechanics govern reading surfaces.

## Separation and grouping

| Between | Step |
| --- | --- |
| Two controls in one group | `--sp-sm` |
| A quiet action and the primary action beside it | `--sp-sm` |
| One group and the next group | `--sp-md` |
| Authored content and the controls that act on it | `--sp-lg` |
| The primary action and the content it acts on | `--sp-lg` |
| Two unrelated regions of one surface | `--sp-xl` |

Separation is generous where a person reads and compact where a person aims:
content and controls are `--sp-lg` apart so authored text is never crowded by
chrome, and controls inside one group are `--sp-sm` apart so the group reads as
one thing. A group that needs more than `--sp-sm` inside it is two groups.

Empty space is the only grouping device Grove has at this level. Reaching for a
box, a tint, or a rule to make a group cohere is a sign the spacing is wrong.

## The cell

The cell is the unit of every footprint. There are no half cells and no sub-cell
offsets.

- A footprint is a whole number of cells on each axis, minimum `1 × 1`.
- Every footprint edge lands on a major line. A placement never straddles one.
- The field has no gutter. Adjacent placements share a major line with no gap,
  and separation between placements is counted in empty cells.
- Nothing on the field reflows. A placement moves only when a person moves it,
  so returning to a remembered position always works.
- Content on the field carries `--r-none`; a rounded corner on the field would
  make a placement read as a card.

Everything cell-bound is drawn from the same geometry: the cursor is exactly one
cell, a placement preview covers the exact cells the content will occupy,
presence is one value per cell with hard edges, and the field response under a
selection brightens whole cells. No one of these has a geometry of its own.

| Form | Footprint |
| --- | --- |
| Note | `1 × 1`. |
| Document | Resolved from readable text demand, clamped to `2 × 2` minimum and `8 × 8` maximum; a `1 × 1` Document is invalid (D-GRID-12). |
| Image | An integral footprint derived from source dimensions; the complete frame is shown without cropping or shrinking (D-GRID-09). |

A placed Document's page texture repeats the cell rhythm in world units, so the
page and the field stay in step at every zoom: the minor pitch at
`--paper-texture-minor` and the major pitch at `--paper-texture-major`.

## Line hierarchy

Three tiers, two inks, one system.

| Tier | Pitch | Weight | Ink |
| --- | --- | --- | --- |
| Minor | `--grid-cell` ÷ `--grid-subdivisions` — one fifth of a cell | 1 screen pixel | `--grid-minor-ink` |
| Major | One cell — the working rhythm, and the line every footprint edge lands on | 1 screen pixel | `--grid-major-ink` |
| Supercell | `--grid-supercell` cells | 1 screen pixel | `--grid-major-ink` |

- A supercell line is not firmer, thicker, or brighter than a major line. Only
  its survival at distance distinguishes it, because its spacing is the last to
  collapse.
- Line weight is one device-independent pixel at every zoom and never thickens
  with the camera; the corpus is silent, and a hairline that holds one pixel is
  the established convention for rules.
- There is no fourth tier and no second grid.
- Only major lines are snap targets. Minor lines are a reading aid, because the
  cell is the unit of every footprint.
- A supercell scopes nothing. It is a counting aid, not a container, a page, or
  a selection boundary — the corpus assigns it no behaviour, and giving it one
  would create a second unit of footprint.
- The origin cell is drawn exactly like every other cell. No axis, no highlight,
  no marked centre, because a marked origin privileges one place on a field
  where every place is addressable.
- The field carries no copy at any zoom. Orientation lives in geometry and ink.

## The fade rule

One rule sets all three tiers. There are no per-tier constants.

```text
spacing = pitch_world × zoom                  // device-independent screen pixels
ink     = clamp((spacing − 6) ÷ 8, 0, 1)      // --grid-fade-start 6, --grid-fade-end 14
alpha   = ink                                 // multiplied into that tier's own ink
```

- A tier is invisible at or below `--grid-fade-start` and at full ink at or
  above `--grid-fade-end`.
- The fade is continuous across the whole zoom range. There are no zoom steps
  and no snap-to-zoom levels, so no tier ever pops.
- Because the tiers sit `--grid-supercell` apart, one tier arrives at the
  spacing another is leaving; the rhythm on screen repeats while the Grid
  in view multiplies.
- Below `1.27%` zoom the supercell tier carries the field alone and dims toward
  `0.63` at the `1%` floor. It never reaches zero inside the product zoom range,
  so the field is never a blank plane.

## Zoom, spacing, and ink

Pitches at `100%`: minor `44px`, major `220px`, supercell `1100px`.

| Zoom | Minor | Ink | Major | Ink | Supercell | Ink |
| --- | --- | --- | --- | --- | --- | --- |
| 1000% | 440.0px | 1.00 | 2200.0px | 1.00 | 11000.0px | 1.00 |
| 400% | 176.0px | 1.00 | 880.0px | 1.00 | 4400.0px | 1.00 |
| 100% | 44.0px | 1.00 | 220.0px | 1.00 | 1100.0px | 1.00 |
| 40% | 17.6px | 1.00 | 88.0px | 1.00 | 440.0px | 1.00 |
| 31.82% | 14.0px | 1.00 | 70.0px | 1.00 | 350.0px | 1.00 |
| 25% | 11.0px | 0.63 | 55.0px | 1.00 | 275.0px | 1.00 |
| 20% | 8.8px | 0.35 | 44.0px | 1.00 | 220.0px | 1.00 |
| 13.64% | 6.0px | 0.00 | 30.0px | 1.00 | 150.0px | 1.00 |
| 10% | 4.4px | 0.00 | 22.0px | 1.00 | 110.0px | 1.00 |
| 6.36% | 2.8px | 0.00 | 14.0px | 1.00 | 70.0px | 1.00 |
| 4% | 1.8px | 0.00 | 8.8px | 0.35 | 44.0px | 1.00 |
| 2.73% | 1.2px | 0.00 | 6.0px | 0.00 | 30.0px | 1.00 |
| 2% | 0.9px | 0.00 | 4.4px | 0.00 | 22.0px | 1.00 |
| 1.27% | 0.6px | 0.00 | 2.8px | 0.00 | 14.0px | 1.00 |
| 1% | 0.4px | 0.00 | 2.2px | 0.00 | 11.0px | 0.63 |

The five boundaries the curve produces:

| Zoom | What happens |
| --- | --- |
| 31.82% | Minor reaches full ink. |
| 13.64% | Minor reaches zero. |
| 6.36% | Major reaches full ink. |
| 2.73% | Major reaches zero. |
| 1.27% | Supercell reaches full ink. |

The major column is the projected cell size. It is the same figure the
representation thresholds read, so line ink and representation are computed from
one number rather than two — `10-grammar/Representation-tiers.md` owns what that
figure does to placements.

## Orientation, not containment

The lines say where. They never say what belongs with what.

- Nothing on the field is given a border, a shadow, or a radius because it sits
  on the field. Content carries its own frame or none.
- No per-item chrome persists at rest. Chrome that serves the hand arrives on
  approach and leaves with the pointer.
- No uniform tiles. A footprint is the size the content needs in whole cells,
  never a fixed cell count applied for visual evenness.
- No chrome parks on the field. A bar or palette that never leaves has turned
  the Grid into an application window.
- Adjacency means co-location and nothing else. Neighbouring cells imply no
  group, no link, and no order, and none is stored.
- A run of empty cells is a statement of separation a person made. Nothing
  collapses it, closes it, or auto-arranges around it.

## Line visibility

Line visibility is one view preference, toggled with `G` on the field
(D-GRID-11).

| Property | Rule |
| --- | --- |
| Scope | One preference per person, applied wherever they are. |
| Storage | Beside the view record. Never on a Layer, a Placement, or a Content payload. |
| Recovery | An unreadable stored value returns to visible and is repaired. |
| Granularity | All three tiers together. There is no per-tier toggle. |
| Relationship to fade | Visibility multiplies the computed ink to zero; it never replaces the curve, so turning lines back on shows exactly the tiers the current zoom produces. |

Hiding lines removes no behaviour and unlocks none. It is not a mode, and it
grants no free placement, no sub-cell position, and no second geometry. With
lines hidden, every one of these is pixel-identical to lines shown:

- Snapping and every footprint edge.
- Placement, move, and resize previews, and refusal hatching.
- The selection outline and the field response beneath it.
- The cursor, its whole-cell edges, and its trail.
- Presence, at every cell value and every hue.
- Anchored marks and ribbons.

The Grid carries no on-screen indicator of the preference; the state is
legible from the field itself, and the control appears wherever the Grid
controls are listed. Its two labels are `Show lines` and `Hide lines`.

## Ruling on two disagreements

**Grid ink token names.** `css/tokens.css` declares `--c-grid-min` and
`--c-grid-maj` and omits the fade band and the subdivision counts entirely; the
token table declares `--grid-minor-ink`, `--grid-major-ink`,
`--grid-subdivisions`, `--grid-supercell`, `--grid-fade-start`, and
`--grid-fade-end`. The table's names win and the projection is regenerated
against it, because the projection is generated from the table and never edited
to disagree with it.

**Where the visibility preference lives.** The catalogue records it as persisted
per person and not per place; the decision record records it as persisting with
the camera view record. Both hold: there is one view record per person, so the
preference is single, applies everywhere, and is never stored per Layer — a
per-Layer preference was an explicitly rejected alternative.
