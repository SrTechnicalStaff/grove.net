using System;
using System.IO;

namespace GroveApp.Engine.Memory;

public sealed class MemoryRepository
{
    public MemoryRepository(
        IMemoryLedger? ledger = null,
        IMemoryVersionTree? versions = null,
        IMemoryRecordStore? store = null,
        string? directory = null)
    {
        Ledger = ledger ?? new ImmutableMemoryLedger();
        Versions = versions ?? new MemoryVersionTree();
        Store = store ?? new MemoryRecordFileStore();
        if (directory is not null && string.IsNullOrWhiteSpace(directory))
        {
            throw new ArgumentException("The Memory directory cannot be empty.", nameof(directory));
        }

        Directory = directory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Grove",
            "memories");
    }

    public IMemoryLedger Ledger { get; }

    public IMemoryVersionTree Versions { get; }

    public IMemoryRecordStore Store { get; }

    public string Directory { get; }
}
