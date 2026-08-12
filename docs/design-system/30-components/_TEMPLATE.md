---
type: design-system-component
status: active
date: 2026-08-09
component: <Product name, exactly as a person would say it>
plane: grid | information | hud
surface_class: placement | slate | local-editor | menu | inline-confirm | chrome
tags: [grove, design-system, component]
---

# <Component>

<One sentence: what this is, in the words a person would use. No plane names,
no architecture nouns, no process vocabulary. If the sentence needs a Grove
concept, use the product word: Note, Document, Memory, Layer, Trace, Anchor,
Gallery.>

## Anatomy

Every visible part, named, with the token that gives it its value. A part with
no token is a part with no contract.

| Part | Required | Value |
| --- | --- | --- |
| <part> | yes / no | <tokens, in the form `--token` or an exact figure with its reason> |

State the containment edge, the fill, the padding, and the type role for each
part. If a figure is not a token, say why this component owns it.

## Geometry

- **Footprint** — how many cells, and the rule that decides. State the rule as
  a procedure, not a description: something a reader could execute twice and
  get the same answer.
- **Growth** — what happens when content exceeds the footprint. Grove's answer
  is always that the footprint grows; if this component differs, say why.
- **Measure** — reading width in `ch`, if the component sets type.
- **Alignment** — which edges land on which grid lines.

## States

All nine. `—` is not an answer; write "No change from Rest" where that is
true.

| State | Appearance | Notes |
| --- | --- | --- |
| Rest | | |
| Approached | | |
| Focused | | |
| Selected | | |
| Engaged | | |
| Pending | | |
| Refused | | |
| Unavailable | | |
| Anchored | | |

## Behaviour

- **Pointer** — what each gesture does, and at what moment it commits.
- **Keyboard** — every key this component answers, and what it does. If the
  component is reachable by keyboard, say where focus arrives and where it
  goes next.
- **Focus order** — the order of focusable parts within the component.
- **Escape** — what dismisses, and what focus returns to.
- **Commit and cancel** — what makes a change durable, and what undoes it.

Behaviour that belongs to a journey rather than to this component lives in the
owning wireframe. Name it, do not restate it.

## Motion

| Transition | Duration | Curve | Reduced motion |
| --- | --- | --- | --- |
| <what moves> | `--d-*` | `--e-*` | <the immediate state change at the identical threshold> |

Nothing loops. Nothing idles. Nothing pulses or blinks.

## Distance

How the component reads at each representation tier. Thresholds are on
projected cell size and belong to `10-grammar/Representation-tiers.md`; this
section says only what this component sheds and what it keeps.

| Tier | Sheds | Keeps |
| --- | --- | --- |
| Working | | |
| Stepped | | |
| Stand-in | | |

Presence, position, and extent are never shed. If this component's stand-in
has a kind-coded form, describe it exactly.

## Accessibility

- **Role and name** — the accessible role, and where the accessible name comes
  from. Authored content is the name; a generic label is a defect.
- **Contrast** — the ratio for every text part against its own background, and
  the ratio for every non-text signal against what it sits on.
- **Without colour** — for each state that uses hue, the second carrier.
- **Forced colours** — what survives a high-contrast mode, and what must be
  redeclared in system colours.
- **Text scaling** — what reflows and what grows when interface text scales.
- **Reduced motion** — restated only if it differs from the Motion table.

## Copy

Every user-visible string this component can show, verbatim. "None" is the
preferred answer.

Each string is checked against `10-grammar/Copy.md`: no plane or architecture
nouns, no documentation or process words, no prose that narrates the concept
instead of naming what a person can do.

## Refusals

What this component must never do, and the one-sentence reason. A refusal is
written so a check could assert it.

- **<Refused pattern>** — <why it is wrong>.

## Conformance

What a machine checks, so this specification cannot drift from the build.

| Assertion | Selector or source | Expected |
| --- | --- | --- |
| | | |

## Sources

- Catalogue deck: `docs/design_catalogue/src/<plane>/<deck>.html`
- Source note: `docs/raw/original-notes/<note>.txt`
- Decision: `docs/decisions/<record>.md`
- Wireframe: `docs/ux/wireframes/<slice>/<file>.md`

Where this specification resolved an open question, state the resolution and
the reason on one line. An open question does not survive in a component
specification.
