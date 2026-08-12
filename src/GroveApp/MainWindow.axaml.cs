using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using GroveApp.Controls;
using GroveApp.DesignSystem;
using GroveApp.Models;
using Colors = GroveApp.DesignSystem.Colors;

namespace GroveApp
{
    public partial class MainWindow : Window
    {
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

            // Handle Keyboard Input
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
                Rect sourceBounds = CanvasControl.GetNoteScreenBounds(LocalEditor.TargetNote);
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
            OpenLocalEditorForNote(note);
        }

        private void OnEmptyCellDoubleClicked(int cellX, int cellY)
        {
            var newNote = new GridNote(cellX, cellY, "New Note", NoteColor.Violet);
            CanvasControl.Notes.Add(newNote);
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
            CanvasControl.SelectedNote = newNote;
            item.IsAnchored = true;
            CanvasControl.InvalidateVisual();
            CanvasControl.Focus();
        }

        private void OnOverlayClosed()
        {
            CanvasControl.Focus();
        }

        private void SetSelectedNoteColor(NoteColor color)
        {
            if (CanvasControl.SelectedNote != null)
            {
                CanvasControl.SelectedNote.Color = color;
                CanvasControl.InvalidateVisual();
            }
        }

        private void OnWindowKeyDown(object? sender, KeyEventArgs e)
        {
            // If LocalEditor or QuickNote overlays are visible, let them process key navigation first
            if (LocalEditor.IsVisible || QuickNote.IsVisible)
            {
                return;
            }

            // Global Keybindings on Spatial Canvas Plane
            if (e.Key == Key.N)
            {
                QuickNote.Open();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                if (CanvasControl.SelectedNote != null)
                {
                    CanvasControl.SelectedNote.IsSelected = false;
                    CanvasControl.SelectedNote = null;
                    CanvasControl.InvalidateVisual();
                }
            }
            else if (e.Key == Key.A)
            {
                if (CanvasControl.SelectedNote != null)
                {
                    CanvasControl.SelectedNote.IsAnchored = !CanvasControl.SelectedNote.IsAnchored;
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
