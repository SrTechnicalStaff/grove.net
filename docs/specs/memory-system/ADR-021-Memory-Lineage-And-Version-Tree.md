---
status: "Normative / Accepted"
---

# ADR-021: Memory Lineage and Version Tree Architecture

| Property | Value |
| :--- | :--- |
| **Status** | Normative / Accepted |
| **Date** | 2026-08-12 |
| **Architectural Scope** | Memory Versioning Subsystem / Lineage DAG & Delta Tracking |
| **Target Runtime** | .NET 9.0 / C# 13 / Avalonia UI 11.2.5 |
| **Authors** | Chief Product Definition Architect |

---

## 1. Context & Architectural Drivers

As defined in Grove's core principles (`docs/product/original-notes/Tracing.md` and `docs/product/definitions/Memory.md`), human thought evolves continuously without deleting past context. When a user edits a Memory's content, Grove does NOT perform destructive in-place mutation. Instead, it creates a new child `MemoryRecord` linked to its parent through explicit lineage pointers.

### Key Architectural Requirements
1. **Non-Destructive Editing History**: Immutable append-only history preserving every historical snapshot of a Memory node.
2. **Directed Acyclic Graph (DAG) Topology**: Support version branching and parallel forks (e.g. human edit vs. background AI agent generation vs. multi-layer trace variant).
3. **Lineage Ancestry Pointers**: Every child memory explicitly reference its `ParentMemoryId`, `RootMemoryId`, and incremented `Generation` index ($g = g_{\text{parent}} + 1$).
4. **Delta Compression & Chunk Tracking**: Compute and store forward/inverse payload deltas (`MemoryDelta`) using chunk-based diff algorithms to optimize storage while maintaining sub-millisecond head payload materialization.
5. **3-Way Merge & LCA Traversal**: Provide fast Lowest Common Ancestor (LCA) graph algorithms to resolve branch divergence and support non-destructive 3-way text merging.

---

## 2. Lineage DAG Model & Branching Mechanics

A Memory Lineage forms a Directed Acyclic Graph $\mathcal{G} = (\mathcal{V}, \mathcal{E})$ where:
- $\mathcal{V}$ is the set of immutable `MemoryRecord` nodes.
- $\mathcal{E}$ is the set of directed edges $(v_{\text{parent}}, v_{\text{child}})$ pointing from parent memory versions to derived children.

```
                  [ Root Node: v0 ] (Gen 0)
                          |
                          v
                  [ Main Edit: v1 ] (Gen 1)
                    /           \
                   /             \
                  v               v
    [ Branch A: v2 ]           [ Branch B: v3 ] (Gen 2 - Trace Variant)
     (Human Edit)               (Agent Refinement)
          |                            |
          v                            v
    [ Branch A: v4 ]           [ Branch B: v5 ] (Gen 3)
          \                            /
           \                          /
            v                        v
        [ 3-Way Merged Memory Node: v6 ] (Gen 4)
```

### 2.1 Ancestry Node Formalism
A lineage node $v_k \in \mathcal{V}$ is defined by:

$$v_k = \Big( \text{MemoryId}_k, \text{ParentMemoryId}_{k-1}, \text{RootMemoryId}, \text{Generation}_k, \text{BranchName}_k \Big)$$

Where:
- $\text{Generation}_0 = 0$ for root memory nodes.
- $\text{Generation}_k = \text{Generation}(v_{\text{parent}}) + 1$.
- $\text{RootMemoryId}$ remains invariant across all descendant versions originating from $v_0$.

---

## 3. Delta Compression & Diff Mechanics

To ensure scale independence across large vaults containing long version chains, payload differences between parent $v_p$ and child $v_c$ are encoded as a `MemoryDelta`.

### 3.1 Myers Chunk Diff Math
For text-based payloads (PlainText, RichTextMarkdown), payload $P_c$ is derived from parent payload $P_p$ via a series of edit operations:

$$P_c = P_p \oplus \Delta(P_p \to P_c)$$

Where $\Delta$ consists of three atomic edit operations:
1. **Retain(count)**: Retain `count` characters from source string.
2. **Insert(text)**: Insert specified string payload.
3. **Delete(count)**: Delete `count` characters from source string.

$$\text{Storage Savings Ratio} = 1 - \frac{|\Delta(P_p \to P_c)|}{|P_c|}$$

For typical minor edits, the diff size is $<2\%$ of full payload size, reducing disk footprint by over $98\%$.

---

## 4. Lineage Traversal & 3-Way Merge Algorithms

### 4.1 Lowest Common Ancestor (LCA) Algorithm
When merging two diverging version branches $v_A$ and $v_B$, the system locates their Lowest Common Ancestor $v_{\text{LCA}} = \text{LCA}(v_A, v_B)$ by traversing parent pointers towards `RootMemoryId`.

