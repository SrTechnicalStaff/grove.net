---
status: "PARTIAL — verified tunnel seam with remaining coverage gaps"
---

# ADR-058: Global Keybind Focus Precedence Router Architecture

| Property | Value |
| :--- | :--- |
| **Status** | PARTIAL — verified tunnel seam with remaining coverage gaps |
| **Date** | 2026-08-12 |
| **Area** | Spatial Grid Engine / Input Routing Subsystem |
| **Target Runtime** | C# 13 / .NET 9 / Avalonia 11.2.5 / SkiaSharp 3.x |
| **Authors** | Chief Product Definition Architect |

---

## 1. Executive Context & Architectural Principles

In Grove v9, keyboard shortcuts control spatial creation, camera manipulation, selection management, and text editing. Without a deterministic focus routing hierarchy, single-key shortcuts such as `N` (Note arming), `A` (Anchor toggle), `D` (Document arming), or `Spacebar` (Canvas drag) would conflict with text input when typing inside a `TextBox`, `TextEditor`, or inline `QuickNote`.

The **Global Keybind Focus Precedence Router** solves this by establishing a 4-tier context hierarchy. All keyboard events are intercepted at the window root during Avalonia's **Tunneling Phase** (`RoutingStrategies.Tunnel`). The router evaluates active visual focus, active modal overlays, and control types to decide whether a shortcut is dispatched to spatial grid commands or passed down to focused UI controls.

### Key Architectural Requirements:
1. **4-Tier Priority Pyramid**:
   - **Level 0 (`FocusedTextBox`)**: Inline editor, Local Notepad, Quick Note input. Highest priority. Intercepts all text entry keys.
   - **Level 1 (`InformationOverlay`)**: Modal popups, Layer Manager dialogs, system notifications on Plane 2.
   - **Level 2 (`HUDPlane`)**: Writing Slate, Memory Slate, Gallery Slate headers, toolbars, HUD buttons, and Layer Manager overlay controls.
   - **Level 3 (`Plane0Canvas`)**: Plane 0 Spatial Grid Canvas, marquee box selection, cursor movement, spatial tool arming. Lowest fallback level.
2. **Strict Text Editing Isolation**: When keyboard focus is inside a text input control (`FocusedTextBox`), single-character spatial hotkeys (`N`, `Shift+N`, `D`, `A`, `Spacebar`, `Del`) MUST NOT be intercepted by Plane 0 canvas handlers. They are forwarded as standard text composition characters.
3. **Deterministic Override Exceptions**:
   - `Esc`: Unfocuses active text box, closes modal overlays, disarms tools, or clears selection, progressing deterministically down the priority levels.
   - `Ctrl+Enter`: Commits text editing in `FocusedTextBox` and transfers focus back to `Plane0Canvas`; opens full Local Editor when pressed over a selected placement on `Plane0Canvas`.
4. **Tunneling-Phase Key Routing**: The precedence router intercepts key events at `IInputRoot.KeyDownEvent` (Tunnel phase) *before* Avalonia's standard element-focused bubbling phase.

---

## 2. 4-Tier Focus Context Precedence Architecture

```
+-----------------------------------------------------------------------------------+
|                        AVALONIA WINDOW TUNNELING PHASE                            |
|                            IInputRoot.KeyDownEvent                                |
+-----------------------------------------------------------------------------------+
                                         |
                                         v
+-----------------------------------------------------------------------------------+
|                     GLOBAL KEYBIND FOCUS PRECEDENCE ROUTER                        |
|                                                                                   |
|  Level 0: FocusedTextBox  ? --[Is Focused Input Control?]--> [Forward to TextBox]  |
|                 | (No)                                         (Handled by Text)  |
|                 v                                                                 |
|  Level 1: InformationOverlay ? --[Modal Overlay Active?]--> [Route to Overlay]    |
|                 | (No)                                      (Handled by Overlay)  |
|                 v                                                                 |
|  Level 2: HUDPlane        ? --[HUD Control Focused?]------> [Route to HUD Window]  |
|                 | (No)                                      (Handled by HUD)      |
|                 v                                                                 |
|  Level 3: Plane0Canvas       -----------------------------> [Route to Spatial Grid]|
|                                                             (Arming / Selection)  |
+-----------------------------------------------------------------------------------+
```

### 2.1 Context Precedence Level Definitions

