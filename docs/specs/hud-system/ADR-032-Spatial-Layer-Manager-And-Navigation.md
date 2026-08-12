---
status: "Accepted"
---

# ADR-032: Spatial Layer Manager and Navigation Specifications

| Property | Value |
| :--- | :--- |
| **Status** | Accepted |
| **Date** | 2026-08-12 |
| **Area** | Spatial Layers / Field Physics / Layer Navigation |
| **Target Runtime** | C# 13 / .NET 9 / Avalonia 11.2.5 |
| **Authors** | Chief Product Definition Architect |

---

## 1. Executive Context & Architectural Principles

As established in [ADR-003](file:///C:/dev/grove-v9/docs/specs/spatial-grid/ADR-003-Spatial-Aura-Physics.md), `docs/design-system/10-grammar/Layer-depth.md`, and `docs/design-system/30-components/Layer-manager.md`, Grove v9 spatial organization is structured across an unbounded stack of spatial **Layers**. 

A Layer is NOT a destructive container or isolated canvas tab. A Layer is an addressable spatial frequency band in a continuous depth stack through which the aura field saturates.

```
Layer Stack (Depth View)
─────────────────────────────────────────────────────────────────
Layer 03 (Upper Band)   [ Content Footprint ] (Full Render on active)
                          │   Aura Decay γ = 0.5
                          ▼
Layer 02 (Active Band)   [ Active Working Layer ] <── User Focus
                          ▲   Aura Decay γ = 0.5
                          │
Layer 01 (Base Band)     [ Content Footprint ] (Presence Heatmap Only)
─────────────────────────────────────────────────────────────────
```

### Architectural Rules
1. **Unbounded Layer Stack**: The spatial stack supports an unbounded number of layers ($L_0, L_1, L_2, \dots, L_n$). Layer `01` is protected as the anchor base layer.
2. **Stable Side-Coded Labels**: Layers mint stable label tokens (`01`, `02`, `03` above base layer; `B01`, `B02` below base layer). Reordering layers changes position in stack order, but NEVER renumbers stable layer labels.
3. **Active-Only Content Frame Rendering**:
   - The **Active Layer** renders full content footprints (text, images, interactive controls, selection marquees).
   - **Inactive Layers** render presence heatmaps and aura isoline fields ONLY. Content frames and text details on inactive layers are unpainted to preserve visual clarity.
4. **Vertical Field Saturation**: Placed work on inactive layers radiates continuous energy into the active layer via vertical aura permeability.
5. **Preservation of Placed Work**: Removing a layer NEVER deletes placed content. Content on a removed layer automatically migrates to the adjacent surviving layer in the stack.

---

## 2. Mathematical Model of Layer Depth Decay

Content placed on layer $L_i$ casts gravitational energy into every layer $L_{\text{target}}$ in the stack. The vertical energy saturation is governed by the layer-depth decay factor $\gamma = 0.5$.

### 2.1 Gravitational Field Equation with Vertical Attenuation
The energy $E_i(d, \Delta L)$ contributed by content item $i$ (mass $M_i$) onto a grid cell at Euclidean distance $d$ across layer depth distance $|\Delta L| = |L_{\text{target}} - L_{\text{source}}|$ is:

$$E_i(d, \Delta L) = \frac{M_i}{1 + 0.4 \cdot d^2} \cdot \gamma^{|\Delta L|} \quad \text{where } \gamma = 0.5$$

Where:
- $M_i \ge 1.0$: Mass of placed content item $i$ (Note $M=1.0$, Document $M=2.5$, Image $M=4.0$).
- $d = \sqrt{\Delta x^2 + \Delta y^2}$: Euclidean distance in cell units from the content footprint.
- $|\Delta L| = |L_{\text{target}} - L_{\text{source}}|$: Absolute integer difference between layer stack positions.

### 2.2 Composite Layer Field Equation
The total composite energy $E_{\text{total}}(c, L_{\text{active}})$ at cell position $c(x,y)$ on active layer $L_{\text{active}}$ combines baseline field energy $E_0 = 0.05$ with all active and inactive layer sources:

$$E_{\text{total}}(c, L_{\text{active}}) = E_0 + \sum_{i \in \text{All Sources}} \left( \frac{M_i}{1 + 0.4 \cdot d_{i,c}^2} \cdot (0.5)^{|L_{\text{active}} - L_i|} \right)$$

### 2.3 Layer Attenuation Cascade Table

| Layer Delta $|\Delta L|$ | Attenuation Factor $\gamma^{|\Delta L|}$ | Energy from Mass $M=1.0$ at $d=0$ | Visible Perimeter Ring ($E \ge 0.15$)? |
| :--- | :--- | :--- | :--- |
| **$\Delta L = 0$ (Same Layer)** | $0.5^0 = 1.000$ | $E = 1.050$ | **YES** (Strong perimeter ring) |
| **$\Delta L = 1$ (Adjacent Layer)** | $0.5^1 = 0.500$ | $E = 0.550$ | **YES** (Moderate presence aura) |
| **$\Delta L = 2$ (2 Layers Away)** | $0.5^2 = 0.250$ | $E = 0.300$ | **YES** (Faint presence aura) |
| **$\Delta L = 3$ (3 Layers Away)** | $0.5^3 = 0.125$ | $E = 0.175$ | **YES** (Threshold boundary) |
| **$\Delta L \ge 4$ (4+ Layers Away)** | $0.5^4 = 0.0625$ | $E = 0.1125 < 0.15$ | **NO** (Falls below perimeter floor) |

---

## 3. Layer Activation & Rendering Mechanics

When a user switches active layers, the visual render loop dynamically adjusts rendering policies per layer.

```text
[Render Pass Begin: Frame N]
         │
         ▼
[Iterate Spatial Layer Stack]
         │
   ┌─────┴─────────────────────────────────────────────┐
   │                                                   │
[Is Current Layer == Active Layer?]        [Is Current Layer Inactive?]
   │                                                   │
   ▼                                                   ▼
- Render Full Content Footprints           - Calculate Aura Energy Contributions
- Render Text / Image Pixels               - Accumulate into Cell Ledger
- Render Active Focus / Selection Rings    - Draw Presence Heatmaps Only
- Enable Input Hit-Testing                 - Suppress Text/Frame Rendering
```

### 3.1 Content Migration on Layer Removal
When layer $L_{\text{target}}$ is removed:
1. If $L_{\text{target}}$ contains placed content, open an inline confirm in the Layer Manager Slate naming the destination layer: `"Remove Layer B02 and move what is on it to Layer 01?"`.
2. Upon confirmation, all content footprints on $L_{\text{target}}$ are reassigned to $L_{\text{destination}}$ (the next layer down, or next layer up if removing bottom layer).
3. If destination cells are occupied, the confirm transitions to refusal state: `"That space is occupied on Layer 01."`.

---

## 4. Bracket Navigation Mechanics & Keybind Specification

Layer navigation and stack management are driven via ergonomic bracket key combinations (`[` / `]`), declared in `docs/reference/Keybind map.md`.

### 4.1 Keybind Command Table

| Keybind | Command Action | Context & Pass-Through Behavior |
| :--- | :--- | :--- |
| `[` | **Traverse Down**: Activate previous layer down in stack. | Global keybind. Unbound in normal grid state. |
| `]` | **Traverse Up**: Activate next layer up in stack. | Global keybind. Unbound in normal grid state. |
| `Shift+[` | **Add Bottom Layer**: Create new layer at bottom of stack. | Mint label `B0x`. Focuses new layer. |
| `Shift+]` | **Add Top Layer**: Create new layer at top of stack. | Mint label `0x`. Focuses new layer. |
| `Ctrl+[` / `Cmd+[` | **Add Layer Below**: Create layer immediately below active layer. | Inserts into stack sequence. Preserves labels. |
| `Ctrl+]` / `Cmd+]` | **Add Layer Above**: Create layer immediately above active layer. | Inserts into stack sequence. Preserves labels. |
| `Alt+[` | **Move Layer Down**: Reorder active layer down by 1 position. | Stack reorder only. Stable label unchanged. |
| `Alt+]` | **Move Layer Up**: Reorder active layer up by 1 position. | Stack reorder only. Stable label unchanged. |
| `L` | **Toggle Layer Manager Slate**: Open/Close Layer Manager on Plane 2. | Focuses active layer row in Slate. |

---

## 5. C# 13 / Avalonia 11.2.5 Implementation Contracts

```csharp
namespace Grove.SpatialGrid.Layers;

using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Input;

public sealed record SpatialLayerId(Guid Value);

public sealed class SpatialLayer
{
    public SpatialLayerId Id { get; } = new(Guid.NewGuid());
    public required string LabelToken { get; init; } // e.g. "01", "02", "B01"
    public string Name { get; set; } = string.Empty;
    public int StackOrder { get; set; }
    public bool IsProtectedBaseLayer { get; init; }
}

public interface ISpatialLayerManager
{
    SpatialLayer ActiveLayer { get; }
    IReadOnlyList<SpatialLayer> LayerStack { get; }
    
    void ActivateLayer(SpatialLayerId layerId);
    SpatialLayer CreateLayerAbove(SpatialLayerId targetLayerId);
    SpatialLayer CreateLayerBelow(SpatialLayerId targetLayerId);
    void ReorderLayer(SpatialLayerId layerId, int newStackOrder);
    bool TryRemoveLayer(SpatialLayerId layerId, out SpatialLayerId? destinationLayerId);
}

/// <summary>
/// Core spatial layer stack engine managing layer ordering, activation, and aura attenuation.
/// </summary>
public sealed class SpatialLayerManagerEngine : ISpatialLayerManager
{
    private readonly List<SpatialLayer> _layers = new();
    private SpatialLayer _activeLayer;

    public SpatialLayerManagerEngine()
    {
        // Initialize protected base layer 01
        var baseLayer = new SpatialLayer
        {
            LabelToken = "01",
            Name = "Base Layer",
            StackOrder = 0,
            IsProtectedBaseLayer = true
        };
        _layers.Add(baseLayer);
        _activeLayer = baseLayer;
    }

    public SpatialLayer ActiveLayer => _activeLayer;
    public IReadOnlyList<SpatialLayer> LayerStack => _layers.OrderBy(l => l.StackOrder).ToList();

    public void ActivateLayer(SpatialLayerId layerId)
    {
        var layer = _layers.FirstOrDefault(l => l.Id == layerId);
        if (layer != null)
        {
            _activeLayer = layer;
        }
    }

    public SpatialLayer CreateLayerAbove(SpatialLayerId targetLayerId)
    {
        var target = _layers.First(l => l.Id == targetLayerId);
        int targetOrder = target.StackOrder;

        // Shift layers above target
        foreach (var l in _layers.Where(l => l.StackOrder > targetOrder))
        {
            l.StackOrder++;
        }

        string nextToken = $"0{_layers.Count + 1:D2}";
        var newLayer = new SpatialLayer
        {
            LabelToken = nextToken,
            StackOrder = targetOrder + 1
        };

        _layers.Add(newLayer);
        _activeLayer = newLayer;
        return newLayer;
    }

    public SpatialLayer CreateLayerBelow(SpatialLayerId targetLayerId)
    {
        var target = _layers.First(l => l.Id == targetLayerId);
        int targetOrder = target.StackOrder;

        foreach (var l in _layers.Where(l => l.StackOrder < targetOrder))
        {
            l.StackOrder--;
        }

        string nextToken = $"B{_layers.Count(l => l.LabelToken.StartsWith("B")) + 1:D2}";
        var newLayer = new SpatialLayer
        {
            LabelToken = nextToken,
            StackOrder = targetOrder - 1
        };

        _layers.Add(newLayer);
        _activeLayer = newLayer;
        return newLayer;
    }

    public void ReorderLayer(SpatialLayerId layerId, int newStackOrder)
    {
        var layer = _layers.FirstOrDefault(l => l.Id == layerId);
        if (layer == null || layer.IsProtectedBaseLayer) return;

        layer.StackOrder = newStackOrder;
    }

    public bool TryRemoveLayer(SpatialLayerId layerId, out SpatialLayerId? destinationLayerId)
    {
        destinationLayerId = null;
        var layer = _layers.FirstOrDefault(l => l.Id == layerId);
        if (layer == null || layer.IsProtectedBaseLayer || _layers.Count <= 1)
            return false;

        // Find adjacent destination layer
        var sorted = LayerStack;
        int idx = sorted.ToList().IndexOf(layer);
        var dest = idx > 0 ? sorted[idx - 1] : sorted[idx + 1];

        destinationLayerId = dest.Id;
        _layers.Remove(layer);

        if (_activeLayer.Id == layerId)
        {
            _activeLayer = dest;
        }

        return true;
    }

    /// <summary>
    /// Calculates vertical aura decay gamma^|deltaL| where gamma = 0.5.
    /// </summary>
    public static float CalculateLayerAuraDecay(int sourceStackOrder, int targetStackOrder)
    {
        int delta = Math.Abs(targetStackOrder - sourceStackOrder);
        return MathF.Pow(0.5f, delta);
    }
}

/// <summary>
/// Keyboard routing module handling bracket layer navigation shortcuts.
/// </summary>
public sealed class LayerNavigationInputHandler
{
    private readonly ISpatialLayerManager _layerManager;

    public LayerNavigationInputHandler(ISpatialLayerManager layerManager)
    {
        _layerManager = layerManager;
    }

    public bool OnKeyDown(KeyEventArgs e)
    {
        bool isCtrlOrCmd = e.KeyModifiers.HasFlag(KeyModifiers.Control) || e.KeyModifiers.HasFlag(KeyModifiers.Meta);
        bool isShift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        bool isAlt = e.KeyModifiers.HasFlag(KeyModifiers.Alt);

        if (e.Key == Key.OemOpenBrackets) // '[' key
        {
            if (isShift)
                _layerManager.CreateLayerBelow(_layerManager.ActiveLayer.Id);
            else if (isCtrlOrCmd)
                _layerManager.CreateLayerBelow(_layerManager.ActiveLayer.Id);
            else if (isAlt)
                _layerManager.ReorderLayer(_layerManager.ActiveLayer.Id, _layerManager.ActiveLayer.StackOrder - 1);
            else
                TraverseLayer(-1);

            return true;
        }
        else if (e.Key == Key.OemCloseBrackets) // ']' key
        {
            if (isShift)
                _layerManager.CreateLayerAbove(_layerManager.ActiveLayer.Id);
            else if (isCtrlOrCmd)
                _layerManager.CreateLayerAbove(_layerManager.ActiveLayer.Id);
            else if (isAlt)
                _layerManager.ReorderLayer(_layerManager.ActiveLayer.Id, _layerManager.ActiveLayer.StackOrder + 1);
            else
                TraverseLayer(1);

            return true;
        }

        return false;
    }

    private void TraverseLayer(int direction)
    {
        var stack = _layerManager.LayerStack;
        int activeIdx = stack.ToList().FindIndex(l => l.Id == _layerManager.ActiveLayer.Id);
        int targetIdx = Math.Clamp(activeIdx + direction, 0, stack.Count - 1);
        _layerManager.ActivateLayer(stack[targetIdx].Id);
    }
}
```
