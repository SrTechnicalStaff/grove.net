using GroveApp.Engine;
using GroveApp.Models;

namespace GroveApp.Tests;

public sealed class FieldLedgerSelectionTests
{
    [Fact]
    public void Selected_field_projection_returns_only_selected_source_contribution()
    {
        var engine = new FieldLedgerEngine
        {
            ProjectionGridLayerId = 0
        };
        var note = new GridNote(0, 0, "one", NoteColor.Violet, layerId: 0)
        {
            Id = Guid.NewGuid().ToString("N")
        };
        var document = new GridDocument(0, 0, 2, 2, "two", "body", layerId: 0)
        {
            Id = Guid.NewGuid().ToString("N")
        };

        engine.RecalculateField(new GridContentItem[] { note, document }, new[] { (0, 0) });

        var selected = engine.QuerySelectedField(
            new HashSet<string>(StringComparer.Ordinal) { note.Id },
            0,
            0,
            0,
            0);

        var cell = Assert.Single(selected);
        Assert.True(cell.SelectedEnergy > 0);
        Assert.True(cell.SelectedEnergy < engine.GetCellLedger(0, 0).FieldEnergy);
    }

    [Fact]
    public void Perimeter_threshold_excludes_the_baseline_energy()
    {
        var engine = new FieldLedgerEngine
        {
            ProjectionGridLayerId = 0
        };
        var note = new GridNote(0, 0, "one", layerId: 0);

        engine.RecalculateField(new[] { note }, new[] { (4, 0), (3, 0) });

        Assert.DoesNotContain((4, 0), engine.PerimeterSubscriber.SaturatedCells);
        Assert.Contains((3, 0), engine.PerimeterSubscriber.SaturatedCells);
    }

    [Fact]
    public void Composite_hue_sums_all_sources_beyond_inline_provenance_slots()
    {
        var engine = new FieldLedgerEngine
        {
            ProjectionGridLayerId = 0
        };
        var sources = Enumerable.Range(0, 5)
            .Select(index => new LowHueContent(index.ToString()))
            .Cast<GridContentItem>()
            .ToArray();

        engine.RecalculateField(sources, new[] { (0, 0) });

        CellLedgerEntry entry = engine.GetCellLedger(0, 0);
        Assert.Equal(5, entry.SourceCount);
        Assert.Equal((byte)5, entry.CompositeColor.R);
        Assert.Equal((byte)10, entry.CompositeColor.G);
        Assert.Equal((byte)15, entry.CompositeColor.B);
    }

    private sealed class LowHueContent : GridContentItem
    {
        public LowHueContent(string id)
            : base(0, 0)
        {
            Id = id;
        }

        public override ContentKind Kind => ContentKind.Note;
        public override string FieldHueHex => "#010203";
    }
}
