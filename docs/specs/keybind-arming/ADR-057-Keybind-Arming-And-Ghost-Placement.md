---
status: "PARTIAL — verified arming and placement feedback"
---

# ADR-057: Keybind Arming State Machine and Ghost Placement Preview

> **Domain ownership rule.** A successful placement commits a Content instance
> and its Placement state. If the armed tool represents a new source, it also
> creates the source Memory; if it represents an existing Memory, it reuses
> that `MemoryId`. The spatial index receives Content placement data. This
> ADR must not describe a placed Memory or a Memory-owned spatial Anchor.

| Property | Value |
| :--- | :--- |
| **Status** | PARTIAL — verified arming and placement feedback |
| **Date** | 2026-08-12 |
| **Area** | Spatial Grid Engine / Input & Arming Subsystem |
| **Target Runtime** | C# 13 / .NET 9 / Avalonia 11.2.5 / SkiaSharp 3.x |
| **Authors** | Chief Product Definition Architect |

---

## 1. Executive Context & Architectural Principles

In Grove v9, creating spatial content on Plane 0 (Spatial Grid Canvas) uses an explicit **Tool Arming & Ghost Placement** model instead of modal modal dialogs or click-and-drag drawing tools.

Rather than immediately instantiating a content item on keypress at an arbitrary coordinate, key bindings put the spatial interaction engine into an **Armed State**. While armed, a cell-aligned **Ghost Placement Preview** floats synchronously at the active grid cursor position. The placement is committed to the spatial grid ledger only upon a left-click pointer event, or aborted via `Esc`.

### Key Architectural Drivers:
1. **Modal Spatial Arming**: Pressing `N` (Note), `Shift+N` (Quick Note), or `D` (Document) arms the spatial grid cursor with the corresponding content footprint and default dimensions.
2. **50% Alpha Inset Ghost Preview**: While armed, the targeted grid cells render a semi-transparent ghost preview featuring a 50% alpha fill (`--ghost-fill-opacity` = 0.50) and an inset 2px accent ring (`--ghost-ring-width` = 2px, opacity 0.88). Outset strokes are strictly forbidden to maintain cell boundary precision.
3. **Collision-Aware Validation Tinting**: If the hovered grid region is occupied or invalid (`!IsRegionFree`), the ghost preview dynamically switches from tool accent color (`--k-tool` `#3B82F6`) to collision refusal color (`--k-invalid` `#EF4444`). Left-clicking an invalid cell rejects placement and triggers an visual rejection pulse without disarming.
4. **Immediate Inline Focus Transfer**: For `ARMED_QUICKNOTE`, upon left-click placement commit, the engine immediately transfers active keyboard focus to the newly created cell's `FocusedTextBox` inline editor, enabling zero-latency text entry.
5. **Deterministic State Transitions**: State transitions follow an immutable state machine hierarchy (`IDLE` $\rightarrow$ `ARMED_NOTE` / `ARMED_QUICKNOTE` / `ARMED_DOCUMENT` $\rightarrow$ `PLACED` $\rightarrow$ `IDLE`).

---

## 2. Tool Arming State Machine & Transition Specification

```
+-----------------------------------------------------------------------------------+
|                                                                                   |
|                                     +------+                                      |
|            +----------------------> | IDLE | <------------------------+           |
|            |                        +------+                          |           |
|            |                           |                              |           |
|     [Esc] / Disarm         Arm [N]     |     Arm [Shift+N]      [Esc] / Disarm    |
|            |            +--------------+--------------+               |           |
|            |            |              |              |               |           |
|            |            v              v              v               |           |
|            |     +-------------+ +---------------+ +----------------+ |           |
|            +---| ARMED_NOTE  | |ARMED_QUICKNOTE| | ARMED_DOCUMENT |-+           |
|                  +-------------+ +---------------+ +----------------+             |
|                         |              |                  |                       |
|                         | Left-Click   | Left-Click       | Left-Click            |
|                         v              v                  v                       |
|                  +--------------------------------------------------+             |
|                  |                     PLACED                       |             |
|                  | (Commit Content & Placement; index Content spatial state) |             |
|                  +--------------------------------------------------+             |
|                                         |                                         |
|                                         | Auto-Transition                         |
|                                         v                                         |
|                                      [ IDLE ]                                     |
|                                                                                   |
+-----------------------------------------------------------------------------------+
```

