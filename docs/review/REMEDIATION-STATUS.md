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
- Memory payload extraction is behind `GridContentMemoryPayloadAdapter`; the
  memory version delta now uses a Myers shortest-edit path; spatial-index
  viewport queries and asynchronous anchor YAML persistence are wired.
- The roadmap and ADR-071 checklist no longer claim that absent implementations
  are complete. RCA files are explicitly historical.

## Deliberately partial or refused

- ADR-002/003 continuous contour extraction and the five-tier GPU representation
  pipeline remain unimplemented. The current renderer is intentionally discrete
  because the Presence contract refuses gradient/halo effects; ADR-061 remains
  conflicting/refused.
- Standalone Mica hosting, a full Skia GPU pipeline, document page-turn/column
  controls, image aspect-lock/alignment controls, and the specified resize
  hatch/cursor affordances remain bounded follow-up work.
- ADR-071's `FlashSweepDrawOperation`/Gaussian sweep is not being introduced:
  it conflicts with the explicit discrete-presence refusal. The current
  discrete insertion feedback is the verified implementation.

The build and publish gates are the authoritative verification for this pass:
`dotnet build` and `dotnet publish` both complete with zero warnings/errors.
