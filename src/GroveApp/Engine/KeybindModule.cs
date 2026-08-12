using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Input;
using GroveApp.Controls;
using GroveApp.Models;

namespace GroveApp.Engine
{
    /// <summary>
    /// Deep engine module for context-aware keybindings across Grove v9 Spatial Desktop.
    /// Handles Spacebar (open/multi-select queue), Ctrl+C (Copy), Ctrl+V (Paste),
    /// Ctrl+Enter (commit), Ctrl+E (WYSIWYG toggle), Tab/Ctrl+Tab (tab cycle),
    /// N (Quick Note), A (Toggle Anchor), Del/Backspace (Delete), Esc (Dismiss/Deselect).
    /// </summary>
    public class KeybindModule
    {
        public bool ProcessKeyDown(
            KeyEventArgs e,
            GridCanvasControl canvas,
            LocalEditorOverlay localEditor,
            QuickNoteOverlay quickNote)
        {
            // 1. Local Editor Active Key Handler
            if (localEditor.IsVisible)
            {
                return ProcessLocalEditorKey(e, localEditor);
            }

            // 2. Quick Note Active Key Handler
            if (quickNote.IsVisible)
            {
                return ProcessQuickNoteKey(e, quickNote);
            }

            // 3. Spatial Grid Canvas Active Key Handler
            return ProcessCanvasKey(e, canvas, localEditor, quickNote);
        }

        private bool ProcessLocalEditorKey(KeyEventArgs e, LocalEditorOverlay localEditor)
        {
            // Ctrl + Enter: Commit Save
            if (e.Key == Key.Enter && e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                localEditor.CommitSave();
                e.Handled = true;
                return true;
            }

            // Ctrl + E: Toggle WYSIWYG vs RAW Mode
            if (e.Key == Key.E && e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                localEditor.ToggleWysiwygMode();
                e.Handled = true;
                return true;
            }

            // Tab / Ctrl+Tab: Cycle through queued tabs in Local Editor
            if (e.Key == Key.Tab)
            {
                if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
                {
                    localEditor.SelectPreviousTab();
                }
                else
                {
                    localEditor.SelectNextTab();
                }
                e.Handled = true;
                return true;
            }

            // Esc: Dismiss / Cancel Editor
            if (e.Key == Key.Escape)
            {
                localEditor.HandleEscape();
                e.Handled = true;
                return true;
            }

            return false;
        }

        private bool ProcessQuickNoteKey(KeyEventArgs e, QuickNoteOverlay quickNote)
        {
            if (e.Key == Key.Escape)
            {
                quickNote.Close();
                e.Handled = true;
                return true;
            }
            return false;
        }

        private bool ProcessCanvasKey(
            KeyEventArgs e,
            GridCanvasControl canvas,
            LocalEditorOverlay localEditor,
            QuickNoteOverlay quickNote)
        {
            // Ctrl + C: Copy selected items to native clipboard (ADR-014)
            if (e.Key == Key.C && e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                var selectedItems = canvas.GetSelectedItems();
                if (selectedItems.Count > 0)
                {
                    _ = canvas.ClipboardService.CopyItemsAsync(selectedItems);
                    e.Handled = true;
                    return true;
                }
            }

            // Ctrl + V: Paste items from native clipboard at cell cursor (ADR-014)
            if (e.Key == Key.V && e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                _ = Task.Run(async () =>
                {
                    var pasted = await canvas.ClipboardService.PasteItemsAsync(new CellCoordinate(canvas.CursorCellX, canvas.CursorCellY));
                    if (pasted.Count > 0)
                    {
                        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                        {
                            canvas.DeselectAllItems();
                            foreach (var item in pasted)
                            {
                                canvas.Items.Add(item);
                                item.IsSelected = true;
                            }
                            canvas.SelectedItem = pasted[^1];
                            canvas.InvalidateVisual();
                        });
                    }
                });
                e.Handled = true;
                return true;
            }

            // Spacebar: Open Local Editor on selected Note(s) or note under cursor
            if (e.Key == Key.Space)
            {
                var selectedNotes = canvas.GetSelectedNotes();
                if (selectedNotes.Count >= 2)
                {
                    Rect primaryBounds = canvas.GetNoteScreenBounds(selectedNotes[0]);
                    localEditor.OpenForNotes(selectedNotes, primaryBounds, canvas.Bounds.Size);
                    e.Handled = true;
                    return true;
                }
                else if (selectedNotes.Count == 1)
                {
                    Rect bounds = canvas.GetNoteScreenBounds(selectedNotes[0]);
                    localEditor.OpenForNote(selectedNotes[0], bounds, canvas.Bounds.Size);
                    e.Handled = true;
                    return true;
                }
                else
                {
                    GridNote? cursorNote = canvas.FindNoteAtCell(canvas.CursorCellX, canvas.CursorCellY);
                    if (cursorNote != null)
                    {
                        canvas.DeselectAllItems();
                        cursorNote.IsSelected = true;
                        canvas.SelectedItem = cursorNote;
                        canvas.InvalidateVisual();

                        Rect bounds = canvas.GetNoteScreenBounds(cursorNote);
                        localEditor.OpenForNote(cursorNote, bounds, canvas.Bounds.Size);
                        e.Handled = true;
                        return true;
                    }
                }
            }

            // N: Quick Note
            if (e.Key == Key.N)
            {
                quickNote.Open();
                e.Handled = true;
                return true;
            }

            // A: Toggle Anchor
            if (e.Key == Key.A)
            {
                var selectedItems = canvas.GetSelectedItems();
                if (selectedItems.Count > 0)
                {
                    foreach (var item in selectedItems)
                    {
                        item.IsAnchored = !item.IsAnchored;
                    }
                    canvas.InvalidateVisual();
                    e.Handled = true;
                    return true;
                }
            }

            // Del / Backspace: Delete selected item(s)
            if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                var selectedItems = canvas.GetSelectedItems();
                if (selectedItems.Count > 0)
                {
                    foreach (var item in selectedItems)
                    {
                        canvas.Items.Remove(item);
                    }
                    canvas.SelectedItem = null;
                    canvas.InvalidateVisual();
                    e.Handled = true;
                    return true;
                }
            }

            // Esc: Clear selection
            if (e.Key == Key.Escape)
            {
                var selectedItems = canvas.GetSelectedItems();
                if (selectedItems.Count > 0 || canvas.SelectedItem != null)
                {
                    canvas.DeselectAllItems();
                    canvas.InvalidateVisual();
                    e.Handled = true;
                    return true;
                }
            }

            // Color Swatches: Keys 2, 3, 4
            if (e.Key == Key.D2 || e.Key == Key.NumPad2)
            {
                SetSelectedColor(canvas, NoteColor.Violet);
                e.Handled = true;
                return true;
            }
            if (e.Key == Key.D3 || e.Key == Key.NumPad3)
            {
                SetSelectedColor(canvas, NoteColor.Clay);
                e.Handled = true;
                return true;
            }
            if (e.Key == Key.D4 || e.Key == Key.NumPad4)
            {
                SetSelectedColor(canvas, NoteColor.SlateBlue);
                e.Handled = true;
                return true;
            }

            return false;
        }

        private static void SetSelectedColor(GridCanvasControl canvas, NoteColor color)
        {
            var selectedNotes = canvas.GetSelectedNotes();
            foreach (var note in selectedNotes)
            {
                note.Color = color;
            }
            canvas.InvalidateVisual();
        }
    }
}
