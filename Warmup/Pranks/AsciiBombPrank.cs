using Warmup.Assets;
using Warmup.Native;

namespace Warmup.Pranks;

internal sealed class AsciiBombPrank : IPrank
{
    public string Name => "ASCII Bomb";

    private const int WindowCount = 5;

    public void Run()
    {
        if (!ArtLibrary.HasAny)
        {
            Console.WriteLine("No photos found in Assets/Images — drop some .png/.jpg files in there and rebuild.");
            return;
        }

        var rng = new Random();
        int screenWidth = NativeMethods.GetSystemMetrics(NativeMethods.SM_CXSCREEN);
        int screenHeight = NativeMethods.GetSystemMetrics(NativeMethods.SM_CYSCREEN);

        for (int i = 0; i < WindowCount; i++)
        {
            string art = ArtLibrary.RandomPiece()!;
            int width = rng.Next(300, 500);
            int height = rng.Next(200, 350);
            int x = rng.Next(0, Math.Max(1, screenWidth - width));
            int y = rng.Next(0, Math.Max(1, screenHeight - height));

            // Relaunching ourselves in "--show-art" mode (rather than a real cmd.exe shell) means
            // the popped-up window has no interactive prompt — nothing typed does anything, and the
            // only way to get rid of it is closing the window. See ArtWindowLauncher for the
            // process-spawn/reposition mechanics shared with ChessPrank.
            ArtWindowLauncher.Spawn(art, x, y, width, height);
        }
    }
}
