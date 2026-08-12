---
type: design-system-conformance
status: normative
date: 2026-08-11
owner: "Grove Design System"
tags: [grove, design-system, information, annotations, implementation, slices]
---

# Information Plane implementation slices

## Contract

| Property | Rule |
| --- | --- |
| Plane owner | Information Plane owns source-local surfaces, annotation markers, cues, routes, ledgers, readers, local chrome, and local handoffs. |
| Coordinate owner | Information owns screen-constant local chrome attached to an exact canonical source anchor. |
| Camera relationship | Information features are Camera-blind. The separate projection bridge reprojects source anchors when Camera geometry changes. |
| Grid relationship | Information receives typed Placement, Memory, Content, surface, or pointer-origin references. It does not read Grid projection, cell geometry, cursor geometry, or Camera state. |
| HUD relationship | HUD is a separate viewport-fixed substrate above Information. Information surfaces never become HUD because they opened later. |
| Plane order | Grid is lowest, Information is next, HUD is highest. A later opening cannot change this order. |
| Surface seam | Every Information surface enters and leaves through the routed surface coordinator. Features do not mount Information DOM directly or call another feature's surface owner. |
| Source ownership | The feature that owns the source chooses the named route and supplies the source reference. The Information Plane resolves local placement and surface state. |
| Durable state | Annotation ledger records, Memory, Content, Placement, Layer, and history remain separate. Marker, cue, route, ledger edition, page, focus, and pin state are derived or session state. |
| Layer depth | Layer count and field traversal are unbounded. Field falloff may omit paint work below the rendering floor; it never rejects, merges, moves, or deletes a Layer, Placement, or source. |
| Information density | The resolver maximizes complete authored information inside the landscape unit subject to `34ch`/`283px` minimum readable text measure and role-specific image fidelity floors. Density never licenses unreadable text, low-detail frames, truncation, crop, stretch, or overflow. |
| Interface copy | No plane name, architecture term, process term, conversation text, subtitle, annotation about the interface, or generic umbrella term appears in product chrome. Authored source text is preserved as authored content. |

## Domain vocabulary

| Term | Identity | Owns | Does not own |
| --- | --- | --- | --- |
| Information anchor | One accepted local point and side in Information coordinates | Initial local placement of a surface or cue | Grid address, Camera projection, viewport fixation |
| Information surface | Named surface with plane, anchor, source reference, state, and lifecycle | Its local DOM, focus, close, and return contract | Grid records, Camera, another surface's state |
| Source reference | Typed Placement, Memory, Content, or intent reference | Route identity and source provenance | Local position or rendered geometry |
| Annotation candidate | Derived source group with demand, support, order, form, and availability | Eligibility and resolver input | Durable annotation identity or DOM position |
| Annotation ledger | Ordered source-owned edition of gathered candidates | Nine forms, layout family, pages, reflow edition, and source order | Camera, Grid cursor, durable source mutation |
| Blip | Derived local engagement mark | Marker, cue, route, and local engagement state | Cell footprint, selection, Camera following |
| Local route | Information-local orthogonal path between projected source anchor and surface | Collision-free route geometry and handoff target | Grid navigation or Camera ownership |
| Context menu | Information-local command surface opened from a source gesture | Command ordering, focus, dismissal, and handoff | Direct feature mutation or global keyboard ownership |

## Dependency direction

```text
Typed source reference ──> source-owned route ──> Information anchor
Field derivation ────────> annotation candidate ──> ledger resolver
Ledger edition ──────────> reader layout ─────────> local surface render
Input coordinator ───────> surface coordinator ───> Information lifecycle
Camera snapshot ─────────> Grid projection only
HUD request ──────────────> HUD substrate only
```

