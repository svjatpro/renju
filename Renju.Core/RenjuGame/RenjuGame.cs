using Renju.Core.Extensions;

namespace Renju.Core.RenjuGame;

public interface IPlayer
{
    Stone Stone { get; }

    void StartGame( IBoard board, Stone playersColor );
    bool TryProceedMove( out Move move );
}
public interface IConsolePlayer : IPlayer
{
    event EventHandler<ConsoleKeyInfo> OtherKeyPressed;
}


public abstract class Player : IPlayer
{
    protected IBoard Board = null!;
    public Stone Stone { get; private set; }

    public void StartGame( IBoard board, Stone playersColor )
    {
        Board = board;
        Stone = playersColor;
    }

    public abstract bool TryProceedMove( out Move move );

    public static IConsolePlayer ConsoleHuman( int boardStartCol, int boardStartRow ) 
        => new ConsoleHuman(boardStartCol, boardStartRow);
    public static IPlayer PcPlayer() => new PcPlayer();
}

internal class ConsoleHuman( int boardStartCol, int boardStartRow ) : Player, IConsolePlayer
{
    public override bool TryProceedMove( out Move move )
    {
        move = default!;
        while ( true )
        {
            var key = Console.ReadKey( true );
            switch ( key.Key )
            {
                case ConsoleKey.UpArrow:
                    if ( Console.CursorTop - boardStartRow > 0 )
                        Console.SetCursorPosition( Console.CursorLeft, Console.CursorTop - 1 );
                    break;
                case ConsoleKey.DownArrow:
                    if ( Console.CursorTop - boardStartRow < Board.Size - 1 )
                        Console.SetCursorPosition( Console.CursorLeft, Console.CursorTop + 1 );
                    break;
                case ConsoleKey.LeftArrow:
                    if ( Console.CursorLeft - boardStartCol > 0 )
                        Console.SetCursorPosition( Console.CursorLeft - 2, Console.CursorTop );
                    break;
                case ConsoleKey.RightArrow:
                    if ( Console.CursorLeft - boardStartCol < Board.Size * 2 - 2 )
                        Console.SetCursorPosition( Console.CursorLeft + 2, Console.CursorTop );
                    break;
                case ConsoleKey.Spacebar:
                    // put player's stone
                    move = new Move( Console.CursorLeft / 2, Console.CursorTop - boardStartRow, Stone );
                    return true;
                default:
                    OtherKeyPressed?.Invoke( this, key );
                    return false;
            }
        }
    }

    public event EventHandler<ConsoleKeyInfo>? OtherKeyPressed;
}

public class PcPlayer : Player
{
    public override bool TryProceedMove( out Move move )
    {
        Thread.Sleep( 500 );
        // todo:
        var rnd = new Random();
        var cell = (col: rnd.Next( Board.Size ), row: rnd.Next( Board.Size ));
        move = new Move( cell.col, cell.row, Stone );

        return true;
    }
}

public class RenjuGame
{
    private Dictionary<Stone, IPlayer> Players;
    private Stone CurrentStone = Stone.Black;

    public IReferee Referee { get; init; }
    public IBoard Board { get; init; }

    public IPlayer CurrentPlayer => Players[CurrentStone];

    public RenjuGame( int size, IPlayer black, IPlayer white )
    {
        Board = new Board.Board( size );
        Board.StoneMoved += ( _, move ) => CurrentStone = move.Stone.Opposite();
        Referee = new Referee( Board );

        Players = new()
        {
            { Stone.Black, black },
            { Stone.White, white },
        };
        black.StartGame( Board, Stone.Black );
        white.StartGame( Board, Stone.White );
    }

    public bool TryProceedMove()
    {
        if ( Referee.IsGameOver ||
             !Players[CurrentStone].TryProceedMove( out var move ) ||
             !Referee.MoveAllowed( move.Col, move.Row, CurrentStone ) )
        {
            return false;
        }

        Board.PutStone( move.Col, move.Row, CurrentStone );
        return true;
    }
}