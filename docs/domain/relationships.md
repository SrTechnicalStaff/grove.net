---
type: canonical-domain-relationship-map
status: accepted
authority: source-of-truth
date: 2026-08-12
---

# Domain relationships

This map connects the individual concept definitions. The individual files
remain authoritative for each concept's boundaries.

```text
Memory
  ^
  | Content.MemoryId
  |
Content ---- optional ----> Anchor
  |
  | owns spatial facts
  v
Placement
  |
  v
Grid cells -> Field Ledger -> Aura / Gap evidence
                           |
                           v
                     Memory Search
```

## Relationship direction

- Content references Memory.
- Anchor labels Content.
- Placement describes Content.
- Field Ledger observes Content and cells.
- Aura is emitted by Content and recorded as derived cell evidence.
- Gap is weak distance evidence between nearby Content.
- Memory Search returns Memory records and joins outward to Content and Anchor.

## Core journeys

### Capture

Capture commits a Memory. Content is optional.

### Place

Place resolves or creates a Memory, then creates Content with `MemoryId` and
Placement facts. It does not place or mutate the Memory.

### Anchor

Anchor labels Content. It does not alter Memory payload.

### Recall

Search matches Anchor labels, Memory titles, and Memory payload, then uses
Field Ledger evidence from Content, Aura overlap, saturation, and nearby Gaps
to order comparable Memory results.

### Remove

Remove deletes Content-side spatial and label relationships. The Memory
remains searchable.
