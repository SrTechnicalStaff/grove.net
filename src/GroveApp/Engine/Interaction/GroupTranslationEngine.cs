using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GroveApp.Models.Interaction;

namespace GroveApp.Engine.Interaction;

public readonly record struct CellDelta(int X, int Y);

public readonly record struct SpatialPlacementSnapshot(
    string ContentId,
    int GridLayerId,
    SpatialRegion Footprint);

public enum GroupTranslationFailureReason
{
    None,
    InvalidCluster,
    MixedGridLayers,
    Collision
}

public readonly record struct GroupTranslationResult(
    bool IsValid,
    GroupTranslationFailureReason FailureReason,
    CellDelta Delta,
    ImmutableHashSet<(int X, int Y)> VacatedCells);

/// <summary>
/// Plans a rigid translation against an immutable occupancy snapshot. The
/// planner does not know about controls, anchors, or rendering state.
/// </summary>
public sealed class GroupTranslationEngine
{
    public GroupTranslationResult Preview(
        IReadOnlyList<SpatialPlacementSnapshot> cluster,
        IReadOnlyList<SpatialPlacementSnapshot> occupancy,
        CellDelta delta)
    {
        ArgumentNullException.ThrowIfNull(cluster);
        ArgumentNullException.ThrowIfNull(occupancy);

        if (cluster.Count == 0 || cluster.Any(snapshot => !snapshot.Footprint.IsValid))
        {
            return Invalid(GroupTranslationFailureReason.InvalidCluster, delta);
        }

        int gridLayerId = cluster[0].GridLayerId;
        if (cluster.Any(snapshot => snapshot.GridLayerId != gridLayerId))
        {
            return Invalid(GroupTranslationFailureReason.MixedGridLayers, delta);
        }

        var clusterIds = cluster
            .Select(snapshot => snapshot.ContentId)
            .ToHashSet(StringComparer.Ordinal);
        var sourceCells = ExpandCells(cluster);
        var targetCells = cluster
            .Select(snapshot => snapshot.Footprint.Translate(delta.X, delta.Y))
            .SelectMany(ExpandCells)
            .ToHashSet();

        var externalOccupiedCells = occupancy
            .Where(snapshot => snapshot.GridLayerId == gridLayerId && !clusterIds.Contains(snapshot.ContentId))
            .SelectMany(snapshot => ExpandCells(snapshot.Footprint))
            .ToHashSet();

        if (targetCells.Except(sourceCells).Any(externalOccupiedCells.Contains))
        {
            return new GroupTranslationResult(
                false,
                GroupTranslationFailureReason.Collision,
                delta,
                sourceCells.Except(targetCells).ToImmutableHashSet());
        }

        return new GroupTranslationResult(
            true,
            GroupTranslationFailureReason.None,
            delta,
            sourceCells.Except(targetCells).ToImmutableHashSet());
    }

    private static GroupTranslationResult Invalid(GroupTranslationFailureReason reason, CellDelta delta) =>
        new(false, reason, delta, ImmutableHashSet<(int X, int Y)>.Empty);

    private static HashSet<(int X, int Y)> ExpandCells(IReadOnlyList<SpatialPlacementSnapshot> snapshots)
    {
        var cells = new HashSet<(int X, int Y)>();
        foreach (SpatialPlacementSnapshot snapshot in snapshots)
        {
            cells.UnionWith(ExpandCells(snapshot.Footprint));
        }

        return cells;
    }

    private static IEnumerable<(int X, int Y)> ExpandCells(SpatialRegion footprint)
    {
        for (int x = footprint.X; x < footprint.Right; x++)
        {
            for (int y = footprint.Y; y < footprint.Bottom; y++)
            {
                yield return (x, y);
            }
        }
    }
}
