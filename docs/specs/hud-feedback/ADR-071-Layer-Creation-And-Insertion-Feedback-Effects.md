---
status: "PARTIAL — discrete insertion feedback is implemented; full row-state verification remains"
authority: normative
date: 2026-08-12
---

# ADR-071: Grid Layer Creation and Insertion Feedback

## Decision

Creating or inserting a Grid Layer receives immediate discrete feedback:

- the new Grid Layer row appears in the HUD Plane at its final geometry;
- the insertion origin receives a bounded cell feedback sweep using existing
  motion tokens;
- the feedback never mutates Content or the Field Ledger from inside drawing;
- reduced-motion settings collapse the feedback to an immediate state change.

The formerly proposed radial flash sweep, Gaussian Aura wave, and continuous
Plane 0 effect are refused because they violate the Presence contract.

The interface uses “Grid Layer” for stack depth and “HUD Plane” for viewport
composition. No other UI surface is called a layer.

## Journey

1. User invokes a Grid Layer insertion command.
2. The stack commits the insertion and raises one insertion result.
3. The HUD row updates immediately.
4. Plane 0 shows a discrete bounded confirmation around the insertion origin.
5. The feedback expires without changing spatial state.

Failure leaves the stack unchanged and reports the refusal at the action point.

## Acceptance

Hotkey, manager command, and context command use the same insertion result;
row identity and active selection remain consistent; reduced motion is
immediate; no gradient or render-pass state mutation exists.
