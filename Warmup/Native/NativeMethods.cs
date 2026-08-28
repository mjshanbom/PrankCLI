using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Warmup.Native;

[StructLayout(LayoutKind.Sequential)]
internal struct MOUSEINPUT
{
    public int dx;
    public int dy;
    public uint mouseData;
    public uint dwFlags;
    public uint time;
    public IntPtr dwExtraInfo;
}

[StructLayout(LayoutKind.Sequential)]
internal struct KEYBDINPUT
{
    public ushort wVk;
    public ushort wScan;
    public uint dwFlags;
    public uint time;
    public IntPtr dwExtraInfo;
}

[StructLayout(LayoutKind.Explicit)]
internal struct InputUnion
{
    [FieldOffset(0)] public MOUSEINPUT mi;
    [FieldOffset(0)] public KEYBDINPUT ki;
}

[StructLayout(LayoutKind.Sequential)]
internal struct INPUT
{
    public int type;
    public InputUnion U;
}

internal static class NativeMethods
{
    public const int INPUT_MOUSE = 0;
    public const int INPUT_KEYBOARD = 1;

    public const uint MOUSEEVENTF_MOVE = 0x0001;

    public const uint KEYEVENTF_UNICODE = 0x0004;
    public const uint KEYEVENTF_KEYUP = 0x0002;

    public const int SM_CXSCREEN = 0;
    public const int SM_CYSCREEN = 1;

    public const uint MB_OK = 0x00000000;
    public const uint MB_ICONINFORMATION = 0x00000040;
    public const uint MB_SETFOREGROUND = 0x00010000;
    public const uint MB_TOPMOST = 0x00040000;

    public const int WH_CBT = 5;
    public const int HCBT_ACTIVATE = 5;

    public const uint SWP_NOSIZE = 0x0001;
    public const uint SWP_NOZORDER = 0x0004;

    public delegate IntPtr CbtHookProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int MessageBoxW(IntPtr hWnd, string text, string caption, uint type);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    [DllImport("user32.dll")]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    public static extern int GetSystemMetrics(int nIndex);

    [DllImport("user32.dll")]
    public static extern IntPtr SetWindowsHookEx(int idHook, CbtHookProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll")]
    public static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll")]
    public static extern uint GetCurrentThreadId();

    public static void SendMouseMove(int dx, int dy)
    {
        var input = new INPUT
        {
            type = INPUT_MOUSE,
            U = new InputUnion
            {
                mi = new MOUSEINPUT
                {
                    dx = dx,
                    dy = dy,
                    dwFlags = MOUSEEVENTF_MOVE,
                }
            }
        };
        SendInput(1, [input], Marshal.SizeOf<INPUT>());
    }

    public static void SendUnicodeChar(char c)
    {
        var down = new INPUT
        {
            type = INPUT_KEYBOARD,
            U = new InputUnion
            {
                ki = new KEYBDINPUT
                {
                    wVk = 0,
                    wScan = c,
                    dwFlags = KEYEVENTF_UNICODE,
                }
            }
        };
        var up = new INPUT
        {
            type = INPUT_KEYBOARD,
            U = new InputUnion
            {
                ki = new KEYBDINPUT
                {
                    wVk = 0,
                    wScan = c,
                    dwFlags = KEYEVENTF_UNICODE | KEYEVENTF_KEYUP,
                }
            }
        };
        SendInput(2, [down, up], Marshal.SizeOf<INPUT>());
    }

    // Moved this function from ChessPrank, InputChaosPrank, and AsciiBombPrank files 
    // in order to reduce duplicated code. 
    public static IntPtr WaitForMainWindowHandle(Process process, int timeoutMs = 2000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
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
