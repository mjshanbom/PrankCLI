using System.Text;

namespace Warmup.Assets;

internal static class Ansi
{
    public const string Reset = "\x1b[0m";

    private static readonly string[] Palette =
    [
        "\x1b[91m", // bright red
        "\x1b[93m", // bright yellow
        "\x1b[92m", // bright green
        "\x1b[96m", // bright cyan
        "\x1b[94m", // bright blue
        "\x1b[95m", // bright magenta
    ];

    public static string TrueColor(int r, int g, int b) => $"\x1b[38;2;{r};{g};{b}m";

    /// <summary>Colors each line of the given text with a different palette color, cycling through it.</summary>
    public static string Rainbow(string text)
    {
        string[] lines = text.Replace("\r\n", "\n").Split('\n');
        var sb = new StringBuilder();
        for (int i = 0; i < lines.Length; i++)
        {
            sb.Append(Palette[i % Palette.Length]).Append(lines[i]).Append(Reset);
            if (i < lines.Length - 1)
            {
                sb.Append('\n');
            }
        }
        return sb.ToString();
    }
}
