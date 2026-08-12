---
type: design-system-component
status: active
date: 2026-08-10
component: Menu
plane: any
surface_class: menu
tags: [grove, design-system, component]
---

# Menu

A short list of what can be done to the thing a person just clicked, opened
where they clicked it.

The menu is neutral cloth. It carries no role hue, because the target has a
role and the list does not, and it is identical on the Grid, beside a
placement, and inside a composed surface — only the words change.

## Anatomy

Every visible part, named, with the token that gives it its value. A part with
no token is a part with no contract.

| Part | Required | Value |
| --- | --- | --- |
| Frame | yes | `--surface-chrome`, opaque, `--r-sm`, `6px` top and bottom, no padding at the sides so a row's highlight reaches the frame's inner edge. |
| Containment edge | yes | `1px` `--edge-quiet` on the frame's own box. |
| Separation | yes | `--shadow-local`, one layer, on the frame only. No second shadow, no inset highlight, no glow. |
| Row | yes | One line. Label left, quick key right, `--sp-sm` top and bottom, `--sp-md` at the sides, `--sp-lg` between the label and the key. |
| Row label | yes | `--f-ui` at weight 400, `--t-caption`, `--lh-ui`, `--text-primary`, no tracking, `white-space: nowrap`. |
| Quick key | yes, where the row has one | One unmodified character in `--f-mono` at weight 500, `--t-label`, `--tr-mono`, `--text-meta`, uppercase by tracking rather than by writing capitals. |
| Group separator | no | `1px` `--edge-hairline` spanning the full inner width, `--sp-xs` above and below, no radius and no box. |
| Destructive row label | no | `--signal-refusal`, alone in the last group, behind a separator, never adjacent to the first row. |
| Highlight fill | no | `--surface-nested` over the row's full width, hard-edged, reaching the frame's inner edge on both sides. |
| Highlight edge | no | `1px` `--signal-interaction`, drawn `inset 0 0 0 1px` on the row's own box so the row does not grow. Drawn only on a row `Enter` would fire. |
| Unavailable row | no | Label and key at `--text-unavailable`, with a `1px` `--edge-hairline` inset edge on the row's own box; position and group held. |
| Flyout indicator | no | One triangular arrow in the key slot, `--sp-sm` after the quick key, same family, size, tracking, and ink as the key, pointing the side the flyout opens. |
| Flyout frame | no | The frame above, unchanged: same fill, same edge, same radius, same padding, same shadow. |
| Flyout digit | no | `1`–`n` in `--f-mono` at `--t-label`, `--tr-mono`, `--text-meta`, in a leading column `1ch` wide, with the label `--sp-md` after it. The digit is the quick key, so the row carries no second one. |
| Title | no — refused | None. The thing that was clicked is the title. |
| Scrim | no — refused | None. The Grid behind stays fully lit, readable, and live. |

The frame's edge is surface treatment rather than a meaning-bearing edge:
`--surface-chrome` reaches only `1.07:1` against `--surface-grid`, so
`--shadow-local` is what states where the menu ends, which is why the shadow is
required and the edge is exempt from the `3:1` floor.

Two figures are this component's own, both derived rather than invented. A row
is `34px` tall — `--t-caption` at `--lh-ui` is `18px`, plus `--sp-sm` above and
below — and a separator occupies `9px`. They are stated here because the
geometry procedure below needs them and no other component shares them.

The menu has no icons, no checkbox or tick column, no metadata column, no
count, no badge, no submenu tree, and no control that is not a row.

## Geometry

- **Footprint** — the menu is chrome in screen space and holds no cells. Its
  width is executed as a procedure: set every label in `--f-ui` 400 at
  `--t-caption` and every key in `--f-mono` 500 at `--t-label`; take the widest
  label and the widest key; the frame's width is that label plus that key plus
  `--sp-lg` between them, plus `--sp-md` at each side, plus the two `1px`
  edges. Its height is `34r + 9g + 10`, where `r` is the number of rows and `g`
  the number of separators. The same list yields the same box every time,
  because nothing in the procedure reads the camera, the target, or the
  viewport. There is no minimum width and no maximum width.
- **Growth** — the menu grows to hold its rows and never scrolls, never wraps,
  and never truncates. A composed list taller than the viewport less `--sp-md`
  at the top and bottom is the wrong list for that target, and the surface that
  supplied it is corrected rather than the menu made scrollable.
- **Measure** — the menu sets type and declares no reading measure, because a
  row is one label rather than a column of prose and `--measure-reading` binds
  continuous text. The one column expressed in characters is a flyout's digit
  column, at `1ch`.
- **Alignment** — the frame's leading corner lands on the invoking point, by
  the placement procedure in *Behaviour*. Inside the frame every label shares
  one left edge and every key one right edge, both `--sp-md` from the inner
  edges; separators span the full inner width, corner to corner, so a group
  boundary reads as a break in the surface rather than a rule floating in it.

