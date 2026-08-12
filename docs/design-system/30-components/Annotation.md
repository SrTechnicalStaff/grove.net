---
type: design-system-component
status: normative
date: 2026-08-10
plane: information
---

# Annotation

## Contract

| Property | Value |
| --- | --- |
| Meaning | Complete, place-aware reading of related Content. |
| Plane | Information Plane. |
| Position | Beside the gathered source group; local coordinates; independent of Camera and Grid projection. |
| Durable identity | Memory and Placement remain authoritative. |
| Supported media | Note, Document, Image, GIF. |
| Unsupported media | Audio, video, PDF, EPUB. |
| Form resolver | `10-grammar/Annotation-ledger.md`. |
| Vertical specifications | `Mixed-media-annotation.md`, `Text-led-annotation.md`, `Image-led-annotation.md`. |
| Route specification | `Annotation-routes.md`. |
| Reader lifetime | One reader remains open until Close, Escape, or explicit replacement. |

## Anatomy

### Reader shell

| Part | Required | Rule |
| --- | --- | --- |
| Frame | yes | Opaque `--surface-chrome`; `--r-sm`; 1px View-role edge; `--sp-md` padding; `--shadow-local`. |
| Form label | yes | Form name in `--f-mono`, `--t-label`, `--tr-wide`, uppercase presentation; one line at head. |
| Answer line | yes | Reserved one-line status region beneath label; `--f-mono`, `--t-label`, `--tr-mono`; refusal ink when refusing. |
| Close | yes | `×`; trailing edge of head row; `--f-ui`, `--t-title`, secondary ink. |
| Keep action | yes | Quiet bordered control in the action row; interaction fill and paper ink while pinned. |
| Page controls | yes | Previous, position, Next on one row; words, not unlabeled symbols. Both controls remain present; spent control uses unavailable ink. |
| Key hints | optional | `Esc closes · P pins`; mono micro text; no global reservation while reader is closed. |

### Page

| Part | Required | Rule |
| --- | --- | --- |
| Sheet | yes | `--surface-page`; no radius; 1px `--edge-on-page`; landscape `8:5`. |
| Margin | yes | Head and side `2.4em`; foot `1.2em`; `1em = --t-body`. |
| Head rule | yes | 2px paper ink; full live width; `0.9em` clear below. |
| Column | yes | `--measure-reading`; `--f-ui`, 400, `--t-body`, `--lh-reading`; paper ink. |
| Gutter | yes | `1.7em`; 1px paper rule centred in the gutter. |
| Spread seam | optional | `3.4em` clear; no seam rule, shadow, or binding ornament. |
| Piece head mark | conditional | 2px source-colored mark at the piece head; `0.65em` clear before heading. |
| Piece title | conditional | Source title only; display title role; no generated title. |
| Source heading | conditional | Source-carried heading only; no page-authored replacement. |
| Provenance | yes | Source Layer and Placement facts in mono label text; source facts only. |
| Paragraph indent | conditional | `1.15em` after the first paragraph in a piece; none after a source heading. |
| Picture frame | conditional | Complete source frame at intrinsic proportion; no crop, stretch, radius, or decorative fill. |
| Picture line | conditional | Source caption facts only; absent when source metadata is absent. |
| Piece separation | yes | `1.1em` clear; paper rule only where a following piece has no head mark. |
| Continuation cue | conditional | Source name and continuation position; mono label; no synthetic summary. |
| Foot rule | yes | 1px paper rule across live width. |
| Folio | yes | Layer context at one edge; `n / m` at the other. |
| Unavailable sentence | conditional | One explicit sentence in the reserved source frame; source position remains. |
| Chosen outline | conditional | 2px interaction outline, 3px outside the piece, with paper keying line. |
| Anchor mark | conditional | Authored-context diamond on the source piece; never on the reader shell. |

## Geometry

