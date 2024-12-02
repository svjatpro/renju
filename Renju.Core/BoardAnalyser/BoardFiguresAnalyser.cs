namespace Renju.Core.BoardAnalyser;

internal class BoardFiguresAnalyser
{
    #region Private fields
    
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
        LineOfCells row,
        FigureDirection direction,
        Func<int,(int col, int row)> cellResolver)
    {
        var rowFigures = RowParser.DefineBestFigures( row, TargetStone );
        for ( var i = 0; i < row.Length; i++ )
        {
            var cell = cellResolver( i );
            var cellFigures = FiguresMap[cell.col, cell.row];
            if ( rowFigures.ContainsKey( i ) )
            {
                if ( cellFigures[direction]! != rowFigures[i] )
                {
                    cellFigures[direction] = rowFigures[i];
                }
            }
        }
    }

    private List<Coord> ProcessMove( Move move )
    {
        var currentLine = new int[Board.Size];
        var lineMap = new Dictionary<int, (int col, int row)>();
        var affectedCells = new List<Coord>();

        // horizontal rows
        var row = move.Row;
        for ( var c = 0; c < Board.Size; c++ )
        {
            currentLine[c] = (int)Board[c, row].Stone;
            if( c != move.Col && Board[c, row].Stone == Stone.None )
                affectedCells.Add( new Coord( c, row ) );
        }
        ProcessRow( new LineOfCells( currentLine, Board.Size ), FigureDirection.Horizontal, cell => (cell, row) );

        // vertical rows
        var col = move.Col;
        for ( var r = 0; r < Board.Size; r++ )
        {
            currentLine[r] = (int)Board[col, r].Stone;
            if ( r != move.Row && Board[col, r].Stone == Stone.None )
                affectedCells.Add( new Coord( col, r ) );
        }
        ProcessRow( new LineOfCells( currentLine, Board.Size ), FigureDirection.Vertical, cell => (col, cell) );

        // diagonal left-top to right-bottom
        var startCol = Math.Max( move.Col - move.Row, 0 );
        var startRow = Math.Max( move.Row - move.Col, 0 );
        var lineIndex = 0;
        for ( int c = startCol, r = startRow; c < Board.Size && r < Board.Size; c++, r++, lineIndex++ )
        {
            currentLine[lineIndex] = (int)Board[c, r].Stone;
            lineMap[lineIndex] = ( c, r );
            if ( c != move.Col && r != move.Row && Board[c, r].Stone == Stone.None )
                affectedCells.Add( new Coord( c, r ) );
        }
        ProcessRow( new LineOfCells( currentLine, lineIndex ), FigureDirection.DiagonalLeft, cell => lineMap[cell] );

        // diagonal right-tom to left-bottom
        startCol = Math.Min( move.Col + move.Row, Board.Size - 1 );
        startRow = Math.Max( move.Row - ( Board.Size - 1 - move.Col ), 0 );
        lineIndex = 0;
        for ( int c = startCol, r = startRow; c > 0 && r < Board.Size; c--, r++, lineIndex++ )
        {
            currentLine[lineIndex] = (int)Board[c, r].Stone;
            lineMap[lineIndex] = ( c, r );
            if ( c != move.Col && r != move.Row && Board[c, r].Stone == Stone.None )
                affectedCells.Add( new Coord( c, r ) );
        }
        ProcessRow( new LineOfCells( currentLine, lineIndex ), FigureDirection.DiagonalRight, cell => lineMap[cell] );

        return affectedCells;
    }

    #endregion

    public BoardFiguresAnalyser( IBoard board, Stone targetStone )
    {
        Board = board;
        TargetStone = targetStone;

        FiguresMap = new Dictionary<FigureDirection, FigureType>[Board.Size, Board.Size];
        InitializeFiguresMap();

        Board.StoneMoved += ( _, move ) =>
        {
            var affectedCells = ProcessMove( move );

            // notify about move analysed with figures
            //  which are not 'potential' anymore but 'actual' in this context
            //  as the move is already processed for the cell
            // be careful - clearing the figures map for the cell is done must be done after the event
            MoveAnalysed?.Invoke( this, (move, FiguresMap[move.Col, move.Row], affectedCells) );
            FiguresMap[move.Col, move.Row] = OccupiedCell;
        };
    }

    public readonly IBoard Board;
    public Stone TargetStone { get; init; }
    
    public IReadOnlyDictionary<FigureDirection, FigureType> this[int col, int row] => FiguresMap[col, row];
    
    public event EventHandler<(
        Move move,
        Dictionary<FigureDirection, FigureType> figures,
        List<Coord> affectedCells)>? MoveAnalysed;
}