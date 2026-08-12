---
status: "PARTIAL — verified type-specific resize solvers"
---

# ADR-056: Interactive Resize Geometry, Type-Specific Footprint Solvers, and Affordance Rendering Engine

| Property | Value |
| :--- | :--- |
| **Status** | PARTIAL — verified type-specific resize solvers |
| **Date** | 2026-08-12 |
| **Area** | Spatial Grid Engine / Interactive Resize & Affordances Subsystem |
| **Target Runtime** | C# 13 / .NET 9 / Avalonia 11.2.5 / SkiaSharp 3.x |
| **Authors** | Chief Product Definition Architect |

---

## 1. Executive Context & Architectural Principles

In Grove v9, content elements on Plane 0 (Spatial Grid Canvas) are cell-quantized rectangular placements (`Note`, `Document`, `Picture`). Rather than allowing unconstrained floating pixel dimensions that introduce sub-cell alignment fragmentation, Grove enforces discrete cell snapping during interactive resize gestures while respecting content-type-specific geometric constraints.

### Key Architectural Drivers:
1. **Interactive Resize Affordances**: Selected items render an interactive **12px bottom-right cell corner hit target** (`SouthEast` corner handle) as the primary resize manipulator, accompanied by a **1.5px containment edge highlight** in `#96B6F8` (`--accent-edge-highlight`).
2. **Pointer Hover Cursor State**: Position-aware pointer hit testing automatically transitions the Avalonia window cursor state to `Cursor = StandardCursorType.SizeNWSE` when hovering over SE or NW corner handles, and `Cursor = StandardCursorType.SizeNESW` over NE or SW corner handles.
3. **Type-Specific Footprint Constraints**:
   - **Picture (Image)**: Aspect ratio preservation derived using principal axis cell equation $L = \lceil \text{long} / 256 \rceil$ with scale divisor $D = 256\text{px}$. Dragging re-calculates secondary axis extent $S$ to preserve intrinsic aspect proportions without cropping.
   - **Document**: Free cell pitch bounds constrained strictly between **$2 \times 2$ cells** ($440 \times 440\text{ DIPs}$) and **$8 \times 8$ cells** ($1760 \times 1760\text{ DIPs}$). Text engine dynamically recalculates line reflow upon cell boundary commits.
   - **Note**: Strict 1:1 square extent solver where width equals height ($n \times n$, $n \in [1, 8]$).
4. **Dynamic Compositor Ghost Preview & Refusal Engine**: During resize drag operations, the canvas projects a real-time cell ghost preview. If candidate footprint dimensions violate spatial boundaries or collide with an occupied cell (`!IsRegionFree`), the ghost transitions instantly into a 12% refusal state with 45° cross-hatch overlay.

---

## 2. Spatial Resize Geometry & Affordance Hit Testing

```
  (X0, Y0) Top-Left Corner                                (X1, Y0) Top-Right Corner
  +-------------------------------------------------------+
  | Content Placement Footprint                           |
  |                                                       |
  | Edge Highlight: 1.5px Solid Inset (#96B6F8)           |
  |                                                       |
  |                                                       |
  |                                                       |
  +-------------------------------------------------------+ (X1, Y1) Bottom-Right Corner
  (X0, Y1) Bottom-Left Corner                             | 12px Hit Target
                                                          | Cursor: SizeNWSE
```

### 2.1 12px Bottom-Right Corner Hit Target Mathematics

Let $B_{\text{world}} = [x_0, y_0, x_1, y_1]$ be the world-space rectangular bounding box of a selected content placement.
The continuous world coordinates of the four cell corner vertices are:

$$V_{\text{NW}} = (x_0, y_0), \quad V_{\text{NE}} = (x_1, y_0), \quad V_{\text{SE}} = (x_1, y_1), \quad V_{\text{SW}} = (x_0, y_1)$$

