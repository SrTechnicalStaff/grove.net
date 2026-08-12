using System;
using System.Collections.Generic;

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

    public class EditorTabItem
    {
        public GridNote Note { get; }
        public string DraftText { get; set; }
        public string OriginalText { get; set; }
        public bool IsDirty => DraftText != OriginalText;

        public EditorTabItem(GridNote note)
        {
            Note = note;
            OriginalText = note.Text ?? string.Empty;
            DraftText = OriginalText;
        }
    }

    public partial class LocalEditorOverlay : UserControl
    {
        private readonly List<EditorTabItem> _tabs = new();
        private int _activeIndex = 0;
        private bool _isWysiwygMode = true; // Default to WYSIWYG live rendering
        private AnchorSide _fixedSide = AnchorSide.Right;

        public GridNote? TargetNote => _tabs.Count > 0 && _activeIndex >= 0 && _activeIndex < _tabs.Count ? _tabs[_activeIndex].Note : null;
        public bool IsDirty
        {
            get
            {
                foreach (var tab in _tabs)
                {
                    if (tab.IsDirty) return true;
                }
                return false;
            }
        }

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
            OpenForNotes(new[] { note }, sourceScreenBounds, viewportSize);
        }

        public void OpenForNotes(IEnumerable<GridNote> notes, Rect primarySourceBounds, Size viewportSize)
        {
            _tabs.Clear();
            foreach (var note in notes)
            {
                _tabs.Add(new EditorTabItem(note));
            }
            _activeIndex = 0;

            InlineConfirmRow.IsVisible = false;

            // Canonical 4-candidate side evaluation (Right -> Left -> Below -> Above) at --sp-md (16px) gap
            const double gap = Tokens.SpaceMd;
            double frameWidth = 580.0;
            double frameHeight = FrameBorder.Bounds.Height > 0 ? FrameBorder.Bounds.Height : 380.0;

            AnchorSide[] candidates = new[] { AnchorSide.Right, AnchorSide.Left, AnchorSide.Below, AnchorSide.Above };
            bool foundFit = false;

            foreach (var side in candidates)
            {
                Point candidatePos = CalculateSidePosition(side, primarySourceBounds, frameWidth, frameHeight, gap);
                Rect candidateRect = new Rect(candidatePos.X, candidatePos.Y, frameWidth, frameHeight);

                if (candidateRect.Left >= gap &&
                    candidateRect.Top >= gap &&
                    candidateRect.Right <= viewportSize.Width - gap &&
                    candidateRect.Bottom <= viewportSize.Height - gap)
                {
                    _fixedSide = side;
                    foundFit = true;
                    break;
                }
            }

            if (!foundFit)
            {
                _fixedSide = AnchorSide.Above; // 4th candidate fallback
            }

            UpdatePosition(primarySourceBounds, viewportSize);

            IsVisible = true;
            RefreshActiveTabUI();
            UpdateSaveButtonState();
        }

        public void SelectNextTab()
        {
            if (_tabs.Count <= 1) return;
            SyncCurrentDraftToTab();
            _activeIndex = (_activeIndex + 1) % _tabs.Count;
            RefreshActiveTabUI();
        }

        public void SelectPreviousTab()
        {
            if (_tabs.Count <= 1) return;
            SyncCurrentDraftToTab();
            _activeIndex = (_activeIndex - 1 + _tabs.Count) % _tabs.Count;
            RefreshActiveTabUI();
        }

        public void SelectTab(int index)
        {
            if (index < 0 || index >= _tabs.Count || index == _activeIndex) return;
            SyncCurrentDraftToTab();
            _activeIndex = index;
            RefreshActiveTabUI();
        }

        private void SyncCurrentDraftToTab()
        {
            if (_activeIndex >= 0 && _activeIndex < _tabs.Count)
            {
                _tabs[_activeIndex].DraftText = TxtDraft.Text ?? string.Empty;
            }
        }

        private void RefreshActiveTabUI()
        {
            RebuildTabStrip();

            if (_activeIndex >= 0 && _activeIndex < _tabs.Count)
            {
                var tab = _tabs[_activeIndex];
                TxtDraft.Text = tab.DraftText;
                WysiwygPreview.Text = tab.DraftText;
                TxtUnsavedBadge.IsVisible = tab.IsDirty;
            }

            UpdateModeUI();
            UpdateSaveButtonState();
        }

        private void RebuildTabStrip()
        {
            TabStripPanel.Children.Clear();

            for (int i = 0; i < _tabs.Count; i++)
            {
                int index = i;
                var tab = _tabs[i];
                bool isActive = (i == _activeIndex);

                var tabButton = new Button
                {
                    Background = isActive ? new SolidColorBrush(Color.Parse("#242428")) : Brushes.Transparent,
                    BorderBrush = isActive ? new SolidColorBrush(Color.Parse("#3A3A40")) : Brushes.Transparent,
                    BorderThickness = new Thickness(1),
                    CornerRadius = Tokens.CornerRadiusSm,
                    Padding = new Thickness(10, 4),
                    Margin = new Thickness(0, 0, 4, 0),
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                };

                string title = $"Note {i + 1}";
                if (tab.IsDirty)
                {
                    title += " •";
                }

                var tabStack = new StackPanel
                {
                    Orientation = Avalonia.Layout.Orientation.Horizontal,
                    Spacing = 6
                };

                var icon = new FluentAvalonia.UI.Controls.SymbolIcon
                {
                    Symbol = FluentAvalonia.UI.Controls.Symbol.Document,
                    FontSize = 11,
                    Foreground = isActive ? Colors.SignalInteractionBrush : Colors.TextSecondaryBrush
                };

                var textBlock = new TextBlock
                {
                    Text = title,
                    FontFamily = Typography.MonoFamily,
                    FontSize = Typography.SizeLabel,
                    FontWeight = isActive ? FontWeight.SemiBold : FontWeight.Normal,
                    Foreground = isActive ? new SolidColorBrush(Color.Parse("#EAEAEA")) : Colors.TextSecondaryBrush
                };

                tabStack.Children.Add(icon);
                tabStack.Children.Add(textBlock);

                tabButton.Content = tabStack;
                tabButton.Click += (s, e) => SelectTab(index);

                TabStripPanel.Children.Add(tabButton);
            }
        }

        public void ToggleWysiwygMode()
        {
            _isWysiwygMode = !_isWysiwygMode;
            SyncCurrentDraftToTab();
            UpdateModeUI();
        }

        private void UpdateModeUI()
        {
            if (_isWysiwygMode)
            {
                TxtDraft.IsVisible = false;
                WysiwygScrollViewer.IsVisible = true;
                WysiwygPreview.Text = TxtDraft.Text ?? string.Empty;
                TxtModeLabel.Text = "WYSIWYG";
                BtnModeToggle.Background = new SolidColorBrush(Color.Parse("#242428"));
            }
            else
            {
                WysiwygScrollViewer.IsVisible = false;
                TxtDraft.IsVisible = true;
                TxtModeLabel.Text = "RAW";
                BtnModeToggle.Background = new SolidColorBrush(Color.Parse("#3A3A40"));
                TxtDraft.Focus();
            }
        }

        public void UpdatePosition(Rect sourceScreenBounds, Size viewportSize)
        {
            const double gap = Tokens.SpaceMd;
            double frameWidth = 580.0;
            double frameHeight = FrameBorder.Bounds.Height > 0 ? FrameBorder.Bounds.Height : 380.0;

            Point pos = CalculateSidePosition(_fixedSide, sourceScreenBounds, frameWidth, frameHeight, gap);

            Margin = new Thickness(pos.X, pos.Y, 0, 0);
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
            SyncCurrentDraftToTab();
            if (_activeIndex >= 0 && _activeIndex < _tabs.Count)
            {
                var tab = _tabs[_activeIndex];
                TxtUnsavedBadge.IsVisible = tab.IsDirty;
                if (_isWysiwygMode)
                {
                    WysiwygPreview.Text = tab.DraftText;
                }
            }
            RebuildTabStrip();
            UpdateSaveButtonState();
        }

        private void UpdateSaveButtonState()
        {
            bool dirty = IsDirty;
            if (dirty)
            {
                BtnSave.Background = Colors.SignalInteractionBrush;
                BtnSave.Foreground = Colors.CPaperInkBrush;
                BtnSave.BorderBrush = null;
            }
            else
            {
                BtnSave.Background = Brushes.Transparent;
                BtnSave.Foreground = Colors.TextUnavailableBrush;
                BtnSave.BorderBrush = Colors.EdgeHairlineBrush;
            }
        }

        private void OnLocalEditorKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                CommitSave();
                e.Handled = true;
            }
            else if (e.Key == Key.E && e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                ToggleWysiwygMode();
                e.Handled = true;
            }
            else if (e.Key == Key.Tab)
            {
                if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
                {
                    SelectPreviousTab();
                }
                else
                {
                    SelectNextTab();
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                HandleEscape();
                e.Handled = true;
            }
        }

        public void CommitSave()
        {
            SyncCurrentDraftToTab();
            foreach (var tab in _tabs)
            {
                if (tab.IsDirty)
                {
                    tab.Note.Text = tab.DraftText;
                    tab.Note.RecalculateFootprint();
                    SaveRequested?.Invoke(tab.Note, tab.DraftText);
                }
            }
            CloseSelf();
        }

        public void HandleEscape()
        {
            if (InlineConfirmRow.IsVisible)
            {
                InlineConfirmRow.IsVisible = false;
            }
            else if (IsDirty)
            {
                InlineConfirmRow.IsVisible = true;
            }
            else
            {
                CloseSelf();
            }
        }

        public void CloseSelf()
        {
            IsVisible = false;
            InlineConfirmRow.IsVisible = false;
            _tabs.Clear();
            _activeIndex = 0;
            Closed?.Invoke();
        }

        private void BtnModeToggle_Click(object? sender, RoutedEventArgs e) => ToggleWysiwygMode();
        private void BtnCancel_Click(object? sender, RoutedEventArgs e) => HandleEscape();
        private void BtnSave_Click(object? sender, RoutedEventArgs e) => CommitSave();
        private void BtnKeepEditing_Click(object? sender, RoutedEventArgs e) => InlineConfirmRow.IsVisible = false;
        private void BtnDiscardConfirm_Click(object? sender, RoutedEventArgs e) => CloseSelf();

        private void BtnEscalate_Click(object? sender, RoutedEventArgs e)
        {
            SyncCurrentDraftToTab();
            if (TargetNote != null && _activeIndex >= 0 && _activeIndex < _tabs.Count)
            {
                EscalationRequested?.Invoke(TargetNote, _tabs[_activeIndex].DraftText);
                CloseSelf();
            }
        }

        private bool IsOutsideFrame(Point pt)
        {
            Point borderPt = this.TranslatePoint(pt, FrameBorder) ?? pt;
            return borderPt.X < 0 || borderPt.Y < 0 || borderPt.X > FrameBorder.Bounds.Width || borderPt.Y > FrameBorder.Bounds.Height;
        }

        private void PassToCanvas(PointerEventArgs e)
        {
            var canvas = (VisualRoot as MainWindow)?.CanvasControl;
            if (canvas != null)
            {
                canvas.RaiseEvent(e);
            }
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (IsOutsideFrame(e.GetPosition(this)))
            {
                PassToCanvas(e);
                return;
            }
            base.OnPointerPressed(e);
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            if (IsOutsideFrame(e.GetPosition(this)))
            {
                PassToCanvas(e);
                return;
            }
            base.OnPointerMoved(e);
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            if (IsOutsideFrame(e.GetPosition(this)))
            {
                PassToCanvas(e);
                return;
            }
            base.OnPointerReleased(e);
        }

        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            if (IsOutsideFrame(e.GetPosition(this)))
            {
                PassToCanvas(e);
                return;
            }
            base.OnPointerWheelChanged(e);
        }
    }
}
