# ADR Surgical Gap Audit Report

| Property | Value |
| :--- | :--- |
| **Audit Date** | 2026-08-12 |
| **Scope** | Deep-Dive Audit of `docs/design_catalogue/src/grid-plane/` (`01-the-grid.html` through `07-selection-placement.html`) against authored ADRs in `docs/specs/spatial-grid/`, `docs/specs/content-types/`, and `docs/specs/grid-systems/` |
| **Integrity Assurance** | **100% Zero Deletions** — Every existing character, formula, struct, and contract preserved. Surgical additions appended to existing sections only. |

---

## 1. Executive Summary & Audit Methodology

A comprehensive deep-dive audit was conducted comparing the canonical design catalogue HTML decks in `docs/design_catalogue/src/grid-plane/` against all authored ADR specifications across:
- `docs/specs/spatial-grid/` (`ADR-001` through `ADR-005`)
- `docs/specs/content-types/` (`ADR-010` through `ADR-014`)
- `docs/specs/grid-systems/` (`ADR-050` through `ADR-056`)

Every metric, color token, font size, stroke width, hysteresis threshold, decay step count, signal role, and visual refusal from the 7 HTML decks was cross-referenced with the corresponding ADR contracts. Missing design details were surgically added without modifying or deleting any pre-existing text.

---

## 2. HTML Deck to ADR Mapping & Audit Matrix

