namespace Renju.CommandLine.Configuration;

/// <summary>Game settings as given by a single source (CLI or config file); null = not set.</summary>
public record ConfigLayer
{
    public int? Board { get; init; }
    public PlayerSpec? Black { get; init; }
    public PlayerSpec? White { get; init; }

    /// <summary>On the CLI layer, non-null also means "--arena was given" (arena mode).</summary>
    public ArenaSpec? Arena { get; init; }
}
