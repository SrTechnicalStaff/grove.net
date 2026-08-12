using System;

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

        public GridNote(int cellX, int cellY, string text = "", NoteColor color = NoteColor.Violet)
        {
            CellX = cellX;
            CellY = cellY;
            Text = text;
            Color = color;
            RecalculateFootprint();
        }

        public void RecalculateFootprint()
        {
            // Minimum 1x1 cell (220x220px). Expands to 2x2 or 3x3 as text grows.
            int charCount = Text.Length;
            if (charCount > 250)
                SizeCells = 3;
            else if (charCount > 80)
                SizeCells = 2;
            else
                SizeCells = 1;
        }

        // Color Hex Values per Grove Design System (Note.md & Color.md)
        public string FillHex => Color switch
        {
            NoteColor.Violet => "#3D2B56",
            NoteColor.Clay => "#5C3A29",
            NoteColor.SlateBlue => "#273B4A",
            _ => "#3D2B56"
        };

        public string FieldHueHex => Color switch
        {
            NoteColor.Violet => "#6B46C1",
            NoteColor.Clay => "#C05621",
            NoteColor.SlateBlue => "#3182CE",
            _ => "#6B46C1"
        };
    }
}
