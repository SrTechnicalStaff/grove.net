---
status: "PARTIAL — verified rigid group translation and trail emission"
---

# ADR-055: Multi-Item Selection Model, Cell-Aligned Marquee Sweep, and Multi-Type Group Translation Engine

| Property | Value |
| :--- | :--- |
| **Status** | PARTIAL — verified rigid group translation and trail emission |
| **Date** | 2026-08-12 |
| **Area** | Spatial Grid Engine / Selection & Group Translation Subsystem |
| **Target Runtime** | C# 13 / .NET 9 / Avalonia 11.2.5 / SkiaSharp 3.x |
| **Authors** | Chief Product Definition Architect |

---

## 1. Executive Context & Architectural Principles

In Grove v9, content elements residing on Plane 0 (Spatial Grid Canvas)—including `Note`, `Document`, and `Picture` (Image) placements—are manipulated through a deterministic, cell-aligned selection and group translation engine. Unlike floating freeform canvas editors that permit sub-pixel offsets and fragmented group states, Grove enforces strict discrete cell alignment, rigid spatial vector cluster translation, and mathematical selection thresholds.

### Key Architectural Requirements:
1. **Cell-Aligned Marquee Sweep with $\ge 50\%$ Overlap Threshold**: Dragging a pointer across empty grid space initializes a continuous marquee sweep rectangle. A candidate placement is accumulated into the selection queue if and only if the continuous marquee rectangle covers **$\ge 50\%$ of the item's total cell surface area**.
2. **Double-Buffered Lock-Free Selection Queue (`SelectionQueue`)**: Selection state management must support high-frequency pointer updates without allocations or thread lock contention. The `SelectionQueue` preserves primary vs secondary item roles, shift-toggle additions, and atomic clearing.
3. **Multi-Type Coordinated Group Translation**: Multi-item selections comprising mixed content types (`Note` $n \times n$, `Document` $W \times H$, `Image` $N_w \times N_h$) translate as a rigid spatial cluster via integer vector displacement $\mathbf{\Delta C} = (\Delta C_x, \Delta C_y)$. Relative spatial distances and layout geometry between items are invariant throughout translation.
4. **All-Or-Nothing Atomic Transactional Movement (`IsClusterRegionFree`)**: Cluster movement is strictly transactional. Before committing a group shift, the engine evaluates spatial region freedom across the **footprint union** of all translating items. If ANY non-anchored item in the cluster collides with an occupied cell outside the cluster's original footprint union, the **ENTIRE group shift is refused**.
5. **Spent-Trail Physics for Group Moves**: Upon committing a group translation, every cell vacated by every item in the translating cluster deposits initial kinetic energy ($E_0 = 0.60$) into the spent-trail render queue, undergoing frame-by-frame exponential decay ($\gamma = 0.84$) over exactly 18 frames.

---

## 2. Selection Mathematics & Cell-Aligned Marquee Sweep

```
  (C_x,start, C_y,start)
  +-------------------------------------------------------------+
  | Continuous Pointer Marquee Sweep Boundary                   |
  | Fill: --signal-interaction (6% opacity, #222220)            |
  | Stroke: 1px Inset Dash (Dash: 4px, Gap: 4px)                |
  |                                                             |
  |        +-------------------------+                          |
  |        | Item A Footprint        |                          |
  |        | Area Overlap = 68.5%    |  --> ACCUMULATED        |
  |        | ( >= 50% Threshold )    |                          |
  |        +-------------------------+                          |
  |                                                             |
  |                                   +-------------------+     |
  |                                   | Item B Footprint  |     |
  |                                   | Overlap = 22.1%   |     |
  +-----------------------------------| ( < 50% Threshold)|-----+
                                      | REJECTED          |
                                      +-------------------+
```

### 2.1 Marquee Continuous Bounds & Grid Cell Projections

Let $P_{\text{start}} = (x_{\text{start}}, y_{\text{start}})$ be the world-space coordinate where a marquee drag gesture was initiated, and $P_{\text{curr}} = (x_{\text{curr}}, y_{\text{curr}})$ be the instantaneous world pointer coordinate.
The continuous world-space marquee bounding rectangle $M_{\text{world}} = [x_{m,0}, y_{m,0}, x_{m,1}, y_{m,1}]$ is:

