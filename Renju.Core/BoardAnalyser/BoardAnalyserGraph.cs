using Renju.Core.Extensions;
using Renju.Core.RenjuGame;

namespace Renju.Core.BoardAnalyser;

public class BoardAnalyserGraph : IBoardAnalyser
{
    private readonly Stone AiStone;
    private readonly IBoard ActualBoard;
    private readonly GraphConfig Config;
    private GraphTreeNode Root;

    public BoardAnalyserGraph( Stone stone, IBoard board, IReferee referee, GraphConfig? config = null )
    {
        AiStone = stone;
        ActualBoard = board;
        Config = config ?? new GraphConfig();

        Root = BuildInitialRoot();
        ExpandToDepth( Root, Config.Depth );
        ComputeSubtreeValue( Root );

        board.StoneMoved += OnStoneMoved;
    }

    public bool TryProceedNextMove( out Move move )
    {
        if ( Root.NextToMove != AiStone || Root.Children.Count == 0 )
        {
            move = default!;
            return false;
        }

        var size = Root.Board.Size;
        var center = size / 2 - 1;
        GraphTreeNode? best = null;
        var bestValue = int.MinValue;
        var bestCenterDist = int.MaxValue;

        foreach ( var child in Root.Children.Values )
        {
            if ( child.Move == null ) continue;
            var dist = Math.Abs( child.Move.Col - center ) + Math.Abs( child.Move.Row - center );

            if ( child.SubtreeValue > bestValue ||
                 ( child.SubtreeValue == bestValue && dist < bestCenterDist ) )
            {
                best = child;
                bestValue = child.SubtreeValue;
                bestCenterDist = dist;
            }
        }

        if ( best?.Move == null )
        {
            move = default!;
            return false;
        }

        move = best.Move;
        return true;
    }

    private void OnStoneMoved( object? sender, Move move )
    {
        // Either side just played `move` on the actual board.
        // Promote the matching child to the new root if we predicted it;
        // otherwise rebuild from old root + the actual move.
        if ( Root.Children.TryGetValue( move.Coord, out var promoted ) )
        {
            Root = promoted;
        }
        else
        {
            Root = RebuildRootFromUnexpectedMove( move );
        }

        ExpandToDepth( Root, Config.Depth );
        ComputeSubtreeValue( Root );
    }

    private GraphTreeNode BuildInitialRoot()
    {
        // MVP assumes the AI is constructed at game start (empty board).
        // For a mid-game start the figures map would be empty and existing
        // stones invisible to the analysers — accept that limitation for now.
        var newBoard = ActualBoard.Clone();
        var blackFigures = new BoardFiguresAnalyser( newBoard, Stone.Black );
        var whiteFigures = new BoardFiguresAnalyser( newBoard, Stone.White );
        var blackWeights = new BoardWeightsAnalyser( blackFigures );
        var whiteWeights = new BoardWeightsAnalyser( whiteFigures );
        var referee = new Referee( newBoard, blackFigures, whiteFigures );

        return new GraphTreeNode
        {
            Move = null,
            Board = newBoard,
            Referee = referee,
            BlackWeights = blackWeights,
            WhiteWeights = whiteWeights,
            NextToMove = ActualBoard.LastMove?.Stone.Opposite() ?? Stone.Black,
            PlyWeight = 0,
            IsTerminal = false,
            Expanded = false
        };
    }

    private GraphTreeNode RebuildRootFromUnexpectedMove( Move actualMove )
    {
        var oldRoot = Root;
        var newBoard = oldRoot.Board.Clone();
        var newBlackWeights = oldRoot.BlackWeights.Clone( newBoard );
        var newWhiteWeights = oldRoot.WhiteWeights.Clone( newBoard );
        var newReferee = new Referee( newBoard, newBlackWeights.Analyser, newWhiteWeights.Analyser );

        newBoard.PutStone( actualMove );

        return new GraphTreeNode
        {
            Move = actualMove,
            Board = newBoard,
            Referee = newReferee,
            BlackWeights = newBlackWeights,
            WhiteWeights = newWhiteWeights,
            NextToMove = actualMove.Stone.Opposite(),
            PlyWeight = 0, // root's PlyWeight is not summed into subtree values
            IsTerminal = newReferee.IsGameOver,
            Expanded = false
        };
    }

