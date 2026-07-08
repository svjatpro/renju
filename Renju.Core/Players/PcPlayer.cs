using Renju.Core.BoardAnalyser;

namespace Renju.Core.Players;

public class PcPlayer( string name, AiType type = AiType.Graph, GraphConfig? config = null, Random? random = null )
    : Player( name )
{
    private IBoardAnalyser? BoardAnalyser;

    public override void StartGame( Stone playersColor, IBoard board, IReferee referee )
    {
        base.StartGame( playersColor, board, referee );
        BoardAnalyser = type switch
        {
            AiType.Plain => new BoardAnalyserPlain( playersColor, board, referee, random ),
            _ => new BoardAnalyserGraph( playersColor, board, referee, config, random ),
        };
    }

    public override bool TryProceedMove(out Move move)
    {
        return BoardAnalyser!.TryProceedNextMove( out move );
    }
}
