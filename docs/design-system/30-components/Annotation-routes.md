---
type: design-system-component
status: active
date: 2026-08-10
component: Source routes
plane: information
surface_class: menu
tags: [grove, design-system, component]
---

# Source routes

The quiet way out of a reading, back to where a piece lives — its Placement
on the Grid, the Memory that owns it, or the writer or viewer built for its
form.

A route is offered by one piece at a time, never by the reading as a whole.
It is neutral cloth raised on demand: `30-components/Menu.md` owns the
frame, the row, and the keyboard grammar this component inherits unchanged.
What belongs here is what only a reading raises — which rows a piece offers,
what each one arrives at, and what a route does when the thing it promised
has moved, been re-placed, or stopped existing since the reading was made.

## Anatomy

Every visible part, named, with the token that gives it its value. A part
with no token is a part with no contract.

| Part | Required | Value |
| --- | --- | --- |
| Frame | yes | `30-components/Menu.md`'s frame, unchanged: `--surface-chrome`, opaque, `--r-sm`, 1px `--edge-quiet`, `--shadow-local`, `--sp-xs` top and bottom, no side padding. |
| Row | yes | `30-components/Menu.md`'s row, unchanged: label left in `--f-ui` 400 at `--t-caption`, `--lh-ui`, `--text-primary`, one line; `--sp-sm` top and bottom, `--sp-md` at the sides. |
| Quick key | yes | One unmodified character in `--f-mono` 500 at `--t-label`, `--tr-mono`, `--text-meta`, right of its row. |
| Group separator | yes, exactly one | `1px` `--edge-hairline`, `--sp-xs` above and below, spanning the frame's inner width. It sits after the first row and before the rest; there is no second separator, because this menu asks exactly two questions. |
| Highlight fill and edge | no | `30-components/Menu.md`'s highlight, unchanged: `--surface-nested` fill, inset `1px` `--signal-interaction` edge, on the row the pointer or the keyboard is on. |

Nothing else is drawn. This component adds no icon, no metadata column, no
count, and no title of its own — the piece the menu was raised on is already
the title, per `30-components/Menu.md`.

**Where it sits.** The menu opens at the pointer's invoking point beside the
piece a context gesture named, by the placement procedure `30-components/Menu.md`
owns; it is never centred over the page and never drawn inside a piece's own
frame. The piece it acts on carries the chosen outline `30-components/Annotation.md`
already specifies — `2px` `--signal-interaction` offset `3px`, seated on the
page's dark keying line — for as long as the menu is open, so the printed
page is never restyled to show which piece a route would act on; only the
outline outside its edge says so.

**Understated, and never the reading's rival.** A route is reached by an
explicit context gesture, never printed as a standing row of buttons beside
every piece; `30-components/Annotation.md` already refuses that as
"Controls printed on the page." At rest the page carries no route at all —
no border, no hint, no ghost of a control — and the row label itself sits at
`--text-primary`, the same ink every quiet menu on Grove uses, never brighter
than the page it was raised over. Approaching a piece with the pointer does
nothing, per `30-components/Annotation.md`'s own Approached state: the page
is content, not a control, and a hand passing over it restyles nothing.

## Geometry

- **Footprint** — none in cells; this is chrome in screen space, and its
  size is `30-components/Menu.md`'s own width and height procedure applied
  to the labels below. **Which rows exist is a procedure, run once each time
  the menu is raised on a piece:**
  1. Read the piece's Memory. If it cannot be found, no menu is raised —
     see States · Unavailable.
  2. **Row one, the focused surface.** If the Memory's current form is a
     Note or a Document, the row is `Open in Writing Slate` at `W`. If its
     current form is an Image or a GIF, the row is `Open` at `O`. Exactly
     one of the two is present; the four supported forms partition into
     these two rows without remainder.
  3. One group separator.
  4. **Row two, the place.** Read the Memory's current Placements — not the
     set the reading's group was gathered with. If at least one current
     Placement exists, the row is `Show on the Grid` at `G`. If none exists,
     the row is `Place` at `P` instead. Exactly one of the two is present.
  5. **Row three, the record.** `Open Memory` at `M`, always, last, in the
     same group as row two.
  6. Three rows and one separator, for every piece whose Memory still
     resolves. The same piece, read twice at the same moment, yields the
     same three rows.
