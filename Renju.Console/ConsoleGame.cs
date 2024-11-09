using Renju.Core;
using Renju.Core.Extensions;
using Renju.Core.RenjuGame;

namespace Renju.Console;

public class ConsoleGame( 
    Stone playerColor,
    int boardSize = 15,
    int boardStartCol = 1,
    int boardStartRow = 4,
    int statusRow = 2,
    int messageRow = 3 )
{
    private Stone PcColor => playerColor.Opposite();
    private RenjuGame Game = null!;
    private bool BreakTheGame;

    private void ShowBoard( IBoard board )
    {
        System.Console.SetCursorPosition( 0, 0 );

        for ( var i = 0; i < boardStartRow; i++ )
            System.Console.WriteLine();

        System.Console.ForegroundColor = ConsoleColor.DarkGray;
        const char barChar = '\u2500'; // ─
        for ( var row = 0; row < board.Size; row++ )
        {
            for ( var i = 0; i < boardStartCol; i++ )
                System.Console.Write( ' ' );

            for ( var col = 0; col < board.Size; col++ )
            {
                ShowCell( board, (col, row), false );
                System.Console.ForegroundColor = ConsoleColor.DarkGray;
                if ( col < board.Size - 1 ) System.Console.Write( barChar );
            }
            System.Console.WriteLine();
        }
    }
    private void ShowCell( IBoard board, (int col, int row) cell, bool locateCursor = true )
    {
        // save cursor position
        (int x, int y) current = (System.Console.CursorLeft, System.Console.CursorTop);

        if ( locateCursor )
            System.Console.SetCursorPosition( boardStartCol + cell.col * 2, boardStartRow + cell.row );
        
        switch ( board[cell.col, cell.row].Stone )
        {
            case Stone.Black:
                System.Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.Write( 'x' );
                break;
            case Stone.White:
                System.Console.ForegroundColor = ConsoleColor.White;
                System.Console.Write( 'o' );
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
                System.Console.Write( crossChar );
                break;
        }

        if ( locateCursor )
            System.Console.SetCursorPosition( current.x, current.y );
    }

    private void ShowMessage( string message, ConsoleColor color, int? row = null )
    {
        // save cursor position
        (int x, int y) current = (System.Console.CursorLeft, System.Console.CursorTop);

        System.Console.SetCursorPosition( 1, row ?? messageRow );
        System.Console.ForegroundColor = color;
        System.Console.Write( message.PadRight( System.Console.WindowWidth, ' ' ) );

        System.Console.ResetColor();

        // restore cursor position
        System.Console.SetCursorPosition( current.x, current.y );
    }

    private void ProcessPressedKey( ConsoleKeyInfo key )
    {
        switch ( key.Key )
        {
            case ConsoleKey.Escape:
                BreakTheGame = true;
                return;
            // todo: add other keys
        }
    }
    private void Initialize()
    {
        // prepare console
        System.Console.Clear();
        System.Console.WriteLine( " Renju v3.0; Sviatoslav Prokipets (c)" );
        System.Console.WriteLine();

        // initialize players
        var (pc, human) = (
            Player.PcPlayer(),
            Player.ConsoleHuman( boardStartCol, boardStartRow, ( _, key ) => ProcessPressedKey( key ) ));

        // initialize game
        Game = new RenjuGame( boardSize,
            playerColor == Stone.Black ? human : pc,
            playerColor == Stone.White ? human : pc );
        
        // initialize referee
        Game.Referee.GameOver += ( _, winner ) =>
        {
            if ( winner == playerColor )
                ShowMessage( "You win!", ConsoleColor.Green, statusRow );
            else if ( winner == PcColor )
                ShowMessage( "You lose!", ConsoleColor.Red, statusRow );
            else
                ShowMessage( "Draw!", ConsoleColor.Yellow, statusRow );
        };
        Game.Referee.ForbiddenMove += ( _, message ) => ShowMessage( message, ConsoleColor.Red );

        // Initialize board
        Game.Board.StoneMoved += ( _, move ) =>
        {
            ShowMessage( " ", ConsoleColor.DarkGray, messageRow ); // clear message line
            ShowCell( Game.Board, (move.Col, move.Row) );
        };

        // show board
        ShowBoard( Game.Board );

        System.Console.ResetColor();
        System.Console.SetCursorPosition( boardStartCol, boardStartRow );
    }
    
    public void Run()
    {
        Initialize();

        while ( true )
        {
            if ( Game.Referee.IsGameOver )
            {
                ProcessPressedKey( System.Console.ReadKey( true ) );
            }
            else
            {
                var player = Game.CurrentPlayer;
                ShowMessage( 
                    $"{player.Name} ({player.Stone.ToString()})'s move ...",
                    Game.CurrentPlayer.Stone switch
                    {
                        Stone.Black => ConsoleColor.DarkGray,
                        Stone.White => ConsoleColor.White,
                        _ => ConsoleColor.Yellow
                    }, 
                    statusRow );
                Game.TryProceedMove();
            }

            if ( BreakTheGame )
            {
                System.Console.ResetColor();
                System.Console.SetCursorPosition( 0, boardStartRow + boardSize );
                System.Console.WriteLine(); 

                return;
            }
        }
    }
}