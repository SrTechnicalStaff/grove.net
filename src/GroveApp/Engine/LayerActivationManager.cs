using System;

namespace GroveApp.Engine;

public enum LayerRenderMode
{
    ActiveFull,
    InactivePresenceOnly,
    IsolatedSolo,
    Hidden
}

/// <summary>
/// Owns active-layer isolation independently from layer storage. Solo mode
/// suppresses inactive content, aura, and ghost rendering.
/// </summary>
public interface ILayerActivationManager
{
    int ActiveLayerId { get; }
    bool IsIsolationModeEnabled { get; }
    event Action? StateChanged;
    void ToggleIsolationMode();
    LayerRenderMode GetRenderMode(int layerId);
}

public sealed class LayerActivationManager : ILayerActivationManager
{
    private readonly SpatialLayerStack _layerStack;

    public LayerActivationManager(SpatialLayerStack layerStack)
    {
        _layerStack = layerStack ?? throw new ArgumentNullException(nameof(layerStack));
        _layerStack.ActiveLayerChanged += _ => StateChanged?.Invoke();
        _layerStack.LayerStackChanged += () => StateChanged?.Invoke();
    }

    public int ActiveLayerId => _layerStack.ActiveLayerId;
    public bool IsIsolationModeEnabled { get; private set; }
    public event Action? StateChanged;

    public void ToggleIsolationMode()
    {
        IsIsolationModeEnabled = !IsIsolationModeEnabled;
        StateChanged?.Invoke();
    }

    public LayerRenderMode GetRenderMode(int layerId)
    {
        SpatialLayer layer = _layerStack.GetLayer(layerId);
        if (!layer.IsVisible)
        {
            return LayerRenderMode.Hidden;
        }

        if (layerId == ActiveLayerId)
        {
            return IsIsolationModeEnabled ? LayerRenderMode.IsolatedSolo : LayerRenderMode.ActiveFull;
        }

        return IsIsolationModeEnabled ? LayerRenderMode.Hidden : LayerRenderMode.InactivePresenceOnly;
    }
}
