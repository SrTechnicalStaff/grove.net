---
type: design-system-conformance
status: normative
date: 2026-08-10
owner: "Grove Design System"
tags: [grove, design-system, grid, implementation, slices]
---

# Grid Plane implementation slices

## Contract

| Property | Rule |
| --- | --- |
| Scene owner | Grid Plane owns cell geometry, Layers, Placements, Grid-space Content, Presence, Grid Cursor, selection, previews, refusal, distance representation, and Layer mark. |
| Camera owner | Camera owns `{ x, y, scale }`, normalization, pan, zoom, focal continuity, and framing math. Camera owns no DOM, Placement, Content, surface, or plane state. |
| Projection seam | One Grid projection consumes a Camera snapshot and Grid geometry. Every Grid-space visual uses the same projection. |
| Information relationship | Information consumes an accepted Placement or Memory reference and an Information-local anchor. It does not subscribe to Camera or derive Grid projection. |
| HUD relationship | HUD consumes viewport-local surface requests. It is Camera-blind and does not consume Grid coordinates. |
| Durable state | Memory, Content, Placement, Layer, Layer order, Camera view state, and history remain distinct records. |
| Derived state | Grid lines, Presence, cursor trail, selection marks, previews, refusals, representation tier, and mounted Grid DOM/canvas are derived. |
| Layer depth | Unbounded. Field falloff changes rendering work only; it never rejects, moves, merges, or deletes a Layer or Placement. |
| Camera rule | Pan changes projected position. Zoom changes projected cell size and Content size by the same ratio. Grid dimensions and Layer membership do not change. |
| Input rule | Grid input enters through the named input coordinator and resolves against Grid state. Camera input changes Camera view state only. |

## Domain vocabulary

| Term | Identity | Owns | Does not own |
| --- | --- | --- | --- |
| Camera | `{ x, y, scale }` view record | View position, scale, focal continuity, framing | Grid records, DOM, surfaces, field, representation |
| Grid cell | Integer `{ col, row }` address | Whole-cell geometry | Content identity, screen position |
| Layer | Stable `LayerId` in explicit order | Layer membership and traversal order | Camera state, field persistence |
| Memory | Stable `MemoryId` | Canonical Content revisions and payload | Spatial occurrence |
| Placement | Stable `PlacementId` | `MemoryId`, `LayerId`, cell origin, width, height, presentation context | Selection, cursor, representation tier |
| Presence | No durable identity | Derived per-cell contribution from all reachable Placements | Relationships, groups, counts, routes |
| Representation tier | Placement-local derived form | Working, stepped, or stand-in drawing | Placement identity, position, extent |
| Grid Cursor | Transient addressed cell | Pointer/keyboard cell attention and trail | Selection, durable state, Content mutation |
| Selection | Transient Memory set | Current-Layer selection state | Content style, persistence, Placement identity |
| Placement preview | Transient prospective footprint | Validity, refusal role, origin projection | Durable Placement mutation before commit |

## Dependency direction

```text
Camera math ───────────────┐
Grid geometry ─────────────┼─> Grid projection ─> Grid renderers
Placement and Layer model ─┘          │
                                      ├─> lines
                                      ├─> Presence
                                      ├─> Content forms
                                      ├─> stand-ins
                                      └─> cursor, selection, previews

PlacementRef / MemoryRef ─> Information-local surfaces
MemoryRef / intent ───────> HUD surfaces
```

| Allowed dependency | Forbidden dependency |
| --- | --- |
| Grid projection reads a Camera snapshot. | Camera reads Grid records. |
| Grid renderers read typed Grid projections and selectors. | Each renderer invents a Camera transform. |
| Presence reads Placement and Layer order. | Presence becomes durable truth. |
| Information receives a validated local anchor at open. | Information recomputes its anchor from Camera movement. |
| HUD receives viewport-local surface requests. | HUD reads Grid coordinates or Camera state. |
| Features call named selectors and commands. | Features reconstruct another feature's model from shared state. |

## Slice map

| Slice | Name | Primary contract owners | Required result |
| --- | --- | --- | --- |
| GP-01 | Camera and Grid projection | `domain/camera.ts`, `domain/grid.ts`, `ui/canvas/grid.ts`, `grid/stage.ts`, camera controls | One pure Camera model and one Grid projection. Lines, Grid world, cell geometry, and Camera input remain focal, finite, and camera-consistent. |
| GP-02 | Presence and Layer field | `domain/field.ts`, `ui/canvas/field.ts`, `30-components/Presence.md`, `10-grammar/Layer-depth.md` | Full derived cross-Layer field calculation with hard cell boundaries, bounded visible work, no saturation mutation, and field beneath Content. |
| GP-03 | Content forms and footprints | `domain/placement.ts`, `domain/image-footprint.ts`, Grid stage, material styles, content commands | Note, Document, and Picture use explicit whole-cell footprints, complete form-specific geometry, intrinsic image proportions, and exact Placement identity. |
| GP-04 | Cursor and Grid interaction | `Grid-cursor.md`, `Selection.md`, `Placement-preview.md`, `Refusal.md`, cursor, selection, drag, keyboard, command owners | Cursor, selection, marquee, move, resize, placement, refusal, handles, Escape, and Delete follow one Grid interaction grammar. |
| GP-05 | Distance representation | `Representation-tiers.md`, `Stand-in.md`, `grid/representations.ts`, Grid content styles | Working, stepped, and stand-in forms use projected cell size, hysteresis, exact footprints, in-place promotion, and no selection pinning. |
| GP-06 | Grid persistence and plane handoff | Grid outcomes, Layer commands, history, persistence, surface seam, Grid QA | Camera/view persistence, Layer traversal, history, transfer, source handoff, and return behavior preserve Grid ownership and plane isolation. |

