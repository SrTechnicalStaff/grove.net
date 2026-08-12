using System;
using System.Collections.Immutable;

namespace GroveApp.Models.Interaction;

public enum ResizeHandleLocation : byte
{
    None = 0,
    NorthWest = 1,
    NorthEast = 2,
    SouthEast = 3,
    SouthWest = 4
}

public readonly record struct ResizeHandleHitTest(
    ResizeHandleLocation Location,
    double ScreenCenterX,
    double ScreenCenterY,
    double TargetSizePixels = 12.0)
{
    public bool ContainsPointer(double pointerX, double pointerY)
    {
        double half = TargetSizePixels * 0.5;
        return pointerX >= ScreenCenterX - half &&
               pointerX <= ScreenCenterX + half &&
               pointerY >= ScreenCenterY - half &&
               pointerY <= ScreenCenterY + half;
    }
}

public readonly record struct ResizeOccupancy(string PlacementId, SpatialRegion Footprint);

public sealed record ResizeCollisionResult(
    bool IsValid,
    ImmutableArray<string> CollidingPlacementIds,
    string? Reason)
{
    public static ResizeCollisionResult Valid { get; } =
        new(true, ImmutableArray<string>.Empty, null);
}

public sealed record ResizeGeometryResult(
    SpatialRegion CandidateFootprint,
    ResizeCollisionResult Collision,
    ImmutableArray<ResizeHandleHitTest> Handles)
{
    public bool IsValid => Collision.IsValid;
}
