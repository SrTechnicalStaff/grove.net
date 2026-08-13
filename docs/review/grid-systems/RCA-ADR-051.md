# RCA Ledger: ADR-051 — Interactive Resize and Cell Alignment

| Metadata Field | Value |
| :--- | :--- |
| **ADR ID** | ADR-051 |
| **Verified Status** | **PARTIAL — four-handle, type-specific, collision-aware resize is wired; the standalone service/draw-operation seam is not** |
| **Audit Date** | 2026-08-12 |

## Verified implementation

- `TypeSpecificFootprintSolver` handles Note, Document, and Image footprint
  rules. `ResizeGeometrySolver` owns candidate geometry, collision evaluation,
  and all four handle hit tests.
- `GridCanvasControl` runs preview/commit/cancel transactions for all Content
  types, preserves the original footprint when a candidate is refused, updates
  the Memory Anchor on commit, and exposes native diagonal cursors.
- Refused candidates render token-backed diagonal hatching and the point-of-
  action occupied-space strip.

## Remaining gaps

- The canvas remains the interaction adapter; the draft `ISpatialResizeService`
  and `ResizePreviewDrawOperation` names are not separate runtime contracts.
- The refusal strip is rendered in the feedback module rather than as a HUD
  control with independent focus semantics.

## Root cause of the original false claim

The original audit was generated against the earlier one-corner Note prototype
and was not regenerated after the type-specific resize seam was introduced.
