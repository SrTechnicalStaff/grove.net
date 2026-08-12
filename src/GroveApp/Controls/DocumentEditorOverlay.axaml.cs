using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using GroveApp.Models;

namespace GroveApp.Controls;

public partial class DocumentEditorOverlay : UserControl
{
    private GridDocument? _document;

    public event Action<GridDocument, string, string>? SaveRequested;
    public event Action? Closed;

    public DocumentEditorOverlay()
    {
        InitializeComponent();
        SaveButton.Click += (_, _) => CommitSave();
        CancelButton.Click += (_, _) => Close();
        BodyBox.KeyDown += OnBodyKeyDown;
        TitleBox.KeyDown += OnBodyKeyDown;
        BodyBox.LostFocus += OnEditorLostFocus;
    }

    public void OpenForDocument(GridDocument document, Rect sourceBounds, Size viewportSize)
    {
        _document = document;
        TitleBox.Text = document.Title;
        BodyBox.Text = document.RawText;
        IsVisible = true;
        UpdatePosition(sourceBounds, viewportSize);
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            BodyBox.Focus();
            BodyBox.CaretIndex = BodyBox.Text?.Length ?? 0;
        });
    }

    public void UpdatePosition(Rect sourceBounds, Size viewportSize)
    {
        double width = 640;
        double height = 420;
        double x = Math.Clamp(sourceBounds.X, 16, Math.Max(16, viewportSize.Width - width - 16));
        double y = Math.Clamp(sourceBounds.Y, 16, Math.Max(16, viewportSize.Height - height - 16));
        Margin = new Thickness(x, y, 0, 0);
        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;
    }

    public void CommitSave()
    {
        if (!IsVisible || _document is null)
        {
            return;
        }

        SaveRequested?.Invoke(_document, TitleBox.Text ?? string.Empty, BodyBox.Text ?? string.Empty);
        Close();
    }

    public void Close()
    {
        if (!IsVisible)
        {
            return;
        }

        IsVisible = false;
        _document = null;
        Closed?.Invoke();
    }

    private void OnBodyKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
            e.Handled = true;
        }
        else if (e.Key == Key.Enter && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            CommitSave();
            e.Handled = true;
        }
    }

    private void OnEditorLostFocus(object? sender, RoutedEventArgs e)
    {
        if (IsVisible)
        {
            CommitSave();
        }
    }
}
