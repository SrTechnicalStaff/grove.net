---
type: design-system-grammar
status: active
date: 2026-08-09
tags: [grove, design-system, states]
---

# The state model

Every component in Grove answers the same nine states. A component
specification fills all nine; "not applicable" is an answer and blank is not.

States divide into three kinds, and the kind decides where the state may be
stored.

| Kind | States | Storage |
| --- | --- | --- |
| Interaction | Approached, Focused, Selected, Engaged, Refused | Transient. Never written to a Memory, a Placement, or a Content payload. |
| Availability | Rest, Unavailable, Pending | Transient, derived from what the application can currently do. |
| Property | Anchored | Durable. Authored by a person and persisted. |

Distance is not a state. It is a representation tier, and every state above
must survive it. `Representation-tiers.md` owns that axis.

## The nine states

### Rest

Nothing is happening. Content is only itself: its own fill, its own text, its
own frame. No control, badge, handle, or outline is present.

Rest is the default and the majority of the Grid at any moment. A
component that cannot be quiet at rest is a defect.

### Approached

The pointer is near enough that the component expects to be used. Chrome that
serves the hand — a resize corner, an edit affordance — fades in over
`--d-fade` and leaves with the pointer.

Approach never restyles the component's own surface. It adds; it does not
change what is already there.

Approach has no keyboard equivalent. A keyboard user reaches Focused directly.

### Focused

Keyboard attention is on this component. Focus is always visible and is never
suppressed.

Off the Grid, focus is a `2px` ring in `--signal-interaction` at `--ink-full`,
offset `2px` outside the component's edge, applied on `:focus-visible` only.
It sits outside so it never covers content, and it is drawn on top of any
other state's outline.

On the Grid, keyboard attention is the Grid cursor at the focused cell. A
placement does not draw a second focus ring; the cursor already says where
attention is, in cells, at the same size as everything else.

Focused and Selected are different states and must be distinguishable when
both are true: the focus ring sits outside the selection outline with a
visible gap.

### Selected

This is what the person is working on. Selection carries three signals
together so it survives without colour:

1. A `2px` outline in `--signal-interaction`, offset `3px` outside the
   content edge. The outline sits clear of the frame.
2. The cells around the footprint brighten, hard-edged, in the interaction
   hue.
3. The component's own surface is untouched — never tinted, dimmed, washed,
   or restyled.

Selection is never stored. Reload, and nothing is selected.

Multiple selection uses one grammar. Every selected placement wears the same
outline and the same field response, whatever form it holds. No badge, count,
or ordinal rides on content.

### Engaged

A gesture is open on this component right now — a drag, a resize, a sweep.

Engagement is drawn in the role of the gesture: `--signal-active-work` for a
selection sweep, `--signal-interaction` for a move or a placement preview. The
amber is only ever present while a hand is moving; when the gesture ends it is
gone, and only the interaction accent remains.

Nothing durable changes while a gesture is open. The source keeps its place,
the Grid is unchanged, and releasing early costs nothing.

Input is never held. A component may lag by a frame in what it draws around
itself; it may not lag in what it does with the hand.

### Pending

A result has been asked for and has not arrived.

Pending is never blank and never a spinner. The component holds its position
and extent, and shows the most complete form it already has — a stand-in, a
previous revision, a coarse frame. An empty space is a lie about what exists.

When the result arrives it replaces the pending form in place, over
`--d-swap`, with the same identity, position, and extent. Arrival never moves
anything and never blocks camera motion.

### Refused

The operation cannot happen. Refusal carries three signals together:

1. Hue — `--signal-refusal` on the thing being refused.
2. Structure — the cells or region that cannot accept the operation are
   hatched at 45°, so refusal reads with no colour at all.
3. Words — one plain sentence, placed beside the thing being refused, in a
   local strip inside the surface that asked.

The action that would commit is present and unavailable, so the person can
see what they were reaching for.

Refusal is complete, never partial. Nothing is half-done, nothing moved, and
there is nothing to undo. Grove has no scrim, no centre-screen alert, and no
confirmation that floats free of the work it is about.

### Unavailable

The component is present but cannot be acted on now.

Unavailable is `--text-unavailable` with a `--edge-hairline` border, and it
keeps its position in the layout. An action that would disappear when
unavailable teaches a person that the interface is unstable; an action that
greys teaches them what is missing.

Unavailable is not Refused. Unavailable is a standing condition; Refused is
the answer to an attempt.

### Anchored

The component carries authored context.

Anchored is geometry first, so a colour-blind read still lands:

- A placement whose form has a head — a Note, a Document — carries a ribbon at
  its top edge: `12 × 22px`, clipped to `polygon(0 0, 100% 0, 100% 100%, 50% 72%, 0 100%)`.
- A placement that is a complete frame — a picture — carries a `9 × 9px`
  square rotated 45° at its top-left corner, outside the frame.

Both are filled `--signal-authored-context`, and the presence the placement
casts takes the same hue. The hue is fixed wherever authored context appears
and is never a colour a person can pick.

## Combination

States combine. The rules for combination are fixed:

| Combination | Result |
| --- | --- |
| Approached + Selected | Both. Chrome appears; the outline and field response stay. |
| Focused + Selected | Both, with the focus ring outside the selection outline. |
| Selected + Anchored | Both. The ribbon or mark stays; the field takes the anchor hue, not the interaction hue. |
| Engaged + Refused | The engagement drawing turns to the refusal role in place. The gesture stays open; the person can keep moving. |
| Pending + Selected | Both. A stand-in is selectable, hit-testable, and wears the same outline as a working form. |
| Unavailable + Approached | Approach adds nothing. An unavailable component does not answer the hand. |

Where two states would draw the same region, precedence is: Focused, then
Refused, then Selected, then Engaged, then Anchored.

## Requirements on every state

- **Never colour alone.** Each state is carried by at least two of hue,
  structure, position, and words.
- **Never motion alone.** The settled frame carries the same meaning as the
  transition. Under reduced motion every transition becomes an immediate
  change at the identical threshold.
- **Never stored unless it is a property.** Only Anchored persists.
- **Never a size change.** No state alters a component's footprint, position,
  or extent. Outlines and rings are drawn outside; fills are drawn within.
- **Never truncation.** No state shortens authored content to make room for
  itself.
