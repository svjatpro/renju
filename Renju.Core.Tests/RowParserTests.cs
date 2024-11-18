using NUnit.Framework;
using Renju.Core.BoardAnalyser;

namespace Renju.Core.Tests;

[TestFixture]
public class RowParserTests
{
    #region Test Data

    private static LineOfCells Line00010012011000 = new( [0, 0, 0, 1, 0, 0, 1, 2, 0, 1, 1, 0, 0, 0], 14 );
    private static LineOfCells Line00022200 = new( [0, 0, 0, 2, 2, 2, 0, 0] );

    private static readonly IEnumerable<TestCaseData> DefineRowFiguresData =
    [
        new TestCaseData( Line00010012011000, Stone.Black )
            .Returns( new Dictionary<int, FigureType>
            {
                { 0, FigureType.ClosedTwo2 },
                { 1, FigureType.ClosedTwo1 },
                { 2, FigureType.ClosedThree2 },
                { 4, FigureType.ClosedThree1 },
                { 5, FigureType.ClosedThree1 },
                { 8, FigureType.ClosedThree },
                { 11, FigureType.OpenThree },
                { 12, FigureType.ClosedThree1 },
                { 13, FigureType.ClosedThree2 },
            } )
            .SetName( "_ _ _ x _ _ x o _ x x _ _ _" ),
        new TestCaseData( Line00022200, Stone.White )
            .Returns( new Dictionary<int, FigureType>
            {
                { 0, FigureType.ClosedThree2 },
                { 1, FigureType.ClosedFour },
                { 2, FigureType.OpenFour},
                { 6, FigureType.OpenFour },
                { 7, FigureType.ClosedFour },
            } )
            .SetName( "_ _ _ o o o _ _" ),
    ];

    private static readonly IEnumerable<TestCaseData> DefineCellFigureData =
    [
        new TestCaseData( Line00010012011000, Stone.Black, 0, 0 ).Returns( FigureType.ClosedTwo2 ).SetName( "[x, 0, 0, 1, 0] 0 - ClosedTwo2" ),
        new TestCaseData( Line00010012011000, Stone.Black, 0, 1 ).Returns( FigureType.ClosedTwo1 ).SetName( "[0, x, 0, 1, 0] 0 - ClosedTwo1" ),
        new TestCaseData( Line00010012011000, Stone.Black, 0, 2 ).Returns( FigureType.OpenTwo ).SetName( "[0, 0, x, 1, 0] 0 - OpenTwo" ),
        new TestCaseData( Line00010012011000, Stone.Black, 0, 4 ).Returns( FigureType.OpenTwo ).SetName( "[0, 0, 0, 1, x] 0 - OpenTwo" ),
        new TestCaseData( Line00010012011000, Stone.Black, 2, 2 ).Returns( FigureType.ClosedThree2 ).SetName( "0 [x, 1, 0, 0, 1] 2 - ClosedThree2" ),
        new TestCaseData( Line00010012011000, Stone.Black, 2, 4 ).Returns( FigureType.ClosedThree1 ).SetName( "0 [0, 1, x, 0, 1] 2 - ClosedThree1" ),
        new TestCaseData( Line00010012011000, Stone.Black, 2, 5 ).Returns( FigureType.ClosedThree1 ).SetName( "0 [0, 1, 0, x, 1] 2 - ClosedThree1" ),
        new TestCaseData( Line00010012011000, Stone.Black, 8, 8 ).Returns( FigureType.ClosedThree ).SetName( "2 [x, 1, 1, 0, 0] 0 - ClosedThree" ),
        new TestCaseData( Line00010012011000, Stone.Black, 8, 11 ).Returns( FigureType.OpenThree ).SetName( "2 [0, 1, 1, x, 0] 0 - OpenThree" ),
        new TestCaseData( Line00010012011000, Stone.Black, 8, 12 ).Returns( FigureType.ClosedThree1 ).SetName( "2 [0, 1, 1, 0, x] 0 - ClosedThree1" ),
        new TestCaseData( Line00010012011000, Stone.Black, 9, 11 ).Returns( FigureType.OpenThree ).SetName( "0 [1, 1, x, 0, 0] - OpenThree" ),
        new TestCaseData( Line00010012011000, Stone.Black, 9, 12 ).Returns( FigureType.ClosedThree1 ).SetName( "0 [1, 1, 0, x, 0] - ClosedThree1" ),
        new TestCaseData( Line00010012011000, Stone.Black, 9, 13 ).Returns( FigureType.ClosedThree2 ).SetName( "0 [1, 1, 0, 0, x] - ClosedThree2" ),
        
        new TestCaseData( Line00022200, Stone.White, 0, 0 ).Returns( FigureType.ClosedThree2 ).SetName( "[x,0,0,2,2]2 - ClosedThree2" ),
        new TestCaseData( Line00022200, Stone.White, 0, 1 ).Returns( FigureType.ClosedThree1 ).SetName( "[0,x,0,2,2]2 - ClosedThree1" ),
        new TestCaseData( Line00022200, Stone.White, 0, 2 ).Returns( FigureType.ClosedThree ).SetName( "[0,0,x,2,2]2 - ClosedThree" ),
        new TestCaseData( Line00022200, Stone.White, 1, 1 ).Returns( FigureType.ClosedFour ).SetName( "0[x,0,2,2,2]0 - ClosedFour" ),
        new TestCaseData( Line00022200, Stone.White, 1, 2 ).Returns( FigureType.OpenFour ).SetName( "0[0,x,2,2,2]0 - OpenFour" ),
    ];

    #endregion

    [TestCaseSource( nameof( DefineCellFigureData ) )]
    public FigureType DefineBestFigureTest( LineOfCells row, Stone target, int start, int cell )
    {
        var sixAllowed = target == Stone.White;
        var area = new CellsArea( row, target, start );
        return RowParser.DefineFigure( area, cell );
    }

    [TestCaseSource( nameof(DefineRowFiguresData) )]
    public IDictionary<int, FigureType> ParseRowTests( LineOfCells row, Stone target )
    {
        var sixAllowed = target == Stone.White;
        return RowParser.DefineBestFigures( row, target, sixAllowed );
    }
}