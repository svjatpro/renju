namespace Renju.Core.BoardAnalyser;

internal class RowParser
{
    /// <summary>
    /// Get best figure for the empty cell in the area
    /// </summary>
    internal static FigureType DefineFigure( CellsArea area, int cellIndex )
    {
        var end = area.Start + area.Length;
        var selfStoneColor = (int)area.Stone;
        var stones = 0;
        var holes = 0;
        var leftSpace = area.LeftEdge == 0 ? 1 : 0;
        var rightSpace = area.RightEdge == 0 ? 1 : 0;
        for ( int i = area.Start, h = 0, r = 0; i < end; i++ )
        {
            if ( i == cellIndex || area.Line[i] == selfStoneColor ) stones++;

            // calculate holes inside the figure
            if ( area.Line[i] == 0 && i != cellIndex )
            {
                if ( stones > 0 ) h++;
                r++; // right space not verified
            }
            else if ( area.Line[i] == selfStoneColor || i == cellIndex )
            {
                if ( h > 0 )
                {
                    holes += h;
                    h = 0;
                }
                r = 0;
            }

            // calculate free space around the potential figure
            if ( stones == 0 && i != cellIndex && area.Line[i] == 0 ) leftSpace++;
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
            4 when holes == 0 && leftSpace >0 && rightSpace > 0 => FigureType.OpenFour,
            4 => FigureType.ClosedFour,
            5 => FigureType.Five,
            > 5 => FigureType.SixOrMore,
            _ => FigureType.None
        };
    }

    /// <summary>
    /// make a map of empty cells with the best potential figure for the cell
    /// </summary>
    internal static IDictionary<int, FigureType> DefineBestFigures( LineOfCells row, Stone targetStone, bool sixAllowed )
    {
        // define all possible areas for figures
        var areas = new List<CellsArea>();
        for ( var i = 0; i <= row.Length - 5; i++)
        {
            if ( CellsArea.IsValid( row, out var area, targetStone, i, sixAllowed: sixAllowed ) )
            {
                areas.Add( area! );
            }
        }

        // get all valid figures for all cells
        var figures = new List<(int cell, FigureType type)>();
        foreach ( var area in areas )
        {
            for ( var i = area.Start; i < area.Start + area.Length; i++ )
            {
                if ( row[i] != 0 ) continue;
                var figure = DefineFigure( area, i );
                if ( figure != FigureType.None ) figures.Add( (i, figure) );
            }
        }

        // select best figure for each cell
        return figures
            .GroupBy( f => f.cell )
            .Select( g => g.MaxBy( f => (int)f.type ) )
            .ToDictionary(g => g.cell, g => g.type);
    }
}