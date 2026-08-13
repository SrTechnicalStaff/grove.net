using System;
using System.Collections.Generic;
using System.Linq;
using GroveApp.DesignSystem;

namespace GroveApp.Engine
{
    public readonly record struct LayerId(int Value)
    {
        public override string ToString() => Value.ToString();
    }

    public readonly record struct LayerLabel(string Value)
    {
        public bool IsValid => !string.IsNullOrWhiteSpace(Value);
        public override string ToString() => Value;
    }

    public readonly record struct LayerMigrationResult(
        bool Succeeded,
        int SourceLayerId,
        int TargetLayerId,
        int MigratedItemCount,
        string ErrorReason);

    public sealed record SpatialLayerState(
        int Id,
        string StableLabel,
        string Name,
        bool IsVisible,
        bool IsLocked,
        string ColorHex);

    public sealed record SpatialLayerStackState(
        IReadOnlyList<SpatialLayerState> Layers,
        int ActiveLayerId);

    public sealed record SpatialLayer(
        int Id,
        string StableLabel,
        string Name,
        bool IsVisible = true,
        bool IsLocked = false)
    {
        public string ColorHex { get; init; } = Colors.LayerFillHex;
        public LayerId TypedId => new(Id);
        public LayerLabel TypedLabel => new(StableLabel);
    }

    /// <summary>
    /// Small field-physics seam. FieldLedgerEngine can consume this later without knowing
    /// how layers are labelled, ordered, or navigated.
    /// </summary>
    public interface ISpatialLayerPermeability
    {
        int ActiveLayerId { get; }
        double GetPermeability(int sourceLayerId, int targetLayerId);
        double ApplyPermeability(double sourceEnergy, int sourceLayerId, int targetLayerId);
    }

    /// <summary>
    /// Owns the ordered layer continuum, B-label stack rules (01, 02, B01, B02...),
    /// vertical aura permeability physics, and layer navigation state.
    /// Implements ISpatialLayerStateService and ISpatialLayerPermeability.
    /// </summary>
    public sealed class SpatialLayerStack : ISpatialLayerStateService
    {
        private sealed class InternalLayer
        {
            public int Id { get; }
            public string StableLabel { get; }
            public string Name { get; set; }
            public bool IsVisible { get; set; } = true;
            public bool IsLocked { get; set; } = false;
            public string ColorHex { get; set; }

            public InternalLayer(int id, string stableLabel, string name, string colorHex)
            {
                Id = id;
                StableLabel = stableLabel;
                Name = name;
                ColorHex = colorHex;
            }
        }

        private readonly List<InternalLayer> _layers = new();
        private readonly InternalLayer _groundLayer;
        private int _nextId = 1;
        private int _nextUpperLabel = 2;
        private int _nextLowerLabel = 1;
        private int _activeLayerId;

        public Func<int, int>? ItemCountProvider { get; set; }
        public Func<int, int, bool>? LayerMigrationValidator { get; set; }

        public event Action<SpatialLayerModel>? ActiveLayerChanged;
        public event Action? LayerStackChanged;
        public event Action<int, int>? LayerItemsTransferRequested;

        // Legacy event support
        public event Action<SpatialLayer>? ActiveSpatialLayerChanged;

        public SpatialLayerStack()
        {
            _groundLayer = new InternalLayer(0, "01", "Main Ground Layer", Colors.LayerFillHex);
            _layers.Add(_groundLayer);
            _activeLayerId = _groundLayer.Id;
        }

        public int ActiveLayerId => _activeLayerId;

        public SpatialLayer ActiveLayer => GetLayer(_activeLayerId);

        SpatialLayerModel ISpatialLayerStateService.ActiveLayer => GetModelForId(_activeLayerId);

        public IReadOnlyList<SpatialLayerModel> Layers => BuildModels();

        public SpatialLayerStackState ExportState() => new(
            _layers.Select(layer => new SpatialLayerState(
                layer.Id,
                layer.StableLabel,
                layer.Name,
                layer.IsVisible,
                layer.IsLocked,
                layer.ColorHex)).ToArray(),
            _activeLayerId);

