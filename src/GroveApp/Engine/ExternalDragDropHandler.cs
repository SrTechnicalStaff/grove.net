using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using GroveApp.Models;

namespace GroveApp.Engine
{
    public sealed record CellCoordinate(int X, int Y);

    public readonly record struct DropPlacementPreview(
        CursorDescriptor Cursor,
        bool IsValid)
    {
        public CellCoordinate Origin => Cursor.PlacementOriginCell;
        public int Width => Cursor.WidthCells;
        public int Height => Cursor.HeightCells;
    }

    /// <summary>Resolves native file drops into grid content placements.</summary>
    public sealed class ExternalDragDropHandler
    {
        private readonly Func<CellCoordinate, int, int, bool> _isRegionFreeChecker;
        private readonly Func<GridContentItem, Task> _onItemPlacedAsync;
        private readonly Func<Point, int, int, CursorDescriptor> _resolvePlacementCursor;

        public DropPlacementPreview? CurrentPreview { get; private set; }
        public event Action? PreviewChanged;
        public event Action<Exception>? DropRejected;

        public ExternalDragDropHandler(
            Func<CellCoordinate, int, int, bool> isRegionFreeChecker,
            Func<GridContentItem, Task> onItemPlacedAsync,
            Func<Point, int, int, CursorDescriptor> resolvePlacementCursor)
        {
            _isRegionFreeChecker = isRegionFreeChecker ?? throw new ArgumentNullException(nameof(isRegionFreeChecker));
            _onItemPlacedAsync = onItemPlacedAsync ?? throw new ArgumentNullException(nameof(onItemPlacedAsync));
            _resolvePlacementCursor = resolvePlacementCursor ?? throw new ArgumentNullException(nameof(resolvePlacementCursor));
        }

        public void Attach(Control control)
        {
            DragDrop.SetAllowDrop(control, true);
            control.AddHandler(DragDrop.DragOverEvent, (s, e) => OnDragOver(s, e));
            control.AddHandler(DragDrop.DropEvent, async (s, e) => await OnDropAsync(s, e));
            control.AddHandler(DragDrop.DragLeaveEvent, (_, _) => ClearPreview());
        }

