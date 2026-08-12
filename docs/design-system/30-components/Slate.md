---
type: design-system-component
status: active
date: 2026-08-10
component: Slate
plane: hud
surface_class: slate
tags: [grove, design-system, component]
---

# Slate

The surface a person opens when the work needs room — Memories, a Gallery, a
Document being written — held at the front of the screen while the Grid
carries on behind it.

## Anatomy

Every visible part, named, with the token that gives it its value. A part with
no token is a part with no contract.

| Part | Required | Value |
| --- | --- | --- |
| Frame | yes | `--surface-chrome`, opaque, `--r-none`, filling the viewport edge to edge, with `--sp-lg` padding on all four sides. |
| Containment edge | yes | `1px` solid `--k-slate-b` on the inner edge shared with another pane only. A Slate meets the screen at its outer edges, so it draws no border there. |
| Header | yes | One row: identity left, host commands right, both on one centre line. `--sp-lg` beneath it, and no rule. |
| Identity | yes | `--f-display` weight 500 at `--t-title-small`, `--tr-label`, `--lh-tight`, uppercase, in `--k-slate`. One line, never wrapped. |
| Host command | yes | `--f-ui` weight 400 at `--t-caption`, `--lh-ui`, `--text-secondary`; `1px` `--edge-hairline`; `--r-sm`; padding `--sp-xs` top and bottom, `--sp-md` at the sides; `--sp-sm` between adjacent commands. |
| Body | yes | The pane's content in neutral ink on the frame's own fill. The Slate hue appears in the header and nowhere else. |
| Nested surface | no | `--surface-nested`, `1px` `--edge-hairline`, `--r-sm`, `--sp-md` padding; its text `--f-ui` at `--t-dense`, `--lh-ui`, `--text-primary`. |
| Scroll track | no | `3px` wide at `--r-sm`, ink `--ink-whisper`, inset `--sp-sm` from the inside of the frame's edge. Present only while the body overflows; the body reserves `--sp-md` at that side so no content runs under it. |
| Scroll thumb | no | `3px` wide at `--r-sm`, ink `--ink-tertiary`, its length the visible fraction of the body. Its drag target is `--sp-md` wide, centred on the thumb and undrawn. |
| Active-pane edge | no | `1px` `--signal-interaction` at `--ink-full`, replacing `--k-slate-b` on the pane holding keyboard attention. Drawn only when two panes are open. |
| Inactive-pane edge | no | `1px` `--edge-hairline`, replacing `--k-slate-b` on the peer. Drawn only when two panes are open. |
| Collapsed rail | no | The same frame at `48px` wide, `--sp-md` padding top and bottom, holding the identity set vertically in the identical role and ink. |

Three figures are this component's own. `3px` is the width of the scroll track
and its thumb, owned here because no other Grove part reports a position, and
three catalogue decks set it identically. `48px` is the collapsed rail, which
must hold one vertical identity line with `--sp-md` clear on both sides.
`880px` is the viewport width below which two panes stop fitting, taken from
`src/features/slates/composition.ts` and recorded rather than invented.

The header carries `Add` while a second pane can still be opened, and `Close`
always, in that order. Nothing else. A Slate has no tab bar, no footer
toolbar, no status bar, no second title, no breadcrumb, and no control drawn
anywhere but that one row.

The containment edge is measured outward, against `--surface-grid`, which
is the boundary it draws. `--k-slate-b` reaches 3.08:1 there and 2.89:1 against
the frame's own fill; the outward ratio is the one that governs, because the
edge exists to say where the surface ends.

## Geometry

- **Footprint** — the viewport exactly. A Slate does not float: it has no
  inset, no margin, no gap at any screen edge, and no rounded corner, because
  a corner radius only exists where a surface has something to sit on and a
  Slate meets the screen. Executed as a procedure: take the viewport
  rectangle; if one pane is open, that rectangle is the pane; if two are open,
  divide it down the middle with no gap between them and give each pane half,
  rounded down to a whole pixel, with any odd pixel going to the left pane so
  the split is the same every time; the two panes meet on a single `1px`
  `--k-slate-b` edge; if the viewport is narrower than `880px`, the pane not
  holding keyboard attention becomes a `48px` rail at the right edge and the
  other pane takes everything left of it.
- **Separation** — none is drawn. A Slate needs no shadow because nothing shows
  beside it to separate from; the edge of the screen is the edge of the
  surface. Padding is the only inset a Slate has, and it is inside the frame.
- **Growth** — the body scrolls and the footprint does not change. This is the
  one place `00-foundations/Principles.md` Law 9 exempts, because a Slate is
  chrome fixed to the viewport rather than a footprint on the field. It may
  never shorten, summarise, crop, or shrink what it carries to avoid scrolling;
  a reading page inside it paginates instead.
