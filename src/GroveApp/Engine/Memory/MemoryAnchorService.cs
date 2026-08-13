using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GroveApp.Models;
using GroveApp.Models.Memory;

namespace GroveApp.Engine.Memory;

public interface IMemoryAnchorService
{
    IReadOnlyList<MemoryAnchor> ActiveAnchors { get; }

    void Attach(GridContentItem placement);

    void UpdatePayload(GridContentItem placement);

    void UpdatePlacement(GridContentItem placement);

    void Detach(GridContentItem placement);

    Task<IReadOnlyList<MemoryRecord>> HydrateAsync();

    Task FlushAsync();
}

public sealed class MemoryAnchorService : IMemoryAnchorService
{
    private readonly IMemoryLedger _ledger;
    private readonly IMemoryVersionTree _versionTree;
    private readonly MemorySpatialIndex _spatialIndex;
    private readonly IMemoryAnchorStore _anchorStore;
    private readonly string _anchorFilePath;
    private readonly IMemoryRecordStore? _recordStore;
    private readonly string? _recordDirectory;
    private readonly Func<int, Guid> _layerIdentity;
    private readonly SemaphoreSlim _persistenceGate = new(1, 1);

    public MemoryAnchorService(
        IMemoryLedger ledger,
        IMemoryVersionTree versionTree,
        MemorySpatialIndex spatialIndex,
        IMemoryAnchorStore anchorStore,
        string anchorFilePath,
        Func<int, Guid> layerIdentity,
        IMemoryRecordStore? recordStore = null,
        string? recordDirectory = null)
    {
        _ledger = ledger ?? throw new ArgumentNullException(nameof(ledger));
        _versionTree = versionTree ?? throw new ArgumentNullException(nameof(versionTree));
        _spatialIndex = spatialIndex ?? throw new ArgumentNullException(nameof(spatialIndex));
        _anchorStore = anchorStore ?? throw new ArgumentNullException(nameof(anchorStore));
        ArgumentException.ThrowIfNullOrWhiteSpace(anchorFilePath);
        _anchorFilePath = anchorFilePath;
        _layerIdentity = layerIdentity ?? throw new ArgumentNullException(nameof(layerIdentity));
        if (recordStore is not null && string.IsNullOrWhiteSpace(recordDirectory))
        {
            throw new ArgumentException("A record directory is required when a record store is configured.", nameof(recordDirectory));
        }

        _recordStore = recordStore;
        _recordDirectory = recordDirectory;
    }

    public event Action<Exception>? PersistenceFailed;

    public IReadOnlyList<MemoryAnchor> ActiveAnchors => _spatialIndex.Snapshot();

    public async Task<IReadOnlyList<MemoryRecord>> HydrateAsync()
    {
        if (_recordStore is null || _recordDirectory is null)
        {
            return Array.Empty<MemoryRecord>();
        }

        IReadOnlyList<MemoryRecord> records = await _recordStore.LoadAsync(_recordDirectory).ConfigureAwait(false);
        foreach (MemoryRecord record in records)
        {
            _ledger.Import(record);
        }

        foreach (MemoryRecord record in records)
        {
            _versionTree.AddVersionNode(record, _ledger);
        }

        _spatialIndex.Clear();
        var anchors = records
            .SelectMany(record => record.Anchors.Select(anchor => (record, anchor)))
            .GroupBy(entry => entry.anchor.AnchorId)
            .Select(group => group
                .OrderByDescending(entry => entry.record.Generation)
                .ThenByDescending(entry => entry.record.CreatedAtTicks)
                .First().anchor)
            .ToArray();

        if (anchors.Length == 0)
        {
            anchors = (await _anchorStore.LoadAsync(_anchorFilePath).ConfigureAwait(false)).ToArray();
        }

        foreach (MemoryAnchor anchor in anchors)
        {
            if (_ledger.GetMemory(anchor.MemoryId) is not null)
            {
                _spatialIndex.Insert(anchor);
            }
        }

        return records;
    }

    public void Attach(GridContentItem placement)
    {
        ArgumentNullException.ThrowIfNull(placement);

        if (!placement.MemoryId.HasValue || _ledger.GetMemory(placement.MemoryId.Value) is null)
        {
            MemoryRecord record = AppendMemory(placement, parentMemoryId: null);
            placement.MemoryId = record.MemoryId;
        }

        ReplaceAnchor(placement);
    }

