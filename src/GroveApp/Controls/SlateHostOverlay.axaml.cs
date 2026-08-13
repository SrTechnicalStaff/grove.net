using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using GroveApp.DesignSystem;
using GroveApp.Models;
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
    private ContentKind? _archiveFilter;
    private SlateKind _mode;

    public SlateHostOverlay()
    {
        InitializeComponent();
        CloseButton.Click += (_, _) => Close();
        SearchBox.TextChanged += (_, _) => ApplyArchiveFilter();
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
        IdentityText.Text = string.IsNullOrWhiteSpace(document.Title) ? "WRITING" : document.Title.ToUpperInvariant();
        WritingEditor.Text = document.RawText;
        WritingPanel.IsVisible = true;
        ArchiveControls.IsVisible = false;
        ArchivePanel.IsVisible = false;
        IsVisible = true;
        WritingEditor.Focus();
    }

    public void OpenMemory(IEnumerable<GridContentItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        _document = null;
        _mode = SlateKind.Memory;
        _archiveFilter = null;
        IdentityText.Text = "MEMORIES";
        ArchiveControls.IsVisible = true;
        WritingPanel.IsVisible = false;
        ArchivePanel.IsVisible = true;
        BuildArchiveCards(items);
        IsVisible = true;
        SearchBox.Focus();
    }

    public void OpenGallery(IEnumerable<GridImage> images)
    {
        ArgumentNullException.ThrowIfNull(images);
        _document = null;
        _mode = SlateKind.Gallery;
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
        _archiveFilter = null;
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

        string identity = item switch
        {
            GridNote note => note.Text,
            GridDocument document => document.Title,
            GridImage imageItem => Path.GetFileName(imageItem.FilePath),
            _ => item.Id
        };
        content.Children.Add(new TextBlock
        {
            Text = identity,
            Foreground = Colors.TextPrimaryBrush,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 196,
            FontFamily = Typography.FontFamilyUi,
            FontSize = Typography.SizeDense
        });

        return new Border
        {
            Width = 208,
            Height = 168,
            Margin = new Avalonia.Thickness(0, 0, 12, 12),
            Padding = new Avalonia.Thickness(8),
            Background = Colors.SurfaceNestedBrush,
            BorderBrush = Colors.HudSlateBorderBrush,
            BorderThickness = new Avalonia.Thickness(1),
            Child = content
        };
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
}