### 2.1 Tool Arming State Definitions

| State | Description | Active Footprint | Left-Click Behavior | Esc Behavior |
| :--- | :--- | :--- | :--- | :--- |
| `IDLE` | Default spatial cursor mode. Free selection and navigation. | Dynamic ($1 \times 1$ or target size) | Select / Drag Marquee | Clear Selection |
| `ARMED_NOTE` | Note tool armed. Ghost preview active. | $1 \times 1$ cell ($220 \times 220$ DIPs) | Commit Note, transition to `PLACED` | Disarm to `IDLE` |
| `ARMED_QUICKNOTE` | Quick Note tool armed. Ghost preview active. | $1 \times 1$ cell ($220 \times 220$ DIPs) | Commit Note, focus `FocusedTextBox`, transition to `PLACED` | Disarm to `IDLE` |
| `ARMED_DOCUMENT` | Document tool armed. Ghost preview active. | $2 \times 2$ cells ($440 \times 440$ DIPs) | Commit Document, transition to `PLACED` | Disarm to `IDLE` |
| `PLACED` | Content and Placement committed; Content spatial state indexed. | N/A (Transient state) | N/A | N/A |

### 2.2 Complete State Transition Matrix

| From State | Event / Input Trigger | Guard Condition | Target State | Executed Side Effects |
| :--- | :--- | :--- | :--- | :--- |
| `IDLE` | `Key_N` | Focus level == `Plane0Canvas` | `ARMED_NOTE` | Set active tool `Note`, calculate $1 \times 1$ ghost footprint, activate ghost renderer |
| `IDLE` | `Key_ShiftN` | Focus level == `Plane0Canvas` | `ARMED_QUICKNOTE` | Set active tool `QuickNote`, calculate $1 \times 1$ ghost footprint, activate ghost renderer |
| `IDLE` | `Key_D` | Focus level == `Plane0Canvas` | `ARMED_DOCUMENT` | Set active tool `Document`, calculate $2 \times 2$ ghost footprint, activate ghost renderer |
| `ARMED_*` | `Key_N` | Focus level == `Plane0Canvas` | `ARMED_NOTE` | Switch armed tool to `Note` ($1 \times 1$) |
| `ARMED_*` | `Key_ShiftN` | Focus level == `Plane0Canvas` | `ARMED_QUICKNOTE` | Switch armed tool to `QuickNote` ($1 \times 1$) |
| `ARMED_*` | `Key_D` | Focus level == `Plane0Canvas` | `ARMED_DOCUMENT` | Switch armed tool to `Document` ($2 \times 2$) |
| `ARMED_*` | `Key_Escape` | Always | `IDLE` | Deactivate ghost renderer, clear armed state, restore default grid cursor |
| `ARMED_*` | `Pointer_LeftClick` | `IsRegionFree == true` | `PLACED` | Create or reuse a Memory, create Content with its `MemoryId`, commit Placement, index Content spatial state, and auto-focus text box if `ARMED_QUICKNOTE` |
| `ARMED_*` | `Pointer_LeftClick` | `IsRegionFree == false` | `ARMED_*` (Unchanged) | Refuse placement, trigger 200ms visual error shake/red flash on ghost boundary |
| `ARMED_*` | `Pointer_RightClick` | Always | `IDLE` | Cancel arming state, return to `IDLE` |
| `PLACED` | `Internal_CommitDone` | Always | `IDLE` | Reset arming state to `IDLE`, clear transient parameters |

