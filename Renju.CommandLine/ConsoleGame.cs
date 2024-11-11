using Renju.Core;
using Renju.Core.Extensions;
using Renju.Core.RenjuGame;

namespace Renju.CommandLine;

public class ConsoleGame( Stone playerColor, int boardSize = 15 )
{
    private Stone PcColor => playerColor.Opposite();
    private RenjuGame Game = null!;
    private bool BreakTheGame;
    private GameStyle Style = null!;

    private const int BoardStartCol = 2;
    private const int BoardStartRow = 3;
    private const int StatusRow = 1;
    private const int MessageRow = 2;
    private int InfoStartRow;

    private void WriteInfo()
    {
        Console.SetCursorPosition( BoardStartCol, InfoStartRow );
        var leftSpace = new string( ' ', BoardStartCol );

        // write players' names
        var player1 = Game.Players[Stone.Black].Name;
        Style.PlayerBlack.SetColor();
        Console.Write( $"{player1,BoardStartCol}" );

        var player2 = Game.Players[Stone.White].Name;
        Style.PlayerWhite.SetColor();
        var player2Padding = boardSize * 2 - BoardStartCol - player1.Length - player2.Length + 1;
        Console.Write( $"{player2.PadLeft( player2Padding + player2.Length, ' ' )}" );
        Console.WriteLine();

        // write game help
        Console.WriteLine();
        Style.Help.SetColor();
        Console.WriteLine( $"{leftSpace}esc - exit game" );
        Console.WriteLine( $"{leftSpace}n   - start new game" );
        Console.WriteLine( $"{leftSpace}h   - detailed help" );

        // write game info
        Console.WriteLine( $"{leftSpace}{new string( '-', 5 )}" );
        Style.Info.SetColor();
        Console.WriteLine( $"{leftSpace}Renju v3.0; (c) Sviatoslav Prokipets" );
    }
    private void WriteBoard( IBoard board )
    {
        const char barChar = '\u2500'; // ─
        for ( var row = 0; row < board.Size; row++ )
        {
            Console.SetCursorPosition( BoardStartCol - 1, BoardStartRow + row );
            
            Style.Board.SetColor();
            Console.Write( ' ' ); // border left frame

            for ( var col = 0; col < board.Size; col++ )
            {
                WriteCell( board, (col, row), false );
                Style.Board.SetColor();
                if ( col < board.Size - 1 ) Console.Write( barChar );
            }
            Console.Write( ' ' ); // border right frame
            Console.WriteLine();
        }
    }
    private void WriteCell( IBoard board, (int col, int row) cell, bool locateCursor = true )
    {
        // save cursor position
        (int x, int y) current = (Console.CursorLeft, Console.CursorTop);

        if ( locateCursor )
            Console.SetCursorPosition( BoardStartCol + cell.col * 2, BoardStartRow + cell.row );

        switch ( board[cell.col, cell.row].Stone )
        {
            case Stone.Black:
                Style.StoneBlack.SetColor();
                Console.Write( Style.StoneCharBlack );
                break;
            case Stone.White:
                Style.StoneWhite.SetColor();
                Console.Write( Style.StoneCharWhite );
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

    private void WriteMessage( string message, ColorStyle color, int? row = null )
    {
        // save cursor position
        (int x, int y) current = (Console.CursorLeft, Console.CursorTop);

        Console.SetCursorPosition( BoardStartCol, row ?? MessageRow );
        color.SetColor();
        Console.Write( message.PadRight( Console.WindowWidth, ' ' ) );

        Console.ResetColor();

        // restore cursor position
        Console.SetCursorPosition( current.x, current.y );
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

    private void InitializeStyle()
    {
        // initialize style
        Style = new GameStyle
        {
            Board = new ColorStyle( ConsoleColor.DarkGray, ConsoleColor.Black ),
            Info = new ColorStyle( ConsoleColor.DarkGray, ConsoleColor.Black ),
            Help = new ColorStyle( ConsoleColor.Blue, ConsoleColor.Black ),
            MessageError = new ColorStyle( ConsoleColor.Red, ConsoleColor.Black ),
            PlayerBlack = new ColorStyle( ConsoleColor.DarkGray, ConsoleColor.Black ),
            PlayerWhite = new ColorStyle( ConsoleColor.White, ConsoleColor.Black ),
            MessageWin = new ColorStyle( ConsoleColor.Green, ConsoleColor.Black ),
            MessageLoose = new ColorStyle( ConsoleColor.Yellow, ConsoleColor.Black ),
            MessageDraw = new ColorStyle( ConsoleColor.Cyan, ConsoleColor.Black ),
            StoneBlack = new ColorStyle( ConsoleColor.Yellow, ConsoleColor.Black ),
            StoneWhite = new ColorStyle( ConsoleColor.White, ConsoleColor.Black ),
            StoneCharBlack = 'x',
            StoneCharWhite = 'o'
        };
        //Console.Write( '\u26ab' ); // ⚫
        //Console.Write( '\u25cf' ); // ●
        //Console.Write( '\u26aa' ); // ⚪
        //Console.Write( '\u25cb' ); // ○   
    }

    private void Initialize()
    {
        // initialize players
        var pc = Player.PcPlayer( "Computer" );
        var human = new ConsolePlayer( BoardStartCol, BoardStartRow, "Human" );
        human.OtherKeyPressed += ( _, k ) => ProcessPressedKey( k );

        // initialize game
        Game = new RenjuGame( boardSize,
            playerColor == Stone.Black ? human : pc,
            playerColor == Stone.White ? human : pc );
        
        // initialize referee
        Game.Referee.GameOver += ( _, winner ) =>
        {
            if ( winner == playerColor )
                WriteMessage( "You win!", Style.MessageWin, StatusRow );
            else if ( winner == PcColor )
                WriteMessage( "You lose!", Style.MessageLoose, StatusRow );
            else
                WriteMessage( "Draw!", Style.MessageDraw, StatusRow );
        };
        Game.Referee.ForbiddenMove += ( _, message ) => WriteMessage( message, Style.MessageError );

        // Initialize board
        Game.Board.StoneMoved += ( _, move ) =>
        {
            WriteMessage( " ", Style.Info, MessageRow ); // clear message line
            WriteCell( Game.Board, (move.Col, move.Row) );
        };

        InfoStartRow = BoardStartRow + boardSize;
        InitializeStyle();

        Console.Clear();
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        WriteBoard( Game.Board );
        WriteInfo();

        Console.ResetColor();
        Console.SetCursorPosition( BoardStartCol + boardSize - 1, BoardStartRow + boardSize / 2 - 1 );
    }
    
    public void Run()
    {
        Initialize();

        while ( true )
        {
            if ( Game.Referee.IsGameOver )
            {
                ProcessPressedKey( Console.ReadKey( true ) );
            }
            else
            {
                var player = Game.CurrentPlayer;
                WriteMessage( 
                    //$"{player.Name} ({player.Stone.ToString()})'s move ...",
                    $"{player.Name}'s move ...",
                    Game.CurrentPlayer.Stone switch
                    {
                        Stone.Black => Style.PlayerBlack,
                        Stone.White => Style.PlayerWhite,
                        _ => Style.Info
                    }, 
                    StatusRow );
                Game.TryProceedMove();
            }

            if ( BreakTheGame )
            {
                Console.ResetColor();
                Console.SetCursorPosition( 0, BoardStartRow + boardSize );
                Console.WriteLine(); 

                return;
            }
        }
    }
}