| HTML Deck | Primary Topic & Metrics | Target ADR Specifications | Surgical Additions Made |
| :--- | :--- | :--- | :--- |
| **`01-the-grid.html`** | Spacetime grid, 220px pitch, 44px minor pitch (`#161618`), 1100px supercell (`#242428`), continuous fade curve `clamp((S-6)/8,0,1)`, Key `G` lines hidden toggle, `#0E0E10` canvas ground, grid cursor addressing, refusal of moiré & card packaging. | `ADR-001`, `ADR-050` | Canvas ground color `#0E0E10` contract, Key `G` view preference persistence per person, lines-off non-destructive contract, single-tier moiré refusal, card container packaging refusal, parked toolbar chrome refusal. |
| **`02-note.html`** | Note anatomy, 1x1 base (220px), authored fills (`#6E62A6`, `#B0524E`, `#4E6E9C`), 1px inset edge (`rgba(255,255,255,0.12)`), selection outline (`#96B6F8`), anchor ribbon (`top: -5px`, `left: 16px`, `12x22px`, `#9E8CEA`), hover handles (`18x18px` handle, `EDIT` mono badge), distance zoom tiers (100%, 24%, 6%), title bar + truncation refusal. | `ADR-010` | Hover handles & `EDIT` mono badge specs, selection field response (`inset 0 0 0 1.5px #96B6F8`), notched anchor ribbon polygon clip & dimensions, distance representation tiers (Working 100% $\to$ Stepped back 24% $\to$ Far 6% stand-in), title bar + truncation refusal, persistent button bar refusal, interior scrollbar refusal. |
| **`03-document.html`** | Document 2x2 to 8x8 whole cell footprint, page texture pitch (44px minor / 220px major), paper tint `#F5F5F5` / ink `#1A1A1A`, 5 front-page zones (Mono front matter 11px 48%, Display title 38px 0.98, Hairline rule 1px 16%, Abstract 15px 1.65 34ch, Close 13px 1.60 50%, Title block mono pair 68%/50%), neutral `234 234 234` aura, distance hysteresis (demote 56px/18px, promote 72px/28px, 96px snapshot stand-in), ellipsis clip refusal, dark file-card stand-in refusal. | `ADR-011` | Front-page 5-zone layout hierarchy & typography specs, paper inset shadow edge (`inset 0 0 0 1px rgba(26,26,26,0.10)`), neutral aura `234 234 234` spec, anchor ribbon specs (`top: -5px`, `left: 20px`, `12x22px`, `#9E8CEA`), precise distance hysteresis thresholds & 96px paper sheet stand-in, ellipsis clip refusal, interior scrollbar refusal, dark file-card stand-in refusal. |
| **`04-image.html`** | Picture placement, 256px divisor footprint formula $L = \lceil\text{long}/256\rceil$, aspect ratio frames (5x7, 3x4, 4x3, 3x3, 8x2), zero-crop rule, quiet 1px edge (`rgba(234,234,234,0.16)`), top-right GIF badge (9px mono, `rgba(14,14,16,0.72)`), selection outline (`#96B6F8`), 9x9px 45° rotated indigo diamond anchor mark (`#9E8CEA`), distance shedding tiers (working $\to$ surface shed $\to$ figure SVG stand-in on `#F5F5F5`), cover crop refusal, caption overlay refusal, decorated card refusal. | `ADR-012` | Quiet 1px edge spec, GIF tag badge & reduced-motion still frame contract, selected state outline & field cell brightening (`rgba(150,182,248,0.13)`), 9x9px 45° rotated indigo diamond anchor mark, distance shedding tiers & paper SVG figure stand-in (`#F5F5F5`), cover crop refusal, caption overlay refusal, decorated card refusal. |
| **`05-presence-fields.html`** | Cell-quantized Aura, lit cells with hard boundaries, discrete alpha tiers (`fw1` 0.19 to `fw4` 0.038, `fh1` 0.60 to `fh4` 0.11, `fi1` 0.30 to `fi4` 0.05), stepped falloff, perimeter isolines ($E \ge 0.15$), derived non-persisted state, cross-Grid-Layer hue summation (~100 layers as atmosphere), local recompute during drag with 1-frame Aura lag trade-off (`PA-02`), refusal of smooth gradient glow, and field brighter than Content. | `ADR-002`, `ADR-003` | Discrete alpha tier breakdown for neutral/warm/indigo modes, hard cell edge boundary rule (no smooth gradient blurs across cell boundaries), 1-frame Aura lag trade-off contract during drag gestures, smooth glow refusal, no cross-Grid-Layer ghost-content mode, field over Content luminance refusal. |
| **`06-distance.html`** | Distance representation framework (`WV-00` Working $\ge 72\text{px}$, `WV-01` Stepped Back 56-19px, `WV-02` Far $\le 18\text{px}$, `WV-03` Approach, `WV-04` Arrived), shedding order (chrome $\to$ surface $\to$ detail), kind-coded stand-ins (Sheet, Note, Figure), hysteresis threshold bounds (demote 18px / promote 28px; shed 56px / restore 72px), 160ms cross-fade promotion in place without blanking, selection on stand-ins, supercell line longevity. | `ADR-003`, `ADR-050` | Unified distance representation tier framework (`WV-00` through `WV-04`), explicit hysteresis threshold bounds, 160ms cross-fade promotion in place contract, kind-coded stand-in specs for Sheet/Note/Figure, scale-independent cell addressing for cursor across all zoom levels. |
| **`07-selection-placement.html`** | Selection sweeps, 50% cell area overlap threshold, 3 signal roles (Interaction `#96B6F8`, Marquee `#E8B964`, Invalid/Refusal `#E2625C` / `#F06543`), 1px dashed preview (`rgba(150,182,248,0.80)`) with 6% fill (`rgba(150,182,248,0.06)`) & mono corner size readout (`3x3`), origin ghost, collision refusal cross-hatch (12% fill, 45° hatching, inset border `inset 0 0 0 1.5px rgba(226,98,92,0.45)`), point-of-action refusal strip toolbar ("This space is occupied"), refusal of fill wash, solid preview, and center-screen alert dialog. | `ADR-051`, `ADR-053`, `ADR-055`, `ADR-056` | 3 distinct signal roles & color hexes (`#96B6F8`, `#E8B964`, `#E2625C` / `#F06543`), precise preview footprint styling & mono size readout, collision refusal 45° cross-hatching & inset border specs, point-of-action refusal strip toolbar contract ("This space is occupied"), refusal of fill wash over content, solid preview refusal, center-screen modal alert dialog refusal. |

---

## 3. Detailed File-by-File Surgical Log

### 1. `docs/specs/spatial-grid/ADR-001-Spatial-Grid-Plane-Architecture.md`
- **Section Modified**: Section 6 (`Verification & Conformance Rules`).
- **Target Line Range**: Lines 255–266.
- **Surgical Addition**: Appended Item 4 (`Canvas Ground & View Preference Contract`: `#0E0E10` canvas ground, Key `G` view preference persistence per person, lines-off non-destructive contract) and Item 5 (`Architectural Grid Refusals`: Single uniform tier moiré refusal, Container-per-item card packaging refusal, Parked toolbar chrome refusal).
- **Existing Content Preserved**: Lines 1–254 completely untouched.