- **Measure** — the host sets one line of type, the identity, and no reading
  column of its own. A column of continuous text inside a pane declares its
  measure in `ch` and the pane's own specification names the value. Where a
  pane is narrower than that measure, the column count drops and then the
  content paginates; the measure holds and the type size never changes.
- **Alignment** — the frame's four edges are the viewport's four edges. The
  identity's box and the host commands share one centre line. The body's first
  row begins `--sp-lg` below the header box, and every internal gap is a step
  of the space scale.

Two panes is the maximum, and `Left`, `Right`, and `Full` are the whole
placement vocabulary. A third pane is not refused; it replaces an occupant, by
the explicit choice `10-grammar/Surface-classes.md` gives the assignment menu.

## States

All nine.

| State | Appearance | Notes |
| --- | --- | --- |
| Rest | The frame, its inner edge where a peer sits, the header, and the body. No control is drawn that is not one of the two host commands. | A Slate is absent from the screen until it is summoned, so Rest is the whole of its untouched appearance while open. |
| Approached | No change from Rest. | A Slate is chrome a person asked for, so it is already answering the hand; nothing fades in on approach and nothing leaves with the pointer. |
| Focused | The pane holding keyboard attention takes the active-pane edge; its peer takes the inactive-pane edge. The focusable part inside it takes `--focus-ring` at `--focus-ring-offset`. | With one pane open the edge stays `--k-slate-b`, because there is no peer to distinguish it from and the ring already says where attention is. |
| Selected | No change from Rest. | A Slate is not selectable. Selection belongs to the content inside a pane and is drawn there. |
| Engaged | No change from the frame. The part under the hand answers: a host command takes the pressed appearance over `--d-press`, the scroll thumb tracks the drag. | The frame never moves, resizes, or restyles while a gesture is open inside it. |
| Pending | The frame, the edge, the header, and the identity all hold. The pane shows the most complete form it already has. | Never blank, never a spinner, never a skeleton. An empty frame is a lie about what has been opened. |
| Refused | No change from the frame. The refusal is drawn inside the pane that asked, by `30-components/Refusal.md`. | The frame is never tinted with the refusal hue, because a whole surface in the refusal colour says the surface is the problem. |
| Unavailable | The frame is never Unavailable. A host command that cannot act now keeps its position in `--text-unavailable` on a `--edge-hairline` border — `Add` takes this while both slots are filled. | A composed surface a person summoned is always readable and always closable; a command that vanished would teach them the header is unstable. |
| Anchored | Not reached. | Anchored is a property of authored content; a Slate is chrome and authors nothing. Content inside a pane carries its own. |

Combination follows `10-grammar/States.md` without exception. The identity hue
never shifts for any state: `00-foundations/Color.md` fixes that a role hue
carries identity and a signal carries state.

## Behaviour

- **Pointer** — a click anywhere inside a pane makes that pane the one holding
  attention, committing on pointer-down so the edge answers the first press. A
  click on `Close` closes that pane, committing on pointer-up over the control.
  A click on `Add` opens the assignment menu at the control, which is a menu
  and belongs to `10-grammar/Surface-classes.md`. A drag on the scroll thumb
  scrolls the body, committing continuously. A click outside the frame acts on
  the Grid and does not dismiss the Slate, because the class has exactly
  two exits and a body of work is not something a stray click should close.
- **Keyboard** — `S` opens Memories or focuses its search when it is already
  open, `V` opens Gallery, `Shift+S` opens the chooser for a second pane, `/`
  focuses the Memory search, `Escape` closes the pane holding attention, and
  `Tab` and `Shift+Tab` move within the composition. Every one of these is
  `docs/reference/Keybind map.md` and none is redefined here. Focus arrives on
  the first focusable part of the pane that just opened, and leaves to the peer
  pane or to the surface that invoked the Slate.
- **Focus order** — the header's commands in reading order, `Add` then `Close`,
  then the body in reading order. The identity is never focusable, because it
  does nothing. With two panes open the two panes form one cycle: `Tab` from
  the last part of one pane reaches the first part of the other, and focus does
  not leave the composition while it is open. One cycle covers both panes, so
  no second key is invented for a move between them.
- **Escape** — Escape closes the pane holding attention and only that pane; the
  peer is left exactly as it was, with its scroll, selection, filters, and
  drafts intact, and focus lands on its first focusable part. Closing the last
  pane returns focus to the surface that invoked the Slate, or to the Grid
  cursor at its last cell when that surface is gone. A menu or an inline
  confirm open inside a pane answers Escape first, and a pane whose own
  specification defines a dirty draft answers before the host does.
