---
status: "IMPLEMENTED - AWAITING USER REVIEW"
---

# ADR-002: Field Ledger and Subscribers

| Property | Value |
| :--- | :--- |
| **Status** | IMPLEMENTED - AWAITING USER REVIEW |
| **Date** | 2026-08-12 |
| **Area** | Field Ledger System / Multi-Layer Energy Topology |
| **Target Runtime** | C# 13 / .NET 9 / High-Performance Span & Memory Primitives |
| **Authors** | Chief Product Definition Architect |

---

## 1. Context & Architectural Principles

As specified in original product notes (`Grove - Field ledger.txt`, `Grove - field metadata.txt`), aura fields are an intrinsic property of all spatial content on the grid plane. The Field Ledger is the centralized relational ledger storing spatial influence, energy saturation, and cross-layer source metadata.

### Core Physical Principles
1. **Innate Existence Value**: Every grid cell possesses a non-zero baseline energy value ($E_0 = 0.05$). The simple existence of a cell on the grid establishes spatial bearing and information value.
2. **Space-Time Gravity Analogy**: Content placement creates a gravitational energy field that attenuates over distance and saturates across adjacent layers.
3. **Metadata Payload Preservation**: Saturation is not merely scalar visual brightness; every affected cell retains source provenance metadata identifying all content items contributing to its field state.

---

## 2. Field Ledger Memory Architecture & Struct Layout

To support high-frequency spatial field queries without garbage collector overhead, cell ledger entries are structured as unmanaged, sequential value types (`struct`) optimized for cache-line alignment in C# / .NET 9.

### 2.1 Non-Zero Baseline Energy
$$\text{Energy}_{\text{baseline}} = 0.05$$

Total energy for cell $c$ at position $(x,y)$ on layer $L$ is:

$$E(c) = E_0 + \sum_{i \in \text{Sources}} E_i(x, y, L)$$

### 2.2 Memory Struct Layout (`CellLedgerEntry`)

```csharp
namespace Grove.SpatialGrid.FieldLedger;

using System;
using System.Runtime.InteropServices;
using System.Numerics;

/// <summary>
/// Packed 2D integer cell coordinate structure.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public readonly record struct GridCellPosition(int X, int Y)
{
    public long ToSpatialKey() => ((long)X << 32) | (uint)Y;
}

/// <summary>
/// Source metadata payload stored within affected grid cells.
/// Preserves complete provenance for multi-layer annotations and field inspection.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 8)]
public readonly record struct FieldSourceMetadata
{
    public Guid ContentId { get; init; }
    public Guid LayerId { get; init; }
    public ushort ContentKind { get; init; } // Note = 1, Document = 2, Picture = 3
    public float Mass { get; init; }
    public float ContributedEnergy { get; init; }
    public Vector4 ColorHue { get; init; }   // Normalized RGBA (0.0 - 1.0)
    public long TimestampTicks { get; init; }
}

/// <summary>
/// High-density cell ledger record representing a single spatial cell's field state.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 8)]
public readonly record struct CellLedgerEntry
{
    public const float BaselineEnergy = 0.05f;

    public GridCellPosition Position { get; init; }
    public Guid LayerId { get; init; }
    public float TotalEnergy { get; init; }
    public Vector4 PrimaryHue { get; init; }
    public int SourceCount { get; init; }

    // Fixed inline array buffer for high-speed zero-alloc source tracking (max 4 concurrent sources per cell)
    public FieldSourceMetadata InlineSource0 { get; init; }
    public FieldSourceMetadata InlineSource1 { get; init; }
    public FieldSourceMetadata InlineSource2 { get; init; }
    public FieldSourceMetadata InlineSource3 { get; init; }

    public CellLedgerEntry(GridCellPosition position, Guid layerId)
    {
        Position = position;
        LayerId = layerId;
        TotalEnergy = BaselineEnergy;
        PrimaryHue = new Vector4(0.086f, 0.086f, 0.094f, 1.0f); // Default #161618 canvas hue
        SourceCount = 0;
        InlineSource0 = default;
        InlineSource1 = default;
        InlineSource2 = default;
        InlineSource3 = default;
    }
}
```

