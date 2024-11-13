namespace Renju.Core;

public record Move( Coord Coord, Stone Stone, int SeqNumber = 0 )
{
    public Move( int col, int row, Stone stone, int seqNumber = 0 ) : 
        this( new Coord(col, row), stone, seqNumber ) { }

    public int Col => Coord.Col;
    public int Row => Coord.Row;

    public static implicit operator Coord( Move move ) => move.Coord;
    public static implicit operator Stone( Move move ) => move.Stone;
}