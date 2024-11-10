using NUnit.Framework;
using Renju.Core.BoardAnalyser;
using Renju.Core.Extensions;

namespace Renju.Core.Tests;

// --------------------------------------------------------------
//  six not allowed:
//int[] row = [0, 0, 0, 1, 0, 0, 1, 2, 0, 1, 1, 0, 0, 0];
// --------------------------------------------------------------
// get all possible 'areas' for figures:
//
//            [0, 0, 0, 1, 0];                              // left: edge, right: none
//                  [0, 1, 0, 0, 1];                        // left: none, right: opponent
//                                    [0, 1, 1, 0, 0];      // left: opponent, right: none
//                                       [1, 1, 0, 0, 0];   // left: none, right: edge
// --------------------------------------------------------------
// get all valid figures:
//
//            [x, 0, 0, 1, 0];                              - close two
//            [0, x, 0, 1, 0];                              - close two
//            [0, 0, x, 1, 0];                              - close two
//            [0, 0, 0, 1, x];                              - close two
//                  [x, 1, 0, 0, 1];                        - close three
//                  [0, 1, x, 0, 1];                        - close three
//                  [0, 1, 0, x, 1];                        - close three
//                                    [x, 1, 1, 0, 0];      - close three
//                                    [0, 1, 1, x, 0];      - open three
//                                    [0, 1, 1, 0, x];      - close three
//                                       [1, 1, x, 0, 0];   - open three
//                                       [1, 1, 0, x, 0];   - close three
//                                       [1, 1, 0, 0, x];   - close three


// --------------------------------------------------------------
//  six allowed:
//int[] row = [0, 0, 0, 1, 0, 0, 1, 2, 0, 1, 1, 0, 0, 0];
// --------------------------------------------------------------
// get all possible 'areas' for figures:
//
//            [0, 0, 0, 1, 0];
//                  [0, 1, 0, 0, 1];
//                                    [0, 1, 1, 0, 0];
//                                       [1, 1, 0, 0, 0];
// --------------------------------------------------------------
// get all valid figures:
//
//            [x, 0, 0, 1, 0];                              - close two
//            [0, x, 0, 1, 0];                              - close two
//            [0, 0, x, 1, 0];                              - close two
//            [0, 0, 0, 1, x];                              - close two
//               [x, 0, 1, 0, 0];                           - close two
//               [0, x, 1, 0, 0];                           - close two
//               [0, 0, 1, x, 0];                           - close two
//               [0, 0, 1, 0, x];                           - close two
//                  [x, 1, 0, 0, 1];                        - close three
//                  [0, 1, x, 0, 1];                        - close three
//                  [0, 1, 0, x, 1];                        - close three
//                                    [x, 1, 1, 0, 0];      - close three
//                                    [0, 1, 1, x, 0];      - open three
//                                    [0, 1, 1, 0, x];      - close three
//                                       [1, 1, x, 0, 0];   - open three
//                                       [1, 1, 0, x, 0];   - close three
//                                       [1, 1, 0, 0, x];   - close three

