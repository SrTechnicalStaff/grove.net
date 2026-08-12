using System;
using System.Security.Cryptography;
using System.Text;

namespace GroveApp.Models.Memory;

/// <summary>
/// Immutable SHA-256 digest used as the content-addressable identity of a payload.
/// The implementation owns its byte array; callers can inspect a read-only span or
/// request a copy, but cannot mutate the digest through the module's interface.
/// </summary>
public readonly record struct ContentHash : IComparable<ContentHash>
{
    public const int ByteLength = 32;

    private readonly byte[]? _bytes;

    public ReadOnlySpan<byte> Span => _bytes is null ? ReadOnlySpan<byte>.Empty : _bytes;

    public bool IsEmpty => Span.IsEmpty;

    public ReadOnlyMemory<byte> Bytes => Span.ToArray();

    /// <summary>
    /// Computes the SHA-256 digest of raw payload bytes.
    /// </summary>
    public ContentHash(ReadOnlySpan<byte> payloadBytes)
    {
        _bytes = SHA256.HashData(payloadBytes);
    }

    private ContentHash(byte[] digest, bool digestAlreadyComputed)
    {
        if (!digestAlreadyComputed || digest.Length != ByteLength)
        {
            throw new ArgumentException("A content hash must contain exactly 32 digest bytes.", nameof(digest));
        }

        _bytes = digest;
    }

    public static ContentHash Compute(ReadOnlySpan<byte> payloadBytes) => new(payloadBytes);

    public static ContentHash Compute(string textPayload)
    {
        ArgumentNullException.ThrowIfNull(textPayload);
        return new ContentHash(Encoding.UTF8.GetBytes(textPayload));
    }

    public static bool TryParse(string? hex, out ContentHash hash)
    {
        if (hex is null || hex.Length != ByteLength * 2)
        {
            hash = default;
            return false;
        }

        try
        {
            hash = new ContentHash(Convert.FromHexString(hex), digestAlreadyComputed: true);
            return true;
        }
        catch (FormatException)
        {
            hash = default;
            return false;
        }
    }

    public bool Equals(ContentHash other) =>
        CryptographicOperations.FixedTimeEquals(Span, other.Span);

    public override int GetHashCode()
    {
        if (Span.IsEmpty)
        {
            return 0;
        }

        HashCode hash = new();
        for (int i = 0; i < Span.Length; i += sizeof(int))
        {
            int value = 0;
            int bytesToRead = Math.Min(sizeof(int), Span.Length - i);
            for (int j = 0; j < bytesToRead; j++)
            {
                value |= Span[i + j] << (j * 8);
            }

            hash.Add(value);
        }

        return hash.ToHashCode();
    }

    public int CompareTo(ContentHash other) => Span.SequenceCompareTo(other.Span);

    public override string ToString() => Convert.ToHexString(Span).ToLowerInvariant();
}
