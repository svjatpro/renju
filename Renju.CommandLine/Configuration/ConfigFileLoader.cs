using System.Text.Json;
using System.Text.Json.Serialization;

namespace Renju.CommandLine.Configuration;

public static class ConfigFileLoader
{
    public const string DefaultFileName = "renju.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    };

    /// <summary>
    /// Explicit path must exist; otherwise renju.json is picked up from the current
    /// directory when present. Null = no config file (defaults + CLI only).
    /// </summary>
    public static ConfigLayer? Load( string? explicitPath )
    {
        var path = explicitPath;
        if ( path == null )
        {
            if ( !File.Exists( DefaultFileName ) ) return null;
            path = DefaultFileName;
        }
        else if ( !File.Exists( path ) )
        {
            throw new ConfigException( $"config file not found: {path}" );
        }

        FileModel? model;
        try
        {
            model = JsonSerializer.Deserialize<FileModel>( File.ReadAllText( path ), JsonOptions );
        }
        catch ( JsonException e )
        {
            throw new ConfigException( $"invalid config file '{path}': {e.Message}" );
        }

        return new ConfigLayer
        {
            Board = model?.Board,
            Black = ToSpec( model?.Players?.Black, $"config file: black player" ),
            White = ToSpec( model?.Players?.White, $"config file: white player" ),
            Arena = model?.Arena == null ? null : new ArenaSpec
            {
                Games = model.Arena.Games,
                Seed = model.Arena.Seed,
                Alternate = model.Arena.Alternate,
            },
        };
    }

    private static PlayerSpec? ToSpec( FilePlayer? player, string context )
    {
        if ( player == null ) return null;
        if ( player.Type == null )
            throw new ConfigException( $"{context}: 'type' is required (human | plain | graph)" );

        var spec = new PlayerSpec
        {
            Type = player.Type.ToLowerInvariant() switch
            {
                "human" => PlayerType.Human,
                "plain" => PlayerType.Plain,
                "graph" => PlayerType.Graph,
                _ => throw new ConfigException( $"{context}: unknown player type '{player.Type}' (human | plain | graph)" ),
            },
            Depth = player.Depth,
            TopK = player.TopK,
            Timeout = player.Timeout,
            MinDelay = player.MinDelay,
        };
        spec.ValidateApplicability( context );
        return spec;
    }

    private sealed class FileModel
    {
        public int? Board { get; set; }
        public FilePlayers? Players { get; set; }
        public FileArena? Arena { get; set; }
    }

    private sealed class FileArena
    {
        public int? Games { get; set; }
        public int? Seed { get; set; }
        public bool? Alternate { get; set; }
    }

    private sealed class FilePlayers
    {
        public FilePlayer? Black { get; set; }
        public FilePlayer? White { get; set; }
    }

    private sealed class FilePlayer
    {
        public string? Type { get; set; }
        public int? Depth { get; set; }
        public int? TopK { get; set; }
        public int? Timeout { get; set; }
        public int? MinDelay { get; set; }
    }
}
