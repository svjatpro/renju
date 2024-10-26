using Rendzu.Core;

namespace Rendzu;

internal class Program
{
    private static void ShowBoard( IBoard board )
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        const char barChar = '\u2500'; // ─
        for ( var row = 0; row < board.Size; row++ )
        {
            for ( var col = 0; col < board.Size; col++ )
            {
                ShowCell( board, col, row );
                Console.ForegroundColor = ConsoleColor.DarkGray;
                if ( col < board.Size - 1 ) Console.Write( barChar );
            }
            Console.WriteLine();
        }
    }
    private static void ShowCell( IBoard board, int col, int row )
    {
        switch ( board[col, row].Stone )
        {
            case Stone.Black:
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write( 'x' );
                break;
            case Stone.White:
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write( 'o' );
                break;
            case Stone.None:
                var crossChar = col switch
                {
                    0 when row == 0 => '\u250c', // ┌
                    0 when row == board.Size - 1 => '\u2514', // └
                    0 => '\u251c', // ├
                    _ when col == board.Size - 1 && row == 0 => '\u2510', // ┐
                    _ when col == board.Size - 1 && row == board.Size - 1 => '\u2518', // ┘
                    _ when col == board.Size - 1 => '\u2524', // ┤
                    _ when row == 0 => '\u252c', // ┬
                    _ when row == board.Size - 1 => '\u2534', // ┴
                    _ => '\u253c' // ┼
                };
                Console.Write( crossChar );
                break;
        }
    }
       
    static void Main( string[] args )
    {
        var playerColor = args.Length > 0 && args[0] == "--white" ? Stone.White : Stone.Black;
        const int boardSize = 15;
        var game = RendzuGame.New( boardSize );

        /*game.Board.PutStone( 0, 0, Stone.Black );
        game.Board.PutStone( 14, 14, Stone.Black );
        game.Board.PutStone( 2, 3, Stone.Black );
        game.Board.PutStone( 0, 5, Stone.White );
        game.Board.PutStone( 14, 0, Stone.White );*/

        ShowBoard( game.Board );
        
        Console.ResetColor();
        Console.SetCursorPosition( 0, Console.CursorTop - boardSize );
        ConsoleKeyInfo key = default;
        while( key.Key != ConsoleKey.Escape )
        {
            key = Console.ReadKey( true );

            switch ( key.Key )
            {
                case ConsoleKey.UpArrow:
                    if ( Console.CursorTop > 0 )
                        Console.SetCursorPosition( Console.CursorLeft, Console.CursorTop - 1 );
                    break;
                case ConsoleKey.DownArrow:
                    if ( Console.CursorTop < boardSize - 1 )
                        Console.SetCursorPosition( Console.CursorLeft, Console.CursorTop + 1 );
                    break;
                case ConsoleKey.LeftArrow:
                    if ( Console.CursorLeft > 0 )
                        Console.SetCursorPosition( Console.CursorLeft - 2, Console.CursorTop );
                    break;
                case ConsoleKey.RightArrow:
                    if ( Console.CursorLeft < boardSize * 2 - 2 )
                        Console.SetCursorPosition( Console.CursorLeft + 2, Console.CursorTop );
                    break;
                case ConsoleKey.Spacebar:
                    game.Board.PutStone( Console.CursorLeft / 2, Console.CursorTop, playerColor );
                    ShowCell( game.Board, Console.CursorLeft / 2, Console.CursorTop );
                    Console.SetCursorPosition( Console.CursorLeft - 1, Console.CursorTop );
                    break;
            }
        }
    }
}