Under camera transformation matrix $T_{\text{camera}}$, the screen-space corner coordinate $P_{\text{screen,SE}} = T_{\text{camera}}(V_{\text{SE}})$.
The bottom-right corner hit target box $H_{\text{SE}}$ is a square bounding box of width $d_{\text{target}} = 12.0\text{ px}$ centered at $P_{\text{screen,SE}}$:

$$H_{\text{SE}} = \left[ P_{\text{screen,SE}}.X - 6.0, \, P_{\text{screen,SE}}.Y - 6.0, \, P_{\text{screen,SE}}.X + 6.0, \, P_{\text{screen,SE}}.Y + 6.0 \right]$$

For a screen pointer coordinate $P_{\text{pointer}} = (x_{\text{ptr}}, y_{\text{ptr}})$, the SE handle hit predicate $\text{IsHit}_{\text{SE}}(P_{\text{pointer}})$ is:

$$\text{IsHit}_{\text{SE}}(P_{\text{pointer}}) = \mathbb{I}\left( |x_{\text{ptr}} - P_{\text{screen,SE}}.X| \le 6.0 \land |y_{\text{ptr}} - P_{\text{screen,SE}}.Y| \le 6.0 \right)$$

### 2.2 1.5px Containment Edge Highlight Inset Geometry

Selected content items render an inset edge highlight stroke of width $w_{\text{edge}} = 1.5\text{ px}$ using color token `#96B6F8` (RGB: 150, 182, 248, Opacity: 100%).
To prevent stroke bleeding outside grid cell boundaries, the Skia stroke path is inset by half-stroke width ($0.75\text{ px}$):

$$B_{\text{inset}} = \left[ x_{\text{screen},0} + 0.75, \, y_{\text{screen},0} + 0.75, \, x_{\text{screen},1} - 0.75, \, y_{\text{screen},1} - 0.75 \right]$$

### 2.3 Pointer Cursor State Machine

```
+------------------------------------+--------------------------------+----------------------------+
| Pointer Location                   | Hover Cursor State             | Avalonia System Cursor     |
+------------------------------------+--------------------------------+----------------------------+
| Over SE Handle (12px Box)          | Resize SE Direction            | StandardCursorType.SizeNWSE|
| Over NW Handle (12px Box)          | Resize NW Direction            | StandardCursorType.SizeNWSE|
| Over NE Handle (12px Box)          | Resize NE Direction            | StandardCursorType.SizeNESW|
| Over SW Handle (12px Box)          | Resize SW Direction            | StandardCursorType.SizeNESW|
| Content Interior / Grid Canvas     | Spatial Grid Cursor (Default)  | StandardCursorType.Arrow   |
+------------------------------------+--------------------------------+----------------------------+
```

---

## 3. Type-Specific Footprint Constraint Solvers

### 3.1 Picture (Image) Aspect Ratio Preservation Solver

Images mapped to grid cell footprints adhere to the principal axis 256px scale divisor invariant $D_{\text{cell}} = 256\text{px}$.
Given an image with intrinsic pixel resolution $W_{\text{px}} \times H_{\text{px}}$:

1. **Identify Intrinsic Principal Axes**:
   $$\text{long}_{\text{px}} = \max(W_{\text{px}}, H_{\text{px}}), \quad \text{short}_{\text{px}} = \min(W_{\text{px}}, H_{\text{px}})$$
   $$\text{Aspect Ratio } r = \frac{W_{\text{px}}}{H_{\text{px}}}$$

2. **Long Axis Cell Count Equation**:
   $$L = \max\left(1, \left\lceil \frac{\text{long}_{\text{px}}}{256} \right\rceil\right)$$

3. **Interactive Resize Aspect Solver**:
   When the user drags the SE handle to a candidate long-axis cell count $L_{\text{drag}} \in [1, 8]$, the secondary axis cell count $S_{\text{derived}}$ is computed as:

   $$S_{\text{derived}} = \text{clamp}\left( \text{round}\left( L_{\text{drag}} \cdot \frac{\text{short}_{\text{px}}}{\text{long}_{\text{px}}} \right), \, 1, \, 8 \right)$$

