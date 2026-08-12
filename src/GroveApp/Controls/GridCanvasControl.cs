using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    /// <summary>
    /// Plane 0: Spatial Grid Canvas Control (ADR-001, ADR-004).
    /// Continuous 2D spatial canvas rendering Notes, Documents, and Images.
    /// Strictly references normative Grove Design System tokens.
    /// </summary>
    public class GridCanvasControl : Control
    {
        public const double CellSize = Tokens.GridCell; // 220.0px
        public const double MinorCellSize = Tokens.MinorCellSize; // 44.0px

        // Engine Modules
        private readonly FieldLedgerModule _fieldLedgerModule = new();
        private readonly NoteRenderModule _noteRenderModule = new();
        private readonly CursorRenderModule _cursorRenderModule = new();
        private readonly GridLineModule _gridLineModule = new();
        public FieldLedgerEngine FieldEngine { get; } = new FieldLedgerEngine();

        // System Handlers (ADR-013 & ADR-014)
        public ExternalDragDropHandler DragDropHandler { get; private set; }
        public NativeClipboardService ClipboardService { get; private set; }

        // Camera Module & State
        public CameraModule Camera { get; } = new CameraModule();
        public double CameraX
        {
            get => Camera.CameraX;
            set
            {
                if (Math.Abs(Camera.CameraX - value) > 1e-6)
                {
                    Camera.CameraX = value;
                    InvalidateVisual();
                    CameraChanged?.Invoke();
                }
            }
        }

        public double CameraY
        {
            get => Camera.CameraY;
            set
            {
                if (Math.Abs(Camera.CameraY - value) > 1e-6)
                {
                    Camera.CameraY = value;
                    InvalidateVisual();
                    CameraChanged?.Invoke();
                }
            }
        }

        public double Zoom
        {
            get => Camera.Zoom;
            set
            {
                double clamped = Math.Clamp(value, CameraModule.MinZoom, CameraModule.MaxZoom);
                if (Math.Abs(Camera.Zoom - clamped) > 1e-6)
                {
                    Camera.Zoom = clamped;
                    InvalidateVisual();
                    CameraChanged?.Invoke();
                }
            }
        }

        // Pointer & Cursor State
        public Point MousePointerScreen { get; private set; }
        public Point MousePointerWorld { get; private set; }
        public int CursorCellX { get; private set; }
        public int CursorCellY { get; private set; }

        // Spent Cell Trail
        public List<SpentCell> SpentCells { get; } = new();
        private int _lastCursorCellX = int.MinValue;
        private int _lastCursorCellY = int.MinValue;

        // Content & Selection (GridContentItem Base Model)
        public List<GridContentItem> Items { get; } = new();
        public GridContentItem? SelectedItem { get; set; }
        public GridContentItem? HoveredItem { get; set; }

        // Backwards compatibility aliases for GridNote
        public List<GridNote> Notes => Items.OfType<GridNote>().ToList();
        public GridNote? SelectedNote
        {
            get => SelectedItem as GridNote;
            set => SelectedItem = value;
        }
        public GridNote? HoveredNote
        {
            get => HoveredItem as GridNote;
            set => HoveredItem = value;
        }

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

        // Drag & Drop Movement State
        private bool _isDraggingItem;
        private GridContentItem? _draggedItem;
        private int _dragOffsetCellX;
        private int _dragOffsetCellY;

        // Drag to Resize Corner State (ADR-010)
        private bool _isResizingItem;
        private GridContentItem? _resizingItem;

        // Native GPU VSync Render Loop State
        private TopLevel? _topLevel;
        private bool _isAnimationFrameRequested;

        // Events
        public event Action<GridContentItem>? ItemSelected;
        public event Action<GridNote>? NoteSelected;
        public event Action<GridNote>? NoteDoubleClicked;
        public event Action<int, int>? EmptyCellDoubleClicked;
        public event Action? CameraChanged;

        public GridCanvasControl()
        {
            ClipToBounds = true;
            Focusable = true;

            DragDropHandler = new ExternalDragDropHandler(
                (origin, w, h) => IsRegionFree(origin, w, h),
                async (newItem) =>
                {
                    Items.Add(newItem);
                    DeselectAllItems();
                    newItem.IsSelected = true;
                    SelectedItem = newItem;
                    InvalidateVisual();
                    await Task.CompletedTask;
                }
            );

            ClipboardService = new NativeClipboardService(() => TopLevel.GetTopLevel(this)?.Clipboard);

            SeedSampleData();
        }

        private void SeedSampleData()
        {
            Items.Add(new GridNote(0, 0, "# Field Ledger\n\n> Spatial grid canvas with **high-DPI** subpixel typography.\n\n- Inline `code` & <u>underline</u> & <del>strikethrough</del>\n- H<sub>2</sub>O and E=mc<sup>2</sup> formulas", NoteColor.Violet, isAnchored: true));
            Items.Add(new GridNote(3, 1, "## Code Engine Parity\n\n```cs\npublic class GridEngine {\n    public string Name { get; set; } = \"Grove\";\n    public bool IsActive() => true;\n}\n```", NoteColor.Clay));
            Items.Add(new GridNote(-2, 3, "### Spacetime Grid\n\n1. **Zero** global overhead\n2. [Primary signal](#E8B964) status\n3. <i>Crisp</i> Inter & Consolas", NoteColor.SlateBlue));
            Items.Add(new GridDocument(-4, -1, 2, 2, "Architecture Manifesto", "# Grove Architectural Principles\n\nContinuous spatial grid plane acting as Plane 0. Enforces physical paper proportions, whole-cell integral footprints, deterministic page texture rules, and multi-column AST text pagination."));
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            _topLevel = TopLevel.GetTopLevel(this);

            DragDropHandler.Attach(this, () => new Point(CameraX, CameraY), () => Zoom);

            RequestNextAnimationFrame();
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            _topLevel = null;
            _isAnimationFrameRequested = false;
        }

        private void RequestNextAnimationFrame()
        {
            if (_topLevel != null && !_isAnimationFrameRequested)
            {
                _isAnimationFrameRequested = true;
                _topLevel.RequestAnimationFrame(OnAnimationFrame);
            }
        }

        private void OnAnimationFrame(TimeSpan timeStamp)
        {
            _isAnimationFrameRequested = false;

            bool needsRedraw = _cursorRenderModule.DecayTrail(SpentCells);

            if (needsRedraw || SpentCells.Count > 0 || _isPanning || _isMarqueeSelecting || _isDraggingItem)
            {
                InvalidateVisual();
            }

            RequestNextAnimationFrame();
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

        public bool IsRegionFree(CellCoordinate origin, int width, int height, GridContentItem? ignoreItem = null)
        {
            foreach (var item in Items)
            {
                if (item == ignoreItem) continue;
                if (item.Intersects(origin.X, origin.Y, width, height))
                {
                    return false;
                }
            }
            return true;
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

            // Handle Drag-to-Resize Corner (ADR-010)
            if (_isResizingItem && _resizingItem != null)
            {
                int newCellWidth = Math.Max(1, cx - _resizingItem.CellX + 1);
                int newCellHeight = Math.Max(1, cy - _resizingItem.CellY + 1);

                if (_resizingItem is GridNote note)
                {
                    int newSize = Math.Max(1, Math.Max(newCellWidth, newCellHeight));
                    if (IsRegionFree(new CellCoordinate(note.CellX, note.CellY), newSize, newSize, note))
                    {
                        note.SizeCells = newSize;
                    }
                }
                else if (_resizingItem is GridDocument doc)
                {
                    int w = Math.Clamp(newCellWidth, 2, 8);
                    int h = Math.Clamp(newCellHeight, 2, 8);
                    if (IsRegionFree(new CellCoordinate(doc.CellX, doc.CellY), w, h, doc))
                    {
                        doc.CellWidth = w;
                        doc.CellHeight = h;
                    }
                }
                else if (_resizingItem is GridImage img)
                {
                    int w = Math.Max(1, newCellWidth);
                    int h = Math.Max(1, newCellHeight);
                    if (IsRegionFree(new CellCoordinate(img.CellX, img.CellY), w, h, img))
                    {
                        img.CellWidth = w;
                        img.CellHeight = h;
                    }
                }
                InvalidateVisual();
                return;
            }

            // Handle Drag & Drop Item Movement
            if (_isDraggingItem && _draggedItem != null)
            {
                int targetX = cx - _dragOffsetCellX;
                int targetY = cy - _dragOffsetCellY;
                if (IsRegionFree(new CellCoordinate(targetX, targetY), _draggedItem.CellWidth, _draggedItem.CellHeight, _draggedItem))
                {
                    _draggedItem.CellX = targetX;
                    _draggedItem.CellY = targetY;
                }
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
            GridContentItem? newHover = FindItemAtCell(cx, cy);
            if (newHover != HoveredItem)
            {
                if (HoveredItem != null) HoveredItem.IsHovered = false;
                HoveredItem = newHover;
                if (HoveredItem != null) HoveredItem.IsHovered = true;
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
                // Check for Corner Resize Handle hit on SelectedItem first
                if (SelectedItem != null)
                {
                    Point brScreen = WorldToScreen(new Point((SelectedItem.CellX + SelectedItem.CellWidth) * CellSize, (SelectedItem.CellY + SelectedItem.CellHeight) * CellSize));
                    Point mouseScreen = e.GetPosition(this);
                    if (Vector.Distance(mouseScreen, brScreen) <= 24.0)
                    {
                        _isResizingItem = true;
                        _resizingItem = SelectedItem;
                        e.Handled = true;
                        return;
                    }
                }

                Point clickWorld = ScreenToWorld(e.GetPosition(this));
                var (cx, cy) = WorldToCell(clickWorld);
                GridContentItem? hitItem = FindItemAtCell(cx, cy);

                if (hitItem != null)
                {
                    _isDraggingItem = true;
                    _draggedItem = hitItem;
                    _dragOffsetCellX = cx - hitItem.CellX;
                    _dragOffsetCellY = cy - hitItem.CellY;

                    SelectItem(hitItem);
                }
                else
                {
                    _isMarqueeSelecting = true;
                    _marqueeStartWorld = clickWorld;
                    _marqueeCurrentWorld = clickWorld;

                    DeselectAllItems();
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
            if (_isResizingItem)
            {
                _isResizingItem = false;
                _resizingItem = null;
                e.Handled = true;
            }
            if (_isDraggingItem)
            {
                _isDraggingItem = false;
                _draggedItem = null;
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
            GridContentItem? hitItem = FindItemAtCell(cx, cy);

            if (hitItem is GridNote hitNote)
            {
                NoteDoubleClicked?.Invoke(hitNote);
            }
            else if (hitItem == null)
            {
                EmptyCellDoubleClicked?.Invoke(cx, cy);
            }
        }

        public GridContentItem? FindItemAtCell(int cx, int cy)
        {
            foreach (var item in Items)
            {
                if (item.ContainsCell(cx, cy))
                {
                    return item;
                }
            }
            return null;
        }

        public GridNote? FindNoteAtCell(int cx, int cy)
        {
            return FindItemAtCell(cx, cy) as GridNote;
        }

        public List<GridContentItem> GetSelectedItems()
        {
            var list = new List<GridContentItem>();
            foreach (var item in Items)
            {
                if (item.IsSelected)
                {
                    list.Add(item);
                }
            }
            if (list.Count == 0 && SelectedItem != null)
            {
                SelectedItem.IsSelected = true;
                list.Add(SelectedItem);
            }
            return list;
        }

        public List<GridNote> GetSelectedNotes()
        {
            return GetSelectedItems().OfType<GridNote>().ToList();
        }

        private void SelectItem(GridContentItem item)
        {
            DeselectAllItems();
            SelectedItem = item;
            SelectedItem.IsSelected = true;
            ItemSelected?.Invoke(SelectedItem);
            if (item is GridNote note)
            {
                NoteSelected?.Invoke(note);
            }
        }

        private void SelectNote(GridNote note)
        {
            SelectItem(note);
        }

        public void DeselectAllItems()
        {
            if (SelectedItem != null) SelectedItem.IsSelected = false;
            SelectedItem = null;
            foreach (var item in Items) item.IsSelected = false;
        }

        public void DeselectAllNotes()
        {
            DeselectAllItems();
        }

        private void UpdateMarqueeSelection()
        {
            var (startCX, startCY) = WorldToCell(_marqueeStartWorld);
            var (currCX, currCY) = WorldToCell(_marqueeCurrentWorld);

            int minCX = Math.Min(startCX, currCX);
            int maxCX = Math.Max(startCX, currCX);
            int minCY = Math.Min(startCY, currCY);
            int maxCY = Math.Max(startCY, currCY);

            double minWorldX = minCX * CellSize;
            double minWorldY = minCY * CellSize;
            double maxWorldX = (maxCX + 1) * CellSize;
            double maxWorldY = (maxCY + 1) * CellSize;

            Rect cellAlignedMarqueeWorld = new Rect(minWorldX, minWorldY, maxWorldX - minWorldX, maxWorldY - minWorldY);

            GridContentItem? lastSelected = null;
            foreach (var item in Items)
            {
                Rect itemWorldRect = new Rect(item.CellX * CellSize, item.CellY * CellSize, item.CellWidth * CellSize, item.CellHeight * CellSize);
                item.IsSelected = cellAlignedMarqueeWorld.Intersects(itemWorldRect);
                if (item.IsSelected)
                {
                    lastSelected = item;
                }
            }
            SelectedItem = lastSelected;
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
            _fieldLedgerModule.RenderFieldLedger(context, WorldToScreen, CellSize, Zoom, minCellX, maxCellX, minCellY, maxCellY, FieldEngine, Items);

            // 2. Grid Line Module: Major 220px, Minor 44px Subdivisions & LOD Fading
            _gridLineModule.RenderGridLines(context, WorldToScreen, CellSize, Zoom, minCellX, maxCellX, minCellY, maxCellY);

            // 3. Note & Content Render Module: Authored Fills, Containment Edge, Padding & Affordances
            _noteRenderModule.RenderContentItems(context, WorldToScreen, CellSize, Zoom, minCellX, maxCellX, minCellY, maxCellY, Items, SelectedItem, HoveredItem);

            // 4. Cursor Render Module: Spent Cell Decay Trail Physics
            _cursorRenderModule.RenderSpentTrail(context, WorldToScreen, CellSize, Zoom, SpentCells);

            // 5. Cursor Render Module: Grid Cursor Head, 22% Fill & Inset 2px Ring
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
