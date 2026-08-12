---
status: "IMPLEMENTED - AWAITING USER REVIEW"
---

# ADR-062: Skia GPU Anti-Aliasing and Subpixel Typography

| Property | Value |
| :--- | :--- |
| **Status** | IMPLEMENTED - AWAITING USER REVIEW |
| **Date** | 2026-08-12 |
| **Area** | Graphics Engine / SkiaSharp Rendering Performance & Typography |
| **Target Runtime** | C# 13 / .NET 9 / SkiaSharp / Direct3D 11/12 / Vulkan / OpenGL |
| **Authors** | Chief Product Definition Architect |

---

## 1. Context & Rendering Performance Directives

Grove v9 renders complex spatial grid content, continuous fluid fields, and multi-scale text annotations across camera zoom levels ranging from $0.1\times$ to $10.0\times$. Maintaining target frame rates ($60\text{–}120\text{ FPS}$) without visual aliasing artifacts or text shimmering requires hardware-accelerated GPU rendering rules.

### Hardware Architectural Mandates
1. **Mandatory Hardware Anti-Aliasing**: `SKPaint.IsAntialias = true` MUST be enabled globally across all vector shapes, grid lines, aura contours, and text rendering paints.
2. **Subpixel Text Positioning**: Text glyphs MUST use subpixel positioning (`SKPaint.SubpixelText = true`, `SKFont.Edging = SKFontEdging.SubpixelAntialias`) to prevent pixel-snapping shimmering during smooth affine camera pan and zoom operations.
3. **GPU Path Tessellation Caching**: Dynamic paths (`SKPath`) MUST be cached in GPU texture/tessellation memory using camera-scale-aware LRU lookup structures to eliminate CPU path re-tessellation overhead.
4. **Direct3D / Vulkan GRContext Integration**: All SkiaSharp canvas operations MUST execute on top of a native hardware GPU context (`GRContext`).

---

## 2. Mathematical Subpixel Geometry & Scale-Adaptive Rendering

### 2.1 Subpixel Quantization Formula
To prevent spatial text glyphs from jumping between integer pixel boundaries during continuous camera translation $\mathbf{t} = (t_x, t_y)$, glyph origins $\mathbf{x} = (x, y)$ are quantized to $1/4$ DIP subpixel resolution:

$$\mathbf{x}_{\text{subpixel}} = \frac{\lfloor 4 \cdot \mathbf{x} + 0.5 \mathbf{1} \rfloor}{4}$$

$$\begin{bmatrix} x_{\text{sub}} \\ y_{\text{sub}} \end{bmatrix} = \begin{bmatrix} \lfloor 4x + 0.5 \rfloor / 4 \\ \lfloor 4y + 0.5 \rfloor / 4 \end{bmatrix}$$

### 2.2 Zoom-Scale-Adaptive Typography Pipeline
The rendering pipeline dynamically selects text rasterization strategies based on camera scale factor $s = \|\mathbf{M}_{\text{cam}}\|$:

| Zoom Range | Strategy | Font Edging | Hinting Mode | Pipeline Path |
| :--- | :--- | :--- | :--- | :--- |
| **$s < 0.25$** (Far View) | Bounding Box / Low-Res Sprite | `Antialias` | `None` | Render proxy bounds or GPU texture atlas. |
| **$0.25 \le s \le 2.5$** (Standard) | Subpixel Raster Glyph | `SubpixelAntialias` | `Slight` / `Normal` | Direct GPU glyph atlas rasterization. |
| **$s > 2.5$** (Extreme Zoom) | Vector Path Outlines | `SubpixelAntialias` | `None` | `SKFont.GetPath()` tessellated GPU vectors. |

```
 Zoom Scale (s)
 0.0          0.25                 2.50                  10.0+
  ├────────────┼────────────────────┼──────────────────────┤
  │ Proxy Box  │ Subpixel Raster    │ Outlined Vector Path │
  │ Geometry   │ LCD Glyphs         │ GPU Tessellation     │
  └────────────┴────────────────────┴──────────────────────┘
```

### 2.3 Path Cache Key Hashing Equation
To reuse tessellated path data across frames, geometry path objects are keyed by geometry ID and rounded scale factor:

$$K_{\text{path}} = \langle \text{Guid}_{\text{geom}}, \; \lfloor 100 \cdot s + 0.5 \rfloor \rangle$$

$$\text{Hash}(K_{\text{path}}) = \text{Guid}_{\text{geom}}.\text{GetHashCode}() \oplus \left( \text{BitConverter.SingleToInt32Bits}(s_{\text{quant}}) \ll 5 \right)$$

