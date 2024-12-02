using NUnit.Framework;
using Renju.Core.BoardAnalyser;

namespace Renju.Core.Tests;

[TestFixture]
public class CellsAreaTests
{
    private static LineOfCells Line00010012011000 = new( [0, 0, 0, 1, 0, 0, 1, 2, 0, 1, 1, 0, 0, 0], 14 );
    private static LineOfCells Line00022200 = new ( [0, 0, 0, 2, 2, 2, 0, 0] );
    private static LineOfCells Line00000122200 = new( [0, 0, 0, 0, 0, 1, 2, 2, 2, 0, 0] );

    private static readonly IEnumerable<TestCaseData> ValidateAreaData =
    [
        new TestCaseData( Line00010012011000, Stone.Black, 0, 4 ).Returns( false ).SetName( "0 4 Inappropriate length" ),
        new TestCaseData( Line00010012011000, Stone.Black, 0, 6 ).Returns( false ).SetName( "0 6 Inappropriate length" ),
        new TestCaseData( Line00010012011000, Stone.Black, 8, 4 ).Returns( false ).SetName( "8 4 Inappropriate length" ),
        new TestCaseData( Line00010012011000, Stone.Black, 8, 6 ).Returns( false ).SetName( "8 6 Inappropriate length" ),
        
        new TestCaseData( Line00010012011000, Stone.Black, 3, 5 ).Returns( false ).SetName( "3 5 Opponent stone" ),
        new TestCaseData( Line00010012011000, Stone.Black, 4, 5 ).Returns( false ).SetName( "4 5 Opponent stone" ),
        new TestCaseData( Line00010012011000, Stone.Black, 5, 5 ).Returns( false ).SetName( "5 5 Opponent stone" ),
        new TestCaseData( Line00010012011000, Stone.Black, 6, 5 ).Returns( false ).SetName( "6 5 Opponent stone" ),
        new TestCaseData( Line00010012011000, Stone.Black, 7, 5 ).Returns( false ).SetName( "7 5 Opponent stone" ),
        
        new TestCaseData( Line00010012011000, Stone.Black, 0, 5 ).Returns( true ).SetName( "[0, 0, 0, 1, 0] 0" ),
        new TestCaseData( Line00010012011000, Stone.Black, 2, 5 ).Returns( true ).SetName( "0 [0, 1, 0, 0, 1] 2" ),
        new TestCaseData( Line00010012011000, Stone.Black, 8, 5 ).Returns( true ).SetName( "2 [0, 1, 1, 0, 0] 0" ),
        new TestCaseData( Line00010012011000, Stone.Black, 9, 5 ).Returns( true ).SetName( "0 [0, 1, 1, 0, 0]" ),
        new TestCaseData( Line00010012011000, Stone.Black, 1, 5).Returns( true ).SetName( "0 [0, 1, 0, 0, 1] 1 - six allowed" ),
                                                               
        new TestCaseData( Line00010012011000, Stone.White, 1, 5 ).Returns( false ).SetName( "1 5 - opponent" ),
        new TestCaseData( Line00010012011000, Stone.White, 3, 5 ).Returns( false ).SetName( "3 5 - opponent" ),
        new TestCaseData( Line00010012011000, Stone.White, 6, 5 ).Returns( false ).SetName( "6 5 - opponent" ),
        new TestCaseData( Line00010012011000, Stone.White, 9, 5 ).Returns( false ).SetName( "9 5 - opponent" ),
        
        new TestCaseData( Line00022200, Stone.White, 1, 5 ).Returns( true ).SetName( "0[0022]2 - white" ),

        new TestCaseData( Line00000122200, Stone.White, 0, 5 ).Returns( true ).SetName( "[00000]122200 - CloseTwo" ),
        new TestCaseData( Line00000122200, Stone.White, 1, 5 ).Returns( false ).SetName( "0[00001]22200 - None" ),
        new TestCaseData( Line00000122200, Stone.White, 2, 5 ).Returns( false ).SetName( "00[00012]2200 - None" ),
        new TestCaseData( Line00000122200, Stone.White, 3, 5 ).Returns( false ).SetName( "000[00122]200 - None" ),
        new TestCaseData( Line00000122200, Stone.White, 4, 5 ).Returns( false ).SetName( "0000[01222]00 - None" ),
        new TestCaseData( Line00000122200, Stone.White, 5, 5 ).Returns( false ).SetName( "00000[12220]0 - None" ),
        new TestCaseData( Line00000122200, Stone.White, 6, 5 ).Returns( true ).SetName( "000001[22200] - ClosedFour" ),
    ];

    [TestCaseSource( nameof( ValidateAreaData ) )]
    public bool ValidateAreaTest( LineOfCells row, Stone target, int start, int length )
    {
        return CellsArea.IsValid( row, out _, target, start, length );
    }
}