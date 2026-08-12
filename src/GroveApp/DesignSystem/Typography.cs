using Avalonia.Media;

namespace GroveApp.DesignSystem
{
    /// <summary>
    /// Normative Grove Design System Foundations - Typography (Typography.md, Tokens.md).
    /// </summary>
    public static class Typography
    {
        // Font Family Constants
        public const string FontFamilyDisplay = "Oswald, sans-serif";
        public const string FontFamilyUi = "Inter, Segoe UI, sans-serif";
        public const string FontFamilyMono = "JetBrains Mono, Consolas, monospace";

        public static FontFamily DisplayFamily => new(FontFamilyDisplay);
        public static FontFamily UiFamily => new(FontFamilyUi);
        public static FontFamily MonoFamily => new(FontFamilyMono);

        // Font Sizes (Pixels)
        public const double SizeMicro = 9.0;         // --t-micro: Mono badge on frame
        public const double SizeLabel = 11.0;        // --t-label: Mono label, front matter
        public const double SizeCaption = 12.0;      // --t-caption: Secondary UI
        public const double SizeDense = 13.0;        // --t-dense: Dense UI
        public const double SizeBody = 15.0;         // --t-body: Reading default, Note text
        public const double SizeLead = 17.0;         // --t-lead: Lead paragraph
        public const double SizeTitleSmall = 20.0;   // --t-title-small: Surface titles
        public const double SizeTitle = 30.0;        // --t-title: Reading title
        public const double SizeDisplay = 38.0;      // --t-display: Placed Document title
        public const double SizeHero = SizeDisplay;  // Retained alias for hero display size

        // Font Weights
        public static FontWeight WeightNormal => FontWeight.Normal;   // 400 (UI default)
        public static FontWeight WeightMedium => FontWeight.Medium;   // 500 (Display, Mono, Note text)
        public static FontWeight WeightSemiBold => FontWeight.SemiBold; // 600
        public static FontWeight WeightBold => FontWeight.Bold;       // 700

        public static FontWeight WeightDisplay => WeightMedium; // 500
        public static FontWeight WeightUiDefault => WeightNormal; // 400
        public static FontWeight WeightNoteText => WeightMedium; // 500 per Typography.md
        public static FontWeight WeightMono => WeightMedium; // 500

        // Line Heights (Unitless Multipliers)
        public const double LineHeightTight = 0.98;   // --lh-tight (Display titles)
        public const double LineHeightSnug = 1.42;    // --lh-snug (Authored Note text)
        public const double LineHeightUi = 1.50;      // --lh-ui (Interface text, menus)
        public const double LineHeightReading = 1.65; // --lh-reading (Continuous prose)

        // Tracking / Letter Spacing (em)
        public const double TrackingDisplay = 0.02; // --tr-display
        public const double TrackingTitle = 0.03;   // --tr-title
        public const double TrackingMono = 0.08;    // --tr-mono
        public const double TrackingLabel = 0.12;   // --tr-label
        public const double TrackingWide = 0.16;    // --tr-wide
        public const double TrackingCaps = 0.20;    // --tr-caps

        // Measure (Characters)
        public const int MeasureReading = 34;       // --measure-reading (34ch)
    }
}
