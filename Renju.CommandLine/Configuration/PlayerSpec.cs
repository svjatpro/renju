namespace Renju.CommandLine.Configuration;

public enum PlayerType
{
    Human,
    Plain,
    Graph,
}

/// <summary>
/// One player's settings as given by a single source (CLI or config file).
/// Null = not set by that source; merging happens in <see cref="ConfigResolver"/>.
/// </summary>
public record PlayerSpec
{
    public PlayerType? Type { get; init; }
    public int? Depth { get; init; }
    public int? TopK { get; init; }
    public int? Timeout { get; init; }
    public int? MinDelay { get; init; }

    /// <summary>Options must match the spec's own type (R7): human takes none, depth/topK are graph-only.</summary>
    public void ValidateApplicability( string context )
    {
        if ( Type == PlayerType.Human && (Depth ?? TopK ?? Timeout ?? MinDelay) != null )
            throw new ConfigException( $"{context}: human takes no options" );
        if ( Type == PlayerType.Plain && (Depth ?? TopK) != null )
            throw new ConfigException( $"{context}: option '{( Depth != null ? "depth" : "topK" )}' applies to graph only" );
    }
}
