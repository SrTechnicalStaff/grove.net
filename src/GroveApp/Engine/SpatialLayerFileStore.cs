using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace GroveApp.Engine;

public interface ISpatialLayerFileStore
{
    Task SaveAsync(string path, SpatialLayerStackState state);

    Task<SpatialLayerStackState?> LoadAsync(string path);
}

public sealed class SpatialLayerFileStore : ISpatialLayerFileStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task SaveAsync(string path, SpatialLayerStackState state)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(state);
        string? directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using FileStream stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, state, SerializerOptions).ConfigureAwait(false);
    }

    public async Task<SpatialLayerStackState?> LoadAsync(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!File.Exists(path))
        {
            return null;
        }

        await using FileStream stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<SpatialLayerStackState>(stream, SerializerOptions).ConfigureAwait(false);
    }
}
