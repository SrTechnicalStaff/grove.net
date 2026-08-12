using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using GroveApp.DesignSystem;
using GroveApp.Models;

namespace GroveApp.Controls;

public partial class ImagePropertyOverlay : UserControl
{
    public event Action? Closed;

    public ImagePropertyOverlay()
    {
        InitializeComponent();
        CloseButton.Click += (_, _) => Close();
    }

    public void OpenForImage(GridImage image, Rect sourceBounds, Size viewportSize)
    {
        TitleText.Text = Path.GetFileName(image.FilePath);
        PropertiesPanel.Children.Clear();
        AddProperty("Natural size", $"{image.IntrinsicWidthPx} × {image.IntrinsicHeightPx} px");
        AddProperty("Footprint", $"{image.CellWidth} × {image.CellHeight} cells");
        AddProperty("Effective PPI", $"{image.EffectivePpi:F1}");
        AddProperty("Aspect", image.Footprint.Aspect.ToString());
        AddProperty("File", File.Exists(image.FilePath) ? $"{new FileInfo(image.FilePath).Length:N0} bytes" : "Unavailable");
        IsVisible = true;

        double x = Math.Clamp(sourceBounds.Right + 12, 16, Math.Max(16, viewportSize.Width - 316));
        double y = Math.Clamp(sourceBounds.Top, 16, Math.Max(16, viewportSize.Height - 260));
        Margin = new Thickness(x, y, 0, 0);
        HorizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment = VerticalAlignment.Top;
    }

    public void Close()
    {
        if (!IsVisible)
        {
            return;
        }

        IsVisible = false;
        Closed?.Invoke();
    }

    private void AddProperty(string label, string value)
    {
        PropertiesPanel.Children.Add(new TextBlock
        {
            Text = $"{label}: {value}",
            Foreground = Colors.TextSecondaryBrush,
            FontFamily = Typography.FontFamilyMono,
            FontSize = Typography.SizeCaption
        });
    }
}
