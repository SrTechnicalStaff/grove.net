---
id: prototype-23-memory-slate
---

# Trace — The Memory Slate (Prototype 23) Design Write-up

This is the companion document for the standalone prototype [`prototype-23-memory-slate.html`](prototype-23-memory-slate.html), which demonstrates the temporal recall lens (docked beside the spatial grid) based on the [`06-memory-slate.md`](../grid-intelligence/06-memory-slate.md) product specification.

---

## 1. Purpose & scope

This prototype demonstrates:
1. **Right-Docked Sub-Application:** Positioning the Memory Slate on the right side of the canvas (opposing the left-side default writing slate) at three escalating widths: 25% (Tray), 50% (Browser), and 100% (Gallery).
2. **Above-Grid Layering:** Siting the slate panel physically on top of the grid canvas (using a high `z-index`, left hairline border, and deep drop-shadow) so that opening or resizing the slate does not warp the canvas aspect ratio or deform the user's focal point.
3. **Pinterest-Inspired Layout:** Adapting the memories list into a column-based variable-height masonry feed of cards (images, text, sketches, and audio). Features content-driven heights, extracted color-palette dots beneath cards, custom play-in-place audio waveforms, and copper-colored agent logs.
4. **Bi-directional Traffic:** Dragging Memories with zero current Placements from the slate onto empty grid cells to "place" them (generating a grid placement and gold burst), and dragging placements from the grid into the slate to "unplace" them (setting their Placement count to zero).
5. **Hue Facets & Recall States:** Selecting dominant colors (Hue Facet flyout) or running saved search chips (`JAMIE'S BDAY`, `BOTANICALS`) to dynamically filter the masonry grid.

---

## 2. Interaction model

The prototype implements the following keyboard and pointer interactions:

| State | Input | Result |
|---|---|---|
| Field / Slate | `\` key | Toggles the Memory Slate open/closed. Keeps width memory. |
| Slate Open | `Ctrl + \` key | Cycles slate width: 25% (Tray) &rarr; 50% (Browser) &rarr; 100% (Gallery) &rarr; 25% |
| Card Selected | `P` key | Places the selected memory at the grid's camera center (with collision avoidance walk). |
| Card (Resting) | Click | Selects the card (Amber outline, flares grid placement if it exists). |
| Card (Resting) | Double-Click | Opens the Detail Modal to view full image, metadata, and description. |
| Card in Slate | Drag to Grid | Spawns a floating gold-bordered ghost (`.obj.carried`). If dropped on an empty grid cell, creates a placement (gold burst). Snaps back on overlap. |
| Grid Placement | Drag to Slate | Triggers the Slate Drop Zone ("Remove current Placement"). If dropped, deletes placement from the canvas. |
| Audio Card | Play Click | Toggles audio play state, animates playback timeline progress and shifts waveform colors. |
| Color Swatch | Click | Filters masonry items by the clicked dominant color (Hue Facet). |
| Category Chip | Click | Filters items by query keyword (Recall mode). Burns gold. |

---

## 3. Visual grammar

Following the Gen-17 successor design grammar, the prototype introduces these visual elements:

* **Slate Panel:** Graphite background (`#0E0E10`), left boundary seam hairline (`1px solid #242428`), and deep soft drop shadow (`box-shadow: -8px 0 32px rgba(0,0,0,0.55)`). This raises it visually above the dark grid.
* **Responsive Masonry columns:** Powered by native CSS Columns to guarantee smooth performance:
  * **25% Tray:** 1 column. Sidebar hidden.
  * **50% Browser:** 2 columns. Sidebar visible.
  * **100% Gallery:** 4 columns. Grid canvas suspended (`opacity: 0`).
* **Pinterest Card Skin:** Borderless at rest, dark grey card background (`#161618`), 10px rounded corners. Hovering reveals a warm-white `1px` hairline outline.
* **Palette Indicators:** A row of circular color dots (3-4 swatches) beneath each card, representing the primary dominant colors extracted at import.
* **Amber Selections:** Selected cards gain a 2px warm-amber outline (`rgb(232, 185 100)`) and amber outer glow.
* **Carried Preview:** Dragged ghost elements gain gold borders (`rgb(242 201 122)`), cast shadows, and upward chevrons (▲) to denote they are lifted off the table. Snapping to blocked cells turns the outline dusty-red (`rgb(226 98 92)`).
* **Agent Cards:** Styled with a copper border outline (`#C4824A`) and feature a copper corner tag with an unvisited glow ember (glowing dot) that vanishes upon inspection.

---

## 4. UX rationale & accountability

1. **Why on the Right?** Docking the Recall panel on the right side balances the interface against the Writing Slate (which docks on the left for text layout composition). This reserves the right margin for visual searching, temporal glances, and drag-and-drop pulls.
2. **Above-Grid Overlay Architecture:** Placing the slate above the grid (instead of shrinking the viewport grid) solves the camera re-centering dilemma. The grid does not deform or trigger size changes on open; instead, the slate acts as drafting equipment placed over a map.
3. **Drafting Masonry over IOS Styles:** The cards reject drop-shadow soup or high-contrast border borders. They are styled in graphite tones so that media images and drawing lines remain the focal points.
4. **CSS Columns for Masonry:** Instead of calculating block heights in JS on scroll, CSS `column-count` is used. This is GPU-friendly and layout shifts are avoided when cycling widths.

---

## 5. Parity notes

* **Camera & Navigation:** Inherits Middle-drag panning, scrollwheel zooming, and cursor speed-reactive trail painting from `trace-proto-base.js`.
* **Placements:** Uses the same `.sheet` and `.sticky` placement elements for text/notes and triggers the same scale-in entrance pops and exit vanishings.
* **Tokens:** Inherits default typography (Oswald, Inter, JetBrains Mono) and eases from `:root`.

---

## 6. Open questions & deferred

1. **Double-Peel Conflicts:** How does the cursor behave when both a left slate and right slate are open? Swap-gestures are deferred until a third slate is introduced.
2. **Hue extraction performance:** The dominant color extraction process should occur asynchronously in a web-worker during ingestion.
