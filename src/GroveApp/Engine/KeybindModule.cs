using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Input;
using GroveApp.Models;

namespace GroveApp.Engine
{
    /// <summary>
    /// Deep engine module for context-aware keybindings across Grove v9 Spatial Desktop.
    /// Handles Spacebar (open Notepad Editor), Ctrl+C (Copy), Ctrl+V (Paste),
    /// N/Shift+N/D (tool arming), A (Toggle Anchor), Del/Backspace (Delete),
    /// Esc (Dismiss/Deselect), bracket layer navigation, and layer management keybindings.
    /// </summary>
    public class KeybindModule
    {
        public bool ProcessKeyDown(
            KeyEventArgs e,
            IKeybindHost host)
        {
            if (host.IsNotepadVisible)
            {
                return ProcessNotepadEditorKey(e, host);
            }

            if (host.IsQuickNoteVisible)
            {
                return ProcessQuickNoteKey(e, host);
            }

            if (host.IsLayerManagerVisible && host.ProcessLayerKeyDown(e))
            {
                return true;
            }

            return ProcessCanvasKey(e, host);
        }

        public bool ProcessRoutedCombination(KeyCombination combination, IKeybindHost host)
        {
            if (combination.Key == Key.F && combination.Modifiers == KeyModifiers.None)
            {
                host.FrameAllContent();
                return true;
            }

            if (combination.Key == Key.M && combination.Modifiers == KeyModifiers.None)
            {
                host.OpenMemorySlate();
                return true;
            }

            if (combination.Key == Key.N && combination.Modifiers == KeyModifiers.None)
            {
                return host.ArmTool(ArmableContentType.Note);
            }

            if (combination.Key == Key.N && combination.Modifiers == KeyModifiers.Shift)
            {
                return host.ArmTool(ArmableContentType.QuickNote);
            }

            if (combination.Key == Key.D && combination.Modifiers == KeyModifiers.None)
            {
                return host.ArmTool(ArmableContentType.Document);
            }

            if (combination.Key == Key.Escape && host.IsToolArmed)
            {
                host.DisarmTool();
                return true;
            }

            return false;
        }

        public bool ProcessKeyUp(KeyEventArgs e, IKeybindHost host)
        {
            if (e.Key == Key.Space)
            {
                bool wasPan = host.EndSpacePan();
                if (!wasPan)
                {
                    OpenSpaceTarget(host);
                }

                e.Handled = true;
                return true;
            }

            return false;
        }

        private bool ProcessNotepadEditorKey(KeyEventArgs e, IKeybindHost host)
        {
            if (e.Key == Key.Escape || (e.Key == Key.Enter && e.KeyModifiers.HasFlag(KeyModifiers.Control)))
            {
                host.CommitNotepadSave();
                e.Handled = true;
                return true;
            }

            return false;
        }

        private bool ProcessQuickNoteKey(KeyEventArgs e, IKeybindHost host)
        {
            if (e.Key == Key.Escape)
            {
                host.CloseQuickNote();
                e.Handled = true;
                return true;
            }
            return false;
        }

        private bool ProcessCanvasKey(
            KeyEventArgs e,
            IKeybindHost host)
        {
            // Ctrl + C: Copy selected items to native clipboard (ADR-014)
            if (e.Key == Key.C && e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                var selectedItems = host.GetSelectedItems();
                if (selectedItems.Count > 0)
                {
                    _ = host.CopyItemsAsync(selectedItems);
                    e.Handled = true;
                    return true;
                }
            }

            // Ctrl + V: Paste items from native clipboard at cell cursor (ADR-014)
            if (e.Key == Key.V && e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                _ = host.PasteItemsAtCursorAsync();
                e.Handled = true;
                return true;
            }

            // Space is a transient pan modifier. A tap preserves the existing
            // editor shortcut on key-up; a drag is consumed by Plane 0 panning.
            if (e.Key == Key.Space)
            {
                host.BeginSpacePan();
                e.Handled = true;
                return true;
            }

            if (e.Key == Key.Apps ||
                (e.Key == Key.F10 && e.KeyModifiers.HasFlag(KeyModifiers.Shift)))
            {
                host.OpenContextMenuAtCursor();
                e.Handled = true;
                return true;
            }

            // Spatial arming keys
            if (e.Key is Key.N or Key.D)
            {
                if (ProcessRoutedCombination(KeyCombination.From(e), host))
                {
                    e.Handled = true;
                    return true;
                }
            }

            // L: Toggle the Layer Manager overlay.
            if (e.Key == Key.L &&
                (e.KeyModifiers == KeyModifiers.None ||
                 (e.KeyModifiers.HasFlag(KeyModifiers.Control) && e.KeyModifiers.HasFlag(KeyModifiers.Shift))))
            {
                host.ToggleLayerManager();
                e.Handled = true;
                return true;
            }

            // Ctrl+I: isolate the active layer and suppress inactive presence.
            if (e.Key == Key.I && e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                host.ToggleLayerIsolation();
                e.Handled = true;
                return true;
            }

            // A: Toggle Anchor
            if (e.Key == Key.G && e.KeyModifiers == KeyModifiers.None)
            {
                host.ToggleGridLines();
                e.Handled = true;
                return true;
            }

            if (e.Key == Key.F && e.KeyModifiers == KeyModifiers.None)
            {
                host.FrameAllContent();
                e.Handled = true;
                return true;
            }

            if (e.Key == Key.M && e.KeyModifiers == KeyModifiers.None)
            {
                host.OpenMemorySlate();
                e.Handled = true;
                return true;
            }

            if (e.Key == Key.A)
            {
                var selectedItems = host.GetSelectedItems();
                if (selectedItems.Count > 0)
                {
                    host.ToggleAnchorOnSelection();
                    e.Handled = true;
                    return true;
                }
            }

            if (e.Key == Key.D1 || e.Key == Key.NumPad1)
            {
                if (host.GetSelectedItems().Count > 0)
                {
                    host.TraceSelectionToActiveLayer();
                    e.Handled = true;
                    return true;
                }
            }

            // Del / Backspace: Delete selected item(s)
            if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                var selectedItems = host.GetSelectedItems();
                if (selectedItems.Count > 0)
                {
                    host.DeleteSelectedItems();
                    e.Handled = true;
                    return true;
                }
            }

