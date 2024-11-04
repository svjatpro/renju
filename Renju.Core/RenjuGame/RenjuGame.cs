namespace Renju.Core.RenjuGame;

public class RenjuGame
{
    public IReferee Referee { get; init; } = null!;
    public IBoard Board { get; init; } = null!;

    public static RenjuGame New(int size)
    {
        var board = new Board.Board(size);
        return new RenjuGame
        {
            Board = board,
            Referee = new Referee(board)
        };
    }
}