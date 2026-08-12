using System;
using Avalonia.Media.Imaging;
using GroveApp.Engine;

namespace GroveApp.Models
{
    /// <summary>
    /// Represents a Picture/Image placement on the spatial Grid (ADR-012).
    /// Maps intrinsic pixel resolution to discrete cell footprint (Nw x Nh) without cropping or letterboxing.
    /// </summary>
    public class GridImage : GridContentItem
    {
        public override ContentKind Kind => ContentKind.Image;
        public override float Mass => 4.0f;

        public string FilePath { get; set; } = "";
        public int IntrinsicWidthPx { get; set; }
        public int IntrinsicHeightPx { get; set; }
        public ImageFootprint Footprint { get; private set; }
        public bool IsAnimatedGif { get; set; }
        public Bitmap? LoadedBitmap { get; set; }

        public GridImage(int cellX, int cellY, string filePath, int widthPx, int heightPx, bool isAnchored = false, int layerId = 0)
            : base(cellX, cellY, 1, 1, isAnchored, layerId)
        {
            FilePath = filePath;
            IntrinsicWidthPx = widthPx;
            IntrinsicHeightPx = heightPx;
            IsAnimatedGif = filePath.EndsWith(".gif", StringComparison.OrdinalIgnoreCase);

            UpdateFootprint(widthPx, heightPx);
        }

        public void UpdateFootprint(int widthPx, int heightPx)
        {
            IntrinsicWidthPx = widthPx;
            IntrinsicHeightPx = heightPx;
            Footprint = ImageFootprintResolver.Resolve(widthPx, heightPx);
            CellWidth = Footprint.CellsW;
            CellHeight = Footprint.CellsH;
        }

        public double EffectivePpi => ImageFootprintResolver.CalculateEffectivePpi(IntrinsicWidthPx, IntrinsicHeightPx, CellWidth, CellHeight);
    }
}
