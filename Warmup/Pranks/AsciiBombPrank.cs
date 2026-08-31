using System.Diagnostics;
using Warmup.Assets;
using Warmup.Native;

namespace Warmup.Pranks;
// We see that WaitForMainWindowHandle is copy pasted in ChessPrank.cs, AsciiBombPrank.cs and InputChaos.cs
// We can keep the code DRY-er by creating a utility class and making this a static method, able to be used across the codebase
// We also write to a temp file, but these files are never deleted. This could cause a large amount of files to be created and never
// removed from disk, before we exit the program we should remove these files.

internal sealed class AsciiBombPrank : IPrank
{
    public string Name => "ASCII Bomb";

    private const int WindowCount = 5;

    public void Run()
    {
        if (!ArtLibrary.HasAny)
        {
            Console.WriteLine("No photos found in Assets/Images — drop some .png/.jpg files in there and rebuild.");
            return;
        }

        var rng = new Random();
        int screenWidth = NativeMethods.GetSystemMetrics(NativeMethods.SM_CXSCREEN);
        int screenHeight = NativeMethods.GetSystemMetrics(NativeMethods.SM_CYSCREEN);
        string exePath = Environment.ProcessPath
            ?? throw new InvalidOperationException("Could not resolve the current executable path.");

        for (int i = 0; i < WindowCount; i++)
        {
            string art = ArtLibrary.RandomPiece()!;
            string tempFile = Path.Combine(Path.GetTempPath(), $"warmup_art_{Guid.NewGuid():N}.txt");
            File.WriteAllText(tempFile, art);

            // Relaunch ourselves in "--show-art" mode rather than a real cmd.exe shell, so the
            // popped-up window has no interactive prompt — nothing typed does anything, and the
            // only way to get rid of it is closing the window.
            var startInfo = new ProcessStartInfo(exePath) { UseShellExecute = true };
            startInfo.ArgumentList.Add("--show-art");
            startInfo.ArgumentList.Add(tempFile);

            var process = Process.Start(startInfo);

            if (process is null)
            {
                continue;
            }

            IntPtr handle = WaitForMainWindowHandle(process);
            if (handle == IntPtr.Zero)
            {
                continue;
            }

            int width = rng.Next(300, 500);
            int height = rng.Next(200, 350);
            int x = rng.Next(0, Math.Max(1, screenWidth - width));
            int y = rng.Next(0, Math.Max(1, screenHeight - height));
            NativeMethods.MoveWindow(handle, x, y, width, height, true);
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
