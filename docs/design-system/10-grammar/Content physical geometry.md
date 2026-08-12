---
type: design-system-grammar
status: normative
date: 2026-08-11
owner: "Grove Design System"
plane: information
---

# Content physical geometry

## Contract

| Property | Rule |
| --- | --- |
| Purpose | Convert intrinsic Content facts into demand, physical primitives, page capacity, and Annotation family selection. |
| Inputs | Normalized Tiptap JSON, intrinsic image dimensions, image role, Placement dimensions, source order, source proximity, field support, and usable Information dimensions. |
| Output | One qualified vertical, one tier, one form, one family, complete page sequence, and preserved active source. |
| Selection basis | Rendered geometry and intrinsic media facts. File bytes, DOM child count, CSS flex behavior, and arbitrary container nesting are not inputs. |
| Completeness | Authored text, authored structure, and complete image frames remain present in source order. |
| Overflow | Forbidden. A failed fit changes region, family, columns, pages, or form. It never clips, ellipsizes, crops, stretches, or hides content. |

## Normative constants

| Constant | Value | Use |
| --- | ---: | --- |
| `PAGE_RATIO_W` | `8` | Landscape unit width. |
| `PAGE_RATIO_H` | `5` | Landscape unit height. |
| `PAGE_MARGIN_PX` | `48` | Horizontal and vertical live-area margin per page edge. |
| `COLUMN_GAP_PX` | `18` | Gap between adjacent text columns or image regions. |
| `MIN_COLUMN_WIDTH_PX` | `160` | Minimum readable column or region width. |
| `MIN_PAGE_HEIGHT_PX` | `180` | Minimum complete page height. |
| `BODY_FONT_PX` | `16` | Reference body size for demand calculation. |
| `BODY_LINE_HEIGHT` | `1.55` | Reference reading line-height multiplier. |
| `REFERENCE_MEASURE_PX` | `340` | Reference readable measure for demand calculation. |
| `REFERENCE_LINES_PER_PAGE` | `18` | Reference text capacity for demand calculation. |
| `CSS_PX_PER_LOGICAL_INCH` | `96` | Conversion used by effective placed resolution. |
| `CHAR_WIDTH_FACTOR` | `0.52` | Reference average glyph width used for text measure calculation. |
| `TEXT_MIN_MEASURE_CH` | `34` | Minimum text measure for text-led composition. |
| `TEXT_MIN_WIDTH_PX` | `283` | `ceil(TEXT_MIN_MEASURE_CH × CHAR_WIDTH_FACTOR × BODY_FONT_PX)`. |

The constants in this table are v0.2.0 design-system values. They are not
user controls. Available Information dimensions scale the page unit and region
geometry; they do not replace the demand equations.

## Page unit

```text
unitWidthPx = min(availableWidthPx,
                  availableHeightPx × PAGE_RATIO_W / PAGE_RATIO_H)
unitHeightPx = unitWidthPx × PAGE_RATIO_H / PAGE_RATIO_W

liveWidthPx = unitWidthPx − 2 × PAGE_MARGIN_PX
liveHeightPx = unitHeightPx − 2 × PAGE_MARGIN_PX

requiredLiveWidthPx(columns, regionKind) =
  columns × regionMinWidthPx(regionKind)
  + (columns − 1) × COLUMN_GAP_PX
requiredUnitWidthPx(columns, regionKind) =
  requiredLiveWidthPx(columns, regionKind) + 2 × PAGE_MARGIN_PX

regionMinWidthPx(text) = max(MIN_COLUMN_WIDTH_PX, TEXT_MIN_WIDTH_PX)
regionMinWidthPx(image) = MIN_COLUMN_WIDTH_PX

familyFits = unitWidthPx ≥ requiredUnitWidthPx(columns, regionKind)
          ∧ unitHeightPx ≥ MIN_PAGE_HEIGHT_PX
```

The reading unit remains landscape at every usable width. A width reduction
changes columns, region sizes, horizontal advance, or page count. It never
changes the unit to portrait.

## Physical source records

### Text source

