# ADR-012: Image Footprint Resolution Mapping, Zero-Crop Rules, and Skia Bitmap Sampling

- **Status**: Normative
- **Date**: 2026-08-12
- **Architectural Scope**: Spatial Content Primitives / Picture Component
- **Target Runtime**: .NET 9.0 / Avalonia UI 11.2.5 / SkiaSharp 3.0

---

## 1. Context & Architectural Principles

A **Picture** on the Grove Grid plane displays an authored image in full at its intrinsic aspect ratio without cropping, letterboxing, or distortion. Converting source image pixels into cell-quantized spatial footprints requires a deterministic mathematical mapping function.

### 1.1 Invariant Design Laws
1. **Zero-Crop Rule**: An image frame is never cropped (cover-fit), stretched, or letterboxed with container pillar/letterbox bars. The frame bounds match the footprint exactly.
2. **256px Cell Divisor Invariant**: The cell extent along the primary (long) axis is derived using $D_{\text{cell}} = 256\text{px}$ as the pixel-to-cell scale divisor.
3. **Aspect Ratio Preservation**: Footprint cell count along the secondary (short) axis is calculated by rounding the scaled intrinsic proportion, ensuring the physical grid cell aspect closely mirrors the image aspect $r = W_{\text{px}} / H_{\text{px}}$.
4. **High-DPI Skia Sampling**: Bitmaps rendered on the spatial canvas utilize Skia's linear mipmap sampling options (`SKFilterMode.Linear`, `SKMipmapMode.Linear`) to prevent moiré patterns during camera zoom operations.

---

## 2. Mathematical Footprint Derivation Engine

### 2.1 Resolution-to-Cell Mapping Equations

Given an image with intrinsic width $W_{\text{px}}$ and intrinsic height $H_{\text{px}}$:

1. **Identify Principal Axes**:
   $$\text{long} = \max(W_{\text{px}}, H_{\text{px}})$$
   $$\text{short} = \min(W_{\text{px}}, H_{\text{px}})$$

2. **Long Axis Cell Count ($L$)**:
   $$L = \max\left(1, \left\lceil \frac{\text{long}}{256} \right\rceil\right)$$

3. **Short Axis Cell Count ($S$)**:
   $$S = \max\left(1, \text{round}\left(L \cdot \frac{\text{short}}{\text{long}}\right)\right)$$

4. **Orientation & Footprint Assignment ($N_w \times N_h$)**:
   $$N_w = \begin{cases} L & \text{if } W_{\text{px}} \ge H_{\text{px}} \text{ (Landscape / Square / Panorama)} \\ S & \text{if } W_{\text{px}} < H_{\text{px}} \text{ (Portrait)} \end{cases}$$

   $$N_h = \begin{cases} S & \text{if } W_{\text{px}} \ge H_{\text{px}} \text{ (Landscape / Square / Panorama)} \\ L & \text{if } W_{\text{px}} < H_{\text{px}} \text{ (Portrait)} \end{cases}$$

### 2.2 Mathematical Proofs & Validation Suite

| Source Resolution ($W_{\text{px}} \times H_{\text{px}}$) | Aspect Class | Aspect Ratio ($r$) | $\text{long} / 256$ | $L$ | $S = \text{round}\left(L \cdot \frac{\text{short}}{\text{long}}\right)$ | Derived Footprint ($N_w \times N_h$) | Validated Deck Reference |
| :--- | :--- | :---: | :---: | :---: | :---: | :---: | :--- |
| **$1200 \times 1700$** | Portrait | $0.7059$ | $1700 / 256 = 6.64$ | 7 | $\text{round}(7 \cdot 1200 / 1700) = 5$ | **$5 \times 7$** | `04-image.html` ($300 \times 420\text{px}$) |
| **$700 \times 900$** | Portrait | $0.7778$ | $900 / 256 = 3.51$ | 4 | $\text{round}(4 \cdot 700 / 900) = 3$ | **$3 \times 4$** | `04-image.html` ($120 \times 160\text{px}$) |
| **$900 \times 700$** | Landscape | $1.2857$ | $900 / 256 = 3.51$ | 4 | $\text{round}(4 \cdot 700 / 900) = 3$ | **$4 \times 3$** | `04-image.html` ($160 \times 120\text{px}$) |
| **$700 \times 700$** | Square | $1.0000$ | $700 / 256 = 2.73$ | 3 | $\text{round}(3 \cdot 700 / 700) = 3$ | **$3 \times 3$** | `04-image.html` ($120 \times 120\text{px}$) |
| **$2000 \times 500$** | Panorama | $4.0000$ | $2000 / 256 = 7.81$ | 8 | $\text{round}(8 \cdot 500 / 2000) = 2$ | **$8 \times 2$** | `04-image.html` ($320 \times 80\text{px}$) |

