namespace Renju.Core.BoardAnalyser;

internal class BoardWeightsAnalyser : IDisposable
{
    #region Private fields

    public static readonly Dictionary<FigureType, int> FigureWeights = new()
    {
        { FigureType.None, 0 },
        { FigureType.ClosedTwo3, 2 },
        { FigureType.ClosedTwo2, 3 },
        { FigureType.ClosedTwo1, 4 },
        { FigureType.ClosedTwo, 5 },
        { FigureType.OpenTwo, 15 },
        { FigureType.ClosedThree2, 25 },
        { FigureType.ClosedThree1, 27 },
        { FigureType.ClosedThree, 29 },
        { FigureType.OpenThree, 90 },
        { FigureType.ClosedFour, 99 },
        { FigureType.OpenFour, 400 },
        { FigureType.Five, 1202 },
        { FigureType.SixOrMore, 1201 }
    };

    private readonly int[,] Weights;

    #endregion

    public readonly BoardFiguresAnalyser Analyser;
    public IBoard Board => Analyser.Board;
    public Stone TargetStone => Analyser.TargetStone;

    public int this[int col, int row] => Weights[col, row];
    public int FigureWeight( FigureType figure ) => FigureWeights[figure];

    public BoardWeightsAnalyser( BoardFiguresAnalyser analyser, int[,]? weights = null )
    {
        Analyser = analyser;
        Weights = weights ?? new int[Analyser.Board.Size, Analyser.Board.Size];

        Analyser.MoveAnalysed += (_, e) =>
        {
            var (_, _, affectedCells) = e;
            foreach (var cell in affectedCells)
            {
                var cellFigures = analyser[cell.Col, cell.Row];
                Weights[cell.Col, cell.Row] =
                    FigureWeights[cellFigures[FigureDirection.Horizontal]] +
                    FigureWeights[cellFigures[FigureDirection.Vertical]] +
                    FigureWeights[cellFigures[FigureDirection.DiagonalLeft]] +
                    FigureWeights[cellFigures[FigureDirection.DiagonalRight]];
            }
        };
    }

    public BoardWeightsAnalyser Clone( IBoard board )
    {
        var weights = new int[board.Size, board.Size];
        for ( var col = 0; col < board.Size; col++ )
            for ( var row = 0; row < board.Size; row++ )
                weights[col, row] = Weights[col, row];

        var analyser = Analyser.Clone( board );
        return new BoardWeightsAnalyser( analyser, weights );
    }

    public void Dispose()
    {
        Analyser.Dispose();
    }
}