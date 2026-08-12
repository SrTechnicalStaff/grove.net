---
type: design-system-grammar
status: active
date: 2026-08-09
tags: [grove, design-system, representation-tiers]
---

# Representation at distance

Every placement on the Grid holds exactly one of three forms at any moment.
The camera decides which. A person never chooses a form, never sees one
named, and never loses anything by being far away.

Distance is not a state. `States.md` owns the nine states, and every one of
them must be drawable in all three forms below.

## The three tiers

| Tier | What it holds |
| --- | --- |
| Working form | The complete placement: every authored word, its surface treatment, and the chrome that serves the hand. |
| Stepped form | The same footprint with structure only — masses, rules, and blocks standing where type and texture were. |
| Stand-in | One kind-coded block covering the exact footprint. |

Three rungs, no fourth. A representation that fits none of them is a defect
in the placement, not a new tier.

The names are *working form*, *stepped form*, and *stand-in*. The shipped
runtime names the third tier `impostor` internally; that word is code
vocabulary and never reaches a specification, a deck, or the interface.

## What decides the tier

**Projected cell size** — `--grid-cell` multiplied by the camera scale. One
number for the whole field, so every placement at the same distance changes
form at the same moment.

The shipped runtime measures the projected **footprint** instead — the
placement's size in cells multiplied by cell size and scale — so a 5×7 Image
keeps its detail at a camera scale where the 1×1 Note beside it is already a
stand-in. Projected cell size is the contract and the runtime is the defect,
because two placements at the same distance are at the same distance and must
read that way.

| Boundary | Token | Figure | Camera scale |
| --- | --- | --- | --- |
| Working → stepped | `--tier-detail-demote` | `56px` | below `0.255` |
| Stepped → working | `--tier-detail-promote` | `72px` | `0.327` and above |
| Stepped → stand-in | `--tier-standin-demote` | `18px` | `0.082` and below |
| Stand-in → stepped | `--tier-standin-promote` | `28px` | `0.127` and above |

The camera ranges from 1% to 1000%. At 1% the projected cell is `2.2px` and
every placement is a stand-in; at 100% and above every placement is a working
form.

Nothing is exempt. No kind, no state, and no selection pins a placement to a
tier — the runtime currently forces a selected placement to the working form,
which is a defect, because `States.md` already specifies a selected stand-in
and requires it to wear the same outline as a working form.

## Hysteresis and the ladder

Demote and promote are distinct figures at every boundary. Between the two, a
placement keeps whatever form it already holds, so a camera resting on a
threshold never flickers between forms while the view drifts.

A placement moves one rung at a time. A stand-in promotes to the stepped form
first and reaches the working form on the following evaluation, even when the
camera jumped past both thresholds in one gesture, so a fast zoom shows the
same sequence of forms as a slow one.

## The shedding order

Whenever a representation must give something up — because the camera pulled
back, or because a frame cannot afford to draw it — it gives it up in this
order and never out of it.

| Order | What goes | Reason it goes before the next line |
| --- | --- | --- |
| 1 | Chrome that serves the hand: the resize corner, edit and view affordances, everything the Approached state adds. | It serves the hand, and the hand cannot reach a target this small. |
| 2 | Decorative surface treatment: page texture, the inset edge, the shadow that lifts the form. | It carries no information a person reads. |
| 3 | Detail: type, captions, title blocks — everything read rather than recognised. | Below legible size it is texture pretending to be text. |
| 4 | Structure: masses, rules, and blocks. | Losing it leaves only kind, position, and extent, which never go. |

Each line is bound to a boundary:

| What goes | At |
| --- | --- |
| Chrome | Projected cell size falls below `--tier-detail-promote`. |
| Surface treatment and detail | Working → stepped. |
| Structure | Stepped → stand-in. |

Chrome leaves before the tier changes, and it leaves at the promote figure
rather than the demote figure so that chrome never appears on a form that is
one small movement away from shedding it.

Detail is not deleted when it goes; it is replaced in place by the mass it
occupied, so a page that loses its text still reads as a page rather than as
a blank sheet.

## What never sheds

Presence, position, and extent. At every distance, in every tier, under every
state:

- The placement occupies its exact cells at the projection's true size.
- The presence it casts is rendered in its own hue, cell-quantized.
- The census of the field at any distance equals the census of the field.

Distance simplifies representation. It never hides, relocates, merges, or
unloads what a person placed.

## Kind coding

No two kinds of content collapse into one glyph. Each kind carries a distinct
fill and a distinct mark, and the fill alone carries kind down to a single
pixel, because below the size at which a mark can be drawn the fill is all
there is.

| Kind | Fill | Edge | Mark |
| --- | --- | --- | --- |
| Note | Its authored fill — `--c-note-violet`, `--c-note-clay`, or `--c-note-slate-blue`. | `--edge-quiet`, inset. | Three light strokes at `--ink-primary`. |
| Document | `--surface-page`. | `--paper-border`, inset. | A ruled head bar above body rules, in `--c-paper-ink`. |
| Image | Its own plate. | `--paper-border`, inset. | The figure mark. |

