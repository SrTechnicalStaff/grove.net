---
type: design-catalogue-index
status: active
date: 2026-08-09
tags: [grove, design, catalogue]
---

# Design catalogue

PDF decks that define the intentional design of every Grove surface, organized
by plane, then by purpose. Each deck breaks one surface or content type down
the way a fashion house documents a garment: the design is the hero,
annotations are a separate monospace layer, and every choice carries a
plain-English reason. DESIGN.md remains the token and rule authority; these
decks are its applied form — what agents build against and what review
measures against.

## Shared

- `00-design-language.pdf` — palette, role hues, typography, spacing, shape,
  depth, the four surface classes, refusals.
- `01-menus.pdf` — the one menu grammar for every plane: rows, quick keys,
  groups, flyouts.

## Grid plane

- `grid-plane/01-the-grid.pdf` — line hierarchy, legibility fade, visibility,
  the cursor.
- `grid-plane/02-note.pdf` — the sticky-note world model on the grid.
- `grid-plane/03-document.pdf` — the research-paper representation.
- `grid-plane/04-image.pdf` — complete frames on cell footprints; GIFs.
- `grid-plane/05-presence-fields.pdf` — cell-quantized aura, hue
  accumulation, cross-Layer presence.
- `grid-plane/06-distance.pdf` — the shedding order, kind-coded stand-ins,
  promotion.
- `grid-plane/07-selection-placement.pdf` — marquee, selection, placement
  previews, refusal.

## Information plane

- `information-plane/01-local-editors.pdf` — the beside-the-source surface
  class; the Note text editor.
- `information-plane/02-image-viewer.pdf` — complete-frame inspection beside
  the source.
- `information-plane/03-quick-note.pdf` — the fixed capture feed.
- `information-plane/04-annotation-markers.pdf` — blips, saturation gating,
  engagement cues, the staircase, pin and return.
- `information-plane/05-annotation-mixed-media.pdf` — the mixed-media
  vertical: Tier 01 brochure, Tier 02 pamphlet, Tier 03 magazine template
  families, ratio and capacity routing, image-resolution variants, source
  identity, pagination, and refusals.
- `information-plane/annotations-mixed-media-physical-forms.md` — the research
  reference for the physical forms, layout families, ratio calculation, image
  quality bands, and source references behind the deck.
- `information-plane/06-annotation-text-led.pdf` — the text-led vertical:
  Tier 01 bulletin, Tier 02 Berliner/compact, Tier 03 broadsheet landscape
  newspaper grids, readable measure, reflow, and refusals.
- `information-plane/annotations-text-led-physical-forms.md` — research on
  bulletin, Berliner/compact, and broadsheet newspaper forms, layout families,
  text capacity, continuation, and physical references.
- `information-plane/07-annotation-image-led.pdf` — the image-led vertical:
  Tier 01 gallery, Tier 02 contact sheet, Tier 03 image edition (physical
  analogue: photobook / image-led magazine sequence), complete frames, aspect
  families, fidelity, and refusals.
- `information-plane/annotations-image-led-physical-forms.md` — research on
  photographic plates, contact/proof sheets, photobooks, image sequences,
  resolution, aspect ratio, and source metadata.
- `information-plane/08-annotation-routes.pdf` — explicit destinations and
  returns from a reading.

Annotation decks use the plate/notes grammar: each design fills its own
slide with numbered markers only; the keyed notes follow on the next page.

### Text-led form vocabulary

The text-led deck maps the three demand tiers to landscape newspaper grammars:

- **Tier 01 — Bulletin**: a short landscape sheet with two or three columns.
- **Tier 02 — Berliner / compact**: a landscape facing spread with stable
  columns.
- **Tier 03 — Broadsheet**: a landscape spread with threaded multi-column text
  and additional pages.

These are compositional names, not literal print dimensions. Berliner is the
middle newspaper reference; “compact” is the plain-language alias. Bulletin is
the small newspaper-like form, not a claim that Grove prints a fixed paper
size.

## HUD

- `hud/01-slate-anatomy.pdf` — the Slate class and the two-pane host.
- `hud/02-memory-slate.pdf` — the browse-first gallery over every Memory form.
- `hud/03-gallery-slate.pdf` — the complete-frame thumbnail wall.
- `hud/04-writing-slate.pdf` — focused long-form writing.

## Maintenance

Deck sources are HTML under `src/`, styled by `src/shared/catalogue.css`
(a mirror of DESIGN.md tokens) and governed by `src/BRIEF.md`. Rebuild any
deck with:

```text
node docs/design_catalogue/src/build.mjs [src/<plane>/<deck>.html]
```

Review slides during authoring with `src/snap.mjs`. A deck change that alters
a normative value must trace back to a DESIGN.md change, not the other way
around.
