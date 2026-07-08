namespace Renju.CommandLine.Arena;

public class ArenaResult
{
    public required int MasterSeed { get; init; }
    public required ArenaPlayerResult P1 { get; init; }
    public required ArenaPlayerResult P2 { get; init; }

    public int GamesPlayed { get; set; }
    public int Draws { get; set; }
    public int Stuck { get; set; }
    public long TotalMoves { get; set; }
    public TimeSpan Elapsed { get; set; }

    public double AvgMovesPerGame => GamesPlayed == 0 ? 0 : (double)TotalMoves / GamesPlayed;
}

public class ArenaPlayerResult( string label )
{
    public string Label { get; } = label;
    public int WinsAsBlack { get; set; }
    public int WinsAsWhite { get; set; }
    public int Wins => WinsAsBlack + WinsAsWhite;
    public List<long> MoveTimesMs { get; } = [];

    public (long P50, long P95, long Max) MoveTimePercentiles()
    {
        if ( MoveTimesMs.Count == 0 ) return (0, 0, 0);
        var sorted = MoveTimesMs.Order().ToList();
        return (
            sorted[sorted.Count / 2],
            sorted[Math.Min( (int)( sorted.Count * 0.95 ), sorted.Count - 1 )],
            sorted[^1]);
    }
}
