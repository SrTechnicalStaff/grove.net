using System;
using Avalonia;
using Avalonia.Media;
using GroveApp.DesignSystem;
using GroveApp.Models;
using Colors = GroveApp.DesignSystem.Colors;

namespace GroveApp.Engine
{
    public enum CursorFootprintKind : byte
    {
        MinorGrid,
        MajorGrid,
        Supercell,
        Content,
        ArmedTool,
        DropPreview
    }

    /// <summary>Dimensions for an armed placement.</summary>
    public readonly record struct CursorPlacementFootprint(int WidthCells, int HeightCells)
    {
        public CursorPlacementFootprint Normalize() => new(
            Math.Max(1, WidthCells),
            Math.Max(1, HeightCells));
    }

    /// <summary>Resolved world-space cursor geometry.</summary>
    public readonly record struct CursorDescriptor(
        Point WorldOrigin,
        Size WorldExtent,
        CursorFootprintKind Kind,
        double RingStrokeWidth)
    {
        public CursorPlacementFootprint PlacementFootprint => new(
            Math.Max(1, (int)Math.Round(WorldExtent.Width / Tokens.GridCell)),
            Math.Max(1, (int)Math.Round(WorldExtent.Height / Tokens.GridCell)));

        public CellCoordinate PlacementOriginCell => new(
            (int)Math.Floor(WorldOrigin.X / Tokens.GridCell),
            (int)Math.Floor(WorldOrigin.Y / Tokens.GridCell));

        public int WidthCells => PlacementFootprint.WidthCells;
        public int HeightCells => PlacementFootprint.HeightCells;

        public Rect WorldBounds => new(WorldOrigin, WorldExtent);

        public bool OccupiesSameFootprint(CursorDescriptor other) =>
            WorldOrigin.Equals(other.WorldOrigin) && WorldExtent.Equals(other.WorldExtent);
    }

    /// <summary>Resolves and renders the grid cursor and spent trail.</summary>
    public class CursorRenderModule
    {
        /// <summary>Resolves cursor geometry from world position and content state.</summary>
        public CursorDescriptor ResolveCursorDescriptor(
            Point worldPoint,
            double zoom,
            GridContentItem? targetItem,
            CursorPlacementFootprint? armedToolFootprint = null)
        {
            if (armedToolFootprint is CursorPlacementFootprint placement)
            {
                CursorPlacementFootprint normalized = placement.Normalize();
                double toolOriginX = Math.Floor(worldPoint.X / Tokens.GridCell) * Tokens.GridCell;
                double toolOriginY = Math.Floor(worldPoint.Y / Tokens.GridCell) * Tokens.GridCell;
                return CreateDescriptor(
                    toolOriginX,
                    toolOriginY,
                    normalized.WidthCells * Tokens.GridCell,
                    normalized.HeightCells * Tokens.GridCell,
                    CursorFootprintKind.ArmedTool,
                    zoom);
            }

            if (targetItem is not null)
            {
                return CreateDescriptor(
                    targetItem.CellX * Tokens.GridCell,
                    targetItem.CellY * Tokens.GridCell,
                    targetItem.CellWidth * Tokens.GridCell,
                    targetItem.CellHeight * Tokens.GridCell,
                    CursorFootprintKind.Content,
                    zoom);
            }

            (double gridPitch, CursorFootprintKind kind) = GetEmptySpaceFootprint(zoom);
            double originX = Math.Floor(worldPoint.X / gridPitch) * gridPitch;
            double originY = Math.Floor(worldPoint.Y / gridPitch) * gridPitch;
            return CreateDescriptor(
                originX,
                originY,
                gridPitch,
                gridPitch,
                kind,
                zoom);
        }

        public CursorDescriptor ResolveDropPreviewDescriptor(
            Point worldPoint,
            double zoom,
            CursorPlacementFootprint placement)
        {
            return ResolveCursorDescriptor(
                worldPoint,
                zoom,
                targetItem: null,
                armedToolFootprint: placement) with
            {
                Kind = CursorFootprintKind.DropPreview
            };
        }

        public void RenderGridCursor(
            DrawingContext context,
            Func<Point, Point> worldToScreen,
            CursorDescriptor descriptor)
        {
            Rect cursorRect = GetScreenBounds(worldToScreen, descriptor);

            RenderDescriptor(
                context,
                worldToScreen,
                descriptor,
                Colors.NoteText,
                Tokens.CursorSteady * Tokens.CursorFillGain,
                Colors.NoteText,
                Tokens.CursorRingInk);
        }

        public void RenderDescriptor(
            DrawingContext context,
            Func<Point, Point> worldToScreen,
            CursorDescriptor descriptor,
            Color fillColor,
            double fillOpacity,
            Color ringColor,
            double ringOpacity)
        {
            Rect cursorRect = GetScreenBounds(worldToScreen, descriptor);
            context.FillRectangle(
                new SolidColorBrush(Color.FromArgb(ToAlphaByte(fillOpacity), fillColor.R, fillColor.G, fillColor.B)),
                cursorRect);
            var ringPen = new Pen(
                new SolidColorBrush(Color.FromArgb(ToAlphaByte(ringOpacity), ringColor.R, ringColor.G, ringColor.B)),
                descriptor.RingStrokeWidth);
            context.DrawRectangle(null, ringPen, cursorRect.Deflate(descriptor.RingStrokeWidth / 2.0));
        }

        public Rect GetScreenBounds(
            Func<Point, Point> worldToScreen,
            CursorDescriptor descriptor)
        {
            Point startScreen = worldToScreen(descriptor.WorldOrigin);
            Point endScreen = worldToScreen(descriptor.WorldBounds.BottomRight);
            return new Rect(
                startScreen.X,
                startScreen.Y,
                endScreen.X - startScreen.X,
                endScreen.Y - startScreen.Y);
        }

        private static CursorDescriptor CreateDescriptor(
            double originX,
            double originY,
            double width,
            double height,
            CursorFootprintKind kind,
            double zoom)
        {
            return new CursorDescriptor(
                new Point(originX, originY),
                new Size(width, height),
                kind,
                GetRingStrokeWidth(zoom));
        }

        private static (double pitch, CursorFootprintKind kind) GetEmptySpaceFootprint(double zoom) =>
            zoom >= Tokens.CursorLodMajorZoom
                ? (Tokens.MinorCellSize, CursorFootprintKind.MinorGrid)
                : zoom >= Tokens.CursorLodSupercellZoom
                    ? (Tokens.GridCell, CursorFootprintKind.MajorGrid)
                    : (Tokens.SupercellPitch, CursorFootprintKind.Supercell);

        private static double GetRingStrokeWidth(double zoom) => zoom >= Tokens.CursorLodMajorZoom
            ? Tokens.CursorLodMinorRing
            : zoom >= Tokens.CursorLodSupercellZoom
                ? Tokens.CursorLodMajorRing
                : Tokens.CursorLodSupercellRing;

        private static byte ToAlphaByte(double alpha) =>
            (byte)Math.Clamp(Math.Round(alpha * byte.MaxValue), 0, byte.MaxValue);
    }
}
