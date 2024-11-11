namespace Renju.CommandLine;

internal record GameStyle
{
    public ColorStyle Board { get; init; } = null!;
    public ColorStyle Info { get; init; } = null!;
    public ColorStyle Help { get; init; } = null!;
    public ColorStyle MessageError { get; init; } = null!;
    public ColorStyle MessageWin { get; init; } = null!;
    public ColorStyle MessageLoose { get; init; } = null!;
    public ColorStyle MessageDraw { get; init; } = null!;
    public ColorStyle PlayerBlack { get; init; } = null!;
    public ColorStyle PlayerWhite { get; init; } = null!;
    public ColorStyle StoneBlack { get; init; } = null!;
    public ColorStyle StoneWhite { get; init; } = null!;
    public char StoneCharBlack { get; init; }
    public char StoneCharWhite { get; init; }
}
