---
type: product-definition
status: canonical
version: v9
date: 2026-08-11
tags: [grove, product-definition, layer, spatial-frequency-band, z-stack]
---

# Product Definition: Layer

> **What is a layer?**
> A **Layer** is a spatial frequency band representing one 2D Grid plane in a vertically stacked multi-plane continuum. It allows users to separate perspectives, workflows, and conceptual dimensions while maintaining unified spatial coordinates and cross-layer Aura saturation.

---

## 1. Core Essence ("What is a layer?")

- **Canonical Statement**: A Layer is a distinct Z-plane instance of the 2D Grid. It extends the 2D spatial model into a multi-dimensional workspace where Content can exist on independent planes while sharing identical cell coordinate spaces `(x, y)`.
- **Primary Function**: Layers provide perspective separation (e.g., separating raw research, structural composition, narrative text, and agent outputs into parallel planes). They allow Tracing of Memories across planes and facilitate vertical Aura bleed.
- **Mental / Physical Model**: 
  1. *Stacked Transparent Acetate Sheets*: Like clear drafting overlays stacked on top of a base map, each Layer holds independent markings while allowing light (Aura) to pass through the stack.
  2. *Spatial Frequency Bands*: Each Layer operates as a distinct frequency channel of the same spatial continuum, letting users isolate or view combined cross-plane work.

---

## 2. Fundamental Invariants & System Properties

1. **Strict 2D Alignment**: Every Layer shares the exact same 2D cell coordinate system `(x, y)`. Cell `(10, 20)` on Layer 1 aligns perfectly with cell `(10, 20)` on Layer 2.
2. **Vertical Aura Permeability**: Aura fields penetrate vertically through stacked Layers. An Aura emitted on Layer 2 saturates Layer 1 and Layer 3, blending composite cell hues.
3. **Trace Target Continuum**: Tracing Content across Layers locks its `(x, y)` cell coordinates during plane navigation. Collision detection triggers on drop-off.
4. **Independent Content Existence**: Content placed natively on Layer A does not block spatial interaction on Layer B unless explicitly committed via Tracing or Copying.
5. **Deterministic Plane Navigation**: Layer movement relies on dedicated keyboard bindings (`[` down, `]` up, `Shift+[` create down, `Shift+]` create up) for rapid, frictionless plane shifting.

### Data & State Schema
- **State Ownership**: Layer Management Subsystem (`LayerManager`).
- **Layer Attributes**:
  - `layer_id`: Zero-indexed integer (`0, 1, 2...`).
  - `name`: Custom human-readable label (e.g., "Composition", "Reference Materials").
  - `visibility`: Boolean active state.
  - `opacity`: Spatial opacity value for overlay rendering.
  - `content_ids`: List of native Content instances on this Layer.
- **Persistence Boundary**: Layer metadata stored in workspace manifest (`.grove/layers.json`).

---

## 3. Intersectionality Matrix

| Primitive | Intersection & Relational Rules |
| :--- | :--- |
| **Grid** | Layers stack identical 2D Grids along the Z-axis, creating a multi-plane 3D simulation out of purely 2D Grid geometry. |
| **Memory** | A single Memory can be Traced across multiple Layers, producing layer-specific contextual variants (`Trace of`) without duplicating data. |
| **Content** | Content resides on a single native Layer. Tracing projects Content onto another Layer as a variant of the same underlying Memory. |
| **Aura** | Aura radiates across Layer boundaries like liquid saturating stacked paper towels, combining hues from adjacent Layers into composite cell colors. |
| **Layer** | *Self-Intersection*: Layers stack sequentially (`Z-index`). Upper Layers can obscure lower Layers visually while maintaining field permeability. |
| **Annotation** | When triggered over a saturated Aura field, Annotations query Content across **all** stacked Layers overlapping that field, listing native layer badges in footers. |
| **Blip** | A Blip aggregates field density across all Layers at a specific `(x, y)` coordinate to signal multi-layer Content clusters. |
| **Slate** | Slate can display layer lineage metadata, allowing users to inspect which Layer a piece of Content or Memory trace originates from. |

---

## 4. User Interaction & Camera Dynamics

- **Keyboard Navigation Controls**:
  - `[` : Move active view down one Layer.
  - `]` : Move active view up one Layer.
  - `Shift+[` : Create new Layer below and shift focus down.
  - `Shift+]` : Create new Layer above and shift focus up.
  - `Ctrl/Cmd + Insert` : Insert Layer at boundary.
- **Cursor Armed States (`1` key cycle)**:
  - In `Trace` mode, holding Content while navigating Layers via bracket keys allows instant drop-off onto target Layer planes.
- **Camera Dynamics**:
  - Shifting Layers smoothly transitions the visual camera focus to the target plane while maintaining spatial viewport center `(x, y)`.

---

## 5. Derived Outcomes Mapping

- **[Multi Contextual Representation Without Duplication](../derived-outcomes/Multi%20Contextual%20Representation%20Without%20Duplication.md)**: Users examine a single idea across different conceptual Layers via Tracing.
- **[Passive Awareness Of Surrounding Work](../derived-outcomes/Passive%20Awareness%20Of%20Surrounding%20Work.md)**: Aura bleed across Layers alerts users to active work on parallel planes.
- **[Operate Layers](../derived-outcomes/Multi%20Contextual%20Representation%20Without%20Duplication.md)**: Direct keyboard controls enable fluid multi-plane context switching without mouse menu hunting.
