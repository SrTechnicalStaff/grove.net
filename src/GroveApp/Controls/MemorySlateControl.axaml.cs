using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using GroveApp.DesignSystem;
using GroveApp.Engine.Memory;
using GroveApp.Models.Memory;
using Colors = GroveApp.DesignSystem.Colors;

namespace GroveApp.Controls;

public partial class MemorySlateControl : UserControl
{
    private readonly List<Bitmap> _bitmaps = new();
    private readonly HashSet<Guid> _favorites = new();
    private IReadOnlyList<MemoryRecord> _allMemories = Array.Empty<MemoryRecord>();
    private MemoryRecord? _activeMemory;
    private string _filterCategory = "All";
    private string _searchQuery = string.Empty;

    public MemorySlateControl()
    {
        InitializeComponent();
    }

    public event Action? Closed;

    public event Action<MemoryRecord, Exception>? MemoryRenderFailed;

    protected override AutomationPeer OnCreateAutomationPeer() => new ControlAutomationPeer(this);

    public void Open(IMemoryLedger ledger)
    {
        ArgumentNullException.ThrowIfNull(ledger);
        DisposeBitmaps();

        var catalogue = new MemorySlateCatalogue(ledger);
        _allMemories = catalogue.Snapshot();

        SearchBox.Text = string.Empty;
        _searchQuery = string.Empty;
        _filterCategory = "All";

        LightboxOverlay.IsVisible = false;
        _activeMemory = null;

        RefreshGallery();

        IsVisible = true;
        Focus();
    }

