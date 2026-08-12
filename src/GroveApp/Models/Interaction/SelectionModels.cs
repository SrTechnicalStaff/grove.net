using System;
using System.Collections.Immutable;

namespace GroveApp.Models.Interaction;

/// <summary>
/// The immutable geometry needed by the selection module. It is an adapter-friendly
/// projection of GridContentItem, not a second content model.
/// </summary>
public readonly record struct SpatialSelectionCandidate(string PlacementId, SpatialRegion Footprint)
{
    public static SpatialSelectionCandidate From(GroveApp.Models.GridContentItem item) =>
        new(item.Id, new SpatialRegion(item.CellX, item.CellY, item.CellWidth, item.CellHeight));
}

public readonly record struct MarqueeSweepState(
    WorldPoint Start,
    WorldPoint Current,
    bool IsAdditiveShift)
{
    public WorldRectangle Bounds => WorldRectangle.FromPoints(Start, Current);
}

public sealed record SelectionSnapshot(
    ImmutableArray<string> SelectedPlacementIds,
    string? PrimarySelectionId,
    MarqueeSweepState? Marquee)
{
    public bool IsMarqueeActive => Marquee.HasValue;
}