$$x_{m,0} = \min(x_{\text{start}}, x_{\text{curr}}), \quad y_{m,0} = \min(y_{\text{start}}, y_{\text{curr}})$$
$$x_{m,1} = \max(x_{\text{start}}, x_{\text{curr}}), \quad y_{m,1} = \max(y_{\text{start}}, y_{\text{curr}})$$

The corresponding cell index span $[C_{x,\text{min}}, Y_{y,\text{min}}] \times [C_{x,\text{max}}, C_{y,\text{max}}]$ covered by the marquee is:

$$C_{x,\text{min}} = \left\lfloor \frac{x_{m,0}}{P_{\text{cell}}} \right\rfloor, \quad C_{y,\text{min}} = \left\lfloor \frac{y_{m,0}}{P_{\text{cell}}} \right\rfloor$$
$$C_{x,\text{max}} = \left\lfloor \frac{x_{m,1}}{P_{\text{cell}}} \right\rfloor, \quad C_{y,\text{max}} = \left\lfloor \frac{y_{m,1}}{P_{\text{cell}}} \right\rfloor$$

where $P_{\text{cell}} = 220.0\text{ DIPs}$.

### 2.2 Spatial Cell Bounding Box Overlap Formula ($\ge 50\%$ Threshold)

Consider a candidate placement item $I_k$ with cell origin $(X_k, Y_k)$ and cell dimensions $(W_k, H_k)$. Its continuous world-space bounding box $B_{I_k} = [x_{k,0}, y_{k,0}, x_{k,1}, y_{k,1}]$ is:

$$x_{k,0} = X_k \cdot P_{\text{cell}}, \quad y_{k,0} = Y_k \cdot P_{\text{cell}}$$
$$x_{k,1} = (X_k + W_k) \cdot P_{\text{cell}}, \quad y_{k,1} = (Y_k + H_k) \cdot P_{\text{cell}}$$

The continuous 1D spatial overlap spans along the X and Y axes between item $I_k$ and marquee $M_{\text{world}}$ are:

$$\Delta x_{\text{overlap}} = \max\left(0.0, \, \min(x_{k,1}, x_{m,1}) - \max(x_{k,0}, x_{m,0})\right)$$
$$\Delta y_{\text{overlap}} = \max\left(0.0, \, \min(y_{k,1}, y_{m,1}) - \max(y_{k,0}, y_{m,0})\right)$$

The intersected 2D surface area $A_{\text{intersect}}(I_k, M)$ is:

$$A_{\text{intersect}}(I_k, M) = \Delta x_{\text{overlap}} \times \Delta y_{\text{overlap}}$$

The total world surface area $A_{\text{total}}(I_k)$ of item $I_k$ is:

$$A_{\text{total}}(I_k) = (W_k \cdot P_{\text{cell}}) \times (H_k \cdot P_{\text{cell}}) = W_k \cdot H_k \cdot P_{\text{cell}}^2$$

The non-dimensional area overlap ratio $\Phi(I_k, M)$ is defined as:

$$\Phi(I_k, M) = \frac{A_{\text{intersect}}(I_k, M)}{A_{\text{total}}(I_k)} = \frac{\Delta x_{\text{overlap}} \times \Delta y_{\text{overlap}}}{W_k \cdot H_k \cdot P_{\text{cell}}^2}$$

The selection membership predicate $\text{SelectPredicate}(I_k)$ evaluates as:

$$\text{SelectPredicate}(I_k) = \begin{cases} \text{True} & \text{if } \Phi(I_k, M) \ge 0.50 \quad (50\% \text{ area threshold}) \\ \text{False} & \text{if } \Phi(I_k, M) < 0.50 \end{cases}$$

```
+-----------------------------------------------------------------------------------+
| Mathematical Overlap Evaluation Table                                            |
+------------------------------------+------------------+-------------------+-------+
| Item Dimensions (W x H)            | Total Area (DIPs)| 50% Min Overlap   | Result|
+------------------------------------+------------------+-------------------+-------+
| Note (1x1 cells)                   | 48,400 DIPs²     | 24,200 DIPs²      | Valid |
| Note (2x2 cells)                   | 193,600 DIPs²    | 96,800 DIPs²      | Valid |
| Document (3x4 cells)               | 580,800 DIPs²    | 290,400 DIPs²     | Valid |
| Image (5x3 cells)                  | 726,000 DIPs²    | 363,000 DIPs²     | Valid |
+------------------------------------+------------------+-------------------+-------+
```

