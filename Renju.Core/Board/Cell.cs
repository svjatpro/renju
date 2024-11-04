namespace Renju.Core.Board;
internal class Cell : ICell
{
    public Stone Stone { get; set; } = Stone.None;
}