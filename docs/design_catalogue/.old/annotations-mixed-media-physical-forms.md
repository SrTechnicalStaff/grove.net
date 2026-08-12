---
type: design-reference
status: working
date: 2026-08-09
tags: [grove, annotations, mixed-media, brochure, pamphlet, magazine, layout]
---

# Mixed-media physical forms

This reference defines the physical-media grammars behind the mixed-media
annotation catalogue. It is a design reference, not a content summary and not
a replacement for the source notes.

## Source boundary

The source notes establish the product rules:

- Mixed-media is selected from the relative weight of text and images.
- Brochure, pamphlet, and magazine are increasing content forms.
- Proximity groups nearby content before page composition.
- The local image-to-text relationship is checked again for each page.
- Content is never truncated or overflowed. More demand creates more pages or
  the next form.
- PDF and EPUB are separate long-form sources and are not treated as primitive
  mixed-media pages here.

The source notes give `60-70% media / 30-40% text` as a starting qualification
band. That is a calibration input, not a universal threshold. The catalogue
therefore separates vertical routing from layout selection.

Primary source notes: [`Grove - Annotations UX.txt`](../../raw/original-notes/Grove%20-%20Annotations%20UX.txt) and [`Grove - annotations.txt`](../../raw/original-notes/Grove%20-%20annotations.txt).

## Physical form findings

### Brochure

A brochure is a short, unbound, usually promotional or explanatory printed
piece. Its defining behavior is not a single page size; it is the sheet,
panel, and fold relationship. A brochure can be a single sheet, a tri-fold,
gatefold, or accordion/concertina fold. The reader encounters panels as a
sequence, then may open the sheet to see a wider composition.

Adobe's print model treats brochure panels as separate pages in a consecutive
2-up, 3-up, or 4-up arrangement. A tri-fold is therefore six reading panels,
not one page with three arbitrary CSS columns. The catalogue models the panel
sequence and the revealed spread separately.

The U.S. Government Publishing Office's fold catalogue confirms the physical
panel counts: a half-fold creates four panels, a tri-fold creates six, a gate
fold creates six with a larger center reveal, a Z-fold creates six in a zig-zag
sequence, and a roll fold creates eight with successive inward folds. These
are different reading geometries, so they remain different brochure templates.

Brochure rules carried into the catalogue:

- Use when the grouped material can be understood as one short sequence.
- Keep each panel bounded. A panel that cannot contain its text creates a new
  panel or moves to pamphlet pagination.
- Use a fold to create reveal, order, and pacing; do not use it as a reason to
  shrink type.
- Allow an image to lead a panel, share a panel with text, or occupy a reveal.
- Do not force a headline, caption, or source label into an empty panel.
- Preserve an image's complete frame. A low-resolution image becomes smaller,
  bounded, or supporting; it is not enlarged until it becomes soft.

### Pamphlet

A pamphlet is a short sequence of leaves rather than a foldout panel strip.
The Library of Congress describes pamphlets as non-serial publications usually
between five and forty-eight pages. The physical book-art model is useful even
when the final object is digital: one sheet folded once makes a folio with four
pages, and multiple folios can be stacked and stitched.

Pamphlet rules carried into the catalogue:

- Use when the material needs a short, ordered reading path across leaves.
- A portrait leaf is the default specimen, but orientation follows the source
  composition; portrait is not a fixed chrome shell.
- Let text continue across leaves at a readable measure. The end of a leaf is
  a pagination boundary, not a truncation boundary.
- Keep an image beside the text it explains when proximity supports that
  relationship; give it a plate leaf when the image needs more area.
- A pamphlet can contain one source, several sources, or no image at all.
- The number of sources changes pagination and grouping, not the name of the
  form and not a generated title.

### Magazine

A magazine is a page-and-spread system. Its capacity comes from repeatable
grids, text threading, image modules, captions when supplied, and pagination.
The physical references are not a newspaper skin: a magazine can use a
full-bleed opener, a single image with a narrow text measure, a multi-column
feature, a sequence of small images, or a deliberately asymmetric spread.

Adobe's magazine workflow uses facing pages, column and row guides, image and
text frames, and threaded text frames. Print specifications also account for
bleed, live area, gutter safety, binding, and the fact that a spread can lose
content near the binding. The catalogue uses these as layout constraints while
keeping the page readable on screen.

Magazine rules carried into the catalogue:

- Use when the grouped material needs several pages, repeatable modules, or
  many source items.
- Choose a spread family from the local ratio of image area and text demand.
- Use one, two, or three text columns only when the measured text demand and
  reading measure justify them.
- Thread long text into the next frame or page. Never make type smaller to
  preserve a fixed spread.
- Keep the gutter quiet. Do not place essential text or a fragile image detail
  where the binding would damage it.
- Magazine pagination is the capacity mechanism for large source sets,
  including hundreds of sources. It does not invent an issue title or merge
  separate sources into one voice.

## Layout catalogue

The deck shows these families as actual page templates. A family is a set of
compositions with defined regions; it is not the tier itself.

