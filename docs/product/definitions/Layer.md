---
type: product-definition
status: derived
authority: derived-from-domain-model
source_of_truth: ../../domain/Grid-Layer.md
version: v9
date: 2026-08-11
tags: [grove, product-definition, grid-layer, spatial-frequency-band, z-stack]
---

# Product Definition: Grid Layer

> This is the derived product explanation for the literal Grid Layer concept.
> Plane and Tier are separate concepts; neither may be called a Layer.

> **What is a Grid Layer?**
> A **Grid Layer** is a literal depth member of the Grid. It shares the Grid's horizontal coordinates with other Grid Layers while keeping Content Placement state independent.

---

## 1. Core Essence ("What is a Grid Layer?")

- **Canonical Statement**: A Grid Layer is a distinct depth member of the 2D Grid. Content can exist independently on multiple Grid Layers while sharing identical cell coordinate spaces `(x, y)`.
- **Primary Function**: Grid Layers provide perspective separation (e.g., separating raw research, structural composition, narrative text, and agent outputs into parallel spatial contexts). They allow Content tracing across Grid Layers and facilitate vertical Aura permeability.
- **Mental / Physical Model**: 
  1. *Stacked Transparent Acetate Sheets*: Like clear drafting overlays stacked on top of a base map, each Grid Layer holds independent Content while allowing Aura to pass through the stack.
  2. *Spatial Frequency Bands*: Each Grid Layer operates as a distinct depth channel of the same spatial continuum, letting users isolate or view combined cross-Grid-Layer work.

---

## 2. Fundamental Invariants & System Properties

1. **Strict 2D Alignment**: Every Grid Layer shares the exact same 2D cell coordinate system `(x, y)`. Cell `(10, 20)` on Grid Layer 01 aligns perfectly with cell `(10, 20)` on Grid Layer 02.
2. **Vertical Aura Permeability**: Aura fields penetrate vertically through stacked Grid Layers. An Aura emitted on one Grid Layer may saturate corresponding cells on other Grid Layers according to vertical decay.
3. **Trace Target Continuum**: Tracing Content across Grid Layers preserves its `(x, y)` cell coordinates during depth navigation. Collision detection triggers on drop-off.
4. **Independent Content Existence**: Content placed natively on one Grid Layer does not block spatial interaction on another Grid Layer unless explicitly committed via tracing or copying.
5. **Deterministic Grid-Layer Navigation**: Grid-Layer movement relies on dedicated keyboard bindings (`[` down, `]` up, `Shift+[` create down, `Shift+]` create up) for rapid, frictionless depth shifting.

### Data & State Schema
- **State Ownership**: Grid-Layer Management Subsystem (`GridLayerManager`).
- **Grid-Layer Attributes**:
  - `layer_id`: Zero-indexed integer (`0, 1, 2...`).
  - `name`: Custom human-readable label (e.g., "Composition", "Reference Materials").
  - `visibility`: Explicit rendering preference; it is not an activation state.
  - `opacity`: Spatial opacity value for overlay rendering.
  - `content_ids`: List of native Content instances on this Grid Layer.
- **Persistence Boundary**: Grid-Layer metadata stored in the Grove manifest (`.grove/grid-layers.json`).

---

## 3. Intersectionality Matrix

| Primitive | Intersection & Relational Rules |
| :--- | :--- |
| **Grid** | Grid Layers stack identical 2D coordinate fields along the depth axis. |
| **Memory** | A single Memory can be referenced by Content on multiple Grid Layers without duplicating the Memory record. |
| **Content** | Each Content instance has Placement on one Grid Layer and may have one authored Anchor label. Multiple Content instances may reference one Memory. |
| **Aura** | Aura radiates across Grid-Layer boundaries like liquid saturating stacked paper towels, summing source hues according to the field contract. |
| **Grid Layer** | *Self-Intersection*: Grid Layers stack sequentially by depth. One Grid Layer may obscure another visually while field permeability remains a separate rule. |
| **Annotation** | When triggered over a saturated Aura field, Annotations query Content across **all** Grid Layers overlapping that field, listing native Grid-Layer identity where needed. |
| **Blip** | A Blip aggregates field density across all Grid Layers at a specific `(x, y)` coordinate to signal multi-Grid-Layer Content clusters. |
| **Slates** | Writing Slate, Memory Slate, and Gallery Slate can display Grid Layer lineage metadata, allowing users to inspect which Grid Layer a piece of Content or Memory trace originates from. |

---

## 4. User Interaction & Camera Dynamics

- **Keyboard Navigation Controls**:
  - `[` : Select the Grid Layer below as the command target.
  - `]` : Select the Grid Layer above as the command target.
  - `Shift+[` : Create and select a new Grid Layer below.
  - `Shift+]` : Create and select a new Grid Layer above.
  - `Ctrl/Cmd + Insert` : Insert a Grid Layer at a boundary.
- **Cursor Armed States (`1` key cycle)**:
  - In `Trace` mode, holding Content while navigating Grid Layers via bracket keys allows instant drop-off onto a target Grid Layer.
- **Camera Dynamics**:
  - Shifting Grid Layers smoothly transitions the visual camera focus to the target Grid Layer while maintaining spatial viewport center `(x, y)`.

---

## 5. Derived Outcomes Mapping

- **[Multi Contextual Representation Without Duplication](../derived-outcomes/Multi%20Contextual%20Representation%20Without%20Duplication.md)**: Users examine a single idea across different conceptual Layers via Tracing.
- **[Passive Awareness Of Surrounding Work](../derived-outcomes/Passive%20Awareness%20Of%20Surrounding%20Work.md)**: Aura bleed across Layers alerts users to active work on parallel planes.
- **[Operate Layers](../derived-outcomes/Multi%20Contextual%20Representation%20Without%20Duplication.md)**: Direct keyboard controls enable fluid multi-plane context switching without mouse menu hunting.
