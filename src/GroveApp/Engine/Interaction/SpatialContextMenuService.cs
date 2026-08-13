using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GroveApp.Models.Interaction;
using InteractionCellCoordinate = GroveApp.Models.Interaction.CellCoordinate;

namespace GroveApp.Engine.Interaction;

public interface ISpatialContextMenuService
{
    SpatialContextMenuModel? ActiveMenu { get; }
    event Action<SpatialContextMenuModel?>? ContextMenuStateChanged;
    event Action<ContextMenuCommandInvocation>? CommandRequested;

    void OpenContextMenuAt(
        ScreenPoint clickPoint,
        InteractionCellCoordinate targetCell,
        int selectedGridLayerId,
        IEnumerable<string> targetPlacementIds,
        ScreenSize viewportSize,
        ScreenSize? menuSize = null);

    void CloseContextMenu();
    bool ProcessPointerPressed(ScreenPoint screenPoint);
    bool TryExecute(string commandId);
}

/// <summary>
/// Pure context-menu state module. It resolves the target class, creates command
/// models, clamps the menu rectangle, and reports commands without owning a view.
/// </summary>
public sealed class SpatialContextMenuService : ISpatialContextMenuService
{
    public const double DefaultMenuWidth = 220;
    public const double DefaultCommandHeight = 32;

    public SpatialContextMenuModel? ActiveMenu { get; private set; }

    public event Action<SpatialContextMenuModel?>? ContextMenuStateChanged;
    public event Action<ContextMenuCommandInvocation>? CommandRequested;

    public void OpenContextMenuAt(
        ScreenPoint clickPoint,
        InteractionCellCoordinate targetCell,
        int selectedGridLayerId,
        IEnumerable<string> targetPlacementIds,
        ScreenSize viewportSize,
        ScreenSize? menuSize = null)
    {
        ArgumentNullException.ThrowIfNull(targetPlacementIds);
        if (!viewportSize.IsValid || viewportSize.Width <= 0 || viewportSize.Height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(viewportSize));
        }

        ImmutableArray<string> ids = targetPlacementIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.Ordinal)
            .ToImmutableArray();
        ContextMenuTargetType targetType = ids.Length switch
        {
            0 => ContextMenuTargetType.EmptyCell,
            1 => ContextMenuTargetType.SinglePlacement,
            _ => ContextMenuTargetType.MultiSelection
        };

        var target = new ContextMenuTargetContext(targetType, targetCell, selectedGridLayerId, ids);
        ImmutableArray<ContextMenuCommand> commands = CreateCommands(target);
        ScreenSize requestedSize = menuSize.GetValueOrDefault();
        double width = requestedSize.Width > 0 ? requestedSize.Width : DefaultMenuWidth;
        double height = requestedSize.Height > 0 ? requestedSize.Height : commands.Length * DefaultCommandHeight;
        ScreenPoint position = ClampPosition(clickPoint, new ScreenSize(width, height), viewportSize);

        ActiveMenu = new SpatialContextMenuModel(
            target,
            position,
            new ScreenRectangle(position.X, position.Y, width, height),
            commands);
        ContextMenuStateChanged?.Invoke(ActiveMenu);
    }

    public void CloseContextMenu()
    {
        if (ActiveMenu is null)
        {
            return;
        }

        ActiveMenu = null;
        ContextMenuStateChanged?.Invoke(null);
    }

    public bool ProcessPointerPressed(ScreenPoint screenPoint)
    {
        if (ActiveMenu is null)
        {
            return false;
        }

        if (ActiveMenu.Bounds.Contains(screenPoint))
        {
            return true;
        }

        CloseContextMenu();
        return false;
    }

    public bool TryExecute(string commandId)
    {
        if (ActiveMenu is null || string.IsNullOrWhiteSpace(commandId))
        {
            return false;
        }

        ContextMenuCommand? command = ActiveMenu.Commands.FirstOrDefault(
            item => string.Equals(item.Id, commandId, StringComparison.Ordinal));
        if (command is null || command.IsSeparator || !command.IsEnabled)
        {
            return false;
        }

        ContextMenuCommandInvocation invocation = new(ActiveMenu.TargetContext, command);
        CommandRequested?.Invoke(invocation);
        CloseContextMenu();
        return true;
    }

    private static ScreenPoint ClampPosition(ScreenPoint clickPoint, ScreenSize menuSize, ScreenSize viewportSize)
    {
        double x = clickPoint.X + menuSize.Width <= viewportSize.Width
            ? clickPoint.X
            : clickPoint.X - menuSize.Width >= 0
                ? clickPoint.X - menuSize.Width
                : Math.Max(0, viewportSize.Width - menuSize.Width);
        double y = clickPoint.Y + menuSize.Height <= viewportSize.Height
            ? clickPoint.Y
            : clickPoint.Y - menuSize.Height >= 0
                ? clickPoint.Y - menuSize.Height
                : Math.Max(0, viewportSize.Height - menuSize.Height);

        return new ScreenPoint(
            Math.Clamp(x, 0, Math.Max(0, viewportSize.Width - menuSize.Width)),
            Math.Clamp(y, 0, Math.Max(0, viewportSize.Height - menuSize.Height)));
    }

    private static ImmutableArray<ContextMenuCommand> CreateCommands(ContextMenuTargetContext target) => target.TargetType switch
    {
        ContextMenuTargetType.EmptyCell =>
        [
            new("create-note", "Create Note", "N", true),
            new("create-document", "Create Document", "D", true),
            new("paste", "Paste", "Ctrl+V", true),
            new("anchor", "Anchor", "A", false),
            new("delete", "Delete", "Del", false),
            new("grid-properties", "Grid Properties", null, true)
        ],
        ContextMenuTargetType.SinglePlacement =>
        [
            new("open", "Open", "Space", true),
            new("anchor", "Anchor", "A", true),
            new("trace-layer", "Trace to Layer", "1", true),
            new("copy", "Copy", "Ctrl+C", true),
            new("cut", "Cut", "Ctrl+X", true),
            new("delete", "Delete", "Del", true)
        ],
        ContextMenuTargetType.MultiSelection =>
        [
            new("group-anchor", "Group Anchor", "A", true),
            new("copy-all", "Copy All", "Ctrl+C", true),
            new("delete-all", "Delete All", "Del", true)
        ],
        _ => []
    };
}
