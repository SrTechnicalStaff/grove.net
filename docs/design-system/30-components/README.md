---
type: design-system-index
status: normative
date: 2026-08-10
owner: "Grove Design System"
---

# Components

| Requirement | Rule |
| --- | --- |
| Ownership | One specification per visible or actionable component. |
| Required sections | Anatomy, Geometry, States, Behaviour, Motion, Distance, Accessibility, Copy, Refusals, Design assertions, Sources. |
| State model | Nine shared states: Rest, Approached, Focused, Selected, Engaged, Pending, Refused, Unavailable, Anchored. |
| Surface classes | Closed set in `10-grammar/Surface-classes.md`. |
| Source precedence | Raw notes → Decisions → References → Catalogue → this system. |
| Inference | Forbidden; every normative rule is closed in a named design-system owner. |

## Grid components

| Component | Contract |
| --- | --- |
| [Note](Note.md) | Complete authored text on the Grid. |
| [Document](Document.md) | Complete paper page on the Grid. |
| [Picture](Picture.md) | Complete intrinsic image frame. |
| [Grid lines](Grid-lines.md) | Three-tier line system and fade. |
| [Grid cursor](Grid-cursor.md) | Whole-cell address and operation state. |
| [Grid click feedback](Grid-click-feedback.md) | Camera-relative primary and secondary action acknowledgement. |
| [Presence](Presence.md) | Per-cell field influence across Layers. |
| [Selection](Selection.md) | Content selection and marquee. |
| [Placement preview](Placement-preview.md) | Complete footprint before commit. |
| [Refusal](Refusal.md) | Inline refusal carriers. |
| [Stand-in](Stand-in.md) | Distance representation preserving kind and extent. |
| [Layer mark](Watermark.md) | Ambient Layer identity mark. |

## Information Plane components

| Component | Contract |
| --- | --- |
| [Local editor](Local-editor.md) | Source-local Note/Document editing. |
| [Image Viewer](Image-viewer.md) | Complete Image/GIF inspection. |
| [Quick Note](Quick-note.md) | One-action Note capture. |
| [Blip](Blip.md) | Qualifying Annotation marker and cue. |
| [Annotation](Annotation.md) | Shared reader shell and common nine-state contract. |
| [Mixed-media annotation](Mixed-media-annotation.md) | Brochure/Pamphlet/Magazine; 12 templates. |
| [Text-led annotation](Text-led-annotation.md) | Bulletin/Berliner/Broadsheet; 9 families. |
| [Image-led annotation](Image-led-annotation.md) | Gallery/Contact sheet/Image edition; 11 families. |
| [Annotation layout families](Annotation-layout-families.md) | Exact 32-family registry, fit predicate, pagination capacity, and Tiptap text contract. |
| [Annotation routes](Annotation-routes.md) | Explicit source destinations and return. |
| [Operation bar](Operation-bar.md) | Open operation feedback until commit/cancel. |

## HUD components

| Component | Contract |
| --- | --- |
| [Slate](Slate.md) | Composed viewport-fixed surface. |
| [Memory Slate](Memory-slate.md) | Memory browsing and recall. |
| [Gallery Slate](Gallery-slate.md) | Image/GIF gallery. |
| [Writing Slate](Writing-slate.md) | Long-form text editing. |
| [Layer manager](Layer-manager.md) | Layer stack management. |

## Cross-plane components

| Component | Contract |
| --- | --- |
| [Menu](Menu.md) | One-level action list. |
| [Inline confirm](Inline-confirm.md) | Inline confirmation/refusal. |

## New component gate

| Step | Requirement |
| --- | --- |
| 1 | Name the owning plane and surface class. |
| 2 | Add one canonical component file. |
| 3 | Answer every required section. |
| 4 | Link raw sources, Decisions, References, catalogue evidence, and wireframes. |
| 5 | Add design assertions and closed decision references. |

## Register

The complete catalogue-to-owner mapping is maintained in
[`90-conformance/Component contract register.md`](../90-conformance/Component%20contract%20register.md).