### 2.3 Selection Queue State Machine (`SelectionQueue`)

The selection state transitions across distinct spatial gestures:

```
                  +-----------------------------------+
                  |            Idle State             |
                  |     SelectedPlacementIds = {}     |
                  +-----------------------------------+
                                    |
                    Pointer Press on Empty Canvas
                                    v
                  +-----------------------------------+
                  |        Marquee Sweep Active       |
                  |     Evaluating Area Overlap       |
                  +-----------------------------------+
                         /                     \
      Pointer Release   /                       \   Escape Key Pressed
                       v                         v
   +-----------------------+                +-----------------------+
   | Selection Accumulated |                | Selection Cancelled   |
   | Primary Item Assigned |                | SelectedPlacementIds  |
   | (First in Queue)      |                | Cleared to Empty      |
   +-----------------------+                +-----------------------+
```

#### Selection Queue Rules:
1. **Primary Selection Item**: The first item registered in the `SelectionQueue` is designated as `PrimarySelectionId`. Resize affordances and spatial inspectors anchor to `PrimarySelectionId`.
2. **Additive Shift Selection**: Holding `Shift` during a marquee sweep or pointer click appends newly qualifying item IDs into `SelectionQueue` without clearing prior selections. If an item is already selected during a `Shift` click, it is toggled off (removed).
3. **Primary Re-assignment**: If the `PrimarySelectionId` item is unselected, `PrimarySelectionId` automatically shifts to the head of the remaining queue (`Queue.First`).

---

## 3. Multi-Type Coordinated Group Translation Engine

### 3.1 Rigid Spatial Cluster Vector Translation

Let $\mathcal{C} = \{I_1, I_2, \dots, I_N\}$ be a cluster of $N$ selected items comprising heterogeneous types (`Note`, `Document`, `Picture`). Each item $I_i$ has an initial spatial cell footprint:

$$\text{Footprint}(I_i^0) = \left[ X_i^0, \, Y_i^0, \, W_i, \, H_i \right]$$

When the user drags the spatial cluster by continuous pointer displacement $\mathbf{\Delta p} = (\Delta x_{\text{drag}}, \Delta y_{\text{drag}})$, the raw cell displacement vector $\mathbf{\Delta C}_{\text{raw}} = (\Delta C_{x,\text{raw}}, \Delta C_{y,\text{raw}})$ is:

$$\Delta C_{x,\text{raw}} = \frac{\Delta x_{\text{drag}}}{P_{\text{cell}}}, \quad \Delta C_{y,\text{raw}} = \frac{\Delta y_{\text{drag}}}{P_{\text{cell}}}$$

Applying half-cell rounding with zero sub-cell jitter hysteresis:

$$\Delta C_x = \left\lfloor \Delta C_{x,\text{raw}} + 0.5 \right\rfloor, \quad \Delta C_y = \left\lfloor \Delta C_{y,\text{raw}} + 0.5 \right\rfloor$$

