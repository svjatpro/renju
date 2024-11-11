namespace Renju.Core;

public interface IPlayer
{
    Stone Stone { get; }
    string Name { get; }

    void StartGame( Stone playersColor, IBoard board, IReferee referee );
    bool TryProceedMove( out Move move );
}