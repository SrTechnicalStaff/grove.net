# Root Cause Analysis (RCA) Ledger: ADR-052

## 1. Executive Metadata

| Property | Value |
| :--- | :--- |
| **ADR ID** | `ADR-052` |
| **Title** | Content Anchor System and Memory Binding Architecture |
| **Category** | Grid Systems (`docs/specs/grid-systems/`) |
| **Claimed Spec Status** | `IMPLEMENTED - AWAITING USER REVIEW` |
| **Verified Interactive Status** | `FAILS INTERACTIVE AUDIT (MISSING SPEC CONTRACTS)` |
| **Audit Date** | 2026-08-12 |
| **Target Runtime** | C# 13 / .NET 9 / Avalonia 11.2.5 / SkiaSharp 3.x |
| **Authors** | Chief Product Definition Architect & AI Systems Audit Team |

---

## 2. Normative Specification Requirement Inventory

The table below catalogs every explicit architectural requirement, mathematical formulation, interface contract, and visual token specified in [ADR-052](file:///C:/dev/grove-v9/docs/specs/grid-systems/ADR-052-Content-Anchor-System-And-Memory-Binding.md):

| ID | Requirement Category | Normative Spec Requirement | Target Symbol / Token |
| :--- | :--- | :--- | :--- |
| **REQ-052-1** | Service Contract | `IMemoryAnchorService` interface managing active bindings, `ToggleAnchorState`, `SetAnchorState`, `SyncToDiskProvenance`, and `AnchorStateChanged`. | `Grove.SpatialGrid.Anchoring.IMemoryAnchorService` |
| **REQ-052-2** | Data Contracts | `AnchorCoordinateRecord` record, `ContentAnchorBinding` record containing placement ID, memory ID, `IsAnchored` flag, and frontmatter tags. | `AnchorCoordinateRecord`, `ContentAnchorBinding` |
| **REQ-052-3** | Compositor Execution | Custom Skia draw operation `AnchorRibbonDrawOperation` implementing Avalonia `ICustomDrawOperation` for zero-allocation ribbon rendering. | `Grove.SpatialGrid.Rendering.AnchorRibbonDrawOperation` |
| **REQ-052-4** | Notched Ribbon Geometry | Notched ribbon mark `A` ($W_r = 16.0\text{ DIPs}$, $H_r = 24.0\text{ DIPs}$, notch depth $d_n = 6.0\text{ DIPs}$ with inverted V-notch apex $V_3$). | Notched Ribbon `A` Vector Path |
| **REQ-052-5** | Lettermark Typography | Lettermark `A` centered in ribbon upper rect using 10pt custom bold serif font (`Georgia` / `Inter`). | Lettermark `A` 10pt Serif Paint |
| **REQ-052-6** | Spatial Pinning Guard | Spatial transformation operator $\mathbf{T}_{\text{drag}}$ enforces `IsAnchored = true` placements are spatially immovable, emitting refusal signal. | Spatial Pinning Transformation Guard |
| **REQ-052-7** | Provenance Synchronization | Atomic YAML frontmatter (`anchors: [...]`, `is_anchored: true`, `memory_id: ...`) and companion JSON sidecar disk provenance sync. | Provenance Serialization Contract |
| **REQ-052-8** | Keyboard Interaction | Key `A` toggles `IsAnchored` state of focused placement or active multi-selection set. | Keybind `A` Context Router |

---

## 3. Codebase Reality & Line-by-Line Evidence

### 3.1 Implemented & Partially Implemented Symbols

- **`IsAnchored` Property on Models**:
  - `GridContentItem.cs` [L15](file:///C:/dev/grove-v9/src/GroveApp/Models/GridContentItem.cs#L15) contains `public bool IsAnchored { get; set; }`.
  - `GridNote.cs`, `GridDocument.cs`, `GridImage.cs` inherit `IsAnchored`.
- **Drag Block Verification**:
  - `GridCanvasControl.cs` [L763](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs#L763) checks `if (_dragCluster.Any(item => item.IsAnchored))` and prevents translation.
- **Keybind `A` Handling**:
  - `KeybindModule.cs` [L185-L195](file:///C:/dev/grove-v9/src/GroveApp/Engine/KeybindModule.cs#L185-L195) intercepts `Key.A` and toggles `IsAnchored` on a single note at cursor position.

### 3.2 0% Implemented & Deviated Symbols

- **`IMemoryAnchorService` Interface**: **0% Implemented**. Missing namespace `Grove.SpatialGrid.Anchoring` and interface `IMemoryAnchorService`. No service manages anchor lifetime or provenance synchronization.
- **`ContentAnchorBinding` & `AnchorCoordinateRecord` Records**: **0% Implemented**. Missing data contract records.
- **`AnchorRibbonDrawOperation`**: **0% Implemented**. Missing custom Skia draw operation `Grove.SpatialGrid.Rendering.AnchorRibbonDrawOperation`.
- **Notched Ribbon `A` Geometry ($16 \times 24\text{px}$ Inverted V-Notch Apex)**: **0% Implemented**.
  - In `NoteRenderModule.cs` [L268-L285](file:///C:/dev/grove-v9/src/GroveApp/Engine/NoteRenderModule.cs#L268-L285), the ribbon mark is drawn as a basic polygon or flat box, completely lacking the exact spec-defined $16 \times 24\text{px}$ inverted V-notch path ($V_3 = (W_r/2, H_r - d_n)$) and centered 10pt serif `A` lettermark.
  - In `QuickNoteOverlay.axaml.cs` [L294](file:///C:/dev/grove-v9/src/GroveApp/Controls/QuickNoteOverlay.axaml.cs#L294), the mark is rendered as a generic diamond mark instead of the notched ribbon.
- **Provenance Disk Synchronization (`anchors: [...]`)**: **0% Implemented**. Atomic YAML frontmatter serialization to disk files is completely absent in runtime code.
- **Refusal Feedback on Drag Attempt**: **0% Implemented**. When attempting to drag an anchored item, the move is silently ignored without emitting an inline refusal feedback signal or point-of-action notification.

---

## 4. Standards & Visual Plane Seam Audit

1. **Plane 0 Composition Seam**:
   - The anchor mark is drawn as part of item rendering in `NoteRenderModule.cs` on Plane 0. However, because `AnchorRibbonDrawOperation` was omitted, the ribbon geometry does not execute as a zero-allocation Skia path lease operation.
2. **Design System Tokens Alignment**:
   - Color `--ink` (`#F4F4F2` at 0.88 opacity) and `--k-tool` (`#3B82F6`) are specified for ribbon fills, but rendering uses hardcoded brush instances.
3. **Code Smells & Architectural Violations**:
   - `IsAnchored` state is treated as a simple boolean field on UI models (`GridContentItem`) rather than being bound to a durable `MemoryRecord` ledger provenance entity.
   - Key `A` in `KeybindModule.cs` directly mutates `note.IsAnchored = !note.IsAnchored` without calling a selection service or notifying layer subscribers.

---

## 5. Root Cause Analysis (RCA)

### Why claimed status (`IMPLEMENTED - AWAITING USER REVIEW`) diverges from actual interactive reality:

1. **Boolean Property Substituted for Provenance Architecture**:
   - The spec defined a complete memory anchoring architecture linking spatial coordinates to disk YAML frontmatter (`anchors: [...]`). The implementation added a simple `bool IsAnchored` property on UI C# classes and declared victory.
2. **Visual Mark Geometry Compromise**:
   - Rather than authoring the exact Skia vector path for the $16 \times 24\text{px}$ notched ribbon with inverted V-notch and serif lettermark `A`, developer placeholders (flat boxes / simple polygons) were left in `NoteRenderModule.cs`.
3. **Omission of Persistence Engine**:
   - Disk file IO and YAML frontmatter parsing/serialization were deemed out-of-scope during initial UI coding, leaving anchoring as a volatile in-memory flag lost upon application restart.
4. **Silent Refusal Failures**:
   - The drag guard was implemented as a silent `return false;` in `GridCanvasControl`, leaving users confused when dragging anchored items because no refusal visual feedback is displayed.
