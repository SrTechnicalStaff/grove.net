---
status: "PARTIAL — discrete ledger, additive hue, selected-source projection, and subscriber batch behavior are implemented; complete reactive integration remains"
authority: normative
date: 2026-08-12
---

# ADR-002: Field Ledger and Subscribers

## Decision

The Field Ledger is a runtime projection of Content into Grid cells. A Memory
without Content emits no field. The ledger stores baseline energy, total
energy, source provenance, source hue, and the target Grid Layer used for the
calculation.

The ledger is semantic evidence, not persistent Memory state. It is rebuilt
from Content mutations and is never allowed to change a Memory record.

## Cell contract

`CellLedgerEntry` is a readonly record struct containing:

- the integer cell position and target Grid Layer;
- baseline energy `0.05`;
- total energy;
- source count and inline provenance slots `InlineSource0` through
  `InlineSource3`;
- the additive display hue;
- the independently clamped display alpha.

The inline slots are a hot-path representation. A selection or metadata query
must use the Field Ledger's source-evidence query when more than four sources
contribute to a cell; it must not silently treat four slots as the complete
source set.

## Update contract

`IFieldSubscriber` receives either a single immutable entry or a contiguous
`ReadOnlySpan<CellLedgerEntry>` batch. Recalculation happens outside drawing
passes. Rendering consumes the latest snapshot and never mutates the ledger.

The required subscribers are:

- `AuraHeatmapSubscriber`: computes display hue and alpha;
- `PerimeterRingSubscriber`: computes same-Grid-Layer perimeter edges;
- `AnnotationMetadataSubscriber`: retains qualified source metadata for
  Information Plane queries;
- the selection-field projection: returns selected-source contribution for
  each affected cell without replacing the passive field.

## Hue and alpha

Hue composition is additive. For source hue vectors `C_i` and contributions
`E_i`, the raw channel sum is:

```text
S = Σ(E_i · C_i)
```

`S` is projected into the display byte range by the fixed display transfer
defined by the implementation. Dividing by `ΣE_i` to calculate a midpoint or
weighted average is forbidden. Alpha is calculated independently from total
field energy and clamped to the design-system range `0.025` through `0.30`.

## Selection field

Selection does not wash or replace the passive field. For selected Content
IDs, the ledger exposes:

```text
selectedEnergy(c) = Σ(E_i(c) for selected sources i)
selectedHue(c)    = additive sum of selected source hues
```

Only cells with selected contribution are highlighted. Unselected source
contributions remain visible beneath that discrete selection mark.

## Presence refusals

Aura presence is cell-bounded. Radial gradients, halos, blur, glow, Gaussian
falloff shaders, and any effect crossing cell boundaries are not valid Field
Ledger renderings. Grid subdivision lines are suppressed only inside active
Aura cells; the field itself remains a discrete cell fill.

## Acceptance

The ADR is complete when field updates are mutation-triggered, batch-published,
selection-aware, same-layer contour-aware, and verified at zoom extremes
without a render pass mutating field state.
