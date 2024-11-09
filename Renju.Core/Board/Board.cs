namespace Renju.Core.Board;

internal class Board : IBoard
{
    #region Private Fields

    private Cell[,] Cells;

    public Board(int size)
    {
        Size = size;
        Cells = new Cell[size, size];

        for (var col = 0; col < size; col++)
            for (var row = 0; row < size; row++)
                Cells[col, row] = new Cell();
    }

    #endregion

    public int Size { get; }

    public ICell this[int col, int row]
    {
        get
        {
            if (col >= 0 && col < Size && row >= 0 && row < Size)
                return Cells[col, row];
            throw new IndexOutOfRangeException($"You've tried to access cell ({col}, {row}), but the board size is {Size}.");
        }
    }

     public Move? LastMove { get; private set; }

    public void PutStone(int col, int row, Stone stone)
    {
        Cells[col, row].Stone = stone;

        LastMove = new Move(col, row, stone, LastMove?.SeqNumber ?? 0);

        StoneMoved?.Invoke(this, LastMove);

        // TODO: add move index
    }

    public event EventHandler<Move>? StoneMoved;
}
 