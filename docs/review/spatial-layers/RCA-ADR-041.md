# RCA Ledger: ADR-041 — Vertical Aura Permeability and Attenuation Physics

| Metadata Field | Value |
| :--- | :--- |
| **ADR ID** | ADR-041 |
| **Verified Status** | **PARTIAL — cross-Grid-Layer attenuation, culling, and normalized hue accumulation are live; the standalone SIMD engine and log-alpha contract are not** |
| **Audit Date** | 2026-08-12 |

## Verified implementation

- `SpatialLayerStack.GetPermeability` and `FieldLedgerEngine.LayerDeltaResolver`
  apply (0.5^{|ΔL|}) to source energy across Grid Layers.
- `FieldLedgerEngine` applies the ADR mass equation, limits influence to the
  token-backed spatial cull radius, and rejects sources outside the supported
  Grid Layer attenuation range.
- `CellLedgerEntry` stores inline source metadata and `AuraHeatmapSubscriber`
  computes normalized energy-weighted hue contributions with bounded alpha.
- Perimeter rendering qualifies exposed edges using the energy threshold and
  does not draw interior cross-lines.

## Remaining gaps

- There is no separate `VerticalAuraPermeabilityEngine` SIMD implementation;
  scalar physics remains inside the field engine and Grid Layer stack seam.
- The renderer uses the accepted discrete Presence model. Continuous isolines,
  Gaussian gradients, and the ADR-041 logarithmic-alpha variant remain absent
  where they conflict with the explicit no-gradient refusal.

## Root cause of the original false claim

The earlier RCA was captured before cross-Grid-Layer source accumulation and
normalized hue qualification were wired and reported the old active-layer-only
implementation as current.
