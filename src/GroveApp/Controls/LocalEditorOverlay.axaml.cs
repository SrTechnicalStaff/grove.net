using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using GroveApp.DesignSystem;
using GroveApp.Models;
using Colors = GroveApp.DesignSystem.Colors;

namespace GroveApp.Controls
{
    public enum AnchorSide
    {
        Right,
        Left,
        Below,
        Above
    }

    public partial class LocalEditorOverlay : UserControl
    {
        private GridNote? _targetNote;
        private string _originalText = string.Empty;
        private AnchorSide _fixedSide = AnchorSide.Right;
        private bool _isDirty;

        public GridNote? TargetNote => _targetNote;
        public bool IsDirty => _isDirty;

        public event Action<GridNote, string>? SaveRequested;
        public event Action<GridNote, string>? EscalationRequested;
        public event Action? Closed;

        public LocalEditorOverlay()
        {
            InitializeComponent();

            TxtDraft.TextChanged += OnDraftTextChanged;
            KeyDown += OnLocalEditorKeyDown;
            UpdateSaveButtonState();
        }

        public void OpenForNote(GridNote note, Rect sourceScreenBounds, Size viewportSize)
        {
            _targetNote = note;
            _originalText = note.Text ?? string.Empty;
            TxtDraft.Text = _originalText;
            _isDirty = false;

            InlineConfirmRow.IsVisible = false;
            RefusalRow.IsVisible = false;
            TxtUnsaved.IsVisible = false;

            // Run anchor procedure to select initial candidate side (Right, Left, Below, Above)
            _fixedSide = ResolveAnchorSide(sourceScreenBounds, viewportSize);
            UpdatePosition(sourceScreenBounds, viewportSize);

            IsVisible = true;
            UpdateSaveButtonState();

            // Focus draft field & set caret
            TxtDraft.Focus();
            TxtDraft.SelectionStart = TxtDraft.Text?.Length ?? 0;
            TxtDraft.SelectionEnd = TxtDraft.SelectionStart;
        }

        public void UpdatePosition(Rect sourceScreenBounds, Size viewportSize)
        {
            const double gap = Tokens.SpaceMd; // 16px gap
            const double margin = Tokens.SpaceMd; // 16px viewport margin
            double frameWidth = 340.0;
            double frameHeight = FrameBorder.Bounds.Height > 0 ? FrameBorder.Bounds.Height : 260.0;

            Point pos = CalculateSidePosition(_fixedSide, sourceScreenBounds, frameWidth, frameHeight, gap);

            // Frame follows source without scaling or re-flipping; clamp position if primary position went offscreen
            double clampedX = Math.Clamp(pos.X, margin, Math.Max(margin, viewportSize.Width - frameWidth - margin));
            double clampedY = Math.Clamp(pos.Y, margin, Math.Max(margin, viewportSize.Height - frameHeight - margin));

            Margin = new Thickness(clampedX, clampedY, 0, 0);
        }

        private AnchorSide ResolveAnchorSide(Rect sourceScreenBounds, Size viewportSize)
        {
            const double gap = Tokens.SpaceMd;
            const double margin = Tokens.SpaceMd;
            double frameWidth = 340.0;
            double frameHeight = FrameBorder.Bounds.Height > 0 ? FrameBorder.Bounds.Height : 260.0;

            AnchorSide[] candidateOrder = new[] { AnchorSide.Right, AnchorSide.Left, AnchorSide.Below, AnchorSide.Above };

            foreach (var side in candidateOrder)
            {
                Point p = CalculateSidePosition(side, sourceScreenBounds, frameWidth, frameHeight, gap);
                bool fitsX = p.X >= margin && (p.X + frameWidth) <= (viewportSize.Width - margin);
                bool fitsY = p.Y >= margin && (p.Y + frameHeight) <= (viewportSize.Height - margin);

                if (fitsX && fitsY)
                {
                    return side;
                }
            }

            // Fallback to 4th candidate (Above) when none fit
            return AnchorSide.Above;
        }

        private static Point CalculateSidePosition(AnchorSide side, Rect s, double width, double height, double gap)
        {
            double centerY = s.Y + (s.Height - height) / 2.0;
            double centerX = s.X + (s.Width - width) / 2.0;

            return side switch
            {
                AnchorSide.Right => new Point(s.Right + gap, centerY),
                AnchorSide.Left => new Point(s.X - gap - width, centerY),
                AnchorSide.Below => new Point(centerX, s.Bottom + gap),
                AnchorSide.Above => new Point(centerX, s.Y - gap - height),
                _ => new Point(s.Right + gap, centerY)
            };
        }

        private void OnDraftTextChanged(object? sender, TextChangedEventArgs e)
        {
            _isDirty = (TxtDraft.Text ?? string.Empty) != _originalText;
            TxtUnsaved.IsVisible = _isDirty;
            UpdateSaveButtonState();
        }

        private void UpdateSaveButtonState()
        {
            if (_isDirty)
            {
                BtnSave.Background = Colors.SignalInteractionBrush;
                BtnSave.Foreground = Colors.CPaperInkBrush;
                BtnSave.BorderBrush = null;
                BtnSave.BorderThickness = new Thickness(0);
            }
            else
            {
                BtnSave.Background = Brushes.Transparent;
                BtnSave.Foreground = Colors.TextUnavailableBrush;
                BtnSave.BorderBrush = Colors.EdgeHairlineBrush;
                BtnSave.BorderThickness = new Thickness(1);
            }
        }

        private void OnLocalEditorKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                CommitSave();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                HandleEscape();
                e.Handled = true;
            }
        }

        private void CommitSave()
        {
            if (_targetNote != null && _isDirty)
            {
                string newText = TxtDraft.Text ?? string.Empty;
                SaveRequested?.Invoke(_targetNote, newText);
                CloseSelf();
            }
            else if (!_isDirty)
            {
                CloseSelf();
            }
        }

        private void HandleEscape()
        {
            if (InlineConfirmRow.IsVisible)
            {
                InlineConfirmRow.IsVisible = false;
            }
            else if (RefusalRow.IsVisible)
            {
                RefusalRow.IsVisible = false;
            }
            else if (_isDirty)
            {
                InlineConfirmRow.IsVisible = true;
            }
            else
            {
                CloseSelf();
            }
        }

        private void CloseSelf()
        {
            IsVisible = false;
            InlineConfirmRow.IsVisible = false;
            RefusalRow.IsVisible = false;
            _targetNote = null;
            Closed?.Invoke();
        }

        private void BtnCancel_Click(object? sender, RoutedEventArgs e) => HandleEscape();
        private void BtnSave_Click(object? sender, RoutedEventArgs e) => CommitSave();
        private void BtnKeepEditing_Click(object? sender, RoutedEventArgs e) => InlineConfirmRow.IsVisible = false;

        private void BtnDiscardConfirm_Click(object? sender, RoutedEventArgs e) => CloseSelf();

        private void BtnEscalate_Click(object? sender, RoutedEventArgs e)
        {
            if (_targetNote != null)
            {
                EscalationRequested?.Invoke(_targetNote, TxtDraft.Text ?? string.Empty);
                CloseSelf();
            }
        }

        private void BtnRefusalDismiss_Click(object? sender, RoutedEventArgs e) => RefusalRow.IsVisible = false;

        private void BtnRefusalRetry_Click(object? sender, RoutedEventArgs e)
        {
            RefusalRow.IsVisible = false;
            CommitSave();
        }
    }
}
