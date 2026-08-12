# UAT-ADR-002: Global Content Framing (`F` Keybind), Double-Click Focus Navigation & Gesture Routing

| Metadata Field    | Value                                                                     |
| ----------------- | ------------------------------------------------------------------------- |
| **Status**        | Proposed / In Review                                                      |
| **Date**          | 2026-08-12                                                                |
| **Authors**       | Senior Technical Staff / Grove Architecture Group                         |
| **Classification**| Architectural Decision Record (ADR) / User Acceptance Testing (UAT) Spec  |
| **Target Seam**   | [KeybindModule.cs](file:///C:/dev/grove-v9/src/GroveApp/Engine/KeybindModule.cs), [GridCanvasControl.cs](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs) |

---

## 1. Domain Context & Scope

Spatial navigation in Grove v9 requires both macro-level orientation (framing the full extent of work across the canvas) and micro-level focus (zooming deeply into a single note or document). 

User Acceptance Testing (UAT) highlighted two key navigation deficiencies:
1. Absence of a global framing keybind (`F`) to bring all content into view regardless of current scale or pan location.
2. Input gesture routing collisions where panning, disarming, marquee selection, and double-click focus were in conflict.

This document defines the normative specification for global framing, double-click focus navigation, and input gesture disambiguation.

---

## 2. Root Cause Analysis (RCA) of Input Routing Collisions

### 2.1 Input Gesture Collision Analysis

In [GridCanvasControl.cs](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs#L504-L592), pointer input handling handles multiple competing actions within a single `OnPointerPressed` method:

| Pointer Action | Trigger Condition | Actual Behavior | Conflict / Issue |
| :------------- | :---------------- | :-------------- | :--------------- |
| Camera Pan     | Middle / Right Drag | Sets `_isPanning = true` | Right-click disarms tool instead of panning when tool is armed ([line 513](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs#L513)). |
| Item Drag      | Left Press on Item | Sets `_isDraggingItem = true` | Prevents panning when starting drag over an item. |
| Resize Handle  | Left Press Corner  | Sets `_isResizingItem = true` | 24px corner hit test takes precedence over item drag. |
| Marquee Select | Left Press Empty   | Sets `_isMarqueeSelecting = true` | Left-drag on empty grid performs marquee selection instead of panning. |

---

## 3. Normative Technical Specifications

### 3.1 Global Framing Command (`F` Keybind)

```
+-----------------------------------------------------------------------+
| Viewport                                                              |
|   +---------------------------------------------------------------+   |
|   | Enclosing World Bounding Box (R_all)                          |   |
|   |  +------------+                +--------------------------+   |   |
|   |  | Note (2x2) |                | Document (4x4)           |   |   |
|   |  +------------+                +--------------------------+   |   |
|   +---------------------------------------------------------------+   |
+-----------------------------------------------------------------------+
```

#### 3.1.1 Bounding Box Calculation
Pressing `F` on the canvas triggers the **Frame All** command:
1. Iterates over all active items across all visible spatial layers and calculates the minimum enclosing world bounding rectangle \(R_{\text{all}} = [X_{\text{min}}, Y_{\text{min}}, W_{\text{total}}, H_{\text{total}}]\).
2. If no items exist on the canvas, `F` resets camera position to origin \((0,0)\) at default zoom `Zoom = 1.0`.

#### 3.1.2 Viewport Framing Scale Calculation
Computes the target zoom scale \(s_{\text{target}}\) required to fit \(R_{\text{all}}\) within current viewport dimensions \((W_{\text{vp}}, H_{\text{vp}})\) with a normative 10% outer margin padding:

\[
s_{\text{target}} = \min\left( \frac{W_{\text{vp}} \cdot 0.8}{W_{\text{total}}}, \frac{H_{\text{vp}} \cdot 0.8}{H_{\text{total}}} \right)
\]

The target scale is clamped between `MinZoom` (0.01) and `MaxZoom` (10.0).

#### 3.1.3 Camera Center Alignment
The target camera offset \((\text{CameraX}_{\text{target}}, \text{CameraY}_{\text{target}})\) centers the midpoint of \(R_{\text{all}}\) in viewport screen space:

\[
\text{CameraX}_{\text{target}} = \frac{W_{\text{vp}}}{2} - \left( X_{\text{min}} + \frac{W_{\text{total}}}{2} \right) \cdot s_{\text{target}}
\]
\[
\text{CameraY}_{\text{target}} = \frac{H_{\text{vp}}}{2} - \left( Y_{\text{min}} + \frac{H_{\text{total}}}{2} \right) \cdot s_{\text{target}}
\]

#### 3.1.4 Camera Animation Integration
Camera transform updates smoothly interpolate from current state to target state using standard motion duration `Tokens.DurationPlace` (300ms) with standard cubic-bezier easing.

---

### 3.2 Double-Click Region Focus Navigation

#### 3.2.1 Deep-Focus Zoom Transition
Double-clicking an arbitrary point or region on the canvas smoothly transitions the camera from framed/macro scale (\(s \le 0.1\)) directly to focused scale (\(s \ge 1.0 \text{ up to } 10.0\)), centering the target double-clicked cell within the viewport.

#### 3.2.2 Note / Document Focusing
Double-clicking directly on a note or document opens the corresponding editor overlay while centering and scaling the item bounds to fit comfortably within the top 60% of the viewport.

---

### 3.3 Disambiguated Input Gesture Router

1. **Unified Pan Modifier**: Holding `Space` + Left-Mouse Drag or Middle-Mouse Drag initiates camera pan unconditionally, regardless of armed tools or hovered items.
2. **Right-Click Priority**: Right-Click drag initiates camera pan. If a tool is armed, a static Right-Click tap disarms the tool, while a Right-Click drag pans the camera.
3. **Esc Key Discipline**: `Esc` disarms tools, clears selections, and closes active overlays in hierarchical precedence order.

---

## 4. Seam Architecture & Code Responsibilities

- **[KeybindModule.cs](file:///C:/dev/grove-v9/src/GroveApp/Engine/KeybindModule.cs)**: Maps global key combination `Key.F` to `canvas.FrameAllContent()`.
- **[GridCanvasControl.cs](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs)**: Implements `FrameAllContent()`, calculates bounding box \(R_{\text{all}}\), and delegates animation execution to `CameraModule`.

---

## 5. UAT Acceptance Criteria & Verification Plan

1. **Framing Keybind Verification**: Scatter 5 notes across distant grid locations. Press `F`. Verify all 5 notes are perfectly contained within the viewport with 10% margin padding.
2. **Empty Canvas Framing Verification**: Clear all items from the canvas. Press `F`. Verify camera returns to origin \((0,0)\) at `Zoom = 1.0`.
3. **Double-Click Focus Verification**: At 5% zoom scale, double-click an empty cell. Verify camera smoothly zooms in to 100% scale centered on the clicked cell.
