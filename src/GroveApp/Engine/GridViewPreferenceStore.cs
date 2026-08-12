using System;
using System.IO;
using System.Text.Json;

namespace GroveApp.Engine;

/// <summary>
/// Per-person preference seam for canvas presentation choices. The store is
/// deliberately independent of spatial documents, so grid visibility follows
/// the person rather than the place being viewed.
/// </summary>
public sealed class GridViewPreferenceStore
{
    private readonly string _path;

    public GridViewPreferenceStore(string? path = null)
    {
        _path = path ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Grove", "preferences.json");
    }

    public event Action<Exception>? PersistenceFailed;

    public bool LoadLinesVisible()
    {
        try
        {
            if (!File.Exists(_path)) return true;
            var state = JsonSerializer.Deserialize<PreferenceState>(File.ReadAllText(_path));
            return state?.LinesVisible ?? true;
        }
        catch (Exception exception) when (exception is IOException or JsonException or UnauthorizedAccessException)
        {
            PersistenceFailed?.Invoke(exception);
            return true;
        }
    }

    public bool SaveLinesVisible(bool value)
    {
        try
        {
            string? directory = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            File.WriteAllText(_path, JsonSerializer.Serialize(new PreferenceState(value)));
            return true;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            PersistenceFailed?.Invoke(exception);
            return false;
        }
    }

    private sealed record PreferenceState(bool LinesVisible);
}