    public void UpdatePayload(GridContentItem placement)
    {
        ArgumentNullException.ThrowIfNull(placement);

        Guid? anchorId = placement.AnchorId;
        Guid? parentMemoryId = placement.MemoryId;
        RemoveAnchor(placement, clearPlacementIdentity: false, persist: false);
        MemoryRecord record = AppendMemory(placement, parentMemoryId);
        placement.MemoryId = record.MemoryId;
        placement.AnchorId = anchorId;
        AddAnchor(placement, anchorId);
        PersistSnapshot();
    }

    public void UpdatePlacement(GridContentItem placement)
    {
        ArgumentNullException.ThrowIfNull(placement);
        if (!placement.MemoryId.HasValue)
        {
            Attach(placement);
            return;
        }

        ReplaceAnchor(placement);
    }

    public void Detach(GridContentItem placement)
    {
        ArgumentNullException.ThrowIfNull(placement);
        RemoveAnchor(placement, clearPlacementIdentity: true, persist: true);
    }

    private MemoryRecord AppendMemory(GridContentItem placement, Guid? parentMemoryId)
    {
        MemoryPayloadKind payloadKind = GridContentMemoryPayloadAdapter.GetKind(placement);
        byte[] payload = GridContentMemoryPayloadAdapter.ReadPayload(placement);
        MemoryRecord record = _ledger.AppendMemory(payloadKind, payload, parentMemoryId);
        _versionTree.AddVersionNode(record, _ledger);
        return record;
    }

    private void ReplaceAnchor(GridContentItem placement)
    {
        Guid? anchorId = placement.AnchorId;
        RemoveAnchor(placement, clearPlacementIdentity: false, persist: false);

        if (!placement.MemoryId.HasValue)
        {
            throw new InvalidOperationException("A placement must have a MemoryId before it can be anchored.");
        }

        AddAnchor(placement, anchorId);
        PersistSnapshot();
    }

    private void AddAnchor(GridContentItem placement, Guid? anchorId = null)
    {
        if (!placement.MemoryId.HasValue)
        {
            throw new InvalidOperationException("A placement must have a MemoryId before it can be anchored.");
        }

        MemoryAnchor anchor = new()
        {
            AnchorId = anchorId ?? Guid.CreateVersion7(),
            MemoryId = placement.MemoryId.Value,
            LayerId = _layerIdentity(placement.LayerId),
            CellX = placement.CellX,
            CellY = placement.CellY,
            CellWidth = placement.CellWidth,
            CellHeight = placement.CellHeight,
            ContentId = placement.Id,
            ContentType = placement.Kind.ToString(),
            ContextLabel = placement is GridDocument document ? document.Title : string.Empty,
            CreatedAtTicks = DateTime.UtcNow.Ticks
        };

        _ledger.AddAnchor(anchor.MemoryId, anchor);
        _spatialIndex.Insert(anchor);
        placement.AnchorId = anchor.AnchorId;
    }

    private void RemoveAnchor(GridContentItem placement, bool clearPlacementIdentity, bool persist)
    {
        if (placement.MemoryId is not Guid memoryId || placement.AnchorId is not Guid anchorId)
        {
            if (clearPlacementIdentity)
            {
                placement.AnchorId = null;
            }

            return;
        }

        _spatialIndex.Remove(anchorId, out _);
        _ledger.RemoveAnchor(memoryId, anchorId);
        if (clearPlacementIdentity)
        {
            placement.AnchorId = null;
        }

        if (persist)
        {
            PersistSnapshot();
        }
    }

    private void PersistSnapshot()
    {
        MemoryAnchor[] snapshot = _spatialIndex.Snapshot();
        _ = PersistSnapshotAsync(snapshot);
    }

    public Task FlushAsync() => PersistSnapshotAsync(_spatialIndex.Snapshot());

    private async System.Threading.Tasks.Task PersistSnapshotAsync(MemoryAnchor[] snapshot)
    {
        await _persistenceGate.WaitAsync().ConfigureAwait(false);
        try
        {
            await _anchorStore.SaveAsync(_anchorFilePath, snapshot).ConfigureAwait(false);
            if (_recordStore is not null && _recordDirectory is not null)
            {
                MemoryRecord[] records = _ledger.GetAllMemories().ToArray();
                await _recordStore.SaveAsync(_recordDirectory, records).ConfigureAwait(false);
            }
        }
        catch (Exception exception)
        {
            PersistenceFailed?.Invoke(exception);
        }
        finally
        {
            _persistenceGate.Release();
        }
    }
}
