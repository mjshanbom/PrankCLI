namespace Warmup.Assets;

internal static class Ansi
{
    public const string Reset = "\x1b[0m";

    public static string TrueColor(int r, int g, int b) => $"\x1b[38;2;{r};{g};{b}m";
}