        public void OnDragOver(object? sender, DragEventArgs e)
        {
            if (!e.Data.Contains(DataFormats.Files))
            {
                e.DragEffects = DragDropEffects.None;
                ClearPreview();
                return;
            }

            Visual? visual = sender as Visual;
            if (visual == null)
            {
                ClearPreview();
                return;
            }

            Point screenPos = e.GetPosition(visual);
            var files = e.Data.GetFiles()?.Select(f => f.Path.LocalPath).ToList();
            (int reqW, int reqH)? requestedFootprint;
            try
            {
                requestedFootprint = ResolveRequestedFootprint(files?.FirstOrDefault());
            }
            catch (Exception exception)
            {
                DropRejected?.Invoke(exception);
                e.DragEffects = DragDropEffects.None;
                ClearPreview();
                e.Handled = true;
                return;
            }

            if (requestedFootprint is null)
            {
                e.DragEffects = DragDropEffects.None;
                ClearPreview();
                e.Handled = true;
                return;
            }

            (int reqW, int reqH) = requestedFootprint.Value;
            CursorDescriptor cursor = _resolvePlacementCursor(screenPos, reqW, reqH);
            CellCoordinate origin = cursor.PlacementOriginCell;

            bool isFree = _isRegionFreeChecker(origin, cursor.WidthCells, cursor.HeightCells);
            SetPreview(new DropPlacementPreview(cursor, isFree));
            e.DragEffects = isFree ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        public async Task OnDropAsync(object? sender, DragEventArgs e)
        {
            if (!e.Data.Contains(DataFormats.Files))
            {
                ClearPreview();
                return;
            }

            var files = e.Data.GetFiles()?.Select(f => f.Path.LocalPath).ToList();
            if (files is null || files.Count == 0)
            {
                ClearPreview();
                return;
            }

            Visual? visual = sender as Visual;
            if (visual == null)
            {
                ClearPreview();
                return;
            }

            Point screenPos = e.GetPosition(visual);
            try
            {
                int currentX;
                int currentY;
                string? firstFile = files.FirstOrDefault();
                (int firstWidth, int firstHeight)? firstFootprint = ResolveRequestedFootprint(firstFile);
                if (firstFootprint is null)
                {
                    return;
                }

                if (CurrentPreview is { IsValid: false })
                {
                    e.Handled = true;
                    return;
                }

                CursorDescriptor cursor = CurrentPreview is { } preview
                    ? preview.Cursor
                    : _resolvePlacementCursor(
                        screenPos,
                        firstFootprint.Value.firstWidth,
                        firstFootprint.Value.firstHeight);
                currentX = cursor.PlacementOriginCell.X;
                currentY = cursor.PlacementOriginCell.Y;

                foreach (string filePath in files)
                {
                    if (!File.Exists(filePath))
                    {
                        continue;
                    }

                    string ext = Path.GetExtension(filePath).ToLowerInvariant();
                    GridContentItem? newItem;
                    try
                    {
                        newItem = await CreatePlacementFromFileAsync(
                            filePath,
                            ext,
                            new CellCoordinate(currentX, currentY));
                    }
                    catch (Exception exception)
                    {
                        DropRejected?.Invoke(exception);
                        continue;
                    }

                    try
                    {
                        bool canPlace = newItem is not null &&
                            _isRegionFreeChecker(
                                new CellCoordinate(newItem.CellX, newItem.CellY),
                                newItem.CellWidth,
                                newItem.CellHeight);
                        if (canPlace)
                        {
                            await _onItemPlacedAsync(newItem!);
                            currentX += newItem!.CellWidth;
                        }
                        else if (newItem is IDisposable rejectedPlacement)
                        {
                            rejectedPlacement.Dispose();
                        }
                    }
                    catch (Exception exception)
                    {
                        if (newItem is IDisposable failedPlacement)
                        {
                            failedPlacement.Dispose();
                        }

                        DropRejected?.Invoke(exception);
                    }
                }

                e.Handled = true;
            }
            finally
            {
                ClearPreview();
            }
        }

        private static (int width, int height)? ResolveRequestedFootprint(string? firstFile)
        {
            if (string.IsNullOrWhiteSpace(firstFile))
            {
                return null;
            }

            string ext = Path.GetExtension(firstFile).ToLowerInvariant();
            if (ext is ".png" or ".jpg" or ".jpeg" or ".webp" or ".gif")
            {
                using var stream = File.OpenRead(firstFile);
                using var bitmap = new Avalonia.Media.Imaging.Bitmap(stream);
                ImageFootprint footprint = ImageFootprintResolver.Resolve(
                    (int)bitmap.Size.Width,
                    (int)bitmap.Size.Height);
                return (footprint.CellsW, footprint.CellsH);
            }

            if (ext is ".md" or ".json" or ".pdf")
            {
                return (2, 2);
            }

            if (ext == ".txt")
            {
                return new FileInfo(firstFile).Length >= 500 ? (2, 2) : (1, 1);
            }

            return (1, 1);
        }

        private void SetPreview(DropPlacementPreview preview)
        {
            if (CurrentPreview == preview)
            {
                return;
            }

            CurrentPreview = preview;
            PreviewChanged?.Invoke();
        }

        private void ClearPreview()
        {
            if (CurrentPreview is null)
            {
                return;
            }

            CurrentPreview = null;
            PreviewChanged?.Invoke();
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
            using var stream = File.OpenRead(filePath);
            Avalonia.Media.Imaging.Bitmap? bitmap = null;
            try
            {
                bitmap = new Avalonia.Media.Imaging.Bitmap(stream);
                int w = (int)bitmap.Size.Width;
                int h = (int)bitmap.Size.Height;
                var img = new GridImage(origin.X, origin.Y, filePath, w, h);
                img.UpdateFootprint(w, h);
                img.LoadedBitmap = bitmap;
                bitmap = null;
                return img;
            }
            finally
            {
                bitmap?.Dispose();
            }
        }

        private static async Task<GridContentItem> CreateTextOrDocumentPlacementAsync(string filePath, string ext, CellCoordinate origin)
        {
            string text = await File.ReadAllTextAsync(filePath);

            if (ext == ".txt" && text.Length < 500)
            {
                var note = new GridNote(origin.X, origin.Y, text, NoteColor.Violet);
                return note;
            }

            string title = Path.GetFileNameWithoutExtension(filePath);
            var doc = new GridDocument(origin.X, origin.Y, 2, 2, title, text);
            return doc;
        }
    }
}