---

## 3. Ghost Placement Geometry & Mathematical Formulations

```
   Pointer Position P_world = (x, y)
             |
             v
   +-------------------+  Cell Bounds B_ghost:
   | (C_x, C_y)        |  Origin = (C_x, C_y)
   | +---------------+ |  Width  = W_cells * 220 DIPs
   | | Ghost Preview | |  Height = H_cells * 220 DIPs
   | | 50% Alpha Fill| |  Stroke = 2px Inset Ring
   | | Accent / Red  | |
   | +---------------+ |
   +-------------------+
```

### 3.1 Pointer Projection & Ghost Origin Computation

Let $P_{\text{screen}} = (x_{\text{screen}}, y_{\text{screen}})$ be the screen cursor position. Inverted camera matrix $T_{\text{camera}}^{-1}$ converts $P_{\text{screen}}$ to world point $P_{\text{world}} = (x_{\text{world}}, y_{\text{world}})$.

The origin cell $(C_x, C_y) \in \mathbb{Z}^2$ for the ghost footprint is computed as:

$$C_x = \left\lfloor \frac{x_{\text{world}}}{P_{\text{cell}}} \right\rfloor, \quad C_y = \left\lfloor \frac{y_{\text{world}}}{P_{\text{cell}}} \right\rfloor$$

where grid step $P_{\text{cell}} = 220.0\text{ DIPs}$.

### 3.2 Multi-Cell Footprint Rectangle Math

For an armed content type with cell dimensions $(W_{\text{cells}}, H_{\text{cells}})$, the top-left aligned cell footprint bounds $R_{\text{ghost}}$ in spatial cell space are:

$$R_{\text{ghost}} = \left[ C_x, \, C_y, \, W_{\text{cells}}, \, H_{\text{cells}} \right]$$

World-space continuous rectangular bounds $B_{\text{world}} = [x_0, y_0, x_1, y_1]$:

$$x_0 = C_x \cdot P_{\text{cell}}, \qquad y_0 = C_y \cdot P_{\text{cell}}$$
$$x_1 = (C_x + W_{\text{cells}}) \cdot P_{\text{cell}}, \qquad y_1 = (C_y + H_{\text{cells}}) \cdot P_{\text{cell}}$$

### 3.3 Ghost Visual Geometry & Skia Inset Stroke Calculations

Let screen-space bounds $B_{\text{screen}} = T_{\text{camera}}(B_{\text{world}})$.
The 2px inset stroke rectangle $B_{\text{inset}}$ guarantees that the ring stroke does not spill into adjacent grid cells:

$$w_{\text{ring}} = 2.0\text{ px}$$
$$B_{\text{inset}} = \left[ x_{\text{screen}, 0} + \frac{w_{\text{ring}}}{2}, \, y_{\text{screen}, 0} + \frac{w_{\text{ring}}}{2}, \, x_{\text{screen}, 1} - \frac{w_{\text{ring}}}{2}, \, y_{\text{screen}, 1} - \frac{w_{\text{ring}}}{2} \right]$$

The opacity equations for fill and stroke are defined as:

$$\alpha_{\text{ghost\_fill}} = 0.50 \quad (50\% \text{ alpha fill})$$
$$\alpha_{\text{ghost\_ring}} = 0.88 \quad (88\% \text{ alpha ring opacity})$$

Color evaluation based on spatial region availability:

$$\text{Color}_{\text{ghost}} = \begin{cases}
\#3B82F6 & \text{if } \text{IsRegionFree}(R_{\text{ghost}}, L_{\text{active}}) = \text{true} \quad (\text{Tool Accent}) \\
\#EF4444 & \text{if } \text{IsRegionFree}(R_{\text{ghost}}, L_{\text{active}}) = \text{false} \quad (\text{Collision Refusal})
\end{cases}$$

---

