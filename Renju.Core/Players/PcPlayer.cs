using Renju.Core.BoardAnalyser;
using Renju.Core.Extensions;

namespace Renju.Core.Players;

public class PcPlayer( string name ) : Player( name )
{
    //private Dictionary<Stone, BoardAnalyzer> BoardAnalyzers = null!;
    //private int[,,] BoardWeights = null!;
    private Dictionary<Stone, BoardWeightsAnalyser> WeightsAnalysers = null!;
    private int Center;
    
    public override void StartGame( Stone playersColor, IBoard board, IReferee referee )
    {
        base.StartGame( playersColor, board, referee );

        Center = (Board.Size / 2) - 1;
        WeightsAnalysers = new Dictionary<Stone, BoardWeightsAnalyser>
        {
            { Stone.Black, new BoardWeightsAnalyser( new BoardFiguresAnalyser( Board, Stone.Black ) ) },
            { Stone.White, new BoardWeightsAnalyser( new BoardFiguresAnalyser( Board, Stone.White ) ) },
        };

        //BoardWeights = new int[Board.Size, Board.Size, 2];

        //BoardAnalyzers = new()
        //{
        //    {Stone.Black, new BoardAnalyzer(Board, Stone.Black)},
        //    {Stone.White, new BoardAnalyzer(Board, Stone.White)}
        //};


        //foreach ( var boardAnalyzer in BoardAnalyzers.Values )
        //{
            //boardAnalyzer.MoveAnalysed += ( sender, e ) =>
            //{
            //    var analyser = (IBoardAnalyser) sender!;
            //    var (_, _, affectedCells) = e;
            //    foreach ( var cell in affectedCells )
            //    {
            //        var cellFigures = analyser[cell.col, cell.row];
            //        var weight = cellFigures.Values.Sum( f => FigureWeights[f] );
            //        BoardWeights[cell.col, cell.row, (int) analyser.TargetStone - 1] = weight;
            //    }
            //};
        //}
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
                //var cellWeight = BoardWeights[col, row, 0] + BoardWeights[col, row, 1];
                var cellWeight = WeightsAnalysers.Values.Sum( w => w[col, row] );
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

    public override int GetDebug( int col, int row, StoneRole role )
    {
        var stone = role == StoneRole.Self ? Stone : Stone.Opposite();
        return WeightsAnalysers[stone][col, row];
    }
}