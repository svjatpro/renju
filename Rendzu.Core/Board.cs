namespace Rendzu.Core;

public enum Stone
{
    None = 0x01,
    White = 0x02,
    Black = 0x03,
}

public interface ICell
{
    Stone Stone { get; }
}
internal class Cell : ICell
{
    public Stone Stone { get; set; } = Stone.None;
}

public interface IBoard
{
    int Size { get; }
    
    ICell this[int col, int row] { get; }

    void PutStone( int col, int row, Stone stone );
}

public class RendzuGame
{
    public IBoard Board { get; private set; } = null!;

    public static RendzuGame New( int size )
    {
        return new RendzuGame { Board = new Board( size ) };
    }
}

internal class Board : IBoard
{
    #region Private Fields

    private Cell[,] Cells;

    public Board(int size)
    {
        Size = size;
        Cells = new Cell[size, size];

        for ( var col = 0; col < size; col++ ) 
            for ( var row = 0; row < size; row++ ) 
                Cells[col, row] = new Cell();
    }

    #endregion

    public int Size { get; }

    public ICell this[int col, int row]
    {
        get
        {
            if ( col >= 0 && col < Size && row >= 0 && row < Size )
                return Cells[col, row];
            throw new IndexOutOfRangeException( $"You've tried to access cell ({col}, {row}), but the board size is {Size}." );
        }
    }

    public void PutStone( int col, int row, Stone stone )
    {
        Cells[col, row].Stone = stone;

        // TODO: add move index
    }
}
