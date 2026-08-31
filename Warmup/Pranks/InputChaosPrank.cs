using System.Diagnostics;
using Warmup.Native;

namespace Warmup.Pranks;

// We see that WaitForMainWindowHandle is copy pasted in ChessPrank.cs, AsciiBombPrank.cs and InputChaos.cs
// We can keep the code DRY-er by creating a utility class and making this a static method, able to be used across the codebase.
// We can also move our Message string from the bee movie to assets, this way we keep our codebase consistant by using the 
// same mechanism as we do in AsciiBombPrank.

internal sealed class InputChaosPrank : IPrank
{
    public string Name => "Input Chaos";

    private const string Message = "According to all known laws \n\nof aviation, \n\n\nthere is no way a bee \nshould be able to fly. \n\n\nIts wings are too small to get \nits fat little body off the ground. \n\n\nThe bee, of course, flies anyway \n\n\nbecause bees don't care \n\nwhat humans think is impossible. \nBARRY BENSON: \n\n(Barry is picking out a shirt) \nYellow, black. Yellow, black. \nYellow, black. Yellow, black. \n\n\nOoh, black and yellow! \nLet's shake it up a little. \nJANET BENSON: \n\nBarry! Breakfast is ready! \nBARRY: \n\nComing! \n\n\nHang on a second. \n(Barry uses his antenna like a phone) \n\n\nHello? \nADAM FLAYMAN: \n\n\n(Through phone) \n\n- Barry? \n\nBARRY: \n\n- Adam? \n\nADAM: \n\n- Can you believe this is happening? \nBARRY: \n\n- | can't. I'll pick you up. \n\n(Barry flies down the stairs) ";

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