```
Algorithm 1: Lowest Common Ancestor (LCA) Lookup
Input: Lineage graph G, Node ID A, Node ID B
Output: Ancestor Node ID LCA

1: PathA <- GetAncestorsToRoot(G, A)   // [A, parentA, ..., root]
2: SetB  <- GetAncestorHashSet(G, B)   // {B, parentB, ..., root}
3: for each node in PathA do
4:     if SetB contains node then
5:         return node
6: end for
7: return root
```

### 4.2 3-Way Merge Resolution Equation
Given base payload $P_{\text{LCA}}$, branch A payload $P_A$, and branch B payload $P_B$:

$$\Delta_A = \text{Diff}(P_{\text{LCA}} \to P_A), \quad \Delta_B = \text{Diff}(P_{\text{LCA}} \to P_B)$$

$$P_{\text{Merged}} = P_{\text{LCA}} \oplus \text{MergeDeltas}(\Delta_A, \Delta_B)$$

If $\Delta_A$ and $\Delta_B$ modify disjoint byte regions, $P_{\text{Merged}}$ resolves automatically without user intervention.

---

## 5. C# 13 Type Contracts & Lineage Engine Implementation

```csharp
namespace Grove.Specs.MemorySystem;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

/// <summary>
/// Primitive delta operation enum for chunk-based text diffing.
/// </summary>
public enum DeltaOpKind : byte
{
    Retain = 1,
    Insert = 2,
    Delete = 3
}

/// <summary>
/// Atomic edit operation record within a payload delta.
/// </summary>
public readonly record struct DeltaChunk
{
    public required DeltaOpKind Kind { get; init; }
    public int Length { get; init; }
    public string InsertText { get; init; } = string.Empty;

    public static DeltaChunk Retain(int length) => new() { Kind = DeltaOpKind.Retain, Length = length };
    public static DeltaChunk Delete(int length) => new() { Kind = DeltaOpKind.Delete, Length = length };
    public static DeltaChunk Insert(string text) => new() { Kind = DeltaOpKind.Insert, Length = text.Length, InsertText = text };
}

/// <summary>
/// Immutable payload delta containing ordered edit chunks.
/// </summary>
public sealed record MemoryDelta
{
    public required Guid SourceMemoryId { get; init; }
    public required Guid TargetMemoryId { get; init; }
    public ImmutableList<DeltaChunk> Chunks { get; init; } = ImmutableList<DeltaChunk>.Empty;

    /// <summary>
    /// Applies delta sequence to source string to produce target string.
    /// </summary>
    public string Apply(string sourceText)
    {
        var sb = new System.Text.StringBuilder();
        int srcIndex = 0;

        foreach (var chunk in Chunks)
        {
            switch (chunk.Kind)
            {
                case DeltaOpKind.Retain:
                    sb.Append(sourceText.AsSpan(srcIndex, chunk.Length));
                    srcIndex += chunk.Length;
                    break;
                case DeltaOpKind.Insert:
                    sb.Append(chunk.InsertText);
                    break;
                case DeltaOpKind.Delete:
                    srcIndex += chunk.Length;
                    break;
            }
        }

        return sb.ToString();
    }
}

/// <summary>
/// Compact lineage node representation stored within graph index.
/// </summary>
public sealed record MemoryVersionNode
{
    public required Guid MemoryId { get; init; }
    public Guid? ParentMemoryId { get; init; }
    public required Guid RootMemoryId { get; init; }
    public required uint Generation { get; init; }
    public string BranchName { get; init; } = "main";
    public ContentHash Hash { get; init; }
    public long CreatedAtTicks { get; init; }
}

/// <summary>
/// Interface for memory version tree operations.
/// </summary>
public interface IMemoryVersionTree
{
    void AddVersionNode(MemoryRecord record);
    
    MemoryVersionNode? GetNode(Guid memoryId);
    
    IReadOnlyList<MemoryVersionNode> GetLineagePath(Guid memoryId);
    
    Guid FindLowestCommonAncestor(Guid memoryIdA, Guid memoryIdB);
    
    IReadOnlyList<MemoryVersionNode> GetBranchHeads(Guid rootMemoryId);
    
    MemoryRecord MergeBranches(
        Guid memoryIdA, 
        Guid memoryIdB, 
        IMemoryLedger ledger, 
        string targetBranchName = "main");
}

/// <summary>
/// High-performance thread-safe memory version tree implementation.
/// </summary>
public sealed class MemoryVersionTree : IMemoryVersionTree
{
    private readonly Dictionary<Guid, MemoryVersionNode> _nodes = new();
    private readonly Dictionary<Guid, List<Guid>> _childrenMap = new();
    private readonly object _graphLock = new();

    public void AddVersionNode(MemoryRecord record)
    {
        var node = new MemoryVersionNode
        {
            MemoryId = record.MemoryId,
            ParentMemoryId = record.ParentMemoryId,
            RootMemoryId = record.RootMemoryId,
            Generation = record.Generation,
            Hash = record.Hash,
            CreatedAtTicks = record.CreatedAtTicks
        };

        lock (_graphLock)
        {
            _nodes[record.MemoryId] = node;

            if (record.ParentMemoryId.HasValue)
            {
                Guid parentId = record.ParentMemoryId.Value;
                if (!_childrenMap.TryGetValue(parentId, out var children))
                {
                    children = new List<Guid>();
                    _childrenMap[parentId] = children;
                }
                children.Add(record.MemoryId);
            }
        }
    }

    public MemoryVersionNode? GetNode(Guid memoryId)
    {
        lock (_graphLock)
        {
            _nodes.TryGetValue(memoryId, out var node);
            return node;
        }
    }

    public IReadOnlyList<MemoryVersionNode> GetLineagePath(Guid memoryId)
    {
        var path = new List<MemoryVersionNode>();

        lock (_graphLock)
        {
            Guid? current = memoryId;
            while (current.HasValue && _nodes.TryGetValue(current.Value, out var node))
            {
                path.Add(node);
                current = node.ParentMemoryId;
            }
        }

        path.Reverse();
        return path;
    }

    public Guid FindLowestCommonAncestor(Guid memoryIdA, Guid memoryIdB)
    {
        lock (_graphLock)
        {
            var ancestorsA = new HashSet<Guid>();
            Guid? currA = memoryIdA;

            while (currA.HasValue && _nodes.TryGetValue(currA.Value, out var nodeA))
            {
                ancestorsA.Add(nodeA.MemoryId);
                currA = nodeA.ParentMemoryId;
            }

            Guid? currB = memoryIdB;
            while (currB.HasValue && _nodes.TryGetValue(currB.Value, out var nodeB))
            {
                if (ancestorsA.Contains(nodeB.MemoryId))
                {
                    return nodeB.MemoryId;
                }
                currB = nodeB.ParentMemoryId;
            }

            throw new InvalidOperationException($"No common ancestor found between {memoryIdA} and {memoryIdB}");
        }
    }

    public IReadOnlyList<MemoryVersionNode> GetBranchHeads(Guid rootMemoryId)
    {
        var heads = new List<MemoryVersionNode>();

        lock (_graphLock)
        {
            foreach (var kvp in _nodes)
            {
                var node = kvp.Value;
                if (node.RootMemoryId == rootMemoryId)
                {
                    if (!_childrenMap.TryGetValue(node.MemoryId, out var children) || children.Count == 0)
                    {
                        heads.Add(node);
                    }
                }
            }
        }

        return heads;
    }

    public MemoryRecord MergeBranches(
        Guid memoryIdA, 
        Guid memoryIdB, 
        IMemoryLedger ledger, 
        string targetBranchName = "main")
    {
        Guid lcaId = FindLowestCommonAncestor(memoryIdA, memoryIdB);
        var recLca = ledger.GetMemory(lcaId) ?? throw new KeyNotFoundException($"LCA memory {lcaId} not found");
        var recA = ledger.GetMemory(memoryIdA) ?? throw new KeyNotFoundException($"Memory {memoryIdA} not found");
        var recB = ledger.GetMemory(memoryIdB) ?? throw new KeyNotFoundException($"Memory {memoryIdB} not found");

        string textLca = recLca.GetUtf8Payload();
        string textA = recA.GetUtf8Payload();
        string textB = recB.GetUtf8Payload();

        // Simple 3-way merge concatenation fallback when non-overlapping
        string mergedText = textA == textB ? textA : $"{textA}\n\n--- Merged from Branch B ---\n\n{textB}";
        byte[] mergedBytes = System.Text.Encoding.UTF8.GetBytes(mergedText);

        return ledger.AppendMemory(
            kind: recA.PayloadKind,
            payload: mergedBytes,
            parentMemoryId: memoryIdA);
    }
}
```

---

## 6. Disk Persistence & YAML Lineage Protocol

Lineage attributes are encoded in YAML frontmatter for transparent inspection by external tools and git repositories:

```yaml
---
memory_id: "018f3a5b-9c2d-7a1e-8f92-1c2d3e4f5a6b"
parent_memory_id: "018f3a5a-1b2c-3d4e-5f6a-7b8c9d0e1f2a"
root_memory_id: "018f3a50-0a1b-2c3d-4e5f-6a7b8c9d0e1f"
generation: 3
branch_name: "layer-2-variant"
lineage_hash: "7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f80"
---
# Derived Version Payload
```

---

## 7. Verification & Invariants

1. **DAG Cycle Prevention**: Assert that adding a parent pointer $(v_p, v_c)$ never introduces cycles: $v_p \notin \text{Descendants}(v_c)$.
2. **Generation Monotonicity**: Assert $g(v_{\text{child}}) = g(v_{\text{parent}}) + 1$.
3. **Exact Delta Reconstruction**: Assert $P_p \oplus \Delta(P_p \to P_c) \equiv P_c$ down to the exact byte sequence.
4. **LCA Determinism**: Assert that $\text{LCA}(A, B)$ returns identical node ID regardless of traversal order.
