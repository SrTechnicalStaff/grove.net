using System;

using Avalonia;
using Avalonia.Media;
using GroveApp.DesignSystem;
using Colors = GroveApp.DesignSystem.Colors;

namespace GroveApp.Engine
{
    public readonly record struct SpatialGridGeometryConfig(
        double CellSize,
        int Subdivisions,
        int SupercellMultiplier)
    {
        public static SpatialGridGeometryConfig Default => new(
            Tokens.GridCell,
            Tokens.GridSubdivisions,
            Tokens.GridSupercell);

        public double MinorCellSize => CellSize / Math.Max(1, Subdivisions);
        public double SupercellPitch => CellSize * Math.Max(1, SupercellMultiplier);
    }

    /// <summary>
    /// Engine module for rendering major grid lines (#242428), minor subdivisions (#161618),
    /// supercells, and cell grid pitch (220px) with seamless LOD distance fading.
    /// </summary>
    public class GridLineModule
    {
        public void RenderGridLines(
            DrawingContext context,
            Func<Point, Point> worldToScreen,
            double cellSize,
            double zoom,
            int minCellX,
            int maxCellX,
            int minCellY,
            int maxCellY,
            SpatialGridGeometryConfig? geometry = null,
            double renderScaling = 1.0)
        {
            SpatialGridGeometryConfig grid = geometry ?? SpatialGridGeometryConfig.Default with { CellSize = cellSize };
            double minorSpacing = grid.MinorCellSize * zoom;
            double majorSpacing = grid.CellSize * zoom;
            double superSpacing = grid.SupercellPitch * zoom;

            // Compute distance ink alpha multipliers: clamp((spacing - 6) / 8, 0, 1) per Tokens.md
            double minorAlpha = Math.Clamp((minorSpacing - Tokens.GridFadeStart) / (Tokens.GridFadeEnd - Tokens.GridFadeStart), 0.0, 1.0);
            double majorAlpha = Math.Clamp((majorSpacing - Tokens.GridFadeStart) / (Tokens.GridFadeEnd - Tokens.GridFadeStart), 0.0, 1.0);
            double superAlpha = Math.Clamp((superSpacing - Tokens.GridFadeStart) / (Tokens.GridFadeEnd - Tokens.GridFadeStart), 0.0, 1.0);

            // Early exit if all grid line tiers are below fade threshold
            if (minorAlpha <= 0.001 && majorAlpha <= 0.001 && superAlpha <= 0.001) return;

            Point screenTopLeft = worldToScreen(new Point(minCellX * cellSize, minCellY * cellSize));
            Point screenBottomRight = worldToScreen(new Point((maxCellX + 1) * cellSize, (maxCellY + 1) * cellSize));

            // 1. Minor subdivisions (44px pitch, #161618) - Active when minorSpacing > 6px
            if (minorAlpha > 0.001)
            {
                Color minorBase = Color.Parse(Colors.GridMinorLineHex);
                Color minorClr = Color.FromArgb((byte)(255 * minorAlpha), minorBase.R, minorBase.G, minorBase.B);
                var minorPen = new Pen(new SolidColorBrush(minorClr), Tokens.StrokeHairline);

                int minMinorX = minCellX * grid.Subdivisions;
                int maxMinorX = maxCellX * grid.Subdivisions;
                int minMinorY = minCellY * grid.Subdivisions;
                int maxMinorY = maxCellY * grid.Subdivisions;

                for (int mx = minMinorX; mx <= maxMinorX; mx++)
                {
                    if (mx % grid.Subdivisions == 0) continue; // Skip major line positions
                    double wx = mx * grid.MinorCellSize;
                    double sx = Snap(worldToScreen(new Point(wx, 0)).X, renderScaling);
                    context.DrawLine(minorPen, new Point(sx, screenTopLeft.Y), new Point(sx, screenBottomRight.Y));
                }

                for (int my = minMinorY; my <= maxMinorY; my++)
                {
                    if (my % grid.Subdivisions == 0) continue; // Skip major line positions
                    double wy = my * grid.MinorCellSize;
                    double sy = Snap(worldToScreen(new Point(0, wy)).Y, renderScaling);
                    context.DrawLine(minorPen, new Point(screenTopLeft.X, sy), new Point(screenBottomRight.X, sy));
                }
            }

            // 2. Major grid lines (220px pitch, #242428) - Active when majorSpacing > 6px
            if (majorAlpha > 0.001)
            {
                Color majorBase = Color.Parse(Colors.GridMajorLineHex);
                Color majorClr = Color.FromArgb((byte)(255 * majorAlpha), majorBase.R, majorBase.G, majorBase.B);
                var majorPen = new Pen(new SolidColorBrush(majorClr), Tokens.StrokeHairline);

                for (int cx = minCellX; cx <= maxCellX; cx++)
                {
                    if (cx % Tokens.GridSupercell == 0) continue; // Skip supercell positions
                    double wx = cx * grid.CellSize;
                    double sx = Snap(worldToScreen(new Point(wx, 0)).X, renderScaling);
                    context.DrawLine(majorPen, new Point(sx, screenTopLeft.Y), new Point(sx, screenBottomRight.Y));
                }

                for (int cy = minCellY; cy <= maxCellY; cy++)
                {
                    if (cy % Tokens.GridSupercell == 0) continue; // Skip supercell positions
                    double wy = cy * grid.CellSize;
                    double sy = Snap(worldToScreen(new Point(0, wy)).Y, renderScaling);
                    context.DrawLine(majorPen, new Point(screenTopLeft.X, sy), new Point(screenBottomRight.X, sy));
                }
            }

            // 3. Supercell Grid Pitch (1100px pitch, #242428) - Active when superSpacing > 6px
            if (superAlpha > 0.001)
            {
                Color superBase = Color.Parse(Colors.GridMajorInkHex);
                Color superClr = Color.FromArgb((byte)(255 * superAlpha), superBase.R, superBase.G, superBase.B);
                var superPen = new Pen(new SolidColorBrush(superClr), Tokens.StrokeHairline);

                int minSuperX = (int)Math.Floor((double)minCellX / grid.SupercellMultiplier);
                int maxSuperX = (int)Math.Ceiling((double)maxCellX / grid.SupercellMultiplier);
                int minSuperY = (int)Math.Floor((double)minCellY / grid.SupercellMultiplier);
                int maxSuperY = (int)Math.Ceiling((double)maxCellY / grid.SupercellMultiplier);

                for (int sx = minSuperX; sx <= maxSuperX; sx++)
                {
                    double wx = sx * grid.SupercellPitch;
                    double px = Snap(worldToScreen(new Point(wx, 0)).X, renderScaling);
                    context.DrawLine(superPen, new Point(px, screenTopLeft.Y), new Point(px, screenBottomRight.Y));
                }

                for (int sy = minSuperY; sy <= maxSuperY; sy++)
                {
                    double wy = sy * grid.SupercellPitch;
                    double py = Snap(worldToScreen(new Point(0, wy)).Y, renderScaling);
                    context.DrawLine(superPen, new Point(screenTopLeft.X, py), new Point(screenBottomRight.X, py));
                }
            }
        }

        private static double Snap(double value, double renderScaling)
        {
            double dpi = Math.Max(0.01, renderScaling);
            return Math.Floor(value * dpi) / dpi;
        }
    }
}
