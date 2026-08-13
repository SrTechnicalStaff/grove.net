---
status: "NORMATIVE — Grid Layer manager navigation"
---

# ADR-032: Grid Layer Manager and Navigation

The HUD Plane exposes the ordered Grid Layer continuum. The manager changes
Grid Layer selection and stack structure; it never activates, deactivates, or
isolates a Grid Layer.

## Interaction rules

- `[` and `]` select the adjacent Grid Layer as command target.
- `Shift+[` and `Shift+]` create and select a Grid Layer at the bottom or top.
- `Ctrl+[` and `Ctrl+]` insert below or above the selected Grid Layer.
- `Alt+[` and `Alt+]` reorder the selected Grid Layer.
- `L` opens the HUD Plane Grid Layer Manager.
- Selection changes placement, direct-edit, trace, and navigation target only.

All visible Grid Layers continue to render their Content and contribute to the
Field Ledger after selection changes. Cross-Grid-Layer Aura uses

\[
E_i(d, \Delta L) = \frac{M_i}{1 + 0.4d^2} \cdot 0.5^{|\Delta L|}
\]

where the target is the Grid Layer whose Plane 0 field projection is being
sampled. The target is not an activation state.

Removal migrates Content atomically to the chosen surviving Grid Layer and
refuses the operation when same-Grid-Layer destination cells are occupied.
