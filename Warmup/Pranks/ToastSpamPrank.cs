using Microsoft.Toolkit.Uwp.Notifications;

namespace Warmup.Pranks;

internal sealed class ToastSpamPrank : IPrank
{
    public string Name => "Toast Spam";

    private const int ToastCount = 8;
    private static readonly TimeSpan Delay = TimeSpan.FromMilliseconds(450);

    private static readonly string[] Messages =
    [
        "Your computer is now 12% more haunted.",
        "System update: everything is fine, probably.",
        "This is not a virus. Definitely not.",
        "Achievement unlocked: Mildly Annoyed.",
        "Someone is definitely pranking you right now.",
        "42 new notifications about nothing.",
        "Please do not feed the toast.",
        "Beep boop, you've been pranked.",
    ];

    public void Run()
    {
        for (int i = 0; i < ToastCount; i++)
        {
            new ToastContentBuilder()
                .AddText("Warmup Prank")
                .AddText(Messages[i % Messages.Length])
                .Show();

            Thread.Sleep(Delay);
        }
    }
}
