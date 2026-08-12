using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Avalonia;
using Avalonia.Media;
using GroveApp.DesignSystem;

namespace GroveApp.Engine
{
    public class RichTextSpan
    {
        public int Start { get; set; }
        public int Length { get; set; }
        public FontWeight? Weight { get; set; }
        public FontStyle? Style { get; set; }
        public double? FontSize { get; set; }
        public FontFamily? FontFamily { get; set; }
        public IBrush? Foreground { get; set; }
        public bool IsInlineCode { get; set; }
    }

    public class RichTextLayout
    {
        public string CleanText { get; set; } = "";
        public FormattedText FormattedText { get; set; } = null!;
        public List<RichTextSpan> Spans { get; set; } = new();
        public List<RichTextSpan> InlineCodeSpans { get; set; } = new();
    }

    public static class RichTextEngine
    {
        private static readonly IBrush CodeBackgroundBrush = new SolidColorBrush(Color.FromArgb(70, 10, 10, 14));
        private static readonly Pen CodeBorderPen = new Pen(new SolidColorBrush(Color.FromArgb(90, 255, 255, 255)), 1.0);

        public static RichTextLayout CreateLayout(
            string markdownText,
            double baseFontSize,
            IBrush defaultForeground,
            double maxWidth,
            double maxHeight,
            double zoom = 1.0)
        {
            if (string.IsNullOrEmpty(markdownText))
            {
                var emptyFt = new FormattedText(
                    "",
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(Typography.UiFamily, FontStyle.Normal, Typography.WeightNoteText),
                    baseFontSize,
                    defaultForeground
                );
                return new RichTextLayout { CleanText = "", FormattedText = emptyFt };
            }

            var (cleanText, spans) = ParseMarkdown(markdownText, baseFontSize, zoom);

            var formattedText = new FormattedText(
                cleanText,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(Typography.UiFamily, FontStyle.Normal, Typography.WeightNoteText),
                baseFontSize,
                defaultForeground
            )
            {
                MaxTextWidth = Math.Max(1.0, maxWidth),
                MaxTextHeight = Math.Max(1.0, maxHeight),
                LineHeight = baseFontSize * Typography.LineHeightSnug
            };

            var inlineCodeSpans = new List<RichTextSpan>();

            foreach (var span in spans)
            {
                if (span.Start < 0 || span.Length <= 0 || span.Start + span.Length > cleanText.Length)
                    continue;

                if (span.Weight.HasValue)
                    formattedText.SetFontWeight(span.Weight.Value, span.Start, span.Length);

                if (span.Style.HasValue)
                    formattedText.SetFontStyle(span.Style.Value, span.Start, span.Length);

                if (span.FontSize.HasValue)
                    formattedText.SetFontSize(span.FontSize.Value, span.Start, span.Length);

                if (span.FontFamily != null)
                    formattedText.SetFontFamily(span.FontFamily, span.Start, span.Length);

                if (span.Foreground != null)
                    formattedText.SetForegroundBrush(span.Foreground, span.Start, span.Length);

                if (span.IsInlineCode)
                    inlineCodeSpans.Add(span);
            }

            return new RichTextLayout
            {
                CleanText = cleanText,
                FormattedText = formattedText,
                Spans = spans,
                InlineCodeSpans = inlineCodeSpans
            };
        }

        public static void Render(DrawingContext context, Point origin, RichTextLayout layout)
        {
            if (layout == null || layout.FormattedText == null || string.IsNullOrEmpty(layout.CleanText))
                return;

            // 1. Render background tint for inline code spans
            foreach (var span in layout.InlineCodeSpans)
            {
                if (span.Start < 0 || span.Length <= 0 || span.Start + span.Length > layout.CleanText.Length)
                    continue;

                var highlightGeom = layout.FormattedText.BuildHighlightGeometry(origin, span.Start, span.Length);
                if (highlightGeom != null)
                {
                    context.DrawGeometry(CodeBackgroundBrush, CodeBorderPen, highlightGeom);
                }
            }

            // 2. Render formatted text
            context.DrawText(layout.FormattedText, origin);
        }

        public static (string CleanText, List<RichTextSpan> Spans) ParseMarkdown(string markdown, double baseFontSize, double zoom)
        {
            var lines = markdown.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var sb = new StringBuilder();
            var allSpans = new List<RichTextSpan>();

            for (int lineIdx = 0; lineIdx < lines.Length; lineIdx++)
            {
                if (lineIdx > 0)
                    sb.Append('\n');

                int lineStartOffset = sb.Length;
                string rawLine = lines[lineIdx];

                double? lineFontSize = null;
                FontWeight? lineFontWeight = null;
                string lineTextToParse = rawLine;

                // Headings
                if (rawLine.StartsWith("# "))
                {
                    lineTextToParse = rawLine.Substring(2);
                    lineFontSize = baseFontSize * (Typography.SizeTitle / Typography.SizeBody);
                    lineFontWeight = FontWeight.Bold;
                }
                else if (rawLine.StartsWith("## "))
                {
                    lineTextToParse = rawLine.Substring(3);
                    lineFontSize = baseFontSize * (Typography.SizeLead / Typography.SizeBody);
                    lineFontWeight = FontWeight.Bold;
                }
                else if (rawLine.StartsWith("### "))
                {
                    lineTextToParse = rawLine.Substring(4);
                    lineFontSize = baseFontSize * (Typography.SizeLead / Typography.SizeBody);
                    lineFontWeight = FontWeight.Bold;
                }
                // Bullet lists
                else if (rawLine.StartsWith("- ") || rawLine.StartsWith("* "))
                {
                    lineTextToParse = "• " + rawLine.Substring(2);
                }

                var (cleanLine, lineSpans) = ParseInline(lineTextToParse, lineStartOffset);
                sb.Append(cleanLine);

                // Add line-level heading style
                if (lineFontSize.HasValue || lineFontWeight.HasValue)
                {
                    allSpans.Add(new RichTextSpan
                    {
                        Start = lineStartOffset,
                        Length = cleanLine.Length,
                        FontSize = lineFontSize,
                        Weight = lineFontWeight
                    });
                }

                allSpans.AddRange(lineSpans);
            }

            return (sb.ToString(), allSpans);
        }