| Field | Derivation |
| --- | --- |
| `bodyLines` | Wrapped body blocks at the reference measure and body type. |
| `headingLines` | Wrapped authored heading/title blocks only. |
| `blankUnits` | Explicit hard breaks and blank-line units. |
| `paragraphCost` | `0.65 × paragraphBreakCount`. |
| `lineUnits` | `bodyLines + headingLines + blankUnits + paragraphCost`. |
| `columnUnits` | `ceil(lineUnits / REFERENCE_LINES_PER_PAGE)`. |
| `Tᵢ` | `max(0.25, lineUnits / REFERENCE_LINES_PER_PAGE)`. |
| `renderedHeightPx` | `lineUnits × BODY_FONT_PX × BODY_LINE_HEIGHT`. |
| `renderedAreaPx²` | `renderedHeightPx × REFERENCE_MEASURE_PX`. |

```text
charsPerLine = max(8,
  floor(REFERENCE_MEASURE_PX /
        max(6, CHAR_WIDTH_FACTOR × BODY_FONT_PX)))
```

Source titles and headings are optional data. Absence produces zero heading
lines. No page-authored title, masthead, issue line, standfirst, summary, or
headline is created.

### Image source

| Field | Derivation |
| --- | --- |
| `wPx`, `hPx` | Intrinsic source pixel dimensions. |
| `aspect` | `r = wPx / hPx`. |
| `pixelReserve` | `clamp(0.70, 1.50, 0.18 × √(wPx × hPx) / F)`. |
| `aspectReserve` | `clamp(0.00, 0.45, 0.12 × |ln(r)|)`. |
| `roleReserve` | Hero `+0.35`; supporting `0`; thumbnail `−0.18`. |
| `Iᵢ` | `max(0.45, pixelReserve + aspectReserve + roleReserve)`. |
| `placedWidthIn` | `frameWidthPx / CSS_PX_PER_LOGICAL_INCH`. |
| `placedHeightIn` | `frameHeightPx / CSS_PX_PER_LOGICAL_INCH`. |
| `PPI_eff` | `min(wPx / placedWidthIn, hPx / placedHeightIn)`. |

Fidelity floors:

| Role | `F` |
| --- | ---: |
| Thumbnail | `96` |
| Supporting | `180` |
| Hero / leading | `320` |

Aspect classification is exact and ordered:

| Class | Predicate |
| --- | --- |
| Extreme panorama | `r ≥ 2.0`. |
| Landscape | `1.0 < r < 2.0`. |
| Square | `r = 1.0`. |
| Portrait | `0.0 < r < 1.0`. |

The class is a selector input. It never authorizes cropping, squaring,
stretching, or portrait page substitution.

### Complete-frame projection

For a source image of intrinsic dimensions `(wPx, hPx)` and a candidate frame
of `(frameWidthPx, frameHeightPx)`:

```text
scale = min(frameWidthPx / wPx, frameHeightPx / hPx)
renderedWidthPx = wPx × scale
renderedHeightPx = hPx × scale
unusedWidthPx = frameWidthPx − renderedWidthPx
unusedHeightPx = frameHeightPx − renderedHeightPx
```

The frame is complete when `scale > 0` and both unused dimensions are
non-negative. Unused space belongs to the page field. It is not filled by
distorting the source.

## Set demand and ratios

```text
D = ΣTᵢ + ΣIᵢ + ΣOᵢ
textShare = ΣTᵢ / D
imageShare = ΣIᵢ / D
otherShare = ΣOᵢ / D
```

`O = 0` for the v0.2.0 supported media boundary.

The set-level resolver uses the shared Annotation gate, vertical boundaries,
and tier bands. Page-local image-to-text balance is calculated only after the
form has been selected:

```text
Rmedia(page) = imageAreaPx² /
               (imageAreaPx² + textAreaPx²)
```

`Rmedia(page)` selects region allocation inside a form. It never changes the
set-level vertical, tier, or form.

## Physical family selection

### Candidate compilation

For every family allowed by the resolved form:

1. Read the family columns, source capacity, text measures, image slots, and
   orientation from `30-components/Annotation-layout-families.md`.
2. Construct the landscape unit at the current usable Information dimensions.
3. Allocate source primitives in source order to the family regions.
4. Compute text line capacity, complete image frames, effective resolution,
   page-local `Rmedia`, and page count.
5. Reject the candidate if any authored block or complete frame cannot be
   assigned without a forbidden operation.
6. Retain the candidate if every source remains ordered, complete, and
   addressable.

### Candidate ordering

Candidate ordering is lexicographic. No CSS layout result is used as a
selector input.

