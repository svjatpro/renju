namespace Renju.Core.BoardAnalyser;

public record GraphConfig
{
    public int Depth { get; init; } = 3;
    public int TopK { get; init; } = 10;
}
