using Renju.CommandLine.Configuration;

namespace Renju.CommandLine.Arena;

public static class ArenaReport
{
    public static string Progress( ArenaResult result, int totalGames ) =>
        $"game {result.GamesPlayed}/{totalGames}   P1 {result.P1.Wins} : {result.P2.Wins} P2, draws {result.Draws}";

    public static string Format( ArenaResult result, GameConfig config )
    {
        var (p1P50, p1P95, p1Max) = result.P1.MoveTimePercentiles();
        var (p2P50, p2P95, p2Max) = result.P2.MoveTimePercentiles();
        var stuck = result.Stuck > 0 ? $", stuck {result.Stuck}" : "";

        return
            $"""
             arena: {result.P1.Label} (P1) vs {result.P2.Label} (P2) — {result.GamesPlayed} games, board {config.Board}, seed {result.MasterSeed}

             P1  {result.P1.Label,-20} {result.P1.Wins,4} wins  ({result.P1.WinsAsBlack} as black, {result.P1.WinsAsWhite} as white)
             P2  {result.P2.Label,-20} {result.P2.Wins,4} wins  ({result.P2.WinsAsBlack} as black, {result.P2.WinsAsWhite} as white)
             draws {result.Draws}{stuck}, avg moves/game {result.AvgMovesPerGame:F0}

             move time ms (p50/p95/max):  P1 {p1P50}/{p1P95}/{p1Max}   P2 {p2P50}/{p2P95}/{p2Max}
             total: {result.Elapsed.TotalSeconds:F1} s
             """;
    }
}
