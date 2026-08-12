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

            // Compute distance ink alpha multipliers: clamp((spacing - 6) / 8, 0, 1)
            double minorAlpha = Math.Clamp((minorSpacing - Tokens.GridFadeStart) / (Tokens.GridFadeEnd - Tokens.GridFadeStart), 0.0, 1.0);
            double majorAlpha = Math.Clamp((majorSpacing - Tokens.GridFadeStart) / (Tokens.GridFadeEnd - Tokens.GridFadeStart), 0.0, 1.0);
            double superAlpha = Math.Clamp((superSpacing - Tokens.GridFadeStart) / (Tokens.GridFadeEnd - Tokens.GridFadeStart), 0.0, 1.0);

            // 1. Minor Subdivisions (44px pitch, #151517)
            if (minorAlpha > 0.01)
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
                    Point p1 = worldToScreen(new Point(wx, minCellY * cellSize));
                    Point p2 = worldToScreen(new Point(wx, maxCellY * cellSize));
                    context.DrawLine(minorPen, p1, p2);
                }

                for (int my = minMinorY; my <= maxMinorY; my++)
                {
                    if (my % Tokens.GridSubdivisions == 0) continue; // Skip major line positions
                    double wy = my * Tokens.MinorCellSize;
                    Point p1 = worldToScreen(new Point(minCellX * cellSize, wy));
                    Point p2 = worldToScreen(new Point(maxCellX * cellSize, wy));
                    context.DrawLine(minorPen, p1, p2);
                }
            }

            // 2. Major Grid Lines (220px pitch, #1F1F22)
            if (majorAlpha > 0.01)
            {
                Color majorBase = Color.Parse(Colors.GridMajorLineHex);
                Color majorClr = Color.FromArgb((byte)(255 * majorAlpha), majorBase.R, majorBase.G, majorBase.B);
                var majorPen = new Pen(new SolidColorBrush(majorClr), Tokens.StrokeHairline);

                for (int cx = minCellX; cx <= maxCellX; cx++)
                {
                    if (cx % Tokens.GridSupercell == 0) continue; // Skip supercell positions
                    double wx = cx * cellSize;
                    Point p1 = worldToScreen(new Point(wx, minCellY * cellSize));
                    Point p2 = worldToScreen(new Point(wx, maxCellX * cellSize));
                    context.DrawLine(majorPen, p1, p2);
                }

                for (int cy = minCellY; cy <= maxCellY; cy++)
                {
                    if (cy % Tokens.GridSupercell == 0) continue; // Skip supercell positions
                    double wy = cy * cellSize;
                    Point p1 = worldToScreen(new Point(minCellX * cellSize, wy));
                    Point p2 = worldToScreen(new Point(maxCellX * cellSize, wy));
                    context.DrawLine(majorPen, p1, p2);
                }
            }

            // 3. Supercell Grid Pitch (1100px pitch, #242428)
            if (superAlpha > 0.01)
            {
                Color superBase = Color.Parse(Colors.GridMajHex);
                Color superClr = Color.FromArgb((byte)(255 * superAlpha), superBase.R, superBase.G, superBase.B);
                var superPen = new Pen(new SolidColorBrush(superClr), 1.5);

                int minSuperX = (int)Math.Floor((double)minCellX / Tokens.GridSupercell);
                int maxSuperX = (int)Math.Ceiling((double)maxCellX / Tokens.GridSupercell);
                int minSuperY = (int)Math.Floor((double)minCellY / Tokens.GridSupercell);
                int maxSuperY = (int)Math.Ceiling((double)maxCellY / Tokens.GridSupercell);

                for (int sx = minSuperX; sx <= maxSuperX; sx++)
                {
                    double wx = sx * Tokens.SupercellPitch;
                    Point p1 = worldToScreen(new Point(wx, minCellY * cellSize));
                    Point p2 = worldToScreen(new Point(wx, maxCellY * cellSize));
                    context.DrawLine(superPen, p1, p2);
                }

                for (int sy = minSuperY; sy <= maxSuperY; sy++)
                {
                    double wy = sy * Tokens.SupercellPitch;
                    Point p1 = worldToScreen(new Point(minCellX * cellSize, wy));
                    Point p2 = worldToScreen(new Point(maxCellX * cellSize, wy));
                    context.DrawLine(superPen, p1, p2);
                }
            }
        }
    }
}
