using Renju.Core;

namespace Renju.CommandLine;

internal class ConsolePlayer( string name, Func<(bool,Coord)> readCoordinates ) : Player( name )
{
    public override bool TryProceedMove(out Move move)
    {
        if ( readCoordinates.Invoke() is not (true, var coord) )
        {
            move = default!;
            return false;
        }
        move = new Move( coord.Col, coord.Row, Stone );
        return true;
    }
}