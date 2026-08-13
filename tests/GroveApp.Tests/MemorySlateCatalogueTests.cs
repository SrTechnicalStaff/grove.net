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

    [Fact]
    public void Snapshot_preserves_payload_kinds_for_diverse_memories()
    {
        var ledger = new ImmutableMemoryLedger();
        MemoryRecord text = ledger.AppendMemory(
            MemoryPayloadKind.PlainText,
            Encoding.UTF8.GetBytes("note content"));
        MemoryRecord img = ledger.AppendMemory(
            MemoryPayloadKind.BinaryImage,
            new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });

        var catalogue = new MemorySlateCatalogue(ledger);
        IReadOnlyList<MemoryRecord> snapshot = catalogue.Snapshot();

        Assert.Equal(2, snapshot.Count);
        Assert.Contains(snapshot, m => m.PayloadKind == MemoryPayloadKind.PlainText);
        Assert.Contains(snapshot, m => m.PayloadKind == MemoryPayloadKind.BinaryImage);
    }
}
