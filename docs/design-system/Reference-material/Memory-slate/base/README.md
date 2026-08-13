---
id: proto-base-readme
---

# Prototype Base — `docs/prototypes/base/`

This directory holds the **canonical shared substrate** for all Trace HTML prototypes.

## Files

| File | Purpose |
|---|---|
| `trace-proto-base.css` | All shared CSS: tokens, viewport/grid, cell light-field, placement animations, entity skins, ghost, burst/ripple, watermark, HUD legend, cursor dot. |
| `trace-proto-base.js` | Shared runtime: camera (pan/zoom), trail, aura, particle helpers, framing, keyboard skeleton. |
| `trace-proto-base.html` | **Fork this** to start a new prototype. Contains the standard canvas shell, HUD, seed placements, and clearly marked variant extension points. |
| `README.md` | This file. |

## How to create a new prototype

1. Copy `trace-proto-base.html` to `docs/prototypes/` and rename it (e.g. `prototype-21-ephemeral-chrome.html`).
2. Update the `<title>` tag.
3. Add feature-specific CSS in the `VARIANT ADDITIONS` `<style>` block — new tokens, new components only.
4. Add feature-specific interaction logic in the `VARIANT SCRIPT` `<script>` block.
5. Remove or replace the seed placements if needed.

The base CSS and JS are loaded via `<link>` and `<script src>` — do not copy them inline. That way any corrections to the base propagate automatically.

## What goes in the base vs. a variant

| Belongs in BASE | Belongs in VARIANT |
|---|---|
| Token `:root` variables (verbatim from proto-16a) | New tokens specific to a feature (e.g., `--c-agent`) |
| Viewport, grid, world transform | Feature-specific overlay panels (slate, HUD extension) |
| Cell light-field CSS (`--b`, `--g`) | Agent/hover-charge specific CSS |
| `pop`, `vanish`, `brought`, `pressed` animations | New interaction animations specific to one prototype |
| Entity skins: sheet, sticky, text-ent, figure, sketch | Sealed/provenance entity variants |
| Ghost preview (valid + invalid) | Feature-specific ghost overlays |
| Burst + ripple particles | Feature-specific particle variants |
| Watermark + layer chip | Feature-specific HUD panels |
| HUD legend skeleton | HUD content rows (keys, color dots) |
| Camera runtime (pan/zoom/trail/aura) | Gesture recognizers (drag, resize, marquee) |
| `frameAll`, `screenToCell` | Placement creation logic |

## Governance

- **Do not retune base values.** Token values in `trace-proto-base.css` match
  `prototype-type-16a.html` exactly and are the visual source of truth per
  `docs/spatial-architecture/09-visual-parity-gaps.md`.
- **New tokens belong in the variant** `<style>` block, not in the base.
- **Variants inherit all base classes.** A variant only needs to declare what is new or different.
- When a visual value is refined (e.g., burst scale corrected), update both the
  base and `07-design-tokens.md` with a provenance comment.

## Prototype inventory

| File | Status | Notes |
|---|---|---|
| `../prototype-type-16a.html` | **Retained — visual source of truth** | Cited by name in governance docs. Do not delete. |
| `base/trace-proto-base.html` | **Active base template** | Fork this for new prototypes. |
| Gen-17 / Gen-18 / Gen-19 HTML variants | **Deleted** | Substrate extracted into `trace-proto-base.css`/`.js`. Design rationale preserved in companion `.md` files. |
| `demo-dual-slates.html`, `demo-focus-editor.html`, `demo-onboarding-settings.html` | **Deleted** | Replaced by work orders (WO-020, WO-021). |
