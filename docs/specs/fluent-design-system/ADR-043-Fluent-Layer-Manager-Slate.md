---
status: "NORMATIVE SPECIFICATION — FLUENT SPATIAL LAYER MANAGER SLATE"
verification: "PARTIAL — the Plane 2 slate and core layer operations are implemented; per-layer swatches and transfer confirmation wording remain."
---

# ADR-043: Fluent Spatial Layer Manager Slate & Physics Integration

| Property | Value |
| :--- | :--- |
| **Document Type** | Normative Spatial Engineering & Component Specification |
| **Target Runtime** | .NET 9 / C# 13 / Avalonia 11.2.5 / SkiaSharp 3.x |
| **Layer Label Convention** | **Main / Upper Floors**: `01`, `02`, `03`... **Below Main Layer**: `B02`, `B03`, `B04`... (`B` = Below Main Layer). |
| **Aura Permeability Law** | $E(c, Z_{\text{active}}) = \sum_{L} E(c, L) \cdot 0.5^{|L - Z_{\text{active}}|}$ |
| **Corner Radius Policy** | Sharp Edges / Low Radii (`Tokens.CornerRadiusNone` = 0.0px / `Tokens.CornerRadiusXs` = 1.0px) |
| **Padding Policy** | Minimal Padding (`Tokens.SpacingXs` = 4.0px / `Tokens.SpacingSm` = 8.0px) |
| **Seam Discipline** | Deep Module Architecture via [`ISpatialLayerStateService`](#3-deep-module-interface-seam) |
| **Date** | 2026-08-12 |
| **Authors** | Chief Product Definition Architect |

---

## 1. Spatial Grid, Aura Field & Layer Label Mechanics

Grove v9 structures continuous 2D space across an infinite vertical layer continuum $Z \in (-\infty, +\infty)$.

### 1.1 Layer Label Convention Rules
- **Main Layer (Ground Level)**: Labeled `01`.
- **Upper Layers (Above Main Layer)**: Labeled sequentially as `02`, `03`, `04`...
- **Lower Layers (Below Main Layer)**: Labeled sequentially as `B02`, `B03`, `B04`... where `B` stands for *Below Main Layer*.
- **Label Alignment**: All layer labels render in a fixed `4ch` monospaced column (`01`, `02`, `B02`, `B03`).

```
                  VERTICAL LAYER CONTINUUM & LABEL CONVENTION
                  
    Layer 03 (Floor 3 Above Main)     [Item A] ---> Label: "03"
    Layer 02 (Floor 2 Above Main)     [Item B] ---> Label: "02"
    Layer 01 (Main Ground Layer)     <=== ACTIVE VIEWPORT (Label: "01")
    Layer B02 (1st Level Below Main)   [Item C] ---> Label: "B02"
    Layer B03 (2nd Level Below Main)   [Item D] ---> Label: "B03"
```

### 1.2 Mathematical Laws of Spatial Layer Interaction
1. **Vertical Aura Permeability**: Energy field sources $M_i$ on layer $L$ contribute energy to Plane 0 cell $c(x,y)$ on active layer $Z_{\text{active}}$ according to:
   $$E(c, Z_{\text{active}}) = \sum_{i \in \text{Sources}} \frac{M_i}{1 + 0.4 \cdot d_i^2} \cdot 0.5^{|L_i - Z_{\text{active}}|}$$
2. **Dynamic Dark Line Suppression**: Grid lines on Plane 0 underneath energy heatmaps ($E \ge 0.05$) are suppressed via 4-pass path difference subtraction:
   $$\mathcal{P}_{\text{visible}} = \mathcal{P}_{\text{grid}} \setminus \Omega_{\text{aura}}$$
3. **Ghost Presence Silhouettes**: Inactive layer items ($L \neq Z_{\text{active}}$) render non-interactive ghost presence outlines on Plane 0 with opacity:
   $$\alpha_{\text{ghost}}(L) = 0.15 \cdot 0.5^{|L - Z_{\text{active}}|}$$
4. **Hit-Testing Isolation**: Mouse hit-testing on Plane 0 evaluates strictly against active layer items ($L = Z_{\text{active}}$). Inactive layer items pass all pointer input directly through to Plane 0.

---

## 2. Layer Manipulation Keybinds & Rules

| User Action | Hotkey / Gesture | Stack Operation & Label Rules |
| :--- | :--- | :--- |
| **Navigate Down** | `[` | Moves active selection down 1 level in stack. |
| **Navigate Up** | `]` | Moves active selection up 1 level in stack. |
| **Jump to Top** | `Shift+]` | Jumps active selection to highest layer in stack. |
| **Jump to Bottom** | `Shift+[` | Jumps active selection to lowest layer in stack (`B...`). |
| **Insert Layer Above** | `Ctrl+Shift+N` | Inserts new layer directly **above** active layer. |
| **Insert Layer Below** | `Ctrl+Alt+Shift+N` | Inserts new layer directly **below** active layer. |
| **Reorder Swap Up** | `Alt+Up` / Mouse Drag Up | **Single Action**: Swaps active layer's position with layer directly above it. |
| **Reorder Swap Down** | `Alt+Down` / Mouse Drag Down | **Single Action**: Swaps active layer's position with layer directly below it. |
| **Delete Layer** | `Del` on HUD row | Triggers inline confirmation row. Transfers items to Main Layer `01` upon commit. |

---

## 3. Deep Module Interface Seam

Following [`codebase-design/SKILL.md`](file:///C:/dev/grove-v9/.agents/skills/codebase-design/SKILL.md), all layer stack calculations, label formatting (`01`, `02`, `B02`), permeability attenuation, and ledger synchronization are hidden behind a **small, deep, testable interface seam**:

```csharp
namespace GroveApp.Engine
{
    public record struct SpatialLayerModel(
        int ZIndex,
        string Label, // "01", "02", "B02", "B03"
        string DisplayName,
        bool IsActive,
        int ContentItemCount
    );

    public interface ISpatialLayerStateService
    {
        IReadOnlyList<SpatialLayerModel> Layers { get; }
        SpatialLayerModel ActiveLayer { get; }
        
        void SetActiveLayer(int zIndex);
        SpatialLayerModel InsertLayerAbove(int currentZIndex);
        SpatialLayerModel InsertLayerBelow(int currentZIndex);
        void ReorderSwap(int sourceZIndex, int targetZIndex);
        bool RenameLayer(int zIndex, string newName, out string errorReason);
        bool RemoveLayer(int zIndex, int targetTransferZIndex);
        
        double CalculateAuraPermeability(int sourceZIndex, int targetZIndex);
        string FormatLayerLabel(int zIndex); // 01, 02, B02, B03
        
        event Action<SpatialLayerModel> ActiveLayerChanged;
        event Action LayerStackChanged;
    }
}
```

---

## 4. User Journey Wireframes

### Journey State 1: Resting View (`01`, `02`, `B02` Labels)
```
+-------------------------------------------------------------------------------+
| LAYERS                                                                [ × ]   |
| [ Find layer...                                                           ]   |
+-------------------------------------------------------------------------------+
| 03    Roof Deck & Mechanical                                                  |
| 02    2nd Floor Architectural Annotations                                     |
| 01    Main Ground Layer (Active Layer)                                        | <-- Active (1px --signal-interaction)
| B02   Basement 1 Utilities & Plumbing                                         |
| B03   Sub-surface Foundation & Structural Anchors                             |
+-------------------------------------------------------------------------------+
```

### Journey State 2: Single-Gesture Reordering (`Alt+Up` / Mouse Drag)
```
+-------------------------------------------------------------------------------+
| LAYERS                                                                [ × ]   |
| [ Find layer...                                                           ]   |
+-------------------------------------------------------------------------------+
| 03    Roof Deck & Mechanical                                                  |
| 01    Main Ground Layer (Active Layer)                                        | <-- Swapped Up (Single Action)
| 02    2nd Floor Architectural Annotations                                     |
| B02   Basement 1 Utilities & Plumbing                                         |
| B03   Sub-surface Foundation & Structural Anchors                             |
+-------------------------------------------------------------------------------+
```

### Journey State 3: Inserting Layer Below Main Layer (`Ctrl+Alt+Shift+N`)
```
+-------------------------------------------------------------------------------+
| LAYERS                                                                [ × ]   |
| [ Find layer...                                                           ]   |
+-------------------------------------------------------------------------------+
| 02    2nd Floor Architectural Annotations                                     |
| 01    Main Ground Layer (Active Layer)                                        |
| B02   New Sub-surface Storage (Inserted Below Main)                           | <-- Inserted Below Main (B02)
| B03   Basement 1 Utilities & Plumbing                                         |
| B04   Sub-surface Foundation & Structural Anchors                             |
+-------------------------------------------------------------------------------+
```
