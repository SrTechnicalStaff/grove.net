---
status: "PARTIAL — verified context menu service and overlay"
---

# ADR-054: Spatial Context Menu System and Zero-Modal Pass-Through Architecture

| Property | Value |
| :--- | :--- |
| **Status** | PARTIAL — verified context menu service and overlay |
| **Date** | 2026-08-12 |
| **Area** | Spatial Grid Engine / HUD Context & Overlay Subsystem |
| **Target Runtime** | C# 13 / .NET 9 / Avalonia 11.2.5 / SkiaSharp 3.x |
| **Authors** | Chief Product Definition Architect |

---

## 1. Executive Context & Architectural Principles

Grove v9 enforces strict visual separation across three composition planes (Plane 0: Grid Canvas, Plane 1: Information Plane, Plane 2: HUD Plane). When a context menu is invoked via right-click, pointer long-press, or `Menu` keyboard key, it must render as a targeted overlay without disrupting continuous rendering on Plane 0.

### Key Architectural Drivers:
1. **Zero-Modal Pass-Through Architecture**: The context menu resides on Plane 2 (HUD Overlay Canvas). It introduces ZERO backdrop scrim (`#000000` opacity = 0%) and ZERO canvas blur. Pointer events occurring outside the menu rect pass through directly to interact with Plane 0 grid content or dismiss the menu without modal input trapping.
2. **Context-Sensitive Target Resolution**: Right-clicking a coordinate evaluates cell occupancy to construct targeted menu models:
   - **Empty Field Context**: Create Note (`N`), Create Document, Paste (`Ctrl+V`), Grid Properties.
   - **Single Item Context**: Open in Writing Slate, Memory Slate, or Gallery Slate, Anchor (`A`), Trace to Layer (`1`), Copy (`Ctrl+C`), Cut (`Ctrl+X`), Delete (`Del`).
   - **Multi-Selection Context**: Group Anchor (`A`), Batch Trace (`1`), Copy All (`Ctrl+C`), Delete All (`Del`).
3. **Viewport Collision Clamping**: Menu screen coordinates dynamically adjust to remain fully inside viewport bounds $[0, 0, W_{\text{viewport}}, H_{\text{viewport}}]$, preventing clipped popups near display edges.

---

## 2. Menu Placement & Positioning Mathematics

```
Screen Viewport Bounds [0, 0, W_screen, H_screen]
+-------------------------------------------------------------+
|                                                             |
|                   Pointer Right-Click P_click = (x_p, y_p)  |
|                   +-------------------------+               |
|                   | Context Menu Bounds     |               |
|                   | [x_menu, y_menu, W, H]  |               |
|                   +-------------------------+               |
|                                                             |
+-------------------------------------------------------------+
```

### 2.1 Screen Position Clamping Algorithm

Let $P_{\text{click}} = (x_p, y_p)$ be the screen coordinate of the pointer click. Let $W_m, H_m$ be the measured physical DIP dimensions of the context menu frame. Let $W_v, H_v$ be the viewport dimensions.

The computed top-left screen position $P_{\text{menu}} = (x_m, y_m)$ is governed by:

$$x_m = \begin{cases} 
x_p & \text{if } x_p + W_m \le W_v \\
x_p - W_m & \text{if } x_p + W_m > W_v \text{ and } x_p - W_m \ge 0 \\
W_v - W_m & \text{otherwise}
\end{cases}$$

$$y_m = \begin{cases} 
y_p & \text{if } y_p + H_m \le H_v \\
y_p - H_m & \text{if } y_p + H_m > H_v \text{ and } y_p - H_m \ge 0 \\
H_v - H_m & \text{otherwise}
\end{cases}$$

---

## 3. C# 13 Data Contracts & Interface Architecture

```csharp
namespace Grove.HUD.ContextMenu;

using System;
using System.Collections.Generic;
using Avalonia;
using Grove.SpatialGrid.Cursor;

public enum ContextMenuTargetType : byte
{
    EmptyCell = 0,
    SinglePlacement = 1,
    MultiSelection = 2
}

public sealed record ContextMenuTargetContext(
    ContextMenuTargetType TargetType,
    CellCoordinate AddressedCell,
    Guid ActiveLayerId,
    IReadOnlyList<Guid> TargetPlacementIds
);

public sealed record ContextMenuItemViewModel(
    string Id,
    string Header,
    string? InputGestureText,
    Action Command,
    bool IsEnabled,
    bool IsSeparator = false
);

public sealed record SpatialContextMenuModel(
    ContextMenuTargetContext TargetContext,
    Point ScreenPosition,
    IReadOnlyList<ContextMenuItemViewModel> Items
);

public interface ISpatialContextMenuService
{
    SpatialContextMenuModel? ActiveMenu { get; }
    event Action<SpatialContextMenuModel?>? ContextMenuStateChanged;

    void OpenContextMenuAt(Point screenPoint, CellCoordinate targetCell, Guid layerId);
    void CloseContextMenu();
    bool ProcessPointerPressed(Point screenPoint);
}
```

---

## 4. Avalonia 11.2.5 HUD Overlay Integration

Context menus are rendered inside Plane 2's `OverlayLayer` canvas:

```xml
<!-- Plane 2 Overlay Layer Integration -->
<Canvas x:Class="Grove.HUD.Views.SpatialContextMenuOverlayView"
        xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        HorizontalAlignment="Stretch"
        VerticalAlignment="Stretch"
        Background="Transparent"
        IsHitTestVisible="True">

    <!-- Zero-Scrim Context Menu Container -->
    <Border Name="ContextMenuFrame"
            Width="220"
            Background="#1C1C1E"
            BorderBrush="#2C2C2E"
            BorderThickness="1"
            CornerRadius="0"
            BoxShadow="0 4 16 0 #40000000"
            IsVisible="{Binding ActiveMenu, Converter={x:Static ObjectConverters.IsNotNull}}">

        <ItemsControl ItemsSource="{Binding ActiveMenu.Items}">
            <ItemsControl.ItemTemplate>
                <DataTemplate>
                    <Button Classes="ContextMenuRow"
                            Command="{Binding Command}"
                            IsEnabled="{Binding IsEnabled}"
                            HorizontalAlignment="Stretch"
                            Height="32">
                        <Grid ColumnDefinitions="*,Auto">
                            <TextBlock Grid.Column="0" Text="{Binding Header}" Foreground="#F4F4F2" VerticalAlignment="Center"/>
                            <TextBlock Grid.Column="1" Text="{Binding InputGestureText}" Foreground="#8E8E93" VerticalAlignment="Center"/>
                        </Grid>
                    </Button>
                </DataTemplate>
            </ItemsControl.ItemTemplate>
        </ItemsControl>
    </Border>
</Canvas>
```

---

## 5. Architectural Invariants & Refusal Protocol

1. **Zero Scrim Mandatory**: Context menus MUST NOT add a dimming backdrop overlay or capture global input. Spatial canvas outside the menu frame remains 100% interactive.
2. **Deterministic Focus Return**: Dismissing a context menu automatically restores keyboard attention to the Grid Cursor at `AddressedCell`.
3. **No Dynamic Geometry Mutation**: Menu height and item layout remain fixed upon opening; menu items do not recalculate layout during mouse hover.
