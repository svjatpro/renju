using Renju.Core.BoardAnalyser;

namespace Renju.Core.Players;

public class PcPlayer( string name ) : Player( name )
{
    private Dictionary<Stone, BoardAnalyzer> BoardAnalyzers = null!;
    private int[,,] BoardWeights = null!;

    private int Center;
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

    public override void StartGame( Stone playersColor, IBoard board, IReferee referee )
    {
        base.StartGame( playersColor, board, referee );
        
        Center = (Board.Size / 2) - 1;
        BoardWeights = new int[Board.Size, Board.Size, 2];

        BoardAnalyzers = new()
        {
            {Stone.Black, new BoardAnalyzer(Board, Stone.Black)},
            {Stone.White, new BoardAnalyzer(Board, Stone.White)}
        };


        foreach ( var boardAnalyzer in BoardAnalyzers.Values )
        {
            boardAnalyzer.MoveAnalysed += ( sender, e ) =>
            {
                var analyzer = (IBoardAnalyser) sender!;
                var (_, _, affectedCells) = e;
                foreach ( var cell in affectedCells )
                {
                    var cellFigures = analyzer[cell.col, cell.row];
                    var weight = cellFigures.Values.Sum( f => FigureWeights[f] );
                    BoardWeights[cell.col, cell.row, (int) analyzer.TargetStone - 1] = weight;
                }
            };
        }
    }

    public override bool TryProceedMove(out Move move)
    {
        Thread.Sleep( 300 );

        var bestWeight = 0;
        var centerCoef = 0;
        move = new Move( 0, 0, Stone );
        for ( var col = 0; col < Board.Size; col++ )
        {
            for ( var row = 0; row < Board.Size; row++ )
            {
                var cellWeight = BoardWeights[col, row, 0] + BoardWeights[col, row, 1];
                if ( Board[col, row].Stone != Stone.None ||
                     cellWeight < bestWeight ||
                     !Referee.MoveAllowed( col, row, Stone ) )
                {
                    continue;
                }

                var coef = Math.Abs( col - Center ) + Math.Abs( row - Center );
                if ( cellWeight > bestWeight || (
                     cellWeight == bestWeight && ( centerCoef == 0 || coef < centerCoef ) ) )
                {
                    bestWeight = cellWeight;
                    move = new Move( col, row, Stone );
                    centerCoef = coef;
                }
            }
        }
        return true;
    }
}