---
type: product-definition
status: derived
authority: derived-from-domain-model
source_of_truth: ../../domain/Memory.md
version: v9
date: 2026-08-11
tags: [grove, product-definition, memory, tracing, anchoring]
---

# Product Definition: Memory

> **What is a memory?**
> A **Memory** is an immutable semantic record with stable identity, payload, and version lineage. It exists independently of Content, Placement, and Anchor records. A Memory may be referenced by no Content, one Content instance, or many Content instances.

---

## 1. Core Essence ("What is a memory?")

- **Canonical Statement**: A Memory is the semantic record that retains identity across space and time. Content is a placed instance that references the Memory through `MemoryId`; one Memory may be referenced by any number of Content instances, or by none.
- **Primary Function**: Memory decouples information from spatial location. Creating a Memory never requires Content. Placing a source that has no Memory creates the Memory first, then creates Content. Placing an existing Memory creates new Content without duplicating or mutating the Memory.
- **Mental / Physical Model**: 
  1. *Animation Keyframes & Cel Tracing*: Like an animator reusing a single character drawing on multiple transparent animation cels with slight contextual alterations per frame, a Memory is reused through different Content instances on Grid Layers to express different roles (e.g., composition vs. perspective vs. narrative).
  2. *Mental Cognition & Association*: Human memories are singular entities that acquire new facets whenever recalled in different real-world contexts.

---

## 2. Fundamental Invariants & System Properties

1. **Semantic Singularity**: A Memory is a single underlying data record. Creating Content across Grid Layers creates additional Content records that reference the same Memory, not independent payload copies.
2. **Versioned Editing**: Editing one Content instance creates a child Memory version for that Content instance. Other Content instances remain bound to their existing Memory version.
3. **External Spatial Relationships**: Memory owns no Placement or Anchor collection. Content may have an optional Content-side Anchor, and a single Memory may be referenced by many Content/Anchor relationships.
4. **User-Driven Relationship Ownership**: Quick Note and other capture actions may create a Memory without creating Content. Creating a new Memory from existing material creates a distinct record; placing an existing Memory creates Content that preserves its identity.
5. **Disk Provenance Synchronization**: For frontmatter-compatible file formats (e.g., Markdown), disk files reflect tracing metadata via structured frontmatter fields (`Trace of: [source_uri]` and `Traces: [variant_uris]`), while non-frontmatter files fall back to filename lineage tracking.

### Data & State Schema
- **State Ownership**: Memory Store / Kernel Core (`MemoryRepository`).
- **Memory Object Attributes**:
  - `memory_id`: Globally unique identifier (UUIDv4).
  - `raw_payload`: Primitive content payload (rich text markup, binary image reference, note string).
  - `content_relationships`: External Content records joined by `memory_id`; not stored on Memory.
  - `anchor_relationships`: External Content-side labels joined through Content; not stored on Memory.
  - `trace_variants`: Version or Content relationships describing contextual reuse; not spatial state on Memory.
  - `created_at` / `updated_at`: Timestamps.
- **Persistence Boundary**: Persisted to local disk storage as Markdown documents with YAML frontmatter or raw media files with companion sidecar metadata.

---

## 3. Intersectionality Matrix

| Primitive | Intersection & Relational Rules |
| :--- | :--- |
| **Grid** | Memory has no grid coordinates. Content referencing the Memory occupies cells and establishes spatial relationships with surrounding Content. |
| **Memory** | *Self-Intersection*: Memories are found through Content-side Anchor context and Content reuse; those external relationships do not become Memory-owned state. |
| **Content** | Content is a placed spatial instance that references a Memory through `MemoryId`. Modifying Content creates a new Memory version and updates that Content's reference; other Content remains on its prior version. |
| **Aura** | Content is the sole Aura source. The referenced Memory supplies semantic payload and identity, while Content geometry and type determine field emission. |
| **Grid Layer** | Content instances referencing one Memory may exist on multiple Grid Layers with distinct spatial context. |
| **Annotation** | Annotations aggregate Memories from overlapping Aura fields across all Grid Layers, organizing them into broadsheet media templates for overview reading. |
| **Blip** | A Blip signals an active cluster of Content and their referenced Memories whose combined Aura fields reach a local threshold on the Spatial Grid Plane. |
| **Slate** | Slates (e.g., Memory Slate, Writing Slate) provide direct, non-spatial viewing and editing interfaces for raw Memory payloads and their trace lineage. |

---

## 4. User Interaction & Camera Dynamics

- **Cursor Armed States**:
  - `Trace` Mode (`1` key cycle): Armed cursor allows the user to create new Content referencing a Memory on an adjacent Grid Layer without copying or mutating the Memory payload.
  - `Copy` / `Duplicate` Modes: Generate a distinct, disconnected new Memory record (unlike `Trace`).
- **Camera Zoom / Level-of-Detail (LOD) Behavior**:
  - At close camera distance, Memories render legible Content text and image thumbnails.
  - At macro zoom, individual Memories collapse into Aura field points and Blips, preserving visual clarity across large knowledge bases.
- **Navigation Mechanics**:
  - Clicking a Content Anchor or trace link in an Annotation or named Slate shifts the camera to the referenced Content's coordinates and native Grid Layer.

---

## 5. Derived Outcomes Mapping

- **[Multi Contextual Representation Without Duplication](../derived-outcomes/Multi%20Contextual%20Representation%20Without%20Duplication.md)**: Users apply the same core idea to multiple project phases or perspectives without creating redundant, desynchronized file copies.
- **[Preserve Context Through Anchoring](../derived-outcomes/Preserve%20Context%20Through%20Anchoring.md)**: Users label Content with authored Anchors based on spatial surroundings, preserving true cognitive context rather than forcing folder hierarchies.
  - **[Recall Memories](../derived-outcomes/Recall%20Memories.md)**: Users search every Memory through Content-side Anchor labels, Memory identity fields, and Memory payload, then navigate associated Content and Grid Layer context.