4. **Footprint Allocation ($N_w \times N_h$)**:

   $$N_w = \begin{cases} L_{\text{drag}} & \text{if } W_{\text{px}} \ge H_{\text{px}} \text{ (Landscape / Square)} \\ S_{\text{derived}} & \text{if } W_{\text{px}} < H_{\text{px}} \text{ (Portrait)} \end{cases}$$

   $$N_h = \begin{cases} S_{\text{derived}} & \text{if } W_{\text{px}} \ge H_{\text{px}} \text{ (Landscape / Square)} \\ L_{\text{drag}} & \text{if } W_{\text{px}} < H_{\text{px}} \text{ (Portrait)} \end{cases}$$

```
+----------------------------------------------------------------------------------------------------+
| Image Aspect Ratio Interactive Resize Discrete Step Table                                         |
+-------------------+--------------+--------------+------------------+-------------------------------+
| Source Res        | Aspect Class | Initial FP   | Min Drag (L=1)   | Max Drag (L=8)                |
+-------------------+--------------+--------------+------------------+-------------------------------+
| 1200 x 1700 px    | Portrait     | 5 x 7 cells  | 1 x 1 cells      | 6 x 8 cells                   |
| 900 x 700 px      | Landscape    | 4 x 3 cells  | 1 x 1 cells      | 8 x 6 cells                   |
| 700 x 700 px      | Square       | 3 x 3 cells  | 1 x 1 cells      | 8 x 8 cells                   |
| 2000 x 500 px     | Panorama     | 8 x 2 cells  | 1 x 1 cells      | 8 x 2 cells (Clamped L=8)     |
+-------------------+--------------+--------------+------------------+-------------------------------+
```

### 3.2 Document Free Cell Pitch Bounds ($2 \times 2$ to $8 \times 8$)

Document placements represent formatted prose bodies. Documents allow unconstrained rectangular cell pitch, strictly bounded within $[2, 8]$ cells on both axes:

$$W_{\text{cells}} \in [2, 8], \quad H_{\text{cells}} \in [2, 8]$$

Given unconstrained drag position $P_{\text{drag}} = (x_{\text{drag}}, y_{\text{drag}})$ from origin $(X_0 \cdot P_{\text{cell}}, Y_0 \cdot P_{\text{cell}})$:

$$W_{\text{raw}} = \frac{x_{\text{drag}} - X_0 \cdot P_{\text{cell}}}{P_{\text{cell}}}, \quad H_{\text{raw}} = \frac{y_{\text{drag}} - Y_0 \cdot P_{\text{cell}}}{P_{\text{cell}}}$$

Discrete cell extent solver applying half-cell rounding with bounds clamping:

$$W_{\text{doc}} = \text{clamp}\left( \left\lfloor W_{\text{raw}} + 0.5 \right\rfloor, \, 2, \, 8 \right)$$
$$H_{\text{doc}} = \text{clamp}\left( \left\lfloor H_{\text{raw}} + 0.5 \right\rfloor, \, 2, \, 8 \right)$$

*Minimum bounding constraint ($2 \times 2$ cells = $440 \times 440\text{ DIPs}$) guarantees typography layout engine maintains readable line lengths without truncating text headers.*

### 3.3 Note Square Extent Solver ($n \times n$)

Notes enforce a strict 1:1 square extent ($W_{\text{cells}} = H_{\text{cells}} = n$), where $n \in [1, 8]$.
When dragging corner handles, the target extent $n$ is resolved by taking the maximum fractional cell displacement across both axes:

$$\Delta C_{\text{max}} = \max\left( \frac{x_{\text{drag}} - X_0 \cdot P_{\text{cell}}}{P_{\text{cell}}}, \, \frac{y_{\text{drag}} - Y_0 \cdot P_{\text{cell}}}{P_{\text{cell}}} \right)$$

