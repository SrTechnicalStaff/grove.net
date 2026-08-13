using GroveApp.Engine.Interaction;
using GroveApp.Models.Interaction;

namespace GroveApp.Tests;

public sealed class GroupTranslationEngineTests
{
    private static SpatialPlacementSnapshot Placement(
        string id,
        int x,
        int y,
        int width = 1,
        int height = 1,
        int layerId = 0) =>
        new(id, layerId, new SpatialRegion(x, y, width, height));

    [Fact]
    public void Group_moves_as_one_rigid_transaction_into_free_space()
    {
        var engine = new GroupTranslationEngine();
        var cluster = new[] { Placement("a", 0, 0), Placement("b", 2, 0, 2, 1) };

        GroupTranslationResult result = engine.Preview(
            cluster,
            cluster,
            new CellDelta(1, 2));

        Assert.True(result.IsValid);
        Assert.Equal(GroupTranslationFailureReason.None, result.FailureReason);
        Assert.Contains((0, 0), result.VacatedCells);
        Assert.Contains((2, 0), result.VacatedCells);
    }

    [Fact]
    public void Group_refuses_only_external_occupied_target_cells()
    {
        var engine = new GroupTranslationEngine();
        var cluster = new[] { Placement("a", 0, 0), Placement("b", 2, 0) };
        var occupancy = cluster.Append(Placement("external", 1, 1)).ToArray();

        GroupTranslationResult result = engine.Preview(
            cluster,
            occupancy,
            new CellDelta(1, 1));

        Assert.False(result.IsValid);
        Assert.Equal(GroupTranslationFailureReason.Collision, result.FailureReason);
    }

    [Fact]
    public void Anchored_content_is_not_a_movement_refusal()
    {
        var engine = new GroupTranslationEngine();
        var cluster = new[] { Placement("anchored", 0, 0), Placement("second", 1, 0) };

        GroupTranslationResult result = engine.Preview(
            cluster,
            cluster,
            new CellDelta(0, 1));

        Assert.True(result.IsValid);
    }
}
