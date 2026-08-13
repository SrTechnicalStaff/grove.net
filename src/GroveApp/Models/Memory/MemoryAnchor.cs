using System;
using System.Collections.Immutable;

namespace GroveApp.Models.Memory;

/// <summary>
/// Immutable authored context attached to Content at a Grid Layer footprint.
/// The MemoryId is a relation key; the anchor is not Memory-owned state.
/// </summary>
public readonly record struct MemoryAnchor
{
    public required Guid AnchorId { get; init; }
    public required Guid MemoryId { get; init; }
    public required Guid LayerId { get; init; }
    public required int CellX { get; init; }
    public required int CellY { get; init; }
    public int CellWidth { get; init; } = 1;
    public int CellHeight { get; init; } = 1;
    public string ContentId { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public string ContextLabel { get; init; } = string.Empty;
    public ImmutableDictionary<string, string> ExtendedFrontmatter { get; init; } =
        ImmutableDictionary<string, string>.Empty;
    public required long CreatedAtTicks { get; init; }

    public MemoryAnchor()
    {
    }

    public static MemoryAnchor Create(
        Guid memoryId,
        Guid layerId,
        int cellX,
        int cellY,
        int cellWidth = 1,
        int cellHeight = 1,
        string contextLabel = "") => new()
        {
            AnchorId = Guid.CreateVersion7(),
            MemoryId = memoryId,
            LayerId = layerId,
            CellX = cellX,
            CellY = cellY,
            CellWidth = Math.Max(1, cellWidth),
            CellHeight = Math.Max(1, cellHeight),
            ContextLabel = contextLabel,
            CreatedAtTicks = DateTime.UtcNow.Ticks
        };
}