| Rule ID | Rule |
| --- | --- |
| AN-G01 | Reader width equals page width plus `2 × --sp-md` and shell rows/padding. |
| AN-G02 | Every form uses a landscape `8:5` reading unit. Narrow width changes columns, pages, or horizontal advance; it never substitutes a portrait book page. |
| AN-G03 | Form resolution follows: demand records → `D` and shares → gate → vertical → tier → form → layout family. |
| AN-G04 | `D < 1.0` produces no reading. A qualifying set uses the shared tier bands and vertical boundaries in `Annotation-ledger.md`. |
| AN-G05 | Page-local ratio may choose composition inside a routed form; it may not re-route the source set. |
| AN-G06 | Text is rendered at readable measure and size. Type size, line height, measure, and completeness do not shrink to avoid pagination. |
| AN-G07 | Image frames preserve intrinsic aspect ratio and complete edges. Image resolution limits display area; the page changes before the frame crops. |
| AN-G08 | Source order is stable. Independent sources retain independent title, provenance, caption, and route. |
| AN-G09 | A full source continues through columns/pages; it is never chunked into unrelated cards. |
| AN-G10 | Layout re-resolves when source membership, text wrapping, image facts, availability, or usable width changes. |
| AN-G11 | Pending re-resolution holds the last complete edition. Failure produces an explicit recovery sentence. |
| AN-G12 | Boundary equality retains the current form; a complete-fit evaluation on the other side is required. `90-conformance/Annotation decisions.md` AN-D02 owns the rule. |

## States

| State | Appearance | Transition rule |
| --- | --- | --- |
| Rest | Reader shell, current page, controls, source order, provenance. No route menu or piece outline. | Baseline open state. |
| Approached | No reader-page change. | Piece proximity does not restyle page content; Blip owns approach cue. |
| Focused | Focus ring outside focused control or piece. | Page composition and source ink remain unchanged. |
| Selected | Interaction outline outside chosen piece. | No page tint, fill, reordering, or neighboring dimming. |
| Engaged | Route menu open at chosen piece; selected outline remains. | Opening menu does not mutate page or source. |
| Pending | Last complete pages, page position, source order, and active source remain. | No blank page, spinner, partial page, or half-applied reflow. |
| Refused | One refusal sentence in answer line or route surface. | Complete edition remains. |
| Unavailable | Missing source retains position, identity, provenance, and reserved frame. | Body becomes one unavailable sentence; routes requiring the source are absent or refused. |
| Anchored | Source-authored context mark on source piece. | Reader shell never receives Anchor state. |

## Behaviour

| Action | Result |
| --- | --- |
| Open from Blip cue | Open local reader beside source group; do not move Camera. |
| Page Next / Previous | Replace complete page; retain form, source order, provenance, active source when present. |
| Select source piece | Apply chosen outline; no source mutation. |
| Open source routes | Raise `Annotation-routes.md` menu at the source piece. |
| Open another reading | Replace the current reader through an explicit router handoff. |
| Close | Dismiss reader and return focus to invoking surface. |
| Source mutation | Re-resolve in place; preserve current source when the new edition can carry it. |
| Source loss | Keep explicit unavailable source frame; never silently rewrite the edition. |
| Camera pan or zoom | Do not change reader content or source-set qualification. |

## Motion

| Transition | Duration | Reduced motion |
| --- | --- | --- |
| Reader arrival/departure | `--d-fade`, `--e-standard` | Present/absent at identical threshold. |
| Page replacement | `--d-swap`, `--e-standard` | New complete page in place; no cross-fade. |
| Pressed control | `--d-press`, `--e-standard` | Pressed state only; no scale transform. |
| Form/page reflow | `--d-swap`, `--e-standard` | New complete edition in place; no cross-fade. |

## Distance

| Representation | Reader rule |
| --- | --- |
| Working | Full page and shell. |
| Stepped | Full page and shell. |
| Stand-in | Full page and shell. |
| Reason | A reader is an Information Plane surface; Grid representation shedding does not rewrite or summarize it. |

## Accessibility

