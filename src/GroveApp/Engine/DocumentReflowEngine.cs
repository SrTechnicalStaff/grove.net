using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GroveApp.DesignSystem;

namespace GroveApp.Engine
{
    public abstract record AstNode;

    public sealed record DocumentAst : AstNode
    {
        public string? Title { get; init; }
        public string? FrontMatter { get; init; }
        public string? Abstract { get; init; }
        public string? ClosingLine { get; init; }
        public IReadOnlyList<string> TitleBlockCells { get; init; } = Array.Empty<string>();
        public IReadOnlyList<BlockNode> Blocks { get; init; } = Array.Empty<BlockNode>();
    }

    public abstract record BlockNode : AstNode;

    public sealed record HeadingBlock(int Level, string Text) : BlockNode;

    public sealed record ParagraphBlock(IReadOnlyList<InlineNode> Inlines) : BlockNode;

    public sealed record CodeBlockNode(string Code, string Language) : BlockNode;

    public sealed record ListBlockNode(IReadOnlyList<string> Items, bool IsOrdered) : BlockNode;

    public abstract record InlineNode : AstNode;

    public sealed record TextRunInline(string Text) : InlineNode;

    public sealed record FormattedInline(string Text, bool IsBold, bool IsItalic, bool IsCode) : InlineNode;

    public readonly record struct ColumnSlice(
        int ColumnIndex,
        double X,
        double Y,
        double Width,
        double Height,
        int StartBlockIndex,
        int EndBlockIndex);

    public readonly record struct PageLayoutResult(
        int PageIndex,
        int TotalPages,
        int ColumnsPerPage,
        double ColumnWidth,
        IReadOnlyList<ColumnSlice> Slices);

    /// <summary>
    /// Implements ADR-011 Document Physical Geometry, Page Texture, AST, and Multi-Column Reflow Engine.
    /// Manages text pagination across integral cell footprints (2x2 to 8x8 cells).
    /// </summary>
    public static class DocumentReflowEngine
    {
        public const double CellPitch = Tokens.GridCell; // 220.0px
        public const double MarginX = 28.0;
        public const double MarginY = 26.0;
        public const double ColumnGap = 18.0;
        public const double MinColumnWidth = 283.0; // TEXT_MIN_WIDTH_PX
        public const double BodyFontSize = 16.0;
        public const double LineHeightPx = 24.8;

        public static PageLayoutResult Reflow(DocumentAst doc, int cellWidth, int cellHeight, int pageIndex = 0)
        {
            cellWidth = Math.Clamp(cellWidth, 2, 8);
            cellHeight = Math.Clamp(cellHeight, 2, 8);

            double outerW = cellWidth * CellPitch;
            double outerH = cellHeight * CellPitch;
            double liveW = outerW - (2.0 * MarginX);
            double liveH = outerH - (2.0 * MarginY);

            int colsPerPage = Math.Max(1, (int)Math.Floor((liveW + ColumnGap) / (MinColumnWidth + ColumnGap)));
            double colWidth = (liveW - ((colsPerPage - 1) * ColumnGap)) / colsPerPage;
            int linesPerCol = Math.Max(1, (int)Math.Floor(liveH / LineHeightPx));

            List<ColumnSlice> slices = new();
            int totalBlocks = doc.Blocks.Count;

            if (totalBlocks == 0)
            {
                slices.Add(new ColumnSlice(0, MarginX, MarginY, colWidth, liveH, 0, -1));
                return new PageLayoutResult(0, 1, colsPerPage, colWidth, slices);
            }

            int currentBlock = 0;
            int colIdx = 0;

            while (currentBlock < totalBlocks)
            {
                double colX = MarginX + (colIdx % colsPerPage) * (colWidth + ColumnGap);
                int startBlock = currentBlock;
                int linesUsed = 0;

                while (currentBlock < totalBlocks && linesUsed < linesPerCol)
                {
                    var block = doc.Blocks[currentBlock];
                    int cost = CalculateBlockLineCost(block, colWidth);
                    if (linesUsed > 0 && linesUsed + cost > linesPerCol)
                    {
                        break; // Move to next column
                    }
                    linesUsed += Math.Max(1, cost);
                    currentBlock++;
                }

                if (currentBlock == startBlock)
                {
                    // Block is larger than column height, force advance at least one block
                    currentBlock++;
                }

                slices.Add(new ColumnSlice(colIdx, colX, MarginY, colWidth, liveH, startBlock, currentBlock - 1));
                colIdx++;
            }

            int totalPages = Math.Max(1, (int)Math.Ceiling((double)slices.Count / colsPerPage));
            pageIndex = Math.Clamp(pageIndex, 0, totalPages - 1);
            int sliceStart = pageIndex * colsPerPage;
            int sliceCount = Math.Min(colsPerPage, slices.Count - sliceStart);

            List<ColumnSlice> pageSlices = sliceStart < slices.Count
                ? slices.GetRange(sliceStart, Math.Max(0, sliceCount))
                : new List<ColumnSlice>();

            return new PageLayoutResult(pageIndex, totalPages, colsPerPage, colWidth, pageSlices);
        }

