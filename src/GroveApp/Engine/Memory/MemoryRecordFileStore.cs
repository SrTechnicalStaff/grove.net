using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GroveApp.Models.Memory;

namespace GroveApp.Engine.Memory;

public interface IMemoryRecordStore
{
    Task SaveAsync(string directory, IReadOnlyCollection<MemoryRecord> records);

    Task<IReadOnlyList<MemoryRecord>> LoadAsync(string directory);
}

public sealed class MemoryRecordFileStore : IMemoryRecordStore
{
    public async Task SaveAsync(string directory, IReadOnlyCollection<MemoryRecord> records)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentNullException.ThrowIfNull(records);
        Directory.CreateDirectory(directory);

        foreach (MemoryRecord record in records)
        {
            if (!record.HasValidHash())
            {
                throw new InvalidDataException($"Memory {record.MemoryId} failed its payload hash check.");
            }

            string stem = Path.Combine(directory, record.MemoryId.ToString("N"));
            bool isBinary = record.PayloadKind == MemoryPayloadKind.BinaryImage;
            string metadataPath = stem + (isBinary ? ".meta.md" : ".md");
            string payloadPath = stem + ".bin";
            string payloadReference = isBinary ? Path.GetFileName(payloadPath) : string.Empty;

            await File.WriteAllTextAsync(
                metadataPath,
                BuildDocument(record, payloadReference),
                Encoding.UTF8).ConfigureAwait(false);

            if (isBinary)
            {
                await File.WriteAllBytesAsync(payloadPath, record.RawPayload.ToArray()).ConfigureAwait(false);
            }
        }
    }

    public async Task<IReadOnlyList<MemoryRecord>> LoadAsync(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        if (!Directory.Exists(directory))
        {
            return Array.Empty<MemoryRecord>();
        }

        var records = new List<MemoryRecord>();
        foreach (string metadataPath in Directory.EnumerateFiles(directory, "*.md", SearchOption.TopDirectoryOnly))
        {
            MemoryRecord? record = await ReadDocumentAsync(metadataPath).ConfigureAwait(false);
            if (record is not null)
            {
                records.Add(record);
            }
        }

        return records
            .OrderBy(record => record.Generation)
            .ThenBy(record => record.CreatedAtTicks)
            .ThenBy(record => record.MemoryId.ToString("N"), StringComparer.Ordinal)
            .ToArray();
    }

    private static string BuildDocument(MemoryRecord record, string payloadReference)
    {
        var builder = new StringBuilder();
        builder.AppendLine("---");
        builder.AppendLine($"memory_id: \"{record.MemoryId:D}\"");
        builder.AppendLine($"content_hash: \"{record.Hash}\"");
        builder.AppendLine($"payload_kind: \"{record.PayloadKind}\"");
        builder.AppendLine($"title: {Quote(record.Title)}");
        builder.AppendLine($"created_at_iso: \"{new DateTime(record.CreatedAtTicks, DateTimeKind.Utc):O}\"");
        builder.AppendLine($"parent_memory_id: {(record.ParentMemoryId is Guid parent ? $"\"{parent:D}\"" : "null")}");
        builder.AppendLine($"root_memory_id: \"{record.RootMemoryId:D}\"");
        builder.AppendLine($"generation: {record.Generation}");
        if (payloadReference.Length > 0)
        {
            builder.AppendLine($"payload_file: \"{payloadReference}\"");
        }

        builder.AppendLine("---");
        if (payloadReference.Length == 0)
        {
            builder.Append(Encoding.UTF8.GetString(record.RawPayload.Span));
        }

        return builder.ToString();
    }

    private static async Task<MemoryRecord?> ReadDocumentAsync(string metadataPath)
    {
        string document = await File.ReadAllTextAsync(metadataPath, Encoding.UTF8).ConfigureAwait(false);
        int frontmatterStartLength = document.StartsWith("---\r\n", StringComparison.Ordinal) ? 5 : 4;
        if (!document.StartsWith("---\n", StringComparison.Ordinal) && frontmatterStartLength != 5)
        {
            return null;
        }

        int closingMarker = document.IndexOf("\n---\n", frontmatterStartLength, StringComparison.Ordinal);
        int closingMarkerLength = 5;
        if (closingMarker < 0)
        {
            closingMarker = document.IndexOf("\r\n---\r\n", frontmatterStartLength, StringComparison.Ordinal);
            closingMarkerLength = 7;
        }

        if (closingMarker < 0)
        {
            throw new InvalidDataException($"Memory document '{metadataPath}' has no closing frontmatter marker.");
        }

        string header = document[frontmatterStartLength..closingMarker].Replace("\r\n", "\n", StringComparison.Ordinal);
        string body = document[(closingMarker + closingMarkerLength)..];
        Dictionary<string, string> fields = ParseFields(header);
        if (!Guid.TryParse(GetRequired(fields, "memory_id"), out Guid memoryId) || memoryId == Guid.Empty ||
            !Guid.TryParse(GetRequired(fields, "root_memory_id"), out Guid rootMemoryId) || rootMemoryId == Guid.Empty ||
            !Enum.TryParse(GetRequired(fields, "payload_kind"), ignoreCase: true, out MemoryPayloadKind payloadKind) ||
            !uint.TryParse(GetRequired(fields, "generation"), out uint generation) ||
            !DateTime.TryParse(GetRequired(fields, "created_at_iso"), out DateTime createdAt))
        {
            throw new InvalidDataException($"Memory document '{metadataPath}' has invalid frontmatter.");
        }

        Guid? parentMemoryId = null;
        string parent = fields.GetValueOrDefault("parent_memory_id", "null");
        if (!string.Equals(parent, "null", StringComparison.OrdinalIgnoreCase))
        {
            if (!Guid.TryParse(parent, out Guid parsedParent) || parsedParent == Guid.Empty)
            {
                throw new InvalidDataException($"Memory document '{metadataPath}' has an invalid parent ID.");
            }

            parentMemoryId = parsedParent;
        }

        byte[] payload;
        if (fields.TryGetValue("payload_file", out string? payloadFile))
        {
            if (!string.Equals(payloadFile, Path.GetFileName(payloadFile), StringComparison.Ordinal))
            {
                throw new InvalidDataException($"Memory document '{metadataPath}' contains an invalid payload file name.");
            }

            string payloadPath = Path.Combine(Path.GetDirectoryName(metadataPath) ?? string.Empty, payloadFile);
            payload = await File.ReadAllBytesAsync(payloadPath).ConfigureAwait(false);
        }
        else
        {
            payload = Encoding.UTF8.GetBytes(body);
        }

        ContentHash hash = ContentHash.Compute(payload);
        if (!ContentHash.TryParse(GetRequired(fields, "content_hash"), out ContentHash declaredHash) || hash != declaredHash)
        {
            throw new InvalidDataException($"Memory document '{metadataPath}' failed its payload hash check.");
        }

        return new MemoryRecord
        {
            MemoryId = memoryId,
            Hash = hash,
            PayloadKind = payloadKind,
            Title = fields.GetValueOrDefault("title", string.Empty),
            RawPayload = payload,
            ParentMemoryId = parentMemoryId,
            RootMemoryId = rootMemoryId,
            Generation = generation,
            CreatedAtTicks = createdAt.ToUniversalTime().Ticks,
            UpdatedAtTicks = createdAt.ToUniversalTime().Ticks
        };
    }

    private static Dictionary<string, string> ParseFields(string header)
    {
        var fields = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (string rawLine in header.Split('\n'))
        {
            if (rawLine.StartsWith(' '))
            {
                continue;
            }

            string line = rawLine.Trim();
            int separator = line.IndexOf(':');
            if (separator <= 0 || line.StartsWith("-", StringComparison.Ordinal))
            {
                continue;
            }

            fields[line[..separator].Trim()] = Unquote(line[(separator + 1)..].Trim());
        }

        return fields;
    }

    private static string GetRequired(IReadOnlyDictionary<string, string> fields, string key) =>
        fields.TryGetValue(key, out string? value) && value.Length > 0
            ? value
            : throw new InvalidDataException($"Memory frontmatter is missing '{key}'.");

    private static string Quote(string value) =>
        $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";

    private static string Unquote(string value) =>
        value.Trim().Trim('"').Replace("\\\"", "\"", StringComparison.Ordinal).Replace("\\\\", "\\", StringComparison.Ordinal);
}
