using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using FluentAvalonia.UI.Windowing;
using GroveApp.Controls;
using GroveApp.DesignSystem;
using GroveApp.Engine;
using GroveApp.Engine.Interaction;
using GroveApp.Engine.Memory;
using GroveApp.Models;
using GroveApp.Models.Interaction;
using Colors = GroveApp.DesignSystem.Colors;
using EngineCellCoordinate = GroveApp.Engine.CellCoordinate;

namespace GroveApp
{
    public partial class MainWindow : AppWindow, IKeybindHost
    {
        private readonly KeybindModule _keybindModule = new();
        private readonly GlobalFocusPrecedenceRouter _focusRouter;
        private readonly SpatialContextMenuService _contextMenuService = new();
        private readonly FluentWindowBackdropManager _backdropManager = new();
        private readonly DispatcherTimer _contextLongPressTimer;
        private Point _contextPressPoint;
        private bool _contextPressActive;
        private HudBindingAdapter? _watermarkBinding;

        public MainWindow()
        {
            InitializeComponent();
            PlaneCompositor.RegisterPlaneView(new ThreePlaneVisualCompositorContainer.ControlPlaneView(
                CanvasControl, VisualPlaneType.Plane0_SpatialGrid, ThreePlaneVisualCompositorContainer.Plane0ZIndex));
            foreach (Control view in new Control[] { NotepadEditor, QuickNote, DocumentEditor, ImageProperties })
            {
                PlaneCompositor.RegisterPlaneView(new ThreePlaneVisualCompositorContainer.ControlPlaneView(
                    view, VisualPlaneType.Plane1_InformationPlane, ThreePlaneVisualCompositorContainer.Plane1ZIndex));
            }
            foreach (Control view in new Control[] { LayerManagerOverlay, ContextMenuOverlay, SpatialWatermark, PerformanceTracker, SlateHost })
            {
                PlaneCompositor.RegisterPlaneView(new ThreePlaneVisualCompositorContainer.ControlPlaneView(
                    view, VisualPlaneType.Plane2_HUDPlane, ThreePlaneVisualCompositorContainer.Plane2ZIndex));
            }
            Opened += (_, _) => _backdropManager.Apply(this);

            LayerManagerOverlay.BindLayerService(CanvasControl.LayerStack);
            ContextMenuOverlay.BindService(_contextMenuService);
            ContextMenuOverlay.CommandRequested += OnContextMenuCommandRequested;
            _contextMenuService.ContextMenuStateChanged += OnContextMenuStateChanged;
            _watermarkBinding = new HudBindingAdapter(CanvasControl, CanvasControl.LayerStack);
            SpatialWatermark.Bind(_watermarkBinding);

            _focusRouter = new GlobalFocusPrecedenceRouter(
                this,
                () => NotepadEditor.IsVisible || QuickNote.IsVisible || DocumentEditor.IsVisible || ImageProperties.IsVisible || LayerManagerOverlay.IsVisible || ContextMenuOverlay.IsVisible || SlateHost.IsVisible,
                () => !NotepadEditor.IsVisible && !QuickNote.IsVisible && !DocumentEditor.IsVisible && !ImageProperties.IsVisible && !LayerManagerOverlay.IsVisible && !ContextMenuOverlay.IsVisible && !SlateHost.IsVisible,
                combination => _keybindModule.ProcessRoutedCombination(combination, this));
            Closed += async (_, _) =>
            {
                _focusRouter.Dispose();
                SpatialWatermark.Dispose();
                _watermarkBinding?.Dispose();
                await CanvasControl.FlushLayerStateAsync().ConfigureAwait(true);
                await CanvasControl.MemoryAnchors.FlushAsync().ConfigureAwait(true);
            };

            TransparencyLevelHint = new[]
            {
                WindowTransparencyLevel.Mica,
                WindowTransparencyLevel.AcrylicBlur,
                WindowTransparencyLevel.Blur,
                WindowTransparencyLevel.None
            };

            TitleBar.ExtendsContentIntoTitleBar = false;
            TitleBar.Height = 40;
            TitleBar.BackgroundColor = Colors.SurfaceChrome;
            TitleBar.ForegroundColor = Colors.NoteText;
            TitleBar.InactiveBackgroundColor = Colors.SurfaceChrome;
            TitleBar.InactiveForegroundColor = Colors.TitleBarInactiveForeground;
            TitleBar.ButtonBackgroundColor = Colors.SurfaceChrome;
            TitleBar.ButtonForegroundColor = Colors.NoteText;
            TitleBar.ButtonHoverBackgroundColor = Colors.GridMaj;
            TitleBar.ButtonHoverForegroundColor = Colors.NoteText;
            TitleBar.ButtonPressedBackgroundColor = Colors.SurfaceRaised;
            TitleBar.ButtonPressedForegroundColor = Colors.NoteText;
            TitleBar.ButtonInactiveBackgroundColor = Colors.SurfaceChrome;
            TitleBar.ButtonInactiveForegroundColor = Colors.TitleBarInactiveForeground;

            CanvasControl.NoteSelected += OnNoteSelected;
            CanvasControl.NoteDoubleClicked += OnNoteDoubleClicked;
            CanvasControl.ContentDoubleClicked += OnContentDoubleClicked;
            CanvasControl.EmptyCellDoubleClicked += OnEmptyCellDoubleClicked;
            CanvasControl.CameraChanged += OnCameraChanged;
            CanvasControl.ArmedItemPlaced += OnArmedItemPlaced;
            CanvasControl.PerformanceChanged += OnPerformanceChanged;

            CanvasControl.AddHandler(PointerPressedEvent, OnCanvasPointerPressed, RoutingStrategies.Tunnel);
            CanvasControl.AddHandler(PointerReleasedEvent, OnCanvasPointerReleased, RoutingStrategies.Tunnel);
            CanvasControl.AddHandler(PointerMovedEvent, OnCanvasPointerMoved, RoutingStrategies.Tunnel);
            CanvasControl.RightClickTapped += OpenContextMenuAt;

            _contextLongPressTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _contextLongPressTimer.Tick += OnContextLongPressTimerTick;

            NotepadEditor.SaveRequested += OnNotepadEditorSaveRequested;
            NotepadEditor.Closed += OnOverlayClosed;

            QuickNote.SaveAndPlaceRequested += OnQuickNoteSaveAndPlaceRequested;
            QuickNote.PlacedNoteTextCommitted += OnQuickNotePlacedNoteTextCommitted;
            QuickNote.Closed += OnOverlayClosed;
            DocumentEditor.SaveRequested += OnDocumentEditorSaveRequested;
            DocumentEditor.Closed += OnOverlayClosed;
            ImageProperties.Closed += OnOverlayClosed;

            LayerManagerOverlay.Closed += OnOverlayClosed;
            SlateHost.Closed += OnOverlayClosed;
            SlateHost.DocumentSaveRequested += OnSlateDocumentSaveRequested;

            SizeChanged += (s, e) => UpdateNotepadEditorPosition();

            KeyDown += OnWindowKeyDown;
            KeyUp += OnWindowKeyUp;

        }

