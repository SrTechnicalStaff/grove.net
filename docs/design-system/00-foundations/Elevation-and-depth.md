---
type: design-system-foundation
status: active
date: 2026-08-09
tags: [grove, design-system, elevation-and-depth]
---

# Elevation and depth

Grove is flat. Nothing in the Grid has a height, nothing is nearer the eye
than anything else, and no surface pretends otherwise. What reads as depth is
produced by five carriers, none of which simulates a third dimension.

The Grid is 2D at every moment. Layers extend the mental model of the Grid so
that presence from one Layer reaches cells on another, and a person experiences
content on other Layers through that presence rather than through drawn sheets
stacked in space. That is the whole of Grove's depth, and it is a fact about
the field, not an illusion about height.

Anything that draws a z-axis is refused: a tilted plane, a parallax offset, a
surface that lifts toward the pointer, a visible stack of sheets, a perspective
transform, a shadow that grows with importance.

## The five carriers of depth

| Carrier | What it does | Governed by |
| --- | --- | --- |
| Tone | Steps a surface off what it sits on. | `--surface-grid`, `--surface-chrome`, `--surface-nested`, `--surface-page` |
| Border | Contains a surface and says where it ends. | `--edge-hairline`, `--edge-quiet`, `--edge-found` |
| Position | Puts a surface beside the thing it serves, so the relationship is read from where it is. | The owning plane |
| The field | Carries content on other Layers into the cells of this one. | `--field-*` |
| Shadow | Separates a floating surface from the Grid. | `--shadow-local` |

Use them in that order. Reach for a shadow only after tone, border, and
position have been used, because a shadow is the one carrier that survives
neither forced colours nor a printed page.

### Tone

There are exactly three dark steps: Grid, chrome, nested. A fourth is a
defect, so **nesting is capped at two levels inside any floating surface** — a
surface inside a surface inside a surface has no step left to take and must be
flattened or split.

A step of tone is always one step. Skipping from Grid fill straight to
nested fill inside one frame is a defect, because the missing step is the only
thing that said a frame was crossed.

Paper sits outside the dark ladder. A paper surface never takes a dark step and
never takes a cast shadow; it separates by being paper.

### Borders

Every floating surface carries a 1px containment border, without exception,
because the border is the only separation that survives forced colours, and a
surface known only by its shadow disappears there.

Border weight is 1px everywhere. A 2px stroke is a state — focus, selection, or
refusal — and never structure. A double stroke, a halo ring, or a second border
inside the first is decoration and is refused.

### Position

A surface that serves something on the Grid opens beside it, with the source
still visible. Locality is the strongest depth signal Grove has: a person reads
"this belongs to that" from adjacency, not from stacking.

A fixed capture surface has no source on the Grid and may sit centred in the
viewport. Centring is not the offence; dimming what is behind it is.

### The field

Presence is the depth of the Grid. A cell's value is its own content's hue plus
every hue saturating that cell from Layers above and below, summed per cell,
with hard edges at every cell boundary.

Fixed rules on the field as depth:

- Other Layers contribute presence only. Ghost frames, faint content, or
  readable text from another Layer is refused; it shows things that are not on
  this Layer.
- The field is never blurred, feathered, or drawn as a gradient. The cell is
  the field's pixel, and a soft edge promises a precision the measure does not
  have.
- The field never exceeds `--field-alpha-max`, so presence never outshines the
  content it points at.
- The field is drawn beneath content, never over it.

## The two shadows

Two shadows exist. A third is a defect, not a new step.

| Token | Used by | Reason |
| --- | --- | --- |
| `--shadow-local` | Local editor, viewer, reading surface, menu, flyout, fixed capture surface, and any strip or bar that floats beside the thing it is about. | Everything that floats and is not a Slate is small and sits next to its source. |
| None | Grid content, stand-ins, previews, ghosts, the marquee, the Grid cursor, the field, paper, nested surfaces, rows and cards inside a surface, inline confirms, and every part of the Grid's own chrome. | Nothing inside a frame, and nothing in the field, is above the Grid. |

Two figures make the rule unambiguous: both shadows are pure black, and
`--shadow-local` is `0 8px 24px rgb(0 0 0 / 0.40)`. Several catalogue decks
draw the local shadow at `0.35`, and others invent `0 12px 32px / 0.45`,
`0 16px 44px / 0.52`, and `0 22px 70px / 0.46`; the shipped stylesheets go as
far as `0 30px 100px / 0.55`. `Tokens.md` owns the values and wins in every
case — those variants are defects and map onto one of the two tokens.

Grid content casting a shadow is a specific and serious error: a placement's
appearance is its footprint under the camera, and a placement that casts a
shadow has floated free of the field and started lying about where it is.

## Shadows separate, never decorate

