namespace Renju.Core;

public interface IReferee
{
    bool MoveAllowed(int col, int row, Stone stone, bool ignoreSequence = false);

    bool IsGameOver { get; }
    Stone Winner { get; }

    event EventHandler<string> ForbiddenMove;
    event EventHandler<Stone> GameOver;
}