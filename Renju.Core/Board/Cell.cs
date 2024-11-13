namespace Renju.Core.Board;

internal record Cell : ICell
{
    private Stone InternalStone = Stone.None;
    public Stone Stone
    {
        get => InternalStone;
        set
        {
            if ( value != Stone.None )
                throw new InvalidOperationException( "Cannot put a stone on a cell that already has a stone." );
            InternalStone = value;
        }
    }
}