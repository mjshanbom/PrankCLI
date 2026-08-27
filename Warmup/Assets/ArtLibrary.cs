namespace Warmup.Assets;

/// <summary>Loads ASCII art from the user-supplied images dropped in Assets/Images.</summary>
internal static class ArtLibrary
{
    private static readonly string[] ImageExtensions = [".png", ".jpg", ".jpeg", ".bmp", ".gif"];

    private static readonly Lazy<List<string>> Pool = new(BuildPool);

    public static bool HasAny => Pool.Value.Count > 0;

    /// <summary>Returns a randomly chosen user-supplied image (converted to ASCII art), or null if none were found.</summary>
    public static string? RandomPiece()
    {
        var pool = Pool.Value;
        return pool.Count == 0 ? null : pool[Random.Shared.Next(pool.Count)];
    }

    private static List<string> BuildPool()
    {
        var pool = new List<string>();

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
