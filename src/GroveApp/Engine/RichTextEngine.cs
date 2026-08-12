using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Media;
using GroveApp.DesignSystem;
using Colors = GroveApp.DesignSystem.Colors;

namespace GroveApp.Engine
{
    public enum BlockKind
    {
        Paragraph,
        Heading1,
        Heading2,
        Heading3,
        CodeBlock,
        Blockquote,
        UnorderedListItem,
        OrderedListItem
    }

    public enum SubSupMode
    {
        None,
        Subscript,
        Superscript
    }

    public class RichTextSpan
    {
        public int Start { get; set; }
        public int Length { get; set; }
        public FontWeight? Weight { get; set; }
        public FontStyle? Style { get; set; }
        public double? FontSize { get; set; }
        public FontFamily? FontFamily { get; set; }
        public IBrush? Foreground { get; set; }
        public IBrush? Background { get; set; }
        public bool IsUnderline { get; set; }
        public bool IsStrikethrough { get; set; }
        public SubSupMode SubSup { get; set; } = SubSupMode.None;
        public bool IsInlineCode { get; set; }
        public bool IsLink { get; set; }
        public bool IsMark { get; set; }
        public string? LinkUrl { get; set; }
    }

    public class RichTextBlock
    {
        public BlockKind Kind { get; set; } = BlockKind.Paragraph;
        public string CleanText { get; set; } = "";
        public List<RichTextSpan> Spans { get; set; } = new();
        public string CodeLanguage { get; set; } = "";
        public List<string> CodeLines { get; set; } = new();
        public int ListIndex { get; set; } = 1;
    }

    public class RichTextDocument
    {
        public string RawContent { get; }
        public List<RichTextBlock> Blocks { get; }

        public RichTextDocument(string rawContent, List<RichTextBlock> blocks)
        {
            RawContent = rawContent ?? "";
            Blocks = blocks ?? new List<RichTextBlock>();
        }
    }

    public class RichTextLayout
    {
        public RichTextDocument Document { get; set; } = null!;
        public string CleanText { get; set; } = "";
        public FormattedText FormattedText { get; set; } = null!;
        public List<RichTextSpan> Spans { get; set; } = new();
        public List<RichTextSpan> InlineCodeSpans { get; set; } = new();
        public double TotalHeight { get; set; }
        public double TotalWidth { get; set; }
        public double BaseFontSize { get; set; } = Typography.SizeBody;
        public IBrush DefaultForeground { get; set; } = Colors.NoteTextBrush;
        public double MaxWidth { get; set; } = 800.0;
        public double MaxHeight { get; set; } = double.PositiveInfinity;
        public double Zoom { get; set; } = 1.0;
    }

    internal class InlineStyleState
    {
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public bool Underline { get; set; }
        public bool Strikethrough { get; set; }
        public SubSupMode SubSup { get; set; } = SubSupMode.None;
        public bool IsInlineCode { get; set; }
        public bool IsMark { get; set; }
        public bool IsLink { get; set; }
        public FontFamily? FontFamily { get; set; }
        public IBrush? Foreground { get; set; }
        public IBrush? Background { get; set; }
        public string? LinkUrl { get; set; }

        public InlineStyleState Clone() => new InlineStyleState
        {
            Bold = Bold,
            Italic = Italic,
            Underline = Underline,
            Strikethrough = Strikethrough,
            SubSup = SubSup,
            IsInlineCode = IsInlineCode,
            IsMark = IsMark,
            IsLink = IsLink,
            FontFamily = FontFamily,
            Foreground = Foreground,
            Background = Background,
            LinkUrl = LinkUrl
        };

        public bool Matches(InlineStyleState other)
        {
            return Bold == other.Bold &&
                   Italic == other.Italic &&
                   Underline == other.Underline &&
                   Strikethrough == other.Strikethrough &&
                   SubSup == other.SubSup &&
                   IsInlineCode == other.IsInlineCode &&
                   IsMark == other.IsMark &&
                   IsLink == other.IsLink &&
                   FontFamily == other.FontFamily &&
                   Foreground == other.Foreground &&
                   Background == other.Background &&
                   LinkUrl == other.LinkUrl;
        }

        public bool IsDefault()
        {
            return !Bold && !Italic && !Underline && !Strikethrough &&
                   SubSup == SubSupMode.None && !IsInlineCode && !IsMark && !IsLink &&
                   FontFamily == null && Foreground == null && Background == null && LinkUrl == null;
        }
    }

    public static class RichTextEngine
    {
        // Styling Brushes & Pens
        private static readonly IBrush DefaultForegroundBrush = Colors.NoteTextBrush;
        private static readonly IBrush CodeBgBrush = Colors.SurfaceChromeBrush;
        private static readonly Pen CodeBorderPen = new Pen(Colors.HudSlateBorderBrush, 1.0);
        private static readonly IBrush InlineCodeBgBrush = Colors.SurfaceNestedBrush;
        private static readonly Pen InlineCodeBorderPen = new Pen(Colors.HudSlateBorderBrush, 1.0);
        private static readonly IBrush InlineCodeInkBrush = Colors.SignalActiveWorkBrush;
        private static readonly IBrush SignalInteractionBrush = Colors.SignalInteractionBrush;
        private static readonly Pen BlockquoteAccentPen = new Pen(Colors.SignalInteractionBrush, 3.0);
        private static readonly IBrush BlockquoteBgBrush = new SolidColorBrush(Color.FromArgb(20, Colors.SignalInteraction.R, Colors.SignalInteraction.G, Colors.SignalInteraction.B));
        private static readonly IBrush MarkBgBrush = new SolidColorBrush(Color.FromArgb(80, Colors.SignalActiveWork.R, Colors.SignalActiveWork.G, Colors.SignalActiveWork.B));
        private static readonly IBrush CodeLineNumberBrush = Colors.ContainmentEdgeBrush;

