---
type: design-system-grammar
status: active
date: 2026-08-09
tags: [grove, design-system, signal-roles]
---

# Signal roles

Grove has four signals. Each one answers a different question, and together
they answer every question a signal is allowed to answer. There is no fifth,
and there is no severity ladder inside any of them.

| Signal | Token | The one question it answers |
| --- | --- | --- |
| Interaction | `--signal-interaction` | What are you working on? |
| Active work | `--signal-active-work` | Is a gesture open right now? |
| Refusal | `--signal-refusal` | What will not happen? |
| Authored context | `--signal-authored-context` | Where did a person write something down? |

A signal is a hue plus a second carrier that is not a hue. The hue makes the
answer fast; the second carrier makes the answer survive a monochrome screen,
a colour-blind reader, and a printed capture. A signal drawn without its second
carrier is a defect, not a light version of the state.

`States.md` owns the nine states and the geometry each state draws.
`00-foundations/Tokens.md` owns every value. This document owns which signal is
correct, what it may never mean, and how the four hand over to each other.

## How each signal is drawn

| Signal | Drawn as | Drawn where | Second carrier |
| --- | --- | --- | --- |
| Interaction | A solid outline or ring, and lit cells beneath a footprint. | Outside the content edge, and on the field around it. Never on or inside content. | Position — the outline is offset clear of the edge, so the gap itself reads the state. |
| Active work | A solid `1.5px` edge over a fill faint enough that content beneath it reads unchanged. | On the field, above content, tracking the pointer. | Shape — a rectangle that belongs to no placement and exists in no other state. |
| Refusal | A `1px` edge, an inset edge on a refused footprint, or a text colour on one control. | On the thing being refused, and on the cells that cannot take it. | Structure and words — 45° hatching on the blocked cells, and one plain sentence beside them. |
| Authored context | A filled mark: a ribbon on a form with a head, a rotated square on a complete frame, a small square before a line in a list. | Outside or above the content edge, and in the presence the placement casts. | Geometry — the mark's silhouette is unique to authored context and belongs to no other state. |

Signals never fill a content body, never tint authored text, and never change a
placement's footprint. Every signal is additive drawing outside or beneath the
thing it describes.

## Interaction

**Job.** Name the thing the person is working on, or about to act on.

**Appears in.** Focused, Selected, and Engaged. Also on exactly one primary
action per surface, and on the highlighted row of an open menu.

**Drawn.** A `2px` outline offset outside a selected placement's edge; a `2px`
ring offset outside a focused component; lit cells around a selected footprint;
a solid fill behind the single primary action, with `--c-paper-ink` text on it;
a low-alpha wash and a border on the highlighted menu row.

**Second carrier.** The offset. Selection and focus are both readable as a gap
between the content and a line that is not part of it, and the two are told
apart by which line sits further out.

Pointer hover raises no interaction signal anywhere except inside an open menu,
where hover and the arrow keys light the identical row — the menus deck fixes
one highlight for both hands, so a person never has to learn two.

Selection lights the cells around a footprint but never repaints a presence
region: the region keeps its own hue and only its perimeter alpha rises to
`--field-perimeter-selected`. Where a selected placement carries authored
context, the field takes the authored-context hue instead.

**Never means:**

- A category, a kind, or a group a person authored.
- A warning, an error, or a result.
- Progress, completion, or a count of anything.
- Quality, recency, importance, or ranking.
- More than one action on a surface. Two primary actions mean neither is.

## Active work

**Job.** Say that a gesture is open right now and has produced nothing yet.

**Appears in.** Engaged, and only where the gesture has no subject — a
selection sweep. An open gesture that already has a subject, such as a move, a
resize, or a placement preview, draws in the interaction signal instead, because
two hues on one gesture would make a person read a colour to learn what their
own hand is doing.

**Drawn.** A rectangle following the pointer exactly, at any angle of travel: a
solid `1.5px` edge with a fill weaker than the edge. The rectangle carries no
label, no handles, no corner figures, and no count.

**Second carrier.** The rectangle itself. It has a shape that no placement, no
chrome, and no other state ever draws, and it exists only while the button is
down.

**Never means:**

- A result. Nothing is selected, moved, or created by the amber.
- A warning, an alert, or anything unresolved.
- A category, a kind, or a highlight.
- A durable mark. Nothing drawn in this signal survives the gesture.
- A second gesture. One sweep at a time; a sweep inside a sweep does not exist.

## Refusal

**Job.** Say that an operation will not happen, and where.