$$n = \text{clamp}\left( \left\lfloor \Delta C_{\text{max}} + 0.5 \right\rfloor, \, 1, \, 8 \right)$$

$$W_{\text{note}} = n, \quad H_{\text{note}} = n$$

---

## 4. C# 13 System Architecture & Interface Contracts

```csharp
namespace Grove.SpatialGrid.Resize;

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Grove.SpatialGrid.Selection;

/// <summary>
/// Corner handle location flags for interactive resize testing.
/// </summary>
public enum ResizeHandleLocation : byte
{
    None = 0,
    NorthWest = 1,
    NorthEast = 2,
    SouthEast = 3,
    SouthWest = 4
}

/// <summary>
/// Screen-space corner handle hit target specification.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public readonly record struct ResizeHandleHitTest(
    ResizeHandleLocation Location,
    double ScreenCenterX,
    double ScreenCenterY,
    double TargetSizePixels = 12.0
)
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsPointer(double pointerX, double pointerY)
    {
        double half = TargetSizePixels * 0.5;
        return pointerX >= ScreenCenterX - half &&
               pointerX <= ScreenCenterX + half &&
               pointerY >= ScreenCenterY - half &&
               pointerY <= ScreenCenterY + half;
    }
}

/// <summary>
/// Solves type-specific footprint constraints for Note, Document, and Image placements.
/// </summary>
public static class TypeSpecificFootprintSolver
{
    /// <summary>
    /// Computes candidate cell footprint bounds during interactive resize gesture.
    /// </summary>
    public static SpatialRegion SolveFootprint(
        string contentType,
        SpatialRegion initialFootprint,
        ResizeHandleLocation handle,
        int deltaCellX,
        int deltaCellY,
        int intrinsicWidthPx = 0,
        int intrinsicHeightPx = 0)
    {
        int originX = initialFootprint.X;
        int originY = initialFootprint.Y;
        int candidateW = initialFootprint.Width;
        int candidateH = initialFootprint.Height;

        switch (handle)
        {
            case ResizeHandleLocation.SouthEast:
                candidateW += deltaCellX;
                candidateH += deltaCellY;
                break;
            case ResizeHandleLocation.SouthWest:
                originX += deltaCellX;
                candidateW -= deltaCellX;
                candidateH += deltaCellY;
                break;
            case ResizeHandleLocation.NorthEast:
                candidateH -= deltaCellY;
                originY += deltaCellY;
                candidateW += deltaCellX;
                break;
            case ResizeHandleLocation.NorthWest:
                originX += deltaCellX;
                originY += deltaCellY;
                candidateW -= deltaCellX;
                candidateH -= deltaCellY;
                break;
            default:
                return initialFootprint;
        }

        return contentType.ToLowerInvariant() switch
        {
            "note" => SolveNoteSquare(originX, originY, candidateW, candidateH),
            "document" => SolveDocumentBounds(originX, originY, candidateW, candidateH),
            "picture" or "image" => SolveImageAspectRatio(originX, originY, candidateW, candidateH, intrinsicWidthPx, intrinsicHeightPx),
            _ => SolveDocumentBounds(originX, originY, candidateW, candidateH)
        };
    }

    private static SpatialRegion SolveNoteSquare(int x, int y, int rawW, int rawH)
    {
        int maxExtent = Math.Max(rawW, rawH);
        int n = Math.Clamp(maxExtent, 1, 8);
        return new SpatialRegion(x, y, n, n);
    }

    private static SpatialRegion SolveDocumentBounds(int x, int y, int rawW, int rawH)
    {
        int w = Math.Clamp(rawW, 2, 8);
        int h = Math.Clamp(rawH, 2, 8);
        return new SpatialRegion(x, y, w, h);
    }

    private static SpatialRegion SolveImageAspectRatio(
        int x, int y, int rawW, int rawH, int intrinsicWidthPx, int intrinsicHeightPx)
    {
        if (intrinsicWidthPx <= 0 || intrinsicHeightPx <= 0)
        {
            return SolveDocumentBounds(x, y, rawW, rawH);
        }

        bool isLandscape = intrinsicWidthPx >= intrinsicHeightPx;
        double longPx = Math.Max(intrinsicWidthPx, intrinsicHeightPx);
        double shortPx = Math.Min(intrinsicWidthPx, intrinsicHeightPx);

        int dragLong = isLandscape ? rawW : rawH;
        int clampedLong = Math.Clamp(dragLong, 1, 8);
        int derivedShort = Math.Clamp((int)Math.Round(clampedLong * (shortPx / longPx)), 1, 8);

        int finalW = isLandscape ? clampedLong : derivedShort;
        int finalH = isLandscape ? derivedShort : clampedLong;

        return new SpatialRegion(x, y, finalW, finalH);
    }
}

/// <summary>
/// Session controller for managing interactive resize drag lifetime.
/// </summary>
public interface IInteractiveResizeEngine
{
    bool IsResizeActive { get; }
    SpatialRegion? ActiveGhostPreview { get; }
    bool IsCurrentResizeValid { get; }

    void BeginResize(Guid placementId, ResizeHandleLocation handle, CellCoordinate startCell);
    void UpdateResize(CellCoordinate currentCell, Guid activeLayerId);
    bool CommitResize();
    void CancelResize();
}
```

