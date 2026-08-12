---
status: "IMPLEMENTED - AWAITING USER REVIEW"
---

# ADR-040: Spatial Layer System Architecture

| Property | Value |
| :--- | :--- |
| **Status** | IMPLEMENTED - AWAITING USER REVIEW |
| **Date** | 2026-08-12 |
| **Area** | Spatial Layers / Field Physics / Layer Indexing |
| **Target Runtime** | C# 13 / .NET 9 / Avalonia 11.2.5 / SkiaSharp |
| **Authors** | Chief Product Definition Architect |

---

## 1. Executive Context & Architectural Principles

In Grove v9, the spatial workspace is organized across a continuous 2D coordinate grid extended into depth via an unbounded stack of **Spatial Layers**. As defined in original architectural specifications (`Grove - Layers.txt`, `Grove - Field ledger.txt`, and [ADR-001](file:///C:/dev/grove-v9/docs/specs/spatial-grid/ADR-001-Spatial-Grid-Plane-Architecture.md)), a Spatial Layer is NOT a destructive container or isolated document tab. It is an addressable spatial frequency band in a continuous vertical depth stack through which energy fields saturate.

```
Spatial Layer Stack Continuum Architecture
─────────────────────────────────────────────────────────────────────────────────────────────
Label   Stack Index (Z)  Layer State    Rendering Policy                Occupancy Rule
─────────────────────────────────────────────────────────────────────────────────────────────
03      StackIndex = 3   Inactive       Presence Heatmap & Isolines     Grid Cell (x,y)
02      StackIndex = 2   Active         Full Content Frames + Text      Grid Cell (x,y) [Exclusive Same-Layer]
01      StackIndex = 1   Inactive       Base Layer Anchor / Heatmaps    Grid Cell (x,y)
B01     StackIndex = 0   Inactive       Below-Base Layer / Heatmaps     Grid Cell (x,y)
B02     StackIndex = -1  Inactive       Deep Below-Base / Heatmaps      Grid Cell (x,y)
─────────────────────────────────────────────────────────────────────────────────────────────
```

### 1.1 Core Architectural Invariants

1. **Unbounded Vertical Continuum**: The stack supports an arbitrary number of layers ($L \in (-\infty, +\infty)$). Layer `01` acts as the permanent base anchor.
2. **Immutable Identity vs. Stack Order**: Every layer possesses an immutable primary key (`LayerId`). Position in stack order (`StackIndex`) dictates visual rendering order, while its human-facing identifier (`StableLabel`) remains fixed throughout reordering operations.
3. **Strict Same-Layer Non-Overlap**: Two content placements on the *same* layer $L_k$ cannot occupy overlapping grid cell bounds:
   $$\text{Rect}_A \cap \text{Rect}_B = \emptyset \quad \forall A, B \in \text{Placements}(L_k), \, A \neq B$$
4. **Permissive Cross-Layer Multi-Occupancy**: Content placements on *different* layers $L_j$ and $L_k$ ($j \neq k$) MAY occupy identical grid cell coordinates $(x,y)$ without spatial collision or refusal.
5. **Durable Content Preservation**: Deleting a layer NEVER deletes placed content. All placements on a deleted layer migrate atomically to an adjacent surviving layer.

---

## 2. Layer Indexing & Stable Side-Coded Labeling System

### 2.1 Label Minting Taxonomy

Layer labels provide human-readable, fixed reference tokens that reflect the relative origin of creation relative to Base Layer `01`. Labels are formatted according to strict formatting rules:

- **Base Layer Anchor**: Fixed label `01`. Permanent anchor created at workspace instantiation. Cannot be removed or re-labeled.
- **Above-Base Layers**: Minted with two-digit zero-padded positive integers (`02`, `03`, `04`, $\dots$, `99`, `100`).
- **Below-Base Layers**: Minted with prefix `B` followed by two-digit zero-padded positive integers (`B01`, `B02`, `B03`, $\dots$, `B99`).

### 2.2 Stable Label vs. Stack Index Disambiguation

`StackIndex` represents the zero-based visual stack position (0 to $N-1$) evaluated during render operations. Reordering layers alters `StackIndex` values for affected layers, but **NEVER** renames or mutates `StableLabel`.

```
Example: Reordering Layer B01 above Layer 02

Initial State:
Index: 3 | Label: 03  | LayerId: guid-03
Index: 2 | Label: 02  | LayerId: guid-02
Index: 1 | Label: 01  | LayerId: guid-01 (Base Anchor)
Index: 0 | Label: B01 | LayerId: guid-b01

After Moving B01 to Index 2:
Index: 3 | Label: 03  | LayerId: guid-03
Index: 2 | Label: B01 | LayerId: guid-b01  <-- Position shifted, Label "B01" PRESERVED
Index: 1 | Label: 02  | LayerId: guid-02
Index: 0 | Label: 01  | LayerId: guid-01
```

### 2.3 Character Metric Requirements

The Layer Manager HUD UI allocates a fixed `4ch` width column for side-coded labels (e.g., `B01`, `100`). The maximum label length handled without truncation is 4 characters.

---

## 3. Placement Migration & Deletion Invariants

When layer $L_{\text{target}}$ is removed from the `SpatialLayerStack`:

```
                       [Initiate Layer Removal: L_target]
                                       │
                                       ▼
                       [Is Placements(L_target) Empty?]
                                  │         │
                         Yes ─────┘         └───── No
                          │                           │
                          ▼                           ▼
                 [Remove Layer Entry]      [Determine L_destination]
                 [Update Stack Indices]    (Next lower layer, or next higher)
                          │                           │
                          │                           ▼
                          │                [Check Cell Collisions]
                          │                [Placements(L_target) vs Placements(L_dest)]
                          │                           │
                          │                  ┌────────┴────────┐
                          │         No Collision           Collision
                          │                  │                 │
                          │                  ▼                 ▼
                          │          [Migrate Content]  [Trigger Refusal State]
                          │          [Remove Layer]     ["That space is occupied"]
                          │                  │                 │
                          └──────────────────┴─────────────────┘
```

1. **Destination Resolution**:
   $$L_{\text{destination}} = \begin{cases} L_{\text{stackIndex} - 1} & \text{if } \text{StackIndex} > 0 \\ L_{\text{stackIndex} + 1} & \text{if } \text{StackIndex} = 0 \end{cases}$$
2. **Collision Validation**:
   If any placement $P_a \in \text{Placements}(L_{\text{target}})$ overlaps with $P_b \in \text{Placements}(L_{\text{destination}})$, the layer deletion fails with inline HUD refusal: `"That space is occupied on Layer {L_destination.Label}."`.
3. **Atomic State Mutation**:
   Upon validation, all placements update their internal `LayerId` property to $L_{\text{destination}}.\text{Id}$ within a single transaction in the `FieldLedger`.

---

## 4. C# 13 Data Structures & Type Contracts

```csharp
namespace Grove.SpatialLayers.Architecture;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;

/// <summary>
/// Strongly typed immutable identifier for a spatial layer.
/// </summary>
public readonly record struct LayerId(Guid Value)
{
    public static LayerId New() => new(Guid.NewGuid());
    public static LayerId BaseLayer { get; } = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));
}

/// <summary>
/// Immutable representation of a stable side-coded layer label (e.g., "01", "02", "B01").
/// </summary>
public readonly record struct LayerLabel
{
    public string Value { get; }

    public LayerLabel(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (value.Length > 4)
            throw new ArgumentException("Layer label must not exceed 4 characters ('4ch').", nameof(value));
        Value = value;
    }

    public override string ToString() => Value;
}

/// <summary>
/// Immutable domain model representing a single spatial layer within the stack continuum.
/// </summary>
public record SpatialLayer(
    LayerId Id,
    LayerLabel Label,
    string Name,
    bool IsVisible,
    bool IsLocked,
    Vector4 ColorTint
);

/// <summary>
/// Thread-safe immutable spatial layer stack continuum.
/// </summary>
public sealed class SpatialLayerStack
{
    private readonly ImmutableList<SpatialLayer> _layers;
    public LayerId ActiveLayerId { get; }

    public SpatialLayerStack(ImmutableList<SpatialLayer> layers, LayerId activeLayerId)
    {
        _layers = layers ?? throw new ArgumentNullException(nameof(layers));
        ActiveLayerId = activeLayerId;
    }

    public IReadOnlyList<SpatialLayer> Layers => _layers;

    public int GetStackIndex(LayerId id)
    {
        for (int i = 0; i < _layers.Count; i++)
        {
            if (_layers[i].Id == id) return i;
        }
        return -1;
    }

    public SpatialLayer ActiveLayer => 
        _layers.Find(l => l.Id == ActiveLayerId) 
        ?? throw new InvalidOperationException($"Active layer {ActiveLayerId} not found in stack.");

    public SpatialLayerStack WithActiveLayer(LayerId id)
    {
        if (GetStackIndex(id) < 0)
            throw new KeyNotFoundException($"Layer {id} does not exist in stack.");
        return new SpatialLayerStack(_layers, id);
    }
}

/// <summary>
/// Core service interface managing spatial layer lifecycle, stack mutations, and migration.
/// </summary>
public interface ISpatialLayerStackManager
{
    SpatialLayerStack CurrentStack { get; }
    
    SpatialLayer AddLayerAbove(LayerId targetId, string? initialName = null);
    SpatialLayer AddLayerBelow(LayerId targetId, string? initialName = null);
    
    bool MoveLayer(LayerId sourceId, int targetStackIndex);
    
    LayerMigrationResult RemoveLayer(LayerId targetId);
    
    void SetActiveLayer(LayerId targetId);
}

public readonly record struct LayerMigrationResult(
    bool Success,
    LayerId TargetLayerId,
    LayerId DestinationLayerId,
    int MigratedPlacementCount,
    string? RefusalReason
);
```

---

## 5. Avalonia 11.2.5 Rendering Pipeline & Visual Ordering

The spatial layer stack maps directly into Avalonia 11.2.5 GPU visual tree rendering order.

```csharp
namespace Grove.SpatialLayers.Rendering;

using Avalonia.Controls;
using Avalonia.Media;
using SkiaSharp;
using Grove.SpatialLayers.Architecture;

/// <summary>
/// Custom Avalonia Skia rendering operator executing layered spatial canvas draw commands.
/// </summary>
public sealed class SpatialLayerCanvasRenderOperation : ICustomDrawOperation
{
    private readonly SpatialLayerStack _stack;
    private readonly Rect _bounds;

    public SpatialLayerCanvasRenderOperation(SpatialLayerStack stack, Rect bounds)
    {
        _stack = stack;
        _bounds = bounds;
    }

    public Rect Bounds => _bounds;

    public void Render(ImmediateDrawingContext context)
    {
        var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
        if (leaseFeature is null) return;

        using var lease = leaseFeature.Lease();
        var canvas = lease.SkCanvas;

        canvas.Save();
        
        // Iterate bottom-to-top through stack index (ZIndex = 0 to N-1)
        for (int i = 0; i < _stack.Layers.Count; i++)
        {
            var layer = _stack.Layers[i];
            if (!layer.IsVisible) continue;

            bool isActive = layer.Id == _stack.ActiveLayerId;

            // Render Layer Content & Aura Permeability Field
            RenderLayerPass(canvas, layer, i, isActive);
        }

        canvas.Restore();
    }

    private void RenderLayerPass(SKCanvas canvas, SpatialLayer layer, int stackIndex, bool isActive)
    {
        // Active layer renders full content frames; inactive layers render presence fields only
        if (isActive)
        {
            // Execute Plane 0 Active Layer Vector & Content Footprint Render
        }
        else
        {
            // Execute Inactive Layer Presence Heatmap & Ghost Silhouette Render
        }
    }

    public bool HitTest(Point p) => true;
    public bool Equals(ICustomDrawOperation? other) => false;
    public void Dispose() { }
}
```

---

## 6. Architectural Traceability & References

- **Original Design Notes**: `docs/product/original-notes/Grove - Layers.txt`
- **Design System Grammar**: `docs/design-system/10-grammar/Layer-depth.md`
- **Design System Component**: `docs/design-system/30-components/Layer-manager.md`
- **Related Specs**: [ADR-001](file:///C:/dev/grove-v9/docs/specs/spatial-grid/ADR-001-Spatial-Grid-Plane-Architecture.md), [ADR-003](file:///C:/dev/grove-v9/docs/specs/spatial-grid/ADR-003-Spatial-Aura-Physics.md), [ADR-032](file:///C:/dev/grove-v9/docs/specs/hud-system/ADR-032-Spatial-Layer-Manager-And-Navigation.md)