---

## 3. Pub-Sub Architecture (`IFieldSubscriber`)

The Field Ledger operates a publish-subscribe dispatch system (`IFieldSubscriber`). When content is placed, moved, or deleted, field energy calculation propagates updates to registered subscribers.

```csharp
namespace Grove.SpatialGrid.FieldLedger;

using System;

/// <summary>
/// Subscriber interface receiving real-time field ledger mutations.
/// </summary>
public interface IFieldSubscriber
{
    /// <summary>
    /// Unique subscriber identifier.
    /// </summary>
    string SubscriberId { get; }

    /// <summary>
    /// Triggered when a single cell's energy or source metadata mutates.
    /// </summary>
    void OnCellFieldUpdated(in CellLedgerEntry entry);

    /// <summary>
    /// Triggered during bulk field recalculation across a spatial region.
    /// Uses zero-allocation ReadOnlySpan for performance.
    /// </summary>
    void OnRegionFieldBatchUpdated(ReadOnlySpan<CellLedgerEntry> regionEntries);
}
```

---

## 4. Concrete Subscriber Specifications

### 4.1 `AuraHeatmapSubscriber`
- **Role**: Computes composite color hue saturation and energy accumulation across layers for rendering Plane 0 field heatmaps.
- **Rendering Input**: Multiplies `TotalEnergy` into cell alpha bound between `--field-alpha-min` ($0.025$) and `--field-alpha-max` ($0.30$).
- **Color Blending Math**:
  $$\vec{C}_{\text{composite}} = \frac{\sum_{i} E_i \cdot \vec{C}_i}{\sum_{i} E_i}$$

```csharp
public sealed class AuraHeatmapSubscriber : IFieldSubscriber
{
    public string SubscriberId => "subscriber.aura-heatmap";

    public void OnCellFieldUpdated(in CellLedgerEntry entry)
    {
        // Update local Plane 0 heatmap buffer cache for cell
    }

    public void OnRegionFieldBatchUpdated(ReadOnlySpan<CellLedgerEntry> regionEntries)
    {
        for (int i = 0; i < regionEntries.Length; i++)
        {
            ref readonly var entry = ref regionEntries[i];
            float clampedAlpha = Math.Clamp(entry.TotalEnergy * 0.22f, 0.025f, 0.30f);
            // Dispatch to GPU texture / Skia heatmap render buffer
        }
    }
}
```

### 4.2 `PerimeterRingSubscriber`
- **Role**: Detects spatial energy boundaries where field energy crosses the threshold $E_{\text{threshold}} = 0.15$.
- **Geometry Output**: Generates continuous 1.5px perimeter containment ring paths (`--field-perimeter-width: 1.5px`, `--field-perimeter-ink: 0.25`).

```csharp
public sealed class PerimeterRingSubscriber : IFieldSubscriber
{
    public string SubscriberId => "subscriber.perimeter-ring";
    public const float EnergyThreshold = 0.15f;

    public void OnCellFieldUpdated(in CellLedgerEntry entry)
    {
        // Evaluate neighbor cell energy gradients to trace isoline boundaries
    }

    public void OnRegionFieldBatchUpdated(ReadOnlySpan<CellLedgerEntry> regionEntries)
    {
        // Reconstruct vector contour paths for affected bounding regions
    }
}
```

### 4.3 `AnnotationMetadataSubscriber`
- **Role**: Intercepts cross-layer cell metadata payloads to identify overlapping spatial content clusters.
- **Trigger Contract**: When multiple source payloads occupy overlapping grid cells on a target layer, checks qualification rules ($D \ge 1.0$, $\ge 24$ supported cells) to notify the Information Plane Annotation system.

