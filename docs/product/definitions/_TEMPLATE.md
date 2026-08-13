---
type: product-definition-template
status: canonical
version: v9
date: 2026-08-11
tags: [grove, product-definition, template, intersectionality]
---

# Product Definition Template: [Concept Name]

> [!NOTE]
> This template establishes the canonical structure for all formal Grove v9 Product Definitions. Every definition must adhere strictly to this schema, maintaining high architectural rigor, zero fluff, and explicit intersectionality across the core primitives (**Grid**, **Memory**, **Content**, **Aura**, **Layer**, **Annotation**, **Blip**, **Writing Slate**, **Memory Slate**, **Gallery Slate**).

---

## 1. Core Essence ("What is [Concept Name]?")
*Provide a single, definitive, high-rigor architectural statement that defines the primitive's core identity within Grove v9 without ambiguity.*

- **Canonical Statement**: [1-2 sentence core definition]
- **Primary Function**: [What role does this primitive play in the system architecture?]
- **Mental / Physical Model**: [The physical world analog, e.g., Spacetime continuum, Sticky note, Saturation on paper towels, Broadsheet print media.]

---

## 2. Fundamental Invariants & System Properties
*List the non-negotiable rules and state invariants that govern this primitive. What must always remain true?*

1. **[Invariant Name 1]**: [Detailed rule, e.g., Content never truncates or overflows on the spatial grid.]
2. **[Invariant Name 2]**: [Detailed rule]
3. **[Invariant Name 3]**: [Detailed rule]

### Data & State Schema
- **State Ownership**: [Which Layer/subsystem owns this entity? e.g., Grid Plane, Information Plane, HUD Plane]
- **Spatial Coordinates**: [How is position/scale expressed? e.g., `(x, y, layer_id, cell_span)`]
- **Persistence Boundary**: [Disk representation vs. runtime state]

---

## 3. Intersectionality Matrix
*Every core primitive in Grove exists in direct relationship with all other seven primitives. Define the exact formal rules governing how this primitive intersects with each.*

| Primitive | Intersection & Relational Rules |
| :--- | :--- |
| **Grid** | *How this primitive maps to, interacts with, or alters the 2D cell continuum.* |
| **Memory** | *How this primitive relates to underlying semantic records, anchors, and traces.* |
| **Content** | *How this primitive manifests as or interacts with placed spatial media/text objects.* |
| **Aura** | *How this primitive influences or responds to field radiation, saturation, and hue composite blending.* |
| **Layer** | *How this primitive behaves across stacked spatial frequency bands / Z-planes.* |
| **Annotation** | *How this primitive relates to curated, source-anchored editorial overviews on the Information Plane.* |
| **Blip** | *How this primitive interacts with passive/dormant indicator blimps at distance or in unengaged states.* |
| **Slate** | *How this primitive transitions to or interfaces with fixed-viewport tactical reading/writing slates.* |

---

## 4. User Interaction & Camera Dynamics
*Describe how the user interacts with this primitive across camera distances, input modes, cursor armed states, and keybindings.*

- **Cursor Armed States**: [Interaction with `1` key armed states: Trace, Resize, Copy, Cut, Duplicate]
- **Camera Zoom / LOD Behavior**: [Behavior when zoomed in (legibility) vs. zoomed out (distance LOD, blips, field aggregation)]
- **Keyboard & Navigation Mechanics**: [Direct navigation controls, e.g., bracket keys `[` / `]`, `Shift+[`, `Ctrl/Cmd+Insert`]

---

## 5. Derived Outcomes Mapping
*Map this primitive directly to the human-behavior changes it enables.*

- **[Derived Outcome Title 1]**: [Brief statement of how this primitive fulfills the outcome]
- **[Derived Outcome Title 2]**: [Brief statement]
