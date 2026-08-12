---
type: design-system-component
status: normative
date: 2026-08-11
owner: "Grove Design System"
tags: [grove, design-system, rich-text, tiptap, editor]
---

# Rich-text editor

## Contract

| Property | Canonical value |
| --- | --- |
| Runtime | Vanilla strict TypeScript through Vite; no UI framework adapter. |
| Editor engine | `@tiptap/core` `3.30.0`, `@tiptap/pm` `3.30.0`, `@tiptap/starter-kit` `3.30.0`. All `@tiptap/*` packages use one version. |
| Extension set | `StarterKit` only until a named product formatting decision adds an extension. |
| Grove styling | `injectCSS: false`; Grove CSS owns all editor surface, type, spacing, focus, selection, and motion values. |
| Canonical storage | Tiptap JSON document with root `{ "type": "doc" }`. HTML is not persisted. |
| Rich fields | Note `payload.note`; Document `payload.body`. |
| Plain fields | Document `payload.title`, `payload.label`, `payload.closing`, and source metadata remain plain strings. |
| Legacy input | A legacy string is converted to one paragraph per line; an empty string becomes one empty paragraph. |
| Editing owner | Tiptap owns selection, composition, undo, redo, keyboard editing, marks, and block structure. |
| Product owner | Grove commands own Memory identity, revision creation, durable writes, refusal, and history. |
| Annotation reader | Uses the same document schema and text extraction rules; it never reparses editor HTML. |
| Source identity | Editor state never owns Memory or Placement identity. The feature supplies a Memory reference and receives a document value. |
| Empty value | A document with one empty paragraph is valid editor state; a commit requiring content refuses when extracted text is empty after trimming. |
| Disposal | Every mounted editor is destroyed with its owning surface or Slate pane. No editor survives its host element. |

## Allowed formatting

| Category | StarterKit contract |
| --- | --- |
| Inline | Bold, italic, strike, inline code. |
| Blocks | Paragraph, heading, blockquote, code block, horizontal rule. |
| Lists | Bullet list, ordered list, list item. |
| History | Undo and redo. |
| Hard break | Supported by the StarterKit schema. |
| Links, images, tables, mentions, comments | Not available until separately accepted and added to the extension set. |

## Serialization

| Operation | Rule |
| --- | --- |
| Load | Normalize the stored field to a validated Tiptap JSON document before constructing `Editor`. |
| Update | Read `editor.getJSON()` from the `update` event; do not read `innerHTML`, DOM text, or CSS classes as state. |
| Save | Pass the JSON document to `updateMemory`; one command creates one Memory revision. |
| Readable text | Traverse text nodes and hard breaks from the JSON document. Paragraph and list boundaries become line boundaries. |
| Grid measurement | Use readable extracted text; formatting never disappears from the durable document because measurement is a projection only. |
| Annotation demand | Use wrapped extracted text plus the declared measure fixture; never use raw JSON length or HTML length. |
| Migration | Legacy string fields remain readable through the normalizer and become canonical JSON on the next successful save. |

## Feature seam

```text
Memory payload ──> normalizeRichText ──> Tiptap Editor
Tiptap update ───> JSON document ──────> updateMemory command
JSON document ───> richTextToPlainText ─> Grid measurement and Annotation demand
```

| Allowed dependency | Forbidden dependency |
| --- | --- |
| A feature creates an editor through the shared Grove rich-text factory. | A feature constructs a second editor schema or directly mutates `contenteditable`. |
| A feature reads `getJSON()` and sends it to a named command. | A feature stores HTML, DOM markup, or editor instance state in Memory. |
| Grid and Annotation consume the shared text projection. | Grid or Annotation imports Tiptap internals to reconstruct text independently. |
| CSS styles `.tiptap` within the owning component. | Tiptap default CSS or browser default editor chrome defines Grove appearance. |
| Surface/Slate teardown calls `Editor.destroy()`. | A hidden editor remains mounted after its host closes. |

## States

| State | Required result |
| --- | --- |
| Rest | Editor contains the normalized document and is editable when the owning surface permits editing. |
| Focused | Native editor focus and selection remain visible through the component focus contract. |
| Changed | Status and save command reflect JSON inequality from the last committed document. |
| Pending | The last complete durable Memory remains unchanged until the save command commits a revision. |
| Refused | Empty required content or a rejected Memory update leaves editor JSON and focus unchanged. |
| Committed | Saved JSON becomes the new Memory revision; dirty state clears. |
| Unavailable | The editor is not created for a missing Memory; the owning surface renders its explicit unavailable state. |

## Refusals

| Refused behavior | Required result |
| --- | --- |
| Persisting editor HTML as canonical content | Persist Tiptap JSON. |
| Treating raw JSON or HTML length as text demand | Extract text, then measure wrapped readable text. |
| Saving through an editor callback that bypasses `updateMemory` | Route the complete document through the Memory command. |
| Replacing an unavailable Memory with an empty document | Preserve source identity and render the owning unavailable state. |
| Loading one extension version beside another | Keep all `@tiptap/*` packages on the same version. |
| Allowing product code to depend on Pro extensions without an accepted contract | Use the open-source StarterKit boundary. |

## Sources

| Source | Rule used |
| --- | --- |
| `.reference/tiptap-docs/src/content/editor/getting-started/install/vanilla-javascript.mdx` | Vanilla Vite setup, `Editor`, `StarterKit`, and ES-module integration. |
| `.reference/tiptap-docs/src/content/editor/getting-started/configure.mdx` | `element`, `extensions`, `content`, `injectCSS`, and editor configuration. |
| `.reference/tiptap-docs/src/content/editor/core-concepts/persistence.mdx` | JSON persistence and editor lifecycle. |
| `.reference/tiptap-docs/src/content/guides/output-json-html.mdx` | JSON output, read-only rendering, update events, and HTML boundary. |
| `docs/versions/0.2.0/design-system/10-grammar/Content-concentration.md` | Readable text demand and measurement inputs. |
| `docs/versions/0.2.0/design-system/10-grammar/Annotation-ledger.md` | Annotation source identity, demand, and re-resolution. |