| Requirement | Rule |
| --- | --- |
| Reading order | Form label → answer line → page content in source order → page controls → keep → close. |
| Focus | Visible focus ring outside the focused target; no focus-only meaning. |
| Page navigation | Previous and Next are named controls with position exposed separately. |
| Source identity | Title, provenance, and unavailable status remain exposed as text. |
| Motion | Reduced-motion mode preserves identical state thresholds and geometry. |
| Contrast | Paper ink, source ink, refusal ink, and interaction outline use their named design tokens. |
| Keyboard | Escape closes; `P` pins while reader is active; routes expose their own printed quick keys. |

## Copy

| String | Use |
| --- | --- |
| `Bulletin` | Text-led Tier 01 form label. |
| `Berliner` | Text-led Tier 02 form label. |
| `Broadsheet` | Text-led Tier 03 form label. |
| `Brochure` | Mixed-media Tier 01 form label. |
| `Pamphlet` | Mixed-media Tier 02 form label. |
| `Magazine` | Mixed-media Tier 03 form label. |
| `Gallery` | Image-led Tier 01 form label. |
| `Contact sheet` | Image-led Tier 02 form label. |
| `Image edition` | Image-led Tier 03 form label. |
| `Reading unavailable` | Reader label when the edition has no available source. |
| `Previous` / `Next` | Page controls. |
| `Esc closes · P pins` | Optional reader key hint. |
| `That destination is unavailable. The reading is still here.` | Route refusal. |
| `This reading is no longer available.` | Terminal pinned-reading recovery. |

## Refusals

| Refusal | Do not do |
| --- | --- |
| Shrink to fit | Reduce type, leading, readable measure, or fidelity floor. |
| Crop to fit | Cut, stretch, or square a frame. |
| Overflow | Clip, scroll the page, ellipsize, or hide continuation. |
| Empty metadata | Reserve an empty title/caption slot or invent source words. |
| Page replacement | Discard source order or active source. |
| Detached route | Centre a dialog, dim the source, or replace the reading with a fixed HUD surface. |
| Camera coupling | Pan, zoom, or frame the Grid as a side effect of reading. |
| Generic umbrella label | Use the exact Grid, Field, Plane, Slate, page, or source-set noun. |

## Design assertions

| ID | Assertion |
| --- | --- |
| AN-C01 | All nine forms exist in the shared ledger and have one vertical specification. |
| AN-C02 | All nine reader states are reachable or explicitly refused by the common state table. |
| AN-C03 | Every form is landscape and every overflow path is complete pagination or continuation. |
| AN-C04 | Reader qualification is Camera-independent and Field-Ledger-derived. |
| AN-C05 | Page-local composition cannot re-route the source set. |
| AN-C06 | Routes are on-demand menus; they are not permanent page chrome. |
| AN-C07 | The reader remains open until Close, Escape, or explicit replacement; unavailable source recovery remains in the same edition. |
| AN-C08 | Boundary hysteresis and unavailable-source behavior follow `90-conformance/Annotation decisions.md` AN-D02–AN-D04. |

## Sources

| Source class | Source |
| --- | --- |
| Raw intent | `docs/raw/original-notes/Grove - Annotations UX.txt`; `Grove - annotations.txt`; `Grove - Field ledger.txt`; `Grove - field metadata.txt`; `Grove - information layer.txt`; `Grove at a distance.txt`; `Grove - Layers.txt`; `Grove - Documents.txt`; `Grove - notes.txt`; `Grove - controlling content.txt`. |
| Accepted decisions | `D-ANNOTATION-01`; `D-ANNOTATION-02`; `D-ANNOTATION-03`; `D-ANNOTATION-04`. |
| Stable references | `Information Plane and Annotation model.md`; `Content concentration and layout model.md`; `Field and relationship model.md`; `Lexicon.md`. |
| Catalogue decks | `04-annotation-markers.html`; `05-annotation-mixed-media.html`; `06-annotation-text-led.html`; `07-annotation-image-led.html`; `08-annotation-routes.html`. |
| Companion system specs | `10-grammar/Annotation-ledger.md`; `10-grammar/Content-concentration.md`; `Mixed-media-annotation.md`; `Text-led-annotation.md`; `Image-led-annotation.md`; `Annotation-routes.md`. |
