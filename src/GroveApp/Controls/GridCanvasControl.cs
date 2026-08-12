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
        public FieldLedgerEngine FieldEngine { get; } = new FieldLedgerEngine();

        // Camera Module & State
        public CameraModule Camera { get; } = new CameraModule();
        public double CameraX { get => Camera.CameraX; set => Camera.CameraX = value; }
        public double CameraY { get => Camera.CameraY; set => Camera.CameraY = value; }
        public double Zoom { get => Camera.Zoom; set => Camera.Zoom = value; }

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
        public event Action? CameraChanged;

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
            Notes.Add(new GridNote(0, 0, "# Field Ledger\n\n> Spatial grid canvas with **high-DPI** subpixel typography.\n\n- Inline `code` & <u>underline</u> & <del>strikethrough</del>\n- H<sub>2</sub>O and E=mc<sup>2</sup> formulas\n- <span style=\"color:#96B6F8\">HTML Color</span> & [Markdown Gold](#E8B964) & <mark>mark highlight</mark>", NoteColor.Violet, isAnchored: true));
            Notes.Add(new GridNote(3, 1, "## Code Engine Parity\n\n```cs\npublic class GridEngine {\n    public string Name { get; set; } = \"Grove\";\n    public bool IsActive() => true;\n}\n```\n\n[Grove Architecture](https://grove.net)", NoteColor.Clay));
            Notes.Add(new GridNote(-2, 3, "### Spacetime Grid\n\n1. **Zero** global overhead\n2. [Primary signal](#E8B964) status\n3. <i>Crisp</i> Inter & Consolas", NoteColor.SlateBlue));
        }

        private void OnAnimationTick(object? sender, EventArgs e)
        {
            bool needsRedraw = _cursorRenderModule.DecayTrail(SpentCells);
            if (needsRedraw)
            {
                InvalidateVisual();
            }
        }

        // Camera Transforms delegated to CameraModule
        public Point ScreenToWorld(Point screenPt) => Camera.ScreenToWorld(screenPt);

        public Point WorldToScreen(Point worldPt) => Camera.WorldToScreen(worldPt);

        public (int cellX, int cellY) WorldToCell(Point worldPt) => Camera.WorldToCell(worldPt, CellSize);

        public Point CellToWorld(int cellX, int cellY) => Camera.CellToWorld(cellX, cellY, CellSize);

        public Rect GetNoteScreenBounds(GridNote note)
        {
            Point worldTopLeft = Camera.CellToWorld(note.CellX, note.CellY, CellSize);
            Point screenTopLeft = Camera.WorldToScreen(worldTopLeft);
            double sizePx = note.SizeCells * CellSize * Camera.Zoom;
            return new Rect(screenTopLeft.X, screenTopLeft.Y, sizePx, sizePx);
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
                CameraChanged?.Invoke();
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
            Camera.ZoomAt(e.GetPosition(this), zoomFactor);

            InvalidateVisual();
            CameraChanged?.Invoke();
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

        public List<GridNote> GetSelectedNotes()
        {
            var list = new List<GridNote>();
            foreach (var note in Notes)
            {
                if (note.IsSelected)
                {
                    list.Add(note);
                }
            }
            if (list.Count == 0 && SelectedNote != null)
            {
                SelectedNote.IsSelected = true;
                list.Add(SelectedNote);
            }
            return list;
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
            var (startCX, startCY) = WorldToCell(_marqueeStartWorld);
            var (currCX, currCY) = WorldToCell(_marqueeCurrentWorld);

            int minCX = Math.Min(startCX, currCX);
            int maxCX = Math.Max(startCX, currCX);
            int minCY = Math.Min(startCY, currCY);
            int maxCY = Math.Max(startCY, currCY);

            // Cell-aligned rectangle in world coordinates
            double minWorldX = minCX * CellSize;
            double minWorldY = minCY * CellSize;
            double maxWorldX = (maxCX + 1) * CellSize;
            double maxWorldY = (maxCY + 1) * CellSize;

            Rect cellAlignedMarqueeWorld = new Rect(minWorldX, minWorldY, maxWorldX - minWorldX, maxWorldY - minWorldY);

            GridNote? lastSelected = null;
            foreach (var note in Notes)
            {
                Rect noteWorldRect = new Rect(note.CellX * CellSize, note.CellY * CellSize, note.SizeCells * CellSize, note.SizeCells * CellSize);
                note.IsSelected = cellAlignedMarqueeWorld.Intersects(noteWorldRect);
                if (note.IsSelected)
                {
                    lastSelected = note;
                }
            }
            SelectedNote = lastSelected;
        }

        // Field Ledger Engine Spatial Lookups
        public CellLedgerEntry GetCellLedger(int col, int row) => FieldEngine.GetCellLedger(col, row);
        public List<CellMetadataSource> QueryMetadataInRegion(Rect cellBounds) => FieldEngine.QueryMetadataInRegion(cellBounds);

        // High-Frequency Engine Pipeline Delegation
        public override void Render(DrawingContext context)
        {
            base.Render(context);

            double w = Bounds.Width;
            double h = Bounds.Height;
            if (w <= 0 || h <= 0) return;

            // Visible Cell Bounds in World Coordinates with 2-cell buffer (-CellSize * 2 to viewport + CellSize * 2)
            double bufferPx = CellSize * 2;
            int minCellX = (int)Math.Floor((-CameraX - bufferPx) / (CellSize * Zoom));
            int maxCellX = (int)Math.Ceiling((w - CameraX + bufferPx) / (CellSize * Zoom));
            int minCellY = (int)Math.Floor((-CameraY - bufferPx) / (CellSize * Zoom));
            int maxCellY = (int)Math.Ceiling((h - CameraY + bufferPx) / (CellSize * Zoom));

            // 1. Field Ledger Module: Gravitational Cell Fills, Atmosphere & Perimeter Rings
            _fieldLedgerModule.RenderFieldLedger(context, WorldToScreen, CellSize, Zoom, minCellX, maxCellX, minCellY, maxCellY, FieldEngine, Notes);

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

            var (startCX, startCY) = WorldToCell(_marqueeStartWorld);
            var (currCX, currCY) = WorldToCell(_marqueeCurrentWorld);

            int minCX = Math.Min(startCX, currCX);
            int maxCX = Math.Max(startCX, currCX);
            int minCY = Math.Min(startCY, currCY);
            int maxCY = Math.Max(startCY, currCY);

            Point startScreen = WorldToScreen(new Point(minCX * CellSize, minCY * CellSize));
            Point endScreen = WorldToScreen(new Point((maxCX + 1) * CellSize, (maxCY + 1) * CellSize));

            double x = startScreen.X;
            double y = startScreen.Y;
            double w = endScreen.X - startScreen.X;
            double h = endScreen.Y - startScreen.Y;

            Rect marqueeRect = new Rect(x, y, w, h);

            Color marqueeColor = Colors.SignalActiveWork;
            var fillBrush = new SolidColorBrush(Color.FromArgb((byte)(255 * 0.10), marqueeColor.R, marqueeColor.G, marqueeColor.B));
            var borderPen = new Pen(Colors.SignalActiveWorkBrush, Tokens.FieldPerimeterWidth, new DashStyle(new double[] { 4, 4 }, 0));

            context.FillRectangle(fillBrush, marqueeRect);
            context.DrawRectangle(null, borderPen, marqueeRect);
        }
    }
}
