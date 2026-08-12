using System;
using Avalonia.Media.Imaging;
using GroveApp.Engine;

namespace GroveApp.Models
{
    /// <summary>
    /// Represents a Picture/Image placement on the spatial Grid (ADR-012).
    /// Maps intrinsic pixel resolution to discrete cell footprint (Nw x Nh) without cropping or letterboxing.
    /// </summary>
    public sealed class GridImage : GridContentItem, IDisposable
    {
        public override ContentKind Kind => ContentKind.Image;
        public override float Mass => 4.0f;
        public override string FieldHueHex => IsAnchored ? DesignSystem.Colors.AnchorHex : DesignSystem.Colors.ToolFillHex;

        public string FilePath { get; set; } = "";
        public int IntrinsicWidthPx { get; set; }
        public int IntrinsicHeightPx { get; set; }
        public ImageFootprint Footprint { get; private set; }
        public bool IsAnimatedGif { get; set; }
        private Bitmap? _loadedBitmap;
        private bool _disposed;

        /// <summary>
        /// Gets or sets the decoded bitmap owned by this placement.
        /// Assigning a bitmap transfers ownership to the placement; replacing it
        /// releases the previous native image resource.
        /// </summary>
        public Bitmap? LoadedBitmap
        {
            get => _loadedBitmap;
            set
            {
                ObjectDisposedException.ThrowIf(_disposed, this);

                if (ReferenceEquals(_loadedBitmap, value))
                {
                    return;
                }

                _loadedBitmap?.Dispose();
                _loadedBitmap = value;
            }
        }

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

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _loadedBitmap?.Dispose();
            _loadedBitmap = null;
            GC.SuppressFinalize(this);
        }
    }
}
