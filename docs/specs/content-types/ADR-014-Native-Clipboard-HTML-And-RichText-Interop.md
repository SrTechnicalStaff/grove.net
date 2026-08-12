---
status: "PARTIAL — verified clipboard placement seam"
---

# ADR-014: Native Clipboard HTML and RichText Interoperability Specification

- **Status**: Normative
- **Date**: 2026-08-12
- **Architectural Scope**: System Interoperability / Clipboard Pipeline
- **Target Runtime**: .NET 9.0 / Avalonia UI 11.2.5 / Native OS Clipboard (`CF_HTML`)

---

## 1. Context & Architectural Principles

Grove spatial placements (Notes, Documents, and Pictures) must interoperate seamlessly with external desktop applications (browsers, IDEs, Microsoft Word, Slack). Copying content from Grove to the clipboard must serialize selected objects into both standard plain text (`text/plain`) and formatted HTML (`CF_HTML` on Windows, `text/html` on macOS/Linux). Pasting external clipboard payloads into Grove must deserialize raw HTML/Markdown into Grove's RichText AST without losing structure or introducing visual artifacts.

### 1.1 Invariant Design Laws
1. **Dual Format Export**: Any copy or cut operation on Grove content simultaneously emits two clipboard formats: plain text (`DataFormats.Text`) and formatted HTML (`DataFormats.Html` with valid `CF_HTML` headers).
2. **CF_HTML Header Rigor**: Windows clipboard compliance requires strict byte-offset `CF_HTML` header formatting (`Version:0.9`, `StartHTML`, `EndHTML`, `StartFragment`, `EndFragment`).
3. **AST Preservation on Import**: HTML pasted into Grove is parsed into structured AST nodes (`HeadingBlock`, `ParagraphBlock`, `ListBlock`, `CodeBlockNode`, `InlineNode`). Synthetic outer wrappers (`<html>`, `<body>`) are stripped during fragment extraction.
4. **Zero-Loss Plain Text Fallback**: Plain text serializations preserve list indentation and heading indicators (`#`, `##`) to retain legibility when pasted into plain text editors.

---

## 2. Windows `CF_HTML` Header Specification & Byte Offset Math

### 2.1 Standard `CF_HTML` Payload Layout

```text
Version:0.9
StartHTML:0000000105
EndHTML:0000000380
StartFragment:0000000141
EndFragment:0000000344
<html>
<body>
<!--StartFragment-->
<h1>Authored Document Title</h1>
<p>Continuous prose with <strong>bold text</strong> and <em>italic text</em>.</p>
<!--EndFragment-->
</body>
</html>
```

### 2.2 Offset Calculation Formulas

Let $L_{\text{header}}$ be the fixed-length 105-byte header block (formatted with 10-digit zero-padded integer strings).

$$\text{StartHTML} = 105$$
$$\text{StartFragment} = 105 + \text{Len}(\text{"<html>\r\n<body>\r\n<!--StartFragment-->\r\n"}) = 141$$
$$\text{EndFragment} = \text{StartFragment} + \text{Len}(\text{FragmentHtml})$$
$$\text{EndHTML} = \text{EndFragment} + \text{Len}(\text{"\r\n<!--EndFragment-->\r\n</body>\r\n</html>"})$$

---

## 3. AST-to-HTML & HTML-to-AST Conversion Pipeline

```
[ System Clipboard ] ---> Read `text/html` or `CF_HTML`
                               |
                               v
                       [ CfHtmlParser ]
                       (Extract Fragment)
                               |
                               v
                    [ HtmlToAstConverter ]
                               |
           +-------------------+-------------------+
           |                   |                   |
    Heading Tags         Paragraph Tags        List Items
   (<h1> - <h6>)             (<p>)            (<ul>, <ol>)
           |                   |                   |
           v                   v                   v
     HeadingBlock        ParagraphBlock        ListBlock
           +-------------------+-------------------+
                               |
                               v
                    [ RichTextDocument AST ]
                               |
                               v
                   [ Spatial Document Control ]
```

---

## 4. C# / Avalonia 11.2.5 Native Clipboard Interop Pipeline

