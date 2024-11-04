using Renju.Core;

namespace Renju.Console;

internal class Program
{
    private static void Main( string[] args )
    {
        //int[] row = { 0, 1, 1, 0, 0, 0, 1, 1, 0, 1, 0 };
        //var figures = RenjuAnalyzer.AnalyzeRow( row );

        //foreach ( var item in figures.OrderBy( kvp => kvp.Key ) )
        //{
        //    Console.WriteLine( $"Cell {item.Key}: {item.Value}" );
        //}

        //return;
        var playerColor = args.Length > 0 && args[0] == "--white" ? Stone.White : Stone.Black;
        new ConsoleGame( playerColor, 19 ).Run();
    }
}


public enum FigureType
{
    None,
    ClosedTwo,
    OpenTwo,
    ClosedThree,
    OpenThree,
    ClosedFour,
    OpenFour,
    Five,
    SixOrMore
}

public class Pattern
{
    public FigureType Figure { get; }
    public int[] PatternArray { get; }
    public int PotentialMovePosition { get; }

    public Pattern( FigureType figure, int[] patternArray, int potentialMovePosition )
    {
        Figure = figure;
        PatternArray = patternArray;
        PotentialMovePosition = potentialMovePosition;
    }
}

public static class RenjuAnalyzer
{
    // Define all possible patterns, considering the potential move
    private static readonly List<Pattern> Patterns = new List<Pattern>
    {
        // Open Four: 0 1 1 1 1 0 (potential move at positions 1-4)
        new Pattern(FigureType.OpenFour, new[] { 0, 1, 1, 1, 1, 0 }, 1),
        new Pattern(FigureType.OpenFour, new[] { 0, 1, 1, 1, 1, 0 }, 2),
        new Pattern(FigureType.OpenFour, new[] { 0, 1, 1, 1, 1, 0 }, 3),
        new Pattern(FigureType.OpenFour, new[] { 0, 1, 1, 1, 1, 0 }, 4),

        // Closed Four: -1 1 1 1 1 0 (potential move at position 5)
        new Pattern(FigureType.ClosedFour, new[] { -1, 1, 1, 1, 1, 0 }, 5),
        // Closed Four: 0 1 1 1 1 -1 (potential move at position 0)
        new Pattern(FigureType.ClosedFour, new[] { 0, 1, 1, 1, 1, -1 }, 0),
        // Closed Four with Gap: 1 1 1 0 1 (potential move at position 3)
        new Pattern(FigureType.ClosedFour, new[] { 1, 1, 1, 0, 1 }, 3),

        // Open Three: 0 1 1 1 0 (potential move at positions 1-3)
        new Pattern(FigureType.OpenThree, new[] { 0, 1, 1, 1, 0 }, 1),
        new Pattern(FigureType.OpenThree, new[] { 0, 1, 1, 1, 0 }, 2),
        new Pattern(FigureType.OpenThree, new[] { 0, 1, 1, 1, 0 }, 3),
        // Open Three with Gap: 0 1 0 1 0  (potential move at position 2)
        new Pattern(FigureType.OpenThree, new[] { 0, 1, 0, 1, 0 }, 2),

        // Closed Three: -1 1 1 1 0 (potential move at position 4)
        new Pattern(FigureType.ClosedThree, new[] { -1, 1, 1, 1, 0 }, 4),
        // Closed Three: 0 1 1 1 -1 (potential move at position 0)
        new Pattern(FigureType.ClosedThree, new[] { 0, 1, 1, 1, -1 }, 0),
        // Closed Three with Gap: 1 1 0 1 (potential move at position 2)
        new Pattern(FigureType.ClosedThree, new[] { 1, 1, 0, 1 }, 2),

        // Open Two: 0 1 1 0 (potential move at positions 1-2)
        new Pattern(FigureType.OpenTwo, new[] { 0, 1, 1, 0 }, 1),
        new Pattern(FigureType.OpenTwo, new[] { 0, 1, 1, 0 }, 2),

        // Closed Two: -1 1 1 0 (potential move at position 3)
        new Pattern(FigureType.ClosedTwo, new[] { -1, 1, 1, 0 }, 3),
        // Closed Two: 0 1 1 -1 (potential move at position 0)
        new Pattern(FigureType.ClosedTwo, new[] { 0, 1, 1, -1 }, 0),
        // Closed Two with Gap: 1 0 1 (potential move at position 1)
        new Pattern(FigureType.ClosedTwo, new[] { 1, 0, 1 }, 1),

        // Five: 1 1 1 1 1 (already formed)
        new Pattern(FigureType.Five, new[] { 1, 1, 1, 1, 1 }, -1),

        // Six or More: 1 1 1 1 1 1
        new Pattern(FigureType.SixOrMore, new[] { 1, 1, 1, 1, 1, 1 }, -1)
    };

    public static Dictionary<int, string> AnalyzeRow( int[] row )
    {
        int length = row.Length;
        var result = new Dictionary<int, FigureType>();

        // For each empty cell, simulate placing a stone and check for patterns
        for ( int index = 0; index < length; index++ )
        {
            if ( row[index] != 0 ) continue; // Skip non-empty cells

            // Simulate placing a stone at the empty cell
            int[] simulatedRow = new int[length];
            Array.Copy( row, simulatedRow, length );
            simulatedRow[index] = 1; // Place the stone

            FigureType bestFigure = FigureType.None;

            // Check all patterns
            foreach ( var pattern in Patterns )
            {
                int patternLength = pattern.PatternArray.Length;
                int patternStart = index - pattern.PotentialMovePosition;
                int patternEnd = patternStart + patternLength - 1;

                // Ensure pattern is within bounds
                if ( patternStart < 0 || patternEnd >= length )
                    continue;

                // Extract the window from the simulated row
                int[] window = new int[patternLength];
                Array.Copy( simulatedRow, patternStart, window, 0, patternLength );

                if ( IsMatch( window, pattern.PatternArray ) )
                {
                    // Update the best figure if necessary
                    if ( bestFigure < pattern.Figure )
                    {
                        bestFigure = pattern.Figure;
                    }
                }
            }

            if ( bestFigure != FigureType.None )
            {
                result[index] = bestFigure;
            }
        }

        // Convert FigureType to string descriptions
        var finalResult = new Dictionary<int, string>();
        foreach ( var kvp in result )
        {
            string figureDescription = kvp.Value.ToString()
                .Replace( "Open", "open " )
                .Replace( "Closed", "closed " )
                .Replace( "SixOrMore", "six or more" )
                .ToLower();
            finalResult[kvp.Key] = figureDescription;
        }

        return finalResult;
    }

    private static bool IsMatch( int[] window, int[] pattern )
    {
        for ( int i = 0; i < pattern.Length; i++ )
        {
            int patternValue = pattern[i];
            int windowValue = window[i];

            if ( patternValue == -1 )
            {
                // Wildcard matches any value
                continue;
            }

            if ( patternValue != windowValue )
            {
                return false;
            }
        }
        return true;
    }
}