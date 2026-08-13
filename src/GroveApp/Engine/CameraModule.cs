using System;
using Avalonia;
using GroveApp.DesignSystem;

namespace GroveApp.Engine
{
    /// <summary>
    /// Decoupled camera module managing 2D affine transformations T(x, y, s).
    /// Deep implementation hiding 2D affine matrix math with zero knowledge of content, grid lines, text, or overlays.
    /// </summary>
    public readonly record struct CameraState(Point Translation, double Scale, Matrix TransformMatrix, Matrix InverseMatrix)
    {
        public static CameraState Create(Point translation, double scale)
        {
            double boundedScale = Math.Clamp(scale, CameraModule.MinZoom, CameraModule.MaxZoom);
            Matrix transform = Matrix.CreateScale(boundedScale, boundedScale) * Matrix.CreateTranslation(translation.X, translation.Y);
            return new CameraState(translation, boundedScale, transform, transform.Invert());
        }
    }

    public interface ICameraEngine
    {
        CameraState CurrentState { get; }
        Matrix TransformMatrix { get; }
        Rect GetVisibleWorldBounds(Size viewport);
        void PanBy(Vector deltaScreen);
        void ZoomAt(Point cursorScreen, double zoomDeltaFactor);
        Point WorldToScreen(Point worldPoint);
        Point ScreenToWorld(Point screenPoint);
    }

    public class CameraModule : ICameraEngine
    {
        public const double MinZoom = 0.01;
        public const double MaxZoom = 10.0;

        private Point _translation = new(100.0, 100.0);
        private double _zoom = 1.0;

        public double CameraX => _translation.X;
        public double CameraY => _translation.Y;
        public double Zoom => _zoom;
        public CameraState CurrentState => CameraState.Create(_translation, _zoom);
        public Matrix TransformMatrix => CurrentState.TransformMatrix;

        public void SetState(Point translation, double zoom)
        {
            _translation = translation;
            _zoom = Math.Clamp(zoom, MinZoom, MaxZoom);
        }

        public Rect GetVisibleWorldBounds(Size viewport)
        {
            Matrix inverse = CurrentState.InverseMatrix;
            Point topLeft = inverse.Transform(new Point(0, 0));
            Point bottomRight = inverse.Transform(new Point(viewport.Width, viewport.Height));
            return new Rect(
                Math.Min(topLeft.X, bottomRight.X),
                Math.Min(topLeft.Y, bottomRight.Y),
                Math.Abs(bottomRight.X - topLeft.X),
                Math.Abs(bottomRight.Y - topLeft.Y));
        }

        public (int minX, int maxX, int minY, int maxY) GetVisibleCellBounds(Size viewport, double cellSize, double bufferCells = 2)
        {
            Rect visibleWorld = GetVisibleWorldBounds(viewport);
            double buffer = cellSize * bufferCells;
            return (
                (int)Math.Floor((visibleWorld.Left - buffer) / cellSize),
                (int)Math.Ceiling((visibleWorld.Right + buffer) / cellSize),
                (int)Math.Floor((visibleWorld.Top - buffer) / cellSize),
                (int)Math.Ceiling((visibleWorld.Bottom + buffer) / cellSize));
        }

        public (int minX, int maxX, int minY, int maxY) GetFieldCellBounds(Size viewport, double cellSize)
        {
            return GetVisibleCellBounds(viewport, cellSize, Tokens.MaxCullingRadiusCells);
        }

        /// <summary>
        /// Transforms a 2D world coordinate into 2D screen coordinate space:
        /// P_screen = P_world * Zoom + CameraOffset
        /// </summary>
        public Point WorldToScreen(Point worldPt)
        {
            return TransformMatrix.Transform(worldPt);
        }

        /// <summary>
        /// Transforms a 2D screen coordinate back into 2D world coordinate space:
        /// P_world = (P_screen - CameraOffset) / Zoom
        /// </summary>
        public Point ScreenToWorld(Point screenPt)
        {
            return CurrentState.InverseMatrix.Transform(screenPt);
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
            SetState(new Point(CameraX + delta.X, CameraY + delta.Y), Zoom);
        }

        public void PanBy(Vector deltaScreen) => Pan(deltaScreen);
        void ICameraEngine.ZoomAt(Point cursorScreen, double zoomDeltaFactor) =>
            ZoomAt(cursorScreen, zoomDeltaFactor, MinZoom, MaxZoom);

        /// <summary>
        /// Translates the 2D camera viewport by scalar x and y deltas.
        /// </summary>
        public void Pan(double deltaX, double deltaY)
        {
            Pan(new Vector(deltaX, deltaY));
        }

        /// <summary>
        /// Zooms the camera relative to a fixed 2D screen origin point (e.g. cursor position).
        /// Range: 1% (0.01) to 1000% (10.0).
        /// </summary>
        public void ZoomAt(Point originScreen, double zoomFactor, double minZoom = MinZoom, double maxZoom = MaxZoom)
        {
            double newZoom = Math.Clamp(Zoom * zoomFactor, minZoom, maxZoom);
            if (Math.Abs(newZoom - Zoom) < 0.000001) return;

            double newX = originScreen.X - (originScreen.X - CameraX) * (newZoom / Zoom);
            double newY = originScreen.Y - (originScreen.Y - CameraY) * (newZoom / Zoom);
            SetState(new Point(newX, newY), newZoom);
        }

        /// <summary>
        /// Obtains the 2D affine transformation matrix T(x, y, s).
        /// </summary>
        public Matrix GetTransformMatrix()
        {
            return TransformMatrix;
        }
    }
}
