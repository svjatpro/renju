using Renju.Core.Extensions;
using Renju.Core.RenjuGame;

namespace Renju.Core.BoardAnalyser;



public class BoardAnalyserGraph : IBoardAnalyser
{
    private readonly Stone SelfStone;
    private readonly IBoard Board;
    //private readonly IReferee Referee;
    
    //private readonly MovePoint?[,] NodesGrid;

    private MovePoint CurrentGamePoint;

    public BoardAnalyserGraph( Stone stone, IBoard board, IReferee referee )
    {
        SelfStone = stone;
        Board = board;
        //Referee = referee;

        Board.StoneMoved += ( _, move ) =>
        {
            var nextPoint = CurrentGamePoint!.CloneFor( move );

            //CurrentGameState.Dispose(); // todo: check why it doesn't work
            CurrentGamePoint = nextPoint;
        };

        //NodesGrid = new MovePoint[Board.Size, Board.Size];

        var currentBoard = board.Clone();
        var currentReferee = referee.Clone( currentBoard );
        CurrentGamePoint = new MovePoint( stone, Stone.Black, currentBoard, currentReferee );
    }

    public bool TryProceedNextMove( out Move move )
    {
        // just to imitate thinking process
        // todo: make it configurable - delay is not needed for AI vs AI games
        Thread.Sleep( 300 );

        CurrentGamePoint.GetBestMove( out move );

        var nextPoint = CurrentGamePoint.NodesGrid[move.Col, move.Row];
        if ( nextPoint == null )
        {
            nextPoint = CurrentGamePoint.CloneFor( move );
            CurrentGamePoint.NodesGrid[move.Col, move.Row] = nextPoint;
        }
        for ( var i = 0; i < 5; i++ )
        {
            ProcessMove( nextPoint );
        }

        return true;

        void ProcessMove( MovePoint move )
        {
            var moves = move
                .GetBestMoves()
                .OrderByDescending( m => m.weight )
                .ToList();

            var bestPoint = moves.FirstOrDefault( m => m.point != null ).point;
            if( bestPoint != null )
            {
                ProcessMove( bestPoint );
            }

            var bestMove = moves.FirstOrDefault( m => m.point == null );
            if ( bestMove != default )
            {
                var nextPoint = move.CloneFor( bestMove.move );
                move.NodesGrid[bestMove.move.Col, bestMove.move.Row] = nextPoint;
            }
            // probability ??
        }    
    }
}