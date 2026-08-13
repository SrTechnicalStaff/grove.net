using System;
using GroveApp.Models.Memory;

namespace GroveApp.Engine.Memory;

/// <summary>
/// Small external seam for append-only semantic Memory storage.
/// Implementations must be thread-safe, preserve payload immutability, and make
/// hash lookup deterministic for the lifetime of the ledger.
/// </summary>
public interface IMemoryLedger
{
    MemoryRecord AppendMemory(
        MemoryPayloadKind kind,
        ReadOnlySpan<byte> payload,
        Guid? parentMemoryId = null,
        string? title = null);

    MemoryRecord? GetMemory(Guid memoryId);

    bool TryGetMemoryByHash(ContentHash hash, out MemoryRecord? record);

    void Import(MemoryRecord record);

    ReadOnlySpan<MemoryRecord> GetAllMemories();
}
