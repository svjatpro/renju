namespace Renju.Core;

//public record BoardCell( int Col, int Row, Stone Stone );
//public record Cell
public record Move( int Col, int Row, Stone Stone, int SeqNumber = 0);