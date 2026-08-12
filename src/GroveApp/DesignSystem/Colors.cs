using Avalonia.Media;

namespace GroveApp.DesignSystem
{
    /// <summary>
    /// Normative Grove Design System Foundations - Colors (Color.md, Tokens.md).
    /// </summary>
    public static class Colors
    {
        // Core Hex Values
        public const string BaseHex = "#0E0E10";
        public const string GridMinHex = "#161618";
        public const string SurfaceRaisedHex = "#1C1C20";
        public const string GridMajHex = "#242428";
        public const string InkHex = "#EAEAEA";
        public const string PaperHex = "#F5F5F5";
        public const string PaperInkHex = "#1A1A1A";

        // Signal Hex Values
        public const string SelectHex = "#96B6F8";   // RGB: 150, 182, 248 --signal-interaction
        public const string InvalidHex = "#E2625C";  // RGB: 226, 98, 92   --signal-refusal
        public const string MarqueeHex = "#E8B964";  // RGB: 232, 185, 100 --signal-active-work
        public const string AnchorHex = "#9E8CEA";   // RGB: 158, 140, 234 --signal-authored-context

        // Authored Note Colors (Fill & Field)
        public const string NoteVioletHex = "#6E62A6";       // --c-note-violet (RGB: 110, 98, 166)
        public const string NoteVioletFieldHex = "#6E62A6";  // --c-note-violet-field (casts own fill)
        public const string NoteClayHex = "#B0524E";         // --c-note-clay (RGB: 176, 82, 78)
        public const string NoteClayFieldHex = "#B0524E";    // --c-note-clay-field (casts own fill)
        public const string NoteSlateBlueHex = "#4E6E9C";    // --c-note-slate-blue (RGB: 78, 110, 156)
        public const string NoteSlateBlueFieldHex = "#4E6E9C"; // --c-note-slate-blue-field (casts own fill)
        public const string NoteTextHex = "#F4F4F2";         // Text on authored note fill

        // Role Hues
        public const string ToolFillHex = "#B3A9E0";
        public const string ToolBorderHex = "#5B5288";
        public const string ViewFillHex = "#9BB6E0";
        public const string ViewBorderHex = "#3F5F8A";
        public const string LayerFillHex = "#E2A6C6";
        public const string LayerBorderHex = "#8A3F63";
        public const string EditFillHex = "#E0A9A3";
        public const string EditBorderHex = "#7A3F3A";
        public const string SlateFillHex = "#CDB8D8";
        public const string SlateBorderHex = "#6B5A78";

        // Semantic Surfaces
        public const string SurfaceGridHex = BaseHex;          // --surface-grid (#0E0E10)
        public const string SurfaceChromeHex = GridMinHex;      // --surface-chrome (#161618)
        public const string SurfaceNestedHex = SurfaceRaisedHex;// --surface-nested (#1C1C20)
        public const string SurfacePageHex = PaperHex;         // --surface-page (#F5F5F5)

        // Semantic Signals
        public const string SignalInteractionHex = SelectHex;
        public const string SignalActiveWorkHex = MarqueeHex;
        public const string SignalRefusalHex = InvalidHex;
        public const string SignalAuthoredContextHex = AnchorHex;

        // Grid Lines
        public const string GridMinorInkHex = GridMinHex; // --grid-minor-ink (#161618)
        public const string GridMajorInkHex = GridMajHex; // --grid-major-ink (#242428)

        // Avalonia Color Structs
        public static Color BaseColor => Color.Parse(BaseHex);
        public static Color SurfaceGrid => Color.Parse(SurfaceGridHex);
        public static Color SurfaceChrome => Color.Parse(SurfaceChromeHex);
        public static Color SurfaceNested => Color.Parse(SurfaceNestedHex);
        public static Color SurfacePage => Color.Parse(SurfacePageHex);

        public static Color NoteViolet => Color.Parse(NoteVioletHex);
        public static Color NoteClay => Color.Parse(NoteClayHex);
        public static Color NoteSlateBlue => Color.Parse(NoteSlateBlueHex);
        public static Color NoteText => Color.Parse(NoteTextHex);

        public static Color SignalInteraction => Color.Parse(SignalInteractionHex);
        public static Color SignalActiveWork => Color.Parse(SignalActiveWorkHex);
        public static Color SignalRefusal => Color.Parse(SignalRefusalHex);
        public static Color SignalAuthoredContext => Color.Parse(SignalAuthoredContextHex);

        public static Color EdgeOnColor => Color.FromArgb((byte)(255 * 0.12), 255, 255, 255); // --edge-on-color
        public static Color InkPrimary => Color.FromArgb((byte)(255 * Tokens.InkPrimary), 234, 234, 234); // --ink-primary
        public static Color InkSecondary => Color.FromArgb((byte)(255 * Tokens.InkSecondary), 234, 234, 234); // --ink-secondary
        public static Color InkTertiary => Color.FromArgb((byte)(255 * Tokens.InkTertiary), 234, 234, 234); // --ink-tertiary
        public static Color InkFaint => Color.FromArgb((byte)(255 * Tokens.InkFaint), 234, 234, 234); // --ink-faint
        public static Color InkEdge => Color.FromArgb((byte)(255 * Tokens.InkEdge), 234, 234, 234); // --ink-edge
        public static Color InkHairline => Color.FromArgb((byte)(255 * Tokens.InkHairline), 234, 234, 234); // --ink-hairline
        public static Color InkQuiet => Color.FromArgb((byte)(255 * Tokens.InkQuiet), 234, 234, 234); // --ink-quiet

        // Avalonia Brushes
        public static IBrush SurfaceGridBrush => new SolidColorBrush(SurfaceGrid);
        public static IBrush SurfaceChromeBrush => new SolidColorBrush(SurfaceChrome);
        public static IBrush SurfaceNestedBrush => new SolidColorBrush(SurfaceNested);
        public static IBrush SurfacePageBrush => new SolidColorBrush(SurfacePage);

        public static IBrush NoteVioletBrush => new SolidColorBrush(NoteViolet);
        public static IBrush NoteClayBrush => new SolidColorBrush(NoteClay);
        public static IBrush NoteSlateBlueBrush => new SolidColorBrush(NoteSlateBlue);
        public static IBrush NoteTextBrush => new SolidColorBrush(NoteText);

        public static IBrush SignalInteractionBrush => new SolidColorBrush(SignalInteraction);
        public static IBrush SignalActiveWorkBrush => new SolidColorBrush(SignalActiveWork);
        public static IBrush SignalRefusalBrush => new SolidColorBrush(SignalRefusal);
        public static IBrush SignalAuthoredContextBrush => new SolidColorBrush(SignalAuthoredContext);

        public static IBrush TextPrimaryBrush => new SolidColorBrush(InkPrimary);
        public static IBrush TextSecondaryBrush => new SolidColorBrush(InkSecondary);
        public static IBrush TextMetaBrush => new SolidColorBrush(InkTertiary);
        public static IBrush TextUnavailableBrush => new SolidColorBrush(InkFaint);

        public static IBrush EdgeHairlineBrush => new SolidColorBrush(InkHairline);
        public static IBrush EdgeQuietBrush => new SolidColorBrush(InkEdge);
        public static IBrush EdgeFoundBrush => new SolidColorBrush(InkQuiet);
        public static IBrush EdgeOnColorBrush => new SolidColorBrush(EdgeOnColor);

        public static IBrush GridMinorInkBrush => new SolidColorBrush(Color.Parse(GridMinorInkHex));
        public static IBrush GridMajorInkBrush => new SolidColorBrush(Color.Parse(GridMajorInkHex));
    }
}
