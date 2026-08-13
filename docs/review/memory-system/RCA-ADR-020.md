# RCA Ledger: ADR-020 — Memory Model and Immutable Ledger Architecture

> Historical review ledger. Its claims describe an earlier tree and are not
> domain authority. The current Memory, Content, and Anchor ownership model is
> defined by [`docs/domain/`](../../domain/README.md) and ADR-023.

| Metadata Field | Value |
| :--- | :--- |
| **ADR ID** | ADR-020 |
| **Claimed Status in Spec Header** | Normative / Accepted |
| **Verified Status** | **PARTIAL — record, anchor, and Grid Layer persistence/restore are wired; contextual metadata UI remains** |
| **Audit Date** | 2026-08-12 |

## Verified implementation

- `ContentHash` computes SHA-256 payload identity.
- `MemoryPayloadKind`, `MemoryAnchor`, `MemoryRecord`, `IMemoryLedger`, and
  `ImmutableMemoryLedger` exist under `src/GroveApp/Models/Memory/` and
  `src/GroveApp/Engine/Memory/`.
- Memory and Content identities are separate. A Content instance carries a
  `MemoryId` and its own `AnchorId`; one Memory can have multiple Anchors.
- `MemoryAnchorService` is wired to Content add, move, resize, edit, layer
  transfer, trace, copy/paste, and removal paths.
- Anchor YAML persistence records anchor identity, Memory identity, Content
  identity, grid layer, origin, and footprint. The reader accepts the prior
  anchor format.
- `MemoryRecordFileStore` writes one canonical Markdown or binary sidecar pair
  per Memory record, verifies SHA-256 payload integrity on read, and persists
  anchor metadata with the record.
- Startup hydration imports records into the ledger/version tree, rebuilds the
  spatial index, and reconstructs placed Content from the active anchors.
- Window shutdown awaits the anchor/record persistence flush.
- `M` opens the Memory Slate using one representative Content instance per
  Memory, so repeated placements do not duplicate the semantic record view.

## Remaining gaps

- The Grid Layer stack persists separately from each Memory record, so arbitrary
  future schema migrations still need an explicit versioned migration journal.
- Content-specific contextual labels and extended frontmatter are not exposed
  by the placement UI.

## Root cause of the original false claim

The original review predates the memory subsystem. It searched for the symbols
before the engine was added and was not regenerated after integration. Its
zero-percent conclusion is historical, not current evidence.
