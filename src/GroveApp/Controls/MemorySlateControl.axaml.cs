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
using FluentAvalonia.UI.Controls;
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
        MainNav.SelectedItem = MainNav.MenuItems.Cast<NavigationViewItem>().FirstOrDefault();

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
        GalleryTimelineContainer.Children.Clear();
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

    private void OnNavItemInvoked(object? sender, NavigationViewItemInvokedEventArgs e)
    {
        if (e.InvokedItemContainer is NavigationViewItem item && item.Tag is string tag)
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
        GalleryTimelineContainer.Children.Clear();

        List<MemoryRecord> filtered = GetFilteredMemories().ToList();
        MemoryCountBadge.Text = $"{filtered.Count} item{(filtered.Count == 1 ? "" : "s")}";

        if (filtered.Count == 0)
        {
            return;
        }

        // Timeline Date Section Grouping (WinUI 3 Photos Experience)
        var groups = filtered
            .GroupBy(m => new DateTime(m.CreatedAtTicks, DateTimeKind.Utc).ToLocalTime().ToString("MMMM yyyy"))
            .ToList();

        foreach (var group in groups)
        {
            var section = new StackPanel
            {
                Spacing = 10
            };

            var headerText = new TextBlock
            {
                Text = group.Key,
                FontSize = 14,
                FontWeight = FontWeight.SemiBold,
                Foreground = Colors.TextSecondaryBrush,
                Margin = new Thickness(6, 0, 0, 0)
            };
            section.Children.Add(headerText);

            var wrapPanel = new WrapPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            foreach (MemoryRecord memory in group)
            {
                wrapPanel.Children.Add(CreateMemoryCard(memory));
            }

            section.Children.Add(wrapPanel);
            GalleryTimelineContainer.Children.Add(section);
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
            Classes = { "FluentMediaCard" }
        };

        var mainLayout = new Grid();

        // 1. Full-bleed preview content (NO black footer box!)
        Control previewContent = CreateMemoryPreview(memory, thumbnailMode: true);
        mainLayout.Children.Add(previewContent);

        // 2. Top-Right Corner Favorite Heart/Bookmark Badge
        if (isFav)
        {
            var badgeBorder = new Border
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(8),
                Padding = new Thickness(6),
                CornerRadius = new CornerRadius(12),
                Background = new SolidColorBrush(Color.Parse("#CC0E0E10"))
            };
            badgeBorder.Child = new SymbolIcon
            {
                Symbol = Symbol.Bookmark,
                FontSize = 12,
                Foreground = Colors.SignalInteractionBrush
            };
            mainLayout.Children.Add(badgeBorder);
        }

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
            return CreateTextCardSurface(memory.Title, "Corrupted image payload", Colors.SurfaceNestedBrush, Colors.TextPrimaryBrush);
        }
    }

    private static Control CreateDocumentPreview(MemoryRecord memory)
    {
        string content = memory.GetUtf8Payload();
        string displayTitle = string.IsNullOrWhiteSpace(memory.Title) ? "PDF Document" : memory.Title;
        return CreateTextCardSurface(displayTitle, content, Colors.SurfacePageBrush, Colors.PaperInkBrush);
    }

    private static Control CreateTextPreview(MemoryRecord memory)
    {
        string content = memory.GetUtf8Payload();
        string displayTitle = string.IsNullOrWhiteSpace(memory.Title) ? "Text Note" : memory.Title;
        return CreateTextCardSurface(displayTitle, content, ResolveTextSurface(memory.MemoryId), Colors.NoteTextBrush);
    }

    private static Control CreateTextCardSurface(string title, string bodyText, IBrush background, IBrush foreground)
    {
        var container = new Border
        {
            Background = background,
            Padding = new Thickness(16),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        var stack = new StackPanel
        {
            Spacing = 8
        };

        var titleBlock = new TextBlock
        {
            Text = title,
            FontWeight = FontWeight.Bold,
            FontSize = 14,
            Foreground = foreground,
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        stack.Children.Add(titleBlock);

        var bodyBlock = new TextBlock
        {
            Text = bodyText,
            Foreground = foreground,
            FontFamily = Typography.UiFamily,
            FontSize = 12,
            LineHeight = 12 * Typography.LineHeightSnug,
            TextWrapping = TextWrapping.Wrap,
            TextTrimming = TextTrimming.WordEllipsis,
            Opacity = 0.9
        };
        stack.Children.Add(bodyBlock);

        container.Child = stack;
        return container;
    }

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
                Width = 72,
                Height = 72,
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
