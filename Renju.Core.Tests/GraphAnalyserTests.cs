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
    public void TryProceedNextMove_BlocksClosedFour_OrLoses()
    {
        // Black has four at row 7, cols 5..8, blocked at (4,7) by White.
        // The only open end is (9,7) — White must block it or Black wins next move.
        var board = new Board.Board( 15 );
        var referee = new Referee( board );
        var graph = new BoardAnalyserGraph( Stone.White, board, referee );

        board.PutStone( 5, 7, Stone.Black );
        board.PutStone( 4, 7, Stone.White );
        board.PutStone( 6, 7, Stone.Black );
        board.PutStone( 0, 0, Stone.White );
        board.PutStone( 7, 7, Stone.Black );
        board.PutStone( 0, 1, Stone.White );
        board.PutStone( 8, 7, Stone.Black );

        var success = graph.TryProceedNextMove( out var move );

        Assert.That( success, Is.True );
        Assert.That( (move.Col, move.Row), Is.EqualTo( (9, 7) ),
            $"White must block the four at (9,7); got ({move.Col},{move.Row})." );
    }

    [Test]
    public void TryProceedNextMove_BlocksOpenThree_WhenItHasNoStrongerOwnThreat()
    {
        // Black has an open diagonal three (7,7),(8,8),(9,9); White has nothing
        // comparable. Not blocking lets Black build an open four (two winning
        // ends — unstoppable). White must play one of the adjacent ends.
        var board = new Board.Board( 15 );
        var referee = new Referee( board );
        var graph = new BoardAnalyserGraph( Stone.White, board, referee );

        board.PutStone( 7, 7, Stone.Black );
        board.PutStone( 0, 0, Stone.White );
        board.PutStone( 8, 8, Stone.Black );
        board.PutStone( 0, 1, Stone.White );
        board.PutStone( 9, 9, Stone.Black );

        var success = graph.TryProceedNextMove( out var move );

        Assert.That( success, Is.True );
        Assert.That(
            (move.Col == 6 && move.Row == 6) || (move.Col == 10 && move.Row == 10),
            $"White must block the open three at (6,6) or (10,10); got ({move.Col},{move.Row})." );
    }

    [Test]
    public void TryProceedNextMove_BlocksOpenThree_EvenWithOwnCounterplay()
    {
        // Horizon-race case (from a real Graph-vs-Graph 9-move loss):
        // Black has an open diagonal three (7,7),(8,8),(9,9); White's own pair
        // (7,8),(8,7) offers a tempting attack line. Without win-in-2 detection
        // the tree ends on White's own move and "build my own threats" nets more
        // than blocking, while Black's five lands one tempo earlier.
        var board = new Board.Board( 19 );
        var referee = new Referee( board );
        var graph = new BoardAnalyserGraph( Stone.White, board, referee );

        board.PutStone( 8, 8, Stone.Black );
        board.PutStone( 7, 8, Stone.White );
        board.PutStone( 7, 7, Stone.Black );
        board.PutStone( 8, 7, Stone.White );
        board.PutStone( 9, 9, Stone.Black );

        var success = graph.TryProceedNextMove( out var move );

        Assert.That( success, Is.True );
        Assert.That(
            (move.Col == 6 && move.Row == 6) || (move.Col == 10 && move.Row == 10),
            $"White must block the open three at (6,6) or (10,10); got ({move.Col},{move.Row})." );
    }

    [Test]
    public void TryProceedNextMove_KeepsPlaying_WhenOpponentAchievesWinInTwo()
    {
        // White builds a real open four (5..8, row 7). The tree marks that move
        // as a win-in-2 terminal; when the actual game reaches it, the root must
        // be reopened — the AI still has to move (block an end / hunt a counter),
        // not get stuck with an unexpandable terminal root.
        var board = new Board.Board( 15 );
        var referee = new Referee( board );
        var graph = new BoardAnalyserGraph( Stone.Black, board, referee );

        board.PutStone( 0, 0, Stone.Black );
        board.PutStone( 5, 7, Stone.White );
        board.PutStone( 0, 1, Stone.Black );
        board.PutStone( 6, 7, Stone.White );
        board.PutStone( 0, 2, Stone.Black );
        board.PutStone( 7, 7, Stone.White );
        board.PutStone( 14, 14, Stone.Black );
        board.PutStone( 8, 7, Stone.White );

        var success = graph.TryProceedNextMove( out var move );

        Assert.That( success, Is.True, "AI must still produce a move in a lost-looking position." );
        Assert.That( referee.MoveAllowed( move.Col, move.Row, Stone.Black ), Is.True );
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
