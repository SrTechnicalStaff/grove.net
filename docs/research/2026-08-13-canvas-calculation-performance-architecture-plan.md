# Canvas Calculation and Rendering Architecture Hard-Cutover Plan

Date: 2026-08-13  
Status: Proposed implementation plan  
Scope: Grove desktop application, with Plane 0 as the first and strictest performance target

## What this plan accomplishes

Grove will stop treating the viewport as the source of truth for the Grid. Content, occupancy, Aura, cross-Grid-Layer saturation, same-Grid-Layer contours, selection evidence, and collision state will exist in a camera-independent world snapshot. Panning and zooming will only transform and submit already-current state for drawing. Nothing will require a click, selection change, zoom nudge, or other user action to become current.

The current rebuild-and-redraw loop will be replaced end to end. Content mutations will update a spatial index and only the affected field chunks; rendering will consume one immutable revision, cached text/image representations, and batched Skia geometry. The permanent animation callback, all-Content scans, all-field rebuilds, per-cell dictionaries, per-cell Avalonia draw calls, render-time parsing/reflow, and viewport-owned Aura sets will be deleted rather than retained as compatibility paths.

Completion will be demonstrated in the real release application. Automated mutation-to-frame revision checks, zoom/pan edge-continuity image comparisons, allocation and CPU traces, and interactive release-build recordings will prove that state is immediately observable and remains continuous from 1% through 1000% zoom. A successful build alone is not acceptance.

## Binding architecture rules

1. **World truth is camera-independent.** Viewport bounds, zoom, pan, window size, and render scaling cannot create, remove, recalculate, or truncate world state.
2. **Projection is not simulation.** A screen clip may prevent physically invisible pixels from reaching the GPU. It may not limit Content queries, Aura support, contour topology, selection evidence, metadata, collision, or Memory Search evidence.
3. **Every mutation publishes a coherent revision.** The next compositor frame after a committed mutation must contain the corresponding world, field, representation, and selection revisions. Mixed revisions are refused.
4. **No wake-up gesture exists.** There is no public `Refresh...` method and no code path that relies on pointer movement, clicking, selecting, panning, or zooming to reveal current state.
5. **Steady state is idle.** With no animation and no mutation, Grove requests no animation frames, performs no field work, performs no scene preparation, and updates no HUD text.
6. **Render is read-only.** Drawing consumes immutable prepared data. It cannot parse text, reflow documents, decode images, rebuild field topology, mutate a ledger, or scan the complete Content collection.
7. **One model per concept.** There is one camera snapshot, one recursive Grid Tier/cursor model, one Content spatial index, one field snapshot, and one Plane 0 renderer. Compatibility renderers and parallel single-cell paths are deleted at cutover.
8. **Grid-Layer semantics remain intact.** All visible Grid Layers participate; Grid Layer selection is command/projection context, not activation. Only same-Grid-Layer source energy produces contours. Cross-Grid-Layer energy affects Aura saturation and recall evidence only.
9. **Field appearance stays discrete.** Optimization does not introduce gradients, blur, glow, halo, interpolation across cell edges, static Windows-blue contours, or averaged midpoint hue.
10. **No hidden degradation mode.** The application cannot silently lower field range, stop drawing Auras, hide Content, reduce update frequency, or serve stale snapshots to recover frame rate.

## Evidence from the current implementation

The following are architectural causes, not isolated micro-optimizations:

