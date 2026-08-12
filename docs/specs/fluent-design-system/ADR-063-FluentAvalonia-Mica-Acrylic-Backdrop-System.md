---
status: "IMPLEMENTED - AWAITING USER REVIEW"
---

# ADR-063: FluentAvalonia Mica and Acrylic Backdrop System

| Property | Value |
| :--- | :--- |
| **Status** | IMPLEMENTED - AWAITING USER REVIEW |
| **Date** | 2026-08-12 |
| **Area** | UI Architecture / Visual Shell & Windowing / FluentAvalonia Backdrops |
| **Target Runtime** | C# 13 / .NET 9 / Avalonia 11 / FluentAvalonia 2.x / Windows 11 DWM |
| **Authors** | Chief Product Definition Architect |

---

## 1. Context & Visual Material Architecture

Grove v9 integrates Windows 11 Desktop Window Manager (DWM) glassmorphic materials to establish visual depth hierarchy across window surfaces, floating HUD slates, and popover chrome.

### Architectural Directives
1. **Material Standardization**: Window backgrounds must consume native Windows 11 materials (`Mica`, `MicaAlt`, `DesktopAcrylic`, `InAppAcrylic`) provided by FluentAvalonia `MicaController` and `AcrylicController`.
2. **Immersive Dark Titlebar Interop**: Application titlebars MUST extend into window content (`ExtendsContentIntoTitleBar = true`) and synchronize theme states via Win32 DWM interop attributes (`DWMWA_USE_IMMERSIVE_DARK_MODE`).
3. **Glassmorphic Elevation Hierarchy**: Surface depths $z \in \{0, 1, 2, 3\}$ define distinct optical blur radii and tint opacity coefficients.
4. **Platform Fallback Integrity**: On unsupported operating systems or software-rendered displays, the backdrop system MUST degrade gracefully without visual corruption to semi-transparent Avalonia acrylic brushes or solid dark surface fills (`#1E1E1E`).

---

## 2. Glassmorphic Elevation & Backdrop Matrix

### 2.1 Surface Material Mapping Matrix

| Depth ($z$) | Surface Target | Backdrop Type | DWM System Backdrop Type | Fallback Brush |
| :--- | :--- | :--- | :--- | :--- |
| **$z = 0$** | Main Grid Root Window | `MicaKind.Base` | `DWMSBT_MAINWINDOW` (`2`) | `#121212` / Solid Dark |
| **$z = 1$** | Local Editor (`ADR-060`) | `MicaKind.MicaAlt` | `DWMSBT_TABBEDWINDOW` (`4`) | `#1E1E1E` / Semi-Dark |
| **$z = 2$** | HUD Slates & Toolbars | `AcrylicKind.Desktop` | `DWMSBT_TRANSIENTWINDOW` (`3`) | `ExperimentalAcrylic` |
| **$z = 3$** | Context Menus & Popovers | `AcrylicKind.InApp` | N/A (In-App Composition) | `#2A2A2A` / Opaque Inset |

### 2.2 Mathematical Optical Blur & Tint Formulas
The blur radius $R_{\text{blur}}(z)$ and tint opacity $\alpha_{\text{tint}}(z)$ are governed by linear depth scaling functions over elevation step $z \in [0, 3]$:

$$R_{\text{blur}}(z) = R_{\text{base}} + 10 \cdot z = 10 + 10 \cdot z \quad (\text{in DIPs})$$

$$\alpha_{\text{tint}}(z) = \alpha_{\text{base}} \cdot \left( 1 - 0.12 \cdot z \right) = 0.85 \cdot \left( 1 - 0.12 \cdot z \right)$$

For elevation steps:
- $z=0$: $R_{\text{blur}} = 10\text{px}$, $\alpha_{\text{tint}} = 0.850$
- $z=1$: $R_{\text{blur}} = 20\text{px}$, $\alpha_{\text{tint}} = 0.748$
- $z=2$: $R_{\text{blur}} = 30\text{px}$, $\alpha_{\text{tint}} = 0.646$
- $z=3$: $R_{\text{blur}} = 40\text{px}$, $\alpha_{\text{tint}} = 0.544$

```
 Elevation z
  z = 0 ──► Main App Shell ──────► Mica Base    (Blur: 10px, Tint Alpha: 0.850)
  z = 1 ──► Local Editor ────────► Mica Alt     (Blur: 20px, Tint Alpha: 0.748)
  z = 2 ──► HUD Slates ──────────► Desktop Acrylic (Blur: 30px, Tint Alpha: 0.646)
  z = 3 ──► Context Menus ───────► InApp Acrylic   (Blur: 40px, Tint Alpha: 0.544)
```

---

## 3. Windows 11 DWM Interop Specification

### 3.1 Win32 DWM Attributes & P/Invoke Definitions
Native DWM API integration utilizes `dwmapi.dll` to set window backdrop and caption behavior:

