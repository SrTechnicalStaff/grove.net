---
type: design-system-component
status: active
date: 2026-08-09
component: Image viewer
plane: information
surface_class: local-editor
tags: [grove, design-system, component]
---

# Image viewer

One picture, opened whole beside the record it came from, with nothing to do
but look at it.

The surface never shows the words above. Its accessible name is the picture's
own name, and `10-grammar/Copy.md` already records `Open Image Viewer` as a
violation with `Open picture` as its correction.

## Anatomy

Every visible part, named, with the token that gives it its value. A part with
no token is a part with no contract.

| Part | Required | Value |
| --- | --- | --- |
| Frame | yes | `--surface-chrome`, opaque; `--r-sm`; `--sp-md` padding on every side; `--shadow-local`. |
| Role border | yes | 1px `--k-view-b`, the frame's only role signal. `10-grammar/Surface-classes.md` writes this token `--c-view-edge`; `00-foundations/Tokens.md` owns names and the name is `--k-view-b`. |
| Stage | yes | Fill `#0B0B0D`; 1px `--edge-quiet` on its own box; `--r-sm`; no padding. It clips the picture to its box, never to its radius, because Grove never rounds the corners off a picture. |
| Picture | yes | The complete source at its intrinsic proportions, centred in the stage at the current scale. No fill, no padding, no border, no radius — the source is the whole of it. |
| Metadata line | yes | `--sp-sm` beneath the stage. `--f-mono` 500 at `--t-label`, `--tr-mono`, case as written, `--text-meta`. One line, never wrapped, never shortened. |
| Anchor mark | no | None. See States · Anchored. |
| Control strip | yes | One row, `--sp-sm` beneath the metadata line, `--sp-sm` between items, items centred on the row. |
| Viewing control | yes | `--f-ui` 400 at `--t-caption`; ink `--k-view`; fill `--surface-grid`; 1px `--edge-found`; `--r-sm`; padding `--sp-xs` top and bottom, `--sp-sm` at the sides. |
| Zoom slider | yes | A `74px` run: track 1px `--edge-found` across it, thumb `6 × 10px` filled `--k-view` at `--r-sm`, centred on the track. |
| Scale readout | Only above the fit scale | `--f-mono` 500 at `--t-label`, `--tr-mono`, `--text-meta`, last item in the strip. |
| Playback control | Only when the source animates | A viewing control whose label is `Play` or `Pause`, first item in the strip. |
| Close | yes | A viewing control in `--text-primary`, at the right end of the strip, with the key hint `Esc` beside it in `--f-mono` 500 at `--t-micro`, `--tr-mono`, `--text-meta`. |
| Retry | Unavailable only | A viewing control in `--text-primary`, on the stage beneath the sentence. |
| Stage words | Unavailable only | The picture's name in `--f-ui` 400 at `--t-dense`, `--text-primary`; one sentence beneath it in `--f-ui` 400 at `--t-caption`, `--text-secondary`, at `--measure-reading`. Centred in the stage. |
| Focus ring | Focused only | `--focus-ring` at `--focus-ring-offset`, drawn outside the focused part. |

Two figures are this component's own. `#0B0B0D` is the viewing-stage fill:
three decks carry it and carry nothing else — `information-plane/02-image-viewer.html`,
`information-plane/08-annotation-routes.html`, and `01-menus.html` — so it is a
decision, not a stray. **No token carries it and `--surface-stage` should.** It
is not a fourth rung of the tonal ladder in `00-foundations/Elevation-and-depth.md`:
that ladder climbs away from the canvas for surfaces that nest, and this is one
step below the canvas so the picture is always the brightest thing in the frame.
The shipped `rgb(7 7 9 / 0.8)` is a different value and translucent, so it does
not stand against three unanimous decks. `74px` is the slider run, the deck's
figure, kept because it is the shortest run on which the strip still holds
`Fit`, `100%`, `−`, the track, `+` and the readout on one line without wrapping.

The stage's edge is drawn although the deck omits it: `--surface-chrome` against
`#0B0B0D` measures 1.09:1, which cannot say where the stage ends while the
picture is letterboxed, and `css/information-plane.css:213` already draws one.

