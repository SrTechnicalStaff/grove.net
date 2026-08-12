---
type: design-system-grammar
status: normative
date: 2026-08-11
owner: "Grove Design System"
---

# Content concentration

## Definitions

| Symbol | Definition |
| --- | --- |
| `Tᵢ` | Rendered text demand for source `i`. |
| `Iᵢ` | Complete-frame image demand for source `i`. |
| `Oᵢ` | Reserved future-media demand for source `i`; `0` in the initial media boundary. |
| `T` | `ΣTᵢ`. |
| `I` | `ΣIᵢ`. |
| `O` | `ΣOᵢ`. |
| `D` | `T + I + O`; source mass. |
| `textShare` | `T / D`. |
| `imageShare` | `I / D`. |
| `otherShare` | `O / D`. |
| `Rmedia` | Page-local image area divided by image area plus text area. It composes a routed page; it does not re-route the source set. |

## Text demand fixture

| Input | Rule |
| --- | --- |
| Measure | Readable text measure in pixels. |
| Font size | Readable body size in pixels. |
| Line height | Reading line-height multiplier. |
| Body lines | Wrapped body lines at the declared measure and type. |
| Heading lines | Wrapped title and source-heading lines. |
| Blank units | Explicit blank-line units. |
| Paragraph cost | `0.65 × paragraphBreakCount`. |

```text
charsPerLine = max(8, floor(measurePx / max(6, 0.52 × fontSizePx)))
lineCount = bodyLines + headingLines + blankUnits
paragraphCost = 0.65 × paragraphBreakCount
Tᵢ = max(0.25, (lineCount + paragraphCost) / linesPerPage)
renderedHeightᵢ = (lineCount + paragraphCost) × fontSizePx × lineHeight
renderedAreaᵢ = renderedHeightᵢ × measurePx
```

| Demand input | Forbidden substitution |
| --- | --- |
| Wrapped text geometry | Raw character count alone. |
| Rendered area | File byte size. |
| Readable measure | Current container width without page rules. |
| Paragraph and heading structure | Storage serialization shape. |

The values `measurePx = 340`, `fontSizePx = 16`, `lineHeight = 1.55`, and
`linesPerPage = 18` are v0.2.0 normative design-system inputs. They are not
user controls. Physical page geometry scales the resulting composition but does
not replace the demand equations.

## Image demand fixture

| Input | Rule |
| --- | --- |
| `w`, `h` | Intrinsic source pixel width and height. |
| `r` | `w / h`. |
| `F` | Fidelity floor: `96` thumbnail, `180` supporting, `320` hero. |
| Role | `hero = +0.35`; `supporting = 0`; `thumbnail = −0.18`. |
| GIF | Same area and fidelity calculation as Image; animation changes playback, not frame demand. |

```text
pixelReserve = clamp(0.70, 1.50, 0.18 × √(w × h) / F)
aspectReserve = clamp(0.00, 0.45, 0.12 × |ln(r)|)
roleReserve = +0.35 hero · 0 supporting · −0.18 thumbnail
Iᵢ = max(0.45, pixelReserve + aspectReserve + roleReserve)
```

```text
PPI_eff = min(px_w / placed_w_in, px_h / placed_h_in)
```

| Effective resolution | Allowed role |
| --- | --- |
| `PPI_eff ≥ 300` | Full measure, leading frame, page opener, or reveal. |
| `150 ≤ PPI_eff < 300` | Smaller complete frame; no dominant or full-bleed emphasis. |
| `PPI_eff < 150` | Small supporting frame or no page frame; Image Viewer route remains available. |

## Source mass and shares

```text
D = T + I + O
textShare = T / D
imageShare = I / D
otherShare = O / D
```

| Rule | Contract |
| --- | --- |
| Demand basis | Normalized rendered layout demand. |
| Item count | Pagination input only; never a demand substitute. |
| Source bytes | Never a demand input. |
| Thumbnail size | Never a demand input. |
| Complete frame | Required for every image placement. |
| Unsupported media | `O = 0` in the initial supported-media contract; no future-media behavior is inferred. |

## Qualification

| Condition | Result |
| --- | --- |
| `D < 1.0` | Below gate; no Annotation. |
| `D ≥ 1.0` and `supportedCellCount ≥ 24` of `32` | Qualified Annotation. |
| `D ≥ 1.0` and one long Document | Qualified under the single-Document exception; no item-count test. |
| Unavailable demand | Preserve source identity, provenance, and an explicit recovery frame; do not drop or silently zero the source. |

