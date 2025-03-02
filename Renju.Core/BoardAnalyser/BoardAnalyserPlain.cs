namespace Renju.Core.BoardAnalyser;

public class BoardAnalyserPlain : IBoardAnalyser
{
    private readonly MovePoint CurrentGameState;

    public BoardAnalyserPlain( Stone stone, IBoard board, IReferee referee )
    {
        // use general board and referee, so it will be updated dynamically
        CurrentGameState = new MovePoint( stone, board, referee );
    }

    public bool TryProceedNextMove( out Move move )
    {
        // just to imitate thinking process
        // todo: make it configurable - delay is not needed for AI vs AI games
        Thread.Sleep( 300 );

        return CurrentGameState.GetBestMove( out move );
    }
}