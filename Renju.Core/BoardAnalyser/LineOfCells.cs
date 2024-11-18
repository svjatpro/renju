namespace Renju.Core.BoardAnalyser;

public record LineOfCells( int[] Line, int Length )
{
    public LineOfCells(int[] Line) : this(Line, Line.Length){}
    public int this[int i] => Line[i];
}