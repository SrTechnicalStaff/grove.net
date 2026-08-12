using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using GroveApp.DesignSystem;
using GroveApp.Models;

namespace GroveApp.Engine
{
    public sealed record CellCoordinate(int X, int Y);

    public sealed record PlacementRequest(
        CellCoordinate Origin,
        int CellWidth,
        int CellHeight,
        GridContentItem CreatedItem);

    public static class SpatialCoordinateResolver
    {
        public const double CellPitch = Tokens.GridCell; // 220.0px

        public static CellCoordinate ScreenToCell(Point screenPoint, Point panOffset, double zoomScale)
        {
            zoomScale = Math.Clamp(zoomScale, 0.01, 10.0);

            double worldX = (screenPoint.X - panOffset.X) / zoomScale;
            double worldY = (screenPoint.Y - panOffset.Y) / zoomScale;

            int cellX = (int)Math.Floor(worldX / CellPitch);
            int cellY = (int)Math.Floor(worldY / CellPitch);

            return new CellCoordinate(cellX, cellY);
        }
    }

    /// <summary>
    /// Implements ADR-013 External Drag-and-Drop System, ScreenToCell Coordinate Resolution, and Content Auto-Creation.
    /// Handles native OS shell file drops (.png, .jpg, .txt, .md, .json, .pdf) onto the Grid.
    /// </summary>
    public sealed class ExternalDragDropHandler
    {
        private readonly Func<CellCoordinate, int, int, bool> _isRegionFreeChecker;
        private readonly Func<GridContentItem, Task> _onItemPlacedAsync;

        public ExternalDragDropHandler(
            Func<CellCoordinate, int, int, bool> isRegionFreeChecker,
            Func<GridContentItem, Task> onItemPlacedAsync)
        {
            _isRegionFreeChecker = isRegionFreeChecker ?? throw new ArgumentNullException(nameof(isRegionFreeChecker));
            _onItemPlacedAsync = onItemPlacedAsync ?? throw new ArgumentNullException(nameof(onItemPlacedAsync));
        }

        public void Attach(Control control, Func<Point> getPanOffset, Func<double> getZoom)
        {
            DragDrop.SetAllowDrop(control, true);
            control.AddHandler(DragDrop.DragOverEvent, (s, e) => OnDragOver(s, e, getPanOffset(), getZoom()));
            control.AddHandler(DragDrop.DropEvent, async (s, e) => await OnDropAsync(s, e, getPanOffset(), getZoom()));
        }

        public void OnDragOver(object? sender, DragEventArgs e, Point panOffset, double zoomScale)
        {
            if (!e.Data.Contains(DataFormats.Files))
            {
                e.DragEffects = DragDropEffects.None;
                return;
            }

            Visual? visual = sender as Visual;
            if (visual == null) return;

            Point screenPos = e.GetPosition(visual);
            CellCoordinate origin = SpatialCoordinateResolver.ScreenToCell(screenPos, panOffset, zoomScale);

            // Check if baseline cell is free
            bool isFree = _isRegionFreeChecker(origin, 1, 1);
            e.DragEffects = isFree ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        public async Task OnDropAsync(object? sender, DragEventArgs e, Point panOffset, double zoomScale)
        {
            if (!e.Data.Contains(DataFormats.Files)) return;

            var files = e.Data.GetFiles()?.Select(f => f.Path.LocalPath).ToList();
            if (files is null || files.Count == 0) return;

            Visual? visual = sender as Visual;
            if (visual == null) return;

            Point screenPos = e.GetPosition(visual);
            CellCoordinate origin = SpatialCoordinateResolver.ScreenToCell(screenPos, panOffset, zoomScale);

            int currentX = origin.X;
            int currentY = origin.Y;

            foreach (string filePath in files)
            {
                if (!File.Exists(filePath)) continue;

                string ext = Path.GetExtension(filePath).ToLowerInvariant();
                GridContentItem? newItem = await CreatePlacementFromFileAsync(filePath, ext, new CellCoordinate(currentX, currentY));

                if (newItem is not null && _isRegionFreeChecker(new CellCoordinate(newItem.CellX, newItem.CellY), newItem.CellWidth, newItem.CellHeight))
                {
                    await _onItemPlacedAsync(newItem);
                    currentX += newItem.CellWidth; // Shift next dropped item horizontally to avoid overlap
                }
            }

            e.Handled = true;
        }

        public static async Task<GridContentItem?> CreatePlacementFromFileAsync(string filePath, string ext, CellCoordinate origin)
        {
            switch (ext)
            {
                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".webp":
                case ".gif":
                    return CreateImagePlacement(filePath, origin);

                case ".txt":
                case ".md":
                case ".json":
                case ".pdf":
                    return await CreateTextOrDocumentPlacementAsync(filePath, ext, origin);

                default:
                    return null;
            }
        }

        private static GridContentItem CreateImagePlacement(string filePath, CellCoordinate origin)
        {
            int w = 800;
            int h = 600;

            try
            {
                using var stream = File.OpenRead(filePath);
                using var bitmap = new Avalonia.Media.Imaging.Bitmap(stream);
                w = (int)bitmap.Size.Width;
                h = (int)bitmap.Size.Height;
                var img = new GridImage(origin.X, origin.Y, filePath, w, h);
                img.LoadedBitmap = bitmap;
                return img;
            }
            catch
            {
                return new GridImage(origin.X, origin.Y, filePath, w, h);
            }
        }

        private static async Task<GridContentItem> CreateTextOrDocumentPlacementAsync(string filePath, string ext, CellCoordinate origin)
        {
            string text = "";
            try
            {
                text = await File.ReadAllTextAsync(filePath);
            }
            catch
            {
                text = Path.GetFileName(filePath);
            }

            if (ext == ".txt" && text.Length < 500)
            {
                var note = new GridNote(origin.X, origin.Y, text, NoteColor.Violet);
                return note;
            }

            // Long text, markdown, json, pdf -> Document placement (2x2 minimum)
            string title = Path.GetFileNameWithoutExtension(filePath);
            var doc = new GridDocument(origin.X, origin.Y, 2, 2, title, text);
            return doc;
        }
    }
}