[TestFixture]
public class RowParserTests
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
        int[] row = [0, 0, 0, 1, 0, 0, 1, 2, 0, 1, 1, 0, 0, 0];
        var end = start + length;
        var leftEdge = start == 0 ? -1 : row[start - 1];
        var rightEdge = end == row.Length ? -1 : row[end];

        return RowParser.IsAreaValid( 
            row, rowSize: row.Length,
            start, length, 
            leftEdge, rightEdge, 
            (int)self, (int)self.Opposite(), sixAllowed );
    }

    [TestCase( 0, 0, ExpectedResult = FigureType.ClosedTwo2, TestName = "[x, 0, 0, 1, 0] 0 - ClosedTwo2" )]
    [TestCase( 0, 1, ExpectedResult = FigureType.ClosedTwo1, TestName = "[0, x, 0, 1, 0] 0 - ClosedTwo1" )]
    [TestCase( 0, 2, ExpectedResult = FigureType.OpenTwo, TestName = "[0, 0, x, 1, 0] 0 - OpenTwo" )]
    [TestCase( 0, 4, ExpectedResult = FigureType.OpenTwo, TestName = "[0, 0, 0, 1, x] 0 - OpenTwo" )]
    [TestCase( 2, 2, ExpectedResult = FigureType.ClosedThree2, TestName = "0 [x, 1, 0, 0, 1] 2 - ClosedThree2" )]
    [TestCase( 2, 4, ExpectedResult = FigureType.ClosedThree1, TestName = "0 [0, 1, x, 0, 1] 2 - ClosedThree1" )]
    [TestCase( 2, 5, ExpectedResult = FigureType.ClosedThree1, TestName = "0 [0, 1, 0, x, 1] 2 - ClosedThree1" )]
    [TestCase( 8, 8, ExpectedResult = FigureType.ClosedThree, TestName = "2 [x, 1, 1, 0, 0] 0 - ClosedThree" )]
    [TestCase( 8, 11, ExpectedResult = FigureType.OpenThree, TestName = "2 [0, 1, 1, x, 0] 0 - OpenThree" )]
    [TestCase( 8, 12, ExpectedResult = FigureType.ClosedThree1, TestName = "2 [0, 1, 1, 0, x] 0 - ClosedThree1" )]
    [TestCase( 9, 11, ExpectedResult = FigureType.OpenThree, TestName = "0 [1, 1, x, 0, 0] - OpenThree" )]
    [TestCase( 9, 12, ExpectedResult = FigureType.ClosedThree1, TestName = "0 [1, 1, 0, x, 0] - ClosedThree1" )]
    [TestCase( 9, 13, ExpectedResult = FigureType.ClosedThree2, TestName = "0 [1, 1, 0, 0, x] - ClosedThree2" )]
    public FigureType ParseFigureTest( int start, int cell )
    {
        int[] row = [0, 0, 0, 1, 0, 0, 1, 2, 0, 1, 1, 0, 0, 0];
        var end = start + 5;
        var leftEdge = start == 0 ? -1 : row[start - 1];
        var rightEdge = end == row.Length ? -1 : row[end];

        return RowParser.ParseFigure( row, start, 5, leftEdge, rightEdge, cell, (int)Stone.White );
    }
    
    [TestCase( 0, Stone.White, ExpectedResult = FigureType.ClosedTwo2 )]
    [TestCase( 1, Stone.White, ExpectedResult = FigureType.ClosedTwo1 )]
    [TestCase( 2, Stone.White, ExpectedResult = FigureType.ClosedThree2 )]
    [TestCase( 3, Stone.White, ExpectedResult = FigureType.None )]
    [TestCase( 4, Stone.White, ExpectedResult = FigureType.ClosedThree1 )]
    [TestCase( 5, Stone.White, ExpectedResult = FigureType.ClosedThree1 )]
    [TestCase( 6, Stone.White, ExpectedResult = FigureType.None )]
    [TestCase( 7, Stone.White, ExpectedResult = FigureType.None )]
    [TestCase( 8, Stone.White, ExpectedResult = FigureType.ClosedThree )]
    [TestCase( 9, Stone.White, ExpectedResult = FigureType.None )]
    [TestCase( 10, Stone.White, ExpectedResult = FigureType.None )]
    [TestCase( 11, Stone.White, ExpectedResult = FigureType.OpenThree )]
    [TestCase( 12, Stone.White, ExpectedResult = FigureType.ClosedThree1 )]
    [TestCase( 13, Stone.White, ExpectedResult = FigureType.ClosedThree2 )]
    public FigureType ParseRowTest( int cell, Stone self )
    {
        int[] row = [0, 0, 0, 1, 0, 0, 1, 2, 0, 1, 1, 0, 0, 0];
        var figures = RowParser
            .ParseRow( row, row.Length, self, false )
            .ToDictionary( f => f.Key, f => f.Value );
        return figures.GetValueOrDefault( cell, FigureType.None );
    }
}