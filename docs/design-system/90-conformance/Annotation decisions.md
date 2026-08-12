---
type: design-system-decision-register
status: normative
date: 2026-08-10
owner: "Grove Design System"
---

# Annotation decisions

| Decision ID | Contract | Canonical rule |
| --- | --- | --- |
| AN-D01 | Demand fixture | The reference calculation uses `measurePx = 340`, `fontSizePx = 16`, `lineHeight = 1.55`, `linesPerPage = 18`, and image fidelity floors `F = 96` thumbnail, `180` supporting, `320` leading. Implementations scale these inputs with the available surface while preserving the calculation order and demand semantics. |
| AN-D02 | Boundary hysteresis | A reading retains its current form at `D = 1.75`, `D = 3.0`, `imageShare = 0.25`, and `imageShare = 0.70`. A form changes only after the next complete-fit evaluation places the source set on the other side of the boundary and the target form can render every source without truncation, crop, or type reduction. No additional numeric buffer exists. |
| AN-D03 | Unavailable source after qualification | Retain the last committed demand record, vertical, tier, form, family, order, page count, and active source. Replace only the unavailable source body/frame with its unavailable state. Do not recalculate shares from the missing source and do not remove it from the reading. |
| AN-D04 | Unavailable source before qualification | A source set with an unavailable source and no committed demand record does not qualify for a marker or Annotation. It remains ordinary Content until all values required for the gate and share calculation are available. |
| AN-D05 | Page-local capacity | A family is valid only when its complete-fit predicate passes for every source assigned to its page, leaf, spread, or frame. Selection order is same-tier family → additional columns/regions where allowed → additional pages/leaves → next tier form. No independent numeric capacity scale is added. |
| AN-D06 | Mixed-media family count | The selectable Mixed-media family set is exactly `B-01`–`B-04`, `P-01`–`P-04`, and `M-01`–`M-04`: twelve families. The catalogue's named fold descriptions are the four Brochure families; no additional fold is selectable. |
| AN-D07 | Publication state | Annotation publication uses the common reader states: Rest, Approached, Focused, Selected, Engaged, Pending, Refused, Unavailable, and Anchored. No separate publication lifecycle state is added. |
| AN-D08 | Route menu entry | A person chooses a source in the reading, then opens that source's route menu. The menu is source-owned; it is never a reading-wide route list. |
| AN-D09 | Multiple Placements | `Show on the Grid` resolves the unique Placement that contributed the source's qualifying Field Ledger observation. If no unique Placement can be established, the route is unavailable and the Annotation remains open; no arbitrary Placement is selected. |

## Required resolution order

| Step | Rule |
| --- | --- |
| 1 | Record source identity, current availability, Placement contribution, and source order. |
| 2 | Compute per-source demand using the shared fixture equations. |
| 3 | Compute `D`, `textShare`, `imageShare`, `otherShare`, and `Rmedia`. |
| 4 | Apply the `D ≥ 1.0` and field-support gates. |
| 5 | Select Text-led, Mixed-media, or Image-led from the shared share boundaries. |
| 6 | Select Tier 01, Tier 02, or Tier 03 from the shared `D` bands. |
| 7 | Select the form and then the allowed layout family using complete-fit predicates. |
| 8 | Paginate, preserve source order and active source, and expose source routes. |

## Required re-resolution behavior

| Trigger | Result |
| --- | --- |
| Source add/remove, text edit, image replacement, role change, or surface-class change | Recompute from Step 1. |
| Boundary equality | Retain the current form. |
| Complete-fit failure in the current family | Select another same-tier family, add permitted capacity, or promote. |
| Source becomes unavailable after commitment | Apply AN-D03; retain the edition and source position. |
| Route destination becomes unavailable | Refuse inline; retain the reading and its page/source selection. |

## Source basis

| Evidence | Role |
| --- | --- |
| `docs/design_catalogue/src/information-plane/04-annotation-markers.html` | Gate, pin, return, unavailable-source state, and nine-state journey. |
| `docs/design_catalogue/src/information-plane/05-annotation-mixed-media.html` | Demand equations, gate, share routing, twelve-family ledger, unavailable demand, and re-resolution. |
| `docs/design_catalogue/src/information-plane/06-annotation-text-led.html` | Text forms, capacity order, boundary retention, and complete-fit promotion. |
| `docs/design_catalogue/src/information-plane/07-annotation-image-led.html` | Image demand, fidelity, complete-frame selection, pagination, and boundary retention. |
| `docs/design_catalogue/src/information-plane/08-annotation-routes.html` | Source-owned route menu, route destinations, pin/return, unavailable routes, and refusal behavior. |
