namespace Renju.Core.BoardAnalyser;

internal class BoardWeightsAnalyser
{
    #region Private fields

    private static readonly Dictionary<FigureType, int> FigureWeights = new()
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
        { FigureType.SixOrMore, 1201 },
        { FigureType.Five, 1201 }
    };
    
    private int[,] Weights;

    #endregion

    public BoardWeightsAnalyser(BoardFiguresAnalyser analyser)
    {
        Analyser = analyser;
        Weights = new int[Analyser.Board.Size, Analyser.Board.Size];

        Analyser.MoveAnalysed += (_, e) =>
        {
            var (_, _, affectedCells) = e;
            foreach (var cell in affectedCells)
            {
                var cellFigures = analyser[cell.Col, cell.Row];
                var weight = cellFigures.Values.Sum(f => FigureWeights[f]);
                Weights[cell.Col, cell.Row] = weight;
            }
        };
    }

    public readonly BoardFiguresAnalyser Analyser;
    public IBoard Board => Analyser.Board;
    public Stone TargetStone => Analyser.TargetStone;
    
    public int this[int col, int row] => Weights[col, row];
}