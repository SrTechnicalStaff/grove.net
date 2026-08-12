# Root Cause Analysis (RCA) Ledger: ADR-055

## 1. Executive Metadata

| Property | Value |
| :--- | :--- |
| **ADR ID** | `ADR-055` |
| **Title** | Multi-Item Selection Model, Cell-Aligned Marquee Sweep, and Multi-Type Group Translation Engine |
| **Category** | Grid Systems (`docs/specs/grid-systems/`) |
| **Claimed Spec Status** | `IMPLEMENTED - AWAITING USER REVIEW` |
| **Verified Interactive Status** | `PARTIALLY IMPLEMENTED (AD-HOC IN-CONTROL)` |
| **Audit Date** | 2026-08-12 |
| **Target Runtime** | C# 13 / .NET 9 / Avalonia 11.2.5 / SkiaSharp 3.x |
| **Authors** | Chief Product Definition Architect & AI Systems Audit Team |

---

## 2. Normative Specification Requirement Inventory

The table below catalogs every explicit architectural requirement, mathematical formulation, interface contract, and visual token specified in [ADR-055](file:///C:/dev/grove-v9/docs/specs/grid-systems/ADR-055-Multi-Item-Selection-And-Group-Translation.md):

| ID | Requirement Category | Normative Spec Requirement | Target Symbol / Token |
| :--- | :--- | :--- | :--- |
| **REQ-055-1** | Service Contract | `ISpatialGroupTranslationEngine` interface managing `IsClusterRegionFree`, `PrepareTransaction`, and `CommitTransaction`. | `Grove.SpatialGrid.Selection.ISpatialGroupTranslationEngine` |
| **REQ-055-2** | Data Contracts | `SelectionQueue` double-buffered collection with `PrimarySelectionId`, `SpatialClusterItem`, `GroupTranslationTransaction`. | `SelectionQueue`, `GroupTranslationTransaction` |
| **REQ-055-3** | Overlap Mathematics | $\ge 50\%$ surface area overlap threshold formula $\Phi(I_k, M) = \frac{\Delta x_{\text{overlap}} \times \Delta y_{\text{overlap}}}{W_k \cdot H_k \cdot P_{\text{cell}}^2} \ge 0.50$. | Spatial Cell Bounding Box Overlap Formula |
| **REQ-055-4** | Cluster Translation | Multi-type rigid cluster translation preserving pairwise spatial offsets $\mathbf{D}_{ij} = (X_j^0 - X_i^0, Y_j^0 - Y_i^0)$ under vector $\mathbf{\Delta C}$. | Rigid Spatial Cluster Vector Translation |
| **REQ-055-5** | Region Freedom Check | `IsClusterRegionFree` footprint union check $\mathcal{U}_{\text{target}} \setminus \mathcal{U}_{\text{source}}$ across spatial R-Tree index. | Footprint Union Collision Predicate |
| **REQ-055-6** | Atomic Transaction | Transactional movement engine (Snapshot $\rightarrow$ Evaluate $\rightarrow$ Commit or Abort with zero coordinate delta). | Atomic Transactional Movement Engine |
| **REQ-055-7** | Spent-Trail Physics | Vacated cells $\mathcal{V} = \mathcal{U}_{\text{source}} \setminus \mathcal{U}_{\text{target}}$ deposit $E_0 = 0.60$ kinetic energy decaying over 18 frames ($\gamma = 0.84$). | Multi-Cell Vacated Spent-Trail Physics |
| **REQ-055-8** | Compositor Ghosting | Real-time Skia routines `DrawMarqueeSweepOverlay` and `DrawGroupTranslationGhost` (6% fill `#96B6F8` / refusal $45^\circ$ cross-hatch `#F06543`). | Skia Overlay Render Operations |
| **REQ-055-9** | Signal Role Matrix | Interaction (`#96B6F8` outline + soft glow), Marquee (`#E8B964` active sweep work), Refusal (`#F06543` / `#E2625C` 12% fill & $45^\circ$ hatch). | Signal Role Color Matrix |
| **REQ-055-10** | Refusal Mechanics | Origin retention on refusal, point-of-action strip toolbar ("This space is occupied"), zero screen modal dialogs. | Point-of-Action Strip Toolbar Notification |

---

## 3. Codebase Reality & Line-by-Line Evidence

### 3.1 Implemented & Partially Implemented Symbols

- **`IsClusterRegionFree` Helper**:
  - `GridCanvasControl.cs` [L746-L788](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs#L746-L788) contains a private method `IsClusterRegionFree(int deltaX, int deltaY)` that checks if anchored items exist in `_dragCluster` or if moving items intersect unselected grid items.
- **Group Drag Delta Tracking**:
  - `GridCanvasControl.cs` [L440-L460, L559-L565](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs#L440) tracks `_dragCluster` and `_dragInitialPositions` dictionary to shift selected items together.
- **Marquee Overlap Calculation**:
  - `GridCanvasControl.cs` [L875-L885](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs#L875-L885) computes `overlapWidth` and `overlapHeight` to test if marquee selection covers items.

### 3.2 0% Implemented & Deviated Symbols

- **`ISpatialGroupTranslationEngine` Interface**: **0% Implemented**. Missing namespace `Grove.SpatialGrid.Selection` and interface `ISpatialGroupTranslationEngine`.
- **`SelectionQueue` Class**: **0% Implemented**. Selection is stored as a generic `List<GridContentItem>` in `GridCanvasControl.cs` [L136](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs#L136), lacking double-buffering, lock-free semantics, and primary selection ID assignment (`PrimarySelectionId`).
- **`GroupTranslationTransaction` & `SpatialClusterItem`**: **0% Implemented**. Missing transaction records.
- **$\ge 50\%$ Area Overlap Threshold Enforcement**: **0% Implemented**. In `GridCanvasControl.cs` [L879](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs#L879), marquee selection checks if `overlapArea > 0` (any non-zero intersection), failing to enforce the spec-mandated $\ge 50\%$ surface area overlap ratio threshold $\Phi(I_k, M) \ge 0.50$.
- **Vacated Group Cell Spent-Trail Registration**: **0% Implemented**. When committing a group move in `GridCanvasControl.cs` [L790-L800](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs#L790-L800), vacated cells $\mathcal{V} = \mathcal{U}_{\text{source}} \setminus \mathcal{U}_{\text{target}}$ are NOT registered into the spent-trail queue, so no 18-step trail decay renders behind moved group clusters.
- **Refusal $45^\circ$ Diagonal Cross-Hatching**: **0% Implemented**. When `IsClusterRegionFree` returns `false`, `RenderGroupDragRefusal` in `GridCanvasControl.cs` [L945-L955](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs#L945) draws a simple red rectangle fill without $45^\circ$ diagonal cross-hatch lines.
- **Point-of-Action Refusal Strip Toolbar**: **0% Implemented**. No local strip toolbar ("This space is occupied") is shown upon refused group translation.

---

## 4. Standards & Visual Plane Seam Audit

1. **Plane 0 Composition Seam**:
   - Group drag previews and marquee overlays are drawn directly inside `GridCanvasControl.OnRender` instead of being modularized into custom Skia pipeline draw operations.
2. **Design System Tokens Alignment**:
   - `Colors.cs` defines `#96B6F8` (Interaction), `#E8B964` (Marquee), and `#F06543` (Refusal). However, refusal cross-hatch pattern specifications (`stroke: rgba(226,98,92,0.40)`) are not implemented.
3. **Code Smells & Architectural Violations**:
   - Group translation mutates item positions directly during mouse move, relying on dictionary snapshot rollbacks rather than an atomic `GroupTranslationTransaction` state machine.
   - Any non-zero marquee intersection selects items, violating the $50\%$ overlap rule and causing accidental selection of neighboring items during tight sweeps.

---

## 5. Root Cause Analysis (RCA)

### Why claimed status (`IMPLEMENTED - AWAITING USER REVIEW`) diverges from actual interactive reality:

1. **Simplified Any-Overlap Selection Short-Cut**:
   - The developer implemented `overlapArea > 0` in `GridCanvasControl` because calculating precise fractional area ratios $\Phi(I_k, M) \ge 0.50$ required additional math routines, leading to inaccurate selection behavior.
2. **Omission of Transactional Engine & Spent Trails**:
   - Group move commits were implemented as direct coordinate assignments (`item.CellX += deltaX`). The author skipped generating the vacated cell set $\mathcal{U}_{\text{source}} \setminus \mathcal{U}_{\text{target}}$ and enqueuing spent trail cells, disabling movement trails.
3. **Ad-Hoc Selection Management**:
   - Rather than building `SelectionQueue` with explicit primary vs secondary item tracking, a primitive C# list was used, preventing inspector tools from identifying the primary target handle.
4. **Refusal Feedback Shortcut**:
   - $45^\circ$ diagonal path clipping in Skia required extra rendering lines. The author used a plain semi-transparent red rectangle fill instead of the complete refusal cross-hatch pattern and strip toolbar.
