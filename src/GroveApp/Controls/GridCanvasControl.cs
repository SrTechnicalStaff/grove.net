using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using GroveApp.DesignSystem;
using GroveApp.Engine;
using GroveApp.Models;
using Colors = GroveApp.DesignSystem.Colors;

namespace GroveApp.Controls
{
    public class GridCanvasControl : Control
    {
        public const double CellSize = Tokens.GridCell;
        public const double MinorCellSize = Tokens.MinorCellSize;

        // Engine Modules
        private readonly FieldLedgerModule _fieldLedgerModule = new();
        private readonly NoteRenderModule _noteRenderModule = new();
        private readonly CursorRenderModule _cursorRenderModule = new();
        private readonly GridLineModule _gridLineModule = new();

        // Camera State
        public double CameraX { get; set; } = 100.0;
        public double CameraY { get; set; } = 100.0;
        public double Zoom { get; set; } = 1.0;

        // Pointer & Cursor State
        public Point MousePointerScreen { get; private set; }
        public Point MousePointerWorld { get; private set; }
        public int CursorCellX { get; private set; }
        public int CursorCellY { get; private set; }

        // Spent Cell Trail
        public List<SpentCell> SpentCells { get; } = new();
        private int _lastCursorCellX = int.MinValue;
        private int _lastCursorCellY = int.MinValue;

        // Content & Selection
        public List<GridNote> Notes { get; } = new();
        public GridNote? SelectedNote { get; set; }
        public GridNote? HoveredNote { get; set; }

        // Active Tool State
        public string ActiveTool { get; set; } = "SELECT"; // SELECT, NOTE, ANCHOR, PAN

        // Interaction States
        private bool _isPanning;
        private Point _panStartScreen;
        private double _panStartCamX;
        private double _panStartCamY;

        // Marquee Selection State
        private bool _isMarqueeSelecting;
        private Point _marqueeStartWorld;
        private Point _marqueeCurrentWorld;

        // Drag & Drop Note Movement State
        private bool _isDraggingNote;
        private GridNote? _draggedNote;
        private int _dragOffsetCellX;
        private int _dragOffsetCellY;

        // Animation Timer (60 FPS)
        private readonly DispatcherTimer _animationTimer;

        // Events
        public event Action<GridNote>? NoteSelected;
        public event Action<GridNote>? NoteDoubleClicked;
        public event Action<int, int>? EmptyCellDoubleClicked;

        public GridCanvasControl()
        {
            ClipToBounds = true;
            Focusable = true;

            // 60 FPS Animation loop for smooth spent cell trail decay
            _animationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16.66)
            };
            _animationTimer.Tick += OnAnimationTick;
            _animationTimer.Start();

