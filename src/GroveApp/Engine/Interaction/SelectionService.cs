using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GroveApp.Models.Interaction;

namespace GroveApp.Engine.Interaction;

public interface ISelectionService
{
    ImmutableArray<string> SelectedPlacementIds { get; }
    string? PrimarySelectionId { get; }
    MarqueeSweepState? ActiveMarquee { get; }
    WorldRectangle? MarqueeBounds { get; }
    bool IsMarqueeActive { get; }
    event Action<SelectionSnapshot>? SelectionChanged;
    event Action<MarqueeSweepState?>? MarqueeChanged;

    void BeginMarqueeSweep(WorldPoint start, bool isShiftHeld = false);
    void UpdateMarqueeSweep(WorldPoint current);
    ImmutableHashSet<string> PreviewMarqueeSweep(IEnumerable<SpatialSelectionCandidate> candidates);
    bool CommitMarqueeSweep(IEnumerable<SpatialSelectionCandidate> candidates);
    void SelectSingle(string placementId, bool isShiftHeld = false);
    void SelectMany(IEnumerable<string> placementIds);
    void Remove(string placementId);
    void ClearSelection();
}

/// <summary>
/// Deep in-process selection module. It owns ordering, primary reassignment,
/// marquee lifetime, and the 50% area rule; input and drawing adapters only
/// translate events and consume snapshots.
/// </summary>
public sealed class SelectionService : ISelectionService
{
    public const double MinimumMarqueeOverlapRatio = 0.50;

    private readonly double _cellPitchDips;
    private readonly List<string> _selectedIds = new();
    private readonly HashSet<string> _selectedSet = new(StringComparer.Ordinal);
    private MarqueeSweepState? _activeMarquee;

    public SelectionService(double cellPitchDips = 220.0)
    {
        if (cellPitchDips <= 0 || double.IsNaN(cellPitchDips) || double.IsInfinity(cellPitchDips))
        {
            throw new ArgumentOutOfRangeException(nameof(cellPitchDips));
        }

        _cellPitchDips = cellPitchDips;
    }

    public ImmutableArray<string> SelectedPlacementIds => _selectedIds.ToImmutableArray();
    public string? PrimarySelectionId => _selectedIds.Count == 0 ? null : _selectedIds[0];
    public MarqueeSweepState? ActiveMarquee => _activeMarquee;
    public WorldRectangle? MarqueeBounds => _activeMarquee?.Bounds;
    public bool IsMarqueeActive => _activeMarquee.HasValue;

    public event Action<SelectionSnapshot>? SelectionChanged;
    public event Action<MarqueeSweepState?>? MarqueeChanged;

    public void BeginMarqueeSweep(WorldPoint start, bool isShiftHeld = false)
    {
        _activeMarquee = new MarqueeSweepState(start, start, isShiftHeld);
        MarqueeChanged?.Invoke(_activeMarquee);
    }

    public void UpdateMarqueeSweep(WorldPoint current)
    {
        if (!_activeMarquee.HasValue)
        {
            return;
        }

        MarqueeSweepState state = _activeMarquee.Value;
        _activeMarquee = state with { Current = current };
        MarqueeChanged?.Invoke(_activeMarquee);
    }

    public ImmutableHashSet<string> PreviewMarqueeSweep(IEnumerable<SpatialSelectionCandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        if (!_activeMarquee.HasValue)
        {
            return _selectedSet.ToImmutableHashSet(StringComparer.Ordinal);
        }

        var preview = _activeMarquee.Value.IsAdditiveShift
            ? new HashSet<string>(_selectedSet, StringComparer.Ordinal)
            : new HashSet<string>(StringComparer.Ordinal);
        WorldRectangle bounds = _activeMarquee.Value.Bounds;
        foreach (SpatialSelectionCandidate candidate in candidates)
        {
            if (string.IsNullOrWhiteSpace(candidate.PlacementId) || !candidate.Footprint.IsValid)
            {
                continue;
            }

            double ratio = candidate.Footprint.CalculateWorldAreaOverlapRatio(
                bounds.MinX,
                bounds.MinY,
                bounds.MaxX,
                bounds.MaxY,
                _cellPitchDips);
            if (ratio >= MinimumMarqueeOverlapRatio)
            {
                preview.Add(candidate.PlacementId);
            }
        }

        return preview.ToImmutableHashSet(StringComparer.Ordinal);
    }

    public bool CommitMarqueeSweep(IEnumerable<SpatialSelectionCandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        if (!_activeMarquee.HasValue)
        {
            return false;
        }

        MarqueeSweepState sweep = _activeMarquee.Value;
        if (!sweep.IsAdditiveShift)
        {
            _selectedIds.Clear();
            _selectedSet.Clear();
        }

        WorldRectangle bounds = sweep.Bounds;
        bool changed = false;
        foreach (SpatialSelectionCandidate candidate in candidates)
        {
            if (string.IsNullOrWhiteSpace(candidate.PlacementId) || !candidate.Footprint.IsValid)
            {
                continue;
            }

            double ratio = candidate.Footprint.CalculateWorldAreaOverlapRatio(
                bounds.MinX,
                bounds.MinY,
                bounds.MaxX,
                bounds.MaxY,
                _cellPitchDips);

            if (ratio >= MinimumMarqueeOverlapRatio)
            {
                changed |= Add(candidate.PlacementId);
            }
        }

        _activeMarquee = null;
        MarqueeChanged?.Invoke(null);
        if (changed || !sweep.IsAdditiveShift)
        {
            PublishSelection();
        }

        return changed;
    }

    public bool CommitMarqueeSweep(IEnumerable<GroveApp.Models.GridContentItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        return CommitMarqueeSweep(items.Select(SpatialSelectionCandidate.From));
    }

    public void SelectSingle(string placementId, bool isShiftHeld = false)
    {
        if (string.IsNullOrWhiteSpace(placementId))
        {
            throw new ArgumentException("A placement id is required.", nameof(placementId));
        }

        if (isShiftHeld)
        {
            if (!RemoveFromSelection(placementId))
            {
                Add(placementId);
            }
        }
        else
        {
            _selectedIds.Clear();
            _selectedSet.Clear();
            Add(placementId);
        }

        PublishSelection();
    }

    public void SelectMany(IEnumerable<string> placementIds)
    {
        ArgumentNullException.ThrowIfNull(placementIds);
        _selectedIds.Clear();
        _selectedSet.Clear();
        foreach (string placementId in placementIds)
        {
            if (!string.IsNullOrWhiteSpace(placementId))
            {
                Add(placementId);
            }
        }

        PublishSelection();
    }

    public void Remove(string placementId)
    {
        if (string.IsNullOrWhiteSpace(placementId) || !RemoveFromSelection(placementId))
        {
            return;
        }

        PublishSelection();
    }

    public void ClearSelection()
    {
        if (_selectedIds.Count == 0)
        {
            return;
        }

        _selectedIds.Clear();
        _selectedSet.Clear();
        PublishSelection();
    }

    private bool Add(string placementId)
    {
        if (!_selectedSet.Add(placementId))
        {
            return false;
        }

        _selectedIds.Add(placementId);
        return true;
    }

    private bool RemoveFromSelection(string placementId)
    {
        if (!_selectedSet.Remove(placementId))
        {
            return false;
        }

        _selectedIds.Remove(placementId);
        return true;
    }

    private void PublishSelection() => SelectionChanged?.Invoke(new SelectionSnapshot(
        _selectedIds.ToImmutableArray(),
        PrimarySelectionId,
        _activeMarquee));
}
