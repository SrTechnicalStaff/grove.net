---
status: "CONFLICTING — REFUSED BY ADR-002 AND THE PRESENCE CONTRACT"
authority: historical
date: 2026-08-12
---

# ADR-061: Aura Field Fluid Gradient Rendering

This proposal is refused and must not be implemented.

Gradient shaders, radial fields, blur, glow, Gaussian falloff, continuous
isoline fills, and any other Aura effect that crosses Grid cell boundaries
contradict ADR-002, ADR-003, and the accepted Presence contract.

The active renderer uses discrete cell-bounded fills, same-Grid-Layer Aura
contours, and source-colored selection evidence. Grid subdivision lines may be
suppressed inside active cells, but the field itself remains quantized.
