using Renju.Core.BoardAnalyser;
using Renju.Core.Extensions;

namespace Renju.Core.Players;


public class PcPlayer( string name ) : Player( name )
{
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
    }

    public override bool TryProceedMove(out Move move)
    {
        Thread.Sleep( 300 );

        var opponent = Stone.Opposite();
        var bestWeight = 0;
        var centerCoef = 0;
        move = new Move( 0, 0, Stone );
        for ( var col = 0; col < Board.Size; col++ )
        {
            for ( var row = 0; row < Board.Size; row++ )
            {
                if ( Board[col, row].Stone != Stone.None ||
                     !Referee.MoveAllowed( col, row, Stone ) )
                {
                    continue;
                }

                // weight of self move
                var cellWeight = WeightsAnalysers[Stone][col, row];

                // consider weight of an opponent's move
                if ( Referee.MoveAllowed( col, row, opponent, ignoreSequence: true ) )
                {
                    cellWeight += WeightsAnalysers[Stone.Opposite()][col, row];
                }
                if (cellWeight < bestWeight ) continue;

                var coef = Math.Abs( col - Center ) + Math.Abs( row - Center );
                if ( cellWeight > bestWeight || (
                     cellWeight == bestWeight && ( centerCoef == 0 || coef < centerCoef ) ) )
                {
                    bestWeight = cellWeight;
                    move = new Move( col, row, Stone );
                    centerCoef = coef;
                }

                // todo: remove temporary code - debug near-win combinations
                //return true;
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