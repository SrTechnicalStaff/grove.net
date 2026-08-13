using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using GroveApp.DesignSystem;
using GroveApp.Engine.Memory;
using GroveApp.Models;
using GroveApp.Models.Memory;
using Colors = GroveApp.DesignSystem.Colors;

namespace GroveApp.Controls;

public enum SlateKind : byte
{
    Writing,
    Memory,
    Gallery
}

public partial class SlateHostOverlay : UserControl
{
    private GridDocument? _document;
    private GridContentItem[] _archiveItems = Array.Empty<GridContentItem>();
    private Func<string, IReadOnlyList<MemorySearchResult>>? _memorySearchProvider;
    private readonly List<Bitmap> _memoryBitmaps = new();
    private ContentKind? _archiveFilter;
    private SlateKind _mode;

    public SlateHostOverlay()
    {
        InitializeComponent();
        CloseButton.Click += (_, _) => Close();
        SearchBox.TextChanged += (_, _) =>
        {
            if (_mode == SlateKind.Memory && _memorySearchProvider is not null)
            {
                BuildMemoryCards(_memorySearchProvider(SearchBox.Text ?? string.Empty));
            }
            else ApplyArchiveFilter();
        };
        AllFilterButton.Click += (_, _) => SetArchiveFilter(null);
        NotesFilterButton.Click += (_, _) => SetArchiveFilter(ContentKind.Note);
        DocumentsFilterButton.Click += (_, _) => SetArchiveFilter(ContentKind.Document);
        ImagesFilterButton.Click += (_, _) => SetArchiveFilter(ContentKind.Image);
    }

    public SlateKind Mode => _mode;

    public event Action? Closed;

    public event Action<GridDocument, string, string>? DocumentSaveRequested;

    public void OpenForDocument(GridDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        _document = document;
        _mode = SlateKind.Writing;
        Margin = new Avalonia.Thickness(16);
        SlateSurface.Padding = new Avalonia.Thickness(16);
        SlateSurface.Background = Colors.SurfaceChromeBrush;
        SlateSurface.BorderThickness = new Avalonia.Thickness(1);
        IdentityText.IsVisible = true;
        CloseButton.IsVisible = true;
        AllFilterButton.IsVisible = true;
        NotesFilterButton.IsVisible = true;
        DocumentsFilterButton.IsVisible = true;
        ImagesFilterButton.IsVisible = true;
        IdentityText.Text = string.IsNullOrWhiteSpace(document.Title) ? "WRITING" : document.Title.ToUpperInvariant();
        WritingEditor.Text = document.RawText;
        WritingPanel.IsVisible = true;
        ArchiveControls.IsVisible = false;
        ArchivePanel.IsVisible = false;
        IsVisible = true;
        WritingEditor.Focus();
    }

    public void OpenMemory(Func<string, IReadOnlyList<MemorySearchResult>> searchProvider)
    {
        ArgumentNullException.ThrowIfNull(searchProvider);
        _document = null;
        _mode = SlateKind.Memory;
        Margin = new Avalonia.Thickness(0);
        SlateSurface.Padding = new Avalonia.Thickness(0);
        SlateSurface.Background = Brushes.Transparent;
        SlateSurface.BorderThickness = new Avalonia.Thickness(0);
        IdentityText.IsVisible = false;
        CloseButton.IsVisible = false;
        AllFilterButton.IsVisible = false;
        NotesFilterButton.IsVisible = false;
        DocumentsFilterButton.IsVisible = false;
        ImagesFilterButton.IsVisible = false;
        ArchiveControls.Margin = new Avalonia.Thickness(0);
        _archiveFilter = null;
        IdentityText.Text = "MEMORIES";
        ArchiveControls.IsVisible = true;
        WritingPanel.IsVisible = false;
        ArchivePanel.IsVisible = true;
        _memorySearchProvider = searchProvider;
        BuildMemoryCards(searchProvider(string.Empty));
        IsVisible = true;
        SearchBox.Focus();
    }

