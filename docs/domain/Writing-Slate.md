---
type: canonical-domain-concept
status: accepted
authority: source-of-truth
concept: Writing Slate
date: 2026-08-12
---

# Writing Slate

## Definition

Writing Slate is the long-form authoring sub-application for semantic Memory
content. It is hosted on the HUD Plane as a full-viewport or half-viewport
Slate and is designed for document-like reading and editing rather than a
collection of generic containers.

## Authoring contract

The editor works on a Memory payload or on a Content-bound Memory Version. It
supports the rich-text and document behaviors defined by the writing product
contract. Saving semantic changes commits a new immutable Memory Version.

When Writing Slate was opened from Content, the originating Content may be
rebound to the new version. Other Content instances remain bound to their
previous versions unless explicitly edited.

Moving or resizing the originating Content is not a writing operation. Adding
an authored local label is an Anchor operation on Content, not a payload edit.

## Boundary decisions

| Tension | Decision |
| --- | --- |
| Writing Slate vs Memory | Slate edits by creating a new version; it does not mutate an old record. |
| Writing Slate vs Content | Content supplies origin context; its Placement is not edited by text formatting. |
| Writing Slate vs Anchor | Anchor context is a separate Content-side operation. |
| Writing Slate vs Quick Note | Quick Note is direct capture; Writing Slate is long-form authoring. |
| Writing Slate vs Local editor | A local editor route must not collapse Quick Note and Writing Slate into one feature. |
