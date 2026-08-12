using System;

namespace GroveApp.Engine
{
    public enum AspectClass : byte
    {
        ExtremePanorama = 0, // r >= 2.0
        Landscape = 1,       // 1.0 < r < 2.0
        Square = 2,          // r == 1.0
        Portrait = 3         // r < 1.0
    }

    public readonly record struct ImageFootprint(int CellsW, int CellsH, AspectClass Aspect)
    {
        public int TotalCells => CellsW * CellsH;
        public double AspectRatio => (double)CellsW / CellsH;
    }

    /// <summary>
    /// Implements ADR-012 Image Footprint Resolution Mapping.
    /// Maps intrinsic image dimensions (W x H) to discrete grid cell footprint (Nw x Nh)
    /// using the 256px long-axis cell divisor formula:
    ///   long = max(W, H), short = min(W, H)
    ///   L = max(1, ceil(long / 256))
    ///   S = max(1, round(L * short / long))
    ///   Nw = W >= H ? L : S
    ///   Nh = W >= H ? S : L
    /// </summary>
    public static class ImageFootprintResolver
    {
        public const double CellDivisor = 256.0;
        public const double CellPitchPx = 220.0;
        public const double LogicalDpi = 96.0;

        public static ImageFootprint Resolve(int widthPx, int heightPx)
        {
            if (widthPx <= 0 || heightPx <= 0)
            {
                return new ImageFootprint(1, 1, AspectClass.Square);
            }

            double r = (double)widthPx / heightPx;
            AspectClass aspectClass = r switch
            {
                >= 2.0 => AspectClass.ExtremePanorama,
                > 1.0 => AspectClass.Landscape,
                1.0 => AspectClass.Square,
                _ => AspectClass.Portrait
            };

            int longPx = Math.Max(widthPx, heightPx);
            int shortPx = Math.Min(widthPx, heightPx);

            int L = Math.Max(1, (int)Math.Ceiling(longPx / CellDivisor));
            int S = Math.Max(1, (int)Math.Round(L * ((double)shortPx / longPx)));

            int cellsW = widthPx >= heightPx ? L : S;
            int cellsH = widthPx >= heightPx ? S : L;

            return new ImageFootprint(cellsW, cellsH, aspectClass);
        }

        public static double CalculateEffectivePpi(int widthPx, int heightPx, int cellsW, int cellsH)
        {
            double placedWidthInches = (cellsW * CellPitchPx) / LogicalDpi;
            double placedHeightInches = (cellsH * CellPitchPx) / LogicalDpi;

            if (placedWidthInches <= 0 || placedHeightInches <= 0) return LogicalDpi;

            double ppiX = widthPx / placedWidthInches;
            double ppiY = heightPx / placedHeightInches;

            return Math.Min(ppiX, ppiY);
        }
    }
}