- **Growth** — none of this component's own; it inherits
  `30-components/Menu.md`'s refusal to scroll, wrap, or truncate a row.
- **Measure** — none. A row is one label, not a column of prose.
- **Alignment** — the frame's leading corner lands on the invoking point and
  flips to stay on screen, exactly as `30-components/Menu.md` fixes; nothing
  about being raised from a reading changes the algorithm.

## States

All nine. Rest here means the menu is open, since chrome absent is not a
state of the thing that is absent — the same convention `30-components/Menu.md`
uses for itself.

| State | Appearance | Notes |
| --- | --- | --- |
| Rest | Frame, edge, shadow, the two or three rows, one separator. No row highlighted. | The piece it was raised on keeps its chosen outline throughout; the page beneath is untouched. |
| Approached | The row under the pointer takes the highlight fill and edge; every other row is unchanged. | Unchanged from `30-components/Menu.md`. |
| Focused | Keyboard attention is on the menu, at the highlighted row; neither the frame nor a row draws `--focus-ring`. | Unchanged from `30-components/Menu.md`. |
| Selected | No change from Rest. | This menu holds no selection of its own; the piece's own Selected state is `30-components/Annotation.md`'s. |
| Engaged | Pointer held on a row: no change from the highlighted appearance. | No pressed acknowledgement, because the row commits on release and the menu leaves the same frame. |
| Pending | Not reachable. | The row set in *Geometry* is composed the instant the menu opens; nothing in it waits for an answer. |
| Refused | The menu closes on the frame the chosen row's operation cannot complete. The reading's own answer line, owned by `30-components/Annotation.md`, carries `That destination is unavailable. The reading is still here.` | A row that looked valid when the menu opened can still fail at the moment it is chosen — a destination that will not open, or a piece whose form or Placement changed underneath the open menu. Neither is a standing condition, so neither is drawn as a grey row; the menu simply closes and the reading says so. |
| Unavailable | No menu is raised at all. A context gesture on a piece whose Memory can no longer be found does nothing. | `30-components/Menu.md`'s own rule: a target with no actions opens nothing. The piece keeps its place, its title, and its provenance, and prints its own unavailable sentence in place of its body — `30-components/Annotation.md`'s part, not this one. |
| Anchored | No change from Rest. | This component carries no durable property; a piece's own Anchored state is `30-components/Annotation.md`'s. |

Combination follows `10-grammar/States.md` without exception. The only
combination this component adds is Refused arriving while Approached or
Focused is true on the row that was chosen: the row's own highlight is gone
the same frame the menu closes, because nothing about it survives to be
highlighted.

## Behaviour

