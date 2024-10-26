namespace Rendzu.Core;

public interface IReferee
{
    bool MoveAllowed( int col, int row, Stone stone );
    bool MoveAllowed( int col, int row, Stone stone, out string? message );

    bool IsGameOver { get; }
}