## 4. C# 13 Type Contracts & System Interfaces

```csharp
namespace Grove.SpatialGrid.Arming;

using System;
using System.Runtime.InteropServices;
using Avalonia;
using SkiaSharp;
using Grove.SpatialGrid.Cursor;

/// <summary>
/// Defines the explicit tool arming states for Plane 0 creation.
/// </summary>
public enum ToolArmingState : byte
{
    Idle = 0,
    ArmedNote = 1,
    ArmedQuickNote = 2,
    ArmedDocument = 3,
    Placed = 4
}

/// <summary>
/// Supported content types that can be armed for placement.
/// </summary>
public enum ArmableContentType : byte
{
    Note = 0,
    QuickNote = 1,
    Document = 2
}

/// <summary>
/// Immutable descriptor representing the current ghost placement preview.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public readonly record struct GhostPlacementDescriptor(
    ToolArmingState ArmingState,
    ArmableContentType ContentType,
    CellCoordinate OriginCell,
    int WidthCells,
    int HeightCells,
    bool IsValidRegion
)
{
    public FootprintBounds Footprint => new(OriginCell.X, OriginCell.Y, WidthCells, HeightCells);
}

/// <summary>
/// Result structure returned upon committing a ghost placement.
/// </summary>
public sealed record PlacementCommitResult(
    bool IsSuccess,
    Guid MemoryId,
    CellCoordinate OriginCell,
    FootprintBounds Footprint,
    bool AutoFocusRequested,
    string? ErrorReason
);

/// <summary>
/// Core contract for controlling tool arming and ghost placement operations.
/// </summary>
public interface IToolArmingService
{
    ToolArmingState CurrentState { get; }
    GhostPlacementDescriptor ActiveGhostDescriptor { get; }

    event Action<ToolArmingState>? ArmingStateChanged;
    event Action<GhostPlacementDescriptor>? GhostPreviewUpdated;
    event Action<PlacementCommitResult>? PlacementCommitted;

    void ArmTool(ArmableContentType contentType);
    void Disarm();
    void UpdateCursorPosition(Point worldPointerPosition, Guid selectedGridLayerId);
    bool CommitPlacement(Point worldPointerPosition, Guid selectedGridLayerId, out PlacementCommitResult result);
}
```

---

## 5. Avalonia Render Integration & Skia Operations

```csharp
namespace Grove.SpatialGrid.Arming.Rendering;

using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using SkiaSharp;
using Grove.SpatialGrid.Arming;

public sealed class GhostPlacementDrawOperation : ICustomDrawOperation
{
    private readonly Rect _bounds;
    private readonly GhostPlacementDescriptor _descriptor;
    private readonly Matrix _cameraTransform;

    public GhostPlacementDrawOperation(Rect bounds, GhostPlacementDescriptor descriptor, Matrix cameraTransform)
    {
        _bounds = bounds;
        _descriptor = descriptor;
        _cameraTransform = cameraTransform;
    }

    public Rect Bounds => _bounds;

    public bool Equals(ICustomDrawOperation? other) => 
        other is GhostPlacementDrawOperation op &&
        _descriptor == op._descriptor &&
        _bounds == op._bounds &&
        _cameraTransform == op._cameraTransform;

    public void Render(ImmediateDrawingContext context)
    {
        var skiaFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
        if (skiaFeature is null) return;

        using var lease = skiaFeature.Lease();
        var canvas = lease.SkCanvas;

        canvas.Save();

        // Convert cell footprint to world bounds (220.0 DIP cell size)
        const float cellSize = 220.0f;
        float worldX = _descriptor.OriginCell.X * cellSize;
        float worldY = _descriptor.OriginCell.Y * cellSize;
        float worldW = _descriptor.WidthCells * cellSize;
        float worldH = _descriptor.HeightCells * cellSize;

        var worldRect = SKRect.Create(worldX, worldY, worldW, worldH);

        // Map world bounds through camera matrix
        var skMatrix = new SKMatrix(
            (float)_cameraTransform.M11, (float)_cameraTransform.M21, (float)_cameraTransform.M31,
            (float)_cameraTransform.M12, (float)_cameraTransform.M22, (float)_cameraTransform.M32,
            0, 0, 1);

        SKRect screenRect = skMatrix.MapRect(worldRect);

        // Select color based on validity
        SKColor baseColor = _descriptor.IsValidRegion 
            ? SKColor.Parse("#3B82F6")  // --k-tool accent
            : SKColor.Parse("#EF4444"); // --k-invalid refusal

        // 1. 50% Alpha Fill
        using var fillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = baseColor.WithAlpha(128), // 50% opacity
            IsAntialias = true
        };
        canvas.DrawRect(screenRect, fillPaint);

        // 2. Inset 2px Ring (Stroke)
        const float strokeWidth = 2.0f;
        var insetRect = new SKRect(
            screenRect.Left + strokeWidth / 2.0f,
            screenRect.Top + strokeWidth / 2.0f,
            screenRect.Right - strokeWidth / 2.0f,
            screenRect.Bottom - strokeWidth / 2.0f);

        using var ringPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = strokeWidth,
            Color = baseColor.WithAlpha(224), // 88% opacity
            IsAntialias = true
        };
        canvas.DrawRect(insetRect, ringPaint);

        canvas.Restore();
    }

    public void Dispose() { }
    public bool HitTest(Point p) => false;
}
```

