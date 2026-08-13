using System.Text;
using GroveApp.Engine;
using GroveApp.Engine.Memory;
using GroveApp.Models;
using GroveApp.Models.Memory;

namespace GroveApp.Tests;

public sealed class MemorySearchTests
{
    [Fact]
    public void Search_returns_zero_content_memories_and_prioritizes_content_anchor_context()
    {
        var ledger = new ImmutableMemoryLedger();
        MemoryRecord anchorMatch = ledger.AppendMemory(
            MemoryPayloadKind.RichTextMarkdown,
            Encoding.UTF8.GetBytes("ordinary payload"),
            title: "Retirement notes");
        MemoryRecord zeroContent = ledger.AppendMemory(
            MemoryPayloadKind.RichTextMarkdown,
            Encoding.UTF8.GetBytes("Greg retirement details"),
            title: "Archive");
        var content = new GridNote(0, 0, "ordinary payload")
        {
            MemoryId = anchorMatch.MemoryId
        };
        MemoryAnchor anchor = MemoryAnchor.Create(
            anchorMatch.MemoryId,
            Guid.Empty,
            0,
            0) with
        {
            ContentId = content.Id,
            ContextLabel = "Greg retirement"
        };
        var service = new MemorySearchService(
            ledger,
            () => new[] { anchor },
            () => new[] { content },
            new FieldLedgerEngine());

        IReadOnlyList<MemorySearchResult> results = service.Search("Greg's retirement");

        Assert.Equal(2, results.Count);
        Assert.Equal(anchorMatch.MemoryId, results[0].Memory.MemoryId);
        Assert.Equal(MemorySearchMatchLevel.Anchor, results[0].MatchLevel);
        Assert.Contains(results, result => result.Memory.MemoryId == zeroContent.MemoryId);
    }

    [Fact]
    public void Spatial_recall_prefers_aura_overlap_over_a_distant_gap()
    {
        var ledger = new ImmutableMemoryLedger();
        MemoryRecord first = ledger.AppendMemory(
            MemoryPayloadKind.RichTextMarkdown,
            Encoding.UTF8.GetBytes("shared phrase"),
            title: "Shared");
        MemoryRecord nearby = ledger.AppendMemory(
            MemoryPayloadKind.RichTextMarkdown,
            Encoding.UTF8.GetBytes("shared phrase"),
            title: "Shared");
        MemoryRecord distant = ledger.AppendMemory(
            MemoryPayloadKind.RichTextMarkdown,
            Encoding.UTF8.GetBytes("shared phrase"),
            title: "Shared");
        var firstContent = new GridNote(0, 0, "first") { MemoryId = first.MemoryId };
        var nearbyContent = new GridNote(1, 0, "nearby") { MemoryId = nearby.MemoryId };
        var distantContent = new GridNote(20, 20, "distant") { MemoryId = distant.MemoryId };
        var service = new MemorySearchService(
            ledger,
            () => Array.Empty<MemoryAnchor>(),
            () => new[] { firstContent, nearbyContent, distantContent },
            new FieldLedgerEngine());

        IReadOnlyList<MemorySearchResult> results = service.Search("shared");
        MemorySearchResult nearbyResult = Assert.Single(results, result => result.Memory.MemoryId == nearby.MemoryId);
        MemorySearchResult distantResult = Assert.Single(results, result => result.Memory.MemoryId == distant.MemoryId);

        Assert.True(nearbyResult.SpatialScore > distantResult.SpatialScore);
    }
}
