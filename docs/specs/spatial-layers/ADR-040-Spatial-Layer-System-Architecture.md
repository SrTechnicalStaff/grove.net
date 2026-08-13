---
status: "NORMATIVE — Grid Layer continuum"
---

# ADR-040: Grid Layer System Architecture

## Decision

The Grid is a shared `(x, y)` coordinate space extended by a literal,
unbounded Grid Layer continuum. Grid Layer `01` is the protected ground
member; labels above and below it are stable side-coded identities. Grid
Layers are spatial depth, not documents, tabs, visual Planes, or HUD Tiers.

Every Grid Layer remains a real participant in the spatial model. Content on
different Grid Layers may occupy the same cells. Same-Grid-Layer Content may
not overlap. Aura permeability is calculated from source and target Grid Layer
stack distance, independently of which Grid Layer is selected for commands.

## State boundary

The stack stores ordered Grid Layer identity, stable label, name, visibility,
lock state, and color metadata. It also stores `SelectedGridLayerId` as command
context. Selection is not a layer lifecycle state:

- all visible Grid Layers render Content and contribute Aura;
- selection chooses the target for new Content, direct manipulation, trace,
  navigation, insertion, and reordering;
- visibility is an explicit rendering preference;
- locking refuses mutation without suppressing rendering or field contribution.

The retired Grid-Layer activation and isolation model is not part of the architecture.

## Deep seam

```csharp
public interface ISpatialLayerStateService
{
    IReadOnlyList<SpatialLayerModel> Layers { get; }
    SpatialLayerModel SelectedGridLayer { get; }
    void SelectGridLayer(int zIndex);
    double GetPermeability(int sourceGridLayerId, int targetGridLayerId);
    double ApplyPermeability(double sourceEnergy, int sourceGridLayerId, int targetGridLayerId);
}
```

Layer insertion, reordering, removal migration, stable label generation, and
vertical permeability stay behind this seam. Plane 0 asks the seam for
geometry and field projection; it does not invent layer state.

## Rendering order

The stack order is used to draw visible Content deterministically. It does not
select one Grid Layer for full rendering and demote the rest to presence-only
rendering. Same-layer perimeter contours use the projected Grid Layer's
same-layer contribution; cross-layer Aura remains a field contribution only.
