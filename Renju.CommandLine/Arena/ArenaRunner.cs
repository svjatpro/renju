using System.Diagnostics;
using Renju.CommandLine.Configuration;
using Renju.Core;
using Renju.Core.RenjuGame;

namespace Renju.CommandLine.Arena;

/// <summary>
/// Plays two configured AI players against each other N times and aggregates results.
/// P1 is the spec given as black, P2 as white; with alternation they swap colors every
/// other game. minDelay is deliberately not applied — the arena runs at full speed (R5).
/// </summary>
public class ArenaRunner( GameConfig config )
{
    private readonly ArenaConfig Arena = config.Arena
        ?? throw new ArgumentException( "config has no arena section", nameof(config) );

    public ArenaResult Run( Action<ArenaResult>? onGameFinished = null )
    {
        var masterSeed = Arena.Seed ?? Random.Shared.Next();
        var result = new ArenaResult
        {
            MasterSeed = masterSeed,
            P1 = new ArenaPlayerResult( PlayerFactory.Describe( config.Black ) ),
            P2 = new ArenaPlayerResult( PlayerFactory.Describe( config.White ) ),
        };

        var watch = Stopwatch.StartNew();
        for ( var index = 0; index < Arena.Games; index++ )
        {
            PlayGame( index, masterSeed, result );
            result.GamesPlayed = index + 1;
            result.Elapsed = watch.Elapsed;
            onGameFinished?.Invoke( result );
        }

        return result;
    }

    private void PlayGame( int index, int masterSeed, ArenaResult result )
    {
        var swapped = Arena.Alternate && index % 2 == 1;
        var (blackConfig, blackStats) = swapped ? (config.White, result.P2) : (config.Black, result.P1);
        var (whiteConfig, whiteStats) = swapped ? (config.Black, result.P1) : (config.White, result.P2);

        // deterministic per-game, per-color seeds: same master seed => same series
        var black = PlayerFactory.CreateAi( blackConfig, "black", new Random( unchecked(masterSeed + index * 2) ) );
        var white = PlayerFactory.CreateAi( whiteConfig, "white", new Random( unchecked(masterSeed + index * 2 + 1) ) );
        var game = new RenjuGame( config.Board, black, white );

        var moves = 0;
        var maxMoves = config.Board * config.Board;
        var failed = false;
        var moveWatch = new Stopwatch();

        while ( !game.Referee.IsGameOver && moves < maxMoves )
        {
            var moverStats = game.CurrentPlayer == black ? blackStats : whiteStats;
            moveWatch.Restart();
            if ( !game.TryProceedMove() )
            {
                failed = true;
                break;
            }
            moverStats.MoveTimesMs.Add( moveWatch.ElapsedMilliseconds );
            moves++;
        }

        result.TotalMoves += moves;
        if ( failed || !game.Referee.IsGameOver )
            result.Stuck++; // player failed to move, or the board² cap was hit
        else if ( game.Referee.Winner == Stone.Black )
            blackStats.WinsAsBlack++;
        else if ( game.Referee.Winner == Stone.White )
            whiteStats.WinsAsWhite++;
        else
            result.Draws++;
    }
}