| Allowed dependency | Forbidden dependency |
| --- | --- |
| A feature requests a named Information surface with a typed source reference and local anchor. | A feature mounts an Information element directly or writes another feature's state. |
| The coordinator validates plane order, anchor, lifecycle, focus, and close behavior. | A surface changes plane order because it opened later. |
| The ledger resolver consumes candidate records and explicit available space. | The resolver chooses a family from source count alone or from a hardcoded approximation. |
| The route resolver consumes local obstacles and the bridge-projected source point. | The route resolver reads Camera or Grid state. |
| A field selector reads all reachable Layers and preserves deep sources. | A finite layer cutoff rejects deep data. |
| Context-menu commands dispatch through named application commands. | Context-menu code mutates shared state or bypasses command routing. |
| HUD receives viewport-local requests independently. | Information reads or writes HUD DOM, z-order, or viewport state. |

## Slice map

| Slice | Name | Primary contract owners | Required result |
| --- | --- | --- | --- |
| IP-01 | Routed Information substrate | `src/surfaces/contracts.ts`, `src/surfaces/coordinator.ts`, `src/surfaces/router.ts`, `src/surfaces/input.ts`, Information host | One registered coordinator owns open, close, focus, z-order, anchors, Escape order, Delete dispatch, and return focus. |
| IP-02 | Local surface placement | Information host, surface definitions, image viewer, anchor editor, trace session, content menu | Every Information surface receives one explicit local anchor and remains fixed in Information coordinates for its lifetime. |
| IP-03 | Unbounded field engagement | `src/domain/field.ts`, `src/domain/annotation.ts`, field selectors, Blip owner | All reachable Layers contribute to derived field support; render floors limit paint only; marker engagement remains source-local and camera-blind. |
| IP-04 | Blip and escalator route | `src/features/annotations/annotation-reading.ts`, route resolver, Information CSS | Markers wait for pointer or Grid-cursor engagement, remain outside occupied Content where possible, and use collision-free orthogonal routes with no viewport promotion. |
| IP-05 | Ledger and reader resolver | `src/domain/annotation.ts`, `Annotation-ledger.md`, `Content physical geometry.md`, vertical component contracts | Nine forms and all 32 layout families resolve from physical content facts, complete-fit predicates, readable/fidelity floors, available dimensions, source order, and source availability. |
| IP-06 | Reflow, recovery, and context chrome | annotation reader, pinning, pagination, context menu, global input | Pagination, density-preserving reflow, hysteresis, unavailable retention, source ordering, pinning, source-owned route handoff, context-menu grammar, plane isolation, and keyboard recovery are executable. |

## Slice boundaries

### IP-01 — Routed Information substrate

| Acceptance | Proof |
| --- | --- |
| Information and HUD have stable independent plane roots and fixed z-order. | Surface coordinator assertion and plane-isolation browser journey. |
| Information opens only through the registered coordinator/router seam. | Static owner check and route journey for every current Information surface. |
| An Information request requires a plane, named surface, typed source or intent, local anchor, and lifecycle. | Contract tests for valid and refused requests. |
| Camera, Grid cursor, Layer, and Grid selection changes do not mutate an open Information anchor. | Camera/Layer/cursor isolation journey with anchor snapshot assertion. |
| `Escape` closes the current Information thing first and clears the next eligible layer on the second tap. | Global keyboard journey with focus and surface assertions. |
| `Delete` dispatches the canonical delete command in every active Information environment. | Context-menu, reader, and local chrome keyboard journeys. |
| Closing returns focus to the source-owned origin without creating HUD content. | Open/close handoff journey for each source route. |

### IP-02 — Local surface placement

| Acceptance | Proof |
| --- | --- |
| Annotation reader, image viewer, anchor editor, trace session, and context menu receive explicit local anchors. | Surface request matrix and browser capture. |
| No Information surface defaults to viewport center when a source anchor is required. | Refused-request test and DOM geometry assertion. |
| Local surfaces follow their exact source during pan and zoom without scaling their chrome. | Source-projection journey with before/after source and surface rectangles. |
| Local surfaces do not move into HUD or outrank HUD when opened later. | Three-plane stacking journey. |
| Local surfaces preserve source-local z-order and do not cover the source route's focus target without a collision resolution. | Anchor placement and focus journey. |

### IP-03 — Unbounded field engagement

