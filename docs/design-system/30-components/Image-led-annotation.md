---
type: design-system-component
status: normative
date: 2026-08-10
plane: information
vertical: image-led
---

# Image-led annotation

## Contract

| Property | Rule |
| --- | --- |
| Vertical | `imageShare ≥ 0.70`; `otherShare = 0`. |
| Tier source | Shared `D` bands in `10-grammar/Annotation-ledger.md`. |
| Forms | Gallery, Contact sheet, Image edition. |
| Reading unit | Landscape sheet, comparison sheet, or facing landscape pages. |
| Content priority | Complete image frames; supporting text yields before a frame crops. |
| Shared shell | `30-components/Annotation.md`. |
| Shared demand | `10-grammar/Content-concentration.md`. |

## Form matrix

| Form | Tier | `D` band | Unit | Primary geometry |
| --- | --- | --- | --- | --- |
| Gallery | Tier 01 | `1.0 ≤ D < 1.75` | One landscape sheet | One leading frame or a small number of complete frames. |
| Contact sheet | Tier 02 | `1.75 ≤ D < 3.0` | Landscape comparison sheet | Several complete frames at common comparison height; widths follow aspect. |
| Image edition | Tier 03 | `D ≥ 3.0` | Facing landscape pages | Large complete frames, paced sequence, ordered pagination. |

## Layout family ledger

| Family | Form | Geometry | Selection rule |
| --- | --- | --- | --- |
| I1-A · single plate | Gallery | One complete leading frame with page field around it. | One image requires inspection. |
| I1-B · plate with label | Gallery | One complete frame; conditional source caption/facts beneath; no reserved label slot. | One image carries source caption metadata. |
| I1-C · paired plates | Gallery | Two independently complete frames; pair chosen by source order/proximity/contrast. | Two images form one light comparison. |
| I2-A · uniform contact sheet | Contact sheet | Common frame height; widths follow aspect; no crop or square forcing. | Comparable sources share a common aspect family/height. |
| I2-B · aspect-preserving sheet | Contact sheet | Each frame preserves own aspect; wide frame receives more width; row count yields before distortion. | Sources vary in aspect ratio. |
| I2-C · strip with facts rail | Contact sheet | Complete frames in a strip; source facts and index in margin rail, never across image. | Comparison needs index/provenance facts beside the frames. |
| I3-A · single-image page | Image edition | One large complete image page; no gutter crossing. | One image needs scale and pause. |
| I3-B · facing-page pair | Image edition | Two complete facing pages; each page has own provenance/folio. | Pairing/contrast belongs across a turn. |
| I3-C · image sequence spread | Image edition | Multiple complete frames paced across a spread at varied sizes. | Sequence needs multiple frames and varied scale. |
| I3-D · image-plus-text spread | Image edition | Dominant complete frame on one side; short supporting text region on the other. | Text supports an image without becoming image geometry. |
| I3-E · paginated sequence | Image edition | Ordered complete frames across pages; frame sequence and folio continue. | More frames require more pages at readable scale. |

Catalogue family count: `3` Gallery + `3` Contact sheet + `5` Image edition =
`11` families.

## Frame geometry

| Rule | Value |
| --- | --- |
| Aspect source | Intrinsic `w / h`; classify before composing. |
| Landscape | Complete frame sits within landscape unit with field around it. |
| Portrait | Full-height frame with support beside it; page remains landscape. |
| Square | Equal margins; no stretch toward row edges. |
| Extreme panorama | Full unit width or whole spread when detail requires it; row/page carries fewer frames. |
| Gutter | Never crossed by frame or caption. |
| Frame selection | Change region, reduce complete frame, choose another family, or add page before crop. |
| Selection state | Outline outside frame; image pixels never tint, dim, or reframe. |
| Source order | Capture/order sequence retained left-to-right, top-to-bottom, then page-to-page. |

## Fidelity

| `PPI_eff` | Allowed treatment |
| --- | --- |
| `≥ 300` | Full display area, leading frame, or full-width sequence moment. |
| `150–299` | Smaller bounded complete frame; no dominant/full-bleed emphasis. |
| `< 150` | Small supporting frame or no page frame; Image Viewer remains the inspection route. |

```text
PPI_eff = min(px_w / placed_w_in, px_h / placed_h_in)
```

## Captions, facts, and provenance

| Content | Rule |
| --- | --- |
| Caption | Source caption metadata only; absent caption removes its line. |
| Contact index | Index and source aspect ratio may appear in the margin beneath/around frame. |
| Source facts | Kind, dimensions, and proportion may appear as quiet mono facts. |
| Provenance | Layer and Placement facts at page foot; never replaces caption or identity. |
| Title | Source title only; no generated group title, issue title, or masthead. |
| Missing text | No empty title/caption region. |

## Motion sources

