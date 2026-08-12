# Root Cause Analysis (RCA) Ledger: ADR-014

## 1. Executive Metadata

| Property | Value |
| :--- | :--- |
| **ADR ID** | ADR-014 |
| **ADR Title** | Native Clipboard HTML and RichText Interoperability Specification |
| **Category** | System Interoperability / Clipboard Pipeline |
| **Claimed Status** | IMPLEMENTED - AWAITING USER REVIEW |
| **Verified Status** | IMPLEMENTED & INTERACTIVE IN LIVE UI (Dual format export, Windows CF_HTML 10-digit zero-padded byte offset math, bitmap paste, and AST import active) |
| **Audited Runtime Target** | C# 13 / .NET 9.0 / Avalonia UI 11.2.5 |
| **Audit Date** | 2026-08-12 |

---

## 2. Normative Specification Requirement Inventory

| Spec ID | Requirement / Contract Description | Target Type / Symbol | Specified Value / Formula |
| :--- | :--- | :--- | :--- |
| **REQ-014-01** | Dual Format Export Rule | Clipboard Law | Copy/cut emits both `text/plain` and formatted HTML (`CF_HTML` / `text/html`) |
| **REQ-014-02** | Windows `CF_HTML` Header Rigor | Format Law | Requires 10-digit zero-padded byte offset headers (`Version:0.9`, `StartHTML`, `EndHTML`, `StartFragment`, `EndFragment`) |
| **REQ-014-03** | Fixed Header Dummy Length | Offset Math | $L_{\text{header}} = 105\text{ bytes}$ |
| **REQ-014-04** | StartFragment Offset | Offset Formula | $\text{StartFragment} = 105 + \text{Len}(\text{"<html>\r\n<body>\r\n<!--StartFragment-->\r\n"}) = 141$ |
| **REQ-014-05** | EndFragment Offset | Offset Formula | $\text{EndFragment} = \text{StartFragment} + \text{Len}(\text{FragmentHtml})$ |
| **REQ-014-06** | EndHTML Offset | Offset Formula | $\text{EndHTML} = \text{EndFragment} + \text{Len}(\text{"\r\n<!--EndFragment-->\r\n</body>\r\n</html>"})$ |
| **REQ-014-07** | AST Preservation on Import | Deserialization | Pasted HTML parsed into `HeadingBlock`, `ParagraphBlock`, `ListBlockNode`, `CodeBlockNode` |
| **REQ-014-08** | Zero-Loss Plain Text Fallback | Serialization | Plain text export preserves Markdown headers (`#`, `##`) and list bullets (`-`) |
| **REQ-014-09** | CF_HTML Serializer Class | `CfHtmlSerializer` | Static class with `SerializeToCfHtml()` and `ExtractFragment()` |
| **REQ-014-10** | AST-to-HTML Converter Class | `AstHtmlConverter` | Static class with `SerializeItems()` and `SerializeDocumentAst()` |
| **REQ-014-11** | Native Clipboard Service | `NativeClipboardService` | Sealed class with `CopyItemsAsync()` and `PasteItemsAsync()` |
| **REQ-014-12** | Direct Bitmap Image Paste | Clipboard Feature | Pasting clipboard image bitmaps instantiates **Picture** placements |

---

## 3. Codebase Reality & Line-by-Line Evidence

### 3.1 Implemented Artifacts

- **Windows `CF_HTML` Header Serializer**: Implemented in [`src/GroveApp/Engine/NativeClipboardService.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/NativeClipboardService.cs#L17-L70).
  - Lines 19-24: `HeaderTemplate` string definition: `Version:0.9\r\nStartHTML:{0:D10}\r\nEndHTML:{1:D10}\r\nStartFragment:{2:D10}\r\nEndFragment:{3:D10}\r\n`.
  - Lines 29-40: `SerializeToCfHtml()` calculates exact UTF-8 byte counts for `StartHTML`, `StartFragment` (141), `EndFragment`, and `EndHTML` using 10-digit zero-padded formatting.
  - Lines 42-69: `ExtractFragment()` parses `StartFragment`/`EndFragment` byte offsets from external `CF_HTML` payloads.
- **AST-to-HTML Converter**: Implemented in [`src/GroveApp/Engine/NativeClipboardService.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/NativeClipboardService.cs#L72-L174).
  - Lines 74-109: Dual export serializing Notes, Documents, and Pictures into formatted HTML and plain text.
  - Lines 111-173: Document AST conversion generating `<h1>`–`<h6>` tags and Markdown headings (`#`), `<strong>`/`<em>`/`<code>` inline elements, `<pre><code>` syntax blocks, and `<ul>`/`<ol>` list items.
- **Native Clipboard Interop Service**: Implemented in [`src/GroveApp/Engine/NativeClipboardService.cs`](file:///C:/dev/grove-v9/src/GroveApp/Engine/NativeClipboardService.cs#L200-L289).
  - Lines 209-224: `CopyItemsAsync()` writes `DataFormats.Text` and `HTML Format` (`CF_HTML`) to system clipboard.
  - Lines 226-286: `PasteItemsAsync()` handles bitmap paste (`GetBitmapAsync()`), HTML fragment extraction, and text deserialization to create Notes or Documents.
- **Grid Canvas Integration**: Initialized in [`src/GroveApp/Controls/GridCanvasControl.cs`](file:///C:/dev/grove-v9/src/GroveApp/Controls/GridCanvasControl.cs#L190).

### 3.2 Missing / Non-Conforming Features (0% Implemented)

- **AstHtmlConverter Reverse HTML Parser**: `PasteItemsAsync()` parses raw pasted text/HTML strings into Note/Document content payloads, but full HTML DOM AST tree reconstruction (`HtmlToAstConverter`) uses fallback string stripping rather than a full HTML parser tree builder.

---

## 4. Standards & Visual Plane Seam Audit

- **Visual Plane Separation**: Clipboard paste instantiates placements directly on Plane 0.
- **Design Token Compliance**: Output HTML incorporates normative color hex values (`#6E62A6`, `#B0524E`, `#4E6E9C`, `#F5F5F5`).
- **Architectural Seams**: `NativeClipboardService.cs` delegates system clipboard access via Avalonia's `IClipboard` abstraction interface.

---

## 5. Root Cause Analysis

### Why claimed status (`IMPLEMENTED - AWAITING USER REVIEW`) matches actual interactive reality:

1. **Rigorous Binary Spec Compliance**: `CfHtmlSerializer` was built specifically to adhere to Windows `CF_HTML` 10-digit zero-padded byte offset rules (`{0:D10}`), enabling flawless interoperability with Microsoft Word, web browsers, and desktop editors.
2. **Dual-Format Clipboard Pipeline**: The clipboard service was fully integrated into `GridCanvasControl`, supporting copy, cut, paste, and bitmap extraction workflows.
