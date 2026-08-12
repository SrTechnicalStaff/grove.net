---
type: design-system-foundation
status: active
date: 2026-08-09
tags: [grove, design-system, typography]
---

# Typography

Three families, nine sizes, four leading steps, six tracking steps. Every one
of those values is named in `Tokens.md`; this document says which of them a
given piece of text takes, what may be combined with what, and what Grove
refuses to set.

Type is declared with the type tokens only. A raw pixel size, a raw tracking
or leading figure, or a font family named directly in a product stylesheet is
a defect, in the same way a raw colour is.

## The three roles

| Role | Family token | Weight | Sets | Never sets |
| --- | --- | --- | --- | --- |
| Display | `--f-display` | 500 | The title of a piece, and the line by which a composed surface names itself. | A sentence, a paragraph, a control, a label, metadata. |
| UI | `--f-ui` | 400, and 500 where this document permits it | Everything a person authored, and every control, menu row, message, and heading that is not a title. | Coordinates, counts, keys, or any figure that changes in place. |
| Monospace | `--f-mono` | 500 | Labels, front matter, title blocks, quick keys, counts, coordinates, dimensions, file kinds and sizes, timestamps, scale readouts, page continuation lines, badges. | Authored text, body text, or any run longer than one line. |

UI type is the default. Text whose role is not settled is UI type at
`--t-body`; display and monospace are the two exceptions a designer has to
justify, not the two decorations available.

Display is sparse. One piece of writing carries exactly one display title, and
no other display setting sits beside it — the exception is a heading the
source itself carries inside a long piece, which is covered under *One title,
never two*. A sheet that holds several pieces carries one display title per
piece and none for the sheet, because a title for the group would be a title
nobody wrote.

Monospace supports and never leads. It is always at reduced ink —
`--text-meta` on chrome, `--paper-label` on paper — and it never reaches
`--text-primary`, so the eye reads past it to the content. A monospace line
that wraps is a defect: it means a label has become a sentence.

## Which role each size belongs to

The nine sizes are the whole scale. A size is chosen for the job, and each
size belongs to one role.

| Token | Role | Carries |
| --- | --- | --- |
| `--t-micro` | Mono | The one quiet badge a frame may carry. The floor of the scale; nothing is set smaller. |
| `--t-label` | Mono | Front matter, title blocks, metadata lines, quick keys, page continuation lines. |
| `--t-caption` | UI, and mono for a metadata line that must read at a glance | Menu rows, quiet controls, secondary interface text. |
| `--t-dense` | UI | Dense interface text — a card inside a composed surface, and the closing line on a placed page. |
| `--t-body` | UI | The reading default: Note text, a Document's opening passage, editor body. |
| `--t-lead` | UI | A capture field, a lead paragraph, or one emphasised line. One per surface. |
| `--t-title-small` | Display | The line by which a composed surface names itself, and a heading the source carries inside a piece. |
| `--t-title` | Display on paper, UI on a writing surface | The title of a piece being read or written. |
| `--t-display` | Display | The title on a placed Document. |

`--t-title` takes UI type wherever the text is editable, because the editing
voice and the reading voice must be identical and the body around it is UI
type. The catalogue's writing deck sets that title at 26px in UI 600 and the
Slate anatomy deck sets a surface identity at 16px; both are off-scale and
both resolve upward — 26px to `--t-title`, 16px to `--t-title-small` — because
the token table is the authority on the scale and the memory deck already
renders that identity at 20px.

Monospace never exceeds `--t-caption`. Above that it competes with reading
text, which is the one thing it exists not to do.

## Weight

Grove sets three weights and no more.

| Family | Weight | Used for |
| --- | --- | --- |
| Display | 500 | Every display setting. There is no second display weight. |
| UI | 400 | Everything. |
| UI | 500 | Two cases only: a title set in UI type, and authored Note text, which sits on a colour fill where 400 goes thin. |
| Mono | 500 | Every monospace setting. |

600 and 700 exist in one place: the rendering of emphasis a person typed
inside their own text. Emphasis stays in the same family at the same size, and
italic likewise — both are content, not hierarchy. Grove never sets 600 itself;
the catalogue's writing deck does, and that is a defect that maps to UI 500.

Two levels of hierarchy never differ by weight alone. Weight is the device a
system reaches for when it has run out of size, ink, and space, and Grove has
not run out of any of them.

## Case and tracking

Uppercase is a label format. It is applied to text Grove writes about content,
and to a title Grove presents as an identity mark — never to a sentence.

| Setting | Case | Tracking |
| --- | --- | --- |
| Placed Document title | Uppercase | `--tr-title` |
| Surface identity line | Uppercase | `--tr-label` |
| Headline on a paper reading page | The source's own case | `--tr-display` |
| Source-carried heading inside a piece | The source's own case | `--tr-title` |
| Title set in UI type | The source's own case | `--tr-title` |
| Front matter, title block | Uppercase | `--tr-caps` |
| Mono label at `--t-label` or `--t-caption` | Uppercase | `--tr-label` |
| Mono badge at `--t-micro` | Uppercase | `--tr-wide` |
| Mono metadata line, mixed case | As written | `--tr-mono` |
| All UI body, controls, menu rows, messages | As written | None |

The reading page keeps the source's case while the placed Document uppercases
its title: the decks disagree and both are kept, because a placed sheet is a
front page read as an identity at a glance, and a reading page is read as
words. Nowhere else is authored text re-cased.

