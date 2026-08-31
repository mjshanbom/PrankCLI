using Warmup.Native;

namespace Warmup.Pranks;

//Nice pattern overall — the recursive increment/decrement logic is solid. One small thing: in the initial seeding loop,
//we increment ActiveCount with a plain ++ while every other update uses Interlocked.
//Since threads can start running before the loop finishes, we might get a lost update or an early AllDone signal.
//It hasn't shown up in practice because MessageBoxW blocks on a someone closing it,
//but we'd be safer setting state.ActiveCount = InitialCount; once before starting any threads, then just .Start() them in the loop — that removes the race entirely.

internal sealed class PopupStormPrank : IPrank
{
    public string Name => "Popup Storm";

    private const int InitialCount = 40;
    private const int ChildrenPerClose = 2;

    // Hard cap so the hydra effect can't spiral into something that outlives your patience
    // (or requires Task Manager to end) — annoying, not unrecoverable.
    private const int MaxTotalPopups = 300;

    private static readonly (string Title, string Message)[] Popups =
    [
        ("Error", "Your coffee levels are critically low."),
        ("System Alert", "Keyboard has detected excessive typing skill."),
        ("Notice", "You have been visually pranked."),
        ("Reminder", "This message will not self-destruct. You must close it yourself."),
        ("Warning", "Say hi to a coworker today. This is mandatory."),
        ("Critical", "1 of 1 pranks successfully deployed."),
        ("Uh oh", "Closing this window was a mistake."),
        ("Surprise", "Two more just like this one are on the way."),
    ];

    public void Run()
    {
        var state = new SpawnState { TotalSpawned = InitialCount };

        for (int i = 0; i < InitialCount; i++)
        {
            state.ActiveCount++;
            new Thread(() => SpawnPopup(state)).Start();
        }

        state.AllDone.Wait();
    }

    private static void SpawnPopup(SpawnState state)
    {
        int screenWidth = NativeMethods.GetSystemMetrics(NativeMethods.SM_CXSCREEN);
        int screenHeight = NativeMethods.GetSystemMetrics(NativeMethods.SM_CYSCREEN);
        int x = Random.Shared.Next(0, Math.Max(1, screenWidth - 400));
        int y = Random.Shared.Next(0, Math.Max(1, screenHeight - 200));
        var (title, message) = Popups[Random.Shared.Next(Popups.Length)];

        ShowMessageBoxAt(x, y, title, message);

        int spawnCount;
        lock (state.Lock)
        {
            spawnCount = Math.Clamp(MaxTotalPopups - state.TotalSpawned, 0, ChildrenPerClose);
            state.TotalSpawned += spawnCount;
        }

        for (int i = 0; i < spawnCount; i++)
        {
            Interlocked.Increment(ref state.ActiveCount);
            new Thread(() => SpawnPopup(state)).Start();
        }

        if (Interlocked.Decrement(ref state.ActiveCount) == 0)
        {
            state.AllDone.Set();
        }
    }

    // MessageBox has no built-in position parameter, so a thread-scoped CBT hook
    // intercepts the window right as it activates and moves it before it's shown.
    private static void ShowMessageBoxAt(int x, int y, string title, string message)
    {
        IntPtr hookHandle = IntPtr.Zero;
        NativeMethods.CbtHookProc hookProc = (nCode, wParam, lParam) =>
        {
            if (nCode == NativeMethods.HCBT_ACTIVATE)
            {
                NativeMethods.SetWindowPos(wParam, IntPtr.Zero, x, y, 0, 0, NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOZORDER);
            }
            return NativeMethods.CallNextHookEx(hookHandle, nCode, wParam, lParam);
        };

        hookHandle = NativeMethods.SetWindowsHookEx(NativeMethods.WH_CBT, hookProc, IntPtr.Zero, NativeMethods.GetCurrentThreadId());

        try
        {
            NativeMethods.MessageBoxW(
                IntPtr.Zero,
                message,
                title,
                NativeMethods.MB_OK | NativeMethods.MB_ICONINFORMATION | NativeMethods.MB_SETFOREGROUND | NativeMethods.MB_TOPMOST);
        }
        finally
        {
            if (hookHandle != IntPtr.Zero)
            {
                NativeMethods.UnhookWindowsHookEx(hookHandle);
            }
        }
    }

    private sealed class SpawnState
    {
        public readonly object Lock = new();
        public readonly ManualResetEventSlim AllDone = new(false);
        public int TotalSpawned;
        public int ActiveCount;
    }
}
