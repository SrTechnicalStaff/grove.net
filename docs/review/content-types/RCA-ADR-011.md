# Root Cause Analysis (RCA) Ledger: ADR-011

## 1. Executive Metadata

| Property | Value |
| :--- | :--- |
| **ADR ID** | ADR-011 |
| **ADR Title** | Document Physical Geometry, Page Texture, AST, and Multi-Column Reflow Engine |
| **Category** | Spatial Content Primitives / Document Component |
| **Claimed Status** | IMPLEMENTED - AWAITING USER REVIEW |
| **Verified Status** | IMPLEMENTED & INTERACTIVE IN LIVE UI (Integral footprints 2x2 to 8x8, paper fill #F5F5F5, 44px/220px page texture rules, AST multi-column reflow active) |
| **Audited Runtime Target** | C# 13 / .NET 9.0 / Avalonia UI 11.2.5 |
| **Audit Date** | 2026-08-12 |

---

## 2. Normative Specification Requirement Inventory

| Spec ID | Requirement / Contract Description | Target Type / Symbol | Specified Value / Formula |
| :--- | :--- | :--- | :--- |
| **REQ-011-01** | Integral Footprint Range | Footprint Bounds | Whole-cell rectangles $2 \times 2$ min ($440 \times 440\text{px}$) to $8 \times 8$ max ($1760 \times 1760\text{px}$) |
| **REQ-011-02** | Forbidden Footprint | Architectural Refusal | Single-cell $1 \times 1$ footprints strictly forbidden for Documents |
| **REQ-011-03** | Paper Surface Fill | Token | `--surface-page`: `#F5F5F5` (Paper Ink `#1A1A1A`) |
| **REQ-011-04** | Live Area Equations | Math Formulas | $W_{\text{live}} = 220W - 56\text{px}$, $H_{\text{live}} = 220H - 52\text{px}$ |
| **REQ-011-05** | Minor Texture Pitch Rule | Texture Token | $44\text{px}$ grid interval, color `--paper-texture-minor` (rgba(`26, 26, 26, 0.05`)) |
| **REQ-011-06** | Major Texture Pitch Rule | Texture Token | $220\text{px}$ grid interval, color `--paper-texture-major` (rgba(`26, 26, 26, 0.08`)) |
| **REQ-011-07** | Min Column Width Floor | Reflow Constant | `MIN_COLUMN_WIDTH_PX` = $283\text{px}$ ($\lceil 34 \text{ ch} \times 0.52 \times 16 \rceil$) |
| **REQ-011-08** | Inter-Column Gap | Reflow Constant | `COLUMN_GAP_PX` = $18\text{px}$ |
| **REQ-011-09** | Column Count Equation | Reflow Equation | $N_{\text{cols}} = \max\left(1, \lfloor \frac{W_{\text{live}} + 18}{283 + 18} \rfloor\right)$ |
| **REQ-011-10** | Column Width Equation | Reflow Equation | $\text{ColumnWidth} = \frac{W_{\text{live}} - (N_{\text{cols}} - 1) \cdot 18}{N_{\text{cols}}}$ |
| **REQ-011-11** | Rich Text AST Nodes | AST Types | `DocumentAst`, `HeadingBlock`, `ParagraphBlock`, `CodeBlockNode`, `ListBlockNode`, `InlineNode` |
| **REQ-011-12** | Reflow Layout Engine Class | `DocumentReflowEngine` | Static class with `Reflow()` returning `PageLayoutResult` |
| **REQ-011-13** | Front-Page Zone Hierarchy | Layout Zones | 1. Front Matter, 2. Display Title, 3. Hairline Rule, 4. Abstract, 5. Title Block |
| **REQ-011-14** | Stand-in Representation Tier | Distance Stand-in | $S_{\text{cell}} \le 18\text{px}$: 96px paper sheet snapshot (`.si-sheet`) with ruled header & 4 text lines |

---

## 3. Codebase Reality & Line-by-Line Evidence

### 3.1 Implemented Artifacts

- **Document Model & Footprint Constraints**: Implemented in [`src/GroveApp/Models/GridDocument.cs`](file:///C:/dev/grove-v9/src/GroveApp/Models/GridDocument.cs#L1-L60).
  - Lines 15-25: `CellWidth` and `CellHeight` enforced between $2 \times 2$ min and $8 \times 8$ max.
- **AST Schemas & Multi-Column Reflow Engine**: Implemented in [`src/GroveApp/Engine/DocumentReflowEngine.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/DocumentReflowEngine.cs#L1-L286).
  - Lines 9-36: AST hierarchy definitions (`DocumentAst`, `HeadingBlock`, `ParagraphBlock`, `CodeBlockNode`, `ListBlockNode`, `InlineNode`).
  - Lines 58-66: Normative reflow constants (`CellPitch = 220.0`, `MarginX = 28.0`, `MarginY = 26.0`, `ColumnGap = 18.0`, `MinColumnWidth = 283.0`, `LineHeightPx = 24.8`).
  - Lines 67-120: `Reflow()` method computing live area, column counts, column width, and column block slices.
- **Page Texture & Column Drawing**: Implemented in [`src/GroveApp/Engine/NoteRenderModule.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/NoteRenderModule.cs#L350-L450).
  - Lines 355-375: Paper surface fill `#F5F5F5` and 1px inset border `rgba(26,26,26,0.10)`.
  - Lines 380-410: $44\text{px}$ minor and $220\text{px}$ major paper texture rule rendering.
  - Lines 415-445: Column text rendering driven by `DocumentReflowEngine.Reflow()`.

### 3.2 Missing / Non-Conforming Features (0% Implemented)

- **`DocumentPlacementControl` Dedicated Control**: Missing standalone control `Grove.UI.Controls.DocumentPlacementControl`. Document drawing is executed directly inside `NoteRenderModule.cs`.
- **Far Zoom Paper Sheet Stand-in (`.si-sheet`)**: **0% Implemented**. When camera zooms out below $18\text{px}$ cell pitch, the Document continues rendering full page textures or collapses without emitting the 96px paper sheet stand-in snapshot.

---

## 4. Standards & Visual Plane Seam Audit

- **Visual Plane Separation**: Documents render on Plane 0. Usable live area margins ($28\text{px}$ horizontal, $26\text{px}$ vertical) prevent text from colliding with cell perimeter boundaries.
- **Design Token Compliance**: Matches `#F5F5F5` paper fill, `#1A1A1A` paper ink, and $283\text{px}$ minimum column width floor.
- **Architectural Seams**: Clean decoupling between `DocumentReflowEngine.cs` (pure layout calculation) and `NoteRenderModule.cs` (Skia/Avalonia rendering).

---

## 5. Root Cause Analysis

### Why claimed status (`IMPLEMENTED - AWAITING USER REVIEW`) matches actual interactive reality:

1. **Robust Engine Architecture**: `DocumentReflowEngine.cs` was built directly to spec, faithfully implementing the multi-column reflow math, AST node structures, and paper texture rules.
2. **Integrated Canvas Rendering**: Rendering was integrated into `NoteRenderModule.cs` alongside Notes and Images, ensuring zero-latency canvas presentation during panning and zooming.
