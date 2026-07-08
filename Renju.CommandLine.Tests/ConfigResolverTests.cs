using FluentAssertions;
using NUnit.Framework;
using Renju.CommandLine.Configuration;

namespace Renju.CommandLine.Tests;

[TestFixture]
public class ConfigResolverTests
{
    [Test]
    public void NoInput_YieldsDefaults()
    {
        var config = ConfigResolver.Resolve( file: null, cli: new ConfigLayer() );

        config.Board.Should().Be( 19 );
        config.Black.Should().Be( new PlayerConfig { Type = PlayerType.Human } );
        config.White.Should().Be( new PlayerConfig { Type = PlayerType.Graph, Depth = 3, TopK = 10, Timeout = 0, MinDelay = 0 } );
    }

    [Test]
    public void FileValues_OverrideDefaults()
    {
        var file = new ConfigLayer
        {
            Board = 15,
            White = new PlayerSpec { Type = PlayerType.Graph, Depth = 4, MinDelay = 300 },
        };

        var config = ConfigResolver.Resolve( file, new ConfigLayer() );

        config.Board.Should().Be( 15 );
        config.White.Should().Be( new PlayerConfig { Type = PlayerType.Graph, Depth = 4, TopK = 10, MinDelay = 300 } );
    }

    [Test]
    public void CliValues_OverrideFile_PerValue()
    {
        // scenario 6: file sets topK=20, CLI overrides only depth
        var file = new ConfigLayer { White = new PlayerSpec { Type = PlayerType.Graph, Depth = 4, TopK = 20 } };
        var cli = new ConfigLayer { White = new PlayerSpec { Type = PlayerType.Graph, Depth = 2 } };

        var config = ConfigResolver.Resolve( file, cli );

        config.White.Depth.Should().Be( 2 );
        config.White.TopK.Should().Be( 20 );
    }

    [Test]
    public void CliTypeChange_DiscardsFileOptions()
    {
        var file = new ConfigLayer { White = new PlayerSpec { Type = PlayerType.Graph, Depth = 5, MinDelay = 300 } };
        var cli = new ConfigLayer { White = new PlayerSpec { Type = PlayerType.Plain } };

        var config = ConfigResolver.Resolve( file, cli );

        // options of the discarded graph spec must not leak into the plain player
        config.White.Should().Be( new PlayerConfig { Type = PlayerType.Plain } );
    }

    [Test]
    public void SameTypeInCliAndFile_KeepsFileOptions()
    {
        var file = new ConfigLayer { White = new PlayerSpec { Type = PlayerType.Graph, TopK = 20 } };
        var cli = new ConfigLayer { White = new PlayerSpec { Type = PlayerType.Graph, Timeout = 1000 } };

        var config = ConfigResolver.Resolve( file, cli );

        config.White.TopK.Should().Be( 20 );
        config.White.Timeout.Should().Be( 1000 );
    }

    [TestCase( 4 )]
    [TestCase( 20 )]
    public void BoardOutOfRange_Throws( int board )
    {
        var act = () => ConfigResolver.Resolve( null, new ConfigLayer { Board = board } );

        act.Should().Throw<ConfigException>().WithMessage( $"board must be 5-19 (got {board})" );
    }

    [TestCase( "depth", 0, "depth must be 1-8 (got 0)" )]
    [TestCase( "depth", 9, "depth must be 1-8 (got 9)" )]
    [TestCase( "topK", 0, "topK must be 1-50 (got 0)" )]
    [TestCase( "topK", 51, "topK must be 1-50 (got 51)" )]
    [TestCase( "timeout", 49, "timeout must be 0 (off) or 50-60000 ms (got 49)" )]
    [TestCase( "timeout", 60001, "timeout must be 0 (off) or 50-60000 ms (got 60001)" )]
    [TestCase( "minDelay", -1, "minDelay must be 0-10000 ms (got -1)" )]
    [TestCase( "minDelay", 10001, "minDelay must be 0-10000 ms (got 10001)" )]
    public void OptionOutOfRange_Throws( string option, int value, string message )
    {
        var white = new PlayerSpec
        {
            Type = PlayerType.Graph,
            Depth = option == "depth" ? value : null,
            TopK = option == "topK" ? value : null,
            Timeout = option == "timeout" ? value : null,
            MinDelay = option == "minDelay" ? value : null,
        };

        var act = () => ConfigResolver.Resolve( null, new ConfigLayer { White = white } );

        act.Should().Throw<ConfigException>().WithMessage( $"white player: {message}" );
    }

    [TestCase( 0 )]
    [TestCase( 50 )]
    [TestCase( 60000 )]
    public void ValidTimeout_Passes( int timeout )
    {
        var cli = new ConfigLayer { White = new PlayerSpec { Type = PlayerType.Graph, Timeout = timeout } };

        var config = ConfigResolver.Resolve( null, cli );

        config.White.Timeout.Should().Be( timeout );
    }
}
