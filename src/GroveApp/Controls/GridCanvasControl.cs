using System;
using System.Buffers;
using System.Collections.Generic;
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
        private readonly GridViewPreferenceStore _viewPreferences;
        private readonly CameraAnimation _cameraAnimation = new();
        private readonly CanvasPanInteraction _panInteraction = new();
        private readonly SelectionService _selectionService = new(CellSize);
        public bool GridLinesVisible { get; private set; }

        private readonly CursorRenderModule _cursorRenderModule = new();
        private readonly CanonicalCursorTrailModel _cursorModel;
        private readonly GridCanvasRenderPipeline _renderPipeline;
        private readonly GridCanvasFeedbackRenderer _feedbackRenderer;
        private static readonly Cursor HiddenCursor = new(StandardCursorType.None);
        private static readonly Cursor ResizeNorthWestCursor = new(StandardCursorType.TopLeftCorner);
        private static readonly Cursor ResizeNorthEastCursor = new(StandardCursorType.TopRightCorner);
        private static readonly Cursor ResizeSouthEastCursor = new(StandardCursorType.BottomRightCorner);
        private static readonly Cursor ResizeSouthWestCursor = new(StandardCursorType.BottomLeftCorner);
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

        public ExternalDragDropHandler DragDropHandler { get; private set; }
        public NativeClipboardService ClipboardService { get; private set; }

        public CameraModule Camera { get; } = new CameraModule();
        public double CameraX
        {
            get => Camera.CameraX;
            set
            {
                if (Math.Abs(Camera.CameraX - value) > 1e-6)
                {
                    Camera.SetState(new Point(value, Camera.CameraY), Camera.Zoom);
                    CancelCameraAnimationAtCurrentState();
                    RefreshCursorDescriptorFromScreen();
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
                    Camera.SetState(new Point(Camera.CameraX, value), Camera.Zoom);
                    CancelCameraAnimationAtCurrentState();
                    RefreshCursorDescriptorFromScreen();
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
                    Camera.SetState(new Point(Camera.CameraX, Camera.CameraY), clamped);
                    CancelCameraAnimationAtCurrentState();
                    RefreshCursorDescriptorFromScreen();
                    RefreshFieldLedger();
                    InvalidateVisual();
                    CameraChanged?.Invoke();
                }
            }
        }

        public Point MousePointerScreen { get; private set; }
        public Point MousePointerWorld { get; private set; }
        public CursorDescriptor CursorDescriptor => _cursorModel.CurrentDescriptor;
        public CellCoordinate CursorPlacementOrigin => CursorDescriptor.PlacementOriginCell;
        public bool IsResizeActive => _isResizingItem;

        public List<SpentCell> SpentCells => _cursorModel.MutableTrailCompatibilityView;

        public List<GridContentItem> Items { get; } = new();
        public GridContentItem? SelectedItem { get; set; }
        public GridContentItem? HoveredItem { get; set; }

        public List<GridNote> Notes => Items.OfType<GridNote>().ToList();
        public GridNote? SelectedNote
        {
            get => SelectedItem as GridNote;
            set
            {
                if (value is null)
                {
                    DeselectAllItems();
                }
                else
                {
                    SelectOnly(value);
                }
            }
        }
        public GridNote? HoveredNote
        {
            get => HoveredItem as GridNote;
            set => HoveredItem = value;
        }

        public CanvasToolMode ActiveTool { get; private set; } = CanvasToolMode.Select;

        private bool _isMarqueeSelecting;
        private Point _marqueeStartWorld;
        private Point _marqueeCurrentWorld;

        private bool _isDraggingItem;
        private GridContentItem? _draggedItem;
        private int _dragOffsetCellX;
        private int _dragOffsetCellY;
        private List<GridContentItem> _dragCluster = new();
        private Dictionary<GridContentItem, (int cellX, int cellY)> _dragInitialPositions = new();
        private int _dragCandidateDeltaX;
        private int _dragCandidateDeltaY;
        private bool _dragCandidateIsValid;

        private bool _isResizingItem;
        private GridContentItem? _resizingItem;
        private ResizeHandleLocation _resizeHandle;
        private SpatialRegion _resizeInitialFootprint;
        private SpatialRegion _resizeCandidateFootprint;
        private bool _resizeCandidateIsValid;

        private TopLevel? _topLevel;
        private bool _isAnimationFrameRequested;
        private TimeSpan _lastAnimationTimestamp;
        private int _knownLayerCount;
        private DateTime _placementRejectedUntilUtc;

        public event Action<GridContentItem>? ItemSelected;
        public event Action<GridNote>? NoteSelected;
        public event Action<GridNote>? NoteDoubleClicked;
        public event Action<GridContentItem>? ContentDoubleClicked;
        public event Action<CursorDescriptor>? EmptyCellDoubleClicked;
        public event Action? CameraChanged;
        public event Action<Point>? RightClickTapped;
        public event Action<GridContentItem, ArmableContentType>? ArmedItemPlaced;
        public event Action<HudPerformanceSnapshot>? PerformanceChanged;
        public event Action<Exception>? MemoryAnchorPersistenceFailed;
        public event Action<Exception>? PreferencePersistenceFailed;

        public GridCanvasControl()
        {
            _cursorModel = new CanonicalCursorTrailModel(_cursorRenderModule);
            _renderPipeline = new GridCanvasRenderPipeline();
            _feedbackRenderer = new GridCanvasFeedbackRenderer(_cursorRenderModule);
            ClipToBounds = true;
            Focusable = true;
            Cursor = HiddenCursor;
            _viewPreferences = new GridViewPreferenceStore();
            _viewPreferences.PersistenceFailed += exception => PreferencePersistenceFailed?.Invoke(exception);
            GridLinesVisible = _viewPreferences.LoadLinesVisible();

            LayerActivation = new LayerActivationManager(LayerStack);
            _selectionService.SelectionChanged += OnSelectionChanged;
            _selectionService.MarqueeChanged += _ => InvalidateVisual();
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
            LayerStack.LayerMigrationValidator = CanMigrateLayerItems;
            _knownLayerCount = LayerStack.Layers.Count;
            LayerStack.ActiveLayerChanged += _ =>
            {
                DeselectAllItems();
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
                    SelectOnly(newItem);
                    RefreshFieldLedger();
                    InvalidateVisual();
                    await Task.CompletedTask;
                },
                ResolvePlacementCursorAtScreenPoint
            );
            DragDropHandler.PreviewChanged += OnDropPreviewChanged;

            ClipboardService = new NativeClipboardService(() => TopLevel.GetTopLevel(this)?.Clipboard);

            SeedSampleData();
            RefreshCursorDescriptor(new Point(0, 0));
            SizeChanged += OnCanvasSizeChanged;
        }

        private void OnCanvasSizeChanged(object? sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width <= 0 || e.NewSize.Height <= 0)
            {
                return;
            }

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

            DragDropHandler.Attach(this);

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
            double frameMilliseconds = Math.Max(0.0, frameDelta.TotalMilliseconds);
            double framesPerSecond = frameMilliseconds > 0.0 ? 1000.0 / frameMilliseconds : 0.0;
            PerformanceChanged?.Invoke(new HudPerformanceSnapshot(
                framesPerSecond,
                frameMilliseconds,
                Items.Count,
                FieldEngine.GetTotalMetadataSourcesCount()));
            bool feedbackActive = LayerFeedback.State.IsActive;
            bool rejectionPulseActive = DateTime.UtcNow < _placementRejectedUntilUtc;
            if (feedbackActive)
            {
                LayerFeedback.Advance(frameDelta < TimeSpan.Zero ? TimeSpan.Zero : frameDelta);
            }

            Point cameraPosition = new(Camera.CameraX, Camera.CameraY);
            double cameraScale = Camera.Zoom;
            bool cameraSettling = _cameraAnimation.Step(frameDelta, ref cameraPosition, ref cameraScale);
            if (cameraSettling)
            {
                Camera.SetState(cameraPosition, cameraScale);
                RefreshCursorDescriptorFromScreen();
                RefreshFieldLedger();
                CameraChanged?.Invoke();
            }

            bool needsRedraw = _cursorModel.AdvanceTrail();

            if (cameraSettling || needsRedraw || SpentCells.Count > 0 || feedbackActive || rejectionPulseActive || _panInteraction.IsActive || _isMarqueeSelecting || _isDraggingItem)
            {
                InvalidateVisual();
            }

            RequestNextAnimationFrame();
        }

        public Point ScreenToWorld(Point screenPt) => Camera.ScreenToWorld(screenPt);

        public Point WorldToScreen(Point worldPt) => Camera.WorldToScreen(worldPt);

        public (int cellX, int cellY) WorldToCell(Point worldPt) => Camera.WorldToCell(worldPt, CellSize);

        private CursorDescriptor RefreshCursorDescriptor(Point worldPoint)
        {
            if (DragDropHandler.CurrentPreview is { } activePreview)
            {
                return ApplyCursorDescriptor(activePreview.Cursor);
            }

            var (pointerCellX, pointerCellY) = WorldToCell(worldPoint);
            GridContentItem? targetItem = FindItemAtCell(pointerCellX, pointerCellY);
            CursorPlacementFootprint? armedToolFootprint = Arming.IsArmed
                ? new CursorPlacementFootprint(
                    Arming.ActiveGhostDescriptor.WidthCells,
                    Arming.ActiveGhostDescriptor.HeightCells)
                : null;
            CursorDescriptor resolved = _cursorModel.Resolve(
                worldPoint,
                Zoom,
                targetItem,
                armedToolFootprint);

            ApplyCursorDescriptor(resolved);

            if (Arming.IsArmed)
            {
                Arming.UpdateCursorPosition(CursorDescriptor, LayerStack.ActiveLayerId);
            }

            return CursorDescriptor;
        }

        private void RefreshCursorDescriptorFromScreen()
        {
            MousePointerWorld = ScreenToWorld(MousePointerScreen);
            RefreshCursorDescriptor(MousePointerWorld);
        }

        public CursorDescriptor ResolveCursorDescriptorAtScreenPoint(Point screenPoint)
        {
            MousePointerScreen = screenPoint;
            MousePointerWorld = ScreenToWorld(screenPoint);
            return RefreshCursorDescriptor(MousePointerWorld);
        }

        private CursorDescriptor ResolvePlacementCursorAtScreenPoint(Point screenPoint, int widthCells, int heightCells)
        {
            MousePointerScreen = screenPoint;
            MousePointerWorld = ScreenToWorld(screenPoint);
            return _cursorModel.ResolveDropPreview(
                MousePointerWorld,
                Zoom,
                placement: new CursorPlacementFootprint(widthCells, heightCells));
        }

        private CursorDescriptor ApplyCursorDescriptor(CursorDescriptor descriptor)
        {
            return _cursorModel.Apply(descriptor);
        }

        private void OnDropPreviewChanged()
        {
            if (DragDropHandler.CurrentPreview is { } preview)
            {
                ApplyCursorDescriptor(preview.Cursor);
            }
            else
            {
                RefreshCursorDescriptor(MousePointerWorld);
            }

            InvalidateVisual();
        }

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
            var visibleBounds = Camera.GetVisibleCellBounds(Bounds.Size, CellSize);
            int minCellX = visibleBounds.minX, maxCellX = visibleBounds.maxX;
            int minCellY = visibleBounds.minY, maxCellY = visibleBounds.maxY;
            var visibleItems = Items
                .Where(item => LayerActivation.GetRenderMode(item.LayerId) != LayerRenderMode.Hidden)
                .Where(item => !LayerActivation.IsIsolationModeEnabled || item.LayerId == LayerStack.ActiveLayerId)
                .ToList();

            var activeAuraCells = FieldLedgerEngine.GetAuraCells(
                visibleItems, minCellX, maxCellX, minCellY, maxCellY);

            FieldEngine.RecalculateField(visibleItems, activeAuraCells);
        }

        public void ToggleGridLines()
        {
            GridLinesVisible = !GridLinesVisible;
            _viewPreferences.SaveLinesVisible(GridLinesVisible);
            InvalidateVisual();
        }

        public void FrameAllContent()
        {
            var frameItems = Items.Where(item =>
                    LayerStack.GetLayer(item.LayerId).IsVisible &&
                    LayerActivation.GetRenderMode(item.LayerId) != LayerRenderMode.Hidden &&
                    (!LayerActivation.IsIsolationModeEnabled || item.LayerId == LayerStack.ActiveLayerId))
                .ToList();

            if (frameItems.Count == 0 || Bounds.Width <= 0 || Bounds.Height <= 0)
            {
                AnimateCameraTo(new Point(0, 0), 1.0);
                return;
            }

            int minCellX = frameItems.Min(item => item.CellX);
            int minCellY = frameItems.Min(item => item.CellY);
            int maxCellX = frameItems.Max(item => item.CellX + item.CellWidth);
            int maxCellY = frameItems.Max(item => item.CellY + item.CellHeight);
            double worldWidth = Math.Max(CellSize, (maxCellX - minCellX) * CellSize);
            double worldHeight = Math.Max(CellSize, (maxCellY - minCellY) * CellSize);
            double targetZoom = Math.Clamp(
                Math.Min(Bounds.Width * 0.8 / worldWidth, Bounds.Height * 0.8 / worldHeight),
                CameraModule.MinZoom,
                CameraModule.MaxZoom);
            Point worldCenter = new(
                (minCellX * CellSize) + (worldWidth / 2.0),
                (minCellY * CellSize) + (worldHeight / 2.0));
            Point targetPosition = new(
                (Bounds.Width / 2.0) - (worldCenter.X * targetZoom),
                (Bounds.Height / 2.0) - (worldCenter.Y * targetZoom));
            AnimateCameraTo(targetPosition, targetZoom);
        }

        public void FocusDoubleClick(Point screenPoint)
        {
            Point worldPoint = ScreenToWorld(screenPoint);
            var (cellX, cellY) = WorldToCell(worldPoint);
            GridContentItem? item = FindItemAtCell(cellX, cellY);
            Point worldCenter;
            double targetZoom;

            if (item is null)
            {
                worldCenter = new Point((cellX + 0.5) * CellSize, (cellY + 0.5) * CellSize);
                targetZoom = Math.Clamp(Math.Max(1.0, Zoom), 1.0, CameraModule.MaxZoom);
            }
            else
            {
                double worldWidth = Math.Max(CellSize, item.CellWidth * CellSize);
                double worldHeight = Math.Max(CellSize, item.CellHeight * CellSize);
                targetZoom = Math.Clamp(
                    Math.Min(Bounds.Width * 0.8 / worldWidth, Bounds.Height * 0.6 / worldHeight),
                    1.0,
                    CameraModule.MaxZoom);
                worldCenter = new Point(
                    (item.CellX + (item.CellWidth / 2.0)) * CellSize,
                    (item.CellY + (item.CellHeight / 2.0)) * CellSize);
            }

            Point targetPosition = new(
                (Bounds.Width / 2.0) - (worldCenter.X * targetZoom),
                (Bounds.Height / 2.0) - (worldCenter.Y * targetZoom));
            AnimateCameraTo(targetPosition, targetZoom);
        }

        private void AnimateCameraTo(Point targetPosition, double targetZoom)
        {
            targetZoom = Math.Clamp(targetZoom, CameraModule.MinZoom, CameraModule.MaxZoom);
            if (Motion.PlaceDuration == TimeSpan.Zero)
            {
                Camera.SetState(targetPosition, targetZoom);
                _cameraAnimation.Cancel(targetPosition, targetZoom);
                RefreshCursorDescriptorFromScreen();
                RefreshFieldLedger();
                CameraChanged?.Invoke();
                InvalidateVisual();
                return;
            }

            _cameraAnimation.Start(
                new Point(Camera.CameraX, Camera.CameraY),
                Camera.Zoom,
                targetPosition,
                targetZoom,
                Motion.PlaceDuration);
            InvalidateVisual();
            RequestNextAnimationFrame();
        }

        private void CancelCameraAnimationAtCurrentState() =>
            _cameraAnimation.Cancel(new Point(Camera.CameraX, Camera.CameraY), Camera.Zoom);

        private int ResolveLayerDelta(int sourceLayerId, int targetLayerId)
        {
            int sourceIndex = LayerStack.GetStackIndex(sourceLayerId);
            int targetIndex = LayerStack.GetStackIndex(targetLayerId);
            return sourceIndex >= 0 && targetIndex >= 0
                ? Math.Abs(sourceIndex - targetIndex)
                : Math.Abs(sourceLayerId - targetLayerId);
        }

        private bool CanMigrateLayerItems(int sourceLayerId, int targetLayerId)
        {
            foreach (GridContentItem item in Items.Where(candidate => candidate.LayerId == sourceLayerId))
            {
                if (!IsRegionFree(
                    new CellCoordinate(item.CellX, item.CellY),
                    item.CellWidth,
                    item.CellHeight,
                    item,
                    targetLayerId))
                {
                    return false;
                }
            }

            return true;
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
            MousePointerWorld = CellToWorld(cellX, cellY);
            MousePointerScreen = WorldToScreen(MousePointerWorld);
            RefreshCursorDescriptor(MousePointerWorld);
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
                DeselectAllItems();
            }
            else
            {
                _selectionService.Remove(item.Id);
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

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            MousePointerScreen = e.GetPosition(this);
            MousePointerWorld = ScreenToWorld(MousePointerScreen);

            var (pointerCellX, pointerCellY) = WorldToCell(MousePointerWorld);
            GridContentItem? pointerItem = FindItemAtCell(pointerCellX, pointerCellY);
            RefreshCursorDescriptor(MousePointerWorld);

            if (DragDropHandler.CurrentPreview is not null)
            {
                InvalidateVisual();
                return;
            }

            (int cx, int cy) = WorldToCell(MousePointerWorld);

            if (_panInteraction.IsActive)
            {
                Cursor = HiddenCursor;
                PanGestureUpdate update = _panInteraction.Update(
                    new ScreenPoint(MousePointerScreen.X, MousePointerScreen.Y));
                CameraX = update.CameraPosition.X;
                CameraY = update.CameraPosition.Y;
                InvalidateVisual();
                CameraChanged?.Invoke();
                return;
            }

            if (Arming.IsArmed)
            {
                Cursor = HiddenCursor;
                InvalidateVisual();
                return;
            }

            if (_isResizingItem && _resizingItem != null)
            {
                Cursor = ResizeCursorFor(_resizeHandle);
                int handleOriginX = _resizeHandle is ResizeHandleLocation.NorthEast or ResizeHandleLocation.SouthEast
                    ? _resizeInitialFootprint.Right - 1
                    : _resizeInitialFootprint.X;
                int handleOriginY = _resizeHandle is ResizeHandleLocation.SouthWest or ResizeHandleLocation.SouthEast
                    ? _resizeInitialFootprint.Bottom - 1
                    : _resizeInitialFootprint.Y;
                int deltaX = cx - handleOriginX;
                int deltaY = cy - handleOriginY;
                int intrinsicWidth = _resizingItem.IntrinsicWidthPx;
                int intrinsicHeight = _resizingItem.IntrinsicHeightPx;
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
                _resizeCandidateFootprint = candidate;
                InvalidateVisual();
                return;
            }

            if (_isDraggingItem && _draggedItem != null)
            {
                Cursor = HiddenCursor;
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

            if (_isMarqueeSelecting)
            {
                Cursor = HiddenCursor;
                _marqueeCurrentWorld = MousePointerWorld;
                UpdateMarqueeSelection();
                InvalidateVisual();
                return;
            }

            GridContentItem? newHover = pointerItem;
            if (newHover != HoveredItem)
            {
                if (HoveredItem != null) HoveredItem.IsHovered = false;
                HoveredItem = newHover;
                if (HoveredItem != null) HoveredItem.IsHovered = true;
            }

            UpdateResizeCursor(MousePointerScreen);
            InvalidateVisual();
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            Focus();
            ResolveCursorDescriptorAtScreenPoint(e.GetPosition(this));
            var props = e.GetCurrentPoint(this).Properties;

            if (props.IsLeftButtonPressed && e.ClickCount >= 2)
            {
                e.Handled = true;
                return;
            }

            if (props.IsMiddleButtonPressed || props.IsRightButtonPressed)
            {
                _panInteraction.TryBegin(new PanGestureStart(
                    new ScreenPoint(e.GetPosition(this).X, e.GetPosition(this).Y),
                    new ScreenPoint(CameraX, CameraY),
                    props.IsRightButtonPressed ? PanInitiator.RightButton : PanInitiator.MiddleButton,
                    Arming.IsArmed));
                e.Handled = true;
                return;
            }

            if (props.IsLeftButtonPressed)
            {
                if (_panInteraction.IsSpacePanModifierActive)
                {
                    _panInteraction.TryBegin(new PanGestureStart(
                        new ScreenPoint(e.GetPosition(this).X, e.GetPosition(this).Y),
                        new ScreenPoint(CameraX, CameraY),
                        PanInitiator.SpaceLeftButton,
                        false));
                    e.Handled = true;
                    return;
                }

                if (Arming.IsArmed)
                {
                    TryPlaceArmedItem();
                    e.Handled = true;
                    return;
                }

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
                        _resizeCandidateFootprint = _resizeInitialFootprint;
                        _resizeCandidateIsValid = true;
                        Cursor = ResizeCursorFor(handle);
                        e.Handled = true;
                        return;
                    }
                }

                Point clickWorld = ScreenToWorld(e.GetPosition(this));
                (int cx, int cy) = WorldToCell(clickWorld);
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

                if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
                {
                    _selectionService.SelectSingle(hitItem.Id, isShiftHeld: true);
                    e.Handled = true;
                    return;
                }

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
                    _selectionService.BeginMarqueeSweep(
                        new WorldPoint(clickWorld.X, clickWorld.Y),
                        e.KeyModifiers.HasFlag(KeyModifiers.Shift));
                    if (!e.KeyModifiers.HasFlag(KeyModifiers.Shift))
                    {
                        _selectionService.ClearSelection();
                    }
                }

                InvalidateVisual();
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            if (_panInteraction.IsActive)
            {
                PanGestureEnd result = _panInteraction.End();
                if (result.ShouldDisarmTool)
                {
                    DisarmTool();
                }
                else if (result.ShouldOpenContextMenu)
                {
                    RightClickTapped?.Invoke(e.GetPosition(this));
                }
                e.Handled = true;
            }
            if (_isResizingItem)
            {
                if (_resizingItem != null && _resizeCandidateIsValid)
                {
                    RemoveMemoryAnchor(_resizingItem);
                    _resizingItem.ResizeTo(_resizeCandidateFootprint);
                    AddMemoryAnchor(_resizingItem);
                    RefreshFieldLedger();
                }
                _isResizingItem = false;
                _resizingItem = null;
                _resizeHandle = ResizeHandleLocation.None;
                _resizeInitialFootprint = default;
                _resizeCandidateFootprint = default;
                _resizeCandidateIsValid = false;
                Cursor = HiddenCursor;
                e.Handled = true;
            }
            if (_isDraggingItem)
            {
                var sourceFootprints = new List<SpatialRegion>();
                var targetFootprints = new List<SpatialRegion>();
                foreach (GridContentItem item in _dragCluster)
                {
                    var origin = _dragInitialPositions[item];
                    if (origin.cellX == item.CellX && origin.cellY == item.CellY)
                    {
                        continue;
                    }

                    sourceFootprints.Add(new SpatialRegion(
                        origin.cellX,
                        origin.cellY,
                        item.CellWidth,
                        item.CellHeight));
                    targetFootprints.Add(new SpatialRegion(
                        item.CellX,
                        item.CellY,
                        item.CellWidth,
                        item.CellHeight));
                    RemoveMemoryAnchor(item);
                    AddMemoryAnchor(item);
                }

                if (sourceFootprints.Count > 0)
                {
                    _cursorModel.RecordContentTranslationTrail(sourceFootprints, targetFootprints);
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
                _selectionService.CommitMarqueeSweep(
                    Items.Where(item => item.LayerId == LayerStack.ActiveLayerId));
                e.Handled = true;
            }
        }

        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            base.OnPointerWheelChanged(e);

            double zoomFactor = e.Delta.Y > 0 ? 1.15 : 0.85;
            Camera.ZoomAt(e.GetPosition(this), zoomFactor);
            CancelCameraAnimationAtCurrentState();
            RefreshCursorDescriptorFromScreen();

            InvalidateVisual();
            CameraChanged?.Invoke();
            e.Handled = true;
        }

        public void HandleDoubleClick(Point pt)
        {
            FocusDoubleClick(pt);
            CursorDescriptor cursor = ResolveCursorDescriptorAtScreenPoint(pt);
            var (cx, cy) = (cursor.PlacementOriginCell.X, cursor.PlacementOriginCell.Y);
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
                EmptyCellDoubleClicked?.Invoke(cursor);
            }
        }

        public void BeginSpacePan()
        {
            _panInteraction.BeginSpacePan();
        }

        public bool EndSpacePan()
        {
            return _panInteraction.EndSpacePan();
        }

        public bool CancelActiveResize()
        {
            if (!_isResizingItem)
            {
                return false;
            }

            _isResizingItem = false;
            _resizingItem = null;
            _resizeHandle = ResizeHandleLocation.None;
            _resizeInitialFootprint = default;
            _resizeCandidateFootprint = default;
            _resizeCandidateIsValid = false;
            Cursor = HiddenCursor;
            InvalidateVisual();
            return true;
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
            double tolerance = Tokens.ResizeHandleTargetPixels / 2.0;

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

        private void UpdateResizeCursor(Point pointerScreen)
        {
            if (_isResizingItem)
            {
                Cursor = ResizeCursorFor(_resizeHandle);
                return;
            }

            if (SelectedItem is not null &&
                !LayerStack.GetLayer(SelectedItem.LayerId).IsLocked &&
                TryGetResizeHandle(SelectedItem, pointerScreen, out ResizeHandleLocation handle))
            {
                Cursor = ResizeCursorFor(handle);
                return;
            }

            Cursor = HiddenCursor;
        }

        private static Cursor ResizeCursorFor(ResizeHandleLocation handle) =>
            handle switch
            {
                ResizeHandleLocation.NorthWest => ResizeNorthWestCursor,
                ResizeHandleLocation.NorthEast => ResizeNorthEastCursor,
                ResizeHandleLocation.SouthEast => ResizeSouthEastCursor,
                ResizeHandleLocation.SouthWest => ResizeSouthWestCursor,
                _ => HiddenCursor
            };

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

            int toolWidth = contentType == ArmableContentType.Document ? 2 : 1;
            CursorDescriptor armedCursor = _cursorModel.Resolve(
                MousePointerWorld,
                Zoom,
                targetItem: null,
                armedToolFootprint: new CursorPlacementFootprint(toolWidth, toolWidth));
            ApplyCursorDescriptor(armedCursor);
            Arming.ArmTool(contentType, armedCursor, LayerStack.ActiveLayerId);
            ActiveTool = contentType switch
            {
                ArmableContentType.Note => CanvasToolMode.Note,
                ArmableContentType.QuickNote => CanvasToolMode.QuickNote,
                ArmableContentType.Document => CanvasToolMode.Document,
                _ => CanvasToolMode.Select
            };
            InvalidateVisual();
            return true;
        }

        public void DisarmTool()
        {
            Arming.Disarm();
            ActiveTool = CanvasToolMode.Select;
            RefreshCursorDescriptor(MousePointerWorld);
            InvalidateVisual();
        }

        public bool NavigateLayer(int direction)
        {
            bool changed = LayerStack.Navigate(direction);
            if (changed)
            {
                Arming.UpdateCursorPosition(
                    CursorDescriptor,
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

        public void SelectItems(IEnumerable<GridContentItem> items)
        {
            ArgumentNullException.ThrowIfNull(items);
            GridContentItem[] selectedItems = items.ToArray();
            _selectionService.SelectMany(selectedItems.Select(item => item.Id));
        }

        private void SelectItem(GridContentItem item)
        {
            _selectionService.SelectSingle(item.Id);
        }

        private void SelectNote(GridNote note)
        {
            SelectItem(note);
        }

        public void DeselectAllItems()
        {
            _selectionService.ClearSelection();
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

            try
            {
                GridContentItem item = descriptor.ContentType switch
                {
                    ArmableContentType.Note => new GridNote(
                        descriptor.PlacementOriginCell.X,
                        descriptor.PlacementOriginCell.Y,
                        "New Note",
                        NoteColor.Violet,
                        layerId: descriptor.LayerId),
                    ArmableContentType.QuickNote => new GridNote(
                        descriptor.PlacementOriginCell.X,
                        descriptor.PlacementOriginCell.Y,
                        "",
                        NoteColor.Violet,
                        layerId: descriptor.LayerId),
                    ArmableContentType.Document => new GridDocument(
                        descriptor.PlacementOriginCell.X,
                        descriptor.PlacementOriginCell.Y,
                        descriptor.WidthCells,
                        descriptor.HeightCells,
                        "New Document",
                        "",
                        layerId: descriptor.LayerId),
                    _ => throw new ArgumentOutOfRangeException()
                };

                AddItem(item);
                SelectOnly(item);
                RefreshFieldLedger();
                ArmedItemPlaced?.Invoke(item, descriptor.ContentType);
            }
            finally
            {
                Arming.CompletePlacement();
                ActiveTool = CanvasToolMode.Select;
                RefreshCursorDescriptor(MousePointerWorld);
                InvalidateVisual();
            }
        }

        public void DeselectAllNotes()
        {
            DeselectAllItems();
        }

        private void UpdateMarqueeSelection()
        {
            _selectionService.UpdateMarqueeSweep(
                new WorldPoint(_marqueeCurrentWorld.X, _marqueeCurrentWorld.Y));
            var previewIds = _selectionService.PreviewMarqueeSweep(
                Items.Where(item => item.LayerId == LayerStack.ActiveLayerId)
                    .Select(SpatialSelectionCandidate.From));
            GridContentItem? lastSelected = null;
            foreach (GridContentItem item in Items)
            {
                item.IsSelected = previewIds.Contains(item.Id);
                if (item.IsSelected)
                {
                    lastSelected = item;
                }
            }
            SelectedItem = lastSelected;
        }

        private void OnSelectionChanged(SelectionSnapshot snapshot)
        {
            var selectedIds = new HashSet<string>(snapshot.SelectedPlacementIds, StringComparer.Ordinal);
            foreach (GridContentItem item in Items)
            {
                item.IsSelected = selectedIds.Contains(item.Id) &&
                    item.LayerId == LayerStack.ActiveLayerId;
            }

            GridContentItem? primary = snapshot.PrimarySelectionId is null
                ? null
                : Items.FirstOrDefault(item => item.Id == snapshot.PrimarySelectionId && item.IsSelected);
            SelectedItem = primary;
            if (primary is null)
            {
                return;
            }

            ItemSelected?.Invoke(primary);
            if (primary is GridNote note)
            {
                NoteSelected?.Invoke(note);
            }
        }

        public CellLedgerEntry GetCellLedger(int col, int row) => FieldEngine.GetCellLedger(col, row);
        public List<CellMetadataSource> QueryMetadataInRegion(Rect cellBounds) => FieldEngine.QueryMetadataInRegion(cellBounds);

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            double w = Bounds.Width;
            double h = Bounds.Height;
            if (w <= 0 || h <= 0) return;
            context.FillRectangle(Colors.SurfaceGridBrush, new Rect(Bounds.Size));

            var visibleBounds = Camera.GetVisibleCellBounds(Bounds.Size, CellSize);
            int minCellX = visibleBounds.minX, maxCellX = visibleBounds.maxX;
            int minCellY = visibleBounds.minY, maxCellY = visibleBounds.maxY;

            var visibleItems = Items
                .Where(item => LayerActivation.GetRenderMode(item.LayerId) != LayerRenderMode.Hidden)
                .Where(item => !LayerActivation.IsIsolationModeEnabled || item.LayerId == LayerStack.ActiveLayerId)
                .ToList();
            HashSet<Guid> indexedMemoryIds = QueryIndexedMemoryIds(minCellX, maxCellX, minCellY, maxCellY);
            var activeItems = Items.Where(item =>
                IsIndexedForViewport(item, indexedMemoryIds) &&
                item.LayerId == LayerStack.ActiveLayerId &&
                LayerStack.GetLayer(item.LayerId).IsVisible).ToList();
            var inactiveItems = Items.Where(item =>
                IsIndexedForViewport(item, indexedMemoryIds) &&
                LayerActivation.GetRenderMode(item.LayerId) == LayerRenderMode.InactivePresenceOnly).ToList();
            double renderScaling = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1.0;
            _renderPipeline.Render(context, new GridCanvasRenderFrame(
                Bounds.Size,
                Camera.CurrentState.TransformMatrix,
                CellSize,
                Zoom,
                minCellX,
                maxCellX,
                minCellY,
                maxCellY,
                renderScaling,
                GridLinesVisible,
                FieldEngine,
                visibleItems,
                activeItems,
                inactiveItems,
                SelectedItem,
                HoveredItem,
                WorldToScreen));

            _feedbackRenderer.RenderResizePreview(
                context,
                _isResizingItem,
                _resizeCandidateIsValid,
                _resizeCandidateFootprint,
                CellSize,
                Zoom,
                Bounds.Size,
                WorldToScreen);

            context.Custom(new CursorTrailDrawOperation(
                new Rect(Bounds.Size),
                Camera.CurrentState.TransformMatrix,
                _cursorModel.Trail,
                _cursorModel.TrailVersion));

            if (DragDropHandler.CurrentPreview is { } dropPreview)
            {
                _feedbackRenderer.RenderDropPreview(context, dropPreview, WorldToScreen);
            }
            else if (Arming.IsArmed)
            {
                _feedbackRenderer.RenderArmingGhost(
                    context,
                    Arming.ActiveGhostDescriptor,
                    CursorDescriptor,
                    DateTime.UtcNow < _placementRejectedUntilUtc,
                    WorldToScreen);
            }
            else
            {
                _cursorRenderModule.RenderGridCursor(context, WorldToScreen, CursorDescriptor);
            }

            _feedbackRenderer.RenderMarqueeSelection(
                context,
                _isMarqueeSelecting,
                _marqueeStartWorld,
                _marqueeCurrentWorld,
                WorldToCell,
                WorldToScreen,
                CellSize);

            _feedbackRenderer.RenderGroupDragPreview(
                context,
                _isDraggingItem,
                _dragCluster,
                _dragInitialPositions,
                _dragCandidateDeltaX,
                _dragCandidateDeltaY,
                _dragCandidateIsValid,
                CellSize,
                Zoom,
                WorldToScreen,
                CellToWorld);

            _feedbackRenderer.RenderLayerFeedback(
                context,
                LayerFeedback.State,
                CellSize,
                Zoom,
                WorldToScreen,
                CellToWorld);
        }

        private void OnLayerStackChanged()
        {
            int currentCount = LayerStack.Layers.Count;
            if (currentCount > _knownLayerCount)
            {
                LayerFeedback.Begin(new LayerInsertionFeedbackOrigin(CursorPlacementOrigin.X, CursorPlacementOrigin.Y));
            }

            _knownLayerCount = currentCount;
            RefreshFieldLedger();
            InvalidateVisual();
        }

        private void OnPlacementRejected()
        {
            _placementRejectedUntilUtc = DateTime.UtcNow.AddMilliseconds(200.0);
            InvalidateVisual();
        }

    }
}