The menu is never addressed in cells, never aligned to a grid line, and never
scaled by the camera. It does not move once open; a viewport resize recomputes
its position from the same invoking point.

## States

All nine. Rest here means the menu is open, since chrome absent is not a state
of the thing that is absent.

| State | Appearance | Notes |
| --- | --- | --- |
| Rest | Frame, edge, shadow, rows, separators. No row highlighted, no flyout open. | A menu that is not open draws nothing; chrome is absent until it is called for. |
| Approached | The row the pointer enters takes the highlight fill and the highlight edge; every other row is unchanged. | The highlight moves only when the pointer enters a row, so crossing the gap to an open flyout leaves the parent row lit. |
| Focused | Keyboard attention is on the menu, and the highlighted row is where that attention is. Neither the frame nor a row draws `--focus-ring`. | A full-width row has no outside for an offset ring, and a ring around the frame would say only that the menu has attention, which its presence already says. |
| Selected | No change from Rest. | A menu holds no selection: choosing a row fires it and closes the menu, so there is nothing for a selection outline to mark. |
| Engaged | Pointer held on a row: no change from the highlighted appearance. A row holding an open flyout keeps its highlight for as long as the flyout is open. | No pressed appearance, because the row commits on release and the menu leaves on the same frame, so an acknowledgement would be a flash. |
| Pending | Not reachable. | The list is composed from what the target can do at the moment the menu opens, so no row is ever waiting for an answer. |
| Refused | Not drawn on the menu. | Choosing a row whose operation cannot happen closes the menu, and the refusal is drawn beside the thing being refused by `30-components/Refusal.md`. An action that cannot run now is Unavailable, not Refused. |
| Unavailable | Label and key at `--text-unavailable` on a `1px` `--edge-hairline` inset edge, in the row's usual position and group. The highlight fill still reaches it so a hand can see where it is; the highlight edge does not, and `Enter` and the row's own key do nothing. | An operation the target does not support is absent from the list; an operation it supports but cannot perform now is present and unavailable, as `10-grammar/Surface-classes.md` fixes. |
| Anchored | No change from Rest. | Anchored is a durable property of authored content and a menu carries none. |

Combination follows `10-grammar/States.md` without exception. The one
combination this component adds is Unavailable plus Approached: the fill
arrives and the edge does not, because the edge is reserved for the row `Enter`
would fire.

## Behaviour

- **Pointer** — a context gesture on a target opens its menu at the pointer. A
  row commits on pointer-up over it, and the menu closes on the same frame. A
  press released off the row commits nothing. A press outside the menu closes
  it and does not reach what is underneath, so a dismissing click never also
  acts. Moving onto a row that owns a flyout opens the flyout immediately, with
  no dwell in either direction, because a delay is a timing window and
  `00-foundations/Accessibility.md` requires none.
- **Keyboard** — `↑` and `↓` move the highlight one row, wrapping at both ends
  so the first `↑` lands on the last row. `Home` and `End` jump to the first
  and last row, a decision taken from the nearest desktop convention for a
  list. `Enter` fires the highlighted row. A row's printed key fires that row
  the instant it is pressed, whether or not the row is highlighted. `→` opens
  the flyout on a row that has one and moves the highlight onto its first
  destination; `←` closes the flyout and returns the highlight to its held
  parent row. `Escape` dismisses. `Space` fires nothing, because the printed
  key and `Enter` are already two ways in and a third is a second model of one
  action. Quick keys live only while the menu is open; closed, the same key
  types a letter or acts on the Grid, and nothing is reserved globally.
  `Shift+F10` and the Menu key open the menu on the selected target, as
  `30-components/Document.md` fixes, and `docs/reference/Keybind map.md` holds
  the row.
- **Typeahead** — none. A letter is a quick key and fires its row, so an
  accumulating prefix search would make every printed key a lie about what the
  next keystroke does. A person who wants to reach a row by its name reads it
  and presses the key printed beside it.
- **Focus order** — one focusable part: the menu. Rows are not in the tab order
  and `Tab` moves nothing inside the menu, because the highlight is the
  keyboard indicator and a second traversal model would give one list two
  cursors. The highlight arrives on the first row when the menu is opened from
  the keyboard, and on no row when it is opened by a pointer, since the pointer
  is at the invoking point rather than on a row; either way `:focus-visible`
  decides whether anything is drawn for a hand that is not asking.
- **Escape** — `Escape` closes the flyout if one is open and returns the
  highlight to its held parent row; a second `Escape` closes the menu. Focus
  returns to the target the menu was opened on — the placement, the frame, the
  row inside a composed surface — or to the control that declared it, with that
  target's own selection and scroll position untouched. `Escape` never reaches
  the Grid while a menu is open.
