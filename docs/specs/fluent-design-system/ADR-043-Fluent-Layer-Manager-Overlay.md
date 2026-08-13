---
status: "NORMATIVE — Grid Layer manager on the HUD Plane"
verification: "PARTIAL — core Grid Layer operations are wired; accessibility and full interaction contract remain."
---

# ADR-043: Grid Layer Manager on the HUD Plane

The Grid Layer Manager is a HUD Plane control for inspecting and changing the
ordered Grid Layer continuum. It does not activate or deactivate Grid Layers.

## Grid Layer identity

- Main Grid Layer: `01`.
- Grid Layers above Main: `02`, `03`, `04`...
- Grid Layers below Main: `B02`, `B03`, `B04`...
- The selected Grid Layer row receives the selection treatment. This is command
  context, not a render-state distinction.

## Runtime behavior

- All visible Grid Layers render their Content and contribute Aura fields.
- Selecting a row changes the target for new Content, direct manipulation,
  bracket navigation, insertion, reordering, and trace operations.
- A locked Grid Layer refuses mutation while continuing to render and
  contribute its field.
- Visibility is an explicit user rendering preference. It is separate from
  selection and locking.
- Deleting a Grid Layer transfers its Content to the requested target Grid
  Layer before the Grid Layer is removed.

## Controls

| User action | Gesture | Result |
|---|---|---|
| Select Grid Layer | Click row, `[`, `]` | Changes command target only |
| Insert above | `Ctrl+Shift+N` | Creates and selects a Grid Layer above the selected one |
| Insert below | `Ctrl+Alt+Shift+N` | Creates and selects a Grid Layer below the selected one |
| Reorder | `Alt+Up`, `Alt+Down` | Swaps the selected Grid Layer's stack position |
| Rename | `F2` / double click | Renames the selected Grid Layer |
| Remove | `Del` | Confirms removal and Content transfer |

The former visibility-as-activation, ghost-presence, and isolation specifications
are retired. No manager control may introduce an active/inactive Grid Layer
state.
