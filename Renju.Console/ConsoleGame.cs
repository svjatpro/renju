using System.Dynamic;
using Renju.Core;
using Renju.Core.Extensions;
using Renju.Core.RenjuGame;

namespace Renju.Console;

public class ConsoleGame( 
    Stone playerColor,
    int boardSize = 15,
    int boardStartCol = 1,
    int boardStartRow = 3,
    int messageRow = 1 )
{
    private Stone PcColor => playerColor.Opposite();
    private RenjuGame Game = RenjuGame.New( boardSize );

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

    private void ShowMessage( string message, ConsoleColor color )
    {
        // save cursor position
        (int x, int y) current = (System.Console.CursorLeft, System.Console.CursorTop);

        System.Console.SetCursorPosition( 1, messageRow );
        System.Console.ForegroundColor = color;
        System.Console.Write( message?.PadRight( System.Console.WindowWidth, ' ' ) );

        System.Console.ResetColor();

        // restore cursor position
        System.Console.SetCursorPosition( current.x, current.y );
    }

    private bool TryToMove( RenjuGame game, (int col, int row) cell, Stone stone )
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
        {
            if( game.Referee.Winner == playerColor )
                ShowMessage( "You win!", ConsoleColor.Green );
            else if ( game.Referee.Winner == PcColor )
                ShowMessage( "You lose!", ConsoleColor.Red );
            else
                ShowMessage( "Draw!", ConsoleColor.Yellow );
            //ShowMessage( "Game over!", ConsoleColor.Blue );
        }

        return true;
    }

    public void Run(  )
    {
        ShowBoard( Game.Board );

        System.Console.ResetColor();
        System.Console.SetCursorPosition( boardStartCol, boardStartRow );
        ConsoleKeyInfo key = default;
        while ( key.Key != ConsoleKey.Escape )
        {
            key = System.Console.ReadKey( true );

            switch ( key.Key )
            {
                case ConsoleKey.UpArrow:
                    if ( System.Console.CursorTop - boardStartRow > 0 )
                        System.Console.SetCursorPosition( System.Console.CursorLeft, System.Console.CursorTop - 1 );
                    break;
                case ConsoleKey.DownArrow:
                    if ( System.Console.CursorTop - boardStartRow < boardSize - 1 )
                        System.Console.SetCursorPosition( System.Console.CursorLeft, System.Console.CursorTop + 1 );
                    break;
                case ConsoleKey.LeftArrow:
                    if ( System.Console.CursorLeft - boardStartCol > 0 )
                        System.Console.SetCursorPosition( System.Console.CursorLeft - 2, System.Console.CursorTop );
                    break;
                case ConsoleKey.RightArrow:
                    if ( System.Console.CursorLeft - boardStartCol < boardSize * 2 - 2 )
                        System.Console.SetCursorPosition( System.Console.CursorLeft + 2, System.Console.CursorTop );
                    break;
                case ConsoleKey.Spacebar:
                    // put player's stone
                    if ( !TryToMove( Game, (System.Console.CursorLeft / 2, System.Console.CursorTop - boardStartRow), playerColor ) )
                        break;

                    // put PC's stone
                    if ( Game.Referee.IsGameOver ) break;
                    
                    // pc player to get a move
                    var rnd = new Random();
                    do { }
                    while ( !TryToMove( Game, (rnd.Next( boardSize ), rnd.Next( boardSize )), PcColor ) );

                    break;
            }
        }

        System.Console.ResetColor();
        System.Console.SetCursorPosition( 0, boardStartRow + boardSize );
        System.Console.WriteLine();
    }
}