---
status: "IMPLEMENTED - AWAITING USER REVIEW"
---

# ADR-013: External Drag-and-Drop System, ScreenToCell Coordinate Resolution, and Content Auto-Creation

- **Status**: Normative
- **Date**: 2026-08-12
- **Architectural Scope**: System Interoperability / Spatial Drag-and-Drop Engine
- **Target Runtime**: .NET 9.0 / Avalonia UI 11.2.5 / OS Shell Integration

---

## 1. Context & Architectural Principles

Grove allows users to drag files directly from native operating system file explorers (Windows File Explorer, macOS Finder, Linux Nautilus) onto the spatial Grid. Drop operations must calculate cell coordinates under active camera transformations and automatically instantiate the appropriate spatial content type (Note, Document, or Picture) without modal dialogs or manual file dialogs.

### 1.1 Invariant Design Laws
1. **Zero-Modal Drop**: Dragging valid OS files onto the Grid immediately instantiates spatial content placements. No confirmation modals or configuration dialogs interrupt the drop flow.
2. **Cell Quantization & Collision Refusal**: Placements must land cleanly on discrete integer grid cell coordinates $(C_x, C_y)$. If any target cell in the calculated footprint is occupied on the active Layer, the operation is **Refused** with a visual refusal hatch and plain-English notice.
3. **Camera-Aware Coordinate Conversion**: Screen pixel coordinates from `DragEventArgs.GetPosition` must be transformed into spatial grid world coordinates accounting for camera scale $S$ and camera pan offset $(O_x, O_y)$.
4. **Deterministic Extension Mapping**: File extensions deterministically select the instantiated content primitive:
   - Images (`.png`, `.jpg`, `.jpeg`, `.webp`, `.gif`) $\to$ **Picture**
   - Short Text (`.txt` $< 500$ chars) $\to$ **Note**
   - Long Text / Structured Files (`.txt` $\ge 500$ chars, `.md`, `.json`, `.pdf`) $\to$ **Document**

---

## 2. ScreenToCell Spatial Transformation Mathematics

### 2.1 Forward Viewport Transformation

Let $(X_{\text{screen}}, Y_{\text{screen}})$ be the raw cursor point relative to the Avalonia Canvas control bounds.
Let $S$ be the camera zoom level ($0.10 \le S \le 4.00$).
Let $(O_x, O_y)$ be the camera translation pan offset in screen pixels.
Let $P_{\text{cell}} = 220\text{px}$ be the canonical cell pitch.

```
Viewport Screen Space (X_screen, Y_screen)
         |
         v   Subtract Pan Offset (O_x, O_y)
Unscaled Translation
         |
         v   Divide by Camera Zoom Scale (S)
World Grid Coordinates (X_world, Y_world)
         |
         v   Divide by Cell Pitch (220px) & Floor
Quantized Cell Index (C_x, C_y)
```

### 2.2 Mathematical Transformation Equations

- **World Coordinate Conversion**:
  $$X_{\text{world}} = \frac{X_{\text{screen}} - O_x}{S}$$
  $$Y_{\text{world}} = \frac{Y_{\text{screen}} - O_y}{S}$$

- **Quantized Cell Coordinate Derivation**:
  $$C_x = \left\lfloor \frac{X_{\text{world}}}{220} \right\rfloor = \left\lfloor \frac{X_{\text{screen}} - O_x}{220 S} \right\rfloor$$

  $$C_y = \left\lfloor \frac{Y_{\text{world}}}{220} \right\rfloor = \left\lfloor \frac{Y_{\text{screen}} - O_y}{220 S} \right\rfloor$$

- **Footprint Collision Bounds Check**:
  For an incoming placement requiring an $N_w \times N_h$ cell region starting at origin $(C_x, C_y)$:
  $$\text{TargetRegion} = \left\{ (c_x, c_y) \in \mathbb{Z}^2 \;\middle|\; C_x \le c_x < C_x + N_w \land C_y \le c_y < C_y + N_h \right\}$$

  $$\text{IsFree}(\text{TargetRegion}) = \forall (c_x, c_y) \in \text{TargetRegion}, \;\; \text{LayerGrid}[c_x, c_y] == \varnothing$$

If $\text{IsFree}$ returns `false`, the drop handler sets `DragEventArgs.DragEffects = DragDropEffects.None` and renders the refusal state.

---

## 3. Extension Dispatch & Content Auto-Creation Matrix

```
                          [ Incoming File Path ]
                                     |
           +-------------------------+-------------------------+
           |                         |                         |
   Image Extensions          Text / Markdown           Document Files
(.png, .jpg, .webp, .gif)      (.txt, .md, .json)            (.pdf)
           |                         |                         |
           v                         v                         v
 [ ImageFootprintResolver ]     Check Byte Length         [ DocumentPlacement ]
           |                         |                     (Minimum 2x2 Cells)
           v           +-------------+-------------+
  [ PicturePlacement ] |                           |
                       v                           v
                Length < 500 Chars          Length >= 500 Chars
                       |                           |
                       v                           v
               [ NotePlacement ]          [ DocumentPlacement ]
             (1x1 or Solved NxN)          (Multi-Column Reflow)
```

