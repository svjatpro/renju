using NUnit.Framework;
using Renju.Core.BoardAnalyser;

namespace Renju.Core.Tests;

[TestFixture]
public class RowParserTests
{
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
        var row = new LineOfCells( [0, 0, 0, 1, 0, 0, 1, 2, 0, 1, 1, 0, 0, 0] );
        var area = new CellsArea( row, Stone.White, start );
        return RowParser.ParseFigure( area, cell );
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
        var row = new LineOfCells( [0, 0, 0, 1, 0, 0, 1, 2, 0, 1, 1, 0, 0, 0], 14 );
        var figures = RowParser
            .ParseRow( row, self, false )
            .ToDictionary( f => f.Key, f => f.Value );
        return figures.GetValueOrDefault( cell, FigureType.None );
    }
}