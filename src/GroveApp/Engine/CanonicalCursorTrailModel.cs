using System;
using System.Collections.Generic;
using Avalonia;
using GroveApp.DesignSystem;
using GroveApp.Models;
using GroveApp.Models.Interaction;

namespace GroveApp.Engine;

public sealed class CanonicalCursorTrailModel
{
    private readonly CursorRenderModule _renderer;
    private readonly List<SpentCell> _trail = new();
    private CursorDescriptor? _previousDescriptor;

    public CanonicalCursorTrailModel(CursorRenderModule renderer)
    {
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
    }

    public CursorDescriptor CurrentDescriptor { get; private set; }

    public IReadOnlyList<SpentCell> Trail => _trail;

    public long TrailVersion { get; private set; }

    public List<SpentCell> MutableTrailCompatibilityView => _trail;

    public CursorDescriptor Resolve(
        Point worldPoint,
        double zoom,
        GridContentItem? targetItem,
        CursorPlacementFootprint? armedToolFootprint = null) =>
        _renderer.ResolveCursorDescriptor(worldPoint, zoom, targetItem, armedToolFootprint);

    public CursorDescriptor ResolveDropPreview(
        Point worldPoint,
        double zoom,
        CursorPlacementFootprint placement) =>
        _renderer.ResolveDropPreviewDescriptor(worldPoint, zoom, placement);

    public CursorDescriptor Apply(CursorDescriptor descriptor)
    {
        if (_previousDescriptor is CursorDescriptor previous &&
            !previous.OccupiesSameFootprint(descriptor))
        {
            AddTrailEntry(new SpentCell(
                previous.WorldOrigin.X,
                previous.WorldOrigin.Y,
                previous.WorldExtent.Width,
                previous.WorldExtent.Height));
        }

        CurrentDescriptor = descriptor;
        _previousDescriptor = descriptor;
        return descriptor;
    }

    public bool AdvanceTrail()
    {
        bool needsRedraw = false;
        for (int index = _trail.Count - 1; index >= 0; index--)
        {
            _trail[index].Energy *= Tokens.CursorTrailDecay;
            if (_trail[index].Energy <= Tokens.CursorTrailMin)
            {
                _trail.RemoveAt(index);
            }

            needsRedraw = true;
        }

        if (needsRedraw)
        {
            TrailVersion++;
        }

        return needsRedraw;
    }

    public void RecordContentTranslationTrail(
        int originX,
        int originY,
        int widthCells,
        int heightCells)
    {
        AddTrailEntry(new SpentCell(
            originX * Tokens.GridCell,
            originY * Tokens.GridCell,
            Math.Max(1, widthCells) * Tokens.GridCell,
            Math.Max(1, heightCells) * Tokens.GridCell));
    }

    public void RecordContentTranslationTrail(
        IEnumerable<SpatialRegion> sourceFootprints,
        IEnumerable<SpatialRegion> targetFootprints)
    {
        ArgumentNullException.ThrowIfNull(sourceFootprints);
        ArgumentNullException.ThrowIfNull(targetFootprints);

        var sourceCells = ExpandCells(sourceFootprints);
        var targetCells = ExpandCells(targetFootprints);
        sourceCells.ExceptWith(targetCells);

        foreach ((int cellX, int cellY) in sourceCells)
        {
            AddTrailEntry(new SpentCell(
                cellX * Tokens.GridCell,
                cellY * Tokens.GridCell,
                Tokens.GridCell,
                Tokens.GridCell));
        }
    }

    private static HashSet<(int cellX, int cellY)> ExpandCells(IEnumerable<SpatialRegion> footprints)
    {
        var cells = new HashSet<(int cellX, int cellY)>();
        foreach (SpatialRegion footprint in footprints)
        {
            if (!footprint.IsValid)
            {
                continue;
            }

            for (int cellY = footprint.Y; cellY < footprint.Bottom; cellY++)
            {
                for (int cellX = footprint.X; cellX < footprint.Right; cellX++)
                {
                    cells.Add((cellX, cellY));
                }
            }
        }

        return cells;
    }

    private void AddTrailEntry(SpentCell entry)
    {
        _trail.Add(entry);
        while (_trail.Count > Tokens.CursorTrailMaxSteps)
        {
            _trail.RemoveAt(0);
        }

        TrailVersion++;
    }
}
