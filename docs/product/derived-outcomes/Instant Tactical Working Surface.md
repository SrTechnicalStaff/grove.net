---
type: derived-outcome
status: active
date: 2026-08-12
tags: [grove, derived-outcome, tactical-working-surface, slate-overlay]
---

# Product Outcome: Instant Tactical Working Surface

> **Plain-English Human-Behavior Statement**:
> "I can instantly bring up a focused writing slate or reference document over my work, complete my edits, and dismiss it without disturbing my spatial layout."

---

## 1. Behavior Change Description

When working across large visual canvasses, opening a document for deep editing often requires zooming the entire canvas camera into a tight view, losing sight of surrounding materials, or opening separate window tabs that break the spatial workspace context.

With instant tactical working surface:
- Double-clicking any item on the spatial surface immediately brings up a clean writing slate fixed to the screen viewport.
- The user writes, formats, and inspects content in a full-screen or focused overlay while the underlying canvas arrangement remains completely fixed in place.
- Closing the slate instantly restores full control to the spatial view exactly as it was left.

---

## 2. Real-World Human Impact

- **Before**: A researcher needs to edit a 5-page research document pinned to a large visual board. Zooming in close forces them to lose visual track of ten related notes scattered nearby, requiring tedious camera panning back and forth.
- **After**: The researcher pulls up the writing slate in one action, completes continuous prose edits in a clean overlay, and closes it. The camera remains positioned precisely over the entire research cluster.

---

## 3. Product Architecture Mapping

- **Core Primitives**: `Slate`, `Content`, `Grid`.
- **System Mechanics**:
  - **Viewport-Fixed HUD Overlay**: Slates occupy Plane 2 (HUD Plane) at fixed screen coordinates, leaving Plane 0 (Grid) intact.
  - **Synchronous Persistence**: Edits made inside the slate update underlying memory objects and disk files synchronously.
  - **Instant Focus Return**: Dismissing slate returns focus immediately to grid camera navigation.
