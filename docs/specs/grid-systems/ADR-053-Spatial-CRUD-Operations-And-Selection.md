---
status: "PARTIAL — verified selection and clipboard contracts"
---

# ADR-053: Spatial CRUD Operations, Selection State Machine, and Marquee Sweep

| Property | Value |
| :--- | :--- |
| **Status** | PARTIAL — verified selection and clipboard contracts |
| **Date** | 2026-08-12 |
| **Area** | Spatial Grid Engine / Operations & Selection Subsystem |
| **Target Runtime** | C# 13 / .NET 9 / Avalonia 11.2.5 / SkiaSharp 3.x |
| **Authors** | Chief Product Definition Architect |

---

## 1. Executive Context & Architectural Principles

Grove v9 manages all content interaction on Plane 0 through a unified **Spatial CRUD & Selection Engine**. Unlike document-centric editors that rely on structural DOM trees or traditional canvas tools that use freeform pixel boundaries, Grove enforces cell-aligned spatial manipulation, marquee box selection sweeps, and deterministic context-aware keybindings.

### Key Architectural Drivers:
1. **Cell-Aligned Marquee Sweep**: Pointer dragging on empty canvas initiates a cell-aligned marquee selection sweep. The marquee bounds snap to whole cell indices $[C_{x, \text{start}}, C_{y, \text{start}}] \times [C_{x, \text{end}}, C_{y, \text{end}}]$, accumulating all intersected content footprints into the active selection set.
2. **Context-Aware Keyboard Navigation**:
   - `N`: Instantiates a new Note placement at the active cursor footprint.
   - `A`: Toggles `IsAnchored` pinning state on selected placements.
   - `Del` / `Backspace`: Removes selected placements from the spatial grid index.
   - `Esc`: Cancels active gestures, clears selections, and disarms tools.
   - `Ctrl+C` / `Ctrl+V`: Copies selected Memory references to system/internal spatial clipboards and pastes payloads relative to cursor location.
3. **Group Translation Invariant**: Moving a multi-item selection translates all constituent placements by uniform cell delta $(\Delta C_x, \Delta C_y)$, preserving relative spatial geometry. If ANY non-anchored item in the group encounters a collision (`!IsRegionFree`), the ENTIRE group move is refused.

---

## 2. Selection Mathematics & Marquee Intersection Algorithms

```
               C_x,start                                C_x,end
  C_y,start    +----------------------------------------+
               | Swept Marquee Rectangle                |
               | Fill: --signal-interaction (6%)        |
               | Border: 1px Dashed Inset               |
               |                                        |
               |     +-------------------+              |
               |     | Intersected Item  |              |
               |     | (Accumulated)     |              |
               |     +-------------------+              |
  C_y,end      +----------------------------------------+
```

### 2.1 Marquee Cell Rectangle Computation

Let $P_{\text{start}} = (x_{\text{start}}, y_{\text{start}})$ and $P_{\text{current}} = (x_{\text{curr}}, y_{\text{curr}})$ be world pointer coordinates during a drag gesture.
The swept marquee cell bounds $R_{\text{marquee}} = [X_{\text{min}}, Y_{\text{min}}, W_{\text{cells}}, H_{\text{cells}}]$ are:

$$C_{x, 0} = \left\lfloor \frac{x_{\text{start}}}{P_{\text{cell}}} \right\rfloor, \quad C_{y, 0} = \left\lfloor \frac{y_{\text{start}}}{P_{\text{cell}}} \right\rfloor$$
$$C_{x, 1} = \left\lfloor \frac{x_{\text{curr}}}{P_{\text{cell}}} \right\rfloor, \quad C_{y, 1} = \left\lfloor \frac{y_{\text{curr}}}{P_{\text{cell}}} \right\rfloor$$

$$X_{\text{min}} = \min(C_{x, 0}, C_{x, 1}), \quad Y_{\text{min}} = \min(C_{y, 0}, C_{y, 1})$$
$$W_{\text{cells}} = |C_{x, 1} - C_{x, 0}| + 1, \quad H_{\text{cells}} = |C_{y, 1} - C_{y, 0}| + 1$$

### 2.2 Axis-Aligned Bounding Box (AABB) Intersection Query

For every item footprint $I_k = [X_k, Y_k, W_k, H_k]$ on the active layer, item $I_k$ is added to selection set $\mathcal{S}$ if and only if:

$$\text{Select}(I_k) = (X_{\text{min}} < X_k + W_k) \land (X_{\text{min}} + W_{\text{cells}} > X_k) \land (Y_{\text{min}} < Y_k + H_k) \land (Y_{\text{min}} + H_{\text{cells}} > Y_k)$$

---

## 3. C# 13 Type Contracts & System Interfaces

```csharp
namespace Grove.SpatialGrid.Operations;

using System;
using System.Collections.Generic;
using Grove.SpatialGrid.Cursor;
using Grove.SpatialGrid.Resize;

public sealed record SpatialClipboardItemPayload(
    Guid OriginalMemoryId,
    string ContentType,
    string RawPayload,
    int RelativeCellX,
    int RelativeCellY,
    int WidthCells,
    int HeightCells
);

public sealed record SpatialClipboardContainer(
    IReadOnlyList<SpatialClipboardItemPayload> Items,
    DateTime CopiedAtUtc
);

public interface ISelectionService
{
    IReadOnlySet<Guid> SelectedPlacementIds { get; }
    SpatialRegion? MarqueeBounds { get; }
    bool IsMarqueeActive { get; }

    event Action<IReadOnlySet<Guid>>? SelectionChanged;
    event Action<SpatialRegion?>? MarqueeBoundsChanged;

    void BeginMarqueeSweep(CellCoordinate startCell);
    void UpdateMarqueeSweep(CellCoordinate currentCell);
    void CommitMarqueeSweep(bool isShiftHeld);
    void ClearSelection();
    void SelectSingle(Guid placementId);
}

public interface ISpatialCrudService
{
    bool CreateNewNoteAt(CellCoordinate cell);
    bool ToggleAnchorOnSelection();
    bool DeleteSelectedPlacements();
    bool MoveSelectionBy(int deltaX, int deltaY);
    SpatialClipboardContainer CopySelectionToClipboard();
    bool PasteFromClipboardAt(CellCoordinate targetCell, SpatialClipboardContainer payload);
}
```

