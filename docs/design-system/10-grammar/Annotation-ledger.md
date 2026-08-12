---
type: design-system-grammar
status: normative
date: 2026-08-10
owner: "Grove Design System"
---

# Annotation ledger

## Scope

| Item | Contract |
| --- | --- |
| Subject | A complete, place-aware reading of related Content. |
| Plane | Information Plane; local to the source group; independent of Camera and Grid projection. |
| Supported source forms | Note, Document, Image, GIF. |
| Excluded source forms | Audio, video, PDF, EPUB. |
| Durable writes | None. Annotation qualification, vertical, tier, form, page, marker, active state, and pin state are derived or session state. |
| Identity owner | Memory. |
| Placement owner | Grid and Layer. |
| Relationship owner | Field Ledger. |
| Reading owner | Annotation ledger and the routed vertical specification. |

## Canonical ownership

| Contract | Owner |
| --- | --- |
| Field contribution, overlap, supported cells, Layer falloff | `docs/reference/Field and relationship model.md`; `30-components/Blip.md` |
| Source set, demand records, gate, shares, vertical, tier | This ledger; demand detail in `10-grammar/Content-concentration.md` |
| Shared reader shell, common states, page controls | `30-components/Annotation.md` |
| Mixed-media forms and templates | `30-components/Mixed-media-annotation.md` |
| Text-led forms and layout families | `30-components/Text-led-annotation.md` |
| Image-led forms and layout families | `30-components/Image-led-annotation.md` |
| Active marker and cue route | `30-components/Blip.md` |
| Open-reader lifetime and explicit replacement | `30-components/Annotation.md` |
| Source destinations and return contracts | `30-components/Annotation-routes.md` |

## Ledger record

| Field | Required | Value or rule |
| --- | --- | --- |
| `candidateId` | yes | Derived identifier for the current related source set; never persisted. |
| `targetLayer` | yes | Layer receiving the camera-independent candidate query. |
| `sourceIds` | yes | Ordered Memory identities whose field contributions overlap on the target Layer. |
| `placements` | yes | Current Placement references for the source set, including Layer and cell context. |
| `fieldCells` | yes | Supported cells returned by the Field Ledger; adjacent unsupported cells do not join. |
| `supportedCellCount` | yes | Count of supported cells in the calibration field. |
| `textDemand` | yes | `T = ΣTᵢ`; rendered text demand only. |
| `imageDemand` | yes | `I = ΣIᵢ`; complete-frame image demand only. |
| `otherDemand` | yes | `O = ΣOᵢ`; `0` for the initial supported-media boundary. |
| `sourceMass` | yes | `D = T + I + O`. |
| `textShare` | yes | `T / D`. |
| `imageShare` | yes | `I / D`. |
| `otherShare` | yes | `O / D`. |
| `qualification` | yes | `qualified` or `below-gate`, derived from `D` and field support. |
| `vertical` | qualified only | `text-led`, `mixed-media`, or `image-led`. |
| `tier` | qualified only | `Tier 01`, `Tier 02`, or `Tier 03`. |
| `form` | qualified only | One of the nine forms in the form matrix. |
| `layoutFamily` | qualified only | Vertical-specific page family chosen from the current form's allowed families. |
| `pageIndex` | open reading only | One-based page position in the current edition. |
| `pageCount` | open reading only | Total pages in the current edition. |
| `activeSourceId` | optional | Current source selected in the reading; source order is unchanged. |
| `pinState` | session only | `none`, `pinned`, `return`, or `stale`. |
| `recovery` | optional | Explicit refusal or unavailable-source result; never a silent drop. |

## Resolution order

```text
field ledger
  → related source set
  → per-source demand records
  → T, I, O, D and shares
  → qualification gate
  → vertical
  → tier
  → form
  → layout family
  → page geometry
  → pagination or continuation
```

| Rule ID | Rule |
| --- | --- |
| AN-L01 | Qualification is camera-independent. Camera scale and viewport size do not create or remove a candidate. |
| AN-L02 | Sources join only when their field contributions overlap on the same target Layer. Adjacency and subject matter do not join sources. |
| AN-L03 | Cross-Layer contribution is included through the existing field falloff. The target Layer remains explicit. |
| AN-L04 | A single long Document may qualify on measured demand. Item count is never a qualification requirement. |
| AN-L05 | A qualified set receives exactly one marker and one reading identity. |
| AN-L06 | The set-level vertical is chosen once per ledger evaluation. Page-local composition may not re-route the set. |
| AN-L07 | The tier is shared across all three verticals. A vertical may not define a second tier scale. |
| AN-L08 | Content order, source identity, provenance, complete text, complete image frames, and the active source survive pagination and reflow. |
| AN-L09 | A reading never truncates, ellipsizes, crops, stretches, summarizes, or overflows source material. |
| AN-L10 | Insufficient room is resolved in this order: use another family in the same tier; add columns or pages within the form; promote to the next tier; preserve an explicit recovery state if no valid layout exists. |