    public void OpenGallery(IEnumerable<GridImage> images)
    {
        ArgumentNullException.ThrowIfNull(images);
        _document = null;
        _mode = SlateKind.Gallery;
        Margin = new Avalonia.Thickness(0);
        SlateSurface.Padding = new Avalonia.Thickness(0);
        SlateSurface.Background = Brushes.Transparent;
        SlateSurface.BorderThickness = new Avalonia.Thickness(0);
        IdentityText.IsVisible = false;
        CloseButton.IsVisible = false;
        ArchiveControls.IsVisible = false;
        _archiveFilter = ContentKind.Image;
        IdentityText.Text = "GALLERY";
        ArchiveControls.IsVisible = true;
        WritingPanel.IsVisible = false;
        ArchivePanel.IsVisible = true;
        BuildArchiveCards(images);
        IsVisible = true;
    }

    public void Close()
    {
        if (!IsVisible)
        {
            return;
        }

        if (_document is not null)
        {
            DocumentSaveRequested?.Invoke(_document, _document.Title, WritingEditor.Text ?? string.Empty);
        }

        _document = null;
        _archiveItems = Array.Empty<GridContentItem>();
        _memorySearchProvider = null;
        _archiveFilter = null;
        DisposeMemoryBitmaps();
        WritingEditor.Text = string.Empty;
        SearchBox.Text = string.Empty;
        ArchivePanel.Children.Clear();
        IsVisible = false;
        Closed?.Invoke();
    }

    private void BuildArchiveCards(IEnumerable<GridContentItem> items)
    {
        _archiveItems = items.ToArray();
        ArchivePanel.Children.Clear();
        foreach (GridContentItem item in _archiveItems)
        {
            if (_archiveFilter.HasValue && item.Kind != _archiveFilter.Value)
            {
                continue;
            }

            ArchivePanel.Children.Add(CreateCard(item));
        }

        ApplyArchiveFilter();
    }

    private void BuildMemoryCards(IEnumerable<MemorySearchResult> results)
    {
        DisposeMemoryBitmaps();
        ArchivePanel.Children.Clear();
        foreach (MemorySearchResult result in results)
        {
            ArchivePanel.Children.Add(CreateMemoryCard(result));
        }
    }

    private Control CreateMemoryCard(MemorySearchResult result)
    {
        MemoryRecord record = result.Memory;
        var content = new StackPanel { Width = 280 };
        if (record.PayloadKind == MemoryPayloadKind.BinaryImage)
        {
            try
            {
                var bitmap = new Bitmap(new MemoryStream(record.GetPayloadCopy()));
                _memoryBitmaps.Add(bitmap);
                content.Children.Add(new Image { Source = bitmap, Stretch = Stretch.Uniform });
            }
            catch (Exception)
            {
                content.Children.Add(new TextBlock
                {
                    Text = record.Title,
                    Foreground = Colors.TextPrimaryBrush,
                    TextWrapping = TextWrapping.Wrap
                });
            }
        }
        else
        {
            content.Children.Add(new TextBlock
            {
                Text = record.GetUtf8Payload(),
                Foreground = Colors.TextPrimaryBrush,
                TextWrapping = TextWrapping.Wrap,
                FontFamily = Typography.FontFamilyUi,
                FontSize = Typography.SizeBody
            });
        }

        return content;
    }

    private Control CreateCard(GridContentItem item)
    {
        var content = new StackPanel { Spacing = 6 };
        if (item is GridImage imagePreview && imagePreview.LoadedBitmap is Bitmap bitmap)
        {
            content.Children.Add(new Image
            {
                Source = bitmap,
                Width = 196,
                Height = 118,
                Stretch = Stretch.Uniform
            });
        }

        return content;
    }

    private void ApplyArchiveFilter()
    {
        if (!ArchiveControls.IsVisible)
        {
            return;
        }

        string query = SearchBox.Text?.Trim() ?? string.Empty;
        foreach (Control child in ArchivePanel.Children)
        {
            if (child is not Border border || border.Child is not StackPanel stack || stack.Children.OfType<TextBlock>().LastOrDefault() is not TextBlock label)
            {
                continue;
            }

            border.IsVisible = query.Length == 0 || label.Text?.Contains(query, StringComparison.OrdinalIgnoreCase) == true;
        }
    }

    private void SetArchiveFilter(ContentKind? filter)
    {
        _archiveFilter = filter;
        BuildArchiveCards(_archiveItems);
    }

    private void DisposeMemoryBitmaps()
    {
        foreach (Bitmap bitmap in _memoryBitmaps)
        {
            bitmap.Dispose();
        }

        _memoryBitmaps.Clear();
    }
}
