using Avalonia.Media;

namespace GroveApp.DesignSystem
{
    /// <summary>
    /// Normative Grove Design System Foundations - Colors (Color.md, Tokens.md).
    /// </summary>
    public static class Colors
    {
        // Core Hex Values
        public const string BaseHex = "#0E0E10";            // --c-base
        public const string GridMinHex = "#161618";         // --c-grid-min
        public const string SurfaceRaisedHex = "#1C1C20";   // --c-surface-raised
        public const string GridMajHex = "#242428";         // --c-grid-maj
        public const string InkHex = "#EAEAEA";             // --c-text
        public const string PaperHex = "#F5F5F5";           // --c-paper
        public const string PaperInkHex = "#1A1A1A";        // --c-paper-ink

        // Grid Lines Hex Values
        public const string GridMajorLineHex = "#242428";  // Major grid line (--c-grid-maj #242428)
        public const string GridMinorLineHex = "#161618";  // Minor grid line (--c-grid-min #161618)
        public const string ContainmentEdgeHex = "#6E6E6A"; // 1px inset containment edge (#6E6E6A)
        public const string HudSlateBorderHex = "#2D2D2A";  // Slate border (#2D2D2A)

        // Signal Hex Values
        public const string SelectHex = "#96B6F8";   // RGB: 150, 182, 248 --signal-interaction / --c-select
        public const string InvalidHex = "#E2625C";  // RGB: 226, 98, 92   --signal-refusal / --c-invalid
        public const string MarqueeHex = "#E8B964";  // RGB: 232, 185, 100 --signal-active-work / --c-marquee
        public const string AnchorHex = "#9E8CEA";   // RGB: 158, 140, 234 --signal-authored-context / --c-anchor

        // Authored Note Colors (Fill & Presence Field)
        public const string NoteVioletHex = "#6E62A6";       // --c-note-violet (RGB: 110, 98, 166)
        public const string NoteVioletFieldHex = "#6E62A6";  // --c-note-violet-field (casts own fill)
        public const string NoteClayHex = "#B0524E";         // --c-note-clay (RGB: 176, 82, 78)
        public const string NoteClayFieldHex = "#B0524E";    // --c-note-clay-field (casts own fill)
        public const string NoteSlateBlueHex = "#4E6E9C";    // --c-note-slate-blue (RGB: 78, 110, 156)
        public const string NoteSlateBlueFieldHex = "#4E6E9C"; // --c-note-slate-blue-field (casts own fill)
        public const string NoteTextHex = "#F4F4F2";         // Text on authored note fill

        // Role Hues & Border Aliases
        public const string ToolFillHex = "#B3A9E0";    // --k-tool
        public const string ToolBorderHex = "#5B5288";  // --k-tool-b
        public const string ViewFillHex = "#9BB6E0";    // --k-view
        public const string ViewBorderHex = "#3F5F8A";  // --k-view-b
        public const string LayerFillHex = "#E2A6C6";   // --k-layer
        public const string LayerBorderHex = "#8A3F63"; // --k-layer-b
        public const string EditFillHex = "#E0A9A3";    // --k-edit
        public const string EditBorderHex = "#7A3F3A";  // --k-edit-b
        public const string SlateFillHex = "#CDB8D8";   // --k-slate
        public const string SlateBorderHex = "#6B5A78"; // --k-slate-b

        // Explicit Role Token Aliases
        public const string KEditBorderHex = EditBorderHex;    // --k-edit-b (#7A3F3A)
        public const string KEditHex = EditFillHex;            // --k-edit (#E0A9A3)
        public const string KSlateBorderHex = HudSlateBorderHex;// --k-slate-b (#2D2D2A)
        public const string CSlateHex = SlateFillHex;          // --c-slate (#CDB8D8)
        public const string CPaperInkHex = PaperInkHex;        // --c-paper-ink (#1A1A1A)

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
        public const string GridMinorInkHex = GridMinorLineHex; // --grid-minor-ink (#161618)
        public const string GridMajorInkHex = GridMajorLineHex; // --grid-major-ink (#242428)

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
        public static Color ContainmentEdge => Color.Parse(ContainmentEdgeHex);

        public static Color SignalInteraction => Color.Parse(SignalInteractionHex);
        public static Color SignalActiveWork => Color.Parse(SignalActiveWorkHex);
        public static Color SignalRefusal => Color.Parse(SignalRefusalHex);
        public static Color SignalAuthoredContext => Color.Parse(SignalAuthoredContextHex);

