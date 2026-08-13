using System;
using System.Collections.Immutable;
using GroveApp.Models.Memory;

namespace GroveApp.Engine.Memory;

/// <summary>
/// Small external seam for append-only memory storage and anchor metadata.
/// Implementations must be thread-safe, preserve payload immutability, and make
/// hash lookup deterministic for the lifetime of the ledger.
/// </summary>
public interface IMemoryLedger
{
    MemoryRecord AppendMemory(
        MemoryPayloadKind kind,
        ReadOnlySpan<byte> payload,
        Guid? parentMemoryId = null,
        ImmutableList<MemoryAnchor>? initialAnchors = null);

    MemoryRecord? GetMemory(Guid memoryId);

    bool TryGetMemoryByHash(ContentHash hash, out MemoryRecord? record);

    MemoryRecord AddAnchor(Guid memoryId, MemoryAnchor anchor);

    MemoryRecord RemoveAnchor(Guid memoryId, Guid anchorId);

    void Import(MemoryRecord record);

    ReadOnlySpan<MemoryRecord> GetAllMemories();
}
