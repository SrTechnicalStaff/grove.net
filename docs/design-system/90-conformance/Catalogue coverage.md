---
type: design-system-coverage
status: normative
date: 2026-08-10
owner: "Grove Design System"
---

# Catalogue coverage

## Coverage criteria

| Criterion | Required result |
| --- | --- |
| Source ownership | Every catalogue deck maps to one or more normative system documents. |
| Anatomy | Visible parts and their roles are named. |
| Geometry | Dimensions, ratios, spacing, thresholds, and reflow rules are named where the source specifies them. |
| States | Reachable states and refusal states are named. |
| Source trace | Raw notes, Decision/Reference, wireframe, and catalogue links are retained. |
| Source closure | Every source contradiction has a named normative decision. |

## Deck ledger

| Deck | Plane/group | Normative owner(s) | Coverage | Source closure |
| --- | --- | --- | --- | --- |
| `00-design-language.html` | Shared foundations | `00-foundations/Color.md`; `Typography.md`; `Shape.md`; `Space-and-grid.md`; `Elevation-and-depth.md`; `Motion.md`; `10-grammar/Surface-classes.md`; `Copy.md` | Complete: palette roles, type roles, shape, spacing, depth, surface classes, refusals, reduced motion. | None in deck; token projections remain separate. |
| `01-menus.html` | Shared action chrome | `30-components/Menu.md`; `30-components/Inline-confirm.md`; `00-foundations/Shape.md`; `Elevation-and-depth.md`; `10-grammar/Copy.md` | Complete: row anatomy, three targets, surface-owned actions, flyout, keys, escape, no scrim, omitted actions. | None. |
| `grid-plane/01-the-grid.html` | Grid Plane | `20-planes/Grid-plane.md`; `30-components/Grid-lines.md`; `Grid-cursor.md`; `Selection.md` | Complete: minor/major/supercell hierarchy, cursor, selection distinction, line visibility, distance behavior, refusals. | None. |
| `grid-plane/02-note.html` | Grid Plane / Note | `30-components/Note.md`; `Stand-in.md`; `Presence.md` | Complete: authored text, footprint growth, states, distance shedding, no title/summary/truncation. | None. |
| `grid-plane/03-document.html` | Grid Plane / Document | `30-components/Document.md`; `Stand-in.md`; `Presence.md` | Complete: paper geometry, title/body/provenance, complete page, anchor/selection/distance, refusals. | None. |
| `grid-plane/04-image.html` | Grid Plane / Image | `30-components/Picture.md`; `Stand-in.md`; `Placement-preview.md` | Complete: intrinsic frame, integral footprint, GIF motion, selection/anchor/distance, no crop. | None. |
| `grid-plane/05-presence-fields.html` | Grid Plane / Field | `30-components/Presence.md`; `20-planes/Grid-plane.md`; `10-grammar/Signal-roles.md` | Complete: per-cell edges, Layer hue accumulation, falloff, recompute, no relationship inference. | None. |
| `grid-plane/06-distance.html` | Grid Plane / representation | `10-grammar/Representation-tiers.md`; `30-components/Stand-in.md`; `00-foundations/Motion.md` | Complete: shedding order, constant position/extent, stand-ins, promotion/demotion thresholds, reduced motion. | None. |
| `grid-plane/07-selection-placement.html` | Grid Plane / selection | `30-components/Selection.md`; `Placement-preview.md`; `Refusal.md`; `10-grammar/Signal-roles.md` | Complete: selection outline, marquee, preview footprint, invalid placement, inline refusal, no partial state. | None. |
| `hud/01-slate-anatomy.html` | HUD Plane / Slate host | `20-planes/HUD-plane.md`; `30-components/Slate.md`; `00-foundations/Elevation-and-depth.md`; `Shape.md` | Complete: opaque surface, pane composition, no scrim, no Grid resize, peer panes, focus, close. | Pane-width decisions remain in accepted HUD composition record. |
| `hud/02-memory-slate.html` | HUD Plane / Memory Slate | `30-components/Memory-slate.md`; `Slate.md`; `20-planes/HUD-plane.md` | Complete: browse-first gallery, Memory identity, selection, empty/recovery, no Grid navigation. | None. |
| `hud/03-gallery-slate.html` | HUD Plane / Gallery Slate | `30-components/Gallery-slate.md`; `Slate.md`; `Picture.md` | Complete: image gallery, complete frames, masonry/provenance, selection, empty/recovery, no source mutation. | None. |
| `hud/04-writing-slate.html` | HUD Plane / Writing Slate | `30-components/Writing-slate.md`; `Slate.md`; `Inline-confirm.md`; `00-foundations/Accessibility.md` | Complete: long-form editor, saved/dirty/recovery/refusal, no scrim, no source loss, return. | None. |
| `information-plane/01-local-editors.html` | Information Plane / local editor | `20-planes/Information-plane.md`; `30-components/Local-editor.md`; `Inline-confirm.md` | Complete: beside-source placement, opaque frame, save/cancel, refusal, return focus, no Camera change. | None. |
| `information-plane/02-image-viewer.html` | Information Plane / Image Viewer | `30-components/Image-viewer.md`; `Picture.md`; `Refusal.md` | Complete: complete fit, zoom/pan/playback, decode failure, source-local placement, no centered dialog/scrim, no Camera change. | None. |
| `information-plane/03-quick-note.html` | Information Plane / Quick Note | `30-components/Quick-note.md`; `Local-editor.md`; `Refusal.md` | Complete: one-action capture, capture session, return/refine, no forced organization, refusal. | None. |
| `information-plane/04-annotation-markers.html` | Information Plane / Blip | `30-components/Blip.md`; `10-grammar/Annotation-ledger.md`; `20-planes/Information-plane.md` | Complete: field gate, 24/32 support, marker/cue, one active route, pin/return linkage, no Camera qualification. | `Annotation decisions.md` AN-D01–AN-D04, AN-D07. |
| `information-plane/05-annotation-mixed-media.html` | Information Plane / Mixed-media | `10-grammar/Annotation-ledger.md`; `Content-concentration.md`; `30-components/Annotation.md`; `Mixed-media-annotation.md` | Complete: 3 tiers, 3 forms, 12 templates, demand/share routing, quality bands, source identity, pagination, landscape, refusals. | `Annotation decisions.md` AN-D01–AN-D07. |
| `information-plane/06-annotation-text-led.html` | Information Plane / Text-led | `10-grammar/Annotation-ledger.md`; `Content-concentration.md`; `30-components/Annotation.md`; `Text-led-annotation.md` | Complete: 3 tiers, 3 forms, 9 families, measure/baseline/columns, continuation, narrow-width reflow, support image rules, refusals. | `Annotation decisions.md` AN-D01, AN-D02, AN-D05. |
| `information-plane/07-annotation-image-led.html` | Information Plane / Image-led | `10-grammar/Annotation-ledger.md`; `Content-concentration.md`; `30-components/Annotation.md`; `Image-led-annotation.md` | Complete: 3 tiers, 3 forms, 11 families, aspect families, fidelity bands, animation, captions/provenance, sequence, pagination, refusals. | `Annotation decisions.md` AN-D01–AN-D05. |
| `information-plane/08-annotation-routes.html` | Information Plane / source routes | `30-components/Annotation-routes.md`; `Annotation.md`; `20-planes/Information-plane.md` | Complete: Grid/Memory/Writing Slate/Image Viewer routes, fresh destination resolution, open-reader lifetime, unavailable recovery, and refusal designs. | `Annotation decisions.md` AN-D03, AN-D07–AN-D09; future media remains outside boundary. |

## Annotation decision cross-reference

| Decision | Affected decks | Owner |
| --- | --- | --- |
| Demand fixture and boundary hysteresis | 04, 05, 06, 07 | `90-conformance/Annotation decisions.md` AN-D01–AN-D02. |
| Unavailable demand and fallback | 04, 05, 06, 07, 08 | `90-conformance/Annotation decisions.md` AN-D03–AN-D04. |
| Page-local capacity | 05, 06, 07 | `90-conformance/Annotation decisions.md` AN-D05. |
| Mixed-media template count | 05 | `90-conformance/Annotation decisions.md` AN-D06. |
| Publication and route states | 04, 08 | `90-conformance/Annotation decisions.md` AN-D07–AN-D09. |

## Result

| Status | Count |
| --- | ---: |
| Catalogue HTML decks inventoried | `21` |
| Decks with a normative owner | `21` |
| Annotation decks with explicit vertical/form/family treatment | `3` |
| Annotation marker/route decks with explicit component treatment | `2` |
| Closed Annotation source contracts | `9` |
