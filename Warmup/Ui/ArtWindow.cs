namespace Warmup.Ui;

/// <summary>
/// Entry point for the "--show-art" child-process mode: displays the given art file and then
/// blocks forever. There's no active read loop, so nothing typed does anything — the only way
/// to get rid of the window is to close it (the X button still works, since that's handled by
/// the OS regardless of what the process is doing).
/// </summary>
internal static class ArtWindow
{
    public static void Show(string artFilePath)
    {
        Console.Title = "Warmup Prank";
        Console.CursorVisible = false;
        Console.Write(File.ReadAllText(artFilePath));

        Thread.Sleep(Timeout.Infinite);
    }
}