---

## 5. Avalonia 11.2.5 & SkiaSharp Rendering Pipeline Integration

```
+---------------------------------------------------------------------------------+
|                       SkiaSharp Resize Render Operations                        |
|                                                                                 |
| 1. Render Containment Edge Highlight                                            |
|    - Path: Inset Rect (-0.75px)                                                 |
|    - Paint: Stroke 1.5px, Color #96B6F8                                         |
| 2. Render 12px SE Corner Hit Target Handle                                      |
|    - Fill: #FFFFFF (100% Alpha)                                                 |
|    - Ring: 1.5px #96B6F8                                                        |
| 3. Update Avalonia Cursor on Hover                                              |
|    - If (Pointer over 12px SE Box) -> Cursor = StandardCursorType.SizeNWSE      |
| 4. Render Dynamic Resize Ghost Preview                                          |
|    - Valid: 6% Fill (#222220), 1.5px #96B6F8 Edge Highlight                     |
|    - Refusal: 12% Fill (#F06543), 45° Cross-Hatch Pattern                       |
+---------------------------------------------------------------------------------+
```

### 5.1 Corner Handle Hit Target & 1.5px Containment Edge Pipeline

```csharp
public void DrawResizeAffordances(
    SKCanvas canvas,
    SpatialRegion itemFootprint,
    bool isSelected,
    float cellPitchDips,
    Matrix3x3 cameraTransform)
{
    if (!isSelected) return;

    SKRect worldRect = new(
        itemFootprint.X * cellPitchDips,
        itemFootprint.Y * cellPitchDips,
        itemFootprint.Right * cellPitchDips,
        itemFootprint.Bottom * cellPitchDips);

    SKRect screenRect = cameraTransform.MapRect(worldRect);

    // 1. Render 1.5px Containment Edge Highlight in #96B6F8
    using var edgePaint = new SKPaint
    {
        Style = SKPaintStyle.Stroke,
        Color = new SKColor(0x96, 0xB6, 0xF8, 0xFF), // #96B6F8
        StrokeWidth = 1.5f,
        IsAntialias = true
    };

    SKRect insetScreenRect = SKRect.Inflate(screenRect, -0.75f, -0.75f);
    canvas.DrawRect(insetScreenRect, edgePaint);

    // 2. Render 12px Bottom-Right SE Corner Target Handle
    SKPoint seCorner = new(screenRect.Right, screenRect.Bottom);
    SKRect handleTargetRect = new(
        seCorner.X - 6.0f,
        seCorner.Y - 6.0f,
        seCorner.X + 6.0f,
        seCorner.Y + 6.0f);

    using var handleFill = new SKPaint
    {
        Style = SKPaintStyle.Fill,
        Color = SKColors.White,
        IsAntialias = true
    };

    using var handleStroke = new SKPaint
    {
        Style = SKPaintStyle.Stroke,
        Color = new SKColor(0x96, 0xB6, 0xF8, 0xFF),
        StrokeWidth = 1.5f,
        IsAntialias = true
    };

    canvas.DrawRect(handleTargetRect, handleFill);
    canvas.DrawRect(handleTargetRect, handleStroke);
}
```

