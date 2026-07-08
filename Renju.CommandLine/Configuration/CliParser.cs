namespace Renju.CommandLine.Configuration;

public record CliResult
{
    public ConfigLayer Config { get; init; } = new();
    public string? ConfigPath { get; init; }
    public bool Help { get; init; }
}

public static class CliParser
{
    public static CliResult Parse( string[] args )
    {
        int? board = null;
        PlayerSpec? black = null, white = null;
        string? configPath = null;
        var arenaMode = false;
        int? arenaGames = null, seed = null;
        bool? alternate = null;

        for ( var i = 0; i < args.Length; i++ )
        {
            switch ( args[i].ToLowerInvariant() )
            {
                case "--help" or "-h":
                    return new CliResult { Help = true };
                case "--arena":
                    if ( arenaMode ) throw new ConfigException( "'--arena' given twice" );
                    arenaMode = true;
                    // the game count is optional here — it may come from the config file
                    if ( i + 1 < args.Length && !args[i + 1].StartsWith( "--" ) )
                        arenaGames = ParseInt( args[++i], "arena games" );
                    break;
                case "--seed":
                    if ( seed != null ) throw new ConfigException( "'--seed' given twice" );
                    seed = ParseInt( RequireValue( args, ref i ), "seed" );
                    break;
                case "--no-alternate":
                    alternate = false;
                    break;
                case "--black":
                    if ( black != null ) throw new ConfigException( "'--black' given twice" );
                    black = ParsePlayerSpec( RequireValue( args, ref i ), "black player" );
                    break;
                case "--white":
                    if ( white != null ) throw new ConfigException( "'--white' given twice" );
                    white = ParsePlayerSpec( RequireValue( args, ref i ), "white player" );
                    break;
                case "--board":
                    if ( board != null ) throw new ConfigException( "'--board' given twice" );
                    board = ParseInt( RequireValue( args, ref i ), "board" );
                    break;
                case "--config":
                    if ( configPath != null ) throw new ConfigException( "'--config' given twice" );
                    configPath = RequireValue( args, ref i );
                    break;
                default:
                    throw new ConfigException( $"unknown argument '{args[i]}'" );
            }
        }

        if ( !arenaMode && ( seed != null || alternate != null ) )
            throw new ConfigException( $"'{( seed != null ? "--seed" : "--no-alternate" )}' requires '--arena'" );

        return new CliResult
        {
            Config = new ConfigLayer
            {
                Board = board,
                Black = black,
                White = white,
                Arena = arenaMode ? new ArenaSpec { Games = arenaGames, Seed = seed, Alternate = alternate } : null,
            },
            ConfigPath = configPath,
        };
    }

    /// <summary>Spec grammar: human | plain[:opt=val,...] | graph[:opt=val,...]</summary>
    public static PlayerSpec ParsePlayerSpec( string spec, string context )
    {
        var colon = spec.IndexOf( ':' );
        var typeToken = colon < 0 ? spec : spec[..colon];

        var type = typeToken.ToLowerInvariant() switch
        {
            "human" => PlayerType.Human,
            "plain" => PlayerType.Plain,
            "graph" => PlayerType.Graph,
            _ => throw new ConfigException( $"{context}: unknown player type '{typeToken}' (human | plain | graph)" ),
        };

        var result = new PlayerSpec { Type = type };
        if ( colon >= 0 )
        {
            foreach ( var pair in spec[( colon + 1 )..].Split( ',' ) )
            {
                var eq = pair.IndexOf( '=' );
                if ( eq <= 0 ) throw new ConfigException( $"{context}: expected opt=value, got '{pair}'" );

                var key = pair[..eq].Trim();
                var value = ParseInt( pair[( eq + 1 )..].Trim(), $"{context}: option '{key}'" );

                result = key.ToLowerInvariant() switch
                {
                    "depth" => Once( result.Depth, key, context ) with { Depth = value },
                    "topk" => Once( result.TopK, key, context ) with { TopK = value },
                    "timeout" => Once( result.Timeout, key, context ) with { Timeout = value },
                    "mindelay" => Once( result.MinDelay, key, context ) with { MinDelay = value },
                    _ => throw new ConfigException( $"{context}: unknown option '{key}' (depth, topK, timeout, minDelay)" ),
                };
            }
        }

        result.ValidateApplicability( context );
        return result;

        PlayerSpec Once( int? current, string key, string ctx ) =>
            current == null ? result : throw new ConfigException( $"{ctx}: option '{key}' set twice" );
    }

    private static string RequireValue( string[] args, ref int i )
    {
        if ( i + 1 >= args.Length ) throw new ConfigException( $"'{args[i]}' requires a value" );
        return args[++i];
    }

    private static int ParseInt( string value, string context ) =>
        int.TryParse( value, out var result )
            ? result
            : throw new ConfigException( $"{context} must be an integer (got '{value}')" );
}
