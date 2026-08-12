# RCA Ledger: ADR-020 — Memory Model and Immutable Ledger Architecture

## 1. Executive Metadata

| Metadata Field | Value |
| :--- | :--- |
| **ADR ID** | ADR-020 |
| **ADR Title** | Memory Model and Immutable Ledger Architecture |
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
| **REQ-020-01** | Cryptographic Identity | `UUIDv7` Layout | 128-bit time-ordered monotonic UUIDv7 identifiers ($48\text{bit timestamp} \mid\mid 4\text{bit version 7} \mid\mid 12\text{bit seq} \mid\mid 2\text{bit var} \mid\mid 62\text{bit rand}$) generated via `Guid.CreateVersion7()`. |
| **REQ-020-02** | Payload Hashing | `ContentHash` Struct | 256-bit SHA-256 cryptographic digest struct (`System.Security.Cryptography.SHA256`) with zero-allocation `ReadOnlySpan<byte>` comparison methods (`Equals`, `CompareTo`, `GetHashCode`). |
| **REQ-020-03** | Enum Type Contract | `MemoryPayloadKind` Enum | Primitive byte enum (`PlainText = 1`, `RichTextMarkdown = 2`, `BinaryImage = 3`, `PDFDocument = 4`, `StructuredJSON = 5`). |
| **REQ-020-04** | Struct Type Contract | `MemoryAnchor` Struct | Immutable record struct containing `AnchorId` (Guid), `MemoryId` (Guid), `LayerId` (Guid), `CellX` (int), `CellY` (int), `ContextLabel` (string), `ExtendedFrontmatter` (`ImmutableDictionary<string, string>`), `CreatedAtTicks` (long). |
| **REQ-020-05** | Record Type Contract | `MemoryRecord` Record | Sealed record containing `MemoryId`, `Hash` (`ContentHash`), `PayloadKind`, `RawPayload` (`ReadOnlyMemory<byte>`), `ParentMemoryId`, `RootMemoryId`, `Generation`, `Anchors` (`ImmutableList<MemoryAnchor>`), `GetUtf8Payload()`. |
| **REQ-020-06** | Interface Contract | `IMemoryLedger` Interface | Thread-safe append-only ledger interface declaring `AppendMemory`, `GetMemory`, `TryGetMemoryByHash`, `AddAnchor`, `RemoveAnchor`, `GetAllMemories`. |
| **REQ-020-07** | Engine Implementation | `ImmutableMemoryLedger` Class | Thread-safe append-only memory ledger implementation using `ConcurrentDictionary<Guid, MemoryRecord>` and `_writeLock` synchronization object. |
| **REQ-020-08** | Multi-Layer Anchoring | Anchor Set $\mathcal{A}(M)$ | Support binding a single `MemoryRecord` to $N \ge 0$ spatial anchors across multiple layers ($M \ge 1$) with layer-specific contextual labels (e.g. "Composition", "Perspective"). |
| **REQ-020-09** | Disk Persistence | YAML Frontmatter Header | Synchronous disk serialization to Markdown `.md` files featuring strict YAML frontmatter (`memory_id`, `content_hash`, `payload_kind`, `created_at_iso`, `parent_memory_id`, `root_memory_id`, `generation`, `anchors`). |
| **REQ-020-10** | Immutability Invariant | Append-Only Enforcement | Content modification MUST NEVER mutate `RawPayload` in place; any payload edit MUST spawn a new child `MemoryRecord` via `AppendMemory`. |

---

## 3. Codebase Reality & Line-by-Line Evidence

### 3.1 Symbol Existence & Implementation Audit

A comprehensive grep audit across all C# and XAML files in [`src/GroveApp/`](file:///C:/dev/grove-v9/src/GroveApp/) yields zero matches for the normative memory models specified in ADR-020:

