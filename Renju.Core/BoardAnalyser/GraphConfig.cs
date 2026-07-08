namespace Renju.Core.BoardAnalyser;

public record GraphConfig
{
    public int Depth { get; init; } = 3;
    public int TopK { get; init; } = 10;

    /// <summary>Hard cap on thinking time per move, ms; 0 = unlimited.</summary>
    public int TimeoutMs { get; init; } = 0;
}
