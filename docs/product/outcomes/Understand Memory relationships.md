---
type: product-outcome
status: discovery
date: 2026-08-08
owner: "Grove Product"
roadmap: "[[../../roadmaps/Memory roadmap]]"
source_notes:
  - "[[../../raw/original-notes/Grove - Field ledger]]"
  - "[[../../raw/original-notes/Grove - information layer]]"
---

# Understand Memory relationships

## Purpose

Define how spatial facts help a person understand relationships between Memories
without assigning meaning that the person did not provide.

## Background

Grid and Field derive distance, Layer separation, overlap, bearing, and gaps
from Placement geometry. Memory owns durable identity and may reference those
facts, but proximity is not automatically a shared topic.

## Outcome

> I can understand how my Memories are spatially related without mistaking proximity or overlap for shared meaning.

## Behavior scenarios

### Inspect explainable facts

I can see which spatial facts produced a relationship, including the two Memory
identities and the Placement revisions involved.

### Handle change

When Content moves, resizes, transfers, or the Field model changes, Grove does
not present stale relationship evidence as current.

### Include unplaced Memories

An unplaced Memory remains durable and is clearly distinguished from a Memory
that has spatial evidence.

## Context

Field produces attributable facts. Memory decides how those facts become
relationship context. Retrieval and Annotation consume the same evidence and
must not recalculate their own spatial math.

## Decisions

- Relationship evidence must remain attributable and versioned.
- An opaque all-pairs ranking is not the first implementation.
- Raw Cell inspection is a developer diagnostic, not this outcome.

## Non-goals

- Automatic semantic grouping.
- A second relationship or identity store.
- Spatial retrieval UI before its entry and return journey is resolved.

## Evidence

- Discovery: [[Memory–Field relationships]]
- Reference: [[../../reference/Field and relationship model]]
