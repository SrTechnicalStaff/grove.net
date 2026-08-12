---
status: "Normative / Accepted"
---

# ADR-020: Memory Model and Immutable Ledger Architecture

| Property | Value |
| :--- | :--- |
| **Status** | Normative / Accepted |
| **Date** | 2026-08-12 |
| **Architectural Scope** | Core Memory Subsystem / Immutable Ledger & Payload Store |
| **Target Runtime** | .NET 9.0 / C# 13 / Avalonia UI 11.2.5 |
| **Authors** | Chief Product Definition Architect |

---

## 1. Context & Architectural Drivers

In Grove v9, a **Memory** is the foundational semantic record and substrate of human thought. As specified in product definitions (`docs/product/definitions/Memory.md`), a Memory is an immutable core unit of information (text, rich text, binary image, or reference) that exists independently of any single spatial placement or layer binding.

### Key Architectural Requirements
1. **Semantic Singularity**: Decouple raw semantic information from spatial location. A Memory retains identity across space and time without forcing data duplication when instantiated across multiple layers.
2. **Payload Immutability**: Memory payloads are append-only and strictly immutable once created. Any modification produces a new version node within the memory lineage tree (see [ADR-021](../memory-system/ADR-021-Memory-Lineage-And-Version-Tree.md)).
3. **Cryptographic Identity & Content Addressability**: Utilize time-ordered UUIDv7 identifiers for monotonic indexing and SHA-256 content digests for payload integrity verification and deduplication.
4. **Anchor Multiplicity**: A single Memory record can bind to $N \ge 0$ spatial anchors across multiple layers ($M \ge 1$), enabling cross-layer Tracing (`Trace of` / `Traces`) with layer-specific contextual metadata.
5. **Disk Provenance Synchronization**: Local storage on disk (Markdown files with YAML frontmatter or media sidecar files) must synchronously reflect identity, SHA-256 payload hash, and anchor metadata without corrupting underlying semantic content.

---

## 2. Cryptographic Identity & Payload Integrity

### 2.1 UUIDv7 Monotonic Identity Architecture
All Memory entities, anchors, and traces are assigned a 128-bit UUIDv7 identifier. UUIDv7 provides time-ordered monotonicity, optimizing spatial R-Tree indices, database B-Trees, and cache alignment in .NET 9.

$$\text{UUIDv7 Layout} = \underbrace{\text{UnixTimestamp}_{\text{ms}}}_{48 \text{ bits}} \,||\, \underbrace{\text{Ver (0111)}}_{4 \text{ bits}} \,||\, \underbrace{\text{Sequence / Rand}}_{\text{12 bits}} \,||\, \underbrace{\text{Var (10)}}_{2 \text{ bits}} \,||\, \underbrace{\text{Rand}}_{62 \text{ bits}}$$

- **Bits 0–47**: Big-endian 48-bit unsigned integer representing Unix epoch timestamp in milliseconds.
- **Bits 48–51**: Version bitmask `0b0111` (7).
- **Bits 52–63**: 12-bit sequence counter for sub-millisecond monotonicity or pseudo-random entropy.
- **Bits 64–65**: Variant bitmask `0b10` (RFC 4122 / 9562).
- **Bits 66–127**: 62 bits of strong pseudo-random entropy generated via `RandomNumberGenerator`.

### 2.2 SHA-256 Payload Content Hashing
Every `MemoryRecord` contains a 256-bit SHA-256 cryptographic digest of its raw byte payload:

$$\text{ContentHash} = \text{SHA256}(\text{RawPayloadBytes})$$

The content hash guarantees:
- **Deduplication Verification**: Detection of duplicate payloads across imports or background agent operations.
- **Tamper Evident Integrity**: On-disk Markdown files edited outside Grove are validated against `ContentHash`. If payload bytes drift, a lineage delta node or external mutation event is raised.
- **Content Addressability**: Secondary lookup indices can locate memories directly by byte digest.

---

## 3. Anchor Multiplicity & Spatial Placement Mapping

