---
status: "IMPLEMENTED - AWAITING USER REVIEW"
---

# ADR-010: Note Physical Geometry, Authored Fills, and Grid Placement

- **Status**: Normative
- **Date**: 2026-08-12
- **Architectural Scope**: Spatial Content Primitives / Note Component
- **Target Runtime**: .NET 9.0 / Avalonia UI 11.2.5 / SkiaSharp 3.0

---

## 1. Context & Architectural Principles

A **Note** represents a single thought authored by a person, placed directly onto the spatial Grid plane. Unlike hierarchical documents or unconstrained canvas cards, a Note strictly adheres to cell-quantized physical geometry, deterministic type measurement, authored color fills, and non-distorting square footprints.

### 1.1 Invariant Design Laws
1. **Square Solver Primacy**: A Note's minimum footprint is the smallest integer square $n \times n$ cells ($n \ge 1$) that encloses its complete, un-truncated text without interior scrolling, text scaling, or text clipping.
2. **Fixed Typography**: Text is rendered in Inter 500 (`--f-ui`) at a fixed reading size of $15\text{px}$ (`--t-body`), line height ratio $1.42$ (`--lh-snug`), and ink `#F4F4F2`. Text size never scales with footprint enlargement or camera distance.
3. **Restricted Authored Palette**: Exactly three authored color fills are permitted: Violet (`#6E62A6`), Clay (`#B0524E`), and Slate Blue (`#4E6E9C`). No arbitrary hex codes or fourth fills are authorized.
4. **Field Radiation**: A Note casts an atmospheric presence field across surrounding grid cells using its own fill RGB triplet. Anchored notes override this field with the authored context signal (`#9E8CEA`).

---

## 2. Mathematical Footprint & Geometry Solver

### 2.1 Cell Pitch & Content Box Derivation

Let $P_{\text{cell}} = 220\text{px}$ be the canonical grid cell pitch (`--grid-cell`).
For a Note occupying an $n \times n$ cell footprint ($n \in \mathbb{Z}^+$):

- **Outer Physical Dimensions**:
  $$\text{Width}_{\text{outer}}(n) = n \cdot P_{\text{cell}} = 220n \quad (\text{px})$$
  $$\text{Height}_{\text{outer}}(n) = n \cdot P_{\text{cell}} = 220n \quad (\text{px})$$

- **Padding Boundaries**:
  - Horizontal Padding: $15\text{px}$ left, $15\text{px}$ right ($\Delta W_{\text{pad}} = 30\text{px}$)
  - Vertical Padding: $16\text{px}$ top, $16\text{px}$ bottom ($\Delta H_{\text{pad}} = 32\text{px}$)

- **Inner Content Box**:
  $$W_{\text{box}}(n) = 220n - 30 \quad (\text{px})$$
  $$H_{\text{box}}(n) = 220n - 32 \quad (\text{px})$$

For the default $1 \times 1$ cell pitch ($n = 1$):
- $W_{\text{box}}(1) = 220 - 30 = 190\text{px}$
- $H_{\text{box}}(1) = 220 - 32 = 188\text{px}$

```
+-------------------------------------------------------+  (0, 0) Outer Cell Edge
| 1px Inset Edge (--edge-on-color rgba(255,255,255,0.12)|
|  +-------------------------------------------------+  |  (15px, 16px) Content Box Top-Left
|  | Inter 500 @ 15px (--t-body), Line-Height 1.42    |  |
|  | Ink: #F4F4F2, Snug wrapping at W_box           |  |
|  |                                                 |  |
|  |                                                 |  |
|  +-------------------------------------------------+  |  (W_box, H_box)
|                                                       |
+-------------------------------------------------------+  (220n, 220n) Outer Cell Edge
```

### 2.2 Discrete Square Footprint Solver Algorithm

Given authored string text $S$, the solver determines the minimal integer cell count $n_{\text{min}}$:

