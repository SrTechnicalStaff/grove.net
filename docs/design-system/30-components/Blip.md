---
type: design-system-component
status: active
date: 2026-08-10
component: Blip
plane: information
surface_class: local-editor
tags: [grove, design-system, component]
---

# Blip

A small Information Plane mark saying that material at a sampled local point
already adds up to something worth reading, and the cue it raises when a
person comes near that source.

The mark itself is the point marker `00-foundations/Marks.md` names, and the
one surface it raises — the cue at the head of its route — is a local editor in
its viewer form, because it serves material that stays visible and it only
looks. That is why this specification carries the local editor's class.

## Anatomy

Every visible part, named, with the token that gives it its value. A part with
no token is a part with no contract.

| Part | Required | Value |
| --- | --- | --- |
| Marker ring | yes | `14 × 14px`, `--r-round`, a `1px` ring in `--signal-interaction` at `0.70`. |
| Marker fill | yes | `--signal-interaction` at `0.20`, filling the ring's box. |
| Marker core | yes | Inset `3px` inside the ring, `--r-round`, `--signal-interaction` at `--ink-primary`. |
| Route | no | A `1px` line in `--signal-interaction` at `--ink-secondary`, run as right-angled steps from the marker to the cue's near edge. No arrowhead, no terminal dot, no anchor at either end. Present on Approached only. |
| Cue frame | no | `--surface-chrome`, opaque; `--r-sm`; `1px` in the View role at `--k-view`; `--sp-md` padding on every side; `--shadow-local`. Present on Approached only. |
| Cue label | no | `--f-mono` 500, `--t-label`, `--tr-label`, uppercase, `--text-meta`. |
| Anchor square | no | `7 × 7px`, `--r-none`, filled `--signal-authored-context`, `--sp-sm` before the cue title, aligned to its cap height. Present only where the cue is a person's own written context. |
| Cue title | no | `--f-display` 500, `--t-title-small`, `--tr-title`, `--lh-tight`, `--text-primary`, in the source's own case, wrapping to at most three lines. |
| Cue picture | no | The picture at its own proportions with its long edge `54px`, `1px` `--edge-quiet`, `--sp-sm` before the title. |
| Cue passage | no | `--f-ui` 400, `--t-body`, `--lh-reading`, `--text-primary`, measured `--measure-reading`, in the source's own case, wrapping. |
| Count line | no | `--f-mono` 500, `--t-label`, `--tr-mono`, `--text-meta`, `--sp-sm` above. |
| Open action | no | The frame's full content width, `--signal-interaction` fill, label `--c-paper-ink` in `--f-mono` 500, `--t-label`, `--tr-label`, uppercase; `--r-sm`; `--sp-sm` padding; `--sp-md` above. |

Three figures are this component's own, and each is expressed against a core
token as `00-foundations/Tokens.md` requires. `0.70` on the ring and `0.20` on
the fill are exact figures no other component shares; three renderings agree on
them and no ink-ramp step sits within `0.05` of either, so they are recorded
rather than rounded. `54px` on the cue picture is the long edge of the one
thumbnail Grove draws inside a cue, and it is a component figure because no
other surface holds a picture at that size.

The marker is drawn in Information Plane coordinates and keeps `14px` for its
entire local-surface lifetime. It receives one anchor when engagement begins;
it never reads or observes Camera state after that handoff. It carries no ring
beyond the one, no second stroke, no halo, and no glow: the
`0 0 0 5px` spread and the `0 0 18px` blur that both annotation decks and
`css/information-plane.css` draw are refused by `00-foundations/Tokens.md`,
which declares two shadows and states that a glow is not available, and by
`00-foundations/Elevation-and-depth.md`, which names this exact mark. The mark
separates by its ring, its fill, and its core.

Nothing else is drawn. There is no badge, no count on the mark, no label beside
it, no leader dot at the route's foot, and no chrome on the Grid when no
marker is engaged.

## Geometry

- **Footprint** — none. The marker occupies no cells, takes no Grid address of
  its own, and is not addressed in cells. Whether a marker exists at all is
  decided by a procedure over the field, executable twice with the same answer:
  1. Take every placement whose field reaches the target Layer, including
     placements on other Layers, whose contribution is attenuated by the
     cross-Layer falloff `00-foundations/Tokens.md` and `Presence.md` own.
  2. Sample each cell the field reaches. A cell is **supported** when its
     coverage is at least `0.50`; a partly covered cell is not supported and
     does not count.
  3. Group supported cells by shared placement — two pieces join when their
     fields overlap on the same target Layer. Sitting next to each other joins
     nothing, and no group is formed from what the material is about.
  4. Sum the group's source demand from `10-grammar/Content-concentration.md`:
     wrapped readable text lines produce `Tᵢ`, complete intrinsic image frames
     produce `Iᵢ`, and reserved unsupported media produces `Oᵢ = 0` in this
     release.
  5. The group qualifies when its source mass is at least `1.0` **and** it
     holds at least `24` supported cells, or when its source mass is at least
     `1.0` and it holds a single Document — one long piece can qualify alone,
     and there is no rule counting items.
  6. Each qualifying group takes exactly one marker. A group inside a larger
     group qualifies on its own evidence and carries its own marker; neighbours
     never merge into one.
  7. A group that falls below either floor loses its marker on the frame it
     falls, and a group that crosses either floor gains one on the frame it
     crosses.
