---
status: "PARTIAL — verified implementation with remaining interaction gaps"
---

# ADR-043: Spatial Layer Manager UI and Controls

| Property | Value |
| :--- | :--- |
| **Status** | PARTIAL — verified implementation with remaining interaction gaps |
| **Date** | 2026-08-12 |
| **Area** | HUD Overlay Plane / Layer Control UI / Avalonia Controls |
| **Target Runtime** | C# 13 / .NET 9 / Avalonia 11.2.5 |
| **Authors** | Chief Product Definition Architect |

---

## 1. Executive Context & Surface Class Contract

The **Spatial Layer Manager** is a viewport-fixed HUD overlay composed on Plane 2 as defined in `docs/design-system/30-components/Layer-manager.md`, `docs/design-system/20-planes/HUD-plane.md`, and [ADR-031](file:///C:/dev/grove-v9/docs/specs/hud-system/ADR-031-Slate-Window-System-And-Anatomy.md). It is not a named Slate.

```
Plane 2 — HUD Plane: Layer Manager Overlay Anatomy
─────────────────────────────────────────────────────────────────────────────────────────────
┌───────────────────────────────────────────────────────────────────────────────────────────┐
│ LAYERS                                                                           [Close]  │ <-- Header Row
│ 12 LAYERS                                                                                 │ <-- Stack Count
│ ┌───────────────────────────────────────────────────────────────────────────────────────┐ │
│ │ Find a Layer                                                                          │ │ <-- Filter Field
│ └───────────────────────────────────────────────────────────────────────────────────────┘ │
│ ───────────────────────────────────────────────────────────────────────────────────────── │
│ B01   Background Drawings                             [Visible] [Color: Clay]   [...]     │
│ 01    Base Field Anchor (Active Layer)                [Visible] [Color: Slate Blue]  [...] │ <-- Selected Row
│ 02    Architectural Annotations                       [Visible] [Color: Violet] [...]     │
│       ┌──────────────────────────────────────────────────────────────────────────────┐    │
│       │ Remove Layer 02 and move what is on it to Layer 01?                          │    │ <-- Inline Confirm
│       │ [Keep it]  [Remove]                                                          │    │
│       └──────────────────────────────────────────────────────────────────────────────┘    │
│ 03    Structural Overlay                              [Hidden]  [Color: Amber]  [...]     │
└───────────────────────────────────────────────────────────────────────────────────────────┘
```

### 1.1 Architectural Invariants

1. **Fixed Viewport Footprint**: Takes zero grid cells. Composed full height or attached to left/right edge on Plane 2. Never floats free, never resizes, never dims Plane 0 canvas behind it.
2. **Stable `4ch` Label Alignment**: Layer labels (`01`, `02`, `B01`) are rendered in a fixed `4ch` monospaced column so all layer names align vertically across rows.
3. **Inline Confirmation**: Destructive removal operations open inline confirmations directly beneath the affected row, pushing lower rows down, maintaining focus without modal scrims.
4. **Current Layer Synchronization**: The active layer's row carries a `2px` `--signal-interaction` selection outline offset `3px` outside its bounds.

---

## 2. Component Anatomy & Design Tokens

| Part Name | Design System Token | Typography / Layout Metrics | Visual Appearance |
| :--- | :--- | :--- | :--- |
| **Frame** | `--surface-chrome` | `--r-sm` corner, `1px` `--k-slate-b` border | Opaque fill, `--sp-lg` padding |
| **Identity Header** | `--f-display` | Weight 500, `--t-title-small`, `--tr-label` | Uppercase text in `--k-slate` |
| **Close Control** | `--f-ui` | `--t-caption`, `--text-primary` | `1px` `--edge-quiet` border, `--r-sm` |
| **Stack Count** | `--f-mono` | Weight 500, `--t-label`, `--tr-label` | Uppercase `--text-meta` (e.g. `12 Layers`) |
| **Filter Field** | `--surface-nested` | `--f-ui`, `--t-dense`, `--sp-sm` padding | `1px` `--edge-quiet`, placeholder `Find a Layer` |
| **Label Token** | `--f-mono` | Fixed `4ch` column, `--t-label`, `--tr-mono` | Monospaced `--text-meta` (`B01`, `01`, `02`) |
| **Row Name** | `--f-ui` | Weight 400, `--t-dense`, `--lh-ui` | `--text-secondary` (active: `--text-primary`) |
| **Visibility Toggle**| `--k-layer` | `16x16px` icon button | Eye icon; dimmed when `IsVisible == false` |
| **Color Tint Swatch**| Layer Color Token | `12x12px` circular swatch (`--r-full`) | Renders assigned layer hue tint |
| **Inline Confirm** | `--surface-nested` | Full width beneath row | `1px` `--edge-hairline` (refusal: `--signal-refusal`)|

---

## 3. Layer Management Workflows & Control Rules

### 3.1 Creation & Reordering Mechanics

- **Add Above / Add Below**: `Add above` inserts a layer immediately above the targeted row. `Add below` inserts directly below. Adding above `01` mints positive labels (`02`, `03`...); adding below `01` mints `B`-prefixed labels (`B01`, `B02`...).
- **Move Up / Move Down**: Swaps row position with its immediate neighbor in the stack. Stable label tokens are **NEVER** renumbered during reorder.

### 3.2 Visibility Toggles (`Show/Hide Layer`)

Each row provides a visibility toggle button:
- `IsVisible = true`: Layer renders active content frames or presence heatmaps according to its activation state.
- `IsVisible = false`: Layer suppresses **ALL** rendering passes (both content frames and presence heatmaps/isolines).
- Keyboard shortcut: `Ctrl+H` / `Cmd+H` toggles visibility on the active layer.

### 3.3 Layer Color Tint Assignments

Layers support custom hue tinting to differentiate visual aura presence fields on Plane 0:
- Color swatches select from canonical design system hues: Slate Blue (`#4E6E9C`), Clay (`#B0524E`), Violet (`#6E62A6`), Amber (`#B08D4E`), Forest (`#4EB07B`).
- Assigning a color tint mutates the layer's `ColorTint` vector, which feeds directly into the `VerticalAuraPermeabilityEngine` energy blending calculations.

### 3.4 Layer Removal & Inline Confirm Flow

```
[User Clicks 'Remove' on Layer B02]
                │
                ▼
[Is Placements(B02) Empty?]
        │               │
  Yes ──┘               └── No
   │                         │
   ▼                         ▼
[Delete Layer B02]   [Open Inline Confirm Beneath B02 Row]
[Update Stack]       ["Remove Layer B02 and move what is on it to Layer 01?"]
                             │
              ┌──────────────┴──────────────┐
              ▼                             ▼
    [User Clicks 'Keep it']       [User Clicks 'Remove']
              │                             │
              ▼                             ▼
       [Close Confirm]            [Validate Cell Occupancy on Layer 01]
                                            │
                             ┌──────────────┴──────────────┐
                    No Collision                      Collision
                             │                             │
                             ▼                             ▼
                     [Migrate Placements]          [Transition Confirm to Refusal State]
                     [Delete Layer B02]            ["That space is occupied on Layer 01."]
```

---

## 4. Avalonia 11.2.5 Controls Architecture & Implementation

### 4.1 Avalonia ViewModel (`LayerManagerViewModel.cs`)

```csharp
namespace Grove.SpatialLayers.UI;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grove.SpatialLayers.Architecture;
using Grove.SpatialLayers.State;

public partial class LayerRowItemViewModel : ObservableObject
{
    public SpatialLayer Layer { get; }

    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private bool _isVisible;
    [ObservableProperty] private bool _isConfirmingRemoval;
    [ObservableProperty] private bool _isRefusalState;
    [ObservableProperty] private string? _refusalMessage;

    public LayerRowItemViewModel(SpatialLayer layer, bool isSelected)
    {
        Layer = layer;
        _isSelected = isSelected;
        _isVisible = layer.IsVisible;
    }
}

public partial class LayerManagerViewModel : ObservableObject
{
    private readonly ISpatialLayerStackManager _stackManager;
    private readonly ILayerActivationManager _activationManager;

    [ObservableProperty] private string _filterText = string.Empty;
    [ObservableProperty] private string _stackCountText = string.Empty;

    public ObservableCollection<LayerRowItemViewModel> LayerRows { get; } = new();

    public LayerManagerViewModel(
        ISpatialLayerStackManager stackManager, 
        ILayerActivationManager activationManager)
    {
        _stackManager = stackManager;
        _activationManager = activationManager;

        RefreshStack();
    }

    public void RefreshStack()
    {
        LayerRows.Clear();
        var stack = _stackManager.CurrentStack;

        foreach (var layer in stack.Layers.Reverse()) // Display top of stack first
        {
            bool isSelected = layer.Id == stack.ActiveLayerId;
            LayerRows.Add(new LayerRowItemViewModel(layer, isSelected));
        }

        UpdateStackCount();
    }

    [RelayCommand]
    private void SelectLayer(LayerRowItemViewModel row)
    {
        _activationManager.ActivateLayer(row.Layer.Id);
        RefreshStack();
    }

    [RelayCommand]
    private void ToggleVisibility(LayerRowItemViewModel row)
    {
        row.IsVisible = !row.IsVisible;
        // Mutate layer visibility state in ISpatialLayerStackManager
    }

    [RelayCommand]
    private void RequestRemoveLayer(LayerRowItemViewModel row)
    {
        if (row.Layer.Id == LayerId.BaseLayer) return; // Protected base layer

        var result = _stackManager.RemoveLayer(row.Layer.Id);
        if (result.Success)
        {
            RefreshStack();
        }
        else if (result.RefusalReason is not null)
        {
            row.IsConfirmingRemoval = true;
            row.IsRefusalState = true;
            row.RefusalMessage = result.RefusalReason;
        }
    }

    private void UpdateStackCount()
    {
        int total = _stackManager.CurrentStack.Layers.Count;
        int visible = LayerRows.Count;
        StackCountText = string.IsNullOrWhiteSpace(FilterText) 
            ? $"{total} LAYERS" 
            : $"{visible} OF {total} LAYERS";
    }
}
```

### 4.2 Avalonia XAML View (`LayerManagerOverlay.axaml`)

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:ui="clr-namespace:Grove.SpatialLayers.UI"
             x:Class="Grove.SpatialLayers.UI.LayerManagerOverlay"
             x:DataType="ui:LayerManagerViewModel"
             Width="320" HorizontalAlignment="Right" VerticalAlignment="Stretch">
    
    <Border Background="{DynamicResource SurfaceChromeBrush}"
            BorderBrush="{DynamicResource SlateBorderBrush}"
            BorderThickness="1"
            CornerRadius="2"
            Padding="16">
        <Grid RowDefinitions="Auto,Auto,Auto,*">
            
            <!-- Identity Header Row -->
            <Grid Grid.Row="0" ColumnDefinitions="*,Auto" Margin="0,0,0,16">
                <TextBlock Grid.Column="0" Text="LAYERS" 
                           FontFamily="{StaticResource DisplayFontFamily}" 
                           FontSize="14" FontWeight="Medium" 
                           Foreground="{DynamicResource SlateInkBrush}" />
                <Button Grid.Column="1" Content="Close" 
                        Classes="QuietButton" Command="{Binding CloseCommand}" />
            </Grid>
            
            <!-- Stack Count -->
            <TextBlock Grid.Row="1" Text="{Binding StackCountText}" 
                       FontFamily="{StaticResource MonoFontFamily}" 
                       FontSize="11" FontWeight="Medium" 
                       Foreground="{DynamicResource TextMetaBrush}" 
                       Margin="0,0,0,8" />
            
            <!-- Filter Field -->
            <TextBox Grid.Row="2" Text="{Binding FilterText, Mode=TwoWay}" 
                     Watermark="Find a Layer" 
                     Margin="0,0,0,12" />
            
            <!-- Stack List -->
            <ScrollViewer Grid.Row="3" VerticalScrollBarVisibility="Auto">
                <ItemsControl ItemsSource="{Binding LayerRows}">
                    <ItemsControl.ItemTemplate>
                        <DataTemplate DataType="ui:LayerRowItemViewModel">
                            <StackPanel Orientation="Vertical" Margin="0,2">
                                <Border Padding="8,4" CornerRadius="2"
                                        Background="{Binding IsSelected, Converter={StaticResource SelectionBackgroundConverter}}">
                                    <Grid ColumnDefinitions="4ch,*,Auto,Auto">
                                        <!-- Fixed 4ch Label Column -->
                                        <TextBlock Grid.Column="0" Text="{Binding Layer.Label.Value}" 
                                                   FontFamily="{StaticResource MonoFontFamily}" 
                                                   Foreground="{DynamicResource TextMetaBrush}" />
                                        
                                        <!-- Row Name -->
                                        <TextBlock Grid.Column="1" Text="{Binding Layer.Name}" 
                                                   TextWrapping="Wrap" MaxWidth="180" 
                                                   Foreground="{DynamicResource TextPrimaryBrush}" />
                                        
                                        <!-- Visibility Toggle -->
                                        <Button Grid.Column="2" Classes="IconButton" 
                                                Command="{Binding $parent[UserControl].((ui:LayerManagerViewModel)DataContext).ToggleVisibilityCommand}" 
                                                CommandParameter="{Binding}">
                                            <PathIcon Data="{StaticResource EyeIconGeometry}" />
                                        </Button>
                                    </Grid>
                                </Border>
                                
                                <!-- Inline Confirm Panel -->
                                <Border IsVisible="{Binding IsConfirmingRemoval}" 
                                        Background="{DynamicResource SurfaceNestedBrush}"
                                        BorderBrush="{Binding IsRefusalState, Converter={StaticResource RefusalBorderConverter}}"
                                        BorderThickness="1" Padding="12" Margin="0,4,0,0">
                                    <StackPanel Orientation="Vertical" Spacing="8">
                                        <TextBlock Text="{Binding RefusalMessage}" 
                                                   TextWrapping="Wrap" FontSize="12" />
                                        <StackPanel Orientation="Horizontal" Spacing="8">
                                            <Button Content="Keep it" Classes="QuietButton" />
                                            <Button Content="Remove" Classes="DestructiveButton" 
                                                    IsEnabled="{Binding !IsRefusalState}" />
                                        </StackPanel>
                                    </StackPanel>
                                </Border>
                            </StackPanel>
                        </DataTemplate>
                    </ItemsControl.ItemTemplate>
                </ItemsControl>
            </ScrollViewer>
        </Grid>
    </Border>
</UserControl>
```

---

## 5. Keyboard Navigation & Accessibility Specifications

- **List Roving Focus**: Arrow keys (`Up` / `Down`) cycle roving focus through row items. `Home` jumps to top of stack list; `End` jumps to bottom.
- **Escape Key Stack**:
  1. Closes open rename field (restores previous text).
  2. Closes open row context menu.
  3. Closes inline confirm (chooses `Keep it`).
  4. Clears non-empty filter field text.
  5. Closes the Layer Manager overlay.
- **WAI-ARIA Accessibility**:
  - Accessible Role: `region` with `aria-label="Layers"`.
  - Stack List Role: `listbox` with `aria-multiselectable="false"`.
  - Accessible Name: Combines label token and layer name (e.g., `"01 Base Field Anchor"`).

---

## 6. Architectural Traceability & References

- **Design System Component**: `docs/design-system/30-components/Layer-manager.md`
- **Plane 2 HUD Specs**: `docs/design-system/20-planes/HUD-plane.md`, [ADR-031](file:///C:/dev/grove-v9/docs/specs/hud-system/ADR-031-Slate-Window-System-And-Anatomy.md)
- **Related Specs**: [ADR-032](file:///C:/dev/grove-v9/docs/specs/hud-system/ADR-032-Spatial-Layer-Manager-And-Navigation.md), [ADR-040](file:///C:/dev/grove-v9/docs/specs/spatial-layers/ADR-040-Spatial-Layer-System-Architecture.md), [ADR-041](file:///C:/dev/grove-v9/docs/specs/spatial-layers/ADR-041-Vertical-Aura-Permeability-And-Attenuation.md), [ADR-042](file:///C:/dev/grove-v9/docs/specs/spatial-layers/ADR-042-Spatial-Layer-State-And-Activation.md)