The translated footprint $\text{Footprint}(I_i')$ for each item $I_i \in \mathcal{C}$ under cluster vector $\mathbf{\Delta C} = (\Delta C_x, \Delta C_y)$ is:

$$\text{Footprint}(I_i') = \left[ X_i^0 + \Delta C_x, \, Y_i^0 + \Delta C_y, \, W_i, \, H_i \right]$$

Because $\mathbf{\Delta C}$ is identical for all $I_i \in \mathcal{C}$, the pairwise spatial offset $\mathbf{D}_{ij} = (X_j^0 - X_i^0, Y_j^0 - Y_i^0)$ between any two items $I_i, I_j$ is invariant:

$$\mathbf{D}_{ij}' = (X_j' - X_i', \, Y_j' - Y_i') = ((X_j^0 + \Delta C_x) - (X_i^0 + \Delta C_x), \, (Y_j^0 + \Delta C_y) - (Y_i^0 + \Delta C_y)) = \mathbf{D}_{ij}$$

### 3.2 Cluster Region Freedom Validation (`IsClusterRegionFree`)

Let $\mathcal{O}_{\text{layer}}$ be the spatial occupancy map (R-Tree index) for the active layer.
The source cell footprint union $\mathcal{U}_{\text{source}}$ and target cell footprint union $\mathcal{U}_{\text{target}}$ of cluster $\mathcal{C}$ are defined as:

$$\mathcal{U}_{\text{source}} = \bigcup_{i=1}^N \left\{ (x, y) \in \mathbb{Z}^2 \;\middle|\; X_i^0 \le x < X_i^0 + W_i, \, Y_i^0 \le y < Y_i^0 + H_i \right\}$$
$$\mathcal{U}_{\text{target}} = \bigcup_{i=1}^N \left\{ (x, y) \in \mathbb{Z}^2 \;\middle|\; X_i^0 + \Delta C_x \le x < X_i^0 + \Delta C_x + W_i, \, Y_i^0 + \Delta C_y \le y < Y_i^0 + \Delta C_y + H_i \right\}$$

The candidate cell evaluation set $\mathcal{E}$ represents newly entered cells:

$$\mathcal{E} = \mathcal{U}_{\text{target}} \setminus \mathcal{U}_{\text{source}}$$

The cluster collision validation predicate $\text{IsClusterRegionFree}(\mathcal{C}, \mathbf{\Delta C})$ is:

$$\text{IsClusterRegionFree}(\mathcal{C}, \mathbf{\Delta C}) = \bigwedge_{c \in \mathcal{E}} \Big( \mathcal{O}_{\text{layer}}(c) = \varnothing \land \text{IsNonAnchored}(\mathcal{C}) \Big)$$

```
                                  CLUSTER REGION FREE EVALUATION
       Source Footprint Union (U_source)                  Target Footprint Union (U_target)
       +--------+------+                                          +--------+------+
       | Item A |      |                                          | Item A'|      |
       +--------+      |  +  Vector (ΔCx, ΔCy)  ===>              +--------+      |
       |        |Item B|                                          |        |Item B'|  <-- Checks
       +--------+------+                                          +--------+------+      Cells in
                                                                                  U_target \ U_source
```

#### Collision Rules:
1. Self-intersection within $\mathcal{U}_{\text{source}} \cap \mathcal{U}_{\text{target}}$ is explicitly permitted (items moving into cells previously occupied by themselves or other members of cluster $\mathcal{C}$).
2. If ANY cell $c \in \mathcal{E}$ is occupied by an external item $J \notin \mathcal{C}$, `IsClusterRegionFree` returns `False`.
3. If ANY item $I_i \in \mathcal{C}$ has `IsAnchored == true`, translation is immediately refused (`IsClusterRegionFree = False`).

### 3.3 Atomic Transactional Movement Engine

Group translation operates as an all-or-nothing transactional mutation state machine:

```
  [ Gesture Start ] ---> Snapshot Initial Footprints ---> Track Pointer Vector (ΔCx, ΔCy)
                                                                 |
                                                                 v
                                                     IsClusterRegionFree == True?
                                                      /                        \
                                                 YES /                          \ NO
                                                    v                            v
                                         [ Valid Translation ]        [ Refusal Translation ]
                                         Render Ghost Preview         Render Refusal Ghosting
                                         (6% Interaction Fill)        (12% Refusal Cross-Hatch)
                                                    |                            |
                                             Pointer Release              Pointer Release
                                                    v                            v
                                         [ Commit Transaction ]       [ Abort Transaction ]
                                         1. Update R-Tree Index       Zero Coordinate Delta
                                         2. Emit Ledger Batch         Cluster Snaps Back
                                         3. Trigger Spent Trails      No Ledger Mutation
```

### 3.4 Multi-Cell Spent-Trail Physics Decay

Upon committing a valid group translation with non-zero delta $\mathbf{\Delta C} \neq (0,0)$, all cells in the vacated set $\mathcal{V} = \mathcal{U}_{\text{source}} \setminus \mathcal{U}_{\text{target}}$ are registered as spent trail cells.

For every cell $c \in \mathcal{V}$, initial kinetic energy $E_0(c) = 0.60$.
At each frame tick $t \ge 0$, kinetic energy decays exponentially:

$$E_{t+1}(c) = E_t(c) \cdot \gamma_{\text{decay}}$$

where $\gamma_{\text{decay}} = 0.84$.
A trail cell $c$ is pruned from the compositor render list when:

$$E_t(c) < E_{\text{min}} \quad (E_{\text{min}} = 0.03)$$

#### Exact Frame Lifetime Proof for Group Vacated Cells:
$$E_{18} = 0.60 \cdot (0.84)^{18} \approx 0.02641 < 0.03$$
Vacated cells across the entire group footprint union persist for **exactly 18 frames** before complete eviction.

---

## 4. C# 13 System Architecture & Concrete Type Contracts

```csharp
namespace Grove.SpatialGrid.Selection;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

/// <summary>
/// Represents a discrete 2D spatial cell coordinate on Plane 0.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public readonly record struct CellCoordinate(int X, int Y)
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public CellCoordinate Translate(int deltaX, int deltaY) => new(X + deltaX, Y + deltaY);
}

/// <summary>
/// Defines discrete cell bounding box footprint [X, Y, Width, Height].
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public readonly record struct SpatialRegion(int X, int Y, int Width, int Height)
{
    public int Right => X + Width;
    public int Bottom => Y + Height;
    public long AreaCells => (long)Width * Height;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsCell(CellCoordinate cell) =>
        cell.X >= X && cell.X < Right && cell.Y >= Y && cell.Y < Bottom;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Intersects(SpatialRegion other) =>
        X < other.Right && Right > other.X && Y < other.Bottom && Bottom > other.Y;

    /// <summary>
    /// Calculates continuous world-space area overlap ratio between this cell footprint and marquee world rect.
    /// </summary>
    public double CalculateWorldAreaOverlapRatio(
        double marqueeMinX, double marqueeMinY, double marqueeMaxX, double marqueeMaxY, double cellPitchDips)
    {
        double itemMinX = X * cellPitchDips;
        double itemMinY = Y * cellPitchDips;
        double itemMaxX = Right * cellPitchDips;
        double itemMaxY = Bottom * cellPitchDips;

        double overlapDx = Math.Max(0.0, Math.Min(itemMaxX, marqueeMaxX) - Math.Max(itemMinX, marqueeMinX));
        double overlapDy = Math.Max(0.0, Math.Min(itemMaxY, marqueeMaxY) - Math.Max(itemMinY, marqueeMinY));
        double overlapArea = overlapDx * overlapDy;

        double totalArea = (Width * cellPitchDips) * (Height * cellPitchDips);
        return totalArea > 0.0 ? overlapArea / totalArea : 0.0;
    }
}

/// <summary>
/// Immutable snapshot of a placement item participating in selection or group translation.
/// </summary>
public sealed record SpatialClusterItem(
    Guid PlacementId,
    string ContentType,
    SpatialRegion Footprint,
    bool IsAnchored
);

/// <summary>
/// Double-buffered, allocation-efficient selection queue.
/// </summary>
public sealed class SelectionQueue : IReadOnlyCollection<Guid>
{
    private readonly List<Guid> _selectionList = new(32);
    private readonly HashSet<Guid> _selectionSet = new(32);

    public Guid? PrimarySelectionId => _selectionList.Count > 0 ? _selectionList[0] : null;
    public int Count => _selectionList.Count;

    public bool Contains(Guid id) => _selectionSet.Contains(id);

    public bool Add(Guid id)
    {
        if (_selectionSet.Add(id))
        {
            _selectionList.Add(id);
            return true;
        }
        return false;
    }

    public bool Remove(Guid id)
    {
        if (_selectionSet.Remove(id))
        {
            _selectionList.Remove(id);
            return true;
        }
        return false;
    }

    public void Clear()
    {
        _selectionList.Clear();
        _selectionSet.Clear();
    }

    public void ReplaceWith(IEnumerable<Guid> newSelection)
    {
        Clear();
        foreach (var id in newSelection)
        {
            Add(id);
        }
    }

    public IEnumerator<Guid> GetEnumerator() => _selectionList.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Active state of a marquee sweep gesture.
/// </summary>
public readonly record struct MarqueeSweepState(
    double StartWorldX,
    double StartWorldY,
    double CurrentWorldX,
    double CurrentWorldY,
    bool IsAdditiveShift
)
{
    public double MinWorldX => Math.Min(StartWorldX, CurrentWorldX);
    public double MinWorldY => Math.Min(StartWorldY, CurrentWorldY);
    public double MaxWorldX => Math.Max(StartWorldX, CurrentWorldX);
    public double MaxWorldY => Math.Max(StartWorldY, CurrentWorldY);
}

/// <summary>
/// Coordinated group translation transaction payload.
/// </summary>
public sealed record GroupTranslationTransaction(
    Guid TransactionId,
    ImmutableArray<SpatialClusterItem> ClusterItems,
    int DeltaCellX,
    int DeltaCellY,
    bool IsValid,
    ImmutableHashSet<CellCoordinate> VacatedCells
);

/// <summary>
/// Engine service contract for spatial group translation validation and execution.
/// </summary>
public interface ISpatialGroupTranslationEngine
{
    /// <summary>
    /// Validates whether a proposed vector translation for a spatial cluster is free of collisions.
    /// </summary>
    bool IsClusterRegionFree(
        IReadOnlyList<SpatialClusterItem> clusterItems,
        int deltaCellX,
        int deltaCellY,
        Guid activeLayerId);

    /// <summary>
    /// Prepares an atomic translation transaction payload.
    /// </summary>
    GroupTranslationTransaction PrepareTransaction(
        IReadOnlyList<SpatialClusterItem> clusterItems,
        int deltaCellX,
        int deltaCellY,
        Guid activeLayerId);

    /// <summary>
    /// Commits an atomic group move transaction to the spatial index and field ledger.
    /// </summary>
    bool CommitTransaction(GroupTranslationTransaction transaction);
}

/// <summary>
/// Concrete implementation of group translation collision evaluation.
/// </summary>
public sealed class SpatialClusterCollisionEvaluator : ISpatialGroupTranslationEngine
{
    private readonly ISpatialIndexProvider _indexProvider;
    private readonly IFieldLedgerEmitter _ledgerEmitter;
    private readonly ISpentTrailQueue _spentTrailQueue;

    public SpatialClusterCollisionEvaluator(
        ISpatialIndexProvider indexProvider,
        IFieldLedgerEmitter ledgerEmitter,
        ISpentTrailQueue spentTrailQueue)
    {
        _indexProvider = indexProvider;
        _ledgerEmitter = ledgerEmitter;
        _spentTrailQueue = spentTrailQueue;
    }

    public bool IsClusterRegionFree(
        IReadOnlyList<SpatialClusterItem> clusterItems,
        int deltaCellX,
        int deltaCellY,
        Guid activeLayerId)
    {
        if (clusterItems.Count == 0) return true;

        // Rule: Any anchored item invalidates group translation.
        for (int i = 0; i < clusterItems.Count; i++)
        {
            if (clusterItems[i].IsAnchored) return false;
        }

        // Build source footprint union set
        var sourceUnion = new HashSet<CellCoordinate>();
        for (int i = 0; i < clusterItems.Count; i++)
        {
            var fp = clusterItems[i].Footprint;
            for (int x = fp.X; x < fp.Right; x++)
            {
                for (int y = fp.Y; y < fp.Bottom; y++)
                {
                    sourceUnion.Add(new CellCoordinate(x, y));
                }
            }
        }

        // Evaluate target footprints for all items
        var spatialIndex = _indexProvider.GetIndexForLayer(activeLayerId);
        for (int i = 0; i < clusterItems.Count; i++)
        {
            var fp = clusterItems[i].Footprint;
            var targetFp = new SpatialRegion(fp.X + deltaCellX, fp.Y + deltaCellY, fp.Width, fp.Height);

            // Query spatial R-Tree for candidate collisions in target region
            var candidateCollisions = spatialIndex.QueryRegion(targetFp);
            foreach (var candidate in candidateCollisions)
            {
                // Skip items inside the translating cluster
                bool isMember = false;
                for (int j = 0; j < clusterItems.Count; j++)
                {
                    if (clusterItems[j].PlacementId == candidate.PlacementId)
                    {
                        isMember = true;
                        break;
                    }
                }
                if (isMember) continue;

                // Intersection with external item -> Collision Refusal
                if (targetFp.Intersects(candidate.Footprint))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public GroupTranslationTransaction PrepareTransaction(
        IReadOnlyList<SpatialClusterItem> clusterItems,
        int deltaCellX,
        int deltaCellY,
        Guid activeLayerId)
    {
        bool isValid = IsClusterRegionFree(clusterItems, deltaCellX, deltaCellY, activeLayerId);

        var sourceCells = new HashSet<CellCoordinate>();
        var targetCells = new HashSet<CellCoordinate>();

        foreach (var item in clusterItems)
        {
            var fp = item.Footprint;
            for (int x = fp.X; x < fp.Right; x++)
            {
                for (int y = fp.Y; y < fp.Bottom; y++)
                {
                    sourceCells.Add(new CellCoordinate(x, y));
                    targetCells.Add(new CellCoordinate(x + deltaCellX, y + deltaCellY));
                }
            }
        }

        // Vacated cells = Source \ Target
        sourceCells.ExceptWith(targetCells);

        return new GroupTranslationTransaction(
            TransactionId: Guid.NewGuid(),
            ClusterItems: clusterItems.ToImmutableArray(),
            DeltaCellX: deltaCellX,
            DeltaCellY: deltaCellY,
            IsValid: isValid,
            VacatedCells: sourceCells.ToImmutableHashSet()
        );
    }

    public bool CommitTransaction(GroupTranslationTransaction transaction)
    {
        if (!transaction.IsValid) return false;

        // 1. Transactional shift in spatial R-Tree
        _indexProvider.BatchUpdatePositions(transaction.ClusterItems, transaction.DeltaCellX, transaction.DeltaCellY);

        // 2. Ledger event emission
        _ledgerEmitter.EmitGroupMoveEvent(transaction);

        // 3. Register spent trail energy for vacated cells
        foreach (var cell in transaction.VacatedCells)
        {
            _spentTrailQueue.EnqueueSpentCell(cell, initialEnergy: 0.60f);
        }

        return true;
    }
}
```

---

## 5. Avalonia 11.2.5 & SkiaSharp Rendering Pipeline Integration

```
+-------------------------------------------------------------------------------+
|                      Avalonia CustomDrawOperation Loop                        |
|                                                                               |
| 1. Render Base Grid Lines & Background                                        |
| 2. Render Placed Content Items                                                |
| 3. Draw Group Translation Ghost / Refusal Overlay                              |
|    - Valid: Skia Paint Fill (0x0F222220), 1.5px Edge (#96B6F8)               |
|    - Refusal: Skia Hatch Path (45° stripes, 0x1EF06543)                       |
| 4. Draw Marquee Sweep Rectangle                                               |
|    - Rect: Continuous World Bounds -> Screen Matrix Transform                 |
|    - Stroke: SKPathEffect.CreateDash([4.0f, 4.0f], 0.0f)                      |
| 5. Draw Spent-Trail Energy Grid Cells (18-step decay render)                  |
+-------------------------------------------------------------------------------+
```

### 5.1 Marquee Sweep Overlay Render Pass (`DrawMarqueeSweepOverlay`)

The continuous marquee selection rect is drawn directly into Skia canvas context during active pointer drag:

```csharp
public void DrawMarqueeSweepOverlay(SKCanvas canvas, MarqueeSweepState marquee, Matrix3x3 cameraTransform)
{
    SKRect worldRect = new(
        (float)marquee.MinWorldX,
        (float)marquee.MinWorldY,
        (float)marquee.MaxWorldX,
        (float)marquee.MaxWorldY);

    SKRect screenRect = cameraTransform.MapRect(worldRect);

    // 1. Fill: --signal-interaction (6% opacity, #222220)
    using var fillPaint = new SKPaint
    {
        Style = SKPaintStyle.Fill,
        Color = new SKColor(0x22, 0x22, 0x20, 0x0F), // 6% alpha
        IsAntialias = true
    };
    canvas.DrawRect(screenRect, fillPaint);

    // 2. Border: 1px Inset Dash (4px dash, 4px gap)
    using var dashEffect = SKPathEffect.CreateDash(new float[] { 4.0f, 4.0f }, 0.0f);
    using var strokePaint = new SKPaint
    {
        Style = SKPaintStyle.Stroke,
        Color = new SKColor(0xF4, 0xF4, 0xF2, 0xE0), // 88% alpha
        StrokeWidth = 1.0f,
        PathEffect = dashEffect,
        IsAntialias = true
    };

    // Inset border geometry by 0.5px to maintain pixel-grid sharpness
    SKRect insetRect = SKRect.Inflate(screenRect, -0.5f, -0.5f);
    canvas.DrawRect(insetRect, strokePaint);
}
```

### 5.2 Group Translation Ghost & Refusal Cross-Hatch Pipeline

When dragging a multi-item selection cluster, candidate offset positions are rendered in real time:

```csharp
public void DrawGroupTranslationGhost(
    SKCanvas canvas,
    IReadOnlyList<SpatialClusterItem> cluster,
    int deltaCellX,
    int deltaCellY,
    bool isValid,
    float cellPitchDips,
    Matrix3x3 cameraTransform)
{
    using var validFillPaint = new SKPaint
    {
        Style = SKPaintStyle.Fill,
        Color = new SKColor(0x22, 0x22, 0x20, 0x18), // 9.4% interaction ghost fill
        IsAntialias = true
    };

    using var validEdgePaint = new SKPaint
    {
        Style = SKPaintStyle.Stroke,
        Color = new SKColor(0x96, 0xB6, 0xF8, 0xFF), // #96B6F8 Edge Highlight
        StrokeWidth = 1.5f,
        IsAntialias = true
    };

    using var refusalFillPaint = new SKPaint
    {
        Style = SKPaintStyle.Fill,
        Color = new SKColor(0xF0, 0x65, 0x43, 0x1F), // 12% refusal fill (#F06543)
        IsAntialias = true
    };

    using var refusalHatchPaint = new SKPaint
    {
        Style = SKPaintStyle.Stroke,
        Color = new SKColor(0xF0, 0x65, 0x43, 0x66), // 40% refusal stroke
        StrokeWidth = 1.5f,
        IsAntialias = true
    };

    foreach (var item in cluster)
    {
        var targetFp = item.Footprint.Translate(deltaCellX, deltaCellY);
        SKRect worldRect = new(
            targetFp.X * cellPitchDips,
            targetFp.Y * cellPitchDips,
            targetFp.Right * cellPitchDips,
            targetFp.Bottom * cellPitchDips);

        SKRect screenRect = cameraTransform.MapRect(worldRect);

        if (isValid)
        {
            canvas.DrawRect(screenRect, validFillPaint);
            canvas.DrawRect(screenRect, validEdgePaint);
        }
        else
        {
            // Refusal state: draw base refusal fill + 45° diagonal cross-hatch
            canvas.DrawRect(screenRect, refusalFillPaint);
            
            canvas.Save();
            canvas.ClipRect(screenRect);
            
            float step = 12.0f; // 12px hatch spacing
            float minDim = Math.Min(screenRect.Width, screenRect.Height);
            for (float d = -screenRect.Height; d < screenRect.Width + screenRect.Height; d += step)
            {
                canvas.DrawLine(
                    screenRect.Left + d, screenRect.Top,
                    screenRect.Left + d + screenRect.Height, screenRect.Bottom,
                    refusalHatchPaint);
            }
            
            canvas.Restore();
        }
    }
}

---

## 6. Signal Role Color Standards & Refusal Overlay Contracts

### 6.1 Signal Role Color Matrix
Group translation and marquee selection enforce 3 non-overlapping signal roles:
- **Interaction (`#96B6F8`)**: Applied to selected item outlines (2px solid outside content edge with soft glow `0 0 24px 6px rgb(150 182 248 / 0.45)`), valid group drag previews, and structural field cell brightening (`rgba(150,182,248,0.13)` edge cells, `0.06` diagonal cells) with perimeter inset `inset 0 0 0 1.5px rgb(150 182 248 / 0.30)`.
- **Marquee (`#E8B964`)**: Applied ONLY while a marquee sweep gesture is open (`rgba(232,185,100,0.08)` fill, 1.5px border `rgba(232,185,100,0.78)`). Hands over to interaction accent `#96B6F8` upon gesture release.
- **Refusal / Invalid (`#F06543` / `#E2625C`)**: Applied ONLY when cluster translation or resize encounters a collision (`!IsClusterRegionFree`). Rendered as a 12% fill (`rgba(226,98,92,0.12)`), $45^\circ$ diagonal cross-hatching, and inset border `inset 0 0 0 1.5px rgba(226,98,92,0.45)`.

### 6.2 Refusal Mechanics & Strip Toolbar Notification
When a group move is refused:
1. **Origin Retention**: All items in the cluster maintain their origin positions without moving.
2. **Point-of-Action Strip**: An inline strip toolbar appears beside the refused footprint ("This space is occupied" with disabled `Place` action and `Cancel` button).
3. **No Screen Modal**: Center-screen dialogs and canvas dimming are strictly forbidden.
```
