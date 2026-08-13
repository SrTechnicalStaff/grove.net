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

    /// <summary>
    /// Represents an authored Note placement on the spatial Grid.
    /// Strictly adheres to cell-quantized square geometry (n x n cells) and authored fill palette (ADR-010).
    /// </summary>
    public class GridNote : GridContentItem
    {
        public override ContentKind Kind => ContentKind.Note;
        public override float Mass => 1.0f;

        public override void ResizeTo(Models.Interaction.SpatialRegion footprint)
        {
            if (!footprint.IsValid || footprint.Width != footprint.Height)
            {
                throw new ArgumentException("A note footprint must be a positive square.", nameof(footprint));
            }

            CellX = footprint.X;
            CellY = footprint.Y;
            SizeCells = footprint.Width;
        }

        public int SizeCells
        {
            get => CellWidth;
            set
            {
                int val = Math.Max(1, value);
                CellWidth = val;
                CellHeight = val;
            }
        }

        public string Text { get; set; } = "";
        public NoteColor Color { get; set; } = NoteColor.Violet;

        public GridNote(int cellX, int cellY, string text = "", NoteColor color = NoteColor.Violet, int layerId = 0)
            : base(cellX, cellY, 1, 1, layerId)
        {
            Text = text;
            Color = color;
            RecalculateFootprint();
        }

        public void RecalculateFootprint()
        {
            // Solve footprint n x n per Note.md & ADR-010 geometry:
            // n=1 box holds up to ~100 chars, n=2 holds up to ~280 chars, n=3 for larger prose.
            int charCount = Text.Length;
            if (charCount > 280)
                SizeCells = 3;
            else if (charCount > 100)
                SizeCells = 2;
            else
                SizeCells = 1;
        }

        // Color Hex Values per Grove Design System (Color.md & Tokens.md & ADR-010)
        public string FillHex => Color switch
        {
            NoteColor.Violet => DesignSystem.Colors.NoteVioletHex,
            NoteColor.Clay => DesignSystem.Colors.NoteClayHex,
            NoteColor.SlateBlue => DesignSystem.Colors.NoteSlateBlueHex,
            _ => DesignSystem.Colors.NoteVioletHex
        };

        public override string FieldHueHex => IsAnchored ? DesignSystem.Colors.AnchorHex : Color switch
        {
            NoteColor.Violet => DesignSystem.Colors.NoteVioletFieldHex,
            NoteColor.Clay => DesignSystem.Colors.NoteClayFieldHex,
            NoteColor.SlateBlue => DesignSystem.Colors.NoteSlateBlueFieldHex,
            _ => DesignSystem.Colors.NoteVioletFieldHex
        };
    }
}
