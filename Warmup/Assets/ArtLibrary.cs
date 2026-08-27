namespace Warmup.Assets;

/// <summary>Combines the built-in ASCII art with any user-supplied images dropped in Assets/Images.</summary>
internal static class ArtLibrary
{
    private static readonly string[] ImageExtensions = [".png", ".jpg", ".jpeg", ".bmp", ".gif"];

    private static readonly Lazy<List<string>> Pool = new(BuildPool);

    public static string RandomPiece() => Pool.Value[Random.Shared.Next(Pool.Value.Count)];

    private static List<string> BuildPool()
    {
        var pool = new List<string>();

        foreach (string art in AsciiArt.Pieces)
        {
            pool.Add(Ansi.Rainbow(art));
        }

        string imagesDir = Path.Combine(AppContext.BaseDirectory, "Assets", "Images");
        if (Directory.Exists(imagesDir))
        {
            foreach (string file in Directory.GetFiles(imagesDir))
            {
                if (!ImageExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
                {
                    continue;
                }

                try
                {
                    pool.Add(ImageAsciiConverter.Convert(file));
                }
                catch (Exception)
                {
                    // Skip images that fail to load/decode rather than crashing the prank.
                }
            }
        }

        return pool;
    }
}
