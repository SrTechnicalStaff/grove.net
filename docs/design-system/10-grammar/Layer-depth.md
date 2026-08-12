---
type: design-system-grammar
status: normative
date: 2026-08-10
owner: "Grove Design System"
---

# Layer depth

| Property | Canonical value |
| --- | --- |
| Layer count | Unbounded positive integer. |
| Layer identity | Stable `LayerId`; order is the explicit `layerOrder` sequence. |
| Placement eligibility | A Placement may exist on any Layer in `layerOrder`; field saturation never rejects, moves, or deletes it. |
| Same-Layer occupancy | Placement rectangles may not overlap on one Layer. Different Layers may occupy the same cells. |
| Field rendering | Contributions are attenuated by Layer distance and may fall below the declared rendering floor. |
| Visibility consequence | A contribution below the floor is not painted; its Placement remains durable and navigable. |
| Layer 15 | Valid. No special case, refusal, compression, or automatic merge. |
| Layer 16+ | Same rule as Layer 15. |
| Persistence | Layer order, Layer identity, and Placement Layer identity persist. Presence is derived and never stored. |

## Procedure

| Step | Rule |
| --- | --- |
| 1 | Resolve the current Layer from `view.currentLayerId`. |
| 2 | Resolve every Placement by its own `layerId`; do not cap the Layer sequence. |
| 3 | Use Layer distance only for field contribution attenuation and rendering work. |
| 4 | Render current-Layer Content only; render other-Layer presence only. |
| 5 | Keep every Layer addressable by Layer navigation and Layer management. |

## Refusals

| Refusal | Required result |
| --- | --- |
| Rejecting a new Layer because field saturation is high | Create the Layer. |
| Moving Content to a different Layer to reduce field density | Preserve the requested Layer and Placement identity. |
| Deleting or merging deep Layers automatically | Refuse the automatic mutation. |
| Drawing another Layer's Content frame | Draw presence only. |
| Treating rendering falloff as a data limit | Keep the durable Layer and Placement. |
