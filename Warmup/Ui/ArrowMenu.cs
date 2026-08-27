namespace Warmup.Ui;

internal static class ArrowMenu
{
    /// <summary>Shows an arrow-key selectable menu. Returns the selected index, or -1 if the user pressed Escape.</summary>
    public static int Show(string title, string[] options)
    {
        int selected = 0;
        Console.CursorVisible = false;

        Console.WriteLine(title);
        Console.WriteLine();
        int menuTop = Console.CursorTop;

        Render(options, selected, menuTop);

        while (true)
        {
            var key = Console.ReadKey(intercept: true).Key;
            switch (key)
            {
                case ConsoleKey.UpArrow:
                    selected = (selected - 1 + options.Length) % options.Length;
                    Render(options, selected, menuTop);
                    break;
                case ConsoleKey.DownArrow:
                    selected = (selected + 1) % options.Length;
                    Render(options, selected, menuTop);
                    break;
                case ConsoleKey.Enter:
                    Console.CursorVisible = true;
                    return selected;
                case ConsoleKey.Escape:
                    Console.CursorVisible = true;
                    return -1;
            }
        }
    }

    private static void Render(string[] options, int selected, int top)
    {
        Console.SetCursorPosition(0, top);
        for (int i = 0; i < options.Length; i++)
        {
            if (i == selected)
            {
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.WriteLine($" > {options[i],-30}");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine($"   {options[i],-30}");
            }
        }
    }
}