    public void Close()
    {
        if (!IsVisible)
        {
            return;
        }

        IsVisible = false;
        MemoryWall.Children.Clear();
        FilmstripStack.Children.Clear();
        LightboxContainer.Child = null;
        DisposeBitmaps();
        _allMemories = Array.Empty<MemoryRecord>();
        _activeMemory = null;
        Closed?.Invoke();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Escape)
        {
            if (LightboxOverlay.IsVisible)
            {
                CloseLightbox();
                e.Handled = true;
            }
            else
            {
                Close();
                e.Handled = true;
            }
        }
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs args)
    {
        _searchQuery = SearchBox.Text?.Trim() ?? string.Empty;
        RefreshGallery();
    }

    private void OnNavCategoryClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is RadioButton button && button.Tag is string tag)
        {
            _filterCategory = tag;
            RefreshGallery();
        }
    }

    private IEnumerable<MemoryRecord> GetFilteredMemories()
    {
        IEnumerable<MemoryRecord> result = _allMemories;

        result = _filterCategory switch
        {
            "BinaryImage" => result.Where(m => m.PayloadKind == MemoryPayloadKind.BinaryImage),
            "TextDoc" => result.Where(m => m.PayloadKind != MemoryPayloadKind.BinaryImage),
            "Favorites" => result.Where(m => _favorites.Contains(m.MemoryId)),
            _ => result
        };

        if (!string.IsNullOrWhiteSpace(_searchQuery))
        {
            result = result.Where(m =>
                m.Title.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase) ||
                (m.HasTextPayload && m.GetUtf8Payload().Contains(_searchQuery, StringComparison.OrdinalIgnoreCase)) ||
                m.MemoryId.ToString().Contains(_searchQuery, StringComparison.OrdinalIgnoreCase));
        }

        return result;
    }

    private void RefreshGallery()
    {
        DisposeBitmaps();
        MemoryWall.Children.Clear();

        List<MemoryRecord> filtered = GetFilteredMemories().ToList();
        MemoryCountBadge.Text = $"{filtered.Count} item{(filtered.Count == 1 ? "" : "s")}";

        foreach (MemoryRecord memory in filtered)
        {
            MemoryWall.Children.Add(CreateMemoryCard(memory));
        }

        if (LightboxOverlay.IsVisible && _activeMemory != null)
        {
            PopulateFilmstrip(filtered);
        }
    }

    private Control CreateMemoryCard(MemoryRecord memory)
    {
        bool isFav = _favorites.Contains(memory.MemoryId);

        var card = new Border
        {
            Classes = { "MemoryCard" }
        };

        var mainLayout = new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto")
        };

        Control previewContent = CreateMemoryPreview(memory, thumbnailMode: true);
        Grid.SetRow(previewContent, 0);
        mainLayout.Children.Add(previewContent);

        var footer = new Border
        {
            Background = Colors.SurfaceChromeBrush,
            Padding = new Thickness(12, 8),
            BorderBrush = Colors.EdgeHairlineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0)
        };
        Grid.SetRow(footer, 1);

        var footerLayout = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto")
        };

        var titleBlock = new TextBlock
        {
            Text = string.IsNullOrWhiteSpace(memory.Title) ? GetDefaultTitle(memory) : memory.Title,
            FontWeight = FontWeight.SemiBold,
            FontSize = 13,
            Foreground = Colors.TextPrimaryBrush,
            TextTrimming = TextTrimming.CharacterEllipsis,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(titleBlock, 0);
        footerLayout.Children.Add(titleBlock);

        if (isFav)
        {
            var favIcon = new PathIcon
            {
                Data = Geometry.Parse("M12 17.27L18.18 21l-1.64-7.03L22 9.24l-7.19-.61L12 2 9.19 8.63 2 9.24l5.46 4.73L5.82 21z"),
                Width = 12,
                Height = 12,
                Foreground = Colors.SignalInteractionBrush,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(6, 0, 0, 0)
            };
            Grid.SetColumn(favIcon, 1);
            footerLayout.Children.Add(favIcon);
        }

        footer.Child = footerLayout;
        mainLayout.Children.Add(footer);

        card.Child = mainLayout;

        card.PointerPressed += (_, e) =>
        {
            if (e.GetCurrentPoint(card).Properties.IsLeftButtonPressed)
            {
                OpenLightbox(memory);
                e.Handled = true;
            }
        };

        return card;
    }

    private Control CreateMemoryPreview(MemoryRecord memory, bool thumbnailMode) => memory.PayloadKind switch
    {
        MemoryPayloadKind.BinaryImage => CreatePicturePreview(memory, thumbnailMode),
        MemoryPayloadKind.PDFDocument => CreateDocumentPreview(memory),
        _ => CreateTextPreview(memory)
    };

    private Control CreatePicturePreview(MemoryRecord memory, bool thumbnailMode)
    {
        try
        {
            var bitmap = new Bitmap(new MemoryStream(memory.GetPayloadCopy()));
            _bitmaps.Add(bitmap);

            return new Image
            {
                Source = bitmap,
                Stretch = thumbnailMode ? Stretch.UniformToFill : Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidDataException)
        {
            Trace.TraceError($"Memory {memory.MemoryId} thumbnail render failed: {exception}");
            MemoryRenderFailed?.Invoke(memory, exception);
            return CreateTextSurface(memory.Title, Colors.SurfaceNestedBrush, Colors.TextPrimaryBrush);
        }
    }

    private static Control CreateDocumentPreview(MemoryRecord memory)
    {
        string content = memory.GetUtf8Payload();
        if (string.IsNullOrWhiteSpace(content))
        {
            content = memory.Title;
        }

        return CreateTextSurface(content, Colors.SurfacePageBrush, Colors.PaperInkBrush);
    }

    private static Control CreateTextPreview(MemoryRecord memory) => CreateTextSurface(
        memory.GetUtf8Payload(),
        ResolveTextSurface(memory.MemoryId),
        Colors.NoteTextBrush);

    private static Control CreateTextSurface(string content, IBrush background, IBrush foreground) => new Border
    {
        Background = background,
        Padding = new Thickness(14),
        HorizontalAlignment = HorizontalAlignment.Stretch,
        VerticalAlignment = VerticalAlignment.Stretch,
        Child = new TextBlock
        {
            Text = content,
            Foreground = foreground,
            FontFamily = Typography.UiFamily,
            FontSize = 13,
            LineHeight = 13 * Typography.LineHeightSnug,
            TextWrapping = TextWrapping.Wrap,
            TextTrimming = TextTrimming.WordEllipsis
        }
    };

    private void OpenLightbox(MemoryRecord memory)
    {
        _activeMemory = memory;
        LightboxTitle.Text = string.IsNullOrWhiteSpace(memory.Title) ? GetDefaultTitle(memory) : memory.Title;

        UpdateFavoriteButtonState();

        LightboxContainer.Child = CreateMemoryPreview(memory, thumbnailMode: false);

        InfoTitleText.Text = string.IsNullOrWhiteSpace(memory.Title) ? GetDefaultTitle(memory) : memory.Title;
        InfoTypeText.Text = memory.PayloadKind.ToString();
        InfoIdText.Text = memory.MemoryId.ToString();
        InfoHashText.Text = memory.Hash.ToString();
        InfoCreatedText.Text = new DateTime(memory.CreatedAtTicks, DateTimeKind.Utc).ToLocalTime().ToString("g");

        PopulateFilmstrip(GetFilteredMemories().ToList());

        LightboxOverlay.IsVisible = true;
    }

    private void CloseLightbox()
    {
        LightboxOverlay.IsVisible = false;
        _activeMemory = null;
        LightboxContainer.Child = null;
        FilmstripStack.Children.Clear();
    }

    private void PopulateFilmstrip(List<MemoryRecord> memories)
    {
        FilmstripStack.Children.Clear();

        foreach (MemoryRecord item in memories)
        {
            bool isActive = _activeMemory?.MemoryId == item.MemoryId;

            var thumb = new Border
            {
                Width = 70,
                Height = 70,
                CornerRadius = new CornerRadius(6),
                ClipToBounds = true,
                BorderBrush = isActive ? Colors.SignalInteractionBrush : Colors.EdgeQuietBrush,
                BorderThickness = new Thickness(isActive ? 2 : 1),
                Background = Colors.SurfaceNestedBrush,
                Cursor = new Cursor(StandardCursorType.Hand)
            };

            thumb.Child = CreateMemoryPreview(item, thumbnailMode: true);

            thumb.PointerPressed += (_, e) =>
            {
                if (e.GetCurrentPoint(thumb).Properties.IsLeftButtonPressed)
                {
                    OpenLightbox(item);
                    e.Handled = true;
                }
            };

            FilmstripStack.Children.Add(thumb);
        }
    }

    private void OnCloseLightboxClicked(object? sender, RoutedEventArgs e) => CloseLightbox();

    private void OnCloseClicked(object? sender, RoutedEventArgs e) => Close();

    private void OnToggleFavoriteClicked(object? sender, RoutedEventArgs e)
    {
        if (_activeMemory == null)
        {
            return;
        }

        if (_favorites.Contains(_activeMemory.MemoryId))
        {
            _favorites.Remove(_activeMemory.MemoryId);
        }
        else
        {
            _favorites.Add(_activeMemory.MemoryId);
        }

        UpdateFavoriteButtonState();
        RefreshGallery();
    }

    private void OnToggleInfoClicked(object? sender, RoutedEventArgs e)
    {
        InfoDrawer.IsVisible = !InfoDrawer.IsVisible;
    }

    private void OnSlideshowClicked(object? sender, RoutedEventArgs e)
    {
        List<MemoryRecord> filtered = GetFilteredMemories().ToList();
        if (filtered.Count > 0)
        {
            OpenLightbox(filtered[0]);
        }
    }

    private void UpdateFavoriteButtonState()
    {
        if (_activeMemory == null)
        {
            return;
        }

        bool isFav = _favorites.Contains(_activeMemory.MemoryId);
        FavoriteIcon.Foreground = isFav ? Colors.SignalInteractionBrush : Colors.TextPrimaryBrush;
    }

    private static string GetDefaultTitle(MemoryRecord memory) => memory.PayloadKind switch
    {
        MemoryPayloadKind.BinaryImage => "Image Memory",
        MemoryPayloadKind.PDFDocument => "Document Memory",
        _ => "Text Note Memory"
    };

    private static IBrush ResolveTextSurface(Guid memoryId) => ((uint)memoryId.GetHashCode() % 3) switch
    {
        0 => Colors.NoteVioletBrush,
        1 => Colors.NoteClayBrush,
        _ => Colors.NoteSlateBlueBrush
    };

    private void DisposeBitmaps()
    {
        foreach (Bitmap bitmap in _bitmaps)
        {
            bitmap.Dispose();
        }

        _bitmaps.Clear();
    }
}
