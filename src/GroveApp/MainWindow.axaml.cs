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
            Opened += (_, _) => _backdropManager.Apply(this);

            LayerSlate.BindLayerService(CanvasControl.LayerStack);
            ContextMenuOverlay.BindService(_contextMenuService);
            ContextMenuOverlay.CommandRequested += OnContextMenuCommandRequested;
            _watermarkBinding = new HudBindingAdapter(CanvasControl, CanvasControl.LayerStack);
            SpatialWatermark.Bind(_watermarkBinding);

            _focusRouter = new GlobalFocusPrecedenceRouter(
                this,
                () => NotepadEditor.IsVisible || QuickNote.IsVisible || DocumentEditor.IsVisible || ImageProperties.IsVisible || LayerSlate.IsVisible,
                () => !NotepadEditor.IsVisible && !QuickNote.IsVisible && !DocumentEditor.IsVisible && !ImageProperties.IsVisible && !LayerSlate.IsVisible,
                combination => _keybindModule.ProcessRoutedCombination(combination, this));
            Closed += (_, _) =>
            {
                _focusRouter.Dispose();
                SpatialWatermark.Dispose();
                _watermarkBinding?.Dispose();
            };

            // Enable Windows 11 Mica / Acrylic backdrop materials where supported
            TransparencyLevelHint = new[]
            {
                WindowTransparencyLevel.Mica,
                WindowTransparencyLevel.AcrylicBlur,
                WindowTransparencyLevel.Blur,
                WindowTransparencyLevel.None
            };

            // Enable FluentAvalonia Dark Titlebar & Extend Content Into Titlebar
            TitleBar.ExtendsContentIntoTitleBar = true;
            TitleBar.ButtonBackgroundColor = Colors.SurfaceChrome;
            TitleBar.ButtonForegroundColor = Colors.NoteText;
            TitleBar.ButtonHoverBackgroundColor = Colors.GridMaj;
            TitleBar.ButtonHoverForegroundColor = Colors.NoteText;
            TitleBar.ButtonPressedBackgroundColor = Colors.SurfaceRaised;
            TitleBar.ButtonPressedForegroundColor = Colors.NoteText;
            TitleBar.ButtonInactiveBackgroundColor = Colors.SurfaceChrome;
            TitleBar.ButtonInactiveForegroundColor = Colors.TitleBarInactiveForeground;

            // Wire GridCanvasControl events
            CanvasControl.NoteSelected += OnNoteSelected;
            CanvasControl.NoteDoubleClicked += OnNoteDoubleClicked;
            CanvasControl.ContentDoubleClicked += OnContentDoubleClicked;
            CanvasControl.EmptyCellDoubleClicked += OnEmptyCellDoubleClicked;
            CanvasControl.CameraChanged += OnCameraChanged;
            CanvasControl.ArmedItemPlaced += OnArmedItemPlaced;

            // Handle Pointer Press on Canvas for Double Click detection
            CanvasControl.AddHandler(PointerPressedEvent, OnCanvasPointerPressed, RoutingStrategies.Tunnel);
            CanvasControl.AddHandler(PointerReleasedEvent, OnCanvasPointerReleased, RoutingStrategies.Tunnel);
            CanvasControl.AddHandler(PointerMovedEvent, OnCanvasPointerMoved, RoutingStrategies.Tunnel);

            _contextLongPressTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _contextLongPressTimer.Tick += OnContextLongPressTimerTick;

            // Wire Information Layer Overlays
            NotepadEditor.SaveRequested += OnNotepadEditorSaveRequested;
            NotepadEditor.Closed += OnOverlayClosed;

            QuickNote.SaveAndPlaceRequested += OnQuickNoteSaveAndPlaceRequested;
            QuickNote.PlacedNoteTextCommitted += OnQuickNotePlacedNoteTextCommitted;
            QuickNote.Closed += OnOverlayClosed;
            DocumentEditor.SaveRequested += OnDocumentEditorSaveRequested;
            DocumentEditor.Closed += OnOverlayClosed;
            ImageProperties.Closed += OnOverlayClosed;

            LayerSlate.Closed += OnOverlayClosed;

            // Handle Window Resizing for Editor Overlay anchor update
            SizeChanged += (s, e) => UpdateNotepadEditorPosition();

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
            Point screenPoint = e.GetPosition(CanvasControl);

            _contextLongPressTimer.Stop();
            _contextPressActive = false;

            if (_contextMenuService.ActiveMenu != null && !clickInfo.Properties.IsRightButtonPressed)
            {
                _contextMenuService.ProcessPointerPressed(new ScreenPoint(screenPoint.X, screenPoint.Y));
            }

            if (clickInfo.Properties.IsRightButtonPressed)
            {
                OpenContextMenuAt(screenPoint);
                e.Handled = true;
                return;
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
            var cell = CanvasControl.WorldToCell(CanvasControl.ScreenToWorld(screenPoint));
            var target = CanvasControl.FindItemAtCell(cell.cellX, cell.cellY);
            IEnumerable<string> targetIds = target is null
                ? Array.Empty<string>()
                : CanvasControl.GetSelectedItems().Count > 1
                    ? CanvasControl.GetSelectedItems().Select(item => item.Id)
                    : new[] { target.Id };
            _contextMenuService.OpenContextMenuAt(
                new ScreenPoint(screenPoint.X, screenPoint.Y),
                new GroveApp.Models.Interaction.CellCoordinate(cell.cellX, cell.cellY),
                CanvasControl.LayerStack.ActiveLayerId,
                targetIds,
                new ScreenSize(CanvasControl.Bounds.Width, CanvasControl.Bounds.Height));
        }

        private void UpdateHudStatus()
        {
            TxtCellCoord.Text = $"CELL: ({CanvasControl.CursorCellX}, {CanvasControl.CursorCellY})";
            int zoomPercent = (int)Math.Round(CanvasControl.Zoom * 100);
            TxtZoom.Text = $"ZOOM: {zoomPercent}%";
            TxtNoteCount.Text = $"ITEMS: {CanvasControl.Items.Count}";
            int metadataCount = CanvasControl.FieldEngine.GetTotalMetadataSourcesCount();
            TxtLedgerCount.Text = $"LEDGER METADATA: {metadataCount}";
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
                DocumentEditor.OpenForDocument(
                    document,
                    CanvasControl.GetContentScreenBounds(document),
                    Bounds.Size);
            }
            else if (item is GridImage image)
            {
                ImageProperties.OpenForImage(
                    image,
                    CanvasControl.GetContentScreenBounds(image),
                    Bounds.Size);
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

        private void OnEmptyCellDoubleClicked(int cellX, int cellY)
        {
            var newNote = new GridNote(cellX, cellY, "New Note", NoteColor.Violet, layerId: CanvasControl.LayerStack.ActiveLayerId);
            CanvasControl.AddItem(newNote);
            CanvasControl.DeselectAllItems();
            newNote.IsSelected = true;
            CanvasControl.SelectedItem = newNote;
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

        private void OnQuickNoteSaveAndPlaceRequested(QuickNoteItem item)
        {
            var newNote = new GridNote(
                CanvasControl.CursorCellX,
                CanvasControl.CursorCellY,
                item.Text,
                NoteColor.Violet,
                isAnchored: true,
                layerId: CanvasControl.LayerStack.ActiveLayerId);
            CanvasControl.AddItem(newNote);
            CanvasControl.DeselectAllItems();
            newNote.IsSelected = true;
            CanvasControl.SelectedItem = newNote;
            item.IsAnchored = true;
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
                    DocumentEditor.OpenForDocument(document, CanvasControl.GetContentScreenBounds(document), Bounds.Size);
                    break;
                case "open" when target is GridImage image:
                    ImageProperties.OpenForImage(image, CanvasControl.GetContentScreenBounds(image), Bounds.Size);
                    break;
                case "anchor" when target != null:
                    IReadOnlyList<GridContentItem> anchorTargets = CanvasControl.GetSelectedItems();
                    if (anchorTargets.Count == 0)
                    {
                        anchorTargets = new[] { target };
                    }

                    foreach (GridContentItem item in anchorTargets)
                    {
                        item.IsAnchored = !item.IsAnchored;
                    }
                    CanvasControl.RefreshFieldLedger();
                    CanvasControl.InvalidateVisual();
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
            }
        }

        private async Task PasteContextMenuItemsAsync()
        {
            List<GridContentItem> pasted = await CanvasControl.ClipboardService.PasteItemsAsync(
                new EngineCellCoordinate(CanvasControl.CursorCellX, CanvasControl.CursorCellY));
            foreach (GridContentItem item in pasted)
            {
                item.LayerId = CanvasControl.LayerStack.ActiveLayerId;
                CanvasControl.AddItem(item);
            }

            CanvasControl.InvalidateVisual();
        }

        private void OnWindowKeyDown(object? sender, KeyEventArgs e)
        {
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

            if (!e.Handled)
            {
                _keybindModule.ProcessKeyDown(e, this);
            }
        }

        bool IKeybindHost.IsNotepadVisible => NotepadEditor.IsVisible;
        bool IKeybindHost.IsQuickNoteVisible => QuickNote.IsVisible;
        bool IKeybindHost.IsLayerSlateVisible => LayerSlate.IsVisible;
        bool IKeybindHost.IsToolArmed => CanvasControl.IsToolArmed;
        int IKeybindHost.CursorCellX => CanvasControl.CursorCellX;
        int IKeybindHost.CursorCellY => CanvasControl.CursorCellY;
        Size IKeybindHost.ViewportSize => CanvasControl.Bounds.Size;

        void IKeybindHost.CommitNotepadSave() => NotepadEditor.CommitSave();

        void IKeybindHost.OpenNotepadForNote(GridNote note, Rect sourceBounds) =>
            NotepadEditor.OpenForNote(note, sourceBounds, CanvasControl.Bounds.Size);

        void IKeybindHost.OpenNotepadForNotes(IReadOnlyList<GridNote> notes, Rect sourceBounds) =>
            NotepadEditor.OpenForNotes(notes.ToList(), sourceBounds, CanvasControl.Bounds.Size);

        void IKeybindHost.CloseQuickNote() => QuickNote.Close();
        void IKeybindHost.ToggleLayerSlate() => LayerSlate.Toggle();
        void IKeybindHost.ToggleLayerIsolation() => CanvasControl.LayerActivation.ToggleIsolationMode();

        bool IKeybindHost.ProcessLayerKeyDown(KeyEventArgs args) => LayerSlate.ProcessKeyDown(args);

        bool IKeybindHost.ArmTool(ArmableContentType contentType) => CanvasControl.ArmTool(contentType);
        void IKeybindHost.DisarmTool() => CanvasControl.DisarmTool();
        bool IKeybindHost.NavigateLayer(int direction) => CanvasControl.NavigateLayer(direction);

        void IKeybindHost.JumpToBottomLayer() => CanvasControl.LayerStack.JumpToBottom();
        void IKeybindHost.JumpToTopLayer() => CanvasControl.LayerStack.JumpToTop();

        void IKeybindHost.InsertLayerAboveActive()
        {
            int activeZ = CanvasControl.LayerStack.GetZIndexForLayerId(CanvasControl.LayerStack.ActiveLayerId);
            CanvasControl.LayerStack.InsertLayerAbove(activeZ);
        }

        void IKeybindHost.InsertLayerBelowActive()
        {
            int activeZ = CanvasControl.LayerStack.GetZIndexForLayerId(CanvasControl.LayerStack.ActiveLayerId);
            CanvasControl.LayerStack.InsertLayerBelow(activeZ);
        }

        void IKeybindHost.ReorderActiveLayer(int direction)
        {
            int activeZ = CanvasControl.LayerStack.GetZIndexForLayerId(CanvasControl.LayerStack.ActiveLayerId);
            CanvasControl.LayerStack.ReorderSwap(activeZ, activeZ + direction);
        }

        IReadOnlyList<GridContentItem> IKeybindHost.GetSelectedItems() => CanvasControl.GetSelectedItems();
        IReadOnlyList<GridNote> IKeybindHost.GetSelectedNotes() => CanvasControl.GetSelectedNotes();

        GridNote? IKeybindHost.FindNoteAtCursor() =>
            CanvasControl.FindNoteAtCell(CanvasControl.CursorCellX, CanvasControl.CursorCellY);

        Rect IKeybindHost.GetNoteScreenBounds(GridNote note) => CanvasControl.GetNoteScreenBounds(note);
        void IKeybindHost.SelectOnly(GridContentItem item) => CanvasControl.SelectOnly(item);
        void IKeybindHost.DeselectAllItems() => CanvasControl.DeselectAllItems();

        void IKeybindHost.ToggleAnchorOnSelection()
        {
            foreach (GridContentItem item in CanvasControl.GetSelectedItems())
            {
                item.IsAnchored = !item.IsAnchored;
            }

            CanvasControl.RefreshFieldLedger();
            CanvasControl.InvalidateVisual();
        }

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
                new EngineCellCoordinate(CanvasControl.CursorCellX, CanvasControl.CursorCellY));
            if (pasted.Count == 0)
            {
                return;
            }

            CanvasControl.DeselectAllItems();
            foreach (GridContentItem item in pasted)
            {
                item.LayerId = CanvasControl.LayerStack.ActiveLayerId;
                CanvasControl.AddItem(item);
                item.IsSelected = true;
            }

            CanvasControl.SelectedItem = pasted[^1];
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
                _layers.ActiveLayerChanged += OnActiveLayerChanged;
                _layers.LayerStackChanged += OnLayerStackChanged;
            }

            public HudCameraState CameraState => new(_canvas.Zoom);

            public HudLayerIdentity ActiveLayer => ToIdentity(_layers.ActiveLayer);

            public event Action<HudCameraState>? CameraStateChanged;
            public event Action<HudLayerIdentity>? ActiveLayerChanged;

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
                    return ValueTask.FromResult(HudLayerRenameValidation.Refused("The active layer no longer exists."));
                }

                bool renamed = _layers.RenameLayer(layerZIndex.Value, request.ProposedName, out string error);
                return ValueTask.FromResult(renamed
                    ? HudLayerRenameValidation.Accepted(request.ProposedName.Trim())
                    : HudLayerRenameValidation.Refused(error));
            }

            public void Dispose()
            {
                _canvas.CameraChanged -= OnCameraChanged;
                _layers.ActiveLayerChanged -= OnActiveLayerChanged;
                _layers.LayerStackChanged -= OnLayerStackChanged;
            }

            private void OnCameraChanged() => CameraStateChanged?.Invoke(CameraState);

            private void OnActiveLayerChanged(SpatialLayerModel model) =>
                ActiveLayerChanged?.Invoke(ToIdentity(model));

            private void OnLayerStackChanged() =>
                ActiveLayerChanged?.Invoke(ActiveLayer);

            private static HudLayerIdentity ToIdentity(SpatialLayerModel model) =>
                new(model.Label, model.Label, model.DisplayName, model.IsLocked);
        }
    }
}
