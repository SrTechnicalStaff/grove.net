---
status: "NORMATIVE SPECIFICATION — HEADERLESS SHARP-EDGE NOTEPAD"
verification: "PARTIAL — the in-canvas headerless editor is implemented; standalone Mica hosting and GPU-specific rendering remain bounded gaps."
---

# ADR-064: Headerless Pure Fluent Notepad Editor Specification

| Property | Value |
| :--- | :--- |
| **Document Type** | Design & Architectural Replacement Specification |
| **Replaces / Deprecates** | [`ADR-060`](file:///C:/dev/grove-v9/docs/specs/fluent-design-system/ADR-060-Fluent-Local-Editor-Notepad-Design.md), [`LocalEditorOverlay.axaml`](file:///C:/dev/grove-v9/src/GroveApp/Controls/LocalEditorOverlay.axaml) |
| **Target Runtime** | Avalonia `11.2.5` / FluentAvalonia `2.2.0` / SkiaSharp `3.x` |
| **Corner Radius Policy** | **Sharp Edges / Low Radii Always**: `Tokens.CornerRadiusNone` (`0.0px`) / `Tokens.CornerRadiusXs` (`1.0px`) |
| **Padding Policy** | **Minimal Padding**: `Tokens.SpacingXs` (`4.0px`) / `Tokens.SpacingSm` (`8.0px`) |
| **Material Contract** | **Native Windows 11 Mica Backdrop** (`WindowTransparencyLevel.Mica`) |
| **Rendering Contract** | **Skia GPU Subpixel Anti-Aliasing** (`SKFontEdging.SubpixelAntialias`) |
| **Date** | 2026-08-12 |
| **Authors** | Chief Product Definition Architect |

---

## 1. Executive Context & Design System Alignment

Notes do not have titles. Header rows, window icons, title strings, subtitling, over-explanational titling, and top-right window controls serve zero operational purpose and are **100% deleted**.

### Design System Rules & Seam Discipline
- **Sharp Edges / Low Radii Always**: Bounded strictly to `Tokens.CornerRadiusNone` (`0.0px`) or `Tokens.CornerRadiusXs` (`1.0px`). High corner radii (e.g. 8px/12px) are strictly forbidden across the entire design system.
- **Minimal Padding**: Uses tight, minimal padding (`Tokens.SpacingXs` = 4px, `Tokens.SpacingSm` = 8px). Double padding, thick outer rims, and nested padding boxes are strictly forbidden.
- **Zero Outer Box Stacking**: The editor canvas rests directly on the single Mica backdrop surface host. No inner frames, no role borders (`#7A3F3A`), no nested cards.
- **Deep Module Seam**: `INotepadStorageService` exposes a simple, testable interface (`Save(noteId, text)`, `Load(noteId)`) hiding disk I/O, draft buffers, and spatial grid synchronization logic.

---

## 2. Headerless Notepad Surface Anatomy & Tokens

```
+-----------------------------------------------------------------------------------+
|  Mica Backdrop Surface (WindowTransparencyLevel.Mica, --r-none = 0px)             |
|                                                                                   |
|  Continuous 2D spatial grid plane acting as Plane 0.                              |
|  Enforces physical paper proportions and whole-cell footprints.                    |
|                                                                                   |
|  - Sharp 0px/1px low radii (Tokens.CornerRadiusNone).                             |
|  - Minimal 4px/8px padding (Tokens.SpacingXs / Tokens.SpacingSm).                 |
|  - Skia GPU subpixel antialiasing (SKFontEdging.SubpixelAntialias).                |
|  - Fixed 68ch reading measure (--measure-reading = 640px).                        |
|  - Monospaced Cascadia Code / Consolas (--f-mono).                                |
|  - ZERO headers, ZERO subtitling, ZERO CTAs, ZERO nested boxes.                   |
|  - Immediate caret focus on open.                                                 |
|  - Auto-saves and dismisses on Escape or Blur.                                    |
|                                                                                   |
|                                                                                   |
+-----------------------------------------------------------------------------------+
| 1:1  •  100%  •  UTF-8                                                            | <-- Minimal Telemetry Line (--t-micro, --text-meta)
+-----------------------------------------------------------------------------------+
```

### 2.1 Design Token Specification

| Part Name | Design System Token | Value | Contract Rule |
| :--- | :--- | :--- | :--- |
| **Material Surface** | `Colors.SurfaceChrome` | `WindowTransparencyLevel.Mica` | Windows 11 DWM Mica backdrop. |
| **Corner Radius** | `Tokens.CornerRadiusNone` | `0.0px` (`--r-none`) | Sharp 0px corners across container. Low radii always. |
| **Padding** | `Tokens.SpacingSm` | `8.0px` (`--sp-sm`) | Minimal padding. Zero nested padding boxes. |
| **Reading Measure** | `Tokens.MeasureReading` | `640.0px` (`68ch`) | Max-width reading measure constraint. |
| **Font Family** | `Typography.FontFamilyMono` | `Cascadia Code, Consolas, monospace` | Primary monospaced authoring typeface (`--f-mono`). |
| **Primary Ink** | `Colors.TextPrimary` | `#EAEAEC` | High-contrast primary text ink. |
| **Telemetry Text** | `Colors.TextMeta` / `Typography.SizeMicro` | `#8A8A92` / `9.0pt` | Muted monospaced telemetry line (`1:1  •  100%  •  UTF-8`). |
| **Dismiss Motion** | `Motion.DurationFadeMs` | `120ms` (`--d-fade`) | Instant fade animation on `Escape` or Blur. |