A single `MemoryRecord` can be placed onto the Grid plane across multiple layers via **Anchors**. An anchor represents the spatial manifestation of a Memory on a specific layer at explicit cell coordinates.

```
+-----------------------------------------------------------------------+
|                             MemoryRecord                              |
|  MemoryId: 00f074d2-7c1a-7b3e-8f92-1c2d3e4f5a6b                       |
|  ContentHash: e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b |
|  RawPayload: "Core product vision note..."                             |
+-----------------------------------------------------------------------+
        |                                       |
        | Anchor 1 (Layer 1)                    | Anchor 2 (Layer 2 - Trace)
        v                                       v
+-----------------------------+       +-----------------------------+
|        MemoryAnchor         |       |        MemoryAnchor         |
| AnchorId: 018f...           |       | AnchorId: 018f...           |
| LayerId: Layer-Composition  |       | LayerId: Layer-Perspective  |
| GridPosition: (10, 15)      |       | GridPosition: (10, 15)      |
| ContextLabel: "Primary"     |       | ContextLabel: "Refined"     |
+-----------------------------+       +-----------------------------+
```

### 3.1 Anchor Mathematical Formalism
Let $M$ be a Memory Record. The anchor mapping set $\mathcal{A}(M)$ is defined as:

$$\mathcal{A}(M) = \{ A_1, A_2, \dots, A_n \}, \quad n \ge 0$$

Where each anchor $A_k$ is a tuple:

$$A_k = \Big( \text{AnchorId}_k, \text{MemoryId}, \text{LayerId}_k, (x_k, y_k), \text{Label}_k, \text{Metadata}_k \Big)$$

- $(x_k, y_k) \in \mathbb{Z}^2$: Discrete cell coordinates on the Spatial Grid.
- $\text{LayerId}_k \in \text{UUID}$: Target spatial layer identifier.
- $\text{Label}_k \in \text{String}$: Optional layer-specific contextual label (e.g. "Composition Perspective", "Source Reference").

---

## 4. C# 13 Type Definitions & Immutable Ledger Contracts

The memory subsystem is implemented in C# 13 using value-type record structs, zero-allocation memory spans, and immutable record collections.

```csharp
namespace Grove.Specs.MemorySystem;

using System;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Immutable 256-bit SHA-256 content hash struct with zero-allocation span comparisons.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 32)]
public readonly record struct ContentHash : IComparable<ContentHash>
{
    private readonly Memory<byte> _hashBytes;

    public ReadOnlySpan<byte> Span => _hashBytes.Span;

    public ContentHash(ReadOnlySpan<byte> payloadBytes)
    {
        byte[] hash = SHA256.HashData(payloadBytes);
        _hashBytes = hash;
    }

    public static ContentHash Compute(string textPayload)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(textPayload);
        return new ContentHash(bytes);
    }

    public bool Equals(ContentHash other) => Span.SequenceEqual(other.Span);

    public override int GetHashCode()
    {
        if (_hashBytes.IsEmpty) return 0;
        return BitConverter.ToInt32(_hashBytes.Span[..4]);
    }

    public int CompareTo(ContentHash other) => Span.SequenceCompareTo(other.Span);

    public override string ToString() => Convert.ToHexStringLower(Span);
}

/// <summary>
/// Primitive enum representing memory raw payload formats.
/// </summary>
public enum MemoryPayloadKind : byte
{
    PlainText = 1,
    RichTextMarkdown = 2,
    BinaryImage = 3,
    PDFDocument = 4,
    StructuredJSON = 5
}

/// <summary>
/// Immutable spatial anchor record binding a Memory to grid cell coordinates on a specific layer.
/// </summary>
public readonly record struct MemoryAnchor
{
    public required Guid AnchorId { get; init; }
    public required Guid MemoryId { get; init; }
    public required Guid LayerId { get; init; }
    public required int CellX { get; init; }
    public required int CellY { get; init; }
    public string ContextLabel { get; init; } = string.Empty;
    public ImmutableDictionary<string, string> ExtendedFrontmatter { get; init; } = ImmutableDictionary<string, string>.Empty;
    public required long CreatedAtTicks { get; init; }

    public MemoryAnchor() { }
}

/// <summary>
/// Primary immutable memory record representing an unalterable node of semantic thought.
/// </summary>
public sealed record MemoryRecord
{
    public required Guid MemoryId { get; init; }
    public required ContentHash Hash { get; init; }
    public required MemoryPayloadKind PayloadKind { get; init; }
    public required ReadOnlyMemory<byte> RawPayload { get; init; }
    public Guid? ParentMemoryId { get; init; }
    public Guid RootMemoryId { get; init; }
    public uint Generation { get; init; } = 0;
    public ImmutableList<MemoryAnchor> Anchors { get; init; } = ImmutableList<MemoryAnchor>.Empty;
    public required long CreatedAtTicks { get; init; }
    public required long UpdatedAtTicks { get; init; }

    /// <summary>
    /// Gets payload decoded as UTF-8 string (valid for PlainText and RichTextMarkdown).
    /// </summary>
    public string GetUtf8Payload() => Encoding.UTF8.GetString(RawPayload.Span);
}

/// <summary>
/// Interface for thread-safe append-only memory ledger repository.
/// </summary>
public interface IMemoryLedger
{
    MemoryRecord AppendMemory(
        MemoryPayloadKind kind, 
        ReadOnlySpan<byte> payload, 
        Guid? parentMemoryId = null, 
        ImmutableList<MemoryAnchor>? initialAnchors = null);

    MemoryRecord? GetMemory(Guid memoryId);
    
    bool TryGetMemoryByHash(ContentHash hash, out MemoryRecord? record);
    
    MemoryRecord AddAnchor(Guid memoryId, MemoryAnchor anchor);
    
    MemoryRecord RemoveAnchor(Guid memoryId, Guid anchorId);
    
    ReadOnlySpan<MemoryRecord> GetAllMemories();
}
```

