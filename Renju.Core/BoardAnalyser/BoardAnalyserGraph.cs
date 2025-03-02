namespace Renju.Core.BoardAnalyser;

public class BoardAnalyserGraph : IBoardAnalyser
{
    private readonly Stone SelfStone;
    private readonly IBoard Board;
    //private readonly IReferee Referee;
    
    //private readonly MovePoint?[,] NodesGrid;

    private MovePoint CurrentGameState;

    public BoardAnalyserGraph( Stone stone, IBoard board, IReferee referee )
    {
        SelfStone = stone;
        Board = board;
        //Referee = referee;

        Board.StoneMoved += ( _, move ) =>
        {
            var nextPoint = CurrentGameState!.CloneFor( move );

            //CurrentGameState.Dispose(); // todo: check why it doesn't work
            CurrentGameState = nextPoint;
        };

        //NodesGrid = new MovePoint[Board.Size, Board.Size];

        var currentBoard = board.Clone();
        var currentReferee = referee.Clone( currentBoard );
        CurrentGameState = new MovePoint( stone, currentBoard, currentReferee );
    }

    public bool TryProceedNextMove( out Move move )
    {
        // just to imitate thinking process
        // todo: make it configurable - delay is not needed for AI vs AI games
        Thread.Sleep( 300 );

        return CurrentGameState.GetBestMove( out move );
    }
}