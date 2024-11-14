using Renju.Core;
using Renju.Core.Extensions;
using Renju.Core.RenjuGame;

namespace Renju.CommandLine;

public class ConsoleGame( Stone playerColor, int boardSize = 15 )
{
    private Stone PcColor => playerColor.Opposite();
    private RenjuGame Game = null!;
    private bool BreakTheGame;
    private LayoutConfig Layout = null!;
    private List<LayoutConfig> LayoutConfigs = null!;

    private const int BoardStartCol = 2;
    private const int BoardStartRow = 3;
    private const int StatusRow = 1;
    private const int MessageRow = 2;
    private DebugMode Debug = DebugMode.None;
    //private (int width, int height) WindowSize = default;
    
    private enum DebugMode { None = 0, Self = 1, Opponent = 2 }
    
    private void WriteInfo()
    {
        var infoStartRow = BoardStartRow + boardSize * Layout.CellHeight - Layout.CellHeight + 1;
        
        Console.SetCursorPosition( BoardStartCol, infoStartRow );
        var leftSpace = new string( ' ', BoardStartCol );

        // write players' names
        var player1 = Game.Players[Stone.Black].Name;
        Layout.PlayerBlack.SetColor();
        Console.Write( $"{player1,BoardStartCol}" );

        var player2 = Game.Players[Stone.White].Name;
        Layout.PlayerWhite.SetColor();
        var player2Padding = // magic number for right alignment
            boardSize * Layout.CellWidth - BoardStartCol - 
            player1.Length - player2.Length - Layout.CellWidth + 2;
        Console.Write( $"{player2.PadLeft( player2Padding + player2.Length, ' ' )}" );
        Console.WriteLine();

        // write game help
        Console.WriteLine();
        Layout.Help.SetColor();
        Console.WriteLine( $"{leftSpace}esc - exit game" );
        Console.WriteLine( $"{leftSpace}n   - start new game" );
        Console.WriteLine( $"{leftSpace}h   - detailed help" );

        // write game info
        Console.WriteLine( $"{leftSpace}{new string( '-', 5 )}" );
        Layout.Info.SetColor();
        Console.WriteLine( $"{leftSpace}Renju v3.0; (c) Sviatoslav Prokipets" );
    }
    private void WriteBoard( IBoard board )
    {
        for ( var row = 0; row < board.Size; row++ )
        {
            WriteRow( board, row );

            if ( Layout.CellHeight <= 1 || row == board.Size - 1 )
            {
                continue;
            }
            for ( var i = 1; i < Layout.CellHeight; i++ )
            {
                WriteEmptyRow( board, row, i );
            }
        }
    }

