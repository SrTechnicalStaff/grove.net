# RCA Ledger: ADR-055 — Multi-Item Selection and Group Translation

| Metadata Field | Value |
| :--- | :--- |
| **ADR ID** | ADR-055 |
| **Verified Status** | **PARTIAL — rigid multi-type translation, occupancy validation, and vacated-footprint trails are wired; the standalone transaction seam remains** |
| **Audit Date** | 2026-08-12 |

## Verified implementation

- Selected Content is moved as a rigid cluster using one cell delta, preserving
  pairwise offsets across Notes, Documents, and Images.
- Candidate movement validates the complete target footprint against unselected
  Content on the same Grid Layer. Refused movement retains the original
  positions and renders discrete refusal feedback.
- Commit updates every Content Anchor and records the vacated source cells in
  the canonical spent-trail model.
- Marquee qualification uses the shared 50% overlap rule from `SelectionService`.

## Remaining gaps

- `ISpatialGroupTranslationEngine`, `SelectionQueue`, and an explicit
  `GroupTranslationTransaction` are not separate runtime types; the canvas
  still coordinates the gesture adapter.
- The group refusal presentation is a Grid feedback pass rather than the draft
  standalone custom draw-operation and HUD strip contracts.

## Root cause of the original false claim

The previous review was frozen at the single-item drag implementation and was
not rerun after group translation and spent-footprint registration were added.
