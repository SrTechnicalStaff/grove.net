---
status: "SUPERSEDED BY ADR-064 — DO NOT IMPLEMENT"
---

# ADR-060: Fluent Local Editor Notepad Design

| Property | Value |
| :--- | :--- |
| **Status** | SUPERSEDED BY ADR-064 — DO NOT IMPLEMENT |
| **Date** | 2026-08-12 |
| **Area** | Fluent Design System / Local Editor Windowing & UX |
| **Target Runtime** | C# 13 / .NET 9 / Avalonia 11 / FluentAvalonia 2.x / WinUI 3 |
| **Authors** | Chief Product Definition Architect |

---

## 1. Context & Problem Statement

The Grove v9 Local Editor component (`docs/design-system/30-components/Local-editor.md`) provides a targeted, distraction-free editing surface anchored adjacent to spatial grid notes and documents. To align with modern Windows 11 desktop ergonomics and Fluent Design System guidelines, the Local Editor adopt the mental model, frame topology, and windowing behaviors of Windows 11 Notepad.

Key requirements:
1. Host the editor inside a `fa:AppWindow` / `FluentWindow` utilizing native Windows 11 Desktop Window Manager (DWM) backdrops (`MicaAlt` or `DesktopAcrylic`).
2. Integrate a native Fluent titlebar with seamless content extension (`ExtendsContentIntoTitleBar = true`) and lightweight tabbed document navigation (`TabView`).
3. Enforce standardized Fluent geometry with `--r-md` (8px / `CornerRadius="8"`) outer radii, role border `--k-edit-b` (`#7A3F3A`), and reading column measure `--measure-reading` (68ch).
4. Implement a zero-latency dual-mode Markdown engine featuring a RAW monospaced text editor (`Cascadia Code`) and a live WYSIWYG rich text preview (`Segoe UI Variable Text`), toggling instantaneously via `Ctrl+E` or header control.

---

## 2. Window Topology & Frame Geometry

### 2.1 Spatial Measurement & Containment Metrics
The Local Editor window geometry adheres strictly to the spatial token system:

| Frame Element | Value / Token | Metric | Behavior |
| :--- | :--- | :--- | :--- |
| **Corner Radius** | `--r-md` | `8px` (`CornerRadius="8"`) | Applied to `fa:AppWindow` outer border and inner container panels. |
| **Role Border** | `--k-edit-b` | `1px solid #7A3F3A` | Fixed accent stroke defining the edit-surface class. |
| **Reading Width** | `--measure-reading` | `68ch` (~`640px`) | Constrains body text column width regardless of viewport scale. |
| **Internal Padding** | `--sp-md` | `16px` | Inset space surrounding draft field, label row, and footer actions. |
| **Min Window Size** | N/A | `480px × 320px` | Absolute minimum constraint preventing control collision. |
| **Max Window Size** | N/A | Viewport $- 32\text{px}$ | Dynamic ceiling before internal scroll viewport activates. |

### 2.2 Height Calculation & Scroll Threshold Formula
The overall editor frame height $H_{\text{editor}}(N)$ scales dynamically with the active line count $N$ up to a maximum constrained by the display bounds $H_{\text{viewport}}$:

$$H_{\text{editor}}(N) = \min \left( H_{\text{viewport}} - 2 \cdot P_{\text{margin}}, \; H_{\text{header}} + H_{\text{footer}} + 2 \cdot P_{\text{inset}} + N \cdot h_{\text{line}} \right)$$

Where:
- $H_{\text{header}} = 40\text{px}$ (Tab strip + titlebar height)
- $H_{\text{footer}} = 36\text{px}$ (Escalation route + action buttons)
- $P_{\text{inset}} = 16\text{px}$ (`--sp-md`)
- $P_{\text{margin}} = 16\text{px}$ (`--sp-md`)
- $h_{\text{line}} = \text{FontSize} \cdot \text{LineHeight} = 14\text{px} \times 1.5 = 21\text{px}$

