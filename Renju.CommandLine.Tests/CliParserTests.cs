using FluentAssertions;
using NUnit.Framework;
using Renju.CommandLine.Configuration;

namespace Renju.CommandLine.Tests;

[TestFixture]
public class CliParserTests
{
    [Test]
    public void NoArgs_YieldsEmptyLayer()
    {
        var result = CliParser.Parse( [] );

        result.Help.Should().BeFalse();
        result.ConfigPath.Should().BeNull();
        result.Config.Should().Be( new ConfigLayer() );
    }

    [TestCase( "--help" )]
    [TestCase( "-h" )]
    public void Help_IsRecognized( string arg )
    {
        CliParser.Parse( [arg] ).Help.Should().BeTrue();
    }

    [Test]
    public void FullSpec_IsParsed()
    {
        var result = CliParser.Parse(
            ["--black", "plain", "--white", "graph:depth=4,topK=8,timeout=2000,minDelay=300", "--board", "15", "--config", "x.json"] );

        result.Config.Board.Should().Be( 15 );
        result.ConfigPath.Should().Be( "x.json" );
        result.Config.Black.Should().Be( new PlayerSpec { Type = PlayerType.Plain } );
        result.Config.White.Should().Be( new PlayerSpec
        {
            Type = PlayerType.Graph, Depth = 4, TopK = 8, Timeout = 2000, MinDelay = 300,
        } );
    }

    [Test]
    public void SpecOptions_AreCaseInsensitive()
    {
        var spec = CliParser.ParsePlayerSpec( "GRAPH:DEPTH=2,topk=5,MinDelay=100", "test" );

        spec.Should().Be( new PlayerSpec { Type = PlayerType.Graph, Depth = 2, TopK = 5, MinDelay = 100 } );
    }

    [Test]
    public void TimingOptions_ApplyToPlain()
    {
        var spec = CliParser.ParsePlayerSpec( "plain:timeout=500,minDelay=200", "test" );

        spec.Should().Be( new PlayerSpec { Type = PlayerType.Plain, Timeout = 500, MinDelay = 200 } );
    }

    [TestCase( new[] { "--flip" }, "unknown argument '--flip'" )]
    [TestCase( new[] { "--black" }, "'--black' requires a value" )]
    [TestCase( new[] { "--black", "human", "--black", "plain" }, "'--black' given twice" )]
    [TestCase( new[] { "--board", "big" }, "board must be an integer (got 'big')" )]
    [TestCase( new[] { "--white", "alien" }, "white player: unknown player type 'alien' (human | plain | graph)" )]
    [TestCase( new[] { "--white", "graph:jump=3" }, "white player: unknown option 'jump' (depth, topK, timeout, minDelay)" )]
    [TestCase( new[] { "--white", "graph:depth=x" }, "white player: option 'depth' must be an integer (got 'x')" )]
    [TestCase( new[] { "--white", "graph:depth=1,depth=2" }, "white player: option 'depth' set twice" )]
    [TestCase( new[] { "--white", "graph:depth" }, "white player: expected opt=value, got 'depth'" )]
    [TestCase( new[] { "--black", "human:minDelay=100" }, "black player: human takes no options" )]
    [TestCase( new[] { "--black", "plain:depth=3" }, "black player: option 'depth' applies to graph only" )]
    public void InvalidInput_Throws( string[] args, string message )
    {
        var act = () => CliParser.Parse( args );

        act.Should().Throw<ConfigException>().WithMessage( message );
    }
}
