using System;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace GroveApp.Engine
{
    public enum FocusPrecedenceLevel : byte
    {
        FocusedTextBox = 0,
        InformationOverlay = 1,
        HudPlane = 2,
        Plane0Canvas = 3
    }

    public readonly record struct KeyCombination(Key Key, KeyModifiers Modifiers)
    {
        public static KeyCombination From(KeyEventArgs args) => new(args.Key, args.KeyModifiers);
        public static KeyCombination FromEventArgs(KeyEventArgs args) => From(args);
    }

    public sealed record FocusContextInfo(
        FocusPrecedenceLevel ActiveLevel,
        IInputElement? FocusedElement,
        bool IsTextEditingActive = false,
        Guid? SelectedGridLayerId = null);

    public enum KeybindHandlingResult : byte
    {
        Ignored = 0,
        ConsumedByFocusedControl = 1,
        DispatchedToSpatialGrid = 2,
        DispatchedToOverlay = 3,
        DispatchedToHud = 4
    }

    public interface IFocusPrecedenceRouter : IDisposable
    {
        FocusContextInfo CurrentContext { get; }
        event Action<FocusContextInfo>? FocusContextChanged;
        event Action<KeyCombination, KeybindHandlingResult>? KeybindRouted;
        bool RouteKeyEvent(KeyEventArgs args, RoutingStrategies strategy);
        void ForcePlane0CanvasFocus();
    }

    /// <summary>
    /// Window-root tunnel adapter for ADR-058. Text focus is classified first, then
    /// only the spatial combinations that the central keybind module can consume are
    /// dispatched to Plane 0.
    /// </summary>
    public sealed class GlobalFocusPrecedenceRouter : IFocusPrecedenceRouter
    {
        private readonly TopLevel _rootWindow;
        private readonly Func<bool> _isInformationOverlayVisible;
        private readonly Func<bool> _isPlane0Context;
        private readonly Func<KeyCombination, bool> _routeSpatialKey;
        private FocusContextInfo _currentContext = new(FocusPrecedenceLevel.HudPlane, null);

        public FocusContextInfo CurrentContext => _currentContext;

        public event Action<FocusContextInfo>? FocusContextChanged;
        public event Action<KeyCombination, KeybindHandlingResult>? KeybindRouted;

        public GlobalFocusPrecedenceRouter(
            TopLevel rootWindow,
            Func<bool> isInformationOverlayVisible,
            Func<bool> isPlane0Context,
            Func<KeyCombination, bool> routeSpatialKey)
        {
            _rootWindow = rootWindow ?? throw new ArgumentNullException(nameof(rootWindow));
            _isInformationOverlayVisible = isInformationOverlayVisible ?? throw new ArgumentNullException(nameof(isInformationOverlayVisible));
            _isPlane0Context = isPlane0Context ?? throw new ArgumentNullException(nameof(isPlane0Context));
            _routeSpatialKey = routeSpatialKey ?? throw new ArgumentNullException(nameof(routeSpatialKey));

            _rootWindow.AddHandler(InputElement.KeyDownEvent, OnWindowKeyDownTunnel, RoutingStrategies.Tunnel);
        }

        public bool RouteKeyEvent(KeyEventArgs args, RoutingStrategies strategy)
        {
            if ((strategy & RoutingStrategies.Tunnel) == 0)
            {
                return false;
            }

            var context = EvaluateFocusContext(args.Source as IInputElement);
            if (context != _currentContext)
            {
                _currentContext = context;
                FocusContextChanged?.Invoke(context);
            }

            var combination = KeyCombination.From(args);
            if (context.ActiveLevel != FocusPrecedenceLevel.Plane0Canvas)
            {
                return false;
            }

            if (IsGlobalSpatialCombination(combination))
            {
                bool routed = _routeSpatialKey(combination);
                if (routed)
                {
                    KeybindRouted?.Invoke(combination, KeybindHandlingResult.DispatchedToSpatialGrid);
                }

                return routed;
            }

            return false;
        }

        public void ForcePlane0CanvasFocus()
        {
            if (_rootWindow is IInputElement inputElement)
            {
                inputElement.Focus();
            }
        }

        public void Dispose()
        {
            _rootWindow.RemoveHandler(InputElement.KeyDownEvent, OnWindowKeyDownTunnel);
        }

        private void OnWindowKeyDownTunnel(object? sender, KeyEventArgs args)
        {
            if (RouteKeyEvent(args, RoutingStrategies.Tunnel))
            {
                args.Handled = true;
            }
        }

        private FocusContextInfo EvaluateFocusContext(IInputElement? focusedElement)
        {
            if (focusedElement is TextBox)
            {
                return new FocusContextInfo(FocusPrecedenceLevel.FocusedTextBox, focusedElement, true);
            }

            if (_isInformationOverlayVisible())
            {
                return new FocusContextInfo(FocusPrecedenceLevel.InformationOverlay, focusedElement);
            }

            if (_isPlane0Context())
            {
                return new FocusContextInfo(FocusPrecedenceLevel.Plane0Canvas, focusedElement);
            }

            return new FocusContextInfo(FocusPrecedenceLevel.HudPlane, focusedElement);
        }

        private static bool IsGlobalSpatialCombination(KeyCombination combination)
        {
            if (combination.Key is Key.N or Key.D or Key.M or Key.P or Key.A or Key.F or Key.Space or Key.Tab or
                Key.Delete or Key.Back or Key.Escape or Key.OemOpenBrackets or Key.OemCloseBrackets)
            {
                return true;
            }

            return combination.Key is Key.C or Key.V or Key.E
                && combination.Modifiers.HasFlag(KeyModifiers.Control);
        }
    }
}
