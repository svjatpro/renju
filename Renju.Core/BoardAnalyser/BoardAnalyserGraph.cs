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

        ProcessMove( CurrentGamePoint );


        //foreach ( var m in moves.Where(m => m.point == null ).Take(10) )
        //{
        //    var nextPoint = CurrentGamePoint.CloneFor( m.move );
        //    CurrentGamePoint.NodesGrid[m.move.Col, m.move.Row] = nextPoint;
        //}

        return CurrentGamePoint.GetBestMove( out move );

        void ProcessMove( MovePoint move )
        {
            var moves = CurrentGamePoint
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
                var nextPoint = CurrentGamePoint.CloneFor( bestMove.move );
                CurrentGamePoint.NodesGrid[bestMove.move.Col, bestMove.move.Row] = nextPoint;
            }

            //foreach ( var m in moves.Where( m => m.point != null ) )
            // probability ??
            // 

            //var nextPoint = CurrentGamePoint.NodesGrid[m.move.Col, m.move.Row];
            //CurrentGamePoint.NodesGrid[m.move.Col, m.move.Row] = nextPoint;
        }    
    }
}