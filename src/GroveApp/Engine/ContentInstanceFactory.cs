using System;
using System.IO;
using Avalonia.Media.Imaging;
using GroveApp.Models;
using GroveApp.Models.Interaction;

namespace GroveApp.Engine;

public static class ContentInstanceFactory
{
    public static GridContentItem CreateTrace(GridContentItem source, int layerId)
    {
        ArgumentNullException.ThrowIfNull(source);

        GridContentItem trace = source switch
        {
            GridNote note => CreateNoteTrace(note, layerId),
            GridDocument document => CreateDocumentTrace(document, layerId),
            GridImage image => CreateImageTrace(image, layerId),
            _ => throw new ArgumentOutOfRangeException(nameof(source))
        };

        trace.MemoryId = source.MemoryId;
        return trace;
    }

    private static GridNote CreateNoteTrace(GridNote source, int layerId)
    {
        GridNote trace = new(source.CellX, source.CellY, source.Text, source.Color, layerId);
        trace.ResizeTo(new SpatialRegion(source.CellX, source.CellY, source.CellWidth, source.CellHeight));
        return trace;
    }

    private static GridDocument CreateDocumentTrace(GridDocument source, int layerId) =>
        new(source.CellX, source.CellY, source.CellWidth, source.CellHeight, source.Title, source.RawText, layerId);

    private static GridImage CreateImageTrace(GridImage source, int layerId)
    {
        GridImage trace = new(
            source.CellX,
            source.CellY,
            source.FilePath,
            source.IntrinsicWidthPx,
            source.IntrinsicHeightPx,
            layerId);
        trace.ResizeTo(new SpatialRegion(source.CellX, source.CellY, source.CellWidth, source.CellHeight));

        if (!string.IsNullOrWhiteSpace(source.FilePath) && File.Exists(source.FilePath))
        {
            using FileStream stream = File.OpenRead(source.FilePath);
            trace.LoadedBitmap = new Bitmap(stream);
        }

        return trace;
    }
}