| Current path | Cost and failure mode | Hard-cutover disposition |
| --- | --- | --- |
| `GridCanvasControl.RefreshFieldLedger()` derives a camera rectangle, expands it by six cells, and rebuilds the field from that rectangle | Camera state changes authoritative Aura state; edge chopping and zoom-dependent disappearance are inevitable | Delete the method and all camera-to-field calls |
| Camera settling calls `RefreshFieldLedger()` every animation frame | Full world-derived work is coupled to pan/zoom | Camera frames publish projection only |
| `OnAnimationFrame()` always requests another frame | UI thread, trail checks, metadata counting, and HUD events continue forever | Replace with an event-driven frame scheduler that stops when clean |
| Every prepared Aura cell loops over every Content item | Approximately `O(A × N)`; because Aura cell count `A` grows with `N`, separated Content trends toward `O(N²)` | Source-driven delta deposition into field chunks |
| Every Aura cell creates a dictionary, metadata objects, arrays, GUID formatting, color parsing, and snippets | High allocation rate and garbage collection pressure | Packed numeric field buffers plus separate sparse provenance postings |
| `CurrentVisibleAuraCells` is stored by the field engine | View state contaminates field state | Delete; projection queries immutable world chunks |
| Render scans all visible-Grid-Layer Content and then render modules bounds-check each item | `O(N)` per frame before useful drawing | Content spatial range query returning only intersecting placements |
| Hit testing, marquee, occupancy, and selected-item retrieval repeatedly scan `Items` | High-frequency interactions scale linearly | Shared Content spatial/occupancy index and explicit selection set |
| Group translation expands clusters and occupancy into fresh hash sets on each pointer move | Allocation and work scale with occupied cell count and total candidates | Indexed candidate query plus reusable translation session buffers |
| Field drawing issues one Avalonia fill per cell and rebuilds selected cells and contour edges | Thousands of managed drawing operations and objects per frame | One Skia operation with chunk meshes and cached contour paths |
| Minor Grid lines are segmented cell-by-cell to omit Aura interiors | Work explodes at macro zoom and scales with visible cell area | Batched line paths plus field-cell mask/clip geometry |
| Notes parse/create rich-text layouts during drawing; Documents reflow and create formatted text during drawing | CPU and allocation scale with Content complexity on every repaint | Revision-keyed representation cache prepared outside render |
| Images retain full bitmaps without a representation pyramid | Decode memory and scaling cost are unrelated to projected size | Async decode cache with original-preserving mip/thumbnail representations |
| `Camera.CurrentState` recreates transform and inverse matrices on access | Repeated calculation and transient values within one frame | Immutable cached `CameraSnapshot` with a monotonically increasing revision |
| Performance telemetry enumerates metadata and changes text on every animation callback | Instrumentation contributes to the regression it reports | Constant-time counters sampled independently at a low fixed rate |
| The compositor fallback invalidates controls during plane rendering | Potential invalidation cascade and unclear ownership | One explicit frame publication seam; remove invalidate-during-render fallback |

The existing field microbenchmark already demonstrates the shape of the problem. Ten separated 1×1 Content placements produce about 1,130 Aura cells, approximately 2.8 ms of field rebuild time, and about 1.7 MiB of managed allocation per rebuild before normal Content, text, Grid, and compositor drawing. Ten separated 10×10 placements produce about 4,280 Aura cells, approximately 4.1 ms, and about 6.5 MiB per rebuild. The current renderer then submits one managed fill operation per Aura cell. These figures are baseline evidence, not final acceptance measurements.

## Target module design

The replacement uses four deep modules and two adapters. Their interfaces remain small; their internals own the complexity.

### 1. `SpatialWorld`

`SpatialWorld` owns authoritative Grid state:

- Content placement records, indexed by stable Content ID;
- a per-Grid-Layer spatial index for point, rectangle, and intersection queries;
- occupied-cell/chunk data for collision and rigid group translation;
- an explicit selected Content ID set;
- Content-to-Memory references without moving Memory into Grid state;
- mutation sequencing and immutable `WorldSnapshot` publication.

Its external seam is limited to applying a typed `WorldMutation` and reading a `WorldSnapshot`. Add, remove, move, resize, recolor, anchor, Grid-Layer transfer, visibility, and batch translation all go through that seam. Direct `Items.Add`, `Items.Remove`, and public collection mutation are removed.

Point hit testing becomes `O(log N + k)`. Rectangle and marquee queries become `O(log N + k)`, where `k` is the intersecting placement count. Full-footprint marquee acceptance is evaluated only for those candidates. Collision evaluates the target footprint against indexed occupancy and explicitly excludes the moving cluster's source occupancy.

### 2. `AuraField`

`AuraField` owns camera-independent derived field state:

- fixed-size sparse chunks, initially 32×32 Grid cells;
- structure-of-arrays numeric storage for total energy, same-Grid-Layer energy, additive hue channels, alpha, and topology flags;
- source-to-affected-chunk reverse postings for subtraction and selection projection;
- sparse provenance records for semantic queries, separate from the render buffer;
- same-Grid-Layer contour geometry per chunk with neighbor-edge stitching;
- cross-Grid-Layer saturation transfer and its convergence/work queue;
- immutable `AuraSnapshot` publication.