        public bool ImportState(SpatialLayerStackState state)
        {
            ArgumentNullException.ThrowIfNull(state);
            SpatialLayerState? ground = state.Layers.FirstOrDefault(layer => layer.Id == _groundLayer.Id);
            if (ground is null || state.Layers.Count == 0 || state.Layers.Select(layer => layer.Id).Distinct().Count() != state.Layers.Count)
            {
                return false;
            }

            _layers.Clear();
            _groundLayer.Name = string.IsNullOrWhiteSpace(ground.Name) ? "Main Ground Layer" : ground.Name;
            _groundLayer.IsVisible = ground.IsVisible;
            _groundLayer.IsLocked = false;
            _groundLayer.ColorHex = string.IsNullOrWhiteSpace(ground.ColorHex) ? Colors.LayerFillHex : ground.ColorHex;

            foreach (SpatialLayerState saved in state.Layers)
            {
                if (saved.Id == _groundLayer.Id)
                {
                    _layers.Add(_groundLayer);
                    continue;
                }

                _layers.Add(new InternalLayer(
                    saved.Id,
                    string.IsNullOrWhiteSpace(saved.StableLabel) ? FormatLayerLabel(saved.Id) : saved.StableLabel,
                    string.IsNullOrWhiteSpace(saved.Name) ? "New Layer" : saved.Name,
                    string.IsNullOrWhiteSpace(saved.ColorHex) ? LayerColor(saved.Id) : saved.ColorHex)
                {
                    IsVisible = saved.IsVisible,
                    IsLocked = saved.IsLocked
                });
            }

            _nextId = Math.Max(1, _layers.Max(layer => layer.Id) + 1);
            _nextUpperLabel = _layers
                .Where(layer => int.TryParse(layer.StableLabel, out _))
                .Select(layer => int.Parse(layer.StableLabel))
                .DefaultIfEmpty(1)
                .Max() + 1;
            _nextLowerLabel = _layers
                .Where(layer => layer.StableLabel.StartsWith("B", StringComparison.OrdinalIgnoreCase) &&
                                int.TryParse(layer.StableLabel[1..], out _))
                .Select(layer => int.Parse(layer.StableLabel[1..]))
                .DefaultIfEmpty(0)
                .Max() + 1;
            _activeLayerId = GetStackIndex(state.ActiveLayerId) >= 0 ? state.ActiveLayerId : _groundLayer.Id;
            LayerStackChanged?.Invoke();
            ActiveLayerChanged?.Invoke(GetModelForId(_activeLayerId));
            return true;
        }

        public string FormatLayerLabel(int zIndex)
        {
            if (zIndex >= 1)
            {
                return zIndex.ToString("D2");
            }
            else
            {
                int belowNum = 1 - zIndex;
                return $"B{belowNum:D2}";
            }
        }

        public double CalculateAuraPermeability(int sourceZIndex, int targetZIndex)
        {
            return Math.Pow(0.5, Math.Abs(sourceZIndex - targetZIndex));
        }

        public double GetPermeability(int sourceLayerId, int targetLayerId)
        {
            int sourceZ = GetZIndexForLayerId(sourceLayerId);
            int targetZ = GetZIndexForLayerId(targetLayerId);
            return CalculateAuraPermeability(sourceZ, targetZ);
        }

        public double ApplyPermeability(double sourceEnergy, int sourceLayerId, int targetLayerId)
        {
            return sourceEnergy * GetPermeability(sourceLayerId, targetLayerId);
        }

        public SpatialLayer GetLayer(int id)
        {
            var internalLayer = GetInternalLayer(id);
            int zIndex = GetZIndexForLayerId(id);
            return new SpatialLayer(internalLayer.Id, internalLayer.StableLabel, internalLayer.Name, internalLayer.IsVisible, internalLayer.IsLocked)
            {
                ColorHex = internalLayer.ColorHex
            };
        }

        public int GetStackIndex(int layerId)
        {
            for (int i = 0; i < _layers.Count; i++)
            {
                if (_layers[i].Id == layerId)
                {
                    return i;
                }
            }
            return -1;
        }

        public int GetZIndexForLayerId(int layerId)
        {
            int groundIdx = GetGroundIndex();
            int stackIdx = GetStackIndex(layerId);
            if (stackIdx < 0) return 1;
            return groundIdx - stackIdx + 1;
        }

        public int GetLayerIdForZIndex(int zIndex)
        {
            int groundIdx = GetGroundIndex();
            int stackIdx = groundIdx - zIndex + 1;
            if (stackIdx >= 0 && stackIdx < _layers.Count)
            {
                return _layers[stackIdx].Id;
            }
            return _groundLayer.Id;
        }

