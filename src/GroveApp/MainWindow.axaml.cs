using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using GroveApp.Controls;
using GroveApp.DesignSystem;
using GroveApp.Engine;
using GroveApp.Models;
using Colors = GroveApp.DesignSystem.Colors;

namespace GroveApp
{
    public partial class MainWindow : Window
    {
        private readonly KeybindModule _keybindModule = new();

        public MainWindow()
        {
            InitializeComponent();

            // Wire GridCanvasControl events
            CanvasControl.NoteSelected += OnNoteSelected;
            CanvasControl.NoteDoubleClicked += OnNoteDoubleClicked;
            CanvasControl.EmptyCellDoubleClicked += OnEmptyCellDoubleClicked;
            CanvasControl.CameraChanged += OnCameraChanged;

            // Handle Pointer Press on Canvas for Double Click detection
            CanvasControl.AddHandler(PointerPressedEvent, OnCanvasPointerPressed, RoutingStrategies.Tunnel);

            // Wire Information Layer Overlays
            LocalEditor.SaveRequested += OnLocalEditorSaveRequested;
            LocalEditor.EscalationRequested += OnLocalEditorEscalationRequested;
            LocalEditor.Closed += OnOverlayClosed;

            QuickNote.SaveAndPlaceRequested += OnQuickNoteSaveAndPlaceRequested;
            QuickNote.Closed += OnOverlayClosed;

            // Handle Window Resizing for Editor Overlay anchor update
            SizeChanged += (s, e) => UpdateLocalEditorPosition();

            // Handle Keyboard Input via KeybindModule
            KeyDown += OnWindowKeyDown;

            // Update HUD status telemetry periodically
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
            TxtCellCoord.Text = $"CELL: ({CanvasControl.CursorCellX}, {CanvasControl.CursorCellY})";
            TxtZoom.Text = $"ZOOM: {(int)(CanvasControl.Zoom * 100)}%";
            TxtNoteCount.Text = $"NOTES: {CanvasControl.Notes.Count}";
        }

        private void OnCameraChanged()
        {
            UpdateLocalEditorPosition();
        }

        private void UpdateLocalEditorPosition()
        {
            if (LocalEditor.IsVisible && LocalEditor.TargetNote != null)
            {
                var targetNote = LocalEditor.TargetNote;
                Point worldTopLeft = CanvasControl.Camera.CellToWorld(targetNote.CellX, targetNote.CellY, GridCanvasControl.CellSize);
                Point screenTopLeft = CanvasControl.WorldToScreen(worldTopLeft);
                double sizePx = targetNote.SizeCells * GridCanvasControl.CellSize * CanvasControl.Zoom;
                Rect sourceBounds = new Rect(screenTopLeft.X, screenTopLeft.Y, sizePx, sizePx);

                LocalEditor.UpdatePosition(sourceBounds, Bounds.Size);
            }
        }

        private void OnNoteSelected(GridNote note)
        {
            if (!LocalEditor.IsVisible && !QuickNote.IsVisible)
            {
                CanvasControl.Focus();
            }
        }

        private void OnNoteDoubleClicked(GridNote note)
        {
            var selectedNotes = CanvasControl.GetSelectedNotes();
            if (selectedNotes.Count >= 2)
            {
                Rect primaryBounds = CanvasControl.GetNoteScreenBounds(selectedNotes[0]);
                LocalEditor.OpenForNotes(selectedNotes, primaryBounds, Bounds.Size);
            }
            else
            {
                OpenLocalEditorForNote(note);
            }
        }

        private void OnEmptyCellDoubleClicked(int cellX, int cellY)
        {
            var newNote = new GridNote(cellX, cellY, "New Note", NoteColor.Violet);
            CanvasControl.Notes.Add(newNote);
            CanvasControl.DeselectAllNotes();
            newNote.IsSelected = true;
            CanvasControl.SelectedNote = newNote;
            CanvasControl.InvalidateVisual();
            OpenLocalEditorForNote(newNote);
        }

        private void OpenLocalEditorForNote(GridNote note)
        {
            Rect sourceBounds = CanvasControl.GetNoteScreenBounds(note);
            LocalEditor.OpenForNote(note, sourceBounds, Bounds.Size);
        }

        private void OnLocalEditorSaveRequested(GridNote note, string newText)
        {
            note.Text = newText;
            note.RecalculateFootprint();
            CanvasControl.InvalidateVisual();
            CanvasControl.Focus();
        }

        private void OnLocalEditorEscalationRequested(GridNote note, string text)
        {
            // Update Note text upon escalation handoff
            note.Text = text;
            note.RecalculateFootprint();
            CanvasControl.InvalidateVisual();
            CanvasControl.Focus();
        }

        private void OnQuickNoteSaveAndPlaceRequested(QuickNoteItem item)
        {
            // Arm & place Quick Note on Grid Plane at current cell cursor
            var newNote = new GridNote(CanvasControl.CursorCellX, CanvasControl.CursorCellY, item.Text, NoteColor.Violet, isAnchored: true);
            CanvasControl.Notes.Add(newNote);
            CanvasControl.DeselectAllNotes();
            newNote.IsSelected = true;
            CanvasControl.SelectedNote = newNote;
            item.IsAnchored = true;
            CanvasControl.InvalidateVisual();
            CanvasControl.Focus();
        }

        private void OnOverlayClosed()
        {
            CanvasControl.Focus();
        }

        private void OnWindowKeyDown(object? sender, KeyEventArgs e)
        {
            _keybindModule.ProcessKeyDown(e, CanvasControl, LocalEditor, QuickNote);
        }
    }
}