- One shadow per surface, on its outermost frame only. Nothing inside a
  shadowed surface casts a shadow of its own.
- One layer per shadow. The declared value is complete: an added
  `inset 0 1px 0 rgb(255 255 255 / 0.05)` top highlight is a bevel, and a bevel
  is decoration. Some decks and the shipped stylesheets carry that inset; they
  are corrected, because `Tokens.md` declares each shadow as a single layer.
- Shadows are black. A shadow tinted with a role hue is a glow wearing a
  shadow's name.
- A shadow never changes with state. It does not deepen on Approached, Focused,
  Selected, or Engaged, and it does not animate on its own — `States.md`
  forbids approach from restyling a component's own surface, and a lifting
  shadow is exactly that.
- A shadow never scales with the camera. It is drawn in screen space at one
  value at every zoom.
- Shadows do not stack. Where two floating surfaces overlap, each keeps its own
  single shadow and neither is restyled by the other.
- A shadow is never the only separator. Every shadowed surface also carries its
  containment border and its own tone.

## A glow is not a shadow

A glow is not available anywhere in Grove, in any hue, at any radius. A glow
implies emission, and only the field emits — as quantized cells, never as
light bleeding past an edge.

This resolves a direct disagreement in the catalogue. The design language deck,
the Slate anatomy deck, the local editor deck, and the image viewer deck all
state "never a glow", "no glow, no halo". Three Grid decks describe a selected
placement as carrying "a soft glow" and render
`box-shadow: 0 0 24px 6px rgb(150 182 248 / 0.45)`, and the shipped stylesheets
do the same for selected and traced placements. **The refusal wins.** Selection
carries exactly the three signals `States.md` names — outline outside the
content edge, the cells around the footprint brightening, and the content
itself untouched — and the glow is a fourth signal that says nothing the first
three do not.

Specific forms, all refused: an outer `box-shadow` with zero offset in any
non-black colour; `filter: drop-shadow` in a hue; a blurred duplicate of an
element behind itself; a soft radial wash behind a marker, a marker ring
expanding as a halo, or a pulsing outline.

The presence marker used on annotations is the one place a wide soft ring is
drawn in the shipped stylesheets. It is a defect. A marker separates by its
ring, its fill, and its solid core.

## An inset box-shadow is a border

`box-shadow: inset 0 0 0 1px …` draws a line inside the element's edge. It is a
border, it is governed entirely by the border rules above, and it is not
elevation. It is the correct technique wherever a 1px edge must not change an
element's box — a paper placement's inset edge at `--paper-edge`, a Note's
inner edge, the Grid cursor's ring.

An inset shadow with a blur radius is refused everywhere: an inner shadow is a
carved recess, and Grove has no recesses.

## Layering order

Order is fixed. A component does not choose its band; its class does.

Within the Grid plane, bottom to top:

| Band | Contents |
| --- | --- |
| 1 | The canvas fill. |
| 2 | Grid lines, all three tiers. |
| 3 | The field. |
| 4 | Stand-ins. |
| 5 | Placements. |
| 6 | Marks belonging to a placement: anchor ribbon or square, selection outline, approach chrome. Drawn outside or on the content edge, never over authored content. |
| 7 | Drawing for an open gesture: placement preview, ghost, marquee, cursor trail. |
| 8 | The Grid cursor head. |
| 9 | The focus ring. |

Within one floating surface, bottom to top:

| Band | Contents |
| --- | --- |
| 1 | Surface fill. |
| 2 | Containment border. |
| 3 | Nested surfaces. |
| 4 | Content. |
| 5 | A bar or strip inside the surface: inline confirm, refusal sentence. |
| 6 | A flyout opened from a row in the surface. |
| 7 | The focus ring. |

Across planes, bottom to top:

| Band | Contents | Reason |
| --- | --- | --- |
| 1 | The Grid plane and everything on it. | The Information layer exists above the Grid and the Grid does not know it exists. |
| 2 | Information plane surfaces. | They are locally coordinated to Grid content and must not cover the composed Grid furniture. |
| 3 | The HUD. | A Slate is composed by its host and always outranks a surface floating beside Content. |
| 4 | Menus and flyouts inside their owning plane. | Open order is resolved inside the plane band; no Information surface may promote above the HUD. |

Where two states would draw the same region, `States.md` decides. These bands
order parts and surfaces, not states.

Bands are not adjustable per component. A component that needs to escape its
band has the wrong surface class, and the class is corrected first. Opening an
Information surface after a Slate never raises the Information band above the
HUD band.

## One scene

A surface may separate itself from the Grid. It may not become a second
scene competing with the first. A reviewer applies these tests, and any single
failure is a defect:

