using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using GroveApp.Controls;
using GroveApp.Models;

namespace GroveApp
{
    public partial class MainWindow : Window
    {
        private GridNote? _editingNote;

        public MainWindow()
        {
            InitializeComponent();

            // Wire GridCanvasControl events
            CanvasControl.NoteSelected += OnNoteSelected;
            CanvasControl.NoteDoubleClicked += OnNoteDoubleClicked;
            CanvasControl.EmptyCellDoubleClicked += OnEmptyCellDoubleClicked;

            // Handle Pointer Press on Canvas for Double Click detection
            CanvasControl.AddHandler(PointerPressedEvent, OnCanvasPointerPressed, RoutingStrategies.Tunnel);

            // Handle Keyboard Input
            KeyDown += OnWindowKeyDown;

            // Update HUD status periodically
            var timer = new Avalonia.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50)
            };
            timer.Tick += (s, e) => UpdateHudStatus();
            timer.Start();
        }

        private void OnCanvasPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var clickInfo = e.GetCurrentPoint(CanvasControl);
            if (clickInfo.Properties.IsLeftButtonPressed && e.ClickCount == 2)
            {
                CanvasControl.HandleDoubleClick(e.GetPosition(CanvasControl));
                e.Handled = true;
            }
        }

        private void UpdateHudStatus()
        {
            TxtCellCoord.Text = $"Cell: ({CanvasControl.CursorCellX}, {CanvasControl.CursorCellY})";
            TxtZoom.Text = $"Zoom: {(int)(CanvasControl.Zoom * 100)}%";
            TxtNoteCount.Text = $"Notes: {CanvasControl.Notes.Count}";
        }

        private void OnNoteSelected(GridNote note)
        {
            // Focus canvas when note is selected
            CanvasControl.Focus();
        }

        private void OnNoteDoubleClicked(GridNote note)
        {
            StartEditingNote(note);
        }

        private void OnEmptyCellDoubleClicked(int cellX, int cellY)
        {
            var newNote = new GridNote(cellX, cellY, "New Note", NoteColor.Violet);
            CanvasControl.Notes.Add(newNote);
            CanvasControl.SelectedNote = newNote;
            StartEditingNote(newNote);
        }

        private void StartEditingNote(GridNote note)
        {
            _editingNote = note;
            TxtEditor.Text = note.Text ?? "";

            // Position Editor Overlay near Note on Screen
            Point startWorld = new Point(note.CellX * GridCanvasControl.CellSize, note.CellY * GridCanvasControl.CellSize);
            Point startScreen = CanvasControl.WorldToScreen(startWorld);

            EditorOverlay.Margin = new Thickness(
                Math.Clamp(startScreen.X, 20, Math.Max(20, Bounds.Width - 320)),
                Math.Clamp(startScreen.Y, 50, Math.Max(50, Bounds.Height - 200)),
                0, 0
            );

            EditorOverlay.IsVisible = true;
            TxtEditor.Focus();
            TxtEditor.SelectAll();
        }

        private void CommitEdit()
        {
            if (_editingNote != null)
            {
                _editingNote.Text = TxtEditor.Text ?? "";
                _editingNote.RecalculateFootprint();
                CanvasControl.InvalidateVisual();
            }
            EditorOverlay.IsVisible = false;
            _editingNote = null;
            CanvasControl.Focus();
        }

        private void CancelEdit()
        {
            EditorOverlay.IsVisible = false;
            _editingNote = null;
            CanvasControl.Focus();
        }

        // Event Handlers for Buttons & Keys
        private void BtnAddNote_Click(object? sender, RoutedEventArgs e)
        {
            OnEmptyCellDoubleClicked(CanvasControl.CursorCellX, CanvasControl.CursorCellY);
        }

        private void BtnColorViolet_Click(object? sender, RoutedEventArgs e) => SetSelectedNoteColor(NoteColor.Violet);
        private void BtnColorClay_Click(object? sender, RoutedEventArgs e) => SetSelectedNoteColor(NoteColor.Clay);
        private void BtnColorSlateBlue_Click(object? sender, RoutedEventArgs e) => SetSelectedNoteColor(NoteColor.SlateBlue);

        private void SetSelectedNoteColor(NoteColor color)
        {
            if (CanvasControl.SelectedNote != null)
            {
                CanvasControl.SelectedNote.Color = color;
                CanvasControl.InvalidateVisual();
            }
        }

        private void BtnZoomIn_Click(object? sender, RoutedEventArgs e)
        {
            CanvasControl.Zoom = Math.Min(3.5, CanvasControl.Zoom * 1.2);
            CanvasControl.InvalidateVisual();
        }

        private void BtnZoomOut_Click(object? sender, RoutedEventArgs e)
        {
            CanvasControl.Zoom = Math.Max(0.2, CanvasControl.Zoom * 0.8);
            CanvasControl.InvalidateVisual();
        }

        private void BtnResetCamera_Click(object? sender, RoutedEventArgs e)
        {
            CanvasControl.CameraX = 100.0;
            CanvasControl.CameraY = 100.0;
            CanvasControl.Zoom = 1.0;
            CanvasControl.InvalidateVisual();
        }

        private void BtnDelete_Click(object? sender, RoutedEventArgs e)
        {
            if (CanvasControl.SelectedNote != null)
            {
                CanvasControl.Notes.Remove(CanvasControl.SelectedNote);
                CanvasControl.SelectedNote = null;
                CanvasControl.InvalidateVisual();
            }
        }

        private void BtnSaveEdit_Click(object? sender, RoutedEventArgs e) => CommitEdit();
        private void BtnCancelEdit_Click(object? sender, RoutedEventArgs e) => CancelEdit();

        private void OnWindowKeyDown(object? sender, KeyEventArgs e)
        {
            // Ctrl+Enter commits edit inside overlay
            if (EditorOverlay.IsVisible)
            {
                if (e.Key == Key.Enter && e.KeyModifiers.HasFlag(KeyModifiers.Control))
                {
                    CommitEdit();
                    e.Handled = true;
                }
                else if (e.Key == Key.Escape)
                {
                    CancelEdit();
                    e.Handled = true;
                }
                return;
            }

            // Keyboard Shortcuts when on Canvas
            if (e.Key == Key.Escape)
            {
                if (CanvasControl.SelectedNote != null)
                {
                    CanvasControl.SelectedNote.IsSelected = false;
                    CanvasControl.SelectedNote = null;
                    CanvasControl.InvalidateVisual();
                }
            }
            else if (e.Key == Key.D2 || e.Key == Key.NumPad2) SetSelectedNoteColor(NoteColor.Violet);
            else if (e.Key == Key.D3 || e.Key == Key.NumPad3) SetSelectedNoteColor(NoteColor.Clay);
            else if (e.Key == Key.D4 || e.Key == Key.NumPad4) SetSelectedNoteColor(NoteColor.SlateBlue);
            else if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                if (CanvasControl.SelectedNote != null)
                {
                    CanvasControl.Notes.Remove(CanvasControl.SelectedNote);
                    CanvasControl.SelectedNote = null;
                    CanvasControl.InvalidateVisual();
                }
            }
        }
    }
}
