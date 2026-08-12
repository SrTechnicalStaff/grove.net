using System;
using GroveApp.DesignSystem;
using GroveApp.Models.Interaction;

namespace GroveApp.Models
{
    public enum ContentKind
    {
        Note = 1,
        Document = 2,
        Image = 3
    }

    /// <summary>
    /// Base model for all spatial grid content placements (Note, Document, Image).
    /// Enforces cell-quantized physical geometry and spatial collision bounds.
    /// </summary>
    public abstract class GridContentItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        /// <summary>
        /// Semantic memory identity backing this spatial placement. The canvas
        /// placement remains mutable; the memory ledger owns immutable revisions.
        /// </summary>
        public Guid? MemoryId { get; set; }
        public int CellX { get; set; }
        public int CellY { get; set; }
        public int CellWidth { get; set; } = 1;
        public int CellHeight { get; set; } = 1;
        public bool IsSelected { get; set; }
        public bool IsHovered { get; set; }
        public bool IsAnchored { get; set; }
        public int LayerId { get; set; } = 0;

        public abstract ContentKind Kind { get; }
        public virtual float Mass => 1.0f;
        public virtual string FieldHueHex => IsAnchored ? DesignSystem.Colors.AnchorHex : DesignSystem.Colors.NoteVioletFieldHex;
        public virtual int IntrinsicWidthPx { get; protected set; }
        public virtual int IntrinsicHeightPx { get; protected set; }

        protected GridContentItem(int cellX, int cellY, int cellWidth = 1, int cellHeight = 1, bool isAnchored = false, int layerId = 0)
        {
            CellX = cellX;
            CellY = cellY;
            CellWidth = Math.Max(1, cellWidth);
            CellHeight = Math.Max(1, cellHeight);
            IsAnchored = isAnchored;
            LayerId = layerId;
        }

        public bool ContainsCell(int cx, int cy)
        {
            return cx >= CellX && cx < CellX + CellWidth &&
                   cy >= CellY && cy < CellY + CellHeight;
        }

        public bool Intersects(int x, int y, int w, int h)
        {
            return !(x + w <= CellX || CellX + CellWidth <= x ||
                     y + h <= CellY || CellY + CellHeight <= y);
        }

        public virtual void ResizeTo(SpatialRegion footprint)
        {
            if (!footprint.IsValid)
            {
                throw new ArgumentOutOfRangeException(nameof(footprint));
            }

            CellX = footprint.X;
            CellY = footprint.Y;
            CellWidth = footprint.Width;
            CellHeight = footprint.Height;
        }
    }
}
