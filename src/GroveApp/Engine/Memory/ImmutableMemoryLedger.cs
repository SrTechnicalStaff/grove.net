using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using GroveApp.Models.Memory;

namespace GroveApp.Engine.Memory;

/// <summary>
/// Thread-safe in-memory adapter for the memory ledger seam.
/// Payload records are append-only; anchor changes replace an immutable snapshot
/// under the same memory identity and never alter the payload or its hash.
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
        ImmutableList<MemoryAnchor>? initialAnchors = null)
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

            // A root import with the same payload is a duplicate. Versioned appends
            // still create a child node so the lineage can represent a real edit.
            if (!parentMemoryId.HasValue &&
                _canonicalIdByHash.TryGetValue(hash, out Guid canonicalId) &&
                _recordsById.TryGetValue(canonicalId, out MemoryRecord? canonical))
            {
                if (initialAnchors is null || initialAnchors.Count == 0)
                {
                    return canonical;
                }

                return AddAnchorsUnderLock(canonical, initialAnchors);
            }

            Guid memoryId = Guid.CreateVersion7();
            Guid rootId = parent?.RootMemoryId ?? memoryId;
            uint generation = parent is null ? 0u : checked(parent.Generation + 1u);
            ImmutableList<MemoryAnchor> anchors = NormalizeAnchors(initialAnchors, memoryId);

            MemoryRecord record = MemoryRecord.Create(
                kind,
                payloadCopy,
                parentMemoryId,
                rootId,
                generation,
                anchors,
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

    public MemoryRecord AddAnchor(Guid memoryId, MemoryAnchor anchor)
    {
        lock (_writeGate)
        {
            MemoryRecord record = GetRequiredMemoryUnderLock(memoryId);
            MemoryAnchor normalized = NormalizeAnchor(anchor, memoryId);

            foreach (MemoryAnchor existing in record.Anchors)
            {
                if (existing.AnchorId == normalized.AnchorId)
                {
                    if (existing != normalized)
                    {
                        throw new InvalidOperationException(
                            $"Anchor ID {normalized.AnchorId} is already assigned to different metadata.");
                    }

                    return record;
                }
            }

            ImmutableList<MemoryAnchor> anchors = record.Anchors.Add(normalized);
            MemoryRecord updated = record.WithAnchors(anchors, NextTimestamp(record.UpdatedAtTicks));
            _recordsById[memoryId] = updated;
            return updated;
        }
    }

    public MemoryRecord RemoveAnchor(Guid memoryId, Guid anchorId)
    {
        if (anchorId == Guid.Empty)
        {
            throw new ArgumentException("An anchor ID is required.", nameof(anchorId));
        }

        lock (_writeGate)
        {
            MemoryRecord record = GetRequiredMemoryUnderLock(memoryId);
            int index = -1;
            for (int i = 0; i < record.Anchors.Count; i++)
            {
                if (record.Anchors[i].AnchorId == anchorId)
                {
                    index = i;
                    break;
                }
            }

            if (index < 0)
            {
                return record;
            }

            MemoryRecord updated = record.WithAnchors(
                record.Anchors.RemoveAt(index),
                NextTimestamp(record.UpdatedAtTicks));
            _recordsById[memoryId] = updated;
            return updated;
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

    private MemoryRecord AddAnchorsUnderLock(
        MemoryRecord record,
        ImmutableList<MemoryAnchor> initialAnchors)
    {
        MemoryRecord updated = record;
        foreach (MemoryAnchor anchor in initialAnchors)
        {
            updated = AddAnchorUnderLock(updated, NormalizeAnchor(anchor, record.MemoryId));
        }

        return updated;
    }

    private MemoryRecord AddAnchorUnderLock(MemoryRecord record, MemoryAnchor anchor)
    {
        foreach (MemoryAnchor existing in record.Anchors)
        {
            if (existing.AnchorId == anchor.AnchorId)
            {
                if (existing != anchor)
                {
                    throw new InvalidOperationException($"Anchor ID {anchor.AnchorId} has conflicting metadata.");
                }

                return record;
            }
        }

        MemoryRecord updated = record.WithAnchors(
            record.Anchors.Add(anchor),
            NextTimestamp(record.UpdatedAtTicks));
        _recordsById[record.MemoryId] = updated;
        return updated;
    }

    private MemoryRecord GetRequiredMemoryUnderLock(Guid memoryId)
    {
        return _recordsById.TryGetValue(memoryId, out MemoryRecord? record)
            ? record
            : throw new KeyNotFoundException($"Memory {memoryId} was not found.");
    }

    private static ImmutableList<MemoryAnchor> NormalizeAnchors(
        ImmutableList<MemoryAnchor>? anchors,
        Guid memoryId)
    {
        if (anchors is null || anchors.Count == 0)
        {
            return ImmutableList<MemoryAnchor>.Empty;
        }

        ImmutableList<MemoryAnchor>.Builder normalized = ImmutableList.CreateBuilder<MemoryAnchor>();
        HashSet<Guid> anchorIds = new();
        foreach (MemoryAnchor anchor in anchors)
        {
            MemoryAnchor normalizedAnchor = NormalizeAnchor(anchor, memoryId);
            if (!anchorIds.Add(normalizedAnchor.AnchorId))
            {
                throw new ArgumentException(
                    $"Initial anchors contain duplicate ID {normalizedAnchor.AnchorId}.",
                    nameof(anchors));
            }

            normalized.Add(normalizedAnchor);
        }

        return normalized.ToImmutable();
    }

    private static MemoryAnchor NormalizeAnchor(MemoryAnchor anchor, Guid memoryId)
    {
        if (anchor.MemoryId != Guid.Empty && anchor.MemoryId != memoryId)
        {
            throw new ArgumentException(
                $"Anchor {anchor.AnchorId} belongs to memory {anchor.MemoryId}, not {memoryId}.",
                nameof(anchor));
        }

        return anchor with
        {
            AnchorId = anchor.AnchorId == Guid.Empty ? Guid.CreateVersion7() : anchor.AnchorId,
            MemoryId = memoryId,
            CreatedAtTicks = anchor.CreatedAtTicks == 0 ? DateTime.UtcNow.Ticks : anchor.CreatedAtTicks
        };
    }

    private static long NextTimestamp(long previous) => Math.Max(DateTime.UtcNow.Ticks, previous + 1);

    private static int CompareRecords(MemoryRecord left, MemoryRecord right)
    {
        int timestamp = left.CreatedAtTicks.CompareTo(right.CreatedAtTicks);
        return timestamp != 0
            ? timestamp
            : string.CompareOrdinal(left.MemoryId.ToString("N"), right.MemoryId.ToString("N"));
    }
}
