---
status: "Accepted"
---

# ADR-031: Slate Window System and Anatomy Specifications

| Property | Value |
| :--- | :--- |
| **Status** | Accepted |
| **Date** | 2026-08-12 |
| **Area** | HUD System / Slate Window Architecture / Component Specifications |
| **Target Runtime** | C# 13 / .NET 9 / Avalonia 11.2.5 |
| **Authors** | Chief Product Definition Architect |

---

## 1. Executive Context & Architectural Principles

As specified in `docs/design-system/20-planes/HUD-plane.md` and `docs/design-system/10-grammar/Surface-classes.md`, the **Writing Slate**, **Memory Slate**, and **Gallery Slate** are the composed sub-applications on the HUD Plane. They provide generous viewport space for reading long documents, browsing every Memory record, and browsing media collections. The Layer Manager is a separate overlay.

### Architectural Rules
1. **Viewport Fixed Mechanics**: Slates are anchored strictly to viewport coordinates and are completely immune to camera pan, zoom, or rotation. A Slate is composed as Full, Left, or Right; it is never free-floating, draggable, or hover-anchored.
2. **Opaque Visual Depth & Zero Scrim**: Named Slates use an opaque dark surface fill (`#161618`, `--surface-chrome`) surrounded by a 1px quiet containment border (`#2D2D2A`, `--edge-control`). Named Slates NEVER dim, blur, or block rendering of Plane 0 (Spatial Grid) or Plane 1 (Information Plane) outside their physical footprint.
3. **Strict Hierarchy Boundary**: Maximum of 3 dark tonal steps in the UI hierarchy:
   - Base Canvas (Plane 0): `#0E0E10` (`--surface-grid`)
   - Named-Slate Chrome (Plane 2): `#161618` (`--surface-chrome`)
   - Named-Slate Nested Pane (Plane 2): `#101012` (`--surface-nested`)
   - *Refusal*: No 4th dark step is permitted under any condition.
4. **Zero-Modal Pass-Through Input**: Unbound pointer clicks outside active named-Slate bounds pass directly through to Plane 1 and Plane 0 without dismissing or blocking user interactions on the spatial grid.
5. **No Truncation / No Ellipsis**: Authoring content inside Slates never crops, truncates, or line-clamps authored text or media frames to fit container boundaries. Masonry columns reflow and scroll vertically.

---

## 2. Named-Slate Surface Design Tokens & Visual Anatomy

```text
+-----------------------------------------------------------------------------------+
| NAMED-SLATE HEADER: [IDENTITY: MEMORIES / GALLERY / WRITING] [CLOSE (Esc)]      |
| (1px Border: #2D2D2A | Chrome Fill: #161618 | Identity Ink: #E6E6E6)              |
+-----------------------------------------------------------------------------------+
| SLATE CONTROLS / FILTERS / SEARCH ROW                                             |
| [Search Input Field: #101012 Fill | 1px #2D2D2A Border]                           |
| [Filter: ALL (On)] [Filter: NOTES] [Filter: DOCUMENTS] [Filter: IMAGES]           |
+-----------------------------------------------------------------------------------+
| MASONRY / CONTENT BODY (Opaque Pane: #161618 or #101012)                          |
|                                                                                   |
|  +--------------------+  +--------------------+  +--------------------+           |
|  | Card A (Note)      |  | Card B (Picture)   |  | Card C (Document)  |           |
|  | Fill: #101012      |  | Intrinsic Aspect   |  | Paper Fill: White  |           |
|  | Border: 1px #2D2D2A|  | 1px #2D2D2A Edge   |  | Ink: #0E0E10       |           |
|  +--------------------+  +--------------------+  +--------------------+           |
|  Identity: Note Text     Identity: Name · JPG    Identity: Doc Title              |
|                                                                                   |
+-----------------------------------------------------------------------------------+
```

### 2.1 Design Token Reference Matrix

| Element Part | Design Token | Hex / Value | Typography / Rendering Rule |
| :--- | :--- | :--- | :--- |
| **Slate Surface Chrome** | `--surface-chrome` | `#161618` | Opaque background fill; edge-to-edge screen contact. |
| **Nested Pane Surface** | `--surface-nested` | `#101012` | Opaque background fill for inner cards and input fields. |
| **Slate Containment Edge** | `--edge-control` / `--k-slate-b` | `#2D2D2A` | 1px solid containment border surrounding Slate boundary. |
| **Slate Header Identity** | `--k-slate` | `#E6E6E6` | `--f-display` at `--t-title-small`, uppercase, `--tr-label`. |
| **Header Action Button** | `--surface-control` | `#1E1E22` | `--f-ui` caption 400, 1px `--edge-control`, `--r-sm`, `--sp-xs` padding. |
| **Selection Outline** | `--signal-interaction` | `#5B86E5` | `2px` solid stroke, offset `3px` outside card bounding box. |
| **Focus Ring** | `--focus-ring` | `#7CA1F7` | `2px` solid stroke, offset `6px` outside card (1px gap clear of selection outline). |
| **Anchor Diamond** | `--signal-authored-context` | `#4E9C8F` | `9x9px` square rotated 45°, offset `-4px` top-left of frame. |
| **Masonry Column Min** | `--wall-column-min` | `220px` (`~28ch`) | Minimum column width before dropping column count $n$. |

