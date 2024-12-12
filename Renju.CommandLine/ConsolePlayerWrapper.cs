using Renju.Core;

namespace Renju.CommandLine;

internal class ConsolePlayerWrapper( IPlayer player, Func<bool> readKeys ) : Player( player.Name )
{
    public override bool TryProceedMove(out Move move)
    {
        if ( readKeys.Invoke() )
            return player.TryProceedMove( out move );

        move = default!;
        return false;
    }

    public override void StartGame( Stone playersColor, IBoard board, IReferee referee )
    {
        player.StartGame( playersColor, board, referee );
    }

    //public override int GetDebug( int col, int row, StoneRole role )
    //{
    //    return player.GetDebug( col, row, role );
    //}
}