```csharp
internal static class NativeDwmApi
{
    internal const string DwmApiDll = "dwmapi.dll";

    internal enum DWMWINDOWATTRIBUTE
    {
        DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
        DWMWA_WINDOW_CORNER_PREFERENCE = 33,
        DWMWA_BORDER_COLOR = 34,
        DWMWA_CAPTION_COLOR = 35,
        DWMWA_TEXT_COLOR = 36,
        DWMWA_SYSTEMBACKDROP_TYPE = 38
    }

    internal enum DWM_SYSTEMBACKDROP_TYPE
    {
        DWMSBT_AUTO = 0,
        DWMSBT_NONE = 1,
        DWMSBT_MAINWINDOW = 2,      // Mica Base
        DWMSBT_TRANSIENTWINDOW = 3, // Desktop Acrylic
        DWMSBT_TABBEDWINDOW = 4     // Mica Alt
    }

    internal enum DWM_WINDOW_CORNER_PREFERENCE
    {
        DWMWCP_DEFAULT = 0,
        DWMWCP_DONOTROUND = 1,
        DWMWCP_ROUND = 2,           // --r-md (8px / 12px)
        DWMWCP_ROUNDSMALL = 3       // --r-sm (4px)
    }

    [DllImport(DwmApiDll, PreserveSig = true)]
    internal static extern int DwmSetWindowAttribute(
        IntPtr hwnd,
        DWMWINDOWATTRIBUTE attribute,
        ref int pvAttribute,
        int cbAttribute);
}
```

---

## 4. C# 13 Backdrop Manager Implementation

```csharp
namespace Grove.UI.FluentDesign.Backdrops;

using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using FluentAvalonia.Styling;
using FluentAvalonia.UI.Media;
using FluentAvalonia.UI.Windowing;

public enum GroveBackdropType
{
    MicaBase,
    MicaAlt,
    DesktopAcrylic,
    InAppAcrylic,
    SolidFallback
}

public sealed class FluentWindowBackdropManager
{
    private static readonly Version Win11Build22H2 = new(10, 0, 22621, 0);

    public static void ApplyBackdrop(AppWindow window, GroveBackdropType backdropType, bool isDarkMode)
    {
        if (window == null) throw new ArgumentNullException(nameof(window));

        // 1. Configure Titlebar Extension & Interop
        window.TitleBar.ExtendsContentIntoTitleBar = true;
        window.TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;

        IntPtr hwnd = window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (hwnd != IntPtr.Zero && OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000))
        {
            // Apply native DWM Immersive Dark Mode
            int darkValue = isDarkMode ? 1 : 0;
            NativeDwmApi.DwmSetWindowAttribute(
                hwnd,
                NativeDwmApi.DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE,
                ref darkValue,
                sizeof(int));

            // Set Corner Radius Preference (--r-md)
            int cornerPref = (int)NativeDwmApi.DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
            NativeDwmApi.DwmSetWindowAttribute(
                hwnd,
                NativeDwmApi.DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE,
                ref cornerPref,
                sizeof(int));

            // Set System Backdrop via DWM (Windows 11 22H2+)
            if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22621))
            {
                int dwmBackdrop = backdropType switch
                {
                    GroveBackdropType.MicaBase => (int)NativeDwmApi.DWM_SYSTEMBACKDROP_TYPE.DWMSBT_MAINWINDOW,
                    GroveBackdropType.MicaAlt => (int)NativeDwmApi.DWM_SYSTEMBACKDROP_TYPE.DWMSBT_TABBEDWINDOW,
                    GroveBackdropType.DesktopAcrylic => (int)NativeDwmApi.DWM_SYSTEMBACKDROP_TYPE.DWMSBT_TRANSIENTWINDOW,
                    _ => (int)NativeDwmApi.DWM_SYSTEMBACKDROP_TYPE.DWMSBT_AUTO
                };

                NativeDwmApi.DwmSetWindowAttribute(
                    hwnd,
                    NativeDwmApi.DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE,
                    ref dwmBackdrop,
                    sizeof(int));
            }
        }

        // 2. FluentAvalonia Backdrop Controller Configuration
        switch (backdropType)
        {
            case GroveBackdropType.MicaBase:
                window.TransparencyBackgroundFallback = Avalonia.Media.Brushes.Transparent;
                window.Backdrop = new MicaBackdrop { Kind = MicaKind.Base };
                break;

            case GroveBackdropType.MicaAlt:
                window.TransparencyBackgroundFallback = Avalonia.Media.Brushes.Transparent;
                window.Backdrop = new MicaBackdrop { Kind = MicaKind.MicaAlt };
                break;

            case GroveBackdropType.DesktopAcrylic:
                window.Backdrop = new DesktopAcrylicBackdrop();
                break;

            case GroveBackdropType.SolidFallback:
            default:
                window.Backdrop = null;
                window.Background = isDarkMode 
                    ? Avalonia.Media.Brush.Parse("#1E1E1E") 
                    : Avalonia.Media.Brush.Parse("#F9F9F9");
                break;
        }
    }
}
```
