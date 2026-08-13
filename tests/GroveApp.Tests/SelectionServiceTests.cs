using GroveApp.Engine.Interaction;
using GroveApp.Models.Interaction;

namespace GroveApp.Tests;

public sealed class SelectionServiceTests
{
    [Fact]
    public void Marquee_requires_complete_content_footprint()
    {
        var service = new SelectionService(220.0);
        service.BeginMarqueeSweep(new WorldPoint(0, 0));
        service.UpdateMarqueeSweep(new WorldPoint(220, 220));

        var fullyCovered = new SpatialSelectionCandidate(
            "full",
            new SpatialRegion(0, 0, 1, 1));
        var partiallyCovered = new SpatialSelectionCandidate(
            "partial",
            new SpatialRegion(0, 0, 2, 2));

        var result = service.PreviewMarqueeSweep(new[] { fullyCovered, partiallyCovered });

        Assert.Contains("full", result);
        Assert.DoesNotContain("partial", result);
    }

    [Fact]
    public void Marquee_contains_multi_cell_content_when_every_cell_is_covered()
    {
        var service = new SelectionService(220.0);
        service.BeginMarqueeSweep(new WorldPoint(220, 220));
        service.UpdateMarqueeSweep(new WorldPoint(660, 660));

        var candidate = new SpatialSelectionCandidate(
            "document",
            new SpatialRegion(1, 1, 2, 2));

        var result = service.PreviewMarqueeSweep(new[] { candidate });

        Assert.Contains("document", result);
    }
}
