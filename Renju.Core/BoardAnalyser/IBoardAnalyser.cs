namespace Renju.Core.BoardAnalyser;

public interface IBoardAnalyser
{
    bool TryProceedNextMove( out Move move );
}