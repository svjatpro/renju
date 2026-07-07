namespace Renju.Core.BoardAnalyser;

// Per-cell figures for the four line directions, passed and stored by value.
// Replaces a per-cell Dictionary so the whole figures map clones as a single
// array copy — node cloning is the Graph AI's hot path.
internal readonly struct CellFigures( FigureType horizontal, FigureType vertical, FigureType diagonalLeft, FigureType diagonalRight )
{
    public FigureType this[FigureDirection direction] => direction switch
    {
        FigureDirection.Horizontal => horizontal,
        FigureDirection.Vertical => vertical,
        FigureDirection.DiagonalLeft => diagonalLeft,
        FigureDirection.DiagonalRight => diagonalRight,
        _ => FigureType.None
    };

    public bool Contains( FigureType figure ) => Count( figure ) > 0;

    public int Count( FigureType figure ) =>
        ( horizontal == figure ? 1 : 0 ) +
        ( vertical == figure ? 1 : 0 ) +
        ( diagonalLeft == figure ? 1 : 0 ) +
        ( diagonalRight == figure ? 1 : 0 );
}