| Priority | Comparison |
| ---: | --- |
| 1 | Candidate satisfies all source, frame, fidelity, and orientation predicates. |
| 2 | Candidate matches the form's physical family predicate. |
| 3 | Candidate preserves the minimum readable text measure: `≥ 34ch` and `≥ 283px` at reference body type. |
| 4 | Candidate preserves the minimum image fidelity floor for every displayed role. |
| 5 | Candidate uses the fewest complete pages or leaves for the current form. |
| 6 | Candidate uses the family order in the form precedence table. |

### Density objective

The resolver maximizes complete authored information inside the current
landscape unit subject to readability and fidelity floors.

```text
valid(candidate) =
  completeText(candidate)
  ∧ everyTextMeasure(candidate) ≥ 34ch
  ∧ completeImages(candidate)
  ∧ everyImagePPI(candidate) ≥ roleFloor

preferred(candidate) = lexicographic maximum of:
  valid(candidate)
  completeSourceArea(candidate) / completePageArea(candidate)
  minimumReadableTextMeasure(candidate)
  minimumImagePPI(candidate)
  negativeCompletePageCount(candidate)
```

No candidate is preferred because it fits more sources by making text
unreadable or images too small to carry their intended detail. More source
material creates more columns, leaves, or pages after the floors are reached.

### Form precedence

| Form | Family precedence |
| --- | --- |
| Bulletin | `T1-A → T1-B → T1-C`. |
| Berliner | `T2-A → T2-C → T2-B`. |
| Broadsheet | `T3-A → T3-C → T3-B`. `T3-B` receives the threaded predicate before page-count comparison. |
| Brochure | `B-01 → B-02 → B-03 → B-04`. Hero/reveal demand selects `B-03` before ordinary panel packing. |
| Pamphlet | `P-01 → P-03 → P-02 → P-04`. A continuation predicate selects `P-04` before page-count comparison. |
| Magazine | `M-01 → M-02 → M-03 → M-04`. Long-run pagination selects `M-04` before page-count comparison. |
| Gallery | `I1-A → I1-B → I1-C`. |
| Contact sheet | `I2-A → I2-B → I2-C`. |
| Image edition | `I3-A → I3-B → I3-D → I3-C → I3-E`. Long-run pagination selects `I3-E` before page-count comparison. |

### Exact family predicates

`N` is source count. `M` is image count. `C` is total text column units.
`Lmax` is the largest single-source text column-unit cost. `T` is total text
demand. `I` is total image demand. `H` is the authored-heading presence bit.
`P` is the panorama presence bit. `F` is the complete-frame predicate.

| Family | Predicate before complete-fit evaluation |
| --- | --- |
| `T1-A` | `N = 1 ∧ C ≤ 1`. |
| `T1-B` | `N ≤ 2 ∧ C = 2`. |
| `T1-C` | `N ≥ 2 ∧ C = 3`. |
| `T2-A` | `C ≤ 2 ∧ Lmax ≤ 1 ∧ H = 0`. |
| `T2-B` | `C = 4 ∨ (N ≥ 2 ∧ source continuation crosses a leaf)`. |
| `T2-C` | `N ≥ 2 ∧ Lmax < C ∧ C ≤ 3`. |
| `T3-A` | `C ≥ 4 ∧ Lmax ≤ 6 ∧ N ≥ 2`. |
| `T3-B` | `Lmax > 6 ∨ one source requires continuation across a page`. |
| `T3-C` | `C ≥ 4 ∧ N ≥ 3 ∧ Lmax ≤ 6 ∧ T3-A does not preserve independent source blocks`. |
| `B-01` | `N = 1 ∧ M ≤ 1`. |
| `B-02` | `2 ≤ N ≤ 6 ∧ no hero/reveal predicate`. |
| `B-03` | `hero image exists ∧ F = true for the centre reveal region`. |
| `B-04` | `N ≥ 4 ∧ no single leading source`. |
| `P-01` | `M = 0 ∧ C ≤ 1`. |
| `P-02` | `M ≥ 1 ∧ T > 0 ∧ image/text pairing exists`. |
| `P-03` | `M = 0 ∧ C ≥ 2 ∧ Lmax ≤ 2`. |
| `P-04` | `Lmax > 2 ∨ one source requires stitched continuation`. |
| `M-01` | `M = 1 ∧ hero image exists ∧ T = 0`. |
| `M-02` | `M ≥ 1 ∧ T > 0 ∧ M ≤ 1`. |
| `M-03` | `M ≥ 2 ∧ every frame is complete within one spread`. |
| `M-04` | `M > spread image capacity ∨ Lmax > 3 ∨ one source requires threaded pagination`. |
| `I1-A` | `M = 1 ∧ caption/facts absent`. |
| `I1-B` | `M = 1 ∧ caption/facts present`. |
| `I1-C` | `M = 2 ∧ both frames are complete within one gallery sheet`. |
| `I2-A` | `M ≥ 3 ∧ all images share one aspect class`. |
| `I2-B` | `M ≥ 3 ∧ aspect classes differ ∧ no facts rail is required`. |
| `I2-C` | `M ≥ 3 ∧ facts rail is required by source facts or index demand`. |
| `I3-A` | `M = 1 ∧ T = 0`. |
| `I3-B` | `M = 2 ∧ T = 0`. |
| `I3-C` | `M ≥ 3 ∧ T = 0 ∧ M ≤ spread image capacity`. |
| `I3-D` | `M ≥ 1 ∧ T > 0`. |
| `I3-E` | `M > spread image capacity ∨ one source requires sequence pagination`. |