        private void OnCanvasPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var clickInfo = e.GetCurrentPoint(CanvasControl);
            Point screenPoint = e.GetPosition(CanvasControl);

            _contextLongPressTimer.Stop();
            _contextPressActive = false;

            if (_contextMenuService.ActiveMenu != null && !clickInfo.Properties.IsRightButtonPressed)
            {
                _contextMenuService.ProcessPointerPressed(new ScreenPoint(screenPoint.X, screenPoint.Y));
            }

            if (clickInfo.Properties.IsLeftButtonPressed && e.ClickCount == 1)
            {
                _contextPressPoint = screenPoint;
                _contextPressActive = true;
                _contextLongPressTimer.Start();
            }

            if (clickInfo.Properties.IsLeftButtonPressed && e.ClickCount == 2)
            {
                CanvasControl.HandleDoubleClick(e.GetPosition(CanvasControl));
                e.Handled = true;
            }
        }

        private void OnCanvasPointerReleased(object? sender, PointerEventArgs e)
        {
            _contextPressActive = false;
            _contextLongPressTimer.Stop();
        }

        private void OnCanvasPointerMoved(object? sender, PointerEventArgs e)
        {
            if (!_contextPressActive)
            {
                return;
            }

            Point current = e.GetPosition(CanvasControl);
            Vector delta = current - _contextPressPoint;
            if (Math.Abs(delta.X) > 8 || Math.Abs(delta.Y) > 8)
            {
                _contextPressActive = false;
                _contextLongPressTimer.Stop();
            }
        }