The viewer has no header, no title line, no tab bar, no status bar, no second
exit, and no control that is present outside the strip.

## Geometry

- **Footprint** — none in cells. The viewer is a screen-positioned surface
  derived once from the source's address, and `00-foundations/Principles.md`
  Law 8 binds placements, not chrome. Its extent is executed as a procedure:

  1. Read the source's intrinsic pixels, `W × H`.
  2. Measure the room beside the source: `Rw` is the distance from the near
     edge of the source's projected footprint, plus the `20px` offset, to the
     viewport edge less `--sp-md`, less `2 × --sp-md` of frame padding. `Rh` is
     the viewport height less `2 × --sp-md`, less `2 × --sp-md` of frame
     padding, less the metadata line, the strip, and the two `--sp-sm` gaps
     between them.
  3. The stage is `min(W, Rw)` wide by `min(H, Rh)` high.
  4. The frame is the stage plus `2 × --sp-md` on both axes.
  5. The fit scale is `min(stage width ÷ W, stage height ÷ H)`, which is at
     most `1` by construction, so the complete frame is visible at open and a
     small picture sits at its natural size rather than upscaled.

  Nothing in the procedure reads the camera, so the same picture beside the
  same source returns the same frame twice.

- **Growth** — the frame grows to the room and stops; past that the picture
  scales down uniformly to fit and is never cropped, because a viewer that runs
  out of room may show a smaller picture and may not show a smaller part of one.
  Letterbox space appears on whichever axis was not the binding constraint, and
  it is honest space: `00-foundations/Principles.md` Law 9 forbids the missing
  picture, not the empty matte. The stage never scrolls; zoom and pan are the
  depth this surface has.
- **Measure** — the viewer sets one column of prose, the sentence in the
  Unavailable state, at `--measure-reading`. The metadata line is a single
  monospace line and takes no measure, because a metadata line that wraps has
  become a sentence.
- **Alignment** — the frame's near edge sits `20px` clear of the source's
  projected footprint and the frame is centred on the source's vertical centre;
  it flips to the source's other side rather than sliding under a viewport
  edge. `20px` is the offset three Information-plane surfaces already share in
  `css/information-plane.css` — the viewer, the text editor, and the annotation
  reader — against single uses of `18px` and `24px`, so the dominant value is
  the design. No token carries it and `--offset-local` should.

## States

All nine.

| State | Appearance | Notes |
| --- | --- | --- |
| Rest | The frame, the stage, the complete picture at the fit scale, the metadata line, and the strip. Nothing else. | The strip is not chrome that leaves: it lives inside a surface a person opened, and it is the whole reason the surface is open. |
| Approached | No change from Rest. | The frame is already open; approach has nothing to add, and a control that appeared under the pointer would have no keyboard equal. |
| Focused | The focused part takes `--focus-ring` at `--focus-ring-offset`, drawn outside it. The frame itself never draws a ring. | Focus is per part — the stage, then each control in the strip — because the frame is a surface and not a target. |
| Selected | Not reachable. The viewer is chrome and is never the thing a person is working on; the picture it shows is selected on the field, not here. | Opening the viewer changes no selection, and closing it returns the invoking surface's selection untouched. |
| Engaged | While a drag pans the picture, the picture tracks the pointer inside the stage and the pointer is captured by the stage. While a control is pressed, that control takes the pressed appearance over `--d-press`. | A pan has a subject, so it draws in no signal hue at all; nothing is being selected, moved, or created. |
| Pending | The stage holds its extent, the metadata line and the strip are already drawn, and the stage carries the fullest form already decoded. | Never blank, never a spinner. The decoded picture replaces the pending form in place over `--d-swap`. |
| Refused | Not reachable. The viewer commits nothing and destroys nothing, so there is no attempt for Grove to answer. | A zoom or a pan at its limit clamps and the control at that limit goes Unavailable; a limit is a standing condition, not an answer to an attempt. |
| Unavailable | Whole surface: the stage keeps its extent and carries the picture's name, one sentence, and `Try again`; its edge goes to `--edge-found` so the empty stage is still bounded. The viewing controls keep their positions in `--text-unavailable` on a `--edge-hairline` border. `Close` stays available. | Reached when the source cannot be shown. No refusal hue anywhere: `10-grammar/Signal-roles.md` rules that content which will not load carries none, because nothing was refused. Single control: `+` is Unavailable at the ceiling, `−` and `Fit` at the fit scale. |
| Anchored | No change from Rest. The viewer draws no anchor mark. | The mark belongs to the placement and to the Memory record, where `00-foundations/Marks.md` already puts it; the only place left inside the viewer is the stage, and a mark on the stage would ride on the picture. |

