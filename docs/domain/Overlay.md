---
type: canonical-domain-concept
status: accepted
authority: source-of-truth
concept: Overlay
date: 2026-08-12
---

# Overlay

## Definition

An Overlay is a transient interface surface used to complete or acknowledge a
focused operation. It has no independent semantic identity and does not become
a Grid object merely because it is positioned near Content.

Examples include a quick capture surface, a refusal response, a placement
preview, or a temporary command surface.

## Rules

- an Overlay may read domain state and propose a domain mutation;
- only the domain operation commits Memory, Content, Placement, or Anchor;
- dismissing an Overlay discards uncommitted proposal state;
- an Overlay must not be used as a substitute for a Slate or as a footer/status
  bar attached to the application shell.

## Boundary decisions

| Tension | Decision |
| --- | --- |
| Overlay vs Slate | Overlay is transient operation chrome; Slate is a named sub-application. |
| Overlay vs Content | A preview is not Content until the placement operation commits it. |
| Overlay vs Memory | Opening or dismissing an Overlay does not create or mutate Memory. |
