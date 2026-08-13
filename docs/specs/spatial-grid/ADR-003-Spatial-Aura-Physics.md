---
status: "PARTIAL — discrete physics, permeability, culling, and same-layer contour inputs are implemented; zoom-tier completion remains"
authority: normative
date: 2026-08-12
---

# ADR-003: Spatial Aura Physics

## Decision

Aura is emitted by Content into surrounding Grid cells. It is not emitted by
Memory, and it is not a state of Memory. The field is evaluated per discrete
cell and may saturate across Grid Layers through permeability.

Presence must remain visible as the camera changes scale. Zoom changes the
representation Tier; it does not switch Aura off or replace it with a second
single-cell model.

## Physics

For Content mass `M`, cell distance `d`, and Grid Layer stack distance `ΔL`:

```text
E_i(d, ΔL) = M / (1 + 0.4 · d²) · 0.5^|ΔL|
E_cell      = 0.05 + ΣE_i
```

Mass is `1.0` for Note, `2.5` for Document, and `4.0` for Image. The spatial
support cutoff is six Grid cells and the supported permeability range is the
visible stack neighborhood. These are calculation bounds, not permission to
drop an Aura merely because the camera is zoomed out.

The field snapshot must be computed from a zoom-aware viewport envelope that
contains the complete support of sources near the viewport edge. Semantic
field queries are not viewport-cropped.

## Perimeter

Same-Grid-Layer Content energy at or above `0.15` defines the perimeter domain.
An edge is emitted only when its neighbor is below the threshold. Internal
edges are omitted and touching regions form one continuous boundary.

Cross-Grid-Layer energy can affect cell fills, metadata, and recall ranking. It
cannot create, extend, merge, or recolor a same-Grid-Layer perimeter.

Every contour uses the additive Aura hue of the same-Grid-Layer sources that
support that contour. The interaction signal is not a contour color.

## Zoom Tiers

The canonical cursor and Content representation use the projected Grid cell
pitch to choose one of the defined zoom Tiers. The Tier changes detail and
cursor footprint representation only. There is no parallel single-cell cursor
path and no distance-based Aura disappearance at macro zoom.

## Selection

When Content is selected, the renderer highlights the selected Content source
contribution across its Aura cells. A shared cell retains unselected field
evidence. The selection mark is discrete and source-colored; it is not a
gradient, halo, glow, or replacement composite.

## Acceptance

The ADR is complete when source mass, permeability, threshold, same-layer
contours, zoom-tier presence, viewport-edge continuity, and selected-source
Aura highlighting are covered by executable tests and live verification.