        private void OnContextLongPressTimerTick(object? sender, EventArgs e)
        {
            _contextLongPressTimer.Stop();
            if (!_contextPressActive)
            {
                return;
            }

            _contextPressActive = false;
            OpenContextMenuAt(_contextPressPoint);
        }

        private void OpenContextMenuAt(Point screenPoint)
        {
            CursorDescriptor cursor = CanvasControl.ResolveCursorDescriptorAtScreenPoint(screenPoint);
            var cell = cursor.PlacementOriginCell;
            var target = CanvasControl.FindItemAtCell(cell.X, cell.Y);
            IEnumerable<string> targetIds = target is null
                ? Array.Empty<string>()
                : CanvasControl.GetSelectedItems().Count > 1
                    ? CanvasControl.GetSelectedItems().Select(item => item.Id)
                    : new[] { target.Id };
            _contextMenuService.OpenContextMenuAt(
                new ScreenPoint(screenPoint.X, screenPoint.Y),
                new GroveApp.Models.Interaction.CellCoordinate(cell.X, cell.Y),
                CanvasControl.LayerStack.SelectedGridLayerId,
                targetIds,
                new ScreenSize(CanvasControl.Bounds.Width, CanvasControl.Bounds.Height));
        }

        private void OnContextMenuStateChanged(SpatialContextMenuModel? model)
        {
            if (model is null)
            {
                Dispatcher.UIThread.Post(() => CanvasControl.Focus());
            }
        }

        private void OnPerformanceChanged(HudPerformanceSnapshot snapshot)
        {
            PerformanceTracker.Update(snapshot);
        }

        private void OnCameraChanged()
        {
            UpdateNotepadEditorPosition();
        }

        private void UpdateNotepadEditorPosition()
        {
            if (NotepadEditor.IsVisible && NotepadEditor.TargetNote != null)
            {
                var targetNote = NotepadEditor.TargetNote;
                Rect sourceBounds = CanvasControl.GetNoteScreenBounds(targetNote);
                NotepadEditor.UpdatePosition(sourceBounds, Bounds.Size);
            }

            if (DocumentEditor.IsVisible)
            {
                GridDocument? document = CanvasControl.Items.OfType<GridDocument>().FirstOrDefault(item => item.IsSelected);
                if (document != null)
                {
                    DocumentEditor.UpdatePosition(CanvasControl.GetContentScreenBounds(document), Bounds.Size);
                }
            }
        }