| Symbol / Class Name | Expected Location | Actual Status in `src/GroveApp/` | Line-by-Line Evidence |
| :--- | :--- | :--- | :--- |
| `ContentHash` | `src/GroveApp/Models/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. Raw byte SHA-256 computation is completely absent. |
| `MemoryPayloadKind` | `src/GroveApp/Models/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. Model classes use UI enums like `ArmableContentType` ([`ToolArmingStateMachine.cs:L6`](file:///C:/dev/grove-v9/src/GroveApp/Engine/ToolArmingStateMachine.cs#L6)). |
| `MemoryAnchor` | `src/GroveApp/Models/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. Anchoring is represented only as an in-memory boolean `IsAnchored` flag on UI items ([`GridContentItem.cs:L15`](file:///C:/dev/grove-v9/src/GroveApp/Models/GridContentItem.cs#L15)). |
| `MemoryRecord` | `src/GroveApp/Models/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. Placements use mutable UI models ([`GridNote.cs`](file:///C:/dev/grove-v9/src/GroveApp/Models/GridNote.cs), [`GridDocument.cs`](file:///C:/dev/grove-v9/src/GroveApp/Models/GridDocument.cs), [`GridImage.cs`](file:///C:/dev/grove-v9/src/GroveApp/Models/GridImage.cs)). |
| `IMemoryLedger` | `src/GroveApp/Engine/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. No append-only ledger repository interface exists. |
| `ImmutableMemoryLedger` | `src/GroveApp/Engine/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. No thread-safe memory ledger implementation exists. |
| `Guid.CreateVersion7()` | `src/GroveApp/Engine/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. All entities call `Guid.NewGuid().ToString()` ([`GridContentItem.cs:L10`](file:///C:/dev/grove-v9/src/GroveApp/Models/GridContentItem.cs#L10)), generating v4 pseudo-random UUIDs instead of time-ordered UUIDv7. |
| `YAML Frontmatter Parser` | `src/GroveApp/Engine/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. Local files are not parsed or synchronized with YAML headers. |

---

## 4. Standards & Visual Plane Seam Audit

### 4.1 Visual Plane Separation (Plane 0 vs Layer 1 vs Plane 2)
- **Plane 0 (Spatial Grid Canvas)**: Placed notes, documents, and images on [`GridCanvasControl.cs`](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs#L30) reference transient UI model instances (`GridNote`, `GridDocument`, `GridImage`) directly inside a `List<GridContentItem>` collection. They lack any backing reference to a semantic `MemoryRecord` or `MemoryAnchor`.
- **Layer 1 (Information Layer Overlays)**: [`LocalEditorOverlay.axaml.cs`](file:///C:/dev/grove-v9/src/GroveApp/Controls/LocalEditorOverlay.axaml.cs#L100) edits text directly inside Avalonia `TextBox` instances. On save ([`LocalEditorOverlay.axaml.cs:L140`](file:///C:/dev/grove-v9/src/GroveApp/Controls/LocalEditorOverlay.axaml.cs#L140)), it mutates the `ContentText` string property of `GridNote` in-place, violating the fundamental immutability invariant of ADR-020.
- **Plane 2 (HUD Slate Plane)**: No Memory Slate or Vault Browser UI exists on Plane 2 to query or view memory records by cryptographic content hash or anchor lineage.

### 4.2 Code Smells & Architectural Violations
1. **Direct In-Place Mutation**: Modifying note text overwrites the string property in memory without recalculating SHA-256 digests or generating immutable ledger entries.
2. **Coupling Semantic Thought to Spatial Placement**: Semantic content is directly embedded inside spatial UI controls rather than existing as a standalone content-addressable `MemoryRecord` bound via multi-layer `MemoryAnchor` objects.

---

## 5. Root Cause Analysis (RCA)

### 5.1 Primary Root Cause
The codebase in `src/GroveApp/` was constructed as an initial visual prototype focused on GPU Skia rendering and basic Avalonia controls. The foundational domain models (`MemoryRecord`, `ContentHash`, `MemoryAnchor`, `ImmutableMemoryLedger`) specified in ADR-020 were never instantiated or integrated into the application's engine layers.

### 5.2 Failure Chain
1. **Specification Divergence**: ADR-020 was authored as a normative architectural standard, but implementation work stopped at basic UI view models (`GridNote`, `GridDocument`, `GridImage`).
2. **Missing Subsystem Seams**: No memory subsystem directory (`src/GroveApp/Memory/` or `src/GroveApp/Engine/Memory/`) was created to house the immutable ledger engine.
3. **Absence of Disk Sync Integration**: File persistence mechanisms were mocked in memory, bypassing YAML frontmatter parsing and SHA-256 hash validation completely.
