using FluentAssertions;
using NUnit.Framework;
using Renju.CommandLine.Configuration;

namespace Renju.CommandLine.Tests;

[TestFixture]
public class ArenaConfigTests
{
    // --- CLI parsing ---

    [Test]
    public void ArenaWithCount_IsParsed()
    {
        var result = CliParser.Parse( ["--arena", "100"] );

        result.Config.Arena.Should().Be( new ArenaSpec { Games = 100 } );
    }

    [Test]
    public void ArenaWithoutCount_LeavesGamesNull()
    {
        var result = CliParser.Parse( ["--arena", "--black", "plain"] );

        result.Config.Arena.Should().Be( new ArenaSpec() );
        result.Config.Black!.Type.Should().Be( PlayerType.Plain );
    }

    [Test]
    public void SeedAndNoAlternate_AreParsed()
    {
        var result = CliParser.Parse( ["--arena", "50", "--seed", "42", "--no-alternate"] );

        result.Config.Arena.Should().Be( new ArenaSpec { Games = 50, Seed = 42, Alternate = false } );
    }

    [Test]
    public void NoArenaFlag_MeansNoArenaSpec()
    {
        CliParser.Parse( ["--black", "plain"] ).Config.Arena.Should().BeNull();
    }

    [TestCase( new[] { "--seed", "42" }, "'--seed' requires '--arena'" )]
    [TestCase( new[] { "--no-alternate" }, "'--no-alternate' requires '--arena'" )]
    [TestCase( new[] { "--arena", "5", "--arena", "5" }, "'--arena' given twice" )]
    [TestCase( new[] { "--arena", "many" }, "arena games must be an integer (got 'many')" )]
    public void InvalidArenaArgs_Throw( string[] args, string message )
    {
        var act = () => CliParser.Parse( args );

        act.Should().Throw<ConfigException>().WithMessage( message );
    }

    // --- resolving ---

    private static ConfigLayer AiPlayers( ArenaSpec? arena = null ) => new()
    {
        Black = new PlayerSpec { Type = PlayerType.Plain },
        White = new PlayerSpec { Type = PlayerType.Graph },
        Arena = arena,
    };

    [Test]
    public void FileArenaSection_WithoutCliFlag_DoesNotTriggerArenaMode()
    {
        var file = new ConfigLayer { Arena = new ArenaSpec { Games = 100 } };

        var config = ConfigResolver.Resolve( file, AiPlayers() );

        config.Arena.Should().BeNull();
    }

    [Test]
    public void GamesAndSeed_FallBackToFile()
    {
        var file = new ConfigLayer { Arena = new ArenaSpec { Games = 100, Seed = 42, Alternate = false } };

        var config = ConfigResolver.Resolve( file, AiPlayers( arena: new ArenaSpec() ) );

        config.Arena.Should().Be( new ArenaConfig { Games = 100, Seed = 42, Alternate = false } );
    }

    [Test]
    public void CliArenaValues_OverrideFile()
    {
        var file = new ConfigLayer { Arena = new ArenaSpec { Games = 100, Seed = 42 } };

        var config = ConfigResolver.Resolve( file, AiPlayers( arena: new ArenaSpec { Games = 10 } ) );

        config.Arena.Should().Be( new ArenaConfig { Games = 10, Seed = 42, Alternate = true } );
    }

    [Test]
    public void GamesNotSetAnywhere_Throws()
    {
        var act = () => ConfigResolver.Resolve( null, AiPlayers( arena: new ArenaSpec() ) );

        act.Should().Throw<ConfigException>().WithMessage( "arena: number of games not set*" );
    }

    [TestCase( 0 )]
    [TestCase( 1000001 )]
    public void GamesOutOfRange_Throws( int games )
    {
        var act = () => ConfigResolver.Resolve( null, AiPlayers( arena: new ArenaSpec { Games = games } ) );

        act.Should().Throw<ConfigException>().WithMessage( $"arena: games must be 1-1000000 (got {games})" );
    }

    [Test]
    public void HumanPlayer_InArenaMode_Throws()
    {
        var cli = new ConfigLayer
        {
            White = new PlayerSpec { Type = PlayerType.Plain },
            Arena = new ArenaSpec { Games = 10 },
        };

        // black defaults to human
        var act = () => ConfigResolver.Resolve( null, cli );

        act.Should().Throw<ConfigException>().WithMessage( "arena: both players must be AI (black is human)" );
    }
}
