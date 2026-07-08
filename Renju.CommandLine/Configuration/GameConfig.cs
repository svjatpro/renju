namespace Renju.CommandLine.Configuration;

/// <summary>Fully resolved, validated settings the game runs with.</summary>
public record GameConfig
{
    public required int Board { get; init; }
    public required PlayerConfig Black { get; init; }
    public required PlayerConfig White { get; init; }

    /// <summary>Set only in arena mode (CLI --arena).</summary>
    public ArenaConfig? Arena { get; init; }
}

public record PlayerConfig
{
    public required PlayerType Type { get; init; }
    public int Depth { get; init; } = 3;
    public int TopK { get; init; } = 10;
    public int Timeout { get; init; } = 0;
    public int MinDelay { get; init; } = 0;
}