        public static Color KEditBorder => Color.Parse(KEditBorderHex);
        public static Color KEdit => Color.Parse(KEditHex);
        public static Color KSlateBorder => Color.Parse(KSlateBorderHex);
        public static Color CSlate => Color.Parse(CSlateHex);
        public static Color CPaperInk => Color.Parse(CPaperInkHex);

        public static Color EdgeOnColor => Color.FromArgb((byte)(255 * 0.12), 255, 255, 255); // --edge-on-color (255 255 255 @ 0.12)
        public static Color InkPrimary => Color.FromArgb((byte)(255 * Tokens.InkPrimary), 234, 234, 234); // --ink-primary (0.82)
        public static Color InkSecondary => Color.FromArgb((byte)(255 * Tokens.InkSecondary), 234, 234, 234); // --ink-secondary (0.62)
        public static Color InkTertiary => Color.FromArgb((byte)(255 * Tokens.InkTertiary), 234, 234, 234); // --ink-tertiary (0.51)
        public static Color InkFaint => Color.FromArgb((byte)(255 * Tokens.InkFaint), 234, 234, 234); // --ink-faint (0.30)
        public static Color InkEdge => Color.FromArgb((byte)(255 * Tokens.InkEdge), 234, 234, 234); // --ink-edge (0.16)
        public static Color InkHairline => Color.FromArgb((byte)(255 * Tokens.InkHairline), 234, 234, 234); // --ink-hairline (0.10)
        public static Color InkQuiet => Color.FromArgb((byte)(255 * Tokens.InkQuiet), 234, 234, 234); // --ink-quiet (0.22)
        public static Color EdgeOnPage => Color.FromArgb((byte)(255 * Tokens.PaperEdge), 26, 26, 26); // --edge-on-page

        // Avalonia Brushes
        public static IBrush SurfaceGridBrush => new SolidColorBrush(SurfaceGrid);
        public static IBrush SurfaceChromeBrush => new SolidColorBrush(SurfaceChrome);
        public static IBrush SurfaceNestedBrush => new SolidColorBrush(SurfaceNested);
        public static IBrush SurfacePageBrush => new SolidColorBrush(SurfacePage);
        public static IBrush PaperInkBrush => new SolidColorBrush(Color.Parse(PaperInkHex));

        public static IBrush NoteVioletBrush => new SolidColorBrush(NoteViolet);
        public static IBrush NoteClayBrush => new SolidColorBrush(NoteClay);
        public static IBrush NoteSlateBlueBrush => new SolidColorBrush(NoteSlateBlue);
        public static IBrush NoteTextBrush => new SolidColorBrush(NoteText);
        public static IBrush ContainmentEdgeBrush => new SolidColorBrush(ContainmentEdge);
        public static IBrush HudSlateBorderBrush => new SolidColorBrush(Color.Parse(HudSlateBorderHex));

        public static IBrush SignalInteractionBrush => new SolidColorBrush(SignalInteraction);
        public static IBrush SignalActiveWorkBrush => new SolidColorBrush(SignalActiveWork);
        public static IBrush SignalRefusalBrush => new SolidColorBrush(SignalRefusal);
        public static IBrush SignalAuthoredContextBrush => new SolidColorBrush(SignalAuthoredContext);

        public static IBrush KEditBorderBrush => new SolidColorBrush(KEditBorder);
        public static IBrush KEditBrush => new SolidColorBrush(KEdit);
        public static IBrush KSlateBorderBrush => new SolidColorBrush(KSlateBorder);
        public static IBrush CSlateBrush => new SolidColorBrush(CSlate);
        public static IBrush CPaperInkBrush => new SolidColorBrush(CPaperInk);

        public static IBrush TextPrimaryBrush => new SolidColorBrush(InkPrimary);
        public static IBrush TextSecondaryBrush => new SolidColorBrush(InkSecondary);
        public static IBrush TextMetaBrush => new SolidColorBrush(InkTertiary);
        public static IBrush TextUnavailableBrush => new SolidColorBrush(InkFaint);

        public static IBrush EdgeHairlineBrush => new SolidColorBrush(InkHairline);
        public static IBrush EdgeQuietBrush => new SolidColorBrush(InkEdge);
        public static IBrush EdgeFoundBrush => new SolidColorBrush(InkQuiet);
        public static IBrush EdgeOnColorBrush => new SolidColorBrush(EdgeOnColor);
        public static IBrush EdgeOnPageBrush => new SolidColorBrush(EdgeOnPage);

        public static IBrush GridMinorInkBrush => new SolidColorBrush(Color.Parse(GridMinorInkHex));
        public static IBrush GridMajorInkBrush => new SolidColorBrush(Color.Parse(GridMajorInkHex));
    }
}
