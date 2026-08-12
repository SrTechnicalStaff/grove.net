---
type: design-system-foundation
status: active
date: 2026-08-09
tags: [grove, design-system, tokens]
---

# Tokens

Two tiers. **Core** tokens name a value. **Semantic** tokens name a use, and
resolve to a core token. Product CSS may only use semantic tokens; a raw
colour, radius, duration, or tracking value in a product stylesheet is a
defect. Core tokens exist so the semantic layer has one place to change.

Component-level values appear only where a component's anatomy is specified
to an exact figure that no other component shares. They live in the component
specification, not here, and they are still expressed against core tokens.

A third tier is deliberately refused. Grove is one product on three planes,
not a multi-brand system, and a per-component token layer would cost more
indirection than it removes.

## Core · colour

Surfaces and inks are declared twice: as hex for direct use, and as an RGB
triplet where alpha composition is needed.

| Token | Value | Triplet token |
| --- | --- | --- |
| `--c-base` | `#0E0E10` | `--base` `14,14,16` |
| `--c-grid-min` | `#161618` | — |
| `--c-surface-raised` | `#1C1C20` | — |
| `--c-grid-maj` | `#242428` | — |
| `--c-text` | `#EAEAEA` | `--ink` `234,234,234` |
| `--c-paper` | `#F5F5F5` | — |
| `--c-paper-ink` | `#1A1A1A` | `--paper-ink` `26,26,26` |
| `--c-select` | `150 182 248` | — |
| `--c-invalid` | `226 98 92` | — |
| `--c-marquee` | `232 185 100` | — |
| `--c-anchor` | `158 140 234` | — |

`--c-grid-min` carries two jobs: the quietest surface in the Grid and the
quietest line in the field. They are one value on purpose, one step off
canvas, so content is always brighter than the field beneath it. Reach for it
through `--surface-chrome` or `--grid-minor-ink`, never by its core name.

`--c-surface-raised` is the fill for a surface that sits above another
surface — a card inside a Slate, a row inside a flyout. It is the only third
dark step; a fourth is a defect.

Signal colours are declared as space-separated triplets because every use
composes them with an alpha from the ink ramp.

### Role hues

Role hues identify an operation family. They are signals, never categories a
person authors.

| Token | Fill | Border |
| --- | --- | --- |
| Tool | `--k-tool` `#B3A9E0` | `--k-tool-b` `#5B5288` |
| View | `--k-view` `#9BB6E0` | `--k-view-b` `#3F5F8A` |
| Layer | `--k-layer` `#E2A6C6` | `--k-layer-b` `#8A3F63` |
| Edit | `--k-edit` `#E0A9A3` | `--k-edit-b` `#7A3F3A` |
| Slate | `--k-slate` `#CDB8D8` | `--k-slate-b` `#6B5A78` |

### Authored Note colours

A Note carries one of three authored fills. There are three, not a palette,
so a colour stays a choice a person can remember.

| Fill token | Value | Presence token | Triplet |
| --- | --- | --- | --- |
| `--c-note-violet` | `#6E62A6` | `--c-note-violet-field` | `110 98 166` |
| `--c-note-clay` | `#B0524E` | `--c-note-clay-field` | `176 82 78` |
| `--c-note-slate-blue` | `#4E6E9C` | `--c-note-slate-blue-field` | `78 110 156` |

**The presence a Note casts is its own fill, not a brightened alias of it.**
A Note may not radiate `158 140 234` or `226 98 92`, because those are the
Anchor and Invalid signals and a signal that also means "a person picked this
colour" has stopped being a signal. This resolves a disagreement between the
catalogue decks in favour of the whole-field rendering, which casts `#6E62A6`
as `rgb(110 98 166)`.

## Core · ink ramps

Alpha over the `--ink` triplet on dark surfaces. Nine steps; a tenth is a
defect.