**Appears in.** Refused. It also marks the single control on a surface that
permanently removes authored work — the same meaning, since that control is the
one place a person may cross the boundary Grove will not take back. `DESIGN.md`
fixes both, declaring the destructive action fill in this role.

**Drawn.** On a refused footprint, an inset edge and a faint fill in the refusal
hue; on the cells that cannot accept the operation, 45° hatching; on a
destructive row in a menu, the row label only; on a destructive button, a solid
fill with `--c-paper-ink` text.

**Second carrier.** Structure and words together. The hatching reads with no
colour at all, and one plain sentence sits beside the refused thing — "This
space is occupied", "Discard unsaved changes?" — with the action that would
commit present and unavailable.

Refusal is binary. There is no caution level, no amber-then-red ramp, and no
partial refusal: what is refused is refused completely, nothing moved, nothing
to undo.

**Never means:**

- Danger, severity, or urgency. Grove has no severity scale.
- A warning short of refusal. Grove has no warning level.
- Emphasis, or a way to make a number or a word louder.
- A result that has not arrived, or one that failed to arrive.
- Unsaved work. Changed state is the Edit role hue plus the words "Unsaved
  changes".
- An action that is merely unavailable. Unavailable is `--text-unavailable` on
  a `--edge-hairline` border and stays in place.

## Authored context

**Job.** Say that a person wrote context of their own onto this Memory or
placement.

**Appears in.** Anchored, and nowhere else. Anchored is the only durable state,
so this is the only signal that persists.

**Drawn.** A ribbon at the top edge of a placement whose form has a head; a
square rotated 45° outside the top-left corner of a complete frame; a small
square before the line in a list or record; and the same hue in the presence the
placement casts, so a mixed cell still shows that authored context is in it.
Where the authored words themselves are quoted, they may take this hue; nothing
else on the line may.

**Second carrier.** Geometry. The ribbon and the rotated square are silhouettes
used for nothing else, so the state lands with the colour removed.

**Never means:**

- A colour a person picked. A Note's fill is one of three authored colours and
  is content, not a signal.
- A category, a tag, or a label family.
- Provenance, authorship, or who made something. Creator identity is a record
  field, never a mark on the field.
- Emphasis or a highlight inside running text.
- A generated summary, a derived relationship, or anything Grove inferred.

## The handover rule

A sweep and its result never share a hue.

| Moment | Amber | Interaction accent |
| --- | --- | --- |
| Button down, sweep opens | The rectangle appears. | Nothing. |
| Sweep in progress | The rectangle tracks the pointer. | Every placement wholly covered answers live: outline plus lit cells. |
| Release | The rectangle is removed on the release frame. | The caught placements keep exactly the drawing they already had. |
| After release | None anywhere on screen. | Selection, unchanged from what the sweep showed. |

Four rules bind the handover:

1. The rectangle is removed, not faded, because a fading rectangle reads as a
   gesture still open — desktop convention for a marquee is instant removal, and
   the corpus fixes no duration here.
2. Nothing changes at release. What the sweep showed as caught is what is
   selected; a person who lets go has already seen the answer.
3. A sweep that catches nothing removes its rectangle and says nothing. There is
   no empty-result message, because nothing was attempted and nothing failed.
4. Escape during a sweep removes the rectangle and restores the selection that
   existed before it, at no cost — an open gesture changes nothing durable.

A selection is complete only when the sweep covers every cell a placement
occupies. Partial cover catches nothing, so the live answer during the sweep is
always the true answer.

## One signal, one job

A signal used for a second job stops being a signal. The moment a hue means two
things, a person has to read context before they can read colour, and the colour
has bought nothing.

This is not a matter of degree. These are all the same defect:

- Refusal hue on a retry action, a status line, or an unsaved marker.
- Active-work amber on a button, a badge, a decorative mark inside content, or a
  stand-in glyph.
- Interaction hue on a second primary action, a category chip, or a progress
  figure.
- Authored-context hue on emphasis, on a search match, or on a derived link.
- A lighter or darker variant of a signal used to mean a weaker or stronger
  version of the same state. Each signal has exactly one value.

## No new accents

An accent invented for one screen fails review. There is no local exception, no
one-off, and no "just this surface".

- If an existing signal answers the question, it is used.
- If no signal answers the question, the answer is not a signal — see below.
- If the design still needs a hue, the design is wrong before the palette is.

A new hue enters Grove only through an accepted Decision record and a row added
to `00-foundations/Tokens.md` in the same commit. The reserved presence family
for agent identity is the only hue currently approved but not yet valued; it is
identity, not a signal, and it never carries a signal's job.

