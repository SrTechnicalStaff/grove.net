# RCA Ledger: ADR-053 — Spatial CRUD Operations and Selection

| Metadata Field | Value |
| :--- | :--- |
| **ADR ID** | ADR-053 |
| **Verified Status** | **PARTIAL — selection service, 50% marquee qualification, structured clipboard, and CRUD paths are wired; the custom draw-operation/readout seams remain** |
| **Audit Date** | 2026-08-12 |

## Verified implementation

- `SelectionService` owns selected Content IDs, primary selection, marquee
  lifetime, additive selection, and the normative minimum 50% footprint-area
  overlap rule.
- `SpatialClipboardContainer` and `SpatialClipboardItemPayload` preserve
  relative placement, footprint, Memory identity, and image intrinsic size.
- Grid Canvas routes add, remove, copy, paste, delete, selection, and refusal
  behavior through the selection and Content seams.
- Marquee and placement feedback use the design-system interaction/work/refusal
  roles without a modal canvas wash.

## Remaining gaps

- `ISpatialCrudService` is not yet a separate service; CRUD remains on the
  canvas adapter.
- Marquee drawing and the footprint size readout remain high-level feedback
  rendering rather than the draft `MarqueeDrawOperation` contract.

## Root cause of the original false claim

The earlier RCA searched for draft type names and generic JSON clipboard usage,
missing the current structured container and selection service.
