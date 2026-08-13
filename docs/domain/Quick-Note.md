---
type: canonical-domain-concept
status: accepted
authority: source-of-truth
concept: Quick Note
date: 2026-08-12
---

# Quick Note

## Definition

Quick Note is the direct-capture operation for committing a new Memory with
minimal interruption. It creates a semantic record without requiring a Grid
destination or a Content instance.

## Result

Submitting a Quick Note commits a Memory payload and identity. The result is
available to Memory Search and Memory Slate even when it has zero Content. A
Quick Note does not silently place Content and does not create an Anchor.

If the person later chooses Place, that separate operation creates Content with
the Quick Note's `MemoryId`. If the person supplies local authored context at
that point, the new Content receives an Anchor.

## Boundary decisions

| Tension | Decision |
| --- | --- |
| Quick Note vs Memory | Quick Note is a Memory-creation operation. |
| Quick Note vs Content | Quick Note does not require or create Content. |
| Quick Note vs Anchor | No Anchor exists unless a later Content placement supplies authored context. |
| Quick Note vs Writing Slate | Quick Note is rapid capture; Writing Slate is long-form authoring. |
| Quick Note vs Local editor | A local capture surface must not redefine Quick Note as a spatial editor or a full writing application. |
