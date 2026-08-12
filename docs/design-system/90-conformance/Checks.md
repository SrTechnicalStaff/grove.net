---
type: design-system-check-register
status: normative
date: 2026-08-10
owner: "Grove Design System"
tags: [grove, design-system, checks]
---

# Design-system checks

## Contract checks

| ID | Check | Required result |
| --- | --- | --- |
| DS-01 | Catalogue ownership | All `21` catalogue decks have at least one normative owner. |
| DS-02 | Component ownership | Every visible or actionable component has one owner. |
| DS-03 | Required sections | Each component owner has Anatomy, Geometry, States, Behaviour, Motion, Distance, Accessibility, Copy, Refusals, Design assertions, and Sources. |
| DS-04 | State coverage | Each component owner names Rest, Approached, Focused, Selected, Engaged, Pending, Refused, Unavailable, and Anchored. |
| DS-05 | Value closure | Every normative dimension, color, type role, spacing, threshold, duration, curve, and refusal value is explicit or references one canonical token. |
| DS-06 | Source trace | Each owner links read-only raw notes, accepted Decisions or References, applicable wireframes, and catalogue evidence. |
| DS-07 | Decision closure | Each catalogue contradiction has one named normative owner and one closed rule. |
| DS-08 | Annotation closure | The ledger names `3` verticals × `3` tiers, all `9` forms, all families, all page rules, all shared reader states, and all route states. |
| DS-09 | Grid expression | Grid pitch, fade, field, cursor, Content forms, distance, and visual exclusions are explicit. |
| DS-10 | Vocabulary | Canonical product names are used; umbrella terms do not conceal a plane, surface, Content kind, Placement, Layer, route, or state. |

## Required component contract

| Section | Minimum evidence |
| --- | --- |
| Anatomy | Part, requiredness, token or exact value, and attachment. |
| Geometry | Footprint, growth, measure, alignment, spacing, and responsive rule. |
| States | Nine-state appearance and transition rules, including unreachable states. |
| Behaviour | Pointer, keyboard, focus, Escape, commit, cancel, persistence, and return. |
| Motion | Duration, curve, affected properties, loop rule, and reduced-motion result. |
| Distance | Camera relation, projected-size threshold, shedding order, and retained identity. |
| Accessibility | Role, accessible name, focus order, keyboard path, target size, and contrast. |
| Copy | Exact labels, sentence case, punctuation, fallback, and forbidden strings. |
| Refusals | Unsupported, unavailable, invalid, stale, recovery, and no-partial-write behavior. |
| Design assertions | Stable contract assertions; no implementation status or build-diff narrative. |
| Sources | Evidence links only; no conversation context or historical commentary. |

## Annotation closure

| Contract | Owner |
| --- | --- |
| Source record, demand, gate, shares, form matrix, state matrix, and re-resolution order | `10-grammar/Annotation-ledger.md` |
| Demand equations, quality floors, vertical and tier selection, capacity | `10-grammar/Content-concentration.md`; `10-grammar/Content physical geometry.md` |
| Shared reader shell, page geometry, states, and refusals | `30-components/Annotation.md` |
| Brochure, Pamphlet, Magazine and `12` Mixed-media families | `30-components/Mixed-media-annotation.md` |
| Bulletin, Berliner, Broadsheet and `9` Text-led families | `30-components/Text-led-annotation.md` |
| Gallery, Contact sheet, Image edition and `11` Image-led families | `30-components/Image-led-annotation.md` |
| Marker and approach cue | `30-components/Blip.md` |
| Source-owned route entry and destination resolution | `30-components/Annotation-routes.md` |
| Open-reader lifetime, explicit replacement, unavailable and terminal recovery | `30-components/Annotation.md` |
| Closed decisions AN-D01 through AN-D09 | `Annotation decisions.md` |
