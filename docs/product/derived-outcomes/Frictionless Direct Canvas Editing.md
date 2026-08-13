---
type: derived-outcome
status: active
date: 2026-08-12
tags: [grove, derived-outcome, direct-canvas-editing, armed-cursor]
---

# Product Outcome: Frictionless Direct Canvas Editing

> **Plain-English Human-Behavior Statement**:
> "I can manipulate, resize, copy, and move items across my spatial field using simple single-key gestures without dealing with complex toolbar menus."

---

## 1. Behavior Change Description

Traditional graphic canvases and document tools rely on crowded toolbars, nested right-click context menus, and confusing mode toggles. Performing simple operations like copying a group of items or tracing a thought across planes requires searching through menus and clicking small icon buttons.

With frictionless direct canvas editing:
- The cursor features an armed state toggled simply with the `1` key (cycling through Trace, Resize, Copy, Cut, Duplicate).
- Visual feedback relies on distinct cell border accents, colors, and markers—mirroring the world's most intuitive spreadsheet cell dynamics.
- Moving, selecting, and arranging content feels immediate, deliberate, and free of mechanical drag friction.

---

## 2. Real-World Human Impact

- **Before**: A manager organizing project cards on a digital board has to click a menu button, choose 'Duplicate', drag the card, click another menu button, choose 'Resize', and drag handles.
- **After**: The manager presses `1` to arm the cursor, sweeps across cells to select items, and executes precise moves and resizes instantly using direct keyboard-and-mouse gestures.

---

## 3. Product Architecture Mapping

- **Core Primitives**: `Grid`, `Content`, `Memory`.
- **System Mechanics**:
  - **Armed Cursor Cycle**: `1` key cycles through Trace, Resize, Copy, Cut, and Duplicate modes.
  - **Excel-Inspired Cell Accents**: Border highlights and marquee selections operate on cell boundaries.
  - **Deterministic Movement**: Spatial engine enforces non-slippery, non-sticky deterministic movement physics.