| Form | Template | Physical analogue | Use when |
| --- | --- | --- | --- |
| Brochure | B-01 single panel | leaf or one panel | one short grouping |
| Brochure | B-02 tri-fold | 3-up consecutive fold | short sequence with six panels |
| Brochure | B-03 gatefold | two outer folds to a reveal | image or relationship needs reveal |
| Brochure | B-04 accordion | concertina sequence | ordered panels with no single cover |
| Pamphlet | P-01 text leaf | folio page | text leads and image is absent |
| Pamphlet | P-02 facing leaves | illustrated folio | image and text share a local grouping |
| Pamphlet | P-03 column leaf | short article page | text demand needs a second measure |
| Pamphlet | P-04 continuation | stitched sequence | one source exceeds one leaf |
| Magazine | M-01 image opener | full-page plate | one image needs a dominant page |
| Magazine | M-02 mixed spread | editorial spread | image and text need distinct regions |
| Magazine | M-03 multi-image sequence | image feature | several images need pacing |
| Magazine | M-04 long-run pagination | threaded feature | long text or many sources |

## Ratio and capacity model

The selector uses two passes.

### Pass 1: qualify the mixed-media cluster

For a proximity group, measure the rendered demand of each primitive rather
than counting raw items.

**Text demand**

1. Normalize rich text to readable blocks; retain paragraph and list
   boundaries.
2. Render at the catalogue's minimum readable body measure.
3. Count the resulting lines, line height, paragraph spacing, and required
   continuation space.
4. Sum the occupied area as `A_text`.

This means one 500-word source has more text demand than five one-line notes,
even if both groups contain five source records in another example. Source
count is a pagination input, not a substitute for rendered area.

**Image demand**

1. Read pixel dimensions and the native aspect ratio of every image.
2. Choose the smallest complete-frame display rectangle that preserves the
   image's role in the group.
3. Cap that rectangle by effective resolution; never crop or upscale to fill a
   template slot.
4. Sum the selected display areas as `A_image`.

For each image, effective resolution is:

`PPI_eff = min(pixel_width / placed_width_in, pixel_height / placed_height_in)`

The catalogue uses three working bands: `>=300 PPI` can support the full
selected frame, `150-299 PPI` stays bounded and avoids full-bleed emphasis, and
`<150 PPI` is a small supporting element or remains in the original viewer.
These are working routing bands; the source file and viewing distance remain
the authority for production quality. Adobe identifies 300 PPI as a common
high-quality print reference and notes that lower resolution can be acceptable
for large viewing distances.

The mixed-media ratio is:

`R_media = A_image / (A_image + A_text)`

The starting source-note band of `0.60-0.70` is applied only as a qualification
signal. It does not pick a page template. A group may remain mixed-media below
that band when its image and text are still meaningfully interdependent; this
is an explicit calibration question for implementation evidence.

### Pass 2: route the form, then choose a template

Route from total capacity and physical behavior:

- **Brochure**: one short sequence fits a sheet or small panel set without
  threaded continuation.
- **Pamphlet**: a short ordered sequence needs multiple leaves or folios, but
  remains a bounded reading object.
- **Magazine**: the group needs repeatable spreads, threaded text, many images,
  or many source items and therefore pagination.

Then select a template using local `R_media`, image aspect ratio, image quality
band, text-column demand, source proximity, and remaining capacity. If a
template cannot contain a complete primitive, the selector promotes to the
next template in the same form or creates another page. It does not overflow.

## Content identity rules

- There is no generated title region.
- A source title appears only when title metadata exists on that source.
- Without a title, text begins at the body measure; the first sentence is not
  promoted into a headline.
- An image caption appears only when caption metadata exists. The filename is
  not a title.
- A page number is physical pagination. It is not `Note 01`, a breadcrumb, or
  a source count.
- Source identity stays attached to its own image and text when multiple
  sources share a page or spread.
- A cluster with zero notes is still valid: image-only templates remain
  available. A cluster with hundreds of notes becomes a paginated sequence;
  no page header is synthesized from the count.

## References

- [Library of Congress: Genre Terms](https://www.loc.gov/collections/broadsides-and-other-printed-ephemera/articles-and-essays/genre-terms/) - broadside, leaflet, and pamphlet distinctions.
- [Library of Congress: Pamphlet Stitch](https://www.loc.gov/preservation/resources/educational/bookarts/pamphlet.pdf) - folio construction and four-page sheet behavior.
- [Adobe: Booklet Types](https://helpx.adobe.com/indesign/desktop/print/print-booklets/booklet-types.html) - consecutive 2-up, 3-up, and 4-up brochure panel arrangements.
- [U.S. Government Publishing Office: Brochures](https://publish.gpo.gov/products/2) - physical panel counts and fold behavior for half-fold, tri-fold, gate-fold, Z-fold, and roll-fold pieces.
- [Adobe: Magazine Layout](https://helpx.adobe.com/ph_fil/indesign/how-to/design-magazine-layout.html) - facing pages, guides, columns, frames, and threaded text.
- [Adobe: Grids](https://helpx.adobe.com/africa/indesign/using/grids.html) - baseline and document grids.
- [Adobe: Printed Image Resolution](https://helpx.adobe.com/photoshop/desktop/crop-resize-transform/resize-adjust-resolution.html) - pixel dimensions, PPI, and placed size.
- [Adobe: Print Resolution Specs](https://helpx.adobe.com/photoshop/desktop/crop-resize-transform/resize-adjust-resolution/resolution-specs-for-printing-images.html) - 300 PPI reference and viewing-distance caveat.
- [V&A: Jobbing Printing](https://www.vam.ac.uk/blog/museum-life/what-in-the-world-is-jobbing-printing) - historical examples of leaflets, catalogues, brochures, and magazine covers as distinct commercial print forms.
