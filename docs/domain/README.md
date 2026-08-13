# Domain model

This directory is the authoritative product-domain source for Grove. Each
concept has its own canonical document so its identity, boundaries, lifecycle,
and relationships can be reviewed without hiding tensions inside a composite
summary.

These documents describe user-experienced behavior, not controls, screens,
render passes, or implementation tasks. UI and implementation artifacts are
projections of this model and must not redefine it.

## Canonical concepts

- [Grid](Grid.md) — unbounded spatial coordinate field.
- [Cell](Cell.md) — addressable Grid unit and evidence membrane.
- [Grid Layer](Grid-Layer.md) — literal depth member of the Grid.
- [Memory](Memory.md) — immutable semantic record and formation lifecycle.
- [Quick Note](Quick-Note.md) — direct Memory capture without required Content.
- [Memory Version](Memory-Version.md) — immutable lineage node created by semantic editing.
- [Trace](Trace.md) — reuse operation that creates Content for an existing Memory.
- [Content](Content.md) — placed spatial instance of a Memory.
- [Anchor](Anchor.md) — authored label attached to Content.
- [Placement](Placement.md) — spatial state and placement operation for Content.
- [Field Ledger](Field-Ledger.md) — spatial evidence system for Content and
  cells.
- [Aura](Aura.md) — field emitted by Content.
- [Gap](Gap.md) — weak spatial relation across empty cells.
- [Spatial Recall Weight](Spatial-Recall-Weight.md) — derived ordering evidence.
- [Memory Search](Memory-Search.md) — semantic lookup and spatial recall
  ranking.
- [Memory Slate](Memory-Slate.md) — Memory-record gallery and placement handoff.
- [Writing Slate](Writing-Slate.md) — long-form semantic authoring surface.
- [Gallery Slate](Gallery-Slate.md) — focused media inspection surface.
- [Annotation](Annotation.md) — derived reading surface from field context.
- [Blip](Blip.md) — minimal field-cluster presence projection.
- [Plane](Plane.md) — visual composition surface.
- [Tier](Tier.md) — presentation grouping or representation level.
- [Slate](Slate.md) — full or half HUD sub-application surface.
- [Overlay](Overlay.md) — transient operation surface.

[Relationships](relationships.md) is the integration map between these
concepts. It is not a replacement for their individual definitions.

## Change rule

A concept document changes only when its domain meaning changes through an
accepted domain decision. Product explainers, UI specifications, tickets,
reviews, and generated plans may link to a concept document but are not
allowed to become alternate definitions.