| Source | Rule |
| --- | --- |
| Still Image | Complete frame remains still. |
| GIF | Native animation plays inside complete frame; frame area does not change. |
| Reduced motion | Freeze a complete still frame at the same location and size. |
| Video/audio | Outside the supported Annotation media boundary. |

## Form selection order

| Order | Rule |
| --- | --- |
| 1 | Resolve Image-led vertical and shared tier. |
| 2 | Classify each intrinsic aspect family. |
| 3 | Apply role, `PPI_eff`, page-local balance, source order, and usable area. |
| 4 | Select an allowed family whose frames remain complete. |
| 5 | Change region or add page before reducing fidelity; never crop. |
| 6 | Promote/reflow through the shared ledger when set demand crosses a shared band. |

## States

| State | Image-led rule |
| --- | --- |
| Rest | Complete selected family, frames, conditional facts/captions, source order, provenance, folio. |
| Approached | No page restyle; route actions remain context-gesture actions. |
| Focused | Focus ring outside control/frame; image pixels unchanged. |
| Selected | Outline outside selected frame; image fill and neighboring frames unchanged. |
| Engaged | Route menu opens at selected source; page does not mutate. |
| Pending | Last complete sheet/pages remain during decode, demand, or family re-resolution. |
| Refused | One explicit refusal sentence; complete frames remain. |
| Unavailable | Frame slot, source identity, provenance, and known facts remain; picture is replaced by unavailable state. |
| Anchored | Authored-context mark on source piece only. |

## Reflow and pagination

| Trigger | Result |
| --- | --- |
| Source add/remove | Recompute `D`, shares, tier, family, page count; preserve order. |
| Image replacement | Recompute dimensions, aspect, role, `PPI_eff`; reselect frame/family. |
| Text/caption mutation | Recompute supporting text region; frame remains complete. |
| Usable width change | Retain landscape unit; change region, row, spread, or page count. |
| More frames | Start another sheet/page before shrinking below comparison/fidelity floor. |
| Panorama | Receive full width or spread; rest of sequence advances. |
| Active source | Remains active when present in the new sequence. |
| Failed recalculation | Last complete pages remain; recovery is explicit. |
| Threshold crossing | Re-resolve using AN-D02; equality retains the current form and complete-fit evaluation controls change. |

## Refusals

| Refused | Required alternative |
| --- | --- |
| Wall of equal tiny crops | Add sheet/page; retain aspect and comparison scale. |
| Crop to fill slot | Change region/family or add page. |
| Stretch/square force | Preserve intrinsic aspect. |
| Upscale below fidelity floor | Reduce frame or use Image Viewer. |
| Text slot with no text | Close region; do not reserve empty title/caption/notes blocks. |
| Caption across gutter | Keep caption with its own frame/page. |
| Panorama reduced to row tile | Give width or spread; let row/page hold fewer frames. |
| Flatten GIF to still | Preserve playback, or reduced-motion still at same frame. |
| Portrait page substitution | Keep landscape unit. |

## Design assertions

| ID | Assertion |
| --- | --- |
| AN-I01 | Exactly three image-led forms exist: Gallery, Contact sheet, Image edition. |
| AN-I02 | Exactly eleven catalogue families exist: I1-A–I1-C, I2-A–I2-C, I3-A–I3-E. |
| AN-I03 | Every frame is complete and aspect-preserving. |
| AN-I04 | Contact sheets use common comparison height while widths follow source aspect. |
| AN-I05 | Image edition sequence and provenance continue across pages without gutter crossing. |
| AN-I06 | `PPI_eff` caps display area; it never authorizes crop or upscale. |
| AN-I07 | GIF animation changes playback only, not room or demand. |
| AN-I08 | Missing captions/text do not leave reserved empty slots. |
| AN-I09 | Boundary hysteresis and unavailable-source behavior follow `90-conformance/Annotation decisions.md` AN-D02–AN-D04. |

## Sources

| Source class | Source |
| --- | --- |
| Catalogue | `docs/design_catalogue/src/information-plane/07-annotation-image-led.html` |
| Raw intent | `docs/raw/original-notes/Grove - Annotations UX.txt`; `Grove - annotations.txt` |
| Decisions | `docs/decisions/Annotation vertical tiers and landscape forms.md`; `docs/decisions/Content concentration and layout rules.md`; `docs/decisions/Annotation source routing and media boundary.md` |
| Reference | `docs/reference/Content concentration and layout model.md`; `docs/reference/Information Plane and Annotation model.md` |
| Discovery | `docs/discovery/Annotation demand and vertical routing.md`; `docs/discovery/Annotation image-led vertical.md`; `docs/discovery/Annotation template thresholds and media forms.md` |
| Wireframe | `docs/ux/wireframes/annotations/image-led/Annotation image vertical.md` |
| Shared component | `docs/design-system/30-components/Annotation.md` |
| Shared grammar | `docs/design-system/10-grammar/Annotation-ledger.md`; `Content-concentration.md` |