| Precedence Level | Target Context | Visual Scope | Interception Policy | Fallback Behavior |
| :--- | :--- | :--- | :--- | :--- |
| **Level 0** | `FocusedTextBox` | Active text editor, inline QuickNote, text inputs | **Exclusive Text Mode**: Blocks all single-key spatial hotkeys. | Text insertion / local edit commands. |
| **Level 1** | `InformationOverlay` | Plane 2 overlays, modal windows, Spatial Layer Manager | **Modal Lock**: Intercepts `Esc`, `Tab`, dialog shortcuts. | Pass unhandled events to Level 2. |
| **Level 2** | `HUDPlane` | Slate controls, window headers, HUD buttons, Layer Manager overlay | **Control Navigation**: Intercepts toolbar hotkeys and tab cycles. | Pass unhandled events to Level 3. |
| **Level 3** | `Plane0Canvas` | Plane 0 Grid Canvas, background surface | **Spatial Grid Engine**: Handles `N`, `Shift+N`, `D`, `A`, `Del`, `Space`. | Global unhandled key drop. |

---

## 3. Comprehensive Keybind Routing Matrix

The following matrix defines the exact resolution of every keyboard shortcut across all 4 focus contexts:

| Shortcut | Level 0: `FocusedTextBox` | Level 1: `InformationOverlay` | Level 2: `HUDPlane` | Level 3: `Plane0Canvas` |
| :--- | :--- | :--- | :--- | :--- |
| `Spacebar` | Inserts space character `' '` | Triggers active overlay button | Triggers focused HUD control | Toggles spatial pan / canvas drag mode |
| `Ctrl+Enter` | Commits text edit, blurs text box, transfers focus to `Plane0Canvas` | Default modal action (OK/Confirm) | Confirms HUD dialog action | Opens full Local Editor / Notepad for selection |
| `Ctrl+E` | Inserts inline code block or formatting | Ignored | Focuses layer search bar | Opens Local Editor for selected placement |
| `Tab` / `Ctrl+Tab` | Inserts indent tab / moves focus to next text field | Cycles modal tab controls / layer list | Cycles focus across HUD controls / named Slates | Cycles spatial layer selection (up / down) |
| `N` | Inserts lowercase character `'n'` | Ignored / Search filter | Ignored | Arms **Note** tool (`ARMED_NOTE`) |
| `Shift+N` | Inserts uppercase character `'N'` | Ignored / Search filter | Ignored | Arms **Quick Note** tool (`ARMED_QUICKNOTE`) |
| `D` | Inserts lowercase character `'d'` | Ignored / Search filter | Ignored | Arms **Document** tool (`ARMED_DOCUMENT`) |
| `A` | Inserts lowercase character `'a'` | Selects all in overlay search | Selects all in HUD container | Creates or removes a Content-side Anchor relation on selection |
| `Del` / `Backspace` | Deletes preceding / selected character | Deletes overlay item | Closed focused named Slate | Deletes selected spatial placements from grid |
| `Esc` | Blurs text focus, cancels editing state | Closes active overlay window | Returns focus from HUD to Plane0Canvas | Disarms armed tool / clears active selection |
| `Ctrl+C` | Copies highlighted text to OS clipboard | Copies selected overlay text | Copies HUD panel descriptor | Copies selected Memory references to spatial clipboard |
| `Ctrl+V` | Pastes text from OS clipboard into text editor | Pastes into overlay search | Ignored | Pastes spatial payload relative to grid cursor |

---

## 4. C# 13 Type Contracts & Interface Specifications