- **Commit and cancel** — opening commits nothing: no selection, no camera
  move, no draft saved, no state written. The first durable change is the
  operation the chosen row performs, and that operation owns its own undo. A
  destructive row that would permanently remove authored work raises the inline
  confirm owned by `10-grammar/Surface-classes.md` after the menu has closed,
  so the question is never asked underneath the list that raised it.

### Placing the menu

1. Compose the list from what the target supports right now, and measure the
   frame by the width and height procedure above.
2. Put the frame's top-left corner at the invoking point.
3. If the frame's right edge would pass the viewport width less `--sp-md`, put
   the frame's right edge at the invoking point instead.
4. If the frame's bottom edge would pass the viewport height less `--sp-md`,
   put the frame's bottom edge at the invoking point instead.
5. If an edge would still pass the margin after flipping, the list is longer or
   wider than the viewport allows and the target's list is corrected.

The menu flips; it never slides. A menu nudged back inside the viewport covers
the target it was opened on, and the target must stay visible.

### Placing the flyout

A flyout's top edge aligns with its parent row's top edge and its near edge
sits `--sp-sm` from the parent frame's side. It opens to the right; if its
right edge would pass the viewport width less `--sp-md` it opens to the left
instead, and the parent row's arrow points the way it went. If its bottom edge
would pass the viewport height less `--sp-md` it rises until it meets that
margin, giving up the top alignment rather than the side, because the side is
what says which parent it belongs to.

One flyout level, ever. A flyout never opens a flyout, and a flyout lists at
most nine destinations, because the digit is the quick key and a tenth
destination has no single stroke left to pick it.

Which rows a target offers, and what each row does, belong to the surface that
supplies the list. Row order within a target kind never changes, so position is
learned once.

## Motion

| Transition | Duration | Curve | Reduced motion |
| --- | --- | --- | --- |
| Menu arriving on open | `--d-fade` | `--ease` | Drawn complete at the invoking point on the frame it opens. |
| Menu leaving on dismiss or on a chosen row | `--d-fade` | `--ease` | Gone on the frame it is dismissed. |
| Flyout arriving on its parent row | `--d-fade` | `--ease` | Drawn complete beside its parent row on the same frame. |
| Flyout leaving | `--d-fade` | `--ease` | Gone on the frame the highlight leaves its parent row. |
| Highlight moving from row to row | None | — | No change; there was nothing to reduce. |

The highlight is not animated. `00-foundations/Motion.md` gives a transition
six jobs and a moving highlight has none of them, and a highlight that eases
lags the hand it is meant to be under.

`00-foundations/Motion.md` names the curve `--ease` and
`00-foundations/Tokens.md` projects the same value as `--ease`; Motion's name
is used here, because Motion owns which curve a transition takes.

Only `opacity` transitions. The frame's position, its width, and its height are
settled before the first frame is drawn, so nothing about the menu moves while
it is arriving. Nothing loops. Nothing idles. Nothing pulses or blinks.

## Distance

Thresholds are on projected cell size and belong to
`10-grammar/Representation-tiers.md`.

| Tier | Sheds | Keeps |
| --- | --- | --- |
| Working | Nothing. | Every part at its declared size. |
| Stepped | Nothing. | Every part at its declared size. |
| Stand-in | Nothing. | Every part at its declared size. |

A menu is chrome in screen space. It holds no cells, so projected cell size
says nothing about it, and it is drawn at the same size at every camera scale.
A menu that shrank with the camera would be answering the field instead of the
hand.

The menu does not follow its target when the camera moves. Its position is
derived once, from the invoking point, and a camera gesture while it is open
dismisses nothing and moves nothing.

## Accessibility

- **Role and name** — the frame is `role="menu"` and its accessible name is the
  target's own name: the placement's authored text, the file name, the title of
  the surface that declared it. A generic name such as `Actions` is a defect,
  because it replaces the one thing that says which target this list belongs
  to. Each row is `role="menuitem"` named by its own label; its printed key is
  exposed as `aria-keyshortcuts` so a key that is drawn is also announced. A
  row that owns a flyout carries `aria-haspopup="menu"` and `aria-expanded`. The
  menu carries `aria-activedescendant` naming the highlighted row, since the
  highlight is owned by the menu rather than by the rows. An unavailable row
  carries `aria-disabled="true"` rather than `disabled`, so it stays reachable
  and announced. Separators are `role="separator"`; the shadow, the frame edge,
  and the flyout arrow are hidden from assistive technology.
