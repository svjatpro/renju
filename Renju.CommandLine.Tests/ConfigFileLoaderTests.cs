using FluentAssertions;
using NUnit.Framework;
using Renju.CommandLine.Configuration;

namespace Renju.CommandLine.Tests;

[TestFixture]
public class ConfigFileLoaderTests
{
    private string Dir = null!;

    [SetUp]
    public void SetUp() => Dir = Directory.CreateTempSubdirectory( "renju-tests-" ).FullName;

    [TearDown]
    public void TearDown() => Directory.Delete( Dir, recursive: true );

    private string WriteFile( string content )
    {
        var path = Path.Combine( Dir, "renju.json" );
        File.WriteAllText( path, content );
        return path;
    }

    [Test]
    public void ValidFile_IsLoaded_CaseInsensitive()
    {
        var path = WriteFile(
            """
            {
              "Board": 15,
              "players": {
                "black": { "type": "human" },
                "white": { "Type": "Graph", "depth": 4, "topk": 8, "TIMEOUT": 2000, "minDelay": 300 }
              }
            }
            """ );

        var layer = ConfigFileLoader.Load( path );

        layer.Should().NotBeNull();
        layer!.Board.Should().Be( 15 );
        layer.Black.Should().Be( new PlayerSpec { Type = PlayerType.Human } );
        layer.White.Should().Be( new PlayerSpec
        {
            Type = PlayerType.Graph, Depth = 4, TopK = 8, Timeout = 2000, MinDelay = 300,
        } );
    }

    [Test]
    public void PartialFile_LeavesOthersNull()
    {
        var layer = ConfigFileLoader.Load( WriteFile( """{ "board": 15 }""" ) );

        layer!.Board.Should().Be( 15 );
        layer.Black.Should().BeNull();
        layer.White.Should().BeNull();
    }

    [Test]
    public void ExplicitPathMissing_Throws()
    {
        var path = Path.Combine( Dir, "nope.json" );

        var act = () => ConfigFileLoader.Load( path );

        act.Should().Throw<ConfigException>().WithMessage( $"config file not found: {path}" );
    }

    [Test]
    public void MalformedJson_Throws()
    {
        var path = WriteFile( "{ not json" );

        var act = () => ConfigFileLoader.Load( path );

        act.Should().Throw<ConfigException>().WithMessage( $"invalid config file '{path}':*" );
    }

    [Test]
    public void UnknownProperty_Throws()
    {
        var path = WriteFile( """{ "boardSize": 15 }""" );

        var act = () => ConfigFileLoader.Load( path );

        act.Should().Throw<ConfigException>().WithMessage( $"invalid config file '{path}':*" );
    }

    [Test]
    public void PlayerWithoutType_Throws()
    {
        var path = WriteFile( """{ "players": { "white": { "depth": 4 } } }""" );

        var act = () => ConfigFileLoader.Load( path );

        act.Should().Throw<ConfigException>()
            .WithMessage( "config file: white player: 'type' is required (human | plain | graph)" );
    }

    [Test]
    public void GraphOptionOnHuman_Throws()
    {
        var path = WriteFile( """{ "players": { "black": { "type": "human", "depth": 4 } } }""" );

        var act = () => ConfigFileLoader.Load( path );

        act.Should().Throw<ConfigException>().WithMessage( "config file: black player: human takes no options" );
    }
}
