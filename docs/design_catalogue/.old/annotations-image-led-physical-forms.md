---
type: design-reference
status: working
date: 2026-08-09
tags: [grove, annotations, image-led, photography, gallery, contact-sheet, photobook]
---

# Image-led physical forms

This reference describes the physical image-publication forms used to ground
image-led annotation tiers. It replaces vague “image edition” language with
recognizable photographic forms and explains how those forms behave when image
count, sequence, aspect ratio, and image quality increase.

## Source boundary

The annotation source notes establish the following constraints:

- Images are a primitive alongside rich text.
- Image content is straightforward to display, except for moving media that
  needs a separate strategy.
- Proximity groups nearby content before a page is composed.
- The local image-to-text or image-to-image relationship is examined again
  after grouping.
- Images are not cropped or distorted to satisfy an arbitrary slot.
- More content creates more pages or a larger form; it does not create a
  thumbnail wall that makes the source impossible to inspect.

Primary source note: [`Grove - Annotations UX.txt`](../../raw/original-notes/Grove%20-%20Annotations%20UX.txt).

## The three physical references

### Tier 01: photographic plate / gallery print

The smallest image-led form is a single photographic plate: one image given
the largest complete frame that its source quality supports. The gallery
reference is useful because an exhibition print treats the image as the
subject, while label and provenance remain separate supporting information.

This is not a fixed paper size. It is a presentation behavior:

- one image or one tightly bound image pair;
- complete frame preserved;
- optional caption or provenance outside the image area;
- generous surrounding field when the image benefits from inspection;
- no forced title region when the source has no title metadata.

Tier 01 is appropriate when one image carries the group or when a small number
of images need individual inspection rather than comparison.

### Tier 02: contact sheet / proof sheet

The middle form is the photographic contact sheet, also called a proof sheet.
The Cleveland Museum of Art describes contact sheets as vital to twentieth-
century photographic practice: the exposures from a roll were printed together
so the photographer could review and mark the frames before making enlargements.
The Library of Congress glossary similarly defines a contact print/contact
sheet as a professional photographic print made from a digital scan or film
original.

The contact sheet has a different purpose from a gallery plate. It supports
comparison, selection, sequence review, and archival reference. Its physical
behavior is:

- several images on one sheet;
- a stable reading order, often inherited from capture order;
- repeated frame sizes or a clear contact-print scale;
- markings or metadata outside the image area when present;
- enough size to compare images, but not enough area to inspect every image as
  a final plate.

Tier 02 is appropriate for a moderate image set where relationship and
comparison matter more than giving every image a dominant page.

### Tier 03: photobook / photographic magazine sequence

The large form is a photobook or image-led magazine sequence. MoMA defines a
photobook as a book in which photographs make a significant contribution to the
content; it specifically identifies sequence, placement, relationship to
text, scale, and materiality as important parts of the work.

This is stronger and more specific than “image edition.” The physical behavior
is:

- images sequenced across pages and facing-page spreads;
- individual plates, pairs, contact-like clusters, and full-bleed moments used
  as different pacing devices;
- image order carrying meaning rather than being a simple sort by filename;
- optional text and captions acting as support, not replacing the images;
- page turns creating reveal, pause, contrast, and continuation;
- pagination when the sequence exceeds one spread.

Tier 03 is appropriate when the set needs an image-reading sequence, repeated
spreads, many images, or enough area that a contact sheet would reduce the
images to inspection thumbnails.

## Tier map

| Tier | Physical analogue | Reading unit | Content behavior |
| --- | --- | --- | --- |
| 01 | Gallery print / photographic plate | one complete image | inspection, optional label |
| 02 | Contact sheet / proof sheet | one comparison sheet | moderate set, sequence visible |
| 03 | Photobook / image-led magazine | pages and facing spreads | visual sequence, pagination |

“Image edition” can remain as an internal alias during migration, but it is
not specific enough to guide layout. The physical reference should be
photobook or image-led magazine sequence.

## Layout families

The tier is the capacity band. The page layout is selected inside the tier.

### Tier 01 layouts

**I1-A: single plate**

One image receives the largest complete frame available. The frame follows
the image's aspect ratio; the page yields around it. This is the default
image-led inspection layout.

**I1-B: plate with supporting label**

One image remains dominant while optional caption, source, date, or provenance
appears outside the frame when metadata exists. The label is not a generated
headline and does not compete with the image.

**I1-C: paired plates**

Two closely related images share a landscape unit or facing pair. Each keeps
its own complete frame. Use only when proximity or sequence makes the pairing
meaningful; do not make a generic two-up grid from unrelated images.

### Tier 02 layouts

**I2-A: uniform contact sheet**

Images appear in a stable grid at a consistent inspection scale. The grid
preserves order and makes comparison easy. The images are not enlarged until
they become soft and are not cropped to make every aspect ratio identical.

**I2-B: aspect-preserving contact sheet**

Images share rows or columns while their complete frames remain visible.
Unequal frame sizes are allowed when aspect ratio or source quality requires
them. Alignment yields before an image is distorted.

**I2-C: contact strip with metadata rail**

A sequence of images runs along one landscape reading unit while a narrow
metadata rail carries optional source facts. The metadata remains outside the
image frames and never replaces them.

### Tier 03 layouts

**I3-A: single-image page**

One image occupies a page or one side of a spread. Use when scale and pause
are more important than density.