---

## 3. Composed Slate Anatomies

### 3.1 Writing Slate Specification
The **Writing Slate** is the dedicated HUD viewing and authoring host for Documents.

- **Header Identity**: Displays `Writing` or document title in uppercase `--f-display` ink `#E6E6E6`.
- **Docking Configuration**: Flexible host docking (Full Viewport, Left Pane, Right Pane). Default width $60\%$ of viewport width.
- **Reading Measure**: Document body text is constrained to `--measure-reading` ($60 - 75\text{ ch}$) centered within the pane padding box.
- **Pagination & Scrolling**: Paginated reading flow over vertical scroll box. Text never line-clamps or truncates.
- **Paper Canvas Rendering**: Document cards and reader surfaces use `--surface-page` background fill with `#0E0E10` paper ink, preserving authentic reading contrast (15.97:1 contrast ratio).

### 3.2 Memory Slate Specification
The **Memory Slate** provides universal archive access for every Memory record,
including records with zero Content instances. It is not a gallery of Content
representatives.

- **Header Identity**: Displays `Memories` (uppercase, `--t-title-small`, ink `#E6E6E6`).
- **Search & Filter Controls**:
  - Full-width search field: Fill `--surface-nested` (`#101012`), 1px border `#2D2D2A`, `--r-sm`, placeholder `Find a Memory`.
  - Filter row: `All` (default resting ON with 1px underline), `Notes`, `Documents`, `Images`.
- **Masonry Layout Algorithm**: Executed dynamically based on content width $W$:

$$\text{Column Count } n = \max\left(1, \left\lfloor \frac{W + g}{220 + g} \right\rfloor\right) \quad \text{where } g = 12\text{px } (\text{--sp-sm})$$

$$\text{Column Width} = \frac{W - (n - 1) \cdot g}{n}$$

- **Cards & Provenance**: Each card represents one Memory record. Note cards (`--surface-nested`), Picture cards (intrinsic aspect ratio), and Document cards (`--surface-page`) render the record payload. A Memory referenced by many Content instances still produces one card; the card reports Content/Anchor count and provenance. A record with no Content remains visible. Double-clicking a card opens its complete record view.

### 3.3 Gallery Slate Specification
The **Gallery Slate** displays every picture imported into Grove in intrinsic proportions.

- **Header Identity**: Displays `Gallery` (uppercase, `--t-title-small`, ink `#E6E6E6`).
- **Facet Row**: `All`, `GIFs`, `Placed`. Header right edge displays total image count (e.g., `128 IMAGES`).
- **Masonry Wall**: Columns calculated using `--wall-column-min` ($220\text{px}$). Cards assigned to column with smallest accumulated height $H_{\text{col}}$, breaking ties to the leftmost column.
- **Identity Line**: Placed `--sp-sm` beneath each frame: `Filename · Format · Date` (e.g., `Studio window · JPG · Jun 02`).
- **Frame Selection & Focus**: Selected picture takes `2px` `--signal-interaction` outline offset `3px` clear of frame. Focused picture takes `--focus-ring` offset `6px`.

---

## 4. Tabbed Docking & Window Management Mechanics

Slates are hosted within a unified Slate host that supports Full, Left, and Right
viewport compositions. A Slate never hovers over the viewport as a movable
window. The Layer Manager is hosted by the HUD overlay container, not by the
Slate host.

```
       [FULL VIEWPORT DOCK]                [SPLIT LEFT / RIGHT DOCK]
+--------------------------------+  +----------------+----------------+
| Named-Slate Header   [Close]   |  | Left Named Slate | Right Named Slate |
+--------------------------------+  | (Width: 50%)   | (Width: 50%)   |
|                                |  |                |                |
| Generous Content Pane          |  | Masonry Pane   | Document Reader|
|                                |  |                |                |
+--------------------------------+  +----------------+----------------+
```

### 4.1 Docking Rules
- **Full Dock**: Fills the complete viewport extent of the HUD Plane.
- **Left / Right Split Dock**: Occupies exactly one half of the viewport while the peer half remains a live HUD composition.
- **Escape Dismissal Sequence**: Pressing `Escape` peels exactly one layer in strict sequence:
  1. Dismisses active contextual menu or rename input field.
  2. Clears active search field filter text.
  3. Returns from record view to gallery view.
  4. Dismisses the topmost active named Slate and returns keyboard focus to the triggering element on Plane 0 / Plane 1.

