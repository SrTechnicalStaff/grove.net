---
type: design-system-foundation
status: normative
date: 2026-08-10
owner: "Grove Design System"
tags: [grove, design-system, grid, expression]
---

# Grid expression

## Canonical field grammar

| Primitive | Canonical value | Constraint |
| --- | --- | --- |
| World cell | `220px` at `100%` camera scale | Every Placement footprint, cursor head, preview, and field cell uses whole cells. |
| Minor pitch | `44px` at `100%` | `--grid-cell / --grid-subdivisions`; one fifth of a cell. |
| Major pitch | `220px` at `100%` | One cell; every Placement edge lands here. |
| Supercell pitch | `1100px` at `100%` | Five cells; orientation only. |
| Pitch ratio | `5:1` | Minor → major → supercell. |
| Line weight | `1px` device-independent | Constant at every camera scale. |
| Line inks | `--grid-minor-ink`, `--grid-major-ink` | Minor uses the minor ink; major and supercell use the major ink. |
| Axes | None | No origin marker, axis line, ruler, coordinate label, or centre mark. |
| Pointer handling | None | Grid lines do not receive pointer input. |
| Copy on field | None | Orientation is geometry and ink, never text. |

## Line visibility and fade

| Rule | Value |
| --- | --- |
| Projected spacing | `spacing = pitchWorld × cameraScale` |
| Ink factor | `clamp((spacing − 6px) / 8px, 0, 1)` |
| Fade start | `6px`; tier is invisible at or below this spacing. |
| Fade end | `14px`; tier is full ink at or above this spacing. |
| Transition | Continuous; no zoom steps and no tier pop. |
| Simultaneous fades | At most one tier may be between `0` and `1` ink. |
| Visibility preference | `G`; all three tiers are hidden or shown together. |
| Hidden state | Computed ink is multiplied by `0`; geometry and all other behavior remain unchanged. |
| Persistence | One view preference; never a Placement, Layer, or Content property. |

## Field composition

| Layer | Drawn representation |
| --- | --- |
| Current Layer | Full authored Content, Placement geometry, marks, cursor, selection, preview, and refusal state. |
| Other reachable Layers | Presence cells only; source hue and cell geometry retained; no frame, text, badge, or readable Content. |
| Unreachable Layers | No contribution. |

| Presence invariant | Canonical value |
| --- | --- |
| Unit | One value per whole cell. |
| Boundary | Hard cell edges; no gradient crossing a cell boundary. |
| Accumulation | Hue and value accumulate by contributing source and Layer. |
| Meaning | Proximity and density only; never a relationship, group, count, category, or route. |
| Maximum | `--field-alpha-max`; presence never outshines authored Content. |
| Perimeter | `--field-perimeter-width` on the outer edges only; no interior edge. |
| Effects refused | Gradient, halo, blur, glow, rounded cell, continuous heatmap, and per-item field container. |

## Content expression

| Content kind | Resting expression | Distance expression | Required distinction |
| --- | --- | --- | --- |
| Note | Authored fill, complete text, square whole-cell footprint. | Authored fill and Note mark; text sheds at the stepped tier. | The fill is the person's authored choice. |
| Document | Paper surface, complete page, title/body/provenance hierarchy. | Paper stand-in with Document mark; footprint remains exact. | Paper is not a generic card. |
| Picture | Complete intrinsic frame; no crop and no forced square. | Paper-toned Picture stand-in; footprint remains exact. | Image aspect ratio and animation remain identifiable. |
| Annotation Blip | `14px` round point marker with `1px` ring and `3px` core inset. | Fixed screen-space point marker; no camera scaling. | A Blip is a qualifying reading cue, not placed Content. |
| Layer watermark | `208px` numeral, display name beneath, right `--sp-xl`, bottom `--sp-md`. | Camera-independent; no fill, border, shadow, or animation. | Layer identity is ambient, never a dashboard. |

## Interaction expression

