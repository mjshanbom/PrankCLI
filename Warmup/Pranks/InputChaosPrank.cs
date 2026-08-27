using System.Diagnostics;
using Warmup.Native;

namespace Warmup.Pranks;

internal sealed class InputChaosPrank : IPrank
{
    public string Name => "Input Chaos";

    private const string Message = "This computer has been pranked. Beware future keystrokes. :)";

    public void Run()
    {
        var process = Process.Start(new ProcessStartInfo("notepad.exe") { UseShellExecute = true });
        if (process is null)
        {
            return;
        }

        IntPtr handle = WaitForMainWindowHandle(process);
        if (handle != IntPtr.Zero)
        {
            NativeMethods.SetForegroundWindow(handle);
        }

        JiggleMouse();

        if (handle != IntPtr.Zero)
        {
            NativeMethods.SetForegroundWindow(handle);
        }

        TypeMessage();
    }

    private static void JiggleMouse()
    {
        var rng = new Random();
        var end = DateTime.UtcNow.AddSeconds(4);
        while (DateTime.UtcNow < end)
        {
            NativeMethods.SendMouseMove(rng.Next(-40, 41), rng.Next(-40, 41));
            Thread.Sleep(rng.Next(150, 300));
        }
    }

    private static void TypeMessage()
    {
        foreach (char c in Message)
        {
            NativeMethods.SendUnicodeChar(c);
            Thread.Sleep(60);
        }
    }

    private static IntPtr WaitForMainWindowHandle(Process process)
    {
        var deadline = DateTime.UtcNow.AddSeconds(2);
        while (DateTime.UtcNow < deadline)
        {
            process.Refresh();
            if (process.MainWindowHandle != IntPtr.Zero)
            {
                return process.MainWindowHandle;
            }
            Thread.Sleep(20);
        }
        return IntPtr.Zero;
    }
}
