namespace Renju.CommandLine.Configuration;

/// <summary>Arena settings as given by a single source (CLI or config file); null = not set.</summary>
public record ArenaSpec
{
    public int? Games { get; init; }
    public int? Seed { get; init; }
    public bool? Alternate { get; init; }
}

/// <summary>Fully resolved arena settings; present on <see cref="GameConfig"/> only in arena mode.</summary>
public record ArenaConfig
{
    public required int Games { get; init; }
    public bool Alternate { get; init; } = true;

    /// <summary>Master seed; null = pick randomly at run start (the run reports which).</summary>
    public int? Seed { get; init; }
}