`source continuation crosses a leaf`, `one source requires continuation across
a page`, `one source requires stitched continuation`, `one source requires
threaded pagination`, `image/text pairing exists`, `single leading source`,
`spread image capacity`, `facts rail is required`, and `independent source
blocks` are derived fields. They are not visual guesses:

| Derived field | Calculation |
| --- | --- |
| `source continuation crosses a leaf` | Source `columnUnits` exceeds the current leaf capacity. |
| `one source requires continuation across a page` | `max(source.columnUnits) > pageColumnCapacity`. |
| `one source requires stitched continuation` | Source line units exceed one leaf's complete line capacity. |
| `one source requires threaded pagination` | Source line units exceed one spread's complete line capacity. |
| `image/text pairing exists` | An image and text source share the same source relationship record and page allocation. |
| `single leading source` | One source has the highest physical demand and all remaining sources fit the supporting regions. |
| `spread image capacity` | Number of complete image slots in the candidate spread. |
| `facts rail is required` | Source order or source count requires an index/provenance rail to preserve unique frame identity. |
| `independent source blocks` | Source blocks have distinct source identities and cannot share one threaded text flow. |

## Text columns and page breaks

### Column types

| Type | Use |
| --- | --- |
| Body | Continuous authored text at the fixed readable measure. |
| Source start | First fragment of a source; authored title/heading appears only when present. |
| Continuation | Later fragment of the same source; source identity and continuation position are retained. |
| Terminal | Final fragment of a source; unused height remains empty. |
| Empty | Unused physical page area; no filler text or placeholder block. |

### Broadsheet rule

Broadsheet composition is body-first. It has no generated headlines, page title,
masthead, issue line, standfirst, group heading, or summary. Source-authored
titles and headings remain optional source content; absent titles produce no
reserved title row.

### Text placement algorithm

```text
for source in sourceOrder:
  for block in source.tiptapBlocks:
    measure block at the fixed readable measure
    while block has remaining line units:
      if remainingColumnLines = 0:
        advance to the next column
      if no column remains on the current page:
        create the next complete page
      place the largest complete line slice that fits
      preserve marks, hard breaks, block identity, and source identity
```

Page creation occurs only when the current family has no remaining column or
region that can accept the next complete line slice. A page never ends because
a headline, decorative fill, or arbitrary padding was inserted.

## Image placement algorithm

```text
for image in sourceOrder:
  classify image by intrinsic aspect
  enumerate the candidate family's complete frame regions
  calculate scale, rendered dimensions, unused dimensions, and PPI_eff
  reject regions below the image role fidelity floor
  select the first complete region in source order
  if no region remains:
    advance to the next permitted row, page, or family
```

Rules:

| Condition | Result |
| --- | --- |
| `r ≥ 2.0` | Give the panorama the widest permitted complete region; the row/page holds fewer sources. |
| `r < 1.0` | Preserve portrait height and provide support beside or around it; do not rotate the page. |
| `r = 1.0` | Preserve equal frame dimensions and equal surrounding field. |
| `PPI_eff < F` | Reduce the complete frame, change region/family, add page, or expose Image Viewer. |
| No complete region | Add page or select another family; never crop or stretch. |

## Mixed-media page balance

