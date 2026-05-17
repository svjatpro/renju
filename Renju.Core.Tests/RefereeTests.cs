using NUnit.Framework;
using Renju.Core.RenjuGame;

namespace Renju.Core.Tests;

[TestFixture]
public class RefereeTests
{
    [Test]
    public void Clone_PreservesForbiddenMoveDetection_When3x3IsSetUp()
    {
        // Position where Black playing (6,6) would create two simultaneous
        // OpenThrees (horizontal via 4,6 + 5,6; vertical via 6,4 + 6,5) = 3x3 fork.
        var board = new Board.Board( 15 );
        var referee = new Referee( board );

        board.PutStone( 4, 6, Stone.Black );
        board.PutStone( 5, 6, Stone.Black );
        board.PutStone( 6, 4, Stone.Black );
        board.PutStone( 6, 5, Stone.Black );

        Assert.That(
            referee.MoveAllowed( 6, 6, Stone.Black, ignoreSequence: true ),
            Is.False,
            "Original referee must block (6,6) as a 3x3 fork." );

        var clone = referee.Clone();

        Assert.That(
            clone.MoveAllowed( 6, 6, Stone.Black, ignoreSequence: true ),
            Is.False,
            "Cloned referee must inherit the figures state and also block (6,6)." );
    }

    [Test]
    public void Clone_PreservesWinDetection_WhenFourInARowIsOnTheBoard()
    {
        // Black has four in a row (5..8, row 7); (4,7) or (9,7) would be Five.
        var board = new Board.Board( 15 );
        var referee = new Referee( board );

        board.PutStone( 5, 7, Stone.Black );
        board.PutStone( 6, 7, Stone.Black );
        board.PutStone( 7, 7, Stone.Black );
        board.PutStone( 8, 7, Stone.Black );

        // Sanity: on the original, both ends are detected as winning moves for Black.
        Assert.That( referee.MoveAllowed( 4, 7, Stone.Black, ignoreSequence: true ), Is.True );
        Assert.That( referee.MoveAllowed( 9, 7, Stone.Black, ignoreSequence: true ), Is.True );

        var clone = referee.Clone();

        // Both ends are still legal on the clone (no 6+ rule trigger; figures map carried over).
        Assert.That( clone.MoveAllowed( 4, 7, Stone.Black, ignoreSequence: true ), Is.True );
        Assert.That( clone.MoveAllowed( 9, 7, Stone.Black, ignoreSequence: true ), Is.True );
    }
}