```csharp
namespace Grove.Input.Precedence;

using System;
using System.Runtime.InteropServices;
using Avalonia.Input;
using Avalonia.Interactivity;

/// <summary>
/// Precedence levels for global keybind routing.
/// Lower integer value indicates HIGHER priority precedence.
/// </summary>
public enum FocusPrecedenceLevel : byte
{
    FocusedTextBox = 0,     // Level 0: Active text editing
    InformationOverlay = 1, // Level 1: Plane 2 modal popups & dialogs
    HUDPlane = 2,           // Level 2: Slates & HUD controls
    Plane0Canvas = 3        // Level 3: Plane 0 Spatial grid canvas
}

/// <summary>
/// Classification of action executed by keybind handler.
/// </summary>
public enum KeybindHandlingResult : byte
{
    Ignored = 0,
    ConsumedByFocusedControl = 1,
    DispatchedToSpatialGrid = 2,
    DispatchedToOverlay = 3,
    DispatchedToHUD = 4
}

/// <summary>
/// Combination descriptor for key matching.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public readonly record struct KeyCombination(
    Key Key,
    KeyModifiers Modifiers
)
{
    public static KeyCombination FromEventArgs(KeyEventArgs args) =>
        new(args.Key, args.KeyModifiers);
}

/// <summary>
/// Evaluated focus context information.
/// </summary>
public sealed record FocusContextInfo(
    FocusPrecedenceLevel ActiveLevel,
    IInputElement? FocusedElement,
    bool IsTextEditingActive,
    Guid? SelectedGridLayerId
);

/// <summary>
/// Core contract for the global focus precedence keybind router.
/// </summary>
public interface IFocusPrecedenceRouter
{
    FocusContextInfo CurrentContext { get; }
    
    event Action<FocusContextInfo>? FocusContextChanged;
    event Action<KeyCombination, KeybindHandlingResult>? KeybindRouted;

    /// <summary>
    /// Evaluates an incoming key event at the window root during tunneling phase.
    /// Returns true if the key event was intercepted and handled by the router.
    /// </summary>
    bool RouteKeyEvent(KeyEventArgs args, RoutingStrategies strategy);
    
    /// <summary>
    /// Explicitly forces focus back to Plane 0 Canvas.
    /// </summary>
    void ForcePlane0CanvasFocus();
}
```

---

## 5. Avalonia Input Routing & Attached Properties Implementation

```csharp
namespace Grove.Input.Precedence;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

public static class FocusPrecedenceAttachedProperties
{
    /// <summary>
    /// Attached property to explicitly override or declare an element's precedence level.
    /// </summary>
    public static readonly AttachedProperty<FocusPrecedenceLevel?> PrecedenceLevelProperty =
        AvaloniaProperty.RegisterAttached<Control, FocusPrecedenceLevel?>(
            "PrecedenceLevel", typeof(FocusPrecedenceAttachedProperties));

    public static void SetPrecedenceLevel(Control element, FocusPrecedenceLevel? value) =>
        element.SetValue(PrecedenceLevelProperty, value);

    public static FocusPrecedenceLevel? GetPrecedenceLevel(Control element) =>
        element.GetValue(PrecedenceLevelProperty);
}
```