## What is not a signal

| Thing | What it is | Rule |
| --- | --- | --- |
| Tool, View, Layer, Edit, Slate hues | Operation families. | They identify which kind of operation a surface or control belongs to. They never say what state anything is in. |
| The three authored Note fills | Content. | A person chose them. They mean only what that person meant. |
| Ink ramp alphas | Reading hierarchy. | Faint ink means supporting text or an unavailable action, never a signal turned down. |
| Paper fill | A page. | A light surface always means an authored page and never chrome. |
| Reserved agent presence family | Identity. | Stable per agent, visually disjoint from every role hue, and never a state. |
| Stand-in glyphs at distance | Content at rest. | Drawn in ink alphas and a placement's own authored fill only. No signal hue and no role hue appears in one, because a stand-in is neither a state nor an operation. |

## Choosing a signal

| What is being communicated | Signal |
| --- | --- |
| Keyboard attention is here | Interaction |
| This is selected | Interaction |
| Many things are selected | Interaction, identically on each — no count, no ordinal on content |
| The one action that commits this surface | Interaction |
| The menu row that Enter would fire | Interaction |
| A placement is being moved or resized | Interaction |
| A placement preview under the pointer, including a Trace | Interaction |
| A sweep is open | Active work |
| This footprint will not fit | Refusal |
| These cells cannot take it | Refusal |
| This operation cannot happen | Refusal |
| The control that permanently removes authored work | Refusal |
| A person's Anchor is on this | Authored context |
| Authored context is inside this cell's presence | Authored context |
| A Content-side Anchor in a record or a list | Authored context |
| A person's chosen Note colour | None — it is content |
| Which operation family a control belongs to | None — role hue |
| A result asked for and not yet arrived | None — hold the fullest form already available |
| A result that failed to arrive | None — keep the footprint, show identity and a retry in quiet ink |
| An action present but not usable now | None — `--text-unavailable` on `--edge-hairline` |
| Unsaved changes | None — the Edit role hue and the words |
| A save that failed after the person asked for it | Refusal — the person attempted, and the attempt was answered |
| Severity, priority, or urgency | None — Grove has none |
| A warning short of refusal | None — Grove has no warning level |
| Progress, counts, sizes, coordinates | None — `--text-meta` |
| Recency, quality, or ranking | None |
| Who made something | None — a record field |
| Distance from the camera | None — representation changes, state does not |

Where this table is silent, the answer is None. A meaning that has no row here
has not earned a signal, and adding one is a change to this document and to the
token table, in that order.

## Rulings

The corpus disagrees with itself in five places. These are the answers.

1. **Four signals, not three.** `Tokens.md` prose and the selection deck both
   say "three signals"; the token table lists four semantic signal tokens and
   `DESIGN.md` names the authored-context hue as fixed everywhere it appears.
   Four is correct — the count predates authored context becoming a token — and
   both the prose line and the deck folio are corrected.
2. **Content that will not load carries no refusal hue.** The Memory deck says a
   card that failed to load gets no refusal red because nothing was refused; the
   Gallery deck borders an undecodable frame in the refusal hue. The Memory deck
   wins: a load that has not arrived is a standing condition, not an answer to an
   attempt. The Gallery frame keeps its footprint, its identity, and a retry, in
   quiet ink on a found edge. `css/slate.css` carries the same defect on an
   unavailable card and is corrected with it.
3. **A stand-in glyph carries no signal hue.** The Image and Distance decks draw
   an amber disc inside the figure stand-in. Amber means an open gesture; a
   stand-in is a placement at rest. The disc becomes ink.
4. **The refusal signal has one value.** Product CSS uses brightened and dulled
   variants of the refusal hue on previews. Only `--c-invalid` exists; the
   variants are corrected to it, because a second value implies a second
   severity and Grove has no severity.
5. **A placement preview is never amber.** The shipped Trace ghost is drawn in
   the active-work hue. `States.md` fixes placement previews to the interaction
   accent, and the source notes ask only that an armed cursor be identifiable
   without words: the armed cursor's own marker distinguishes the operation, and
   the previewed footprint stays in the interaction accent.

## Refusals

- No fifth signal.
- No severity ramp, no caution level, no colour that means "almost".
- No signal drawn without its second carrier.
- No signal on a content body, on authored text, or inside a frame.
- No signal that persists except authored context.
- No accent invented for one screen, one state board, or one mockup.
- No signal used to say something a person authored.