---

## 5. C# 13 / Avalonia 11.2.5 Implementation Contracts

```csharp
namespace Grove.HUD.Slates;

using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Layout;

public enum SlateDockMode
{
    FullViewport,
    LeftPane,
    RightPane
}

public interface ISlateWindow
{
    string IdentityTitle { get; }
    SlateDockMode DockMode { get; set; }
    bool HandleEscapeKey();
    void OnSlateOpened();
    void OnSlateClosed();
}

/// <summary>
/// Avalonia ContentControl providing viewport-fixed dark chrome (#161618) and 1px border (#2D2D2A).
/// </summary>
public class SlateFrameControl : ContentControl
{
    public static readonly StyledProperty<string> IdentityTitleProperty =
        AvaloniaProperty.Register<SlateFrameControl, string>(nameof(IdentityTitle), "SLATE");

    public static readonly StyledProperty<SlateDockMode> DockModeProperty =
        AvaloniaProperty.Register<SlateFrameControl, SlateDockMode>(nameof(DockMode), SlateDockMode.FullViewport);

    public SlateFrameControl()
    {
        Background = SolidColorBrush.Parse("#161618");
        BorderBrush = SolidColorBrush.Parse("#2D2D2A");
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(4);
        Padding = new Thickness(24); // --sp-lg
        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment = VerticalAlignment.Stretch;
    }

    public string IdentityTitle
    {
        get => GetValue(IdentityTitleProperty);
        set => SetValue(IdentityTitleProperty, value);
    }

    public SlateDockMode DockMode
    {
        get => GetValue(DockModeProperty);
        set => SetValue(DockModeProperty, value);
    }
}

/// <summary>
/// Executable masonry layout panel for Memory and Gallery Slates in Avalonia 11.2.5.
/// </summary>
public sealed class MasonryGalleryPanel : Panel
{
    public static readonly StyledProperty<double> ColumnMinWidthProperty =
        AvaloniaProperty.Register<MasonryGalleryPanel, double>(nameof(ColumnMinWidth), 220.0);

    public static readonly StyledProperty<double> GutterSpacingProperty =
        AvaloniaProperty.Register<MasonryGalleryPanel, double>(nameof(GutterSpacing), 12.0);

    public double ColumnMinWidth
    {
        get => GetValue(ColumnMinWidthProperty);
        set => SetValue(ColumnMinWidthProperty, value);
    }

    public double GutterSpacing
    {
        get => GetValue(GutterSpacingProperty);
        set => SetValue(GutterSpacingProperty, value);
    }

    protected override Size MeasureOverride(Size constraint)
    {
        double availableWidth = constraint.Width;
        if (double.IsInfinity(availableWidth) || availableWidth <= 0)
            availableWidth = 800;

        double g = GutterSpacing;
        int numColumns = Math.Max(1, (int)Math.Floor((availableWidth + g) / (ColumnMinWidth + g)));
        double colWidth = (availableWidth - (numColumns - 1) * g) / numColumns;

        double[] colHeights = new double[numColumns];

        foreach (var child in Children)
        {
            child.Measure(new Size(colWidth, double.PositiveInfinity));
            int targetCol = GetShortestColumnIndex(colHeights);
            colHeights[targetCol] += child.DesiredSize.Height + g;
        }

        double maxHeight = 0;
        for (int i = 0; i < numColumns; i++)
        {
            if (colHeights[i] > maxHeight) maxHeight = colHeights[i];
        }

        return new Size(availableWidth, Math.Max(0, maxHeight - g));
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double availableWidth = finalSize.Width;
        double g = GutterSpacing;
        int numColumns = Math.Max(1, (int)Math.Floor((availableWidth + g) / (ColumnMinWidth + g)));
        double colWidth = (availableWidth - (numColumns - 1) * g) / numColumns;

        double[] colHeights = new double[numColumns];

        foreach (var child in Children)
        {
            int targetCol = GetShortestColumnIndex(colHeights);
            double x = targetCol * (colWidth + g);
            double y = colHeights[targetCol];

            child.Arrange(new Rect(x, y, colWidth, child.DesiredSize.Height));
            colHeights[targetCol] += child.DesiredSize.Height + g;
        }

        return finalSize;
    }

    private static int GetShortestColumnIndex(double[] heights)
    {
        int shortest = 0;
        double minH = heights[0];
        for (int i = 1; i < heights.Length; i++)
        {
            if (heights[i] < minH)
            {
                minH = heights[i];
                shortest = i;
            }
        }
        return shortest;
    }
}
```
