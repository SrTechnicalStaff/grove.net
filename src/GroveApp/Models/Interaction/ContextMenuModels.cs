using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GroveApp.Models.Interaction;

public enum ContextMenuTargetType : byte
{
    EmptyCell = 0,
    SinglePlacement = 1,
    MultiSelection = 2
}

public sealed record ContextMenuTargetContext
{
    public ContextMenuTargetType TargetType { get; }
    public CellCoordinate AddressedCell { get; }
    public int SelectedGridLayerId { get; }
    public ImmutableArray<string> TargetPlacementIds { get; }

    public ContextMenuTargetContext(
        ContextMenuTargetType targetType,
        CellCoordinate addressedCell,
        int selectedGridLayerId,
        IEnumerable<string> targetPlacementIds)
    {
        ArgumentNullException.ThrowIfNull(targetPlacementIds);
        TargetType = targetType;
        AddressedCell = addressedCell;
        SelectedGridLayerId = selectedGridLayerId;
        TargetPlacementIds = targetPlacementIds.Where(id => !string.IsNullOrWhiteSpace(id)).ToImmutableArray();
    }
}

public sealed record ContextMenuCommand(
    string Id,
    string Header,
    string? InputGestureText,
    bool IsEnabled,
    bool IsSeparator = false);

public sealed record SpatialContextMenuModel(
    ContextMenuTargetContext TargetContext,
    ScreenPoint Position,
    ScreenRectangle Bounds,
    ImmutableArray<ContextMenuCommand> Commands);

public sealed record ContextMenuCommandInvocation(
    ContextMenuTargetContext TargetContext,
    ContextMenuCommand Command);
