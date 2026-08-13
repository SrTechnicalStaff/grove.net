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
            builder.AppendLine($"  - anchor_id: \"{anchor.AnchorId:D}\"");
            builder.AppendLine($"    memory_id: \"{anchor.MemoryId:D}\"");
            builder.AppendLine($"    layer_id: \"{anchor.LayerId:D}\"");
            builder.AppendLine($"    cell_x: {anchor.CellX}");
            builder.AppendLine($"    cell_y: {anchor.CellY}");
            builder.AppendLine($"    cell_width: {anchor.CellWidth}");
            builder.AppendLine($"    cell_height: {anchor.CellHeight}");
            builder.AppendLine($"    content_id: {Quote(anchor.ContentId)}");
            builder.AppendLine($"    content_type: {Quote(anchor.ContentType)}");
            builder.AppendLine($"    label: {Quote(anchor.ContextLabel)}");
            builder.AppendLine($"    created_at_iso: \"{new DateTime(anchor.CreatedAtTicks, DateTimeKind.Utc):O}\"");
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
        int cellWidth = 1;
        int cellHeight = 1;
        string contentId = string.Empty;
        string contentType = string.Empty;
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
                CellWidth = Math.Max(1, cellWidth),
                CellHeight = Math.Max(1, cellHeight),
                ContentId = contentId.Length > 0 ? contentId : context,
                ContentType = contentType,
                ContextLabel = contentId.Length > 0 ? context : string.Empty,
                CreatedAtTicks = createdAt
            });
            anchorId = Guid.Empty;
            memoryId = Guid.Empty;
            layerId = Guid.Empty;
            cellX = 0;
            cellY = 0;
            contentId = string.Empty;
            contentType = string.Empty;
            context = string.Empty;
            cellWidth = 1;
            cellHeight = 1;
            createdAt = DateTime.UtcNow.Ticks;
        }

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();
            if (line.StartsWith("- anchor_id:", StringComparison.Ordinal))
            {
                Flush();
                anchorId = ParseGuid(line[12..]);
            }
            else if (line.StartsWith("- id:", StringComparison.Ordinal))
            {
                Flush();
                anchorId = ParseGuid(line[5..]);
            }
            else if (line.StartsWith("memory_id:", StringComparison.Ordinal))
            {
                memoryId = ParseGuid(line[10..]);
            }
            else if (line.StartsWith("memory:", StringComparison.Ordinal))
            {
                memoryId = ParseGuid(line[7..]);
            }
            else if (line.StartsWith("layer_id:", StringComparison.Ordinal))
            {
                layerId = ParseGuid(line[9..]);
            }
            else if (line.StartsWith("layer:", StringComparison.Ordinal))
            {
                layerId = ParseGuid(line[6..]);
            }
            else if (line.StartsWith("cell_x:", StringComparison.Ordinal))
            {
                int.TryParse(line[7..].Trim(), out cellX);
            }
            else if (line.StartsWith("cell_y:", StringComparison.Ordinal))
            {
                int.TryParse(line[7..].Trim(), out cellY);
            }
            else if (line.StartsWith("cell_width:", StringComparison.Ordinal))
            {
                int.TryParse(line[11..].Trim(), out cellWidth);
            }
            else if (line.StartsWith("cell_height:", StringComparison.Ordinal))
            {
                int.TryParse(line[12..].Trim(), out cellHeight);
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
            else if (line.StartsWith("content_id:", StringComparison.Ordinal))
            {
                contentId = Unquote(line[11..].Trim());
            }
            else if (line.StartsWith("content_type:", StringComparison.Ordinal))
            {
                contentType = Unquote(line[13..].Trim());
            }
            else if (line.StartsWith("label:", StringComparison.Ordinal))
            {
                context = Unquote(line[6..].Trim());
            }
            else if (line.StartsWith("context:", StringComparison.Ordinal))
            {
                context = Unquote(line[8..].Trim());
            }
            else if (line.StartsWith("created_at_iso:", StringComparison.Ordinal) &&
                     DateTime.TryParse(Unquote(line[15..].Trim()), out DateTime parsed))
            {
                createdAt = parsed.ToUniversalTime().Ticks;
            }
        }

        Flush();
        return result;
    }

    private static string Quote(string value) =>
        $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";

    private static string Unquote(string value) =>
        value.Trim().Trim('"').Replace("\\\"", "\"", StringComparison.Ordinal).Replace("\\\\", "\\", StringComparison.Ordinal);

    private static Guid ParseGuid(string value) =>
        Guid.TryParse(Unquote(value.Trim()), out Guid parsed) ? parsed : Guid.Empty;
}