    private void WriteRow( IBoard board, int row )
    {
        Console.SetCursorPosition( BoardStartCol - 1, BoardStartRow + row * Layout.CellHeight );

        Layout.Board.SetColor();
        Console.Write( ' ' ); // border left frame

        // write row of cells
        for ( var col = 0; col < board.Size; col++ )
        {
            WriteCell( board, (col, row), false );
        }
        Console.Write( ' ' ); // border right frame
        Console.WriteLine();
    }
    private void WriteEmptyRow( IBoard board, int row, int emptyRowIndex )
    {
        Console.SetCursorPosition( BoardStartCol - 1, BoardStartRow + row * Layout.CellHeight + emptyRowIndex );
        Layout.Board.SetColor();
        Console.Write( ' ' ); // border left frame

        for ( var col = 0; col < board.Size; ++col )
        {
            Console.Write( '\u2502' );
            if( col < board.Size - 1 ) Console.Write( new string( ' ', Layout.CellWidth - 1 ) );
        }

        Console.Write( ' ' ); // border right frame
        Console.WriteLine();
    }
    private void WriteCell( IBoard board, (int col, int row) cell, bool locateCursor = true )
    {
        // save cursor position
        (int x, int y) current = (Console.CursorLeft, Console.CursorTop);

        if ( locateCursor )
        {
            Console.SetCursorPosition(
                BoardStartCol + cell.col * Layout.CellWidth,
                BoardStartRow + cell.row * Layout.CellHeight );
        }

        const char barChar = '\u2500'; // ─
        var cellCharLength = 1; // length of cell content
        switch ( board[cell.col, cell.row].Stone )
        {
            case Stone.Black:
                Layout.StoneBlack.SetColor();
                Console.Write( Layout.StoneCharBlack );
                break;
            case Stone.White:
                Layout.StoneWhite.SetColor();
                Console.Write( Layout.StoneCharWhite );
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
                if ( Debug != DebugMode.None )
                {
                    var debugStone = Debug == DebugMode.Self ? PcColor : playerColor;
                    var debugWeight = Game.Players[PcColor]
                        .GetDebug( cell.col, cell.row, debugStone )
                        .ToString();
                    var debug =
                        debugWeight == "0" ? "" :
                        ( debugWeight.Length > 2 ? $"{debugWeight[0]}~" : debugWeight );

                    if ( debug.Length > 0 )
                    {
                        Layout.Help.SetColor();
                        Console.Write( debug );
                        cellCharLength = debug.Length;
                    }
                    else
                    {
                        Layout.Board.SetColor();
                        Console.Write( crossChar );
                        cellCharLength = 1;
                    }
                }
                else
                {
                    Layout.Board.SetColor();
                    Console.Write( crossChar );
                    cellCharLength = 1;
                }
                break;
        }

        if ( cellCharLength == 1 && cell.col < board.Size - 1 )
        {
            Layout.Board.SetColor();
            Console.Write( new string( barChar, Layout.CellWidth - cellCharLength ) );
        }

        if ( locateCursor )
        {
            Console.SetCursorPosition( current.x, current.y );
        }
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

    private bool ReadPlayerMove( out Coord coord )
    {
        while ( true )
        {
            var key = Console.ReadKey( true );
            switch ( key.Key )
            {
                case ConsoleKey.UpArrow:
                    if ( Console.CursorTop - BoardStartRow > 0 )
                        Console.SetCursorPosition( Console.CursorLeft, Console.CursorTop - Layout.CellHeight );
                    break;
                case ConsoleKey.DownArrow:
                    if ( Console.CursorTop - BoardStartRow < (Game.Board.Size - 1) * Layout.CellHeight )
                        Console.SetCursorPosition( Console.CursorLeft, Console.CursorTop + Layout.CellHeight );
                    break;
                case ConsoleKey.LeftArrow:
                    if ( Console.CursorLeft - BoardStartCol > 0 )
                        Console.SetCursorPosition( Console.CursorLeft - Layout.CellWidth, Console.CursorTop );
                    break;
                case ConsoleKey.RightArrow:
                    if ( Console.CursorLeft - BoardStartCol < (Game.Board.Size - 1) * Layout.CellWidth )
                        Console.SetCursorPosition( Console.CursorLeft + Layout.CellWidth, Console.CursorTop );
                    break;
                case ConsoleKey.Spacebar:
                    // put player's stone
                    coord = new Coord(
                        ( Console.CursorLeft - BoardStartCol ) / Layout.CellWidth,
                        ( Console.CursorTop - BoardStartRow ) / Layout.CellHeight );
                    return true;
                default:
                    ProcessPressedKey( key );
                    coord = default!;
                    return false;
            }
        }
    }
    private void ProcessPressedKey( ConsoleKeyInfo key )
    {
        (int x, int y) current = (Console.CursorLeft, Console.CursorTop);
        switch ( key.Key )
        {
            case ConsoleKey.Escape:
                BreakTheGame = true;
                return;
            case ConsoleKey.D:
                Debug = Debug switch
                {
                    DebugMode.None => DebugMode.Self,
                    DebugMode.Self => DebugMode.Opponent,
                    _ => DebugMode.None
                };
                WriteBoard( Game.Board );
                Console.SetCursorPosition( current.x, current.y );
                return;
            case ConsoleKey.N:
                // start new game
                return;
            case ConsoleKey.H:
                // show help instead of board
                return;
            case ConsoleKey.L:  // change layout
                var coord = new Coord(
                    ( current.x  - BoardStartCol ) / Layout.CellWidth,
                    ( current.y  - BoardStartRow ) / Layout.CellHeight );
                var currentLayout = LayoutConfigs.IndexOf( Layout );
                Layout = LayoutConfigs[currentLayout + 1 >= LayoutConfigs.Count ? 0 : currentLayout + 1];
                WritLayout();
                Console.SetCursorPosition(
                    BoardStartCol + coord.Col * Layout.CellWidth,
                    BoardStartRow + coord.Row * Layout.CellHeight );
                return;
            //case ConsoleKey.F:
            //    if ( WindowSize == default )
            //    {
            //        WindowSize = ( Console.WindowWidth, Console.WindowHeight );
            //        Console.SetWindowSize( Console.LargestWindowWidth, Console.LargestWindowHeight );
            //        //Console.SetBufferSize( Console.LargestWindowWidth, Console.LargestWindowHeight );
            //    }
            //    else
            //    {
            //        Console.SetWindowSize( WindowSize.width, WindowSize.height );
            //        //Console.SetBufferSize( WindowSize.width, WindowSize.height );
            //        WindowSize = default;
            //    }
            //    return;
        }
    }

    private void InitializeStyle()
    {
        //const char barChar = '\u2500'; // ─
        //0 when cell.row == 0 => '\u250c', // ┌
        //0 when cell.row == board.Size - 1 => '\u2514', // └
        //0 => '\u251c', // ├
        //_ when cell.col == board.Size - 1 && cell.row == 0 => '\u2510', // ┐
        //_ when cell.col == board.Size - 1 && cell.row == board.Size - 1 => '\u2518', // ┘
        //_ when cell.col == board.Size - 1 => '\u2524', // ┤
        //_ when cell.row == 0 => '\u252c', // ┬
        //_ when cell.row == board.Size - 1 => '\u2534', // ┴
        //_ => '\u253c' // ┼
        
        // initialize styles
        LayoutConfigs =
        [
            new()
            {
                Name = "default",
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
                StoneCharWhite = 'o',
                CellHeight = 1,
                CellWidth = 2,
            },
            new()
            {
                Name = "BigCells",
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
                StoneCharBlack = 'X',
                StoneCharWhite = 'O',
                CellHeight = 2,
                CellWidth = 4,
            }
        ];
        Layout = LayoutConfigs[0];
        
        //Console.Write( '\u26ab' ); // ⚫
        //Console.Write( '\u25cf' ); // ●
        //Console.Write( '\u26aa' ); // ⚪
        //Console.Write( '\u25cb' ); // ○   
    }
    
    private void Initialize()
    {
        // initialize players
        var pc = Player.PcPlayer( "Computer" );
        var human = new ConsolePlayer( "Human", () => (ReadPlayerMove( out var coord ), coord) );

        // initialize game
        Game = new RenjuGame( boardSize,
            playerColor == Stone.Black ? human : pc,
            playerColor == Stone.White ? human : pc );
        
        // initialize referee
        Game.Referee.GameOver += ( _, winner ) =>
        {
            if ( winner == playerColor )
                WriteMessage( "You win!", Layout.MessageWin, StatusRow );
            else if ( winner == PcColor )
                WriteMessage( "You lose!", Layout.MessageLoose, StatusRow );
            else
                WriteMessage( "Draw!", Layout.MessageDraw, StatusRow );
        };
        Game.Referee.ForbiddenMove += ( _, message ) => WriteMessage( message, Layout.MessageError );

        // Initialize board
        Game.Board.StoneMoved += ( _, move ) =>
        {
            WriteMessage( " ", Layout.Info, MessageRow ); // clear message line
            WriteCell( Game.Board, (move.Col, move.Row) );
        };

        InitializeStyle();
        
        Console.SetCursorPosition( BoardStartCol + boardSize - 1, BoardStartRow + boardSize / 2 - 1 );
        WritLayout();
    }

    private void WritLayout()
    {
        (int x, int y) current = (Console.CursorLeft, Console.CursorTop);
        
        Console.Clear();
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.CursorVisible = false;
        
        WriteBoard( Game.Board );
        WriteInfo();

        Console.ResetColor();
        Console.SetCursorPosition( current.x, current.y );
        Console.CursorVisible = true;
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
                    $"{player.Name}'s move ...",
                    Game.CurrentPlayer.Stone switch
                    {
                        Stone.Black => Layout.PlayerBlack,
                        Stone.White => Layout.PlayerWhite,
                        _ => Layout.Info
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