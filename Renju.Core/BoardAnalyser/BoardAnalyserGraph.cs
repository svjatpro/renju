using System.Diagnostics;
using Renju.Core.Extensions;
using Renju.Core.RenjuGame;

namespace Renju.Core.BoardAnalyser;

public class BoardAnalyserGraph : IBoardAnalyser
{
    private readonly Stone AiStone;
    private readonly IBoard ActualBoard;
    private readonly GraphConfig Config;
    private readonly Random Random;
    private GraphTreeNode Root;

    public BoardAnalyserGraph( Stone stone, IBoard board, IReferee referee, GraphConfig? config = null, Random? random = null )
    {
        AiStone = stone;
        ActualBoard = board;
        Config = config ?? new GraphConfig();
        Random = random ?? Random.Shared;

        Root = BuildInitialRoot();

        board.StoneMoved += OnStoneMoved;
    }

    public bool TryProceedNextMove( out Move move )
    {
        if ( Root.NextToMove != AiStone )
        {
            move = default!;
            return false;
        }

        // All thinking happens here, on the AI's own turn — not in OnStoneMoved.
        // That keeps per-move timing attributable to this player and makes the
        // timeout a true per-move cap.
        Expand();
        ComputeSubtreeValue( Root );

        if ( Root.Children.Count == 0 )
        {
            move = default!;
            return false;
        }

        var size = Root.Board.Size;
        var center = size / 2 - 1;
        GraphTreeNode? best = null;
        var bestValue = int.MinValue;
        var bestCenterDist = int.MaxValue;
        var tieCount = 0;

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
                tieCount = 1;
            }
            // equal best candidates: pick one uniformly (reservoir sampling),
            // so AI-vs-AI games are not identical replays
            else if ( child.SubtreeValue == bestValue && dist == bestCenterDist &&
                      Random.Next( ++tieCount ) == 0 )
            {
                best = child;
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

        // A win-in-2 terminal is a prediction that cut the subtree. If the real
        // game arrives here anyway (e.g. the opponent actually made that open
        // four), the position must stay playable — reopen it for expansion.
        if ( Root.IsTerminal && !Root.Referee.IsGameOver )
        {
            Root.IsTerminal = false;
        }
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
        // Ply weight = the mover's OWN Plain weight on the parent's pre-move board.
        // The opponent's weight must NOT be added here (unlike candidate selection):
        // crediting a blocking move with the blocked threat's weight makes the tree
        // treat defence as profit, so threat lines cancel out instead of scoring
        // negative. Blocking pays off through the subtree it avoids, not the cell.
        var selfWeights = move.Stone == Stone.Black ? parent.BlackWeights : parent.WhiteWeights;
        var unsigned = selfWeights[move.Col, move.Row];
        var signed = move.Stone == AiStone ? unsigned : -unsigned;

        var newBoard = parent.Board.Clone();
        var newBlackWeights = parent.BlackWeights.Clone( newBoard );
        var newWhiteWeights = parent.WhiteWeights.Clone( newBoard );
        var newReferee = new Referee( newBoard, newBlackWeights.Analyser, newWhiteWeights.Analyser );

        newBoard.PutStone( move );

        // Win-in-2 short-circuit: a move that leaves the mover two or more
        // five-completing cells (open four, four-four fork) has decided the
        // game — the opponent can block only one. Score it near Five and stop
        // expanding, UNLESS the opponent has an immediate five of their own:
        // they move first, so the line stays open and min/max refutes it.
        // This is what makes tempo visible at any depth: without it, odd depth
        // ends on our own move and overplays attack, even depth ends on the
        // opponent's and overplays defence.
        var isTerminal = newReferee.IsGameOver;
        if ( !isTerminal )
        {
            var moverWeights = move.Stone == Stone.Black ? newBlackWeights : newWhiteWeights;
            var nextWeights = move.Stone == Stone.Black ? newWhiteWeights : newBlackWeights;
            if ( CountFiveThreats( moverWeights, newBoard, upTo: 2 ) >= 2 &&
                 CountFiveThreats( nextWeights, newBoard, upTo: 1 ) == 0 )
            {
                isTerminal = true;
                signed += move.Stone == AiStone ? WinInTwoWeight : -WinInTwoWeight;
            }
        }

        return new GraphTreeNode
        {
            Move = move,
            Board = newBoard,
            Referee = newReferee,
            BlackWeights = newBlackWeights,
            WhiteWeights = newWhiteWeights,
            NextToMove = move.Stone.Opposite(),
            PlyWeight = signed,
            IsTerminal = isTerminal,
            Expanded = false
        };
    }

    // Slightly below Five so an actual immediate five always outranks a win-in-2.
    private static readonly int WinInTwoWeight = BoardWeightsAnalyser.FigureWeights[FigureType.Five] - 2;

    private static int CountFiveThreats( BoardWeightsAnalyser weights, IBoard board, int upTo )
    {
        var five = BoardWeightsAnalyser.FigureWeights[FigureType.Five];
        var found = 0;
        for ( var col = 0; col < board.Size; col++ )
        {
            for ( var row = 0; row < board.Size; row++ )
            {
                if ( board[col, row].Stone != Stone.None ) continue;
                if ( weights[col, row] >= five && ++found >= upTo ) return found;
            }
        }
        return found;
    }

    private void Expand()
    {
        if ( Config.TimeoutMs <= 0 )
        {
            ExpandToDepth( Root, Config.Depth, deadline: null );
            return;
        }

        // With a time cap, deepen iteratively: on expiry the tree is a complete
        // shallower prediction (best found so far) instead of a lopsided one where
        // only the first candidates were explored deep. Re-walking finished levels
        // is cheap — nodes are never re-generated (Expanded flag).
        var deadline = Stopwatch.StartNew();
        for ( var depth = 1; depth <= Config.Depth && !Expired( deadline ); depth++ )
        {
            ExpandToDepth( Root, depth, deadline );
        }
    }

    private bool Expired( Stopwatch? deadline ) =>
        deadline != null && deadline.ElapsedMilliseconds >= Config.TimeoutMs;

    private void ExpandToDepth( GraphTreeNode node, int remainingDepth, Stopwatch? deadline )
    {
        if ( remainingDepth <= 0 || node.IsTerminal || Expired( deadline ) ) return;

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
            ExpandToDepth( child, remainingDepth - 1, deadline );
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
