namespace Renju.Core.BoardAnalyser;

internal interface IBoardAnalyser
{
    Stone TargetStone { get; }
    Dictionary<FigureDirection, FigureType> this[int col, int row] { get; }
    event EventHandler<(
        Move move,
        Dictionary<FigureDirection, FigureType> figures,
        List<(int col, int row)> affectedCells)> MoveAnalysed;
}

internal class BoardAnalyzer : IBoardAnalyser
{
    #region Private fields

    private readonly IBoard Board;
    private readonly Dictionary<FigureDirection, FigureType>[,] FiguresMap;
    private readonly Dictionary<FigureDirection, FigureType> OccupiedCell = new();

    #endregion

    #region Private methods

    private void InitializeFiguresMap()
    {
        OccupiedCell[FigureDirection.Horizontal] = FigureType.None;
        OccupiedCell[FigureDirection.Vertical] = FigureType.None;
        OccupiedCell[FigureDirection.DiagonalLeft] = FigureType.None;
        OccupiedCell[FigureDirection.DiagonalRight] = FigureType.None;

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

    private void ProcessRow(
        IList<int> row, int rowSize,
        FigureDirection direction,
        Func<int,(int col, int row)> cellResolver,
        List<(int col, int row)> affectedCells )
    {
        var rowFigures = RowParser.ParseRow( row, rowSize, TargetStone, TargetStone == Stone.White );
        foreach ( var figure in rowFigures )
        {
            var cell = cellResolver(figure.cell);
            var cellFigures = FiguresMap[cell.col, cell.row];
            if ( cellFigures[direction]! != figure.type )
            {
                affectedCells.Add( cell );
                cellFigures[direction] = figure.type;
            }
        }
    }

    private void ProcessMove( Move move, out List<(int col, int row)> affectedCells )
    {
        var currentLine = new int[Board.Size];
        var lineMap = new Dictionary<int, (int col, int row)>();
        affectedCells = [];

        // horizontal rows
        var row = move.Row;
        for ( var c = 0; c < Board.Size; c++ ) currentLine[c] = (int)Board[c, row].Stone;

        ProcessRow(
            currentLine, Board.Size, FigureDirection.Horizontal, 
            cell => (cell, row), affectedCells );

        // vertical rows
        var col = move.Col;
        for ( var r = 0; r < Board.Size; r++ ) currentLine[r] = (int)Board[col, r].Stone;
        ProcessRow( 
            currentLine, Board.Size, FigureDirection.Vertical, 
            cell => (col, cell), affectedCells );

        // diagonal left-top to right-bottom
        var startCol = Math.Max( move.Col - move.Row, 0 );
        var startRow = Math.Max( move.Row - move.Col, 0 );
        var lineIndex = 0;
        for ( int c = startCol, r = startRow; c < Board.Size && r < Board.Size; c++, r++, lineIndex++ )
        {
            currentLine[lineIndex] = (int)Board[c, r].Stone;
            lineMap[lineIndex] = ( c, r );
        }

        ProcessRow(
            currentLine, lineIndex, FigureDirection.DiagonalLeft, 
            cell => lineMap[cell], affectedCells );

        // diagonal right-tom to left-bottom
        startCol = Math.Min( move.Col + move.Row, Board.Size - 1 );
        startRow = Math.Max( move.Row - move.Col, 0 );
        lineIndex = 0;
        for ( int c = startCol, r = startRow; c > 0 && r < Board.Size; c--, r++, lineIndex++ )
        {
            currentLine[lineIndex] = (int)Board[c, r].Stone;
            lineMap[lineIndex] = ( c, r );
        }
        ProcessRow( 
            currentLine, lineIndex, FigureDirection.DiagonalRight,
            cell => lineMap[cell], affectedCells );
    }

    #endregion

    public BoardAnalyzer( IBoard board, Stone targetStone )
    {
        Board = board;
        TargetStone = targetStone;

        FiguresMap = new Dictionary<FigureDirection, FigureType>[Board.Size, Board.Size];
        InitializeFiguresMap();

        Board.StoneMoved += ( _, move ) =>
        {
            ProcessMove( move, out var affectedCells );

            // notify about move analysed with figures
            //  which are not 'potential' anymore but 'actual' in this context
            //  as the move is already processed for the cell
            MoveAnalysed?.Invoke( this, (move, FiguresMap[move.Col, move.Row], affectedCells) );

            // clear figures map for the cell
            FiguresMap[move.Col, move.Row] = OccupiedCell;
        };
    }

    public Stone TargetStone { get; init; }
    public Dictionary<FigureDirection, FigureType> this[int col, int row] => FiguresMap[col, row];
    public event EventHandler<(
        Move move,
        Dictionary<FigureDirection, FigureType> figures,
        List<(int col, int row)> affectedCells)>? MoveAnalysed;
}