namespace GroveApp.Models.Memory;

/// <summary>
/// The encoding or semantic kind of a memory payload.
/// </summary>
public enum MemoryPayloadKind : byte
{
    PlainText = 1,
    RichTextMarkdown = 2,
    BinaryImage = 3,
    PDFDocument = 4,
    StructuredJSON = 5,
    StructuredJson = StructuredJSON
}
