---
status: "SUPERSEDED — use ADR-023 and docs/domain/Anchor.md"
authority: historical
date: 2026-08-12
---

# ADR-052: Content Anchor System and Memory Binding

This ADR is superseded. Its former `IsAnchored` pin/lock behavior was a domain
error that conflated authored context with movement protection.

The accepted contract is:

- Memory is an immutable semantic record with no spatial state.
- Content references Memory and owns its spatial Placement.
- Anchor is optional authored context attached to Content.
- Moving or resizing Content updates the Anchor's spatial record when needed.
- An Anchor never blocks Content movement and never changes Memory identity.

Use [`ADR-023`](../memory-system/ADR-023-Memory-Content-Anchor-Relationship.md)
and [`docs/domain/Anchor.md`](../../domain/Anchor.md) for the active contract.
