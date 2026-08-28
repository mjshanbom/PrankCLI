using System.Diagnostics;
using Warmup.Native;

namespace Warmup.Pranks;

// Factored out of AsciiBombPrank and ChessPrank, which both relaunched this exe in
// "--show-art" mode (temp file + child process + reposition) with near-identical code.
// Centralizing it means the relaunch trick only has to be gotten right once.
internal static class ArtWindowLauncher
{
    /// <summary>
    /// Writes <paramref name="content"/> to a temp file, relaunches this exe in "--show-art"
    /// mode to display it in a bare console window, and moves that window to the given
    /// position/size. Returns the spawned window handle, or IntPtr.Zero if the process or its
    /// window couldn't be created/located in time.
    /// </summary>
    public static IntPtr Spawn(string content, int x, int y, int width, int height)
    {
        string exePath = Environment.ProcessPath
            ?? throw new InvalidOperationException("Could not resolve the current executable path.");

        string tempFile = Path.Combine(Path.GetTempPath(), $"warmup_art_{Guid.NewGuid():N}.txt");
        File.WriteAllText(tempFile, content);

        var startInfo = new ProcessStartInfo(exePath) { UseShellExecute = true };
        startInfo.ArgumentList.Add("--show-art");
        startInfo.ArgumentList.Add(tempFile);

        var process = Process.Start(startInfo);
        if (process is null)
        {
            return IntPtr.Zero;
        }

        IntPtr handle = NativeMethods.WaitForMainWindowHandle(process);
        if (handle != IntPtr.Zero)
        {
            NativeMethods.MoveWindow(handle, x, y, width, height, true);
        }

        return handle;
    }
}
