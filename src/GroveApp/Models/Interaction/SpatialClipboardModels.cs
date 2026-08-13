using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Serialization;

namespace GroveApp.Models.Interaction;

/// <summary>
/// A serializable spatial copy item. Relative geometry is preserved while the
/// destination layer and paste origin remain an adapter concern.
/// </summary>
public sealed record SpatialClipboardItemPayload(
    string OriginalPlacementId,
    string ContentType,
    string RawPayload,
    int RelativeCellX,
    int RelativeCellY,
    int WidthCells,
    int HeightCells,
    Guid? MemoryId = null,
    int IntrinsicWidthPx = 0,
    int IntrinsicHeightPx = 0)
{
    public SpatialRegion RelativeFootprint =>
        new(RelativeCellX, RelativeCellY, WidthCells, HeightCells);
}

public sealed record SpatialClipboardPlacement(
    string OriginalPlacementId,
    string ContentType,
    string RawPayload,
    SpatialRegion Footprint,
    Guid? MemoryId,
    int IntrinsicWidthPx,
    int IntrinsicHeightPx);

/// <summary>
/// Structured clipboard container for spatial placements. It carries no native
/// clipboard object and is therefore safe to use from a background or test adapter.
/// </summary>
public sealed record SpatialClipboardContainer
{
    public ImmutableArray<SpatialClipboardItemPayload> Items { get; }
    public DateTime CopiedAtUtc { get; }

    public SpatialClipboardContainer(
        IEnumerable<SpatialClipboardItemPayload> items,
        DateTime copiedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(items);
        Items = items.ToImmutableArray();
        CopiedAtUtc = copiedAtUtc.Kind == DateTimeKind.Utc
            ? copiedAtUtc
            : copiedAtUtc.ToUniversalTime();
    }

    [JsonConstructor]
    public SpatialClipboardContainer(
        ImmutableArray<SpatialClipboardItemPayload> items,
        DateTime copiedAtUtc)
        : this((IEnumerable<SpatialClipboardItemPayload>)items, copiedAtUtc)
    {
    }

    public static SpatialClipboardContainer FromItems(
        IEnumerable<GroveApp.Models.GridContentItem> items,
        Func<GroveApp.Models.GridContentItem, string>? rawPayload = null,
        DateTime? copiedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(items);
        var source = items.ToArray();
        if (source.Length == 0)
        {
            return new SpatialClipboardContainer(ImmutableArray<SpatialClipboardItemPayload>.Empty, copiedAtUtc ?? DateTime.UtcNow);
        }

        int originX = source.Min(item => item.CellX);
        int originY = source.Min(item => item.CellY);
        return new SpatialClipboardContainer(
            source.Select(item => new SpatialClipboardItemPayload(
                item.Id,
                item.Kind.ToString(),
                rawPayload?.Invoke(item) ?? GetDefaultRawPayload(item),
                item.CellX - originX,
                item.CellY - originY,
                item.CellWidth,
                item.CellHeight,
                item.MemoryId,
                item.IntrinsicWidthPx,
                item.IntrinsicHeightPx)),
            copiedAtUtc ?? DateTime.UtcNow);
    }

    public ImmutableArray<SpatialClipboardPlacement> PlaceAt(CellCoordinate targetOrigin) =>
        Items.Select(item => new SpatialClipboardPlacement(
            item.OriginalPlacementId,
            item.ContentType,
            item.RawPayload,
            new SpatialRegion(
                targetOrigin.X + item.RelativeCellX,
                targetOrigin.Y + item.RelativeCellY,
                item.WidthCells,
                item.HeightCells),
            item.MemoryId,
            item.IntrinsicWidthPx,
            item.IntrinsicHeightPx)).ToImmutableArray();

    private static string GetDefaultRawPayload(GroveApp.Models.GridContentItem item) => item switch
    {
        GroveApp.Models.GridNote note => note.Text,
        GroveApp.Models.GridDocument document => document.RawText,
        GroveApp.Models.GridImage image => image.FilePath,
        _ => string.Empty
    };
}
