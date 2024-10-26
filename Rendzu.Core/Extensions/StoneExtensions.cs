namespace Rendzu.Core.Extensions;

public static class StoneExtensions
{
    public static Stone Opposite( this Stone stone ) => stone switch
    {
        Stone.Black => Stone.White,
        Stone.White => Stone.Black,
        _ => Stone.None
    };
}