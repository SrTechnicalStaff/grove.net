using System;
using Avalonia.Controls;

namespace GroveApp.Controls;

public readonly record struct HudPerformanceSnapshot(
    double FramesPerSecond,
    double FrameMilliseconds,
    int ItemCount,
    int FieldSourceCount);

public partial class HudPerformanceTracker : UserControl
{
    private bool _isCollapsed;

    public HudPerformanceTracker()
    {
        InitializeComponent();
        ToggleButton.Click += OnToggleButtonClick;
        Update(new HudPerformanceSnapshot(60.0, 16.67, 0, 0));
    }

    public bool IsCollapsed => _isCollapsed;

    public void Update(HudPerformanceSnapshot snapshot)
    {
        FpsText.Text = $"{Math.Max(0.0, snapshot.FramesPerSecond):F0}";
        FrameText.Text = $"{Math.Max(0.0, snapshot.FrameMilliseconds):F1}ms";
        ItemsText.Text = snapshot.ItemCount.ToString();
        FieldText.Text = snapshot.FieldSourceCount.ToString();
        CollapsedSummaryText.Text = $"{Math.Max(0.0, snapshot.FramesPerSecond):F0} FPS  {Math.Max(0.0, snapshot.FrameMilliseconds):F1}ms";
    }

    private void OnToggleButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _isCollapsed = !_isCollapsed;
        DetailsPanel.IsVisible = !_isCollapsed;
        CollapsedPanel.IsVisible = _isCollapsed;
        ToggleButton.Content = _isCollapsed ? "+" : "—";
    }
}