        public void SetActiveLayer(int zIndex)
        {
            int layerId = GetLayerIdForZIndex(zIndex);
            if (layerId == _activeLayerId) return;

            _activeLayerId = layerId;
            var model = GetModelForId(_activeLayerId);
            ActiveLayerChanged?.Invoke(model);
            ActiveSpatialLayerChanged?.Invoke(GetLayer(_activeLayerId));
            LayerStackChanged?.Invoke();
        }

        public bool SetActiveLayerId(int layerId)
        {
            if (GetStackIndex(layerId) < 0 || layerId == _activeLayerId)
            {
                return false;
            }
            _activeLayerId = layerId;
            var model = GetModelForId(_activeLayerId);
            ActiveLayerChanged?.Invoke(model);
            ActiveSpatialLayerChanged?.Invoke(GetLayer(_activeLayerId));
            LayerStackChanged?.Invoke();
            return true;
        }

        public SpatialLayerModel InsertLayerAbove(int currentZIndex)
        {
            int currentLayerId = GetLayerIdForZIndex(currentZIndex);
            int stackIdx = GetStackIndex(currentLayerId);
            if (stackIdx < 0) stackIdx = 0;

            var newLayer = new InternalLayer(_nextId++, _nextUpperLabel++.ToString("D2"), "New Layer", LayerColor(_nextId));
            // Insert above means inserting before stackIdx (towards top)
            _layers.Insert(stackIdx, newLayer);

            _activeLayerId = newLayer.Id;
            LayerStackChanged?.Invoke();
            var model = GetModelForId(newLayer.Id);
            ActiveLayerChanged?.Invoke(model);
            return model;
        }

        public SpatialLayerModel InsertLayerBelow(int currentZIndex)
        {
            int currentLayerId = GetLayerIdForZIndex(currentZIndex);
            int stackIdx = GetStackIndex(currentLayerId);
            if (stackIdx < 0) stackIdx = _layers.Count - 1;

            var newLayer = new InternalLayer(_nextId++, $"B{_nextLowerLabel++:D2}", "New Layer", LayerColor(_nextId));
            // Insert below means inserting after stackIdx (towards bottom)
            _layers.Insert(stackIdx + 1, newLayer);

            _activeLayerId = newLayer.Id;
            LayerStackChanged?.Invoke();
            var model = GetModelForId(newLayer.Id);
            ActiveLayerChanged?.Invoke(model);
            return model;
        }

        public SpatialLayerModel InsertLayerAtTop() =>
            InsertLayerAbove(GetZIndexForLayerId(_layers[0].Id));

        public SpatialLayerModel InsertLayerAtBottom() =>
            InsertLayerBelow(GetZIndexForLayerId(_layers[^1].Id));

        public void ReorderSwap(int sourceZIndex, int targetZIndex)
        {
            int sourceId = GetLayerIdForZIndex(sourceZIndex);
            int targetId = GetLayerIdForZIndex(targetZIndex);

            int sourceIdx = GetStackIndex(sourceId);
            int targetIdx = GetStackIndex(targetId);

            if (sourceIdx < 0 || targetIdx < 0 || sourceIdx == targetIdx) return;

            (_layers[sourceIdx], _layers[targetIdx]) = (_layers[targetIdx], _layers[sourceIdx]);
            LayerStackChanged?.Invoke();
        }

        public bool RenameLayer(int zIndex, string newName, out string errorReason)
        {
            if (string.IsNullOrWhiteSpace(newName))
            {
                errorReason = "Layer name cannot be empty.";
                return false;
            }

            string candidateName = newName.Trim();
            int layerId = GetLayerIdForZIndex(zIndex);
            var layer = GetInternalLayer(layerId);
            if (_layers.Any(other =>
                other.Id != layerId &&
                string.Equals(other.Name, candidateName, StringComparison.OrdinalIgnoreCase)))
            {
                errorReason = "Layer names must be unique.";
                return false;
            }

            layer.Name = candidateName;
            errorReason = string.Empty;
            LayerStackChanged?.Invoke();
            return true;
        }