---

## 5. Disk Persistence & YAML Frontmatter Synchronization

Grove stores semantic memory records as Markdown files with YAML frontmatter headers (or `.meta.json` sidecars for binary media assets) stored within local vault directories.

### 5.1 YAML Frontmatter Specification
When writing a `MemoryRecord` to a Markdown document (`.md`), frontmatter metadata MUST adhere strictly to the following canonical format:

```yaml
---
memory_id: "018f3a5b-9c2d-7a1e-8f92-1c2d3e4f5a6b"
content_hash: "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b"
payload_kind: "RichTextMarkdown"
created_at_iso: "2026-08-12T08:50:00.0000000Z"
parent_memory_id: null
root_memory_id: "018f3a5b-9c2d-7a1e-8f92-1c2d3e4f5a6b"
generation: 0
anchors:
  - anchor_id: "018f3a5b-9c2d-7b4f-9e11-2a3b4c5d6e7f"
    layer_id: "4a8c1f2e-3d4b-5c6a-7b8c-9d0e1f2a3b4c"
    cell_x: 12
    cell_y: -4
    label: "Composition"
  - anchor_id: "018f3a5b-9c2d-7c50-af22-3b4c5d6e7f80"
    layer_id: "8f9a0b1c-2d3e-4f5a-6b7c-8d9e0f1a2b3c"
    cell_x: 12
    cell_y: -4
    label: "Perspective (Trace)"
---
# Memory Title or Body Content

Raw memory payload content starts here. Edits to this body trigger SHA-256 recalculation and version tree branching.
```

---

## 6. Implementation Engine Architecture

