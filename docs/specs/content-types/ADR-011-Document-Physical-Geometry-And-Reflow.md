---
status: "PARTIAL — verified document rendering and editor seam"
---

# ADR-011: Document Physical Geometry, Page Texture, AST, and Multi-Column Reflow Engine

- **Status**: Normative
- **Date**: 2026-08-12
- **Architectural Scope**: Spatial Content Primitives / Document Component
- **Target Runtime**: .NET 9.0 / Avalonia UI 11.2.5 / SkiaSharp 3.0

---

## 1. Context & Architectural Principles

A **Document** represents a structured long-form written work placed on the spatial Grid. It renders as paper (`--surface-page` `#F5F5F5` carrying `--paper-ink` `#1A1A1A`). Unlike infinite canvas text nodes or dynamic web reflow windows, a Grove Document enforces physical paper proportions, whole-cell integral footprints, deterministic page texture rules, AST-driven multi-column text pagination, and zero synthetic chrome generation.

### 1.1 Invariant Design Laws
1. **Integral Footprint Range**: Footprints are whole-cell rectangles bounded between a $2 \times 2$ cell minimum ($440\text{px} \times 440\text{px}$) and an $8 \times 8$ cell maximum ($1760\text{px} \times 1760\text{px}$). Width $W \ge 2$, Height $H \ge 2$. Single-cell ($1 \times 1$) footprints are forbidden because a $220\text{px}$ cell minus padding leaves only $164\text{px}$ of interior measure, failing reading measure constraints.
2. **Page Texture Pitch**: Horizontal and vertical texture rules are drawn in page space beneath all content zones: minor rules at $44\text{px}$ pitch (`--paper-texture-minor` alpha $0.05$), major rules at $220\text{px}$ pitch (`--paper-texture-major` alpha $0.08$).
3. **No Synthetic Chrome**: No masthead, title bar, issue line, or generated headline is synthesized. Authored titles are rendered only when present in the source AST; if absent, the filename is used as the accessible identifier without inserting visual placeholder text.
4. **Pagination Overflow Rule**: Page creation occurs solely when line units exhaust column capacity. Reflow never clips, ellipsizes, or introduces interior scrollbars.

---

## 2. Physical Geometry & Page Texture Specification

### 2.1 Footprint & Padding Equations

Let $P_{\text{cell}} = 220\text{px}$ be the canonical cell pitch.
For a Document footprint of $W \times H$ cells ($2 \le W \le 8, 2 \le H \le 8$):

- **Outer Dimensions**:
  $$\text{Width}_{\text{outer}}(W) = 220W \quad (\text{px})$$
  $$\text{Height}_{\text{outer}}(H) = 220H \quad (\text{px})$$

- **Page Padding**:
  - Horizontal Padding: $28\text{px}$ left, $28\text{px}$ right ($\Delta W_{\text{pad}} = 56\text{px}$)
  - Vertical Padding: $26\text{px}$ top, $26\text{px}$ bottom ($\Delta H_{\text{pad}} = 52\text{px}$)

- **Usable Interior Live Area**:
  $$W_{\text{live}}(W) = 220W - 56 \quad (\text{px})$$
  $$H_{\text{live}}(H) = 220H - 52 \quad (\text{px})$$

| Footprint ($W \times H$) | Outer Size ($\text{px}$) | Live Area ($W_{\text{live}} \times H_{\text{live}}$) | Column Capacity ($N_{\text{cols}}$) |
| :---: | :---: | :---: | :---: |
| **$2 \times 2$ (Min)** | $440 \times 440$ | $384 \times 388$ | 1 Column |
| **$3 \times 3$** | $660 \times 660$ | $604 \times 608$ | 2 Columns |
| **$4 \times 4$** | $880 \times 880$ | $824 \times 828$ | 2-3 Columns |
| **$8 \times 8$ (Max)** | $1760 \times 1760$ | $1704 \times 1708$ | 4-6 Columns |

### 2.2 Page Texture Pitch Geometry

