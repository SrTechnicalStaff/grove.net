using System.Collections.Generic;
using GroveApp.Models;

namespace GroveApp.Engine;

/// <summary>
/// Pure field-ledger preparation module. Rendering adapters consume its cell
/// selection and the engine's topology data without bringing UI types into the
/// field implementation.
/// </summary>
public sealed class FieldLedgerModule
{
    public IReadOnlySet<(int col, int row)> PrepareAuraCells(
        IEnumerable<GridContentItem> items,
        int minCellX,
        int maxCellX,
        int minCellY,
        int maxCellY) =>
        FieldLedgerEngine.GetAuraCells(items, minCellX, maxCellX, minCellY, maxCellY);
}
