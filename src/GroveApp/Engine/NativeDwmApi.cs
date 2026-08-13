using System;
using System.Runtime.InteropServices;
using Avalonia.Controls;

namespace GroveApp.Engine;

/// <summary>
/// Small optional Windows backdrop seam. Avalonia remains the material owner;
/// this adapter only applies the native dark-mode preference when a Win32 handle
/// is available, and is a no-op on other platforms.
/// </summary>
public static class NativeDwmApi
{
    private const int DwmwaUseImmersiveDarkMode = 20;

    public static bool TryApplyDarkMode(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        if (!OperatingSystem.IsWindows())
        {
            return false;
        }

        IntPtr? handle = window.TryGetPlatformHandle()?.Handle;
        if (!handle.HasValue || handle.Value == IntPtr.Zero)
        {
            return false;
        }

        int enabled = 1;
        return DwmSetWindowAttribute(handle.Value, DwmwaUseImmersiveDarkMode, ref enabled, sizeof(int)) == 0;
    }

    [DllImport("dwmapi.dll", ExactSpelling = true)]
    private static extern int DwmSetWindowAttribute(
        IntPtr hwnd,
        int attribute,
        ref int value,
        int valueSize);
}

public sealed class WindowBackdropManager
{
    public bool Apply(Window window) => NativeDwmApi.TryApplyDarkMode(window);
}