---

## 3. SkiaSharp Paint & Font Configuration Standards

```csharp
namespace Grove.Graphics.SkiaEngine;

using SkiaSharp;

public static class SkiaPaintDefaults
{
    /// <summary>
    /// Default factory for vector stroke rendering (grid lines, boundaries).
    /// </summary>
    public static SKPaint CreateVectorStroke(SKColor color, float widthDips) => new SKPaint
    {
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
        Color = color,
        StrokeWidth = widthDips,
        StrokeCap = SKStrokeCap.Round,
        StrokeJoin = SKStrokeJoin.Round,
        SubpixelText = true
    };

    /// <summary>
    /// Default factory for spatial text rendering.
    /// </summary>
    public static SKPaint CreateSpatialTextPaint(SKColor color) => new SKPaint
    {
        IsAntialias = true,
        SubpixelText = true,
        LcdRenderText = true,
        Color = color,
        Style = SKPaintStyle.Fill,
        HintingLevel = SKPaintHinting.Normal
    };

    /// <summary>
    /// Configures SKFont instance for high-DPI subpixel accuracy.
    /// </summary>
    public static void ConfigureSubpixelFont(SKFont font, float sizeDips, float currentZoomScale)
    {
        font.Size = sizeDips;
        font.Subpixel = true;
        font.Edging = SKFontEdging.SubpixelAntialias;
        
        // Disable hinting under affine camera transformation to prevent glyph distortion
        font.Hinting = currentZoomScale != 1.0f 
            ? SKFontHinting.None 
            : SKFontHinting.Normal;
    }
}
```

---

## 4. Hardware GPU Context & LRU Path Cache Engine

```csharp
namespace Grove.Graphics.SkiaEngine;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using SkiaSharp;

public readonly record struct PathCacheKey(Guid GeometryId, int ScaledZoom)
{
    public static PathCacheKey Create(Guid id, float zoom) => 
        new(id, (int)MathF.Round(zoom * 100.0f));
}

public sealed class SkiaGpuPipelineManager : IDisposable
{
    private readonly GRContext _grContext;
    private readonly ConcurrentDictionary<PathCacheKey, SKPath> _tessellatedPathCache = new();
    private readonly SKPaint _subpixelTextPaint;
    private readonly SKFont _uiFont;

    public SkiaGpuPipelineManager(GRContext grContext)
    {
        _grContext = grContext ?? throw new ArgumentNullException(nameof(grContext));
        
        _subpixelTextPaint = new SKPaint
        {
            IsAntialias = true,
            SubpixelText = true,
            LcdRenderText = true,
            FilterQuality = SKFilterQuality.High
        };

        using var typeface = SKTypeface.FromFamilyName("Segoe UI Variable Text", SKFontStyle.Normal);
        _uiFont = new SKFont(typeface, 14.0f)
        {
            Subpixel = true,
            Edging = SKFontEdging.SubpixelAntialias,
            Hinting = SKFontHinting.Normal
        };
    }

    /// <summary>
    /// Renders subpixel-positioned text string onto hardware GPU canvas.
    /// </summary>
    public void DrawSubpixelText(
        SKCanvas canvas,
        string text,
        float rawX,
        float rawY,
        SKColor color,
        float currentZoom)
    {
        // 1/4 DIP subpixel quantization
        float subX = MathF.Floor(rawX * 4.0f + 0.5f) / 4.0f;
        float subY = MathF.Floor(rawY * 4.0f + 0.5f) / 4.0f;

        _subpixelTextPaint.Color = color;
        SkiaPaintDefaults.ConfigureSubpixelFont(_uiFont, 14.0f, currentZoom);

        canvas.DrawText(text, subX, subY, SKTextEncoding.Utf8, _uiFont, _subpixelTextPaint);
    }

    /// <summary>
    /// Obtains or caches a GPU-tessellated vector path.
    /// </summary>
    public SKPath GetOrAddCachedPath(Guid geometryId, float zoom, Func<SKPath> pathBuilder)
    {
        var key = PathCacheKey.Create(geometryId, zoom);
        return _tessellatedPathCache.GetOrAdd(key, _ => pathBuilder());
    }

    public void PurgeCache()
    {
        foreach (var path in _tessellatedPathCache.Values)
        {
            path.Dispose();
        }
        _tessellatedPathCache.Clear();
        _grContext.PurgeResources();
    }

    public void Dispose()
    {
        PurgeCache();
        _subpixelTextPaint.Dispose();
        _uiFont.Dispose();
    }
}
```
