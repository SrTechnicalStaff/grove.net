# RCA Ledger: ADR-040 — Spatial Layer System Architecture

| Metadata Field | Value |
| :--- | :--- |
| **ADR ID** | ADR-040 |
| **Verified Status** | **PARTIAL — executable Grid Layer stack and migration seam are wired; the normative Guid-based contract is not** |
| **Audit Date** | 2026-08-12 |

## Verified implementation

- `SpatialLayerStack` owns the ordered Grid Layer continuum, stable side-coded
  labels, active selection, visibility/lock state, insertion, navigation,
  reordering, and deletion.
- Same-Grid-Layer occupancy is checked against full Content footprints. Content
  on different Grid Layers may share cell coordinates.
- Deleting a Grid Layer validates migration occupancy, transfers its Content to
  the destination Grid Layer, updates each Content Anchor, and preserves the
  Content instance.
- `LayerId`, `LayerLabel`, and `LayerMigrationResult` exist as the current
  integer-backed compatibility contracts. `SpatialLayer` exposes typed label
  accessors and token-backed color state.
- `SpatialLayerFileStore` persists Grid Layer order, stable labels, names,
  visibility, lock state, colors, and active Grid Layer. The canvas restores it
  before hydrating Content anchors.
- Grid rendering consumes active/inactive Grid Layer state through the render
  pipeline and `FieldLedgerEngine`; no separate placeholder
  `SpatialLayerCanvasRenderOperation` is required by the current renderer.

## Remaining gaps

- The accepted ADR's sample contract uses Guid-backed `LayerId` and an
  immutable stack manager interface; the application still uses stable integer
  IDs behind the same named value types.
- Layer persistence does not yet preserve arbitrary future layer-stack schema
  versions or a separate transactional migration journal.

## Root cause of the original false claim

The original RCA was not regenerated after the executable stack and migration
seams were added. Its zero-occurrence search is historical evidence, not the
current source-of-truth implementation result.