```csharp
namespace Grove.Input.Precedence;

using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Grove.SpatialGrid.Arming;
using Grove.SpatialGrid.Operations;

public sealed class GlobalFocusPrecedenceRouter : IFocusPrecedenceRouter
{
    private readonly TopLevel _rootWindow;
    private readonly IToolArmingService _armingService;
    private readonly ISelectionService _selectionService;
    private FocusContextInfo _currentContext;

    public FocusContextInfo CurrentContext => _currentContext;

    public event Action<FocusContextInfo>? FocusContextChanged;
    public event Action<KeyCombination, KeybindHandlingResult>? KeybindRouted;

    public GlobalFocusPrecedenceRouter(
        TopLevel rootWindow,
        IToolArmingService armingService,
        ISelectionService selectionService)
    {
        _rootWindow = rootWindow ?? throw new ArgumentNullException(nameof(rootWindow));
        _armingService = armingService ?? throw new ArgumentNullException(nameof(armingService));
        _selectionService = selectionService ?? throw new ArgumentNullException(nameof(selectionService));

        _currentContext = EvaluateFocusContext();

        // Subscribe to global Tunneling KeyDown event on Window root
        _rootWindow.AddHandler(
            InputElement.KeyDownEvent,
            OnWindowKeyDownTunnel,
            RoutingStrategies.Tunnel);
    }

    private FocusContextInfo EvaluateFocusContext()
    {
        var focused = FocusManager.Instance?.Current;
        if (focused is null)
        {
            return new FocusContextInfo(FocusPrecedenceLevel.Plane0Canvas, null, false, null);
        }

        // Check if focused element is text box or inline text editor
        if (focused is TextBox or TextEditor || IsControlTextEditing(focused))
        {
            return new FocusContextInfo(FocusPrecedenceLevel.FocusedTextBox, focused, true, null);
        }

        // Check explicit attached property on control hierarchy
        if (focused is Control ctrl)
        {
            var explicitLevel = FocusPrecedenceAttachedProperties.GetPrecedenceLevel(ctrl);
            if (explicitLevel.HasValue)
            {
                return new FocusContextInfo(explicitLevel.Value, focused, false, null);
            }
        }

        return new FocusContextInfo(FocusPrecedenceLevel.Plane0Canvas, focused, false, null);
    }

    private static bool IsControlTextEditing(IInputElement element)
    {
        // Property inspection or type check for custom inline editors
        return element.GetType().Name.Contains("Text", StringComparison.OrdinalIgnoreCase);
    }

    private void OnWindowKeyDownTunnel(object? sender, KeyEventArgs e)
    {
        if (RouteKeyEvent(e, RoutingStrategies.Tunnel))
        {
            e.Handled = true;
        }
    }

    public bool RouteKeyEvent(KeyEventArgs e, RoutingStrategies strategy)
    {
        _currentContext = EvaluateFocusContext();
        var combo = KeyCombination.FromEventArgs(e);

        // Level 0: FocusedTextBox takes precedence over spatial shortcuts
        if (_currentContext.ActiveLevel == FocusPrecedenceLevel.FocusedTextBox)
        {
            // Escape key in text box: Blur focus and pass down
            if (combo.Key == Key.Escape)
            {
                _rootWindow.FocusManager?.ClearFocus();
                KeybindRouted?.Invoke(combo, KeybindHandlingResult.ConsumedByFocusedControl);
                return true;
            }

            // Ctrl+Enter in text box: Commit editing, clear focus to canvas
            if (combo.Key == Key.Enter && combo.Modifiers.HasFlag(KeyModifiers.Control))
            {
                _rootWindow.FocusManager?.ClearFocus();
                KeybindRouted?.Invoke(combo, KeybindHandlingResult.ConsumedByFocusedControl);
                return true;
            }

            // Single character keys (N, D, A, Space, etc.) are allowed to bubble to text editor
            KeybindRouted?.Invoke(combo, KeybindHandlingResult.ConsumedByFocusedControl);
            return false; // Do not intercept in Tunnel phase; allow text control to receive character
        }

        // Level 1: Information Overlay
        if (_currentContext.ActiveLevel == FocusPrecedenceLevel.InformationOverlay)
        {
            if (combo.Key == Key.Escape)
            {
                // Close active overlay
                KeybindRouted?.Invoke(combo, KeybindHandlingResult.DispatchedToOverlay);
                return true;
            }
        }

        // Level 3: Plane0Canvas Spatial Shortcuts
        if (_currentContext.ActiveLevel == FocusPrecedenceLevel.Plane0Canvas)
        {
            switch (combo.Key)
            {
                case Key.N when combo.Modifiers == KeyModifiers.None:
                    _armingService.ArmTool(ArmableContentType.Note);
                    KeybindRouted?.Invoke(combo, KeybindHandlingResult.DispatchedToSpatialGrid);
                    return true;

                case Key.N when combo.Modifiers.HasFlag(KeyModifiers.Shift):
                    _armingService.ArmTool(ArmableContentType.QuickNote);
                    KeybindRouted?.Invoke(combo, KeybindHandlingResult.DispatchedToSpatialGrid);
                    return true;

                case Key.D when combo.Modifiers == KeyModifiers.None:
                    _armingService.ArmTool(ArmableContentType.Document);
                    KeybindRouted?.Invoke(combo, KeybindHandlingResult.DispatchedToSpatialGrid);
                    return true;

                case Key.Escape:
                    if (_armingService.CurrentState != ToolArmingState.Idle)
                    {
                        _armingService.Disarm();
                    }
                    else
                    {
                        _selectionService.ClearSelection();
                    }
                    KeybindRouted?.Invoke(combo, KeybindHandlingResult.DispatchedToSpatialGrid);
                    return true;
            }
        }

        return false;
    }

    public void ForcePlane0CanvasFocus()
    {
        _rootWindow.FocusManager?.ClearFocus();
        _currentContext = new FocusContextInfo(FocusPrecedenceLevel.Plane0Canvas, null, false, null);
        FocusContextChanged?.Invoke(_currentContext);
    }
}
```

---

## 6. Verification & Test Plan

1. **Text Input Isolation Tests**:
   - Focus `TextBox` control and press `N`, `Shift+N`, `D`, `A`, `Spacebar`.
   - Verify that characters are typed into the text box and NO tool arming state transitions occur.
2. **Context Fallback Tests**:
   - With text box unfocused, press `N`. Verify tool arms `ARMED_NOTE`.
   - Press `Esc`. Verify tool disarms to `IDLE`.
3. **Tunneling Interception Verification**:
   - Verify that `OnWindowKeyDownTunnel` handles key events during `RoutingStrategies.Tunnel` before element bubble handlers execute.