A Content mutation produces one `FieldDelta`: subtract the source's previous support, add its new support, update only touched chunks and their contour neighbors, then publish. No cell loops over all Content. Horizontal influence values, footprint-distance values, vertical transfer coefficients, alpha transfer, and additive channel transfer use lookup tables or contiguous numeric loops. `Span<T>`, pooled scratch buffers, and measured SIMD are implementation tools inside the module; they are not exposed in its interface.

The visual Aura support rule remains a product/physics rule, never a viewport rule. The six-cell support limit is therefore either affirmed by the field definition or replaced by the revised saturation definition; it cannot survive merely as a performance cull. The unbounded but weaker Gap-distance relation used by Memory Search is queried analytically through the spatial index and is not materialized as an infinite visual cell field.

The vertical model must be corrected before optimizing its kernel. Native-layer deposition is proportional to the complete Content footprint. Adjacent-Grid-Layer transfer uses a strong, thresholded falloff so one isolated source cannot visibly wet a large stack, while combined local saturation can cross the threshold and travel farther. Transfer is solved from dirty `(chunk, Grid Layer)` work only, to deterministic convergence, and color channels travel with the contributing energy. This preserves the paper-towel outcome: overlap changes depth and intensity; source proximity to a target Grid Layer matters; same-Grid-Layer contours never propagate vertically.

### 3. `ContentRepresentationCache`

This module turns source data into immutable draw representations before rendering:

- Note rich-text parse/layout keyed by text revision, footprint, representation Tier, and typography revision;
- Document reflow and page glyph runs keyed by AST revision, page, dimensions, and Tier;
- Image decode representations keyed by asset revision, projected size band, color space, and render scaling;
- cached brushes, paints, paths, glyph runs, paper texture paths, anchor marks, and stand-ins;
- bounded memory policy with reference-aware eviction and disposal.

Changing the camera within one representation Tier reuses the same representation. Crossing a Tier requests the canonical adjacent representation without creating a parallel interaction model. The current representation remains valid until the new representation is ready, and promotion occurs in place without blanking. Original source data is never discarded or downsampled; only cached draw representations vary.

### 4. `CanvasFrameScheduler`

The scheduler owns freshness and frame demand. It receives revisioned invalidations from world mutation, field publication, representation readiness, camera change, selection change, HUD change, and active motion. It coalesces them into one frame request and stops requesting frames when no motion or dirty revision remains.

Before a frame is submitted, the scheduler constructs one `CanvasSceneSnapshot` whose dependency revisions are mutually compatible. The snapshot contains the camera transform, projected world rectangle, canonical Grid Tier, intersecting Content handles, intersecting Aura chunks, contour handles, selection handles, and feedback state. Render never reaches back into mutable controls or engines.

If an active animation exists, only that animation's state advances. Cursor trail expiry, placement feedback, and camera interpolation each register and unregister their own frame demand. Performance telemetry is not a frame-demand source.

### 5. `CanvasSceneRenderer` adapter

One Plane 0 Skia adapter consumes `CanvasSceneSnapshot`:

- applies one camera matrix;
- emits Grid tiers as batched paths whose count depends on screen dimensions, not the number of microscopic world cells;
- emits Aura fills by chunk mesh/color buffers rather than managed rectangles;
- suppresses minor Grid lines with discrete Aura mask geometry without segmenting every line through every cell;
- emits same-Grid-Layer contours from cached paths and their Aura-derived colors;
- emits cached Content representations;
- emits selection, canonical recursive cursor, trails, and transient feedback last;
- reuses paints, paths, vertices, and command buffers when their revision is unchanged.

The viewport clip is a final pixel-submission clip. The projection query covers the exact exposed world rectangle plus the screen-space extent required by strokes and in-place transitions. That margin is calculated in device pixels and transformed to world space, so it remains correct from 1% through 1000% zoom. No fixed “two cells outside the viewport” rule exists.

### 6. Avalonia and HUD adapters

Avalonia controls forward input as typed intentions and display published HUD state. They do not own spatial algorithms or field state. `GridCanvasControl` is reduced to a shallow host for input adaptation, snapshot presentation, accessibility, and lifecycle. Gesture sessions, camera commands, placement commands, and selection commands live behind the `SpatialWorld` and scheduler seams.

