using NUnit.Framework;
using Renju.Core.BoardAnalyser;
using Renju.Core.RenjuGame;

namespace Renju.Core.Tests;

[TestFixture]
public class GraphAnalyserTests
{
    [Test]
    public void Construct_OnEmptyBoard_DoesNotThrow()
    {
        var board = new Board.Board( 15 );
        var referee = new Referee( board );

        Assert.DoesNotThrow( () => new BoardAnalyserGraph( Stone.Black, board, referee ) );
    }

    [Test]
    public void TryProceedNextMove_OnEmptyBoard_AsBlack_PicksCenter()
    {
        var board = new Board.Board( 15 );
        var referee = new Referee( board );
        var graph = new BoardAnalyserGraph( Stone.Black, board, referee );

        var success = graph.TryProceedNextMove( out var move );

        Assert.That( success, Is.True );
        // Matches Plain's center calc (size / 2 - 1) for apples-to-apples comparison.
        Assert.That( move.Col, Is.EqualTo( 6 ) );
        Assert.That( move.Row, Is.EqualTo( 6 ) );
        Assert.That( move.Stone, Is.EqualTo( Stone.Black ) );
    }

    [Test]
    public void TryProceedNextMove_TakesImmediateFive_WhenOneIsAvailable()
    {
        // Black has four in a row at row 7, cols 5..8.
        // Both (4,7) and (9,7) score Five = 1202 → AI must pick one of them.
        var board = new Board.Board( 15 );
        var referee = new Referee( board );
        var graph = new BoardAnalyserGraph( Stone.Black, board, referee, new GraphConfig { Depth = 2, TopK = 6 } );

        board.PutStone( 5, 7, Stone.Black );
        board.PutStone( 0, 0, Stone.White );
        board.PutStone( 6, 7, Stone.Black );
        board.PutStone( 0, 1, Stone.White );
        board.PutStone( 7, 7, Stone.Black );
        board.PutStone( 0, 2, Stone.White );
        board.PutStone( 8, 7, Stone.Black );
        board.PutStone( 0, 3, Stone.White );

        var success = graph.TryProceedNextMove( out var move );

        Assert.That( success, Is.True );
        Assert.That( move.Stone, Is.EqualTo( Stone.Black ) );
        Assert.That(
            (move.Col == 4 && move.Row == 7) || (move.Col == 9 && move.Row == 7),
            $"Expected (4,7) or (9,7) to complete the Five; got ({move.Col},{move.Row})." );
    }
}
