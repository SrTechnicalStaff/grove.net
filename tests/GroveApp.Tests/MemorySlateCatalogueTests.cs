using System.Text;
using GroveApp.Engine.Memory;
using GroveApp.Models.Memory;

namespace GroveApp.Tests;

public sealed class MemorySlateCatalogueTests
{
    [Fact]
    public void Snapshot_contains_each_memory_record_once_without_spatial_input()
    {
        var ledger = new ImmutableMemoryLedger();
        MemoryRecord first = ledger.AppendMemory(
            MemoryPayloadKind.PlainText,
            Encoding.UTF8.GetBytes("first"));
        MemoryRecord second = ledger.AppendMemory(
            MemoryPayloadKind.PlainText,
            Encoding.UTF8.GetBytes("second"));
        var catalogue = new MemorySlateCatalogue(ledger);

        IReadOnlyList<MemoryRecord> memories = catalogue.Snapshot();

        Assert.Equal(new[] { first.MemoryId, second.MemoryId }, memories.Select(memory => memory.MemoryId));
    }
}
