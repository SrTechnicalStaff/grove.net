using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using GroveApp.DesignSystem;
using Colors = GroveApp.DesignSystem.Colors;

namespace GroveApp.Controls
{
    public class QuickNoteItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Text { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public bool IsAnchored { get; set; }
        public string FormattedTime => Timestamp.ToString("HH:mm");
    }

    public partial class QuickNoteOverlay : UserControl
    {
        private readonly List<QuickNoteItem> _feed = new();
        private QuickNoteItem? _editingItem;
        private string? _pendingDiscardItemId;

        public event Action<QuickNoteItem>? SaveAndPlaceRequested;
        public event Action<QuickNoteItem>? NoteSaved;
        public event Action? Closed;

        public IReadOnlyList<QuickNoteItem> Feed => _feed;

        public QuickNoteOverlay()
        {
            InitializeComponent();

            TxtCapture.GotFocus += (s, e) => CaptureFieldBorder.BorderBrush = Colors.SignalInteractionBrush;
            TxtCapture.LostFocus += (s, e) => CaptureFieldBorder.BorderBrush = Colors.EdgeQuietBrush;

            KeyDown += OnQuickNoteKeyDown;
        }

        public void Open()
        {
            UnsavedConfirmRow.IsVisible = false;
            RefusalRow.IsVisible = false;
            IsVisible = true;

            TxtCapture.Focus();
            if (!string.IsNullOrEmpty(TxtCapture.Text))
            {
                TxtCapture.SelectionStart = TxtCapture.Text.Length;
                TxtCapture.SelectionEnd = TxtCapture.Text.Length;
            }
        }

        public void Close()
        {
            HandleEscape();
        }

        private void OnQuickNoteKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
                {
                    SaveCurrentAndPlace();
                    e.Handled = true;
                }
                else if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
                {
                    SaveCurrentAndNext();
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Escape)
            {
                HandleEscape();
                e.Handled = true;
            }
        }

        private void SaveCurrentAndNext()
        {
            string text = TxtCapture.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(text)) return;

            if (_editingItem != null)
            {
                _editingItem.Text = text;
                _editingItem = null;
                ComposerActionRow.IsVisible = false;
            }
            else
            {
                var newItem = new QuickNoteItem
                {
                    Text = text,
                    Timestamp = DateTime.Now
                };
                _feed.Insert(0, newItem);
                NoteSaved?.Invoke(newItem);
            }

