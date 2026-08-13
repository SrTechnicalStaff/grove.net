# RCA Ledger: ADR-057 — Keybind Arming and Ghost Placement

| Metadata Field | Value |
| :--- | :--- |
| **ADR ID** | ADR-057 |
| **Verified Status** | **PARTIAL — arming, recursive ghost placement, collision tint, and rejection feedback are wired; the standalone draw-operation contract is not** |
| **Audit Date** | 2026-08-12 |

## Verified implementation

- `ToolArmingStateMachine` implements the Idle, armed Note, armed Quick Note,
  armed Document, and Placed transitions and now exposes the
  `IToolArmingService` seam.
- `GhostPlacementDescriptor` carries the canonical recursive cursor footprint,
  target Grid Layer, and occupancy validity.
- `GridCanvasControl` updates the ghost from pointer movement, commits only
  free regions, creates Content through the normal Memory/Anchor path, and
  keeps the tool armed after a refused placement.
- Rejected placement produces the discrete refusal pulse used by the Grid
  feedback renderer; Quick Note focus is handed to its editor after placement.

## Remaining gaps

- `PlacementCommitResult` exists as a domain contract, but the current commit
  adapter still returns the descriptor and raises placement events separately.
- Ghost rendering remains a bounded Avalonia drawing pass rather than the
  draft `GhostPlacementDrawOperation` custom draw operation.

## Root cause of the original false claim

The original review searched for the draft namespace and custom draw-operation
names instead of validating the live arming behavior. It therefore reported a
missing state machine even though the concrete behavior was already present.
