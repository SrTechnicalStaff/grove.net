using System;
using GroveApp.Engine;

namespace GroveApp.Models
{
    /// <summary>
    /// Represents a structured Document placement on the spatial Grid (ADR-011).
    /// Enforces paper styling (--surface-page #F5F5F5), multi-column reflow, and integral footprints (2x2 to 8x8 cells).
    /// </summary>
    public class GridDocument : GridContentItem
    {
        public override ContentKind Kind => ContentKind.Document;
        public override float Mass => 2.5f;

        public string Title { get; set; } = "";
        public string RawText { get; set; } = "";
        public DocumentAst AstDocument { get; set; }
        public int CurrentPage { get; set; } = 0;

        public GridDocument(int cellX, int cellY, int cellWidth = 2, int cellHeight = 2, string title = "", string rawText = "", bool isAnchored = false, int layerId = 0)
            : base(cellX, cellY, Math.Clamp(cellWidth, 2, 8), Math.Clamp(cellHeight, 2, 8), isAnchored, layerId)
        {
            Title = title;
            RawText = rawText;
            AstDocument = DocumentReflowEngine.ParseToAst(title, rawText);
        }

        public void UpdateText(string title, string rawText)
        {
            Title = title;
            RawText = rawText;
            AstDocument = DocumentReflowEngine.ParseToAst(title, rawText);
        }
    }
}
