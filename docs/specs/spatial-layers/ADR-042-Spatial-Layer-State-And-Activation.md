---
status: "NORMATIVE — Grid Layer selection and rendering"
supersedes: "The retired Grid Layer activation and isolation model previously described here"
---

# ADR-042: Grid Layer Selection and Rendering

## Decision

A Grid Layer is a persistent depth member of the Grid. Every Grid Layer is a
real, simultaneously participating spatial surface. There is no active Grid
Layer, inactive Grid Layer, working Grid Layer, solo Grid Layer, or
presence-only Grid Layer.

The application keeps one `SelectedGridLayerId` as command context. Selection
answers “which Grid Layer receives a new Content placement, keyboard navigation,
direct pointer editing, or a trace target?” It does not answer “which Grid
Layer exists” or “which Grid Layers render.”

## Rendering contract

- Every visible Grid Layer renders its Content geometry and contributes its
  Content to the Field Ledger.
- Every Grid Layer participates in cross-Grid-Layer Aura permeability.
- Same-Grid-Layer contours are derived from the projected Grid Layer only and
  never cross, merge through, or recolor another Grid Layer.
- Pointer hit-testing and direct manipulation use the selected Grid Layer to
  resolve ambiguity when multiple Grid Layers occupy the same `(x, y)` cells.
- A Grid Layer's `IsVisible` flag is an explicit rendering preference. It is
  not an activation state and does not create an inactive-layer mode.
- A Grid Layer's `IsLocked` flag is an edit refusal boundary. It is not a
  rendering or participation state.
- There are no ghost silhouettes, inactive presence-only frames, or isolation
  mode. A Grid Layer is never rendered differently merely because another Grid
  Layer is selected.

## Navigation and commands

`[`, `]`, top/bottom navigation, insertion, deletion, and reordering update the
selected Grid Layer command target. Insertion selects the newly created Grid
Layer so the next placement is deterministic. The former Ctrl+I isolation
command is removed; it had no valid product meaning under the Grid Layer
model.

## Runtime seam

The executable seam is `ISpatialLayerStateService`:

```csharp
SpatialLayerModel SelectedGridLayer { get; }
event Action<SpatialLayerModel>? SelectedGridLayerChanged;
void SelectGridLayer(int zIndex);
```

`FieldLedgerEngine.ProjectionGridLayerId` names the Grid Layer onto which the
current Plane 0 field projection is sampled. It is a projection coordinate,
not an activation state.

## Refusals

- No `Selected-as-activation`, `InactiveGridLayer`, `LayerActivationManager`, or
  `LayerRenderMode` vocabulary.
- No isolation/solo mode that suppresses other Grid Layers.
- No cross-Grid-Layer ghost content pass.
- No layer state hidden inside a Plane or HUD Tier. Plane and Tier remain
  visual composition concepts; Grid Layer remains literal spatial depth.