Combination follows `10-grammar/States.md` without exception. Focused plus
Unavailable keeps the ring on the unavailable control, because an unavailable
control still takes focus and still says what is missing.

## Behaviour

- **Pointer** — a wheel over the stage zooms about the pointer and the event
  never reaches the Grid, so the Camera does not move. A press-and-drag on the
  stage pans, tracking the pointer frame by frame and committing nothing on
  release. A double-click on the stage toggles the fit scale and `100%`. A click
  on a control acts on pointer-up over it. `Fit` restores the complete frame
  from any scale and any pan position in one action.
- **Keyboard** — `+` and `=` zoom in, `-` zooms out, `0` fits the complete
  frame; all three are `docs/reference/Keybind map.md` and none is redefined
  here. `Escape` closes the viewer. While the stage holds focus the arrow keys
  pan by `--sp-xl` and `Shift` with an arrow pans by the stage's own extent on
  that axis less `--sp-xl`, which is the desktop page convention. `Enter` and
  `Space` activate the focused control, which is how playback is reached from
  the keyboard; no key is bound to playback, because a control already in the
  tab order needs none. Focus arrives on the stage and leaves to the strip.
- **Focus order** — the stage, then the strip in reading order: the playback
  control when the source animates, `Fit`, `100%`, `−`, the slider, `+`, then
  `Close`. The scale readout is not focusable, because it is a figure and not a
  control.
- **Escape** — Escape closes the viewer and only the viewer, and focus returns
  to the surface that opened it. A menu opened inside the viewer closes first.
- **Commit and cancel** — nothing here is durable. Scale, pan position, and
  playback are presentation and are never written to the Memory, the placement,
  or the source bytes; the record keeps its revision and its bytes across every
  open, every zoom, and every pause. There is nothing to cancel, because there
  is nothing to commit.

The return path is the invoking surface's, restated nowhere: a Gallery wall
regains focus with the same scroll position and the same tile selected, a
Memory record regains focus with its detail open, and a placement returns
keyboard attention to the Grid cursor at its origin cell. The Grid, the Camera,
and every Slate are untouched by the whole visit. The viewer closes if its
source is removed, per `20-planes/Information-plane.md`.

## Motion

| Transition | Duration | Curve | Reduced motion |
| --- | --- | --- | --- |
| The viewer opening | `--d-fade` | `--ease` | Present at the identical threshold, with no fade. |
| The viewer closing on Escape or Close | `--d-fade` | `--ease` | Gone on the frame the dismissal is pressed. |
| The decoded picture replacing the pending form | `--d-swap` | `--ease` | The decoded picture in place on the same frame, at the same threshold. |
| Pointer-down on a control | `--d-press` | `--ease` | The pressed appearance applied on press and removed on release, with no scaling. |
| A zoom step from `+`, `−`, `Fit`, `100%`, a key, or a double-click | None | None | No change; there was no transition to remove. |
| Pan under the hand, and zoom under the wheel | None — continuous rendering | None | Unchanged; it depicts a current value rather than a change over time. |
| Playback of an animated source | The source's own timing | The source's own | Opens paused on a complete still frame with `Play` offered. |

A zoom step lands in one frame with no travel, because
`00-foundations/Motion.md` gives a view change no step of the scale and refuses
a view that flies while a person waits to find out where they are.

Nothing loops. Nothing idles. Nothing pulses or blinks. An animated source's own
motion is the one exception and it is not Grove's: it is content a person
placed, it plays only inside the stage, and everything around it is still.

## Distance

The viewer holds no representation tier. It is drawn in screen space at one
size, the camera never reprojects it, and it does not move when the camera
moves. `10-grammar/Representation-tiers.md` binds the placement the viewer was
opened from, and that placement may demote to a stand-in behind an open viewer
without changing anything the viewer draws.

