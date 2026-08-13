using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using System.Runtime.InteropServices;
using Xunit;

namespace GroveApp.UiTests;

public sealed class PublishedApplicationTests
{
    [Fact]
    public void Published_application_launches_and_exposes_its_main_window()
    {
        using var session = PublishedApplicationSession.Start();

        Assert.Equal(PublishedApplicationSession.MainWindowTitle, session.MainWindow.Title);
    }

    [Fact]
    public void Pressing_m_opens_memory_slate_without_grid_context()
    {
        using var session = PublishedApplicationSession.Start();
        session.Activate();

        Keyboard.Press(VirtualKeyShort.KEY_M);
        Wait.UntilInputIsProcessed();

        var memorySlate = Polling.WaitFor(
            () => session.MainWindow.FindFirstDescendant(
                condition => condition.ByAutomationId("MemorySlate")),
            TimeSpan.FromSeconds(5));

        if (memorySlate is null)
        {
            session.Capture("memory-slate-acceptance-failure.png");
        }

        Assert.True(
            memorySlate is not null,
            $"Pressing M did not expose the Memory Slate. Automation tree:{Environment.NewLine}{session.DescribeAutomationTree()}");
        Assert.False(memorySlate.IsOffscreen, "The Memory Slate exists in automation but is not visible to the user.");
        session.Capture("memory-slate-acceptance.png");
    }
}

internal sealed class PublishedApplicationSession : IDisposable
{
    internal const string MainWindowTitle = "Grove v9 — Spatial Grid Desktop Engine";

    private readonly Application _application;
    private readonly UIA3Automation _automation;

    private PublishedApplicationSession(
        Application application,
        UIA3Automation automation,
        Window mainWindow)
    {
        _application = application;
        _automation = automation;
        MainWindow = mainWindow;
    }

    public Window MainWindow { get; }

    public void Activate()
    {
        IntPtr previousForeground = NativeWindow.GetForegroundWindow();
        uint currentThread = NativeWindow.GetCurrentThreadId();
        uint foregroundThread = NativeWindow.GetWindowThreadProcessId(previousForeground, IntPtr.Zero);
        bool threadsAttached = foregroundThread != 0 && foregroundThread != currentThread &&
                               NativeWindow.AttachThreadInput(currentThread, foregroundThread, true);
        try
        {
            NativeWindow.ShowWindow(_application.MainWindowHandle, NativeWindow.Restore);
            NativeWindow.BringWindowToTop(_application.MainWindowHandle);
            NativeWindow.SetForegroundWindow(_application.MainWindowHandle);
            MainWindow.SetForeground();
            MainWindow.Focus();
        }
        finally
        {
            if (threadsAttached)
            {
                NativeWindow.AttachThreadInput(currentThread, foregroundThread, false);
            }
        }

        bool activated = Polling.WaitUntil(
            () => NativeWindow.GetForegroundWindow() == _application.MainWindowHandle,
            TimeSpan.FromSeconds(5));
        Assert.True(activated, "The published Grove window did not receive foreground keyboard ownership.");
    }

    public void Capture(string fileName) => MainWindow.CaptureToFile(
        Path.Combine(Path.GetDirectoryName(ResolveExecutablePath())!, fileName));

    public string DescribeAutomationTree() => string.Join(
        Environment.NewLine,
        MainWindow.FindAllDescendants().Select(element =>
            $"{Read(() => element.ControlType.ToString())} id='{Read(() => element.AutomationId)}' " +
            $"name='{Read(() => element.Name)}' offscreen={Read(() => element.IsOffscreen.ToString())}"));

    private static string Read(Func<string> read)
    {
        try
        {
            return read();
        }
        catch (FlaUI.Core.Exceptions.PropertyNotSupportedException)
        {
            return "<unsupported>";
        }
    }

    public static PublishedApplicationSession Start()
    {
        var executablePath = ResolveExecutablePath();
        var application = Application.Launch(executablePath);
        var automation = new UIA3Automation();

        var mainWindow = Polling.WaitFor(
            () => application.GetAllTopLevelWindows(automation)
                .FirstOrDefault(window => window.Title == MainWindowTitle),
            TimeSpan.FromSeconds(15));

        if (mainWindow is null)
        {
            automation.Dispose();
            application.Dispose();
            throw new InvalidOperationException(
                $"The published Grove application did not expose its main window. Executable: {executablePath}");
        }

        return new PublishedApplicationSession(application, automation, mainWindow);
    }

    public void Dispose()
    {
        if (!_application.HasExited)
        {
            _application.Close();
            Polling.WaitUntil(() => _application.HasExited, TimeSpan.FromSeconds(3));
            if (!_application.HasExited)
            {
                _application.Kill();
            }
        }

        _automation.Dispose();
        _application.Dispose();
    }

    private static string ResolveExecutablePath()
    {
        var configuredPath = Environment.GetEnvironmentVariable("GROVE_RELEASE_EXE");
        var path = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.GetFullPath(
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "GroveApp-Release", "GroveApp.exe"))
            : Path.GetFullPath(configuredPath);

        if (!File.Exists(path))
        {
            throw new FileNotFoundException(
                "Publish GroveApp to the canonical GroveApp-Release directory before running UI acceptance tests.",
                path);
        }

        return path;
    }
}

internal static class NativeWindow
{
    internal const int Restore = 9;

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool SetForegroundWindow(IntPtr windowHandle);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool ShowWindow(IntPtr windowHandle, int command);

    [DllImport("user32.dll")]
    internal static extern IntPtr GetForegroundWindow();

    [DllImport("kernel32.dll")]
    internal static extern uint GetCurrentThreadId();

    [DllImport("user32.dll")]
    internal static extern uint GetWindowThreadProcessId(IntPtr windowHandle, IntPtr processId);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool AttachThreadInput(uint attachingThread, uint targetThread, bool attach);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool BringWindowToTop(IntPtr windowHandle);
}

internal static class Polling
{
    public static T? WaitFor<T>(Func<T?> operation, TimeSpan timeout)
        where T : class
    {
        var deadline = DateTime.UtcNow + timeout;
        do
        {
            var result = operation();
            if (result is not null)
            {
                return result;
            }

            Thread.Sleep(50);
        }
        while (DateTime.UtcNow < deadline);

        return null;
    }

    public static bool WaitUntil(Func<bool> condition, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        do
        {
            if (condition())
            {
                return true;
            }

            Thread.Sleep(50);
        }
        while (DateTime.UtcNow < deadline);

        return condition();
    }
}
