using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using GroveApp.DesignSystem;
using GroveApp.Engine;
using GroveApp.Models;
using Colors = GroveApp.DesignSystem.Colors;

namespace GroveApp.Controls;

/// <summary>
/// Plane 0 rendering adapter for the pure field-ledger module. Discrete aura
/// cells are filled here; contour topology is handed to a Skia draw operation.
/// </summary>
public sealed class FieldLedgerRenderModule
{
    private readonly FieldLedgerModule _fieldLedger = new();

    public void RenderAuraFills(
        DrawingContext context,
        Matrix cameraTransform,
        double cellSize,
        double zoom,
        int minCellX,
        int maxCellX,
        int minCellY,
        int maxCellY,
        FieldLedgerEngine fieldEngine,
        IEnumerable<GridContentItem> items) =>
        RenderAuraFills(
            context,
            cameraTransform.Transform,
            cellSize,
            zoom,
            minCellX,
            maxCellX,
            minCellY,
            maxCellY,
            fieldEngine,
            items);

    public void RenderAuraFills(
        DrawingContext context,
        Func<Point, Point> worldToScreen,
        double cellSize,
        double zoom,
        int minCellX,
        int maxCellX,
        int minCellY,
        int maxCellY,
        FieldLedgerEngine fieldEngine,
        IEnumerable<GridContentItem> items)
    {
        IReadOnlySet<(int col, int row)> auraCells = _fieldLedger.PrepareAuraCells(
            items,
            minCellX,
            maxCellX,
            minCellY,
            maxCellY);
        foreach (var (col, row) in auraCells)
        {
            CellLedgerEntry entry = fieldEngine.GetCellLedger(col, row);
            if (entry.FieldEnergy <= CellLedgerEntry.BaselineEnergy || entry.CompositeColor == Colors.SurfaceGrid)
            {
                continue;
            }

            Point screenOrigin = worldToScreen(new Point(col * cellSize, row * cellSize));
            context.FillRectangle(
                new SolidColorBrush(entry.CompositeColor),
                new Rect(screenOrigin.X, screenOrigin.Y, cellSize * zoom, cellSize * zoom));
        }
    }

    public IReadOnlyList<PerimeterContourEdge> GetPerimeterEdges(
        FieldLedgerEngine fieldEngine,
        int minCellX,
        int maxCellX,
        int minCellY,
        int maxCellY) =>
        fieldEngine.PerimeterSubscriber.GetBoundaryEdges(minCellX, maxCellX, minCellY, maxCellY);

    public void RenderSelectedAura(
        DrawingContext context,
        Matrix cameraTransform,
        double cellSize,
        double zoom,
        IReadOnlyList<SelectedFieldCell> selectedCells)
    {
        foreach (SelectedFieldCell selectedCell in selectedCells)
        {
            double alpha = Math.Clamp(
                selectedCell.SelectedEnergy * Tokens.FieldGain,
                Tokens.FieldAlphaMin,
                Tokens.FieldAlphaMax);
            Color color = Color.FromArgb(
                ToByte(alpha * byte.MaxValue),
                selectedCell.SelectedColor.R,
                selectedCell.SelectedColor.G,
                selectedCell.SelectedColor.B);
            Point screenOrigin = cameraTransform.Transform(
                new Point(selectedCell.Col * cellSize, selectedCell.Row * cellSize));
            context.FillRectangle(
                new SolidColorBrush(color),
                new Rect(screenOrigin.X, screenOrigin.Y, cellSize * zoom, cellSize * zoom));
        }
    }

    public static Color ResolveContourHue(FieldLedgerEngine fieldEngine)
    {
        double totalEnergy = 0.0;
        double red = 0.0;
        double green = 0.0;
        double blue = 0.0;

        foreach (var (col, row) in fieldEngine.PerimeterSubscriber.PerimeterCells)
        {
            CellLedgerEntry entry = fieldEngine.GetCellLedger(col, row);
            double energy = Math.Max(0.0, entry.SameLayerEnergy - CellLedgerEntry.BaselineEnergy);
            if (energy <= 0.0 || entry.SameLayerColor.A == 0)
            {
                continue;
            }

            totalEnergy += energy;
            red += entry.SameLayerColor.R * energy;
            green += entry.SameLayerColor.G * energy;
            blue += entry.SameLayerColor.B * energy;
        }

        if (totalEnergy <= 0.0)
        {
            return Colors.SurfaceGrid;
        }

        // Preserve the same-layer additive hue rule at the contour aggregate.
        // A perimeter stroke must not calculate a midpoint color.
        return Color.FromArgb(
            byte.MaxValue,
            ToByte(red),
            ToByte(green),
            ToByte(blue));
    }

    private static byte ToByte(double value) =>
        (byte)Math.Clamp(Math.Round(value), 0, byte.MaxValue);
}