```csharp
namespace Grove.Specs.MemorySystem;

using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Collections.Generic;

/// <summary>
/// Thread-safe append-only memory ledger implementation using lock-free concurrent dictionaries.
/// </summary>
public sealed class ImmutableMemoryLedger : IMemoryLedger
{
    private readonly ConcurrentDictionary<Guid, MemoryRecord> _recordsById = new();
    private readonly ConcurrentDictionary<ContentHash, Guid> _idByHash = new();
    private readonly object _writeLock = new();

    public MemoryRecord AppendMemory(
        MemoryPayloadKind kind, 
        ReadOnlySpan<byte> payload, 
        Guid? parentMemoryId = null, 
        ImmutableList<MemoryAnchor>? initialAnchors = null)
    {
        byte[] payloadArray = payload.ToArray();
        ContentHash hash = new(payloadArray);
        long nowTicks = DateTime.UtcNow.Ticks;
        Guid memoryId = Guid.CreateVersion7();

        Guid rootId = memoryId;
        uint gen = 0;

        if (parentMemoryId.HasValue && _recordsById.TryGetValue(parentMemoryId.Value, out var parentRecord))
        {
            rootId = parentRecord.RootMemoryId;
            gen = parentRecord.Generation + 1;
        }

        var record = new MemoryRecord
        {
            MemoryId = memoryId,
            Hash = hash,
            PayloadKind = kind,
            RawPayload = payloadArray,
            ParentMemoryId = parentMemoryId,
            RootMemoryId = rootId,
            Generation = gen,
            Anchors = initialAnchors ?? ImmutableList<MemoryAnchor>.Empty,
            CreatedAtTicks = nowTicks,
            UpdatedAtTicks = nowTicks
        };

        lock (_writeLock)
        {
            if (!_recordsById.TryAdd(memoryId, record))
            {
                throw new InvalidOperationException($"Duplicate MemoryId collision: {memoryId}");
            }
            _idByHash[hash] = memoryId;
        }

        return record;
    }

    public MemoryRecord? GetMemory(Guid memoryId)
    {
        _recordsById.TryGetValue(memoryId, out var record);
        return record;
    }

    public bool TryGetMemoryByHash(ContentHash hash, out MemoryRecord? record)
    {
        if (_idByHash.TryGetValue(hash, out Guid id))
        {
            return _recordsById.TryGetValue(id, out record);
        }
        record = null;
        return false;
    }

    public MemoryRecord AddAnchor(Guid memoryId, MemoryAnchor anchor)
    {
        lock (_writeLock)
        {
            if (!_recordsById.TryGetValue(memoryId, out var existing))
            {
                throw new KeyNotFoundException($"Memory {memoryId} not found in ledger.");
            }

            var updatedAnchors = existing.Anchors.Add(anchor);
            var updatedRecord = existing with 
            { 
                Anchors = updatedAnchors, 
                UpdatedAtTicks = DateTime.UtcNow.Ticks 
            };

            _recordsById[memoryId] = updatedRecord;
            return updatedRecord;
        }
    }

    public MemoryRecord RemoveAnchor(Guid memoryId, Guid anchorId)
    {
        lock (_writeLock)
        {
            if (!_recordsById.TryGetValue(memoryId, out var existing))
            {
                throw new KeyNotFoundException($"Memory {memoryId} not found in ledger.");
            }

            var updatedAnchors = existing.Anchors.RemoveAll(a => a.AnchorId == anchorId);
            var updatedRecord = existing with 
            { 
                Anchors = updatedAnchors, 
                UpdatedAtTicks = DateTime.UtcNow.Ticks 
            };

            _recordsById[memoryId] = updatedRecord;
            return updatedRecord;
        }
    }

    public ReadOnlySpan<MemoryRecord> GetAllMemories()
    {
        var values = _recordsById.Values;
        MemoryRecord[] array = new MemoryRecord[values.Count];
        values.CopyTo(array, 0);
        return array;
    }
}
```

---

## 7. Verification & Invariants

1. **UUIDv7 Time Ordering**: Verified through unit tests asserting that sequential memory allocations satisfy $M_{k}.\text{MemoryId} > M_{k-1}.\text{MemoryId}$ under lexicographical bitwise comparison.
2. **Payload Hash Integrity**: Any write or external disk read must compute $\text{SHA256}(\text{RawPayload})$ and assert equivalence to `ContentHash`. Discrepancies raise a `PayloadIntegrityException`.
3. **Immutability Invariant**: Updating a Memory payload ALWAYS creates a child record via `AppendMemory(..., parentMemoryId: currentId)` rather than mutating `RawPayload` in place.
4. **Anchor Discretization**: Anchor `CellX` and `CellY` MUST be integer cell values corresponding to spatial grid cells.
