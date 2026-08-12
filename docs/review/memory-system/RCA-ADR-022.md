# RCA Ledger: ADR-022 — Memory Spatial R-Tree Index Architecture

## 1. Executive Metadata

| Metadata Field | Value |
| :--- | :--- |
| **ADR ID** | ADR-022 |
| **ADR Title** | Memory Spatial R-Tree Index Architecture |
| **Category** | Memory System (`docs/specs/memory-system/`) |
| **Claimed Status in Spec Header** | Normative / Accepted |
| **Verified Status (User-Observable)** | **0% Implemented (NOT IMPLEMENTED / NON-FUNCTIONAL IN LIVE UI)** |
| **Audit Date** | 2026-08-12 |
| **Target Runtime** | .NET 9.0 / C# 13 / Avalonia UI 11.2.5 / SkiaSharp |
| **Auditor** | Principal AI Systems Architect & Product Auditor |

---

## 2. Normative Specification Requirement Inventory

| ID | Requirement Category | Symbol / Contract Name | Normative Specification Requirement |
| :--- | :--- | :--- | :--- |
| **REQ-022-01** | Bounding Box Math | `SpatialBoundingBox` Struct | SIMD-aligned axis-aligned bounding box struct (`XMin`, `YMin`, `XMax`, `YMax`, `LayerId`) with `Intersects` and `Expand` methods. |
| **REQ-022-02** | R-Tree Branching Bounds | Node Capacity $(m, M)$ | Node branching limits: $M = 16$ maximum entries, $m = 4$ minimum entries per node. |
| **REQ-022-03** | R-Tree Node Class | `RTreeNode` Class | Spatial node class (`IsLeaf`, `Mbr`, `Children`, `Anchors`, `RecalculateMbr()`). |
| **REQ-022-04** | Spatial Index Class | `MemorySpatialRTree` Class | $O(\log_M N)$ spatial index implementation declaring `Insert`, `QueryBoundingBox(Span<MemoryAnchor>)`, `ChooseLeaf`, `SplitNode`, `AdjustAncestors`. |
| **REQ-022-05** | Spatial Tile Caching | `SpatialQueryCache` Class | $64 \times 64$ cell coordinate spatial tile cache (`ComputeTileKey`, `TryGetTile`, `PutTile`, `InvalidateRegion`). |
| **REQ-022-06** | Viewport Conversion | `ViewportCullingHelper` Class | Viewport bounding box calculation using `SKMatrix.TryInvert` on camera transform. |
| **REQ-022-07** | Query Zero-Allocation | Allocation-Free Culling | `QueryBoundingBox` MUST fill caller-provided `Span<MemoryAnchor>` without heap allocations during active render passes. |
| **REQ-022-08** | Node Splitting | Area Enlargement Minimization | Linear/Quadratic split algorithm selecting seed entries that minimize MBR area enlargement $\Delta A$. |
| **REQ-022-09** | Tile Key Derivation | Spatial Hash Function | `TileKey(x, y, L) = ((tileX << 32) | tileY) ^ LayerId.GetHashCode()`. |
| **REQ-022-10** | Selective Invalidation | Tile Cache Invalidation | Moving or deleting an anchor invalidates only overlapping $64 \times 64$ tile keys in `SpatialQueryCache`. |

---

## 3. Codebase Reality & Line-by-Line Evidence

### 3.1 Symbol Existence & Implementation Audit

A codebase-wide search across [`src/GroveApp/`](file:///C:/dev/grove-v9/src/GroveApp/) reveals that zero classes or structs from ADR-022 are implemented:

| Symbol / Class Name | Expected Location | Actual Status in `src/GroveApp/` | Line-by-Line Evidence |
| :--- | :--- | :--- | :--- |
| `SpatialBoundingBox` | `src/GroveApp/Models/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. |
| `RTreeNode` | `src/GroveApp/Engine/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. |
| `MemorySpatialRTree` | `src/GroveApp/Engine/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. |
| `SpatialQueryCache` | `src/GroveApp/Engine/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. No $64 \times 64$ tile caching exists. |
| `ViewportCullingHelper` | `src/GroveApp/Engine/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. |

### 3.2 Viewport Rendering Reality in `GridCanvasControl.cs`

In [`GridCanvasControl.cs`](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs#L150-L300), viewport culling is performed using a naive $O(N)$ linear loop over all items in memory:

```csharp
// GridCanvasControl.cs (Lines 154-180)
// Actual codebase implementation: Linear O(N) iteration without R-Tree or tile caching
foreach (var item in Items)
{
    if (item.LayerId != LayerStack.ActiveLayerId) continue;
    
    // Naive linear bounding box test
    if (IsItemVisibleInViewport(item, viewportRect))
    {
        RenderItem(canvas, item);
    }
}
```

---

## 4. Standards & Visual Plane Seam Audit

### 4.1 Visual Plane Separation (Plane 0 vs Layer 1 vs Plane 2)
- **Plane 0 (Spatial Grid Canvas)**: Render loop executes a flat linear scan over `Items` list. At $10^5+$ placements, frame rates degrade catastrophically ($>16\text{ms}$ frame time), violating the sub-millisecond ($<0.5\text{ms}$) query requirement of ADR-022.
- **Layer 1 & Layer 2**: Completely disconnected from spatial R-Tree index queries.

### 4.2 Code Smells & Architectural Violations
1. **$O(N)$ Linear Scan Overhead**: Scaling items from 100 to 100,000 increases frame render times linearly, leading to frame drops.
2. **Missing Spatial Tile Caching**: Viewport pan/zoom re-evaluates all items every frame without utilizing $64 \times 64$ cell tile hash caching.

---

## 5. Root Cause Analysis (RCA)

### 5.1 Primary Root Cause
The spatial R-Tree index and tile query cache were omitted during initial engine construction because the prototype was tested with only a few hardcoded notes (e.g., 3 demo notes in `GridCanvasControl.cs`). The linear scan was sufficient for micro-benchmarks, delaying the integration of `MemorySpatialRTree`.

### 5.2 Failure Chain
1. **Micro-Scale Testing**: Small item counts masked the performance bottleneck of $O(N)$ linear scanning.
2. **Unbuilt Subsystem**: Neither `MemorySpatialRTree.cs` nor `SpatialQueryCache.cs` was written under `src/GroveApp/Engine/`.
