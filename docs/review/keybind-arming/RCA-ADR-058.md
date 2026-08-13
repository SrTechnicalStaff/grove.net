# RCA Ledger: ADR-058 — Global Keybind Focus Precedence Router

| Metadata Field | Value |
| :--- | :--- |
| **ADR ID** | ADR-058 |
| **Verified Status** | **PARTIAL — tunneling focus precedence and the global key matrix are wired; attached-property declaration remains** |
| **Audit Date** | 2026-08-12 |

## Verified implementation

- `GlobalFocusPrecedenceRouter` implements `IFocusPrecedenceRouter`, evaluates
  Focused TextBox, Information Plane, HUD Plane, and Plane 0 contexts, and
  attaches at the window tunneling phase.
- `KeyCombination`, `KeybindHandlingResult`, and `FocusContextInfo` exist;
  text focus prevents spatial arming leakage.
- The routed matrix covers arming, framing, memory access, clipboard, anchor,
  delete, escape, layer navigation, and Plane 0 tool commands through
  `KeybindModule` and `IKeybindHost`.

## Remaining gaps

- The router receives visibility/context delegates from `MainWindow` rather
  than deriving every context from attached properties.
- `FocusPrecedenceAttachedProperties` is not yet used to declare precedence in
  XAML, and routing telemetry is limited to the existing routed-event payload.

## Root cause of the original false claim

The earlier zero-occurrence findings predated the router contract and were not
reconciled with the current source after the tunneling handler was added.
