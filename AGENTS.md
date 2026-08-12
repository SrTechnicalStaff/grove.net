# AGENTS.md — Grove v9 AI Agent Operating Manual & Architecture Guide

This document defines the core guidelines, architectural foundations, skill decision routing, and engineering standards for AI agents working in the `grove-v9` repository.

---

## 1. Product Outcome First Philosophy

Grove evaluates architectural changes, UI design tokens, and feature work through a **Product Outcome First** lens. Technical achievements (e.g., rendering optimizations, state refactorings) are treated as implementations of human-facing behavioral outcomes.

- **Outcome Source of Truth**: All product outcomes reside in [docs/product/derived-outcomes/](file:///C:/dev/grove-v9/docs/product/derived-outcomes/).
- **Plain-English Measurement**: Outcomes are articulated strictly as plain-English shifts in user behavior without technical jargon.
- **Durable Value Rule**: Code changes must directly support or preserve an established product outcome. Pure technical abstractions without clear behavioral intent are strictly avoided.

---

## 2. Decision Router for Skill Selection

When handling tasks within `grove-v9`, agents must select and apply the appropriate skill methodology based on the nature of the requested work:

| Skill | Primary Trigger / Scenario | Expected Methodology & Output |
| :--- | :--- | :--- |
| `codebase-design` | Architectural design, module boundaries, defining clean seams, refactoring API contracts. | Focus on deep module design, hiding complexity, preventing shallow abstractions, and establishing clean interfaces before implementation. |
| `implement` | Systematic feature execution, component creation, engine integrations. | Multi-phase execution: plan, establish foundational types, construct visual components, tune engine behavior, and optimize performance. |
| `tdd` | Test-driven feature addition, red-green-refactor loops, core logic validation. | Write failing unit/integration tests first, write minimal code to pass, and refactor while maintaining green tests. |
| `code-review` | Inspecting diffs prior to committing or merging. | Two-axis review evaluating **Standards Alignment** (design system adherence, C# standards) vs **Spec Compliance** (product outcome completion). |
| `diagnosing-bugs` | Resolving unexpected behavior, crashes, or rendering glitches. | Perform root-cause diagnosis using empirical log evidence and stack traces *before* modifying code. Never apply superficial symptom patches. |
| `prototype` | Exploratory spikes, visual spikes, testing novel canvas effects. | Rapid, isolated throwaway experiments to validate technical feasibility or design choices before full integration. |
| `research` | Investigating Avalonia UI APIs, SkiaSharp behaviors, or documentation lookups. | Direct doc inspection and targeted searches to gather primary technical facts and verify assumptions. |
| `domain-modeling` | Defining domain terminology, ubiquitous language, and system concepts. | Establish ubiquitous language, record Architectural Decision Records (ADRs), and formalize domain entity contracts. |
| `resolving-merge-conflicts` | Git branch reconciliation during merges or rebases. | Systematic resolution of git merge/rebase conflicts ensuring semantic integrity of both branches. |

---

## 3. Durable Application Architecture

### 3.1 Target Framework & Core Engine Stack

- **Runtime Target**: .NET 9 (`net9.0`)
- **Primary UI Framework**: Avalonia UI `11.2.5`
- **UI Theme & Controls**: `FluentAvaloniaUI` `2.2.0`
- **Language**: C# 13.0 with modern language features enabled.
- **Rendering System**: GPU Skia vector canvas rendering via SkiaSharp integrations in custom Avalonia controls.

---

### 3.2 Visual System & 3-Plane Hierarchy

Grove v9 organizes the user experience across three strict, non-overlapping visual planes:

```
+-----------------------------------------------------------------------+
|  Layer 2: HUD Plane (Viewport-Fixed Slates, Tools, Telemetry Bars)    |
|  +-----------------------------------------------------------------+  |
|  | Layer 1: Information Layer (Content Editors & Capture Slates)  |  |
|  |  +-----------------------------------------------------------+  |  |
|  |  | Plane 0: Spatial Grid Canvas (GPU Skia Vector Canvas)     |  |  |
|  |  +-----------------------------------------------------------+  |  |
|  +-----------------------------------------------------------------+  |
+-----------------------------------------------------------------------+
```

#### Plane 0 — Spatial Grid Canvas
- **Implementation**: Implemented in [GridCanvasControl.cs](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs).
- **Grid Structure**: 220px primary cell pitch with 44px minor subdivisions.
- **Gravitational Field Physics**: Energy accumulation field governed by $E = \frac{M}{1 + 0.4 \cdot d^2}$, where $M$ represents cell mass and $d$ is spatial distance.
- **Containment & Focus**: 1.5px perimeter containment rings surrounding active content zones. Footprint-aware grid cursor reflecting target tile span.
- **Trail Physics**: 18-step spent cell trail decay physics rendering historical movement inertia across the grid.
- **Dynamic Fading**: Continuous 3-tier grid line fading spanning zoom scales from 1% to 1000%.

#### Layer 1 — Information Layer
- **Implementation**: [LocalEditorOverlay.axaml](file:///C:/dev/grove-v9/src/GroveApp/Controls/LocalEditorOverlay.axaml) (content-anchored local editors) and [QuickNoteOverlay.axaml](file:///C:/dev/grove-v9/src/GroveApp/Controls/QuickNoteOverlay.axaml) (viewport-centered capture slates).
- **Surface Styling**: Surface chrome `--surface-chrome` (`#161618`) framed with role border `--k-edit-b` (`#7A3F3A`).
- **Anchoring**: Dynamically anchored to spatial grid coordinates while handling focused text input and live preview rendering.

#### Layer 2 — HUD Plane
- **Scope**: Viewport-fixed tools, status indicators, scale readouts, layer controls, and operational slates anchored to screen space.

---

### 3.3 Decoupled Engine Modules (`src/GroveApp/Engine/`)

The core engine is structured into decoupled modules with zero unnecessary inter-dependencies:

- **[CameraModule.cs](file:///C:/dev/grove-v9/src/GroveApp/Engine/CameraModule.cs)**: Manages the 2D affine transform matrix $T(x,y,s)$ representing viewport translation and scale. Fully decoupled from content and UI overlays.
- **[FieldLedgerEngine.cs](file:///C:/dev/grove-v9/src/GroveApp/Engine/FieldLedgerEngine.cs)**: High-performance cell ledger store managing `CellLedgerEntry` objects containing energy, color, and content metadata sources. Provides an `IFieldSubscriber` pub-sub architecture powering:
  - `AuraHeatmapSubscriber`: Energy field heatmap visualization.
  - `PerimeterRingSubscriber`: Spatial boundary containment rings.
  - `AnnotationMetadataSubscriber`: Grid note metadata and link indicators.
- **[RichTextEngine.cs](file:///C:/dev/grove-v9/src/GroveApp/Engine/RichTextEngine.cs)** & **[RichTextPreviewControl.cs](file:///C:/dev/grove-v9/src/GroveApp/Controls/RichTextPreviewControl.cs)**: AST typesetting engine converting Markdown and HTML into formatted Avalonia `FormattedText` runs. Supports headings, bold, italic, underline, strikethrough, inline code pills, multiline syntax-highlighted code blocks, blockquotes, lists, and hyperlinks.
- **[KeybindModule.cs](file:///C:/dev/grove-v9/src/GroveApp/Engine/KeybindModule.cs)**: Central context-aware key binding router handling navigation and editing hotkeys (`Spacebar`, `Ctrl+Enter`, `Ctrl+E`, `Tab`, `N`, `A`, `Del`, `Esc`).
- **Render Subsystems**:
  - [NoteRenderModule.cs](file:///C:/dev/grove-v9/src/GroveApp/Engine/NoteRenderModule.cs): Direct Skia vector rendering of grid note cards, borders, and text blocks.
  - [CursorRenderModule.cs](file:///C:/dev/grove-v9/src/GroveApp/Engine/CursorRenderModule.cs): Grid cursor rendering, focus outlines, and motion trails.
  - [GridLineModule.cs](file:///C:/dev/grove-v9/src/GroveApp/Engine/GridLineModule.cs): Multi-scale 3-tier grid line rendering and LOD alpha calculations.
  - [FieldLedgerModule.cs](file:///C:/dev/grove-v9/src/GroveApp/Engine/FieldLedgerModule.cs): Bridge between ledger energy data and canvas rendering layers.

---

### 3.4 Design System Foundations (`src/GroveApp/DesignSystem/`)

The design system provides a normative single source of truth for visual tokens:

- **[Tokens.cs](file:///C:/dev/grove-v9/src/GroveApp/DesignSystem/Tokens.cs)**: Grid dimensions (220px cell pitch, 44px subdivisions), corner radii (`--r-none`), stroke weights (1.5px containment), and alpha scale constants.
- **[Colors.cs](file:///C:/dev/grove-v9/src/GroveApp/DesignSystem/Colors.cs)**: Exact color definitions and ramps:
  - Surface ramps: `#0E0E10` (Canvas Background), `#161618` (`--surface-chrome`), `#7A3F3A` (`--k-edit-b`), `#96B6F8` (Accent Slate).
  - Dynamic tokens: `--surface-grid`, `--c-note-violet`, `--c-note-clay`, `--c-note-slate-blue`, `--c-note-violet-field`, `--edge-on-color`, `--signal-interaction`, `--signal-refusal`, `--ink-primary`, `--ink-secondary`.
- **[Motion.cs](file:///C:/dev/grove-v9/src/GroveApp/DesignSystem/Motion.cs)**: Standard duration tokens (`--d-fade`, `--d-press`, `--d-place`, `--d-exit`, `--d-swap`) and cubic bezier curve animation helpers.
- **[Typography.cs](file:///C:/dev/grove-v9/src/GroveApp/DesignSystem/Typography.cs)**: Primary font families (`Inter`, `Consolas`), scale steps (`--t-body`, `--t-hero`, `--t-micro`), line height multipliers, and font weight constants.

---

## 4. Engineering Standards & Protocols

1. **Zero Compilation Warnings**: Code must compile cleanly with 0 warnings or errors via `dotnet build src/GroveApp/GroveApp.csproj`.
2. **Empirical Diagnostics First**: Never guess root causes or swallow exceptions. Always read full build outputs and logs.
3. **Strict Code Seams**: Maintain strict decoupling between engine modules (`CameraModule`, `FieldLedgerEngine`, `RichTextEngine`) and UI visual layers.
