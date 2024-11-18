namespace Renju.CommandLine;

internal record LayoutConfig
{
    public string Name { get; init; } = null!;
    
    public ColorStyle Board { get; init; } = new( ConsoleColor.DarkGray, ConsoleColor.Black );
    public ColorStyle Info { get; init; } = new( ConsoleColor.DarkGray, ConsoleColor.Black );
    public ColorStyle Help { get; init; } = new( ConsoleColor.Blue, ConsoleColor.Black );
    public ColorStyle MessageError { get; init; } = new( ConsoleColor.Red, ConsoleColor.Black );
    public ColorStyle MessageWin { get; init; } = new(ConsoleColor.Green, ConsoleColor.Black);
    public ColorStyle MessageLoose { get; init; } = new( ConsoleColor.Yellow, ConsoleColor.Black );
    public ColorStyle MessageDraw { get; init; } = new( ConsoleColor.Cyan, ConsoleColor.Black );
    public ColorStyle PlayerBlack { get; init; } = new( ConsoleColor.DarkGray, ConsoleColor.Black );
    public ColorStyle PlayerWhite { get; init; } = new( ConsoleColor.White, ConsoleColor.Black );
    public ColorStyle StoneBlack { get; init; } = new( ConsoleColor.Yellow, ConsoleColor.Black );
    public ColorStyle StoneWhite { get; init; } = new( ConsoleColor.White, ConsoleColor.Black );
    public ColorStyle DebugBlack { get; init; } = new( ConsoleColor.DarkYellow, ConsoleColor.Black );
    public ColorStyle DebugWhite { get; init; } = new( ConsoleColor.Cyan, ConsoleColor.Black );
    
    public char StoneCharBlack { get; init; }
    public char StoneCharWhite { get; init; }

    public string GridChars =
        "┌─┬─┐" + // 0 1 2   4
        "├─┼─┤" + // 5   7   9
        "│ │ │" + // 10
        "└─┴─┘";  // 15  17  19

    public int CellHeight { get; set; }
    public int CellWidth { get; set; }
}