### 2. `docs/specs/spatial-grid/ADR-002-Field-Ledger-And-Subscribers.md`
- **Section Modified**: Appended Section 6 (`Field Quantization, Latency Trade-offs & Refusal Invariants`).
- **Target Line Range**: Lines 274–296.
- **Surgical Addition**: Appended Section 6.1 (Discrete Alpha Tiers for neutral `--fw` 0.19–0.038, warm `--fh` 0.60–0.11, anchor indigo `--fi` 0.30–0.05; hard cell edge boundaries), Section 6.2 (Gesture Latency Trade-off: 0ms pointer tracking, local recompute, 1-frame Aura lag `PA-02`, origin feedback), and Section 6.3 (Presence Field Refusal Contracts: smooth glow refusal, no cross-Grid-Layer ghost-content mode, field over Content luminance refusal).
- **Existing Content Preserved**: Lines 1–273 completely untouched.

### 3. `docs/specs/spatial-grid/ADR-003-Spatial-Aura-Physics.md`
- **Section Modified**: Appended Section 6 (`Distance Representation Tiers & Selection Aura Integration`).
- **Target Line Range**: Lines 222–247.
- **Surgical Addition**: Appended Section 6.1 (5 Distance Representation Tiers `WV-00` through `WV-04` and kind-coded stand-ins for Sheet, Note, Figure), Section 6.2 (Viewport Hysteresis Thresholds: demote 18px / promote 28px; shed 56px / restore 72px), and Section 6.3 (Selection State Aura Response: 2px outline `#96B6F8`, soft glow `0 0 24px 6px rgb(150 182 248 / 0.45)`, structural field brightening `rgba(150,182,248,0.13)` edge / `0.06` diagonal with perimeter inset `1.5px`).
- **Existing Content Preserved**: Lines 1–221 completely untouched.

### 4. `docs/specs/content-types/ADR-010-Note-Physical-Geometry-And-Fills.md`
- **Section Modified**: Appended Section 6 (`Hover Affordances, Distance Tiers & Explicit Refusals`).
- **Target Line Range**: Lines 311–333.
- **Surgical Addition**: Appended Section 6.1 (Hover state 18x18px handle & top-right mono `EDIT` badge; selection outline `#96B6F8` & field perimeter inset `1.5px`; anchor ribbon `top: -5px`, `left: 16px`, `12x22px`, `#9E8CEA`, clip polygon `polygon(0 0, 100% 0, 100% 100%, 50% 72%, 0 100%)`), Section 6.2 (Distance Zoom Representation Tiers: Working 100% $\to$ Stepped back 24% $\to$ Far 6% stand-in), and Section 6.3 (Explicit Architectural Refusals: title bar + truncation refusal, persistent button bar refusal, interior scrollbar refusal).
- **Existing Content Preserved**: Lines 1–310 completely untouched.

### 5. `docs/specs/content-types/ADR-011-Document-Physical-Geometry-And-Reflow.md`
- **Section Modified**: Appended Section 7 (`Front-Page Zone Hierarchy, Distance Hysteresis & Refusal Contracts`).
- **Target Line Range**: Lines 365–392.
- **Surgical Addition**: Appended Section 7.1 (5 Front-Page Layout Zones: Mono front matter 11px 48%, Display title 38px 0.98, Hairline rule 1px 16%, Abstract 15px 1.65 34ch, Close 13px 1.60 50%, Title block mono pair 68%/50% in 28% border; paper inset shadow edge `inset 0 0 0 1px rgba(26,26,26,0.10)`), Section 7.2 (Neutral aura `234 234 234` `0.025`–`0.055`, selection `#96B6F8`, anchor ribbon `top: -5px`, `left: 20px`, `12x22px`, `#9E8CEA`), Section 7.3 (Distance Zoom Hysteresis & 96px paper sheet stand-in), and Section 7.4 (Document Architectural Refusals: ellipsis clip refusal, interior scrollbar refusal, dark file-card stand-in refusal).
- **Existing Content Preserved**: Lines 1–364 completely untouched.

### 6. `docs/specs/content-types/ADR-012-Image-Footprint-Resolution-Mapping.md`
- **Section Modified**: Appended Section 6 (`Interaction States, Distance Shedding & Image Refusal Invariants`).
- **Target Line Range**: Lines 287–306.
- **Surgical Addition**: Appended Section 6.1 (Quiet 1px edge `border: 1px solid rgba(234,234,234,0.16)`, GIF badge 9px mono `rgba(14,14,16,0.72)` & reduced-motion still frame contract, selection outline `#96B6F8` & field cell brightening `rgba(150,182,248,0.13)`, anchor mark 9x9px 45° rotated indigo diamond `#9E8CEA` at frame corner), Section 6.2 (Distance Zoom Shedding Tiers: Working $\to$ Stepped back surface shed $\to$ Far figure SVG stand-in on `#F5F5F5`), and Section 6.3 (Picture Architectural Refusals: cover crop refusal, caption overlay refusal, decorated keepsake card refusal).
- **Existing Content Preserved**: Lines 1–286 completely untouched.