        private void OnNoteSelected(GridNote note)
        {
            if (!NotepadEditor.IsVisible && !QuickNote.IsVisible)
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
                NotepadEditor.OpenForNotes(selectedNotes, primaryBounds, Bounds.Size);
            }
            else
            {
                OpenNotepadEditorForNote(note);
            }
        }

        private void OnContentDoubleClicked(GridContentItem item)
        {
            if (item is GridDocument document)
            {
                SlateHost.OpenForDocument(document);
            }
            else if (item is GridImage image)
            {
                SlateHost.OpenGallery(new[] { image });
            }
        }

        private void OnDocumentEditorSaveRequested(GridDocument document, string title, string rawText)
        {
            document.UpdateText(title, rawText);
            CanvasControl.UpdateMemoryForItem(document);
            CanvasControl.RefreshFieldLedger();
            CanvasControl.InvalidateVisual();
            CanvasControl.Focus();
        }

        private void OnEmptyCellDoubleClicked(CursorDescriptor cursor)
        {
            EngineCellCoordinate placementOrigin = cursor.PlacementOriginCell;
            var newNote = new GridNote(placementOrigin.X, placementOrigin.Y, "New Note", NoteColor.Violet, layerId: CanvasControl.LayerStack.SelectedGridLayerId);
            CanvasControl.AddItem(newNote);
            CanvasControl.SelectOnly(newNote);
            CanvasControl.InvalidateVisual();
            OpenNotepadEditorForNote(newNote);
        }

        private void OpenNotepadEditorForNote(GridNote note)
        {
            Rect sourceBounds = CanvasControl.GetNoteScreenBounds(note);
            NotepadEditor.OpenForNote(note, sourceBounds, Bounds.Size);
        }

        private void OnNotepadEditorSaveRequested(GridNote note, string newText)
        {
            note.Text = newText;
            note.RecalculateFootprint();
            CanvasControl.UpdateMemoryForItem(note);
            CanvasControl.RefreshFieldLedger();
            CanvasControl.InvalidateVisual();
            CanvasControl.Focus();
        }

        private void OnSlateDocumentSaveRequested(GridDocument document, string title, string rawText)
        {
            document.UpdateText(title, rawText);
            CanvasControl.UpdateMemoryForItem(document);
            CanvasControl.RefreshFieldLedger();
            CanvasControl.InvalidateVisual();
        }

        private void OnQuickNoteSaveAndPlaceRequested(QuickNoteItem item)
        {
            var newNote = new GridNote(
                CanvasControl.CursorPlacementOrigin.X,
                CanvasControl.CursorPlacementOrigin.Y,
                item.Text,
                NoteColor.Violet,
                layerId: CanvasControl.LayerStack.SelectedGridLayerId);
            CanvasControl.AddItem(newNote);
            CanvasControl.SelectOnly(newNote);
            CanvasControl.InvalidateVisual();
            CanvasControl.Focus();
        }

        private void OnArmedItemPlaced(GridContentItem item, ArmableContentType contentType)
        {
            if (item is not GridNote note)
            {
                return;
            }

            if (contentType == ArmableContentType.QuickNote)
            {
                QuickNote.OpenForPlacedNote(note);
            }
            else if (contentType == ArmableContentType.Note)
            {
                OpenNotepadEditorForNote(note);
            }
        }

        private void OnQuickNotePlacedNoteTextCommitted(GridNote note, string text)
        {
            note.Text = text;
            note.RecalculateFootprint();
            CanvasControl.UpdateMemoryForItem(note);
            CanvasControl.RefreshFieldLedger();
            CanvasControl.InvalidateVisual();
        }

        private void OnOverlayClosed()
        {
            CanvasControl.Focus();
        }

        private void OnContextMenuCommandRequested(ContextMenuCommandInvocation invocation)
        {
            GridContentItem? target = CanvasControl.Items.Find(item =>
                invocation.TargetContext.TargetPlacementIds.Contains(item.Id, StringComparer.Ordinal));

            switch (invocation.Command.Id)
            {
                case "create-note":
                    CanvasControl.MoveCursorToCell(invocation.TargetContext.AddressedCell.X, invocation.TargetContext.AddressedCell.Y);
                    CanvasControl.ArmTool(ArmableContentType.Note);
                    break;
                case "create-document":
                    CanvasControl.MoveCursorToCell(invocation.TargetContext.AddressedCell.X, invocation.TargetContext.AddressedCell.Y);
                    CanvasControl.ArmTool(ArmableContentType.Document);
                    break;
                case "open" when target is GridNote note:
                    OpenNotepadEditorForNote(note);
                    break;
                case "open" when target is GridDocument document:
                    SlateHost.OpenForDocument(document);
                    break;
                case "open" when target is GridImage image:
                    SlateHost.OpenGallery(new[] { image });
                    break;
                case "anchor" when target != null:
                    IReadOnlyList<GridContentItem> anchorTargets = CanvasControl.GetSelectedItems();
                    if (anchorTargets.Count == 0)
                    {
                        anchorTargets = new[] { target };
                    }

                    foreach (GridContentItem item in anchorTargets)
                    {
                        CanvasControl.SetContentAnchored(item, !item.IsAnchored);
                    }
                    break;
                case "copy" or "copy-all":
                    if (target is GridContentItem copyTarget)
                    {
                        _ = CanvasControl.ClipboardService.CopyItemsAsync(new[] { copyTarget });
                    }
                    else
                    {
                        _ = CanvasControl.ClipboardService.CopyItemsAsync(CanvasControl.GetSelectedItems());
                    }
                    break;
                case "paste":
                    _ = PasteContextMenuItemsAsync();
                    break;
                case "delete" or "delete-all":
                    IReadOnlyList<GridContentItem> deleteTargets = CanvasControl.GetSelectedItems();
                    if (deleteTargets.Count == 0 && target is GridContentItem deleteTarget)
                    {
                        deleteTargets = new[] { deleteTarget };
                    }

                    foreach (GridContentItem item in deleteTargets)
                    {
                        CanvasControl.RemoveItem(item);
                    }
                    CanvasControl.InvalidateVisual();
                    break;
                case "grid-properties":
                    CanvasControl.ToggleGridLines();
                    break;
                case "group-anchor":
                    foreach (GridContentItem item in CanvasControl.GetSelectedItems())
                    {
                        CanvasControl.SetContentAnchored(item, !item.IsAnchored);
                    }
                    break;
                case "trace-layer" when target is GridContentItem traceTarget:
                    IReadOnlyList<GridContentItem> traceTargets = CanvasControl.GetSelectedItems();
                    if (traceTargets.Count == 0)
                    {
                        traceTargets = new[] { traceTarget };
                    }

                    CanvasControl.TryTraceSelectionToSelectedGridLayer(traceTargets, out _);
                    break;
                case "cut" when target is GridContentItem cutTarget:
                    _ = CutContextMenuItemAsync(cutTarget);
                    break;
            }

            CanvasControl.Focus();
        }

        private async Task CutContextMenuItemAsync(GridContentItem item)
        {
            await CanvasControl.ClipboardService.CopyItemsAsync(new[] { item });
            CanvasControl.RemoveItem(item);
            CanvasControl.DeselectAllItems();
            CanvasControl.Focus();
        }

        private async Task PasteContextMenuItemsAsync()
        {
            List<GridContentItem> pasted = await CanvasControl.ClipboardService.PasteItemsAsync(
                CanvasControl.CursorPlacementOrigin);
            foreach (GridContentItem item in pasted)
            {
                item.LayerId = CanvasControl.LayerStack.SelectedGridLayerId;
                CanvasControl.AddItem(item);
            }

            CanvasControl.InvalidateVisual();
        }

        private void OnWindowKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && CanvasControl.CancelActiveResize())
            {
                e.Handled = true;
                return;
            }

            if (DocumentEditor.IsVisible)
            {
                if (e.Key == Key.Escape)
                {
                    DocumentEditor.Close();
                    e.Handled = true;
                }
                return;
            }

            if (ImageProperties.IsVisible)
            {
                if (e.Key == Key.Escape)
                {
                    ImageProperties.Close();
                    e.Handled = true;
                }
                return;
            }

            if (SlateHost.IsVisible)
            {
                if (e.Key == Key.Escape)
                {
                    SlateHost.Close();
                    e.Handled = true;
                }
                return;
            }

            if (_contextMenuService.ActiveMenu != null && e.Key != Key.Escape)
            {
                return;
            }

            if (!e.Handled)
            {
                if (e.Key == Key.Escape && _contextMenuService.ActiveMenu != null)
                {
                    _contextMenuService.CloseContextMenu();
                    CanvasControl.Focus();
                    e.Handled = true;
                    return;
                }

                _keybindModule.ProcessKeyDown(e, this);
            }
        }

        private void OnWindowKeyUp(object? sender, KeyEventArgs e)
        {
            if (!e.Handled)
            {
                _keybindModule.ProcessKeyUp(e, this);
            }
        }

        bool IKeybindHost.IsNotepadVisible => NotepadEditor.IsVisible;
        bool IKeybindHost.IsQuickNoteVisible => QuickNote.IsVisible;
        bool IKeybindHost.IsLayerManagerVisible => LayerManagerOverlay.IsVisible;
        bool IKeybindHost.IsToolArmed => CanvasControl.IsToolArmed;
        Size IKeybindHost.ViewportSize => CanvasControl.Bounds.Size;

        void IKeybindHost.CommitNotepadSave() => NotepadEditor.CommitSave();

        void IKeybindHost.OpenNotepadForNote(GridNote note, Rect sourceBounds) =>
            NotepadEditor.OpenForNote(note, sourceBounds, CanvasControl.Bounds.Size);

        void IKeybindHost.OpenNotepadForNotes(IReadOnlyList<GridNote> notes, Rect sourceBounds) =>
            NotepadEditor.OpenForNotes(notes.ToList(), sourceBounds, CanvasControl.Bounds.Size);

        void IKeybindHost.CloseQuickNote() => QuickNote.Close();
        void IKeybindHost.ToggleLayerManager() => LayerManagerOverlay.Toggle();
        void IKeybindHost.OpenContextMenuAtCursor()
        {
            Point screenPoint = CanvasControl.WorldToScreen(CanvasControl.CursorDescriptor.WorldOrigin);
            OpenContextMenuAt(screenPoint);
            Dispatcher.UIThread.Post(ContextMenuOverlay.FocusFirstCommand);
        }
        void IKeybindHost.OpenMemorySlate()
        {
            var search = new MemorySearchService(
                CanvasControl.MemoryLedger,
                () => CanvasControl.MemoryAnchors.ActiveAnchors,
                () => CanvasControl.Items.ToArray(),
                CanvasControl.FieldEngine);
            SlateHost.OpenMemory(search.Search);
        }
        void IKeybindHost.ToggleGridLines() => CanvasControl.ToggleGridLines();
        void IKeybindHost.FrameAllContent() => CanvasControl.FrameAllContent();
        void IKeybindHost.BeginSpacePan() => CanvasControl.BeginSpacePan();
        bool IKeybindHost.EndSpacePan() => CanvasControl.EndSpacePan();

        bool IKeybindHost.ProcessLayerKeyDown(KeyEventArgs args) => LayerManagerOverlay.ProcessKeyDown(args);

        bool IKeybindHost.ArmTool(ArmableContentType contentType) => CanvasControl.ArmTool(contentType);
        void IKeybindHost.DisarmTool() => CanvasControl.DisarmTool();
        bool IKeybindHost.NavigateLayer(int direction) => CanvasControl.NavigateLayer(direction);

        void IKeybindHost.JumpToBottomLayer() => CanvasControl.LayerStack.JumpToBottom();
        void IKeybindHost.JumpToTopLayer() => CanvasControl.LayerStack.JumpToTop();
        void IKeybindHost.CreateLayerAtBottom() => CanvasControl.LayerStack.InsertLayerAtBottom();
        void IKeybindHost.CreateLayerAtTop() => CanvasControl.LayerStack.InsertLayerAtTop();

        void IKeybindHost.InsertGridLayerAboveSelection()
        {
            int selectedZ = CanvasControl.LayerStack.GetZIndexForLayerId(CanvasControl.LayerStack.SelectedGridLayerId);
            CanvasControl.LayerStack.InsertLayerAbove(selectedZ);
        }

        void IKeybindHost.InsertGridLayerBelowSelection()
        {
            int selectedZ = CanvasControl.LayerStack.GetZIndexForLayerId(CanvasControl.LayerStack.SelectedGridLayerId);
            CanvasControl.LayerStack.InsertLayerBelow(selectedZ);
        }

        void IKeybindHost.ReorderSelectedGridLayer(int direction)
        {
            int selectedZ = CanvasControl.LayerStack.GetZIndexForLayerId(CanvasControl.LayerStack.SelectedGridLayerId);
            CanvasControl.LayerStack.ReorderSwap(selectedZ, selectedZ + direction);
        }

        IReadOnlyList<GridContentItem> IKeybindHost.GetSelectedItems() => CanvasControl.GetSelectedItems();
        IReadOnlyList<GridNote> IKeybindHost.GetSelectedNotes() => CanvasControl.GetSelectedNotes();

        GridNote? IKeybindHost.FindNoteAtCursor() =>
            CanvasControl.FindNoteAtCell(CanvasControl.CursorPlacementOrigin.X, CanvasControl.CursorPlacementOrigin.Y);

        Rect IKeybindHost.GetNoteScreenBounds(GridNote note) => CanvasControl.GetNoteScreenBounds(note);
        void IKeybindHost.SelectOnly(GridContentItem item) => CanvasControl.SelectOnly(item);
        void IKeybindHost.DeselectAllItems() => CanvasControl.DeselectAllItems();

        void IKeybindHost.ToggleAnchorOnSelection()
        {
            foreach (GridContentItem item in CanvasControl.GetSelectedItems())
            {
                CanvasControl.SetContentAnchored(item, !item.IsAnchored);
            }
        }

        void IKeybindHost.TraceSelectionToSelectedGridLayer() =>
            CanvasControl.TryTraceSelectionToSelectedGridLayer(CanvasControl.GetSelectedItems(), out _);

        void IKeybindHost.DeleteSelectedItems()
        {
            foreach (GridContentItem item in CanvasControl.GetSelectedItems().ToArray())
            {
                CanvasControl.RemoveItem(item);
            }

            CanvasControl.DeselectAllItems();
            CanvasControl.InvalidateVisual();
        }

        void IKeybindHost.SetSelectedColor(NoteColor color)
        {
            foreach (GridNote note in CanvasControl.GetSelectedNotes())
            {
                note.Color = color;
            }

            CanvasControl.RefreshFieldLedger();
            CanvasControl.InvalidateVisual();
        }

        Task IKeybindHost.CopyItemsAsync(IReadOnlyList<GridContentItem> items) =>
            CanvasControl.ClipboardService.CopyItemsAsync(items);

        async Task IKeybindHost.PasteItemsAtCursorAsync()
        {
            List<GridContentItem> pasted = await CanvasControl.ClipboardService.PasteItemsAsync(
                CanvasControl.CursorPlacementOrigin);
            if (pasted.Count == 0)
            {
                return;
            }

            foreach (GridContentItem item in pasted)
            {
                item.LayerId = CanvasControl.LayerStack.SelectedGridLayerId;
                CanvasControl.AddItem(item);
            }

            CanvasControl.SelectItems(pasted);
            CanvasControl.InvalidateVisual();
        }

        void IKeybindHost.RefreshVisuals()
        {
            CanvasControl.RefreshFieldLedger();
            CanvasControl.InvalidateVisual();
        }

        private sealed class HudBindingAdapter : IHudSpatialWatermarkBinding, IDisposable
        {
            private readonly GridCanvasControl _canvas;
            private readonly ISpatialLayerStateService _layers;

            public HudBindingAdapter(GridCanvasControl canvas, ISpatialLayerStateService layers)
            {
                _canvas = canvas;
                _layers = layers;
                _canvas.CameraChanged += OnCameraChanged;
                _layers.SelectedGridLayerChanged += OnSelectedGridLayerChanged;
                _layers.LayerStackChanged += OnLayerStackChanged;
            }

            public HudCameraState CameraState => new(_canvas.Zoom);

            public HudLayerIdentity SelectedGridLayer => ToIdentity(_layers.SelectedGridLayer);

            public event Action<HudCameraState>? CameraStateChanged;
            public event Action<HudLayerIdentity>? SelectedGridLayerChanged;

            public ValueTask<HudLayerRenameValidation> CommitLayerRenameAsync(
                HudLayerRenameRequest request,
                CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();
                int? layerZIndex = null;
                foreach (SpatialLayerModel candidate in _layers.Layers)
                {
                    if (string.Equals(candidate.Label, request.LayerId, StringComparison.Ordinal))
                    {
                        layerZIndex = candidate.ZIndex;
                        break;
                    }
                }

                if (layerZIndex is null)
                {
                    return ValueTask.FromResult(HudLayerRenameValidation.Refused("The selected Grid Layer no longer exists."));
                }

                bool renamed = _layers.RenameLayer(layerZIndex.Value, request.ProposedName, out string error);
                return ValueTask.FromResult(renamed
                    ? HudLayerRenameValidation.Accepted(request.ProposedName.Trim())
                    : HudLayerRenameValidation.Refused(error));
            }

            public void Dispose()
            {
                _canvas.CameraChanged -= OnCameraChanged;
                _layers.SelectedGridLayerChanged -= OnSelectedGridLayerChanged;
                _layers.LayerStackChanged -= OnLayerStackChanged;
            }

            private void OnCameraChanged() => CameraStateChanged?.Invoke(CameraState);

            private void OnSelectedGridLayerChanged(SpatialLayerModel model) =>
                SelectedGridLayerChanged?.Invoke(ToIdentity(model));

            private void OnLayerStackChanged() =>
                SelectedGridLayerChanged?.Invoke(SelectedGridLayer);

            private static HudLayerIdentity ToIdentity(SpatialLayerModel model) =>
                new(model.Label, model.Label, model.DisplayName, model.IsLocked);
        }
    }
}
