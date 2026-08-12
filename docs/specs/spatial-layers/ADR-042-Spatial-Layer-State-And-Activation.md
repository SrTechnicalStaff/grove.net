---
status: "IMPLEMENTED - AWAITING USER REVIEW"
---

# ADR-042: Spatial Layer State and Activation

| Property | Value |
| :--- | :--- |
| **Status** | IMPLEMENTED - AWAITING USER REVIEW |
| **Date** | 2026-08-12 |
| **Area** | Visual Pipeline / Layer Activation / Render Rules / Keyboard Map |
| **Target Runtime** | C# 13 / .NET 9 / Avalonia 11.2.5 / SkiaSharp |
| **Authors** | Chief Product Definition Architect |

---

## 1. Executive Context & State Rendering Principles

In Grove v9, switching focus across layers mutates visual rendering policies and pointer hit-test routing. As established in `docs/design-system/10-grammar/Layer-depth.md` and [ADR-032](file:///C:/dev/grove-v9/docs/specs/hud-system/ADR-032-Spatial-Layer-Manager-And-Navigation.md), exactly ONE layer in the spatial continuum is designated as the **Active Working Layer** at any given moment.

```
Render Loop State Machine (Active vs. Inactive Layers)
─────────────────────────────────────────────────────────────────────────────────────────────
[Begin Frame Render Pass]
          │
          ▼
[Iterate Layer Stack (StackIndex = 0..N-1)]
          │
          ├─────────────────────────────────────────┐
          ▼                                         ▼
[Is Current Layer == Active Layer?]        [Is Current Layer Inactive?]
          │                                         │
          ├──────────────────────────┐              ├──────────────────────────┐
          ▼                          ▼              ▼                          ▼
  [Isolation Mode OFF]    [Isolation Mode ON]  [Isolation Mode OFF]    [Isolation Mode ON]
          │                          │              │                          │
          ▼                          ▼              ▼                          ▼
- Full Content Frames      - Full Content Frames  - Calculate Aura Field   - Zero Render Pass
- Formatted Text & Media   - Formatted Text       - Render Heatmaps        - Skip All Elements
- Interactive Controls     - Interactive Controls - Ghost Silhouettes      - (Pure Isolation)
- Hit-Test ENABLED         - Hit-Test ENABLED     - Hit-Test DISABLED
─────────────────────────────────────────────────────────────────────────────────────────────
```

---

## 2. Active vs. Inactive Layer Rendering Rules Matrix

| Component Feature | Active Layer ($L_{\text{active}}$) | Inactive Layer ($L_{\text{inactive}}$) | Isolation Mode ($L_{\text{active}}$ Solo) |
| :--- | :--- | :--- | :--- |
| **Content Footprint Frame** | Rendered (`1px` `--edge-found` / `--edge-quiet`) | **Unpainted** (Suppressed) | Rendered |
| **Text Content & Media** | Rendered (Full typography, images, canvas strokes) | **Unpainted** (Suppressed) | Rendered |
| **Aura Isoline Perimeter** | Rendered ($1.5\text{px}$ ring at $E \ge 0.15$) | Rendered ($1.5\text{px}$ attenuated ring) | Rendered ($L_{\text{active}}$ local only) |
| **Presence Heatmap** | Rendered (Local cell energy fill) | Rendered (Cross-layer attenuated fill) | Suppressed for other layers |
| **Ghost Silhouettes** | N/A | Rendered ($0.15$ alpha content outline) | **Unpainted** (Suppressed) |
| **Pointer Hit-Testing** | **ACTIVE** (Receives clicks, drag, hover) | **PASS-THROUGH** (Transparent to input) | **ACTIVE** |
| **Selection Marquee** | Enabled | Disabled | Enabled |

### 2.1 Ghost Silhouettes Specification

On inactive layers, content items leave a subtle structural visual footprint termed a **Ghost Silhouette**:
- **Stroke Width**: `1.0px` hairline.
- **Stroke Color**: Authored content hue at $\alpha = 0.15$ (15% opacity).
- **Fill**: Unpainted (100% transparent).
- **Purpose**: Provides spatial boundary awareness of off-layer work without obscuring active text reading or visual focus.

---

## 3. Layer Isolation (Solo) Mode Mechanics

Layer Isolation Mode allows users to temporarily isolate the active layer from all cross-layer field interference.

### 3.1 Mathematical Isolation Rule

When Layer Isolation Mode is enabled on active layer $L_{\text{active}}$:

$$\gamma_{\text{inactive}} = 0.0$$

$$E_{\text{total}}(c, L_{\text{active}}) = E_0 + \sum_{i \in \text{Sources}(L_{\text{active}})} \frac{M_i}{1 + 0.4 \cdot d_{i,c}^2}$$

All energy contributions from sources where $L_i \neq L_{\text{active}}$ are forced to zero ($E_{i \text{ (inactive)}} = 0$). Inactive layer presence heatmaps, perimeter isolines, and ghost silhouettes are unpainted.

---

## 4. Bracket Keyboard Navigation Map & Keybind Architecture

Spatial layer navigation is bound to standard bracket keys across all editor modes, adhering strictly to `docs/reference/Keybind map.md`:

| Key Shortcut | Action Description | Target Layer Effect | System Behavior |
| :--- | :--- | :--- | :--- |
| `]` | **Next Layer Up** | Active Layer $\to L_{\text{stackIndex} + 1}$ | Traverses active focus upward. Clamped at top of stack. |
| `[` | **Previous Layer Down** | Active Layer $\to L_{\text{stackIndex} - 1}$ | Traverses active focus downward. Clamped at bottom of stack. |
| `Shift+]` | **Add Layer Top** | New Layer created at top of stack | Mints next available label (`02`, `03`...), sets active focus. |
| `Shift+[` | **Add Layer Bottom** | New Layer created at bottom of stack | Mints next available label (`B01`, `B02`...), sets active focus. |
| `Ctrl+]` / `Cmd+]` | **Add Layer Above** | New Layer created directly above current | Inserts layer above $L_{\text{active}}$, shifts upper indices. |
| `Ctrl+[` / `Cmd+[` | **Add Layer Below** | New Layer created directly below current | Inserts layer below $L_{\text{active}}$, shifts lower indices. |
| `Alt+]` | **Reorder Layer Up** | Swap $L_{\text{active}}$ with $L_{\text{stackIndex} + 1}$ | Exchanges stack position. Label token PRESERVED. |
| `Alt+[` | **Reorder Layer Down** | Swap $L_{\text{active}}$ with $L_{\text{stackIndex} - 1}$ | Exchanges stack position. Label token PRESERVED. |

---

## 5. Avalonia 11.2.5 Render Pipeline & State Machine

```csharp
namespace Grove.SpatialLayers.State;

using System;
using Avalonia.Input;
using Grove.SpatialLayers.Architecture;

public enum LayerRenderMode
{
    ActiveFull,
    InactivePresenceOnly,
    IsolatedSolo,
    Hidden
}

public readonly record struct LayerStateChangeEventArgs(
    LayerId PreviousActiveLayerId,
    LayerId NewActiveLayerId,
    bool IsolationModeEnabled
);

/// <summary>
/// Core service managing active layer focus, isolation state, and navigation shortcuts.
/// </summary>
public interface ILayerActivationManager
{
    LayerId ActiveLayerId { get; }
    bool IsIsolationModeEnabled { get; }
    
    event EventHandler<LayerStateChangeEventArgs>? ActiveLayerChanged;
    
    void ActivateLayer(LayerId id);
    void ActivateNextLayer();
    void ActivatePreviousLayer();
    void ToggleIsolationMode();
    
    LayerRenderMode GetRenderMode(LayerId id);
}

public sealed class LayerActivationManager : ILayerActivationManager
{
    private readonly ISpatialLayerStackManager _stackManager;
    public bool IsIsolationModeEnabled { get; private set; }

    public LayerActivationManager(ISpatialLayerStackManager stackManager)
    {
        _stackManager = stackManager;
    }

    public LayerId ActiveLayerId => _stackManager.CurrentStack.ActiveLayerId;

    public event EventHandler<LayerStateChangeEventArgs>? ActiveLayerChanged;

    public void ActivateLayer(LayerId id)
    {
        if (id == ActiveLayerId) return;

        var prev = ActiveLayerId;
        _stackManager.SetActiveLayer(id);
        
        ActiveLayerChanged?.Invoke(this, new LayerStateChangeEventArgs(prev, id, IsIsolationModeEnabled));
    }

    public void ActivateNextLayer()
    {
        var stack = _stackManager.CurrentStack;
        int index = stack.GetStackIndex(ActiveLayerId);
        if (index < stack.Layers.Count - 1)
        {
            ActivateLayer(stack.Layers[index + 1].Id);
        }
    }

    public void ActivatePreviousLayer()
    {
        var stack = _stackManager.CurrentStack;
        int index = stack.GetStackIndex(ActiveLayerId);
        if (index > 0)
        {
            ActivateLayer(stack.Layers[index - 1].Id);
        }
    }

    public void ToggleIsolationMode()
    {
        IsIsolationModeEnabled = !IsIsolationModeEnabled;
        ActiveLayerChanged?.Invoke(this, new LayerStateChangeEventArgs(ActiveLayerId, ActiveLayerId, IsIsolationModeEnabled));
    }

    public LayerRenderMode GetRenderMode(LayerId id)
    {
        var stack = _stackManager.CurrentStack;
        int layerIdx = stack.GetStackIndex(id);
        if (layerIdx < 0 || !stack.Layers[layerIdx].IsVisible)
            return LayerRenderMode.Hidden;

        if (id == ActiveLayerId)
            return IsIsolationModeEnabled ? LayerRenderMode.IsolatedSolo : LayerRenderMode.ActiveFull;

        return IsIsolationModeEnabled ? LayerRenderMode.Hidden : LayerRenderMode.InactivePresenceOnly;
    }
}
```

---

## 6. Key Routing & Keyboard Navigation Handler

```csharp
namespace Grove.SpatialLayers.Navigation;

using Avalonia.Input;
using Grove.SpatialLayers.Architecture;
using Grove.SpatialLayers.State;

public sealed class SpatialLayerNavigationHandler
{
    private readonly ILayerActivationManager _activationManager;
    private readonly ISpatialLayerStackManager _stackManager;

    public SpatialLayerNavigationHandler(
        ILayerActivationManager activationManager, 
        ISpatialLayerStackManager stackManager)
    {
        _activationManager = activationManager;
        _stackManager = stackManager;
    }

    public bool HandleKeyDown(KeyEventArgs e)
    {
        bool isShift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        bool isCtrlCmd = e.KeyModifiers.HasFlag(KeyModifiers.Control) || e.KeyModifiers.HasFlag(KeyModifiers.Meta);
        bool isAlt = e.KeyModifiers.HasFlag(KeyModifiers.Alt);

        switch (e.Key)
        {
            case Key.OemCloseBrackets: // ']' key
                if (isShift)
                    _stackManager.AddLayerAbove(_stackManager.CurrentStack.Layers[^1].Id);
                else if (isCtrlCmd)
                    _stackManager.AddLayerAbove(_activationManager.ActiveLayerId);
                else if (isAlt)
                    MoveActiveLayerRelative(+1);
                else
                    _activationManager.ActivateNextLayer();
                return true;

            case Key.OemOpenBrackets: // '[' key
                if (isShift)
                    _stackManager.AddLayerBelow(_stackManager.CurrentStack.Layers[0].Id);
                else if (isCtrlCmd)
                    _stackManager.AddLayerBelow(_activationManager.ActiveLayerId);
                else if (isAlt)
                    MoveActiveLayerRelative(-1);
                else
                    _activationManager.ActivatePreviousLayer();
                return true;

            case Key.I:
                if (isCtrlCmd)
                {
                    _activationManager.ToggleIsolationMode();
                    return true;
                }
                break;
        }

        return false;
    }

    private void MoveActiveLayerRelative(int delta)
    {
        var stack = _stackManager.CurrentStack;
        int currentIdx = stack.GetStackIndex(_activationManager.ActiveLayerId);
        int targetIdx = currentIdx + delta;

        if (targetIdx >= 0 && targetIdx < stack.Layers.Count)
        {
            _stackManager.MoveLayer(_activationManager.ActiveLayerId, targetIdx);
        }
    }
}
```

---

## 7. Architectural Traceability & References

- **Design System Grammar**: `docs/design-system/10-grammar/Layer-depth.md`, `docs/design-system/10-grammar/States.md`
- **Keybind Specifications**: `docs/reference/Keybind map.md`
- **Component Docs**: `docs/design-system/30-components/Layer-manager.md`
- **Related Specs**: [ADR-032](file:///C:/dev/grove-v9/docs/specs/hud-system/ADR-032-Spatial-Layer-Manager-And-Navigation.md), [ADR-040](file:///C:/dev/grove-v9/docs/specs/spatial-layers/ADR-040-Spatial-Layer-System-Architecture.md), [ADR-041](file:///C:/dev/grove-v9/docs/specs/spatial-layers/ADR-041-Vertical-Aura-Permeability-And-Attenuation.md)