### 5.2 Dynamic Ghost Preview & Collision Refusal Render Pass

```csharp
public void DrawResizeGhostPreview(
    SKCanvas canvas,
    SpatialRegion candidateFootprint,
    bool isValidRegion,
    float cellPitchDips,
    Matrix3x3 cameraTransform)
{
    SKRect worldRect = new(
        candidateFootprint.X * cellPitchDips,
        candidateFootprint.Y * cellPitchDips,
        candidateFootprint.Right * cellPitchDips,
        candidateFootprint.Bottom * cellPitchDips);

    SKRect screenRect = cameraTransform.MapRect(worldRect);

    if (isValidRegion)
    {
        // Valid Candidate Ghost: 6% Interaction Fill + 1.5px Accent Edge Highlight
        using var validFill = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = new SKColor(0x22, 0x22, 0x20, 0x0F),
            IsAntialias = true
        };
        using var validStroke = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = new SKColor(0x96, 0xB6, 0xF8, 0xFF),
            StrokeWidth = 1.5f,
            IsAntialias = true
        };

        canvas.DrawRect(screenRect, validFill);
        canvas.DrawRect(screenRect, validStroke);
    }
    else
    {
        // Refusal Candidate Ghost: 12% Refusal Fill (#F06543) + 45° Diagonal Cross-Hatch
        using var refusalFill = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = new SKColor(0xF0, 0x65, 0x43, 0x1F),
            IsAntialias = true
        };
        using var refusalStroke = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = new SKColor(0xF0, 0x65, 0x43, 0x80),
            StrokeWidth = 1.5f,
            IsAntialias = true
        };

        canvas.DrawRect(screenRect, refusalFill);

        canvas.Save();
        canvas.ClipRect(screenRect);

        float spacing = 12.0f;
        for (float d = -screenRect.Height; d < screenRect.Width + screenRect.Height; d += spacing)
        {
            canvas.DrawLine(
                screenRect.Left + d, screenRect.Top,
                screenRect.Left + d + screenRect.Height, screenRect.Bottom,
                refusalStroke);
        }

        canvas.Restore();
    }
}

---

## 6. Interactive Resize Refusal & Point-of-Action Feedback Rules

### 6.1 Collision Refusal Cross-Hatch Pattern
When an interactive resize gesture expands a footprint into an occupied spatial cell:
1. **Visual State**: The candidate footprint preview transitions to the refusal role `#F06543` / `#E2625C` with a 12% fill (`rgba(226,98,92,0.12)`), $45^\circ$ diagonal cross-hatch stripes (`stroke: rgba(226,98,92,0.40)`), and an inset border `inset 0 0 0 1.5px rgba(226,98,92,0.45)`.
2. **Dual Representation**: Structure (diagonal hatching) and color (`#F06543`) convey refusal together so the signal remains readable without color vision.

### 6.2 Point-of-Action Refusal Strip Toolbar
Refusal feedback is rendered at the point of action beside the candidate footprint via a local strip toolbar:
- Text: "This space is occupied"
- Actions: `Cancel` (resets bounds to original size), `Place` (disabled/off state `rgba(234,234,234,0.28)`).
- **Zero Modal Invariant**: Center-screen alert dialogs and canvas dimming scrims are strictly forbidden.
```