            SeedSampleData();
        }

        private void SeedSampleData()
        {
            Notes.Add(new GridNote(0, 0, "Grove v9 Field Ledger\n\nEvery cell derives its background color directly from aura field gravity.", NoteColor.Violet, isAnchored: true));
            Notes.Add(new GridNote(3, 1, "Marquee & Drag Mechanics\n\nDrag notes across cells or sweep a marquee selection box.", NoteColor.Clay));
            Notes.Add(new GridNote(-2, 3, "Spacetime Grid\n\nCell energy accumulates from surrounding content presence.", NoteColor.SlateBlue));
        }

        private void OnAnimationTick(object? sender, EventArgs e)
        {
            bool needsRedraw = _cursorRenderModule.DecayTrail(SpentCells);
            if (needsRedraw)
            {
                InvalidateVisual();
            }
        }

        // Camera Transforms
        public Point ScreenToWorld(Point screenPt)
        {
            double wx = (screenPt.X - CameraX) / Zoom;
            double wy = (screenPt.Y - CameraY) / Zoom;
            return new Point(wx, wy);
        }

        public Point WorldToScreen(Point worldPt)
        {
            double sx = worldPt.X * Zoom + CameraX;
            double sy = worldPt.Y * Zoom + CameraY;
            return new Point(sx, sy);
        }

        public (int cellX, int cellY) WorldToCell(Point worldPt)
        {
            int cx = (int)Math.Floor(worldPt.X / CellSize);
            int cy = (int)Math.Floor(worldPt.Y / CellSize);
            return (cx, cy);
        }

        // Pointer Events
        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            MousePointerScreen = e.GetPosition(this);
            MousePointerWorld = ScreenToWorld(MousePointerScreen);

            var (cx, cy) = WorldToCell(MousePointerWorld);
            CursorCellX = cx;
            CursorCellY = cy;

            // Handle Camera Panning
            if (_isPanning)
            {
                Vector delta = MousePointerScreen - _panStartScreen;
                CameraX = _panStartCamX + delta.X;
                CameraY = _panStartCamY + delta.Y;
                InvalidateVisual();
                return;
            }

            // Handle Drag & Drop Note Movement
            if (_isDraggingNote && _draggedNote != null)
            {
                _draggedNote.CellX = cx - _dragOffsetCellX;
                _draggedNote.CellY = cy - _dragOffsetCellY;
                InvalidateVisual();
                return;
            }

            // Handle Marquee Selection Sweep
            if (_isMarqueeSelecting)
            {
                _marqueeCurrentWorld = MousePointerWorld;
                UpdateMarqueeSelection();
                InvalidateVisual();
                return;
            }

            // Update Spent Cells Trail Physics when cell changes
            if (cx != _lastCursorCellX || cy != _lastCursorCellY)
            {
                _cursorRenderModule.RegisterCellTransition(SpentCells, _lastCursorCellX, _lastCursorCellY);
                _lastCursorCellX = cx;
                _lastCursorCellY = cy;
            }

            // Hover Detection
            GridNote? newHover = FindNoteAtCell(cx, cy);
            if (newHover != HoveredNote)
            {
                if (HoveredNote != null) HoveredNote.IsHovered = false;
                HoveredNote = newHover;
                if (HoveredNote != null) HoveredNote.IsHovered = true;
            }

            InvalidateVisual();
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            Focus();
            var props = e.GetCurrentPoint(this).Properties;

            // Middle Click or Right Click to Pan Camera
            if (props.IsMiddleButtonPressed || props.IsRightButtonPressed)
            {
                _isPanning = true;
                _panStartScreen = e.GetPosition(this);
                _panStartCamX = CameraX;
                _panStartCamY = CameraY;
                e.Handled = true;
                return;
            }

            // Left Click
            if (props.IsLeftButtonPressed)
            {
                Point clickWorld = ScreenToWorld(e.GetPosition(this));
                var (cx, cy) = WorldToCell(clickWorld);
                GridNote? hitNote = FindNoteAtCell(cx, cy);

                if (hitNote != null)
                {
                    _isDraggingNote = true;
                    _draggedNote = hitNote;
                    _dragOffsetCellX = cx - hitNote.CellX;
                    _dragOffsetCellY = cy - hitNote.CellY;

                    SelectNote(hitNote);
                }
                else
                {
                    _isMarqueeSelecting = true;
                    _marqueeStartWorld = clickWorld;
                    _marqueeCurrentWorld = clickWorld;

                    DeselectAllNotes();
                }

                InvalidateVisual();
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            if (_isPanning)
            {
                _isPanning = false;
                e.Handled = true;
            }
            if (_isDraggingNote)
            {
                _isDraggingNote = false;
                _draggedNote = null;
                e.Handled = true;
            }
            if (_isMarqueeSelecting)
            {
                _isMarqueeSelecting = false;
                e.Handled = true;
            }
        }

        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            base.OnPointerWheelChanged(e);

            double zoomFactor = e.Delta.Y > 0 ? 1.15 : 0.85;
            double newZoom = Math.Clamp(Zoom * zoomFactor, 0.2, 3.5);

            Point mousePt = e.GetPosition(this);
            CameraX = mousePt.X - (mousePt.X - CameraX) * (newZoom / Zoom);
            CameraY = mousePt.Y - (mousePt.Y - CameraY) * (newZoom / Zoom);
            Zoom = newZoom;

            InvalidateVisual();
            e.Handled = true;
        }

        public void HandleDoubleClick(Point pt)
        {
            var (cx, cy) = WorldToCell(ScreenToWorld(pt));
            GridNote? hitNote = FindNoteAtCell(cx, cy);

            if (hitNote != null)
            {
                NoteDoubleClicked?.Invoke(hitNote);
            }
            else
            {
                EmptyCellDoubleClicked?.Invoke(cx, cy);
            }
        }

        public GridNote? FindNoteAtCell(int cx, int cy)
        {
            foreach (var note in Notes)
            {
                if (cx >= note.CellX && cx < note.CellX + note.SizeCells &&
                    cy >= note.CellY && cy < note.CellY + note.SizeCells)
                {
                    return note;
                }
            }
            return null;
        }

        private void SelectNote(GridNote note)
        {
            DeselectAllNotes();
            SelectedNote = note;
            SelectedNote.IsSelected = true;
            NoteSelected?.Invoke(SelectedNote);
        }

        public void DeselectAllNotes()
        {
            if (SelectedNote != null) SelectedNote.IsSelected = false;
            SelectedNote = null;
            foreach (var n in Notes) n.IsSelected = false;
        }

        private void UpdateMarqueeSelection()
        {
            double minX = Math.Min(_marqueeStartWorld.X, _marqueeCurrentWorld.X);
            double maxX = Math.Max(_marqueeStartWorld.X, _marqueeCurrentWorld.X);
            double minY = Math.Min(_marqueeStartWorld.Y, _marqueeCurrentWorld.Y);
            double maxY = Math.Max(_marqueeStartWorld.Y, _marqueeCurrentWorld.Y);

            Rect marqueeWorldRect = new Rect(minX, minY, maxX - minX, maxY - minY);

            foreach (var note in Notes)
            {
                Rect noteWorldRect = new Rect(note.CellX * CellSize, note.CellY * CellSize, note.SizeCells * CellSize, note.SizeCells * CellSize);
                note.IsSelected = marqueeWorldRect.Intersects(noteWorldRect);
                if (note.IsSelected)
                {
                    SelectedNote = note;
                }
            }
        }

        // High-Frequency Engine Pipeline Delegation
        public override void Render(DrawingContext context)
        {
            base.Render(context);

            double w = Bounds.Width;
            double h = Bounds.Height;
            if (w <= 0 || h <= 0) return;

            // Visible Cell Bounds in World Coordinates
            int minCellX = (int)Math.Floor((-CameraX) / (CellSize * Zoom)) - 1;
            int maxCellX = (int)Math.Ceiling((w - CameraX) / (CellSize * Zoom)) + 1;
            int minCellY = (int)Math.Floor((-CameraY) / (CellSize * Zoom)) - 1;
            int maxCellY = (int)Math.Ceiling((h - CameraY) / (CellSize * Zoom)) + 1;

            // 1. Field Ledger Module: Gravitational Cell Fills, Atmosphere & Perimeter Rings
            _fieldLedgerModule.RenderFieldLedger(context, WorldToScreen, CellSize, Zoom, minCellX, maxCellX, minCellY, maxCellY, Notes);

            // 2. Grid Line Module: Major 220px, Minor 44px Subdivisions & LOD Fading
            _gridLineModule.RenderGridLines(context, WorldToScreen, CellSize, Zoom, minCellX, maxCellX, minCellY, maxCellY);

            // 3. Note Render Module: Authored Fills, Containment Edge, Padding & Affordances
            _noteRenderModule.RenderNotes(context, WorldToScreen, CellSize, Zoom, minCellX, maxCellX, minCellY, maxCellY, Notes, SelectedNote, HoveredNote);

            // 4. Cursor Render Module: Spent Cell Decay Trail Physics
            _cursorRenderModule.RenderSpentTrail(context, WorldToScreen, CellSize, Zoom, SpentCells);

            // 5. Cursor Render Module: Grid Cursor Head, 13.2% Fill & Inset 2px Ring
            GridNote? targetNote = FindNoteAtCell(CursorCellX, CursorCellY);
            _cursorRenderModule.RenderGridCursor(context, WorldToScreen, CellSize, Zoom, CursorCellX, CursorCellY, targetNote);

            // 6. Marquee Selection Box Sweep
            RenderMarqueeSelection(context);
        }

        private void RenderMarqueeSelection(DrawingContext context)
        {
            if (!_isMarqueeSelecting) return;

            Point startScreen = WorldToScreen(_marqueeStartWorld);
            Point currentScreen = WorldToScreen(_marqueeCurrentWorld);

            double x = Math.Min(startScreen.X, currentScreen.X);
            double y = Math.Min(startScreen.Y, currentScreen.Y);
            double w = Math.Abs(currentScreen.X - startScreen.X);
            double h = Math.Abs(currentScreen.Y - startScreen.Y);

            Rect marqueeRect = new Rect(x, y, w, h);

            Color marqueeColor = Colors.SignalActiveWork;
            var fillBrush = new SolidColorBrush(Color.FromArgb((byte)(255 * 0.10), marqueeColor.R, marqueeColor.G, marqueeColor.B));
            var borderPen = new Pen(Colors.SignalActiveWorkBrush, Tokens.FieldPerimeterWidth, new DashStyle(new double[] { 4, 4 }, 0));

            context.FillRectangle(fillBrush, marqueeRect);
            context.DrawRectangle(null, borderPen, marqueeRect);
        }
    }
}
