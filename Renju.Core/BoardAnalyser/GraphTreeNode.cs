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

    public bool IsTerminal { get; init; }
    public bool Expanded { get; set; }

    public Dictionary<Coord, GraphTreeNode> Children { get; } = new();
}
