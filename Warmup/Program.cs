using Warmup.Pranks;
using Warmup.Ui;

if (args.Length >= 2 && args[0] == "--show-art")
{
    ArtWindow.Show(args[1]);
    return;
}

var pranks = new IPrank[]
{
    new AsciiBombPrank(),
    new ToastSpamPrank(),
    new InputChaosPrank(),
    new PopupStormPrank(),
};

var options = pranks.Select(p => p.Name).Append("All").Append("Exit").ToArray();

while (true)
{
    Console.Clear();
    int choice = ArrowMenu.Show("Code Jam Prank Tool - pick your chaos (Up/Down, Enter, Esc to exit)", options);

    if (choice == -1 || options[choice] == "Exit")
    {
        break;
    }

    if (options[choice] == "All")
    {
        pranks[0].Run();
        var background = Task.WhenAll(
            Task.Run(() => pranks[1].Run()),
            Task.Run(() => pranks[3].Run()));
        pranks[2].Run();
        background.Wait();
    }
    else
    {
        pranks[choice].Run();
    }

    Console.WriteLine();
    Console.WriteLine("Done. Press any key to return to the menu...");
    Console.ReadKey(intercept: true);
}
