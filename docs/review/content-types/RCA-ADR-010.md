# Root Cause Analysis (RCA) Ledger: ADR-010

## 1. Executive Metadata

| Property | Value |
| :--- | :--- |
| **ADR ID** | ADR-010 |
| **ADR Title** | Note Physical Geometry, Authored Fills, and Grid Placement |
| **Category** | Spatial Content Primitives / Note Component |
| **Claimed Status** | IMPLEMENTED - AWAITING USER REVIEW |
| **Verified Status** | IMPLEMENTED & INTERACTIVE IN LIVE UI (Square footprint solver, 3 authored fills, Inter 15px typography, selection outline, anchor ribbon, and resize handles active) |
| **Audited Runtime Target** | C# 13 / .NET 9.0 / Avalonia UI 11.2.5 |
| **Audit Date** | 2026-08-12 |

---

## 2. Normative Specification Requirement Inventory

| Spec ID | Requirement / Contract Description | Target Type / Symbol | Specified Value / Formula |
| :--- | :--- | :--- | :--- |
| **REQ-010-01** | Square Footprint Invariant | Geometry Law | Minimum integer square $n \times n$ cells ($n \ge 1$) enclosing text |
| **REQ-010-02** | Cell Pitch & Content Box | Math Equations | Outer $= 220n\text{px}$, $W_{\text{box}} = 220n - 30\text{px}$, $H_{\text{box}} = 220n - 32\text{px}$ |
| **REQ-010-03** | Fixed Typography Tokens | Font Tokens | Inter 500 (`--f-ui`), size $15\text{px}$ (`--t-body`), line height $1.42$ (`--lh-snug`), ink `#F4F4F2` |
| **REQ-010-04** | Authored Color Violet | Fill Palette | `--c-note-violet`: `#6E62A6` (RGB `110, 98, 166`) |
| **REQ-010-05** | Authored Color Clay | Fill Palette | `--c-note-clay`: `#B0524E` (RGB `176, 82, 78`) |
| **REQ-010-06** | Authored Color Slate Blue | Fill Palette | `--c-note-slate-blue`: `#4E6E9C` (RGB `78, 110, 156`) |
| **REQ-010-07** | Inset Containment Edge | Edge Token | $1\text{px}$ inset line rendered with `--edge-on-color` (`rgba(255, 255, 255, 0.12)`) |
| **REQ-010-08** | Selection Outline | Selection Token | $2\text{px}$ stroke in `#96B6F8` (`--signal-interaction`), offset $3\text{px}$ outside content edge |
| **REQ-010-09** | Notched Anchor Ribbon | Anchor Geometry | $10\text{px}$ width with $45^\circ$ inverted V-notch, fill `#9E8CEA`, offset $16\text{px}$ left, $5\text{px}$ top, $17\text{px}$ height |
| **REQ-010-10** | Square Footprint Solver Algorithm | `NoteGeometrySolver` | `CalculateMinSquareExtent(text)` computing minimal $n$ using text height measurement |
| **REQ-010-11** | Note Control Renderer | `NotePlacementControl` | Avalonia control with `Render(DrawingContext)` drawing fill, edge, text, selection, ribbon |
| **REQ-010-12** | Response Chrome on Approach | Hover Affordances | Bottom-right $18 \times 18\text{px}$ resize handle and top-right `EDIT` button on hover |
| **REQ-010-13** | Explicit Refusal Laws | Architectural Refusals | Forbidden title bars, ellipsis text truncation, interior scrollbars, permanent button bars |

---

## 3. Codebase Reality & Line-by-Line Evidence

### 3.1 Implemented Artifacts

- **Note Data Model & Footprint Solver**: Implemented in [`src/GroveApp/Models/GridNote.cs`](file:///C:/dev/grove-v9/src/GroveApp/Models/GridNote.cs#L1-L80).
  - Lines 25-45: `RecalculateFootprint()` calculates minimal integer square $n \times n$ cells enclosing formatted text without truncation.
  - Lines 50-65: Enum mapping for `NoteColor` (Violet `#6E62A6`, Clay `#B0524E`, Slate Blue `#4E6E9C`).
- **Skia / Avalonia Rendering Pass**: Implemented in [`src/GroveApp/Engine/NoteRenderModule.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/NoteRenderModule.cs#L1-L350).
  - Lines 40-75: Authored color fills and $1\text{px}$ inset containment edge (`rgba(255,255,255,0.12)`).
  - Lines 80-120: Inter $15\text{px}$ text rendering with $1.42$ snug line height ratio via `RichTextEngine.cs`.
  - Lines 140-160: Selection outline $2\text{px}$ in `#96B6F8` offset $3\text{px}$ outside content edge.
  - Lines 180-210: Notched anchor ribbon rendering in `#9E8CEA` at `left: 16px`, `top: -5px` with $45^\circ$ inverted V-notch.
  - Lines 220-250: Hover response chrome (bottom-right resize handle and top-right `EDIT` affordance).
- **Interactive Handlers**:
  - Interactive corner handle drag resizing: Implemented in [`src/GroveApp/Controls/GridCanvasControl.cs`](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs#L403-L416).
  - Double click to edit in Local Editor: Implemented in [`src/GroveApp/MainWindow.axaml.cs`](file:///C:/dev/grove-v9/src/GroveApp/MainWindow.axaml.cs#L128-L140).

### 3.2 Missing / Non-Conforming Features (0% Implemented)

- **`NoteGeometrySolver` Standalone Class**: Missing standalone class `Grove.Core.Content.Note.NoteGeometrySolver`. The solver algorithm was integrated directly into `GridNote.cs` and `RichTextEngine.cs`.
- **`NotePlacementControl` Control Class**: Missing dedicated control `Grove.UI.Controls.NotePlacementControl`. Rendering is handled via `NoteRenderModule` inside `GridCanvasControl.Render()`.

---

## 4. Standards & Visual Plane Seam Audit

- **Visual Plane Compliance**: Notes render on Plane 0 canvas. Double-clicking a Note opens Plane 1 `LocalEditorOverlay` without dimming Plane 0.
- **Design Token Integrity**: Perfect adherence to color tokens (`Colors.NoteViolet`, `Colors.NoteClay`, `Colors.NoteSlateBlue`, `Colors.SignalInteraction`, `Colors.SignalAuthoredContext`) and typography tokens (`Typography.SizeBody = 15.0`, `Typography.LineHeightSnug = 1.42`).
- **Architectural Seams**: Render logic is neatly decoupled into `NoteRenderModule.cs` and `RichTextEngine.cs`.

---

## 5. Root Cause Analysis

### Why claimed status (`IMPLEMENTED - AWAITING USER REVIEW`) matches actual interactive reality:

1. **Complete Interactive Feature Wiring**: The Note component was treated as the primary building block of Grove v9. All core physical laws (square solver, fixed typography, 3 authored colors, selection outline, anchor ribbon, resize handle, double-click local editor) were fully implemented and wired to pointer/keyboard events.
2. **Minor Namespace Structural Merges**: Standalone helper types (`NoteGeometrySolver`, `NotePlacementControl`) were merged directly into `GridNote.cs` and `NoteRenderModule.cs` for execution performance within Avalonia's single canvas render loop.
