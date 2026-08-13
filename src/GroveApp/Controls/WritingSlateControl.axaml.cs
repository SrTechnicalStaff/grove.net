using System;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using GroveApp.Models;

namespace GroveApp.Controls;

public partial class WritingSlateControl : UserControl
{
    private GridDocument? _document;

    public WritingSlateControl()
    {
        InitializeComponent();
        CloseButton.Click += (_, _) => Close();
    }

    public event Action? Closed;

    public event Action<GridDocument, string, string>? SaveRequested;

    protected override AutomationPeer OnCreateAutomationPeer() => new ControlAutomationPeer(this);

    public void Open(GridDocument document)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        WritingEditor.Text = document.RawText;
        IsVisible = true;
        WritingEditor.Focus();
    }

    public void Close()
    {
        if (!IsVisible)
        {
            return;
        }

        if (_document is GridDocument document)
        {
            SaveRequested?.Invoke(document, document.Title, WritingEditor.Text ?? string.Empty);
        }

        _document = null;
        WritingEditor.Text = string.Empty;
        IsVisible = false;
        Closed?.Invoke();
    }
}
