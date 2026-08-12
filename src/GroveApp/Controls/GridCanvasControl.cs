using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using GroveApp.Models;

namespace GroveApp.Controls
{
    public class GridCanvasControl : Control
    {
        public const double CellSize = 220.0;
        public const double MinorCellSize = 44.0;

        // Camera State
        public double CameraX { get; set; } = 100.0;
        public double CameraY { get; set; } = 100.0;
        public double Zoom { get; set; } = 1.0;

        // Pointer & Cursor State
        public Point MousePointerScreen { get; private set; }
        public Point MousePointerWorld { get; private set; }
        public int CursorCellX { get; private set; }
        public int CursorCellY { get; private set; }

        // Spent Cell Trail (Tail Physics)
        public List<SpentCell> SpentCells { get; } = new();
        private int _lastCursorCellX = int.MinValue;
        private int _lastCursorCellY = int.MinValue;

        // Content & Selection
        public List<GridNote> Notes { get; } = new();
        public GridNote? SelectedNote { get; set; }
        public GridNote? HoveredNote { get; set; }

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

            // 60 FPS Animation loop for smooth tail-physics decay
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
            Notes.Add(new GridNote(0, 0, "Grove v9 Field Ledger\n\nEvery cell derives its background color directly from aura field gravity.", NoteColor.Violet));
            Notes.Add(new GridNote(3, 1, "Marquee & Drag Mechanics\n\nDrag notes across cells or sweep a marquee selection box.", NoteColor.Clay));
            Notes.Add(new GridNote(-2, 3, "Spacetime Grid\n\nCell energy accumulates from surrounding content presence.", NoteColor.SlateBlue));
        }