| Tier | Sheds | Keeps |
| --- | --- | --- |
| Working | Nothing. | The frame, the stage, the complete picture, the metadata line, the strip. |
| Stepped | Nothing. | All of the above, unchanged and at the same size. |
| Stand-in | Nothing. | All of the above, unchanged and at the same size. |

Presence, position, and extent are never shed — of the placement, which keeps
them while the viewer is open, and of the picture, which is complete on the
stage at every scale. The viewer has no kind-coded stand-in because it is never
distant.

## Accessibility

- **Role and name** — the frame is a dialog with `aria-modal="false"`, which is
  the ARIA for a surface with its own focus scope that blocks nothing; its
  accessible name is the picture's authored name. The stage is `role="img"`
  named the source name then the kind — `Harbor at dusk, GIF` — matching
  `30-components/Picture.md`. `View Image`, `Image Viewer`, or any surface class
  name is a defect: it replaces the one thing that distinguishes this picture
  from every other. The live region announces the failure to show a source and
  the return of a decoded one, and announces nothing about scale, pan, or
  playback, because a person can see all three.
- **Contrast** — the metadata line and the scale readout at `--text-meta` on
  `--surface-chrome`: 4.71:1. A viewing control's label in `--k-view` on its own
  `--surface-grid` fill: 9.33:1, and 8.75:1 where it meets the frame.
  `Close` and `Try again` at `--text-primary` on their own fill: 10.83:1. The
  stage words at `--text-primary` and `--text-secondary` over `#0B0B0D`: at
  least 10.83:1 and 6.53:1, the canvas figures, because the stage is darker than
  canvas. The focus ring on chrome: 8.90:1. The role border at 2.77:1 inside and
  2.95:1 outside is surface treatment and not a meaning-bearing edge: the frame
  is located by its tone, its position beside the source, and its shadow, and
  the View role is repeated in every control label. The control border at
  `--edge-found` is 1.81:1 and is likewise decorative, because the control is
  identified by a label that clears 8.75:1. `--text-unavailable` at 2.46:1 is
  the declared exception in `00-foundations/Accessibility.md`, paired with a
  retained position and a hairline border.
- **Without colour** — Approached: nothing to lose, nothing changes. Focused: a
  ring outside the part, at a distinct offset. Engaged: the picture moving under
  the hand, and the pressed control. Pending: the stage holding its extent.
  Unavailable: the retained positions, the hairline borders, the name, and one
  plain sentence. Anchored: no mark to lose. The View role hue confirms what the
  surface is; it never carries it, because a viewer is told from a text editor
  by holding a picture and no draft field.
- **Forced colours** — redeclared in system colours: the frame fill, the role
  border, the stage fill and edge, every control fill, border and label, the
  focus ring, and the unavailable ink. The picture is a raster and survives. The
  shadow does not survive and is not asked to: the border and the tone carry the
  frame.
- **Text scaling** — the metadata line, the control labels, the scale readout,
  and the stage words are interface text and grow to 200%. The strip wraps to a
  second row before any label shortens, the frame grows to its maximum width and
  then the stage gives up height, and the metadata line wraps rather than
  truncating — a line that cannot fit at 200% was too long at 100%. The picture
  does not scale with interface text: it is measured against the stage and
  answers only the scale controls.
- **Reduced motion** — no change from the Motion table. An animated source opens
  paused on a complete still frame, which is the same frame the playing source
  settles on, so nothing is unreachable and `Play` starts it.

## Copy

Every user-visible string this component can show, verbatim.

