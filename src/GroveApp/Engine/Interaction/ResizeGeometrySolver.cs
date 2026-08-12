using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GroveApp.Models;
using GroveApp.Models.Interaction;

namespace GroveApp.Engine.Interaction;

/// <summary>
/// Type-specific footprint math for interactive resize. It has no knowledge of
/// pointer devices, rendering, or the content mutation adapter.
/// </summary>
public static class TypeSpecificFootprintSolver
{
    public static SpatialRegion SolveFootprint(
        ContentKind contentKind,
        SpatialRegion initialFootprint,
        ResizeHandleLocation handle,
        int deltaCellX,
        int deltaCellY,
        int intrinsicWidthPx = 0,
        int intrinsicHeightPx = 0)
    {
        if (!initialFootprint.IsValid)
        {
            throw new ArgumentOutOfRangeException(nameof(initialFootprint), "The initial footprint must be positive.");
        }

        ResizeDraft draft = ResizeDraft.From(initialFootprint, handle, deltaCellX, deltaCellY);
        return contentKind switch
        {
            ContentKind.Note => SolveNote(initialFootprint, handle, draft),
            ContentKind.Document => SolveDocument(initialFootprint, handle, draft),
            ContentKind.Image => SolveImage(initialFootprint, handle, draft, intrinsicWidthPx, intrinsicHeightPx),
            _ => throw new ArgumentOutOfRangeException(nameof(contentKind))
        };
    }

    private static SpatialRegion SolveNote(SpatialRegion initial, ResizeHandleLocation handle, ResizeDraft draft)
    {
        int size = Math.Clamp(Math.Max(draft.Width, draft.Height), 1, 8);
        return Anchor(initial, handle, size, size);
    }

    private static SpatialRegion SolveDocument(SpatialRegion initial, ResizeHandleLocation handle, ResizeDraft draft)
    {
        int width = Math.Clamp(draft.Width, 2, 8);
        int height = Math.Clamp(draft.Height, 2, 8);
        return Anchor(initial, handle, width, height);
    }

    private static SpatialRegion SolveImage(
        SpatialRegion initial,
        ResizeHandleLocation handle,
        ResizeDraft draft,
        int intrinsicWidthPx,
        int intrinsicHeightPx)
    {
        if (intrinsicWidthPx <= 0 || intrinsicHeightPx <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(intrinsicWidthPx), "Image dimensions are required for aspect-preserving resize.");
        }

        bool landscape = intrinsicWidthPx >= intrinsicHeightPx;
        double longPx = Math.Max(intrinsicWidthPx, intrinsicHeightPx);
        double shortPx = Math.Min(intrinsicWidthPx, intrinsicHeightPx);
        int draggedLongAxis = landscape ? draft.Width : draft.Height;
        int longAxis = Math.Clamp(draggedLongAxis, 1, 8);
        int shortAxis = Math.Clamp((int)Math.Round(longAxis * shortPx / longPx, MidpointRounding.AwayFromZero), 1, 8);
        int width = landscape ? longAxis : shortAxis;
        int height = landscape ? shortAxis : longAxis;

        return Anchor(initial, handle, width, height);
    }

    private static SpatialRegion Anchor(SpatialRegion initial, ResizeHandleLocation handle, int width, int height)
    {
        bool anchorsRight = handle is ResizeHandleLocation.NorthWest or ResizeHandleLocation.SouthWest;
        bool anchorsBottom = handle is ResizeHandleLocation.NorthWest or ResizeHandleLocation.NorthEast;
        int x = anchorsRight ? initial.Right - width : initial.X;
        int y = anchorsBottom ? initial.Bottom - height : initial.Y;
        return new SpatialRegion(x, y, width, height);
    }

    private readonly record struct ResizeDraft(int X, int Y, int Width, int Height)
    {
        public static ResizeDraft From(SpatialRegion initial, ResizeHandleLocation handle, int deltaX, int deltaY) => handle switch
        {
            ResizeHandleLocation.SouthEast => new(initial.X, initial.Y, initial.Width + deltaX, initial.Height + deltaY),
            ResizeHandleLocation.SouthWest => new(initial.X + deltaX, initial.Y, initial.Width - deltaX, initial.Height + deltaY),
            ResizeHandleLocation.NorthEast => new(initial.X, initial.Y + deltaY, initial.Width + deltaX, initial.Height - deltaY),
            ResizeHandleLocation.NorthWest => new(initial.X + deltaX, initial.Y + deltaY, initial.Width - deltaX, initial.Height - deltaY),
            _ => new(initial.X, initial.Y, initial.Width, initial.Height)
        };
    }
}

