namespace Rendzu.Core;

internal class Referee : IReferee
{
    // todo: rules variations / customization

    private readonly IBoard Board;

    private void StoneMoved( object? sender, Move e )
    {
        // todo: refresh board state

        // tmp code
        for ( var col = 0; col < Board.Size; col++ )
        {
            for ( var row = 0; row < Board.Size; row++ )
            {
                if ( Board[col, row].Stone == Stone.None )
                    return;
            }
        }
        IsGameOver = true;
    }

    public Referee( IBoard board )
    {
        Board = board;
        Board.StoneMoved += StoneMoved;
    }
    
    public bool MoveAllowed( int col, int row, Stone stone )
    {
        return MoveAllowed( col, row, stone, out _ );
    }

    public bool MoveAllowed( int col, int row, Stone stone, out string? message )
    {
        message = null;

        // game is over
        if ( IsGameOver )
            message = "Game is over.";

        // border size check
        else if ( col < 0 || col >= Board.Size || row < 0 || row >= Board.Size )
            message = "Cell is out of the board.";

        // cell is empty
        else if ( Board[col, row].Stone != Stone.None )
            message = "Cell is not empty.";

        // right player's (by stone color) turn
        else if ( Board.LastMove?.Stone == stone )
            message = "It's not your turn.";
        
        return message == null;
    }

    public bool IsGameOver { get; private set; }
}