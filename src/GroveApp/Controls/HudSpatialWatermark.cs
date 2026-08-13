#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using GroveApp.DesignSystem;
using Colors = GroveApp.DesignSystem.Colors;

namespace GroveApp.Controls;

/// <summary>
/// The small state seam consumed by <see cref="HudSpatialWatermark"/>.
/// Implementations own camera/layer synchronization and the actual layer rename.
/// </summary>
public interface IHudSpatialWatermarkBinding
{
    HudCameraState CameraState { get; }

    HudLayerIdentity SelectedGridLayer { get; }

    event Action<HudCameraState>? CameraStateChanged;

    event Action<HudLayerIdentity>? SelectedGridLayerChanged;

    ValueTask<HudLayerRenameValidation> CommitLayerRenameAsync(
        HudLayerRenameRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Camera state required by the watermark. The rest of the camera transform stays hidden
/// behind the composition-root adapter.
/// </summary>
public readonly record struct HudCameraState(double ZoomScale)
{
    public static HudCameraState Default => new(1.0);

    public string FormattedZoomPercent => $"{Math.Round(Math.Max(0.0, ZoomScale) * 100.0):F0}%";
}

/// <summary>
/// Selected Grid Layer identity required by the watermark.
/// </summary>
public readonly record struct HudLayerIdentity(
    string LayerId,
    string LabelToken,
    string DisplayName,
    bool IsLocked = false);

public readonly record struct HudLayerRenameRequest(string LayerId, string ProposedName);

public readonly record struct HudLayerRenameValidation(
    bool IsAccepted,
    string SanitizedName,
    string? ErrorMessage = null)
{
    public static HudLayerRenameValidation Accepted(string name) =>
        new(true, name, null);

    public static HudLayerRenameValidation Refused(string message) =>
        new(false, string.Empty, message);
}

/// <summary>
/// Plane 2 spatial watermark. Its interface is intentionally narrow: callers push two
/// immutable snapshots through the binding seam, while this module owns formatting,
/// inline-edit state, and validation feedback.
/// </summary>
public sealed class HudSpatialWatermark : UserControl, IDisposable
{
    public const int MaximumLayerNameLength = 64;

    private readonly TextBlock _zoomReadout;
    private readonly TextBlock _layerLabel;
    private readonly TextBlock _layerName;
    private readonly StackPanel _layerPillContent;
    private readonly Border _layerPill;
    private readonly TextBox _renameField;

    private IHudSpatialWatermarkBinding? _binding;
    private HudCameraState _cameraState = HudCameraState.Default;
    private HudLayerIdentity _selectedGridLayer = new("grid-layer-01", "01", "Main Ground Layer");
    private bool _isRenaming;
    private bool _renameInFlight;

    public HudSpatialWatermark()
    {
        ZIndex = 300;
        HorizontalAlignment = HorizontalAlignment.Right;
        VerticalAlignment = VerticalAlignment.Bottom;
        Margin = new Thickness(0, 0, Tokens.SpaceMd, Tokens.SpaceMd);

        _zoomReadout = CreateTextBlock(
            HudCameraState.Default.FormattedZoomPercent,
            Typography.SizeMicro,
            Colors.TextMetaBrush,
            Typography.TrackingMono);

        _layerLabel = CreateTextBlock(
            string.Empty,
            Typography.SizeLabel,
            LayerFillBrush,
            Typography.TrackingMono);

        _layerName = CreateTextBlock(
            string.Empty,
            Typography.SizeDense,
            Colors.TextPrimaryBrush,
            0.0);
        _layerName.TextTrimming = TextTrimming.CharacterEllipsis;
        _layerName.MaxWidth = 240;
        _layerName.FontFamily = Typography.UiFamily;
        _layerName.FontWeight = Typography.WeightUiDefault;

        _renameField = new TextBox
        {
            MaxLength = MaximumLayerNameLength,
            MinWidth = 160,
            MaxWidth = 240,
            FontFamily = Typography.UiFamily,
            FontSize = Typography.SizeDense,
            Foreground = Colors.TextPrimaryBrush,
            Background = Colors.SurfaceNestedBrush,
            BorderBrush = Colors.EdgeFoundBrush,
            BorderThickness = new Thickness(Tokens.StrokeHairline),
            CornerRadius = Tokens.CornerRadiusSm,
            Padding = new Thickness(Tokens.SpaceXs, 0),
            IsVisible = false
        };

        _layerPillContent = CreateLayerPillContent();

        _layerPill = new Border
        {
            Background = Colors.SurfaceNestedBrush,
            BorderBrush = LayerBorderBrush,
            BorderThickness = new Thickness(Tokens.StrokeHairline),
            CornerRadius = Tokens.CornerRadiusSm,
            Padding = new Thickness(Tokens.SpaceXs, Tokens.SpaceXs / 2, Tokens.SpaceSm, Tokens.SpaceXs / 2),
            Focusable = true,
            Child = _layerPillContent
        };

        _layerPill.PointerPressed += OnLayerPillPointerPressed;
        _layerPill.DoubleTapped += OnLayerPillDoubleTapped;
        _layerPill.KeyDown += OnLayerPillKeyDown;
        _renameField.KeyDown += OnRenameFieldKeyDown;
        _renameField.LostFocus += OnRenameFieldLostFocus;

        var root = new Border
        {
            Background = Colors.SurfaceChromeBrush,
            BorderBrush = Colors.EdgeHairlineBrush,
            BorderThickness = new Thickness(Tokens.StrokeHairline),
            CornerRadius = Tokens.CornerRadiusSm,
            Padding = new Thickness(Tokens.SpaceXs, Tokens.SpaceXs, Tokens.SpaceSm, Tokens.SpaceXs),
            Child = CreateRootContent()
        };

        Content = root;
        UpdateCameraState(_cameraState);
        UpdateSelectedGridLayer(_selectedGridLayer);
    }

    public event Action<HudLayerRenameRequest>? RenameCommitted;

    public event Action<string>? RenameRefused;

    public HudCameraState CameraState => _cameraState;

    public HudLayerIdentity SelectedGridLayer => _selectedGridLayer;

    public bool IsRenaming => _isRenaming;

    public string? RenameErrorMessage { get; private set; }

    /// <summary>
    /// Attaches the one external adapter at the composition seam. Rebinding unsubscribes
    /// the previous adapter, keeping ownership and locality explicit.
    /// </summary>
    public void Bind(IHudSpatialWatermarkBinding? binding)
    {
        if (_binding is not null)
        {
            _binding.CameraStateChanged -= OnBindingCameraStateChanged;
            _binding.SelectedGridLayerChanged -= OnBindingSelectedGridLayerChanged;
        }

        _binding = binding;
        if (_binding is null)
        {
            return;
        }

        UpdateCameraState(_binding.CameraState);
        UpdateSelectedGridLayer(_binding.SelectedGridLayer);
        _binding.CameraStateChanged += OnBindingCameraStateChanged;
        _binding.SelectedGridLayerChanged += OnBindingSelectedGridLayerChanged;
    }

    public void UpdateCameraState(HudCameraState state)
    {
        _cameraState = state;
        _zoomReadout.Text = state.FormattedZoomPercent;
    }

    public void UpdateSelectedGridLayer(HudLayerIdentity identity)
    {
        _selectedGridLayer = identity;
        _layerLabel.Text = identity.LabelToken;
        _layerName.Text = identity.DisplayName;

        if (_isRenaming)
        {
            CancelInlineRename();
        }
    }

    public void BeginInlineRename()
    {
        if (_isRenaming || _selectedGridLayer.IsLocked)
        {
            return;
        }

        _isRenaming = true;
        RenameErrorMessage = null;
        _renameField.Text = _selectedGridLayer.DisplayName;
        _renameField.BorderBrush = Colors.EdgeFoundBrush;
        _renameField.IsVisible = true;
        _layerLabel.IsVisible = false;
        _layerName.IsVisible = false;
        _layerPill.Child = _renameField;

        Dispatcher.UIThread.Post(() =>
        {
            if (!_isRenaming)
            {
                return;
            }

            _renameField.Focus();
            _renameField.SelectAll();
        });
    }

    public void CancelInlineRename()
    {
        _renameInFlight = false;
        _isRenaming = false;
        RenameErrorMessage = null;
        _renameField.IsVisible = false;
        _layerLabel.IsVisible = true;
        _layerName.IsVisible = true;
        _layerPill.Child = _layerPillContent;
    }

    public void Dispose()
    {
        Bind(null);
        _layerPill.PointerPressed -= OnLayerPillPointerPressed;
        _layerPill.DoubleTapped -= OnLayerPillDoubleTapped;
        _layerPill.KeyDown -= OnLayerPillKeyDown;
        _renameField.KeyDown -= OnRenameFieldKeyDown;
        _renameField.LostFocus -= OnRenameFieldLostFocus;
    }

    private StackPanel CreateRootContent()
    {
        var rootContent = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = Tokens.SpaceXs,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        var brand = CreateTextBlock(
            "GROVE v9",
            Typography.SizeMicro,
            Colors.TextUnavailableBrush,
            Typography.TrackingWide);
        brand.IsHitTestVisible = false;

        var separator = CreateTextBlock("·", Typography.SizeMicro, Colors.EdgeHairlineBrush, 0.0);
        separator.IsHitTestVisible = false;

        _zoomReadout.IsHitTestVisible = false;

        rootContent.Children.Add(brand);
        rootContent.Children.Add(_zoomReadout);
        rootContent.Children.Add(separator);
        rootContent.Children.Add(_layerPill);
        return rootContent;
    }

    private StackPanel CreateLayerPillContent()
    {
        var content = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = Tokens.SpaceXs
        };

        content.Children.Add(_layerLabel);
        content.Children.Add(_layerName);
        return content;
    }

    private static TextBlock CreateTextBlock(
        string text,
        double fontSize,
        IBrush foreground,
        double letterSpacing)
    {
        return new TextBlock
        {
            Text = text,
            FontFamily = Typography.MonoFamily,
            FontSize = fontSize,
            FontWeight = Typography.WeightMono,
            Foreground = foreground,
            LetterSpacing = letterSpacing,
            VerticalAlignment = VerticalAlignment.Center
        };
    }

    private void OnLayerPillPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(_layerPill).Properties.IsLeftButtonPressed)
        {
            _layerPill.Focus();
        }
    }

    private void OnLayerPillDoubleTapped(object? sender, TappedEventArgs e)
    {
        BeginInlineRename();
        e.Handled = true;
    }

    private void OnLayerPillKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.F2)
        {
            BeginInlineRename();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape && _isRenaming)
        {
            CancelInlineRename();
            e.Handled = true;
        }
    }

    private async void OnRenameFieldKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            CancelInlineRename();
            e.Handled = true;
        }
        else if (e.Key == Key.Enter)
        {
            await CommitInlineRenameAsync();
            e.Handled = true;
        }
    }

    private async void OnRenameFieldLostFocus(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await CommitInlineRenameAsync();
    }

    private async Task CommitInlineRenameAsync()
    {
        if (!_isRenaming || _renameInFlight)
        {
            return;
        }

        var proposedName = (_renameField.Text ?? string.Empty).Trim();
        if (proposedName.Length == 0)
        {
            RefuseRename("A layer name is required.");
            return;
        }

        if (proposedName.Length > MaximumLayerNameLength)
        {
            RefuseRename($"Layer names are limited to {MaximumLayerNameLength} characters.");
            return;
        }

        _renameInFlight = true;
        var request = new HudLayerRenameRequest(_selectedGridLayer.LayerId, proposedName);
        HudLayerRenameValidation result;

        try
        {
            result = _binding is null
                ? HudLayerRenameValidation.Accepted(proposedName)
                : await _binding.CommitLayerRenameAsync(request);
        }
        catch (Exception exception)
        {
            result = HudLayerRenameValidation.Refused(exception.Message);
        }

        _renameInFlight = false;
        if (!result.IsAccepted)
        {
            RefuseRename(result.ErrorMessage ?? "Layer rename was refused.");
            return;
        }

        var committedName = string.IsNullOrWhiteSpace(result.SanitizedName)
            ? proposedName
            : result.SanitizedName.Trim();

        _selectedGridLayer = _selectedGridLayer with { DisplayName = committedName };
        _layerName.Text = committedName;
        RenameCommitted?.Invoke(request with { ProposedName = committedName });
        CancelInlineRename();
    }

    private void RefuseRename(string message)
    {
        RenameErrorMessage = message;
        _renameField.BorderBrush = Colors.SignalRefusalBrush;
        ToolTip.SetTip(_renameField, message);
        RenameRefused?.Invoke(message);
        _renameField.Focus();
        _renameField.SelectAll();
    }

    private void OnBindingCameraStateChanged(HudCameraState state)
    {
        Dispatcher.UIThread.Post(() => UpdateCameraState(state));
    }

    private void OnBindingSelectedGridLayerChanged(HudLayerIdentity identity)
    {
        Dispatcher.UIThread.Post(() => UpdateSelectedGridLayer(identity));
    }

    private static IBrush LayerFillBrush =>
        new SolidColorBrush(Color.Parse(Colors.LayerFillHex));

    private static IBrush LayerBorderBrush =>
        new SolidColorBrush(Color.Parse(Colors.LayerBorderHex));
}