- **Commit and cancel** — opening is side-effect free: nothing is created,
  moved, selected, or committed by opening a Slate or by choosing where it
  sits. The composition itself is not durable and does not survive a reload,
  because a way of looking is not a change to the work. Content changes commit
  inside the pane that holds them, by that Slate's own rule.

Which Slate holds what, what its body lists, and how its own content commits
belong to the specification of that Slate. Assignment, replacement, and the
chooser's anatomy belong to the menu class.

## Motion

| Transition | Duration | Curve | Reduced motion |
| --- | --- | --- | --- |
| The frame arriving on summon | `--d-fade` | `--ease` | Present at the identical threshold, with no fade. |
| The frame leaving on Close or Escape | `--d-fade` | `--ease` | Absent on the frame the close commits. |
| A pane's content replaced when that slot is reassigned | `--d-swap` | `--ease` | The new content in place on the same frame, at the same extent. |
| A host command's press acknowledgement | `--d-press` | `--ease` | The pressed appearance applied on press and removed on release, with no scaling. |

Nothing else moves. A pane collapsing to the rail and returning from it changes
the frame with no transition, because those are width changes and
`00-foundations/Motion.md` refuses animating a layout property. The containment
edge, the active-pane edge, the focus ring, the identity, and the shadow never
animate. Nothing loops. Nothing idles. Nothing pulses or blinks.

## Distance

Thresholds are on projected cell size and belong to
`10-grammar/Representation-tiers.md`.

| Tier | Sheds | Keeps |
| --- | --- | --- |
| Working | Nothing. | The whole surface, unchanged. |
| Stepped | Nothing. | The whole surface, unchanged. |
| Stand-in | Nothing. | The whole surface, unchanged. |

A Slate is fixed to the viewport and has no cells, so the tiers never reach it:
panning and zooming change the scene behind the frame and leave the frame
identical, at the same size, in the same place. This is the proof that the
Grid behind is a live scene rather than a picture — it keeps moving while
the surface does not.

## Accessibility

- **Role and name** — each pane is a `region`, named by its own identity line
  through `aria-labelledby` so the accessible name and the visible name are the
  same string and can never drift apart. `dialog` is a defect here, because it
  claims a modality Law 7 refuses; `aria-modal` is a defect for the same
  reason, because it tells assistive technology the Grid is background
  when the Grid is live. The word `Slate` as an accessible name is a
  defect under `00-foundations/Accessibility.md`: it names the class rather
  than the body of work a person opened.
- **Contrast** — the identity in `--k-slate` reaches 9.85:1 on
  `--surface-chrome`. A host command's label at `--text-secondary` reaches
  6.35:1, which is what identifies it as a control; its `--edge-hairline`
  border is surface treatment at 1.28:1 and is exempt. Body text at
  `--text-primary` reaches 10.36:1 on the frame and 9.85:1 on a nested surface.
  The containment edge at `--k-slate-b` reaches 3.08:1 against
  `--surface-grid`. The active-pane edge in `--signal-interaction` reaches
  9.50:1 against the Grid and 8.90:1 against the frame. The scroll thumb
  at `--ink-tertiary` reaches 4.71:1; its track at `--ink-whisper` carries no
  information and is exempt. The nested surface's own step reaches only 1.06:1
  against the frame, which is why its `--edge-hairline` border is drawn — the
  edge is what says a frame was crossed, and it separates groups rather than
  establishing a boundary a person must find.
- **Without colour** — Focused: the interaction edge against the peer's
  hairline, a difference that reads as lightness in greyscale, plus the focus
  ring outside the focused part. Unavailable: a retained position and a
  hairline border. Rest, Approached, Selected, Engaged, Pending, Refused, and
  Anchored add no hue of their own at this component, so a greyscale read of a
  Slate loses nothing Grove said. The identity's hue is identity, not meaning:
  in greyscale the header is still the one display line on the surface.
- **Forced colours** — redeclared in system colours: the frame fill, the
  containment edge, the identity, the host commands' labels and borders, the
  active and inactive pane edges, and the nested surface's fill and edge. What
  survives without redeclaration is the header's position, the space beneath
  it, the focus ring's offset, and the rail's silhouette. The shadow does not
  survive, which is why the border is mandatory: a Slate known only by its
  shadow disappears there.
- **Text scaling** — the identity and the host commands grow with interface
  text and the header row grows taller rather than clipping; a command never
  truncates or wraps. The frame is measured from the viewport and does not
  scale with text, so the body takes the growth: columns drop before frames
  narrow, and the pane scrolls. Nothing truncates at any scale.
- **Reduced motion** — no change from the Motion table.

## Copy

Two strings.

