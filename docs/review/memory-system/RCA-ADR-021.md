# RCA Ledger: ADR-021 — Memory Lineage and Version Tree Architecture

## 1. Executive Metadata

| Metadata Field | Value |
| :--- | :--- |
| **ADR ID** | ADR-021 |
| **ADR Title** | Memory Lineage and Version Tree Architecture |
| **Category** | Memory System (`docs/specs/memory-system/`) |
| **Claimed Status in Spec Header** | Normative / Accepted |
| **Verified Status (User-Observable)** | **0% Implemented (NOT IMPLEMENTED / NON-FUNCTIONAL IN LIVE UI)** |
| **Audit Date** | 2026-08-12 |
| **Target Runtime** | .NET 9.0 / C# 13 / Avalonia UI 11.2.5 |
| **Auditor** | Principal AI Systems Architect & Product Auditor |

---

## 2. Normative Specification Requirement Inventory

| ID | Requirement Category | Symbol / Contract Name | Normative Specification Requirement |
| :--- | :--- | :--- | :--- |
| **REQ-021-01** | DAG Model | Lineage DAG $\mathcal{G} = (\mathcal{V}, \mathcal{E})$ | Directed Acyclic Graph topology preserving non-destructive history. Every child memory node explicitly references `ParentMemoryId`, `RootMemoryId`, and incremented `Generation` index ($g = g_{\text{parent}} + 1$). |
| **REQ-021-02** | Atomic Edit Enum | `DeltaOpKind` Enum | Byte enum for chunk-based diffing (`Retain = 1`, `Insert = 2`, `Delete = 3`). |
| **REQ-021-03** | Edit Chunk Struct | `DeltaChunk` Struct | Atomic edit operation record struct containing `Kind` (`DeltaOpKind`), `Length` (int), `InsertText` (string), and factory methods (`Retain`, `Delete`, `Insert`). |
| **REQ-021-04** | Delta Storage Record | `MemoryDelta` Record | Sealed record containing `SourceMemoryId` (Guid), `TargetMemoryId` (Guid), `Chunks` (`ImmutableList<DeltaChunk>`), and `Apply(string sourceText)` string reconstruction method. |
| **REQ-021-05** | Graph Node Record | `MemoryVersionNode` Record | Compact lineage node struct containing `MemoryId`, `ParentMemoryId`, `RootMemoryId`, `Generation`, `BranchName`, `Hash`, `CreatedAtTicks`. |
| **REQ-021-06** | Interface Contract | `IMemoryVersionTree` Interface | Interface declaring `AddVersionNode`, `GetNode`, `GetLineagePath`, `FindLowestCommonAncestor`, `GetBranchHeads`, `MergeBranches`. |
| **REQ-021-07** | Engine Implementation | `MemoryVersionTree` Class | Thread-safe lineage graph implementation using `Dictionary<Guid, MemoryVersionNode>`, `Dictionary<Guid, List<Guid>>` children map, and `_graphLock`. |
| **REQ-021-08** | Traversal Algorithm | Lowest Common Ancestor (LCA) | Fast graph traversal algorithm finding $v_{\text{LCA}} = \text{LCA}(v_A, v_B)$ by intersecting ancestor set paths up to `RootMemoryId`. |
| **REQ-021-09** | Text Merge Equation | 3-Way Merge Resolution | Automatic resolving of non-overlapping branch edits using base payload $P_{\text{LCA}}$: $P_{\text{Merged}} = P_{\text{LCA}} \oplus \text{MergeDeltas}(\Delta_A, \Delta_B)$. |
| **REQ-021-10** | Disk Protocol | Lineage YAML Protocol | Serialization of lineage metadata into YAML frontmatter (`parent_memory_id`, `root_memory_id`, `generation`, `branch_name`, `lineage_hash`). |

---

## 3. Codebase Reality & Line-by-Line Evidence

### 3.1 Symbol Existence & Implementation Audit

A comprehensive search across [`src/GroveApp/`](file:///C:/dev/grove-v9/src/GroveApp/) confirms that zero classes, structs, or algorithms from ADR-021 exist in the codebase:

| Symbol / Class Name | Expected Location | Actual Status in `src/GroveApp/` | Line-by-Line Evidence |
| :--- | :--- | :--- | :--- |
| `DeltaOpKind` | `src/GroveApp/Models/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. |
| `DeltaChunk` | `src/GroveApp/Models/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. |
| `MemoryDelta` | `src/GroveApp/Models/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. No chunk diffing or delta compression exists. |
| `MemoryVersionNode` | `src/GroveApp/Models/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. |
| `IMemoryVersionTree` | `src/GroveApp/Engine/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. No version tree interface exists. |
| `MemoryVersionTree` | `src/GroveApp/Engine/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. No version tree DAG implementation exists. |
| `FindLowestCommonAncestor` | `src/GroveApp/Engine/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. No LCA graph algorithm is implemented. |
| `MergeBranches` | `src/GroveApp/Engine/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. 3-way branch merging is absent. |

---

## 4. Standards & Visual Plane Seam Audit

### 4.1 Visual Plane Separation (Plane 0 vs Layer 1 vs Plane 2)
- **Plane 0 (Spatial Grid Canvas)**: Placed content items have no concept of parent/child lineage. Editing a note on Plane 0 replaces the text string directly without spawning version nodes.
- **Layer 1 (Information Layer Overlays)**: [`LocalEditorOverlay.axaml.cs`](file:///C:/dev/grove-v9/src/GroveApp/Controls/LocalEditorOverlay.axaml.cs#L140) handles text editing without versioning, branching, or diff tracking.
- **Plane 2 (HUD Slate Plane)**: No lineage DAG visualizer or version history tree control exists on Plane 2.

### 4.2 Code Smells & Architectural Violations
1. **Destructive In-Place Overwrites**: Every user edit destroys the previous text state, violating Grove's foundational principle of continuous non-destructive thought preservation (`docs/product/original-notes/Tracing.md`).
2. **Absence of Delta Compression**: Storage footprint relies on full string buffers with zero Myers diff compression.

---

## 5. Root Cause Analysis (RCA)

### 5.1 Primary Root Cause
The lineage DAG and version tree architecture defined in ADR-021 was deferred during initial prototype construction. The development focused exclusively on basic text entry inside `LocalEditorOverlay.axaml.cs` without implementing the underlying `MemoryVersionTree` engine.

### 5.2 Failure Chain
1. **Unimplemented Subsystem**: The `IMemoryVersionTree` engine was never instantiated in `src/GroveApp/Engine/`.
2. **Missing History Hooks**: `LocalEditorOverlay` was wired directly to string properties on UI models rather than dispatching version branch events to a lineage engine.
