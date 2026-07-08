namespace Renju.CommandLine.Configuration;

public static class ConfigResolver
{
    /// <summary>Merge precedence: defaults &lt; config file &lt; CLI, per value (R6).</summary>
    public static GameConfig Resolve( ConfigLayer? file, ConfigLayer cli )
    {
        var config = new GameConfig
        {
            Board = cli.Board ?? file?.Board ?? 19,
            Black = MergePlayer( file?.Black, cli.Black, defaultType: PlayerType.Human ),
            White = MergePlayer( file?.White, cli.White, defaultType: PlayerType.Graph ),
        };

        Validate( config.Board is >= 5 and <= 19, $"board must be 5-19 (got {config.Board})" );
        ValidatePlayer( config.Black, "black player" );
        ValidatePlayer( config.White, "white player" );
        return config;
    }

    private static PlayerSpec? DiscardOnTypeChange( PlayerSpec? file, PlayerSpec? cli ) =>
        // CLI changing the player's type invalidates the file's options for it —
        // they belonged to a different type (R6).
        cli?.Type != null && file?.Type != null && cli.Type != file.Type ? null : file;

    private static PlayerConfig MergePlayer( PlayerSpec? file, PlayerSpec? cli, PlayerType defaultType )
    {
        file = DiscardOnTypeChange( file, cli );
        var defaults = new PlayerConfig { Type = cli?.Type ?? file?.Type ?? defaultType };

        return defaults with
        {
            Depth = cli?.Depth ?? file?.Depth ?? defaults.Depth,
            TopK = cli?.TopK ?? file?.TopK ?? defaults.TopK,
            Timeout = cli?.Timeout ?? file?.Timeout ?? defaults.Timeout,
            MinDelay = cli?.MinDelay ?? file?.MinDelay ?? defaults.MinDelay,
        };
    }

    private static void ValidatePlayer( PlayerConfig player, string context )
    {
        Validate( player.Depth is >= 1 and <= 8, $"{context}: depth must be 1-8 (got {player.Depth})" );
        Validate( player.TopK is >= 1 and <= 50, $"{context}: topK must be 1-50 (got {player.TopK})" );
        Validate( player.Timeout is 0 or >= 50 and <= 60000, $"{context}: timeout must be 0 (off) or 50-60000 ms (got {player.Timeout})" );
        Validate( player.MinDelay is >= 0 and <= 10000, $"{context}: minDelay must be 0-10000 ms (got {player.MinDelay})" );
    }

    private static void Validate( bool condition, string message )
    {
        if ( !condition ) throw new ConfigException( message );
    }
}