- **Growth** — the marker never grows; a group that gathers more material moves
  the marker's point and leaves its size alone. The cue frame grows downward as
  its cue needs and never truncates, ellipsizes, or sets type below the reading
  size.
- **Measure** — `--measure-reading` on the cue passage, which sets the frame's
  content column and therefore the frame's width. The decks' `270px` is that
  column rendered; the column is the contract, because a reading column
  expressed in surface width is a defect.
- **Alignment** — the opening feature supplies the coverage-weighted local
  anchor and the collision-free side selected for the source footprint. The
  Information Plane does not calculate Grid cells, project a Grid point, or
  subscribe to Camera frames. A staircase line connects the sampled source
  point to the marker. The cue frame's near edge sits `--sp-md` from the route
  head and flips to the other side rather than crossing a viewport edge.

## States

All nine.

| State | Appearance | Notes |
| --- | --- | --- |
| Rest | No marker, route, cue, or words. | Quiet is the default, and no Information Plane element is mounted before source engagement. |
| Approached | The ring goes to `--ink-full`, the fill goes to `0.35`, the staircase route fades in over `--d-fade`, and the cue frame appears at the sampled local anchor. | A pointer or Grid cursor engagement supplies one local anchor. One marker is approached at a time; moving to another candidate replaces it. |
| Focused | The marker and its Open action take `--focus-ring` at `--focus-ring-offset`, drawn outside them. | Keyboard attention on the Grid is the Grid cursor; the cursor arriving on a supported cell raises the same cue the pointer raises. |
| Selected | The marker is an explicit reading target outside occupied Content; it receives the interaction outline and does not enter Content selection. | The dot can open its own reading, while Grid selection still names durable Content. |
| Engaged | The Open action takes the pressed appearance over `--d-press` while a pointer is down on it. | The marker opens the reading; it accepts no drag or resize. |
| Pending | The markers already drawn hold their points and their drawing while the field is recomputed. | A marker is never blank, never a spinner, and never a placeholder ring drawn before a group qualifies. |
| Refused | The mark, the route, and the cue are unchanged. Asking to pin a second reading leaves the first pinned, turns the pin control to `--signal-refusal`, and states the condition in one plain sentence inside the reading that asked. | Refusal is complete: nothing is swapped, nothing is lost, and the kept reading is untouched. Only the control and its sentence change. |
| Unavailable | Unreachable, and that is the answer. A candidate whose reading cannot open has already stopped qualifying, so its marker is gone rather than greyed. | No Information Plane return control is created. |
| Anchored | The mark is never Anchored — it is derived, and Anchored is authored. Where a gathered piece carries a person's written context, that context is the cue and the anchor square sits before it. | The square is `10-grammar/Signal-roles.md`'s form for authored context on a line in a record, which is what a cue is. The cue's words stay `--text-primary`; only the square takes the hue. |

Combination follows `10-grammar/States.md`. The only combination this component
reaches is Approached with Refused, which is the pin refusal while a cue is
raised: the cue is unchanged and the refusal stays inside the reading.

## Behaviour

- **Pointer** — a pointer engagement on a supported source supplies one local
  anchor and raises that group's cue. The marker is placed outside the
  occupied footprint where possible and may be clicked to open the same
  reading as its Open action. Moving to another group's reach replaces the cue;
  leaving every reach lowers it. There is no context gesture, drag, or resize
  on a marker.
- **Keyboard** — the Grid cursor arriving on a supported cell raises that
  group's cue, which is the keyboard equal of pointer proximity. Raising a cue
  never moves focus; `Tab` moves focus into the raised cue, where the Open
  action is the only stop, and `Enter` there opens the reading and moves focus
  into it. `Escape` lowers the cue and returns attention to the Grid cursor at
  its cell. This is a decision, because `docs/reference/Keybind map.md` assigns
  this component no key while `00-foundations/Accessibility.md` requires every
  action to be reachable from the keyboard, and adding a tab stop to a raised
  surface is cheaper than a new global key for a surface that is already on
  screen.
