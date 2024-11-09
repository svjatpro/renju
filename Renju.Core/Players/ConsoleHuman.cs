namespace Renju.Core.Players;

internal class ConsoleHuman(int boardStartCol, int boardStartRow, string name ) : Player( name )
{
    public override bool TryProceedMove(out Move move)
    {
        move = default!;
        while (true)
        {
            var key = Console.ReadKey(true);
            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                    if (Console.CursorTop - boardStartRow > 0)
                        Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop - 1);
                    break;
                case ConsoleKey.DownArrow:
                    if (Console.CursorTop - boardStartRow < Board.Size - 1)
                        Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop + 1);
                    break;
                case ConsoleKey.LeftArrow:
                    if (Console.CursorLeft - boardStartCol > 0)
                        Console.SetCursorPosition(Console.CursorLeft - 2, Console.CursorTop);
                    break;
                case ConsoleKey.RightArrow:
                    if (Console.CursorLeft - boardStartCol < Board.Size * 2 - 2)
                        Console.SetCursorPosition(Console.CursorLeft + 2, Console.CursorTop);
                    break;
                case ConsoleKey.Spacebar:
                    // put player's stone
                    move = new Move(Console.CursorLeft / 2, Console.CursorTop - boardStartRow, Stone);
                    return true;
                default:
                    OtherKeyPressed?.Invoke(this, key);
                    return false;
            }
        }
    }

    public event EventHandler<ConsoleKeyInfo>? OtherKeyPressed;
}