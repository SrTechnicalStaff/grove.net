using System;
using System.IO;
using System.Text;
using GroveApp.Models;
using GroveApp.Models.Memory;

namespace GroveApp.Engine.Memory;

/// <summary>
/// Converts a spatial placement into the immutable payload consumed by the
/// memory ledger. File and encoding concerns stay behind this engine seam so
/// Plane 0 controls do not own persistence details.
/// </summary>
public static class GridContentMemoryPayloadAdapter
{
    public static MemoryPayloadKind GetKind(GridContentItem item) => item.Kind switch
    {
        ContentKind.Note => MemoryPayloadKind.RichTextMarkdown,
        ContentKind.Document => MemoryPayloadKind.RichTextMarkdown,
        ContentKind.Image => MemoryPayloadKind.BinaryImage,
        _ => MemoryPayloadKind.PlainText
    };

    public static byte[] ReadPayload(GridContentItem item) => item switch
    {
        GridNote note => Encoding.UTF8.GetBytes(note.Text),
        GridDocument document => Encoding.UTF8.GetBytes(document.RawText),
        GridImage image => ReadImagePayload(image),
        _ => Encoding.UTF8.GetBytes(item.Id)
    };

    private static byte[] ReadImagePayload(GridImage image)
    {
        if (string.IsNullOrWhiteSpace(image.FilePath))
        {
            throw new InvalidDataException($"Image placement '{image.Id}' has no source file.");
        }

        return File.ReadAllBytes(image.FilePath);
    }
}