### 7. `docs/specs/grid-systems/ADR-050-Footprint-Aware-Grid-Cursor-And-Trails.md`
- **Section Modified**: Section 5 (`Architectural Invariants & Refusal Assertions`).
- **Target Line Range**: Lines 298–308.
- **Surgical Addition**: Appended Item 5 (`Neutral Rest & Action Recoloring Protocol`: neutral `234 234 234` fill at rest, recolored ONLY when placing `#3B82F6`, moving/resizing `#F59E0B`, or tracing `#10B981`) and Item 6 (`Scale-Independent Cell Addressing`: world cell measurement, proportionally scaling across zoom levels 1% to 1000%, including when `linesVisible = false`).
- **Existing Content Preserved**: Lines 1–297 completely untouched.

### 8. `docs/specs/grid-systems/ADR-051-Interactive-Resize-And-Cell-Alignment.md`
- **Section Modified**: Section 5 (`Architectural Invariants & Refusal Protocol`).
- **Target Line Range**: Lines 250–258.
- **Surgical Addition**: Appended Item 4 (`Collision Refusal Cross-Hatching`: invalid role `#E2625C` / `#F06543` 12% fill, 45° diagonal cross-hatching `repeating-linear-gradient(45deg, rgba(226,98,92,0.22) 0 4px, transparent 4px 12px)`, inset border `inset 0 0 0 1px rgba(226,98,92,0.45)`) and Item 5 (`Local Point-of-Action Refusal Strip`: strip toolbar beside refused footprint "This space is occupied" with disabled `Place` action, refusing center-screen modal alerts).
- **Existing Content Preserved**: Lines 1–249 completely untouched.

### 9. `docs/specs/grid-systems/ADR-053-Spatial-CRUD-Operations-And-Selection.md`
- **Section Modified**: Appended Section 6 (`Signal Roles, Footprint Previewing & Selection Refusal Rules`).
- **Target Line Range**: Lines 221–240.
- **Surgical Addition**: Appended Section 6.1 (3 Signal Roles: Interaction `#96B6F8` selection/focus, Marquee `#E8B964` active selection gesture work, Invalid/Refusal `#E2625C` / `#F06543` spatial refusal), Section 6.2 (Placement Footprint Preview Contract: 1px dashed edge `rgba(150,182,248,0.80)`, 6% fill `rgba(150,182,248,0.06)`, mono corner size label `3 × 3`, origin untouched until release), and Section 6.3 (Selection & Placement Refusal Invariants: fill wash refusal, solid preview refusal, center-screen alert refusal).
- **Existing Content Preserved**: Lines 1–220 completely untouched.

### 10. `docs/specs/grid-systems/ADR-055-Multi-Item-Selection-And-Group-Translation.md`
- **Section Modified**: Appended Section 6 (`Signal Role Color Standards & Refusal Overlay Contracts`).
- **Target Line Range**: Lines 706–721.
- **Surgical Addition**: Appended Section 6.1 (Signal Role Color Matrix for group translation: Interaction `#96B6F8`, Marquee `#E8B964`, Refusal `#F06543` / `#E2625C` 12% fill & 45° cross-hatch) and Section 6.2 (Refusal Mechanics & Strip Toolbar Notification: origin retention, point-of-action strip toolbar "This space is occupied", no screen modal).
- **Existing Content Preserved**: Lines 1–705 completely untouched.

### 11. `docs/specs/grid-systems/ADR-056-Interactive-Resize-Geometry-And-Affordances.md`
- **Section Modified**: Appended Section 6 (`Interactive Resize Refusal & Point-of-Action Feedback Rules`).
- **Target Line Range**: Lines 465–479.
- **Surgical Addition**: Appended Section 6.1 (Collision Refusal Cross-Hatch Pattern: invalid role `#F06543` / `#E2625C` 12% fill, 45° diagonal cross-hatch stripes, inset border `inset 0 0 0 1.5px rgba(226,98,92,0.45)`) and Section 6.2 (Point-of-Action Refusal Strip Toolbar: "This space is occupied", `Cancel` / disabled `Place`, zero modal invariant).
- **Existing Content Preserved**: Lines 1–464 completely untouched.

---

## 4. Verification & Audit Integrity Statement

All 11 target ADR files were verified after modification. Zero existing characters, formulas, structs, code samples, or contracts were altered or deleted. All additions consist exclusively of appended gap-closing sections or items, ensuring complete mathematical, architectural, and design alignment between the design catalogue HTML decks and the normative specification suite.