| Test | Pass |
| --- | --- |
| Surround | The Grid is visible on all four sides. A Slate is inset from every viewport edge by at least `--sp-md`, decided here on the space scale because a surface that touches an edge has stopped being a surface in a Grid and become a screen. |
| Non-interference | Nothing outside the surface's own frame changes when it opens — nothing dims, blurs, tints, desaturates, scales, or shifts. |
| Liveness | The field keeps running behind it and stays usable, not merely visible. |
| Opacity | The fill is opaque. Translucency and `backdrop-filter` are refused: they make the Grid an ingredient of the surface's own colour, and a blurred Grid is a scrim by another name. This corrects the shipped stylesheets, which fill at `0.96`–`0.99` over a 12–18px backdrop blur. |
| Locality | A surface with a source on the Grid opens beside that source, never centred over it. |
| Singularity | One Slate composition on screen, one flyout level, one shadow per surface. |
| Quiet | The surface carries no ornament that the Grid does not also use: no gradient fill, no gloss, no bevel, no rounded corner beyond `--r-sm`, no drop-shadowed text. |

## No scrim, anywhere

A scrim is any wash placed between a surface and what is behind it: full
viewport or partial, any colour, any alpha, blurred or not. **Grove has none,
in any plane, in any state, for any surface.** Nothing in Grove is modal enough
to earn one, and dimming the Grid claims an interruption no Grove surface
makes.

Refused in every form:

- A dimmed or darkened Grid behind a Slate, a menu, an editor, a viewer, a
  reading surface, or a capture surface.
- A blurred or desaturated Grid behind anything.
- An invisible full-viewport layer that swallows pointer events, because the
  Grid behind must stay usable and not merely lit.
- Dimming one pane to emphasise another, dimming unselected cards to emphasise a
  selected one, or dimming a picture to show that it is chosen.
- A dark wash laid over an image to make text readable on top of it. Text never
  rides on a picture; details live off the frame.
- A centre-screen alert, dialog, or confirmation, with or without a wash.
  Refusals and confirmations are inline, in the surface that asked, beside the
  thing they are about.

The shipped stylesheets contain exactly one scrim: the Layer rename overlay,
which fills the viewport at `rgb(14 14 16 / 0.82)` behind a 3px backdrop blur.
It is a defect. Renaming is a text field in the surface that lists Layers.

## When this document is silent

A surface that appears to need a third shadow, a fourth tonal step, a glow, or
a scrim has been given the wrong surface class. Fix the class first; the depth
question disappears with it.

If a genuine gap remains, resolve it in the order `README.md` sets: an accepted
Decision record, then established desktop convention, then print and page
mechanics for reading surfaces. Where a printed page would carry no depth cue
at all, Grove carries none either.

## Refusals

| Refused | Reason |
| --- | --- |
| A third shadow value. | Two separations are all Grove distinguishes. |
| A glow, halo, or coloured shadow. | Only the field emits, and it emits in cells. |
| A shadow on Grid content, a stand-in, a preview, or the field. | These live in the field, not above it. |
| A shadow that changes with state or hover. | Depth is a property of the surface class, not of attention. |
| A shadow on a nested surface. | Depth inside a frame is tone, not elevation. |
| A bevel, inner shadow, or top highlight. | Grove has no lit edges and no recesses. |
| A fourth tonal step, or nesting past two levels. | The ladder has three rungs. |
| Translucent fills and backdrop blur. | Both make the Grid part of the surface's colour. |
| A scrim, dim, or blur behind anything. | No Grove surface is modal. |
| A z-axis effect: parallax, tilt, perspective, a lifting card, a visible stack. | The Grid is 2D and Layers are felt through presence. |
| Depth carried only by shadow. | Shadows do not survive forced colours; the border and the tone do. |

## Sources

- `DESIGN.md` — Elevation & Depth, Overlays & Chrome, Shapes.
- `00-foundations/Tokens.md` — Core · depth, surface tones, borders, field.
- `10-grammar/States.md` — the three carriers of Selected, and the rule that
  approach never restyles a component's own surface.
- `docs/raw/original-notes/Grove - Layers.txt` — the Grid is 2D; depth is
  presence summed across Layers.
- `docs/raw/original-notes/Grove - information layer.txt` — the Information
  layer sits above the Grid and the Grid does not know it exists.
- `docs/design_catalogue/src/00-design-language.html` — borders contain,
  shadows separate, never a glow.
- `docs/design_catalogue/src/grid-plane/05-presence-fields.html` — cells, not
  clouds; no gradient blob; presence never louder than content.
- `docs/design_catalogue/src/hud/01-slate-anatomy.html` — no scrim, no dimming,
  never floats free.
- `docs/design_catalogue/src/information-plane/01-local-editors.html` — soft
  subordinate shadow, separation only.
- `docs/reference/Surface seam and contract model.md` — plane ownership and
  camera relationships behind the cross-plane order.