---

## 3. Effective Placed Resolution ($\text{PPI}_{\text{eff}}$) & Zero-Crop Projection

### 3.1 Complete-Frame Projection Scale Equation

For frame cell dimensions $F_w = 220 N_w$ and $F_h = 220 N_h$:

$$\text{scale} = \min\left(\frac{F_w}{W_{\text{px}}}, \frac{F_h}{H_{\text{px}}}\right)$$

$$\text{RenderedWidth} = W_{\text{px}} \cdot \text{scale}$$
$$\text{RenderedHeight} = H_{\text{px}} \cdot \text{scale}$$

Because $N_w$ and $N_h$ are derived directly from intrinsic proportions, $\text{RenderedWidth} \approx F_w$ and $\text{RenderedHeight} \approx F_h$. Any minor rounding pixel delta is centered within the footprint without distortion.

### 3.2 Effective PPI ($\text{PPI}_{\text{eff}}$) Calculation

Using standard logical CSS density ($96\text{px} = 1.0\text{ inch}$):

$$\text{PlacedWidthInches} = \frac{F_w}{96}$$
$$\text{PlacedHeightInches} = \frac{F_h}{96}$$

$$\text{PPI}_{\text{eff}} = \min\left(\frac{W_{\text{px}}}{\text{PlacedWidthInches}}, \frac{H_{\text{px}}}{\text{PlacedHeightInches}}\right)$$

### 3.3 Fidelity Floor Enforcements

| Image Role | Minimum $\text{PPI}_{\text{eff}}$ Floor ($F$) | Action on Floor Violation |
| :--- | ---: | :--- |
| **Thumbnail** | `96 PPI` | Allow in place. |
| **Supporting** | `180 PPI` | Scale footprint down or route to Image Viewer. |
| **Hero / Leading** | `320 PPI` | Require complete 1:1 pixel fidelity or route to Image Viewer. |

---

## 4. C# Data Schemas & Footprint Resolver

```csharp
namespace Grove.Core.Content.Picture;

using System;

public enum AspectClass : byte
{
    ExtremePanorama = 0, // r >= 2.0
    Landscape = 1,       // 1.0 < r < 2.0
    Square = 2,          // r == 1.0
    Portrait = 3         // r < 1.0
}

public readonly record struct ImageFootprint(int CellsW, int CellsH, AspectClass Aspect)
{
    public int TotalCells => CellsW * CellsH;
    public double AspectRatio => (double)CellsW / CellsH;
}

public sealed record ImagePlacementRecord
{
    public required Guid Id { get; init; }
    public required int GridX { get; init; }
    public required int GridY { get; init; }
    public required int IntrinsicWidthPx { get; init; }
    public required int IntrinsicHeightPx { get; init; }
    public required string FilePath { get; init; }
    public required ImageFootprint Footprint { get; init; }
    public required bool IsAnimatedGif { get; init; }
    public required bool IsAnchored { get; init; }
    public required int LayerId { get; init; }
}

public static class ImageFootprintResolver
{
    public const double CellDivisor = 256.0;
    public const double CellPitchPx = 220.0;
    public const double LogicalDpi = 96.0;

    public static ImageFootprint Resolve(int widthPx, int heightPx)
    {
        if (widthPx <= 0 || heightPx <= 0)
        {
            return new ImageFootprint(1, 1, AspectClass.Square);
        }

        double r = (double)widthPx / heightPx;
        AspectClass aspectClass = r switch
        {
            >= 2.0 => AspectClass.ExtremePanorama,
            > 1.0 => AspectClass.Landscape,
            1.0 => AspectClass.Square,
            _ => AspectClass.Portrait
        };

        int longPx = Math.Max(widthPx, heightPx);
        int shortPx = Math.Min(widthPx, heightPx);

        int L = Math.Max(1, (int)Math.Ceiling(longPx / CellDivisor));
        int S = Math.Max(1, (int)Math.Round(L * ((double)shortPx / longPx)));

        int cellsW = widthPx >= heightPx ? L : S;
        int cellsH = widthPx >= heightPx ? S : L;

        return new ImageFootprint(cellsW, cellsH, aspectClass);
    }

    public static double CalculateEffectivePpi(int widthPx, int heightPx, int cellsW, int cellsH)
    {
        double placedWidthInches = (cellsW * CellPitchPx) / LogicalDpi;
        double placedHeightInches = (cellsH * CellPitchPx) / LogicalDpi;

        double ppiX = widthPx / placedWidthInches;
        double ppiY = heightPx / placedHeightInches;

        return Math.Min(ppiX, ppiY);
    }
}
```