```csharp
namespace Grove.UI.Services;

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Input;
using Grove.Core.Content.Document;

public static class CfHtmlSerializer
{
    private const string HeaderTemplate =
        "Version:0.9\r\n" +
        "StartHTML:{0:D10}\r\n" +
        "EndHTML:{1:D10}\r\n" +
        "StartFragment:{2:D10}\r\n" +
        "EndFragment:{3:D10}\r\n";

    private const string PreFragment = "<html>\r\n<body>\r\n<!--StartFragment-->\r\n";
    private const string PostFragment = "\r\n<!--EndFragment-->\r\n</body>\r\n</html>";

    public static string SerializeToCfHtml(string fragmentHtml)
    {
        int headerDummyLength = string.Format(HeaderTemplate, 0, 0, 0, 0).Length;

        int startHtml = headerDummyLength;
        int startFragment = startHtml + Encoding.UTF8.GetByteCount(PreFragment);
        int endFragment = startFragment + Encoding.UTF8.GetByteCount(fragmentHtml);
        int endHtml = endFragment + Encoding.UTF8.GetByteCount(PostFragment);

        string header = string.Format(HeaderTemplate, startHtml, endHtml, startFragment, endFragment);
        return header + PreFragment + fragmentHtml + PostFragment;
    }

    public static string ExtractFragment(string rawCfHtml)
    {
        Match matchStart = Regex.Match(rawCfHtml, @"StartFragment:(\d+)");
        Match matchEnd = Regex.Match(rawCfHtml, @"EndFragment:(\d+)");

        if (matchStart.Success && matchEnd.Success)
        {
            int start = int.Parse(matchStart.Groups[1].Value);
            int end = int.Parse(matchEnd.Groups[1].Value);

            byte[] bytes = Encoding.UTF8.GetBytes(rawCfHtml);
            if (start < bytes.Length && end <= bytes.Length && start < end)
            {
                return Encoding.UTF8.GetString(bytes, start, end - start);
            }
        }

        // Fallback: strip comments manually if offsets are missing
        int commentStart = rawCfHtml.IndexOf("<!--StartFragment-->", StringComparison.Ordinal);
        int commentEnd = rawCfHtml.IndexOf("<!--EndFragment-->", StringComparison.Ordinal);

        if (commentStart >= 0 && commentEnd > commentStart)
        {
            commentStart += "<!--StartFragment-->".Length;
            return rawCfHtml.Substring(commentStart, commentEnd - commentStart).Trim();
        }

        return rawCfHtml;
    }
}

public static class AstHtmlConverter
{
    public static (string Html, string PlainText) SerializeDocument(RichTextDocument doc)
    {
        StringBuilder htmlBuilder = new();
        StringBuilder textBuilder = new();

        if (!string.IsNullOrEmpty(doc.Title))
        {
            htmlBuilder.AppendLine($"<h1>{System.Net.WebUtility.HtmlEncode(doc.Title)}</h1>");
            textBuilder.AppendLine($"# {doc.Title}\n");
        }

        foreach (var block in doc.Blocks)
        {
            if (block is HeadingBlock heading)
            {
                htmlBuilder.AppendLine($"<h{heading.Level}>{System.Net.WebUtility.HtmlEncode(heading.Text)}</h{heading.Level}>");
                textBuilder.AppendLine($"{new string('#', heading.Level)} {heading.Text}\n");
            }
            else if (block is ParagraphBlock para)
            {
                htmlBuilder.Append("<p>");
                foreach (var inline in para.Inlines)
                {
                    if (inline is TextRunInline textRun)
                    {
                        string encoded = System.Net.WebUtility.HtmlEncode(textRun.Text);
                        htmlBuilder.Append(encoded);
                        textBuilder.Append(textRun.Text);
                    }
                    else if (inline is FormattedInline fmt)
                    {
                        string encoded = System.Net.WebUtility.HtmlEncode(fmt.Text);
                        if (fmt.IsBold) encoded = $"<strong>{encoded}</strong>";
                        if (fmt.IsItalic) encoded = $"<em>{encoded}</em>";
                        if (fmt.IsCode) encoded = $"<code>{encoded}</code>";
                        htmlBuilder.Append(encoded);
                        textBuilder.Append(fmt.Text);
                    }
                }
                htmlBuilder.AppendLine("</p>");
                textBuilder.AppendLine("\n");
            }
        }

        return (htmlBuilder.ToString(), textBuilder.ToString());
    }
}

public sealed class NativeClipboardService
{
    private readonly IClipboard _clipboard;

    public NativeClipboardService(IClipboard clipboard)
    {
        _clipboard = clipboard;
    }

    public async Task CopyDocumentToClipboardAsync(RichTextDocument doc)
    {
        var (fragmentHtml, plainText) = AstHtmlConverter.SerializeDocument(doc);
        string cfHtml = CfHtmlSerializer.SerializeToCfHtml(fragmentHtml);

        DataObject dataObject = new();
        dataObject.Set(DataFormats.Text, plainText);
        dataObject.Set(DataFormats.Html, cfHtml);

        await _clipboard.SetDataObjectAsync(dataObject);
    }

    public async Task<string?> ReadClipboardTextOrHtmlAsync()
    {
        var dataObject = await _clipboard.GetDataAsync(DataFormats.Html);
        if (dataObject is string rawHtml && !string.IsNullOrWhiteSpace(rawHtml))
        {
            return CfHtmlSerializer.ExtractFragment(rawHtml);
        }

        return await _clipboard.GetTextAsync();
    }
}
```
