---
type: product-definition
status: derived
authority: derived-from-domain-model
source_of_truth: ../../domain/Slate.md
version: v9
date: 2026-08-11
tags: [grove, product-definition, slate, viewport-overlay, writing-slate]
---

# Product Definition: Slates

> **What is a slate?**
> Grove has three fixed-viewport Slates: Writing Slate, Memory Slate, and Gallery Slate. Each provides a focused, non-spatial interface for direct reading, editing, or browsing without camera movement or spatial layout disruption. Quick Note and Layer Manager are overlays, not Slates.

---

## 1. Core Essence ("What is a slate?")

- **Canonical Statement**: Writing Slate, Memory Slate, and Gallery Slate are screen-fixed HUD Plane sub-applications. Each is composed as either the full viewport or one half of the viewport; none is a floating or hover surface.
- **Primary Function**: The three Slates decouple deep text/media work from spatial canvas mechanics. They allow users to write long-form Markdown, inspect Memory trace lineages, or review Gallery media without moving the spatial camera or altering Grid cell bounds.
- **Mental / Physical Model**: 
  1. *Physical Handheld Slate / Clipboard*: Like holding a physical clipboard or handheld writing slate in front of your eyes while standing in a room, a named Slate brings the work directly to the foreground without disturbing the surrounding spatial layout.
  2. *Focus Mode Viewport Surface*: A clean editing interface pinned directly to the display viewport.

---

## 2. Fundamental Invariants & System Properties

1. **Fixed Viewport Locality**: Slate elements do not possess `(x, y)` Grid coordinates. They are positioned relative to the screen display frame (`viewport_x, viewport_y`).
2. **Non-Destructive Spatial Integrity**: Opening, editing within, or closing a Slate leaves the underlying Grid layout, cell bounds, and Grid Layer structures completely untouched.
3. **Dedicated Variants**: Each Slate has one responsibility:
   - *Writing Slate*: Rich Markdown editing, document formatting, and full-text review without spatial overflow constraints.
  - *Memory Slate*: Masonry browsing of every Memory record, with derived Content and Anchor context where available, and handoff to creating Content or opening the appropriate content Slate.
   - *Gallery Slate*: High-resolution media viewing for images, GIFs, and videos.
4. **Bi-Directional Persistence**: Edits made inside Writing Slate create the next immutable Memory record, update the targeted Content binding when one exists, and persist the record. Memory Slate browsing and placement handoff do not mutate records.
5. **Instant Dismissal & Return**: Closing a Slate restores instant full focus to the exact spatial camera view and selected Grid Layer occupied prior to opening.

### Data & State Schema
- **State Ownership**: Viewport Overlay Subsystem (`SlateHostOverlay`).
- **Named Slate Instance Attributes**:
  - `slate_id`: Unique session identifier.
  - `slate_type`: Enum (`writing_slate`, `memory_slate`, `gallery_slate`).
  - `source_reference`: `{ content_id, memory_id, annotation_id }`.
  - `is_active`: Boolean viewport visibility flag.
  - `editor_state`: Active document buffer or media playback state.
- **Persistence Boundary**: Buffer changes committed to the target file on disk via Grove save handlers.

---

## 3. Intersectionality Matrix

| Primitive | Intersection & Relational Rules |
| :--- | :--- |
| **Grid** | A Slate sits on the viewport above the Grid; opening one suspends active Grid panning while preserving underlying Grid cell positions. |
| **Memory** | Memory Slate browses every Memory record and derives Content/Anchor counts and Grid Layer context through external relationships. A Memory does not have placement state. |
| **Content** | Double-clicking Content opens it in Writing Slate for Markdown/notes or Gallery Slate for media, enabling in-depth editing without truncation. |
| **Aura** | Writing Slate, Memory Slate, and Gallery Slate may display Field Ledger metadata derived from Content Aura, but do not render Aura fields within their frames. |
| **Grid Layer** | Opening Content in a Slate reveals its native Grid Layer attribution and permits shifting target Grid Layer drop-offs during composition. |
| **Annotation** | Annotations can be sent to a Slate for focused reading and navigation without Information Plane spatial constraints. |
| **Blip** | Interacting with a Blip allows users to send its aggregated Annotation directly to a Slate for uninterrupted reading. |
| **Slates** | *Self-Intersection*: opening a different Slate target transitions the active Slate viewport. |

---

## 4. User Interaction & Camera Dynamics

- **Activation & Invocation**:
  - Double-clicking any Content item on the Grid.
  - Opening the Memory Slate from the HUD keybind or a routed Memory action.
  - Selecting "Open in Writing Slate" or "Open in Gallery Slate" from an Annotation overlay or context menu.
- **Viewport Mechanics**:
  - While a Slate is active, camera panning across the Grid is paused, allowing wheel scrolling to navigate its document or media smoothly.
- **Dismissal Controls**:
  - Pressing `Escape` or clicking the close CTA dismisses the Slate immediately, returning keyboard focus to the spatial Grid camera.

---

## 5. Derived Outcomes Mapping

- **[Continuous Uninterrupted Focus](../derived-outcomes/Continuous%20Uninterrupted%20Focus.md)**: Users edit long-form documents in a clean, focused viewport overlay without moving their spatial camera position.
- **[Instant Tactical Working Surface](../derived-outcomes/Instant%20Tactical%20Working%20Surface.md)**: Users pull up an active document or reference item on demand, edit it, and dismiss it cleanly.
- **[Browse Memory Slate](../derived-outcomes/Instant%20Tactical%20Working%20Surface.md)**: Users inspect complete Memory histories and trace relationships in a dedicated interface.