/// <summary>
/// Adds collision evaluation and handle geometry around the pure footprint solver.
/// </summary>
public static class ResizeGeometrySolver
{
    public static ResizeGeometryResult Solve(
        ContentKind contentKind,
        SpatialRegion initialFootprint,
        ResizeHandleLocation handle,
        int deltaCellX,
        int deltaCellY,
        string placementId,
        IEnumerable<ResizeOccupancy>? occupiedRegions = null,
        int intrinsicWidthPx = 0,
        int intrinsicHeightPx = 0,
        double cellPitchDips = 220,
        double zoom = 1,
        ScreenPoint screenOrigin = default)
    {
        ArgumentNullException.ThrowIfNull(placementId);
        if (cellPitchDips <= 0 || zoom <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(cellPitchDips));
        }

        SpatialRegion candidate = TypeSpecificFootprintSolver.SolveFootprint(
            contentKind,
            initialFootprint,
            handle,
            deltaCellX,
            deltaCellY,
            intrinsicWidthPx,
            intrinsicHeightPx);
        ResizeCollisionResult collision = CheckCollision(placementId, candidate, occupiedRegions);
        ImmutableArray<ResizeHandleHitTest> handles = CreateHandleHitTests(
            candidate,
            cellPitchDips,
            zoom,
            screenOrigin);

        return new ResizeGeometryResult(candidate, collision, handles);
    }

    public static ResizeCollisionResult CheckCollision(
        string placementId,
        SpatialRegion candidate,
        IEnumerable<ResizeOccupancy>? occupiedRegions)
    {
        ArgumentNullException.ThrowIfNull(placementId);
        if (!candidate.IsValid)
        {
            return new ResizeCollisionResult(false, ImmutableArray<string>.Empty, "The candidate footprint is invalid.");
        }

        ImmutableArray<string> collisions = (occupiedRegions ?? [])
            .Where(occupancy =>
                !string.Equals(occupancy.PlacementId, placementId, StringComparison.Ordinal) &&
                occupancy.Footprint.Intersects(candidate))
            .Select(occupancy => occupancy.PlacementId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.Ordinal)
            .ToImmutableArray();

        return collisions.Length == 0
            ? ResizeCollisionResult.Valid
            : new ResizeCollisionResult(false, collisions, "This space is occupied.");
    }

    public static ImmutableArray<ResizeHandleHitTest> CreateHandleHitTests(
        SpatialRegion footprint,
        double cellPitchDips,
        double zoom,
        ScreenPoint screenOrigin)
    {
        if (!footprint.IsValid || cellPitchDips <= 0 || zoom <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(footprint));
        }

        double left = screenOrigin.X + footprint.X * cellPitchDips * zoom;
        double top = screenOrigin.Y + footprint.Y * cellPitchDips * zoom;
        double right = screenOrigin.X + footprint.Right * cellPitchDips * zoom;
        double bottom = screenOrigin.Y + footprint.Bottom * cellPitchDips * zoom;

        return
        [
            new(ResizeHandleLocation.NorthWest, left, top),
            new(ResizeHandleLocation.NorthEast, right, top),
            new(ResizeHandleLocation.SouthEast, right, bottom),
            new(ResizeHandleLocation.SouthWest, left, bottom)
        ];
    }
}
