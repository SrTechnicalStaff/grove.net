# RCA Ledger: ADR-031 — Slate Window System and Anatomy Specifications

| Metadata Field | Value |
| :--- | :--- |
| **ADR ID** | ADR-031 |
| **Claimed Status in Spec Header** | Accepted |
| **Verified Status** | **PARTIAL — the three Slate modes and archive surface exist; docking and dedicated controls remain** |
| **Audit Date** | 2026-08-12 |

## Verified implementation

- `SlateHostOverlay` is mounted on the HUD Plane and exposes the three
  approved modes: Writing, Memory, and Gallery.
- Writing opens for a Document and saves through the Content-to-Memory binding
  path.
- Memory is reachable with `M` and receives one representative Content item per
  Memory identity.
- Gallery opens for Image Content and displays the existing decoded bitmap.
- `MasonryGalleryPanel` now performs token-backed shortest-column placement with
  the ADR column minimum and gutter.
- The Layer Manager remains a separate HUD overlay.

## Remaining gaps

- The host is a single mode-driven overlay rather than separate
  `WritingSlate`, `MemorySlate`, and `GallerySlate` control types.
- Left/right docking modes are not exposed.
- Archive cards do not yet expose full Memory provenance or record history.
- Gallery facet controls and image-count identity are incomplete.
- Escape peeling is implemented at the host level but does not provide a
  record-view-to-archive transition.

## Root cause of the original false claim

The original RCA was a pre-integration scan that searched for the exact control
class names and treated their absence as absence of the observable surface. The
current implementation uses one HUD host with three explicit modes, so the
class-name conclusion was too strong; the remaining requirements are still
listed above.
