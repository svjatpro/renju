using System.Diagnostics;
using Renju.Core;

namespace Renju.CommandLine;

/// <summary>
/// Enforces a minimum wall-clock time per move so fast AIs stay watchable:
/// if the wrapped player answers sooner, the rest of the delay is slept off.
/// </summary>
internal class PlayerDelayWrapper( IPlayer player, int minDelayMs ) : Player( player.Name )
{
    public override void StartGame( Stone playersColor, IBoard board, IReferee referee )
    {
        base.StartGame( playersColor, board, referee );
        player.StartGame( playersColor, board, referee );
    }

    public override bool TryProceedMove( out Move move )
    {
        var watch = Stopwatch.StartNew();
        var result = player.TryProceedMove( out move );

        var remainder = minDelayMs - (int)watch.ElapsedMilliseconds;
        if ( result && remainder > 0 ) Thread.Sleep( remainder );

        return result;
    }
}