The field gate uses a `1920 × 1080` calibration surface, `220 × 220` cells,
`8 × 4 = 32` fully fitting cells, and per-cell support coverage `≥ 0.50`.
These are calibration fixtures pending fixture-backed threshold review.

## Vertical routing

| Condition | Vertical |
| --- | --- |
| `imageShare ≤ 0.25` and `otherShare = 0` | Text-led. |
| `0.25 < imageShare < 0.70` and `otherShare = 0` | Mixed-media. |
| `imageShare ≥ 0.70` and `otherShare = 0` | Image-led. |
| Share unavailable | Apply `90-conformance/Annotation decisions.md` AN-D03–AN-D04; retain the committed form after qualification and create no candidate before qualification. |

`0.25` belongs to Text-led. `0.70` belongs to Image-led. The mixed-media
interval is open at both boundaries.

## Tier routing

| Tier | `D` band |
| --- | --- |
| Tier 01 | `1.0 ≤ D < 1.75`. |
| Tier 02 | `1.75 ≤ D < 3.0`. |
| Tier 03 | `D ≥ 3.0`. |

## Form routing

| Vertical | Tier 01 | Tier 02 | Tier 03 |
| --- | --- | --- | --- |
| Text-led | Bulletin | Berliner / compact | Broadsheet |
| Mixed-media | Brochure | Pamphlet | Magazine |
| Image-led | Gallery | Contact sheet | Image edition |

## Page-local composition

| Rule | Contract |
| --- | --- |
| Set-level ratio | Chooses the vertical once after qualification. |
| Page-level `Rmedia` | Chooses a composition inside the routed form. |
| Page-level re-routing | Forbidden. |
| Narrow usable width | Changes columns, page count, or horizontal advance; preserves landscape reading unit. |
| Text overflow | Thread to the next column/page at the same readable measure. |
| Image overflow | Reduce complete frame, change region/family, or add page; never crop or stretch. |
| Missing caption | Remove the caption line; do not reserve an empty caption slot. |
| Missing title | Remove the title line; do not synthesize one. |
| Missing provenance | Preserve the known provenance fields; do not invent a source label. |

## Re-resolution

| Trigger | Re-resolve |
| --- | --- |
| Source add/remove | Source set, demand, shares, gate, vertical, tier, form, pages. |
| Text edit | Wrapped lines, demand, tier, family, pages. |
| Image replacement | Dimensions, aspect, role, fidelity, demand, family, pages. |
| GIF playback state | Playback only; not demand. |
| Source availability | Recovery and page frame; source position remains. |
| Usable width change | Page geometry, columns, family, pages. |
| Boundary crossing | Form or page resolution at the same frame; no partial edition. |

## Boundary stability

| Boundary | Numeric rule |
| --- | --- |
| Gate, tier, vertical | Accepted boundary values are listed above. |
| Promote/demote hysteresis | Equality retains the current form; a complete-fit evaluation on the other side of the boundary is required. Apply `90-conformance/Annotation decisions.md` AN-D02. |
| Last-good edition | Remains visible while re-resolution is pending or fails. |
| Current source | Retained when the new edition can carry it. |

## Refusals

| Refused behavior | Required result |
| --- | --- |
| Shrink type below reading floor | Add columns/pages, change family, or promote. |
| Crop or stretch frame | Preserve complete frame; change geometry or add page. |
| Truncate, summarize, or reorder source | Preserve complete ordered source. |
| Invent title, caption, masthead, issue number, or source label | Omit absent metadata. |
| Treat file bytes as demand | Recalculate from rendered text or intrinsic image facts. |
| Re-route from a page-local ratio | Keep the set-level vertical and tier. |

## Sources

- `docs/reference/Content concentration and layout model.md`
- `docs/discovery/Annotation demand and vertical routing.md`
- `docs/decisions/Content concentration and layout rules.md`
- `docs/decisions/Annotation field saturation gate.md`
- `docs/decisions/Annotation vertical tiers and landscape forms.md`
- `docs/design_catalogue/src/information-plane/05-annotation-mixed-media.html`
- `docs/design_catalogue/src/information-plane/06-annotation-text-led.html`
- `docs/design_catalogue/src/information-plane/07-annotation-image-led.html`
- `docs/design-system/10-grammar/Annotation-ledger.md`
