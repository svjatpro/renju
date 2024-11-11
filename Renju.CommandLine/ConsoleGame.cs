using Renju.Core;
using Renju.Core.Extensions;
using Renju.Core.RenjuGame;

namespace Renju.CommandLine;

public class ConsoleGame( 
    Stone playerColor,
    int boardSize = 15,
    int boardStartCol = 2,
    int boardStartRow = 4,
    int statusRow = 2,
    int messageRow = 3 )
{
    private Stone PcColor => playerColor.Opposite();
    private RenjuGame Game = null!;
    private bool BreakTheGame;
    private GameStyle Style = null!;

    private record ColorStyle( ConsoleColor Foreground, ConsoleColor Background );

    private record GameStyle
    {
        public ColorStyle Board { get; init; } = null!;
        public ColorStyle Info { get; init; } = null!;
        public ColorStyle MessageError { get; init; } = null!;
        public ColorStyle MessageMoveBlack { get; init; } = null!;
        public ColorStyle MessageMoveWhite { get; init; } = null!;
        public ColorStyle MessageWin { get; init; } = null!;
        public ColorStyle MessageLoose { get; init; } = null!;
        public ColorStyle MessageDraw { get; init; } = null!;
        public ColorStyle Black { get; init; } = null!;
        public ColorStyle White { get; init; } = null!;
        public char StoneBlack { get; init; }
        public char StoneWhite { get; init; }
    }

    private void ShowBoard( IBoard board )
    {
        Console.SetCursorPosition( 0, 0 );

        for ( var i = 0; i < boardStartRow; i++ )
            Console.WriteLine();

        const char barChar = '\u2500'; // ─
        for ( var row = 0; row < board.Size; row++ )
        {
            Console.ResetColor();
            Console.Write( ' ' );
            Console.BackgroundColor = Style.Board.Background;
            Console.ForegroundColor = Style.Board.Foreground;

            for ( var i = Console.CursorLeft; i < boardStartCol; i++ )
                Console.Write( ' ' );

            for ( var col = 0; col < board.Size; col++ )
            {
                ShowCell( board, (col, row), false );
                Console.BackgroundColor = Style.Board.Background;
                Console.ForegroundColor = Style.Board.Foreground;
                if ( col < board.Size - 1 ) Console.Write( barChar );
            }
            Console.Write( ' ' );
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
                Console.ForegroundColor = Style.Black.Foreground;
                Console.BackgroundColor = Style.Black.Background;
                Console.Write( Style.StoneBlack );
                break;
            case Stone.White:
                Console.ForegroundColor = Style.White.Foreground;
                Console.BackgroundColor = Style.White.Background;
                Console.Write( Style.StoneWhite );
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

    private void ShowMessage( string message, ColorStyle color, int? row = null )
    {
        // save cursor position
        (int x, int y) current = (Console.CursorLeft, Console.CursorTop);

        Console.SetCursorPosition( 1, row ?? messageRow );
        Console.BackgroundColor = color.Background;
        Console.ForegroundColor = color.Foreground;
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
    private void Initialize()
    {
        // initialize style
        Style = new GameStyle
        {
            Board = new ColorStyle( ConsoleColor.DarkGray, ConsoleColor.Black ),
            Info = new ColorStyle( ConsoleColor.DarkGray, ConsoleColor.Black ),
            MessageError = new ColorStyle( ConsoleColor.Red, ConsoleColor.Black ),
            MessageMoveBlack = new ColorStyle( ConsoleColor.DarkGray, ConsoleColor.Black ),
            MessageMoveWhite = new ColorStyle( ConsoleColor.White, ConsoleColor.Black ),
            MessageWin = new ColorStyle( ConsoleColor.Green, ConsoleColor.Black ),
            MessageLoose = new ColorStyle( ConsoleColor.Yellow, ConsoleColor.Black ),
            MessageDraw = new ColorStyle( ConsoleColor.Cyan, ConsoleColor.Black ),
            Black = new ColorStyle( ConsoleColor.Yellow, ConsoleColor.Black ),
            White = new ColorStyle( ConsoleColor.White, ConsoleColor.Black ),
            StoneBlack = 'x',
            StoneWhite = 'o'
        };
        //Console.Write( '\u26ab' ); // ⚫
        //Console.Write( '\u25cf' ); // ●
        //Console.Write( '\u26aa' ); // ⚪
        //Console.Write( '\u25cb' ); // ○

        // prepare console
        Console.Clear();
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.BackgroundColor = Style.Info.Background;
        Console.ForegroundColor = Style.Info.Foreground;
        Console.WriteLine( " Renju v3.0; Sviatoslav Prokipets (c)" );
        Console.WriteLine();
        Console.ResetColor();

        // initialize players
        var pc = Player.PcPlayer();
        var human = new ConsolePlayer( boardStartCol, boardStartRow, "Human" );
        human.OtherKeyPressed += ( _, k ) => ProcessPressedKey( k );

        // initialize game
        Game = new RenjuGame( boardSize,
            playerColor == Stone.Black ? human : pc,
            playerColor == Stone.White ? human : pc );
        
        // initialize referee
        Game.Referee.GameOver += ( _, winner ) =>
        {
            if ( winner == playerColor )
                ShowMessage( "You win!", Style.MessageWin, statusRow );
            else if ( winner == PcColor )
                ShowMessage( "You lose!", Style.MessageLoose, statusRow );
            else
                ShowMessage( "Draw!", Style.MessageDraw, statusRow );
        };
        Game.Referee.ForbiddenMove += ( _, message ) => ShowMessage( message, Style.MessageError );

        // Initialize board
        Game.Board.StoneMoved += ( _, move ) =>
        {
            ShowMessage( " ", Style.Info, messageRow ); // clear message line
            ShowCell( Game.Board, (move.Col, move.Row) );
        };

        // show board
        ShowBoard( Game.Board );

        Console.ResetColor();
        Console.SetCursorPosition( boardStartCol, boardStartRow );
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
                ShowMessage( 
                    $"{player.Name} ({player.Stone.ToString()})'s move ...",
                    Game.CurrentPlayer.Stone switch
                    {
                        Stone.Black => Style.MessageMoveBlack,
                        Stone.White => Style.MessageMoveWhite,
                        _ => Style.Info
                    }, 
                    statusRow );
                Game.TryProceedMove();
            }

            if ( BreakTheGame )
            {
                Console.ResetColor();
                Console.SetCursorPosition( 0, boardStartRow + boardSize );
                Console.WriteLine(); 

                return;
            }
        }
    }
}