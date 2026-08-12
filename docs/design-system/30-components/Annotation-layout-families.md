---
type: design-system-component
status: normative
date: 2026-08-11
plane: information
component: annotation-layout-families
---

# Annotation layout families

Family selection is owned by
[`10-grammar/Content physical geometry.md`](../10-grammar/Content%20physical%20geometry.md).
This registry owns family capacity and fit data; it does not permit an
implementation-specific selector.

## Registry

| Family | Vertical | Form | Tier | Columns | Source capacity per page | Text measures | Image slots | Orientation |
| --- | --- | --- | --- | ---: | ---: | ---: | ---: | --- |
| T1-A | Text-led | Bulletin | Tier 01 | 1 | 1 | 1 | 0 | Landscape |
| T1-B | Text-led | Bulletin | Tier 01 | 2 | 2 | 2 | 0 | Landscape |
| T1-C | Text-led | Bulletin | Tier 01 | 3 | 3 | 3 | 0 | Landscape |
| T2-A | Text-led | Berliner | Tier 02 | 2 | 2 | 2 | 0 | Landscape |
| T2-B | Text-led | Berliner | Tier 02 | 4 | 4 | 4 | 0 | Landscape |
| T2-C | Text-led | Berliner | Tier 02 | 2 | 3 | 3 | 0 | Landscape |
| T3-A | Text-led | Broadsheet | Tier 03 | 4 | 6 | 6 | 0 | Landscape |
| T3-B | Text-led | Broadsheet | Tier 03 | 2 | 1 | 4 | 0 | Landscape |
| T3-C | Text-led | Broadsheet | Tier 03 | 4 | 6 | 4 | 0 | Landscape |
| B-01 | Mixed-media | Brochure | Tier 01 | 1 | 1 | 1 | 1 | Landscape |
| B-02 | Mixed-media | Brochure | Tier 01 | 2 | 2 | 2 | 2 | Landscape |
| B-03 | Mixed-media | Brochure | Tier 01 | 2 | 2 | 2 | 2 | Landscape |
| B-04 | Mixed-media | Brochure | Tier 01 | 3 | 3 | 3 | 3 | Landscape |
| P-01 | Mixed-media | Pamphlet | Tier 02 | 1 | 1 | 1 | 1 | Landscape |
| P-02 | Mixed-media | Pamphlet | Tier 02 | 2 | 2 | 2 | 2 | Landscape |
| P-03 | Mixed-media | Pamphlet | Tier 02 | 2 | 2 | 2 | 2 | Landscape |
| P-04 | Mixed-media | Pamphlet | Tier 02 | 1 | 1 | 1 | 1 | Landscape |
| M-01 | Mixed-media | Magazine | Tier 03 | 1 | 1 | 1 | 1 | Landscape |
| M-02 | Mixed-media | Magazine | Tier 03 | 2 | 2 | 2 | 2 | Landscape |
| M-03 | Mixed-media | Magazine | Tier 03 | 3 | 4 | 3 | 4 | Landscape |
| M-04 | Mixed-media | Magazine | Tier 03 | 3 | 3 | 3 | 3 | Landscape |
| I1-A | Image-led | Gallery | Tier 01 | 1 | 1 | 0 | 1 | Landscape |
| I1-B | Image-led | Gallery | Tier 01 | 1 | 1 | 0 | 1 | Landscape |
| I1-C | Image-led | Gallery | Tier 01 | 2 | 2 | 0 | 2 | Landscape |
| I2-A | Image-led | Contact sheet | Tier 02 | 3 | 4 | 0 | 4 | Landscape |
| I2-B | Image-led | Contact sheet | Tier 02 | 3 | 3 | 0 | 3 | Landscape |
| I2-C | Image-led | Contact sheet | Tier 02 | 3 | 3 | 0 | 3 | Landscape |
| I3-A | Image-led | Image edition | Tier 03 | 1 | 1 | 0 | 1 | Landscape |
| I3-B | Image-led | Image edition | Tier 03 | 2 | 2 | 0 | 2 | Landscape |
| I3-C | Image-led | Image edition | Tier 03 | 3 | 3 | 0 | 3 | Landscape |
| I3-D | Image-led | Image edition | Tier 03 | 2 | 2 | 1 | 1 | Landscape |
| I3-E | Image-led | Image edition | Tier 03 | 3 | 4 | 0 | 4 | Landscape |

## Fit predicate

```text
requiredWidth = columns × 160px
              + (columns − 1) × 18px
              + 2 × 48px
requiredHeight = 180px
familyFits = availableWidth ≥ requiredWidth
           ∧ availableHeight ≥ requiredHeight
```

| Value | Contract |
| --- | --- |
| Minimum column width | `160px` |
| Column gap | `18px` |
| Page side margin | `48px` per side |
| Minimum page height | `180px` |
| Orientation | Landscape `8:5`; text families require `≥ 34ch` and `≥ 283px` at reference body type; image families require complete frames; a fit failure changes family, columns, or pages. |

## Selection order

1. Resolve source demand, shares, qualification, vertical, and tier.
2. Select the preferred family from source count, text demand, image demand, image share, aspect variance, and caption presence.
3. Preserve the committed family at exact boundary equality.
4. If the preferred family fails `familyFits`, select the first fitting family in the same form.
5. Paginate complete source pieces in source order by source capacity.
6. Preserve authored text and complete image frames; never truncate, crop, stretch, or reduce the readable measure.

## Reflow inputs

| Input change | Recomputed values |
| --- | --- |
| Source membership | Source set, demand, shares, vertical, tier, form, family, pages |
| Text document | Wrapped demand, tier, family, pages |
| Image metadata | Image demand, aspect variance, family, pages |
| Information usable width or height | Fit predicate, family, columns, pages |
| Source availability | Reserved source frame and recovery state; source order retained |
| Boundary equality | Current form and family retained |
| Boundary crossing | New complete-fit evaluation required before switching form |

## Tiptap text contract

| Area | Contract |
| --- | --- |
| Source text | Tiptap JSON document stored in the Memory payload |
| Reader projection | Read-only Tiptap instance |
| Pagination | Rich-text slices retain block structure, inline marks, and hard breaks |
| Plain-text demand | Derived from the normalized Tiptap document; storage bytes are not used |
| Writing route | Source-owned route opens the shared Writing Slate Tiptap editor |

## Assertions

| ID | Assertion |
| --- | --- |
| ALF-01 | Registry count is exactly `32`. |
| ALF-02 | Text-led count is `9`; Mixed-media count is `12`; Image-led count is `11`. |
| ALF-03 | Every family declares columns, source capacity, text measures, image slots, and landscape orientation. |
| ALF-04 | A family changes only after a complete-fit predicate or an explicit boundary re-resolution permits it. |
| ALF-05 | Pagination retains source order and complete authored content. |
| ALF-06 | Tiptap JSON is the only rich-text editor and reader projection format. |
| ALF-07 | Every family is selected through a physical predicate and fixed precedence in `Content physical geometry.md`. |
| ALF-08 | Family selection never depends on CSS fill, flex growth, span count, container nesting, or arbitrary padding. |
| ALF-09 | A candidate is invalid when readable text measure falls below `34ch`/`283px` or an image falls below its role fidelity floor. |
| ALF-10 | Additional content creates additional columns, leaves, or pages after readability and fidelity floors are satisfied. |
