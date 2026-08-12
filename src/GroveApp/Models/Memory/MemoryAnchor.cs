using System;
using System.Collections.Immutable;

namespace GroveApp.Models.Memory;

/// <summary>
/// Immutable spatial manifestation of a memory on one layer and one grid cell.
/// </summary>
public readonly record struct MemoryAnchor
{
    public required Guid AnchorId { get; init; }
    public required Guid MemoryId { get; init; }
    public required Guid LayerId { get; init; }
    public required int CellX { get; init; }
    public required int CellY { get; init; }
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
        string contextLabel = "") => new()
        {
            AnchorId = Guid.CreateVersion7(),
            MemoryId = memoryId,
            LayerId = layerId,
            CellX = cellX,
            CellY = cellY,
            ContextLabel = contextLabel,
            CreatedAtTicks = DateTime.UtcNow.Ticks
        };
}