Forbidden as a stand-in: a filename card, a file-type icon, a neutral block
used for every kind, and any role or signal hue as a fill. The runtime fills
Document stand-ins with `--c-ink` and Image stand-ins with `--c-view`; both
are defects, because ink is not paper and a signal hue is not content.

A stand-in states what the thing is, never what file it came from — a
filename card proves only that a file exists and puts a dark container where a
page belongs.

## Promotion

Promotion is asynchronous and never blocks camera motion. Crossing a promote
threshold queues the fuller form; the camera keeps moving at full rate while
the queue drains.

| Rule | |
| --- | --- |
| Never blank | A pending form holds the footprint with the most complete form it already has. An empty cell is a lie about what exists. |
| Never a spinner | A progress mark on the Grid replaces content with a report about content. |
| Arrival in place | The fuller form takes over with the same identity, position, and extent the distant form promised. Nothing moves to arrive. |
| Cross-fade | The exchange runs over `--d-swap` on `--ease`. Under reduced motion it is an immediate swap at the identical threshold. |
| Demotion is immediate | Stepping back needs no preparation, because drawing less is always available this frame. |
| Cancellation | A queued promotion for a placement that has left the view or crossed back below its promote threshold is cancelled, not completed — desktop convention for view-driven work, and finishing it would spend a frame on something nobody is looking at. |
| Order of service | The queue is served largest projected footprint first, because the largest placement is the one whose coarseness is most visible. |

## The stand-in is the placement

A stand-in is not a picture of a placement standing in front of it. It **is**
the placement, drawn simply.

- It is hit-testable. Pointer and keyboard reach it exactly as they reach a
  working form.
- It is selectable, and it wears the same interaction accent, the same
  outline, and the same field response as a working form.
- It has no geometry of its own: no sub-cell offset, no minimum size, no
  snap-to-screen, no rounding of its edges to whole screen pixels.
- It is derived and disposable. Content is not: a stand-in is never written,
  never a revision, and never what a Memory or a Trace refers to.

## States on a stand-in

Every state survives every tier. Where the drawing differs at the stand-in
tier, it differs like this:

| State | On a stand-in |
| --- | --- |
| Rest | Fill, edge, mark. |
| Approached | Nothing. Chrome is already gone, and an unreachable target does not answer the hand. |
| Focused | The Grid cursor at the focused cell, which is measured in cells and shrinks with the field. |
| Selected | The same outline and the same brightened cells as a working form. |
| Engaged | The gesture's role drawn on the footprint, unchanged. |
| Pending | The stand-in itself. It is the pending form. |
| Refused | Refusal hue on the block and 45° hatching on the cells; the sentence stays in the surface that asked, because the block has no room for words. |
| Unavailable | `--text-unavailable` applied to the fill; the footprint is unchanged. |
| Anchored | The presence hue alone — the ribbon and the corner mark are content geometry and fall below one pixel here, and presence never sheds, so the anchor hue always survives. |

## Three refusals

**Nothing counter-scales to a fixed screen size.** A placement's on-screen
size is its footprint under the camera, and content pinned at a constant
screen size floats free of the field and lies about where and how big it is.
This binds everything drawn in Grid coordinates: placements, placement
previews, the Grid cursor, and the presence field.

**Distance never aggregates into clusters or counts.** Three placements are
three marks at three positions. A cluster bubble replaces positions with a
number, invents a shape the field does not have, and hides what exists.

**An empty cell is never shown.** Where a fuller form is not ready the
fallback is a kind-coded block; unloading content for smoothness makes the
Grid report less than it holds.

## Disagreements ruled

| Disagreement | Ruling |
| --- | --- |
| The Distance deck measures thresholds on projected cell size; the Document deck labels its distance moments in projected footprint, and the runtime computes on footprint. | Projected cell size wins, so the field sheds together instead of by placement size. |
| The Document deck says the stepped form keeps "still every word"; the Distance deck and `css/materials.css` replace type with structural masses in the same tier. | The Distance deck wins: at a `56px` projected cell a `--t-body` line is under `4px` on screen, so keeping every word is a claim the pixels cannot honour. |
| The Reading markers deck states that a marker keeps its size as the view moves, which reads as counter-scaling. | Both stand. The counter-scaling refusal binds what is drawn in Grid coordinates; a reading marker is drawn on the reading layer, cannot be selected, and never becomes a placement. |
| The Reading markers deck shows a marker stating how many pieces it gathered, which reads as a count. | Both stand. The refusal forbids a count that stands **in place of** placements; every placement a marker names is still drawn at its true position beneath it. |
| The runtime pins a selected placement to the working form. | Defect. Selection never changes tier, and `States.md` specifies the selected stand-in. |

## Not owned here

Grid line fading with spacing is a Grid rule and its figures are
`--grid-fade-start` and `--grid-fade-end` in the token table. Presence field
rendering, hue accumulation, and cross-layer presence belong to the field.
The camera is pure view state: it stores position and scale and nothing about
representation, and a placement stores nothing about which form it last held.
