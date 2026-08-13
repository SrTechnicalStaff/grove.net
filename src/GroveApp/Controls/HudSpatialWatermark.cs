using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using GroveApp.DesignSystem;
using GroveColors = GroveApp.DesignSystem.Colors;

namespace GroveApp.Controls;

public interface IHudSpatialWatermarkBinding
{
    HudGridLayerIdentity SelectedGridLayer { get; }

    event Action<HudGridLayerIdentity>? SelectedGridLayerChanged;
}

public readonly record struct HudGridLayerIdentity(
    string LayerId,
    string LabelToken,
    string DisplayName);

public sealed class HudSpatialWatermark : UserControl, IDisposable
{
    private readonly TextBlock _gridLayerName;
    private readonly TextBlock _gridLayerNumber;
    private IHudSpatialWatermarkBinding? _binding;

    public HudSpatialWatermark()
    {
        ZIndex = 300;
        HorizontalAlignment = HorizontalAlignment.Right;
        VerticalAlignment = VerticalAlignment.Bottom;
        Margin = new Thickness(0, 0, Tokens.SpaceXl, Tokens.SpaceMd);
        IsHitTestVisible = false;
        Focusable = false;

        _gridLayerName = new TextBlock
        {
            FontFamily = Typography.DisplayFamily,
            FontSize = Typography.SizeBody,
            FontWeight = Typography.WeightDisplay,
            Foreground = GroveColors.EdgeQuietBrush,
            LetterSpacing = Typography.TrackingCaps,
            LineHeight = Typography.SizeBody * Typography.LineHeightTight,
            HorizontalAlignment = HorizontalAlignment.Right,
            TextAlignment = TextAlignment.Right,
            TextWrapping = TextWrapping.NoWrap,
            Margin = new Thickness(0, 0, Tokens.WatermarkNameOpticalInset, Tokens.SpaceXs)
        };

        _gridLayerNumber = new TextBlock
        {
            FontFamily = Typography.DisplayFamily,
            FontSize = Tokens.WatermarkNumeralSize,
            FontWeight = Typography.WeightDisplay,
            Foreground = GroveColors.WatermarkNumeralBrush,
            LetterSpacing = Typography.TrackingTitle,
            LineHeight = Tokens.WatermarkNumeralSize * Tokens.WatermarkLineHeight,
            HorizontalAlignment = HorizontalAlignment.Right,
            TextAlignment = TextAlignment.Right,
            TextWrapping = TextWrapping.NoWrap
        };

        Content = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Right,
            Children =
            {
                _gridLayerName,
                _gridLayerNumber
            }
        };

        UpdateSelectedGridLayer(new HudGridLayerIdentity("grid-layer-01", "01", "Main Ground Layer"));
    }

    public void Bind(IHudSpatialWatermarkBinding? binding)
    {
        if (_binding is not null)
        {
            _binding.SelectedGridLayerChanged -= OnSelectedGridLayerChanged;
        }

        _binding = binding;
        if (_binding is null)
        {
            return;
        }

        UpdateSelectedGridLayer(_binding.SelectedGridLayer);
        _binding.SelectedGridLayerChanged += OnSelectedGridLayerChanged;
    }

    public void Dispose() => Bind(null);

    private void UpdateSelectedGridLayer(HudGridLayerIdentity identity)
    {
        _gridLayerNumber.Text = identity.LabelToken;
        _gridLayerName.Text = string.IsNullOrWhiteSpace(identity.DisplayName)
            ? null
            : identity.DisplayName.ToUpperInvariant();
        _gridLayerName.IsVisible = _gridLayerName.Text is not null;
    }

    private void OnSelectedGridLayerChanged(HudGridLayerIdentity identity) =>
        Dispatcher.UIThread.Post(() => UpdateSelectedGridLayer(identity));
}
