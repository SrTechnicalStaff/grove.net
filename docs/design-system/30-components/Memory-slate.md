---
type: design-system-component
status: active
authority: derived-from-domain-model
source_of_truth: ../../domain/Memory-Slate.md
date: 2026-08-13
component: Memory Slate
plane: hud
surface_class: slate
tags: [grove, design-system, component]
---

# Memory Slate

Memory Slate is the direct, masonry-style presentation of every Memory in the
Memory ledger. It is a HUD-plane sub-application. It never appears as a Grid
overlay, Grid search control, placement inspector, or floating card.

## Product boundary

- Memory Slate reads the Memory ledger directly.
- One Memory record produces one river entry.
- The number and location of Content instances never determine whether a
  Memory appears.
- Opening Memory Slate does not require Grid focus, a selected Content item, a
  camera location, a Grid Layer, or a field snapshot.
- Browsing, filtering, and selecting Memories do not create Content.
- Placing a selected Memory is a separate handoff that creates Content with a
  foreign key to the existing Memory.
- Grid-facing lookup returns Content. Memory lookup exists only inside Memory
  Slate.

## Resting anatomy

The resting surface contains exactly two structural parts:

| Part | Contract |
| --- | --- |
| Slate field | Opaque `--surface-grid`, occupying the full or assigned half of the application client area on the HUD Plane. It has no floating offset, scrim, shadow, title, header, footer, toolbar, sidebar, tabs, or nested frame. |
| Memory river | Edge-to-edge masonry columns with `--sp-sm` gutters. It has no outer padding, row alignment, column rule, count, caption, or application-authored introduction. |

The application title bar remains the normal full-width window title bar. It
is window chrome, not Memory Slate anatomy.

## Memory representations

The river renders the Memory payload itself. It does not wrap every payload in
a uniform card.

| Payload | Resting representation |
| --- | --- |
| Picture | The complete image at its intrinsic aspect ratio. No crop, mat, caption, overlay, badge, edge, or added background. |
| Plain text or Note | The complete authored text on its authored note surface. Padding intrinsic to the Note belongs to the content representation; the river adds none. |
| Document | The document's own paper representation and authored hierarchy. The river adds no title strip or metadata block. |
| Unsupported or unavailable payload | Preserve its intended extent and expose the condition through accessibility. Do not invent substitute prose or silently omit the Memory. |

Titles, filenames, generated descriptions, payload-kind labels, Content
counts, Anchor labels, provenance, and relevance scores are absent at rest.
Authored text already contained in the Memory remains content and is rendered
unchanged.

## Masonry geometry

1. The usable width is the Slate width; no outer inset is subtracted.
2. The minimum column width is `280px` at 100% interface scale.
3. The horizontal and vertical gutter is `--sp-sm`.
4. Column count is `max(1, floor((width + gutter) / (minimum + gutter)))`.
5. Each Memory is placed into the shortest current column, with ties resolved
   to the left.
6. Pictures preserve intrinsic proportions. Text and documents grow to their
   complete authored height.
7. The river scrolls vertically. No entry is clamped, summarised, cropped, or
   forced to a uniform height.

## Entry and exit

- `M` opens Memory Slate from the application shell without consulting Grid
  state.
- Opening takes keyboard focus into the Slate, not into a hidden query field.
- `Escape` closes Memory Slate when no deeper explicit Memory operation is in
  progress and returns focus to the prior application context.
- The Grid viewport is neither resized nor used as a visual backdrop while a
  full Memory Slate is open.

## Lookup boundary

Memory lookup is an explicit mode inside Memory Slate. It is not present in the
resting anatomy and must not create a permanent search field, filter row,
invitation, chip, or control pinned over the Grid. Lookup results remain Memory
records. Content, Anchors, and Field Ledger evidence may contribute ranking,
but they do not become the result identity or the Slate's source.

The visual and interaction design of an invoked lookup mode must be approved as
its own state before implementation. The absence of that state does not permit
a resting query strip as a substitute.

## States

| State | Contract |
| --- | --- |
| Rest | Only Memory representations and gutters are visible. |
| Focused | A keyboard focus ring is drawn outside the active representation without changing its content. |
| Selected | A `2px` `--signal-interaction` outline is drawn outside the representation. Selection performs no other action. |
| Engaged | Open or Place begins only from an explicit command. Selection alone never opens or places. |
| Pending | The intended extent remains reserved while payload decoding completes. No spinner or invented label is added. |
| Unavailable | The Memory remains present and accessible. Recovery is explicit and does not use refusal colour for a decode failure. |

## Accessibility

- The Slate automation identity is `MemorySlate`; its accessible name is
  `Memories` even though no visible title is drawn.
- Each entry's accessible name comes from authored Memory content or source
  identity. Application prose is not substituted.
- Reading and keyboard order follows ledger order, not visual column order.
- At 200% interface text scale, column count drops rather than truncating text.
- Focus and selection remain distinguishable without colour.

## Refusals

- Any fixed Grid control whose purpose is to open or search Memories.
- A resting search field, search invitation, filter row, query chip, or result
  count.
- A visible `Memories` title, header, footer, toolbar, sidebar, tab strip, or
  close button inside the Slate.
- Outer Slate padding.
- A floating or modal Memory panel over a visible Grid.
- Uniform card wrappers, metadata footers, kind badges, and generated captions.
- Cropped pictures or text line clamps.
- Shadows, gradients, blur, glow, glass, or rounded-container stacks.
- Any language describing a Memory as placed, unplaced, anchored, located, or
  owned by the Grid.

## Visual target

The binding visual target is
`docs/design-system/Reference-material/Memory-slate/memory-slate-target-v2.png`.
It demonstrates the edge-to-edge river and content hierarchy. Generated sample
payloads in the target are illustrative only and must never be seeded into the
application.