| String | Where | Why it passes |
| --- | --- | --- |
| `Close` | The host command that leaves the Slate. | A verb for an action, ordinary English, sentence case, no terminal punctuation, and true after any rebuild. |
| `Add` | The host command that opens the chooser for a second pane. | A verb for an action, and the control sits in the header of the thing it adds to, so the subject is named by position rather than by a noun. |

Everything else is **None**. The identity line is the name of the body of work
the pane holds — `Memories`, `Gallery`, a Document's own title — and belongs to
the specification of the Slate that fills the frame; the word `Slate` is the
name of the class and is never shown. The collapsed rail shows that same
identity and is restored by clicking it, so it carries no second string. Titles
on host controls, status announcements about surfaces opening or closing, and
counts riding on the header are all refused.

## Refusals

- **A scrim, dim, blur, or tint behind the frame** — no Grove surface is modal,
  and darkening the Grid claims an interruption a Slate does not make.
- **An inert or unreachable Grid** — a Grid that is lit but takes no
  click is a scrim with the paint removed.
- **A translucent fill or a backdrop filter** — both make the Grid an
  ingredient of the surface's own colour, and a surface must stay readable at
  the moment it matters most.
- **A free-floating, draggable, or resizable frame** — position and extent are
  consequences of what the host composed, never things a person is asked to
  manage.
- **A drag divider between two panes** — the gap between peers is live
  Grid, and a handle in it turns the composition into a layout to tune.
- **A third pane, a quadrant, or a corner** — `Left`, `Right`, and `Full` is
  the whole vocabulary, and a fourth arrangement makes the placement glyphs
  stop saying where a thing lands.
- **A Slate inside a Slate, or a pane inside a pane** — the dark ladder has
  three rungs and a nested pane has no step left to take, so a composition that
  needs one has not decided its hierarchy.
- **A second title anywhere inside the frame** — a surface names itself once,
  and a repeated name is hierarchy nobody wrote.
- **A footer toolbar, a status bar, or a tab bar** — the header is the whole of
  a Slate's chrome, and a second strip charges the surface permanently for an
  occasional action.
- **A rule under the header** — space separates the header from the body;
  a hairline there ranks rather than separates.
- **An ellipsis or a clipped line in the identity or a pane's title** — a name
  that cannot fit at its size was too long before the frame was drawn.
- **Any shadow, or an inset top highlight** — a Slate meets the screen at
  every edge, so it has nothing to separate from, and a bevel fakes a light
  source Grove does not have.
- **A glow in any hue at any radius** — only the field emits, and it emits in
  cells.
- **Resizing or reflowing the Grid viewport when a Slate opens** — the Slate
  overlays the Grid; squeezing it would move work a person did not touch.
- **A position or scale that tracks the camera** — the frame is fixed to the
  viewport, and a surface that drifts with the scene has become part of it.
- **A control pinned over the Grid whose only job is to open a Slate** —
  chrome is absent at rest, and a permanent opener charges the whole Grid
  for an occasional action.


## Design assertions

| ID | Assertion |
| --- | --- |
| C-01 | Every visible part has a named token or explicit component value. |
| C-02 | Footprint, growth, measure, alignment, and responsive rules are explicit. |
| C-03 | Rest, Approached, Focused, Selected, Engaged, Pending, Refused, Unavailable, and Anchored are explicit. |
| C-04 | Pointer, keyboard, focus, Escape, commit, cancel, persistence, and return behavior are explicit. |
| C-05 | Refusal and recovery preserve identity, provenance, source order, and unchanged durable state. |
| C-06 | Motion, reduced-motion behavior, camera relation, and distance shedding are explicit. |
| C-07 | Component-specific Grid, Information Plane, or HUD visual signatures are retained. |

## Sources

- Catalogue deck: `docs/design_catalogue/src/hud/01-slate-anatomy.html` — the contract for this component: opaque fill, one quiet border, `2px` corner, `24px` padding, header identity in the Slate hue, no scrim, never floats free, two panes and no more.
- Catalogue deck: `docs/design_catalogue/src/hud/02-memory-slate.html` — the identity at `20px` with `--sp-lg` beneath it, and the nested card at `#1C1C20`.
- Catalogue deck: `docs/design_catalogue/src/hud/03-gallery-slate.html` — the same frame at `--sp-lg` padding, and the `3px` scroll cue.
- Catalogue deck: `docs/design_catalogue/src/hud/04-writing-slate.html` — one header row as the whole of the chrome.
- Plane: `docs/design-system/20-planes/HUD-plane.md` — one header, one body of panes, one return path; panes on `--surface-nested`.
- Grammar: `docs/design-system/10-grammar/Surface-classes.md` — the class anatomy, the escape order, and the assignment vocabulary.
- Reference: `docs/reference/Keybind map.md` — `S`, `V`, `Shift+S`, `/`, and `Escape`.