When $H_{\text{editor}}(N) = H_{\text{viewport}} - 2 \cdot P_{\text{margin}}$, the inner body editor converts from auto-expanding height to an internal scrolling container (`ScrollViewer.VerticalScrollBarVisibility = Auto`).

---

## 3. Dual-Mode WYSIWYG & RAW Markdown Engine

### 3.1 Mode Transition State Machine
The editor toggles between two presentation modes without mutating the underlying document model `IMemoryDocument`:

```
                       ┌────────────────────────────────┐
                       │   Mode 0: RAW Markdown Mode    │
                       │ - Cascadia Code / Monospace    │
                       │ - Line numbers & Gutter        │
                       │ - Direct AST text manipulation │
                       └───────────────┬────────────────┘
                                       │
                         Ctrl+E / Mode Toggle Command
                                       │
                       ┌───────────────▼────────────────┐
                       │ Mode 1: WYSIWYG Preview Mode   │
                       │ - Segoe UI Variable / Proportional│
                       │ - Rendered headings & blocks   │
                       │ - Inline interactive elements  │
                       └────────────────────────────────┘
```

### 3.2 Dual Viewport Synchronization
When switching between `RAW` and `WYSIWYG` modes, scroll position preservation is governed by character offset mapping ratio $R_{\text{scroll}}$:

$$y_{\text{target}} = y_{\text{source}} \cdot \frac{H_{\text{target\_content}}}{H_{\text{source\_content}}}$$

---

## 4. C# 13 Data Models & ViewState Interfaces

```csharp
namespace Grove.UI.FluentDesign.LocalEditor;

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;

public enum LocalEditorMode
{
    RawMarkdown,
    WysiwygPreview
}

public interface ILocalEditorViewModel : INotifyPropertyChanged
{
    string DocumentId { get; }
    string Title { get; }
    string ContentText { get; set; }
    bool IsDirty { get; }
    LocalEditorMode EditMode { get; set; }
    
    Task SaveAsync();
    Task DiscardAsync();
    void ToggleEditMode();
}

public sealed record LocalEditorConfig(
    double CornerRadius = 8.0,
    double ReadingMeasureWidth = 640.0,
    string RawFontFamily = "Cascadia Code, Consolas, monospace",
    string WysiwygFontFamily = "Segoe UI Variable Text, Segoe UI, sans-serif",
    Color RoleBorderColor = default
)
{
    public static LocalEditorConfig Default { get; } = new(
        CornerRadius: 8.0,
        ReadingMeasureWidth: 640.0,
        RawFontFamily: "Cascadia Code, Consolas, monospace",
        WysiwygFontFamily: "Segoe UI Variable Text, Segoe UI, sans-serif",
        RoleBorderColor: Color.Parse("#7A3F3A")
    );
}
```

---

## 5. FluentAvalonia Window Implementation