$$\text{TextHeight}(S, W) = \left\lceil \text{MeasureFormattedText}(S, W, f_{\text{body}}, \text{lh}_{\text{snug}}) \right\rceil$$

$$n_{\text{min}} = \min \left\{ n \in \mathbb{Z}^+ \;\middle|\; \text{TextHeight}(S, 220n - 30) \le (220n - 32) \right\}$$

The active footprint extent $N$ is governed by hysteresis:
$$N = \max\left(n_{\text{min}}, n_{\text{user}}\right)$$

Where $n_{\text{user}}$ is the user's explicitly dragged corner extent. If a user resizes below $n_{\text{min}}$, the layout system **refuses** the operation rather than truncating text.

---

## 3. Authored Fills, Containment, Selection & Anchor Geometry

### 3.1 Authored Color Fills & Contrast Specification

| Color Name | Token | Hex Code | RGB Triplet | Text Ink | Text Contrast (WCAG) | Canvas Contrast | Field Triplet |
| :--- | :--- | :--- | :--- | :--- | :---: | :---: | :--- |
| **Violet** | `--c-note-violet` | `#6E62A6` | `110, 98, 166` | `#F4F4F2` | 4.81:1 | 3.64:1 | `110 98 166` |
| **Clay** | `--c-note-clay` | `#B0524E` | `176, 82, 78` | `#F4F4F2` | 4.58:1 | 3.82:1 | `176 82 78` |
| **Slate Blue** | `--c-note-slate-blue` | `#4E6E9C` | `78, 110, 156` | `#F4F4F2` | 4.73:1 | 3.70:1 | `78 110 156` |

### 3.2 Edge & Ring Geometry

1. **Containment Edge**: $1\text{px}$ inset line rendered along the outer boundary using token `--edge-on-color` (`rgba(255, 255, 255, 0.12)` or equivalent `#6E6E6A` overlay).
2. **Selection Outline**: $2\text{px}$ stroke in `--signal-interaction` (`#96B6F8` / RGB `150, 182, 248`), offset exactly $3\text{px}$ **outside** the containment edge (bounding box $x = -5, y = -5, w = 220n + 10, h = 220n + 10$).
3. **Anchor Ribbon (`A` Keybind)**:
   - Width: $10\text{px}$ with a $45^\circ$ inverted V-notch cut into the bottom edge.
   - Fill: `--signal-authored-context` (`#9E8CEA` / RGB `158, 140, 234`).
   - Position: Left edge offset $16\text{px}$ (`--sp-md`) from Note left edge. Hangs $5\text{px}$ above top edge and extends $17\text{px}$ down over the Note head.

---

## 4. C# / Avalonia 11.2.5 Data Schemas & Layout Resolver

