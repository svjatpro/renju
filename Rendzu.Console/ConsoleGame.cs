using Rendzu.Core;
using Rendzu.Core.Extensions;

namespace Rendzu;

public class ConsoleGame( 
    Stone playerColor,
    int boardSize = 15,
    int boardStartCol = 1,
    int boardStartRow = 3,
    int messageRow = 1 )
{
    private Stone PcStone => playerColor.Opposite();
    private RendzuGame Game = RendzuGame.New( boardSize );

    private void ShowBoard( IBoard board )
    {
        Console.SetCursorPosition( 0, 0 );

        for ( var i = 0; i < boardStartRow; i++ )
            Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.DarkGray;
        const char barChar = '\u2500'; // ─
        for ( var row = 0; row < board.Size; row++ )
        {
            for ( var i = 0; i < boardStartCol; i++ )
                Console.Write( ' ' );

            for ( var col = 0; col < board.Size; col++ )
            {
                ShowCell( board, (col, row), false );
                Console.ForegroundColor = ConsoleColor.DarkGray;
                if ( col < board.Size - 1 ) Console.Write( barChar );
            }
            Console.WriteLine();
        }
    }
    private void ShowCell( IBoard board, (int col, int row) cell, bool locateCursor = true )
    {
        // save cursor position
        (int x, int y) current = (Console.CursorLeft, Console.CursorTop);

        if ( locateCursor )
            Console.SetCursorPosition( boardStartCol + cell.col * 2, boardStartRow + cell.row );
        
        switch ( board[cell.col, cell.row].Stone )
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
                var crossChar = cell.col switch
                {
                    0 when cell.row == 0 => '\u250c', // ┌
                    0 when cell.row == board.Size - 1 => '\u2514', // └
                    0 => '\u251c', // ├
                    _ when cell.col == board.Size - 1 && cell.row == 0 => '\u2510', // ┐
                    _ when cell.col == board.Size - 1 && cell.row == board.Size - 1 => '\u2518', // ┘
                    _ when cell.col == board.Size - 1 => '\u2524', // ┤
                    _ when cell.row == 0 => '\u252c', // ┬
                    _ when cell.row == board.Size - 1 => '\u2534', // ┴
                    _ => '\u253c' // ┼
                };
                Console.Write( crossChar );
                break;
        }

        if ( locateCursor )
            Console.SetCursorPosition( current.x, current.y );
    }

    private void ShowMessage( string message, ConsoleColor color )
    {
        // save cursor position
        (int x, int y) current = (Console.CursorLeft, Console.CursorTop);

        Console.SetCursorPosition( 1, messageRow );
        Console.ForegroundColor = color;
        Console.Write( message?.PadRight( Console.WindowWidth, ' ' ) );

        Console.ResetColor();

        // restore cursor position
        Console.SetCursorPosition( current.x, current.y );
    }

    private bool TryToMove( RendzuGame game, (int col, int row) cell, Stone stone )
    {
        // check if move is allowed
        if ( !game.Referee.MoveAllowed( cell.col, cell.row, stone, out var message ) )
        {
            ShowMessage( message!, ConsoleColor.Red );
            return false;
        }
        
        // clear message line
        ShowMessage( " ", ConsoleColor.DarkGray );

        // put stone
        game.Board.PutStone( cell.col, cell.row, stone );

        // show new stone
        ShowCell( game.Board, cell );

        // check if game is over
        if ( game.Referee.IsGameOver )
            ShowMessage( "Game over!", ConsoleColor.Blue );

        return true;
    }

    public void Run(  )
    {
        ShowBoard( Game.Board );

        Console.ResetColor();
        Console.SetCursorPosition( boardStartCol, boardStartRow );
        ConsoleKeyInfo key = default;
        while ( key.Key != ConsoleKey.Escape )
        {
            key = Console.ReadKey( true );

            switch ( key.Key )
            {
                case ConsoleKey.UpArrow:
                    if ( Console.CursorTop - boardStartRow > 0 )
                        Console.SetCursorPosition( Console.CursorLeft, Console.CursorTop - 1 );
                    break;
                case ConsoleKey.DownArrow:
                    if ( Console.CursorTop - boardStartRow < boardSize - 1 )
                        Console.SetCursorPosition( Console.CursorLeft, Console.CursorTop + 1 );
                    break;
                case ConsoleKey.LeftArrow:
                    if ( Console.CursorLeft - boardStartCol > 0 )
                        Console.SetCursorPosition( Console.CursorLeft - 2, Console.CursorTop );
                    break;
                case ConsoleKey.RightArrow:
                    if ( Console.CursorLeft - boardStartCol < boardSize * 2 - 2 )
                        Console.SetCursorPosition( Console.CursorLeft + 2, Console.CursorTop );
                    break;
                case ConsoleKey.Spacebar:
                    // put player's stone
                    if ( !TryToMove( Game, (Console.CursorLeft / 2, Console.CursorTop - boardStartRow), playerColor ) )
                        break;

                    // put PC's stone
                    if ( Game.Referee.IsGameOver ) break;
                    
                    var rnd = new Random();
                    do { }
                    while ( !TryToMove( Game, (rnd.Next( boardSize ), rnd.Next( boardSize )), PcStone ) );

                    break;
            }
        }

        Console.ResetColor();
        Console.SetCursorPosition( 0, boardStartRow + boardSize );
        Console.WriteLine();
    }
}