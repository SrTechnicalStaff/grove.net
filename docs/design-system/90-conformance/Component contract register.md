---
type: design-system-register
status: normative
date: 2026-08-10
owner: "Grove Design System"
tags: [grove, design-system, component, catalogue]
---

# Component contract register

## Required contract

| Section | Required content |
| --- | --- |
| Anatomy | Every visible part, role, token, and exact component-only value. |
| Geometry | Footprint, width, height, measure, spacing, alignment, growth, and reflow. |
| States | Rest, Approached, Focused, Selected, Engaged, Pending, Refused, Unavailable, Anchored. |
| Behaviour | Pointer, keyboard, focus, Escape, commit, cancel, return, and persistence. |
| Motion | Transition, duration, curve, reduced-motion result, and no-motion rules. |
| Distance | Camera relation, thresholds, shedding order, and retained identity. |
| Accessibility | Role, name, focus order, keyboard access, target size, and contrast. |
| Copy | Canonical labels, messages, case, and forbidden fallback strings. |
| Refusals | Unsupported, unavailable, invalid, stale, and recovery behavior. |
| Design assertions | Stable assertions against this design system; no implementation status. |
| Sources | Read-only raw notes, accepted Decisions, References, wireframes, and catalogue decks. |

## Catalogue-to-owner register

| Catalogue deck | Normative design-system owner(s) | Contract |
| --- | --- | --- |
| `src/00-design-language.html` | `00-foundations/Color.md`; `Typography.md`; `Shape.md`; `Space-and-grid.md`; `Elevation-and-depth.md`; `Motion.md`; `10-grammar/Surface-classes.md`; `Copy.md` | Foundations |
| `src/01-menus.html` | `30-components/Menu.md`; `Inline-confirm.md` | Action chrome |
| `src/grid-plane/01-the-grid.html` | `00-foundations/Grid-expression.md`; `30-components/Grid-lines.md`; `Grid-cursor.md`; `Selection.md` | Grid expression |
| `src/grid-plane/02-note.html` | `30-components/Note.md`; `Stand-in.md`; `Presence.md` | Note |
| `src/grid-plane/03-document.html` | `30-components/Document.md`; `Stand-in.md`; `Presence.md` | Document |
| `src/grid-plane/04-image.html` | `30-components/Picture.md`; `Stand-in.md`; `Placement-preview.md` | Picture |
| `src/grid-plane/05-presence-fields.html` | `00-foundations/Grid-expression.md`; `30-components/Presence.md` | Presence |
| `src/grid-plane/06-distance.html` | `10-grammar/Representation-tiers.md`; `30-components/Stand-in.md` | Distance |
| `src/grid-plane/07-selection-placement.html` | `30-components/Selection.md`; `Placement-preview.md`; `Refusal.md` | Selection and placement |
| `src/hud/01-slate-anatomy.html` | `30-components/Slate.md`; `20-planes/HUD-plane.md` | Slate host |
| `src/hud/02-memory-slate.html` | `30-components/Memory-slate.md`; `Slate.md` | Memory Slate |
| `src/hud/03-gallery-slate.html` | `30-components/Gallery-slate.md`; `Slate.md`; `Picture.md` | Gallery Slate |
| `src/hud/04-writing-slate.html` | `30-components/Writing-slate.md`; `Slate.md`; `Inline-confirm.md` | Writing Slate |
| `src/information-plane/01-local-editors.html` | `30-components/Local-editor.md`; `Inline-confirm.md`; `20-planes/Information-plane.md` | Local editor |
| `src/information-plane/02-image-viewer.html` | `30-components/Image-viewer.md`; `Picture.md`; `Refusal.md` | Image Viewer |
| `src/information-plane/03-quick-note.html` | `30-components/Quick-note.md`; `Local-editor.md`; `Refusal.md` | Quick Note |
| `src/information-plane/04-annotation-markers.html` | `30-components/Blip.md`; `Annotation.md`; `10-grammar/Annotation-ledger.md` | Annotation marker |
| `src/information-plane/05-annotation-mixed-media.html` | `Annotation.md`; `Mixed-media-annotation.md`; `10-grammar/Content-concentration.md` | Mixed-media Annotation |
| `src/information-plane/06-annotation-text-led.html` | `Annotation.md`; `Text-led-annotation.md`; `10-grammar/Content-concentration.md` | Text-led Annotation |
| `src/information-plane/07-annotation-image-led.html` | `Annotation.md`; `Image-led-annotation.md`; `10-grammar/Content-concentration.md` | Image-led Annotation |
| `src/information-plane/08-annotation-routes.html` | `Annotation-routes.md`; `Annotation.md` | Annotation routes |

## Non-Annotation component owners

| Owner | Deck source | Status |
| --- | --- | --- |
| `Blip.md` | `information-plane/04-annotation-markers.html` | Complete |
| `Document.md` | `grid-plane/03-document.html` | Complete |
| `Gallery-slate.md` | `hud/03-gallery-slate.html` | Complete |
| `Grid-cursor.md` | `grid-plane/01-the-grid.html` | Complete |
| `Grid-lines.md` | `grid-plane/01-the-grid.html` | Complete |
| `Image-viewer.md` | `information-plane/02-image-viewer.html` | Complete |
| `Inline-confirm.md` | `01-menus.html`; `hud/04-writing-slate.html` | Complete |
| `Layer-manager.md` | `00-design-language.html`; HUD plane source | Complete |
| `Local-editor.md` | `information-plane/01-local-editors.html` | Complete |
| `Memory-slate.md` | `hud/02-memory-slate.html` | Complete |
| `Menu.md` | `01-menus.html` | Complete |
| `Note.md` | `grid-plane/02-note.html` | Complete |
| `Operation-bar.md` | `00-design-language.html`; Information Plane sources | Complete |
| `Picture.md` | `grid-plane/04-image.html` | Complete |
| `Placement-preview.md` | `grid-plane/07-selection-placement.html`; `grid-plane/04-image.html` | Complete |
| `Presence.md` | `grid-plane/05-presence-fields.html` | Complete |
| `Quick-note.md` | `information-plane/03-quick-note.html` | Complete |
| `Refusal.md` | `grid-plane/07-selection-placement.html`; `01-menus.html` | Complete |
| `Selection.md` | `grid-plane/07-selection-placement.html` | Complete |
| `Slate.md` | `hud/01-slate-anatomy.html` | Complete |
| `Stand-in.md` | `grid-plane/06-distance.html` | Complete |
| `Watermark.md` | `grid-plane/01-the-grid.html`; `00-design-language.html` | Complete |
| `Writing-slate.md` | `hud/04-writing-slate.html` | Complete |

## Coverage rule

| Rule | Value |
| --- | --- |
| Catalogue decks inventoried | `21` |
| Catalogue owners | `21` |
| Non-Annotation component owners | `22` |
| Annotation source contracts | `9` closed decisions in `Annotation decisions.md` |
| Normative status | Every owner listed above has a design-system file. |
| Missing-owner result | A deck without an owner fails the register. |