- **Pointer** — a context gesture on a piece chooses that piece, if it was
  not chosen already, and opens its route menu at the invoking point in the
  same motion. This is a decision: the corpus fixes clicking to choose a
  piece and a context gesture to open its menu as two facts without saying
  whether one gesture can do both, and combining them is the nearest
  established desktop convention for a context click on an unselected
  target, per the resolution order `docs/design-system/README.md` sets. A
  row commits on pointer-up over it, and the menu closes on the same frame,
  exactly as `30-components/Menu.md` fixes.

  **Show on the Grid** arrives at the piece's current Placement on its
  Layer. It resolves that Placement fresh, at the moment the row is chosen
  — never the Layer or cell the reading's group was gathered with — so a
  Memory that has moved to another Layer since the reading opened is framed
  on the Layer it is on now, and a Memory re-placed to a different cell on
  the same Layer is framed at its current cell, not the one it held at
  gather time. Where a Memory holds more than one current Placement, the
  route reveals the one nearest the reading's own target Layer — the same
  Field Ledger observation, per `docs/reference/Field and relationship
  model.md`, that made this piece a member of this group in the first
  place — because that is the Placement the reading is actually pointing
  at; a Memory with no Placement on or near the target Layer cannot be a
  member of the group, so this case does not arise. Taking this route makes
  the revealed Placement's Layer the active Layer, moves the Camera the
  least distance needed to bring its whole footprint into view at the
  working form, and selects it — the same framing and the same selection
  the Grid already draws for any placement, not redrawn here. This is the
  one route allowed to move the Camera, and only because it was chosen by
  name; every other route leaves the Camera exactly where it was. Switching
  the active Layer and selecting the placement are view state, never a
  write: no Memory changes, no Placement moves, and nothing is created.

  Because the reading is a screen-fixed surface beside its source and the
  Camera has just been asked to travel, this route pins the reading if it
  was not pinned already — a decision, because a reading left unpinned at
  its old screen position would sit over whatever the Grid now shows
  underneath, with no way back to it once its source scrolls out of view,
  which is exactly the condition `30-components/Blip.md`'s return mark
  exists to answer. The pin persists exactly as any other pin does: it is
  released by dismissing the reading, never automatically on return.

  **Place** arrives at the placement handoff `30-components/Placement-preview.md`
  owns. Choosing this row starts that handoff exactly as choosing Place
  anywhere else does; nothing is written until the person commits a cell
  there, and cancelling the handoff returns to the reading with the record
  unchanged.

  **Open in Writing Slate** and **Open** arrive at `30-components/Writing-slate.md`
  and `30-components/Image-viewer.md` respectively, each carrying the
  Memory's own identity. Neither moves the Camera. The viewer opens above
  the reading as a second local editor on the same source, per
  `10-grammar/Surface-classes.md`; the writer opens as its own HUD pane
  beside it. Both destinations keep their own commit and cancel rules,
  named here and not restated.

  **Open Memory** arrives at `30-components/Memory-slate.md` open directly on
  the piece's record — never the gallery front — whether the Memory is
  placed once, placed many times, or never placed at all; how many times it
  sits on the Grid is a line of provenance beside the record, not a
  condition on whether the route works. This route never moves the Camera
  and never selects anything on the Grid.

  Every route leaves the reading open, unclosed, exactly where it was. The
  chosen piece keeps its outline for the whole trip. Nothing a route reaches
  edits, renames, or duplicates the piece it was raised on; a destination
  that writes does so only through its own commit, on the person's own
  later action there — never as a side effect of arriving.
- **Keyboard** — this component declares no key of its own beyond the rows'
  own printed keys, which live only while the menu is open, per
  `30-components/Menu.md`. `Shift+F10` and the Menu key open the route menu
  on the chosen piece when a piece holds keyboard focus inside the reading —
  a decision, because `00-foundations/Accessibility.md` requires a keyboard
  route to every action and this is the desktop convention `30-components/Document.md`
  already uses for reaching a content menu from the keyboard. Once open, the
  menu's own grammar — arrow keys, `Home`, `End`, `Enter`, each row's
  printed key, no typeahead — is `30-components/Menu.md`'s, unchanged.
- **Focus order** — one focusable part while open: the menu, per
  `30-components/Menu.md`. Rows are not in the tab order.
- **Escape** — `Escape` closes the menu and returns focus to the piece it
  was opened on, per the Menu row of `10-grammar/Surface-classes.md`'s
  escape order; the reading stays open, unchanged. Where the destination is
  Memory Slate opened on a record, `Escape` there first steps back to its
  own gallery per `30-components/Memory-slate.md`'s own rule, and only a
  second `Escape` — or its `Close` control — closes the pane and returns to
  the reading; every other destination returns to the reading on its own
  first `Escape` or `Close`.
- **Commit and cancel** — opening the menu commits nothing, and choosing a
  row commits nothing about the reading itself. Show on the Grid, Open,
  Open in Writing Slate, and Open Memory write nothing at all; only Place
  can eventually write, and only through the placement handoff's own
  commit. Closing any destination and returning writes nothing either: the
  reading resumes at the same page, with the same piece chosen, because the
  trip itself — not the destination — remembers where it started.

## Motion

| Transition | Duration | Curve | Reduced motion |
| --- | --- | --- | --- |
| Menu arriving on open | `--d-fade` | `--e-standard` | Drawn complete at the invoking point on the frame it opens. |
| Menu leaving on dismiss, on a chosen row, or on Refused | `--d-fade` | `--e-standard` | Gone on the frame it is dismissed. |
| Highlight moving row to row | None | — | No change; there was nothing to reduce. |