```csharp
namespace Grove.Core.Content.Note;

using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;

public enum NoteColorKind : byte
{
    Violet = 0,
    Clay = 1,
    SlateBlue = 2
}

public readonly record struct NoteColorPalette(
    Color FillColor,
    Color FieldTriplet,
    Color TextColor,
    string TokenName)
{
    public static readonly NoteColorPalette Violet = new(
        Color.Parse("#6E62A6"), Color.FromRgb(110, 98, 166), Color.Parse("#F4F4F2"), "--c-note-violet");

    public static readonly NoteColorPalette Clay = new(
        Color.Parse("#B0524E"), Color.FromRgb(176, 82, 78), Color.Parse("#F4F4F2"), "--c-note-clay");

    public static readonly NoteColorPalette SlateBlue = new(
        Color.Parse("#4E6E9C"), Color.FromRgb(78, 110, 156), Color.Parse("#F4F4F2"), "--c-note-slate-blue");

    public static NoteColorPalette FromKind(NoteColorKind kind) => kind switch
    {
        NoteColorKind.Violet => Violet,
        NoteColorKind.Clay => Clay,
        NoteColorKind.SlateBlue => SlateBlue,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
    };
}

public sealed record NotePlacementRecord
{
    public required Guid Id { get; init; }
    public required int GridX { get; init; }
    public required int GridY { get; init; }
    public required int UserExtentN { get; init; }
    public required string TextContent { get; init; }
    public required NoteColorKind ColorKind { get; init; }
    public required bool IsAnchored { get; init; }
    public required int LayerId { get; init; }
}

public static class NoteGeometrySolver
{
    public const double CellPitchPx = 220.0;
    public const double HorizontalPaddingPx = 15.0;
    public const double VerticalPaddingPx = 16.0;
    public const double FontSizePx = 15.0; // --t-body
    public const double LineHeightMultiplier = 1.42; // --lh-snug

    private static readonly Typeface InterTypeface = new("Inter", FontStyle.Normal, FontWeight.Medium);

    public static int CalculateMinSquareExtent(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 1;

        int n = 1;
        while (n <= 16)
        {
            double boxWidth = (n * CellPitchPx) - (2.0 * HorizontalPaddingPx);
            double boxHeight = (n * CellPitchPx) - (2.0 * VerticalPaddingPx);

            double formattedHeight = MeasureTextHeight(text, boxWidth);
            if (formattedHeight <= boxHeight)
            {
                return n;
            }
            n++;
        }
        return n;
    }

    public static double MeasureTextHeight(string text, double targetWidth)
    {
        var formattedText = new FormattedText(
            text,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            InterTypeface,
            FontSizePx,
            Brushes.White)
        {
            MaxTextWidth = Math.Max(10.0, targetWidth),
            LineHeight = FontSizePx * LineHeightMultiplier
        };

        return formattedText.Height;
    }
}
```

---

## 5. Avalonia 11.2.5 Custom Drawing Rendering Protocol