UI type carries no tracking except on a title, where it takes `--tr-title`.
Negative tracking does not exist in Grove; the writing deck's `-0.01em` is a
defect and resolves to `--tr-title`.

An uppercase run in UI or monospace is one line and never wraps. An uppercase
display title may run to three lines and no further, held together by
`--lh-tight`; beyond three lines a capitalised block stops being a title and
becomes a wall, which is the print convention this rule is taken from.

## Measure

Every column of continuous text declares its measure in `ch`. A measure
expressed in pixels, percent, viewport units, or the width of the thing
containing it is a defect.

The reason is one sentence: `ch` ties line length to the type size, so the
same passage reads at the same rhythm on a placed page, in a composed surface,
and on a printed reading page, and no line becomes unreadable because a window
was dragged wider.

| Setting | Measure |
| --- | --- |
| A placed Document's body, and any column on a page that has two or more columns | `--measure-reading` |
| The single column on a surface given over to writing | 65ch, a value that belongs to that surface's own specification because no other component shares it |

When there is less room than the measure needs, the measure holds and the
layout gives way, in this order: the column count drops, then the content
paginates. Type is never set smaller and the line is never set shorter. A
narrower window produces more pages, never a different reading.

## Leading

| Token | Carries |
| --- | --- |
| `--lh-tight` | Display titles, at any of the three display sizes. |
| `--lh-snug` | Authored Note text, where the block is short and the fill is coloured. |
| `--lh-ui` | Interface text: menu rows, controls, labels, messages, card text, and every monospace line. |
| `--lh-reading` | Continuous prose: a placed Document's passages, editor body, reading page body, and any UI run over three lines. |

Monospace takes `--lh-ui` because a monospace line never wraps and the value
only ever sets the height of a single row. There is no fifth step; the 1.55,
1.6, and 1.62 figures scattered through the catalogue resolve to `--lh-ui` and
`--lh-reading`.

## Size is absolute

The scale answers the role of the text, never the room available.

- Type size is identical at every footprint. A Note set on one cell and the
  same Note grown to four cells set `--t-body` alike.
- Content never shrinks to fit. When text outgrows its footprint on the Grid,
  the footprint grows; on a composed surface, the surface scrolls; on paper,
  the piece paginates.
- Camera distance scales a whole placement uniformly and is not a size change:
  the type keeps its proportion and the measure stays the same count of
  characters. `Representation-tiers.md` owns what sheds and when.
- `clamp()` on a font size, viewport units, fit-to-box scaling, auto-shrink,
  `text-overflow: ellipsis`, and line clamping are all defects. None of them
  are available anywhere in Grove.

## Hierarchy

Levels are built from three devices, in this order of preference: size, then
ink, then space. A rule or hairline separates; it does not rank.

| Want | Do |
| --- | --- |
| A passage that comes after the main one | Drop one size step and one ink step — `--t-body` at `--text-page` down to `--t-dense` at a lower paper step. |
| A line that must be found but not read | Keep the size, drop the ink to `--text-meta`. |
| A new section | Add space above it, and a heading only if the source wrote one. |
| Something to feel primary | Give it space, not weight. |

Two levels differing only in ink are permitted; two levels differing only in
weight are not. Three visible levels on one surface is the working limit —
past three, a reader is decoding rather than reading, which is the desktop
convention this limit is taken from.

Figures that change in place — counts, coordinates, scale readouts, timestamps
— are set with tabular figures so the line does not shift as the number
changes. Monospace gives this for free; UI type must ask for it.

## One title, never two

- A surface names itself once. If it carries an identity line, nothing inside
  it repeats that name.
- A piece of content carries one title: its own, as authored. Grove never
  generates a title, never lifts a first sentence into a headline, never makes
  a caption from a file name, and never numbers a piece to give it a name.
- Subtitles, standfirsts, decks, and kickers do not exist. If a source wrote a
  heading inside its own text, that heading is carried through at
  `--t-title-small` in the source's case; nothing else sits between a title
  and the text under it.
- Untitled content begins at the body measure in body type. Its first sentence
  is not enlarged, emboldened, or promoted.
- A caption is metadata, not a second title: monospace, `--t-label` or
  `--t-micro`, at reduced ink, beneath or beside the frame and never over it.
  `Harbor at dusk · JPG · 5472 × 3648` is a caption. `Harbor at dusk` set
  larger above the picture is a second title.

## Refusals

| Refused | Why |
| --- | --- |
| A fourth family, a serif face, an icon font | Three families carry every job Grove has. |
| Monospace as body text | It is the supporting voice; a paragraph in it is a paragraph nobody can scan. |
| Uppercase on a sentence or a paragraph | Caps are a label format and cost reading speed over a few words. |
| A size outside the nine | The scale is the whole scale; an off-scale value resolves to the nearest step. |
| Underline for emphasis or hierarchy | Underline says nothing that size, ink, and space do not say more quietly. |
| Type set smaller than `--t-micro` | Below the floor a mark is decoration, not information. |
| A reading column measured in anything but `ch` | Line length must follow the type, not the window. |
| All-caps blocks over three display lines, or any wrapped uppercase label | A wrapped label has become prose in the wrong dress. |
| Text scaled, clipped, faded, or ellipsised to fit | The container answers the content; content never answers the container. |