| Acceptance | Proof |
| --- | --- |
| Layer 15 and Layer 16+ sources contribute to derived field support. | Domain fixture with arbitrary Layer depth. |
| Field falloff can reduce rendered contribution without rejecting durable data. | Render-floor fixture plus persistence invariant. |
| Cross-Layer contributions use the same cell-based support calculation for every Layer. | Field matrix across same-Layer, adjacent-Layer, and deep-Layer sources. |
| Field rendering remains below authored Content and accepts no pointer input. | Draw-order and pointer pass-through assertions. |
| A field change does not move an open Information surface. | Field mutation plus Information anchor snapshot journey. |

### IP-04 — Blip and escalator route

| Acceptance | Proof |
| --- | --- |
| Rest has no marker or cue; pointer or Grid-cursor engagement raises one candidate cue. | Blip state journey. |
| A marker is placed outside the complete occupied footprint where local space permits. | Footprint collision matrix including rectangular Placements. |
| Marker coordinates are bridge-projected Information coordinates and follow their exact source. | Source-projection assertion. |
| The escalator route is orthogonal, collision-free, and terminates at the cue edge without a terminal dot. | Route geometry property tests and visual capture. |
| A marker never becomes a Grid Placement, selection target, or HUD surface. | Input and plane-isolation journey. |
| Unavailable candidates disappear according to the ledger contract without closing an already-open reader. | Unavailable recovery journey. |

### IP-05 — Ledger and reader resolver

| Acceptance | Proof |
| --- | --- |
| Text-led resolves Bulletin, Berliner, and Broadsheet. | Three-form resolver matrix. |
| Mixed-media resolves Brochure, Pamphlet, and Magazine. | Three-form resolver matrix. |
| Image-led resolves Gallery, Contact sheet, and Image edition. | Three-form resolver matrix. |
| Exactly 32 layout families are registered and reachable. | Family registry count and exhaustive resolver test. |
| Family choice uses complete-fit predicates over demand, source count, image share, and available dimensions. | Boundary matrix around every family threshold. |
| Family choice preserves the `34ch`/`283px` readable text floor and every role-specific `PPI_eff` floor. | Density/fidelity matrix across narrow, ordinary, and wide Information units. |
| A form never truncates authored content to fit a family. | Oversize-content refusal and reflow journey. |
| Ten mixed-resolution images are evaluated by intrinsic dimensions, aspect, role, and placed fidelity before aggregation. | Ten-image physical fixture with 4K, 1080p, 720p, panorama, portrait, square, and mixed-role sources. |
| Source ordering is stable and source-owned. | Mutation-order and reload journey. |

### IP-06 — Reflow, recovery, and context chrome

| Acceptance | Proof |
| --- | --- |
| Pagination exposes complete pages and preserves the current source when pages reflow. | Pagination journey across all verticals and forms. |
| Reflow uses an edition boundary and never shows a blank or partial page. | Reflow interruption and reduced-motion journey. |
| Hysteresis prevents family thrashing near a boundary. | Resize oscillation property test. |
| Unavailable sources retain identity and show the canonical recovery state. | Source removal and recovery journey. |
| One reader remains source-relative until Close, Escape, or explicit replacement and never creates a viewport return control. | Reader lifetime journey under Camera and Layer changes. |
| Context menu uses ordered commands, focus return, keyboard navigation, refusal, and source-owned handoffs. | Context-menu journey with arrows, Home, End, Enter, Escape, Delete, and unavailable commands. |
| Every Information action is reachable through the canonical keybind registry. | Keybind coverage check and keyboard journey matrix. |

## Vertical build order and closure

The shared physical resolver is completed before any vertical renderer is
accepted. Each vertical is a separate implementation round. The shared
interaction model is integrated after each renderer has a complete reader
surface and before the round is closed.

