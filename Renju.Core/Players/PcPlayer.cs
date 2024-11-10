using Renju.Core.BoardAnalyser;

namespace Renju.Core.Players;

public class PcPlayer( string name ) : Player( name )
{
    private Dictionary<Stone, BoardAnalyzer> BoardAnalyzers = null!;
    private int[,] BoardWeights = null!;

    //#define WEIGHT_FIVE          250 //700 //213
    //#define WEIGHT_OPEN_FOUR     117 //150 //53
    //#define WEIGHT_CLOSE_FOUR    29  //35  //13
    //#define WEIGHT_OPEN_THREE    22  //25  //13
    //#define WEIGHT_CLOSE_THREE   7   //10  //6
    //#define WEIGHT_OPEN_TWO      4   //5   //3
    //#define WEIGHT_CLOSE_TWO     1   //2   //1

    private int Center = 0;
    private static readonly Dictionary<FigureType, int> FigureWeights = new()
    {
        { FigureType.None, 0 },
        { FigureType.ClosedTwo, 5 },
        { FigureType.ClosedTwo1, 6 },
        { FigureType.ClosedTwo2, 7 },
        { FigureType.ClosedTwo3, 8 },
        { FigureType.OpenTwo, 10 },
        { FigureType.ClosedThree, 20 },
        { FigureType.ClosedThree1, 22 },
        { FigureType.ClosedThree2, 24 },
        { FigureType.OpenThree, 50 },
        { FigureType.ClosedFour, 80 },
        { FigureType.OpenFour, 200 },
        { FigureType.SixOrMore, 250 },
        { FigureType.Five, 500 }
    };

    public override void StartGame( Stone playersColor, IBoard board, IReferee referee )
    {
        base.StartGame( playersColor, board, referee );
        
        Center = Board.Size / 2;
        BoardWeights = new int[Board.Size, Board.Size];

        BoardAnalyzers = new()
        {
            {Stone.Black, new BoardAnalyzer(Board, Stone.Black)},
            {Stone.White, new BoardAnalyzer(Board, Stone.White)}
        };

        BoardAnalyzers[Stone].MoveAnalysed += ( sender, e ) =>
        {
            var analyzer = (IBoardAnalyser) sender!;
            var (_, _, affectedCells) = e;
            foreach ( var cell in affectedCells )
            {
                var cellFigures = analyzer[cell.col, cell.row];
                var weight = cellFigures.Values.Sum( f => FigureWeights[f] );
                BoardWeights[cell.col, cell.row] = weight;
            }
        };
    }

    public override bool TryProceedMove(out Move move)
    {
        Thread.Sleep( 200 );

        var bestWeight = 0;
        var centerCoef = 0;
        move = new Move( 0, 0, Stone );
        for ( var col = 0; col < Board.Size; col++ )
        {
            for ( var row = 0; row < Board.Size; row++ )
            {
                if ( Board[col, row].Stone != Stone.None ||
                     BoardWeights[col, row] < bestWeight ||
                     !Referee.MoveAllowed( col, row, Stone ) )
                {
                    continue;
                }

                var coef = Math.Abs( col - Center ) + Math.Abs( row - Center );
                if ( BoardWeights[col, row] > bestWeight || (
                     BoardWeights[col, row] == bestWeight && ( centerCoef == 0 || coef < centerCoef ) ) )
                {
                    bestWeight = BoardWeights[col, row];
                    move = new Move( col, row, Stone );
                    centerCoef = coef;
                }
            }
        }
        return true;
    }
}