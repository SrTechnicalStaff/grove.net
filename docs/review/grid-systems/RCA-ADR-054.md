# RCA Ledger: ADR-054 — Spatial Context Menu System

| Metadata Field | Value |
| :--- | :--- |
| **ADR ID** | ADR-054 |
| **Verified Status** | **PARTIAL — right-click, long-press, commands, clamping, and pass-through are wired; the draft namespace contracts differ** |
| **Audit Date** | 2026-08-12 |

## Verified implementation

- `SpatialContextMenuService` owns target context, active state, command models,
  dismissal, and state-change notifications.
- `SpatialContextMenuOverlay` is mounted on the HUD Plane without a scrim and
  clamps its frame to the viewport.
- Right-click and long-press open the menu from Grid Canvas. Outside clicks
  dismiss it and return focus to the Grid.
- Empty-cell, single-Content, and multi-selection command sets are evaluated;
  create, open, anchor, trace, copy, paste, and delete commands are routed.
- Locked or occupied actions are represented as disabled/refused commands.

## Remaining gaps

- The application uses `SpatialContextMenuService` and
  `SpatialContextMenuOverlay` rather than the draft `Grove.HUD.ContextMenu`
  namespace and `SpatialContextMenuOverlayView` names.
- Long-press timing and pointer capture are implemented at the window adapter,
  not in a dedicated Plane 0 interaction service.

## Root cause of the original false claim

The zero-percent review was generated before the context-menu seam was added
and was not regenerated after the overlay became interactive.
