# Design catalogue authoring brief

The catalogue is a set of PDF slide decks under `docs/design_catalogue/`,
organized by plane, then by purpose. Each deck breaks down one surface or
content type the way a fashion house documents a garment: the design is the
hero, annotations are a separate layer, and every choice has a stated reason
in plain English.

## Binding rules

1. **The design is the deliverable.** Every deck's core slides are large,
   product-faithful renderings built in HTML/CSS with the real tokens. A deck
   whose slides are mostly text has failed. Grey-box abstraction is allowed
   only for context (a distant Grid context behind the subject), never for the
   subject itself.
2. **Two layers, never mixed.** Product copy inside a rendering must pass
   DESIGN.md's zero-tolerance copy rule (no plane, architecture, or process
   nouns; product vocabulary only: Note, Document, Memory, Layer, Trace,
   Anchor, Gallery, surface titles). The annotation layer is monospace,
   reduced ink, visually distinct — it may use state IDs and token names.
3. **Plain-English rationale, no prose for its own sake.** Every annotation
   states what the choice is and why it is right, in simple precise terms.
   No mood language, no narrative, no conversation residue, no double
   titling, no subtitles, no "therefore" storytelling.
4. **Token-exact.** Colors, radii, spacing, and type come from
   `shared/catalogue.css` variables, which mirror DESIGN.md. Radius is 2px.
   Selection is interaction blue outline outside the frame; marquee amber is
   active selection work only; invalid red is refusal only. Inventing an
   accent fails the deck.
5. **No truncation anywhere.** Not in renderings (canon forbids it) and not
   in the deck's own layout: a slide that overflows 1280×720 is broken.
   Verify with screenshots before finishing.

## Deck grammar

Slides are 1280×720 sections (`.slide`), rendered by `build.mjs`. Use the
shared classes: `.running` header (`DECK NAME · SLIDE SUBJECT` left, figure
`NN / NN` right), `.cover` for slide one, `.folio` for the bottom-right
line, `.plate` for photographic stand-ins, `.verdict.keep` /
`.verdict.refuse` for comparison slides.

**Plates and notes.** A design slide (a plate) carries the design alone:
`.stage.bleed` filling the slide, numbered `.mark` circles on the design,
running header and folio only — no annotation text shares the slide. The
plain-English notes for those numbers live on the page that follows,
running header `DECK · NOTES ON NN`, set in the mono annotation layer as
`.notes` columns keyed to the marker numbers. One plate, then its notes: a
reader always gets the content whole, never competing with commentary.
Slides that are inherently textual (refusal grids, threshold tables) keep
their own layout. The legacy side-column classes (`.ann-col`) remain only
for decks authored before this rule.

A typical deck: cover → anatomy plate + notes → one plate (+ notes) per
distinct state or form — rendered, not described → geometry rules
(measure, pagination, masonry math) → refusals with rendered
counter-examples.

Slide count follows content. Never pad, and never crowd a plate to save a
page.

## Workflow

1. Write `src/<plane>/<nn>-<name>.html` (plane dirs: `grid-plane`,
   `information-plane`, `hud`; cross-plane decks sit in `src/` root).
2. `node docs/design_catalogue/src/snap.mjs <deck.html> <scratch-dir>` and
   read every PNG. Fix overflow, spacing collisions, and weak hierarchy.
3. `node docs/design_catalogue/src/build.mjs <deck.html>` to emit the PDF
   beside the plane directory.

## Authorities

DESIGN.md is the visual contract. Wireframe state contracts own behavior and
state IDs. The `docs/ux/wireframes/assets/` boards are visual references —
where a PNG board conflicts with DESIGN.md tokens (amber action buttons,
serif titles), the tokens win. Apple HIG conventions fill gaps the canon
leaves open (menu behavior, viewer control parity); InDesign-derived page
mechanics (margins, columns, gutters, baseline grid, threaded text) govern
reading editions.
