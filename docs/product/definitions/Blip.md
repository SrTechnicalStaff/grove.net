---
type: product-definition
status: derived
authority: derived-from-domain-model
source_of_truth: ../../domain/Aura.md
version: v9
date: 2026-08-11
tags: [grove, product-definition, blip, spatial-indicator, field-notification]
---

# Product Definition: Blip

> **What is a blip?**
> A **Blip** is the passive, unengaged visual indicator of an available Annotation on the Grid. It manifests as a compact indicator at the center of dense Aura field concentrations, signaling synthesized multi-Grid-Layer knowledge without cluttering the spatial canvas.

---

## 1. Core Essence ("What is a blip?")

- **Canonical Statement**: A Blip is a dormant Annotation trigger. It marks the focal point of a high-density Aura cluster on the Information Plane, maintaining visual quietness until cursor proximity escalates it into an active Annotation overlay.
- **Primary Function**: Blips prevent visual noise across large spatial fields. They compress complex multi-Grid-Layer Content clusters into minimal ambient signals, letting users scan distant spatial regions without opening a surface prematurely.
- **Mental / Physical Model**: 
  1. *Gacha / Status Red Dot Notifications*: Like subtle red badge indicators in software UI, a Blip informs the user that unread or aggregated context is ready for engagement at a specific location.
  2. *Navigational Buoy / Beacon*: Acts as a spatial beacon anchored to gravitational centers in the spatial field.

---

## 2. Fundamental Invariants & System Properties

1. **Passive Existence**: A Blip exists in a passive state by default. It requires zero user interaction to maintain its visual beacon on the Information Plane.
2. **Cluster Center Positioning**: A Blip is positioned deterministically at the geometric center of highest Aura field overlap and smallest inter-content distance within a detected cluster.
3. **Proximity Engagement**: Direct cursor hover over the Blip or over any cell occupied by its parent Content cluster triggers engagement, animating an escalator line and title preview.
4. **Singular Unpinned Active State**: Only one Blip can escalate into an active, unpinned Annotation overlay at any given time.
5. **Noise Reduction Threshold**: To prevent visual overcrowding, local Blip generation enforces a minimum spatial separation distance between neighboring Aura clusters within the same galaxy-level field.

### Data & State Schema
- **State Ownership**: Information Plane subsystem (`BlipManager`).
- **Blip Instance Attributes**:
  - `blip_id`: Unique identifier.
  - `target_cluster_id`: Associated Aura cluster.
  - `coordinate`: `{ grid_x, grid_y, primary_layer_id }`.
  - `pulse_frequency`: Animation render rate.
  - `engagement_state`: Enum (`dormant`, `hovered`, `expanded_annotation`).
- **Persistence Boundary**: Derived runtime state calculated from spatial Field Ledger density.

---

## 3. Intersectionality Matrix

| Primitive | Intersection & Relational Rules |
| :--- | :--- |
| **Grid** | Blips anchor to precise Grid cell coordinates, rendering as floating indicators above the 2D plane on the Information Plane. |
| **Memory** | Blips represent clusters of underlying Memories; their presence indicates rich semantic overlap across spatial coordinates. |
| **Content** | Hovering over cells occupied by Content activates the associated Blip, drawing an escalator line to the cluster center. |
| **Aura** | Blips are derived at points of peak Aura field overlap and intensity within the Field Ledger across Grid Layers. |
| **Grid Layer** | Blips aggregate Aura energy vertically across **all** stacked Grid Layers, signaling activity even on inactive depth planes. |
| **Annotation** | A Blip is an unengaged Annotation. Clicking or hovering over a Blip expands it into a full broadsheet Annotation modal. |
| **Blip** | *Self-Intersection*: Neighboring Blips maintain minimum distance bounds; dense clusters merge into single Blips at macro zoom distances. |
| **Slates** | Opening a Blip's Annotation in Memory Slate or Writing Slate dismisses the spatial Blip overlay in favor of a fixed-viewport reading window. |

---

## 4. User Interaction & Camera Dynamics

- **Cursor Interaction**:
  - *Dormant State*: Renders as a small pulsating dot inside the Aura field.
  - *Proximity Hover*: Moving the cursor near the Blip or its cluster content animates an escalator guide line and displays a title badge (anchor label, title, or 280-char text preview).
  - *Selection*: Clicking the Blip expands it into an Annotation modal.
- **Camera Zoom / LOD Behavior**:
  - At close zoom, Blips remain minimal to prioritize direct Content legibility.
  - At distance / macro zoom, Blips become the primary visual indicators for navigating large clusters across the spatial field.

---

## 5. Derived Outcomes Mapping

- **[Uncluttered Visual Scanning At Distance](../derived-outcomes/Uncluttered%20Visual%20Scanning%20At%20Distance.md)**: Users scan macro spatial maps without being bombarded by open popups or illegible text cards.
- **[Notice Annotation Markers](../derived-outcomes/Uncluttered%20Visual%20Scanning%20At%20Distance.md)**: Users spot pulsating ambient signals indicating rich context clusters across the spatial field.
