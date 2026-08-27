namespace Warmup.Assets;

/// <summary>Combines the built-in ASCII art with any user-supplied images dropped in Assets/Images.</summary>
internal static class ArtLibrary
{
    private static readonly string[] ImageExtensions = [".png", ".jpg", ".jpeg", ".bmp", ".gif"];

    private static readonly Lazy<(List<string> All, List<string> UserImages)> Pool = new(BuildPool);

    public static string RandomPiece()
    {
        var (all, _) = Pool.Value;
        return all[Random.Shared.Next(all.Count)];
    }

    /// <summary>Returns a randomly chosen user-supplied image (converted to ASCII art), or null if none were found.</summary>
    public static string? RandomUserImage()
    {
        var (_, userImages) = Pool.Value;
        return userImages.Count == 0 ? null : userImages[Random.Shared.Next(userImages.Count)];
    }

    private static (List<string> All, List<string> UserImages) BuildPool()
    {
        var all = new List<string>();
        var userImages = new List<string>();

        foreach (string art in AsciiArt.Pieces)
        {
            all.Add(Ansi.Rainbow(art));
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
                    string converted = ImageAsciiConverter.Convert(file);
                    all.Add(converted);
                    userImages.Add(converted);
                }
                catch (Exception)
                {
                    // Skip images that fail to load/decode rather than crashing the prank.
                }
            }
        }

        return (all, userImages);
    }
}