Mixed-media pages allocate text and image regions after form and family
selection:

```text
imageAreaPx² = Σ complete rendered image frame areas
textAreaPx² = Σ rendered text region areas
Rmedia(page) = imageAreaPx² / (imageAreaPx² + textAreaPx²)
```

The page may change region widths, column count, or page count to satisfy the
physical primitives. `Rmedia(page)` does not change the ledger's vertical or
form.

## Reflow and hysteresis

| Input change | Recomputed values |
| --- | --- |
| Text edit | Tiptap blocks, line units, `Tᵢ`, `D`, family, columns, pages. |
| Image replacement | `wPx`, `hPx`, `r`, aspect class, `Iᵢ`, `PPI_eff`, family, pages. |
| Role change | Fidelity floor, `Iᵢ`, complete-frame eligibility, family, pages. |
| Source add/remove | Source order, demand, shares, gate, vertical, tier, form, family, pages. |
| Usable dimensions | Page unit, fit predicates, frame sizes, columns, regions, pages. |
| Availability change | Reserved source frame and recovery state; committed order and form remain after qualification. |
| Boundary equality | Current form and family remain. |
| Boundary crossing | Run the complete candidate compilation before changing form or family. |

The last complete edition remains visible while physical compilation is
pending. A failed compilation produces the explicit recovery state and never a
blank, clipped, partially composed, or silently simplified page.

## Refusals

| Refused operation | Required result |
| --- | --- |
| Generated headline or title | Omit it; use only authored source title/heading. |
| Type reduction | Add columns/pages, select another family, or promote. |
| Column overflow | Thread complete source content into the next column/page. |
| Page overflow | Create the next complete page. |
| Image crop | Change region/family or add page. |
| Image stretch | Preserve intrinsic aspect and surrounding field. |
| Low-resolution enlargement | Reduce frame or route to Image Viewer. |
| Empty image/text slot | Close the region; do not insert placeholder copy or filler. |
| Metadata substitution | Preserve source identity and provenance; never invent caption, title, or issue text. |
| CSS-only family decision | Use the physical resolver output; DOM geometry is conformance evidence, not design authority. |

## Assertions

| ID | Assertion |
| --- | --- |
| CPG-01 | Every supported source has a physical record before demand is calculated. |
| CPG-02 | Image demand uses intrinsic pixels, aspect ratio, role, and effective placed resolution. |
| CPG-03 | `4K`, `1080p`, and `720p` sources with equal aspect ratio retain different pixel reserves but the same aspect class. |
| CPG-04 | Ten images are evaluated individually, then aggregated into `I`, `D`, and `imageShare`; item count never substitutes for demand. |
| CPG-05 | Every family is selected by a named predicate, complete-fit evaluation, and fixed precedence. |
| CPG-06 | Broadsheet does not require a title or headline and never creates one. |
| CPG-07 | Broadsheet page breaks occur at exhausted column capacity, not at decorative or synthetic chrome. |
| CPG-08 | Every image frame is complete, aspect-preserving, and fidelity-bounded. |
| CPG-09 | No authored text is truncated, summarized, ellipsized, or hidden. |
| CPG-10 | `Rmedia(page)` affects only page-local composition. |
| CPG-11 | Reflow retains source order and the active source whenever the source remains in the edition. |
| CPG-12 | No family decision depends on CSS fill, flex growth, span count, container nesting, or arbitrary padding. |

## Sources

| Source class | Source |
| --- | --- |
| Shared grammar | `10-grammar/Content-concentration.md`; `10-grammar/Annotation-ledger.md`. |
| Family registry | `30-components/Annotation-layout-families.md`. |
| Text vertical | `30-components/Text-led-annotation.md`; `docs/versions/0.2.0/design_catalogue/src/information-plane/06-annotation-text-led.html`. |
| Mixed-media vertical | `30-components/Mixed-media-annotation.md`; `docs/versions/0.2.0/design_catalogue/src/information-plane/05-annotation-mixed-media.html`. |
| Image vertical | `30-components/Image-led-annotation.md`; `docs/versions/0.2.0/design_catalogue/src/information-plane/07-annotation-image-led.html`. |
| Reader geometry | `30-components/Annotation.md`. |
| Source intent | `docs/versions/0.1.0/raw/original-notes/Grove - Annotations UX.txt`; `Grove - annotations.txt`; `Grove - Documents.txt`. |
