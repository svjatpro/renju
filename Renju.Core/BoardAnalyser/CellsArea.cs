using Renju.Core.Extensions;

namespace Renju.Core.BoardAnalyser;

internal record CellsArea
{
    public LineOfCells Line { get; set; }
    public Stone Stone { get; set; }
    public int Start { get; set; }
    public int Length { get; set; } 
    public int LeftEdge { get; set; }
    public int RightEdge { get; set; }


    public CellsArea( LineOfCells line, Stone targetStone, int start, int length = 5 )
    {
        Line = line;
        Stone = targetStone;
        Start = start;
        Length = length;
        LeftEdge = start > 0 ? line[start - 1] : -1;
        RightEdge = ( start + length ) < line.Length ? line[start + length] : -1;
    }
    public CellsArea( LineOfCells line, Stone targetStone, int start, int length, int leftEdge, int rightEdge )
    {
        Line = line;
        Stone = targetStone;
        Start = start;
        Length = length;
        LeftEdge = leftEdge;
        RightEdge = rightEdge;
    }
    
    public static bool IsValid( 
        LineOfCells line, 
        out CellsArea? area,
        Stone targetStone, int start, int length = 5, bool sixAllowed = false )
    {
        area = null;
        var self = (int)targetStone;
        
        // must be 5 cells length
        if ( length != 5 || ( start + length ) > line.Length ) return false;
        
        var l = start > 0 ? line[start - 1] : -1;
        var r = ( start + length ) < line.Length ? line[start + length] : -1;
        
        // opponentStoneColor stone
        var end = start + length;
        for ( var i = start; i < end; i++ ) if ( line[i] == self.Opposite() ) return false;

        // six
        if ( !sixAllowed && ( l == self || r == self) ) return false;

        area = new CellsArea( line, targetStone, start, length, l, r );
        return true;
    }
}