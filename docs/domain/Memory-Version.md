---
type: canonical-domain-concept
status: accepted
authority: source-of-truth
concept: Memory Version
date: 2026-08-12
---

# Memory Version

## Definition

A Memory Version is one immutable semantic record in a Memory lineage. A
version is itself a Memory with its own stable `MemoryId`; version lineage
explains how records relate without making any record mutable.

## Formation

When semantic content is edited, Grove commits a new Memory Version and links
it to the prior record through lineage metadata. The prior record remains
valid and searchable. Editing a Content instance then changes that Content's
`MemoryId` to the new version.

Other Content instances continue to reference the prior version unless the
person explicitly edits or rebinds them. This prevents an edit made in one
spatial context from silently rewriting every context that used the prior
record.

## Lineage facts

A version may expose:

- its parent version and root lineage identity;
- generation or ordering metadata;
- immutable payload and integrity hash;
- creation time and source of the edit.

Lineage is semantic history. It is not a placement history and does not record
where Content has been moved.

## Boundary decisions

| Tension | Decision |
| --- | --- |
| Version vs edit-in-place | Editing creates a new immutable record. |
| Version vs Content | Content chooses which version it references; the version owns no Content list. |
| Version vs Placement | Moving or resizing Content never creates a version. |
| Version vs Anchor | Adding or changing Content context never creates a Memory Version. |