Inherited unchanged from `30-components/Menu.md`. The Camera's own movement
to a revealed Placement, and any destination surface's own arrival, belong
to the Grid and to that destination's specification and are not this
component's motion. Nothing loops. Nothing idles. Nothing pulses or blinks.

## Distance

Thresholds are on projected cell size and belong to
`10-grammar/Representation-tiers.md`.

| Tier | Sheds | Keeps |
| --- | --- | --- |
| Working | Nothing. | Every part at its declared size. |
| Stepped | Nothing. | Every part at its declared size. |
| Stand-in | Nothing. | Every part at its declared size. |

This menu is chrome in screen space raised on a reading that itself holds no
representation tier, per `30-components/Annotation.md`'s own Distance
section. Projected cell size says nothing about it, and it never follows the
Camera once open — a Camera gesture while it is open dismisses it, exactly
as any menu is dismissed by a click outside.

## Accessibility

- **Role and name** — `role="menu"`, unchanged from `30-components/Menu.md`;
  its accessible name is the chosen piece's own title, or — where the piece
  carries none — the source name and the kind, in that order, never a
  generic label such as "Source destinations." Each row is `role="menuitem"`
  named by its own label, with its printed key exposed as
  `aria-keyshortcuts`.
- **Contrast** — identical to `30-components/Menu.md`'s figures, because the
  tokens are unchanged: row label `--text-primary` on `--surface-chrome`
  reaches `10.36:1`; quick key `--text-meta` reaches `4.71:1`; the highlight
  edge `--signal-interaction` reaches `8.90:1`, the state's non-text
  carrier; the frame edge `--edge-quiet` reaches `1.54:1` and is exempt,
  because `--shadow-local` carries the frame's extent.
- **Without colour** — Approached and Focused: a fill step no other row
  carries and a stroke on one row only. Refused: the menu's own absence,
  plus one plain sentence on the reading's answer line, which reads with no
  colour at all. Rest, Selected, Engaged, Pending, Unavailable, Anchored:
  nothing here uses hue alone.
- **Forced colours** — identical to `30-components/Menu.md`: the frame fill,
  edge, row inks, and highlight are redeclared in system colours; the
  separator's break between the two groups survives as structure.
- **Text scaling** — rows grow taller and the frame grows wider to its
  widest row, per `30-components/Menu.md`; nothing wraps or truncates at any
  scale, and the reading behind is untouched because it scales independently
  of the Grid.
- **Reduced motion** — no change from the Motion table.

## Copy

Five row labels, and the one sentence this component's own Refused state
prints. `30-components/Annotation.md` already accepted all six strings; they
are restated here because this component is what raises them.

| String | Where | Why it passes |
| --- | --- | --- |
| `Open in Writing Slate` | The row for a Note or a Document. | A destination named in words, and `Writing Slate` is the surface's own product title, which `10-grammar/Copy.md` permits by name. |
| `Open` | The row for an Image or a GIF. | The piece it is beside is already the title; a second word would repeat what the frame already shows. |
| `Show on the Grid` | The row for a piece with a current Placement. | Names the destination in plain words a person already owns. |
| `Place` | The row for a piece with none. | One verb, no terminal punctuation, matching the same word used everywhere else Grove starts this handoff. |
| `Open Memory` | Always, last. | `Memory` is Grove's own noun for the durable piece, per `10-grammar/Copy.md`'s own correction of `Open record`. |
| `That destination is unavailable. The reading is still here.` | The reading's answer line, when a chosen row's operation cannot complete. | States the condition, not the operation or the attempt, and says what is still true; one sentence, one clause, present tense. |

Everything else is **None**. This component writes no title of its own —
the piece the menu was raised on is the title — and prints nothing beside a
row that Copy.md would not already pass in `30-components/Menu.md`.

## Refusals

- **A route printed on the page** — a standing row of buttons beside every
  piece charges the whole page for an occasional act and puts chrome above
  content; a route is reached by a context gesture or the keyboard, never
  by scanning the page for controls.
