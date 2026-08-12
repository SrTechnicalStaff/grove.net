---
type: design-system-index
status: normative
date: 2026-08-10
owner: "Grove Design System"
---

# Grove design system

## Layers

| Directory | Contract |
| --- | --- |
| `00-foundations/` | Color, type, space, shape, depth, motion, marks, accessibility, tokens. |
| `10-grammar/` | Surface classes, signals, states, representation, copy, content concentration, Annotation ledger. |
| `20-planes/` | Grid Plane, Information Plane, HUD Plane ownership and locality. |
| `30-components/` | Visible and actionable component contracts. |
| `90-conformance/` | Catalogue coverage, component contract register, design assertions, and closed Annotation decisions. |

| Explicit contract | Owner |
| --- | --- |
| Unbounded Layer depth and saturation behavior | [`10-grammar/Layer-depth.md`](10-grammar/Layer-depth.md) |

## Authority order

| Order | Source | Ownership |
| --- | --- | --- |
| 1 | `docs/raw/original-notes/` | Product meaning and source constraints; read-only. |
| 2 | Accepted `docs/decisions/` | Settled product and model boundaries. |
| 3 | `docs/reference/` | Stable concepts and invariants. |
| 4 | `docs/design_catalogue/` | Deliberate visual evidence and rendered examples. |
| 5 | `docs/design-system/` | Normative visual, layout, state, and refusal rules. |
| 6 | `docs/ux/wireframes/` | Journey order and state identifiers. |
| 7 | Implementation | Conformance subject; not design authority. |

## Annotation subsystem

| Owner | Scope |
| --- | --- |
| `10-grammar/Annotation-ledger.md` | Source record, demand, gate, shares, 9-form matrix, common 9-state matrix, and re-resolution. |
| `10-grammar/Content-concentration.md` | Text/image demand equations, fidelity, gate, tier, vertical, page-local composition. |
| `10-grammar/Content physical geometry.md` | Intrinsic media geometry, text column units, complete-frame projection, deterministic family predicates, page breaks, and physical reflow. |
| `30-components/Annotation.md` | Reader shell, shared page geometry, shared behavior, common states, common refusals. |
| `30-components/Mixed-media-annotation.md` | Brochure, Pamphlet, Magazine; 12 rendered templates. |
| `30-components/Text-led-annotation.md` | Bulletin, Berliner, Broadsheet; 9 layout families. |
| `30-components/Image-led-annotation.md` | Gallery, Contact sheet, Image edition; 11 layout families. |
| `30-components/Blip.md` | Qualification marker, field support, cue, approach. |
| `30-components/Annotation-routes.md` | Grid, Memory, Writing Slate, Image Viewer, and placement routes. |
| `30-components/Rich-text-editor.md` | Tiptap editor, JSON persistence, text projection, and editor lifecycle. |
| `90-conformance/Annotation decisions.md` | Closed Annotation demand, hysteresis, unavailable-source, capacity, family, state, and route decisions. |

## Annotation form matrix

| Vertical | Tier 01 | Tier 02 | Tier 03 |
| --- | --- | --- | --- |
| Text-led | Bulletin | Berliner | Broadsheet |
| Mixed-media | Brochure | Pamphlet | Magazine |
| Image-led | Gallery | Contact sheet | Image edition |

## Catalogue coverage

| Catalogue group | Coverage record |
| --- | --- |
| All 21 decks | [`90-conformance/Catalogue coverage.md`](90-conformance/Catalogue%20coverage.md) |
| All component owners | [`90-conformance/Component contract register.md`](90-conformance/Component%20contract%20register.md) |
| Annotation decks 04–08 | `Annotation-ledger.md`, `Annotation.md`, `Blip.md`, vertical specs, routes, pinning. |
| Grid Plane implementation slices | [`90-conformance/Grid Plane implementation slices.md`](90-conformance/Grid%20Plane%20implementation%20slices.md) |
| Information Plane implementation slices | [`90-conformance/Information Plane implementation slices.md`](90-conformance/Information%20Plane%20implementation%20slices.md) |

## Change gates

| Gate | Requirement |
| --- | --- |
| G-01 | Raw meaning remains unchanged. |
| G-02 | One canonical owner per rule. |
| G-03 | Every form has anatomy, geometry, states, behavior, motion, accessibility, copy, refusals, assertions, and sources. |
| G-04 | No unaccepted value is inferred. |
| G-05 | Every catalogue deck has an owner and coverage status. |
| G-06 | Product copy uses canonical Lexicon terms; rejected umbrella terms do not appear. |