The paper texture consists of 1px rules drawn in local page coordinates:
1. **Minor Texture Rules**:
   - Interval: $44\text{px}$ along X and Y axes ($x = 44k, y = 44m$).
   - Color: `--paper-texture-minor` (rgba(`26, 26, 26, 0.05`)).
2. **Major Texture Rules**:
   - Interval: $220\text{px}$ along X and Y axes ($x = 220k, y = 220m$).
   - Color: `--paper-texture-major` (rgba(`26, 26, 26, 0.08`)).

---

## 3. Rich Text Formatting AST Architecture

```
                  +-------------------+
                  |  RichTextDocument |
                  +---------+---------+
                            |
           +----------------+----------------+
           |                                 |
  +--------+--------+               +--------+--------+
  |  FrontMatterNode |               |   BlockNode    |
  +-----------------+               +--------+--------+
                                             |
            +----------------+---------------+----------------+
            |                |               |                |
   +--------+-------+ +------+------+ +------+------+ +-------+-------+
   | HeadingBlock   | |ParagraphBlock| |  ListBlock   | | CodeBlockNode |
   +----------------+ +------+------+ +-------------+ +---------------+
                             |
                      +------+------+
                      | InlineNode  |
                      +------+------+
                             |
             +---------------+---------------+
             |                               |
    +--------+-------+              +--------+-------+
    | TextRunInline  |              | FormattedInline|
    +----------------+              +----------------+
```

---

## 4. Multi-Column Reflow & Pagination Mathematics

### 4.1 Normative Constants & Demand Equations

| Constant | Value | Description |
| :--- | ---: | :--- |
| `MIN_COLUMN_WIDTH_PX` | `160` | Hard lower bound for column width. |
| `COLUMN_GAP_PX` | `18` | Inter-column gutter width. |
| `BODY_FONT_PX` | `16` | Reference body type font size. |
| `BODY_LINE_HEIGHT` | `1.55` | Line height multiplier ($24.8\text{px}$ per line). |
| `CHAR_WIDTH_FACTOR` | `0.52` | Glyph width factor ($8.32\text{px}$ per character at $16\text{px}$). |
| `TEXT_MIN_MEASURE_CH` | `34` | Minimum text reading measure in characters. |
| `TEXT_MIN_WIDTH_PX` | `283` | $\lceil 34 \times 0.52 \times 16 \rceil = 283\text{px}$. |
| `REFERENCE_LINES_PER_PAGE` | `18` | Baseline capacity per column page unit. |

### 4.2 Column Count Calculation

For a live interior width $W_{\text{live}}$:

$$N_{\text{cols}} = \max\left(1, \left\lfloor \frac{W_{\text{live}} + \text{COLUMN\_GAP\_PX}}{\text{TEXT\_MIN\_WIDTH\_PX} + \text{COLUMN\_GAP\_PX}} \right\rfloor\right)$$

$$\text{ColumnWidth}(N_{\text{cols}}) = \frac{W_{\text{live}} - (N_{\text{cols}} - 1) \cdot \text{COLUMN\_GAP\_PX}}{N_{\text{cols}}}$$

Validation: If $\text{ColumnWidth} < \text{TEXT\_MIN\_WIDTH\_PX}$ ($283\text{px}$), $N_{\text{cols}}$ is decremented to preserve readability floors.

### 4.3 Text Line Capacity & Pagination Solver

Let $H_{\text{live}}$ be the live height.
Line height $h_{\text{line}} = \text{BODY\_FONT\_PX} \cdot \text{BODY\_LINE\_HEIGHT} = 16 \times 1.55 = 24.8\text{px}$.
Lines per column $L_{\text{col}} = \left\lfloor \frac{H_{\text{live}}}{24.8} \right\rfloor$.

$$\text{TotalLineUnits} = \sum_{\text{block} \in \text{AST}} \left( \left\lceil \frac{\text{CharCount}(\text{block})}{\lfloor \text{ColumnWidth} / 8.32 \rfloor} \right\rceil + \text{SpacingCost}(\text{block}) \right)$$