| Round | Slice | Prerequisites | Required result |
| --- | --- | --- | --- |
| 1 | IP-05A — Physical resolver and density gate | IP-01 through IP-04; `Content physical geometry.md` | Deterministic source records, physical demand, complete-fit candidates, readable text floors, image fidelity floors, family predicates, and page capacity. |
| 2 | IP-05B — Text-led vertical | IP-05A | Bulletin, Berliner, and Broadsheet; all 9 families; body-first broadsheet pagination; no generated headline. |
| 3 | IP-05C — Mixed-media vertical | IP-05A | Brochure, Pamphlet, and Magazine; all 12 families; complete image frames; page-local media balance; text/image region allocation. |
| 4 | IP-05D — Image-led vertical | IP-05A | Gallery, Contact sheet, and Image edition; all 11 families; aspect-preserving frames; resolution-bounded scale; sequence pagination. |
| 5 | IP-06A — Source-owned routing and interaction integration | IP-05B through IP-05D | Blip engagement, escalator route, local reader anchor, source selection, source-owned context menu, named destination route, pin/return, and plane isolation operate across all verticals. |
| 6 | IP-06B — Reflow, recovery, and keyboard closure | IP-06A | Edition boundary, hysteresis, unavailable recovery, source ordering, current-source retention, Escape order, Delete dispatch, context-menu refusal, and reduced-motion behavior are closed across all forms and families. |

### IP-05A — Physical resolver and density gate

| Acceptance | Proof |
| --- | --- |
| Every source receives a physical record before vertical or family selection. | Text/image physical-record fixture matrix. |
| A candidate is invalid below `34ch`/`283px` text measure. | Narrow-width rejection matrix. |
| A candidate is invalid below the assigned image role fidelity floor. | PPI floor matrix for thumbnail, supporting, and hero roles. |
| Intrinsic aspect controls frame geometry without crop, stretch, or portrait-page substitution. | Aspect matrix for panorama, landscape, square, and portrait sources. |
| `Rmedia` remains page-local and cannot change the routed vertical, tier, or form. | Mixed-media page-balance re-route refusal. |
| Page count increases before readable measure or image fidelity is reduced. | Density objective fixture with increasing source demand. |

### IP-05B — Text-led vertical

| Acceptance | Proof |
| --- | --- |
| Bulletin selects T1-A, T1-B, or T1-C from physical column units and complete-fit predicates. | T1 family boundary matrix. |
| Berliner selects T2-A, T2-B, or T2-C from leaf capacity, source continuation, and independent source blocks. | T2 family boundary matrix. |
| Broadsheet selects T3-A, T3-B, or T3-C from column capacity and threading predicates. | T3 family boundary matrix. |
| Broadsheet requires no title, headline, masthead, issue line, standfirst, group heading, or summary. | Missing-title and authored-heading journey. |
| Text flows through columns and pages without truncation, clipping, ellipsis, or synthetic filler. | Long-source pagination journey. |
| A column is never accepted below the readable measure floor. | Width-resize journey across all 9 families. |

### IP-05C — Mixed-media vertical

| Acceptance | Proof |
| --- | --- |
| Brochure selects B-01 through B-04 from source count, hero/reveal demand, panel capacity, and complete frames. | B-family predicate matrix. |
| Pamphlet selects P-01 through P-04 from text/image pairing, leaf capacity, and stitched continuation. | P-family predicate matrix. |
| Magazine selects M-01 through M-04 from opener, mixed spread, multi-image, and threaded pagination predicates. | M-family predicate matrix. |
| Text and image regions preserve their own physical floors. | Mixed density/fidelity matrix. |
| A page-local media balance never re-routes the source set. | `Rmedia` mutation journey. |
| Fold, leaf, page, and spread geometry remain complete and ordered. | Form progression and pagination journey. |

### IP-05D — Image-led vertical

| Acceptance | Proof |
| --- | --- |
| Gallery selects I1-A, I1-B, or I1-C from image count, caption/facts presence, and complete-frame fit. | I1 family boundary matrix. |
| Contact sheet selects I2-A, I2-B, or I2-C from aspect-class consistency, frame fit, and facts-rail requirement. | I2 aspect and facts matrix. |
| Image edition selects I3-A through I3-E from image count, supporting text, spread capacity, and sequence pagination. | I3 family boundary matrix. |
| Panoramas receive the widest permitted complete region and reduce row/page source count before frame distortion. | Panorama density journey. |
| Portraits, squares, and landscapes retain intrinsic proportions in a landscape reading unit. | Aspect-preservation journey. |
| 4K sources are not enlarged beyond their effective fidelity floor and 720p sources do not occupy hero-scale frames when the floor fails. | Mixed-resolution fidelity journey. |

