# RCA Ledger: ADR-021 — Memory Lineage and Version Tree Architecture

| Metadata Field | Value |
| :--- | :--- |
| **ADR ID** | ADR-021 |
| **Claimed Status in Spec Header** | Normative / Accepted |
| **Verified Status** | **PARTIAL — lineage, deltas, LCA, and merge engine exist; no persisted history UI** |
| **Audit Date** | 2026-08-12 |

## Verified implementation

- `DeltaOpKind`, `DeltaChunk`, `MemoryDelta`, `MemoryVersionNode`,
  `IMemoryVersionTree`, and `MemoryVersionTree` exist.
- `MemoryDelta` uses a Myers shortest-edit path and reconstructs the target
  payload through `Apply`.
- Root, parent, root lineage, generation, branch heads, cycle checks, and LCA
  traversal are enforced by the version tree.
- Content edits append child Memory records through `MemoryAnchorService`; the
  edited Content moves to the child while other Content remains bound to the
  prior Memory.
- Non-overlapping text branch edits are merged into a new immutable child.
  Overlapping edits raise `MemoryMergeConflictException`; no conflict marker is
  inserted into user payload data.
- Version nodes can retain a computed text delta from their parent when a ledger
  is supplied.

## Remaining gaps

- Version nodes and deltas are not persisted to the lineage YAML protocol.
- There is no user-facing branch/history/merge surface.
- Binary payload branching records full payload bytes; delta compression is
  currently text-only.
- Merge creates a single-parent child, so the second branch is represented by
  the merge operation rather than a multi-parent node.

## Root cause of the original false claim

The original review was a pre-integration symbol scan. It described the
prototype state and was not updated after the version engine was introduced.
