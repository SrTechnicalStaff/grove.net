using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Automation.Peers;
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
        MemoryWall.Children.Clear();

        var catalogue = new MemorySlateCatalogue(ledger);
        foreach (MemoryRecord memory in catalogue.Snapshot())
        {
            MemoryWall.Children.Add(CreateMemoryView(memory));
        }

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
        DisposeBitmaps();
        Closed?.Invoke();
    }

    private Control CreateMemoryView(MemoryRecord memory) => memory.PayloadKind switch
    {
        MemoryPayloadKind.BinaryImage => CreatePicture(memory),
        MemoryPayloadKind.PDFDocument => CreateDocument(memory),
        MemoryPayloadKind.PlainText or MemoryPayloadKind.RichTextMarkdown => CreateText(memory),
        _ => CreateText(memory)
    };

    private Control CreatePicture(MemoryRecord memory)
    {
        try
        {
            var bitmap = new Bitmap(new MemoryStream(memory.GetPayloadCopy()));
            _bitmaps.Add(bitmap);
            return new Viewbox
            {
                Stretch = Stretch.Uniform,
                StretchDirection = StretchDirection.DownOnly,
                Child = new Image
                {
                    Source = bitmap,
                    Width = bitmap.PixelSize.Width,
                    Height = bitmap.PixelSize.Height,
                    Stretch = Stretch.Uniform
                }
            };
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidDataException)
        {
            Trace.TraceError($"Memory {memory.MemoryId} could not be rendered: {exception}");
            MemoryRenderFailed?.Invoke(memory, exception);
            return CreateTextSurface(memory.Title, Colors.SurfaceNestedBrush, Colors.TextPrimaryBrush);
        }
    }

    private static Control CreateDocument(MemoryRecord memory)
    {
        string content = memory.GetUtf8Payload();
        if (string.IsNullOrWhiteSpace(content))
        {
            content = memory.Title;
        }

        return CreateTextSurface(content, Colors.SurfacePageBrush, Colors.PaperInkBrush);
    }

    private static Control CreateText(MemoryRecord memory) => CreateTextSurface(
        memory.GetUtf8Payload(),
        ResolveTextSurface(memory.MemoryId),
        Colors.NoteTextBrush);

    private static Control CreateTextSurface(string content, IBrush background, IBrush foreground) => new Border
    {
        Background = background,
        Padding = new Thickness(16),
        Child = new TextBlock
        {
            Text = content,
            Foreground = foreground,
            FontFamily = Typography.UiFamily,
            FontSize = Typography.SizeBody,
            FontWeight = Typography.WeightUiDefault,
            LineHeight = Typography.SizeBody * Typography.LineHeightSnug,
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Stretch
        }
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