The performance tracker reads constant-time counters from a diagnostics snapshot at no more than four samples per second. Minimizing it unregisters visual detail and leaves only the approved minimal HUD chip. It never enumerates Content or field metadata and never drives rendering.

## Implementation sequence

Each slice has a hard entry gate, a hard exit gate, and a deletion list. A slice is not complete while its old path remains callable.

### Slice 0 — Correct the contracts and install red performance/freshness tests

1. Revise ADR-002, ADR-003, ADR-005, ADR-041, ADR-050, and the Field Ledger/Aura domain definitions to state the binding rules above.
2. Remove language that treats a zoom-aware viewport envelope as field truth. Replace it with camera-independent field state and projection-only clipping.
3. Specify the thresholded vertical saturation transfer with executable examples: isolated source depth, overlapping-source depth, footprint proportionality, strong falloff, same-Grid-Layer contours, and additive hue.
4. Add benchmark scenes for 0, 10, 100, 1,000, and 10,000 placements; overlapping and separated Auras; 1×1 through large footprints; 1, 20, and 100 Grid Layers; image-heavy, Note-heavy, and Document-heavy scenes.
5. Add red revision tests proving that mutation, selection, Grid-Layer movement, visibility, and representation updates are visible on the next eligible frame without input.
6. Capture `dotnet-counters`, `dotnet-trace`/PerfView, allocation, UI-thread, render-thread, draw-operation, and screenshot baselines from both `dotnet run -c Release` and the published executable.

Exit gate: the tests reproduce stale refresh, viewport chopping, `O(A × N)` field work, render allocations, and the 145-to-20 FPS regression.

### Slice 1 — Introduce `SpatialWorld` and cut over all spatial queries

1. Build typed world mutations and immutable revisioned snapshots.
2. Add the per-Grid-Layer Content spatial index and chunk occupancy index.
3. Route add/remove/move/resize/anchor/Grid-Layer transfer/visibility through one mutation seam.
4. Cut hit testing, marquee, selection, collision, drop placement, resize validation, group translation, frame-all, and render candidate lookup to the index.
5. Make group drag a provisional transform over one captured translation session; commit one atomic batch mutation when the cell delta changes or the gesture ends.
6. Delete linear helper paths, direct collection mutation, duplicate occupancy logic, and per-pointer-move hash-set expansion.

Exit gate: 10,000 non-intersecting offscreen placements do not change point-hit, collision, marquee, or current-viewport candidate complexity beyond index traversal.

### Slice 2 — Replace Field Ledger rebuilds with `AuraField` deltas

1. Implement packed field chunks and sparse provenance postings.
2. Implement source-driven deposition and subtraction for old/new support.
3. Implement footprint-proportional native energy and the thresholded adjacent-Grid-Layer saturation solver.
4. Build additive display hue and alpha transfer in contiguous numeric passes.
5. Build same-Grid-Layer contour updates only for dirty cells plus one-cell topology neighbors.
6. Build selected-source projection from reverse postings, with an immediate empty-selection fast path.
7. Publish immutable Aura revisions and notify the scheduler once per commit.
8. Delete `RecalculateField`, `RefreshFieldLedger`, `GetAuraCells` as a viewport helper, `CurrentVisibleAuraCells`, per-cell dictionaries/arrays, render-time contour scans, and camera-triggered field work.

Exit gate: pan, zoom, resize, window movement, and render scaling cause exactly zero Aura calculations; moving one placement touches only its old/new support and neighboring topology chunks.

### Slice 3 — Cut over to immutable scene snapshots and demand-driven frames

1. Implement revision dependency checks and one-frame coalescing.
2. Replace the permanent `RequestAnimationFrame` recursion with explicit animation registrations.
3. Cache one `CameraSnapshot` per camera revision, including transform and inverse.
4. Remove unconditional pointer-move invalidation; invalidate only when cursor cell, hover target, gesture preview, or camera state actually changes.
5. Remove any invalidate-during-render compositor behavior.
6. Decouple diagnostics sampling from frame demand.

Exit gate: an untouched scene produces zero callbacks and zero paints after settling; every committed mutation appears in the immediately scheduled coherent frame.

### Slice 4 — Replace managed per-cell drawing with the Skia scene adapter

