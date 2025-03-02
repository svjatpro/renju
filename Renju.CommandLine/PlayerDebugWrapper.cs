using Renju.Core;

namespace Renju.CommandLine;

/// <summary>
/// Wrapper for player that allows to debug the game by reading keys from console.
/// </summary>
/// <param name="player"></param>
/// <param name="readKeys"></param>
internal class PlayerDebugWrapper( IPlayer player, Func<bool> readKeys ) : Player( player.Name )
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