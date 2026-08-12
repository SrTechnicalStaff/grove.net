---
type: design-system-component
status: normative
date: 2026-08-10
plane: information
vertical: mixed-media
---

# Mixed-media annotation

## Contract

| Property | Rule |
| --- | --- |
| Vertical | `0.25 < imageShare < 0.70`; `otherShare = 0`. |
| Tier source | Shared `D` bands in `10-grammar/Annotation-ledger.md`. |
| Forms | Brochure, Pamphlet, Magazine. |
| Reading unit | Landscape sheet, spread, or paginated landscape edition. |
| Content priority | Text and images retain their source roles, proportions, identity, and order. |
| Shared shell | `30-components/Annotation.md`. |
| Shared demand | `10-grammar/Content-concentration.md`. |

## Form matrix

| Form | Tier | `D` band | Unit | Capacity mechanism |
| --- | --- | --- | --- | --- |
| Brochure | Tier 01 | `1.0 ≤ D < 1.75` | One complete landscape sheet or folded sheet | Change among B-01–B-04 within one sheet; promote when no brochure family holds the complete set. |
| Pamphlet | Tier 02 | `1.75 ≤ D < 3.0` | Landscape reading spread; folded leaves | Add leaves/pages; retain stable measure and source order. |
| Magazine | Tier 03 | `D ≥ 3.0` | Landscape pages and spreads | Pagination; add complete pages without shrinking or cropping. |

## Template ledger

| Code | Form | Template | Geometry | Selection rule |
| --- | --- | --- | --- | --- |
| B-01 | Brochure | Single panel | One complete sheet; one bounded reading panel; no fold. | One short grouping fits one sheet. |
| B-02 | Brochure | Tri-fold | Two strips; three consecutive panels per strip; two same-direction creases; six reading panels. | Short sequence requires bounded panels. |
| B-03 | Brochure | Gatefold | Two outer panels fold inward to a wider centre; six panels; centre may hold a complete reveal module. | Image or relationship requires wider centre. |
| B-04 | Brochure | Accordion | Six panels; alternating inward/outward creases; ordered sequence; no single cover. | Ordered panels without one leading panel. |
| P-01 | Pamphlet | Text leaf | One folio leaf; one stable text measure; no image region unless source image exists. | Text leads and one leaf holds it. |
| P-02 | Pamphlet | Facing leaves | Image and associated text occupy facing leaves; each leaf has own folio and provenance. | Proximity groups image with its explanatory text. |
| P-03 | Pamphlet | Column leaf | One leaf; two stable measures; same measure across the sequence. | Text requires a second measure but not a second leaf. |
| P-04 | Pamphlet | Continuation | Stitched ordered leaves; catalogue range `5–48` pages; source flow remains continuous. | One source exceeds one leaf. |
| M-01 | Magazine | Image opener | One image occupies a dominant complete page; caption/provenance remain conditional. | One image carries the opening claim and fidelity supports the area. |
| M-02 | Magazine | Mixed spread | Image and text occupy distinct facing regions; neither is an inset of the other. | Both media make a real page claim. |
| M-03 | Magazine | Multi-image sequence | Several complete frames at varied sizes; size follows role, aspect, and fidelity; no uniform tile wall. | Several images require pacing across a spread. |
| M-04 | Magazine | Long-run pagination | Ordered threaded pages; up to three text measures per page fixture; source identity repeats at continuation. | Long text or many sources require an edition run. |

The canonical mixed-media catalogue contains twelve selectable templates:
B-01–B-04, P-01–P-04, and M-01–M-04. Unrendered fold names are not additional
templates.

## Shared page geometry

| Measure | Rule |
| --- | --- |
| Unit ratio | Landscape `8:5` at every form and width. |
| Page margin | Shared `Annotation.md` page margin. |
| Text measure | Readable `--measure-reading`; no type reduction to fit. |
| Image frame | Intrinsic aspect ratio; complete edge; resolution-bounded size. |
| Gutter | Explicit page/column gutter; no frame crop to fill remainder. |
| Fold/seam | A crease or seam communicates reading geometry only; no decorative spine. |
| Provenance | Layer and Placement facts remain separate from caption and title. |
| Folio | `n / m` and Layer context at page foot. |

## Local composition inputs

| Input | Effect |
| --- | --- |
| Page-local `Rmedia` | Chooses text/image region balance inside the routed form. |
| Text-column demand | Chooses column count and continuation. |
| Image aspect ratio | Chooses complete-frame region and family. |
| Effective placed resolution | Caps display area and dominant-frame use. |
| Image role | Hero, supporting, or thumbnail reserve. |
| Source count | Pagination and family input; not source mass. |
| Source sequence/proximity | Determines pairing and order; never invents meaning. |
| Available page area | Changes region, columns, or pages; never crop. |

## Text rules

| Rule | Value |
| --- | --- |
| Text flow | Ordered source text threads through its columns/pages. |
| Text size | Fixed readable size from shared page tokens. |
| Baseline | Shared reading baseline across columns and pages. |
| Title | Source title only; no issue title or group headline. |
| Heading | Source-carried heading only. |
| Paragraph | Source order and paragraph boundaries retained. |
| Continuation | Name the source and continuation position; do not summarize. |

## Image rules

