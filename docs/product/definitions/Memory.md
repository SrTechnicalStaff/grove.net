---
type: product-definition
status: canonical
version: v9
date: 2026-08-11
tags: [grove, product-definition, memory, tracing, anchoring]
---

# Product Definition: Memory

> **What is a memory?**
> A **Memory** is the foundational semantic record and substrate of human thought within Grove. It represents an immutable core unit of information (text, image, thought, or reference) that exists independent of any single spatial placement, allowing it to be instantiated, anchored, and traced across multiple spatial layers under distinct contextual meanings without data duplication.

---

## 1. Core Essence ("What is a memory?")

- **Canonical Statement**: A Memory is the underlying semantic object that retains identity across space and time. When placed on the Grid, it manifests as Content; when traced across Layers, it gains contextual variants while maintaining a single unified provenance record.
- **Primary Function**: Memory decouples pure information from spatial location. It allows a single piece of knowledge to participate in multiple workflows, hold multiple anchors, and acquire layered contextual nuances without generating fragmented, out-of-sync copies.
- **Mental / Physical Model**: 
  1. *Animation Keyframes & Cel Tracing*: Like an animator reusing a single character drawing on multiple transparent animation cels with slight contextual alterations per frame, a Memory is traced across different Layers to express different roles (e.g., composition vs. perspective vs. narrative).
  2. *Mental Cognition & Association*: Human memories are singular entities that acquire new facets whenever recalled in different real-world contexts.

---

## 2. Fundamental Invariants & System Properties

1. **Semantic Singularity**: A Memory is a single underlying data record. Tracing a Memory across Layers creates context variants, not independent file copies.
2. **Context Variance via Tracing**: When a Memory is placed on multiple Layers via Tracing, each placement gains a layer-specific variant record (`Trace of` / `Traces`), allowing the Memory to hold distinct contextual meanings simultaneously.
3. **Anchor Multiplicity**: A single Memory can hold multiple Anchors corresponding to different spatial surroundings, capturing the natural complexity of human thought passively.
4. **User-Driven Relationship Ownership**: Creating new content by highlighting existing material instantiates a new Memory; the user retains explicit ownership over placing and defining its relationship relative to source material.
5. **Disk Provenance Synchronization**: For frontmatter-compatible file formats (e.g., Markdown), disk files reflect tracing metadata via structured frontmatter fields (`Trace of: [source_uri]` and `Traces: [variant_uris]`), while non-frontmatter files fall back to filename lineage tracking.

### Data & State Schema
- **State Ownership**: Memory Store / Kernel Core (`MemoryRepository`).
- **Memory Object Attributes**:
  - `memory_id`: Globally unique identifier (UUIDv4).
  - `raw_payload`: Primitive content payload (rich text markup, binary image reference, note string).
  - `anchors`: List of `{ anchor_id, spatial_coordinate, label_text }`.
  - `trace_variants`: Array of `{ layer_id, grid_coordinate, context_notes, frontmatter_metadata }`.
  - `created_at` / `updated_at`: Timestamps.
- **Persistence Boundary**: Persisted to local disk storage as Markdown documents with YAML frontmatter or raw media files with companion sidecar metadata.

---

## 3. Intersectionality Matrix

| Primitive | Intersection & Relational Rules |
| :--- | :--- |
| **Grid** | Memory has no innate physical dimensions until placed on the Grid; once placed, its location dictates spatial proximity and relationship to surrounding Memories. |
| **Memory** | *Self-Intersection*: Memories connect via Anchors and Tracing relationships (`Trace of` / `Traces`), forming non-duplicative semantic networks across the workspace. |
| **Content** | Content is the spatial manifestation of a Memory placed on the Grid. Modifying Content in-place modifies the underlying Memory payload or its layer variant. |
| **Aura** | The semantic energy of a Memory determines the base color and intensity of the Aura field cast by its spatial Content manifestation. |
| **Layer** | Memories are placed across Layers via Tracing. A Memory placed on Layer 1 can be traced to Layer 2 at identical or distinct grid coordinates with unique layer context. |
| **Annotation** | Annotations aggregate Memories from overlapping Aura fields across all Layers, organizing them into broadsheet media templates for overview reading. |
| **Blip** | A Blip signals an active cluster of Memories whose combined Aura fields reach a local threshold on the Information Layer. |
| **Slate** | Slates (e.g., Memory Slate, Writing Slate) provide direct, non-spatial viewing and editing interfaces for raw Memory payloads and their trace lineage. |

---

## 4. User Interaction & Camera Dynamics

- **Cursor Armed States**:
  - `Trace` Mode (`1` key cycle): Armed cursor allows user to pick up a Memory and project it onto adjacent Layers without breaking its spatial alignment or copying disk data.
  - `Copy` / `Duplicate` Modes: Generate a distinct, disconnected new Memory record (unlike `Trace`).
- **Camera Zoom / Level-of-Detail (LOD) Behavior**:
  - At close camera distance, Memories render legible Content text and image thumbnails.
  - At macro zoom, individual Memories collapse into Aura field points and Blips, preserving visual clarity across large knowledge bases.
- **Navigation Mechanics**:
  - Clicking a Memory anchor or trace link in an Annotation or Slate immediately shifts the camera plane to that Memory's exact spatial coordinates and native Layer.

---

## 5. Derived Outcomes Mapping

- **[Multi Contextual Representation Without Duplication](../derived-outcomes/Multi%20Contextual%20Representation%20Without%20Duplication.md)**: Users apply the same core idea to multiple project phases or perspectives without creating redundant, desynchronized file copies.
- **[Preserve Context Through Anchoring](../derived-outcomes/Preserve%20Context%20Through%20Anchoring.md)**: Users label and anchor memories based on spatial surroundings, preserving true cognitive context rather than forcing folder hierarchies.
- **[Recall Memories](../derived-outcomes/Recall%20Memories.md)**: Users quickly search, inspect, and navigate Memory lineages across the entire spatial workspace.
