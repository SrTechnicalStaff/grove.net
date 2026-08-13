using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Input;
using GroveApp.Models;

namespace GroveApp.Engine;

/// <summary>
/// Application seam consumed by the key router. The engine owns key policy;
/// the window owns how overlays, selection, clipboard, and layer controls are
/// represented in the UI.
/// </summary>
public interface IKeybindHost
{
    bool IsNotepadVisible { get; }
    bool IsQuickNoteVisible { get; }
    bool IsLayerManagerVisible { get; }
    bool IsToolArmed { get; }
    Size ViewportSize { get; }
    void ToggleGridLines();
    void FrameAllContent();
    void BeginSpacePan();
    bool EndSpacePan();

    void CommitNotepadSave();
    void OpenNotepadForNote(GridNote note, Rect sourceBounds);
    void OpenNotepadForNotes(IReadOnlyList<GridNote> notes, Rect sourceBounds);
    void CloseQuickNote();
    void ToggleLayerManager();
    void OpenContextMenuAtCursor();
    void OpenMemorySlate();
    void ToggleLayerIsolation();
    bool ProcessLayerKeyDown(KeyEventArgs args);

    bool ArmTool(ArmableContentType contentType);
    void DisarmTool();
    bool NavigateLayer(int direction);
    void JumpToBottomLayer();
    void JumpToTopLayer();
    void CreateLayerAtBottom();
    void CreateLayerAtTop();
    void InsertLayerAboveActive();
    void InsertLayerBelowActive();
    void ReorderActiveLayer(int direction);

    IReadOnlyList<GridContentItem> GetSelectedItems();
    IReadOnlyList<GridNote> GetSelectedNotes();
    GridNote? FindNoteAtCursor();
    Rect GetNoteScreenBounds(GridNote note);
    void SelectOnly(GridContentItem item);
    void DeselectAllItems();
    void ToggleAnchorOnSelection();
    void TraceSelectionToActiveLayer();
    void DeleteSelectedItems();
    void SetSelectedColor(NoteColor color);
    Task CopyItemsAsync(IReadOnlyList<GridContentItem> items);
    Task PasteItemsAtCursorAsync();
    void RefreshVisuals();
}