- **Contrast** — the row label at `--text-primary` reaches `10.36` on
  `--surface-chrome` and `9.84` on the highlight's `--surface-nested`. The
  quick key at `--text-meta` reaches `4.71` and `4.61`. The destructive label
  at `--signal-refusal` reaches `5.28` and `4.96`. The highlight edge at
  `--signal-interaction` reaches `8.90` and `8.37` — the state's non-text
  carrier, far above the `3:1` floor. The unavailable row at
  `--text-unavailable` reaches `2.46`, which
  `00-foundations/Accessibility.md` exempts for unavailable text and pairs
  here with a held position and a drawn edge. The frame edge at `--edge-quiet`
  reaches `1.54` and is exempt, because `--shadow-local` carries the menu's
  extent.
- **Without colour** — Approached and Focused: a fill step no other row carries
  and a stroke drawn on one row only. Engaged: the same drawing, held while the
  flyout is open. Unavailable: a held position in a held group, ink dropped a
  step, and no highlight edge when the fill reaches it. The destructive row:
  the last position, alone behind a hairline, which is the carrier — the
  refusal hue confirms it and never carries it alone. Selected, Pending,
  Refused, and Anchored draw nothing, so there is nothing to lose. Rendered in
  greyscale every one of these still reads.
- **Forced colours** — the frame fill, the frame edge, the row inks, the
  highlight fill, the highlight edge, the separators, the destructive label, and
  the unavailable ink are redeclared in system colours. `--shadow-local` does
  not survive, so the frame edge is the whole of the menu's containment in a
  forced-colours mode and is redeclared at full system contrast rather than at
  `--edge-quiet`. What survives without redeclaration is the structure: the
  highlight's stroke on one row, the separator's break between groups, the
  destructive row's position, and the flyout's digit column.
- **Text scaling** — rows grow taller and the frame grows wider to its widest
  row; nothing wraps, truncates, or clips at any scale. The placement procedure
  runs again on the grown box, so a menu that no longer fits where it opened
  flips rather than sliding under an edge. A list that cannot fit the viewport
  at 200% was too long at 100%.
- **Reduced motion** — no change from the Motion table.

## Copy

**None.** The menu writes nothing of its own: it has no title, no header, no
empty state, no status line, and no message. Every word inside it is a row
label supplied by the surface that owns the target, and each of those is
checked against `10-grammar/Copy.md` where it is written.

Three constraints the menu places on any label it is given: a verb or a verb
phrase for what the row does, sentence case, and no terminal punctuation. A
label that would wrap is a label that was too long before it reached the menu.

The flyout's digits and the flyout arrow are figures and geometry, not copy.

## Refusals

- **A title** — a title names the list rather than the target, and the thing
  that was clicked is already the title.
- **An icon, a glyph strip, or an icon-only row** — a row is a labelled word,
  and a picture makes a person decode before acting while hiding the quick key.
- **A scrim, a dim, a blur, or a tint behind the menu** — the Grid stays
  lit and live, because a menu is chrome answering the hand and not an
  interruption.
- **Any separation beyond `--shadow-local`** — a second shadow layer, an inset
  highlight line, or a glow claims a height Grove does not have.
- **A role hue on the frame** — the target has a role and the list does not, so
  a tinted frame would say the menu belongs to an operation family.
- **Translucency or a backdrop blur** — a fill that lets the Grid through
  makes the Grid an ingredient of the menu's own colour.
- **A second flyout level** — navigation wearing menu cloth, after which the
  digits stop saying which list they pick from.
- **A scrollbar, a wrapped row, or an ellipsis** — a list that does not fit is
  the wrong list for that target, never a shortened one.
- **A destructive row adjacent to the first row** — a fast hand aims at the
  first row by habit, and one slip must never remove authored work.
- **The refusal hue on a highlighted destructive row** — the highlight answers
  what `Enter` would fire, which is the interaction signal's one question, and
  repainting it in refusal would say the row cannot be chosen.
- **A checkbox, tick, radio, or state column** — a menu lists actions; a
  setting that toggles belongs to the surface that owns it.
- **A modifier chord printed as a quick key** — every printed key is one
  unmodified stroke, so a person never memorises a combination to use a list
  they can already see.
- **Accumulating typeahead** — a letter is a quick key and fires its row.
- **A menu opened by approach, or a row fired by hovering** — a list appears
  because a person asked for it and acts because a person chose a row.
- **A filler row, or a menu of one filler row** — a target with nothing to
  offer opens nothing.
- **Rows reordered between openings on the same kind of target** — position is
  learned once, and a list that reshuffles teaches a person to stop aiming.
- **A menu that outlives its target** — if the target goes, the menu closes.


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

- Catalogue deck: `docs/design_catalogue/src/01-menus.html`
- Catalogue deck: `docs/design_catalogue/src/information-plane/08-annotation-routes.html`
- Catalogue deck: `docs/design_catalogue/src/hud/03-gallery-slate.html`
- Catalogue deck: `docs/design_catalogue/src/hud/01-slate-anatomy.html`
- Catalogue stylesheet: `docs/design_catalogue/src/shared/catalogue.css`
- Reference: `docs/reference/Keybind map.md`
