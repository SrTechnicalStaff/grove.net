using System;

namespace GroveApp.Models.Interaction;

/// <summary>
/// A discrete Plane 0 cell coordinate. The type is intentionally independent of
/// Avalonia so input adapters can translate into it at the seam.
/// </summary>
public readonly record struct CellCoordinate(int X, int Y)
{
    public CellCoordinate Translate(int deltaX, int deltaY) => new(X + deltaX, Y + deltaY);
}

/// <summary>
/// A positive, cell-aligned footprint.
/// </summary>
public readonly record struct SpatialRegion(int X, int Y, int Width, int Height)
{
    public int Right => X + Width;
    public int Bottom => Y + Height;
    public long AreaCells => Width > 0 && Height > 0 ? (long)Width * Height : 0;
    public bool IsValid => Width > 0 && Height > 0;

    public bool ContainsCell(CellCoordinate cell) =>
        cell.X >= X && cell.X < Right && cell.Y >= Y && cell.Y < Bottom;

    public bool Intersects(SpatialRegion other) =>
        IsValid && other.IsValid &&
        X < other.Right && Right > other.X &&
        Y < other.Bottom && Bottom > other.Y;

    public bool ContainsRegion(SpatialRegion other) =>
        IsValid && other.IsValid &&
        X <= other.X &&
        Y <= other.Y &&
        Right >= other.Right &&
        Bottom >= other.Bottom;

    public SpatialRegion Translate(int deltaX, int deltaY) =>
        new(X + deltaX, Y + deltaY, Width, Height);

    /// <summary>
    /// Returns the continuous world-space overlap fraction against a marquee.
    /// The denominator is this placement's complete area, as required by ADR-055.
    /// </summary>
    public double CalculateWorldAreaOverlapRatio(
        double marqueeMinX,
        double marqueeMinY,
        double marqueeMaxX,
        double marqueeMaxY,
        double cellPitchDips)
    {
        if (!IsValid || cellPitchDips <= 0 || marqueeMaxX <= marqueeMinX || marqueeMaxY <= marqueeMinY)
        {
            return 0;
        }

        double itemMinX = X * cellPitchDips;
        double itemMinY = Y * cellPitchDips;
        double itemMaxX = Right * cellPitchDips;
        double itemMaxY = Bottom * cellPitchDips;
        double overlapWidth = Math.Max(0, Math.Min(itemMaxX, marqueeMaxX) - Math.Max(itemMinX, marqueeMinX));
        double overlapHeight = Math.Max(0, Math.Min(itemMaxY, marqueeMaxY) - Math.Max(itemMinY, marqueeMinY));
        double totalArea = Width * cellPitchDips * (Height * cellPitchDips);

        return totalArea > 0 ? overlapWidth * overlapHeight / totalArea : 0;
    }
}

public readonly record struct WorldPoint(double X, double Y);

public readonly record struct WorldRectangle(double MinX, double MinY, double MaxX, double MaxY)
{
    public double Width => Math.Max(0, MaxX - MinX);
    public double Height => Math.Max(0, MaxY - MinY);

    public static WorldRectangle FromPoints(WorldPoint first, WorldPoint second) =>
        new(
            Math.Min(first.X, second.X),
            Math.Min(first.Y, second.Y),
            Math.Max(first.X, second.X),
            Math.Max(first.Y, second.Y));

    public SpatialRegion ToCoveredCellRegion(double cellPitchDips)
    {
        if (cellPitchDips <= 0 || double.IsNaN(cellPitchDips) || double.IsInfinity(cellPitchDips))
        {
            throw new ArgumentOutOfRangeException(nameof(cellPitchDips));
        }

        int minX = (int)Math.Floor(MinX / cellPitchDips);
        int minY = (int)Math.Floor(MinY / cellPitchDips);
        int maxX = (int)Math.Ceiling(MaxX / cellPitchDips) - 1;
        int maxY = (int)Math.Ceiling(MaxY / cellPitchDips) - 1;
        maxX = Math.Max(minX, maxX);
        maxY = Math.Max(minY, maxY);
        return new SpatialRegion(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }
}

public readonly record struct ScreenPoint(double X, double Y);

public readonly record struct ScreenSize(double Width, double Height)
{
    public bool IsValid => Width >= 0 && Height >= 0;
}

public readonly record struct ScreenRectangle(double X, double Y, double Width, double Height)
{
    public double Right => X + Width;
    public double Bottom => Y + Height;

    public bool Contains(ScreenPoint point) =>
        point.X >= X && point.X <= Right && point.Y >= Y && point.Y <= Bottom;
}