## Qualification gate

| Test | Rule |
| --- | --- |
| Source mass | `D ≥ 1.0`. |
| Field calibration | `1920 × 1080` calibration surface; `220 × 220` cells; `8 × 4 = 32` fully fitting cells. |
| Field support | At least `24` supported cells, with per-cell coverage `≥ 0.50`. |
| Single-Document exception | A single Document may qualify at `D ≥ 1.0` without the `24`-cell floor. |
| Below gate | No Annotation marker and no reading. Content remains ordinary placed Content. |
| Unavailable source | Retain source identity and provenance; after qualification retain the committed demand, vertical, tier, form, family, order, and page count under AN-D03; before qualification create no marker or Annotation under AN-D04. |

## Demand records

| Source | Demand record | Required inputs | Forbidden proxy |
| --- | --- | --- | --- |
| Note / Document | `Tᵢ` | Wrapped body lines, title/heading lines, paragraph breaks, readable measure, font size, line height, lines-per-page capacity. | Raw character count alone, byte size. |
| Image / GIF | `Iᵢ` | Intrinsic pixel dimensions, aspect ratio, display role, effective placed resolution, complete-frame reserve. | File byte size, current thumbnail size, crop. |
| Unsupported / unavailable | Reserved source record | Source identity, known dimensions where available, provenance, explicit unavailable frame. | Dropping the source or inventing a supported media type. |
| Future media | `Oᵢ` | Reserved only after a future media contract exists. `O = 0` in this release. | Audio/video assumptions in the current contract. |

The operational equations are owned by `Content-concentration.md` and the
physical family-routing grammar in `Content physical geometry.md`. They are not
replaced by a second formula in any vertical specification.

## Vertical routing

| Condition after qualification | Vertical | Boundary ownership |
| --- | --- | --- |
| `imageShare ≤ 0.25` and `otherShare = 0` | Text-led | `0.25` belongs to Text-led. |
| `0.25 < imageShare < 0.70` and `otherShare = 0` | Mixed-media | Open interval only. |
| `imageShare ≥ 0.70` and `otherShare = 0` | Image-led | `0.70` belongs to Image-led. |
| Share unavailable before qualification | Recovery | Apply `Annotation decisions.md` AN-D04; no marker or Annotation is created. |
| Share unavailable after qualification | Retained ledger | Apply `Annotation decisions.md` AN-D03; retain the committed vertical and form. |

## Tier routing

| Tier | Source-mass band | Capacity meaning |
| --- | --- | --- |
| Tier 01 | `1.0 ≤ D < 1.75` | Light source material. |
| Tier 02 | `1.75 ≤ D < 3.0` | Moderate source material. |
| Tier 03 | `D ≥ 3.0` | Dense source material. |

## Nine-form matrix

| Vertical | Tier 01 | Tier 02 | Tier 03 |
| --- | --- | --- | --- |
| Text-led | Bulletin | Berliner / compact | Broadsheet |
| Mixed-media | Brochure | Pamphlet | Magazine |
| Image-led | Gallery | Contact sheet | Image edition |

| Form | Reading unit | Primary geometry | Overflow rule |
| --- | --- | --- | --- |
| Bulletin | Landscape sheet | One sheet; one to three stable text measures; no thread. | Change family inside Tier 01; then promote. |
| Berliner / compact | Landscape facing spread | Stable newspaper measures; one or more leaves; text-first modules. | Continue in order; add pages before reducing measure. |
| Broadsheet | Landscape threaded pages | Four to six narrow measures across a spread; continuous flow. | Add pages; preserve baseline and order. |
| Brochure | Landscape sheet / folded sheet | Twelve-form catalogue family B-01 through B-04; bounded panels; complete frames. | Change brochure family; then promote to Pamphlet. |
| Pamphlet | Landscape reading spread | P-01 through P-04; leaves, columns, facing pages, continuation. | Add leaves/pages; then promote to Magazine. |
| Magazine | Landscape page / spread | M-01 through M-04; editorial regions, image pacing, threaded pagination. | Add pages; no uniform tiny grid. |
| Gallery | Landscape sheet | Large complete frame or small number of complete frames; supporting label conditional. | Change family or add page; never crop. |
| Contact sheet | Landscape sheet / spread | Multiple complete frames; aspect-preserving comparison; index metadata only where specified. | Reduce frame size before crop; add sheet/page. |
| Image edition | Landscape facing pages | Large complete frames; sequence and pagination; no gutter crossing. | Add pages; preserve sequence and active source. |

