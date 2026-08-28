using System.Diagnostics;
using Warmup.Native;

namespace Warmup.Pranks;

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

        IntPtr handle = NativeMethods.WaitForMainWindowHandle(process);
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
}