$$\text{RequiredColumns} = \left\lceil \frac{\text{TotalLineUnits}}{L_{\text{col}}} \right\rceil$$

$$\text{RequiredPages} = \left\lceil \frac{\text{RequiredColumns}}{N_{\text{cols}}} \right\rceil$$

---

## 5. C# Data Schemas & Multi-Column Reflow Engine

```csharp
namespace Grove.Core.Content.Document;

using System;
using System.Collections.Generic;

public abstract record AstNode;

public sealed record RichTextDocument : AstNode
{
    public string? Title { get; init; }
    public string? FrontMatter { get; init; }
    public string? Abstract { get; init; }
    public string? ClosingLine { get; init; }
    public IReadOnlyList<string> TitleBlockCells { get; init; } = Array.Empty<string>();
    public IReadOnlyList<BlockNode> Blocks { get; init; } = Array.Empty<BlockNode>();
}

public abstract record BlockNode : AstNode;

public sealed record HeadingBlock(int Level, string Text) : BlockNode;

public sealed record ParagraphBlock(IReadOnlyList<InlineNode> Inlines) : BlockNode;

public sealed record CodeBlockNode(string Code, string Language) : BlockNode;

public abstract record InlineNode : AstNode;

public sealed record TextRunInline(string Text) : InlineNode;

public sealed record FormattedInline(string Text, bool IsBold, bool IsItalic, bool IsCode) : InlineNode;

public readonly record struct ColumnSlice(
    int ColumnIndex,
    double X,
    double Y,
    double Width,
    double Height,
    int StartBlockIndex,
    int EndBlockIndex);

public readonly record struct PageLayoutResult(
    int PageIndex,
    int TotalPages,
    int ColumnsPerPage,
    double ColumnWidth,
    IReadOnlyList<ColumnSlice> Slices);

public static class DocumentReflowEngine
{
    public const double CellPitch = 220.0;
    public const double MarginX = 28.0;
    public const double MarginY = 26.0;
    public const double ColumnGap = 18.0;
    public const double MinColumnWidth = 283.0; // TEXT_MIN_WIDTH_PX
    public const double BodyFontSize = 16.0;
    public const double LineHeightPx = 24.8;

    public static PageLayoutResult Reflow(RichTextDocument doc, int cellWidth, int cellHeight, int pageIndex = 0)
    {
        cellWidth = Math.Clamp(cellWidth, 2, 8);
        cellHeight = Math.Clamp(cellHeight, 2, 8);

        double outerW = cellWidth * CellPitch;
        double outerH = cellHeight * CellPitch;
        double liveW = outerW - (2.0 * MarginX);
        double liveH = outerH - (2.0 * MarginY);

        int colsPerPage = Math.Max(1, (int)Math.Floor((liveW + ColumnGap) / (MinColumnWidth + ColumnGap)));
        double colWidth = (liveW - ((colsPerPage - 1) * ColumnGap)) / colsPerPage;
        int linesPerCol = Math.Max(1, (int)Math.Floor(liveH / LineHeightPx));

        List<ColumnSlice> slices = new();
        int totalBlocks = doc.Blocks.Count;
        int currentBlock = 0;
        int colIdx = 0;

        while (currentBlock < totalBlocks)
        {
            double colX = MarginX + (colIdx % colsPerPage) * (colWidth + ColumnGap);
            int startBlock = currentBlock;
            int linesUsed = 0;

            while (currentBlock < totalBlocks && linesUsed < linesPerCol)
            {
                linesUsed += 2; // Estimate line cost per block
                currentBlock++;
            }

            slices.Add(new ColumnSlice(colIdx, colX, MarginY, colWidth, liveH, startBlock, currentBlock - 1));
            colIdx++;
        }

        int totalPages = Math.Max(1, (int)Math.Ceiling((double)slices.Count / colsPerPage));
        int sliceStart = pageIndex * colsPerPage;
        int sliceCount = Math.Min(colsPerPage, slices.Count - sliceStart);

        List<ColumnSlice> pageSlices = sliceStart < slices.Count
            ? slices.GetRange(sliceStart, Math.Max(0, sliceCount))
            : new List<ColumnSlice>();

        return new PageLayoutResult(pageIndex, totalPages, colsPerPage, colWidth, pageSlices);
    }
}
```

