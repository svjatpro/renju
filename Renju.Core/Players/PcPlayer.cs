using Renju.Core.BoardAnalyser;

namespace Renju.Core.Players;

public class PcPlayer( string name ) : Player( name )
{
    private IBoardAnalyser? BoardAnalyser;

    public override void StartGame( Stone playersColor, IBoard board, IReferee referee )
    {
        base.StartGame( playersColor, board, referee );
        BoardAnalyser = new BoardAnalyserGraph( playersColor, board, referee );
        //BoardAnalyser = new BoardAnalyserPlain( playersColor, board, referee );
    }

    public override bool TryProceedMove(out Move move)
    {
        return BoardAnalyser!.TryProceedNextMove( out move );
    }

    //public override int GetDebug( int col, int row, StoneRole role )
    //{
    //    var stone = role == StoneRole.Self ? Stone : Stone.Opposite();
    //    return WeightsAnalysers[stone][col, row];
    //}
}