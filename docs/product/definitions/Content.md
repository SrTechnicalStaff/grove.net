---
type: product-definition
status: derived
authority: derived-from-domain-model
source_of_truth: ../../domain/Content.md
version: v9
date: 2026-08-11
tags: [grove, product-definition, content, media-primitives, no-overflow]
---

# Product Definition: Content

> **What is content?**
> **Content** is a placed spatial instance of exactly one Memory on the Grid. It defines the physical footprint, visual form, and layout geometry of text and media objects across 2D spatial cell boundaries on a given Grid Layer.

---

## 1. Core Essence ("What is content?")

- **Canonical Statement**: Content is one placed instance of a Memory. It translates the Memory payload into a spatially bounded visual representation, owns its Placement, and may carry an optional Content-side Anchor. A Memory can exist without Content, while Content cannot exist without a Memory.
- **Primary Function**: Content gives form, legibility, and spatial presence to knowledge. It acts as the anchor point for user manipulation (moving, tracing, resizing) and serves as the physical energy source emitting Aura fields onto the Grid.
- **Mental / Physical Model**: 
  1. *Physical Documents & Sticky Notes*: Notes embody physical sticky notes placed on a working surface. Documents embody structured research papers (title, frontmatter, abstract-like layout) laid out across a 2D table.
  2. *Broadsheet Print Formatting*: Content layout follows print media principles where text reflows gracefully across columns and pages rather than suffering digital truncation or scrollbar overflow.

---

## 2. Fundamental Invariants & System Properties

1. **Memory-Content Binding**: Content cannot exist without an underlying Memory. Multiple Content instances may reference the same Memory; each instance has its own Placement and optional Content-side Anchor. Creating Content from an unbound source creates its Memory before the Content commit.
2. **Zero Truncation / Zero Overflow**: Under no circumstances does Content use text truncation (`text-overflow: ellipsis`) or element container overflow clipping. If content expands beyond template boundaries, it triggers layout scaling or generates additional contiguous spatial pages.
3. **Primitive Type Scope**: Initial primitive content types are strictly constrained to:
   - *Rich Text*: Notes (`.txt`), Markdown documents (`.md`).
   - *Media*: Standard image formats (`.png`, `.jpg`), animated GIFs (`.gif`), and Video (`.mp4`). (Complex long-form files like PDF and EPUB are explicitly out of initial scope).
4. **Representative Grid Presence**: On the spatial Grid, Documents present structured representative layouts (title, frontmatter, lead excerpt) optimized for quick spatial scanning rather than exhaustive wall-of-text reading. Deep reading and full text editing occur in Writing Slate.
5. **Strict Marquee Cell Coverage**: In marquee selection, Content is only selected if all cells occupied by its spatial bounding box are fully enclosed within the marquee selection area.

### Data & State Schema
- **State Ownership**: Grid Render Engine (`GridContentManager`).
- **Content Instance Attributes**:
  - `content_id`: Unique Content instance ID.
  - `memory_id`: FK to underlying Memory object.
  - `anchor_id`: Optional Content-side authored label/context binding.
  - `layer_id`: Native Grid Layer identifier.
  - `grid_bounds`: `{ start_cell_x, start_cell_y, cell_span_w, cell_span_h }`.
  - `media_type`: Enum (`rich_text`, `markdown_doc`, `image`, `gif`, `video`).
  - `media_text_ratio`: Calculated percentage split between visual media and text payload.
- **Persistence Boundary**: Document files on disk (e.g., Markdown files in the Grove content directory) updated synchronously on save actions.

---

## 3. Intersectionality Matrix

| Primitive | Intersection & Relational Rules |
| :--- | :--- |
| **Grid** | Content occupies discrete 2D Grid cells. Cell dimensions dictate spatial bounds; moving or resizing Content updates cell occupancy. |
| **Memory** | Content references Memory through `MemoryId`. Editing Content creates a new immutable Memory version for that Content; tracing creates additional Content referencing the selected Memory. |
| **Content** | *Self-Intersection*: Content objects enforce collision and proximity rules; placing Content near other Content forms dense spatial clusters and merges local Aura fields. |
| **Aura** | Content is the sole source generator of Aura. Content size, type, and media ratio determine the hue, intensity, and cell radius of its emitted Aura field. |
| **Grid Layer** | Content resides natively on one specific Grid Layer, but can be traced across multiple Grid Layers while preserving spatial cell coordinates. |
| **Annotation** | Annotations extract and organize Content from saturated Aura fields, reflowing Content into broadsheet print media templates (brochure, pamphlet, magazine). |
| **Blip** | Hovering over or interacting with cells occupied by Content activates the local Blip on the Information Plane, displaying escalator lines and title previews. |
| **Slate** | Double-clicking or selecting Content opens it in Slate (Writing Slate for text, Gallery Slate for media) for full-screen, un-truncated editing and inspection. |

---

## 4. User Interaction & Camera Dynamics

- **Cursor Armed States (`1` key cycle)**:
  - *Trace*: Projects Content to another Layer at target cell location.
  - *Resize*: Modifies cell span dimensions, dynamically reflowing text without truncation.
  - *Copy / Cut / Duplicate*: Perform cell-level spatial clipboard operations with Microsoft Excel-inspired cell accents and color highlights.
- **Camera Zoom / Level-of-Detail (LOD) Behavior**:
  - *Zoomed In*: Content renders legible text, full image fidelity, and interactive controls.
  - *Zoomed Out*: Content legibility drops naturally as camera retreats. Grid tiers and cell-bounded Aura evidence remain available without a glow or halo representation.
- **Creation Controls**:
  - Quick Note creates a Memory without requiring a Grid Placement.
  - A Grid placement action creates Content at target cursor coordinates and assigns its `MemoryId` to an existing Memory, or creates the Memory first when the source is unbound.

---

## 5. Derived Outcomes Mapping

- **[Spatial Association Without Forcing Structure](../derived-outcomes/Spatial%20Association%20Without%20Forcing%20Structure.md)**: Users position Content freely on the Grid to establish intuitive relationships without rigid folder structures.
- **[Continuous Uninterrupted Focus](../derived-outcomes/Continuous%20Uninterrupted%20Focus.md)**: Zero truncation and representative spatial layouts keep the spatial field readable, while Slate integration enables focused editing.
- **[Quick Note Capture](../derived-outcomes/Instant%20Tactical%20Working%20Surface.md)**: Users capture thoughts in a single action using physical sticky note behavior.
