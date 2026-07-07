namespace Renju.Core.BoardAnalyser;

internal class GraphTreeNode
{
    public Move? Move { get; init; }
    public IBoard Board { get; init; } = null!;
    public IReferee Referee { get; init; } = null!;
    public BoardWeightsAnalyser BlackWeights { get; init; } = null!;
    public BoardWeightsAnalyser WhiteWeights { get; init; } = null!;
    public Stone NextToMove { get; init; }

    // PlyWeight: Plain weight of the move that produced this node, signed
    // from the AI's perspective (+ for AI move, − for opponent).
    public int PlyWeight { get; init; }

    // SubtreeValue: alternating-sum value propagated bottom-up, signed
    // from the AI's perspective. Higher = better for AI.
    public int SubtreeValue { get; set; }

    // Settable: a win-in-2 "terminal" is a prediction; if the real game reaches
    // such a node, the root must be re-opened and keep playing (see OnStoneMoved).
    public bool IsTerminal { get; set; }
    public bool Expanded { get; set; }

    public Dictionary<Coord, GraphTreeNode> Children { get; } = new();
}
