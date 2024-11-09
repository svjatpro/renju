using Renju.Core.Players;

namespace Renju.Core;

public abstract class Player( string name ) : IPlayer
{
    protected IBoard Board = null!;
    protected IReferee Referee = null!;

    public Stone Stone { get; private set; }
    public string Name { get; init; } = name;

    public virtual void StartGame( Stone playersColor, IBoard board, IReferee referee )
    {
        Stone = playersColor;
        Board = board;
        Referee = referee;
    }

    public abstract bool TryProceedMove( out Move move );

    public static IPlayer ConsoleHuman(
        int boardStartCol, int boardStartRow,
        Action<IPlayer, ConsoleKeyInfo>? otherKeyPressed = null,
        string name = "Human" )
    {
        var player = new ConsoleHuman( boardStartCol, boardStartRow, name );
        player.OtherKeyPressed += (_, k) => otherKeyPressed?.Invoke(player, k);
        return player;
    }

    public static IPlayer PcPlayer( string name = "PC" ) => new PcPlayer( name );
}