| Interaction | Canonical result |
| --- | --- |
| Move pointer over field | Grid cursor occupies exactly one cell; no text label appears. |
| Move pointer across cells | Cursor trail records whole-cell history and decays; no sub-cell movement. |
| Place, move, or resize | Preview covers the exact prospective footprint; edges remain on major lines. |
| Invalid placement | Refusal hue and refusal hatch cover only the invalid cells; source and field remain unchanged. |
| Select | Selection ring is outside the Placement edge; no count, card, or persistent toolbar appears. |
| Camera move | Grid, Content, Presence, cursor, and marks remain cell-addressed and camera-scaled. |
| Camera pullback | Representation sheds by projected cell size; Placement identity, position, and footprint remain. |
| Grid idle | Only Content, lines, Presence, cursor, and permitted authored marks are visible. |

## State expression

The Grid uses the shared nine-state vocabulary. A state may add only the mark
specified by the component owner.

| State | Grid expression |
| --- | --- |
| Rest | Authored Content and ambient field; no per-item chrome. |
| Approached | Placement-owned resize/edit affordance may appear; field and footprint do not change. |
| Focused | Keyboard focus is the Grid cursor or the focused surface; no duplicate placement ring. |
| Selected | Selection outline and selected perimeter; no count or card chrome. |
| Engaged | Active pointer operation and preview; commit has not occurred. |
| Pending | Existing footprint remains; representation may show pending without becoming blank. |
| Refused | Refusal hatch, refusal role, and plain sentence at the attempted footprint; no partial write. |
| Unavailable | Action remains in place with unavailable ink; placement geometry and source identity remain. |
| Anchored | Anchor ribbon or diamond outside the authored footprint; no footprint change. |

## Visual exclusions

| Refused pattern | Reason |
| --- | --- |
| Uniform card per Placement | Replaces spatial composition with a card catalogue. |
| Persistent toolbar, palette, or utility rail on the field | Converts the Grid into application chrome. |
| Single uniform line tier | Removes scale, orientation, and the authored 5:1 rhythm. |
| Labels, coordinates, axes, or badges on empty field | Replaces spatial discovery with dashboard annotation. |
| Gradient field, glow, halo, or softened cell edge | Removes cell ownership and Layer accumulation. |
| Aggregated bubble, count, or cluster marker | Infers a relationship the field does not own. |
| Generic rounded card for Note, Document, or Picture | Erases the distinct authored, paper, and intrinsic-frame forms. |
| Fixed-screen Content while the camera moves | Breaks cell ownership and remembered placement. |

## Source basis

| Source | Contract retained |
| --- | --- |
| `docs/design_catalogue/src/grid-plane/01-the-grid.html` | Three pitches, two inks, one line system, cursor, visibility, and field restraint. |
| `docs/design_catalogue/src/grid-plane/02-note.html` | Authored Note fill, whole-cell form, text-led identity, and authored-context mark. |
| `docs/design_catalogue/src/grid-plane/03-document.html` | Paper page, reading hierarchy, provenance, and complete page expression. |
| `docs/design_catalogue/src/grid-plane/04-image.html` | Complete intrinsic image frame, aspect ratio, animation, and image identity. |
| `docs/design_catalogue/src/grid-plane/05-presence-fields.html` | Hard-edged per-cell presence and Layer hue accumulation. |
| `docs/design_catalogue/src/grid-plane/06-distance.html` | Projected-cell thresholds, exact footprint preservation, and representation shedding. |
| `docs/design_catalogue/src/grid-plane/07-selection-placement.html` | Selection, preview, refusal, and no-partial-placement behavior. |
| `docs/design-system/20-planes/Grid-plane.md` | Plane ownership, chrome budget, Layer locality, and cross-plane boundary. |
| `docs/design-system/30-components/Grid-lines.md` | Line pitch, fade, visibility, and projection. |
| `docs/design-system/30-components/Presence.md` | Field formulas, alpha limits, hue accumulation, and perimeter. |
| `docs/design-system/30-components/Stand-in.md` | Distance representation and footprint preservation. |