| Rule | Value |
| --- | --- |
| Complete frame | Required in every form. |
| Crop | Forbidden. |
| Stretch | Forbidden. |
| Upscale | Forbidden past effective fidelity floor. |
| GIF | Plays at source rate unless reduced-motion policy freezes a complete still frame. |
| Caption | Printed only when source metadata carries caption. |
| Provenance | Printed as source fact; never used as caption substitute. |
| No image | No empty reserved image region. |

## Image quality bands

| `PPI_eff` | Allowed mixed-media treatment |
| --- | --- |
| `≥ 300` | Full measure, leading panel, opener, or reveal. |
| `150–299` | Smaller complete frame; no dominant/full-bleed treatment. |
| `< 150` | Small supporting frame or no page frame; Image Viewer route remains available. |

## Form selection order

| Order | Rule |
| --- | --- |
| 1 | Resolve Mixed-media vertical and shared tier. |
| 2 | Choose an allowed template from source count, page-local balance, text demand, image aspect, role, and fidelity. |
| 3 | Use another template in the same form if it preserves complete sources. |
| 4 | Add pages/leaves when the current template cannot contain the complete source. |
| 5 | Promote to the next mixed-media form when no same-form template can contain the set. |
| 6 | If vertical boundary changes, re-route through the shared ledger; page-local balance never re-routes. |

## States

| State | Mixed-media rule |
| --- | --- |
| Rest | Complete selected template, source order, provenance, captions where present, and folio. |
| Approached | No page restyle; source routes require context gesture. |
| Focused | Focus ring outside control/source; template geometry unchanged. |
| Selected | Outline follows selected source frame/block; page fill and neighboring sources unchanged. |
| Engaged | Route menu opens at source; no page mutation. |
| Pending | Last complete sheet/spread/pages remain while demand or template re-resolves. |
| Refused | One explicit refusal sentence; complete edition remains. |
| Unavailable | Source keeps place, title/provenance, and reserved frame; body/frame carries unavailable state. |
| Anchored | Authored-context mark on source piece only. |

## Reflow and promotion

| Trigger | Result |
| --- | --- |
| Text edit | Recalculate rendered text demand; reflow text; preserve source order. |
| Image replacement | Recalculate dimensions/aspect/fidelity; reselect frame/family if required. |
| Source add/remove | Recalculate `D`, shares, tier, template, and pages. |
| Width change | Retain landscape unit; change region, columns, pages, or horizontal advance. |
| Template full | Select another same-form template; then add pages; then promote. |
| Tier boundary | Re-resolve with AN-D02: equality retains the current form; the target form must pass complete-fit evaluation. |
| Failure | Last complete edition remains; answer line names recovery. |
| Active source | Remains current where the replacement edition contains it. |

## Refusals

| Refused | Required alternative |
| --- | --- |
| Type shrink to save page | Add pages or promote. |
| Frame crop/square-fill | Change frame region, reduce complete frame, or add page. |
| Uniform tiny image wall | Use contact/sequence pagination with readable complete frames. |
| Invented headline/caption/issue | Source metadata only. |
| Empty image/text slot | Close unused region; no placeholder. |
| Source identity merge | Keep title, provenance, and route per source. |
| Scrollable page substitute | Pagination and continuation. |
| Portrait book page | Keep landscape form. |

## Design assertions

| ID | Assertion |
| --- | --- |
| AN-M01 | Exactly three mixed-media forms exist: Brochure, Pamphlet, Magazine. |
| AN-M02 | Exactly twelve selectable catalogue templates exist: four per form. |
| AN-M03 | Brochure is a one-sheet/fold geometry; Pamphlet is a leaf/spread geometry; Magazine is a page/spread/pagination geometry. |
| AN-M04 | Text and image demand are evaluated by the shared equations once. |
| AN-M05 | Page-local `Rmedia` cannot re-route the set. |
| AN-M06 | Complete source frames and ordered text survive every family and reflow. |
| AN-M07 | GIF playback changes motion only, not frame demand. |
| AN-M08 | Image quality caps display area; it never licenses crop or stretch. |
| AN-M09 | Boundary hysteresis and unavailable-source behavior follow `90-conformance/Annotation decisions.md` AN-D02–AN-D04. |

## Sources

| Source class | Source |
| --- | --- |
| Catalogue | `docs/design_catalogue/src/information-plane/05-annotation-mixed-media.html` |
| Raw intent | `docs/raw/original-notes/Grove - Annotations UX.txt`; `Grove - annotations.txt`; `Grove - Documents.txt` |
| Decisions | `docs/decisions/Content concentration and layout rules.md`; `docs/decisions/Annotation vertical tiers and landscape forms.md`; `docs/decisions/Annotation source routing and media boundary.md` |
| Reference | `docs/reference/Content concentration and layout model.md`; `docs/reference/Information Plane and Annotation model.md` |
| Discovery | `docs/discovery/Annotation template thresholds and media forms.md`; `docs/discovery/Annotation demand and vertical routing.md` |
| Wireframes | `docs/ux/wireframes/annotations/mixed-media/Annotation page forms.md`; `Annotation media forms.md`; `Annotation brochure.md`; `Annotation pamphlet.md`; `Annotation magazine.md` |
| Shared component | `docs/design-system/30-components/Annotation.md` |
| Shared grammar | `docs/design-system/10-grammar/Annotation-ledger.md`; `Content-concentration.md` |