1. Build reusable chunk meshes for Aura fills and selected contribution marks.
2. Build cached, stitchable contour paths colored from same-Grid-Layer Aura channels.
3. Replace per-cell Avalonia fills, brushes, and draw operations with batched Skia draws.
4. Replace per-cell Grid-line segmentation with batched tier paths and discrete field mask geometry.
5. Make custom draw-operation equality revision/value based so unchanged scene data is reusable.
6. Remove the Avalonia compatibility Grid renderer and all duplicate render paths.

Exit gate: after warm-up, stationary rendering allocates zero managed bytes; draw-operation count is bounded by visible chunks/passes rather than Aura cell count.

### Slice 5 — Move Content preparation out of render

1. Cache Note parsing/layout and its glyph-ready representation.
2. Cache Document reflow, page slices, formatted text/glyph runs, texture paths, and stand-ins.
3. Add image metadata probing, decode-size selection, thumbnail/mip cache, and disposal outside render.
4. Cache immutable paints, brushes, geometry, and token-derived colors.
5. Ensure canonical recursive Grid Tier changes representation detail only; interaction and cursor cell addressing remain one model.
6. Delete render-time `CreateLayout`, `Reflow`, `FormattedText` construction, `Color.Parse`, per-item brush/pen construction, and repeated paper-grid path construction.

Exit gate: repainting an unchanged scene performs no parsing, layout, reflow, image decode, or representation allocation.

### Slice 6 — Apply the same calculation discipline to HUD Plane Slates, search, and hydration

1. Make Memory Slate masonry/river layout incremental and keyed by Memory revision and viewport width.
2. Use a spatial interval/indexed realization model for river items while keeping Memory records and layout truth independent of scroll position.
3. Predecode upcoming image representations and retain the currently displayed representation during promotion; no blank tiles or user-triggered refresh.
4. Make search indexing incremental by Memory revision rather than rebuilding on every keystroke.
5. Cache Writing Slate document layout and pagination by document/editor revision.
6. Keep UI virtualization a materialization optimization only; it cannot alter Memory, result ordering, layout geometry, or scroll extent.
7. Bulk-load persisted Content placements into `SpatialWorld` once at startup, build the spatial and occupancy indexes from that batch, and derive one coherent Aura revision before the Grid becomes interactive.
8. Keep Memory hydration independent of Content placement hydration. Rebuilding Grid-derived state cannot create, delete, or alter a Memory record.
9. Move file reads, image probing/decoding, and persistence writes off the UI thread. Publish their results through typed revisioned mutations and schedule the resulting frame directly.
10. Coalesce persistence writes by committed world revision without delaying the in-memory world commit or its visible frame.

Exit gate: application launch, opening, resizing, scrolling, searching, returning to a Slate, and reopening a persisted Grid never show stale results or require a second action to populate content.

### Slice 7 — Delete the old architecture and verify the release journey

Delete or collapse:

- the monolithic calculation and gesture portions of `GridCanvasControl`;
- `RefreshFieldLedger` and all callers;
- full-rebuild Field Ledger entry points;
- `CurrentVisibleAuraCells` and other viewport-owned field state;
- linear Content lookup helpers and duplicate occupancy implementations;
- perpetual frame requests and HUD-per-frame events;
- compatibility Grid and Content renderers;
- render-time rich-text/document/image preparation;
- stale active/inactive Grid-Layer render vocabulary and code;
- any second single-cell cursor/action model;
- comments that preserve migration or conversation history instead of explaining durable invariants.

Then run a two-axis review: repository standards and originating product/spec outcomes. Any compatibility branch, hidden refresh trigger, duplicate model, viewport-owned semantic state, or unverified claim reopens the slice.

## Non-negotiable acceptance matrix

### Freshness

- Add, delete, move, resize, recolor, anchor, Grid-Layer transfer, visibility change, selection, and multi-selection translation appear on the next eligible frame without any unrelated input.
- Camera movement changes only the camera/scene projection revision. Aura and world revisions remain unchanged.
- Representation completion schedules its own frame; it never waits for pointer or keyboard input.
- Published scene snapshots contain matching dependency revisions; debug builds fail fast on a mixed snapshot.

### Viewability

