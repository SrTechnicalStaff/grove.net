using System;
using System.Collections.Generic;
using GroveApp.DesignSystem;

namespace GroveApp.Engine
{
    public record struct SpatialLayerModel(
        int ZIndex,
        string Label, // "01", "02", "B01", "B02"
        string DisplayName,
        bool IsSelected,
        int ContentItemCount,
        bool IsVisible = true,
        bool IsLocked = false,
        bool IsProtected = false,
        string ColorHex = Colors.LayerFillHex
    );

    public interface ISpatialLayerStateService : ISpatialLayerPermeability
    {
        IReadOnlyList<SpatialLayerModel> Layers { get; }
        SpatialLayerModel SelectedGridLayer { get; }

        void SelectGridLayer(int zIndex);
        SpatialLayerModel InsertLayerAbove(int currentZIndex);
        SpatialLayerModel InsertLayerBelow(int currentZIndex);
        SpatialLayerModel InsertLayerAtTop();
        SpatialLayerModel InsertLayerAtBottom();
        void ReorderSwap(int sourceZIndex, int targetZIndex);
        bool RenameLayer(int zIndex, string newName, out string errorReason);
        bool RemoveLayer(int zIndex, int targetTransferZIndex);
        bool SetLayerVisibility(int zIndex, bool isVisible);
        bool SetLayerLocked(int zIndex, bool isLocked);
        void JumpToTop();
        void JumpToBottom();

        double CalculateAuraPermeability(int sourceZIndex, int targetZIndex);
        string FormatLayerLabel(int zIndex); // 01, 02, B01, B02

        event Action<SpatialLayerModel>? SelectedGridLayerChanged;
        event Action? LayerStackChanged;
    }
}
