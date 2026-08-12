---
type: design-reference
status: working
date: 2026-08-09
tags: [grove, annotations, text-led, newspapers, bulletin, berliner, broadsheet]
---

# Text-led physical forms

This reference describes the physical newspaper and newsletter forms used to
ground text-led annotation tiers. It defines the media analogues, their layout
behavior, and the content-capacity implications. It is research, not an
implementation contract.

## Source boundary

The annotation source notes establish the following constraints:

- Rich text is a primitive and its amount contributes to content weight.
- Proximity groups nearby content before a page is composed.
- The page layout is dictated by content demand.
- Text is never truncated or overflowed. More text creates continuation and
  more pages.
- The original annotation language is broadsheet-inspired, but a text-led
  vertical still needs smaller and medium physical forms before it reaches a
  broadsheet.

Primary source notes: [`Grove - Annotations UX.txt`](../../raw/original-notes/Grove%20-%20Annotations%20UX.txt) and [`Grove - annotations.txt`](../../raw/original-notes/Grove%20-%20annotations.txt).

## The three physical references

### Tier 01: bulletin / bullet-sheet newsletter

The small form is a bulletin or bullet-sheet newsletter, not a miniature
broadsheet. The University of Florida IFAS describes a bullet-sheet newsletter
as one sheet printed front and back, with short articles of roughly one or two
paragraphs. It describes ordinary newsletters as four to eight pages and
notes that one, two, three, or four columns commonly distinguish newsletter
designs.

The physical behavior is therefore:

- one sheet or one short landscape reading unit;
- one to three readable measures, with a fourth only when the text remains
  comfortably readable;
- short independent items or one short continuous source;
- front/back or one-page ending before a threaded newspaper sequence is needed;
- a small publication mark may exist in a real bulletin, but it is not a
  generated title for the source content.

Tier 01 is the smallest text-led form because it limits both page area and
continuation. It is useful for a few short paragraphs, several short entries,
or a small cluster whose sources can be read together without a second leaf.

### Tier 02: Berliner / compact newspaper

The middle form is a Berliner or compact newspaper. The Guardian Print Centre
describes Berliner as a format between tabloid and broadsheet, with a page
dimension of 470mm by 315mm, sold folded. Compact is a related middle-size
newspaper grammar: it keeps newspaper-quality composition while reducing the
physical page size.

The physical behavior is:

- a wider-than-tall open reading unit made from one or two landscape pages;
- stable columns and gutters across a page or spread;
- more than one source or a source that needs a short continuation;
- a consistent reading order from column to column and page to page;
- enough area for text to remain text, rather than collapsing into a card or
  being reduced to a label.

Berliner is the appropriate middle reference because it is not merely “more
columns.” Its physical page is smaller than a broadsheet but still supports a
newspaper reading system. The grid tightens; the reading grammar does not
become a portrait book block.

### Tier 03: broadsheet newspaper

The large form is the traditional broadsheet. Newspaper Club gives a
traditional broadsheet page as 380mm wide by 578mm high, with the physical
paper folded through the center. The open spread is therefore much wider than
it is tall, while each folded page is tall and narrow.

The physical behavior is:

- a large landscape spread or a large folded newspaper page;
- many narrow, repeatable columns with clear gutters;
- multiple sources, long sources, or both;
- threaded continuation across columns and additional pages;
- page folios and continuation markers that identify sequence, not invented
  source titles;
- a quiet gutter and protected live area where binding or folding can damage
  text.

Broadsheet is the highest-capacity form, not the default appearance of every
text-led annotation. Its scale earns the larger reading field. A short source
should not be promoted into a broadsheet merely to make it look editorial.

## Tier map

| Tier | Physical analogue | Reading unit | Content behavior |
| --- | --- | --- | --- |
| 01 | Bulletin / bullet sheet | one short landscape sheet | short text, few entries, no thread |
| 02 | Berliner / compact | landscape page or facing spread | moderate text, stable columns, short continuation |
| 03 | Broadsheet newspaper | large landscape spread and pages | dense text, many sources, threaded continuation |

The names describe physical grammars, not fixed screen dimensions. A digital
annotation may use a proportional landscape surface while preserving the
relationship between the three forms.

## Layout families

The tier is the capacity band. The page layout is selected inside the tier.

### Tier 01 layouts

**T1-A: single measure bulletin**

Use for one short source or one compact cluster. The reading starts at the body
measure and ends on the same sheet. No title block is required. If source
metadata supplies a title, it can be shown as source data; it is not invented
by the layout.

**T1-B: two-measure bulletin**

Use for a source with enough text to benefit from a second measure or for two
nearby short sources. The columns are short and stable. The second measure is
not a broadsheet gesture; it is the smallest newspaper-like way to preserve
readability on one sheet.