### IP-06A — Source-owned routing and interaction integration

```text
field support
→ candidate Blip
→ pointer or Grid-cursor engagement
→ local escalator route
→ local Annotation reader
→ source selection
→ source-owned route menu
→ named destination
→ local return or one-reader pin
```

| Acceptance | Proof |
| --- | --- |
| Blips do not force a reader or route into the viewport. | Rest/Approached journey. |
| Blip and reader anchors follow their exact sources during Camera changes without scaling. | Source-projection journey. |
| The escalator route remains outside occupied Content where local space permits and never becomes a Grid or HUD target. | Footprint collision and plane-isolation matrix. |
| The reader is opened beside the gathered source group, not at viewport center. | Local-anchor geometry journey. |
| Route menus are source-owned and expose only destinations valid for the selected source. | Source route matrix across all verticals. |
| The open reader preserves source-relative locality under pan, zoom, and Layer change. | Reader locality journey. |
| Information remains below HUD regardless of opening order. | Three-plane stacking journey. |

### IP-06B — Reflow, recovery, and keyboard closure

| Acceptance | Proof |
| --- | --- |
| The last complete edition remains visible during physical recalculation. | Interrupted reflow journey. |
| Boundary equality retains the current family/form; crossing requires complete-fit re-resolution. | Oscillating threshold property test. |
| Unavailable sources retain identity, position, provenance, and committed form after qualification. | Source loss/recovery matrix. |
| Active source remains selected when present in the replacement edition. | Reflow selection-retention journey. |
| `Escape` clears the current Information thing on the first tap and the next eligible layer on the second. | Global keyboard sequence journey. |
| `Delete` dispatches the canonical delete command in every Information environment. | Reader, route, context-menu, and local-editor Delete matrix. |
| Context-menu refusal leaves the complete reading intact and returns focus to the source-owned origin. | Unavailable-destination journey. |
| Reduced motion preserves the same complete page, frame, route, and keyboard states. | Reduced-motion journey across all three verticals. |

## Round gate

Every slice is a separate round.

| Order | Gate | Result |
| --- | --- | --- |
| 1 | Slice tests and repository verification | Exact commands and failures recorded. |
| 2 | Specification review | Every changed owner is checked against this contract and its component source. |
| 3 | Installer rebuild | NSIS and MSI rebuilt; installation is not performed. |
| 4 | Artifact check | Installer metadata matches the active application version. |
| 5 | Scope check | Only slice files are staged; unrelated dirty work remains untouched. |
| 6 | Commit | Commit names the slice and verified result. |
| 7 | Push | Commit is pushed to the selected branch. |
| 8 | Handoff | Remaining unproved rows and blockers are listed before the next slice. |

## Source owners

| Source | Role |
| --- | --- |
| `design-system/20-planes/Information-plane.md` | Plane ownership, coordinate relationship, and chrome boundary. |
| `design-system/10-grammar/Annotation-ledger.md` | Ledger record, nine forms, nine states, and re-resolution. |
| `design-system/10-grammar/Content-concentration.md` | Demand, fidelity, gate, tier, and family inputs. |
| `design-system/10-grammar/Content physical geometry.md` | Physical source records, density objective, family predicates, readable/fidelity floors, and page-break rules. |
| `design-system/30-components/Annotation.md` | Reader shell, page geometry, state, and refusal contract. |
| `design-system/30-components/Blip.md` | Marker, cue, local engagement, and route boundary. |
| `design-system/30-components/Annotation-routes.md` | Source-owned route and handoff contract. |
| `design-system/90-conformance/Annotation decisions.md` | Closed annotation decisions and evidence obligations. |
| `design-system/20-planes/HUD-plane.md` | HUD separation and viewport-fixed boundary. |