- **Focus order** — the raised cue has one focusable marker and one Open action.
  The route and cue words are not in the tab order.
- **Escape** — `Escape` with a cue raised lowers the cue and leaves the
  Grid, the camera, and the selection exactly as they were; focus returns
  to the Grid cursor at its cell. `Escape` with a reading open closes that
  reading only, per the escape order in `10-grammar/Surface-classes.md`.
- **Commit and cancel** — nothing this component does is durable. Raising a cue,
  opening a reading, paging it, pinning it, and returning to it write no Memory,
  no Placement, and no history entry, so there is nothing to commit and nothing
  to undo. A pin lasts the session and is released by dismissing the reading.

Pinning is the one behaviour that outlives the cue. Pinning holds one reading
while a person looks around; the reading remains at its sampled Information
Plane anchor. Camera movement, panning, zooming, Layer changes, and Grid cursor
movement do not replace, hide, move, or reproject it. One reading is kept at a
time: a second pin request is refused in words and never swaps the kept reading
silently.

The reading itself — its pages, its frames, its routes back to source, and its
pin control — belongs to the reading surface and is named here, not restated.

## Motion

| Transition | Duration | Curve | Reduced motion |
| --- | --- | --- | --- |
| The route and the cue frame arriving on approach | `--d-fade` | `--ease` | Present at the identical threshold, with no fade. |
| The route and the cue frame leaving | `--d-fade` | `--ease` | Absent at the identical threshold. |
| The ring and fill taking their approached values | `--d-fade` | `--ease` | Applied at the identical threshold. |
| Pointer-down on the Open action | `--d-press` | `--ease` | The pressed appearance applied on press and removed on release, with no scaling. |

The route appears by fading, not by drawing itself along its own length:
  `00-foundations/Motion.md` permits `opacity`, `transform`, and colour only, and
a stroke that grows animates a length. The decks' "the line draws on approach"
is honoured as the fade.

The marker never animates on arrival or removal: a group crossing a floor gains
or loses its marker on that frame. Nothing loops. Nothing idles. Nothing pulses
or blinks.

## Distance

Thresholds are on projected cell size and belong to
`10-grammar/Representation-tiers.md`.

| Tier | Sheds | Keeps |
| --- | --- | --- |
| Working | Nothing. | The mark at `14px`, unscaled; the route; the cue; the count. |
| Stepped | Nothing. | The mark at `14px`, unscaled; the route; the cue; the count. |
| Stand-in | Nothing. | The mark at `14px`, unscaled; the route; the cue; the count. |

The marker sheds nothing at any distance and never scales with the camera,
which `00-foundations/Marks.md` already fixes for the point marker and
`10-grammar/Representation-tiers.md` already reconciles with the refusal on
counter-scaling: that refusal binds what is drawn in Grid coordinates, and this
mark is drawn in screen space, cannot be selected, and never becomes a
placement.

Nothing about the Camera decides whether a marker exists. The same material
qualifies at every zoom and every pan because qualification belongs to the
Field Ledger, not the view. After engagement, the marker and cue remain at
their sampled Information Plane coordinates until their local surface closes
or the person engages another source. They are not reprojected, clipped into a
new viewport position, or promoted to HUD.

## Accessibility

- **Role and name** — the marker itself is hidden from assistive technology: it
  is geometry that says "here", and everything it names is said in full by the
  cue it raises. The raised cue is a named group whose accessible name is the
  cue text verbatim — the person's own written context, or the piece's own
  title, or the passage. `Nearby reading` as an accessible name is a defect, and
  `10-grammar/Copy.md` already records that exact string as one; so is `dialog`
  as the role, because Grove has no modal.
- **Contrast** — the ring at `--signal-interaction` `0.70` reaches 5.12:1 on
  `--surface-grid` and the core at `--ink-primary` reaches 6.66:1; either
  alone clears the 3:1 floor for a mark. The fill at `0.20` reaches 1.45:1,
  carries no meaning, and is exempt as surface treatment inside the ring. The
  approached ring at `--ink-full` reaches 16.03:1. The route at
  `--ink-secondary` reaches 4.25:1 on the Grid; where it crosses its own
  lit cells the field beneath is capped at `--field-alpha-max` and the ratio
  falls to 2.49:1, so the fact the route carries — which material the cue names
  — is carried at the same moment by the approached marker at its foot, at
  16.03:1, and by the cue frame at its head. On the cue frame, `--text-primary`
  reaches 10.36:1 and `--text-meta` 4.71:1; the Open action's label at
  `--c-paper-ink` on `--signal-interaction` reaches 8.55:1; the frame's View
  role edge at `--k-view` reaches 9.33:1 on the Grid.
