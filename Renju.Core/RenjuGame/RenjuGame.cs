using Renju.Core.Extensions;

namespace Renju.Core.RenjuGame;

public interface IPlayer
{
    Stone Stone { get; }
    bool TryProceedMove( out Move move );
}
internal class ConsolePlayer( IBoard Board, Stone Color ) : IPlayer
{
    public Stone Stone => Color;
    public bool TryProceedMove( out Move move )
    {
        move = new Move( 0, 0, Stone, 0 );
        return false;
    }
}
internal class PCPlayer( IBoard Board, Stone Color ) : IPlayer
{
    public Stone Stone => Color;
    public bool TryProceedMove( out Move move )
    {
        var rnd = new Random();
        var cell = (col: rnd.Next( Board.Size ), row: rnd.Next( Board.Size ));
        move = new Move( cell.col, cell.row, Stone, 0 );

        return true;
    }
}

public class RenjuGame
{
    private Dictionary<Stone, IPlayer> Players;

    public IReferee Referee { get; init; }
    public IBoard Board { get; init; }
    
    public Stone CurrentPlayer { get; private set; } = Stone.Black;

    public RenjuGame(int size, Stone playerColor)
    {
        Board = new Board.Board( size );
        Board.StoneMoved += ( _, move ) => CurrentPlayer = move.Stone.Opposite();
        Referee = new Referee( Board );

        var (human, pc) = (
            new ConsolePlayer( Board, playerColor ),
            new PCPlayer( Board, playerColor.Opposite() ) );

        Players = new()
        {
            { Stone.Black, playerColor == Stone.Black ? human : pc },
            { Stone.White, playerColor == Stone.Black ? pc : human },
        };
    }

    public bool TryProceedMove()
    {
        if ( Referee.IsGameOver ||
             !Players[CurrentPlayer].TryProceedMove( out var move ) ||
             !Referee.MoveAllowed( move.Col, move.Row, move.Stone ) )
        {
            return false;
        }

        Board.PutStone( move.Col, move.Row, move.Stone );
        return true;
    }
}