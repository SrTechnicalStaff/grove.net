---
type: canonical-domain-concept
status: accepted
authority: source-of-truth
concept: Aura
date: 2026-08-12
---

# Aura

## Definition

Aura is the spatial field emitted by Content into surrounding Grid cells. It
expresses the presence and influence of placed work without changing the
Memory record behind that Content.

Content is the source. Memory identity and payload are available as source
metadata, but a Memory with no Content emits no spatial Aura.

## Field behavior

Aura contribution is derived from Content mass, cell distance, footprint, and
Grid Layer permeability. Multiple Content sources may contribute to one cell.
The cell stores normalized field evidence and source provenance rather than
mutating the sources.

Aura overlap and saturation are strong evidence that Content belongs to a
shared local working context. They are therefore inputs to spatial recall.

Color composition is additive by source contribution. The renderer sums the
source hues weighted by their field energy and projects the resulting sum into
the display range; it does not calculate a midpoint or blend two sources into
an invented third midpoint color. A contour uses the resulting Aura color for
the same-Grid-Layer field it outlines.

## Visual and semantic roles

Aura has two consumers:

- the Grid renderer, which shows discrete presence in cells;
- Memory Search, which uses overlap and saturation as spatial ranking
  evidence.

The visual representation may be limited by viewport rendering rules. Semantic
recall must not inherit those visual cutoffs merely because a source is outside
the current viewport.

Presence is rendered as discrete cell-bounded field marks. A smooth gradient,
blur, halo, or glow that crosses cell boundaries is not an Aura representation.

## Grid Layer boundary

Aura may saturate cells across Grid Layers through permeability. A same-Grid-
Layer contour may be drawn only from same-Grid-Layer Content geometry and
energy. Its stroke takes the Aura color of that same-Grid-Layer field. Cross-
Grid-Layer saturation can affect presence and recall evidence, but cannot
create, extend, merge, or recolour a same-Grid-Layer outline.

## Boundary decisions

| Tension | Decision |
| --- | --- |
| Aura vs Memory | Aura belongs to Content's spatial manifestation, not Memory existence. |
| Aura vs Content | Content emits Aura; the field does not become Content. |
| Aura vs Gap | Aura overlap is direct evidence; a Gap carries only a weaker distance relation. |
| Aura vs search | Aura contributes ranking evidence and never filters Memory existence. |
