using Renju.Core.BoardAnalyser;

namespace Renju.Core.RenjuGame;

internal class Referee : IReferee
{
    #region Private Fields

    private readonly IBoard Board;
    private readonly Dictionary<Stone, BoardAnalyzer> BoardAnalyzers;

    private void StoneMoved(object? sender, Move move)
    {
        if( IsGameOver ) return;

        // check for draw
        for (var col = 0; col < Board.Size; col++)
        {
            for (var row = 0; row < Board.Size; row++)
            {
                if (Board[col, row].Stone == Stone.None)
                    return;
            }
        }
        // there is no allowed moves
        IsGameOver = true;
        Winner = Stone.None;
        GameOver?.Invoke( this, Winner );
    }

    private void MoveAnalysed( object? sender, 
        (Move move, 
            Dictionary<FigureDirection, FigureType> figures,
            List<(int col, int row)> affectedCells) e )
    {
        var analyzer = (IBoardAnalyser) sender!;
        var (move, figures, _) = e;
        if ( move.Stone == analyzer.TargetStone && 
             figures.Any( f => 
                 f.Value == FigureType.Five || 
                 ( move.Stone == Stone.White && f.Value == FigureType.SixOrMore ) ) )
        {
            IsGameOver = true;
            Winner = move.Stone;
            GameOver?.Invoke( this, Winner );
        }
    }

    #endregion

    public Referee( IBoard board )
    {
        Board = board;
        Board.StoneMoved += StoneMoved;

        BoardAnalyzers = new Dictionary<Stone, BoardAnalyzer>
        {
            { Stone.Black, new BoardAnalyzer( board, Stone.Black ) },
            { Stone.White, new BoardAnalyzer( board, Stone.White ) },
        };

        foreach ( var analyzer in BoardAnalyzers.Values )
        {
            analyzer.MoveAnalysed += MoveAnalysed;
        }
    }

    public bool MoveAllowed(int col, int row, Stone stone)
    {
        string? message = null;

        // game is over
        if (IsGameOver)
            message = "Game is over.";

        // border size check
        else if (col < 0 || col >= Board.Size || row < 0 || row >= Board.Size)
            message = "Cell is out of the board.";

        // cell is empty
        else if (Board[col, row].Stone != Stone.None)
            message = "Cell is not empty.";

        // right player's (by stone color) turn
        else if (Board.LastMove?.Stone == stone)
            message = "It's not your turn.";

        // 3x3
        else if ( stone == Stone.Black && BoardAnalyzers[stone][col, row].Count( f => f.Value == FigureType.OpenThree ) > 1 )
        {
            message = "3x3 rule violation.";
        }

        // 4x4
        else if ( stone == Stone.Black && BoardAnalyzers[stone][col, row].Count( f => f.Value == FigureType.OpenFour ) > 1 )
        {
            message = "4x4 rule violation.";
        }

        // 6+
        else if ( stone == Stone.Black && BoardAnalyzers[stone][col, row].Any( f => f.Value == FigureType.SixOrMore ) )
        {
            message = "6+ rule violation.";
        }

        if( message != null ) ForbiddenMove?.Invoke( this, message );
        return message == null;
    }

    public bool IsGameOver { get; private set; } = false;
    public Stone Winner { get; private set; } = Stone.None;

    public event EventHandler<string>? ForbiddenMove;
    public event EventHandler<Stone>? GameOver;
}