```csharp
namespace Grove.UI.Controls;

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Grove.Core.Content.Note;

public sealed class NotePlacementControl : Control
{
    public static readonly StyledProperty<NotePlacementRecord?> PlacementProperty =
        AvaloniaProperty.Register<NotePlacementControl, NotePlacementRecord?>(nameof(Placement));

    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.Register<NotePlacementControl, bool>(nameof(IsSelected));

    public NotePlacementRecord? Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    static NotePlacementControl()
    {
        AffectsRender<NotePlacementControl>(PlacementProperty, IsSelectedProperty);
    }

    public override void Render(DrawingContext context)
    {
        if (Placement is null) return;

        int solvedN = NoteGeometrySolver.CalculateMinSquareExtent(Placement.TextContent);
        int activeN = Math.Max(solvedN, Placement.UserExtentN);
        double sizePx = activeN * NoteGeometrySolver.CellPitchPx;

        Rect bounds = new(0, 0, sizePx, sizePx);
        var palette = NoteColorPalette.FromKind(Placement.ColorKind);

        // 1. Render Authored Fill
        context.FillRectangle(new SolidColorBrush(palette.FillColor), bounds);

        // 2. Render 1px Inset Containment Edge (--edge-on-color)
        var edgePen = new Pen(new SolidColorBrush(Color.FromArgb(31, 255, 255, 255)), 1.0);
        context.DrawRectangle(null, edgePen, bounds.Deflate(0.5));

        // 3. Render Authored Text Block
        if (!string.IsNullOrEmpty(Placement.TextContent))
        {
            var textBrush = new SolidColorBrush(palette.TextColor);
            var formattedText = new FormattedText(
                Placement.TextContent,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Inter", FontStyle.Normal, FontWeight.Medium),
                NoteGeometrySolver.FontSizePx,
                textBrush)
            {
                MaxTextWidth = sizePx - (2.0 * NoteGeometrySolver.HorizontalPaddingPx),
                MaxTextHeight = sizePx - (2.0 * NoteGeometrySolver.VerticalPaddingPx),
                LineHeight = NoteGeometrySolver.FontSizePx * NoteGeometrySolver.LineHeightMultiplier
            };

            context.DrawText(formattedText, new Point(NoteGeometrySolver.HorizontalPaddingPx, NoteGeometrySolver.VerticalPaddingPx));
        }

        // 4. Render Selection Outline if Selected (Offset 3px, 2px Stroke #96B6F8)
        if (IsSelected)
        {
            var selectPen = new Pen(new SolidColorBrush(Color.Parse("#96B6F8")), 2.0);
            Rect selectRect = bounds.Inflate(3.0);
            context.DrawRectangle(null, selectPen, selectRect);
        }

        // 5. Render Notched Anchor Ribbon if Anchored
        if (Placement.IsAnchored)
        {
            RenderAnchorRibbon(context);
        }
    }

    private static void RenderAnchorRibbon(DrawingContext context)
    {
        var ribbonBrush = new SolidColorBrush(Color.Parse("#9E8CEA"));
        double left = 16.0; // --sp-md
        double top = -5.0;
        double width = 10.0;
        double totalHeight = 22.0; // 5px above + 17px down

        var streamGeometry = new StreamGeometry();
        using (var geometryContext = streamGeometry.Open())
        {
            geometryContext.BeginFigure(new Point(left, top), true);
            geometryContext.LineTo(new Point(left + width, top));
            geometryContext.LineTo(new Point(left + width, top + totalHeight));
            geometryContext.LineTo(new Point(left + (width / 2.0), top + totalHeight - 4.0)); // Inverted V-notch
            geometryContext.LineTo(new Point(left, top + totalHeight));
            geometryContext.EndFigure(true);
        }

        context.DrawGeometry(ribbonBrush, null, streamGeometry);
    }
}

---

## 6. Hover Affordances, Distance Tiers & Explicit Refusals

### 6.1 Response Chrome on Approach
- **Hover State**: Hovering near a Note causes the 18x18px bottom-right resize corner handle and top-right `g-edit` affordance (8px mono uppercase `EDIT`, padding 4px 6px, border `1px solid rgba(255,255,255,0.42)`, background `rgba(20,20,24,0.38)`) to fade in. At rest, the surface is untouched.
- **Selection State**: Outline `2px solid rgb(150 182 248 / 0.9)` (`#96B6F8`) with soft glow `0 0 24px 6px rgb(150 182 248 / 0.45)`. Surrounding cells answer with perimeter inset `inset 0 0 0 1.5px rgb(150 182 248 / 0.30)`.
- **Anchor State**: Ribbon positioned at `top: -5px`, `left: 16px`, `width: 12px`, `height: 22px`, fill `#9E8CEA`, clip polygon `polygon(0 0, 100% 0, 100% 100%, 50% 72%, 0 100%)`. Aura takes anchor indigo hue `#9E8CEA`.

### 6.2 Distance Zoom Representation Tiers
- **Working (100%)**: Cell pitch 220px, minor pitch 44px. Full form, true type, response chrome available, presence field cast into surrounding cells.
- **Stepped Back (24%)**: Cell pitch 52.8px, minor pitch 10.6px at 57% opacity. Hover chrome and inset ring shed. True text remains legible and scaled without summary substitution.
- **Far (6%)**: Cell pitch 13.2px, minor pitch gone. Detail collapses to a kind-coded stand-in on the exact $1 \times 1$ footprint using authored color fill `#6E62A6`, `#B0524E`, or `#4E6E9C` with hard-edged cell presence.

### 6.3 Explicit Architectural Refusals
- **Refused Title Bar + Truncation**: Headers restating text and ellipsis clipping withholding text are strictly forbidden. Nothing stands between one glance and the whole thought.
- **Refused Persistent Chrome**: Button bars (edit, color, delete) parked permanently on the Note are forbidden. Affordances answer approach and leave with pointer.
- **Refused Interior Scroll**: Scrollbars inside a Note are forbidden. When text outgrows the footprint, the footprint steps up to $2 \times 2$ or $3 \times 3$ whole cells.
```
