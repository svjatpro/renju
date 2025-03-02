namespace Renju.Core;

public interface IBoard : IDisposable
{
    int Size { get; }

    ICell this[int col, int row] { get; }

    Move? LastMove { get; }

    // todo: remove from interface    
    void PutStone( Move move );
    void PutStone( int col, int row, Stone stone );
    IBoard Clone();

    event EventHandler<Move> StoneMoved;    
}