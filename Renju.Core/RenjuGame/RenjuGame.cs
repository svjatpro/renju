using Renju.Core.Extensions;

namespace Renju.Core.RenjuGame;

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
        black.StartGame( Stone.Black, Board, Referee );
        white.StartGame( Stone.White, Board, Referee );
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