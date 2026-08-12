using System;

using Avalonia;
using Avalonia.Media;
using GroveApp.DesignSystem;
using Colors = GroveApp.DesignSystem.Colors;

namespace GroveApp.Engine
{
    /// <summary>
    /// Engine module for rendering major grid lines (#1F1F22), minor subdivisions (#151517),
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
            int maxCellY)
        {
            double minorSpacing = Tokens.MinorCellSize * zoom;
            double majorSpacing = cellSize * zoom;
            double superSpacing = Tokens.SupercellPitch * zoom;

            // Compute distance ink alpha multipliers: clamp((spacing - 6) / 8, 0, 1) per Tokens.md
            double minorAlpha = Math.Clamp((minorSpacing - Tokens.GridFadeStart) / (Tokens.GridFadeEnd - Tokens.GridFadeStart), 0.0, 1.0);
            double majorAlpha = Math.Clamp((majorSpacing - Tokens.GridFadeStart) / (Tokens.GridFadeEnd - Tokens.GridFadeStart), 0.0, 1.0);
            double superAlpha = Math.Clamp((superSpacing - Tokens.GridFadeStart) / (Tokens.GridFadeEnd - Tokens.GridFadeStart), 0.0, 1.0);

            // Early exit if all grid line tiers are below fade threshold
            if (minorAlpha <= 0.001 && majorAlpha <= 0.001 && superAlpha <= 0.001) return;

            Point screenTopLeft = worldToScreen(new Point(minCellX * cellSize, minCellY * cellSize));
            Point screenBottomRight = worldToScreen(new Point((maxCellX + 1) * cellSize, (maxCellY + 1) * cellSize));

            // 1. Minor Subdivisions (44px pitch, #151517) - Active when minorSpacing > 6px
            if (minorAlpha > 0.001)
            {
                Color minorBase = Color.Parse(Colors.GridMinorLineHex);
                Color minorClr = Color.FromArgb((byte)(255 * minorAlpha), minorBase.R, minorBase.G, minorBase.B);
                var minorPen = new Pen(new SolidColorBrush(minorClr), Tokens.StrokeHairline);

                int minMinorX = minCellX * Tokens.GridSubdivisions;
                int maxMinorX = maxCellX * Tokens.GridSubdivisions;
                int minMinorY = minCellY * Tokens.GridSubdivisions;
                int maxMinorY = maxCellY * Tokens.GridSubdivisions;

                for (int mx = minMinorX; mx <= maxMinorX; mx++)
                {
                    if (mx % Tokens.GridSubdivisions == 0) continue; // Skip major line positions
                    double wx = mx * Tokens.MinorCellSize;
                    double sx = worldToScreen(new Point(wx, 0)).X;
                    context.DrawLine(minorPen, new Point(sx, screenTopLeft.Y), new Point(sx, screenBottomRight.Y));
                }

                for (int my = minMinorY; my <= maxMinorY; my++)
                {
                    if (my % Tokens.GridSubdivisions == 0) continue; // Skip major line positions
                    double wy = my * Tokens.MinorCellSize;
                    double sy = worldToScreen(new Point(0, wy)).Y;
                    context.DrawLine(minorPen, new Point(screenTopLeft.X, sy), new Point(screenBottomRight.X, sy));
                }
            }

            // 2. Major Grid Lines (220px pitch, #1F1F22) - Active when majorSpacing > 6px
            if (majorAlpha > 0.001)
            {
                Color majorBase = Color.Parse(Colors.GridMajorLineHex);
                Color majorClr = Color.FromArgb((byte)(255 * majorAlpha), majorBase.R, majorBase.G, majorBase.B);
                var majorPen = new Pen(new SolidColorBrush(majorClr), Tokens.StrokeHairline);

                for (int cx = minCellX; cx <= maxCellX; cx++)
                {
                    if (cx % Tokens.GridSupercell == 0) continue; // Skip supercell positions
                    double wx = cx * cellSize;
                    double sx = worldToScreen(new Point(wx, 0)).X;
                    context.DrawLine(majorPen, new Point(sx, screenTopLeft.Y), new Point(sx, screenBottomRight.Y));
                }

                for (int cy = minCellY; cy <= maxCellY; cy++)
                {
                    if (cy % Tokens.GridSupercell == 0) continue; // Skip supercell positions
                    double wy = cy * cellSize;
                    double sy = worldToScreen(new Point(0, wy)).Y;
                    context.DrawLine(majorPen, new Point(screenTopLeft.X, sy), new Point(screenBottomRight.X, sy));
                }
            }

            // 3. Supercell Grid Pitch (1100px pitch, #242428) - Active when superSpacing > 6px
            if (superAlpha > 0.001)
            {
                Color superBase = Color.Parse(Colors.GridMajorInkHex);
                Color superClr = Color.FromArgb((byte)(255 * superAlpha), superBase.R, superBase.G, superBase.B);
                var superPen = new Pen(new SolidColorBrush(superClr), Tokens.StrokeHairline);

                int minSuperX = (int)Math.Floor((double)minCellX / Tokens.GridSupercell);
                int maxSuperX = (int)Math.Ceiling((double)maxCellX / Tokens.GridSupercell);
                int minSuperY = (int)Math.Floor((double)minCellY / Tokens.GridSupercell);
                int maxSuperY = (int)Math.Ceiling((double)maxCellY / Tokens.GridSupercell);

                for (int sx = minSuperX; sx <= maxSuperX; sx++)
                {
                    double wx = sx * Tokens.SupercellPitch;
                    double px = worldToScreen(new Point(wx, 0)).X;
                    context.DrawLine(superPen, new Point(px, screenTopLeft.Y), new Point(px, screenBottomRight.Y));
                }

                for (int sy = minSuperY; sy <= maxSuperY; sy++)
                {
                    double wy = sy * Tokens.SupercellPitch;
                    double py = worldToScreen(new Point(0, wy)).Y;
                    context.DrawLine(superPen, new Point(screenTopLeft.X, py), new Point(screenBottomRight.X, py));
                }
            }
        }
    }
}
