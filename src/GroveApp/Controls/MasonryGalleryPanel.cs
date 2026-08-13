using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using GroveApp.DesignSystem;

namespace GroveApp.Controls;

public sealed class MasonryGalleryPanel : Panel
{
    public static readonly StyledProperty<double> ColumnMinWidthProperty =
        AvaloniaProperty.Register<MasonryGalleryPanel, double>(nameof(ColumnMinWidth), Tokens.MasonryColumnMin);

    public static readonly StyledProperty<double> GutterSpacingProperty =
        AvaloniaProperty.Register<MasonryGalleryPanel, double>(nameof(GutterSpacing), Tokens.MasonryGutter);

    public double ColumnMinWidth
    {
        get => GetValue(ColumnMinWidthProperty);
        set => SetValue(ColumnMinWidthProperty, value);
    }

    public double GutterSpacing
    {
        get => GetValue(GutterSpacingProperty);
        set => SetValue(GutterSpacingProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double width = double.IsFinite(availableSize.Width) && availableSize.Width > 0
            ? availableSize.Width
            : ColumnMinWidth;
        int columnCount = GetColumnCount(width);
        double columnWidth = GetColumnWidth(width, columnCount);
        foreach (Control child in Children)
        {
            child.Measure(new Size(columnWidth, double.PositiveInfinity));
        }

        double[] columnHeights = ArrangeColumns(columnCount, columnWidth, measureOnly: true);
        return new Size(width, GetMaximumColumnHeight(columnHeights));
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double width = finalSize.Width > 0 ? finalSize.Width : ColumnMinWidth;
        int columnCount = GetColumnCount(width);
        double columnWidth = GetColumnWidth(width, columnCount);
        ArrangeColumns(columnCount, columnWidth, measureOnly: false);
        return new Size(width, GetMaximumColumnHeight(_columnHeights));
    }

    private double[] _columnHeights = Array.Empty<double>();

    private double[] ArrangeColumns(int columnCount, double columnWidth, bool measureOnly)
    {
        double[] columnHeights = new double[columnCount];
        foreach (Control child in Children)
        {
            int targetColumn = 0;
            for (int column = 1; column < columnHeights.Length; column++)
            {
                if (columnHeights[column] < columnHeights[targetColumn])
                {
                    targetColumn = column;
                }
            }

            double x = targetColumn * (columnWidth + GutterSpacing);
            double y = columnHeights[targetColumn];
            if (!measureOnly)
            {
                child.Arrange(new Rect(x, y, columnWidth, child.DesiredSize.Height));
            }

            columnHeights[targetColumn] += child.DesiredSize.Height + GutterSpacing;
        }

        if (!measureOnly)
        {
            _columnHeights = columnHeights;
        }

        return columnHeights;
    }

    private int GetColumnCount(double width) =>
        Math.Max(1, (int)Math.Floor((width + GutterSpacing) / (Math.Max(1.0, ColumnMinWidth) + GutterSpacing)));

    private double GetColumnWidth(double width, int columnCount) =>
        Math.Max(1.0, (width - ((columnCount - 1) * GutterSpacing)) / columnCount);

    private static double GetMaximumColumnHeight(double[] heights) =>
        heights.Length == 0 ? 0 : Math.Max(0, heights.Max());
}