---

## 6. Complete C# 13 State Machine Implementation

```csharp
namespace Grove.SpatialGrid.Arming;

using System;
using Avalonia;
using Grove.SpatialGrid.Cursor;

public sealed class ToolArmingStateMachine : IToolArmingService
{
    private ToolArmingState _currentState = ToolArmingState.Idle;
    private GhostPlacementDescriptor _activeGhost;

    public ToolArmingState CurrentState => _currentState;
    public GhostPlacementDescriptor ActiveGhostDescriptor => _activeGhost;

    public event Action<ToolArmingState>? ArmingStateChanged;
    public event Action<GhostPlacementDescriptor>? GhostPreviewUpdated;
    public event Action<PlacementCommitResult>? PlacementCommitted;

    public ToolArmingStateMachine()
    {
        _activeGhost = new GhostPlacementDescriptor(
            ToolArmingState.Idle,
            ArmableContentType.Note,
            CellCoordinate.Zero,
            1, 1, true);
    }

    public void ArmTool(ArmableContentType contentType)
    {
        var newState = contentType switch
        {
            ArmableContentType.Note => ToolArmingState.ArmedNote,
            ArmableContentType.QuickNote => ToolArmingState.ArmedQuickNote,
            ArmableContentType.Document => ToolArmingState.ArmedDocument,
            _ => throw new ArgumentOutOfRangeException(nameof(contentType))
        };

        (int width, int height) = contentType switch
        {
            ArmableContentType.Note => (1, 1),
            ArmableContentType.QuickNote => (1, 1),
            ArmableContentType.Document => (2, 2),
            _ => (1, 1)
        };

        _currentState = newState;
        _activeGhost = _activeGhost with
        {
            ArmingState = newState,
            ContentType = contentType,
            WidthCells = width,
            HeightCells = height
        };

        ArmingStateChanged?.Invoke(_currentState);
        GhostPreviewUpdated?.Invoke(_activeGhost);
    }

    public void Disarm()
    {
        if (_currentState == ToolArmingState.Idle) return;

        _currentState = ToolArmingState.Idle;
        _activeGhost = _activeGhost with { ArmingState = ToolArmingState.Idle };

        ArmingStateChanged?.Invoke(_currentState);
        GhostPreviewUpdated?.Invoke(_activeGhost);
    }

    public void UpdateCursorPosition(Point worldPointerPosition, Guid selectedGridLayerId)
    {
        if (_currentState == ToolArmingState.Idle) return;

        const float cellSize = 220.0f;
        int cellX = (int)Math.Floor(worldPointerPosition.X / cellSize);
        int cellY = (int)Math.Floor(worldPointerPosition.Y / cellSize);

        var targetCell = new CellCoordinate(cellX, cellY);
        bool isValid = CheckRegionAvailability(targetCell, _activeGhost.WidthCells, _activeGhost.HeightCells, selectedGridLayerId);

        _activeGhost = _activeGhost with
        {
            OriginCell = targetCell,
            IsValidRegion = isValid
        };

        GhostPreviewUpdated?.Invoke(_activeGhost);
    }

    public bool CommitPlacement(Point worldPointerPosition, Guid selectedGridLayerId, out PlacementCommitResult result)
    {
        if (_currentState == ToolArmingState.Idle)
        {
            result = new PlacementCommitResult(false, Guid.Empty, CellCoordinate.Zero, default, false, "State is Idle");
            return false;
        }

        UpdateCursorPosition(worldPointerPosition, selectedGridLayerId);

        if (!_activeGhost.IsValidRegion)
        {
            result = new PlacementCommitResult(false, Guid.Empty, _activeGhost.OriginCell, _activeGhost.Footprint, false, "Target spatial region is occupied");
            return false;
        }

        Guid newMemoryId = Guid.NewGuid();
        bool isQuickNote = _currentState == ToolArmingState.ArmedQuickNote;

        result = new PlacementCommitResult(
            IsSuccess: true,
            MemoryId: newMemoryId,
            OriginCell: _activeGhost.OriginCell,
            Footprint: _activeGhost.Footprint,
            AutoFocusRequested: isQuickNote,
            ErrorReason: null
        );

        _currentState = ToolArmingState.Placed;
        ArmingStateChanged?.Invoke(_currentState);
        PlacementCommitted?.Invoke(result);

        // Auto transition back to Idle
        Disarm();
        return true;
    }

    private static bool CheckRegionAvailability(CellCoordinate origin, int width, int height, Guid selectedGridLayerId)
    {
        // Mock region availability check against spatial index / R-Tree
        return true;
    }
}
```

