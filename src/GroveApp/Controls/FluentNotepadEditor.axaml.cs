using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using GroveApp.DesignSystem;
using GroveApp.Engine;
using GroveApp.Models;

namespace GroveApp.Controls
{
    public partial class FluentNotepadEditor : UserControl
    {
        private GridNote? _targetNote;
        private List<GridNote>? _targetNotes;
        private string _originalText = string.Empty;
        private int _openGeneration;

        public GridNote? TargetNote => _targetNote;

        public event Action<GridNote, string>? SaveRequested;
        public event Action? Closed;

        public INotepadStorageService StorageService { get; set; } = new FileNotepadStorageService();

        public FluentNotepadEditor()
        {
            InitializeComponent();

            EditorTextBox.PropertyChanged += (s, e) =>
            {
                if (e.Property == TextBox.CaretIndexProperty ||
                    e.Property == TextBox.SelectionStartProperty ||
                    e.Property == TextBox.SelectionEndProperty)
                {
                    UpdateTelemetry();
                }
            };
            EditorTextBox.TextChanged += (_, _) => UpdateTelemetry();
            EditorTextBox.KeyUp += (_, _) => UpdateTelemetry();
            EditorTextBox.PointerReleased += (_, _) => UpdateTelemetry();

            // Auto-save and dismiss on LostFocus (Blur)
            EditorTextBox.LostFocus += OnEditorLostFocus;

            // Handle Escape key
            EditorTextBox.KeyDown += OnEditorKeyDown;
        }

        public void OpenForNote(GridNote note, Rect sourceBounds, Size viewportSize)
        {
            _targetNote = note;
            _targetNotes = null;
            _originalText = note.Text;

            EditorTextBox.Text = note.Text;
            IsVisible = true;

            UpdatePosition(sourceBounds, viewportSize);

            // Immediate caret focus on open
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                EditorTextBox.Focus();
                EditorTextBox.CaretIndex = EditorTextBox.Text?.Length ?? 0;
                UpdateTelemetry();
            });

            int generation = ++_openGeneration;
            _ = LoadStoredTextAsync(note, generation);
        }

        public void OpenForNotes(List<GridNote> notes, Rect primaryBounds, Size viewportSize)
        {
            if (notes == null || notes.Count == 0) return;

            _targetNotes = notes;
            _targetNote = notes[0];
            _originalText = notes[0].Text;

            EditorTextBox.Text = notes[0].Text;
            IsVisible = true;

            UpdatePosition(primaryBounds, viewportSize);

            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                EditorTextBox.Focus();
                EditorTextBox.CaretIndex = EditorTextBox.Text?.Length ?? 0;
                UpdateTelemetry();
            });
        }

        public void UpdatePosition(Rect sourceBounds, Size viewportSize)
        {
            if (viewportSize.Width <= 0 || viewportSize.Height <= 0) return;

            double width = EditorFrame.Width > 0 ? EditorFrame.Width : Tokens.MeasureReading;
            double height = EditorFrame.Height > 0 ? EditorFrame.Height : Tokens.NotepadDefaultHeight;

            double targetX = sourceBounds.X;
            double targetY = sourceBounds.Y;

            // Clamp within viewport
            targetX = Math.Clamp(targetX, Tokens.SpaceMd, Math.Max(Tokens.SpaceMd, viewportSize.Width - width - Tokens.SpaceMd));
            targetY = Math.Clamp(targetY, Tokens.SpaceMd, Math.Max(Tokens.SpaceMd, viewportSize.Height - height - Tokens.SpaceMd));

            Margin = new Thickness(targetX, targetY, 0, 0);
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;
        }

        public void CommitSave()
        {
            if (!IsVisible) return;

            string currentText = EditorTextBox.Text ?? string.Empty;

            if (_targetNote != null)
            {
                SaveRequested?.Invoke(_targetNote, currentText);
                _ = StorageService.SaveAsync(_targetNote.Id.ToString(), currentText);
            }

            IsVisible = false;
            _openGeneration++;
            Closed?.Invoke();
        }

        private async Task LoadStoredTextAsync(GridNote note, int generation)
        {
            string storedText = await StorageService.LoadAsync(note.Id.ToString());
            if (!IsVisible || generation != _openGeneration || !ReferenceEquals(_targetNote, note) ||
                string.IsNullOrEmpty(storedText))
            {
                return;
            }

            EditorTextBox.Text = storedText;
            EditorTextBox.CaretIndex = storedText.Length;
            UpdateTelemetry();
        }

        public void HandleEscape()
        {
            CommitSave();
        }

        private void OnEditorLostFocus(object? sender, RoutedEventArgs e)
        {
            if (IsVisible)
            {
                CommitSave();
            }
        }

        private void OnEditorKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CommitSave();
                e.Handled = true;
            }
            else if (e.Key == Key.Enter && e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                CommitSave();
                e.Handled = true;
            }
        }

        private void UpdateTelemetry()
        {
            string text = EditorTextBox.Text ?? string.Empty;
            int caretIdx = Math.Clamp(EditorTextBox.CaretIndex, 0, text.Length);

            int line = 1;
            int lastLineBreak = -1;

            for (int i = 0; i < caretIdx; i++)
            {
                if (text[i] == '\n')
                {
                    line++;
                    lastLineBreak = i;
                }
            }

            int col = caretIdx - lastLineBreak;

            TxtTelemetry.Text = $"Ln {line}, Col {col} • 100% • UTF-8";
        }
    }
}
