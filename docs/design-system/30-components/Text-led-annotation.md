---
type: design-system-component
status: normative
date: 2026-08-10
plane: information
vertical: text-led
---

# Text-led annotation

## Contract

| Property | Rule |
| --- | --- |
| Vertical | `imageShare ≤ 0.25`; `otherShare = 0`. |
| Tier source | Shared `D` bands in `10-grammar/Annotation-ledger.md`. |
| Forms | Bulletin, Berliner, Broadsheet. |
| Reading unit | Landscape sheet, spread, or threaded landscape pages. |
| Content priority | Complete ordered text; incidental images remain complete and secondary. |
| Shared shell | `30-components/Annotation.md`. |
| Shared demand | `10-grammar/Content-concentration.md`. |

## Form matrix

| Form | Tier | `D` band | Unit | Columns | Pages | Thread |
| --- | --- | --- | --- | --- | --- | --- |
| Bulletin | Tier 01 | `1.0 ≤ D < 1.75` | One landscape sheet | `1–3` | `1` | No |
| Berliner | Tier 02 | `1.75 ≤ D < 3.0` | Landscape facing spread | `2` per leaf | `1–2` | Leaf 1 → leaf 2 only |
| Broadsheet | Tier 03 | `D ≥ 3.0` | Landscape facing pages | `4–6` across spread; `2–3` per leaf | `2+` | Continuous across columns, leaves, pages |

## Shared page geometry

| Measure | Value |
| --- | --- |
| Head/side margin | `2.4em` at `--t-body`. |
| Foot margin | `1.2em` at `--t-body`. |
| Column measure | `34ch` / `--measure-reading`. |
| Column gutter | `1.7em`; 1px paper rule centred. |
| Baseline | `--t-body × --lh-reading`; `15px × 1.65 = 24.75px` fixture. |
| Spread seam | `3.4em` clear; no rule, shadow, spine, or ornament. |
| Body type | `--f-ui`, 400, `--t-body`, `--lh-reading`. |
| Paragraph indent | `1.15em` after first paragraph in a source piece. |
| Source boundary | Head mark, source title/heading, provenance, and continuation remain distinct. |
| Picture support | Complete intrinsic frame in its own region; no text wrap around frame. |

## Layout families

| Family | Form | Geometry | Select when |
| --- | --- | --- | --- |
| T1-A · single measure | Bulletin | One `34ch` measure; generous unused margin allowed; one complete source run. | One short source or compact related run fits one sheet. |
| T1-B · two measures | Bulletin | Two stable `34ch` measures; same baseline; no thread. | One source needs a second measure or two short sources share one sheet. |
| T1-C · three measures | Bulletin | Three stable `34ch` measures; independent source boundaries; no thread. | Several short sources belong to one reading and fit one sheet. |
| T2-A · compact single page | Berliner | Two stable measures on one landscape leaf; no forced newspaper width. | Moderate source set fits one leaf. |
| T2-B · facing compact spread | Berliner | Two equal measures per leaf; same measure and baseline across spread; no seam rule. | Source crosses one leaf or related pieces belong side by side. |
| T2-C · compact modular page | Berliner | Dominant text region plus supporting region; all regions remain text-first; source boundaries explicit. | One source leads and another supports inside the same spread capacity. |
| T3-A · broadsheet field | Broadsheet | Four to six narrow measures across spread; one baseline; independent source blocks. | Dense set requires several measures on a spread. |
| T3-B · threaded | Broadsheet | One source or connected set flows column → leaf → page; continuation repeats source identity. | One long source or connected set exceeds a page. |
| T3-C · modular | Broadsheet | Dominant region plus supporting regions; rules separate independent pieces; no group headline. | Many independent sources require distinct blocks on one page. |

## Family selection

| Order | Rule |
| --- | --- |
| 1 | Resolve the text-led vertical and shared tier. |
| 2 | Select a family inside the tier from source count, source length, source proximity, and whether sources share a leaf. |
| 3 | Use the largest readable column count that fits the landscape live width. |
| 4 | If the complete source does not fit, add columns only where the family permits, then add pages. |
| 5 | If the family cannot contain the complete source within its tier, promote to the next text-led form. |
| 6 | Never shrink type, narrow below `34ch`, crop an image, summarize text, or substitute portrait geometry. |

## Continuation

