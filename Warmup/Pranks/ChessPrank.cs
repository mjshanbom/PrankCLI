using System.Text;
using Warmup.Native;

namespace Warmup.Pranks;

// A real (if simplified) two-player chess game played in the console. The prank is that every
// move relaunches the app in "--show-art" mode to pop up a brand new window snapshotting the
// resulting position — old windows are never closed, so the screen slowly fills with a cascade
// of boards as the game goes on.
internal sealed class ChessPrank : IPrank
{
    public string Name => "Chess Chaos";

    private const int WindowWidth = 420;
    private const int WindowHeight = 380;
    private const int CascadeStep = 36;

    public void Run()
    {
        char[,] board = CreateStartingBoard();
        bool whiteToMove = true;
        int moveNumber = 0;
        int windowIndex = 0;

        Console.Clear();
        Console.WriteLine("Chess Chaos - every move you make pops open a new window showing the board.");
        Console.WriteLine("Enter moves like 'e2e4'. Type 'quit' to resign and close the prank.");
        Console.WriteLine();
        Console.Write(BuildBoardText(board, moveNumber: 0, moverIsWhite: true, moveText: null));

        while (true)
        {
            Console.WriteLine();
            Console.Write($"{(whiteToMove ? "White" : "Black")} to move: ");
            string? input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input)
                || input.Equals("quit", StringComparison.OrdinalIgnoreCase)
                || input.Equals("resign", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (!TryParseMove(input, out int fromRow, out int fromCol, out int toRow, out int toCol))
            {
                Console.WriteLine("Couldn't parse that move. Use square notation, e.g. 'e2e4'.");
                continue;
            }

            if (!IsLegalMove(board, fromRow, fromCol, toRow, toCol, whiteToMove))
            {
                Console.WriteLine("Illegal move, try again.");
                continue;
            }

            char captured = board[toRow, toCol];
            ApplyMove(board, fromRow, fromCol, toRow, toCol);
            moveNumber++;

            string boardText = BuildBoardText(board, moveNumber, whiteToMove, input);
            Console.Write(boardText);
            SpawnBoardWindow(boardText, windowIndex++);

            if (char.ToUpperInvariant(captured) == 'K')
            {
                Console.WriteLine();
                Console.WriteLine($"{(whiteToMove ? "White" : "Black")} wins by capturing the king!");
                break;
            }

            whiteToMove = !whiteToMove;
        }
    }

    // Cascades each new board window down-and-right from the last so the trail of moves stays
    // readable instead of stacking exactly on top of itself. See ArtWindowLauncher for the
    // process-spawn/reposition mechanics shared with AsciiBombPrank.
    private static void SpawnBoardWindow(string boardText, int windowIndex)
    {
        int screenWidth = NativeMethods.GetSystemMetrics(NativeMethods.SM_CXSCREEN);
        int screenHeight = NativeMethods.GetSystemMetrics(NativeMethods.SM_CYSCREEN);
        int columns = Math.Max(1, (screenWidth - WindowWidth) / CascadeStep);
        int rows = Math.Max(1, (screenHeight - WindowHeight) / CascadeStep);
        int cell = windowIndex % (columns * rows);
        int x = (cell % columns) * CascadeStep;
        int y = (cell / columns) * CascadeStep;

        ArtWindowLauncher.Spawn(boardText, x, y, WindowWidth, WindowHeight);
    }

    private static char[,] CreateStartingBoard()
    {
        var board = new char[8, 8];
        string[] backRank = ["r", "n", "b", "q", "k", "b", "n", "r"];

        for (int col = 0; col < 8; col++)
        {
            board[0, col] = backRank[col][0];
            board[1, col] = 'p';
            board[6, col] = 'P';
            board[7, col] = char.ToUpperInvariant(backRank[col][0]);
            for (int row = 2; row < 6; row++)
            {
                board[row, col] = '.';
            }
        }

        return board;
    }

    private static bool TryParseMove(string input, out int fromRow, out int fromCol, out int toRow, out int toCol)
    {
        fromRow = fromCol = toRow = toCol = -1;
        string compact = input.Replace(" ", "").Replace("-", "").ToLowerInvariant();

        if (compact.Length != 4)
        {
            return false;
        }

        return TrySquareToRowCol(compact[..2], out fromRow, out fromCol)
            && TrySquareToRowCol(compact[2..], out toRow, out toCol);
    }