        // Syntax Highlighting Brushes
        private static readonly IBrush SyntaxKeywordBrush = Colors.SignalInteractionBrush;
        private static readonly IBrush SyntaxStringBrush = Colors.SignalAuthoredContextBrush;
        private static readonly IBrush SyntaxNumberBrush = Colors.NoteClayBrush;
        private static readonly IBrush SyntaxCommentBrush = Colors.ContainmentEdgeBrush;
        private static readonly IBrush SyntaxNormalBrush = Colors.NoteTextBrush;

        private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
        {
            "class", "public", "private", "protected", "static", "void", "return", "string",
            "int", "double", "float", "bool", "var", "if", "else", "true", "false", "new",
            "async", "await", "const", "function", "let", "import", "export", "from", "null",
            "this", "using", "namespace", "override", "virtual"
        };

        // -------------------------------------------------------------------
        // Public API Principles: CreateDocument, Measure, Render
        // -------------------------------------------------------------------

        public static RichTextDocument CreateDocument(string rawContent)
        {
            if (string.IsNullOrEmpty(rawContent))
            {
                return new RichTextDocument("", new List<RichTextBlock>());
            }

            var lines = rawContent.Replace("\r\n", "\n").Split('\n');
            var blocks = new List<RichTextBlock>();

            int i = 0;
            int orderedCounter = 1;

            while (i < lines.Length)
            {
                string line = lines[i];
                string trimmed = line.Trim();

                // 1. Code Block: ``` or <pre>
                if (trimmed.StartsWith("```") || trimmed.StartsWith("<pre>", StringComparison.OrdinalIgnoreCase))
                {
                    var codeBlock = new RichTextBlock
                    {
                        Kind = BlockKind.CodeBlock,
                        CodeLanguage = ExtractCodeLanguage(trimmed)
                    };

                    i++;
                    while (i < lines.Length)
                    {
                        string codeLine = lines[i];
                        string codeTrimmed = codeLine.Trim();
                        if (codeTrimmed.StartsWith("```") ||
                            codeTrimmed.EndsWith("</pre>", StringComparison.OrdinalIgnoreCase) ||
                            codeTrimmed.EndsWith("</code></pre>", StringComparison.OrdinalIgnoreCase))
                        {
                            i++;
                            break;
                        }
                        codeBlock.CodeLines.Add(codeLine);
                        i++;
                    }

                    blocks.Add(codeBlock);
                    orderedCounter = 1;
                    continue;
                }

                // 2. Headings (# H1, ## H2, ### H3 or <h1>, <h2>, <h3>)
                if (trimmed.StartsWith("# ") || trimmed.StartsWith("<h1>", StringComparison.OrdinalIgnoreCase))
                {
                    string inner = trimmed.StartsWith("# ") ? trimmed.Substring(2) : StripHtmlTag(trimmed, "h1");
                    var (clean, spans) = ParseInlineContent(inner);
                    blocks.Add(new RichTextBlock { Kind = BlockKind.Heading1, CleanText = clean, Spans = spans });
                    i++;
                    orderedCounter = 1;
                    continue;
                }
                if (trimmed.StartsWith("## ") || trimmed.StartsWith("<h2>", StringComparison.OrdinalIgnoreCase))
                {
                    string inner = trimmed.StartsWith("## ") ? trimmed.Substring(3) : StripHtmlTag(trimmed, "h2");
                    var (clean, spans) = ParseInlineContent(inner);
                    blocks.Add(new RichTextBlock { Kind = BlockKind.Heading2, CleanText = clean, Spans = spans });
                    i++;
                    orderedCounter = 1;
                    continue;
                }
                if (trimmed.StartsWith("### ") || trimmed.StartsWith("<h3>", StringComparison.OrdinalIgnoreCase))
                {
                    string inner = trimmed.StartsWith("### ") ? trimmed.Substring(4) : StripHtmlTag(trimmed, "h3");
                    var (clean, spans) = ParseInlineContent(inner);
                    blocks.Add(new RichTextBlock { Kind = BlockKind.Heading3, CleanText = clean, Spans = spans });
                    i++;
                    orderedCounter = 1;
                    continue;
                }

                // 3. Blockquote (> quote or <blockquote>)
                if (trimmed.StartsWith("> ") || trimmed.StartsWith("<blockquote>", StringComparison.OrdinalIgnoreCase))
                {
                    var quoteLines = new List<string>();
                    while (i < lines.Length)
                    {
                        string qLine = lines[i].Trim();
                        if (qLine.StartsWith("> "))
                        {
                            quoteLines.Add(qLine.Substring(2));
                            i++;
                        }
                        else if (qLine.StartsWith("<blockquote>", StringComparison.OrdinalIgnoreCase))
                        {
                            string inner = StripHtmlTag(qLine, "blockquote");
                            quoteLines.Add(inner);
                            i++;
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    string fullQuoteText = string.Join("\n", quoteLines);
                    var (clean, spans) = ParseInlineContent(fullQuoteText);
                    blocks.Add(new RichTextBlock { Kind = BlockKind.Blockquote, CleanText = clean, Spans = spans });
                    orderedCounter = 1;
                    continue;
                }

                // 4. Unordered List (- item, * item, <ul><li>)
                if (trimmed.StartsWith("- ") || trimmed.StartsWith("* ") || trimmed.StartsWith("<ul>", StringComparison.OrdinalIgnoreCase) || trimmed.StartsWith("<li>", StringComparison.OrdinalIgnoreCase))
                {
                    string itemText = trimmed;
                    if (itemText.StartsWith("- ")) itemText = itemText.Substring(2);
                    else if (itemText.StartsWith("* ")) itemText = itemText.Substring(2);
                    else itemText = StripHtmlTag(itemText, "li").Replace("<ul>", "").Replace("</ul>", "").Trim();

                    var (clean, spans) = ParseInlineContent(itemText);
                    blocks.Add(new RichTextBlock { Kind = BlockKind.UnorderedListItem, CleanText = clean, Spans = spans });
                    i++;
                    continue;
                }

                // 5. Ordered List (1. item, 2. item, <ol><li>)
                if (IsOrderedListPattern(trimmed, out string ordText, out int parsedIndex))
                {
                    int listIdx = parsedIndex > 0 ? parsedIndex : orderedCounter++;
                    var (clean, spans) = ParseInlineContent(ordText);
                    blocks.Add(new RichTextBlock { Kind = BlockKind.OrderedListItem, CleanText = clean, Spans = spans, ListIndex = listIdx });
                    i++;
                    continue;
                }

                // Empty line separator
                if (string.IsNullOrWhiteSpace(trimmed))
                {
                    i++;
                    orderedCounter = 1;
                    continue;
                }

                // Default Paragraph
                var (pClean, pSpans) = ParseInlineContent(trimmed);
                blocks.Add(new RichTextBlock { Kind = BlockKind.Paragraph, CleanText = pClean, Spans = pSpans });
                i++;
                orderedCounter = 1;
            }

            return new RichTextDocument(rawContent, blocks);
        }

        public static Size Measure(
            RichTextDocument document,
            double baseFontSize = Typography.SizeBody,
            double maxWidth = 800.0,
            double maxHeight = double.PositiveInfinity,
            double zoom = 1.0)
        {
            if (document == null || document.Blocks.Count == 0)
                return new Size(0, 0);

            double totalHeight = 0.0;
            double maxBlockWidth = 0.0;

            foreach (var block in document.Blocks)
            {
                if (totalHeight >= maxHeight) break;
                double blockMaxH = Math.Max(1.0, maxHeight - totalHeight);

                double bh = 0.0;
                switch (block.Kind)
                {
                    case BlockKind.Heading1:
                        bh = MeasureHeading(block, Typography.SizeHero, FontWeight.Bold, 1.3, maxWidth, blockMaxH) + 10.0;
                        break;
                    case BlockKind.Heading2:
                        bh = MeasureHeading(block, 18.0, FontWeight.Bold, 1.35, maxWidth, blockMaxH) + 8.0;
                        break;
                    case BlockKind.Heading3:
                        bh = MeasureHeading(block, Typography.SizeBody, FontWeight.SemiBold, 1.4, maxWidth, blockMaxH) + 6.0;
                        break;
                    case BlockKind.Paragraph:
                        bh = MeasureFormattedBlock(block, baseFontSize, 1.45, maxWidth, blockMaxH) + 8.0;
                        break;
                    case BlockKind.Blockquote:
                        bh = MeasureBlockquote(block, baseFontSize, maxWidth, blockMaxH, zoom) + 8.0;
                        break;
                    case BlockKind.CodeBlock:
                        bh = MeasureCodeBlock(block, baseFontSize, maxWidth, zoom) + 10.0;
                        break;
                    case BlockKind.UnorderedListItem:
                    case BlockKind.OrderedListItem:
                        bh = MeasureListItem(block, baseFontSize, maxWidth, blockMaxH, zoom) + 4.0;
                        break;
                }

                totalHeight += bh;
                maxBlockWidth = Math.Max(maxBlockWidth, maxWidth);
            }

            return new Size(maxBlockWidth, totalHeight);
        }

        public static void Render(
            DrawingContext context,
            Point origin,
            RichTextDocument document,
            double baseFontSize = Typography.SizeBody,
            IBrush? defaultForeground = null,
            double maxWidth = 800.0,
            double maxHeight = double.PositiveInfinity,
            double zoom = 1.0)
        {
            if (document == null || document.Blocks.Count == 0)
                return;

            defaultForeground ??= DefaultForegroundBrush;
            double currentY = origin.Y;

            foreach (var block in document.Blocks)
            {
                if (currentY - origin.Y >= maxHeight)
                    break;

                double blockMaxH = Math.Max(1.0, maxHeight - (currentY - origin.Y));

                switch (block.Kind)
                {
                    case BlockKind.Heading1:
                        currentY += RenderHeading(context, new Point(origin.X, currentY), block, Typography.SizeHero, FontWeight.Bold, 1.3, maxWidth, blockMaxH, defaultForeground, zoom, tracking: true);
                        currentY += 10.0;
                        break;

                    case BlockKind.Heading2:
                        currentY += RenderHeading(context, new Point(origin.X, currentY), block, 18.0, FontWeight.Bold, 1.35, maxWidth, blockMaxH, defaultForeground, zoom, tracking: false);
                        currentY += 8.0;
                        break;

                    case BlockKind.Heading3:
                        currentY += RenderHeading(context, new Point(origin.X, currentY), block, Typography.SizeBody, FontWeight.SemiBold, 1.4, maxWidth, blockMaxH, defaultForeground, zoom, tracking: false);
                        currentY += 6.0;
                        break;

                    case BlockKind.Paragraph:
                        currentY += RenderFormattedBlock(context, new Point(origin.X, currentY), block, baseFontSize, 1.45, maxWidth, blockMaxH, defaultForeground, zoom);
                        currentY += 8.0;
                        break;

                    case BlockKind.Blockquote:
                        currentY += RenderBlockquote(context, new Point(origin.X, currentY), block, baseFontSize, maxWidth, blockMaxH, zoom);
                        currentY += 8.0;
                        break;

                    case BlockKind.CodeBlock:
                        currentY += RenderCodeBlock(context, new Point(origin.X, currentY), block, baseFontSize, maxWidth, blockMaxH, zoom);
                        currentY += 10.0;
                        break;

                    case BlockKind.UnorderedListItem:
                        currentY += RenderListItem(context, new Point(origin.X, currentY), block, "•", baseFontSize, maxWidth, blockMaxH, defaultForeground, zoom);
                        currentY += 4.0;
                        break;

                    case BlockKind.OrderedListItem:
                        currentY += RenderListItem(context, new Point(origin.X, currentY), block, $"{block.ListIndex}.", baseFontSize, maxWidth, blockMaxH, defaultForeground, zoom);
                        currentY += 4.0;
                        break;
                }
            }
        }

        // -------------------------------------------------------------------
        // Backward-Compatibility Helpers: CreateLayout & Render(layout)
        // -------------------------------------------------------------------

        public static RichTextLayout CreateLayout(
            string markdownText,
            double baseFontSize,
            IBrush defaultForeground,
            double maxWidth,
            double maxHeight,
            double zoom = 1.0)
        {
            var doc = CreateDocument(markdownText);
            var size = Measure(doc, baseFontSize, maxWidth, maxHeight, zoom);

            // Fallback FormattedText for raw string compatibility
            var emptyFt = new FormattedText(
                doc.RawContent,
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

            return new RichTextLayout
            {
                Document = doc,
                CleanText = doc.RawContent,
                FormattedText = emptyFt,
                Spans = new List<RichTextSpan>(),
                InlineCodeSpans = new List<RichTextSpan>(),
                TotalWidth = size.Width,
                TotalHeight = size.Height,
                BaseFontSize = baseFontSize,
                DefaultForeground = defaultForeground,
                MaxWidth = maxWidth,
                MaxHeight = maxHeight,
                Zoom = zoom
            };
        }

        public static void Render(DrawingContext context, Point origin, RichTextLayout layout)
        {
            if (layout == null) return;
            if (layout.Document != null && layout.Document.Blocks.Count > 0)
            {
                Render(
                    context,
                    origin,
                    layout.Document,
                    layout.BaseFontSize,
                    layout.DefaultForeground,
                    layout.MaxWidth,
                    layout.MaxHeight,
                    layout.Zoom
                );
            }
            else if (layout.FormattedText != null && !string.IsNullOrEmpty(layout.CleanText))
            {
                context.DrawText(layout.FormattedText, origin);
            }
        }

        // -------------------------------------------------------------------
        // Sub-Renderers & Measure Helpers
        // -------------------------------------------------------------------

        private static double MeasureHeading(RichTextBlock block, double fontSize, FontWeight weight, double lineHeightMult, double maxWidth, double maxHeight)
        {
            var ft = CreateFormattedText(block.CleanText, block.Spans, fontSize, weight, FontStyle.Normal, Typography.UiFamily, DefaultForegroundBrush, lineHeightMult, maxWidth, maxHeight);
            return ft.Height;
        }

        private static double RenderHeading(
            DrawingContext context, Point point, RichTextBlock block, double fontSize, FontWeight weight, double lineHeightMult,
            double maxWidth, double maxHeight, IBrush defaultForeground, double zoom, bool tracking)
        {
            var ft = CreateFormattedText(block.CleanText, block.Spans, fontSize, weight, FontStyle.Normal, Typography.UiFamily, defaultForeground, lineHeightMult, maxWidth, maxHeight);
            RenderSpanHighlightsAndDecorations(context, point, ft, block.CleanText, block.Spans);
            context.DrawText(ft, point);
            return ft.Height;
        }

        private static double MeasureFormattedBlock(RichTextBlock block, double fontSize, double lineHeightMult, double maxWidth, double maxHeight)
        {
            var ft = CreateFormattedText(block.CleanText, block.Spans, fontSize, Typography.WeightNoteText, FontStyle.Normal, Typography.UiFamily, DefaultForegroundBrush, lineHeightMult, maxWidth, maxHeight);
            return ft.Height;
        }

        private static double RenderFormattedBlock(
            DrawingContext context, Point point, RichTextBlock block, double fontSize, double lineHeightMult,
            double maxWidth, double maxHeight, IBrush defaultForeground, double zoom)
        {
            var ft = CreateFormattedText(block.CleanText, block.Spans, fontSize, Typography.WeightNoteText, FontStyle.Normal, Typography.UiFamily, defaultForeground, lineHeightMult, maxWidth, maxHeight);
            RenderSpanHighlightsAndDecorations(context, point, ft, block.CleanText, block.Spans);
            context.DrawText(ft, point);
            return ft.Height;
        }

        private static double MeasureBlockquote(RichTextBlock block, double fontSize, double maxWidth, double maxHeight, double zoom)
        {
            double textMaxW = Math.Max(1.0, maxWidth - 16.0);
            var ft = CreateFormattedText(block.CleanText, block.Spans, fontSize, Typography.WeightNoteText, FontStyle.Italic, Typography.UiFamily, Colors.TextSecondaryBrush, 1.45, textMaxW, maxHeight);
            return ft.Height + 12.0;
        }

        private static double RenderBlockquote(
            DrawingContext context, Point point, RichTextBlock block, double fontSize, double maxWidth, double maxHeight, double zoom)
        {
            double textMaxW = Math.Max(1.0, maxWidth - 16.0);
            var ft = CreateFormattedText(block.CleanText, block.Spans, fontSize, Typography.WeightNoteText, FontStyle.Italic, Typography.UiFamily, Colors.TextSecondaryBrush, 1.45, textMaxW, maxHeight);

            double bqH = ft.Height + 12.0;
            Rect bgRect = new Rect(point.X, point.Y, maxWidth, bqH);

            context.FillRectangle(BlockquoteBgBrush, bgRect, 2.0f);
            context.DrawLine(BlockquoteAccentPen, new Point(point.X + 1.5, point.Y), new Point(point.X + 1.5, point.Y + bqH));

            Point textPt = new Point(point.X + 16.0, point.Y + 6.0);
            RenderSpanHighlightsAndDecorations(context, textPt, ft, block.CleanText, block.Spans);
            context.DrawText(ft, textPt);

            return bqH;
        }

        private static double MeasureCodeBlock(RichTextBlock block, double baseFontSize, double maxWidth, double zoom)
        {
            double codeFontSize = Math.Max(8.0, baseFontSize * 0.88);
            double codeLineH = codeFontSize * 1.35;
            int lineCount = Math.Max(1, block.CodeLines.Count);
            return 20.0 + lineCount * codeLineH;
        }

        private static double RenderCodeBlock(
            DrawingContext context, Point point, RichTextBlock block, double baseFontSize, double maxWidth, double maxHeight, double zoom)
        {
            double codeFontSize = Math.Max(8.0, baseFontSize * 0.88);
            double codeLineH = codeFontSize * 1.35;
            int lineCount = Math.Max(1, block.CodeLines.Count);

            int digitCount = Math.Max(2, lineCount.ToString().Length);
            double numColW = digitCount * (codeFontSize * 0.65) + 10.0;

            double padLeft = 12.0;
            double padTop = 10.0;
            double padBottom = 10.0;
            double totalH = padTop + padBottom + lineCount * codeLineH;

            Rect codeRect = new Rect(point.X, point.Y, maxWidth, totalH);
            context.FillRectangle(CodeBgBrush, codeRect, 4.0f);
            context.DrawRectangle(null, CodeBorderPen, codeRect, 4.0f);

            double codeTextX = point.X + padLeft + numColW;
            double codeTextMaxW = Math.Max(1.0, maxWidth - padLeft * 2 - numColW);

            for (int idx = 0; idx < block.CodeLines.Count; idx++)
            {
                double lineY = point.Y + padTop + idx * codeLineH;
                if (lineY - point.Y >= maxHeight) break;

                // Draw line number
                string numStr = (idx + 1).ToString().PadLeft(digitCount);
                var numFt = new FormattedText(
                    numStr,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(Typography.MonoFamily, FontStyle.Normal, Typography.WeightMono),
                    codeFontSize,
                    CodeLineNumberBrush
                );
                context.DrawText(numFt, new Point(point.X + padLeft, lineY));

                // Draw code line with syntax highlighting
                string rawLine = block.CodeLines[idx];
                var codeFt = HighlightCodeLine(rawLine, codeFontSize, codeTextMaxW);
                context.DrawText(codeFt, new Point(codeTextX, lineY));
            }

            return totalH;
        }

        private static FormattedText HighlightCodeLine(string rawLine, double fontSize, double maxW)
        {
            var ft = new FormattedText(
                rawLine,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(Typography.MonoFamily, FontStyle.Normal, Typography.WeightMono),
                fontSize,
                SyntaxNormalBrush
            )
            {
                MaxTextWidth = Math.Max(1.0, maxW)
            };

            if (string.IsNullOrEmpty(rawLine)) return ft;

            // Highlight comments
            int commentIdx = rawLine.IndexOf("//", StringComparison.Ordinal);
            if (commentIdx >= 0)
            {
                ft.SetForegroundBrush(SyntaxCommentBrush, commentIdx, rawLine.Length - commentIdx);
                ft.SetFontStyle(FontStyle.Italic, commentIdx, rawLine.Length - commentIdx);
                rawLine = rawLine.Substring(0, commentIdx);
            }

            // Highlight strings
            var stringMatches = Regex.Matches(rawLine, @"""([^""\\]|\\.)*""|'([^'\\]|\\.)*'");
            foreach (Match m in stringMatches)
            {
                ft.SetForegroundBrush(SyntaxStringBrush, m.Index, m.Length);
            }

            // Highlight numbers
            var numMatches = Regex.Matches(rawLine, @"\b\d+(\.\d+)?\b");
            foreach (Match m in numMatches)
            {
                ft.SetForegroundBrush(SyntaxNumberBrush, m.Index, m.Length);
            }

            // Highlight keywords
            var wordMatches = Regex.Matches(rawLine, @"\b[A-Za-z_][A-Za-z0-9_]*\b");
            foreach (Match m in wordMatches)
            {
                if (Keywords.Contains(m.Value))
                {
                    ft.SetForegroundBrush(SyntaxKeywordBrush, m.Index, m.Length);
                }
            }

            return ft;
        }

        private static double MeasureListItem(RichTextBlock block, double fontSize, double maxWidth, double maxHeight, double zoom)
        {
            double textMaxW = Math.Max(1.0, maxWidth - 22.0);
            var ft = CreateFormattedText(block.CleanText, block.Spans, fontSize, Typography.WeightNoteText, FontStyle.Normal, Typography.UiFamily, DefaultForegroundBrush, 1.45, textMaxW, maxHeight);
            return ft.Height;
        }

        private static double RenderListItem(
            DrawingContext context, Point point, RichTextBlock block, string prefix, double fontSize,
            double maxWidth, double maxHeight, IBrush defaultForeground, double zoom)
        {
            var prefixFt = new FormattedText(
                prefix,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(Typography.MonoFamily, FontStyle.Normal, Typography.WeightMono),
                fontSize,
                SignalInteractionBrush
            );
            context.DrawText(prefixFt, new Point(point.X + 4.0, point.Y));

            double textMaxW = Math.Max(1.0, maxWidth - 22.0);
            Point textPt = new Point(point.X + 22.0, point.Y);

            var ft = CreateFormattedText(block.CleanText, block.Spans, fontSize, Typography.WeightNoteText, FontStyle.Normal, Typography.UiFamily, defaultForeground, 1.45, textMaxW, maxHeight);
            RenderSpanHighlightsAndDecorations(context, textPt, ft, block.CleanText, block.Spans);
            context.DrawText(ft, textPt);

            return ft.Height;
        }

        // -------------------------------------------------------------------
        // FormattedText Creation & Span Decorations
        // -------------------------------------------------------------------

        private static FormattedText CreateFormattedText(
            string text, List<RichTextSpan> spans, double fontSize, FontWeight defaultWeight, FontStyle defaultStyle,
            FontFamily defaultFamily, IBrush defaultForeground, double lineHeightMult, double maxWidth, double maxHeight)
        {
            var ft = new FormattedText(
                text ?? "",
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(defaultFamily, defaultStyle, defaultWeight),
                fontSize,
                defaultForeground
            )
            {
                MaxTextWidth = Math.Max(1.0, maxWidth),
                MaxTextHeight = Math.Max(1.0, maxHeight),
                LineHeight = fontSize * lineHeightMult
            };

            if (string.IsNullOrEmpty(text) || spans == null) return ft;

            foreach (var span in spans)
            {
                if (span.Start < 0 || span.Length <= 0 || span.Start + span.Length > text.Length)
                    continue;

                if (span.Weight.HasValue) ft.SetFontWeight(span.Weight.Value, span.Start, span.Length);
                if (span.Style.HasValue) ft.SetFontStyle(span.Style.Value, span.Start, span.Length);
                if (span.FontSize.HasValue) ft.SetFontSize(span.FontSize.Value, span.Start, span.Length);
                if (span.FontFamily != null) ft.SetFontFamily(span.FontFamily, span.Start, span.Length);
                if (span.Foreground != null) ft.SetForegroundBrush(span.Foreground, span.Start, span.Length);

                if (span.SubSup != SubSupMode.None)
                {
                    ft.SetFontSize(fontSize * 0.75, span.Start, span.Length);
                }

                if (span.IsInlineCode)
                {
                    ft.SetFontFamily(Typography.MonoFamily, span.Start, span.Length);
                    ft.SetForegroundBrush(InlineCodeInkBrush, span.Start, span.Length);
                }
                else if (span.IsLink)
                {
                    ft.SetForegroundBrush(SignalInteractionBrush, span.Start, span.Length);
                }
            }

            return ft;
        }

        private static void RenderSpanHighlightsAndDecorations(
            DrawingContext context, Point origin, FormattedText ft, string text, List<RichTextSpan> spans)
        {
            if (string.IsNullOrEmpty(text) || spans == null || ft == null) return;

            foreach (var span in spans)
            {
                if (span.Start < 0 || span.Length <= 0 || span.Start + span.Length > text.Length)
                    continue;

                var geom = ft.BuildHighlightGeometry(origin, span.Start, span.Length);
                if (geom == null) continue;

                // 1. Inline Code background pill
                if (span.IsInlineCode)
                {
                    context.DrawGeometry(InlineCodeBgBrush, InlineCodeBorderPen, geom);
                }

                // 2. Mark background highlight
                if (span.IsMark)
                {
                    context.DrawGeometry(span.Background ?? MarkBgBrush, null, geom);
                }

                // 3. Underline / Link decoration
                if (span.IsUnderline || span.IsLink)
                {
                    Rect bounds = geom.Bounds;
                    var underlinePen = new Pen(span.Foreground ?? SignalInteractionBrush, 1.0);
                    context.DrawLine(underlinePen, new Point(bounds.Left, bounds.Bottom - 1), new Point(bounds.Right, bounds.Bottom - 1));
                }

                // 4. Strikethrough decoration
                if (span.IsStrikethrough)
                {
                    Rect bounds = geom.Bounds;
                    var strikePen = new Pen(span.Foreground ?? DefaultForegroundBrush, 1.0);
                    double midY = bounds.Top + bounds.Height * 0.5;
                    context.DrawLine(strikePen, new Point(bounds.Left, midY), new Point(bounds.Right, midY));
                }
            }
        }

        // -------------------------------------------------------------------
        // Inline Content Tokenizer & Parser
        // -------------------------------------------------------------------

        public static (string CleanText, List<RichTextSpan> Spans) ParseInlineContent(string input, int startOffset = 0)
        {
            if (string.IsNullOrEmpty(input))
                return ("", new List<RichTextSpan>());

            var stack = new Stack<InlineStyleState>();
            stack.Push(new InlineStyleState());

            var segments = new List<(char Ch, InlineStyleState State)>();

            int i = 0;
            while (i < input.Length)
            {
                // HTML Tag
                if (input[i] == '<' && TryParseHtmlTag(input, i, out int tagLen, out bool isClosing, out string tagName, out Dictionary<string, string> attrs))
                {
                    if (tagName == "br")
                    {
                        segments.Add(('\n', stack.Peek().Clone()));
                    }
                    else if (isClosing)
                    {
                        if (stack.Count > 1) stack.Pop();
                    }
                    else
                    {
                        var nextState = stack.Peek().Clone();
                        ApplyHtmlTagToState(tagName, attrs, nextState);
                        stack.Push(nextState);
                    }
                    i += tagLen;
                    continue;
                }

                // Markdown Link or Color Span: [text](#color) or [label](url)
                if (input[i] == '[' && TryParseMarkdownLinkOrColor(input, i, out int matchLen, out string labelText, out string targetStr))
                {
                    var currentState = stack.Peek().Clone();
                    if (TryParseColor(targetStr, out IBrush? colorBrush))
                    {
                        currentState.Foreground = colorBrush;
                    }
                    else
                    {
                        currentState.IsLink = true;
                        currentState.LinkUrl = targetStr;
                        currentState.Foreground = SignalInteractionBrush;
                    }

                    stack.Push(currentState);
                    var (innerClean, _) = ParseInlineContent(labelText);
                    foreach (char c in innerClean)
                    {
                        segments.Add((c, currentState.Clone()));
                    }
                    stack.Pop();

                    i += matchLen;
                    continue;
                }

                // Bold: **text** or __text__
                if ((HasPrefix(input, i, "**") || HasPrefix(input, i, "__")) && TryMatchPair(input, i, input.Substring(i, 2), out int boldLen, out string boldInner))
                {
                    var nextState = stack.Peek().Clone();
                    nextState.Bold = true;
                    stack.Push(nextState);
                    var (innerClean, _) = ParseInlineContent(boldInner);
                    foreach (char c in innerClean) segments.Add((c, nextState.Clone()));
                    stack.Pop();
                    i += boldLen;
                    continue;
                }

                // Strikethrough: ~~text~~
                if (HasPrefix(input, i, "~~") && TryMatchPair(input, i, "~~", out int strikeLen, out string strikeInner))
                {
                    var nextState = stack.Peek().Clone();
                    nextState.Strikethrough = true;
                    stack.Push(nextState);
                    var (innerClean, _) = ParseInlineContent(strikeInner);
                    foreach (char c in innerClean) segments.Add((c, nextState.Clone()));
                    stack.Pop();
                    i += strikeLen;
                    continue;
                }

                // Inline Code: `text`
                if (input[i] == '`' && TryMatchPair(input, i, "`", out int codeLen, out string codeInner))
                {
                    var nextState = stack.Peek().Clone();
                    nextState.IsInlineCode = true;
                    stack.Push(nextState);
                    foreach (char c in codeInner) segments.Add((c, nextState.Clone()));
                    stack.Pop();
                    i += codeLen;
                    continue;
                }

                // Italic: *text* or _text_
                if ((input[i] == '*' || input[i] == '_') && TryMatchSingleStarOrUnderscore(input, i, out int italicLen, out string italicInner))
                {
                    var nextState = stack.Peek().Clone();
                    nextState.Italic = true;
                    stack.Push(nextState);
                    var (innerClean, _) = ParseInlineContent(italicInner);
                    foreach (char c in innerClean) segments.Add((c, nextState.Clone()));
                    stack.Pop();
                    i += italicLen;
                    continue;
                }

                // Normal Character
                segments.Add((input[i], stack.Peek().Clone()));
                i++;
            }

            return ConvertSegmentsToSpans(segments, startOffset);
        }

        private static (string CleanText, List<RichTextSpan> Spans) ConvertSegmentsToSpans(List<(char Ch, InlineStyleState State)> segments, int startOffset)
        {
            var sb = new StringBuilder();
            var spans = new List<RichTextSpan>();

            if (segments == null || segments.Count == 0)
                return ("", spans);

            int runStart = 0;
            var currentState = segments[0].State;

            for (int i = 0; i < segments.Count; i++)
            {
                sb.Append(segments[i].Ch);

                if (i == segments.Count - 1 || !segments[i + 1].State.Matches(currentState))
                {
                    int runLen = i - runStart + 1;
                    if (!currentState.IsDefault())
                    {
                        spans.Add(new RichTextSpan
                        {
                            Start = startOffset + runStart,
                            Length = runLen,
                            Weight = currentState.Bold ? FontWeight.Bold : null,
                            Style = currentState.Italic ? FontStyle.Italic : null,
                            IsUnderline = currentState.Underline,
                            IsStrikethrough = currentState.Strikethrough,
                            SubSup = currentState.SubSup,
                            IsInlineCode = currentState.IsInlineCode,
                            IsMark = currentState.IsMark,
                            IsLink = currentState.IsLink,
                            LinkUrl = currentState.LinkUrl,
                            FontFamily = currentState.FontFamily,
                            Foreground = currentState.Foreground,
                            Background = currentState.Background
                        });
                    }

                    if (i < segments.Count - 1)
                    {
                        runStart = i + 1;
                        currentState = segments[i + 1].State;
                    }
                }
            }

            return (sb.ToString(), spans);
        }

        // -------------------------------------------------------------------
        // Lexical & Parsing Helpers
        // -------------------------------------------------------------------

        private static bool HasPrefix(string text, int index, string prefix)
        {
            if (index + prefix.Length > text.Length) return false;
            for (int k = 0; k < prefix.Length; k++)
            {
                if (text[index + k] != prefix[k]) return false;
            }
            return true;
        }

        private static bool TryMatchPair(string input, int startIdx, string delimiter, out int matchLen, out string innerText)
        {
            matchLen = 0;
            innerText = "";

            int searchFrom = startIdx + delimiter.Length;
            int endIdx = input.IndexOf(delimiter, searchFrom, StringComparison.Ordinal);
            if (endIdx < 0) return false;

            innerText = input.Substring(searchFrom, endIdx - searchFrom);
            matchLen = (endIdx + delimiter.Length) - startIdx;
            return true;
        }

        private static bool TryMatchSingleStarOrUnderscore(string input, int startIdx, out int matchLen, out string innerText)
        {
            matchLen = 0;
            innerText = "";

            char delim = input[startIdx];
            int searchFrom = startIdx + 1;
            int endIdx = searchFrom;

            while (endIdx < input.Length)
            {
                if (input[endIdx] == delim)
                {
                    if (endIdx + 1 < input.Length && input[endIdx + 1] == delim)
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

        private static bool TryParseHtmlTag(string input, int startIdx, out int tagLen, out bool isClosing, out string tagName, out Dictionary<string, string> attrs)
        {
            tagLen = 0;
            isClosing = false;
            tagName = "";
            attrs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            int endIdx = input.IndexOf('>', startIdx);
            if (endIdx < 0) return false;

            string content = input.Substring(startIdx + 1, endIdx - startIdx - 1).Trim();
            tagLen = endIdx - startIdx + 1;

            if (content.StartsWith("/"))
            {
                isClosing = true;
                content = content.Substring(1).Trim();
            }

            if (content.EndsWith("/"))
            {
                content = content.Substring(0, content.Length - 1).Trim();
            }

            int spaceIdx = content.IndexOf(' ');
            if (spaceIdx < 0)
            {
                tagName = content.ToLowerInvariant();
            }
            else
            {
                tagName = content.Substring(0, spaceIdx).ToLowerInvariant();
                string attrStr = content.Substring(spaceIdx + 1);

                var attrMatches = Regex.Matches(attrStr, @"([a-zA-Z0-9_\-]+)=(?:""([^""]*)""|'([^']*)'|(\S+))");
                foreach (Match m in attrMatches)
                {
                    string key = m.Groups[1].Value;
                    string val = m.Groups[2].Success ? m.Groups[2].Value :
                                 m.Groups[3].Success ? m.Groups[3].Value :
                                 m.Groups[4].Value;
                    attrs[key] = val;
                }
            }

            return !string.IsNullOrEmpty(tagName);
        }

        private static void ApplyHtmlTagToState(string tag, Dictionary<string, string> attrs, InlineStyleState state)
        {
            switch (tag.ToLowerInvariant())
            {
                case "b":
                case "strong":
                    state.Bold = true;
                    break;
                case "i":
                case "em":
                    state.Italic = true;
                    break;
                case "u":
                    state.Underline = true;
                    break;
                case "del":
                case "s":
                case "strike":
                    state.Strikethrough = true;
                    break;
                case "sub":
                    state.SubSup = SubSupMode.Subscript;
                    break;
                case "sup":
                    state.SubSup = SubSupMode.Superscript;
                    break;
                case "code":
                    state.IsInlineCode = true;
                    break;
                case "mark":
                    state.IsMark = true;
                    state.Background = MarkBgBrush;
                    break;
                case "font":
                    if (attrs.TryGetValue("face", out string? faceVal))
                    {
                        state.FontFamily = new FontFamily(faceVal);
                    }
                    break;
                case "span":
                    if (attrs.TryGetValue("style", out string? styleVal))
                    {
                        ParseStyleAttribute(styleVal, state);
                    }
                    break;
                case "a":
                    state.IsLink = true;
                    if (attrs.TryGetValue("href", out string? hrefVal))
                    {
                        state.LinkUrl = hrefVal;
                    }
                    state.Foreground = SignalInteractionBrush;
                    break;
            }
        }

        private static void ParseStyleAttribute(string styleStr, InlineStyleState state)
        {
            var parts = styleStr.Split(';');
            foreach (var part in parts)
            {
                var kv = part.Split(':');
                if (kv.Length == 2)
                {
                    string prop = kv[0].Trim().ToLowerInvariant();
                    string val = kv[1].Trim();

                    if (prop == "color")
                    {
                        if (TryParseColor(val, out IBrush? brush))
                        {
                            state.Foreground = brush;
                        }
                    }
                    else if (prop == "background-color" || prop == "background")
                    {
                        if (TryParseColor(val, out IBrush? brush))
                        {
                            state.Background = brush;
                            state.IsMark = true;
                        }
                    }
                    else if (prop == "font-family")
                    {
                        state.FontFamily = new FontFamily(val);
                    }
                }
            }
        }

        private static bool TryParseMarkdownLinkOrColor(string input, int startIdx, out int matchLen, out string labelText, out string targetStr)
        {
            matchLen = 0;
            labelText = "";
            targetStr = "";

            int closingBracket = input.IndexOf(']', startIdx + 1);
            if (closingBracket < 0 || closingBracket + 1 >= input.Length || input[closingBracket + 1] != '(')
                return false;

            int closingParen = input.IndexOf(')', closingBracket + 2);
            if (closingParen < 0) return false;

            labelText = input.Substring(startIdx + 1, closingBracket - (startIdx + 1));
            targetStr = input.Substring(closingBracket + 2, closingParen - (closingBracket + 2));
            matchLen = closingParen - startIdx + 1;
            return true;
        }

        private static bool TryParseColor(string str, out IBrush? brush)
        {
            brush = null;
            if (string.IsNullOrWhiteSpace(str)) return false;

            try
            {
                string hex = str.Trim();
                if (!hex.StartsWith("#") && !hex.StartsWith("rgb", StringComparison.OrdinalIgnoreCase))
                {
                    hex = "#" + hex;
                }
                Color c = Color.Parse(hex);
                brush = new SolidColorBrush(c);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string ExtractCodeLanguage(string line)
        {
            if (line.StartsWith("```"))
            {
                return line.Substring(3).Trim();
            }
            if (line.Contains("class=\"language-"))
            {
                int idx = line.IndexOf("class=\"language-", StringComparison.Ordinal) + 16;
                int endIdx = line.IndexOf('"', idx);
                if (endIdx > idx) return line.Substring(idx, endIdx - idx);
            }
            return "";
        }

        private static string StripHtmlTag(string input, string tag)
        {
            string openPattern = $"<{tag}>";
            string closePattern = $"</{tag}>";

            string s = input;
            if (s.StartsWith(openPattern, StringComparison.OrdinalIgnoreCase))
            {
                s = s.Substring(openPattern.Length);
            }
            if (s.EndsWith(closePattern, StringComparison.OrdinalIgnoreCase))
            {
                s = s.Substring(0, s.Length - closePattern.Length);
            }
            return s.Trim();
        }

        private static bool IsOrderedListPattern(string trimmed, out string content, out int index)
        {
            content = "";
            index = 1;
            var match = Regex.Match(trimmed, @"^(\d+)\.\s+(.*)$");
            if (match.Success)
            {
                int.TryParse(match.Groups[1].Value, out index);
                content = match.Groups[2].Value;
                return true;
            }
            return false;
        }
    }
}
