namespace Warmup.Assets;

// We might want to note what this class does, especially because of all the magic numbers we use, it may be unclear to some users
// to the functionality and use case of the class

internal static class Ansi
{
    public const string Reset = "\x1b[0m";

    public static string TrueColor(int r, int g, int b) => $"\x1b[38;2;{r};{g};{b}m";
}
