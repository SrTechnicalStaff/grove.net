---
type: canonical-domain-concept
status: accepted
authority: source-of-truth
concept: Gallery Slate
date: 2026-08-12
---

# Gallery Slate

## Definition

Gallery Slate is the focused media browsing and inspection sub-application on
the HUD Plane. It presents media payloads associated with Memory records
without changing their Grid Placement or creating additional Content merely by
being opened.

## Actions

Opening or browsing media is read-only. A person may route from a media record
to Memory Slate, Writing Slate, or an explicit Grid placement action. A new
Grid Content instance is created only when Place is committed.

## Boundary decisions

| Tension | Decision |
| --- | --- |
| Gallery Slate vs Memory | Gallery presents a Memory payload; it does not own the record. |
| Gallery Slate vs Content | A Content origin may be shown as context; the viewer does not become Content. |
| Gallery Slate vs Placement | Viewing does not place. Placement is explicit. |
| Gallery Slate vs Writing Slate | Media inspection and semantic authoring are separate routes. |
