using System;
using System.Collections.Generic;
using System.Text;

namespace GroveApp.Models.Memory;

/// <summary>
/// Immutable semantic memory node. Payload changes are represented by a new record
/// linked through <see cref="ParentMemoryId"/>. Content-side anchor metadata is
/// stored through a separate relation and never becomes Memory ownership.
/// </summary>
public sealed record MemoryRecord
{
    private byte[] _payload = Array.Empty<byte>();

    public required Guid MemoryId { get; init; }
    public required ContentHash Hash { get; init; }
    public required MemoryPayloadKind PayloadKind { get; init; }
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// The init accessor copies incoming memory so the record never aliases a caller-owned array.
    /// </summary>
    public required ReadOnlyMemory<byte> RawPayload
    {
        get => _payload;
        init => _payload = value.ToArray();
    }

    public Guid? ParentMemoryId { get; init; }
    public required Guid RootMemoryId { get; init; }
    public uint Generation { get; init; }
    public required long CreatedAtTicks { get; init; }
    public required long UpdatedAtTicks { get; init; }

    public string GetUtf8Payload() => Encoding.UTF8.GetString(RawPayload.Span);

    public byte[] GetPayloadCopy() => RawPayload.ToArray();

    public bool HasTextPayload =>
        PayloadKind is MemoryPayloadKind.PlainText or MemoryPayloadKind.RichTextMarkdown;

    public bool HasValidHash() => Hash.Equals(new ContentHash(RawPayload.Span));

    public static MemoryRecord Create(
        MemoryPayloadKind payloadKind,
        ReadOnlySpan<byte> payload,
        Guid? parentMemoryId,
        Guid rootMemoryId,
        uint generation,
        string? title = null,
        Guid? memoryId = null,
        long? nowTicks = null)
    {
        byte[] payloadCopy = payload.ToArray();
        long timestamp = nowTicks ?? DateTime.UtcNow.Ticks;

        return new MemoryRecord
        {
            MemoryId = memoryId ?? Guid.CreateVersion7(),
            Hash = new ContentHash(payloadCopy),
            PayloadKind = payloadKind,
            Title = title?.Trim() ?? string.Empty,
            RawPayload = payloadCopy,
            ParentMemoryId = parentMemoryId,
            RootMemoryId = rootMemoryId,
            Generation = generation,
            CreatedAtTicks = timestamp,
            UpdatedAtTicks = timestamp
        };
    }

}