- Aura remains visible and continuous at 1%, 2%, 6%, 10%, 24%, 50%, 100%, 500%, and 1000% zoom.
- Automated pan paths cross every viewport edge and corner with Content and Aura straddling the boundary; image comparisons show no chopped support, blank frame, or pop-in.
- A rapid pan over thousands of cells draws the newly exposed state on the first frame because world/field truth already exists.
- Same-Grid-Layer contours never appear on another Grid Layer. Cross-Grid-Layer fill saturation remains present according to the field model.
- Overlap strengthens saturation and can extend propagation depth; an isolated source exhibits the specified strong falloff.
- Aura geometry grows from actual Content footprint rather than a flat minimum by Content kind.

### Complexity and allocations

- Field mutation complexity is proportional to changed source support and touched Grid Layers/chunks, not total Content count.
- Hit testing and range queries are index-based; no high-frequency path scans the complete `Items` collection.
- Stationary and camera-only frames perform zero field recomputation.
- Warm unchanged frames allocate zero managed bytes in Plane 0 preparation and drawing.
- HUD metric collection is constant time and cannot enumerate field provenance.
- 10,000 offscreen placements do not materially increase current-viewport draw preparation or draw command count.

### Frame budgets

Budgets are measured on the user's machine and recorded with hardware, resolution, render scaling, and scene fixture:

- 144 Hz target: p95 CPU frame preparation plus submission at or below 6.94 ms for the 10- and 100-placement interaction fixtures;
- 60 Hz floor: p99 at or below 16.67 ms for the 1,000-placement navigation fixture;
- drag pointer-to-visible-transform latency: at most one compositor frame;
- mutation-to-current-Aura latency: at most one compositor frame for bounded source support;
- zero Gen 0 collections during a 30-second warm pan/zoom/drag run;
- no frame-rate recovery mechanism may violate the freshness or viewability gates.

If hardware cannot meet a budget, the failure remains visible and blocks release. The test may not pass by shrinking field support, hiding Content, dropping Grid Layers, lowering correctness, or serving an old snapshot.

### End-to-end proof

Acceptance evidence must include:

1. before/after `dotnet-counters` captures for CPU, allocation rate, GC count, and working set;
2. before/after `dotnet-trace` or PerfView call trees for field mutation, pan/zoom, drag, and Slate scroll;
3. benchmark output with operation counts, touched chunks, field cells, scene candidates, allocations, and percentile timings;
4. release-build screenshots or recordings in which the Grove window, invoked feature, input action, and visible outcome are all present;
5. automated image comparisons for viewport-edge and zoom continuity;
6. automated revision logs proving mutation-to-frame propagation without wake-up input;
7. a final code review proving the old paths are deleted and the product outcomes are observable.

## Primary-source guidance used

- Microsoft recommends beginning with runtime counters and then using trace/profiling tools such as `dotnet-trace` and PerfView to identify CPU call paths rather than guessing: [dotnet-counters](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-counters), [debug high CPU usage](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/debug-highcpu), and [.NET diagnostics overview](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/).
- Microsoft documents lighter-weight batched drawing structures, geometry consolidation, image sizing, and minimizing repeated pixel work in its desktop 2D performance guidance: [Optimizing Performance: 2D Graphics and Imaging](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/optimizing-performance-2d-graphics-and-imaging) and [Taking Advantage of Hardware](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/optimizing-performance-taking-advantage-of-hardware). The WPF object types are not copied into Avalonia; the supported architectural principles are applied through Avalonia's own Skia seam.
- Avalonia explicitly advises keeping `Render` fast and allocation-light, reusing pens/brushes/formatted text, invalidating only when data changes, and using `ICustomDrawOperation` for complex Skia scenes: [Avalonia custom rendering](https://docs.avaloniaui.net/docs/graphics-animation/custom-rendering) and [`ICustomDrawOperation`](https://docs.avaloniaui.net/api/avalonia/rendering/scenegraph/icustomdrawoperation).
- .NET provides contiguous-buffer and reuse mechanisms appropriate for the field hot path: [`Span<T>`/`Memory<T>` usage guidance](https://learn.microsoft.com/en-us/dotnet/standard/memory-and-spans/memory-t-usage-guidelines), [`ArrayPool<T>`](https://learn.microsoft.com/en-us/dotnet/api/system.buffers.arraypool-1?view=net-9.0), and [SIMD/hardware intrinsics](https://learn.microsoft.com/en-us/dotnet/standard/simd). SIMD is admitted only after benchmarks prove a gain.
- Hot-path instrumentation must itself be cheap and measured: [.NET metrics instrumentation](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/metrics-instrumentation).
