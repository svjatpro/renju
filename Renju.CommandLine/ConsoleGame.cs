using Renju.Core;
using Renju.Core.Extensions;
using Renju.Core.RenjuGame;
using System.Drawing;

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
    private StoneRole DebugRole = StoneRole.None;

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
            WriteCell( board, new Coord(col, row), false );
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
            Console.Write( Layout.GridChars[10] );
            if( col < board.Size - 1 ) Console.Write( new string( ' ', Layout.CellWidth - 1 ) );
        }

        Console.Write( ' ' ); // border right frame
        Console.WriteLine();
    }
    private void WriteCell( IBoard board, Coord cell, bool locateCursor = true )
    {
        // save cursor position
        (int x, int y) current = (Console.CursorLeft, Console.CursorTop);

        if ( locateCursor ) SetCursor( cell );

        var cellCharLength = 1; // length of cell content
        switch ( board[cell.Col, cell.Row].Stone )
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
                var crossChar = cell.Col switch
                {
                    0 when cell.Row == 0 => Layout.GridChars[0], // ┌
                    0 when cell.Row == board.Size - 1 => Layout.GridChars[15], // └
                    0 => Layout.GridChars[5], // ├
                    _ when cell.Col == board.Size - 1 && cell.Row == 0 => Layout.GridChars[4], // ┐
                    _ when cell.Col == board.Size - 1 && cell.Row == board.Size - 1 => Layout.GridChars[19], // ┘
                    _ when cell.Col == board.Size - 1 => Layout.GridChars[9], // ┤
                    _ when cell.Row == 0 => Layout.GridChars[2], // ┬
                    _ when cell.Row == board.Size - 1 => Layout.GridChars[17], // ┴
                    _ => Layout.GridChars[7] // ┼
                };
                if ( DebugRole != StoneRole.None )
                {
                    var debugStone = DebugRole == StoneRole.Self ? PcColor : playerColor;
                    var debugValue = Game.Players[PcColor].GetDebug( cell.Col, cell.Row, DebugRole );
                    var debugText = debugValue == 0 ? "" : debugValue.ToString();
                    if ( debugText.Length > 0 )
                    {
                        if( debugStone == Stone.Black ) Layout.DebugBlack.SetColor();
                        else Layout.DebugWhite.SetColor();
                        Console.Write( debugText );
                        cellCharLength = debugText.Length;
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

        if ( cellCharLength < Layout.CellWidth && cell.Col < board.Size - 1 )
        {
            Layout.Board.SetColor();
            Console.Write( new string( Layout.GridChars[1], Layout.CellWidth - cellCharLength ) );
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

    private void SetCursor( Coord? coord = null )
    {
        coord ??= new Coord( boardSize / 2, boardSize / 2 );
        Console.SetCursorPosition(
            BoardStartCol + coord.Col * Layout.CellWidth,
            BoardStartRow + coord.Row * Layout.CellHeight );
    }
    private void SwitchLayout( string? name = null )
    {
        if ( name != null && Layout.Name == name ) return;
        
        var coord = new Coord(
            ( Console.CursorLeft - BoardStartCol ) / Layout.CellWidth,
            ( Console.CursorTop - BoardStartRow ) / Layout.CellHeight );
        
        var currentLayout = LayoutConfigs.IndexOf( Layout );
        var newIndex = 
            ( name != null && LayoutConfigs.Exists( l => l.Name == name )) ? 
            LayoutConfigs.FindIndex( l => l.Name == name ) :
            ( currentLayout + 1 >= LayoutConfigs.Count ? 0 : currentLayout + 1 );
        Layout = LayoutConfigs[newIndex];
        
        WriteLayout();
        SetCursor( coord );
    }

    private bool ReadDebugPlayerMove()
    {
        while ( DebugRole != StoneRole.None )
        {
            var key = Console.ReadKey( true );
            switch ( key.Key )
            {
                case ConsoleKey.Spacebar:
                    return true;
                default:
                    ProcessPressedKey( key );
                    return false;
            }
        }
        return true;
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
        switch ( key.Key )
        {
            case ConsoleKey.Escape:
                BreakTheGame = true;
                return;
            case ConsoleKey.D:
                // switch debug role (player side)
                SwitchLayout( "big" );
                DebugRole = DebugRole switch
                {
                    StoneRole.None => StoneRole.Self,
                    StoneRole.Self => StoneRole.Opponent,
                    _ => StoneRole.None
                };
                (int x, int y) current = (Console.CursorLeft, Console.CursorTop);
                WriteBoard( Game.Board );
                Console.SetCursorPosition( current.x, current.y );
                return;
            case ConsoleKey.N:
                // start new game
                playerColor = playerColor.Opposite();
                InitializeNewGame();
                return;
            case ConsoleKey.H:
                // show help instead of board
                return;
            case ConsoleKey.L:  
                // change layout
                SwitchLayout();
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
            //private (int width, int height) WindowSize = default;
        }
    }

    private void InitializeStyle()
    {
        // initialize styles
        LayoutConfigs =
        [
            new()
            {
                Name = "default",
                StoneCharBlack = 'x',
                StoneCharWhite = 'o',
                CellHeight = 1,
                CellWidth = 2,
            },
            new()
            {
                Name = "big",
                //StoneCharBlack =  '\u26ab'
                StoneCharBlack = 'X',
                //StoneCharWhite = '\u26aa',
                StoneCharWhite = 'O',
                CellHeight = 2,
                CellWidth = 4,
            }
        ];

        //Console.Write( '\u26ab' ); // ⚫
        //Console.Write( '\u25cf' ); // ●
        //Console.Write( '\u26aa' ); // ⚪
        //Console.Write( '\u25cb' ); // ○   
        Layout = LayoutConfigs[0];
    }

    private void InitializeNewGame()
    {
        // initialize players
        var pc = new ConsolePlayerWrapper( Player.PcPlayer( "Computer" ), ReadDebugPlayerMove );
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
            if ( DebugRole == StoneRole.None )
            {
                WriteCell( Game.Board, new Coord(move.Col, move.Row) );
            }
            else
            {
                (int x, int y) current = (Console.CursorLeft, Console.CursorTop);
                WriteBoard( Game.Board );
                Console.SetCursorPosition( current.x, current.y );
            }
        };

        SetCursor();
        //Console.SetCursorPosition( BoardStartCol + boardSize - 1, BoardStartRow + boardSize / 2 - 1 );
        WriteLayout();
    }

    private void WriteLayout()
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
        InitializeStyle();
        InitializeNewGame();
        
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