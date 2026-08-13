using System;
using System.Collections.Generic;
using System.IO;
using GroveApp.Models.Memory;

namespace GroveApp.Engine.Memory;

/// <summary>
/// Thread-safe in-memory adapter for the memory ledger seam.
/// Payload records are append-only and never contain Content-side relations.
/// </summary>
public sealed class ImmutableMemoryLedger : IMemoryLedger
{
    private readonly Dictionary<Guid, MemoryRecord> _recordsById = new();
    private readonly Dictionary<ContentHash, Guid> _canonicalIdByHash = new();
    private readonly object _writeGate = new();

    public MemoryRecord AppendMemory(
        MemoryPayloadKind kind,
        ReadOnlySpan<byte> payload,
        Guid? parentMemoryId = null,
        string? title = null)
    {
        byte[] payloadCopy = payload.ToArray();
        ContentHash hash = new(payloadCopy);

        lock (_writeGate)
        {
            MemoryRecord? parent = null;
            if (parentMemoryId.HasValue && !_recordsById.TryGetValue(parentMemoryId.Value, out parent))
            {
                throw new KeyNotFoundException($"Parent memory {parentMemoryId.Value} was not found.");
            }

            Guid memoryId = Guid.CreateVersion7();
            Guid rootId = parent?.RootMemoryId ?? memoryId;
            uint generation = parent is null ? 0u : checked(parent.Generation + 1u);
            MemoryRecord record = MemoryRecord.Create(
                kind,
                payloadCopy,
                parentMemoryId,
                rootId,
                generation,
                title,
                memoryId);

            if (!_recordsById.TryAdd(record.MemoryId, record))
            {
                throw new InvalidOperationException($"Memory ID collision: {record.MemoryId}");
            }

            // Keep the first record as the stable content-addressable representative.
            _canonicalIdByHash.TryAdd(hash, record.MemoryId);
            return record;
        }
    }

    public MemoryRecord? GetMemory(Guid memoryId)
    {
        lock (_writeGate)
        {
            return _recordsById.TryGetValue(memoryId, out MemoryRecord? record) ? record : null;
        }
    }

    public bool TryGetMemoryByHash(ContentHash hash, out MemoryRecord? record)
    {
        lock (_writeGate)
        {
            if (_canonicalIdByHash.TryGetValue(hash, out Guid memoryId) &&
                _recordsById.TryGetValue(memoryId, out record))
            {
                return true;
            }

            record = null;
            return false;
        }
    }

    public void Import(MemoryRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        if (!record.HasValidHash())
        {
            throw new InvalidDataException($"Memory {record.MemoryId} failed its payload hash check.");
        }

        lock (_writeGate)
        {
            if (record.MemoryId == Guid.Empty || record.RootMemoryId == Guid.Empty)
            {
                throw new InvalidDataException("Imported memory records require non-empty identities.");
            }

            if (record.ParentMemoryId is Guid parentId && !_recordsById.ContainsKey(parentId))
            {
                throw new KeyNotFoundException($"Parent memory {parentId} was not found while importing {record.MemoryId}.");
            }

            MemoryRecord normalized = record;

            if (_recordsById.TryGetValue(normalized.MemoryId, out MemoryRecord? existing))
            {
                if (existing != normalized)
                {
                    throw new InvalidDataException($"Memory {normalized.MemoryId} has conflicting persisted records.");
                }

                return;
            }

            _recordsById.Add(normalized.MemoryId, normalized);
            _canonicalIdByHash.TryAdd(normalized.Hash, normalized.MemoryId);
        }
    }

    public ReadOnlySpan<MemoryRecord> GetAllMemories()
    {
        lock (_writeGate)
        {
            MemoryRecord[] snapshot = new MemoryRecord[_recordsById.Count];
            _recordsById.Values.CopyTo(snapshot, 0);
            Array.Sort(snapshot, CompareRecords);
            return snapshot;
        }
    }

    private static int CompareRecords(MemoryRecord left, MemoryRecord right)
    {
        int timestamp = left.CreatedAtTicks.CompareTo(right.CreatedAtTicks);
        return timestamp != 0
            ? timestamp
            : string.CompareOrdinal(left.MemoryId.ToString("N"), right.MemoryId.ToString("N"));
    }
}