        public bool RemoveLayer(int zIndex, int targetTransferZIndex)
        {
            if (_layers.Count <= 1) return false;

            int removeLayerId = GetLayerIdForZIndex(zIndex);
            if (removeLayerId == _groundLayer.Id)
            {
                return false;
            }
            int removeIdx = GetStackIndex(removeLayerId);
            if (removeIdx < 0) return false;

            int targetTransferLayerId = GetLayerIdForZIndex(targetTransferZIndex);
            if (LayerMigrationValidator is not null &&
                !LayerMigrationValidator(removeLayerId, targetTransferLayerId))
            {
                return false;
            }

            LayerItemsTransferRequested?.Invoke(removeLayerId, targetTransferLayerId);

            _layers.RemoveAt(removeIdx);

            if (_activeLayerId == removeLayerId)
            {
                int newActiveIdx = Math.Clamp(removeIdx, 0, _layers.Count - 1);
                _activeLayerId = _layers[newActiveIdx].Id;
                ActiveLayerChanged?.Invoke(GetModelForId(_activeLayerId));
            }

            LayerStackChanged?.Invoke();
            return true;
        }

        public bool SetLayerVisibility(int zIndex, bool isVisible)
        {
            int layerId = GetLayerIdForZIndex(zIndex);
            var layer = GetInternalLayer(layerId);
            if (layer.IsVisible == isVisible) return false;
            layer.IsVisible = isVisible;
            LayerStackChanged?.Invoke();
            return true;
        }

        public bool SetLayerLocked(int zIndex, bool isLocked)
        {
            int layerId = GetLayerIdForZIndex(zIndex);
            if (layerId == _groundLayer.Id)
            {
                return false;
            }

            var layer = GetInternalLayer(layerId);
            if (layer.IsLocked == isLocked) return false;
            layer.IsLocked = isLocked;
            LayerStackChanged?.Invoke();
            return true;
        }

        public bool Navigate(int direction)
        {
            if (direction == 0 || _layers.Count == 0) return false;

            int currentZ = GetZIndexForLayerId(_activeLayerId);
            // direction > 0 means navigate UP (+1 ZIndex)
            // direction < 0 means navigate DOWN (-1 ZIndex)
            int targetZ = currentZ + direction;
            int groundIdx = GetGroundIndex();
            int targetStackIdx = groundIdx - targetZ + 1;

            if (targetStackIdx < 0 || targetStackIdx >= _layers.Count) return false;

            SetActiveLayer(targetZ);
            return true;
        }

        public void JumpToTop()
        {
            if (_layers.Count == 0) return;
            int topId = _layers[0].Id;
            int topZ = GetZIndexForLayerId(topId);
            SetActiveLayer(topZ);
        }

        public void JumpToBottom()
        {
            if (_layers.Count == 0) return;
            int bottomId = _layers[^1].Id;
            int bottomZ = GetZIndexForLayerId(bottomId);
            SetActiveLayer(bottomZ);
        }

        private int GetGroundIndex()
        {
            for (int i = 0; i < _layers.Count; i++)
            {
                if (_layers[i].Id == _groundLayer.Id)
                {
                    return i;
                }
            }
            return 0;
        }

        private InternalLayer GetInternalLayer(int id)
        {
            foreach (var l in _layers)
            {
                if (l.Id == id) return l;
            }
            throw new KeyNotFoundException($"Layer {id} not found.");
        }

        private SpatialLayerModel GetModelForId(int id)
        {
            var l = GetInternalLayer(id);
            int z = GetZIndexForLayerId(id);
            int count = ItemCountProvider?.Invoke(id) ?? 0;
            return new SpatialLayerModel(z, l.StableLabel, l.Name, id == _activeLayerId, count, l.IsVisible, l.IsLocked, id == _groundLayer.Id, l.ColorHex);
        }

        private IReadOnlyList<SpatialLayerModel> BuildModels()
        {
            var result = new List<SpatialLayerModel>(_layers.Count);
            for (int i = 0; i < _layers.Count; i++)
            {
                var l = _layers[i];
                int z = GetZIndexForLayerId(l.Id);
                int count = ItemCountProvider?.Invoke(l.Id) ?? 0;
                result.Add(new SpatialLayerModel(z, l.StableLabel, l.Name, l.Id == _activeLayerId, count, l.IsVisible, l.IsLocked, l.Id == _groundLayer.Id, l.ColorHex));
            }
            return result;
        }

        private static string LayerColor(int id) => (id % 4) switch
        {
            0 => Colors.LayerFillHex,
            1 => Colors.ToolFillHex,
            2 => Colors.ViewFillHex,
            _ => Colors.EditFillHex
        };
    }
}
