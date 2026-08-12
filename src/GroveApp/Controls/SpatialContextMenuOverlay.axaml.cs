using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using GroveApp.DesignSystem;
using GroveApp.Engine.Interaction;
using GroveApp.Models.Interaction;
using Colors = GroveApp.DesignSystem.Colors;

namespace GroveApp.Controls;

public partial class SpatialContextMenuOverlay : UserControl
{
    private ISpatialContextMenuService? _service;

    public event Action<ContextMenuCommandInvocation>? CommandRequested;

    public SpatialContextMenuOverlay()
    {
        InitializeComponent();
    }

    public void BindService(ISpatialContextMenuService service)
    {
        if (_service != null)
        {
            _service.ContextMenuStateChanged -= OnMenuStateChanged;
            _service.CommandRequested -= OnCommandRequested;
        }

        _service = service;
        _service.ContextMenuStateChanged += OnMenuStateChanged;
        _service.CommandRequested += OnCommandRequested;
        OnMenuStateChanged(_service.ActiveMenu);
    }

    private void OnCommandRequested(ContextMenuCommandInvocation invocation)
    {
        CommandRequested?.Invoke(invocation);
    }

    private void OnMenuStateChanged(SpatialContextMenuModel? model)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => OnMenuStateChanged(model));
            return;
        }

        CommandsPanel.Children.Clear();
        if (model is null)
        {
            IsVisible = false;
            IsHitTestVisible = false;
            return;
        }

        Width = model.Bounds.Width;
        Height = model.Bounds.Height;
        Margin = new Thickness(model.Position.X, model.Position.Y, 0, 0);
        HorizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment = VerticalAlignment.Top;
        IsVisible = true;
        IsHitTestVisible = true;

        foreach (ContextMenuCommand command in model.Commands)
        {
            if (command.IsSeparator)
            {
                CommandsPanel.Children.Add(new Separator
                {
                    Background = Colors.EdgeHairlineBrush,
                    Height = 1,
                    Margin = new Thickness(4, 2)
                });
                continue;
            }

            var button = new Button
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Padding = new Thickness(8, 5),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(0),
                Foreground = command.IsEnabled ? Colors.TextPrimaryBrush : Colors.TextUnavailableBrush,
                IsEnabled = command.IsEnabled,
                Content = command.InputGestureText is null
                    ? command.Header
                    : $"{command.Header}    {command.InputGestureText}"
            };
            button.Click += (_, _) => _service?.TryExecute(command.Id);
            CommandsPanel.Children.Add(button);
        }
    }
}
