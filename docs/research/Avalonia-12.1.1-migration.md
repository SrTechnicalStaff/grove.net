# Avalonia 12.1.1 migration

Date: 2026-08-13

## Decision

Grove remains on .NET 9 and upgrades all Avalonia packages to 12.1.1. Avalonia
12 supports .NET 8 and later, so .NET 9 is supported. Grove removes
FluentAvaloniaUI 2.2.0 instead of mixing an Avalonia 11 control library into an
Avalonia 12 application. FluentAvaloniaUI 3.0.2 is not a replacement on the
current runtime because its package target is .NET 10.

Grove uses Avalonia's native Fluent theme and native `Window` shell. The normal
system title bar remains full-width. Grove's own tokens continue to define
application surfaces and signals.

## Binding migration facts

- Avalonia 12.1.1 targets .NET 8 and .NET 10 and is compatible with .NET 9.
- All Avalonia package references must use the same 12.x patch line.
- `Avalonia.Diagnostics` was removed.
- Compiled bindings are enabled by default.
- `TopLevel.GetTopLevel(visual)` remains the supported way to resolve a host.
- Window decorations were redesigned; old title-bar types are removed.
- The clipboard now uses `IAsyncDataTransfer`, `DataTransfer`, and
  `DataTransferItem`; `IDataObject` and the old `DataObject` shim are removed.
- `TextBox.Watermark` is renamed to `PlaceholderText`.
- `RenderOptions.TextRenderingMode` is renamed to
  `TextOptions.TextRenderingMode`.
- `GotFocus` and `LostFocus` now use `FocusChangedEventArgs`.
- `Avalonia.Headless.XUnit` 12 targets xUnit v3; Grove's Windows UI Automation
  suite remains the executable-level test path while the unit suite remains on
  xUnit v2.

## Expected Grove compiler breaks

1. `AppWindow`, `FluentAvaloniaTheme`, and FluentAvalonia namespaces.
2. Title-bar customization through FluentAvalonia's `TitleBar` property.
3. Native clipboard read/write methods and data formats.
4. Drag-and-drop data access if it still uses the compatibility data object.
5. Focus event handler signatures.
6. Renamed XAML text and rendering properties.

## Primary sources

- Avalonia, “Breaking changes in Avalonia 12”:
  https://docs.avaloniaui.net/docs/avalonia12-breaking-changes
- NuGet, Avalonia 12.1.1 package frameworks and dependencies:
  https://www.nuget.org/packages/Avalonia/12.1.1
- NuGet, FluentAvaloniaUI 3.0.2 package frameworks and dependencies:
  https://www.nuget.org/packages/FluentAvaloniaUI/3.0.2