```csharp
public sealed class AnnotationMetadataSubscriber : IFieldSubscriber
{
    public string SubscriberId => "subscriber.annotation-metadata";

    public void OnCellFieldUpdated(in CellLedgerEntry entry)
    {
        if (entry.SourceCount >= 2)
        {
            // Evaluate candidate source set qualification for Information Plane Annotation triggers
        }
    }

    public void OnRegionFieldBatchUpdated(ReadOnlySpan<CellLedgerEntry> regionEntries)
    {
        // Batch query candidate overlapping placements across layers
    }
}
```

---

## 5. Ledger Manager Engine Implementation

```csharp
namespace Grove.SpatialGrid.FieldLedger;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public sealed class FieldLedgerManager
{
    private readonly ConcurrentDictionary<long, CellLedgerEntry> _gridLedger = new();
    private readonly List<IFieldSubscriber> _subscribers = new();
    private readonly object _subscriberLock = new();

    public void RegisterSubscriber(IFieldSubscriber subscriber)
    {
        lock (_subscriberLock)
        {
            _subscribers.Add(subscriber);
        }
    }

    public void MutateCellField(CellLedgerEntry newEntry)
    {
        long key = newEntry.Position.ToSpatialKey();
        _gridLedger[key] = newEntry;

        lock (_subscriberLock)
        {
            foreach (var sub in _subscribers)
            {
                sub.OnCellFieldUpdated(in newEntry);
            }
        }
    }

    public void BatchMutateRegion(ReadOnlySpan<CellLedgerEntry> entries)
    {
        for (int i = 0; i < entries.Length; i++)
        {
            long key = entries[i].Position.ToSpatialKey();
            _gridLedger[key] = entries[i];
        }

        lock (_subscriberLock)
        {
            foreach (var sub in _subscribers)
            {
                sub.OnRegionFieldBatchUpdated(entries);
            }
        }
    }
}

---

## 6. Field Quantization, Latency Trade-offs & Refusal Invariants

### 6.1 Discrete Alpha Tiers & Hue Summation
Presence field values are evaluated per discrete grid cell ($220\text{px} \times 220\text{px}$) and rendered with hard cell boundaries ("the cell is the field's pixel"). The discrete alpha tiers across presence modes are:
- **Neutral Content (`--fw`)**: Tier 1 = $0.19$, Tier 2 = $0.12$, Tier 3 = $0.070$, Tier 4 = $0.038$.
- **Warm Note (`--fh`, `#6B5540`)**: Tier 1 = $0.60$, Tier 2 = $0.38$, Tier 3 = $0.22$, Tier 4 = $0.11$.
- **Anchor Indigo (`--fi`, `#9E8CEA`)**: Tier 1 = $0.30$, Tier 2 = $0.18$, Tier 3 = $0.10$, Tier 4 = $0.05$.

Where multiple fields overlap across layers, cell hues accumulate additively without blending cell boundary edges.

### 6.2 Gesture Latency Trade-off & Trail Lag
During active drag-and-drop operations:
1. **Input Priority**: The dragged footprint cursor tracks pointer position with zero input latency.
2. **Local Recompute**: Destination cells light as origin cells dim (`PA-01`).
3. **1-Frame Aura Lag**: The field heatmap evaluation MAY trail cursor position by exactly 1 frame (`PA-02`). Field evaluation lag is explicitly accepted to preserve 0ms pointer response.
4. **Origin Ghosting**: An origin ghost outline persists at the source position until pointer release (`commit`).

### 6.3 Presence Field Refusal Contracts
- **Refused Smooth Glow**: Radial gradient halos or continuous gaussian blurs crossing cell edges are forbidden; field intensity must remain cell-quantized.
- **Refused Ghost Content**: Materializing faint frames or readable text from inactive layers is forbidden; non-active layers contribute presence (lit cell heatmaps) only.
- **Refused Field Over Content**: Aura brightness must never exceed content surface luminance; field intensity is capped below content contrast floors.
```