**T1-C: three-measure bulletin**

Use when several short entries belong together. Each entry keeps its boundary;
the page does not concatenate independent sources into one article. If the
entries become long enough to thread, promote to Tier 02.

### Tier 02 layouts

**T2-A: compact single page**

Use for a moderate source set that fits one Berliner-like landscape page. A
stable two- or three-column grid provides newspaper structure without using
the scale of a broadsheet.

**T2-B: facing compact spread**

Use when the source continues beyond one page or when proximity binds several
sources into a larger reading field. Each page keeps the same column rhythm;
the center seam is a continuation boundary, not a decorative book spine.

**T2-C: compact modular page**

Use when one source needs a dominant text region and a second source needs a
smaller adjacent region. The modules remain text-first. An optional image is
supporting material, not a forced image slot.

### Tier 03 layouts

**T3-A: broadsheet field**

Use for a dense set that needs several narrow measures on one large spread.
Columns remain aligned to a shared baseline and the reading order remains
explicit.

**T3-B: threaded broadsheet spread**

Use for one long source or a connected set of sources. Text flows from frame to
frame and page to page. The layout adds pages instead of reducing type or
cutting the source at the fold.

**T3-C: modular broadsheet page**

Use for many independent sources that require separate blocks within one large
page. Each block retains its source boundary. The grid provides order without
inventing a single publication headline for the cluster.

## What newspaper research contributes

### Columns are a reading device

The column is not decoration. It controls line length, vertical rhythm, and
the order in which a reader moves through a large amount of text. A smaller
form can use fewer columns; a larger form can use more, but additional columns
must not make the measure too narrow to read.

### The open spread is the useful landscape unit

A broadsheet is folded physically, but its open reading field is a wide spread.
Berliner is likewise experienced as a folded newspaper format with a smaller
page. For annotations, the landscape relationship should be preserved in the
reading surface even when the physical analogue has a fold or a tall individual
page.

### Continuation is physical, not decorative

Newspaper pages use page numbers, continuation cues, and column order because
the content exceeds one local frame. These cues identify where the source
continues. They do not supply a title, summarize missing text, or authorize
truncation.

### Publication chrome is not source chrome

Real newspapers and newsletters may have a masthead, nameplate, issue date,
or section label. Those belong to the publication as a whole. They must not be
confused with a generated title for every source in an annotation. A source
without title metadata begins with its body text.

## Content-capacity model

Text-led routing should measure rendered demand rather than raw character
count or source count.

### Text demand

1. Normalize the source while preserving paragraphs, lists, and explicit
   breaks.
2. Render it at the minimum readable body measure for the candidate form.
3. Measure line count, line height, paragraph spacing, and continuation space.
4. Sum the occupied area as `A_text`.

The useful density value is a comparison between `A_text` and the usable area
of a candidate reading unit. A practical form is:

`D_text = A_text / A_unit`

The existing catalogue's numeric bands are provisional routing hypotheses. The
physical references support the direction of the progression, but they do not
prove a universal value for every viewport, font, or source language. Those
values require rendered evidence.

### Source count

Source count is a separate input. Five short notices may fit Tier 01 while one
long source may require Tier 02 or Tier 03. A high count of short sources may
also require pagination when proximity and source boundaries prevent them from
sharing a page.

### Promotion rules

- Promote within the same tier when another layout family can contain the
  complete source at a readable measure.
- Promote from bulletin to Berliner when the source requires a spread or short
  threaded continuation.
- Promote from Berliner to broadsheet when the source set needs a large field,
  many columns, or sustained pagination.
- Add pages before reducing type, narrowing measure below readability, or
  cutting text.

## Non-text inputs

An optional image can appear beside text when proximity establishes that
relationship. It must not force the text-led surface into an image-led form.
Image quality, aspect ratio, and complete-frame rules belong to the image-led
reference. PDF and EPUB remain long-form source types rather than being
flattened into newspaper primitives.

## References

- [University of Florida IFAS: Brochures and Newsletters](https://ask.ifas.ufl.edu/publication/WC131) - bullet-sheet newsletters, page counts, and column patterns.
- [The Guardian Print Centre: Berliner Format](https://www.theguardian.com/gpc/berliner-format) - Berliner dimensions and its position between tabloid and broadsheet.
- [Newspaper Club: Traditional Broadsheet Artwork Guidelines](https://www.newspaperclub.com/create/artwork-guidelines/traditional-broadsheet) - broadsheet page dimensions and physical setup.
- [Adobe: Magazine Layout](https://helpx.adobe.com/ph_fil/indesign/how-to/design-magazine-layout.html) - columns, guides, frames, and threaded text as a related print-layout reference.
- [Adobe: Grids](https://helpx.adobe.com/africa/indesign/using/grids.html) - baseline and document grids.
