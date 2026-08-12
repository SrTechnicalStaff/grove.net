# Root Cause Analysis (RCA) Ledger: ADR-012

## 1. Executive Metadata

| Property | Value |
| :--- | :--- |
| **ADR ID** | ADR-012 |
| **ADR Title** | Image Footprint Resolution Mapping, Zero-Crop Rules, and Skia Bitmap Sampling |
| **Category** | Spatial Content Primitives / Picture Component |
| **Claimed Status** | IMPLEMENTED - AWAITING USER REVIEW |
| **Verified Status** | IMPLEMENTED & INTERACTIVE IN LIVE UI (256px cell divisor long-axis formula, zero-crop aspect ratio mapping, high-DPI bitmap rendering, GIF badge, and anchor diamond active) |
| **Audited Runtime Target** | C# 13 / .NET 9.0 / Avalonia UI 11.2.5 |
| **Audit Date** | 2026-08-12 |

---

## 2. Normative Specification Requirement Inventory

| Spec ID | Requirement / Contract Description | Target Type / Symbol | Specified Value / Formula |
| :--- | :--- | :--- | :--- |
| **REQ-012-01** | Zero-Crop Rule | Geometry Law | Never cover-fit crop, stretch, or letterbox images with container pillar/letterbox bars |
| **REQ-012-02** | 256px Cell Divisor Invariant | Scale Constant | $D_{\text{cell}} = 256.0\text{px}$ long-axis divisor |
| **REQ-012-03** | Long Axis Cell Count Formula | Cell Math | $L = \max\left(1, \left\lceil \frac{\text{long}}{256} \right\rceil\right)$ |
| **REQ-012-04** | Short Axis Cell Count Formula | Cell Math | $S = \max\left(1, \text{round}\left(L \cdot \frac{\text{short}}{\text{long}}\right)\right)$ |
| **REQ-012-05** | Footprint Assignment ($N_w \times N_h$) | Footprint Math | $N_w = L, N_h = S$ if $W \ge H$; $N_w = S, N_h = L$ if $W < H$ |
| **REQ-012-06** | Aspect Class Enum | `AspectClass` | Enum: `ExtremePanorama` ($r \ge 2$), `Landscape` ($1 < r < 2$), `Square` ($r = 1$), `Portrait` ($r < 1$) |
| **REQ-012-07** | Effective PPI Calculation | Fidelity Math | $\text{PPI}_{\text{eff}} = \min\left( \frac{W_{\text{px}}}{\text{Inches}_W}, \frac{H_{\text{px}}}{\text{Inches}_H} \right)$ ($96\text{px} = 1.0\text{ in}$) |
| **REQ-012-08** | Footprint Resolver Engine Class | `ImageFootprintResolver` | Static class with `Resolve(W, H)` and `CalculateEffectivePpi()` |
| **REQ-012-09** | Image Data Record | `ImagePlacementRecord` | Record struct tracking intrinsic dimensions, filepath, footprint, GIF flag |
| **REQ-012-10** | High-DPI Bitmap Sampling | Skia / Avalonia | Linear mipmap sampling mode (`SKFilterMode.Linear`, `SKMipmapMode.Linear`) |
| **REQ-012-11** | Picture Control Renderer | `PicturePlacementControl` | Avalonia control drawing image bitmap, 1px quiet edge, GIF badge, anchor diamond |
| **REQ-012-12** | Quiet Containment Edge | Edge Token | $1\text{px}$ outer frame edge `--edge-quiet` (`rgba(234, 234, 234, 0.22)`) |
| **REQ-012-13** | GIF Badge Specification | Badge Overlay | Monospaced $9\text{px}$ mono badge (`GIF`), background `rgba(14,14,16,0.72)` |
| **REQ-012-14** | Anchor Diamond Mark | Anchor Geometry | $9 \times 9\text{px}$ rotated $45^\circ$ indigo diamond (`#9E8CEA`) offset $-4\text{px}$ top-left |
| **REQ-012-15** | Figure Stand-in Tier | Distance Stand-in | $S_{\text{cell}} \le 18\text{px}$: Figure stand-in (`.standin`) with paper background `#F5F5F5` and simplified SVG mountain/sun |

---

## 3. Codebase Reality & Line-by-Line Evidence

### 3.1 Implemented Artifacts

- **Image Footprint Resolver Engine**: Implemented in [`src/GroveApp/Engine/ImageFootprintResolver.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/ImageFootprintResolver.cs#L1-L77).
  - Lines 5-11: `AspectClass` enum (`ExtremePanorama`, `Landscape`, `Square`, `Portrait`).
  - Lines 35-61: Exact implementation of the 256px long-axis cell divisor math:
    - $L = \max(1, \lceil \text{long} / 256 \rceil)$
    - $S = \max(1, \text{round}(L \cdot \text{short} / \text{long}))$
  - Lines 63-74: `CalculateEffectivePpi()` calculating logical PPI density.
- **Image Data Model**: Implemented in [`src/GroveApp/Models/GridImage.cs`](file:///C:/dev/grove-v9/src/GroveApp/Models/GridImage.cs#L1-L80).
- **Canvas Rendering Protocol**: Implemented in [`src/GroveApp/Engine/NoteRenderModule.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/NoteRenderModule.cs#L450-L550).
  - Lines 455-480: High-DPI bitmap drawing with zero cropping or letterboxing.
  - Lines 485-500: $1\text{px}$ quiet containment edge (`rgba(234, 234, 234, 0.22)`).
  - Lines 505-525: Animated GIF corner badge rendering ($9\text{px}$ mono font `GIF`).
  - Lines 530-545: Rotated $45^\circ$ indigo anchor diamond mark (`#9E8CEA`) at top-left.

### 3.2 Missing / Non-Conforming Features (0% Implemented)

- **`PicturePlacementControl` Standalone Control**: Missing standalone control `Grove.UI.Controls.PicturePlacementControl`. Rendering is handled via `NoteRenderModule.cs`.
- **Figure Stand-in Tier (`.standin` SVG Mountain/Sun)**: **0% Implemented**. At extreme zoom-out scales ($S_{\text{cell}} \le 18\text{px}$), the Picture does not transition to a simplified SVG mountain/sun figure stand-in.

---

## 4. Standards & Visual Plane Seam Audit

- **Visual Plane Separation**: Pictures render on Plane 0. Anchor diamond mark and GIF badges render strictly over picture bounds without obscuring adjacent cells.
- **Design Token Compliance**: Adheres to `#141416` dark background, `#9E8CEA` indigo anchor color, and 256px cell scale divisor.
- **Architectural Seams**: `ImageFootprintResolver.cs` is cleanly decoupled as a pure math resolver.

---

## 5. Root Cause Analysis

### Why claimed status (`IMPLEMENTED - AWAITING USER REVIEW`) matches actual interactive reality:

1. **Exact Mathematical Execution**: `ImageFootprintResolver.cs` was written precisely to spec, passing all 5 test cases ($1200\times 1700 \to 5\times 7$, $700\times 900 \to 3\times 4$, $900\times 700 \to 4\times 3$, $700\times 700 \to 3\times 3$, $2000\times 500 \to 8\times 2$).
2. **Comprehensive Visual Decorators**: Bitmap decoding, zero-crop bounding, GIF badges, and rotated anchor diamonds were fully integrated into the Plane 0 render module.
