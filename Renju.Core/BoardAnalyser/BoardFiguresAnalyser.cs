namespace Renju.Core.BoardAnalyser;

internal class BoardFiguresAnalyser : IDisposable
{
    #region Private fields

    // Flat figures map: 4 direction slots per cell, [(col * Size + row) * 4 + slot].
    // A flat value array (vs a Dictionary per cell) makes Clone a single memcpy.
    private readonly FigureType[] FiguresMap;
    private const int DirectionCount = 4;

    #endregion

    #region Private methods

    private int CellIndex( int col, int row ) => ( col * Board.Size + row ) * DirectionCount;

    // FigureDirection enum values are 1..4
    private static int DirectionSlot( FigureDirection direction ) => (int)direction - 1;

    private void ClearCell( int col, int row )
    {
        var idx = CellIndex( col, row );
        for ( var slot = 0; slot < DirectionCount; slot++ )
            FiguresMap[idx + slot] = FigureType.None;
    }

    private void ProcessRow(
        LineOfCells row,
        FigureDirection direction,
        Func<int,(int col, int row)> cellResolver)
    {
        var rowFigures = RowParser.DefineBestFigures( row, TargetStone );
        for ( var i = 0; i < row.Length; i++ )
        {
            if ( !rowFigures.ContainsKey( i ) ) continue;
            var cell = cellResolver( i );
            FiguresMap[CellIndex( cell.col, cell.row ) + DirectionSlot( direction )] = rowFigures[i];
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

    public BoardFiguresAnalyser(
        IBoard board,
        Stone targetStone,
        FigureType[]? figuresMap = null )
    {
        Board = board;
        TargetStone = targetStone;
        FiguresMap = figuresMap ?? new FigureType[board.Size * board.Size * DirectionCount];

        Board.StoneMoved += ( _, move ) =>
        {
            var affectedCells = ProcessMove( move );

            // notify about move analysed with figures
            //  which are not 'potential' anymore but 'actual' in this context
            //  as the move is already processed for the cell
            // the figures are captured by value, so clearing the cell right
            //  after is safe even if a handler keeps the payload
            MoveAnalysed?.Invoke( this, (move, this[move.Col, move.Row], affectedCells) );
            ClearCell( move.Col, move.Row );
        };
    }

    public readonly IBoard Board;
    public Stone TargetStone { get; init; }

    public CellFigures this[int col, int row]
    {
        get
        {
            var idx = CellIndex( col, row );
            return new CellFigures( FiguresMap[idx], FiguresMap[idx + 1], FiguresMap[idx + 2], FiguresMap[idx + 3] );
        }
    }

    public event EventHandler<(
        Move move,
        CellFigures figures,
        List<Coord> affectedCells)>? MoveAnalysed;

    public BoardFiguresAnalyser Clone( IBoard board ) =>
        new( board, TargetStone, (FigureType[])FiguresMap.Clone() );

    public void Dispose()
    {
        Array.Clear( FiguresMap );
    }
}
