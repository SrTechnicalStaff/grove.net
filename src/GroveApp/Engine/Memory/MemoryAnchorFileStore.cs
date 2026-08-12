using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GroveApp.Models.Memory;

namespace GroveApp.Engine.Memory;

public interface IMemoryAnchorStore
{
    Task SaveAsync(string path, IReadOnlyCollection<MemoryAnchor> anchors);
    Task<IReadOnlyList<MemoryAnchor>> LoadAsync(string path);
}

/// <summary>
/// Minimal YAML-frontmatter adapter for spatial anchors. It deliberately owns
/// only the anchors block; document body content remains the caller's concern.
/// </summary>
public sealed class MemoryAnchorFileStore : IMemoryAnchorStore
{
    public async Task SaveAsync(string path, IReadOnlyCollection<MemoryAnchor> anchors)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(anchors);

        var builder = new StringBuilder();
        builder.AppendLine("---");
        builder.AppendLine("anchors:");
        foreach (MemoryAnchor anchor in anchors.OrderBy(item => item.CreatedAtTicks))
        {
            builder.AppendLine($"  - id: {anchor.AnchorId:N}");
            builder.AppendLine($"    memory: {anchor.MemoryId:N}");
            builder.AppendLine($"    layer: {anchor.LayerId:N}");
            builder.AppendLine($"    cell: [{anchor.CellX}, {anchor.CellY}]");
            builder.AppendLine($"    context: {Quote(anchor.ContextLabel)}");
        }
        builder.AppendLine("---");

        string? directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
        await File.WriteAllTextAsync(path, builder.ToString()).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<MemoryAnchor>> LoadAsync(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!File.Exists(path))
        {
            return Array.Empty<MemoryAnchor>();
        }

        string[] lines = await File.ReadAllLinesAsync(path).ConfigureAwait(false);
        var result = new List<MemoryAnchor>();
        Guid anchorId = Guid.Empty;
        Guid memoryId = Guid.Empty;
        Guid layerId = Guid.Empty;
        int cellX = 0;
        int cellY = 0;
        string context = string.Empty;
        long createdAt = DateTime.UtcNow.Ticks;

        void Flush()
        {
            if (anchorId == Guid.Empty || memoryId == Guid.Empty || layerId == Guid.Empty)
            {
                return;
            }

            result.Add(new MemoryAnchor
            {
                AnchorId = anchorId,
                MemoryId = memoryId,
                LayerId = layerId,
                CellX = cellX,
                CellY = cellY,
                ContextLabel = context,
                CreatedAtTicks = createdAt
            });
            anchorId = Guid.Empty;
            memoryId = Guid.Empty;
            layerId = Guid.Empty;
            context = string.Empty;
        }

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();
            if (line.StartsWith("- id:", StringComparison.Ordinal))
            {
                Flush();
                Guid.TryParse(line[5..].Trim(), out anchorId);
            }
            else if (line.StartsWith("memory:", StringComparison.Ordinal))
            {
                Guid.TryParse(line[7..].Trim(), out memoryId);
            }
            else if (line.StartsWith("layer:", StringComparison.Ordinal))
            {
                Guid.TryParse(line[6..].Trim(), out layerId);
            }
            else if (line.StartsWith("cell:", StringComparison.Ordinal))
            {
                string[] parts = line[5..].Trim().Trim('[', ']').Split(',', StringSplitOptions.TrimEntries);
                if (parts.Length == 2)
                {
                    int.TryParse(parts[0], out cellX);
                    int.TryParse(parts[1], out cellY);
                }
            }
            else if (line.StartsWith("context:", StringComparison.Ordinal))
            {
                context = Unquote(line[8..].Trim());
            }
        }

        Flush();
        return result;
    }

    private static string Quote(string value) =>
        $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";

    private static string Unquote(string value) =>
        value.Trim().Trim('"').Replace("\\\"", "\"", StringComparison.Ordinal).Replace("\\\\", "\\", StringComparison.Ordinal);
}