| Token | Alpha | Use |
| --- | --- | --- |
| `--ink-whisper` | `0.04` | Watermark scale, ambient marks. |
| `--ink-hairline` | `0.10` | Group separators, the quietest border. |
| `--ink-edge` | `0.16` | The default 1px containment edge. |
| `--ink-quiet` | `0.22` | A border that must be found without being read. |
| `--ink-faint` | `0.30` | Unavailable actions, spent state. |
| `--ink-tertiary` | `0.51` | Metadata, counts, supporting figures. The lowest step legal for text: `0.45` reaches only 3.96:1 on canvas and fails at every size Grove sets. |
| `--ink-secondary` | `0.62` | Secondary reading text. |
| `--ink-primary` | `0.82` | Primary reading text on dark surfaces. |
| `--ink-full` | `1` | Focus rings, the cursor ring, full-strength marks. |

Alpha over `--c-paper-ink` on paper surfaces. Seven steps.

| Token | Alpha | Use |
| --- | --- | --- |
| `--paper-texture-minor` | `0.05` | Minor rule of the page texture. |
| `--paper-texture-major` | `0.08` | Major rule of the page texture. |
| `--paper-edge` | `0.10` | The inset edge of a paper placement. |
| `--paper-rule` | `0.16` | A horizontal rule on paper. |
| `--paper-border` | `0.28` | A drawn box on paper, and the stand-in edge. |
| `--paper-label` | `0.62` | Front matter and labels. Front matter separates from body by type role — mono, uppercase, `--t-label`, `--tr-caps` — not by ink, because ink faint enough to separate it reaches only 3.08:1. |
| `--paper-body` | `0.62` | Reading text on paper. |

`--paper-strong` `0.82` is reserved for the resize corner and any mark that
must survive at the smallest legible size.

## Core · typography

Three families, from `DESIGN.md`: `--f-display` Oswald 500, `--f-ui` Inter
400, `--f-mono` JetBrains Mono 500.

Nine sizes. The shipped product currently uses thirteen unrelated sizes; these
nine are the whole scale, and every existing value maps onto one.

| Token | Size | Role | Replaces |
| --- | --- | --- | --- |
| `--t-micro` | `9px` | Mono badge on a frame. | `8px` |
| `--t-label` | `11px` | Mono label, front matter, title block. | `10px` |
| `--t-caption` | `12px` | Secondary interface text. | — |
| `--t-dense` | `13px` | Dense interface text, the closing line on a page. | — |
| `--t-body` | `15px` | Reading default: Note text, abstract, editor body. | `14px`, `16px` |
| `--t-lead` | `17px` | A lead paragraph or a single emphasised line. | — |
| `--t-title-small` | `20px` | Surface titles. | — |
| `--t-title` | `30px` | A reading surface title. | — |
| `--t-display` | `38px` | The display title on a placed Document. | — |

Six tracking steps. Nineteen are currently in use.

| Token | Value | Use |
| --- | --- | --- |
| `--tr-display` | `0.02em` | Display type. |
| `--tr-title` | `0.03em` | Titles set in display or UI type. |
| `--tr-mono` | `0.08em` | The monospace default. Replaces `0.04`–`0.07` and `0.09`. It is the most-used tracking value in both the shipped product and the catalogue; the `0.07em` that `DESIGN.md` carried was stale. |
| `--tr-label` | `0.12em` | Short uppercase labels. Replaces `0.10`, `0.13`–`0.15`. |
| `--tr-wide` | `0.16em` | Uppercase micro labels. Replaces `0.18`. |
| `--tr-caps` | `0.20em` | Front matter and title-block capitals. Replaces `0.22`. |

Four leading steps: `--lh-tight` `0.98` for display titles, `--lh-snug`
`1.42` for authored Note text, `--lh-ui` `1.5` for interface text, and
`--lh-reading` `1.65` for continuous prose.

Measure is set in characters, never in surface width: `--measure-reading`
`34ch`. A reading column that is not expressed in `ch` is a defect.

## Core · shape

