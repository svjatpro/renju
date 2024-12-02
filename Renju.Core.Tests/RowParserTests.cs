using NUnit.Framework;
using Renju.Core.BoardAnalyser;

namespace Renju.Core.Tests;

[TestFixture]
public class RowParserTests
{
    #region Test Data

    private static LineOfCells Line00010012011000 = new( [0, 0, 0, 1, 0, 0, 1, 2, 0, 1, 1, 0, 0, 0], 14 );
    private static LineOfCells Line00022200 = new( [0, 0, 0, 2, 2, 2, 0, 0] );
    private static LineOfCells Line00000122200 = new( [0, 0, 0, 0, 0, 1, 2, 2, 2, 0, 0] );
    private static LineOfCells Line00000111011000 = new( [0,0,0,0,0,1,1,1,0,1,1,0,0,0] );

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
        
        new TestCaseData( Line00000122200, Stone.White, 0, 4 ).Returns( FigureType.None ).SetName( "[00000]122200 - None" ),
        //new TestCaseData( Line00000122200, Stone.White, 1, 4 ).Returns( FigureType.None ).SetName( "0[00001]22200 - None" ),
        //new TestCaseData( Line00000122200, Stone.White, 2, 4 ).Returns( FigureType.None ).SetName( "00[00012]2200 - None" ),
        //new TestCaseData( Line00000122200, Stone.White, 3, 4 ).Returns( FigureType.None ).SetName( "000[00122]200 - None" ),
        //new TestCaseData( Line00000122200, Stone.White, 4, 4 ).Returns( FigureType.None ).SetName( "0000[01222]00 - None" ),
        //new TestCaseData( Line00000122200, Stone.White, 5, 9 ).Returns( FigureType.None ).SetName( "00000[12220]0 - None" ),
        new TestCaseData( Line00000122200, Stone.White, 6, 9 ).Returns( FigureType.ClosedFour ).SetName( "000001[22200] - ClosedFour" ),
        
        new TestCaseData( Line00000111011000, Stone.Black, 5, 8 ).Returns( FigureType.SixOrMore ).SetName( "00000[11101]1000 - Six" ),
        new TestCaseData( Line00000111011000, Stone.Black, 6, 8 ).Returns( FigureType.SixOrMore ).SetName( "000001[11011]000 - Six" ),
    ];

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
            .SetName( "00010012011000" ),
        new TestCaseData( Line00022200, Stone.White )
            .Returns( new Dictionary<int, FigureType>
            {
                { 0, FigureType.ClosedThree2 },
                { 1, FigureType.ClosedFour },
                { 2, FigureType.OpenFour},
                { 6, FigureType.OpenFour },
                { 7, FigureType.ClosedFour },
            } )
            .SetName( "00022200" ),
        new TestCaseData( Line00000122200, Stone.White )
            .Returns( new Dictionary<int, FigureType>
            {
                { 9, FigureType.ClosedFour },
                { 10, FigureType.ClosedFour },
            } )
            .SetName( "00000122200" ),

    ];

    #endregion

    [TestCaseSource( nameof( DefineCellFigureData ) )]
    public FigureType DefineBestFigureTest( LineOfCells row, Stone target, int start, int cell )
    {
        var area = new CellsArea( row, target, start );
        return RowParser.DefineFigure( area, cell );
    }

    [TestCaseSource( nameof(DefineRowFiguresData) )]
    public IDictionary<int, FigureType> ParseRowTests( LineOfCells row, Stone target )
    {
        return RowParser.DefineBestFigures( row, target );
    }
}