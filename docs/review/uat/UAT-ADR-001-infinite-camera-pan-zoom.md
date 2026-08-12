# UAT-ADR-001: Decoupled Affine Camera System, Infinite Viewport & Cursor-Anchored Zoom

| Metadata Field    | Value                                                                     |
| ----------------- | ------------------------------------------------------------------------- |
| **Status**        | Proposed / In Review                                                      |
| **Date**          | 2026-08-12                                                                |
| **Authors**       | Senior Technical Staff / Grove Architecture Group                         |
| **Classification**| Architectural Decision Record (ADR) / User Acceptance Testing (UAT) Spec  |
| **Target Seam**   | [CameraModule.cs](file:///C:/dev/grove-v9/src/GroveApp/Engine/CameraModule.cs) |

---

## 1. Domain Separation & Context

In Grove v9, the **Camera System** is strictly decoupled from spatial content, grid line rendering, cell pitch math, and aura field physics (per [AGENTS.md](file:///C:/dev/grove-v9/AGENTS.md)). The camera operates as a pure 2D affine transformation engine \(T(x, y, s)\) projecting world coordinate space onto screen coordinate space.

User Acceptance Testing (UAT) identified issues where camera panning felt restricted, zooming appeared bound to a fixed origin, and system OS pointer artifacts overlapped the canvas. This document defines the root cause analysis and technical specification for an unconstrained, infinite affine camera system.

---

## 2. Root Cause Analysis (RCA)

### 2.1 Scale-Dependent Coordinate Contracting (\(\text{Zoom} \to 0\))

#### Symptom
Zooming out forces the world origin \((0,0)\) back into the center of the viewport, giving the user the impression that the canvas is bounded or locked to a fixed spatial center.

#### Empirical Analysis
The 2D affine world-to-screen transform in [CameraModule.cs](file:///C:/dev/grove-v9/src/GroveApp/Engine/CameraModule.cs#L24-L30) is expressed as:

\[
P_{\text{screen}} = P_{\text{world}} \cdot \text{Zoom} + \begin{pmatrix} \text{CameraX} \\ \text{CameraY} \end{pmatrix}
\]

As `Zoom` decreases toward `MinZoom = 0.01` (1%), the term \(P_{\text{world}} \cdot \text{Zoom}\) contracts toward \(0\) for all world coordinates \(P_{\text{world}}\). Consequently:

\[
\lim_{\text{Zoom} \to 0} P_{\text{screen}} = \begin{pmatrix} \text{CameraX} \\ \text{CameraY} \end{pmatrix}
\]

Because \(\text{CameraX}\) and \(\text{CameraY}\) represent viewport pixel offsets (e.g. \(\approx 100\text{px}\)), zooming out compresses thousands of world coordinates into a small pixel cluster centered at \((\text{CameraX}, \text{CameraY})\). This mathematically pulls world origin \((0,0)\) into the visible viewport regardless of how far away in world space the user had panned.

---

### 2.2 Sub-Pixel Scale Panning Friction

#### Symptom
Panning feels sluggish or stuck at high zoom levels.

#### Empirical Analysis
In [GridCanvasControl.cs](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs#L407-L415), pan drag deltas are added directly in screen pixels:

```csharp
CameraX = _panStartCamX + delta.X;
CameraY = _panStartCamY + delta.Y;
```

At high zoom (`Zoom = 5.0`), dragging 500 screen pixels shifts world space by only \(\frac{500}{5.0} = 100\) units (less than half a cell). Short mouse drags produce sub-pixel world displacements, appearing visually frozen.

---

### 2.3 System Pointer Overlay Artifact

#### Symptom
The system OS arrow pointer remains visible over top of the spatial canvas.

#### Empirical Analysis
In Avalonia UI controls, suppressing the default operating system mouse pointer requires assigning `Cursor = new Cursor(StandardCursorType.None)`. In [GridCanvasControl.cs](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs), the `Cursor` property is never set to `None`, leaving the OS arrow pointer visible alongside custom canvas elements.

---

## 3. Normative Technical Specifications

### 3.1 Unconstrained 2D Affine Camera Engine

#### 3.1.1 Floating-Point Infinity
The camera transformation matrix \(T(x, y, s)\) managed by [CameraModule.cs](file:///C:/dev/grove-v9/src/GroveApp/Engine/CameraModule.cs) must support an unbounded floating-point domain \((-\infty, +\infty)\). No spatial wrapping, bounding box clamps, or artificial origin locks are permitted on `CameraX` or `CameraY`.

#### 3.1.2 Scale-Proportional Panning Rate
Panning speed in world coordinate space must scale inversely with the active zoom factor so that mouse motion maps 1:1 to visible screen displacement:

\[
\Delta P_{\text{world}} = \frac{\Delta P_{\text{screen}}}{\text{Zoom}}
\]

```csharp
public void PanScreenDelta(Vector screenDelta)
{
    CameraX += screenDelta.X;
    CameraY += screenDelta.Y;
}
```

#### 3.1.3 Cursor-Anchored Zoom Stability
The `ZoomAt` algorithm in [CameraModule.cs](file:///C:/dev/grove-v9/src/GroveApp/Engine/CameraModule.cs#L85-L93) must preserve focal point stability. Zooming in or out at a target screen point \(P_{\text{screen}}\) must hold the underlying world point \(P_{\text{world}}\) stationary beneath the cursor:

\[
P_{\text{world}} = \frac{P_{\text{screen}} - \text{CameraOffset}_{\text{old}}}{\text{Zoom}_{\text{old}}} = \frac{P_{\text{screen}} - \text{CameraOffset}_{\text{new}}}{\text{Zoom}_{\text{new}}}
\]

#### 3.1.4 System OS Cursor Suppression
`GridCanvasControl` must set `Cursor = new Cursor(StandardCursorType.None)` during initialization to suppress the OS mouse pointer across the spatial canvas.

---

## 4. Seam Architecture & Module Boundaries

```
+-----------------------------------------------------------------------+
| CameraModule (Decoupled Engine)                                       |
|  - Manages T(x, y, s) Affine Matrix                                  |
|  - ScreenToWorld(pt), WorldToScreen(pt)                               |
|  - Zero knowledge of Grid Pitch, Cells, Notes, or Field Ledgers       |
+-----------------------------------------------------------------------+
```

1. **Zero Domain Knowledge**: `CameraModule` must remain 100% decoupled from grid pitch, cell coordinates, note models, and rendering pipelines.
2. **Matrix Translation Interface**: High-frequency render pipelines consume affine transformation matrices directly via `Camera.GetTransformMatrix()`.

---

## 5. UAT Acceptance Criteria & Verification Plan

1. **Infinite Pan Verification**: Pan camera to coordinates exceeding \((1,000,000, 1,000,000)\). Verify smooth pan translation with zero coordinate wrapping or origin reset.
2. **Zoom Anchor Verification**: Zoom from 1% to 1000% centered on a specific target screen point. Verify the world point directly under the cursor does not drift.
3. **Cursor Suppression Verification**: Hover pointer over the grid canvas. Verify standard OS arrow cursor is hidden.