            // Esc: Clear selection
            if (e.Key == Key.Escape)
            {
                if (ProcessRoutedCombination(KeyCombination.From(e), host))
                {
                    e.Handled = true;
                    return true;
                }

                var selectedItems = host.GetSelectedItems();
                if (selectedItems.Count > 0)
                {
                    host.DeselectAllItems();
                    host.RefreshVisuals();
                    e.Handled = true;
                    return true;
                }
            }

            // Layer Management Keybindings:
            // Shift+[ / Shift+] (Create Bottom / Top)
            if (e.Key == Key.OemOpenBrackets &&
                e.KeyModifiers == KeyModifiers.Shift)
            {
                host.CreateLayerAtBottom();
                e.Handled = true;
                return true;
            }
            if (e.Key == Key.OemCloseBrackets &&
                e.KeyModifiers == KeyModifiers.Shift)
            {
                host.CreateLayerAtTop();
                e.Handled = true;
                return true;
            }

            // [ / ] (Navigate Down / Up)
            if (e.Key == Key.OemOpenBrackets && e.KeyModifiers == KeyModifiers.None)
            {
                host.NavigateLayer(-1);
                e.Handled = true;
                return true;
            }
            if (e.Key == Key.OemCloseBrackets && e.KeyModifiers == KeyModifiers.None)
            {
                host.NavigateLayer(1);
                e.Handled = true;
                return true;
            }

            if (e.Key == Key.OemOpenBrackets && e.KeyModifiers == KeyModifiers.Control)
            {
                host.InsertLayerBelowActive();
                e.Handled = true;
                return true;
            }
            if (e.Key == Key.OemCloseBrackets && e.KeyModifiers == KeyModifiers.Control)
            {
                host.InsertLayerAboveActive();
                e.Handled = true;
                return true;
            }

            if (e.Key == Key.OemOpenBrackets && e.KeyModifiers == KeyModifiers.Alt)
            {
                host.ReorderActiveLayer(-1);
                e.Handled = true;
                return true;
            }
            if (e.Key == Key.OemCloseBrackets && e.KeyModifiers == KeyModifiers.Alt)
            {
                host.ReorderActiveLayer(1);
                e.Handled = true;
                return true;
            }

            // Ctrl+Shift+N (Insert Layer Above)
            if (e.Key == Key.N && e.KeyModifiers.HasFlag(KeyModifiers.Control) && e.KeyModifiers.HasFlag(KeyModifiers.Shift) && !e.KeyModifiers.HasFlag(KeyModifiers.Alt))
            {
                host.InsertLayerAboveActive();
                e.Handled = true;
                return true;
            }

            // Ctrl+Alt+Shift+N (Insert Layer Below)
            if (e.Key == Key.N && e.KeyModifiers.HasFlag(KeyModifiers.Control) && e.KeyModifiers.HasFlag(KeyModifiers.Shift) && e.KeyModifiers.HasFlag(KeyModifiers.Alt))
            {
                host.InsertLayerBelowActive();
                e.Handled = true;
                return true;
            }

            // Alt+Up / Alt+Down (Reorder Swap Up / Down)
            if (e.Key == Key.Up && e.KeyModifiers.HasFlag(KeyModifiers.Alt))
            {
                host.ReorderActiveLayer(1);
                e.Handled = true;
                return true;
            }
            if (e.Key == Key.Down && e.KeyModifiers.HasFlag(KeyModifiers.Alt))
            {
                host.ReorderActiveLayer(-1);
                e.Handled = true;
                return true;
            }

            // Color Swatches: Keys 2, 3, 4
            if (e.Key == Key.D2 || e.Key == Key.NumPad2)
            {
                host.SetSelectedColor(NoteColor.Violet);
                e.Handled = true;
                return true;
            }
            if (e.Key == Key.D3 || e.Key == Key.NumPad3)
            {
                host.SetSelectedColor(NoteColor.Clay);
                e.Handled = true;
                return true;
            }
            if (e.Key == Key.D4 || e.Key == Key.NumPad4)
            {
                host.SetSelectedColor(NoteColor.SlateBlue);
                e.Handled = true;
                return true;
            }

            return false;
        }

        private static void OpenSpaceTarget(IKeybindHost host)
        {
            var selectedNotes = host.GetSelectedNotes();
            if (selectedNotes.Count >= 2)
            {
                host.OpenNotepadForNotes(selectedNotes, host.GetNoteScreenBounds(selectedNotes[0]));
            }
            else if (selectedNotes.Count == 1)
            {
                host.OpenNotepadForNote(selectedNotes[0], host.GetNoteScreenBounds(selectedNotes[0]));
            }
            else
            {
                GridNote? cursorNote = host.FindNoteAtCursor();
                if (cursorNote != null)
                {
                    host.SelectOnly(cursorNote);
                    host.OpenNotepadForNote(cursorNote, host.GetNoteScreenBounds(cursorNote));
                }
            }
        }

    }
}