| Rule | Value |
| --- | --- |
| Break position | Wherever the current column runs out, including mid-sentence or mid-word. |
| Source identity | Repeated at the head of the continuation; source-authored title only. |
| Continuation cue | Mono label naming continuation; no summary and no apology. |
| Source order | Exact source order; no repeat across break; no dropped text. |
| Active source | Preserved when the new page sequence contains it. |
| Page count | Increases before type or measure decreases. |

## Supporting images

| Rule | Value |
| --- | --- |
| Vertical boundary | Image demand remains `≤ 0.25D`. |
| Placement | Own complete region beside or within the source block; no text wrap. |
| Caption | Source caption metadata only. |
| Provenance | Source Layer and Placement facts remain separate from caption. |
| Resolution floor failure | Text keeps readable measure; image receives a separate complete frame/page or its Image Viewer route. |
| No image in source set | No empty image slot. |

## States

| State | Text-led rule |
| --- | --- |
| Rest | Selected family, complete ordered text, complete supporting frames, provenance, folio. |
| Approached | No page restyle; routes remain context-gesture actions. |
| Focused | Focus ring outside control or source piece; thread identity unchanged. |
| Selected | Outline follows the selected source across its visible continuation; no page tint. |
| Engaged | Route menu opens at selected source; page does not mutate. |
| Pending | Last complete edition remains while text/layout re-resolves. |
| Refused | One explicit refusal sentence; edition remains. |
| Unavailable | Missing source retains its frame/column position and provenance; body becomes unavailable sentence. |
| Anchored | Authored-context mark appears on source piece only. |

## Reflow

| Event | Result |
| --- | --- |
| Text edit changes wrapping | Recompute `Tᵢ`, `D`, family, columns, and pages. |
| Source add/remove | Recompute set and family; preserve source order. |
| Usable width decreases | Reduce column count, then add pages; retain `34ch` measure and landscape unit. |
| Usable width increases | Increase columns only where family permits; do not change vertical from width alone. |
| Boundary crossed | Re-resolve using AN-D02; equality retains the current form and complete-fit evaluation controls change. |
| Failed recalculation | Keep last complete pages and state refusal/recovery. |

## Refusals

| Refused | Required alternative |
| --- | --- |
| Standfirst or summary | Source title/heading only. |
| Pull quote | Continuous source text only. |
| Invented image slot | No region. |
| Fixed columns regardless of width | Largest `n` that fits the live width. |
| Bulletin thread or second page | Promote to Berliner. |
| Berliner second spread | Promote to Broadsheet. |
| Page padding/trimming for tidy stop | Break at actual column capacity. |
| Portrait substitution | Keep landscape; change columns/pages. |
| Independent sources welded into one voice | Preserve separate boundaries and provenance. |
| Cropped/supporting image | Complete frame or separate route. |

## Design assertions

| ID | Assertion |
| --- | --- |
| AN-T01 | Exactly three text-led forms exist: Bulletin, Berliner, Broadsheet. |
| AN-T02 | Text-led routing reads the shared `D` and `imageShare` values; it owns no alternate threshold scale. |
| AN-T03 | Bulletin is one sheet and never threads. |
| AN-T04 | Berliner is one landscape spread with stable measures. |
| AN-T05 | Broadsheet threads continuously across columns, leaves, and pages. |
| AN-T06 | Every text column is `34ch` / `--measure-reading`. |
| AN-T07 | Text completeness outranks page count. |
| AN-T08 | A supporting image never changes text-led page grammar while `imageShare ≤ 0.25`. |
| AN-T09 | Boundary hysteresis follows `90-conformance/Annotation decisions.md` AN-D02. |

## Sources

| Source class | Source |
| --- | --- |
| Catalogue | `docs/design_catalogue/src/information-plane/06-annotation-text-led.html` |
| Decision | `docs/decisions/Annotation vertical tiers and landscape forms.md` |
| Reference | `docs/reference/Content concentration and layout model.md`; `docs/reference/Information Plane and Annotation model.md` |
| Discovery | `docs/discovery/Annotation demand and vertical routing.md`; `docs/discovery/Annotation text-led vertical.md`; `docs/discovery/Annotation template thresholds and media forms.md` |
| Wireframe | `docs/ux/wireframes/annotations/text-led/Annotation text vertical.md` |
| Shared component | `docs/design-system/30-components/Annotation.md` |
| Shared grammar | `docs/design-system/10-grammar/Annotation-ledger.md`; `Content-concentration.md` |
