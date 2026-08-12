# ADR-004: Three-Plane Visual Hierarchy

| Property | Value |
| :--- | :--- |
| **Status** | Accepted |
| **Date** | 2026-08-12 |
| **Area** | Visual Architecture / Compositor & Input Pipeline |
| **Target Runtime** | C# 13 / .NET 9 / Avalonia 11.2.5 TopLevel Compositor |
| **Authors** | Chief Product Definition Architect |

---

## 1. Context & Architectural Principles

As specified in `docs/design-system/20-planes/Grid-plane.md`, `Information-plane.md`, `HUD-plane.md`, and original notes (`Grove - information layer.txt`), Grove v9 enforces a strict non-overlapping **Three-Plane Visual Hierarchy**. 

The system separates spatial storage, local operational overlays, and global application chrome into three distinct visual and interaction planes.

### Architectural Rules
1. **Zero Level-Lifting**: Plane 0 (Grid) NEVER raises its compositor z-index level to compete with Plane 1 (Information) or Plane 2 (HUD).
2. **Independent Coordinate Coupling**:
   - **Plane 0**: Fully camera-projected world coordinates.
   - **Plane 1**: Locally coordinated with world position, but camera-independent scale/scroll mechanics.
   - **Plane 2**: Fixed screen-space viewport coordinates.
3. **No Canvas Dimming**: Opening an overlay or slate on Plane 1 or Plane 2 NEVER dims, blurs, or disables rendering on Plane 0.

---

## 2. Three-Plane Specification Matrix

| Plane | Name | Plane Order | CSS / Compositor Band | Coordinate Space | Camera Relationship | Surface Ownership |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **Plane 0** | **Spatial Grid Canvas** | `0` (Lowest) | `z-index: 10` | Grid Cells projected by Camera Matrix $T(x,y,s)$ | Fully transformed by Camera | Placed Notes, Documents, Pictures, Grid Lines, Presence Aura Heatmaps, Field Perimeter Rings, Selection Marquee, Grid Cursor. |
| **Plane 1** | **Information Layer** | `1` (Middle) | `z-index: 20` | Spatial locality coordinates $(x_{\text{world}}, y_{\text{world}} \to x_{\text{screen}}, y_{\text{screen}})$ | Position-linked to camera; unscaled by camera zoom | Local Text Editors, Full-Size Image Viewers, Annotation Markers, Annotation Overlays, Quick Notes. |
| **Plane 2** | **HUD Plane** | `2` (Highest) | `z-index: 30` | Viewport-relative screen pixels | Completely independent of Camera | Slates (Memory Slate, Gallery Slate, Writing Slate), Operation Bar, Context Menus, Global Mode Controllers, Notification Strips. |

---

## 3. Compositor & Render Loop Pipeline

Rendering follows a strict bottom-to-top execution sequence during each GPU frame pass:

```text
[Frame Begin]
  ↓
1. Render Plane 0 (Spatial Grid Canvas)
   - Skia background `#0E0E10` (`--c-base`)
   - Presence / Aura Heatmaps & 1.5px Perimeter Rings
   - 3-Tier Grid Lines (Minor, Major, Supercell)
   - Placed Content Footprints & Renderables
   - Grid Cursor & Selection Marquee
  ↓
2. Render Plane 1 (Information Layer)
   - Evaluate active local editors & annotation markers
   - Project spatial origin to screen position
   - Render overlay controls with fixed screen scale
  ↓
3. Render Plane 2 (HUD Plane)
   - Render Slates, Operation Bar, and Menus
   - Draw focus rings (`--focus-ring`)
[Frame Present / Swap Buffers]
```

### 3.1 Transparency & Compositor Blending Rules
- **Plane 0**: Opaque base canvas fill (`#0E0E10`).
- **Plane 1**: Fully transparent root layer. Individual local editor surfaces are opaque over their own footprint only (`--surface-chrome` `#161618`) with `--shadow-local` (`0 8px 24px rgb(0 0 0 / 0.40)`).
- **Plane 2**: Fully transparent root layer. Slates fill designated screen regions with `--surface-chrome` `#161618`.

---

## 4. Input Routing Matrix & Pointer Dispatch

Pointer events (`PointerPressed`, `PointerMoved`, `PointerReleased`, `PointerWheelChanged`) and Keyboard input are routed top-down through an `InputCoordinator` service.

```text
Input Event Received
  ↓
1. Test Plane 2 (HUD Plane) Hit Targets
   - Hit? → Consume event, dispatch to Plane 2 Control. Stop.
  ↓
2. Test Plane 1 (Information Layer) Hit Targets
   - Hit? → Consume event, dispatch to Plane 1 Overlay. Stop.
  ↓
3. Pass Event to Plane 0 (Spatial Grid Canvas)
   - Execute Grid Selection, Pan, Zoom, or Placement actions.
```

---

## 5. C# / Avalonia 11.2.5 Implementation Contracts

```csharp
namespace Grove.SpatialGrid.VisualHierarchy;

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using SkiaSharp;

public enum VisualPlaneType
{
    Plane0_SpatialGrid = 0,
    Plane1_InformationLayer = 1,
    Plane2_HUDPlane = 2
}

public interface IPlaneView
{
    VisualPlaneType PlaneType { get; }
    int CompositorZIndex { get; }
    bool HandlesPointerInput(Point viewportPoint);
    void RenderPlane(SKCanvas canvas, Size viewportSize);
}

public interface IPlaneCompositor
{
    void RegisterPlaneView(IPlaneView planeView);
    void RenderAllPlanes(SKCanvas canvas, Size viewportSize);
    bool RoutePointerEvent(PointerEventArgs e, Point viewportPoint);
}

/// <summary>
/// Top-level Avalonia 11.2.5 three-plane visual compositor container.
/// </summary>
public sealed class ThreePlaneVisualCompositorContainer : Panel, IPlaneCompositor
{
    private readonly IPlaneView[] _planes = new IPlaneView[3];

    public ThreePlaneVisualCompositorContainer()
    {
        // Enforce z-index stack integrity
        ZIndex = 0;
    }

    public void RegisterPlaneView(IPlaneView planeView)
    {
        int index = (int)planeView.PlaneType;
        _planes[index] = planeView ?? throw new ArgumentNullException(nameof(planeView));
    }

    public void RenderAllPlanes(SKCanvas canvas, Size viewportSize)
    {
        // 1. Render Plane 0 (Grid)
        _planes[0]?.RenderPlane(canvas, viewportSize);

        // 2. Render Plane 1 (Information)
        _planes[1]?.RenderPlane(canvas, viewportSize);

        // 3. Render Plane 2 (HUD)
        _planes[2]?.RenderPlane(canvas, viewportSize);
    }

    public bool RoutePointerEvent(PointerEventArgs e, Point viewportPoint)
    {
        // Top-down input routing: Plane 2 -> Plane 1 -> Plane 0
        for (int i = 2; i >= 0; i--)
        {
            if (_planes[i] != null && _planes[i].HandlesPointerInput(viewportPoint))
            {
                // Event consumed by higher plane
                return true;
            }
        }
        return false;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        var point = e.GetPosition(this);
        if (RoutePointerEvent(e, point))
        {
            e.Handled = true;
        }
        base.OnPointerPressed(e);
    }
}
```