    private GraphTreeNode CreateChildNode( GraphTreeNode parent, Move move )
    {
        // Ply weight = Plain weight of this move on the parent's pre-move board.
        var selfWeights = move.Stone == Stone.Black ? parent.BlackWeights : parent.WhiteWeights;
        var oppWeights = move.Stone == Stone.Black ? parent.WhiteWeights : parent.BlackWeights;
        var unsigned = selfWeights[move.Col, move.Row];
        if ( parent.Referee.MoveAllowed( move.Col, move.Row, move.Stone.Opposite(), ignoreSequence: true ) )
        {
            unsigned += oppWeights[move.Col, move.Row];
        }
        var signed = move.Stone == AiStone ? unsigned : -unsigned;

        var newBoard = parent.Board.Clone();
        var newBlackWeights = parent.BlackWeights.Clone( newBoard );
        var newWhiteWeights = parent.WhiteWeights.Clone( newBoard );
        var newReferee = new Referee( newBoard, newBlackWeights.Analyser, newWhiteWeights.Analyser );

        newBoard.PutStone( move );

        return new GraphTreeNode
        {
            Move = move,
            Board = newBoard,
            Referee = newReferee,
            BlackWeights = newBlackWeights,
            WhiteWeights = newWhiteWeights,
            NextToMove = move.Stone.Opposite(),
            PlyWeight = signed,
            IsTerminal = newReferee.IsGameOver,
            Expanded = false
        };
    }

    private void ExpandToDepth( GraphTreeNode node, int remainingDepth )
    {
        if ( remainingDepth <= 0 || node.IsTerminal ) return;

        if ( !node.Expanded )
        {
            foreach ( var coord in EnumerateTopKCandidates( node, Config.TopK ) )
            {
                var move = new Move( coord.Col, coord.Row, node.NextToMove );
                node.Children[coord] = CreateChildNode( node, move );
            }
            node.Expanded = true;
        }

        foreach ( var child in node.Children.Values )
        {
            ExpandToDepth( child, remainingDepth - 1 );
        }
    }

    private IEnumerable<Coord> EnumerateTopKCandidates( GraphTreeNode node, int k )
    {
        var size = node.Board.Size;
        var center = size / 2 - 1;
        var selfWeights = node.NextToMove == Stone.Black ? node.BlackWeights : node.WhiteWeights;
        var oppWeights = node.NextToMove == Stone.Black ? node.WhiteWeights : node.BlackWeights;
        var opp = node.NextToMove.Opposite();

        var candidates = new List<(Coord coord, int weight)>();
        for ( var col = 0; col < size; col++ )
        {
            for ( var row = 0; row < size; row++ )
            {
                if ( node.Board[col, row].Stone != Stone.None ) continue;
                if ( !node.Referee.MoveAllowed( col, row, node.NextToMove ) ) continue;

                var w = selfWeights[col, row];
                if ( node.Referee.MoveAllowed( col, row, opp, ignoreSequence: true ) )
                {
                    w += oppWeights[col, row];
                }

                candidates.Add( (new Coord( col, row ), w) );
            }
        }

        // Prefer cells with non-zero Plain weight (near existing patterns).
        // Fall back to all legal cells if everything is cold (e.g. empty board).
        var warm = candidates.Where( c => c.weight > 0 ).ToList();
        var pool = warm.Count > 0 ? warm : candidates;

        return pool
            .OrderByDescending( c => c.weight )
            .ThenBy( c => Math.Abs( c.coord.Col - center ) + Math.Abs( c.coord.Row - center ) )
            .Take( k )
            .Select( c => c.coord );
    }

    private void ComputeSubtreeValue( GraphTreeNode node )
    {
        if ( node.IsTerminal || node.Children.Count == 0 )
        {
            node.SubtreeValue = node.PlyWeight;
            return;
        }

        foreach ( var child in node.Children.Values )
        {
            ComputeSubtreeValue( child );
        }

        // Whoever moves at this node picks the child outcome.
        // If it's the AI's turn here, AI picks max (best for AI).
        // If it's the opponent's, they pick min (worst for AI).
        var pickedChildValue = node.NextToMove == AiStone
            ? node.Children.Values.Max( c => c.SubtreeValue )
            : node.Children.Values.Min( c => c.SubtreeValue );

        node.SubtreeValue = node.PlyWeight + pickedChildValue;
    }
}
