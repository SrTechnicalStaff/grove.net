using Avalonia;
using Avalonia.Controls;

namespace GroveApp.Engine;

/// <summary>
/// Optional XAML metadata for controls that participate in the four-tier focus
/// precedence model. The router remains the owner of routing decisions.
/// </summary>
public static class FocusPrecedenceAttachedProperties
{
    public static readonly AttachedProperty<FocusPrecedenceLevel> LevelProperty =
        AvaloniaProperty.RegisterAttached<Control, FocusPrecedenceLevel>(
            "Level",
            typeof(FocusPrecedenceAttachedProperties));

    public static void SetLevel(Control element, FocusPrecedenceLevel value) =>
        element.SetValue(LevelProperty, value);

    public static FocusPrecedenceLevel GetLevel(Control element) =>
        element.GetValue(LevelProperty);
}
