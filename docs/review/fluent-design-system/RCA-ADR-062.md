# Root Cause Analysis (RCA) Ledger: ADR-062

| Property | Value |
| :--- | :--- |
| **ADR ID** | [ADR-062](file:///C:/dev/grove-v9/docs/specs/fluent-design-system/ADR-062-Skia-GPU-AntiAliasing-And-Subpixel-Typography.md) |
| **Title** | Skia GPU Anti-Aliasing and Subpixel Typography |
| **Category** | Native Fluent Design (`fluent-design-system`) |
| **Claimed Status** | `IMPLEMENTED - AWAITING USER REVIEW` |
| **Verified Status** | `NOT IMPLEMENTED / 0% LIVE UI` |
| **Audit Date** | 2026-08-12 |

---

## 1. Executive Metadata & Audit Summary

- **Claimed Implementation Files**: [`Typography.cs`](file:///C:/dev/grove-v9/src/GroveApp/DesignSystem/Typography.cs), [`GridCanvasControl.cs`](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs)
- **Verified Runtime Reality**: `Typography.cs` provides high-level design system font family strings, font size DIP constants, line height multipliers, and letter-spacing tracking definitions. However, the hardware GPU subpixel typography rendering pipeline and GPU path tessellation cache system specified in ADR-062 are **0% Implemented**. The classes `SkiaGpuPipelineManager`, `SkiaPaintDefaults`, struct `PathCacheKey`, native hardware `GRContext` integration, 1/4 DIP subpixel quantization math ($\mathbf{x}_{\text{subpixel}} = \lfloor 4\mathbf{x} + 0.5\mathbf{1} \rfloor / 4$), `SKFontEdging.SubpixelAntialias`, 3-tier zoom scale typography pipeline ($s < 0.25$ proxy bounding box, $0.25 \le s \le 2.5$ LCD glyphs, $s > 2.5$ tessellated vector paths), and LRU path cache dictionary do **NOT** exist anywhere in `src/GroveApp/`. All canvas text is rendered using Avalonia's high-level `FormattedText` wrapper.

---

## 2. Normative Specification Requirement Inventory

| Requirement ID | Spec Requirement / Symbol Name | Target Specification Details |
| :--- | :--- | :--- |
| `REQ-062-01` | Global Hardware Anti-Aliasing | Mandatory `SKPaint.IsAntialias = true` enabled globally across all vector shapes, grid lines, aura contours, and text rendering paints. |
| `REQ-062-02` | Subpixel Glyph Positioning | Glyph rendering using `SKPaint.SubpixelText = true`, `SKFont.Edging = SKFontEdging.SubpixelAntialias`, and 1/4 DIP subpixel quantization formula $\mathbf{x}_{\text{subpixel}} = \frac{\lfloor 4\mathbf{x} + 0.5\mathbf{1} \rfloor}{4}$. |
| `REQ-062-03` | Zoom-Adaptive Typography | 3-tier zoom strategy: $s < 0.25$ proxy bounding box; $0.25 \le s \le 2.5$ subpixel raster LCD glyphs; $s > 2.5$ vector path outlines via `SKFont.GetPath()` GPU tessellation. |
| `REQ-062-04` | GPU Path Cache Key & Hashing | Tessellated geometry paths keyed by $K_{\text{path}} = \langle \text{Guid}_{\text{geom}}, \lfloor 100 s + 0.5 \rfloor \rangle$ with hash equation $\text{Hash}(K_{\text{path}}) = \text{Guid}.\text{GetHashCode}() \oplus (\text{SingleToInt32Bits}(s_{\text{quant}}) \ll 5)$. |
| `REQ-062-05` | SkiaPaintDefaults Factory | Static class `SkiaPaintDefaults` providing `CreateVectorStroke()`, `CreateSpatialTextPaint()`, and `ConfigureSubpixelFont()`. |
| `REQ-062-06` | SkiaGpuPipelineManager Class | Sealed class `SkiaGpuPipelineManager` in namespace `Grove.Graphics.SkiaEngine` managing `GRContext`, `_tessellatedPathCache`, `DrawSubpixelText()`, `GetOrAddCachedPath()`, and `PurgeCache()`. |
| `REQ-062-07` | Hardware GRContext Bindings | Direct3D 11/12 / Vulkan / OpenGL native GPU context (`GRContext`) backing all canvas operations. |

---

## 3. Codebase Reality & Line-by-Line Evidence

| Requirement ID | Codebase Symbol / Location | Implementation Status & Evidence |
| :--- | :--- | :--- |
| `REQ-062-01` | `NoteRenderModule.cs` | **PARTIAL**: High-level Avalonia `Pen` anti-aliasing is enabled by default in Avalonia's DirectWrite/Skia backend, but low-level `SKPaint.IsAntialias` is not managed by Grove engine code. |
| `REQ-062-02` | `src/GroveApp/` | **0% Implemented**: `SKPaint.SubpixelText`, `SKFontEdging.SubpixelAntialias`, and 1/4 DIP subpixel quantization math $\mathbf{x}_{\text{subpixel}} = \frac{\lfloor 4\mathbf{x} + 0.5 \rfloor}{4}$ do **NOT** exist anywhere in `src/GroveApp/`. |
| `REQ-062-03` | [`NoteRenderModule.cs:150-230`](file:///C:/dev/grove-v9/src/GroveApp/Engine/NoteRenderModule.cs#L150-L230) | **NON-COMPLIANT**: `NoteRenderModule.cs` switches between Stand-In, Stepped, and Working tiers based on cell size in DIPs, but always uses high-level `FormattedText` for all text blocks above Stand-In tier. Extreme zoom ($s > 2.5$) vector path outline rendering via `SKFont.GetPath()` is **0% Implemented**. |
| `REQ-062-04` | `src/GroveApp/` | **0% Implemented**: `PathCacheKey` and the path cache hashing equation do **NOT** exist. No GPU path caching is performed. |
| `REQ-062-05` | `src/GroveApp/` | **0% Implemented**: Class `SkiaPaintDefaults` and factory methods `CreateVectorStroke`, `CreateSpatialTextPaint`, `ConfigureSubpixelFont` do **NOT** exist anywhere in the codebase. |
| `REQ-062-06` | `src/GroveApp/` | **0% Implemented**: Namespace `Grove.Graphics.SkiaEngine` and class `SkiaGpuPipelineManager` do **NOT** exist anywhere in the codebase. |
| `REQ-062-07` | `src/GroveApp/` | **0% Implemented**: Symbol `GRContext` returns 0 results across all files under `src/GroveApp/`. No direct GPU context management is written in application code. |

---

## 4. Standards & Visual Plane Seam Audit

- **Visual Plane Isolation (Plane 0 vs Plane 1 vs Plane 2)**:
  - Spec mandates low-level Skia GPU pipeline management (`GRContext`, `SKFont`, `SKPaint`) on Plane 0 for vector stroke and subpixel text rendering.
  - Codebase reality: Rendering relies entirely on Avalonia's high-level `DrawingContext.DrawText()` and `FormattedText` wrappers inside [`NoteRenderModule.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/NoteRenderModule.cs).
- **Subpixel Shimmering Artifacts**:
  - Text rendered via standard Avalonia `FormattedText` without subpixel origin quantization ($\mathbf{x}_{\text{subpixel}}$) snaps to integer pixel boundaries during continuous affine camera translation and scale operations, causing subpixel shimmering.

---

## 5. Root Cause Analysis

### 5.1 Why Gaps Exist Between Claimed Status and Interactive UI Reality
1. **Reliance on Avalonia Abstractions vs Direct Skia Interop**: The engine was built using Avalonia's `DrawingContext` wrapper (`FormattedText`, `Pen`, `Brush`) rather than accessing raw SkiaSharp `SKCanvas` / `SKFont` / `SKPaint` primitives. This prevented the implementation of 1/4 DIP subpixel quantization and `SKFontEdging.SubpixelAntialias`.
2. **Omission of Low-Level GPU Infrastructure**: Section 4 of ADR-062 specified a custom GPU pipeline manager (`SkiaGpuPipelineManager`) to manage `GRContext` resource purging, path tessellation LRU caching, and scale-adaptive vector outline rendering ($s > 2.5$). None of this low-level graphics infrastructure was ever built.
3. **Misrepresentation of Token Definitions as Architectural Implementation**: In `ADR-ROADMAP.md`, `Typography.cs` was cited as the implementation file for ADR-062. `Typography.cs` only contains basic font names and pixel size constants, leaving 100% of the actual GPU rendering pipeline unwritten.
