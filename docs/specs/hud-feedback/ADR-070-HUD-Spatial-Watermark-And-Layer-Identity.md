---
status: "PARTIAL — watermark is wired; full ledger-backed metadata contract remains"
---

# ADR-070: HUD Spatial Watermark and Selected Grid Layer Identity

The watermark is a viewport-fixed HUD Plane element. It reports camera scale
and the selected Grid Layer command context. It does not claim that other Grid
Layers are inactive, hidden, or absent.

## Contract

- The scale readout is non-interactive and passes pointer input through.
- The selected Grid Layer identity is the only interactive portion.
- Renaming updates Grid Layer metadata through the state service and is
  reflected in the Grid Layer Manager.
- Selecting a different Grid Layer updates the watermark without changing the
  existence, rendering participation, or Aura contribution of any other Grid
  Layer.
- The watermark belongs to the HUD Plane; it is not a Grid Layer and does not
  create a spatial ledger.

## Runtime seam

```csharp
public interface IHudSpatialWatermarkBinding
{
    HudCameraState CameraState { get; }
    HudLayerIdentity SelectedGridLayer { get; }
    event Action<HudCameraState>? CameraStateChanged;
    event Action<HudLayerIdentity>? SelectedGridLayerChanged;
    ValueTask<HudLayerRenameValidation> CommitLayerRenameAsync(
        HudLayerRenameRequest request,
        CancellationToken cancellationToken = default);
}
```

The former activation-state language is retired. The watermark is a
selection indicator, not an activation indicator, and it is not evidence of a
layer-specific ledger.
