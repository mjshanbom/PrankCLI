using System.Drawing;
using System.Text;

namespace Warmup.Assets;

/// <summary>Converts an image file into full-color ASCII art using ANSI 24-bit color escape codes per character.</summary>
internal static class ImageAsciiConverter
{
    private const string Ramp = " .:-=+*#%@";

    public static string Convert(string imagePath, int targetWidth = 70)
    {
        using var original = new Bitmap(imagePath);

        // Console character cells are roughly twice as tall as wide, so halve the height
        // to keep the converted art from looking vertically stretched.
        int targetHeight = Math.Max(1, (int)(original.Height * (targetWidth / (double)original.Width) * 0.5));

        using var resized = new Bitmap(original, new Size(targetWidth, targetHeight));

        var sb = new StringBuilder();
        for (int y = 0; y < resized.Height; y++)
        {
            for (int x = 0; x < resized.Width; x++)
            {
                Color pixel = resized.GetPixel(x, y);
                double brightness = ((pixel.R * 0.299) + (pixel.G * 0.587) + (pixel.B * 0.114)) / 255.0;
                char c = Ramp[(int)Math.Min(Ramp.Length - 1, brightness * Ramp.Length)];
                sb.Append(Ansi.TrueColor(pixel.R, pixel.G, pixel.B)).Append(c);
            }
            sb.Append(Ansi.Reset).Append('\n');
        }
        return sb.ToString();
    }
}