---

## 6. Avalonia 11.2.5 Document Page Drawing Protocol

```csharp
namespace Grove.UI.Controls;

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Grove.Core.Content.Document;

public sealed class DocumentPlacementControl : Control
{
    public static readonly StyledProperty<RichTextDocument?> DocumentProperty =
        AvaloniaProperty.Register<DocumentPlacementControl, RichTextDocument?>(nameof(Document));

    public static readonly StyledProperty<int> CellWidthProperty =
        AvaloniaProperty.Register<DocumentPlacementControl, int>(nameof(CellWidth), 2);

    public static readonly StyledProperty<int> CellHeightProperty =
        AvaloniaProperty.Register<DocumentPlacementControl, int>(nameof(CellHeight), 2);

    public RichTextDocument? Document
    {
        get => GetValue(DocumentProperty);
        set => SetValue(DocumentProperty, value);
    }

    public int CellWidth
    {
        get => GetValue(CellWidthProperty);
        set => SetValue(CellWidthProperty, value);
    }

    public int CellHeight
    {
        get => GetValue(CellHeightProperty);
        set => SetValue(CellHeightProperty, value);
    }

    static DocumentPlacementControl()
    {
        AffectsRender<DocumentPlacementControl>(DocumentProperty, CellWidthProperty, CellHeightProperty);
    }

    public override void Render(DrawingContext context)
    {
        double width = Math.Clamp(CellWidth, 2, 8) * DocumentReflowEngine.CellPitch;
        double height = Math.Clamp(CellHeight, 2, 8) * DocumentReflowEngine.CellPitch;
        Rect bounds = new(0, 0, width, height);

        // 1. Render Paper Fill (--surface-page #F5F5F5)
        context.FillRectangle(new SolidColorBrush(Color.Parse("#F5F5F5")), bounds);

        // 2. Render Page Texture Rules
        RenderPageTexture(context, width, height);

        // 3. Render Inset Edge (--paper-edge rgba(26,26,26,0.10))
        var edgePen = new Pen(new SolidColorBrush(Color.FromArgb(25, 26, 26, 26)), 1.0);
        context.DrawRectangle(null, edgePen, bounds.Deflate(0.5));

        if (Document is null) return;

        // 4. Calculate Multi-Column Reflow
        var layout = DocumentReflowEngine.Reflow(Document, CellWidth, CellHeight, 0);

        // 5. Render Columns
        var textBrush = new SolidColorBrush(Color.Parse("#1A1A1A"));
        var font = new Typeface("Inter", FontStyle.Normal, FontWeight.Regular);

        foreach (var slice in layout.Slices)
        {
            double yCursor = slice.Y;
            for (int b = slice.StartBlockIndex; b <= slice.EndBlockIndex && b < Document.Blocks.Count; b++)
            {
                var block = Document.Blocks[b];
                if (block is ParagraphBlock para)
                {
                    string text = string.Join("", para.Inlines.Select(i => i switch
                    {
                        TextRunInline t => t.Text,
                        FormattedInline f => f.Text,
                        _ => ""
                    }));

                    var fmt = new FormattedText(
                        text,
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        font,
                        16.0,
                        textBrush)
                    {
                        MaxTextWidth = slice.Width,
                        LineHeight = DocumentReflowEngine.LineHeightPx
                    };

                    context.DrawText(fmt, new Point(slice.X, yCursor));
                    yCursor += fmt.Height + 12.0;
                }
            }
        }
    }

    private static void RenderPageTexture(DrawingContext context, double width, double height)
    {
        var minorPen = new Pen(new SolidColorBrush(Color.FromArgb(13, 26, 26, 26)), 1.0);
        var majorPen = new Pen(new SolidColorBrush(Color.FromArgb(20, 26, 26, 26)), 1.0);

        for (double x = 44.0; x < width; x += 44.0)
        {
            var pen = (Math.Abs(x % 220.0) < 0.01) ? majorPen : minorPen;
            context.DrawLine(pen, new Point(x, 0), new Point(x, height));
        }

        for (double y = 44.0; y < height; y += 44.0)
        {
            var pen = (Math.Abs(y % 220.0) < 0.01) ? majorPen : minorPen;
            context.DrawLine(pen, new Point(0, y), new Point(width, y));
        }
    }
}

---

## 7. Front-Page Zone Hierarchy, Distance Hysteresis & Refusal Contracts

### 7.1 Front-Page Zone Hierarchy & Typography
The Document front page layout is structured into 5 vertical zones rendered over paper `#F5F5F5` (`--surface-page`) with inset shadow edge `inset 0 0 0 1px rgba(26,26,26,0.10)`:
1. **Front Matter (Mono 11px / 0.20em / 48% Ink)**: Metadata line set uppercase in JetBrains Mono (`rgba(26,26,26,0.48)`).
2. **Display Title (38px / 0.98 Leading / 0.03em Tracking)**: Oswald Display uppercase title with tight leading to keep title lines consolidated.
3. **Hairline Rule (1px Height / 16% Ink)**: Divider rule (`rgba(26,26,26,0.16)`), margin 18px top/bottom.
4. **Abstract (UI Font 15px / 1.65 / 62% Ink / 34ch Max-Width)**: Primary reading excerpt set at comfortable 34-character measure. Optional close section sits under a tight 56px hairline (`rgba(26,26,26,0.16)`) set in 13px / 1.60 UI font (`50% ink`).
5. **Title Block (Mono Pair in 28% Border)**: Stamped metadata box pushed to bottom-right by flexible spacer (`flex: 1`, min-height 12px).

