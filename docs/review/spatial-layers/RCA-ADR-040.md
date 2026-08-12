# RCA Ledger: ADR-040 — Spatial Layer System Architecture

## 1. Executive Metadata

| Metadata Field | Value |
| :--- | :--- |
| **ADR ID** | ADR-040 |
| **ADR Title** | Spatial Layer System Architecture |
| **Category** | Spatial Layers (`docs/specs/spatial-layers/`) |
| **Claimed Status in Spec Header** | IMPLEMENTED - AWAITING USER REVIEW |
| **Verified Status (User-Observable)** | **FALSE CLAIM — PARTIALLY IMPLEMENTED (20% Implemented, Core Contracts Missing)** |
| **Audit Date** | 2026-08-12 |
| **Target Runtime** | C# 13 / .NET 9 / Avalonia 11.2.5 / SkiaSharp |
| **Auditor** | Principal AI Systems Architect & Product Auditor |

---

## 2. Normative Specification Requirement Inventory

| ID | Requirement Category | Symbol / Contract Name | Normative Specification Requirement |
| :--- | :--- | :--- | :--- |
| **REQ-040-01** | Layer Identifier Struct| `LayerId` Struct | Strongly typed immutable Guid wrapper struct (`LayerId.New()`, `LayerId.BaseLayer`). |
| **REQ-040-02** | Label Token Struct | `LayerLabel` Struct | Immutable side-coded label token struct enforcing a maximum character length of 4 characters (`4ch`, e.g., `01`, `02`, `B01`). |
| **REQ-040-03** | Layer Domain Model | `SpatialLayer` Record | Immutable record containing `Id` (`LayerId`), `Label` (`LayerLabel`), `Name` (string), `IsVisible` (bool), `IsLocked` (bool), `ColorTint` (`Vector4`). |
| **REQ-040-04** | Layer Stack Continuum| `SpatialLayerStack` Class | Thread-safe immutable spatial layer stack continuum providing `ActiveLayerId`, `GetStackIndex`, `WithActiveLayer`. |
| **REQ-040-05** | Service Interface | `ISpatialLayerStackManager` | Interface declaring `CurrentStack`, `AddLayerAbove`, `AddLayerBelow`, `MoveLayer`, `RemoveLayer`, `SetActiveLayer`. |
| **REQ-040-06** | Migration Result | `LayerMigrationResult` Struct | Migration result record struct (`Success`, `TargetLayerId`, `DestinationLayerId`, `MigratedPlacementCount`, `RefusalReason`). |
| **REQ-040-07** | Placement Occupancy | Same-Layer Non-Overlap | Placements on the *same* layer $L_k$ MUST NOT overlap: $\text{Rect}_A \cap \text{Rect}_B = \emptyset \quad \forall A, B \in \text{Placements}(L_k)$. |
| **REQ-040-08** | Cross-Layer Occupancy| Multi-Occupancy Rule | Placements on *different* layers $L_j$ and $L_k$ ($j \neq k$) MAY occupy identical grid cell coordinates $(x,y)$ without spatial collision or refusal. |
| **REQ-040-09** | Content Preservation | Atomic Migration Engine | Deleting a layer migrates all placed items to an adjacent surviving layer ($L_{\text{stackIndex}-1}$ or $L_{\text{stackIndex}+1}$). |
| **REQ-040-10** | Skia Render Operation| `SpatialLayerCanvasRenderOperation` | Custom Avalonia Skia rendering operator iterating bottom-to-top through stack index (ZIndex = 0 to $N-1$). |

---

## 3. Codebase Reality & Line-by-Line Evidence

### 3.1 Symbol Existence & Implementation Audit

| Symbol / Contract Name | Expected Location | Actual Status in `src/GroveApp/` | Line-by-Line Evidence |
| :--- | :--- | :--- | :--- |
| `LayerId` Struct | `src/GroveApp/Engine/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. [`SpatialLayerStack.cs:L7`](file:///C:/dev/grove-v9/src/GroveApp/Engine/SpatialLayerStack.cs#L7) uses primitive `int Id`. |
| `LayerLabel` Struct | `src/GroveApp/Engine/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. Uses raw `string StableLabel` without `4ch` validation. |
| `SpatialLayer` Record | `src/GroveApp/Engine/` | **PARTIALLY IMPLEMENTED (DIVERGENT)** | Implemented in [`SpatialLayerStack.cs:L6-L11`](file:///C:/dev/grove-v9/src/GroveApp/Engine/SpatialLayerStack.cs#L6) as `record SpatialLayer(int Id, string StableLabel, string Name, bool IsVisible = true, bool IsLocked = false)`. Lacks `Vector4 ColorTint`. |
| `ISpatialLayerStackManager`| `src/GroveApp/Engine/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. |
| `LayerMigrationResult` | `src/GroveApp/Engine/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. No migration result struct exists. |
| `SpatialLayerCanvasRenderOperation` | `src/GroveApp/Engine/` | **0% Implemented (MISSING SYMBOL)** | 0 occurrences in codebase. Skia rendering is handled inside `GridCanvasControl.cs` without layer stack render operations. |
| Placement Migration Engine | `src/GroveApp/Engine/` | **0% Implemented (MISSING LOGIC)** | Deleting a layer in `SpatialLayerStack.cs` does NOT migrate placed items or perform cell collision checks. |

---

## 4. Standards & Visual Plane Seam Audit

### 4.1 Visual Plane Separation (Plane 0 vs Layer 1 vs Plane 2)
- **Plane 0**: `GridCanvasControl` references `SpatialLayerStack`, but does NOT execute a multi-pass `SpatialLayerCanvasRenderOperation` to paint active content vs inactive presence heatmaps.
- **Layer 2**: No HUD layer manager interface is connected to `SpatialLayerStack`.

### 4.2 Code Smells & Architectural Violations
1. **False Claim in Spec Header**: The spec header asserts `status: "IMPLEMENTED - AWAITING USER REVIEW"`, yet core domain contracts (`LayerId`, `LayerLabel`, `ISpatialLayerStackManager`, `LayerMigrationResult`, `SpatialLayerCanvasRenderOperation`) are 0% implemented.
2. **Primitive Type Drift**: Using raw `int` for layer IDs and unvalidated `string` for side-coded labels allows invalid state mutations.

---

## 5. Root Cause Analysis (RCA)

### 5.1 Primary Root Cause
The spec header for ADR-040 was prematurely updated to `"IMPLEMENTED - AWAITING USER REVIEW"` despite the codebase only containing a simplified initial stub (`SpatialLayerStack.cs`). The actual implementation work for strongly-typed `LayerId` structs, migration engines, and Skia layer render operations was never completed.

### 5.2 Failure Chain
1. **False Status Marking**: Spec status was marked implemented without code verification.
2. **Simplified Class Implementation**: `SpatialLayerStack.cs` was written as a lightweight prototype class using `int` IDs rather than the normative `LayerId` domain contracts specified in ADR-040.