| Token | Value | Use |
| --- | --- | --- |
| `--r-none` | `0` | Placements. Content on the Grid is never rounded. |
| `--r-sm` | `2px` | Every piece of Grove chrome, without exception. |
| `--r-round` | `50%` | Point markers only, where the mark denotes a position rather than a region. |

There is no third rounding value. The shipped product currently uses ten;
`3px`, `5px`, `6px`, `7px`, `9px`, `11px`, `12px`, and the asymmetric
`2px 2px 0 0` are all defects and migrate to `--r-sm`.

## Core · space

| Token | Value |
| --- | --- |
| `--sp-xs` | `4px` |
| `--sp-sm` | `8px` |
| `--sp-md` | `16px` |
| `--sp-lg` | `24px` |
| `--sp-xl` | `32px` |

The scale governs rhythm between things. A specified anatomy figure — the
`26px 28px` padding of a placed Document, the `16px 15px` of a Note — is a
component value and belongs in that component's specification.

## Core · motion

| Token | Value | Use |
| --- | --- | --- |
| `--d-press` | `90ms` | Pointer-down acknowledgement. |
| `--d-fade` | `120ms` | Chrome answering approach or leaving. |
| `--d-swap` | `160ms` | Exchanging one representation for another. |
| `--d-exit` | `200ms` | Something leaving the Grid. |
| `--d-place` | `280ms` | Arrival and placement confirmation. |
| `--d-sweep` | `480ms` | A single expanding acknowledgement. |

`320ms`, `360ms`, and `440ms` are in the shipped product and have no distinct
job; they migrate to the nearest step.

`--ease` `cubic-bezier(0.25,0.1,0.25,1)` carries everything. `--overshoot`
`cubic-bezier(0.2,1.25,0.3,1)` is reserved for placement confirmation and
arrival. There is no third curve.

Every duration token has a reduced-motion equivalent of `0`. Meaning never
lives only in the transition.

## Core · depth

| Token | Value | Use |
| --- | --- | --- |
| `--shadow-local` | `0 8px 24px rgb(0 0 0 / 0.40)` | A surface separating itself from the Grid beside its source. |

One shadow. A Slate fills the screen and has nothing to separate from, so it casts none. Shadows separate; they never decorate, and they never imply a
height Grove does not have. A glow is not a shadow and is not available.

## Semantic tokens

Product CSS uses these names. Each resolves to a core token.

### Surface and text

| Token | Resolves to | Use |
| --- | --- | --- |
| `--surface-grid` | `--c-base` | The field itself. |
| `--surface-chrome` | `--c-grid-min` | Slates, flyouts, local editors, bars. |
| `--surface-nested` | `--c-surface-raised` | A surface inside a surface. |
| `--surface-page` | `--c-paper` | Any reading or written surface. |
| `--text-primary` | `--ink` @ `--ink-primary` | Reading text on chrome. |
| `--text-secondary` | `--ink` @ `--ink-secondary` | Supporting text. |
| `--text-meta` | `--ink` @ `--ink-tertiary` | Counts, sizes, coordinates. |
| `--text-page` | `--paper-ink` @ `--paper-body` | Reading text on paper. |
| `--text-unavailable` | `--ink` @ `--ink-faint` | An action present but not available. |

### Borders

| Token | Resolves to | Use |
| --- | --- | --- |
| `--edge-hairline` | `--ink` @ `--ink-hairline` | Separating groups within one surface. |
| `--edge-quiet` | `--ink` @ `--ink-edge` | The default containment edge on a dark surface. |
| `--edge-found` | `--ink` @ `--ink-quiet` | An edge that must be locatable. |
| `--edge-on-color` | `255 255 255` @ `0.12` | The containment edge on an authored colour fill. |
| `--edge-on-page` | `--paper-ink` @ `--paper-edge` | The containment edge on a paper surface. |

An edge takes its ink from the surface it sits on, not from one global value.
`--edge-on-color` is pure white rather than a step of the `--ink` ramp because
the warm neutral muddies against a saturated fill; four catalogue decks and
the shipped product agree on it, which makes it design rather than drift.

