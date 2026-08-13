---
type: product-definition
status: derived
authority: derived-from-domain-model
source_of_truth: ../../domain/Aura.md
version: v9
date: 2026-08-11
tags: [grove, product-definition, aura, field-ledger, saturation]
---

# Product Definition: Aura

> **What is an aura?**
> An **Aura** is the continuous spatial radiation field cast by Content onto surrounding Grid cells and across vertical Layer boundaries. It serves as the physical and visual manifestation of semantic energy, establishing spatial influence, passive association, and multi-layer metadata accumulation without requiring formal links or explicit hierarchy.

---

## 1. Core Essence ("What is an aura?")

- **Canonical Statement**: An Aura is a non-local spatial field emitted by Content on the Grid that saturates local cells and bleeds through vertical Layer bounds. It stores provenance and relational energy inside the Grid's cell metadata ledger.
- **Primary Function**: Aura provides passive context propagation, visual weight, field-based selection feedback, and multi-layer awareness. It enables the system to discover clusters of related work and generate Annotation overlays without explicit user classification.
- **Mental / Physical Model**: 
  1. *Astrophysics / Gravity in Spacetime*: Content behaves like mass in spacetime; Aura is the gravitational field cast onto surrounding space. Dense clusters of content form galaxy-like field concentrations.
  2. *Fluid Saturation on Stacked Paper Towels*: When liquid is poured on a stack of paper towels, it bleeds through multiple sheets. Similarly, an Aura field originating on one Layer saturates adjacent Layers above and below.

---

## 2. Fundamental Invariants & System Properties

1. **Innate Non-Zero Baseline**: Every cell on the Grid possesses a non-zero baseline information value simply by existing. Content emission adds field energy atop this baseline.
2. **Multi-Layer Saturation & Bleed**: An Aura field is never restricted to its native Layer; it radiates vertically through adjacent stacked Layers, saturating corresponding `(x, y)` coordinates across the Z-axis.
3. **Additive Hue Composition**: The visual color/hue of an Aura cell is the deterministic composite sum of its native Content hue plus all saturating field hues bleeding from Layers above and below.
4. **Field Ledger Provenance**: Every cell affected by an Aura field records metadata identifying every originating Content source contributing to that cell's brightness and saturation across all Layers.
5. **Selection Field Integrity**: When multiple Content items are selected, their individual Auras aggregate into a unified selection field. Cursor accents and marquee highlights overlay the field without destroying the passive underlying system state (mirroring spreadsheet cell selection dynamics).

### Data & State Schema
- **State Ownership**: Grid Field Ledger Subsystem (`GridFieldLedger`).
- **Cell Metadata Attributes**:
  - `cell_id`: Unique spatial coordinate `(x, y)`.
  - `baseline_energy`: Non-zero default float value.
  - `field_intensity`: Total combined brightness/energy score.
  - `composite_hue`: Combined RGB/HSL color space vector.
  - `sources`: Array of `{ content_id, layer_id, contribution_ratio, metadata }`.
- **Persistence Boundary**: Derived at runtime from active spatial Content locations; field ledger indexes are updated lazily or on spatial mutations.

---

## 3. Intersectionality Matrix

| Primitive | Intersection & Relational Rules |
| :--- | :--- |
| **Grid** | Aura saturates Grid cells, populating cell metadata in the Field Ledger and establishing innate spatial relationships between adjacent grid coordinates. |
| **Memory** | Aura reflects the semantic weight and contextual energy of underlying Memories; as Memories gain traces across layers, their composite Aura footprint expands. |
| **Content** | Content is the physical emitter of Aura. Every placed Content item continuously casts an Aura field proportional to its media ratio, scale, and text volume. |
| **Aura** | *Self-Intersection*: Neighboring Auras overlap and merge into continuous field concentrations (galaxies), altering local hue saturation and triggering cluster detection algorithms. |
| **Layer** | Aura bleeds vertically across stacked Layers. Saturation penetrates Z-planes like liquid through paper towels, allowing users on one Layer to perceive work on adjacent Layers. |
| **Annotation** | Annotations query cell Field Ledger metadata within dense Aura concentrations across all stacked Layers to curate source content for broadsheet editorial overlays. |
| **Blip** | A Blip manifests as a small pulsating indicator dot at the geometric center of highest Aura overlap within a dense Content cluster when viewed at scale or when unengaged. |
| **Slates** | Writing Slate, Memory Slate, and Gallery Slate bypass Aura rendering; inspecting an Annotation or Memory there displays Field Ledger metadata derived from the originating Aura field. |

---

## 4. User Interaction & Camera Dynamics

- **Cursor Armed States**:
  - When the cursor is armed (`1` key cycle: Trace, Resize, Copy, Cut, Duplicate), hovering over Content highlights its native Aura.
  - Bulk marquee selection requires enclosing all cells occupied by the Content; once selected, individual Auras aggregate into a combined selection field.
- **Camera Zoom / Level-of-Detail (LOD) Behavior**:
  - *Close Distance*: Content is fully legible; native Aura fields display subtle background glows around Content boundaries.
  - *Extreme Distance*: Content legibility drops by design; Aura fields transition to LOD map representations (similar to Google Earth macro views). Dense clusters merge into glowing galactic field concentrations.
- **Navigation & Hover Mechanics**:
  - Bringing the cursor close to an active Aura concentration or hovering over a cell within the field engages the local Blip, triggering the escalator guide line and annotation title preview.

---

## 5. Derived Outcomes Mapping

- **[Passive Awareness Of Surrounding Work](../derived-outcomes/Passive%20Awareness%20Of%20Surrounding%20Work.md)**: Users perceive activity and related content across multiple layers via vertical Aura saturation without needing to manually toggle planes.
- **[Spatial Association Without Forcing Structure](../derived-outcomes/Spatial%20Association%20Without%20Forcing%20Structure.md)**: Users establish innate relationships between ideas simply by placing them in proximity, allowing Aura fields to merge into shared contexts.
- **[See The Whole Field](../derived-outcomes/Continuous%20Uninterrupted%20Focus.md)**: Macro zoom levels leverage Aura LOD rendering to give users an immediate bird's-eye view of spatial-field density and focus areas.
