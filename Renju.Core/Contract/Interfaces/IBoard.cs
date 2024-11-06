namespace Renju.Core;

public interface IBoard
{
    int Size { get; }

    ICell this[int col, int row] { get; }

    Move? LastMove { get; }

    // todo: remove from interface
    void PutStone(int col, int row, Stone stone);

    event EventHandler<Move> StoneMoved;
}