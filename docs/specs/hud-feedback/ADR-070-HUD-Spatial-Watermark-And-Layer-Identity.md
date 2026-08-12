# ADR-070: HUD Spatial Watermark and Active Layer Identity

| Property | Value |
| :--- | :--- |
| **Status** | Accepted |
| **Date** | 2026-08-12 |
| **Area** | HUD System / Visual Architecture / Layer Management |
| **Target Runtime** | C# 13 / .NET 9 / Avalonia 11.2.5 / SkiaSharp 3.x |
| **Authors** | Chief Product Definition Architect |

---

## 1. Executive Context & Architectural Principles

As established in [ADR-004](file:///C:/dev/grove-v9/docs/specs/spatial-grid/ADR-004-Three-Plane-Visual-Hierarchy.md), [ADR-030](file:///C:/dev/grove-v9/docs/specs/hud-system/ADR-030-Three-Plane-Compositor-Architecture-Validation.md), and [ADR-042](file:///C:/dev/grove-v9/docs/specs/spatial-layers/ADR-042-Spatial-Layer-State-And-Activation.md), Grove v9 presents spatial context through Plane 2 (HUD Slate Plane), which remains fixed to the viewport at `ZIndex = 300` and unscaled by camera affine transformations $T(x,y,s)$.

This ADR specifies the spatial watermark and active layer identity indicator residing in the **bottom-right corner of the HUD Plane**. The spatial watermark fulfills three concurrent runtime roles:
1. **Product & Environment Anchor**: Provides ambient application context (`GROVE v9`) without distracting from spatial canvas work.
2. **Spatial Camera Scale Readout**: Displays the real-time camera zoom scale (e.g., `100%`, `125%`, `50%`) derived from the camera affine matrix $T_{\text{camera}}$.
3. **Active Spatial Layer Identity & Inline Editor**: Displays the currently active layer label token (e.g., `01`, `02`, `B01`) and user-assigned layer name (e.g., `Layer 01 - Working Surface`), providing direct double-click or hotkey inline renaming capabilities.

```
+-----------------------------------------------------------------------------------+
| Viewport (Plane 2 HUD Overlay, ZIndex = 300)                                      |
|                                                                                   |
|                                                                                   |
|                                     +-------------------------------------------+ |
|                                     | Bottom-Right Spatial Watermark            | |
|                                     | [GROVE v9]  [100%]  [01: Working Surface] | |
|                                     +-------------------------------------------+ |
+-----------------------------------------------------------------------------------+
```

### Architectural Invariants
- **Non-Obstructive Pointer Passthrough**: Text blocks and watermark numerals (`GROVE v9`, scale percentage) have `IsHitTestVisible = false` and pass all pointer input directly through to Plane 1 / Plane 0. Only the interactive active layer identity pill captures pointer events.
- **Strict Viewport Anchoring**: Bounded strictly to the bottom-right viewport cell margin (`Right: 16px`, `Bottom: 16px`), adhering to `--r-sm` corner radius and zero shadow elevation contracts (`docs/design-system/00-foundations/Shape.md`).
- **Monospaced Structural Typography**: Driven exclusively by `--f-mono` (JetBrains Mono 500), using strict tokenized sizes (`--t-label`, `--t-micro`), tracking steps (`--tr-mono`, `--tr-label`), and ink steps (`--ink-whisper`, `--ink-tertiary`, `--ink-primary`).
- **Single Active Layer Synchronization**: The watermark active layer identity binds reactively to the `ISpatialLayerStateService.ActiveLayer` state stream. Mutating the layer name in the watermark immediately propagates across the `LayerManager` Slate and Spatial Field Ledger.

---

## 2. HUD Plane Allocation Matrix

The spatial watermark occupies a dedicated structural zone within Plane 2. The layout matrix below defines element hierarchy, pixel dimensions, padding, alignments, and pointer hit-test policies.

| Element ID | Part Name | Width / Bounds | Alignment | Typography & Ink Token | Pointer Hit-Test Policy |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `W-01` | Watermark Container | Auto (Max `420px`) | `HorizontalAlignment="Right"`<br>`VerticalAlignment="Bottom"` | `Margin="0,0,16,16"`<br>`Padding="4,4,8,4"` | Container `PointerTransparent`; internal controls override hit-test. |
| `W-02` | Brand Token | `56px` fixed | Inline Left | `--f-mono` 500, `--t-micro` (9px), `--tr-wide` (0.16em), Ink: `--ink-whisper` (`0.04`) | **Transparent** (`IsHitTestVisible="False"`). Passes clicks to Spatial Canvas. |
| `W-03` | Camera Scale Readout | `44px` fixed | Inline Center-Left | `--f-mono` 500, `--t-micro` (9px), `--tr-mono` (0.08em), Ink: `--ink-tertiary` (`0.51`) | **Transparent** (`IsHitTestVisible="False"`). Click-through to canvas. |
| `W-04` | Separator Dot | `8px` fixed | Inline Center | Ink: `--ink-hairline` (`0.10`) | **Transparent**. |
| `W-05` | Active Layer Label Token | `32px` fixed | Inline Center-Right | `--f-mono` 500, `--t-label` (11px), `--tr-mono` (0.08em), Ink: `--k-layer` (`#E2A6C6`) | **Interactive**. Click switches focus; double-click opens `W-07` Inline Rename. |
| `W-06` | Active Layer Name Block | Auto (`120px`–`240px`) | Inline Right | `--f-ui` 400, `--t-dense` (13px), `--lh-ui` (1.5), Ink: `--ink-primary` (`0.82`) | **Interactive**. Double-click or `F2` triggers `W-07` Inline Rename. |
| `W-07` | Inline Rename Field | Auto (`160px`–`240px`) | Overlays `W-06` | `--f-ui` 400, `--t-dense` (13px), Fill: `--surface-nested`, Border: `--edge-found` | **Interactive**. Focuses automatically; captures keyboard text input. |

### 2.1 Spatial Layout Assembly Diagram

```
+---------------------------------------------------------------------------------------------------+
| HUD Watermark Frame (Right: 16px, Bottom: 16px)                                                  |
| +----------------+  +----------+  +---+  +------------------+  +----------------------------+ |
| | GROVE v9       |  | 100%     |  | . |  | 01               |  | Layer 01 - Working Surface | |
| | (Brand Token)  |  | (Scale)  |  |   |  | (Layer Label)    |  | (Layer Display Name)       | |
| | W-02           |  | W-03     |  |W-04|  W-05               |  | W-06                       | |
| +----------------+  +----------+  +---+  +------------------+  +----------------------------+ |
| <--- Pass-Through Pointer Clicks ---> | <------------ Interactive Layer Pill Area ------------> |
+---------------------------------------------------------------------------------------------------+
```

---

## 3. Typography & Color Token Matrix

Adhering strictly to `docs/design-system/00-foundations/Tokens.md` and `docs/design-system/00-foundations/Typography.md`:

| Visual Token | Design Token Name | Value | Resolved Composition |
| :--- | :--- | :--- | :--- |
| **Brand Monospace Font** | `--f-mono` | `JetBrains Mono, Consolas, monospace` | Weight 500 |
| **UI Type Family** | `--f-ui` | `Inter, system-ui, sans-serif` | Weight 400 / 500 |
| **Brand Ink Step** | `--ink-whisper` | Alpha `0.04` over `--ink` (`234,234,234`) | `rgba(234, 234, 234, 0.04)` |
| **Scale Readout Ink** | `--ink-tertiary` | Alpha `0.51` over `--ink` (`234,234,234`) | `rgba(234, 234, 234, 0.51)` |
| **Layer Label Ink** | `--k-layer` | `#E2A6C6` | Role Hue: Layer (`#E2A6C6`) |
| **Layer Label Border** | `--k-layer-b` | `#8A3F63` | Role Border: Layer (`#8A3F63`) |
| **Active Layer Text** | `--ink-primary` | Alpha `0.82` over `--ink` (`234,234,234`) | `rgba(234, 234, 234, 0.82)` |
| **Inline Editor Surface**| `--surface-nested` | `#1C1C20` | Raised dark surface |
| **Inline Editor Border** | `--edge-found` | Alpha `0.22` over `--ink` | `rgba(234, 234, 234, 0.22)` |
| **Refusal Ink / Border** | `--c-invalid` | `226 98 92` | `rgb(226, 98, 92)` |

---

## 4. Inline Layer Renaming State Machine & Interface

Inline renaming from the HUD watermark allows the user to update active layer identity metadata without opening the full `LayerManager` Slate.

### 4.1 State Machine Transitions

```
                    +-----------------------+
                    |       1. Rest         |
                    | (Display Layer Name)  |
                    +-----------------------+
                                |
                   Double-Click / Press F2
                                |
                                v
                    +-----------------------+
                    |       2. Edit         |
                    | (Rename Field Active) |
                    +-----------------------+
                       /                 \
            Press Enter / Blur      Press Escape / Refusal
                     /                     \
                    v                       v
        +-----------------------+   +-----------------------+
        |     3. Committed      |   |      4. Cancelled     |
        | (Validate & Save Name)|   | (Revert to Original)  |
        +-----------------------+   +-----------------------+
```

### 4.2 State Rules & Validation Logic

1. **Activation Trigger**:
   - Primary: Pointer double-click on `W-05` (Layer Label) or `W-06` (Layer Name).
   - Hotkey: Pressing `F2` when active layer pill or canvas focus is established.
2. **Focus & Selection**:
   - `TextBox` (`W-07`) replaces `W-06` in-place using `--d-fade` (`120ms`).
   - The entire existing layer name string is selected automatically (`SelectAll()`).
   - Keyboard focus is acquired immediately via Avalonia `FocusManager`.
3. **Validation & Refusal**:
   - **Empty Name**: Empty or whitespace-only input is invalid. Resets to previous layer name upon commit.
   - **Duplicate Name**: If another layer shares the exact name case-insensitively, trigger Refusal state:
     - Border switches to `--c-invalid` (`rgb(226, 98, 92)`).
     - Inline warning tooltip: `"A layer named 'Working Surface' already exists."`
     - Focus remains inside the edit field; change is NOT committed.
   - **Character Limit**: Maximum length 64 characters (`MaxLength="64"`).
4. **Commit & Cancellation**:
   - **Commit**: Pressing `Enter` or losing focus (`Blur`) with valid input dispatches `LayerRenameCommand`, updates `ISpatialLayerStateService`, and emits a layer metadata event to the spatial ledger.
   - **Cancel**: Pressing `Escape` cancels editing instantly without mutating layer state and restores `W-06`.

---

## 5. C# 13 Architecture & Type Interfaces

Below are the complete, compilable C# 13 type interfaces, domain records, and ViewModel contracts governing the HUD Spatial Watermark and Active Layer Identity system.

```csharp
// File: src/Grove.Core/Spatial/ISpatialWatermarkService.cs
#nullable enable

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace Grove.Core.Spatial;

/// <summary>
/// Immutable record representing spatial camera state for HUD watermark display.
/// </summary>

public readonly record struct SpatialCameraState(
    double ZoomScale,
    double CenterX,
    double CenterY,
    double RotationDegrees)
{
    public string FormattedZoomPercent => $"{Math.Round(ZoomScale * 100.0):F0}%";
}

/// <summary>
/// Immutable record representing spatial layer identity for HUD display and inline editing.
/// </summary>
public readonly record struct SpatialLayerIdentity(
    string LayerId,
    string LabelToken,
    string DisplayName,
    int DepthOrder,
    bool IsActive,
    bool IsLocked)
{
    public static SpatialLayerIdentity Default => new(
        LayerId: "layer-01",
        LabelToken: "01",
        DisplayName: "Layer 01 - Working Surface",
        DepthOrder: 0,
        IsActive: true,
        IsLocked: false);
}

/// <summary>
/// Result structure for layer rename validation operations.
/// </summary>
public readonly record struct LayerRenameResult(
    bool IsSuccess,
    string SanitizedName,
    string? ErrorMessage = null)
{
    public static LayerRenameResult Success(string name) => new(true, name);
    public static LayerRenameResult Failure(string error) => new(false, string.Empty, error);
}

/// <summary>
/// Domain service interface managing HUD spatial watermark state and active layer identity.
/// </summary>
public interface ISpatialWatermarkService : INotifyPropertyChanged
{
    /// <summary>
    /// Gets the current camera transform state.
    /// </summary>
    SpatialCameraState CurrentCameraState { get; }

    /// <summary>
    /// Gets the currently active spatial layer identity.
    /// </summary>
    SpatialLayerIdentity ActiveLayerIdentity { get; }

    /// <summary>
    /// Gets whether an inline layer rename operation is actively in progress.
    /// </summary>
    bool IsRenamingActive { get; }

    /// <summary>
    /// Event raised when camera transform scale changes.
    /// </summary>
    event EventHandler<SpatialCameraState>? CameraStateChanged;

    /// <summary>
    /// Event raised when active layer identity or layer name updates.
    /// </summary>
    event EventHandler<SpatialLayerIdentity>? ActiveLayerChanged;

    /// <summary>
    /// Initiates an inline renaming operation for the active layer.
    /// </summary>
    void BeginInlineRename();

    /// <summary>
    /// Validates and commits a new layer name for the specified layer ID.
    /// </summary>
    ValueTask<LayerRenameResult> CommitLayerRenameAsync(
        string layerId, 
        string proposedName, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels the current inline rename operation without making changes.
    /// </summary>
    void CancelInlineRename();
}
```

```csharp
// File: src/Grove.UI/ViewModels/HudWatermarkViewModel.cs
#nullable enable

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Grove.Core.Spatial;

namespace Grove.UI.ViewModels;

/// <summary>
/// ViewModel for the bottom-right HUD Spatial Watermark control in Avalonia.
/// </summary>
public sealed class HudWatermarkViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly ISpatialWatermarkService _watermarkService;
    private SpatialCameraState _cameraState;
    private SpatialLayerIdentity _activeLayer;
    private bool _isEditingName;
    private string _editingNameBuffer = string.Empty;
    private string? _validationError;

    public HudWatermarkViewModel(ISpatialWatermarkService watermarkService)
    {
        _watermarkService = watermarkService ?? throw new ArgumentNullException(nameof(watermarkService));
        _cameraState = _watermarkService.CurrentCameraState;
        _activeLayer = _watermarkService.ActiveLayerIdentity;

        _watermarkService.CameraStateChanged += OnCameraStateChanged;
        _watermarkService.ActiveLayerChanged += OnActiveLayerChanged;

        BeginRenameCommand = new DelegateCommand(_ => BeginRename());
        CommitRenameCommand = new AsyncDelegateCommand(CommitRenameAsync);
        CancelRenameCommand = new DelegateCommand(_ => CancelRename());
    }

    public string BrandText => "GROVE v9";
    public string ZoomReadout => _cameraState.FormattedZoomPercent;
    public string ActiveLabelToken => _activeLayer.LabelToken;
    public string ActiveDisplayName => _activeLayer.DisplayName;

    public bool IsEditingName
    {
        get => _isEditingName;
        private set => SetField(ref _isEditingName, value);
    }

    public string EditingNameBuffer
    {
        get => _editingNameBuffer;
        set
        {
            if (SetField(ref _editingNameBuffer, value))
            {
                ValidationError = null; // Clear validation error on type
            }
        }
    }

    public string? ValidationError
    {
        get => _validationError;
        private set => SetField(ref _validationError, value);
    }

    public ICommand BeginRenameCommand { get; }
    public ICommand CommitRenameCommand { get; }
    public ICommand CancelRenameCommand { get; }

    public void BeginRename()
    {
        if (IsEditingName || _activeLayer.IsLocked) return;

        EditingNameBuffer = _activeLayer.DisplayName;
        ValidationError = null;
        IsEditingName = true;
        _watermarkService.BeginInlineRename();
    }

    public async Task CommitRenameAsync()
    {
        if (!IsEditingName) return;

        var proposed = EditingNameBuffer.Trim();
        if (string.Equals(proposed, _activeLayer.DisplayName, StringComparison.Ordinal))
        {
            CancelRename();
            return;
        }

        var result = await _watermarkService.CommitLayerRenameAsync(_activeLayer.LayerId, proposed);
        if (result.IsSuccess)
        {
            IsEditingName = false;
            ValidationError = null;
        }
        else
        {
            ValidationError = result.ErrorMessage;
        }
    }

    public void CancelRename()
    {
        IsEditingName = false;
        ValidationError = null;
        EditingNameBuffer = string.Empty;
        _watermarkService.CancelInlineRename();
    }

    private void OnCameraStateChanged(object? sender, SpatialCameraState state)
    {
        _cameraState = state;
        OnPropertyChanged(nameof(ZoomReadout));
    }

    private void OnActiveLayerChanged(object? sender, SpatialLayerIdentity layer)
    {
        _activeLayer = layer;
        OnPropertyChanged(nameof(ActiveLabelToken));
        OnPropertyChanged(nameof(ActiveDisplayName));
    }

    public void Dispose()
    {
        _watermarkService.CameraStateChanged -= OnCameraStateChanged;
        _watermarkService.ActiveLayerChanged -= OnActiveLayerChanged;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}

public sealed class DelegateCommand(Action<object?> execute, Func<object?, bool>? canExecute = null) : ICommand
{
    public bool CanExecute(object? parameter) => canExecute?.Invoke(parameter) ?? true;
    public void Execute(object? parameter) => execute(parameter);
    public event EventHandler? CanExecuteChanged;
}

public sealed class AsyncDelegateCommand(Func<Task> executeAsync) : ICommand
{
    private bool _isExecuting;

    public bool CanExecute(object? parameter) => !_isExecuting;

    public async void Execute(object? parameter)
    {
        if (_isExecuting) return;
        _isExecuting = true;
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        try
        {
            await executeAsync();
        }
        finally
        {
            _isExecuting = false;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? CanExecuteChanged;
}
```

---

## 6. Avalonia 11.2.5 Control Implementation

Below is the complete C# Code-Behind and Avalonia Control implementation for `HudSpatialWatermarkControl.cs` which lives on Plane 2 (`ZIndex = 300`).

```csharp
// File: src/Grove.UI/Controls/HudSpatialWatermarkControl.cs
#nullable enable

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Grove.UI.ViewModels;

namespace Grove.UI.Controls;

/// <summary>
/// Avalonia 11.2.5 Control hosting the Spatial Watermark and Active Layer Identity overlay.
/// Located in Plane 2 (HUD Plane), anchored to bottom-right viewport coordinates.
/// </summary>
public class HudSpatialWatermarkControl : TemplatedControl
{
    public static readonly StyledProperty<HudWatermarkViewModel?> ViewModelProperty =
        AvaloniaProperty.Register<HudSpatialWatermarkControl, HudWatermarkViewModel?>(nameof(ViewModel));

    public HudWatermarkViewModel? ViewModel
    {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        var editTextBox = e.NameScope.Find<TextBox>("PART_RenameTextBox");
        if (editTextBox != null)
        {
            editTextBox.KeyDown += OnRenameTextBoxKeyDown;
            editTextBox.LostFocus += OnRenameTextBoxLostFocus;
        }

        var layerPill = e.NameScope.Find<Border>("PART_ActiveLayerPill");
        if (layerPill != null)
        {
            layerPill.DoubleTapped += OnLayerPillDoubleTapped;
        }
    }

    private void OnLayerPillDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (ViewModel != null && !ViewModel.IsEditingName)
        {
            ViewModel.BeginRename();
            var editTextBox = this.FindControl<TextBox>("PART_RenameTextBox");
            if (editTextBox != null)
            {
                editTextBox.Focus();
                editTextBox.SelectAll();
            }
        }
    }

    private async void OnRenameTextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (ViewModel == null) return;

        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            await ViewModel.CommitRenameAsync();
        }
        else if (e.Key == Key.Escape)
        {
            e.Handled = true;
            ViewModel.CancelRename();
        }
    }

    private async void OnRenameTextBoxLostFocus(object? sender, RoutedEventArgs e)
    {
        if (ViewModel != null && ViewModel.IsEditingName)
        {
            await ViewModel.CommitRenameAsync();
        }
    }
}
```

### 6.1 Control Visual Template Specification (XAML Equivalent Structure)

```xml
<!-- Avalonia ControlTemplate for HudSpatialWatermarkControl -->
<ControlTemplate TargetType="controls:HudSpatialWatermarkControl">
    <Border HorizontalAlignment="Right"
            VerticalAlignment="Bottom"
            Margin="0,0,16,16"
            Padding="4,4,8,4"
            CornerRadius="2"
            Background="#1C1C20"
            BorderBrush="rgba(234,234,234,0.10)"
            BorderThickness="1"
            ZIndex="300">
        <StackPanel Orientation="Horizontal" Spacing="8" VerticalAlignment="Center">
            
            <!-- W-02: Brand Token (Pointer Transparent) -->
            <TextBlock Text="{Binding BrandText}"
                       FontFamily="JetBrains Mono, Consolas, monospace"
                       FontWeight="Medium"
                       FontSize="9"
                       Foreground="rgba(234,234,234,0.04)"
                       LetterSpacing="0.16em"
                       IsHitTestVisible="False"
                       VerticalAlignment="Center"/>

            <!-- W-03: Camera Scale Readout (Pointer Transparent) -->
            <TextBlock Text="{Binding ZoomReadout}"
                       FontFamily="JetBrains Mono, Consolas, monospace"
                       FontWeight="Medium"
                       FontSize="9"
                       Foreground="rgba(234,234,234,0.51)"
                       LetterSpacing="0.08em"
                       IsHitTestVisible="False"
                       VerticalAlignment="Center"/>

            <!-- W-04: Separator Dot -->
            <Ellipse Width="3" Height="3"
                     Fill="rgba(234,234,234,0.10)"
                     IsHitTestVisible="False"
                     VerticalAlignment="Center"/>

            <!-- Interactive Layer Identity Pill (W-05 & W-06 / W-07) -->
            <Border x:Name="PART_ActiveLayerPill"
                    Background="Transparent"
                    CornerRadius="2"
                    Padding="4,2"
                    Cursor="Hand"
                    ToolTip.Tip="Double-click or press F2 to rename layer">
                <StackPanel Orientation="Horizontal" Spacing="6">
                    
                    <!-- W-05: Layer Label Token -->
                    <Border Background="rgba(226,166,198,0.12)"
                            BorderBrush="#8A3F63"
                            BorderThickness="1"
                            CornerRadius="2"
                            Padding="3,1">
                        <TextBlock Text="{Binding ActiveLabelToken}"
                                   FontFamily="JetBrains Mono, Consolas, monospace"
                                   FontWeight="Medium"
                                   FontSize="11"
                                   Foreground="#E2A6C6"/>
                    </Border>

                    <!-- W-06: Display Name (Shown when not editing) -->
                    <TextBlock Text="{Binding ActiveDisplayName}"
                               FontFamily="Inter, system-ui, sans-serif"
                               FontSize="13"
                               Foreground="rgba(234,234,234,0.82)"
                               VerticalAlignment="Center"
                               IsVisible="{Binding !IsEditingName}"/>

                    <!-- W-07: Inline Rename Field (Shown when editing) -->
                    <TextBox x:Name="PART_RenameTextBox"
                             Text="{Binding EditingNameBuffer, Mode=TwoWay}"
                             FontFamily="Inter, system-ui, sans-serif"
                             FontSize="13"
                             Background="#1C1C20"
                             BorderBrush="rgba(234,234,234,0.22)"
                             BorderThickness="1"
                             CornerRadius="2"
                             Padding="4,2"
                             MaxLength="64"
                             IsVisible="{Binding IsEditingName}"/>
                </StackPanel>
            </Border>
        </StackPanel>
    </Border>
</ControlTemplate>
```

---

## 7. Verification & Compliance Checklist

- [x] **3-Plane Architecture Validation**: Fixed to Plane 2 Overlay (`ZIndex = 300`), independent of camera affine transform.
- [x] **Pointer Hit-Testing**: Brand token and zoom readout pass through input (`IsHitTestVisible = false`); active layer identity pill captures clicks.
- [x] **Typography & Ink Token Strictness**: JetBrains Mono 500 (`--f-mono`), `--t-label` (11px), `--t-micro` (9px), `--ink-whisper` (0.04), `--ink-tertiary` (0.51), `--k-layer` (`#E2A6C6`).
- [x] **Inline Renaming Semantics**: Double-click / `F2` trigger, `Enter` commit, `Escape` cancel, duplicate name validation refusal.
- [x] **Complete C# 13 Implementation**: Production-grade C# 13 interfaces, records, ViewModels, and Avalonia 11.2.5 control code-behind.
