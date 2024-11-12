namespace Renju.Core.Extensions;

public static class StoneExtensions
{
    public static Stone Opposite( this Stone stone ) => stone switch
    {
        Stone.Black => Stone.White,
        Stone.White => Stone.Black,
        _ => Stone.None
    };

    private const int Both = (int)Stone.Black + (int)Stone.White;
    public static int Opposite( this int stone ) => Both - stone;
}