        private static int CalculateBlockLineCost(BlockNode block, double colWidth)
        {
            double charsPerLine = Math.Max(10.0, colWidth / 8.32);
            switch (block)
            {
                case HeadingBlock h:
                    return (int)Math.Ceiling(h.Text.Length / charsPerLine) + 1;
                case ParagraphBlock p:
                    int totalLen = p.Inlines.Sum(i => i switch
                    {
                        TextRunInline t => t.Text.Length,
                        FormattedInline f => f.Text.Length,
                        _ => 0
                    });
                    return (int)Math.Ceiling(totalLen / charsPerLine) + 1;
                case CodeBlockNode c:
                    int lines = c.Code.Split('\n').Length;
                    return lines + 1;
                case ListBlockNode l:
                    return l.Items.Count + 1;
                default:
                    return 2;
            }
        }

        public static DocumentAst ParseToAst(string? title, string content)
        {
            List<BlockNode> blocks = new();
            if (string.IsNullOrWhiteSpace(content))
            {
                return new DocumentAst { Title = title, Blocks = blocks };
            }

            string[] lines = content.Replace("\r\n", "\n").Split('\n');
            List<InlineNode> paragraphInlines = new();

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].TrimEnd();

                if (line.StartsWith("#"))
                {
                    if (paragraphInlines.Count > 0)
                    {
                        blocks.Add(new ParagraphBlock(new List<InlineNode>(paragraphInlines)));
                        paragraphInlines.Clear();
                    }

                    int level = 0;
                    while (level < line.Length && line[level] == '#') level++;
                    string text = line[level..].Trim();

                    if (level == 1 && string.IsNullOrEmpty(title))
                    {
                        title = text;
                    }
                    else
                    {
                        blocks.Add(new HeadingBlock(Math.Clamp(level, 1, 6), text));
                    }
                }
                else if (line.StartsWith("```"))
                {
                    if (paragraphInlines.Count > 0)
                    {
                        blocks.Add(new ParagraphBlock(new List<InlineNode>(paragraphInlines)));
                        paragraphInlines.Clear();
                    }

                    string lang = line[3..].Trim();
                    List<string> codeLines = new();
                    i++;
                    while (i < lines.Length && !lines[i].TrimStart().StartsWith("```"))
                    {
                        codeLines.Add(lines[i]);
                        i++;
                    }
                    blocks.Add(new CodeBlockNode(string.Join('\n', codeLines), lang));
                }
                else if (line.StartsWith("- ") || line.StartsWith("* "))
                {
                    if (paragraphInlines.Count > 0)
                    {
                        blocks.Add(new ParagraphBlock(new List<InlineNode>(paragraphInlines)));
                        paragraphInlines.Clear();
                    }

                    List<string> listItems = new() { line[2..].Trim() };
                    while (i + 1 < lines.Length && (lines[i + 1].TrimStart().StartsWith("- ") || lines[i + 1].TrimStart().StartsWith("* ")))
                    {
                        i++;
                        string itemLine = lines[i].TrimStart();
                        listItems.Add(itemLine[2..].Trim());
                    }
                    blocks.Add(new ListBlockNode(listItems, IsOrdered: false));
                }
                else if (string.IsNullOrWhiteSpace(line))
                {
                    if (paragraphInlines.Count > 0)
                    {
                        blocks.Add(new ParagraphBlock(new List<InlineNode>(paragraphInlines)));
                        paragraphInlines.Clear();
                    }
                }
                else
                {
                    if (paragraphInlines.Count > 0)
                    {
                        paragraphInlines.Add(new TextRunInline(" "));
                    }
                    ParseInlinesInto(line, paragraphInlines);
                }
            }

            if (paragraphInlines.Count > 0)
            {
                blocks.Add(new ParagraphBlock(new List<InlineNode>(paragraphInlines)));
            }

            return new DocumentAst
            {
                Title = title,
                Blocks = blocks
            };
        }

        private static void ParseInlinesInto(string text, List<InlineNode> targetList)
        {
            // Simple markdown inline parsing for bold (**), italic (*), code (`)
            var matches = Regex.Matches(text, @"(\*\*.*?\*\*|\*.*?\*|`.*?`|[^\*`]+)");
            foreach (Match match in matches)
            {
                string val = match.Value;
                if (val.StartsWith("**") && val.EndsWith("**") && val.Length >= 4)
                {
                    targetList.Add(new FormattedInline(val[2..^2], IsBold: true, IsItalic: false, IsCode: false));
                }
                else if (val.StartsWith("*") && val.EndsWith("*") && val.Length >= 2)
                {
                    targetList.Add(new FormattedInline(val[1..^1], IsBold: false, IsItalic: true, IsCode: false));
                }
                else if (val.StartsWith("`") && val.EndsWith("`") && val.Length >= 2)
                {
                    targetList.Add(new FormattedInline(val[1..^1], IsBold: false, IsItalic: false, IsCode: true));
                }
                else
                {
                    targetList.Add(new TextRunInline(val));
                }
            }
        }
    }
}