            TxtCapture.Text = string.Empty;
            RebuildFeedUi();
            TxtCapture.Focus();
        }

        private void SaveCurrentAndPlace()
        {
            string text = TxtCapture.Text?.Trim() ?? string.Empty;
            QuickNoteItem? targetItem = _editingItem;

            if (!string.IsNullOrEmpty(text))
            {
                if (targetItem != null)
                {
                    targetItem.Text = text;
                    _editingItem = null;
                    ComposerActionRow.IsVisible = false;
                }
                else
                {
                    targetItem = new QuickNoteItem
                    {
                        Text = text,
                        Timestamp = DateTime.Now
                    };
                    _feed.Insert(0, targetItem);
                    NoteSaved?.Invoke(targetItem);
                }

                TxtCapture.Text = string.Empty;
                RebuildFeedUi();
                SaveAndPlaceRequested?.Invoke(targetItem);
                CloseSelf();
            }
            else if (_feed.Count > 0)
            {
                // If capture field is empty, place top item in feed
                SaveAndPlaceRequested?.Invoke(_feed[0]);
                CloseSelf();
            }
        }

        private void HandleEscape()
        {
            if (UnsavedConfirmRow.IsVisible)
            {
                UnsavedConfirmRow.IsVisible = false;
            }
            else if (RefusalRow.IsVisible)
            {
                RefusalRow.IsVisible = false;
            }
            else if (_pendingDiscardItemId != null)
            {
                _pendingDiscardItemId = null;
                RebuildFeedUi();
            }
            else if (_editingItem != null)
            {
                StopEditing();
            }
            else if (!string.IsNullOrWhiteSpace(TxtCapture.Text))
            {
                UnsavedConfirmRow.IsVisible = true;
            }
            else
            {
                CloseSelf();
            }
        }

        private void StopEditing()
        {
            _editingItem = null;
            TxtCapture.Text = string.Empty;
            ComposerActionRow.IsVisible = false;
            RebuildFeedUi();
        }

        private void StartEditingItem(QuickNoteItem item)
        {
            _editingItem = item;
            TxtCapture.Text = item.Text;
            ComposerActionRow.IsVisible = true;
            RebuildFeedUi();
            TxtCapture.Focus();
            TxtCapture.SelectionStart = TxtCapture.Text?.Length ?? 0;
        }

        private void ConfirmDiscardItem(QuickNoteItem item)
        {
            _pendingDiscardItemId = item.Id;
            RebuildFeedUi();
        }

        private void ExecuteDiscardItem(QuickNoteItem item)
        {
            _feed.Remove(item);
            if (_pendingDiscardItemId == item.Id) _pendingDiscardItemId = null;
            if (_editingItem == item) StopEditing();
            else RebuildFeedUi();
        }

        private void RebuildFeedUi()
        {
            FeedContainer.Children.Clear();

            foreach (var item in _feed)
            {
                bool isItemInConfirm = _pendingDiscardItemId == item.Id;
                bool isEditingActive = _editingItem != null;

                var itemCard = new Border
                {
                    Background = Colors.SurfaceNestedBrush,
                    BorderBrush = Colors.EdgeHairlineBrush,
                    BorderThickness = new Thickness(1),
                    CornerRadius = Tokens.CornerRadiusSm,
                    Padding = new Thickness(16, 10)
                };

                var itemContentStack = new StackPanel { Spacing = 6 };

                if (isItemInConfirm)
                {
                    // Discard confirm view inside item card
                    var confirmGrid = new Grid
                    {
                        ColumnDefinitions = new ColumnDefinitions("*, Auto, Auto")
                    };

                    var promptText = new TextBlock
                    {
                        Text = "Discard this Note?",
                        FontFamily = Typography.UiFamily,
                        FontSize = Typography.SizeCaption,
                        Foreground = Colors.TextPrimaryBrush,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                    };
                    Grid.SetColumn(promptText, 0);

                    var btnKeep = new Button
                    {
                        Content = "Keep it",
                        Background = Brushes.Transparent,
                        BorderThickness = new Thickness(0),
                        Foreground = Colors.TextSecondaryBrush,
                        FontSize = Typography.SizeCaption,
                        Margin = new Thickness(0, 0, 12, 0),
                        Padding = new Thickness(0)
                    };
                    btnKeep.Click += (s, e) => { _pendingDiscardItemId = null; RebuildFeedUi(); };
                    Grid.SetColumn(btnKeep, 1);

                    var btnConfirmDiscard = new Button
                    {
                        Content = "Discard",
                        Background = Colors.SignalRefusalBrush,
                        BorderThickness = new Thickness(0),
                        CornerRadius = Tokens.CornerRadiusSm,
                        Foreground = Brushes.White,
                        FontSize = Typography.SizeCaption,
                        Padding = new Thickness(8, 4)
                    };
                    btnConfirmDiscard.Click += (s, e) => ExecuteDiscardItem(item);
                    Grid.SetColumn(btnConfirmDiscard, 2);

                    confirmGrid.Children.Add(promptText);
                    confirmGrid.Children.Add(btnKeep);
                    confirmGrid.Children.Add(btnConfirmDiscard);

                    itemContentStack.Children.Add(confirmGrid);
                }
                else
                {
                    // Item top header row (Timestamp + Actions)
                    var topRow = new Grid
                    {
                        ColumnDefinitions = new ColumnDefinitions("Auto, *, Auto")
                    };

                    var leftHeaderStack = new StackPanel
                    {
                        Orientation = Avalonia.Layout.Orientation.Horizontal,
                        Spacing = 8
                    };

                    // Anchor diamond mark
                    if (item.IsAnchored)
                    {
                        var diamondContainer = new Canvas { Width = 12, Height = 12, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                        var diamondPoly = new Avalonia.Controls.Shapes.Polygon
                        {
                            Points = new Points { new Point(6, 1), new Point(11, 6), new Point(6, 11), new Point(1, 6) },
                            Fill = Colors.SignalAuthoredContextBrush
                        };
                        diamondContainer.Children.Add(diamondPoly);
                        leftHeaderStack.Children.Add(diamondContainer);
                    }

                    var timeTxt = new TextBlock
                    {
                        Text = item.FormattedTime,
                        FontFamily = Typography.MonoFamily,
                        FontSize = Typography.SizeLabel,
                        Foreground = Colors.TextMetaBrush,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                    };
                    leftHeaderStack.Children.Add(timeTxt);
                    Grid.SetColumn(leftHeaderStack, 0);

                    // Actions: Edit / Discard
                    var actionsStack = new StackPanel
                    {
                        Orientation = Avalonia.Layout.Orientation.Horizontal,
                        Spacing = 16
                    };

                    var editStack = new StackPanel
                    {
                        Orientation = Avalonia.Layout.Orientation.Horizontal,
                        Spacing = 4
                    };
                    editStack.Children.Add(new FluentAvalonia.UI.Controls.SymbolIcon
                    {
                        Symbol = FluentAvalonia.UI.Controls.Symbol.Edit,
                        FontSize = 11,
                        Foreground = isEditingActive ? Colors.TextUnavailableBrush : Colors.TextSecondaryBrush
                    });
                    editStack.Children.Add(new TextBlock { Text = "Edit" });

                    var btnEdit = new Button
                    {
                        Content = editStack,
                        Background = Brushes.Transparent,
                        BorderThickness = new Thickness(0),
                        Foreground = isEditingActive ? Colors.TextUnavailableBrush : Colors.TextSecondaryBrush,
                        FontSize = Typography.SizeCaption,
                        Padding = new Thickness(0),
                        IsEnabled = !isEditingActive
                    };
                    btnEdit.Click += (s, e) => StartEditingItem(item);

                    var discardStack = new StackPanel
                    {
                        Orientation = Avalonia.Layout.Orientation.Horizontal,
                        Spacing = 4
                    };
                    discardStack.Children.Add(new FluentAvalonia.UI.Controls.SymbolIcon
                    {
                        Symbol = FluentAvalonia.UI.Controls.Symbol.Delete,
                        FontSize = 11,
                        Foreground = isEditingActive ? Colors.TextUnavailableBrush : Colors.TextSecondaryBrush
                    });
                    discardStack.Children.Add(new TextBlock { Text = "Discard" });

                    var btnDiscard = new Button
                    {
                        Content = discardStack,
                        Background = Brushes.Transparent,
                        BorderThickness = new Thickness(0),
                        Foreground = isEditingActive ? Colors.TextUnavailableBrush : Colors.TextSecondaryBrush,
                        FontSize = Typography.SizeCaption,
                        Padding = new Thickness(0),
                        IsEnabled = !isEditingActive
                    };
                    btnDiscard.Click += (s, e) => ConfirmDiscardItem(item);

                    actionsStack.Children.Add(btnEdit);
                    actionsStack.Children.Add(btnDiscard);
                    Grid.SetColumn(actionsStack, 2);

                    topRow.Children.Add(leftHeaderStack);
                    topRow.Children.Add(actionsStack);

                    // Item text
                    var textBlock = new TextBlock
                    {
                        Text = item.Text,
                        FontFamily = Typography.UiFamily,
                        FontSize = Typography.SizeDense,
                        Foreground = Colors.TextPrimaryBrush,
                        TextWrapping = TextWrapping.Wrap
                    };

                    itemContentStack.Children.Add(topRow);
                    itemContentStack.Children.Add(textBlock);
                }

                itemCard.Child = itemContentStack;
                FeedContainer.Children.Add(itemCard);
            }
        }

        private void CloseSelf()
        {
            IsVisible = false;
            UnsavedConfirmRow.IsVisible = false;
            RefusalRow.IsVisible = false;
            _pendingDiscardItemId = null;
            Closed?.Invoke();
        }

        private void BtnHeaderClose_Click(object? sender, RoutedEventArgs e) => HandleEscape();
        private void BtnKeepEditingUnsaved_Click(object? sender, RoutedEventArgs e) => UnsavedConfirmRow.IsVisible = false;
        private void BtnDiscardUnsaved_Click(object? sender, RoutedEventArgs e) => CloseSelf();
        private void BtnStopEditing_Click(object? sender, RoutedEventArgs e) => StopEditing();
        private void BtnSaveComposer_Click(object? sender, RoutedEventArgs e) => SaveCurrentAndNext();
        private void BtnRefusalDismiss_Click(object? sender, RoutedEventArgs e) => RefusalRow.IsVisible = false;
        private void BtnRefusalRetry_Click(object? sender, RoutedEventArgs e) => SaveCurrentAndNext();
    }
}
