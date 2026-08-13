# RCA: Nomenclature Poisoning — Overloading "Layer" vs "Plane"

| Metadata Field    | Value                                                                     |
| ----------------- | ------------------------------------------------------------------------- |
| **Status**        | Resolved / Normative Directive                                            |
| **Date**          | 2026-08-12                                                                |
| **Authors**       | Senior Technical Staff / Grove Architecture Group                         |
| **Classification**| Root Cause Analysis (RCA) & Ubiquitous Language Correction Directive      |
| **Target Files**  | [`AGENTS.md`](file:///C:/dev/grove-v9/AGENTS.md), [`docs/review/`](file:///C:/dev/grove-v9/docs/review/) |

---

## 1. Executive Summary & Symptom Statement

During architectural review and developer interaction, significant domain ambiguity was identified surrounding the term **"Layer"**. The term was being overloaded to describe two fundamentally different architectural concepts:

1. **Spatial Grid Stack Continuum** (`01`, `02`, `B01`, etc.): Addressable 2D grid depth bands where content items reside, permit multi-occupancy, and participate in vertical energy permeability (\(E \cdot 0.5^{|\Delta Z|}\)).
2. **Viewport Compositor Hierarchy** (Plane 0, Plane 1, Plane 2): Visual z-order stacking separating the GPU Skia canvas (Plane 0), content editor overlays (Plane 1), and viewport-fixed HUD tools (Plane 2).

In documentation artifacts—specifically [`AGENTS.md`](file:///C:/dev/grove-v9/AGENTS.md#L55) (lines 55 & 71) and several RCA ledgers in [`docs/review/`](file:///C:/dev/grove-v9/docs/review/)—the viewport planes were improperly labeled as *"Layer 1: Information Plane"* and *"Layer 2: HUD Plane"*.

This cross-contamination created **Nomenclature Poisoning**: developers and AI sub-agents began confusing Spatial Layer navigation (`01` \(\leftrightarrow\) `02`) with UI overlay rendering (Plane 1 vs Plane 2), leading to specification drift, broken event routing, and corrupted domain abstractions.

---

## 2. Empirical Root Cause Breakdown

```
Nomenclature Poisoning Mechanics:

  [Standard Graphics Cliché]                [Grove v9 Domain Reality]
  UI Overlay = "Layer 1"  ─── POISON ───►  "Layer" MUST BE EXCLUSIVE TO GRID
  HUD Slates = "Layer 2"   ─── DRIFT  ───►  Viewport Hierarchy MUST BE "Planes"
```

### 2.1 Cause 1: Graphic Design Slippage
In conventional desktop software (Photoshop, Illustrator, Figma), the word "Layer" is commonly used to describe UI z-index stacking (background, foreground content, UI chrome). When Grove v9 documentation was authored, writers informally reused "Layer 1" and "Layer 2" to describe viewport z-ordering.

### 2.2 Cause 2: Domain Model Collision
In Grove v9, **Layer** was chosen as a formal domain concept for the **Spatial Grid Depth Continuum** ([ADR-040](file:///C:/dev/grove-v9/docs/specs/spatial-layers/ADR-040-Spatial-Layer-System-Architecture.md)):
- Content items possess an immutable `LayerId`.
- Two items on different spatial layers (`01` vs `02`) can occupy the same 2D cell coordinate \((x,y)\).
- Energy field ledgers calculate vertical attenuation across spatial layers.

When the documentation simultaneously referred to Plane 1 as "Layer 1", the boundary between spatial Grid depth and viewport UI chrome collapsed.

### 2.3 Cause 3: AI Agent & Specification Drift
AI sub-agents inspecting [`AGENTS.md`](file:///C:/dev/grove-v9/AGENTS.md) picked up the term "Layer 1" and generated code where spatial item operations (like layer isolation `Ctrl+I` or item layer migration) incorrectly attempted to manipulate UI editor overlay controls (`FluentNotepadEditor`), creating architectural seam leaks.

---

## 3. Impact Analysis

| System Dimension | Symptom of Nomenclature Poisoning | Architectural Impact |
| :--- | :--- | :--- |
| **Domain Model** | Confusion between `GridContentItem.LayerId` and UI Slate visibility | Developers attempted to assign `LayerId` to HUD controls. |
| **Keybind Routing** | `[`, `]` spatial layer keys confused with `L` HUD slate toggles | Keybind router logic mixed spatial navigation with HUD visibility. |
| **Render Engine** | Skia canvas pass confused with Avalonia XAML overlay layout | Inactive spatial layer ghost outlines were misrouted into Plane 1 layout controls. |
| **AI Agent Navigation**| Sub-agents reading `AGENTS.md` hallucinated "Information Planes" as spatial Layers | Specification drift across generated code and review ledgers. |

---

## 4. Normative Corrective Action & Ubiquitous Language Directive

Effective immediately, the following **Ubiquitous Language Rules** are strictly binding across all code, comments, documentation, and agent prompts in `grove-v9`:

### Rule 1: "Layer" is 100% Exclusive to the Spatial Grid
The word **`Layer`** (capitalized or lower-case) is **strictly and exclusively reserved** for Spatial Grid Layers (`01`, `02`, `B01`, etc.) within the continuous depth stack of Plane 0.
- **Prohibited**: Never use "layer" to refer to text editors, capture slates, HUD controls, overlays, or window z-indexes.

### Rule 2: Viewport Z-Ordering is 100% Exclusive to "Planes"
The depth hierarchy of the screen viewport is strictly named **`Plane`**:
- **Plane 0 — Spatial Grid Canvas**: GPU Skia vector canvas rendering grid lines, cell ledgers, aura energy heatmaps, spent cell decay trails, grid cursor, and spatial placements.
- **Plane 1 — Information Plane**: Viewport/cell-anchored local content editors ([`FluentNotepadEditor.axaml`](file:///C:/dev/grove-v9/src/GroveApp/Controls/FluentNotepadEditor.axaml)) and capture overlays ([`QuickNoteOverlay.axaml`](file:///C:/dev/grove-v9/src/GroveApp/Controls/QuickNoteOverlay.axaml)).
- **Plane 2 — HUD Tools**: Viewport-fixed tools, status indicators, scale readouts, grid layer controls ([`LayerManagerOverlay.axaml`](file:///C:/dev/grove-v9/src/GroveApp/Controls/LayerManagerOverlay.axaml)), and watermark telemetry.

---

## 5. Remediation Verification & Documentation Scrub

1. **[`AGENTS.md`](file:///C:/dev/grove-v9/AGENTS.md)**: The required vocabulary is "Plane 1 — Information Plane" and "Plane 2 — HUD Plane"; this review ledger does not edit `AGENTS.md`.
2. **Review Ledgers**: Update all references in [`docs/review/`](file:///C:/dev/grove-v9/docs/review/) to strictly enforce Plane 0 / Plane 1 / Plane 2 visual isolation language.
