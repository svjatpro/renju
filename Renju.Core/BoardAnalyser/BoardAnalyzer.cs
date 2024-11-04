
namespace Renju.Core.BoardAnalyser;

internal interface IBoardAnalyser
{
    Dictionary<FigureDirection, FigureType> this[int col, int row] { get; }
    void ProcessMove( Move move );
    //event EventHandler<Move> GameWon;
}

internal class BoardAnalyzer : IBoardAnalyser
{
    #region Private fields

    private readonly IBoard Board;
    private readonly Stone TargetStone;
    private readonly Dictionary<FigureDirection, FigureType>[,] FiguresMap;

    #endregion

    #region Private methods

    private void InitializeFiguresMap()
    {
        for ( var col = 0; col < Board.Size; col++ )
        {
            for ( var row = 0; row < Board.Size; row++ )
            {
                FiguresMap[col, row] = new Dictionary<FigureDirection, FigureType>
                {
                    {FigureDirection.Horizontal, FigureType.None},
                    {FigureDirection.Vertical, FigureType.None},
                    {FigureDirection.DiagonalLeft, FigureType.None},
                    {FigureDirection.DiagonalRight, FigureType.None},
                };
            }
        }
    }

    private void ProcessRow( IList<int> row, int rowSize, Action<(int cell, FigureType type)> action )
    {
        var figures = RowParser.ParseRow( row, rowSize, TargetStone, TargetStone == Stone.White );
        foreach ( var figure in figures )
        {
            action( figure );
        }
    }
    private void StoneMoved( Move move )
    {
        var currentRow = new int[Board.Size];

        // horizontal rows
        //for ( var r = 0; r < Board.Size; r++ )
        //{

        var row = move.Row;
        for ( var c = 0; c < Board.Size; c++ )
        {
            currentRow[c] = (int)Board[c, row].Stone;
        }
        ProcessRow( currentRow, Board.Size, figure => FiguresMap[figure.cell, row][FigureDirection.Horizontal] = figure.type );
        //}

        // vertical rows
        //for ( var c = 0; c < Board.Size; c++ ) 
        //{
        var col = move.Col;
        for ( var r = 0; r < Board.Size; r++ )
        {
            currentRow[r] = (int)Board[col, r].Stone;
        }
        ProcessRow( currentRow, Board.Size, figure => FiguresMap[col, figure.cell][FigureDirection.Vertical] = figure.type );
        //}

        // todo: diagonal rows
    }

    #endregion

    public BoardAnalyzer( IBoard board, Stone targetStone )
    {
        Board = board;
        //Board.StoneMoved += StoneMoved;
        
        TargetStone = targetStone;

        FiguresMap = new Dictionary<FigureDirection, FigureType>[Board.Size, Board.Size];
        InitializeFiguresMap();
    }
    
    public Dictionary<FigureDirection, FigureType> this[int col, int row] => FiguresMap[col, row];
    
    public void ProcessMove( Move move )
    {
        StoneMoved( move );
    }
}