namespace Rendzu.Core;

public class RendzuGame
{
    public IReferee Referee { get; init; } = null!;
    public IBoard Board { get; init; } = null!;
    
    public static RendzuGame New( int size )
    {
        var board = new Board( size );
        return new RendzuGame
        {
            Board = board,
            Referee = new Referee( board )
        };
    }
}