using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using GroveApp.DesignSystem;
using GroveApp.Engine;
using GroveApp.Engine.Memory;
using GroveApp.Engine.Interaction;
using GroveApp.Models;
using GroveApp.Models.Memory;
using GroveApp.Models.Interaction;
using CellCoordinate = GroveApp.Engine.CellCoordinate;
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
        public IMemoryLedger MemoryLedger { get; } = new ImmutableMemoryLedger();
        public IMemoryVersionTree MemoryVersionTree { get; } = new MemoryVersionTree();
        public MemorySpatialIndex MemorySpatialIndex { get; } = new();
        public IMemoryAnchorStore MemoryAnchorStore { get; } = new MemoryAnchorFileStore();
        public string MemoryAnchorFilePath { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Grove",
            "anchors.yml");
        public SpatialLayerStack LayerStack { get; } = new SpatialLayerStack();
        public LayerActivationManager LayerActivation { get; }
        public ToolArmingStateMachine Arming { get; }
        public LayerFeedbackAnimationController LayerFeedback { get; } = new();

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
                    RefreshFieldLedger();
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
                    RefreshFieldLedger();
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
                    RefreshFieldLedger();
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
        private List<GridContentItem> _dragCluster = new();
        private Dictionary<GridContentItem, (int cellX, int cellY)> _dragInitialPositions = new();
        private int _dragCandidateDeltaX;
        private int _dragCandidateDeltaY;
        private bool _dragCandidateIsValid;

        // Drag to Resize Corner State (ADR-010)
        private bool _isResizingItem;
        private GridContentItem? _resizingItem;
        private ResizeHandleLocation _resizeHandle;
        private SpatialRegion _resizeInitialFootprint;
        private bool _resizeCandidateIsValid;

        // Native GPU VSync Render Loop State
        private TopLevel? _topLevel;
        private bool _isAnimationFrameRequested;
        private TimeSpan _lastAnimationTimestamp;
        private int _knownLayerCount;
        private DateTime _placementRejectedUntilUtc;

        // Events
        public event Action<GridContentItem>? ItemSelected;
        public event Action<GridNote>? NoteSelected;
        public event Action<GridNote>? NoteDoubleClicked;
        public event Action<GridContentItem>? ContentDoubleClicked;
        public event Action<int, int>? EmptyCellDoubleClicked;
        public event Action? CameraChanged;
        public event Action<GridContentItem, ArmableContentType>? ArmedItemPlaced;
        public event Action<Exception>? MemoryAnchorPersistenceFailed;

        public GridCanvasControl()
        {
            ClipToBounds = true;
            Focusable = true;

            LayerActivation = new LayerActivationManager(LayerStack);
            LayerActivation.StateChanged += () =>
            {
                RefreshFieldLedger();
                InvalidateVisual();
            };

            Arming = new ToolArmingStateMachine(
                (origin, width, height, layerId) => IsRegionFree(origin, width, height, ignoreItem: null, layerId: layerId));
            Arming.GhostPreviewUpdated += _ => InvalidateVisual();
            Arming.PlacementRejected += OnPlacementRejected;
            FieldEngine.LayerDeltaResolver = ResolveLayerDelta;
            FieldEngine.ActiveLayerId = LayerStack.ActiveLayerId;
            LayerStack.ItemCountProvider = layerId => Items.Count(item => item.LayerId == layerId);
            _knownLayerCount = LayerStack.Layers.Count;
            LayerStack.ActiveLayerChanged += _ =>
            {
                FieldEngine.ActiveLayerId = LayerStack.ActiveLayerId;
                RefreshFieldLedger();
                InvalidateVisual();
            };
            LayerStack.LayerStackChanged += OnLayerStackChanged;
            LayerStack.LayerItemsTransferRequested += (fromLayerId, toLayerId) =>
            {
                foreach (var item in Items)
                {
                    if (item.LayerId == fromLayerId)
                    {
                        RemoveMemoryAnchor(item);
                        item.LayerId = toLayerId;
                        AddMemoryAnchor(item);
                    }
                }
                RefreshFieldLedger();
                InvalidateVisual();
            };

            DragDropHandler = new ExternalDragDropHandler(
                (origin, w, h) => IsRegionFree(origin, w, h),
                async (newItem) =>
                {
                    newItem.LayerId = LayerStack.ActiveLayerId;
                    AddItem(newItem);
                    DeselectAllItems();
                    newItem.IsSelected = true;
                    SelectedItem = newItem;
                    RefreshFieldLedger();
                    InvalidateVisual();
                    await Task.CompletedTask;
                }
            );
            DragDropHandler.PreviewChanged += InvalidateVisual;

            ClipboardService = new NativeClipboardService(() => TopLevel.GetTopLevel(this)?.Clipboard);

            SeedSampleData();
            SizeChanged += OnCanvasSizeChanged;
        }

        private void OnCanvasSizeChanged(object? sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width <= 0 || e.NewSize.Height <= 0)
            {
                return;
            }

            // The visual-tree attachment can precede layout. Rebuild the field
            // snapshot when the canvas first receives usable bounds so the first
            // render does not depend on a later pointer or selection interaction.
            RefreshFieldLedger();
            InvalidateVisual();
        }

        private void SeedSampleData()
        {
            AddItem(new GridNote(0, 0, "# Field Ledger\n\n> Spatial grid canvas with **high-DPI** subpixel typography.\n\n- Inline `code` & <u>underline</u> & <del>strikethrough</del>\n- H<sub>2</sub>O and E=mc<sup>2</sup> formulas", NoteColor.Violet, isAnchored: true));
            AddItem(new GridNote(3, 1, "## Code Engine Parity\n\n```cs\npublic class GridEngine {\n    public string Name { get; set; } = \"Grove\";\n    public bool IsActive() => true;\n}\n```", NoteColor.Clay));
            AddItem(new GridNote(-2, 3, "### Spacetime Grid\n\n1. **Zero** global overhead\n2. [Primary signal](#E8B964) status\n3. <i>Crisp</i> Inter & Consolas", NoteColor.SlateBlue));
            AddItem(new GridDocument(-4, -1, 2, 2, "Architecture Manifesto", "# Grove Architectural Principles\n\nContinuous spatial grid plane acting as Plane 0. Enforces physical paper proportions, whole-cell integral footprints, deterministic page texture rules, and multi-column AST text pagination."));
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            _topLevel = TopLevel.GetTopLevel(this);

            DragDropHandler.Attach(this, () => new Point(CameraX, CameraY), () => Zoom);

            RefreshFieldLedger();
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

            TimeSpan frameDelta = _lastAnimationTimestamp == TimeSpan.Zero
                ? TimeSpan.FromMilliseconds(16.67)
                : timeStamp - _lastAnimationTimestamp;
            _lastAnimationTimestamp = timeStamp;
            bool feedbackActive = LayerFeedback.State.IsActive;
            bool rejectionPulseActive = DateTime.UtcNow < _placementRejectedUntilUtc;
            if (feedbackActive)
            {
                LayerFeedback.Advance(frameDelta < TimeSpan.Zero ? TimeSpan.Zero : frameDelta);
            }

            bool needsRedraw = _cursorRenderModule.DecayTrail(SpentCells);

            if (needsRedraw || SpentCells.Count > 0 || feedbackActive || rejectionPulseActive || _isPanning || _isMarqueeSelecting || _isDraggingItem)
            {
                InvalidateVisual();
            }

            RequestNextAnimationFrame();
        }

        // Camera Transforms delegated to CameraModule
        public Point ScreenToWorld(Point screenPt) => Camera.ScreenToWorld(screenPt);

        public Point WorldToScreen(Point worldPt) => Camera.WorldToScreen(worldPt);

        public (int cellX, int cellY) WorldToCell(Point worldPt) => Camera.WorldToCell(worldPt, CellSize);

        /// <summary>
        /// Rebuilds the visible field snapshot outside the Skia render pass.
        /// Mutations and viewport changes call this seam before invalidating visuals.
        /// </summary>
        public void RefreshFieldLedger()
        {
            if (Bounds.Width <= 0 || Bounds.Height <= 0 || Items.Count == 0)
            {
                if (Items.Count == 0)
                {
                    FieldEngine.Clear();
                }
                return;
            }

            FieldEngine.ActiveLayerId = LayerStack.ActiveLayerId;
            double bufferPx = CellSize * 2;
            int minCellX = (int)Math.Floor((-CameraX - bufferPx) / (CellSize * Zoom));
            int maxCellX = (int)Math.Ceiling((Bounds.Width - CameraX + bufferPx) / (CellSize * Zoom));
            int minCellY = (int)Math.Floor((-CameraY - bufferPx) / (CellSize * Zoom));
            int maxCellY = (int)Math.Ceiling((Bounds.Height - CameraY + bufferPx) / (CellSize * Zoom));
            var visibleItems = Items
                .Where(item => LayerActivation.GetRenderMode(item.LayerId) != LayerRenderMode.Hidden)
                .Where(item => !LayerActivation.IsIsolationModeEnabled || item.LayerId == LayerStack.ActiveLayerId)
                .ToList();

            var activeAuraCells = FieldLedgerEngine.GetAuraCells(
                visibleItems, minCellX, maxCellX, minCellY, maxCellY);

            FieldEngine.RecalculateField(visibleItems, activeAuraCells);
        }

        private int ResolveLayerDelta(int sourceLayerId, int targetLayerId)
        {
            int sourceIndex = LayerStack.GetStackIndex(sourceLayerId);
            int targetIndex = LayerStack.GetStackIndex(targetLayerId);
            return sourceIndex >= 0 && targetIndex >= 0
                ? Math.Abs(sourceIndex - targetIndex)
                : Math.Abs(sourceLayerId - targetLayerId);
        }

        private HashSet<Guid> QueryIndexedMemoryIds(int minCellX, int maxCellX, int minCellY, int maxCellY)
        {
            int capacity = Math.Max(1, MemorySpatialIndex.Count);
            MemoryAnchor[] rented = ArrayPool<MemoryAnchor>.Shared.Rent(capacity);
            try
            {
                var query = new SpatialBoundingBox(
                    Math.Max(int.MinValue + 8, minCellX - 8),
                    Math.Max(int.MinValue + 8, minCellY - 8),
                    Math.Min(int.MaxValue - 8, maxCellX + 8),
                    Math.Min(int.MaxValue - 8, maxCellY + 8),
                    Guid.Empty);
                int count = MemorySpatialIndex.QueryBoundingBox(query, rented.AsSpan());
                var ids = new HashSet<Guid>();
                for (int i = 0; i < count; i++)
                {
                    ids.Add(rented[i].MemoryId);
                }

                return ids;
            }
            finally
            {
                ArrayPool<MemoryAnchor>.Shared.Return(rented, clearArray: true);
            }
        }

        private static bool IsIndexedForViewport(GridContentItem item, HashSet<Guid> indexedMemoryIds) =>
            item.MemoryId is not Guid memoryId || indexedMemoryIds.Contains(memoryId);

        public Point CellToWorld(int cellX, int cellY) => Camera.CellToWorld(cellX, cellY, CellSize);

        public Rect GetNoteScreenBounds(GridNote note)
        {
            Point worldTopLeft = Camera.CellToWorld(note.CellX, note.CellY, CellSize);
            Point screenTopLeft = Camera.WorldToScreen(worldTopLeft);
            double sizePx = note.SizeCells * CellSize * Camera.Zoom;
            return new Rect(screenTopLeft.X, screenTopLeft.Y, sizePx, sizePx);
        }

        public Rect GetContentScreenBounds(GridContentItem item)
        {
            Point worldTopLeft = Camera.CellToWorld(item.CellX, item.CellY, CellSize);
            Point screenTopLeft = Camera.WorldToScreen(worldTopLeft);
            return new Rect(
                screenTopLeft.X,
                screenTopLeft.Y,
                item.CellWidth * CellSize * Camera.Zoom,
                item.CellHeight * CellSize * Camera.Zoom);
        }

        public bool IsRegionFree(CellCoordinate origin, int width, int height, GridContentItem? ignoreItem = null)
        {
            return IsRegionFree(origin, width, height, ignoreItem, null);
        }

        public void MoveCursorToCell(int cellX, int cellY)
        {
            CursorCellX = cellX;
            CursorCellY = cellY;
            if (Arming.IsArmed)
            {
                Arming.UpdateCursorPosition(new CellCoordinate(cellX, cellY), LayerStack.ActiveLayerId);
            }
            InvalidateVisual();
        }

        /// <summary>
        /// Commits a placement and refreshes the prepared field snapshot.
        /// </summary>
        public void AddItem(GridContentItem item)
        {
            ArgumentNullException.ThrowIfNull(item);
            Items.Add(item);
            try
            {
                EnsureMemoryRecord(item);
            }
            catch
            {
                Items.Remove(item);
                if (item is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                throw;
            }
            RefreshFieldLedger();
        }

        /// <summary>
        /// Removes a placement through the ownership seam so image resources are released.
        /// </summary>
        public bool RemoveItem(GridContentItem item)
        {
            ArgumentNullException.ThrowIfNull(item);
            if (!Items.Remove(item))
            {
                return false;
            }

            if (ReferenceEquals(SelectedItem, item))
            {
                SelectedItem = null;
            }

            if (item is IDisposable disposable)
            {
                disposable.Dispose();
            }

            RemoveMemoryAnchor(item);

            RefreshFieldLedger();
            InvalidateVisual();
            return true;
        }

        public void UpdateMemoryForItem(GridContentItem item)
        {
            ArgumentNullException.ThrowIfNull(item);
            EnsureMemoryRecord(item, forceNewVersion: true);
            RefreshFieldLedger();
        }

        private void EnsureMemoryRecord(GridContentItem item, bool forceNewVersion = false)
        {
            if (item.MemoryId.HasValue && !forceNewVersion)
            {
                AddMemoryAnchor(item);
                return;
            }

            Guid? parentId = forceNewVersion ? item.MemoryId : null;
            MemoryPayloadKind payloadKind = GridContentMemoryPayloadAdapter.GetKind(item);
            byte[] payload = GridContentMemoryPayloadAdapter.ReadPayload(item);
            MemoryRecord record = MemoryLedger.AppendMemory(payloadKind, payload, parentId);
            MemoryVersionTree.AddVersionNode(record);
            RemoveMemoryAnchor(item);
            item.MemoryId = record.MemoryId;
            AddMemoryAnchor(item);
        }

        private void AddMemoryAnchor(GridContentItem item)
        {
            if (!item.MemoryId.HasValue)
            {
                return;
            }

            var anchor = new MemoryAnchor
            {
                AnchorId = item.MemoryId.Value,
                MemoryId = item.MemoryId.Value,
                LayerId = LayerGuid(item.LayerId),
                CellX = item.CellX,
                CellY = item.CellY,
                ContextLabel = item.Id,
                CreatedAtTicks = DateTime.UtcNow.Ticks
            };
            MemorySpatialIndex.Insert(anchor);
            PersistMemoryAnchors();
        }

        private void RemoveMemoryAnchor(GridContentItem item)
        {
            if (item.MemoryId is Guid memoryId)
            {
                MemorySpatialIndex.Remove(memoryId, out _);
                PersistMemoryAnchors();
            }
        }

        private void PersistMemoryAnchors()
        {
            MemoryAnchor[] snapshot = MemorySpatialIndex.Snapshot();
            _ = PersistMemoryAnchorsAsync(snapshot);
        }

        private async Task PersistMemoryAnchorsAsync(MemoryAnchor[] snapshot)
        {
            try
            {
                await MemoryAnchorStore.SaveAsync(MemoryAnchorFilePath, snapshot).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                MemoryAnchorPersistenceFailed?.Invoke(exception);
            }
        }

        private static Guid LayerGuid(int layerId) => new(layerId, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        public bool IsRegionFree(
            CellCoordinate origin,
            int width,
            int height,
            GridContentItem? ignoreItem,
            int? layerId)
        {
            int targetLayerId = layerId ?? LayerStack.ActiveLayerId;
            foreach (var item in Items)
            {
                if (item == ignoreItem) continue;
                if (item.LayerId != targetLayerId) continue;
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

            if (Arming.IsArmed)
            {
                Arming.UpdateCursorPosition(new CellCoordinate(cx, cy), LayerStack.ActiveLayerId);
                InvalidateVisual();
                return;
            }

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
                int handleOriginX = _resizeHandle is ResizeHandleLocation.NorthEast or ResizeHandleLocation.SouthEast
                    ? _resizeInitialFootprint.Right - 1
                    : _resizeInitialFootprint.X;
                int handleOriginY = _resizeHandle is ResizeHandleLocation.SouthWest or ResizeHandleLocation.SouthEast
                    ? _resizeInitialFootprint.Bottom - 1
                    : _resizeInitialFootprint.Y;
                int deltaX = cx - handleOriginX;
                int deltaY = cy - handleOriginY;
                int intrinsicWidth = _resizingItem is GridImage image ? image.IntrinsicWidthPx : 0;
                int intrinsicHeight = _resizingItem is GridImage image2 ? image2.IntrinsicHeightPx : 0;
                SpatialRegion candidate = TypeSpecificFootprintSolver.SolveFootprint(
                    _resizingItem.Kind,
                    _resizeInitialFootprint,
                    _resizeHandle,
                    deltaX,
                    deltaY,
                    intrinsicWidth,
                    intrinsicHeight);
                _resizeCandidateIsValid = IsRegionFree(
                    new CellCoordinate(candidate.X, candidate.Y),
                    candidate.Width,
                    candidate.Height,
                    _resizingItem,
                    _resizingItem.LayerId);

                if (_resizeCandidateIsValid)
                {
                    _resizingItem.CellX = candidate.X;
                    _resizingItem.CellY = candidate.Y;
                    _resizingItem.CellWidth = candidate.Width;
                    _resizingItem.CellHeight = candidate.Height;
                    if (_resizingItem is GridNote note)
                    {
                        note.SizeCells = candidate.Width;
                    }
                }
                RefreshFieldLedger();
                InvalidateVisual();
                return;
            }

            // Handle Drag & Drop Item Movement
            if (_isDraggingItem && _draggedItem != null)
            {
                int targetX = cx - _dragOffsetCellX;
                int targetY = cy - _dragOffsetCellY;

                _dragCandidateDeltaX = targetX - _dragInitialPositions[_draggedItem].cellX;
                _dragCandidateDeltaY = targetY - _dragInitialPositions[_draggedItem].cellY;
                _dragCandidateIsValid = IsClusterRegionFree(_dragCandidateDeltaX, _dragCandidateDeltaY);

                if (_dragCandidateIsValid)
                {
                    ApplyDragDelta(_dragCandidateDeltaX, _dragCandidateDeltaY);
                    RefreshFieldLedger();
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
                if (props.IsRightButtonPressed && Arming.IsArmed)
                {
                    DisarmTool();
                    e.Handled = true;
                    return;
                }

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
                if (Arming.IsArmed)
                {
                    TryPlaceArmedItem();
                    e.Handled = true;
                    return;
                }

                // Check for Corner Resize Handle hit on SelectedItem first
                if (SelectedItem != null)
                {
                    if (LayerStack.GetLayer(SelectedItem.LayerId).IsLocked)
                    {
                        e.Handled = true;
                        return;
                    }

                    Point mouseScreen = e.GetPosition(this);
                    if (TryGetResizeHandle(SelectedItem, mouseScreen, out ResizeHandleLocation handle))
                    {
                        _isResizingItem = true;
                        _resizingItem = SelectedItem;
                        _resizeHandle = handle;
                        _resizeInitialFootprint = new SpatialRegion(
                            SelectedItem.CellX,
                            SelectedItem.CellY,
                            SelectedItem.CellWidth,
                            SelectedItem.CellHeight);
                        _resizeCandidateIsValid = true;
                        e.Handled = true;
                        return;
                    }
                }

                Point clickWorld = ScreenToWorld(e.GetPosition(this));
                var (cx, cy) = WorldToCell(clickWorld);
            GridContentItem? hitItem = FindItemAtCell(cx, cy);

            if (hitItem != null)
            {
                if (LayerStack.GetLayer(hitItem.LayerId).IsLocked)
                {
                    e.Handled = true;
                    return;
                }

                _isDraggingItem = true;
                _draggedItem = hitItem;
                _dragOffsetCellX = cx - hitItem.CellX;
                _dragOffsetCellY = cy - hitItem.CellY;

                bool preserveCluster = hitItem.IsSelected && GetSelectedItems().Count > 1;
                if (preserveCluster)
                {
                    SelectedItem = hitItem;
                }
                else
                {
                    SelectItem(hitItem);
                }

                _dragCluster = GetSelectedItems();
                _dragInitialPositions = _dragCluster.ToDictionary(
                    item => item,
                    item => (item.CellX, item.CellY));
                _dragCandidateDeltaX = 0;
                _dragCandidateDeltaY = 0;
                _dragCandidateIsValid = true;
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
                if (_resizingItem != null && _resizeCandidateIsValid)
                {
                    RemoveMemoryAnchor(_resizingItem);
                    AddMemoryAnchor(_resizingItem);
                }
                _isResizingItem = false;
                _resizingItem = null;
                _resizeHandle = ResizeHandleLocation.None;
                _resizeInitialFootprint = default;
                _resizeCandidateIsValid = false;
                e.Handled = true;
            }
            if (_isDraggingItem)
            {
                foreach (GridContentItem item in _dragCluster)
                {
                    var origin = _dragInitialPositions[item];
                    if (origin.cellX == item.CellX && origin.cellY == item.CellY)
                    {
                        continue;
                    }

                    EmitTranslationTrail(origin.cellX, origin.cellY, item);
                    RemoveMemoryAnchor(item);
                    AddMemoryAnchor(item);
                }

                _isDraggingItem = false;
                _draggedItem = null;
                _dragCluster.Clear();
                _dragInitialPositions.Clear();
                _dragCandidateDeltaX = 0;
                _dragCandidateDeltaY = 0;
                _dragCandidateIsValid = false;
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
            else if (hitItem != null)
            {
                ContentDoubleClicked?.Invoke(hitItem);
            }
            else if (hitItem == null)
            {
                EmptyCellDoubleClicked?.Invoke(cx, cy);
            }
        }

        private bool TryGetResizeHandle(
            GridContentItem item,
            Point pointerScreen,
            out ResizeHandleLocation handle)
        {
            Point topLeft = WorldToScreen(new Point(item.CellX * CellSize, item.CellY * CellSize));
            Point bottomRight = WorldToScreen(new Point(
                (item.CellX + item.CellWidth) * CellSize,
                (item.CellY + item.CellHeight) * CellSize));
            double tolerance = 12.0;

            var candidates = new[]
            {
                (ResizeHandleLocation.NorthWest, new Point(topLeft.X, topLeft.Y)),
                (ResizeHandleLocation.NorthEast, new Point(bottomRight.X, topLeft.Y)),
                (ResizeHandleLocation.SouthEast, new Point(bottomRight.X, bottomRight.Y)),
                (ResizeHandleLocation.SouthWest, new Point(topLeft.X, bottomRight.Y))
            };

            foreach (var candidate in candidates)
            {
                if (Vector.Distance(pointerScreen, candidate.Item2) <= tolerance)
                {
                    handle = candidate.Item1;
                    return true;
                }
            }

            handle = ResizeHandleLocation.None;
            return false;
        }

        public GridContentItem? FindItemAtCell(int cx, int cy)
        {
            if (!LayerStack.GetLayer(LayerStack.ActiveLayerId).IsVisible)
            {
                return null;
            }

            foreach (var item in Items)
            {
                if (item.LayerId != LayerStack.ActiveLayerId) continue;
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
                if (item.LayerId != LayerStack.ActiveLayerId) continue;
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

        public bool IsToolArmed => Arming.IsArmed;

        public string ActiveLayerLabel => LayerStack.ActiveLayer.StableLabel;

        public bool ArmTool(ArmableContentType contentType)
        {
            SpatialLayer activeLayer = LayerStack.ActiveLayer;
            if (activeLayer.IsLocked || !activeLayer.IsVisible)
            {
                return false;
            }

            Arming.ArmTool(
                contentType,
                new CellCoordinate(CursorCellX, CursorCellY),
                LayerStack.ActiveLayerId);
            ActiveTool = contentType switch
            {
                ArmableContentType.Note => "NOTE",
                ArmableContentType.QuickNote => "QUICKNOTE",
                ArmableContentType.Document => "DOCUMENT",
                _ => "SELECT"
            };
            InvalidateVisual();
            return true;
        }

        public void DisarmTool()
        {
            Arming.Disarm();
            ActiveTool = "SELECT";
            InvalidateVisual();
        }

        public bool NavigateLayer(int direction)
        {
            bool changed = LayerStack.Navigate(direction);
            if (changed)
            {
                Arming.UpdateCursorPosition(
                    new CellCoordinate(CursorCellX, CursorCellY),
                    LayerStack.ActiveLayerId);
                InvalidateVisual();
            }
            return changed;
        }

        public List<GridNote> GetSelectedNotes()
        {
            return GetSelectedItems().OfType<GridNote>().ToList();
        }

        public void SelectOnly(GridContentItem item)
        {
            ArgumentNullException.ThrowIfNull(item);
            SelectItem(item);
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

        private bool IsClusterRegionFree(int deltaX, int deltaY)
        {
            if (_dragCluster.Count <= 1)
            {
                if (_draggedItem == null || !_dragInitialPositions.TryGetValue(_draggedItem, out var origin))
                {
                    return true;
                }

                return IsRegionFree(
                    new CellCoordinate(origin.cellX + deltaX, origin.cellY + deltaY),
                    _draggedItem.CellWidth,
                    _draggedItem.CellHeight,
                    _draggedItem,
                    _draggedItem.LayerId);
            }

            if (_dragCluster.Any(item => item.IsAnchored))
            {
                return false;
            }

            var cluster = new HashSet<GridContentItem>(_dragCluster);
            foreach (var movingItem in _dragCluster)
            {
                var origin = _dragInitialPositions[movingItem];
                int targetX = origin.cellX + deltaX;
                int targetY = origin.cellY + deltaY;

                foreach (var candidate in Items)
                {
                    if (cluster.Contains(candidate) || candidate.LayerId != movingItem.LayerId)
                    {
                        continue;
                    }

                    if (candidate.Intersects(targetX, targetY, movingItem.CellWidth, movingItem.CellHeight))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void ApplyDragDelta(int deltaX, int deltaY)
        {
            foreach (var item in _dragCluster)
            {
                var origin = _dragInitialPositions[item];
                item.CellX = origin.cellX + deltaX;
                item.CellY = origin.cellY + deltaY;
            }
        }

        private void TryPlaceArmedItem()
        {
            if (!Arming.TryCommit(out var descriptor))
            {
                return;
            }

            GridContentItem item = descriptor.ContentType switch
            {
                ArmableContentType.Note => new GridNote(
                    descriptor.OriginCell.X,
                    descriptor.OriginCell.Y,
                    "New Note",
                    NoteColor.Violet,
                    layerId: descriptor.LayerId),
                ArmableContentType.QuickNote => new GridNote(
                    descriptor.OriginCell.X,
                    descriptor.OriginCell.Y,
                    "",
                    NoteColor.Violet,
                    layerId: descriptor.LayerId),
                ArmableContentType.Document => new GridDocument(
                    descriptor.OriginCell.X,
                    descriptor.OriginCell.Y,
                    2,
                    2,
                    "New Document",
                    "",
                    layerId: descriptor.LayerId),
                _ => throw new ArgumentOutOfRangeException()
            };

            AddItem(item);
            DeselectAllItems();
            item.IsSelected = true;
            SelectedItem = item;
            RefreshFieldLedger();
            ArmedItemPlaced?.Invoke(item, descriptor.ContentType);
            Arming.CompletePlacement();
            ActiveTool = "SELECT";
            InvalidateVisual();
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
                if (item.LayerId != LayerStack.ActiveLayerId)
                {
                    item.IsSelected = false;
                    continue;
                }
                Rect itemWorldRect = new Rect(item.CellX * CellSize, item.CellY * CellSize, item.CellWidth * CellSize, item.CellHeight * CellSize);
                double overlapWidth = Math.Max(0, Math.Min(cellAlignedMarqueeWorld.Right, itemWorldRect.Right) - Math.Max(cellAlignedMarqueeWorld.Left, itemWorldRect.Left));
                double overlapHeight = Math.Max(0, Math.Min(cellAlignedMarqueeWorld.Bottom, itemWorldRect.Bottom) - Math.Max(cellAlignedMarqueeWorld.Top, itemWorldRect.Top));
                double itemArea = itemWorldRect.Width * itemWorldRect.Height;
                double overlapRatio = itemArea <= 0 ? 0 : overlapWidth * overlapHeight / itemArea;
                item.IsSelected = overlapRatio >= 0.5;
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
            var visibleItems = Items
                .Where(item => LayerActivation.GetRenderMode(item.LayerId) != LayerRenderMode.Hidden)
                .Where(item => !LayerActivation.IsIsolationModeEnabled || item.LayerId == LayerStack.ActiveLayerId)
                .ToList();
            _fieldLedgerModule.RenderFieldLedger(context, WorldToScreen, CellSize, Zoom, minCellX, maxCellX, minCellY, maxCellY, FieldEngine, visibleItems);

            // 2. Grid Line Module: Major 220px, Minor 44px Subdivisions & LOD Fading
            double renderScaling = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1.0;
            _gridLineModule.RenderGridLines(
                context,
                WorldToScreen,
                CellSize,
                Zoom,
                minCellX,
                maxCellX,
                minCellY,
                maxCellY,
                SpatialGridGeometryConfig.Default with { CellSize = CellSize },
                renderScaling);

            // 3. Note & Content Render Module: Inactive layer outlines & active layer items.
            // Inactive content is presence-only: it must not paint a second full document
            // or image into the active plane, and it must not participate in hit testing.
            HashSet<Guid> indexedMemoryIds = QueryIndexedMemoryIds(minCellX, maxCellX, minCellY, maxCellY);
            var inactiveItems = LayerActivation.IsIsolationModeEnabled
                ? new List<GridContentItem>()
                : Items.Where(item =>
                    IsIndexedForViewport(item, indexedMemoryIds) &&
                    item.LayerId != LayerStack.ActiveLayerId &&
                    LayerStack.GetLayer(item.LayerId).IsVisible).ToList();
            int activeZ = LayerStack.GetZIndexForLayerId(LayerStack.ActiveLayerId);
            foreach (var inactiveItem in inactiveItems)
            {
                int itemZ = LayerStack.GetZIndexForLayerId(inactiveItem.LayerId);
                double ghostOpacity = 0.15 * Math.Pow(0.5, Math.Abs(itemZ - activeZ));
                _noteRenderModule.RenderGhostOutline(
                    context,
                    WorldToScreen,
                    CellSize,
                    Zoom,
                    minCellX,
                    maxCellX,
                    minCellY,
                    maxCellY,
                    inactiveItem,
                    ghostOpacity);
            }

            var activeItems = Items.Where(item =>
                IsIndexedForViewport(item, indexedMemoryIds) &&
                item.LayerId == LayerStack.ActiveLayerId &&
                LayerStack.GetLayer(item.LayerId).IsVisible).ToList();
            _noteRenderModule.RenderContentItems(context, WorldToScreen, CellSize, Zoom, minCellX, maxCellX, minCellY, maxCellY, activeItems, SelectedItem, HoveredItem);

            RenderResizeRefusal(context);

            // 4. Cursor Render Module: Spent Cell Decay Trail Physics
            _cursorRenderModule.RenderSpentTrail(context, WorldToScreen, CellSize, Zoom, SpentCells);

            // 5. Cursor Render Module: Grid Cursor Head, 22% Fill & Inset 2px Ring
            GridContentItem? targetItem = FindItemAtCell(CursorCellX, CursorCellY);
            _cursorRenderModule.RenderGridCursor(context, WorldToScreen, CellSize, Zoom, CursorCellX, CursorCellY, targetItem);

            // 6. Marquee Selection Box Sweep
            RenderMarqueeSelection(context);

            // 7. Transaction preview for a multi-item rigid translation.
            RenderGroupDragPreview(context);

            // 8. Cell-aligned arming ghost.
            RenderArmingGhost(context);

            // 9. Cell-aligned OS drag placement preview.
            RenderDropPlacementPreview(context);

            // 10. Discrete layer insertion feedback sweep (no cross-cell blur).
            RenderLayerFeedback(context);
        }

        private void RenderDropPlacementPreview(DrawingContext context)
        {
            DropPlacementPreview? preview = DragDropHandler.CurrentPreview;
            if (preview is null)
            {
                return;
            }

            DropPlacementPreview value = preview.Value;
            Color signal = value.IsValid ? Colors.SignalInteraction : Colors.SignalRefusal;
            var fill = new SolidColorBrush(Color.FromArgb(32, signal.R, signal.G, signal.B));
            var pen = new Pen(new SolidColorBrush(Color.FromArgb(210, signal.R, signal.G, signal.B)), Tokens.StrokeState);
            Point screen = WorldToScreen(CellToWorld(value.Origin.X, value.Origin.Y));
            var rect = new Rect(
                screen.X,
                screen.Y,
                value.Width * CellSize * Zoom,
                value.Height * CellSize * Zoom);
            context.FillRectangle(fill, rect);
            context.DrawRectangle(null, pen, rect.Deflate(Tokens.StrokeState / 2.0));
        }

        private void RenderLayerFeedback(DrawingContext context)
        {
            LayerFeedbackAnimationState state = LayerFeedback.State;
            if (!state.IsActive)
            {
                return;
            }

            int radius = Math.Max(0, (int)Math.Round(state.SweepProgress * 4.0));
            byte alpha = (byte)Math.Clamp((int)Math.Round(state.SweepOpacity * 255.0), 0, 255);
            var pen = new Pen(new SolidColorBrush(Color.FromArgb(
                alpha,
                state.SweepColor.R,
                state.SweepColor.G,
                state.SweepColor.B)), Tokens.StrokeState);
            int originX = (int)Math.Round(state.Origin.X);
            int originY = (int)Math.Round(state.Origin.Y);

            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    if (Math.Max(Math.Abs(dx), Math.Abs(dy)) != radius)
                    {
                        continue;
                    }

                    Point screen = WorldToScreen(CellToWorld(originX + dx, originY + dy));
                    var rect = new Rect(screen.X, screen.Y, CellSize * Zoom, CellSize * Zoom);
                    context.DrawRectangle(null, pen, rect.Deflate(Tokens.StrokeState / 2.0));
                }
            }
        }

        private void OnLayerStackChanged()
        {
            int currentCount = LayerStack.Layers.Count;
            if (currentCount > _knownLayerCount)
            {
                LayerFeedback.Begin(new LayerInsertionFeedbackOrigin(CursorCellX, CursorCellY));
            }

            _knownLayerCount = currentCount;
            RefreshFieldLedger();
            InvalidateVisual();
        }

        private void RenderGroupDragPreview(DrawingContext context)
        {
            if (!_isDraggingItem || _dragCluster.Count <= 1 ||
                (_dragCandidateDeltaX == 0 && _dragCandidateDeltaY == 0))
            {
                return;
            }

            Color signal = _dragCandidateIsValid ? Colors.SignalInteraction : Colors.SignalRefusal;
            var fill = new SolidColorBrush(Color.FromArgb(38, signal.R, signal.G, signal.B));
            var pen = new Pen(new SolidColorBrush(Color.FromArgb(220, signal.R, signal.G, signal.B)), Tokens.StrokeState);

            foreach (var item in _dragCluster)
            {
                var origin = _dragInitialPositions[item];
                Point world = CellToWorld(origin.cellX + _dragCandidateDeltaX, origin.cellY + _dragCandidateDeltaY);
                Point screen = WorldToScreen(world);
                double width = item.CellWidth * CellSize * Zoom;
                double height = item.CellHeight * CellSize * Zoom;
                var rect = new Rect(screen.X, screen.Y, width, height);
                context.FillRectangle(fill, rect);
                context.DrawRectangle(null, pen, rect.Deflate(Tokens.StrokeState / 2));
            }
        }

        private void RenderResizeRefusal(DrawingContext context)
        {
            if (!_isResizingItem || _resizeCandidateIsValid || _resizingItem is null)
            {
                return;
            }

            Rect rect = GetContentScreenBounds(_resizingItem);
            Color refusal = Colors.SignalRefusal;
            context.FillRectangle(new SolidColorBrush(Color.FromArgb(32, refusal.R, refusal.G, refusal.B)), rect);
            using (context.PushClip(rect))
            {
                var pen = new Pen(new SolidColorBrush(Color.FromArgb(190, refusal.R, refusal.G, refusal.B)), Math.Max(1.0, Zoom));
                double spacing = Math.Max(8.0, 12.0 * Zoom);
                for (double start = rect.Left - rect.Height; start < rect.Right; start += spacing)
                {
                    context.DrawLine(pen, new Point(start, rect.Bottom), new Point(start + rect.Height, rect.Top));
                }
            }

            double stripHeight = Math.Min(24.0, Math.Max(18.0, rect.Height));
            var strip = new Rect(rect.Left, rect.Top, rect.Width, stripHeight);
            context.FillRectangle(new SolidColorBrush(Color.FromArgb(220, refusal.R, refusal.G, refusal.B)), strip);
            var text = new FormattedText(
                "SPACE OCCUPIED",
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface(Typography.FontFamilyMono, FontStyle.Normal, FontWeight.Bold),
                Typography.SizeMicro * Math.Max(1.0, Zoom),
                Colors.CPaperInkBrush)
            {
                MaxTextWidth = Math.Max(1.0, rect.Width - Tokens.SpaceSm)
            };
            context.DrawText(text, new Point(rect.Left + Tokens.SpaceXs, rect.Top + Tokens.SpaceXs));
        }

        private void RenderArmingGhost(DrawingContext context)
        {
            GhostPlacementDescriptor ghost = Arming.ActiveGhostDescriptor;
            if (!ghost.IsVisible)
            {
                return;
            }

            Color signal = ghost.IsValidRegion ? Colors.SignalInteraction : Colors.SignalRefusal;
            bool rejectionPulse = DateTime.UtcNow < _placementRejectedUntilUtc;
            double fillAlpha = ghost.IsValidRegion ? 0.06 : rejectionPulse ? 0.22 : 0.12;
            double insetAlpha = ghost.IsValidRegion ? 0.60 : rejectionPulse ? 1.0 : 0.85;
            double edgeAlpha = ghost.IsValidRegion ? 0.80 : rejectionPulse ? 1.0 : 0.90;
            var fill = new SolidColorBrush(Color.FromArgb(
                ToAlphaByte(fillAlpha), signal.R, signal.G, signal.B));
            var insetPen = new Pen(new SolidColorBrush(Color.FromArgb(
                ToAlphaByte(insetAlpha), signal.R, signal.G, signal.B)), Tokens.StrokeState);
            var edgePen = new Pen(new SolidColorBrush(Color.FromArgb(
                ToAlphaByte(edgeAlpha), signal.R, signal.G, signal.B)), 1.0, DashStyle.Dash);
            Point world = CellToWorld(ghost.OriginCell.X, ghost.OriginCell.Y);
            Point screen = WorldToScreen(world);
            var rect = new Rect(
                screen.X,
                screen.Y,
                ghost.WidthCells * CellSize * Zoom,
                ghost.HeightCells * CellSize * Zoom);

            context.FillRectangle(fill, rect);
            context.DrawRectangle(null, insetPen, rect.Deflate(Tokens.StrokeState));
            context.DrawRectangle(null, edgePen, rect);
        }

        private void OnPlacementRejected()
        {
            _placementRejectedUntilUtc = DateTime.UtcNow.AddMilliseconds(200.0);
            InvalidateVisual();
        }

        private void EmitTranslationTrail(int originX, int originY, GridContentItem item)
        {
            for (int x = 0; x < item.CellWidth; x++)
            {
                for (int y = 0; y < item.CellHeight; y++)
                {
                    SpentCells.Add(new SpentCell(originX + x, originY + y));
                }
            }

            while (SpentCells.Count > Tokens.CursorTrailMaxSteps)
            {
                SpentCells.RemoveAt(0);
            }
        }

        private static byte ToAlphaByte(double alpha) =>
            (byte)Math.Clamp(Math.Round(alpha * byte.MaxValue), 0, byte.MaxValue);

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
