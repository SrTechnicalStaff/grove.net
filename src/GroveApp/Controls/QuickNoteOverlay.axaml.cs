using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using GroveApp.DesignSystem;
using GroveApp.Models;

namespace GroveApp.Controls
{
    public class QuickNoteItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Text { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string FormattedTime => Timestamp.ToString("HH:mm");
    }

    public partial class QuickNoteOverlay : UserControl
    {
        public event Action<QuickNoteItem>? SaveAndPlaceRequested;
        public event Action<QuickNoteItem>? NoteSaved;
        public event Action<GridNote, string>? PlacedNoteTextCommitted;
        public event Action? Closed;

        private GridNote? _placedNote;

        public QuickNoteOverlay()
        {
            InitializeComponent();

            KeyDown += OnQuickNoteKeyDown;
            TxtCapture.LostFocus += OnLostFocus;
        }

        public void Open()
        {
            _placedNote = null;
            IsVisible = true;
            TxtCapture.Text = string.Empty;
            FeedContainer.Children.Clear();
            FeedContainer.IsVisible = false;

            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                TxtCapture.Focus();
            });
        }

        public void OpenForPlacedNote(GridNote note)
        {
            ArgumentNullException.ThrowIfNull(note);
            _placedNote = note;
            IsVisible = true;
            TxtCapture.Text = note.Text;

            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                TxtCapture.Focus();
                TxtCapture.CaretIndex = TxtCapture.Text?.Length ?? 0;
            });
        }

        public void Close()
        {
            if (!IsVisible) return;
            IsVisible = false;
            TxtCapture.Text = string.Empty;
            _placedNote = null;
            FeedContainer.Children.Clear();
            FeedContainer.IsVisible = false;
            Closed?.Invoke();
        }

        private void BtnHeaderClose_Click(object? sender, RoutedEventArgs e)
        {
            Close();
            e.Handled = true;
        }

        private void OnLostFocus(object? sender, RoutedEventArgs e)
        {
            if (IsVisible)
            {
                Close();
            }
        }

        private void OnQuickNoteKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                SaveCurrentAndPlace();
                e.Handled = true;
            }
            else if (e.Key == Key.Enter && e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            {
                if (_placedNote is null)
                {
                    SaveCurrentAndContinue();
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Escape)
            {
                Close();
                e.Handled = true;
            }
        }

        private void SaveCurrentAndPlace()
        {
            string text = TxtCapture.Text?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(text))
            {
                if (_placedNote != null)
                {
                    PlacedNoteTextCommitted?.Invoke(_placedNote, text);
                }
                else
                {
                    var item = new QuickNoteItem
                    {
                        Text = text,
                        Timestamp = DateTime.Now
                    };
                    AppendSavedNoteToFeed(item);
                    NoteSaved?.Invoke(item);
                    SaveAndPlaceRequested?.Invoke(item);
                }
            }
            Close();
        }

        private void SaveCurrentAndContinue()
        {
            string text = TxtCapture.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var item = new QuickNoteItem
            {
                Text = text,
                Timestamp = DateTime.Now
            };
            AppendSavedNoteToFeed(item);
            NoteSaved?.Invoke(item);
            TxtCapture.Text = string.Empty;
            TxtCapture.Focus();
        }

        private void AppendSavedNoteToFeed(QuickNoteItem item)
        {
            var itemBorder = new Border
            {
                Background = Colors.SurfaceNestedBrush,
                BorderBrush = Colors.EdgeHairlineBrush,
                BorderThickness = new Avalonia.Thickness(1),
                CornerRadius = Tokens.CornerRadiusSm,
                Padding = Tokens.QuickNoteActionPadding
            };

            var itemText = new TextBlock
            {
                Text = $"{item.FormattedTime}  {item.Text}",
                FontFamily = Typography.FontFamilyUi,
                FontSize = Typography.SizeDense,
                Foreground = Colors.TextPrimaryBrush,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            };
            itemBorder.Child = itemText;
            FeedContainer.Children.Insert(0, itemBorder);
            FeedContainer.IsVisible = true;
        }
    }
}
