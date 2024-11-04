using Renju.Core.Extensions;

namespace Renju.Core.BoardAnalyser;

internal class RowParser
{
    // todo: maybe we can not validate for six here
    // edge: -1, none: 0,  stone: 1 / 2
    internal static bool IsAreaValid( IList<int> row, int start, int length, int leftEdge, int rightEdge, int self, int opponent, bool sixAllowed )
    {
        // must be 5 cells length
        if( length != 5 || start + length > row.Count ) return false;

        // opponent stone
        var end = start + length;
        for ( var i = start; i < end; i++ ) if ( row[i] == opponent ) return false;

        // six
        if ( !sixAllowed && ( leftEdge == self || rightEdge == self ) ) return false;

        return true;
    }

    // area must be valid
    internal static FigureType ParseFigure( IList<int> row, int start, int length, int leftEdge, int rightEdge, int cell, int self )
    {
        var end = start + length;
        var stones = 0;
        var holes = 0;
        var leftSpace = leftEdge == 0 ? 1 : 0;
        var rightSpace = rightEdge == 0 ? 1 : 0;
        for ( int i = start, h = 0, r = 0; i < end; i++ )
        {
            if ( i == cell || row[i] == self ) stones++;

            // calculate holes inside the figure
            if ( row[i] == 0 && i != cell )
            {
                if ( stones > 0 ) h++;
                r++; // right space not verified
            }
            else if ( row[i] == self || i == cell )
            {
                if ( h > 0 )
                {
                    holes += h;
                    h = 0;
                }
                r = 0;
            }

            // calculate free space around the potential figure
            if ( stones == 0 && i != cell && row[i] == 0 ) leftSpace++;
            if ( i == end - 1 && r > 0 ) rightSpace += r;
        }

        return stones switch
        {
            1 => FigureType.None,
            2 when holes == 0 && leftSpace > 0 && rightSpace > 0 => FigureType.OpenTwo,
            2 when holes == 0 => FigureType.ClosedTwo,
            2 when holes == 1 => FigureType.ClosedTwo1,
            2 when holes == 2 => FigureType.ClosedTwo2,
            3 when holes == 0 && leftSpace > 0 && rightSpace > 0 => FigureType.OpenThree,
            3 when holes == 0 => FigureType.ClosedThree,
            3 when holes == 1 => FigureType.ClosedThree1,
            3 when holes == 2 => FigureType.ClosedThree2,
            4 => FigureType.ClosedFour,
            5 => FigureType.Five,
            > 5 => FigureType.SixOrMore,
            _ => FigureType.None
        };
    }

    internal static IList<(int cell, FigureType type)> ParseRow(IList<int> row, int rowSize, Stone targetStone, bool sixAllowed)
    {
        var target = (int)targetStone;
        var opponent = (int)targetStone.Opposite();
        var areas = new List<(int start, int length, int leftEdge, int rightEdge)>();

        // define all possible areas for figures
        for ( int i = 0, l = -1, r; i < rowSize; i++)
        {
            r = ( i + 5 ) < rowSize ? row[i + 5] : -1;
            if ( IsAreaValid( row, i, 5, l, r, target, opponent, sixAllowed ) )
                areas.Add( (start: i, length: 5, l, r) );
            l = row[i];
        }

        // get all valid figures
        var figures = new List<(int cell, FigureType type)>();
        foreach ( var (start, length, leftEdge, rightEdge) in areas )
        {
            for ( var i = start; i < start + length; i++ )
            {
                if ( row[i] != 0 ) continue;
                var figure = ParseFigure( row, start, length, leftEdge, rightEdge, i, target );
                if ( figure != FigureType.None ) figures.Add( (i, figure) );
            }
        }

        // select best figure for each cell
        return figures
            .GroupBy( f => f.cell )
            .Select( g => g.MaxBy( f => (int)f.type ) )
            .ToList();
    }
}