### 7.2 Presence Field & States
- **Neutral Aura**: Documents cast a neutral `234 234 234` aura (`0.025`–`0.055` alpha) into surrounding cells.
- **Selection State**: Outline `2px solid rgb(150 182 248 / 0.9)` (`#96B6F8`) with soft glow `0 0 24px 6px rgb(150 182 248 / 0.45)`. Field answers with perimeter inset `inset 0 0 0 1.5px rgb(150 182 248 / 0.30)`.
- **Anchor State**: Indigo ribbon at `top: -5px`, `left: 20px`, `width: 12px`, `height: 22px`, fill `#9E8CEA`. Aura switches to anchor indigo hue `#9E8CEA`.

### 7.3 Distance Zoom Hysteresis & Stand-in Specs
- **Working (50%)**: Cell pitch 110px. Renders complete front page with paper texture rules.
- **Stepped Back (12%)**: Cell pitch 26.4px. Paper texture and inset edge shed; true text lines remain rendered.
- **Far (3%)**: Cell pitch 6.6px. Footprint demotes to a 96px paper sheet snapshot stand-in on exact $2 \times 2$ footprint bounds (`.si-sheet`) featuring paper fill `#F5F5F5`, 1px border `rgba(26,26,26,0.28)`, ruled header bar (`rgba(26,26,26,0.44)`), and 4 text line strokes.
- **Hysteresis Bands**: Demote to stepped back below 56px, restore to working at 72px; demote to stand-in below 18px, promote back to page at 28px.

### 7.4 Document Architectural Refusals
- **Refused Ellipsis Clip**: Truncating text with ellipses is strictly forbidden. Front pages carry complete chosen passages or fewer of them.
- **Refused Interior Scrollbar**: Interior scrollbars inside a Document placement are forbidden. Full reading occurs by opening the Writing Slate on Plane 2.
- **Refused File-Card Stand-in**: Replacing a Document with a dark filename container card is forbidden. Front page paper says what the writing is before it is opened.
```
