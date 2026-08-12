---
type: design-system-grammar
status: active
date: 2026-08-09
tags: [grove, design-system, copy]
---

# Interface copy

Grove never explains itself in the system's own vocabulary. This rule has zero
tolerance: a single architectural noun in user-visible text fails review, no
matter how correct everything else is.

The default is to write nothing. Most chrome needs no copy at all — a
surface's position, its hue role, and its structure already say what it is. A
label exists to give a person a name for something they can act on, not to
classify it.

## Prohibited vocabulary

None of these may appear in any window title, header, button, caption,
subtitle, placeholder, tooltip, empty state, status line, error, or accessible
name.

**Plane and architecture nouns.** Information Plane, HUD, Grid Plane, plane,
surface, router, controller, contract, kernel, registry, adapter, port,
handler, store, state, model, instance, entity, payload, schema.

**Documentation and process words.** Outcome, discovery, decision record,
wireframe, vertical, tier, resolver, slice, spec, task, evidence, acceptance,
scope, journey, gate, iteration, backlog.

**Prose that narrates the concept.** Any phrase that describes the idea behind
a feature instead of naming what a person can do — "Store your context", "Your
information layer", "Where your thinking lives", "Capture and organise".

**Development residue.** Anything that reads as a note between people building
the product: "coming soon", "not yet implemented", "TODO", "beta", version
numbers, internal names, and any sentence that addresses the reader as a
tester.

## Permitted vocabulary

Grove's product words are the names of things a person handles: **Note,
Document, Image, Memory, Layer, Trace, Anchor, Gallery**, and the titles of
the surfaces they open. These are capitalised because they are the product's
own nouns.

Everything else is ordinary English. A word is permitted if a person who has
never read a line of Grove's documentation would understand it in context.

## The test

Read the label to someone who has never seen Grove and cannot see the screen.
If they cannot say what it names or what it would do, it fails.

A second test catches the subtler case: if the label would still be true after
the feature was rebuilt on a completely different architecture, it is about
the product. If it would need changing, it is about the implementation, and it
is wrong.

## Voice

Copy is plain, specific, and short. It names what is true rather than what
went wrong.

- **Sentence case.** Not Title Case, not ALL CAPS in prose. Uppercase is a
  type treatment for short monospace labels only, and it is applied with
  tracking rather than by writing capitals.
- **No terminal punctuation on labels.** A full sentence in a strip or an
  empty state takes a full stop; a button, a label, and a menu row do not.
- **No exclamation marks.** Ever.
- **Second person only where a person is addressed directly**, and never
  possessive about their work — "Nothing here yet", not "Your Notes will
  appear here".
- **Verbs for actions, nouns for places.** A button says what it does; a
  header says what it is.
- **No apology and no praise.** Grove does not say sorry, and it does not
  congratulate.

## Refusals

A refusal states the condition, not the failure, and never blames the person
or the product.

Write what is true about the world: "This space is occupied." Not what the
system did: "Placement rejected." Not what the person did wrong: "You cannot
place here." Not what broke: "Error: collision detected."

A refusal sentence is one clause, present tense, and it sits beside the thing
being refused. The action that would have committed stays present and
unavailable, so a person can see what they were reaching for.

## Empty states

An empty state is a designed state with a next step, never a blank surface and
never an apology.

Name what is absent in the product's own words, then give the one action that
changes it. "Nothing placed on this Layer yet" followed by the action that
places something. Do not explain the concept, do not describe the feature, and
do not fill the space with an illustration of emptiness.

## Confirmations

Grove has no dialog box. A confirmation is an inline bar or row inside the
surface that asked, with explicit wording and a quiet action beside a primary
one.

The wording states the consequence, not the mechanism: "Delete this Note"
rather than "Confirm deletion". A destructive action names the thing it will
destroy, sits in its own group, and is never adjacent to the default action.

## Where this rule also binds

The rule binds anything styled as product text, wherever it appears:

- The application itself, including accessible names and live-region
  announcements.
- Catalogue decks, mockups, and state boards. Anything rendered to look like
  shipping interface must pass.
- Engineering annotations on those boards are permitted **only** in a visually
  distinct annotation layer — monospace, reduced ink, clearly separate — that
  could never be mistaken for interface. That layer may name state
  identifiers and token names freely.

## Corrections

Real violations and what replaces them.

| Violation | Why it fails | Correction |
| --- | --- | --- |
| `Content on current Layer` | "Content" is an architecture noun for the thing a person calls a Note, a Document, or a picture. | `Notes, Documents and pictures on this Layer` |
| `Open Image Viewer` | Names an internal surface class rather than the act. | `Open picture` |
| `Nearby reading` | Narrates the concept behind annotations instead of naming what is there. | The reading's own title |
| `Provide context` | Describes the mechanism rather than the act. | `Write what this means` |
| `Write the context you hold…` | Conversation-derived prose about a concept. | `What does this connect to?` |
| `Placement rejected` | Names the operation and blames the attempt. | `This space is occupied` |
| `2 selected` | A count riding on the Grid, in system framing. | Nothing; selection is already visible on every selected placement. |
| `Image` as a caption under a picture | Classifies rather than names, and rides on the frame. | Nothing; a picture is legible as a picture. |
| `Surface closed` | Architecture noun in a status announcement. | Announce nothing; the return of focus is the signal. |

## What is not yet asserted

No script scans user-visible strings for the prohibited vocabulary.
`90-conformance/Checks.md` records that gap. Until it exists, every change
that adds or edits a string is reviewed against this document by hand, and the
table above is extended whenever a new violation is found and corrected.
