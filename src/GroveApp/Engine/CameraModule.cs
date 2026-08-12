using System;
using Avalonia;
using GroveApp.DesignSystem;

namespace GroveApp.Engine
{
    /// <summary>
    /// Decoupled camera module managing 2D affine transformations T(x, y, s).
    /// Deep implementation hiding 2D affine matrix math with zero knowledge of content, grid lines, text, or overlays.
    /// </summary>
    public class CameraModule
    {
        public double CameraX { get; set; } = 100.0;
        public double CameraY { get; set; } = 100.0;
        public double Zoom { get; set; } = 1.0;

        /// <summary>
        /// Transforms a 2D world coordinate into 2D screen coordinate space:
        /// P_screen = P_world * Zoom + CameraOffset
        /// </summary>
        public Point WorldToScreen(Point worldPt)
        {
            return new Point(
                worldPt.X * Zoom + CameraX,
                worldPt.Y * Zoom + CameraY
            );
        }

        /// <summary>
        /// Transforms a 2D screen coordinate back into 2D world coordinate space:
        /// P_world = (P_screen - CameraOffset) / Zoom
        /// </summary>
        public Point ScreenToWorld(Point screenPt)
        {
            return new Point(
                (screenPt.X - CameraX) / Zoom,
                (screenPt.Y - CameraY) / Zoom
            );
        }

        /// <summary>
        /// Maps a 2D world coordinate to its corresponding cell index (cellX, cellY).
        /// </summary>
        public (int cellX, int cellY) WorldToCell(Point worldPt, double cellSize = Tokens.GridCell)
        {
            int cx = (int)Math.Floor(worldPt.X / cellSize);
            int cy = (int)Math.Floor(worldPt.Y / cellSize);
            return (cx, cy);
        }

        /// <summary>
        /// Maps a cell index (cellX, cellY) to its top-left 2D world position.
        /// </summary>
        public Point CellToWorld(int cellX, int cellY, double cellSize = Tokens.GridCell)
        {
            return new Point(cellX * cellSize, cellY * cellSize);
        }

        /// <summary>
        /// Translates the 2D camera viewport by the specified delta vector.
        /// </summary>
        public void Pan(Vector delta)
        {
            CameraX += delta.X;
            CameraY += delta.Y;
        }

        /// <summary>
        /// Translates the 2D camera viewport by scalar x and y deltas.
        /// </summary>
        public void Pan(double deltaX, double deltaY)
        {
            CameraX += deltaX;
            CameraY += deltaY;
        }

        /// <summary>
        /// Zooms the camera relative to a fixed 2D screen origin point (e.g. cursor position).
        /// </summary>
        public void ZoomAt(Point originScreen, double zoomFactor, double minZoom = 0.2, double maxZoom = 3.5)
        {
            double newZoom = Math.Clamp(Zoom * zoomFactor, minZoom, maxZoom);
            if (Math.Abs(newZoom - Zoom) < 0.000001) return;

            CameraX = originScreen.X - (originScreen.X - CameraX) * (newZoom / Zoom);
            CameraY = originScreen.Y - (originScreen.Y - CameraY) * (newZoom / Zoom);
            Zoom = newZoom;
        }

        /// <summary>
        /// Obtains the 2D affine transformation matrix T(x, y, s).
        /// </summary>
        public Matrix GetTransformMatrix()
        {
            return Matrix.CreateScale(Zoom, Zoom) * Matrix.CreateTranslation(CameraX, CameraY);
        }
    }
}
