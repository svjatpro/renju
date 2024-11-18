using NUnit.Framework;
using Renju.Core.BoardAnalyser;

namespace Renju.Core.Tests;

[TestFixture]
public class CellsAreaTests
{
    private static LineOfCells Line00010012011000 = new( [0, 0, 0, 1, 0, 0, 1, 2, 0, 1, 1, 0, 0, 0], 14 );
    private static LineOfCells Line00022200 = new ( [0, 0, 0, 2, 2, 2, 0, 0] );

    private static readonly IEnumerable<TestCaseData> ValidateAreaData =
    [
        new TestCaseData( Line00010012011000, Stone.Black, 0, 4, false ).Returns( false ).SetName( "0 4 Inappropriate length" ),
        new TestCaseData( Line00010012011000, Stone.Black, 0, 6, false ).Returns( false ).SetName( "0 6 Inappropriate length" ),
        new TestCaseData( Line00010012011000, Stone.Black, 8, 4, false ).Returns( false ).SetName( "8 4 Inappropriate length" ),
        new TestCaseData( Line00010012011000, Stone.Black, 8, 6, false ).Returns( false ).SetName( "8 6 Inappropriate length" ),
                                                               
        new TestCaseData( Line00010012011000, Stone.Black, 3, 5, false ).Returns( false ).SetName( "3 5 Opponent stone" ),
        new TestCaseData( Line00010012011000, Stone.Black, 4, 5, false ).Returns( false ).SetName( "4 5 Opponent stone" ),
        new TestCaseData( Line00010012011000, Stone.Black, 5, 5, false ).Returns( false ).SetName( "5 5 Opponent stone" ),
        new TestCaseData( Line00010012011000, Stone.Black, 6, 5, false ).Returns( false ).SetName( "6 5 Opponent stone" ),
        new TestCaseData( Line00010012011000, Stone.Black, 7, 5, false ).Returns( false ).SetName( "7 5 Opponent stone" ),
                                                               
        new TestCaseData( Line00010012011000, Stone.Black, 1, 5, false ).Returns( false ).SetName( "0 [0, 1, 0, 0, 1] 1 - six not allowed" ),
        new TestCaseData( Line00010012011000, Stone.Black, 0, 5, false ).Returns( true ).SetName( "[0, 0, 0, 1, 0] 0" ),
        new TestCaseData( Line00010012011000, Stone.Black, 2, 5, false ).Returns( true ).SetName( "0 [0, 1, 0, 0, 1] 2" ),
        new TestCaseData( Line00010012011000, Stone.Black, 8, 5, false ).Returns( true ).SetName( "2 [0, 1, 1, 0, 0] 0" ),
        new TestCaseData( Line00010012011000, Stone.Black, 9, 5, false ).Returns( true ).SetName( "0 [0, 1, 1, 0, 0]" ),
        new TestCaseData( Line00010012011000, Stone.Black, 1, 5, true ).Returns( true ).SetName( "0 [0, 1, 0, 0, 1] 1 - six allowed" ),
                                                               
        new TestCaseData( Line00010012011000, Stone.White, 1, 5, false ).Returns( false ).SetName( "1 5 - opponent" ),
        new TestCaseData( Line00010012011000, Stone.White, 3, 5, false ).Returns( false ).SetName( "3 5 - opponent" ),
        new TestCaseData( Line00010012011000, Stone.White, 6, 5, false ).Returns( false ).SetName( "6 5 - opponent" ),
        new TestCaseData( Line00010012011000, Stone.White, 9, 5, false ).Returns( false ).SetName( "9 5 - opponent" ),
        
        new TestCaseData( Line00022200, Stone.White, 1, 5, true ).Returns( true ).SetName( "0[0022]2 - white" ),
    ];

    [TestCaseSource( nameof( ValidateAreaData ) )]
    public bool ValidateAreaTest( LineOfCells row, Stone target, int start, int length, bool sixAllowed )
    {
        return CellsArea.IsValid( row, out _, target, start, length, sixAllowed );
    }
}