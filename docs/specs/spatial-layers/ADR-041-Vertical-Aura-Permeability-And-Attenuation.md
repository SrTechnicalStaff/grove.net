---
status: "PARTIAL — cross-Grid-Layer attenuation, source projection, same-Grid-Layer contour integration, and zoom-safe snapshot coverage are implemented; optimized execution remains"
authority: normative
date: 2026-08-12
---

# ADR-041: Vertical Aura Permeability and Attenuation

## Decision

Aura emitted by Content may contribute to cells on other Grid Layers. The
source remains Content on its original Grid Layer; permeability changes only
the contribution observed by the target Grid Layer.

```text
E_i(d, ΔL) = M_i / (1 + 0.4 · d²) · 0.5^|ΔL|
```

Mass is `1.0` for Note, `2.5` for Document, and `4.0` for Image. Spatial
support is bounded at six cells and the supported stack neighborhood is
bounded by the active Grid Layer range. These bounds optimize calculation and
do not make cross-Grid-Layer field evidence disappear from semantic queries.

## Color and contour rules

Source hues are accumulated additively by contribution. They are not blended
into a midpoint color and are not averaged by total weight.

Cross-Grid-Layer contributions may affect discrete fills, metadata, and recall
ranking. A contour is built only from same-Grid-Layer Content energy at or
above the perimeter threshold `0.15`. Its color is the additive Aura color of
that same-Grid-Layer source set.

## Acceptance

The implementation is complete when stack distance, attenuation,
source-colored same-Grid-Layer contours, selected-source projection, and
viewport-edge continuity are covered by executable tests. A SIMD or
logarithmic-alpha implementation is not required unless it produces an
observable product outcome and has a real adapter seam.