### Signals

Three signals, three jobs, no overlap. A signal is never the only carrier of
its meaning: each pairs with structure, position, or words.

| Token | Resolves to | Means | Never means |
| --- | --- | --- | --- |
| `--signal-interaction` | `--c-select` | This is what you are working on. | A category, a warning, a result. |
| `--signal-active-work` | `--c-marquee` | A gesture is open right now. | A result, a warning, a category. |
| `--signal-refusal` | `--c-invalid` | This cannot happen. | Danger, error severity, emphasis. |
| `--signal-authored-context` | `--c-anchor` | Authored context is present. | A colour a person picked. |

Keyboard attention off the Grid uses `--focus-ring` (`2px solid` the
interaction hue at `--ink-full`) drawn at `--focus-ring-offset` `2px`. It is
applied on `:focus-visible` only, is never suppressed, and is drawn on top of
any other state's outline. On the Grid, keyboard attention is the Grid cursor;
a placement draws no second ring.

### Field

| Token | Value | Use |
| --- | --- | --- |
| `--field-gain` | `0.22` | Visible strength multiplied into cell alpha. |
| `--field-alpha-min` | `0.025` | A lit cell is never fainter than this. |
| `--field-alpha-max` | `0.30` | A lit cell never exceeds this, so the field never outshines content. |
| `--field-perimeter-width` | `1.5px` | The ring on the outer edge of an occupied region. |
| `--field-perimeter-ink` | `0.25` | Perimeter alpha in the region's own hue. |
| `--field-perimeter-selected` | `0.80` | Perimeter alpha when the region is selected. |

### Grid

| Token | Value | Use |
| --- | --- | --- |
| `--grid-cell` | `220px` | One cell. The unit of every footprint. |
| `--grid-subdivisions` | `5` | Minor lines per cell. |
| `--grid-supercell` | `5` | Cells per supercell. |
| `--grid-fade-start` | `6px` | A tier is invisible at or below this screen spacing. |
| `--grid-fade-end` | `14px` | A tier is at full ink at or above this screen spacing. |
| `--grid-minor-ink` | `--c-grid-min` | Minor line colour. |
| `--grid-major-ink` | `--c-grid-maj` | Major and supercell line colour. |
| `--cell-size` | `220px` | Retained alias of `--grid-cell`, consumed by the cell layer in stylesheets that predate the semantic name. |

A tier's ink is `clamp((spacing − 6) ÷ 8, 0, 1)`. One rule sets all three
tiers; there is no second grid.

### Cursor

| Token | Value | Use |
| --- | --- | --- |
| `--cursor-ring` | `2px` | The inset ring of the cursor head. |
| `--cursor-ring-ink` | `0.88` | Ring alpha. |
| `--cursor-fill-gain` | `0.22` | Energy multiplied into the head fill. |
| `--cursor-steady` | `0.6` | Head energy at rest. |
| `--cursor-trail-decay` | `0.84` | Per-frame decay of a spent cell. |
| `--cursor-trail-min` | `0.03` | Below this a spent cell is dropped. |

### Distance

| Token | Value | Use |
| --- | --- | --- |
| `--tier-standin-demote` | `18px` | Below this projected cell size, a placement is a stand-in. |
| `--tier-standin-promote` | `28px` | At or above this, a stand-in becomes a stepped form. |
| `--tier-detail-demote` | `56px` | Below this, surface treatment sheds. |
| `--tier-detail-promote` | `72px` | At or above this, the working form returns. |

Thresholds are measured on **projected cell size**, so the whole field sheds
together rather than by placement size. Demote and promote differ at every
boundary; a camera resting on a threshold never flickers.

## Projection

`css/tokens.css` is the runtime projection of this table. It declares core
tokens first, then semantic tokens, in the order given here. `DESIGN.md`
front matter declares the identity subset that `npm run design:check`
verifies against the projection.

`90-conformance/Checks.md` names which of these values are asserted against
the catalogue decks and the product stylesheets, and by which script.
