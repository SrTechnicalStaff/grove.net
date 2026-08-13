# Current remediation status

This is the current companion to the historical RCA ledgers. It records what
was actually changed in the working tree and what remains intentionally
partial after cross-referencing the ADRs.

## Closed in this pass

- Image drops now transfer decoded bitmap ownership to `GridImage`; rejected
  or colliding placements dispose the temporary bitmap. Decode failures are
  reported through the drop boundary and never receive fabricated dimensions.
- Image hover/card overlays were removed from the picture surface.
- Quick Note and Local Editor have separate seams. Quick Note retains its
  capture identity/header/hint/feed anatomy; Local Editor remains a
  headerless editor for an existing placement. Their keyboard contracts no
  longer overlap.
- Engine key routing now depends on `IKeybindHost`, not concrete controls.
- Raw typography and shadow literals called out by the standards review now
  resolve through design-system tokens.
- Context-menu dismissal, long-press opening, paste/open/delete commands, and
  empty-cell disabled actions are wired.
- Layer Solo/isolation (`Ctrl+I`) suppresses inactive aura/content/ghost
  rendering. Layer rows now expose token-backed color swatches and explicit
  transfer confirmation wording.
- Perimeter rendering now emits only exposed edges (no interior cross-lines),
  annotation metadata records the ADR qualification thresholds, and invalid
  resize attempts show a discrete refusal hatch/strip.
- Memory is a distinct immutable ledger record from its placed Content
  instances. Content carries a shared `MemoryId` and an independent `AnchorId`;
  trace, structured copy/paste, edit, move, resize, layer transfer, and delete
  paths preserve that distinction. Memory version deltas use a Myers
  shortest-edit path, spatial anchors index full footprints, and viewport
  queries use the R-tree/tile-cache seam. Canonical per-record payload
  persistence, Grid Layer stack restoration, startup hydration, and shutdown
  flushing are wired.
- The roadmap and ADR-071 checklist no longer claim that absent implementations
  are complete. RCA files are explicitly historical.
- The canonical cursor model now has one descriptor resolver for passive,
  footprint, armed-tool, and drop-preview cursors; recursive LOD is selected
  from zoom thresholds and the spent trail stores complete world footprints.
- Pan gesture state is isolated in `CanvasPanInteraction`; the canvas remains
  the Avalonia adapter that applies camera results and raises product events.
- Resize geometry mutation is polymorphic on `GridContentItem`, so the canvas
  no longer reaches into concrete Note, Document, or Image dimension fields.
- Resize handles now expose the matching native diagonal cursor while hovered
  and return to the hidden grid cursor everywhere else.

## Deliberately partial or refused

- ADR-002/003 continuous contour extraction and the five-tier GPU representation
  pipeline remain unimplemented. The current renderer is intentionally discrete
  because the Presence contract refuses gradient/halo effects; ADR-061 remains
  conflicting/refused.
- Standalone Mica hosting, a full Skia GPU pipeline, document page-turn/column
  controls, image aspect-lock/alignment controls, and the specified standalone
  resize draw-operation seam remain bounded follow-up work.
- ADR-071's `FlashSweepDrawOperation`/Gaussian sweep is not being introduced:
  it conflicts with the explicit discrete-presence refusal. The current
  discrete insertion feedback is the verified implementation.

The build and publish gates are the authoritative verification for this pass:
`dotnet build` and `dotnet publish` both complete with zero warnings/errors.
