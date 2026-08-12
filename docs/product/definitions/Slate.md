---
type: product-definition
status: canonical
version: v9
date: 2026-08-11
tags: [grove, product-definition, slate, viewport-overlay, writing-slate]
---

# Product Definition: Slate

> **What is a slate?**
> A **Slate** is a fixed-viewport tactical working surface and modal layer in Grove. It provides a focused, non-spatial interface (e.g., Writing Slate, Memory Slate, Gallery Slate) for direct reading, editing, browsing, and composition of Content, Memories, and Annotations without camera movement or spatial layout disruption.

---

## 1. Core Essence ("What is a slate?")

- **Canonical Statement**: A Slate is a screen-fixed viewport overlay. Unlike the Grid (2D spatial plane) or the Information Layer (grid-anchored overlays), a Slate sits fixed to the screen viewport, offering an immersive, distraction-free environment for deep inspection and rich editing.
- **Primary Function**: Slate decouples deep text/media work from spatial canvas mechanics. It allows users to write long-form Markdown, inspect complete Memory trace lineages, or review Gallery media without moving the spatial camera or altering Grid cell bounds.
- **Mental / Physical Model**: 
  1. *Physical Handheld Slate / Clipboard*: Like holding a physical clipboard or handheld writing slate in front of your eyes while standing in a room, Slate brings the work directly to the foreground without disturbing the surrounding room layout.
  2. *Focus Mode Viewport Overlay*: A clean, HUD-like editing interface pinned directly to the display viewport.

---

## 2. Fundamental Invariants & System Properties

1. **Fixed Viewport Locality**: Slate elements do not possess `(x, y)` Grid coordinates. They are positioned relative to the screen display frame (`viewport_x, viewport_y`).
2. **Non-Destructive Spatial Integrity**: Opening, editing within, or closing a Slate leaves the underlying Grid layout, cell bounds, and Layer structures completely untouched.
3. **Dedicated Slate Variants**: Slate adapts its interface based on target payload:
   - *Writing Slate*: Rich Markdown editing, document formatting, and full-text review without spatial overflow constraints.
   - *Memory Slate*: Inspection of Memory records, anchor histories, and cross-layer trace lineage graphs.
   - *Gallery Slate*: High-resolution media viewing for images, GIFs, and videos.
4. **Bi-Directional Persistence**: Edits made inside Writing Slate synchronously update the underlying Content item, Memory payload, and associated disk file.
5. **Instant Dismissal & Return**: Closing a Slate restores instant full focus to the exact spatial camera view and selected Layer occupied prior to opening.

### Data & State Schema
- **State Ownership**: Viewport Overlay Subsystem (`SlateManager`).
- **Slate Instance Attributes**:
  - `slate_id`: Unique session identifier.
  - `slate_type`: Enum (`writing_slate`, `memory_slate`, `gallery_slate`).
  - `source_reference`: `{ content_id, memory_id, annotation_id }`.
  - `is_active`: Boolean viewport visibility flag.
  - `editor_state`: Active document buffer or media playback state.
- **Persistence Boundary**: Buffer changes committed to target file on disk via workspace save handlers.

---

## 3. Intersectionality Matrix

| Primitive | Intersection & Relational Rules |
| :--- | :--- |
| **Grid** | Slate sits on the viewport overlay above the Grid; opening Slate suspends active Grid panning while preserving underlying Grid cell positions. |
| **Memory** | Slate provides raw payload editing for Memories. In Memory Slate, users examine trace lineage, anchor tags, and contextual variants across Layers. |
| **Content** | Double-clicking Content opens it in Slate (Writing Slate for Markdown/notes, Gallery Slate for media), enabling in-depth editing without truncation. |
| **Aura** | Slate displays Field Ledger metadata derived from the Content's Aura, but does not render continuous Aura field glows within the Slate frame itself. |
| **Layer** | Opening Content in Slate reveals its native Layer attribution badge and permits shifting target Layer drop-offs during composition. |
| **Annotation** | Annotations can be sent to Slate for full broadsheet reading and navigation without Information Layer spatial constraints. |
| **Blip** | Interacting with a Blip allows users to send its aggregated Annotation directly into Slate for uninterrupted reading. |
| **Slate** | *Self-Intersection*: Slate operates as a singular modal focus layer; opening a new Slate target cleanly transitions the active Slate viewport. |

---

## 4. User Interaction & Camera Dynamics

- **Activation & Invocation**:
  - Double-clicking any Content item on the Grid.
  - Selecting "Open in Slate" from an Annotation modal or context menu.
- **Viewport Mechanics**:
  - While Slate is active, camera panning across the Grid is paused, allowing wheel scrolling to navigate Slate document text smoothly.
- **Dismissal Controls**:
  - Pressing `Escape` or clicking the close CTA dismisses Slate immediately, returning keyboard focus to the spatial Grid camera.

---

## 5. Derived Outcomes Mapping

- **[Continuous Uninterrupted Focus](../derived-outcomes/Continuous%20Uninterrupted%20Focus.md)**: Users edit long-form documents in a clean, focused viewport overlay without moving their spatial camera position.
- **[Instant Tactical Working Surface](../derived-outcomes/Instant%20Tactical%20Working%20Surface.md)**: Users pull up an active document or reference item on demand, edit it, and dismiss it cleanly.
- **[Browse Memory Slate](../derived-outcomes/Instant%20Tactical%20Working%20Surface.md)**: Users inspect complete Memory histories and trace relationships in a dedicated interface.
