using NUnit.Framework;
using Renju.Core.BoardAnalyser;

namespace Renju.Core.Tests;

[TestFixture]
public class CellsAreaTests
{
    [TestCase( 0, 4, Stone.White, false, ExpectedResult = false, TestName = "0 4 Inappropriate length")]
    [TestCase( 0, 6, Stone.White, false, ExpectedResult = false, TestName = "0 6 Inappropriate length" )]
    [TestCase( 8, 4, Stone.White, false, ExpectedResult = false, TestName = "8 4 Inappropriate length" )]
    [TestCase( 8, 6, Stone.White, false, ExpectedResult = false, TestName = "8 6 Inappropriate length" )]

    [TestCase( 3, 5, Stone.White, false, ExpectedResult = false, TestName = "3 5 Opponent stone" )]
    [TestCase( 4, 5, Stone.White, false, ExpectedResult = false, TestName = "4 5 Opponent stone" )]
    [TestCase( 5, 5, Stone.White, false, ExpectedResult = false, TestName = "5 5 Opponent stone" )]
    [TestCase( 6, 5, Stone.White, false, ExpectedResult = false, TestName = "6 5 Opponent stone" )]
    [TestCase( 7, 5, Stone.White, false, ExpectedResult = false, TestName = "7 5 Opponent stone" )]

    [TestCase( 1, 5, Stone.White, false, ExpectedResult = false, TestName = "0 [0, 1, 0, 0, 1] 1 - six not allowed" )]

    [TestCase( 0, 5, Stone.White, false, ExpectedResult = true, TestName = "[0, 0, 0, 1, 0] 0" )]
    [TestCase( 2, 5, Stone.White, false, ExpectedResult = true, TestName = "0 [0, 1, 0, 0, 1] 2" )]
    
    [TestCase( 8, 5, Stone.White, false, ExpectedResult = true, TestName = "2 [0, 1, 1, 0, 0] 0" )]
    [TestCase( 9, 5, Stone.White, false, ExpectedResult = true, TestName = "0 [0, 1, 1, 0, 0]" )]

    [TestCase( 1, 5, Stone.White, true, ExpectedResult = true, TestName = "0 [0, 1, 0, 0, 1] 1 - six allowed" )]
    
    [TestCase( 1, 5, Stone.Black, false, ExpectedResult = false, TestName = "1 5 - opponent" )]
    [TestCase( 3, 5, Stone.Black, false, ExpectedResult = false, TestName = "3 5 - opponent" )]
    [TestCase( 6, 5, Stone.Black, false, ExpectedResult = false, TestName = "6 5 - opponent" )]
    [TestCase( 9, 5, Stone.Black, false, ExpectedResult = false, TestName = "9 5 - opponent" )]
    public bool IsAreaValidTests( int start, int length, Stone self, bool sixAllowed )
    {
        var row = new LineOfCells( [0, 0, 0, 1, 0, 0, 1, 2, 0, 1, 1, 0, 0, 0] );
        return CellsArea.IsValid( row, out _, self, start, length, sixAllowed );
    }
}