- **A route that infers a destination** — selecting a piece must never open
  a writer or a viewer by itself; what a piece is decides which rows exist,
  never which one is taken.
- **A row that runs before the menu is dismissed** — opening the menu
  commits nothing; only choosing a row acts, and the menu closes on the
  same frame the row commits.
- **A grey row for a piece that has changed** — a row either exists, because
  the procedure in *Geometry* just composed it, or the operation it named
  fails at the moment it is chosen and the whole menu closes; there is no
  third state where a route sits visibly disabled inside an open menu.
- **A stale destination** — Show on the Grid never reveals the Layer or the
  cell the reading's group was gathered with; it reads the Memory's current
  Placement at the moment the row is chosen.
- **A route that moves the Camera without being asked** — Open, Open in
  Writing Slate, Place, and Open Memory never move the Camera; only Show on
  the Grid does, and only on explicit choice.
- **A route that writes on arrival** — no destination reached from this
  menu creates a Memory, a Placement, or a second reading merely by opening;
  the first durable change is the person's own later commit there.
- **The reading closing when a destination opens** — the reading stays open
  underneath every destination this component reaches, so the return is
  always real.
- **A second reading of the same material** — returning restores the
  reading that sent the person, at the same page, with the same piece
  chosen, never a rebuilt one.
- **A centred menu, a scrim, or a dim behind it** — this menu is neutral
  chrome raised at the invoking point; the Grid and the reading behind it
  stay fully lit and live.

## Design assertions

| Assertion | Required result |
| --- | --- |
| Routes are on-demand chrome | Route controls are not persistent page content. |
| Row set | The selected source form determines the form-specific route; current Placement availability determines `Show on the Grid` or `Place`. |
| Canonical row labels | `Open in Writing Slate`, `Open`, `Show on the Grid`, `Place`, and `Open Memory` are the only route labels. |
| Source identity | Every route carries the selected Memory identity; no route creates a replacement source record. |
| Camera ownership | Only `Show on the Grid` may move the Camera, and only after explicit selection. |
| Reading continuity | Opening a route leaves the Annotation edition, page, and selected source intact for return. |
| Refusal | A failed destination uses `That destination is unavailable. The reading is still here.` and leaves the Annotation open. |

## Sources

- Catalogue deck: `docs/design_catalogue/src/information-plane/08-annotation-routes.html` — the whole contract for this component: the row set by form, Show on the Grid, Place, Open Memory, the writer and viewer, the return, unavailable routes, and the four refused route designs.
- Component: `docs/design-system/30-components/Menu.md` — the frame, the row, the highlight, and the keyboard grammar this component inherits unchanged.
- Component: `docs/design-system/30-components/Annotation.md` — the reading, its chosen outline, its answer line, and the piece states this component acts on.
- Component: `docs/design-system/30-components/Blip.md` — pinning and the return mark this component's Show on the Grid route relies on.
- Component: `docs/design-system/30-components/Memory-slate.md`, `docs/design-system/30-components/Writing-slate.md`, `docs/design-system/30-components/Image-viewer.md`, `docs/design-system/30-components/Placement-preview.md` — the four destinations, owned there and named here.
- Grammar: `docs/design-system/10-grammar/Surface-classes.md` — the menu class, the escape order, and the local-editor stacking rule this component's viewer route uses.
- Reference: `docs/reference/Field and relationship model.md` — the Field Ledger observation Show on the Grid consults for a multiply-placed piece.
- Wireframe: `docs/ux/wireframes/annotations/source-routes/Annotation source routes.md` — states AS-00 through AS-04.
- Task: `docs/engineering/tasks/T-AN09 Annotation source routes.md`.
## Closed route decisions

| Decision | Required result |
| --- | --- |
| Route-menu entry | A person chooses a source in the reading, then opens that source's route menu. The menu is source-owned. |
| Multiple current Placements | `Show on the Grid` uses the unique Field Ledger contribution that qualified the source. If no unique Placement can be established, the route is unavailable and the Annotation remains open. |
| Route-to-reading return | Every destination returns to the same Annotation edition, page, and selected source. |
