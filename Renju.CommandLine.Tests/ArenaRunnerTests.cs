using FluentAssertions;
using NUnit.Framework;
using Renju.CommandLine.Arena;
using Renju.CommandLine.Configuration;

namespace Renju.CommandLine.Tests;

[TestFixture]
public class ArenaRunnerTests
{
    private static GameConfig Config( int games, int? seed = 42, bool alternate = true ) => new()
    {
        Board = 9,
        Black = new PlayerConfig { Type = PlayerType.Plain },
        White = new PlayerConfig { Type = PlayerType.Plain },
        Arena = new ArenaConfig { Games = games, Seed = seed, Alternate = alternate },
    };

    [Test]
    public void EveryGame_HasAnOutcome()
    {
        var result = new ArenaRunner( Config( games: 4 ) ).Run();

        result.GamesPlayed.Should().Be( 4 );
        ( result.P1.Wins + result.P2.Wins + result.Draws + result.Stuck ).Should().Be( 4 );
        result.TotalMoves.Should().BeGreaterThan( 0 );
        result.MasterSeed.Should().Be( 42 );
    }

    [Test]
    public void SameSeed_ReproducesTheSeries()
    {
        var first = new ArenaRunner( Config( games: 4 ) ).Run();
        var second = new ArenaRunner( Config( games: 4 ) ).Run();

        second.P1.WinsAsBlack.Should().Be( first.P1.WinsAsBlack );
        second.P1.WinsAsWhite.Should().Be( first.P1.WinsAsWhite );
        second.P2.WinsAsBlack.Should().Be( first.P2.WinsAsBlack );
        second.P2.WinsAsWhite.Should().Be( first.P2.WinsAsWhite );
        second.Draws.Should().Be( first.Draws );
        second.TotalMoves.Should().Be( first.TotalMoves );
    }

    [Test]
    public void DifferentSeeds_ProduceDifferentSeries()
    {
        var first = new ArenaRunner( Config( games: 4, seed: 1 ) ).Run();
        var second = new ArenaRunner( Config( games: 4, seed: 2 ) ).Run();

        // move counts colliding across 4 games with different seeds is practically impossible
        second.TotalMoves.Should().NotBe( first.TotalMoves );
    }

    [Test]
    public void NoAlternate_KeepsPlayersOnTheirColors()
    {
        var result = new ArenaRunner( Config( games: 4, alternate: false ) ).Run();

        result.P1.WinsAsWhite.Should().Be( 0, "P1 never plays white without alternation" );
        result.P2.WinsAsBlack.Should().Be( 0, "P2 never plays black without alternation" );
    }

    [Test]
    public void NoSeed_PicksOne_AndReportsIt()
    {
        var result = new ArenaRunner( Config( games: 1, seed: null ) ).Run();

        result.GamesPlayed.Should().Be( 1 );
        // the picked seed must reproduce the run
        var replay = new ArenaRunner( Config( games: 1, seed: result.MasterSeed ) ).Run();
        replay.TotalMoves.Should().Be( result.TotalMoves );
    }

    [Test]
    public void Progress_IsReportedPerGame()
    {
        var updates = new List<int>();

        new ArenaRunner( Config( games: 3 ) ).Run( r => updates.Add( r.GamesPlayed ) );

        updates.Should().Equal( 1, 2, 3 );
    }
}
