using Renju.Core.Extensions;

namespace Renju.Core.BoardAnalyser;

public class BoardAnalyserPlain : IBoardAnalyser
{
    private readonly Stone AiStone;
    private readonly IBoard Board;
    private readonly IReferee Referee;
    private readonly BoardWeightsAnalyser BlackWeights;
    private readonly BoardWeightsAnalyser WhiteWeights;
    private readonly Random Random;

    public BoardAnalyserPlain( Stone stone, IBoard board, IReferee referee, Random? random = null )
    {
        AiStone = stone;
        Board = board;
        Referee = referee;
        Random = random ?? Random.Shared;
        BlackWeights = new BoardWeightsAnalyser( new BoardFiguresAnalyser( board, Stone.Black ) );
        WhiteWeights = new BoardWeightsAnalyser( new BoardFiguresAnalyser( board, Stone.White ) );
    }

    public bool TryProceedNextMove( out Move move )
    {
        var opponent = AiStone.Opposite();
        var selfWeights = AiStone == Stone.Black ? BlackWeights : WhiteWeights;
        var oppWeights = AiStone == Stone.Black ? WhiteWeights : BlackWeights;
        var size = Board.Size;
        var center = size / 2 - 1;

        var bestWeight = -1;
        var bestCenterDist = int.MaxValue;
        var tieCount = 0;
        var found = false;
        move = new Move( 0, 0, AiStone );

        for ( var col = 0; col < size; col++ )
        {
            for ( var row = 0; row < size; row++ )
            {
                if ( Board[col, row].Stone != Stone.None ) continue;
                if ( !Referee.MoveAllowed( col, row, AiStone ) ) continue;

                var w = selfWeights[col, row];
                if ( Referee.MoveAllowed( col, row, opponent, ignoreSequence: true ) )
                {
                    w += oppWeights[col, row];
                }

                var dist = Math.Abs( col - center ) + Math.Abs( row - center );

                if ( w > bestWeight || ( w == bestWeight && dist < bestCenterDist ) )
                {
                    bestWeight = w;
                    bestCenterDist = dist;
                    tieCount = 1;
                    move = new Move( col, row, AiStone );
                    found = true;
                }
                // equal best candidates: pick one uniformly (reservoir sampling),
                // so AI-vs-AI games are not identical replays
                else if ( w == bestWeight && dist == bestCenterDist && Random.Next( ++tieCount ) == 0 )
                {
                    move = new Move( col, row, AiStone );
                }
            }
        }

        return found;
    }
}
