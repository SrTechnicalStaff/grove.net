using Avalonia;

namespace GroveApp.DesignSystem
{
    /// <summary>
    /// Normative Grove Design System Foundations - Tokens (Tokens.md, Space-and-grid.md, Shape.md).
    /// </summary>
    public static class Tokens
    {
        // Grid & Cell Primitives
        public const double GridCell = 220.0;
        public const double CellSize = GridCell;
        public const int GridSubdivisions = 5;
        public const double MinorCellSize = GridCell / GridSubdivisions; // 44.0px
        public const int GridSupercell = 5;
        public const double SupercellPitch = GridCell * GridSupercell; // 1100.0px

        // Grid Fade Parameters
        public const double GridFadeStart = 6.0;
        public const double GridFadeEnd = 14.0;

        // Radii Tokens
        public const double RadiusNone = 0.0; // --r-none: Placements & content on Grid
        public const double RadiusSm = 2.0;   // --r-sm: All Grove chrome
        public const double RadiusRound = 0.5; // Point markers / dots

        public static CornerRadius CornerRadiusNone => new(RadiusNone);
        public static CornerRadius CornerRadiusSm => new(RadiusSm);

        // Stroke Weights
        public const double StrokeHairline = 1.0;
        public const double StrokeContainment = 1.0;
        public const double StrokeState = 2.0;
        public const double StrokeCursorRing = 2.0;
        public const double FieldPerimeterWidth = 1.5;

        // Ink Ramp Alphas (Dark Surfaces)
        public const double InkWhisper = 0.04;
        public const double InkHairline = 0.10;
        public const double InkEdge = 0.16;
        public const double InkQuiet = 0.22;
        public const double InkFaint = 0.30;
        public const double InkTertiary = 0.51;
        public const double InkSecondary = 0.62;
        public const double InkPrimary = 0.82;
        public const double InkFull = 1.0;

        // Paper Ink Ramp Alphas
        public const double PaperTextureMinor = 0.05;
        public const double PaperTextureMajor = 0.08;
        public const double PaperEdge = 0.10;
        public const double PaperRule = 0.16;
        public const double PaperBorder = 0.28;
        public const double PaperLabel = 0.62;
        public const double PaperBody = 0.62;
        public const double PaperStrong = 0.82;

        // Field & Cursor Parameters
        public const double FieldGain = 0.22;
        public const double FieldAlphaMin = 0.025;
        public const double FieldAlphaMax = 0.30;
        public const double FieldPerimeterInk = 0.25;
        public const double FieldPerimeterSelected = 0.80;
        public const double CursorRing = 2.0;
        public const double CursorRingInk = 0.88;
        public const double CursorFillGain = 0.132; // 13.2% cursor fill
        public const double CursorSteady = 0.6;
        public const double CursorTrailDecay = 0.84; // 18-step decay factor per frame
        public const double CursorTrailMin = 0.03;
        public const int CursorTrailMaxSteps = 18;

        // Distance Thresholds (Projected Cell Size in px)
        public const double TierStandinDemote = 18.0;
        public const double TierStandinPromote = 28.0;
        public const double TierDetailDemote = 56.0;
        public const double TierDetailPromote = 72.0;

        // Spacing Scale (Screen Pixels)
        public const double SpaceXs = 4.0;
        public const double SpaceSm = 8.0;
        public const double SpaceMd = 16.0;
        public const double SpaceLg = 24.0;
        public const double SpaceXl = 32.0;

        public static Thickness ThicknessXs => new(SpaceXs);
        public static Thickness ThicknessSm => new(SpaceSm);
        public static Thickness ThicknessMd => new(SpaceMd);
        public static Thickness ThicknessLg => new(SpaceLg);
        public static Thickness ThicknessXl => new(SpaceXl);

        // Focus & Depth
        public const double FocusRingOffset = 2.0;
        public const string ShadowLocal = "0 8px 24px rgb(0 0 0 / 0.40)";
    }
}
