---
type: derived-outcome
status: active
date: 2026-08-12
tags: [grove, derived-outcome, macro-scanning, distance-lod]
---

# Product Outcome: Macro Scanning Without Context Loss

> **Plain-English Human-Behavior Statement**:
> "I can step back to view my entire spatial field from a distance without unreadable text or artificial summary bubbles hiding the true layout of my work."

---

## 1. Behavior Change Description

When zooming out on conventional infinite canvas tools, text becomes tiny pixelated noise, or the application replaces individual cards with generic counter bubbles (e.g. "+15 items"). The user loses sight of where things are physically located and what shapes they form.

With macro scanning without context loss:
- Pulling back the camera simplifies how content is drawn without hiding its existence or position.
- Every placement maintains its exact cell footprint in a recognizable representation tier (stand-in form, stepped form).
- Macro zoom levels transition aura fields to geographic map-like level-of-detail (LOD) views, letting users perceive spatial-field density and major focus clusters instantly.

---

## 2. Real-World Human Impact

- **Before**: A director zooms out on a project board to see overall progress, but all 200 tasks turn into unreadable microscopic dots or aggregate into generic group bubbles with numbers, destroying visual context.
- **After**: The director zooms out to a $10\%$ scale view. Grid lines fade continuously, items simplify into clean stand-in cards, and aura fields glow warmly around active work areas, giving an immediate, accurate bird's-eye map of project health.

---

## 3. Product Architecture Mapping

- **Core Primitives**: `Grid`, `Aura`, `Content`, `Blip`.
- **System Mechanics**:
  - **Representation Tiers**: Content sheds detail gracefully across demote/promote thresholds without disappearing.
  - **Continuous Grid Line Fade**: Formula `ink = clamp((spacing - 6)/8, 0, 1)` ensures lines never pop discretely.
  - **Aura Field Map LOD**: Macro distance switches fields to continuous map representations while blip markers signal cluster centers.
