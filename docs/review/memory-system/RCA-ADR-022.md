# RCA Ledger: ADR-022 — Memory Spatial R-Tree Index Architecture

| Metadata Field | Value |
| :--- | :--- |
| **ADR ID** | ADR-022 |
| **Claimed Status in Spec Header** | Normative / Accepted |
| **Verified Status** | **PARTIAL — R-tree and tile cache are integrated; benchmark and dedicated helper remain** |
| **Audit Date** | 2026-08-12 |

## Verified implementation

- `SpatialBoundingBox`, `RTreeNode`, `MemorySpatialRTree`, and
  `SpatialQueryCache` exist in `src/GroveApp/Engine/Memory/MemorySpatialIndex.cs`.
- The tree supports multi-layer boxes, insert/update, removal, MBR
  recalculation, node splitting, and caller-provided span queries.
- Anchors index the full Content footprint, not only its origin cell.
- The `64 × 64` tile cache invalidates the old and new footprint on move and
  the old footprint on removal.
- `GridCanvasControl` uses the index for viewport item selection and field
  queries; the old render-only linear visibility scan is no longer the source
  of viewport membership.

## Remaining gaps

- There is no separately named `ViewportCullingHelper`; camera conversion and
  the index query are still composed by the canvas adapter.
- The cached tile materialization allocates arrays on cache misses. The
  caller-provided span contract is present, but a production benchmark has not
  established the ADR's sub-millisecond target.
- The cache is a selective invalidation cache, not an LRU with a bounded memory
  policy.
- Composite layer queries and the render pipeline need scale testing at the
  ADR's stated placement counts.

## Root cause of the original false claim

The original RCA captured the prototype's linear scan before the memory index
was introduced. It remains useful as historical evidence but is false as a
current symbol or integration audit.