## Slice boundaries

### GP-01 — Camera and Grid projection

| Acceptance | Proof |
| --- | --- |
| Camera math is DOM-free and Grid-record-free. | Domain tests for normalization, inverse projection, pan, focal zoom, and framing. |
| Grid world, lines, and cell-bound visuals use one Camera snapshot. | Projection test plus browser capture at working, far, and reversed zoom. |
| Pan changes position without changing projected cell size. | Grown Grid navigation journey. |
| Focal zoom preserves the pointed-to Grid coordinate. | Camera focal-continuity test and browser wheel journey. |
| Information and HUD remain unchanged by Camera movement. | Plane-isolation journey and surface coordinator assertions. |

### GP-02 — Presence and Layer field

| Acceptance | Proof |
| --- | --- |
| Current-Layer Content is full; other Layers contribute Presence only. | Presence across Layers journey and rendered-layer assertions. |
| Every reachable source contributes through the same field math. | Domain field matrix across Layer distances, footprints, and overlaps. |
| Saturated deep Layers remain durable and navigable. | Layer-depth fixture including Layer 15 and Layer 16+. |
| Cells remain hard-edged and accumulation remains per cell. | Visual capture and field canvas pixel assertions. |
| Field never outranks authored Content or receives pointer input. | Draw-order assertion and pointer pass-through journey. |

### GP-03 — Content forms and footprints

| Acceptance | Proof |
| --- | --- |
| Note, Document, and Picture have distinct canonical resting forms. | Component visual assertions and Grid form journey. |
| Every footprint is integral, complete, and aligned to major lines. | Geometry property tests and browser footprint assertions. |
| Document sizing resolves from readable demand within its declared bounds. | Document demand fixture covering minimum, ordinary, and maximum forms. |
| Picture frames preserve intrinsic aspect ratio and source identity. | Image fixture covering portrait, landscape, animated, and unavailable sources. |
| Placement geometry remains separate from Memory identity. | Persistence and revision invariant tests. |

### GP-04 — Cursor and Grid interaction

| Acceptance | Proof |
| --- | --- |
| Empty-cell cursor is one cell; occupied cursor covers the complete rectangular footprint. | Cursor browser journey and DOM footprint assertion. |
| Full-footprint cursor has one perimeter treatment and no interior grid lines. | Visual capture at 1×1, rectangular, and saturated footprints. |
| Selection is outside the Placement edge and does not restyle authored Content. | Selection visual assertion and multi-selection journey. |
| Marquee resolves whole Placements live and restores prior selection on Escape. | Instant working surface journey. |
| Move, resize, and placement preview preserve source identity until commit. | Commit/cancel/refusal browser journeys. |
| Delete removes selected current-Layer Content outside authored text. | Application keybind journey. |
| Grid interaction never opens Information or HUD surfaces implicitly. | Pointer and keyboard isolation journey. |

### GP-05 — Distance representation

| Acceptance | Proof |
| --- | --- |
| Tier selection uses projected cell size, not projected Placement footprint. | Representation domain matrix with mixed-size Placements. |
| Selected stand-ins remain stand-ins and keep the selection treatment. | Selected-at-distance browser journey. |
| Demotion and promotion use distinct thresholds and preserve the exact footprint. | Hysteresis property test and zoom reversal journey. |
| Promotion is in place, asynchronous, cancellable, and never blank. | Promotion queue fixture and whole-field view journey. |
| Kind-coded stand-ins preserve Note, Document, and Picture distinction. | Stand-in visual matrix at the 1% floor. |

### GP-06 — Grid persistence and plane handoff

| Acceptance | Proof |
| --- | --- |
| Camera view and Grid line preference survive reload without changing product records. | Persistence reload journey. |
| Layer traversal preserves Layer and Placement identity. | Layer navigation and transfer journeys. |
| Undo and redo restore Grid records atomically. | Session trust journey and history invariants. |
| Source-owned handoffs carry typed Placement or Memory references only. | Surface contract checks and route journey. |
| Information and HUD never gain Camera or Grid ownership through a handoff. | Coordinator and plane-isolation assertions. |

## Round gate

Every slice is a separate round.

| Order | Gate | Result |
| --- | --- | --- |
| 1 | Slice tests and repository verification | Exact commands and failures recorded. |
| 2 | Two-axis review | Standards and specification findings remain separate. |
| 3 | Installer rebuild | NSIS and MSI rebuilt; installation is not performed. |
| 4 | Artifact check | Installer metadata matches the active application version. |
| 5 | Scope check | Only slice files are staged; unrelated dirty work remains untouched. |
| 6 | Commit | Commit names the slice and verified result. |
| 7 | Push | Commit is pushed to the explicitly selected branch. |
| 8 | Handoff | Remaining unproved rows and blockers are listed before the next slice. |

## Source owners

| Source | Role |
| --- | --- |
| `design-system/20-planes/Grid-plane.md` | Plane ownership and chrome budget. |
| `design-system/00-foundations/Grid-expression.md` | Grid visual grammar and exclusions. |
| `design-system/00-foundations/Space-and-grid.md` | Cell, footprint, line, and camera-scaled geometry. |
| `design-system/10-grammar/Layer-depth.md` | Unbounded Layer and saturation rules. |
| `design-system/10-grammar/Representation-tiers.md` | Distance tiers, thresholds, shedding, and promotion. |
| `design-system/30-components/*.md` | Component-level visual and interaction contracts. |
| `reference/Grid and Placement model.md` | Stable domain identities and operations. |
| `reference/Surface seam and contract model.md` | Cross-plane camera and source boundaries. |
| `decisions/Camera observer and Grid projection boundary.md` | Pure Camera and single Grid projection decision. |
| `ux/wireframes/grid-scale/*.md` | Journey order, recovery, and evidence states. |
