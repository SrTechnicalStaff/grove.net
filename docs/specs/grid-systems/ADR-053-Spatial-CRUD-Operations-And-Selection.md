---
status: "PARTIAL — full-footprint selection and selected Aura projection are implemented; marquee readout and structured clipboard container remain"
authority: normative
date: 2026-08-12
---

# ADR-053: Spatial CRUD Operations and Selection

## Decision

Selection is owned by a deep in-process selection module. The Grid Canvas is
an input and drawing adapter. Selection state contains ordered Content IDs,
the primary Content ID, additive Shift behavior, and the active marquee.

## Marquee membership

The marquee is projected to an inclusive integer `SpatialRegion`. A Content
candidate is selected if and only if the marquee contains its complete
footprint:

```text
marquee.X <= content.X
marquee.Y <= content.Y
marquee.Right >= content.Right
marquee.Bottom >= content.Bottom
```

Partial coverage never selects Content. The rule is symmetric for all drag
directions and applies to Note, Document, and Image footprints.

The former 50% area-overlap rule is retired. No continuous-area threshold is
used for selection.

## Selection rendering

Selection has two visible parts:

1. a content affordance around the complete Content footprint;
2. a discrete Aura highlight for the selected Content's Field Ledger
   contribution outside and inside that footprint.

In shared Aura cells, selected contributions are highlighted while unselected
contributions remain passive. Selection never paints an opaque wash over
Content and never uses a gradient or glow.

## CRUD contract

The selection module exposes ordered selection snapshots and marquee preview/
commit operations. Content mutation remains behind the Canvas host until a
separate CRUD module has at least two real adapters. Structured spatial copy,
paste, delete, anchor-label, and trace operations must preserve the
Memory/Content relationship.

## Acceptance

The complete journey is: begin marquee, preview complete-footprint membership,
show Content plus selected Aura evidence, commit or cancel, then return focus
to the Grid without altering passive field state.