| Extension Family | File Formats | Content Type | Footprint Derivation Rule |
| :--- | :--- | :--- | :--- |
| **Image** | `.png`, `.jpg`, `.jpeg`, `.webp`, `.gif` | **Picture** | `ImageFootprintResolver.Resolve(W_px, H_px)` ($N_w \times N_h$) |
| **Short Text** | `.txt` ($< 500$ chars) | **Note** | `NoteGeometrySolver.CalculateMinSquareExtent(text)` ($n \times n$) |
| **Rich Prose** | `.md`, `.txt` ($\ge 500$ chars) | **Document** | Minimum $2 \times 2$, dynamic vertical cell growth |
| **Data AST** | `.json` | **Document** | Formatted JSON AST inside $2 \times 2$ Document |
| **Portable Doc** | `.pdf` | **Document** | PDF Page Render AST inside $2 \times 3$ Document |

---

## 4. C# / Avalonia 11.2.5 DragDrop System Handler Implementation

```csharp
namespace Grove.UI.Services;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Input;

public sealed record CellCoordinate(int X, int Y);

public sealed record PlacementRequest(
    CellCoordinate Origin,
    int CellWidth,
    int CellHeight,
    object ContentPayload);

public static class SpatialCoordinateResolver
{
    public const double CellPitch = 220.0;

    public static CellCoordinate ScreenToCell(Point screenPoint, Point panOffset, double zoomScale)
    {
        zoomScale = Math.Clamp(zoomScale, 0.10, 4.00);

        double worldX = (screenPoint.X - panOffset.X) / zoomScale;
        double worldY = (screenPoint.Y - panOffset.Y) / zoomScale;

        int cellX = (int)Math.Floor(worldX / CellPitch);
        int cellY = (int)Math.Floor(worldY / CellPitch);

        return new CellCoordinate(cellX, cellY);
    }
}

public sealed class ExternalDragDropHandler
{
    private readonly Func<CellCoordinate, int, int, bool> _collisionChecker;
    private readonly Func<PlacementRequest, Task> _placementCommitter;

    public ExternalDragDropHandler(
        Func<CellCoordinate, int, int, bool> collisionChecker,
        Func<PlacementRequest, Task> placementCommitter)
    {
        _collisionChecker = collisionChecker;
        _placementCommitter = placementCommitter;
    }

    public void OnDragOver(object? sender, DragEventArgs e, Point panOffset, double zoomScale)
    {
        if (!e.Data.Contains(DataFormats.Files))
        {
            e.DragEffects = DragDropEffects.None;
            return;
        }

        Point screenPos = e.GetPosition((Visual)sender!);
        CellCoordinate origin = SpatialCoordinateResolver.ScreenToCell(screenPos, panOffset, zoomScale);

        // Assume baseline 1x1 footprint for preview check
        bool isFree = _collisionChecker(origin, 1, 1);
        e.DragEffects = isFree ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    public async Task OnDropAsync(object? sender, DragEventArgs e, Point panOffset, double zoomScale)
    {
        if (!e.Data.Contains(DataFormats.Files)) return;

        var fileNames = e.Data.GetFiles()?.Select(f => f.Path.LocalPath).ToList();
        if (fileNames is null || fileNames.Count == 0) return;

        Point screenPos = e.GetPosition((Visual)sender!);
        CellCoordinate origin = SpatialCoordinateResolver.ScreenToCell(screenPos, panOffset, zoomScale);

        int currentX = origin.X;
        int currentY = origin.Y;

        foreach (string filePath in fileNames)
        {
            if (!File.Exists(filePath)) continue;

            string ext = Path.GetExtension(filePath).ToLowerInvariant();
            var placement = await CreatePlacementFromFileAsync(filePath, ext, new CellCoordinate(currentX, currentY));

            if (placement is not null && _collisionChecker(placement.Origin, placement.CellWidth, placement.CellHeight))
            {
                await _placementCommitter(placement);
                currentX += placement.CellWidth; // Offset next dropped file horizontally
            }
        }

        e.Handled = true;
    }

    private static async Task<PlacementRequest?> CreatePlacementFromFileAsync(string filePath, string ext, CellCoordinate origin)
    {
        switch (ext)
        {
            case ".png":
            case ".jpg":
            case ".jpeg":
            case ".webp":
            case ".gif":
                return CreateImagePlacement(filePath, origin);

            case ".txt":
            case ".md":
            case ".json":
                return await CreateTextOrDocumentPlacementAsync(filePath, ext, origin);

            default:
                return null;
        }
    }

    private static PlacementRequest CreateImagePlacement(string filePath, CellCoordinate origin)
    {
        using var stream = File.OpenRead(filePath);
        using var bitmap = new Avalonia.Media.Imaging.Bitmap(stream);

        int w = (int)bitmap.Size.Width;
        int h = (int)bitmap.Size.Height;

        var fp = Core.Content.Picture.ImageFootprintResolver.Resolve(w, h);
        return new PlacementRequest(origin, fp.CellsW, fp.CellsH, filePath);
    }

    private static async Task<PlacementRequest> CreateTextOrDocumentPlacementAsync(string filePath, string ext, CellCoordinate origin)
    {
        string text = await File.ReadAllTextAsync(filePath);

        if (ext == ".txt" && text.Length < 500)
        {
            int n = Core.Content.Note.NoteGeometrySolver.CalculateMinSquareExtent(text);
            return new PlacementRequest(origin, n, n, text);
        }

        // Long text or markdown -> Document
        return new PlacementRequest(origin, 2, 2, text);
    }
}
```
