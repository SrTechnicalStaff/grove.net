using System;
using GroveApp.Models.Interaction;

namespace GroveApp.Engine.Interaction;

public sealed class CanvasPanInteraction
{
    private const double MovementThresholdPixels = 4.0;
    private PanGestureStart? _start;
    private bool _hasMoved;
    private bool _spacePanModifier;
    private bool _spacePanUsed;

    public bool IsActive => _start.HasValue;
    public bool IsSpacePanModifierActive => _spacePanModifier;

    public bool TryBegin(PanGestureStart start)
    {
        if (IsActive)
        {
            return false;
        }

        _start = start;
        _hasMoved = false;
        return true;
    }

    public PanGestureUpdate Update(ScreenPoint currentScreen)
    {
        if (!_start.HasValue)
        {
            return new PanGestureUpdate(currentScreen, false);
        }

        PanGestureStart start = _start.Value;
        double deltaX = currentScreen.X - start.ScreenPosition.X;
        double deltaY = currentScreen.Y - start.ScreenPosition.Y;
        if (Math.Abs(deltaX) > MovementThresholdPixels || Math.Abs(deltaY) > MovementThresholdPixels)
        {
            _hasMoved = true;
            if (_spacePanModifier && start.Initiator == PanInitiator.SpaceLeftButton)
            {
                _spacePanUsed = true;
            }
        }

        return new PanGestureUpdate(
            new ScreenPoint(start.CameraPosition.X + deltaX, start.CameraPosition.Y + deltaY),
            _hasMoved);
    }

    public PanGestureEnd End()
    {
        if (!_start.HasValue)
        {
            return default;
        }

        PanGestureStart start = _start.Value;
        bool staticRightClick = start.Initiator == PanInitiator.RightButton && !_hasMoved;
        PanGestureEnd result = new(
            staticRightClick && start.ToolWasArmed,
            staticRightClick && !start.ToolWasArmed);
        _start = null;
        _hasMoved = false;
        return result;
    }

    public void BeginSpacePan()
    {
        _spacePanModifier = true;
        _spacePanUsed = false;
    }

    public bool EndSpacePan()
    {
        if (!_spacePanModifier)
        {
            return true;
        }

        bool used = _spacePanUsed;
        _spacePanModifier = false;
        _spacePanUsed = false;
        return used;
    }
}
