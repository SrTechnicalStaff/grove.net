---
type: product-definition
status: canonical
version: v9
date: 2026-08-11
tags: [grove, product-definition, annotation, broadsheet-layout, media-templates]
---

# Product Definition: Annotation

> **What is an annotation?**
> An **Annotation** is a source-anchored, broadsheet-inspired editorial overview generated on the Information Layer. It synthesizes multi-layer Content from saturated Aura fields into structured print media layouts (brochures, pamphlets, magazines) without truncating text or mutating spatial Grid arrangements.

---

## 1. Core Essence ("What is an annotation?")

- **Canonical Statement**: An Annotation is a curated, non-destructive editorial overlay. It queries cell Field Ledger metadata within a dense Aura cluster across all stacked Layers, presenting an organized overview in a multi-page modal anchored to the Grid by an escalator line.
- **Primary Function**: Annotations provide macro-level comprehension without requiring users to manually zoom in or inspect individual files. They bridge distant spatial viewing with readable content aggregation.
- **Mental / Physical Model**: 
  1. *Print Broadsheet / Editorial Layouts*: Operates as a cross between a pamphlet, notebook, and magazine. Content is laid out using real-world print publishing ratios (text columns, full-bleed media, footer page/layer notes).
  2. *Architectural Callout Line*: Connects to the grid via an escalator line (staircase guide line) pointing directly to the center of highest Aura concentration.

---

## 2. Fundamental Invariants & System Properties

1. **Multi-Layer Field Querying**: When an Annotation is generated, it automatically queries all Content across **all** stacked Layers that overlap the target saturated Aura field.
2. **Dynamic Media Template Selection**: Layout templates are selected dynamically based on content volume and the ratio of visual media to rich text (e.g., 60–70% media vs. 30–40% text):
   - *Brochure*: Small content volume, balanced text/image ratio.
   - *Pamphlet*: Medium content volume, multi-column text lead.
   - *Magazine*: Large content volume, multi-page editorial chapters.
3. **Zero Truncation / Zero Overflow**: Annotation templates never truncate text (`ellipsis`) or clip containers. If content exceeds a template section, it reflows into the next size up, generating additional pages as needed.
4. **Deterministic Titling Hierarchy**: Annotation titles are derived automatically using a strict priority chain:
   1. Explicit Anchor label (if present).
   2. Native title of the closest Content on the active Layer.
   3. First 280 characters of text (if text content).
   4. Image thumbnail with file title (if media content).
5. **Layer Attribution Footers**: Every item within an Annotation includes a footer note indicating its native Layer of origin, preserving provenance.
6. **Information Layer Locality**: Pinned Annotations reside on the Information Layer—spatially anchored to Grid coordinates so they move naturally with camera panning, rather than sticking to the viewport screen.

### Data & State Schema
- **State Ownership**: Information Layer Subsystem (`InformationLayerManager`).
- **Annotation Instance Attributes**:
  - `annotation_id`: Unique identifier.
  - `cluster_id`: Target Aura cluster ID.
  - `escalator_anchor`: `{ grid_x, grid_y, layer_id }` (highest aura concentration center).
  - `template_type`: Enum (`brochure`, `pamphlet`, `magazine`).
  - `media_text_ratio`: Calculated split float.
  - `pages`: Array of `{ page_num, section_layouts, content_references }`.
  - `is_pinned`: Boolean state on Information Layer.
- **Persistence Boundary**: Ephemeral runtime overlay derived from cell metadata; pinned state saved in workspace session state.

---

## 3. Intersectionality Matrix

| Primitive | Intersection & Relational Rules |
| :--- | :--- |
| **Grid** | Annotations hover over the Grid on the Information Layer. An escalator line links the Annotation window to the exact center of highest Aura overlap on the Grid. |
| **Memory** | Annotations display Memories in context; users can highlight text inside an Annotation to spawn new Memories or add contextual notes. |
| **Content** | Annotations curate Content from multiple Layers into print templates. Interacting with a Content item inside an Annotation provides a CTA to jump directly to that Content's native Grid location. |
| **Aura** | Annotations are triggered by saturated Aura fields. Field density and cluster boundaries dictate whether a field-wide or local-cluster Annotation is generated. |
| **Layer** | Annotations aggregate Content across **all** stacked Layers, placing native Layer attribution badges in page footers. |
| **Annotation** | *Self-Intersection*: Only one active unpinned Annotation exists at a time; pinning allows an Annotation to remain open on the Information Layer while navigating. |
| **Blip** | A Blip is the unengaged state of an Annotation; hovering near a Blip expands it into an active Annotation overlay. |
| **Slate** | An Annotation can be sent to Slate for full-screen, focused reading and editing without spatial camera constraints. |

---

## 4. User Interaction & Camera Dynamics

- **Activation & Engagement**:
  - Hovering the cursor over an active Blip or occupied Aura cluster animates the escalator line and title badge.
  - Selecting the Blip expands it into the broadsheet Annotation modal.
- **Navigation & Source Route**:
  - Hovering over individual Content items inside the Annotation reveals a small CTA button; clicking it closes the Annotation and pans the camera directly to that Content on its native Layer.
- **Pinning Mechanics**:
  - Clicking the Pin CTA attaches the Annotation modal to the Information Layer at its local Grid coordinates, allowing the user to pan the camera away while keeping the reference open in 2D space.

---

## 5. Derived Outcomes Mapping

- **[Effortless Synthesis Of Clustered Information](../derived-outcomes/Effortless%20Synthesis%20Of%20Clustered%20Information.md)**: Users read structured editorial overviews of multi-layer work without manually opening dozens of files.
- **[Follow Annotation Source Routes](../derived-outcomes/Effortless%20Synthesis%20Of%20Clustered%20Information.md)**: Users navigate seamlessly from aggregated summaries back to original spatial Content locations.
- **[Read Broadsheet Annotations](../derived-outcomes/Effortless%20Synthesis%20Of%20Clustered%20Information.md)**: Users enjoy beautiful, un-truncated print media layouts tailored to content volume.
