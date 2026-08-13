using System;
using System.Collections.Generic;
using GroveApp.Models.Memory;

namespace GroveApp.Engine.Memory;

public sealed class MemorySlateCatalogue
{
    private readonly IMemoryLedger _ledger;

    public MemorySlateCatalogue(IMemoryLedger ledger)
    {
        _ledger = ledger ?? throw new ArgumentNullException(nameof(ledger));
    }

    public IReadOnlyList<MemoryRecord> Snapshot() => _ledger.GetAllMemories().ToArray();
}
