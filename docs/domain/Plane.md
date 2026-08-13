---
type: canonical-domain-concept
status: accepted
authority: source-of-truth
concept: Plane
date: 2026-08-12
---

# Plane

## Definition

A Plane is a visual composition surface with its own coordinate behavior and
focus responsibility. Grove has three named Planes:

- **Spatial Grid Plane** — the camera-driven Grid projection;
- **Information Plane** — content-authoring and capture surfaces;
- **HUD Plane** — viewport-fixed tools, telemetry, and operational surfaces.

Planes are not semantic records and do not create relationships between
Memory, Content, or Anchor.

## Ownership

The Spatial Grid Plane projects Grid Layers through the camera. The Information
Plane presents authoring or capture work without redefining Grid coordinates.
The HUD Plane presents viewport-fixed operations without becoming a spatial
Grid object.

The same Memory may be represented on different Planes without duplication.
The representation is a route or projection; the domain identity remains the
Memory record.

## Boundary decisions

| Tension | Decision |
| --- | --- |
| Plane vs Grid Layer | Plane is visual composition; Grid Layer is literal Grid depth. |
| Plane vs Slate | A Slate is a named sub-application surface hosted on the HUD Plane. |
| Plane vs Overlay | An Overlay is transient UI; a Plane is the stable composition model. |
| Plane vs Memory | Plane visibility never changes Memory identity or lifecycle. |
