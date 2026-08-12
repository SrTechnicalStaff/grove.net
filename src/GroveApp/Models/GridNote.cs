using System;
using GroveApp.DesignSystem;

namespace GroveApp.Models
{
    public enum NoteColor
    {
        Violet,
        Clay,
        SlateBlue
    }

    public class GridNote
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public int CellX { get; set; }
        public int CellY { get; set; }
        public int SizeCells { get; set; } = 1; // Footprint: SizeCells x SizeCells
        public string Text { get; set; } = "";
        public NoteColor Color { get; set; } = NoteColor.Violet;
        public bool IsSelected { get; set; }
        public bool IsHovered { get; set; }
        public bool IsAnchored { get; set; }

        public GridNote(int cellX, int cellY, string text = "", NoteColor color = NoteColor.Violet, bool isAnchored = false)
        {
            CellX = cellX;
            CellY = cellY;
            Text = text;
            Color = color;
            IsAnchored = isAnchored;
            RecalculateFootprint();
        }

        public void RecalculateFootprint()
        {
            // Solve footprint n x n per Note.md geometry:
            // n=1 box holds up to ~100 chars, n=2 holds up to ~280 chars, n=3 for larger prose.
            int charCount = Text.Length;
            if (charCount > 280)
                SizeCells = 3;
            else if (charCount > 100)
                SizeCells = 2;
            else
                SizeCells = 1;
        }

        // Color Hex Values per Grove Design System (Color.md & Tokens.md)
        public string FillHex => Color switch
        {
            NoteColor.Violet => DesignSystem.Colors.NoteVioletHex,
            NoteColor.Clay => DesignSystem.Colors.NoteClayHex,
            NoteColor.SlateBlue => DesignSystem.Colors.NoteSlateBlueHex,
            _ => DesignSystem.Colors.NoteVioletHex
        };

        public string FieldHueHex => IsAnchored ? DesignSystem.Colors.AnchorHex : Color switch
        {
            NoteColor.Violet => DesignSystem.Colors.NoteVioletFieldHex,
            NoteColor.Clay => DesignSystem.Colors.NoteClayFieldHex,
            NoteColor.SlateBlue => DesignSystem.Colors.NoteSlateBlueFieldHex,
            _ => DesignSystem.Colors.NoteVioletFieldHex
        };
    }
}
