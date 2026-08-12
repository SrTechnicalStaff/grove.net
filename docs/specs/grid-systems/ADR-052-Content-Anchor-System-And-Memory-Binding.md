---
status: "IMPLEMENTED - AWAITING USER REVIEW"
---

# ADR-052: Content Anchor System and Memory Binding Architecture

| Property | Value |
| :--- | :--- |
| **Status** | IMPLEMENTED - AWAITING USER REVIEW |
| **Date** | 2026-08-12 |
| **Area** | Spatial Grid Engine / Memory Ledger & Anchoring Subsystem |
| **Target Runtime** | C# 13 / .NET 9 / Avalonia 11.2.5 / SkiaSharp 3.x |
| **Authors** | Chief Product Definition Architect |

---

## 1. Executive Context & Architectural Principles

As defined in the core product specification [docs/product/definitions/Memory.md](file:///C:/dev/grove-v9/docs/product/definitions/Memory.md), a **Memory** is an immutable, non-duplicative semantic unit of thought decoupling raw payload content from spatial placement. When a Memory is placed on Plane 0, it manifests as a spatial **Content Placement**.

Grove v9 introduces the **Content Anchor System** (`IsAnchored`), which establishes a durable spatial lock binding a Placement footprint to its specific cell coordinate while maintaining a bidirectional provenance link to its underlying `MemoryRecord` ledger.

### Key Architectural Drivers:
1. **Spatial Pinning Invariant**: An anchored content placement (`IsAnchored = true`) is spatially immovable via drag-and-drop gestures, protecting surrounding context from accidental displacement while remaining editable in-place.
2. **Visual Marker (Notched Ribbon `A`)**: Anchored content displays a distinctive notched ribbon mark `A` in its top-right corner. The mark uses an inverted V-notch geometry rendered in `--ink` (0.88 opacity) or role hue `--k-tool`.
3. **Ledger & Frontmatter Provenance Sync**: Anchoring state and spatial coordinate changes synchronize atomically with local disk storage via YAML frontmatter (`anchors: [...]`) or companion JSON sidecars without duplicating raw content payloads.
4. **Contextual Keybinding (`A`)**: Pressing the `A` key toggles the `IsAnchored` state of the focused placement or active selection.

---

## 2. Geometry & Spatial Pinning Mathematics

```
+--------------------------------------------+--+
| Content Placement Footprint                |A |  <-- Notched Ribbon Mark 'A'
| Cell Bounds: [C_x, C_y, W, H]              |  |      (16px x 24px)
| IsAnchored: TRUE                           +--+
|                                            \/  (Inverted V Notch)
| Payload: MemoryRecord (UUIDv4)                |
+-----------------------------------------------+
```

### 2.1 Notched Ribbon `A` Path Geometry

Let $B_{\text{item}} = [x_0, y_0, x_1, y_1]$ be the world rectangle of a content placement. The top-right anchor ribbon origin $P_{\text{ribbon}} = (x_1 - 20.0, y_0)$ in DIPs.
Ribbon width $W_r = 16.0\text{ DIPs}$, total height $H_r = 24.0\text{ DIPs}$, and notch depth $d_n = 6.0\text{ DIPs}$.

The closed Skia path vector $V_{\text{ribbon}}$ in local coordinates is defined by vertices:

$$V_0 = (0, 0)$$
$$V_1 = (W_r, 0)$$
$$V_2 = (W_r, H_r)$$
$$V_3 = \left( \frac{W_r}{2}, \, H_r - d_n \right) \quad (\text{Inverted V Notch Apex})$$
$$V_4 = (0, H_r)$$

Lettermark `A` is rendered centered within the ribbon upper rect $[0, 0, W_r, H_r - d_n]$ using a 10pt custom serif font.

### 2.2 Spatial Pinning Transformation Guard

Let $\vec{v}_{\text{drag}} = (\Delta x, \Delta y)$ be a proposed spatial drag vector initiated by a pointer gesture.
The spatial translation operator $\mathbf{T}_{\text{drag}}$ acting on placement item $I$ with state $(C_x, C_y, \text{IsAnchored})$ is governed by:

$$\mathbf{T}_{\text{drag}}(I, \vec{v}_{\text{drag}}) = \begin{cases}
(C_x + \lfloor \frac{\Delta x + P/2}{P} \rfloor, \, C_y + \lfloor \frac{\Delta y + P/2}{P} \rfloor, \, \text{true}) & \text{if } \text{IsAnchored} = \text{false} \\
(C_x, \, C_y, \, \text{true}) & \text{if } \text{IsAnchored} = \text{true} \quad (\text{Refusal State})
\end{cases}$$

Attempting to translate an anchored placement emits an inline refusal feedback signal and retains exact cell alignment.

---

## 3. C# 13 Data Schema & System Contracts

```csharp
namespace Grove.SpatialGrid.Anchoring;

using System;
using System.Collections.Generic;
using Grove.SpatialGrid.Cursor;

public readonly record struct AnchorCoordinateRecord(
    Guid AnchorId,
    Guid LayerId,
    CellCoordinate Coordinates,
    string LabelText,
    DateTime CreatedAtUtc
);

public sealed record ContentAnchorBinding(
    Guid PlacementId,
    Guid MemoryId,
    bool IsAnchored,
    AnchorCoordinateRecord SpatialAnchor,
    IReadOnlyList<string> FrontmatterTags
);

public interface IMemoryAnchorService
{
    IReadOnlyDictionary<Guid, ContentAnchorBinding> ActiveBindings { get; }
    event Action<ContentAnchorBinding>? AnchorStateChanged;

    bool ToggleAnchorState(Guid placementId);
    bool SetAnchorState(Guid placementId, bool isAnchored);
    bool SyncToDiskProvenance(Guid memoryId, string markdownFilePath);
    ContentAnchorBinding? GetBindingForPlacement(Guid placementId);
}
```

---

## 4. Avalonia 11.2.5 & SkiaSharp Notched Ribbon Rendering

```csharp
namespace Grove.SpatialGrid.Rendering;

using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;

public sealed class AnchorRibbonDrawOperation : ICustomDrawOperation
{
    private readonly Rect _bounds;
    private readonly SKRect _itemWorldBounds;
    private readonly bool _isAnchored;
    private readonly Matrix3x3 _cameraTransform;

    public AnchorRibbonDrawOperation(
        Rect bounds,
        SKRect itemWorldBounds,
        bool isAnchored,
        Matrix3x3 cameraTransform)
    {
        _bounds = bounds;
        _itemWorldBounds = itemWorldBounds;
        _isAnchored = isAnchored;
        _cameraTransform = cameraTransform;
    }

    public Rect Bounds => _bounds;
    public void Dispose() { }
    public bool Equals(ICustomDrawOperation? other) => false;
    public bool HitTest(Point p) => false;

    public void Render(ImmediateDrawingContext context)
    {
        if (!_isAnchored) return;

        var skiaContext = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
        if (skiaContext is null) return;

        using var lease = skiaContext.Lease();
        var canvas = lease.SkCanvas;

        canvas.Save();
        canvas.SetMatrix(_cameraTransform);

        float ribbonWidth = 16.0f;
        float ribbonHeight = 24.0f;
        float notchDepth = 6.0f;

        float rx = _itemWorldBounds.Right - ribbonWidth - 4.0f;
        float ry = _itemWorldBounds.Top;

        using var path = new SKPath();
        path.MoveTo(rx, ry);
        path.LineTo(rx + ribbonWidth, ry);
        path.LineTo(rx + ribbonWidth, ry + ribbonHeight);
        path.LineTo(rx + (ribbonWidth / 2.0f), ry + ribbonHeight - notchDepth);
        path.LineTo(rx, ry + ribbonHeight);
        path.Close();

        // Fill Ribbon Body (--ink at 0.88 opacity or --k-tool when active)
        using (var fillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColor.Parse("#F4F4F2").WithAlpha((byte)(0.88f * 255)),
            IsAntialias = true
        })
        {
            canvas.DrawPath(path, fillPaint);
        }

        // Lettermark 'A'
        using (var textPaint = new SKPaint
        {
            Color = SKColor.Parse("#161618"), // Dark contrast ink
            TextSize = 11.0f,
            IsAntialias = true,
            Typeface = SKTypeface.FromFamilyName("Georgia", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright),
            TextAlign = SKTextAlign.Center
        })
        {
            float textX = rx + (ribbonWidth / 2.0f);
            float textY = ry + 13.0f;
            canvas.DrawText("A", textX, textY, textPaint);
        }

        canvas.Restore();
    }
}
```

---

## 5. Provenance Serialization Contract

When an anchor state transitions, frontmatter serialization executes atomically:

```yaml
---
memory_id: "7f9c2a1e-8b3d-4c91-9e2a-1b2c3d4e5f6a"
is_anchored: true
anchors:
  - anchor_id: "anc-001"
    layer_id: "layer-spatial-main"
    cell_x: 14
    cell_y: -8
    label: "Primary Hypothesis Anchor"
---
```
