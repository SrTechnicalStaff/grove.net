# RCA Ledger: ADR-050 — Footprint-Aware Grid Cursor and Spent-Cell Trails

| Metadata Field | Value |
| :--- | :--- |
| **ADR ID** | ADR-050 |
| **Verified Status** | **PARTIAL — one canonical recursive cursor/trail model is live; the standalone GPU draw-operation seam remains** |
| **Audit Date** | 2026-08-12 |

## Verified implementation

- `CanonicalCursorTrailModel` is the single cursor state model. Empty-space
  cursor tiers are selected from Grid Layer pitch constants and camera scale;
  Content, armed-tool, and drop-preview footprints use the same descriptor
  path.
- There is no second single-cell cursor model. Recursive empty-space tiers are
  minor grid, major Grid cell, and supercell; Content and tool footprints take
  precedence when addressed.
- Spent trails store complete world footprints and vacated Content cells, with
  the token-backed 18-step decay model.
- The operating-system cursor is hidden over the Grid except while a resize
  handle is active, when the matching diagonal cursor is shown.

## Remaining gaps

- `IGridCursorService`, `CursorRole`, and `GridCursorDrawOperation` are not
  separate runtime contracts; descriptor resolution and feedback drawing remain
  in the existing engine/control seam.
- The current trail draw operation is custom Skia, but cursor ring/fill drawing
  still uses the Avalonia drawing adapter.

## Root cause of the original false claim

The earlier review inspected the former single-cell path and did not account for
the later canonical descriptor resolver and recursive tier model.
