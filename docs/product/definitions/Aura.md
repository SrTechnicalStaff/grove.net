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
> An **Aura** is the spatial field cast by Content onto discrete Grid cells and across vertical Grid-Layer boundaries. It is the physical and visual manifestation of Content presence, establishing spatial influence, passive association, and multi-Grid-Layer metadata accumulation without requiring formal links or explicit hierarchy.

---

## 1. Core Essence ("What is an aura?")

- **Canonical Statement**: An Aura is a non-local, cell-bounded field emitted by Content on the Grid that saturates local cells and passes through vertical Grid-Layer bounds. It stores provenance and relational energy inside the Grid's Field Ledger.
- **Primary Function**: Aura provides passive context propagation, visual weight, field-based selection feedback, and multi-layer awareness. It enables the system to discover clusters of related work and generate Annotation overlays without explicit user classification.
- **Mental / Physical Model**: 
  1. *Astrophysics / Gravity in Spacetime*: Content behaves like mass in spacetime; Aura is the gravitational field cast onto surrounding space. Dense clusters of content form galaxy-like field concentrations.
2. *Saturation on Stacked Paper Towels*: When liquid is poured on a stack of paper towels, it passes through multiple sheets. Similarly, an Aura field originating on one Grid Layer saturates adjacent Grid Layers above and below.

---

## 2. Fundamental Invariants & System Properties

1. **Innate Non-Zero Baseline**: Every cell on the Grid possesses a non-zero baseline information value simply by existing. Content emission adds field energy atop this baseline.
2. **Multi-Grid-Layer Saturation**: An Aura field is never restricted to its native Grid Layer; it contributes vertically through adjacent stacked Grid Layers, saturating corresponding `(x, y)` coordinates according to permeability.
3. **Additive Hue Composition**: The visual color/hue of an Aura cell is the deterministic additive sum of source hues weighted by field contribution. No midpoint or blended third hue is calculated.
4. **Field Ledger Provenance**: Every cell affected by an Aura field records metadata identifying every originating Content source contributing to that cell's brightness and saturation across all Layers.
5. **Selection Field Integrity**: When multiple Content items are selected, their individual source contributions are highlighted across their Aura cells. Unselected contributions remain in the passive field.

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
| **Memory** | Memory supplies semantic identity and payload only. Content instances referencing a Memory emit Aura; additional Content instances expand the spatial evidence without changing the Memory. |
| **Content** | Content is the physical emitter of Aura. Every placed Content item continuously casts an Aura field proportional to its media ratio, scale, and text volume. |
| **Aura** | *Self-Intersection*: Neighboring Auras contribute to the same discrete cells; their channel sums and saturation provide shared-context evidence without a smooth surface. |
| **Grid Layer** | Aura passes vertically across stacked Grid Layers. Saturation penetrates the stack, allowing users on one Grid Layer to perceive work on adjacent Grid Layers. |
| **Annotation** | Annotations query cell Field Ledger metadata within dense Aura concentrations across all stacked Layers to curate source content for broadsheet editorial overlays. |
| **Blip** | A Blip manifests as a small pulsating indicator dot at the geometric center of highest Aura overlap within a dense Content cluster when viewed at scale or when unengaged. |
| **Slates** | Writing Slate, Memory Slate, and Gallery Slate bypass Aura rendering; inspecting an Annotation or Memory there displays Field Ledger metadata derived from the originating Aura field. |

---

## 4. User Interaction & Camera Dynamics

- **Cursor Armed States**:
  - When the cursor is armed, hovering over Content addresses its native footprint and Aura contribution.
  - Bulk marquee selection requires enclosing every cell occupied by the Content; once selected, the selected source contribution is highlighted across its combined Aura field.
- **Camera Zoom / Level-of-Detail (LOD) Behavior**:
  - *Close Distance*: Content is fully legible; native Aura fields display discrete cell marks and same-Grid-Layer contours.
  - *Extreme Distance*: Content legibility drops by design; the single recursive Grid cursor and grid tiers remain readable while Aura evidence stays cell-bounded.
- **Navigation & Hover Mechanics**:
  - Bringing the cursor close to an active Aura concentration or hovering over a cell within the field engages the local Blip, triggering the escalator guide line and annotation title preview.

---

## 5. Derived Outcomes Mapping

- **[Passive Awareness Of Surrounding Work](../derived-outcomes/Passive%20Awareness%20Of%20Surrounding%20Work.md)**: Users perceive activity and related content across multiple layers via vertical Aura saturation without needing to manually toggle planes.
- **[Spatial Association Without Forcing Structure](../derived-outcomes/Spatial%20Association%20Without%20Forcing%20Structure.md)**: Users establish innate relationships between ideas simply by placing them in proximity, allowing Aura fields to merge into shared contexts.
- **[See The Whole Field](../derived-outcomes/Continuous%20Uninterrupted%20Focus.md)**: Macro zoom levels leverage Aura LOD rendering to give users an immediate bird's-eye view of spatial-field density and focus areas.