**I3-B: facing-page pair**

Two images occupy facing pages. Their relationship is created by proximity,
sequence, contrast, or shared source grouping. Each image remains independently
inspectable.

**I3-C: image sequence spread**

Several images share a spread with deliberately varied scale. One may lead,
one may bridge, and one may close the sequence. Variation is intentional
sequencing, not a masonry wall that disregards order.

**I3-D: image-plus-text spread**

An image remains primary while a short text measure, caption, or provenance
block sits beside it. Text is optional and data-driven. The absence of text
does not create an empty title slot.

**I3-E: paginated visual sequence**

The sequence continues across several spreads. Page turns are part of the
composition. More images create more pages; they do not force all images into
one contact sheet.

## Image handling rules from physical practice

### The frame is complete

A photographic print, contact print, and photobook page can all use different
scales, but none requires an arbitrary crop merely to fill a rectangle. The
layout must choose a frame that preserves the source image. If the source
aspect ratio does not match the available region, the region changes, the
image becomes smaller, or another layout family is selected.

### Resolution limits display area

Image dimensions and resolution are different. Adobe describes resolution as
how densely pixels are assigned to a printed inch and identifies 300 PPI as a
common high-quality print reference. The working quality bands for annotation
research are:

- `>=300 PPI`: can support the selected display area when the source is clean;
- `150-299 PPI`: use a bounded frame and avoid making the image the largest
  possible bleed;
- `<150 PPI`: keep the image small and supporting, or route it to full-item
  inspection rather than presenting a soft hero frame.

Effective placed resolution is:

`PPI_eff = min(pixel_width / placed_width_in, pixel_height / placed_height_in)`

These are routing bands, not a universal print-production guarantee. The
source dimensions, intended viewing distance, and final output medium remain
the authority.

### Aspect ratio is a layout input

Classify each image as landscape, portrait, square, or extreme panorama before
choosing a family. The classification selects candidate frames; it does not
authorize cropping. A contact sheet may use unequal frame widths, while a
photobook sequence may give a panorama a spread and a portrait a single page.

### Captions and provenance are conditional

A physical photograph can have a label, catalog number, date, or caption, but
those are separate records. The annotation should show them only when source
metadata exists. A filename is not a title. A page number is pagination, not a
source identity.

## Sequence and proximity

Contact sheets usually preserve capture order because they are working records.
Photobooks and photographic magazines may edit and resequence images because
the order itself creates meaning. MoMA's photobook definition explicitly treats
sequence and placement as part of the work.

For annotations, proximity supplies the first grouping signal. Within a group:

- keep related images adjacent;
- preserve source boundaries when images are not one sequence;
- do not sort by filename merely because it is convenient;
- do not make a stronger image the “cover” if the source relationship does not
  support that role;
- add pages when the visual sequence exceeds the current spread.

## Content-capacity model

Image-led routing should measure display demand, not only the number of image
records.

### Image demand

1. Read each image's pixel dimensions and aspect ratio.
2. Select the smallest complete frame that preserves the image's intended role.
3. Cap the frame by effective resolution.
4. Sum the selected areas as `A_image`.

The useful density value is:

`D_image = A_image / A_unit`

where `A_unit` is the usable area of the candidate plate, contact sheet, or
spread. Source count is separate: one extremely large image can require Tier
01 or Tier 03 depending on the required display area, while many small images
can fit Tier 02 when comparison is the purpose.

### Promotion rules

- Promote from plate to contact sheet when comparison between several images
  is more useful than individual inspection.
- Promote from contact sheet to photobook sequence when the images need
  readable scale, deliberate pacing, or multiple spreads.
- Add pages before reducing images to unreadable thumbnails.
- Use a smaller complete frame before cropping or enlarging a low-quality
  source.

## Non-still media

GIFs and video are not equivalent to a still photographic plate. A static
representative frame may be used only when it is an explicit derivative and
the source remains identifiable. Otherwise, moving media needs a playback or
inspection surface rather than being flattened into a misleading image-led
page.

## References

- [Cleveland Museum of Art: PROOF / Photography in the Era of the Contact Sheet](https://www.clevelandart.org/exhibitions/proof-photography-era-contact-sheet) - contact sheets as proof, selection, and archival reference.
- [Library of Congress: Duplication Services Glossary](https://www.loc.gov/duplicationservices/glossary/) - contact print/contact sheet terminology.
- [National Portrait Gallery: Contact Print](https://www.npg.org.uk/collections/explore/glossary-of-art-terms/contact-print) - contact-print scale and viewing a roll as one sheet.
- [MoMA: Art Terms / Photobook](https://www.moma.org/collection/terms/?filter=P) - sequence, placement, text relationship, scale, and materiality.
- [MoMA: Distilling Martha Graham's Dance in Photographs](https://www.moma.org/magazine/articles/1097) - photographic sequence as a publication structure.
- [MoMA: Written by Dorothea Lange](https://www.moma.org/magazine/articles/245) - editing and sequencing images alongside text.
- [Adobe: Printed Image Resolution](https://helpx.adobe.com/photoshop/desktop/crop-resize-transform/resize-adjust-resolution.html) - pixels, PPI, and placed size.
- [Adobe: Print Resolution Specs](https://helpx.adobe.com/photoshop/desktop/crop-resize-transform/resize-adjust-resolution/resolution-specs-for-printing-images.html) - 300 PPI reference and viewing-distance caveat.