    private static bool TrySquareToRowCol(string square, out int row, out int col)
    {
        row = col = -1;
        if (square.Length != 2 || square[0] < 'a' || square[0] > 'h' || square[1] < '1' || square[1] > '8')
        {
            return false;
        }

        col = square[0] - 'a';
        row = 8 - (square[1] - '0');
        return true;
    }

    private static bool IsLegalMove(char[,] board, int fromRow, int fromCol, int toRow, int toCol, bool whiteToMove)
    {
        if (fromRow == toRow && fromCol == toCol)
        {
            return false;
        }

        char piece = board[fromRow, fromCol];
        if (piece == '.')
        {
            return false;
        }

        bool pieceIsWhite = char.IsUpper(piece);
        if (pieceIsWhite != whiteToMove)
        {
            return false;
        }

        char target = board[toRow, toCol];
        if (target != '.' && char.IsUpper(target) == pieceIsWhite)
        {
            return false;
        }

        int rowDelta = toRow - fromRow;
        int colDelta = toCol - fromCol;

        return char.ToUpperInvariant(piece) switch
        {
            'P' => IsLegalPawnMove(board, fromRow, fromCol, toRow, toCol, pieceIsWhite, target),
            'N' => (Math.Abs(rowDelta), Math.Abs(colDelta)) is (1, 2) or (2, 1),
            'B' => Math.Abs(rowDelta) == Math.Abs(colDelta) && IsPathClear(board, fromRow, fromCol, toRow, toCol),
            'R' => (rowDelta == 0 || colDelta == 0) && IsPathClear(board, fromRow, fromCol, toRow, toCol),
            'Q' => (rowDelta == 0 || colDelta == 0 || Math.Abs(rowDelta) == Math.Abs(colDelta))
                && IsPathClear(board, fromRow, fromCol, toRow, toCol),
            'K' => Math.Abs(rowDelta) <= 1 && Math.Abs(colDelta) <= 1,
            _ => false,
        };
    }

    private static bool IsLegalPawnMove(char[,] board, int fromRow, int fromCol, int toRow, int toCol, bool pieceIsWhite, char target)
    {
        int direction = pieceIsWhite ? -1 : 1;
        int startRow = pieceIsWhite ? 6 : 1;
        int rowDelta = toRow - fromRow;
        int colDelta = toCol - fromCol;

        if (colDelta == 0 && target == '.')
        {
            if (rowDelta == direction)
            {
                return true;
            }

            return rowDelta == 2 * direction && fromRow == startRow && board[fromRow + direction, fromCol] == '.';
        }

        return Math.Abs(colDelta) == 1 && rowDelta == direction && target != '.';
    }

    private static bool IsPathClear(char[,] board, int fromRow, int fromCol, int toRow, int toCol)
    {
        int rowStep = Math.Sign(toRow - fromRow);
        int colStep = Math.Sign(toCol - fromCol);
        int row = fromRow + rowStep;
        int col = fromCol + colStep;

        while (row != toRow || col != toCol)
        {
            if (board[row, col] != '.')
            {
                return false;
            }
            row += rowStep;
            col += colStep;
        }

        return true;
    }

    private static void ApplyMove(char[,] board, int fromRow, int fromCol, int toRow, int toCol)
    {
        char piece = board[fromRow, fromCol];

        // Auto-queen; under-promotion isn't worth the extra input parsing for a prank minigame.
        if (char.ToUpperInvariant(piece) == 'P' && (toRow == 0 || toRow == 7))
        {
            piece = char.IsUpper(piece) ? 'Q' : 'q';
        }

        board[toRow, toCol] = piece;
        board[fromRow, fromCol] = '.';
    }

    private static string BuildBoardText(char[,] board, int moveNumber, bool moverIsWhite, string? moveText)
    {
        var sb = new StringBuilder();
        sb.AppendLine("  a b c d e f g h");

        for (int row = 0; row < 8; row++)
        {
            int rank = 8 - row;
            sb.Append(rank).Append(' ');
            for (int col = 0; col < 8; col++)
            {
                sb.Append(board[row, col]).Append(' ');
            }
            sb.Append(rank).AppendLine();
        }

        sb.AppendLine("  a b c d e f g h");
        sb.AppendLine();

        sb.AppendLine(moveText is null
            ? "Chess Chaos - starting position"
            : $"Move {moveNumber}: {(moverIsWhite ? "White" : "Black")} played {moveText}");

        return sb.ToString();
    }
}
