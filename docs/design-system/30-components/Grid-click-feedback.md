---
type: design-system-component
status: active
date: 2026-08-10
component: Grid click feedback
plane: grid
surface_class: mark
tags: [grove, design-system, component, grid]
---

# Grid click feedback

The transient geometric acknowledgement of a completed primary or secondary
pointer action on the Grid.

## Anatomy

| Part | Required | Value |
| --- | --- | --- |
| Feedback layer | yes | A child of `#gridWorld`, transformed with the Grid world, `pointer-events: none`, `z-index: 13`. |
| Primary mark | yes | `14 × 14px`, `2px` solid `--signal-interaction`, `--r-round`. |
| Secondary mark | yes | `12 × 12px`, `1px` dashed `--signal-interaction`, `--r-sm`. |
| Fill | no | None. |
| Text | no | None. |
| Shadow or glow | no | None. |

The mark's origin is the centre of the addressed cell in Grid coordinates. It
is never positioned from the viewport edge and never follows the camera as a
screen overlay.

## Geometry

| Property | Value |
| --- | --- |
| Primary size | `14 × 14px`. |
| Secondary size | `12 × 12px`. |
| Primary stroke | `2px` solid. |
| Secondary stroke | `1px` dashed. |
| Initial scale | `0.45`. |
| Final scale | `2.2`. |
| Radius | Primary `--r-round`; secondary `--r-sm`. |
| Placement | Cell centre in Grid-world coordinates. |

## States

| State | Appearance |
| --- | --- |
| Rest | Not drawn. |
| Approached | Not drawn. |
| Focused | Not drawn. |
| Selected | Not drawn. |
| Engaged | Not drawn. |
| Pending | Not drawn. |
| Refused | Not drawn. |
| Unavailable | Not drawn. |
| Anchored | Not drawn. |

The mark is an acknowledgement transition, not a durable component state.

## Behaviour

| Input | Mark | Trigger | Result |
| --- | --- | --- | --- |
| Primary pointer click | Primary | `click` | One mark at the addressed cell. |
| Secondary pointer click | Secondary | `contextmenu` | One secondary mark at the addressed cell. |
| Middle pointer action | None | `pointerdown` | Camera pan only. |
| Keyboard action | None | Keybind dispatch | The command owns its own acknowledgement. |

Both marks are non-interactive and never prevent selection, placement, context
menu routing, camera movement, or hit testing.

## Motion

| Transition | Duration | Curve | Reduced motion |
| --- | --- | --- | --- |
| Mark acknowledgement | `--d-sweep` | `--ease` | No mark is drawn. |

The mark begins at `0.45` scale and expands to `2.2` scale while opacity falls
to `0`. It runs once and is removed at the end of the transition.

## Distance

No distance tier. The mark is drawn only at the working Grid representation.

## Accessibility

The feedback layer is `aria-hidden="true"`, has no focusable descendants, and
does not change the accessible name, role, or state of the addressed content.

## Design assertions

- The mark is a child of `#gridWorld`.
- The mark uses `--signal-interaction` for both pointer buttons.
- The primary and secondary marks have different geometry and no different hue.
- The mark has no fill, text, shadow, glow, or pointer events.
- The mark is removed after one `--d-sweep` transition.

## Sources

- `00-foundations/Motion.md` — `--d-sweep`, one-shot acknowledgement, and
  reduced-motion behavior.
- `00-foundations/Marks.md` — geometry-first marks and no glow.
- `00-foundations/Shape.md` — expanding acknowledgement geometry.
- `10-grammar/Signal-roles.md` — `--signal-interaction` ownership.
- `30-components/Grid-cursor.md` — Grid coordinates, camera locality, and
  no text on Grid chrome.

## Copy

None.

## Refusals

- No viewport-fixed mark.
- No camera movement.
- No field illumination as the acknowledgement.
- No color variant for the secondary action.
- No label, coordinate, count, ordinal, subscript, or superscript.
- No persistence.
- No loop, pulse, blink, flash, glow, or hover mark.
