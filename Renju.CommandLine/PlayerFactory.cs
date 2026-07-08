using Renju.CommandLine.Configuration;
using Renju.Core;
using Renju.Core.BoardAnalyser;
using Renju.Core.Players;

namespace Renju.CommandLine;

internal static class PlayerFactory
{
    public static IPlayer CreateAi( PlayerConfig player, string name, Random? random = null ) =>
        Player.PcPlayer( name,
            player.Type == PlayerType.Plain ? AiType.Plain : AiType.Graph,
            new GraphConfig { Depth = player.Depth, TopK = player.TopK, TimeoutMs = player.Timeout },
            random );

    /// <summary>Short spec-style label, non-default options only: "plain", "graph:depth=5".</summary>
    public static string Describe( PlayerConfig player )
    {
        if ( player.Type == PlayerType.Human ) return "human";

        var defaults = new PlayerConfig { Type = player.Type };
        var options = new List<string>();
        if ( player.Type == PlayerType.Graph )
        {
            if ( player.Depth != defaults.Depth ) options.Add( $"depth={player.Depth}" );
            if ( player.TopK != defaults.TopK ) options.Add( $"topK={player.TopK}" );
        }
        if ( player.Timeout != defaults.Timeout ) options.Add( $"timeout={player.Timeout}" );

        var name = player.Type == PlayerType.Plain ? "plain" : "graph";
        return options.Count > 0 ? $"{name}:{string.Join( ',', options )}" : name;
    }
}