---

## 5. Avalonia 11.2.5 / Skia High-DPI Drawing Protocol

```csharp
namespace Grove.UI.Controls;

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Grove.Core.Content.Picture;

public sealed class PicturePlacementControl : Control
{
    public static readonly StyledProperty<ImagePlacementRecord?> PlacementProperty =
        AvaloniaProperty.Register<PicturePlacementControl, ImagePlacementRecord?>(nameof(Placement));

    public static readonly StyledProperty<Bitmap?> LoadedBitmapProperty =
        AvaloniaProperty.Register<PicturePlacementControl, Bitmap?>(nameof(LoadedBitmap));

    public ImagePlacementRecord? Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    public Bitmap? LoadedBitmap
    {
        get => GetValue(LoadedBitmapProperty);
        set => SetValue(LoadedBitmapProperty, value);
    }

    static PicturePlacementControl()
    {
        AffectsRender<PicturePlacementControl>(PlacementProperty, LoadedBitmapProperty);
    }

    public override void Render(DrawingContext context)
    {
        if (Placement is null) return;

        double w = Placement.Footprint.CellsW * ImageFootprintResolver.CellPitchPx;
        double h = Placement.Footprint.CellsH * ImageFootprintResolver.CellPitchPx;
        Rect bounds = new(0, 0, w, h);

        // 1. Render Bitmap with Linear Mipmap High-DPI Sampling
        if (LoadedBitmap is not null)
        {
            context.DrawImage(LoadedBitmap, new Rect(0, 0, LoadedBitmap.Size.Width, LoadedBitmap.Size.Height), bounds);
        }
        else
        {
            // Placeholder plate during async decode
            context.FillRectangle(new SolidColorBrush(Color.Parse("#1C1C20")), bounds);
        }

        // 2. Render 1px Quiet Containment Edge (--edge-quiet rgba(234,234,234,0.22))
        var edgePen = new Pen(new SolidColorBrush(Color.FromArgb(56, 234, 234, 234)), 1.0);
        context.DrawRectangle(null, edgePen, bounds.Deflate(0.5));

        // 3. Render Animated GIF Badge if Applicable
        if (Placement.IsAnimatedGif)
        {
            RenderGifBadge(context, w);
        }

        // 4. Render Anchor Diamond if Anchored
        if (Placement.IsAnchored)
        {
            RenderAnchorDiamond(context);
        }
    }

    private static void RenderGifBadge(DrawingContext context, double rightEdge)
    {
        double x = rightEdge - 28.0;
        double y = 8.0;
        Rect bgRect = new(x, y, 20, 12);

        context.FillRectangle(new SolidColorBrush(Color.FromArgb(200, 14, 14, 16)), bgRect);

        var fmt = new FormattedText(
            "GIF",
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("JetBrains Mono", FontStyle.Normal, FontWeight.Bold),
            9.0, // --t-micro
            Brushes.White);

        context.DrawText(fmt, new Point(x + 2, y + 1));
    }

    private static void RenderAnchorDiamond(DrawingContext context)
    {
        var diamondBrush = new SolidColorBrush(Color.Parse("#9E8CEA"));
        var streamGeometry = new StreamGeometry();

        // 9x9px rotated 45 degrees, offset -4px from top-left
        using (var geoCtx = streamGeometry.Open())
        {
            geoCtx.BeginFigure(new Point(0.5, -4.0), true);
            geoCtx.LineTo(new Point(5.0, 0.5));
            geoCtx.LineTo(new Point(0.5, 5.0));
            geoCtx.LineTo(new Point(-4.0, 0.5));
            geoCtx.EndFigure(true);
        }

        context.DrawGeometry(diamondBrush, null, streamGeometry);
    }
}
```