---

## 4. Avalonia 11.2.5 Input & Rendering Integration

### 4.1 Marquee Selection Skia Draw Operation

```csharp
namespace Grove.SpatialGrid.Rendering;

using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Grove.SpatialGrid.Resize;
using SkiaSharp;

public sealed class MarqueeDrawOperation : ICustomDrawOperation
{
    private readonly Rect _bounds;
    private readonly SpatialRegion _marqueeRegion;
    private readonly Matrix3x3 _cameraTransform;
    private readonly float _cellPitchDip;

    public MarqueeDrawOperation(
        Rect bounds,
        SpatialRegion marqueeRegion,
        Matrix3x3 cameraTransform,
        float cellPitchDip = 220.0f)
    {
        _bounds = bounds;
        _marqueeRegion = marqueeRegion;
        _cameraTransform = cameraTransform;
        _cellPitchDip = cellPitchDip;
    }

    public Rect Bounds => _bounds;
    public void Dispose() { }
    public bool Equals(ICustomDrawOperation? other) => false;
    public bool HitTest(Point p) => false;

    public void Render(ImmediateDrawingContext context)
    {
        var skiaContext = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
        if (skiaContext is null) return;

        using var lease = skiaContext.Lease();
        var canvas = lease.SkCanvas;

        canvas.Save();
        canvas.SetMatrix(_cameraTransform);

        float pitch = _cellPitchDip;
        var rect = SKRect.Create(
            _marqueeRegion.X * pitch,
            _marqueeRegion.Y * pitch,
            _marqueeRegion.Width * pitch,
            _marqueeRegion.Height * pitch
        );

        // Fill: --signal-interaction at 6% opacity
        using (var fillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColor.Parse("#3B82F6").WithAlpha((byte)(0.06f * 255)),
            IsAntialias = false
        })
        {
            canvas.DrawRect(rect, fillPaint);
        }

        // Dashed Border: 1px dashed --signal-interaction at 80% opacity
        using (var borderPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.0f / canvas.TotalMatrix.ScaleX,
            Color = SKColor.Parse("#3B82F6").WithAlpha((byte)(0.80f * 255)),
            PathEffect = SKPathEffect.CreateDash(new float[] { 4.0f, 4.0f }, 0.0f),
            IsAntialias = true
        })
        {
            canvas.DrawRect(rect, borderPaint);
        }

        canvas.Restore();
    }
}
```

---

## 5. Keyboard Shortcut Execution Matrix

| Key Combo | Action | Conditions |
| :--- | :--- | :--- |
| `N` | Create Note Placement | Focus on Plane 0 empty cell footprint |
| `A` | Toggle Anchor Pin (`IsAnchored`) | Selection count $\ge 1$ |
| `Del` / `Backspace` | Delete Placements | Selection count $\ge 1$ |
| `Esc` | Clear Selection / Cancel Mode | Active selection or marquee drag active |
| `Ctrl+C` | Copy Memory References | Selection count $\ge 1$ |
| `Ctrl+V` | Paste Memory Footprints | Clipboard contains valid payload |

---

## 6. Signal Roles, Footprint Previewing & Selection Refusal Rules

### 6.1 Three Distinct Signal Roles
Working chrome enforces strict role-to-color mapping with zero semantic overlap:
1. **Interaction Role (`#96B6F8`)**: Selection and focus. Applied as a 2px outline outside the content edge, a soft glow `0 0 24px 6px rgb(150 182 248 / 0.45)`, and structural field brightening (`rgba(150,182,248,0.13)` edge cells, `0.06` diagonal cells) with perimeter inset `inset 0 0 0 1.5px rgb(150 182 248 / 0.30)`.
2. **Marquee Role (`#E8B964`)**: Active selection work during an open drag gesture. Rendered with 6% fill `rgba(232,185,100,0.08)` and 1.5px border `rgba(232,185,100,0.78)`. Upon pointer release, amber disappears and hands over to interaction accent `#96B6F8`.
3. **Invalid / Refusal Role (`#E2625C` / `#F06543`)**: Spatial refusal. Rendered with 12% fill `rgba(226,98,92,0.12)`, $45^\circ$ diagonal cross-hatching, and inset border `inset 0 0 0 1.5px rgba(226,98,92,0.45)`.

### 6.2 Placement Footprint Preview Contract
Before commit, placement previews cover every cell the content will occupy:
- Rendered with a 1px dashed edge `rgba(150,182,248,0.80)`, 6% transparent fill `rgba(150,182,248,0.06)`, and a quiet monospaced corner size label (`3 × 3`).
- Origin content remains untouched at its source position until pointer release.

### 6.3 Selection & Placement Refusal Invariants
- **Refused Fill Wash**: Selection is not a coat of paint. Tinting or washing over content surfaces to show selection is forbidden.
- **Refused Solid Preview**: Previews must never look like placed content facts. Solid opaque previews are forbidden.
- **Refused Center-Screen Alert**: Refusal notification belongs at the point of action beside the footprint ("This space is occupied" strip toolbar). Center-screen modal alert dialogs and canvas dimming are strictly forbidden.
