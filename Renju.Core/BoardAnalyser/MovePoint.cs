using Renju.Core.Extensions;

namespace Renju.Core.BoardAnalyser;

internal class MovePoint : IDisposable
{
    private IBoard Board;
    private IReferee Referee;

    private readonly Stone SelfStone;
    private readonly Stone NextStone;
    private readonly int Center;
    private readonly Dictionary<Stone, BoardWeightsAnalyser> WeightsAnalysers;
    
    private int Weight;
    private decimal NormalizedWeight;

    public readonly MovePoint?[,] NodesGrid;
    
    public MovePoint( 
        Stone selfStone, Stone nextStone,
        IBoard board, IReferee referee,
        BoardWeightsAnalyser? blackAnalyser = null,
        BoardWeightsAnalyser? whiteAnalyser = null )
    {
        SelfStone = selfStone;
        NextStone = nextStone;
        Board = board;
        Referee = referee;

        Center = Board.Size / 2 - 1;
        WeightsAnalysers = new Dictionary<Stone, BoardWeightsAnalyser>
        {
            { 
                Stone.Black, 
                blackAnalyser ?? new BoardWeightsAnalyser( new BoardFiguresAnalyser( Board, Stone.Black ) )
            },
            { 
                Stone.White, 
                whiteAnalyser ?? new BoardWeightsAnalyser( new BoardFiguresAnalyser( Board, Stone.White ) ) 
            },
        };

        NodesGrid = new MovePoint?[Board.Size, Board.Size];

        
    }

    public string[] DebugNodes
    {
        get
        {
            var debugNodes = new List<string>();
            TrackNodes( this, string.Empty, debugNodes );
            return debugNodes.ToArray();

            void TrackNodes( MovePoint point, string currentPath, List<string> nodes )
            {
                var anyNodes = false;
                for ( var col = 0; col < point.NodesGrid.GetLength( 0 ); col++ )
                {
                    for ( var row = 0; row < point.NodesGrid.GetLength( 1 ); row++ )
                    {
                        if ( point.NodesGrid[col, row] != null )
                        {
                            anyNodes = true;
                            var stone = point.NextStone == Stone.Black ? "x" : "o";
                            var node = $" => {stone}({col}, {row})";
                            TrackNodes( point.NodesGrid[col, row]!, currentPath + node, nodes );
                        }
                    }
                }
                if ( !anyNodes )
                {
                    nodes.Add( currentPath );
                }
            }            
        }
    }

    public bool GetBestMove( out Move move )
    {
        var opponent = SelfStone.Opposite();
        var bestWeight = 0;
        var centerCoef = 0;
        move = new Move( 0, 0, SelfStone );
        for ( var col = 0; col < Board.Size; col++ )
        {
            for ( var row = 0; row < Board.Size; row++ )
            {
                if ( Board[col, row].Stone != Stone.None ||
                     !Referee.MoveAllowed( col, row, SelfStone ) )
                {
                    continue;
                }

                // weight of self move
                var cellWeight = WeightsAnalysers[SelfStone][col, row];

                // consider weight of an opponent's move
                if ( Referee.MoveAllowed( col, row, opponent, ignoreSequence: true ) )
                {
                    cellWeight += WeightsAnalysers[SelfStone.Opposite()][col, row];
                }
                if ( cellWeight < bestWeight ) continue;

                var coef = Math.Abs( col - Center ) + Math.Abs( row - Center );
                if ( cellWeight > bestWeight ||
                     cellWeight == bestWeight && ( centerCoef == 0 || coef < centerCoef ) )
                {
                    bestWeight = cellWeight;
                    move = new Move( col, row, SelfStone );
                    centerCoef = coef;
                }
            }
        }
        return true;
    }
    public IList<(int weight, Move move, MovePoint? point)> GetBestMoves()
    {
        var opponent = NextStone.Opposite();
        var moves = new List<(int weight, Move move, MovePoint? point)>();
        for ( var col = 0; col < Board.Size; col++ )
        {
            for ( var row = 0; row < Board.Size; row++ )
            {
                if ( Board[col, row].Stone != Stone.None ||
                     !Referee.MoveAllowed( col, row, NextStone ) )
                {
                    continue;
                }
                // weight of self move
                var cellWeight = WeightsAnalysers[NextStone][col, row];
                // consider weight of an opponent's move
                if ( Referee.MoveAllowed( col, row, opponent, ignoreSequence: true ) )
                {
                    cellWeight += WeightsAnalysers[NextStone.Opposite()][col, row];
                }
                moves.Add( (cellWeight, new Move( col, row, NextStone ), NodesGrid[col, row]) );
            }
        }
        return moves;
    }

    public MovePoint CloneFor( Move move )
    {
        if( NodesGrid[move.Col, move.Row] != null )
        {
            return NodesGrid[move.Col, move.Row]!;
        }

        var board = Board.Clone();
        var referee = Referee.Clone( board );
        
        var next = new MovePoint( 
            SelfStone, move.Stone.Opposite(), board, referee,
            WeightsAnalysers[Stone.Black].Clone( board ),
            WeightsAnalysers[Stone.White].Clone( board ) );

        var w = WeightsAnalysers[move.Stone][move.Col, move.Row] +
                WeightsAnalysers[move.Stone.Opposite()][move.Col, move.Row];
        var n = Math.Round((decimal)w / BoardWeightsAnalyser.FigureWeights[FigureType.Five], 2);

        board.PutStone( move );
        return next;
    }

    public void Dispose()
    {
        WeightsAnalysers[Stone.Black].Dispose();
        WeightsAnalysers[Stone.White].Dispose();

        Board.Dispose();
        Board = null!;
        Referee = null!;
    }
}