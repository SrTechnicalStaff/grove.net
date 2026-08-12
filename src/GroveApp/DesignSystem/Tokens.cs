using Avalonia;

namespace GroveApp.DesignSystem
{
    /// <summary>
    /// Normative Grove Design System Foundations - Tokens (Tokens.md, Space-and-grid.md, Shape.md).
    /// </summary>
    public static class Tokens
    {
        // Grid & Cell Primitives
        public const double GridCell = 220.0; // --grid-cell
        public const double CellSize = GridCell; // --cell-size
        public const int GridSubdivisions = 5; // --grid-subdivisions
        public const double MinorCellSize = GridCell / GridSubdivisions; // 44.0px
        public const int GridSupercell = 5; // --grid-supercell
        public const double SupercellPitch = GridCell * GridSupercell; // 1100.0px

        // Grid Fade Parameters
        public const double GridFadeStart = 6.0; // --grid-fade-start
        public const double GridFadeEnd = 14.0; // --grid-fade-end

        // Radii Tokens
        public const double RadiusNone = 0.0; // --r-none: Placements & content on Grid
        public const double RadiusSm = 2.0;   // --r-sm: All Grove chrome
        public const double RadiusRound = 0.5; // --r-round: Point markers / dots

        public static CornerRadius CornerRadiusNone => new(RadiusNone);
        public static CornerRadius CornerRadiusSm => new(RadiusSm);

        // Stroke Weights
        public const double StrokeHairline = 1.0;
        public const double StrokeContainment = 1.0;
        public const double StrokeState = 2.0;
        public const double StrokeCursorRing = 2.0;
        public const double FieldPerimeterWidth = 1.5; // --field-perimeter-width

        // Ink Ramp Alphas (Dark Surfaces)
        public const double InkWhisper = 0.04;   // --ink-whisper
        public const double InkHairline = 0.10;  // --ink-hairline
        public const double InkEdge = 0.16;      // --ink-edge
        public const double InkQuiet = 0.22;     // --ink-quiet
        public const double InkFaint = 0.30;     // --ink-faint
        public const double InkTertiary = 0.51;  // --ink-tertiary
        public const double InkSecondary = 0.62; // --ink-secondary
        public const double InkPrimary = 0.82;   // --ink-primary
        public const double InkFull = 1.0;       // --ink-full

        // Paper Ink Ramp Alphas
        public const double PaperTextureMinor = 0.05; // --paper-texture-minor
        public const double PaperTextureMajor = 0.08; // --paper-texture-major
        public const double PaperEdge = 0.10;         // --paper-edge
        public const double PaperRule = 0.16;         // --paper-rule
        public const double PaperBorder = 0.28;       // --paper-border
        public const double PaperLabel = 0.62;        // --paper-label
        public const double PaperBody = 0.62;         // --paper-body
        public const double PaperStrong = 0.82;       // --paper-strong

        // Field & Cursor Parameters
        public const double FieldGain = 0.22;              // --field-gain
        public const double FieldAlphaMin = 0.025;         // --field-alpha-min
        public const double FieldAlphaMax = 0.30;          // --field-alpha-max
        public const double FieldPerimeterInk = 0.25;      // --field-perimeter-ink
        public const double FieldPerimeterSelected = 0.80; // --field-perimeter-selected
        public const double CursorRing = 2.0;              // --cursor-ring
        public const double CursorRingInk = 0.88;          // --cursor-ring-ink
        public const double CursorFillGain = 0.22;         // --cursor-fill-gain
        public const double CursorSteady = 0.6;            // --cursor-steady
        public const double CursorTrailDecay = 0.84;       // --cursor-trail-decay
        public const double CursorTrailMin = 0.03;         // --cursor-trail-min
        public const int CursorTrailMaxSteps = 18;

        // Distance Thresholds (Projected Cell Size in px)
        public const double TierStandinDemote = 18.0;  // --tier-standin-demote
        public const double TierStandinPromote = 28.0; // --tier-standin-promote
        public const double TierDetailDemote = 56.0;   // --tier-detail-demote
        public const double TierDetailPromote = 72.0;  // --tier-detail-promote

        // Spacing Scale (Screen Pixels)
        public const double SpaceXs = 4.0;  // --sp-xs
        public const double SpaceSm = 8.0;  // --sp-sm
        public const double SpaceMd = 16.0; // --sp-md
        public const double SpaceLg = 24.0; // --sp-lg
        public const double SpaceXl = 32.0; // --sp-xl

        public static Thickness ThicknessXs => new(SpaceXs);
        public static Thickness ThicknessSm => new(SpaceSm);
        public static Thickness ThicknessMd => new(SpaceMd);
        public static Thickness ThicknessLg => new(SpaceLg);
        public static Thickness ThicknessXl => new(SpaceXl);

        // Focus & Depth
        public const double FocusRingOffset = 2.0;
        public const string ShadowLocal = "0 8px 24px rgb(0 0 0 / 0.40)"; // --shadow-local
    }
}