```xaml
<fa:AppWindow xmlns="https://github.com/avaloniaui"
              xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
              xmlns:fa="using:FluentAvalonia.UI.Windowing"
              xmlns:ui="using:FluentAvalonia.UI.Controls"
              xmlns:local="using:Grove.UI.FluentDesign.LocalEditor"
              x:Class="Grove.UI.FluentDesign.LocalEditor.LocalEditorWindow"
              Title="{Binding Title}"
              Width="672" Height="520"
              MinWidth="480" MinHeight="320"
              WindowStartupLocation="CenterOwner"
              CornerRadius="8"
              BorderThickness="1"
              BorderBrush="#7A3F3A">

    <fa:AppWindow.Styles>
        <Style Selector="fa|AppWindow">
            <Setter Property="TitleBarConfiguration">
                <fa:TitleBarConfiguration ExtendsContentIntoTitleBar="True"
                                         CaptionButtonsVisible="True"
                                         ButtonBackgroundColor="Transparent" />
            </Setter>
        </Style>
    </fa:AppWindow.Styles>

    <Grid RowDefinitions="Auto,*,Auto" Margin="0">
        <!-- Fluent Tabbed TitleBar Header -->
        <Grid Grid.Row="0" ColumnDefinitions="Auto,*,Auto" Height="40" Background="Transparent" Margin="8,0,8,0">
            <StackPanel Grid.Column="0" Orientation="Horizontal" Spacing="8" VerticalAlignment="Center">
                <TextBlock Text="LOCAL EDITOR" FontFamily="Segoe UI Variable Display" FontSize="11" 
                           FontWeight="SemiBold" Foreground="#7A3F3A" VerticalAlignment="Center"/>
                <TextBlock Text="•" Foreground="{DynamicResource TextFillColorSecondaryBrush}" VerticalAlignment="Center"/>
                <TextBlock Text="{Binding Title}" FontSize="12" Foreground="{DynamicResource TextFillColorPrimaryBrush}" 
                           FontWeight="Medium" VerticalAlignment="Center"/>
            </StackPanel>

            <!-- RAW / WYSIWYG Mode Toggle Segmented Control -->
            <StackPanel Grid.Column="2" Orientation="Horizontal" Spacing="6" VerticalAlignment="Center">
                <ui:Segmented SelectedIndex="{Binding EditModeIndex, Mode=TwoWay}">
                    <ui:SegmentedItem Content="RAW" ToolTip.Tip="Toggle Raw Markdown (Ctrl+E)"/>
                    <ui:SegmentedItem Content="PREVIEW" ToolTip.Tip="Toggle WYSIWYG Rich Text (Ctrl+E)"/>
                </ui:Segmented;
            </StackPanel>
        </Grid>

        <!-- Body Content Viewport -->
        <Border Grid.Row="1" Padding="16,8,16,8" Background="Transparent">
            <Grid>
                <!-- Mode 0: RAW Markdown Editor -->
                <TextBox x:Name="RawEditorTextBox"
                         IsVisible="{Binding IsRawMode}"
                         Text="{Binding ContentText, Mode=TwoWay}"
                         AcceptsReturn="True"
                         TextWrapping="Wrap"
                         FontFamily="Cascadia Code, Consolas, monospace"
                         FontSize="14"
                         LineHeight="21"
                         MaxWidth="640"
                         HorizontalAlignment="Center"
                         Background="Transparent"
                         BorderThickness="0"/>

                <!-- Mode 1: WYSIWYG Rich Text Render Surface -->
                <ScrollViewer IsVisible="{Binding IsWysiwygMode}" HorizontalScrollBarVisibility="Disabled">
                    <ContentControl x:Name="WysiwygPreviewControl" 
                                    Content="{Binding FormattedDocument}"
                                    MaxWidth="640" 
                                    HorizontalAlignment="Center"/>
                </ScrollViewer>
            </Grid>
        </Border>

        <!-- Footer Actions Bar -->
        <Grid Grid.Row="2" ColumnDefinitions="Auto,*,Auto" Height="44" Padding="16,0,16,0" 
              Background="{DynamicResource SolidBackgroundFillColorSecondaryBrush}">
            <Button Grid.Column="0" Content="Open in Writing Slate" Classes="accent" Theme="{DynamicResource HyperlinkButtonTheme}"/>
            
            <StackPanel Grid.Column="2" Orientation="Horizontal" Spacing="12" VerticalAlignment="Center">
                <TextBlock Text="Ctrl+S" FontSize="11" Foreground="{DynamicResource TextFillColorTertiaryBrush}" VerticalAlignment="Center"/>
                <Button Content="Cancel" Command="{Binding DiscardCommand}" Classes="standard"/>
                <Button Content="Save" Command="{Binding SaveCommand}" Classes="accent" Background="#7A3F3A" Foreground="White"/>
            </StackPanel>
        </Grid>
    </Grid>
</fa:AppWindow>