| String | Where | Why it passes |
| --- | --- | --- |
| `Fit` | The control that restores the complete frame. | A verb for an action, ordinary English, sentence case, no terminal punctuation. |
| `100%` | The control that sets one source pixel to one pixel. | A figure, not prose; it names the size a person gets. |
| `−` | Zoom out. Accessible name `Zoom out`. | A character every viewer uses for the same act, with a name for the hand that cannot see it. |
| `+` | Zoom in. Accessible name `Zoom in`. | The same. |
| `Zoom` | The slider's accessible name. | Names what the control does, in one ordinary word. |
| `Play` | The playback control while an animated source is paused. | A verb for an action, true after any rebuild. |
| `Pause` | The playback control while an animated source is playing. | The same. |
| `Close` | The one exit, with the key hint `Esc` beside it. | Names the act, not the surface. |
| `Esc` | The key hint beside `Close`. | The key's own name. |
| `Try again` | The recovery on the stage when a source cannot be shown. | Verb phrase, sentence case, names what happens next. |
| `This Image can't be shown right now. The Memory is safe — nothing was changed or removed.` | The stage, when a source cannot be shown. | States the condition rather than the failure, blames nobody, uses no code or jargon, and says the one thing a person needs to know. |

The metadata line is not copy. It is the picture's authored name followed by
figures read from the source — kind, pixel dimensions, byte size, date — joined
by a middle dot. Grove writes none of it, re-cases none of it, and shortens none
of it.

Two corrections to the deck's strings. `Image` and `Memory` are capitalised
because they are Grove's own nouns. `Back` in `index.html:149` is replaced by
`Close`, because a viewer is not a place a person navigated into.

## Refusals

- **A centred frame over a dimmed Grid** — a box floating centre-screen
  detaches the picture from the record it belongs to, and a dim claims an
  interruption Grove does not make.
- **Cover-fit, crop, or any fill that loses an edge** — the complete frame
  always fits, and letterbox space is honest where missing picture is not.
- **Upscaling past `100%` at open** — a small picture blown up to fill a stage
  reports detail the source does not have.
- **Crop, rotate, straighten, draw, or any control that changes pixels** — this
  frame only looks, and changing what a person placed belongs to a surface that
  says so.
- **A full-screen control** — a surface that fills the viewport covers the
  source it serves and blocks the Grid, which is the modal Grove refuses.
- **A kind badge on the stage** — the metadata line beneath already carries the
  kind as a word, and `00-foundations/Marks.md` draws the badge only where there
  is nowhere else for the word to go.
- **Anything drawn over the picture** — no name, no dimensions, no scale, no
  mark, no wash to make text readable on it; details live off the frame.
- **A shortened name, an ellipsis, or a second title** — a name cut to fit is
  information lost, and a name set large above the picture is a title nobody
  wrote.
- **A second exit** — a close control in a header beside the one in the strip
  makes a person choose between two identical acts.
- **A spinner, a progress bar, or a percentage while a source decodes** — a
  waiting surface holds the fullest form it already has.
- **A refusal hue on a source that will not show** — nothing was refused, and a
  hue that also means "this did not arrive" has stopped being a signal.
- **Writing scale, pan, or playback to the Memory** — presentation is not
  content, and a picture that remembers how it was last looked at has been
  edited by being read.
- **A scrollbar on the stage** — depth inside a picture is zoom and pan, and a
  scrollbar hides part of what a person placed.


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

- Catalogue deck: `docs/design_catalogue/src/information-plane/02-image-viewer.html` — the contract for this component: the complete fitted frame, the View role border, the control strip, the metadata line, playback and its reduced-motion still frame, the return, and the four refusals.
- Catalogue deck: `docs/design_catalogue/src/information-plane/01-local-editors.html` — one class, one variable: the 1px role border is the only per-role change, and a viewer's controls read in the View fill.
- Catalogue deck: `docs/design_catalogue/src/grid-plane/04-image.html` — playback is transient and never saved with the picture; reduced motion holds a complete still frame at the same size and place.
- Catalogue deck: `docs/design_catalogue/src/hud/03-gallery-slate.html` — the kind declared as a word in the identity line, and the wall a close returns to.
- Catalogue deck: `docs/design_catalogue/src/information-plane/08-annotation-routes.html`, `docs/design_catalogue/src/01-menus.html` — the second and third uses of the viewing-stage fill.
- Component: `docs/design-system/30-components/Picture.md` — the placement this viewer opens, its double-click route, and the ruling that an unreadable source is Unavailable rather than Refused.
- Reference: `docs/reference/Keybind map.md` — `+`, `=`, `-`, `0`, and `Escape`.
- Wireframe: `docs/ux/wireframes/image-viewer/Image Viewer.md` — IV-00 through IV-05 and the photos-app parity boundary.
