# Root Cause Analysis (RCA) Ledger: ADR-002

## 1. Executive Metadata

| Property | Value |
| :--- | :--- |
| **ADR ID** | ADR-002 |
| **ADR Title** | Field Ledger and Subscribers |
| **Category** | Field Ledger System / Multi-Layer Energy Topology |
| **Claimed Status** | IMPLEMENTED - AWAITING USER REVIEW |
| **Verified Status** | PARTIALLY IMPLEMENTED (Cell energy calculations and heatmap subscriber active; reactive pub-sub manager, perimeter ring subscriber, and annotation subscriber missing/unwired) |
| **Audited Runtime Target** | C# 13 / .NET 9.0 / Avalonia UI 11.2.5 |
| **Audit Date** | 2026-08-12 |

---

## 2. Normative Specification Requirement Inventory

| Spec ID | Requirement / Contract Description | Target Type / Symbol | Specified Value / Formula |
| :--- | :--- | :--- | :--- |
| **REQ-002-01** | Non-Zero Baseline Cell Energy | Energy Constant | $E_0 = 0.05$ (`CellLedgerEntry.BaselineEnergy`) |
| **REQ-002-02** | Total Cell Energy Equation | Energy Equation | $E(c) = E_0 + \sum_{i} E_i(x, y, L)$ |
| **REQ-002-03** | Spatial Cell Position Struct | `GridCellPosition` | Sequential struct (Pack=4) with `ToSpatialKey()` |
| **REQ-002-04** | Source Metadata Payload | `FieldSourceMetadata` | Sequential struct (Pack=8) tracking ContentId, LayerId, Mass, Energy, Hue |
| **REQ-002-05** | Packed Cell Ledger Record | `CellLedgerEntry` | Sequential struct (Pack=8) with 4 inline source metadata slots |
| **REQ-002-06** | Subscriber Interface | `IFieldSubscriber` | Interface with `OnCellFieldUpdated` & `OnRegionFieldBatchUpdated` |
| **REQ-002-07** | Aura Heatmap Subscriber | `AuraHeatmapSubscriber` | Calculates energy-weighted composite hue and alpha ($0.025 - 0.30$) |
| **REQ-002-08** | Perimeter Ring Subscriber | `PerimeterRingSubscriber` | Detects $E \ge 0.15$ boundaries and emits continuous $1.5\text{px}$ vector paths |
| **REQ-002-09** | Annotation Metadata Subscriber | `AnnotationMetadataSubscriber` | Intercepts multi-source overlapping cell payloads ($\ge 2$ sources) |
| **REQ-002-10** | Field Ledger Manager Engine | `FieldLedgerManager` | Central pub-sub manager with `RegisterSubscriber`, `MutateCellField`, `BatchMutateRegion` |
| **REQ-002-11** | Discrete Alpha Tiers (Neutral) | Design Tokens | Tier 1 = $0.19$, Tier 2 = $0.12$, Tier 3 = $0.070$, Tier 4 = $0.038$ |
| **REQ-002-12** | Warm Note Field Alpha Tiers | Design Tokens | `#6B5540`: Tier 1 = $0.60$, Tier 2 = $0.38$, Tier 3 = $0.22$, Tier 4 = $0.11$ |
| **REQ-002-13** | Anchor Indigo Field Alpha Tiers | Design Tokens | `#9E8CEA`: Tier 1 = $0.30$, Tier 2 = $0.18$, Tier 3 = $0.10$, Tier 4 = $0.05$ |
| **REQ-002-14** | 1-Frame Aura Lag Trade-off | Performance Constraint | Pointer cursor 0ms latency, field heatmap trailing by max 1 frame |
| **REQ-002-15** | Presence Field Refusal Laws | Architectural Refusals | Forbidden radial gaussian blurs crossing cell boundaries; cell is field pixel |

---

## 3. Codebase Reality & Line-by-Line Evidence

### 3.1 Implemented Artifacts

- **Memory Struct Layouts & Core Engine**: Implemented in [`src/GroveApp/Engine/FieldLedgerEngine.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/FieldLedgerEngine.cs#L30-L125).
  - Lines 32-36: `GridCellPosition` struct with `ToSpatialKey()`.
  - Lines 41-51: `FieldSourceMetadata` struct with inline payload attributes.
  - Lines 58-105: `CellLedgerEntry` struct with `BaselineEnergy = 0.05f` and four inline source slots (`InlineSource0` through `InlineSource3`).
  - Lines 105-125: `IFieldSubscriber` interface definition.
  - Lines 130-180: `AuraHeatmapSubscriber` class computing energy-weighted composite colors.
- **Atmospheric Canvas Rendering**: Implemented in [`src/GroveApp/Engine/FieldLedgerModule.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/FieldLedgerModule.cs#L1-L120) and invoked in [`src/GroveApp/Controls/GridCanvasControl.cs`](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs#L911).

### 3.2 Missing / Non-Conforming Features (0% Implemented)

- **`FieldLedgerManager` Central Pub-Sub Class**: **0% Implemented**. Missing class `Grove.SpatialGrid.FieldLedger.FieldLedgerManager`. Field calculations are manually triggered via pull calls (`RefreshFieldLedger()`) inside `GridCanvasControl.cs` [L270-L292](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs#L270-L292) rather than reactive push notifications via `BatchMutateRegion()`.
- **`PerimeterRingSubscriber`**: **0% Implemented**. Missing class `PerimeterRingSubscriber`. No perimeter contour isolation vector paths are extracted or drawn when cell energy crosses $E \ge 0.15$.
- **`AnnotationMetadataSubscriber`**: **0% Implemented**. Missing class `AnnotationMetadataSubscriber`. Overlapping multi-source cell payloads ($\ge 2$ sources) are ignored by the Information Plane.

---

## 4. Standards & Visual Plane Seam Audit

- **Visual Plane Compliance**: Field heatmap fills render strictly on Plane 0 underneath grid lines and content cards.
- **Token Compliance**: Standard cell fills use `Colors.SurfaceGrid` (`#0E0E10`). Authored colors match Violet (`#6E62A6`), Clay (`#B0524E`), and Slate Blue (`#4E6E9C`).
- **Architectural Seams & Coupling**: `GridCanvasControl.cs` is coupled directly to `FieldLedgerEngine.cs` by invoking synchronous recalculations during pointer movements. A clean decoupled event-driven pub-sub bus (`FieldLedgerManager`) is absent.

---

## 5. Root Cause Analysis

### Why claimed status (`IMPLEMENTED - AWAITING USER REVIEW`) diverges from actual interactive reality:

1. **Synchronous Polling Substituted for Reactive Pub-Sub**: The engine developer implemented `FieldLedgerEngine` as a synchronous calculation module called by `GridCanvasControl.RefreshFieldLedger()`. Because visual results (heatmap cell fills) were observable on screen, the task was marked "IMPLEMENTED" despite missing the reactive `FieldLedgerManager` pub-sub infrastructure.
2. **Omission of Boundary Subscribers**: `PerimeterRingSubscriber` and `AnnotationMetadataSubscriber` required complex vector contour tracing and cross-plane messaging. They were left as specification placeholders and never integrated into the live render pipeline.