## Common reader state ledger

| State ID | State | Required result |
| --- | --- | --- |
| AN-S01 | Rest | Reader shell, current page, controls, source order, and provenance are visible. No route menu or piece outline. |
| AN-S02 | Approached | Reader page remains unchanged. A source piece is not restyled by pointer proximity; cue behavior belongs to Blip. |
| AN-S03 | Focused | Focus ring is outside the focused control or source piece. Page composition and source ink remain unchanged. |
| AN-S04 | Selected | Selected piece receives the interaction outline. Page order, fill, type, and neighboring pieces remain unchanged. |
| AN-S05 | Engaged | Source route menu is open at the chosen piece. The selected outline remains; no page mutation occurs until a row is chosen. |
| AN-S06 | Pending | Last complete edition remains visible with source order, active source, and page position retained. No blank reader, spinner, partial page, or half-applied reflow. |
| AN-S07 | Refused | One explicit refusal sentence appears in the reader answer line or local route surface. The complete edition remains. |
| AN-S08 | Unavailable | Missing source keeps its position, identity, provenance, and reserved frame. Body is replaced by one explicit unavailable sentence. |
| AN-S09 | Anchored | Source-authored context mark appears on the source piece. The Annotation surface itself is never Anchored. |

## Re-resolution triggers

| Trigger | Recompute |
| --- | --- |
| Related source added or removed | Source set, demand, shares, gate, vertical, tier, form, family, pages. |
| Text edit changes wrapping, headings, paragraphs, or page count | Text demand, source mass, tier, family, pages. |
| Image replacement changes intrinsic dimensions, aspect, role, or fidelity | Image demand, source mass, tier, family, pages. |
| Source availability changes | Reserved source record, recovery, page composition; source position retained. |
| Usable Information Plane width changes | Page ratio, columns, family, pages; vertical and tier remain source-set decisions unless their accepted resolver explicitly re-evaluates them. |
| Threshold crossed | Re-resolve using `Annotation decisions.md` AN-D02; equality retains the current form and complete-fit evaluation controls change. |

## Explicit non-inference rules

| Gap or conflict | Required handling |
| --- | --- |
| Boundary hysteresis | Apply `Annotation decisions.md` AN-D02. No additional numeric buffer exists. |
| Unavailable-source demand and vertical fallback | Apply `Annotation decisions.md` AN-D03–AN-D04. |
| Page-local template thresholds | Apply `Annotation decisions.md` AN-D05. |
| Mixed-media template count | Apply `Annotation decisions.md` AN-D06: exactly twelve selectable families. |

## Sources

| Source class | Sources |
| --- | --- |
| Raw intent | `docs/raw/original-notes/Grove - Annotations UX.txt`; `Grove - annotations.txt`; `Grove - Field ledger.txt`; `Grove - field metadata.txt`; `Grove - information layer.txt`; `Grove at a distance.txt`; `Grove - Layers.txt`; `Grove - Documents.txt`; `Grove - notes.txt`; `Grove - controlling content.txt`. |
| Accepted decisions | `D-ANNOTATION-01`; `D-ANNOTATION-02`; `D-ANNOTATION-03`; `D-ANNOTATION-04`. |
| Stable references | `Information Plane and Annotation model.md`; `Content concentration and layout model.md`; `Field and relationship model.md`; `Lexicon.md`. |
| Open discovery | `Annotation demand and vertical routing.md`; `Annotation template thresholds and media forms.md`; `Annotation text-led vertical.md`; `Annotation image-led vertical.md`; `Annotation source routes and plane handoffs.md`; `Annotation markers and publication.md`. |
| Catalogue evidence | `04-annotation-markers.html`; `05-annotation-mixed-media.html`; `06-annotation-text-led.html`; `07-annotation-image-led.html`; `08-annotation-routes.html`. |
| Product outcomes | `Choose annotation vertical.md`; `Choose annotation page form.md`; `Read text-led annotations.md`; `Read image-led annotations.md`; `Read brochure annotations.md`; `Read pamphlet annotations.md`; `Read magazine annotations.md`; `Read media in annotations.md`; `Read place-aware annotations.md`; `Notice annotation markers.md`; `Follow annotation source routes.md`; `Keep annotation reading current.md`. |
