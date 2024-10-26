using System;
using Rendzu.Core;

namespace Rendzu;


class Program
{
    static void Main( string[] args )
    {
        var playerColor = args.Length > 0 && args[0] == "--white" ? Stone.White : Stone.Black;
        const int boardSize = 15;
        var game = RendzuGame.New( boardSize );

        game.Board.PutStone( 0, 0, Stone.Black );
        game.Board.PutStone( 14, 14, Stone.Black );
        game.Board.PutStone( 2, 3, Stone.Black );
        game.Board.PutStone( 0, 5, Stone.White );
        game.Board.PutStone( 14, 0, Stone.White );

        Console.ForegroundColor = ConsoleColor.DarkGray;
        const char barChar = '\u2500'; // ─
        for ( var row = 0; row < boardSize; row++ )
        {
            for ( var col = 0; col < boardSize; col++ )
            {
                switch ( game.Board[col, row].Stone )
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
                            0 when row == boardSize - 1 => '\u2514', // └
                            0 => '\u251c', // ├
                            boardSize - 1 when row == 0 => '\u2510', // ┐
                            boardSize - 1 when row == boardSize - 1 => '\u2518', // ┘
                            boardSize - 1 => '\u2524', // ┤
                            _ when row == 0 => '\u252c', // ┬
                            _ when row == boardSize - 1 => '\u2534', // ┴
                            _ => '\u253c' // ┼
                        };
                        Console.Write( crossChar );
                        break;
                }
                Console.ForegroundColor = ConsoleColor.DarkGray;
                if ( col < boardSize - 1 ) Console.Write( barChar );
            }
            Console.WriteLine();
        }


        Console.ResetColor();
    }
}