- **Without colour** — Rest: a ring with a solid core, a silhouette Grove draws
  for nothing else. Approached: a route and a frame that were absent, plus the
  core reaching full ink. Focused: the focus ring outside the control, at its
  own offset. Selected, Engaged, Pending, Unavailable, Anchored: no state uses
  hue alone, and the anchor square is a silhouette. Refused: the plain sentence
  inside the reading, which reads with no colour at all.
- **Forced colours** — the ring, the fill, the core, the route, both frames,
  their edges, the Open action's fill and label, and the anchor square are
  redeclared in system colours. What survives without redeclaration is the ring
  and core silhouette, the route's steps, the anchor square's shape, the focus
  ring's offset, and every word.
- **Text scaling** — the cue's text is interface text and grows with it: the
  frame grows downward and sideways to its measure and then wraps, and nothing
  truncates at any scale. The marker keeps `14px`, because it
  is a mark rather than text, and its position is unchanged by interface text
  scaling.
- **Reduced motion** — no change from the Motion table.

## Copy

Five strings.

| String | Where | Why it passes |
| --- | --- | --- |
| `Everything nearby` | The cue label, when the cue names everything one concentration holds. | Ordinary English, names what is there rather than the machinery that found it, sentence case, no terminal punctuation. |
| `One group nearby` | The cue label, when the cue names one dense group inside a larger one. | Same, and it distinguishes the two without naming a tier or a measure. |
| `4 pieces gathered nearby` | The count line, with `1 piece gathered nearby` in the singular. | A plain count of what the cue holds; `10-grammar/Representation-tiers.md` already ruled this count admissible, because every piece it counts is still drawn at its true position beneath the mark. |
| `Open reading` | The Open action. | A verb for an action, ordinary English, true after any rebuild. |

Everything else is **None**. The cue's title, picture, and passage are authored
content: Grove never edits, shortens, summarises, re-cases, or prefixes them,
and never writes a title for a group nobody named. The pin control's labels and
the pin refusal sentence are shown by the reading and are written in its own
specification.

`Nearby reading`, `Field reading`, and `Local reading` are refused wherever they
appear: the first narrates the idea behind the feature and the other two name
Grove's own machinery. `Reading saved` is refused because a pin lasts the
session and saves nothing, and copy names what is true.

## Refusals

- **A marker that can be dragged or resized** — the mark has no footprint and
  cannot become Content chrome. Its explicit click/focus path only opens the
  reading it represents.
- **A marker that scales with the camera** — the mark belongs to the reading
  layer and not to the field, and a mark that shrank with the view would claim
  a footprint it does not have.
- **A marker that appears or disappears with the camera** — the same material
  qualifies at every zoom and pan, or a person watches markers blink while they
  look around.
- **A count, a score, a saturation figure, or any measurement on the mark** —
  supported cells and source mass are the evidence behind the mark, and putting
  evidence on screen turns a Grid into a readout.
- **A glow, halo, or soft ring around the mark** — only the field emits, and it
  emits in cells with hard edges; a soft ring crosses cell boundaries and
  promises a precision the measure does not have.
- **A second marker for the same group, or one marker for two groups** — groups
  are found first and each takes exactly one mark, or a busy area sprouts a mark
  for every pair of pieces.
- **Two routes drawn at once** — one cue is raised at a time, and a second
  staircase turns the Grid into a diagram of itself.
- **A cue that is centred, that dims the Grid, or that covers its own
  material** — a reading is local to its material or it is not a reading.
- **A cue that truncates, ellipsizes, crops, or shrinks its words or its
  picture** — pages are the only answer to material that does not fit.
- **A marker stored anywhere** — a marker is derived from the field on every
  evaluation, and a stored marker would survive the material that justified it.
- **A silent swap of a kept reading** — a second pin is answered in words,
  because a reading a person chose to keep is never taken back without saying
  so.
- **A marker for material Grove cannot read** — audio, video, and paged
  documents are absent from a reading rather than half-shown, so no mark
  promises them.


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

- Catalogue deck: `docs/design_catalogue/src/information-plane/04-annotation-markers.html` — historical source for the quiet field, floor, mark, cue hierarchy, route, and pinning; its viewport return mark is superseded by this contract.
- Catalogue deck: `docs/design_catalogue/src/information-plane/08-annotation-routes.html` — historical route source; its viewport return mark is superseded by this contract.
- Catalogue deck: `docs/design_catalogue/src/00-design-language.html` — borders contain, shadows separate, never a glow.
- Plane: `docs/design-system/20-planes/Information-plane.md` — locality, and the reading this marker opens.
- Reference: `docs/reference/Keybind map.md` — `Escape` on a local surface, `Tab` within one.
