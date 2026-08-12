using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Input;
using Avalonia.Input.Platform;
using GroveApp.Models;

namespace GroveApp.Engine
{
    /// <summary>
    /// Implements ADR-014 Windows CF_HTML Header Serialization & Offset Math.
    /// Formats clipboard HTML payloads with 10-digit zero-padded byte offsets.
    /// </summary>
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
            int dummyHeaderLen = string.Format(HeaderTemplate, 0, 0, 0, 0).Length;

            int startHtml = dummyHeaderLen;
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
        public static (string Html, string PlainText) SerializeItems(IEnumerable<GridContentItem> items)
        {
            StringBuilder htmlBuilder = new();
            StringBuilder textBuilder = new();

            foreach (var item in items)
            {
                if (item is GridNote note)
                {
                    htmlBuilder.AppendLine($"<div class=\"grove-note\" style=\"background-color: {note.FillHex};\">");
                    htmlBuilder.AppendLine($"<p>{System.Net.WebUtility.HtmlEncode(note.Text).Replace("\n", "<br/>")}</p>");
                    htmlBuilder.AppendLine("</div>");

                    textBuilder.AppendLine(note.Text);
                    textBuilder.AppendLine();
                }
                else if (item is GridDocument doc)
                {
                    var (docHtml, docText) = SerializeDocumentAst(doc.AstDocument);
                    htmlBuilder.AppendLine("<article class=\"grove-document\">");
                    htmlBuilder.AppendLine(docHtml);
                    htmlBuilder.AppendLine("</article>");

                    textBuilder.AppendLine(docText);
                    textBuilder.AppendLine();
                }
                else if (item is GridImage img)
                {
                    htmlBuilder.AppendLine($"<img src=\"{System.Net.WebUtility.HtmlEncode(img.FilePath)}\" width=\"{img.IntrinsicWidthPx}\" height=\"{img.IntrinsicHeightPx}\"/>");
                    textBuilder.AppendLine($"[Image: {System.IO.Path.GetFileName(img.FilePath)}]");
                    textBuilder.AppendLine();
                }
            }

            return (htmlBuilder.ToString(), textBuilder.ToString().TrimEnd());
        }

        public static (string Html, string PlainText) SerializeDocumentAst(DocumentAst doc)
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
                else if (block is CodeBlockNode code)
                {
                    htmlBuilder.AppendLine($"<pre><code class=\"language-{code.Language}\">{System.Net.WebUtility.HtmlEncode(code.Code)}</code></pre>");
                    textBuilder.AppendLine($"``` {code.Language}\n{code.Code}\n```\n");
                }
                else if (block is ListBlockNode list)
                {
                    string tag = list.IsOrdered ? "ol" : "ul";
                    htmlBuilder.AppendLine($"<{tag}>");
                    foreach (var item in list.Items)
                    {
                        htmlBuilder.AppendLine($"  <li>{System.Net.WebUtility.HtmlEncode(item)}</li>");
                        textBuilder.AppendLine($"- {item}");
                    }
                    htmlBuilder.AppendLine($"</{tag}>");
                    textBuilder.AppendLine();
                }
            }

            return (htmlBuilder.ToString(), textBuilder.ToString());
        }
    }

    /// <summary>
    /// Implements ADR-014 Native System Clipboard Interop Service.
    /// Provides copy/paste operations supporting CF_HTML and plain text AST deserialization.
    /// </summary>
    public sealed class NativeClipboardService
    {
        private readonly Func<IClipboard?> _getClipboardFunc;

        public NativeClipboardService(Func<IClipboard?> getClipboardFunc)
        {
            _getClipboardFunc = getClipboardFunc ?? throw new ArgumentNullException(nameof(getClipboardFunc));
        }

        public async Task CopyItemsAsync(IEnumerable<GridContentItem> items)
        {
            var clipboard = _getClipboardFunc();
            if (clipboard is null) return;

            var (fragmentHtml, plainText) = AstHtmlConverter.SerializeItems(items);
            if (string.IsNullOrEmpty(plainText) && string.IsNullOrEmpty(fragmentHtml)) return;

            string cfHtml = CfHtmlSerializer.SerializeToCfHtml(fragmentHtml);

            DataObject dataObject = new DataObject();
            dataObject.Set(DataFormats.Text, plainText);
            dataObject.Set("HTML Format", cfHtml);

            await clipboard.SetDataObjectAsync(dataObject);
        }

        public async Task<List<GridContentItem>> PasteItemsAsync(CellCoordinate dropOrigin)
        {
            var clipboard = _getClipboardFunc();
            var results = new List<GridContentItem>();
            if (clipboard is null) return results;

            string? html = null;
            try
            {
                object? htmlData = await clipboard.GetDataAsync("HTML Format");
                if (htmlData is string rawHtml && !string.IsNullOrWhiteSpace(rawHtml))
                {
                    html = CfHtmlSerializer.ExtractFragment(rawHtml);
                }
            }
            catch { }

            string? text = null;
            try
            {
                text = await clipboard.GetTextAsync();
            }
            catch { }

            string contentToUse = !string.IsNullOrWhiteSpace(text) ? text : (html ?? "");
            if (string.IsNullOrWhiteSpace(contentToUse)) return results;

            if (contentToUse.Length < 500)
            {
                // Create Note
                var note = new GridNote(dropOrigin.X, dropOrigin.Y, contentToUse, NoteColor.Violet);
                results.Add(note);
            }
            else
            {
                // Create Document
                var doc = new GridDocument(dropOrigin.X, dropOrigin.Y, 2, 2, "Pasted Content", contentToUse);
                results.Add(doc);
            }

            return results;
        }
    }
}
