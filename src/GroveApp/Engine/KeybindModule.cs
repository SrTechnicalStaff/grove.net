using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Input;
using GroveApp.Controls;
using GroveApp.Models;

namespace GroveApp.Engine
{
    /// <summary>
    /// Deep engine module for context-aware keybindings across Grove v9 Spatial Desktop.
    /// Handles Spacebar (open/multi-select queue), Ctrl+Enter (commit), Ctrl+E (WYSIWYG toggle),
    /// Tab/Ctrl+Tab (tab cycle), N (Quick Note), A (Toggle Anchor), Del/Backspace (Delete), Esc (Dismiss/Deselect).
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
            // Spacebar: Open Local Editor on selected Note(s) or note under cursor
            if (e.Key == Key.Space)
            {
                var selectedNotes = canvas.GetSelectedNotes();
                if (selectedNotes.Count >= 2)
                {
                    // Multi-selection queue in Local Editor
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
                    // No selection: check note under grid cursor
                    GridNote? cursorNote = canvas.FindNoteAtCell(canvas.CursorCellX, canvas.CursorCellY);
                    if (cursorNote != null)
                    {
                        canvas.DeselectAllNotes();
                        cursorNote.IsSelected = true;
                        canvas.SelectedNote = cursorNote;
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
                var selectedNotes = canvas.GetSelectedNotes();
                if (selectedNotes.Count > 0)
                {
                    foreach (var note in selectedNotes)
                    {
                        note.IsAnchored = !note.IsAnchored;
                    }
                    canvas.InvalidateVisual();
                    e.Handled = true;
                    return true;
                }
            }

            // Del / Backspace: Delete selected note(s)
            if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                var selectedNotes = canvas.GetSelectedNotes();
                if (selectedNotes.Count > 0)
                {
                    foreach (var note in selectedNotes)
                    {
                        canvas.Notes.Remove(note);
                    }
                    canvas.SelectedNote = null;
                    canvas.InvalidateVisual();
                    e.Handled = true;
                    return true;
                }
            }

            // Esc: Clear selection
            if (e.Key == Key.Escape)
            {
                var selectedNotes = canvas.GetSelectedNotes();
                if (selectedNotes.Count > 0 || canvas.SelectedNote != null)
                {
                    canvas.DeselectAllNotes();
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