        private void OnAnimationTick(object? sender, EventArgs e)
        {
            bool needsRedraw = false;

            // Decay spent trail energy (18-step decay curve)
            for (int i = SpentCells.Count - 1; i >= 0; i--)
            {
                SpentCells[i].Energy -= 0.055;
                if (SpentCells[i].Energy <= 0.0)
                {
                    SpentCells.RemoveAt(i);
                }
                needsRedraw = true;
            }

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
                if (_lastCursorCellX != int.MinValue && _lastCursorCellY != int.MinValue)
                {
                    SpentCells.Add(new SpentCell(_lastCursorCellX, _lastCursorCellY));
                    if (SpentCells.Count > 18)
                    {
                        SpentCells.RemoveAt(0);
                    }
                }
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

            // Left Click: Drag Note or Sweep Marquee
            if (props.IsLeftButtonPressed)
            {
                Point clickWorld = ScreenToWorld(e.GetPosition(this));
                var (cx, cy) = WorldToCell(clickWorld);
                GridNote? hitNote = FindNoteAtCell(cx, cy);

                if (hitNote != null)
                {
                    // Start Dragging Note
                    _isDraggingNote = true;
                    _draggedNote = hitNote;
                    _dragOffsetCellX = cx - hitNote.CellX;
                    _dragOffsetCellY = cy - hitNote.CellY;

                    // Update Selection
                    SelectNote(hitNote);
                }
                else
                {
                    // Start Marquee Selection on empty space
                    _isMarqueeSelecting = true;
                    _marqueeStartWorld = clickWorld;
                    _marqueeCurrentWorld = clickWorld;

                    // Deselect previous selection
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

        // Field Ledger Matrix Calculations (Gravitational Field Inheritance per cell)
        private Color CalculateCellFieldColor(int cx, int cy)
        {
            // Base spacetime canvas ground color (#0C0C0B -> RGB: 12, 12, 11)
            double r = 12.0;
            double g = 12.0;
            double b = 11.0;

            // Sum gravitational field energy vectors from all placed notes
            foreach (var note in Notes)
            {
                // Distance in cell units to note bounds
                double dx = 0.0;
                if (cx < note.CellX) dx = note.CellX - cx;
                else if (cx >= note.CellX + note.SizeCells) dx = cx - (note.CellX + note.SizeCells - 1);

                double dy = 0.0;
                if (cy < note.CellY) dy = note.CellY - cy;
                else if (cy >= note.CellY + note.SizeCells) dy = cy - (note.CellY + note.SizeCells - 1);

                double distSq = dx * dx + dy * dy;

                // Field ledger gravity equation: FieldWeight = Mass / (1 + 0.4 * distSq)
                double mass = 0.35; // Mass intensity
                double fieldWeight = mass / (1.0 + 0.4 * distSq);

                // Note field color RGB
                Color noteFieldColor = Color.Parse(note.FieldHueHex);

                r += noteFieldColor.R * fieldWeight;
                g += noteFieldColor.G * fieldWeight;
                b += noteFieldColor.B * fieldWeight;
            }

            byte finalR = (byte)Math.Clamp(r, 0, 255);
            byte finalG = (byte)Math.Clamp(g, 0, 255);
            byte finalB = (byte)Math.Clamp(b, 0, 255);

            return Color.FromRgb(finalR, finalG, finalB);
        }

        // High-Frequency 60 FPS Render Pipeline
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

            // 1. Field Ledger Grid Canvas: Each cell box's background color IS derived from its field ledger vector!
            RenderFieldLedgerGridCanvas(context, minCellX, maxCellX, minCellY, maxCellY);

            // 2. Grid Lines (Major 220px & Minor 44px)
            RenderGridLines(context, minCellX, maxCellX, minCellY, maxCellY);

            // 3. Placed Notes
            RenderNotes(context, minCellX, maxCellX, minCellY, maxCellY);

            // 4. Spent Cell Decay Trail (Grid Cursor Tail Physics)
            RenderCursorTrail(context);

            // 5. Grid Cursor Head & Inset Ring
            RenderGridCursor(context);

            // 6. Marquee Selection Box
            RenderMarqueeSelection(context);
        }

        private void RenderFieldLedgerGridCanvas(DrawingContext context, int minX, int maxX, int minY, int maxY)
        {
            // Evaluate Field Ledger color for every cell in viewport
            for (int cx = minX; cx <= maxX; cx++)
            {
                for (int cy = minY; cy <= maxY; cy++)
                {
                    Point startScreen = WorldToScreen(new Point(cx * CellSize, cy * CellSize));
                    double sizeScreen = CellSize * Zoom;
                    Rect cellRect = new Rect(startScreen.X, startScreen.Y, sizeScreen, sizeScreen);

                    Color cellFieldColor = CalculateCellFieldColor(cx, cy);
                    var cellBrush = new SolidColorBrush(cellFieldColor);
                    context.FillRectangle(cellBrush, cellRect);
                }
            }
        }

        private void RenderGridLines(DrawingContext context, int minX, int maxX, int minY, int maxY)
        {
            var majorPen = new Pen(new SolidColorBrush(Color.Parse("#1E1E1C")), 1.0);
            var minorPen = new Pen(new SolidColorBrush(Color.Parse("#141412")), 1.0);

            if (Zoom >= 0.45)
            {
                int minMinorX = minX * 5;
                int maxMinorX = maxX * 5;
                int minMinorY = minY * 5;
                int maxMinorY = maxY * 5;

                for (int mx = minMinorX; mx <= maxMinorX; mx++)
                {
                    if (mx % 5 == 0) continue;
                    double wx = mx * MinorCellSize;
                    Point p1 = WorldToScreen(new Point(wx, minY * CellSize));
                    Point p2 = WorldToScreen(new Point(wx, maxY * CellSize));
                    context.DrawLine(minorPen, p1, p2);
                }

                for (int my = minMinorY; my <= maxMinorY; my++)
                {
                    if (my % 5 == 0) continue;
                    double wy = my * MinorCellSize;
                    Point p1 = WorldToScreen(new Point(minX * CellSize, wy));
                    Point p2 = WorldToScreen(new Point(maxX * CellSize, wy));
                    context.DrawLine(minorPen, p1, p2);
                }
            }

            for (int cx = minX; cx <= maxX; cx++)
            {
                double wx = cx * CellSize;
                Point p1 = WorldToScreen(new Point(wx, minY * CellSize));
                Point p2 = WorldToScreen(new Point(wx, maxY * CellSize));
                context.DrawLine(majorPen, p1, p2);
            }

            for (int cy = minY; cy <= maxY; cy++)
            {
                double wy = cy * CellSize;
                Point p1 = WorldToScreen(new Point(minX * CellSize, wy));
                Point p2 = WorldToScreen(new Point(maxX * CellSize, wy));
                context.DrawLine(majorPen, p1, p2);
            }
        }

        private void RenderNotes(DrawingContext context, int minX, int maxX, int minY, int maxY)
        {
            var textInkBrush = new SolidColorBrush(Color.Parse("#F4F4F2"));
            var insetEdgePen = new Pen(new SolidColorBrush(Color.Parse("#6E6E6A")), 1.0);
            var selectionPen = new Pen(new SolidColorBrush(Color.Parse("#4D90FE")), 2.0);

            foreach (var note in Notes)
            {
                if (note.CellX + note.SizeCells < minX || note.CellX > maxX ||
                    note.CellY + note.SizeCells < minY || note.CellY > maxY)
                {
                    continue;
                }

                Point startScreen = WorldToScreen(new Point(note.CellX * CellSize, note.CellY * CellSize));
                Point endScreen = WorldToScreen(new Point((note.CellX + note.SizeCells) * CellSize, (note.CellY + note.SizeCells) * CellSize));

                double rectW = endScreen.X - startScreen.X;
                double rectH = endScreen.Y - startScreen.Y;
                Rect noteRect = new Rect(startScreen.X, startScreen.Y, rectW, rectH);

                // Authored Fill
                var fillBrush = new SolidColorBrush(Color.Parse(note.FillHex));
                context.FillRectangle(fillBrush, noteRect);

                // Inset Containment Edge
                context.DrawRectangle(null, insetEdgePen, noteRect.Deflate(0.5));

                // Selection Outline (2px #4D90FE offset by 3px)
                if (note.IsSelected)
                {
                    Rect selRect = noteRect.Inflate(3.0 * Zoom);
                    context.DrawRectangle(null, selectionPen, selRect);
                }

                // Render Text Block inside Note
                if (!string.IsNullOrEmpty(note.Text) && Zoom >= 0.3)
                {
                    double fontSize = Math.Max(10, 14.0 * Zoom);
                    var formattedText = new FormattedText(
                        note.Text,
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface("Inter, Segoe UI, sans-serif", FontStyle.Normal, FontWeight.Normal),
                        fontSize,
                        textInkBrush
                    )
                    {
                        MaxTextWidth = Math.Max(10, rectW - 30 * Zoom),
                        MaxTextHeight = Math.Max(10, rectH - 32 * Zoom)
                    };

                    Point textPos = new Point(startScreen.X + 15 * Zoom, startScreen.Y + 16 * Zoom);
                    context.DrawText(formattedText, textPos);
                }

                // Render Edit Affordance Pill on Hover
                if (note.IsHovered && Zoom >= 0.5)
                {
                    Rect pillRect = new Rect(startScreen.X + rectW - 48 * Zoom, startScreen.Y + 8 * Zoom, 40 * Zoom, 18 * Zoom);
                    var pillBg = new SolidColorBrush(Color.Parse("#1A1A18"));
                    var pillPen = new Pen(new SolidColorBrush(Color.Parse("#F4F4F2")), 1.0);
                    context.FillRectangle(pillBg, pillRect, 3);
                    context.DrawRectangle(null, pillPen, pillRect, 3);

                    var pillText = new FormattedText(
                        "EDIT",
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface("Consolas, monospace", FontStyle.Normal, FontWeight.Bold),
                        Math.Max(8, 9.0 * Zoom),
                        textInkBrush
                    );
                    context.DrawText(pillText, new Point(pillRect.X + 8 * Zoom, pillRect.Y + 3 * Zoom));
                }
            }
        }

        private void RenderCursorTrail(DrawingContext context)
        {
            foreach (var spent in SpentCells)
            {
                Point startWorld = new Point(spent.CellX * CellSize, spent.CellY * CellSize);
                Point startScreen = WorldToScreen(startWorld);
                double sizeScreen = CellSize * Zoom;

                Rect cellRect = new Rect(startScreen.X, startScreen.Y, sizeScreen, sizeScreen);
                byte alpha = (byte)(255 * 0.132 * spent.Energy);
                var trailBrush = new SolidColorBrush(Color.FromArgb(alpha, 244, 244, 242));
                context.FillRectangle(trailBrush, cellRect);
            }
        }

        private void RenderGridCursor(DrawingContext context)
        {
            GridNote? targetNote = FindNoteAtCell(CursorCellX, CursorCellY);

            int startCellX = targetNote?.CellX ?? CursorCellX;
            int startCellY = targetNote?.CellY ?? CursorCellY;
            int spanCells = targetNote?.SizeCells ?? 1;

            Point startWorld = new Point(startCellX * CellSize, startCellY * CellSize);
            Point endWorld = new Point((startCellX + spanCells) * CellSize, (startCellY + spanCells) * CellSize);

            Point startScreen = WorldToScreen(startWorld);
            Point endScreen = WorldToScreen(endWorld);

            double curW = endScreen.X - startScreen.X;
            double curH = endScreen.Y - startScreen.Y;
            Rect cursorRect = new Rect(startScreen.X, startScreen.Y, curW, curH);

            var headFillBrush = new SolidColorBrush(Color.FromArgb(34, 244, 244, 242));
            context.FillRectangle(headFillBrush, cursorRect);

            var ringPen = new Pen(new SolidColorBrush(Color.FromArgb(204, 244, 244, 242)), 2.0);
            context.DrawRectangle(null, ringPen, cursorRect.Deflate(1.0));
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

            var fillBrush = new SolidColorBrush(Color.FromArgb(26, 77, 144, 254)); // 10% opacity blue
            var borderPen = new Pen(new SolidColorBrush(Color.Parse("#4D90FE")), 1.5, new DashStyle(new double[] { 4, 4 }, 0));

            context.FillRectangle(fillBrush, marqueeRect);
            context.DrawRectangle(null, borderPen, marqueeRect);
        }
    }
}