        private static (string Clean, List<RichTextSpan> Spans) ParseInline(string input, int currentOffset)
        {
            var sb = new StringBuilder();
            var spans = new List<RichTextSpan>();

            int i = 0;
            while (i < input.Length)
            {
                // Color Span: [text](#color)
                if (input[i] == '[' && TryMatchColorSpan(input, i, out int colorMatchLen, out string colorText, out string colorHex))
                {
                    int spanStart = currentOffset + sb.Length;
                    var (cleanSub, subSpans) = ParseInline(colorText, spanStart);
                    sb.Append(cleanSub);
                    spans.AddRange(subSpans);

                    IBrush? brush = ParseColorBrush(colorHex);
                    if (brush != null)
                    {
                        spans.Add(new RichTextSpan
                        {
                            Start = spanStart,
                            Length = cleanSub.Length,
                            Foreground = brush
                        });
                    }

                    i += colorMatchLen;
                }
                // Bold: **text**
                else if (i + 1 < input.Length && input[i] == '*' && input[i + 1] == '*' && TryMatchPair(input, i, "**", out int boldMatchLen, out string boldText))
                {
                    int spanStart = currentOffset + sb.Length;
                    var (cleanSub, subSpans) = ParseInline(boldText, spanStart);
                    sb.Append(cleanSub);
                    spans.AddRange(subSpans);

                    spans.Add(new RichTextSpan
                    {
                        Start = spanStart,
                        Length = cleanSub.Length,
                        Weight = FontWeight.Bold
                    });

                    i += boldMatchLen;
                }
                // Inline Code: `code`
                else if (input[i] == '`' && TryMatchPair(input, i, "`", out int codeMatchLen, out string codeText))
                {
                    int spanStart = currentOffset + sb.Length;
                    sb.Append(codeText);

                    spans.Add(new RichTextSpan
                    {
                        Start = spanStart,
                        Length = codeText.Length,
                        FontFamily = Typography.MonoFamily,
                        IsInlineCode = true
                    });

                    i += codeMatchLen;
                }
                // Italic: *text*
                else if (input[i] == '*' && (i + 1 >= input.Length || input[i + 1] != '*') && TryMatchSingleStarItalic(input, i, out int italicMatchLen, out string italicText))
                {
                    int spanStart = currentOffset + sb.Length;
                    var (cleanSub, subSpans) = ParseInline(italicText, spanStart);
                    sb.Append(cleanSub);
                    spans.AddRange(subSpans);

                    spans.Add(new RichTextSpan
                    {
                        Start = spanStart,
                        Length = cleanSub.Length,
                        Style = FontStyle.Italic
                    });

                    i += italicMatchLen;
                }
                else
                {
                    sb.Append(input[i]);
                    i++;
                }
            }

            return (sb.ToString(), spans);
        }

        private static bool TryMatchColorSpan(string input, int startIdx, out int matchLen, out string text, out string hex)
        {
            matchLen = 0;
            text = "";
            hex = "";

            int closingBracket = input.IndexOf(']', startIdx + 1);
            if (closingBracket < 0 || closingBracket + 1 >= input.Length || input[closingBracket + 1] != '(')
                return false;

            int closingParen = input.IndexOf(')', closingBracket + 2);
            if (closingParen < 0)
                return false;

            text = input.Substring(startIdx + 1, closingBracket - (startIdx + 1));
            hex = input.Substring(closingBracket + 2, closingParen - (closingBracket + 2));
            matchLen = closingParen - startIdx + 1;
            return true;
        }

        private static bool TryMatchPair(string input, int startIdx, string delimiter, out int matchLen, out string innerText)
        {
            matchLen = 0;
            innerText = "";

            int searchFrom = startIdx + delimiter.Length;
            int endIdx = input.IndexOf(delimiter, searchFrom, StringComparison.Ordinal);
            if (endIdx < 0)
                return false;

            innerText = input.Substring(searchFrom, endIdx - searchFrom);
            matchLen = (endIdx + delimiter.Length) - startIdx;
            return true;
        }

        private static bool TryMatchSingleStarItalic(string input, int startIdx, out int matchLen, out string innerText)
        {
            matchLen = 0;
            innerText = "";

            int searchFrom = startIdx + 1;
            int endIdx = searchFrom;
            while (endIdx < input.Length)
            {
                if (input[endIdx] == '*')
                {
                    // Check if it's double asterisk **
                    if (endIdx + 1 < input.Length && input[endIdx + 1] == '*')
                    {
                        endIdx += 2;
                        continue;
                    }

                    innerText = input.Substring(searchFrom, endIdx - searchFrom);
                    matchLen = endIdx + 1 - startIdx;
                    return true;
                }
                endIdx++;
            }

            return false;
        }

        private static IBrush? ParseColorBrush(string colorStr)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(colorStr))
                    return null;

                string hex = colorStr.Trim();
                if (!hex.StartsWith("#") && !hex.StartsWith("rgb", StringComparison.OrdinalIgnoreCase))
                {
                    hex = "#" + hex;
                }

                Color c = Color.Parse(hex);
                return new SolidColorBrush(c);
            }
            catch
            {
                return null;
            }
        }
    }
}
