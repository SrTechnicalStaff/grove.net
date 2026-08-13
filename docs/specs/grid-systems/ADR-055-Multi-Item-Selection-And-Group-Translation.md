---
status: "PARTIAL — rigid movement, collision planning, anchor-independent translation, trails, and selected Aura projection are implemented; broader service ownership remains"
authority: normative
date: 2026-08-12
---

# ADR-055: Multi-Item Selection and Group Translation

## Decision

Selected Content translates as a rigid spatial cluster. Every selected item
receives the same integer cell delta, preserving footprint geometry and
pairwise offsets.

Selection order and primary-selection behavior are owned by `SelectionService`.
Group movement is owned by a translation module with a small interface:

```csharp
GroupTranslationPreview Preview(
    IReadOnlyList<ContentFootprint> cluster,
    IReadOnlyList<ContentFootprint> occupancy,
    CellDelta delta);

GroupTranslationCommit Commit(GroupTranslationPreview preview);
```

The module owns source snapshots, target-union calculation, external
occupancy validation, atomic coordinate mutation, and the vacated-cell result.
The Canvas only translates pointer state and consumes results.

## Marquee selection

The marquee selects a Content item only when it fully encloses every occupied
cell in that Content footprint. The old 50% overlap rule is not part of Grove.

## Collision contract

Let `U_source` be the union of the original cluster footprints and `U_target`
the translated union. Only:

```text
U_target \ U_source
```

is checked against external Content on the same Grid Layer. Cluster members
may overlap their own source union during translation. Any external occupied
cell rejects the entire transaction; no item is partially moved.

An Anchor is authored context attached to Content. It is not a movement lock.
Moving anchored Content is valid and updates its Content-side Anchor geometry.

Refusal results distinguish collision, locked Grid Layer, invalid geometry, and
cancelled gesture. “Collision detected” is never used for another refusal.

## Trail contract

On commit, the translation module returns the vacated cell union. The canonical
trail model records those cells with `E0 = 0.60`, decay `0.84`, and an 18-frame
retention threshold.

## Acceptance

A multi-selection can move into free space, refuses atomically on external
collision, preserves Content/Memory/Anchor identity, updates spatial indexes,
refreshes Aura, and emits trails for every vacated cell.