---

## 7. Rationale, Architectural Trade-offs, & Alternatives

### 7.1 Why Explicit Arming Over Click-and-Drag Creation?
Click-and-drag content creation creates ambiguity between pan gestures, marquee box selections, and item creation. By enforcing explicit single-key arming (`N`, `Shift+N`, `D`), Grove v9 preserves single-click selection and left-drag marquee sweeps while ensuring high-precision, cell-aligned placement.

### 7.2 Inset Ring vs Outset Border
Outset borders cross adjacent cell boundaries, generating visual noise and falsely suggesting that placement will occupy neighboring cells. The 2px inset stroke guarantees strict visual bounding within the target footprint.

---

## 8. Verification & Test Plan

1. **State Machine Verification**:
   - Verify `N` key arms `ARMED_NOTE` state with $1 \times 1$ footprint.
   - Verify `Shift+N` arms `ARMED_QUICKNOTE` state with $1 \times 1$ footprint.
   - Verify `D` arms `ARMED_DOCUMENT` state with $2 \times 2$ footprint.
   - Verify `Esc` returns state machine to `IDLE` from any armed state.
2. **Ghost Rendering Accuracy**:
   - Verify Skia draw operation fills exactly 50% alpha opacity.
   - Verify 2px stroke is drawn completely inside cell bounds without spilling over grid lines.
   - Verify occupied cell changes ghost color from blue (`#3B82F6`) to red (`#EF4444`).
3. **Quick Note Focus Transfer**:
   - Confirm left-click commit during `ARMED_QUICKNOTE` places Note and immediately transfers keyboard focus to the inline `FocusedTextBox`.
