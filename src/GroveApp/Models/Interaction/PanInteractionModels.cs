namespace GroveApp.Models.Interaction;

public enum PanInitiator : byte
{
    MiddleButton = 0,
    RightButton = 1,
    SpaceLeftButton = 2
}

public readonly record struct PanGestureStart(
    ScreenPoint ScreenPosition,
    ScreenPoint CameraPosition,
    PanInitiator Initiator,
    bool ToolWasArmed);

public readonly record struct PanGestureUpdate(
    ScreenPoint CameraPosition,
    bool HasMoved);

public readonly record struct PanGestureEnd(
    bool ShouldDisarmTool,
    bool ShouldOpenContextMenu);
